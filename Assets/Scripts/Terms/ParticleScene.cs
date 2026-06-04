using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// 파티클 + DOTween 연출 비교 (교육용)
/// Button 1: 상자가 위에서 떨어지며 쫀득 착지 + 먼지 파티클 + 부들부들
/// Button 2: 시네마틱 블랙바 + 차징 파티클 + 강렬한 떨림 + 폭발(2종) + 이미지 변경 + 글로우 루프
/// </summary>
public class ParticleScene : MonoBehaviour
{
    [Header("상자")]
    public RectTransform boxTransform;
    public Image boxImage;

    [Header("이미지")]
    public Sprite closedBoxSprite;   // 닫힌 상자
    public Sprite shakingBoxSprite;  // 터지기 직전 상자 (금 간 / 흔들리는)
    public Sprite openedBoxSprite;   // 열린 상자

    [Header("파티클")]
    public ParticleSystem chargeParticle;     // 차징 이펙트
    public ParticleSystem chargeChild;
    public ParticleSystem dustParticle;       // 착지 먼지 이펙트

    public ParticleSystem explodeParticle;    // 폭발 이펙트 1 (파편)
    public ParticleSystem explodeParticle2;   // 폭발 이펙트 2 (섬광/링)
    public ParticleSystem glowLoopParticle;   // 열린 상자 후 반짝이 루프

    [Header("시네마틱 블랙바 (Focus)")]
    public RectTransform blackBarTop;         // 상단 검정 바
    public RectTransform blackBarBottom;      // 하단 검정 바
    public float barSize = 80f;               // 바 높이
    public float barDuration = 0.4f;          // 등장/퇴장 시간

    [Header("버튼")]
    public Button button1;  // 등장 연출
    public Button button2;  // 폭발 연출

    [Header("설정")]
    public float dropStartY = 800f;
    public float dropDuration = 0.6f;
    public float shakeDuration = 0.5f;
    public float chargeDuration = 1.5f;

    private Vector2 originalPos;
    private bool isAnimating = false;

    void Start()
    {
        originalPos = boxTransform.anchoredPosition;

        // 최초: 상자 안 보이게 + 모든 파티클 오브젝트 끄기 + 블랙바 숨기기
        boxImage.sprite = closedBoxSprite;
        boxTransform.gameObject.SetActive(false);
        HideAllParticles();
        HideBlackBarsImmediate();

        button1.onClick.AddListener(OnClickButton1);
        button2.onClick.AddListener(OnClickButton2);
    }

    // ===== Button 1: 위에서 떨어지며 쫀득 착지 + 먼지 + 부들부들 =====
    public void OnClickButton1()
    {
        if (isAnimating) return;
        isAnimating = true;

        HideAllParticles();
        HideBlackBarsImmediate();

        boxTransform.gameObject.SetActive(true);
        boxImage.sprite = closedBoxSprite;
        boxTransform.localScale = Vector3.one;
        boxTransform.localRotation = Quaternion.identity;
        boxTransform.anchoredPosition = new Vector2(originalPos.x, originalPos.y + dropStartY);

        // 1) 위에서 떨어지기 (OutBounce → 쫀득한 착지)
        boxTransform.DOAnchorPosY(originalPos.y, dropDuration)
            .SetEase(Ease.OutBounce)
            .OnComplete(() =>
            {
                // 2) 착지 → 먼지 파티클 + 부들부들 떨림
                PlayParticle(dustParticle);
                boxTransform.DOShakePosition(shakeDuration, 10f, 20, 90f, false, true)
                    .OnComplete(() => isAnimating = false);
            });
    }

    // ===== Button 2: 블랙바 + 차징 → 강렬한 떨림 → 폭발 → 이미지 변경 =====
    public void OnClickButton2()
    {
        if (isAnimating) return;
        isAnimating = true;

        HideAllParticles();

        boxTransform.gameObject.SetActive(true);
        boxImage.sprite = closedBoxSprite;
        boxTransform.localScale = Vector3.one;
        boxTransform.localRotation = Quaternion.identity;
        boxTransform.anchoredPosition = originalPos;

        Sequence seq = DOTween.Sequence();

        // 0) 블랙바 등장 (시네마틱 강조)
        seq.AppendCallback(() => ShowBlackBars());
        seq.AppendInterval(barDuration);

        // 1) 차징 파티클 시작 + 스케일 키우기 + 부들부들 동시에
        seq.AppendCallback(() =>
        {
            PlayParticle(chargeParticle);
            PlayParticle(chargeChild);
        });
        seq.Append(boxTransform.DOScale(1.15f, chargeDuration).SetEase(Ease.InQuad));
        seq.Join(boxTransform.DOShakePosition(chargeDuration, 12f, 25, 90f, false, true));
        seq.Join(boxTransform.DOShakeRotation(chargeDuration, 6f, 25, 90f));

        // 2) 터지기 직전 — 스프라이트 변경 + 더 강하게 떨림
        seq.AppendCallback(() =>
        {
            if (shakingBoxSprite != null) boxImage.sprite = shakingBoxSprite;
        });
        seq.Append(boxTransform.DOShakePosition(shakeDuration, 30f, 50, 90f, false, true));
        seq.Join(boxTransform.DOShakeRotation(shakeDuration, 20f, 50, 90f));

        // 3) 폭발! → 차징 끄고 + 폭발 2종 + 이미지 변경
        seq.AppendCallback(() =>
        {
            StopParticle(chargeParticle);
            StopParticle(chargeChild);
            PlayParticle(explodeParticle);
            PlayParticle(explodeParticle2);
            boxImage.sprite = openedBoxSprite;
        });

        // 4) 폭발 스케일 연출 (팡! 커졌다 원래대로)
        seq.Append(boxTransform.DOScale(1.4f, 0.08f).SetEase(Ease.OutQuad));
        seq.Append(boxTransform.DOScale(1f, 0.35f).SetEase(Ease.OutBack));

        // 5) 열린 상자 반짝이 루프 시작
        seq.AppendCallback(() =>
        {
            PlayParticle(glowLoopParticle);
        });

        // 6) 블랙바 퇴장
        seq.AppendCallback(() => HideBlackBars());

        seq.OnComplete(() => isAnimating = false);
    }

    // ========== 블랙바 ==========

    void ShowBlackBars()
    {
        if (blackBarTop != null)
        {
            blackBarTop.gameObject.SetActive(true);
            blackBarTop.anchoredPosition = new Vector2(0, 1920);
            blackBarTop.DOAnchorPosY(800, barDuration).SetEase(Ease.OutQuad);
        }
        if (blackBarBottom != null)
        {
            blackBarBottom.gameObject.SetActive(true);
            blackBarBottom.anchoredPosition = new Vector2(0, -1920);
            blackBarBottom.DOAnchorPosY(-800, barDuration).SetEase(Ease.OutQuad);
        }
    }

    void HideBlackBars()
    {
        if (blackBarTop != null)
            blackBarTop.DOAnchorPosY(1920, barDuration).SetEase(Ease.InQuad)
                .OnComplete(() => blackBarTop.gameObject.SetActive(false));
        if (blackBarBottom != null)
            blackBarBottom.DOAnchorPosY(-1920, barDuration).SetEase(Ease.InQuad)
                .OnComplete(() => blackBarBottom.gameObject.SetActive(false));
    }

    void HideBlackBarsImmediate()
    {
        if (blackBarTop != null)
        {
            blackBarTop.anchoredPosition = new Vector2(0, barSize);
            blackBarTop.gameObject.SetActive(false);
        }
        if (blackBarBottom != null)
        {
            blackBarBottom.anchoredPosition = new Vector2(0, -barSize);
            blackBarBottom.gameObject.SetActive(false);
        }
    }

    // ========== 파티클 ==========

    // 오브젝트 켜고 → Play
    void PlayParticle(ParticleSystem ps)
    {
        if (ps == null) return;
        ps.gameObject.SetActive(true);
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ps.Play();
    }

    // 정지 + 오브젝트 끄기
    void StopParticle(ParticleSystem ps)
    {
        if (ps == null) return;
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ps.gameObject.SetActive(false);
    }

    // 전부 끄기
    void HideAllParticles()
    {
        StopParticle(chargeParticle);
        StopParticle(chargeChild);
        StopParticle(dustParticle);
        StopParticle(explodeParticle);
        StopParticle(explodeParticle2);
        StopParticle(glowLoopParticle);
    }
}

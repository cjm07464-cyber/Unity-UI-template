using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// 파티클 + DOTween 연출 비교 (교육용)
/// Button 1: 상자가 위에서 떨어지며 쫀득 착지 + 먼지 파티클 + 부들부들
/// Button 2: 시네마틱 블랙바 + 차징 파티클 + 강렬한 떨림 + 폭발(2종) + 이미지 변경 + 글로우 루프
/// Button 3: 일반 차징 후 스페셜 상자로 전환되는 강화 오픈 연출
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
    public Sprite specialBoxSprite;  // 스페셜 닫힌 상자
    public Sprite specialOpenedBoxSprite; // 스페셜 열린 상자

    [Header("파티클")]
    public ParticleSystem chargeParticle;     // 차징 이펙트
    public ParticleSystem chargeChild;
    public ParticleSystem dustParticle;       // 착지 먼지 이펙트

    public ParticleSystem explodeParticle;    // 폭발 이펙트 1 (파편)
    public ParticleSystem explodeParticle2;   // 폭발 이펙트 2 (섬광/링)
    public ParticleSystem glowLoopParticle;   // 열린 상자 후 반짝이 루프
    public ParticleSystem specialChargeParticle; // 스페셜 전환/차지 이펙트
    public ParticleSystem specialExplodeParticle; // 스페셜 폭발 이펙트
    public ParticleSystem specialFlashParticle; // 스페셜 섬광 파티클

    [Header("시네마틱 블랙바 (Focus)")]
    public RectTransform blackBarTop;         // 상단 검정 바
    public RectTransform blackBarBottom;      // 하단 검정 바
    public float barSize = 80f;               // 바 높이
    public float barDuration = 0.4f;          // 등장/퇴장 시간

    [Header("화면 플래시")]
    public Image flashImage;

    [Header("버튼")]
    public Button button1;  // 등장 연출
    public Button button2;  // 폭발 연출
    public Button button3;  // 스페셜 폭발 연출

    [Header("설정")]
    public float dropStartY = 800f;
    public float dropDuration = 0.6f;
    public float shakeDuration = 0.5f;
    public float chargeDuration = 1.5f;

    private Vector2 originalPos;
    private bool isAnimating = false;

    void Start()
    {
        if (!HasCoreReferences()) return;

        originalPos = boxTransform.anchoredPosition;

        // 최초: 상자 안 보이게 + 모든 파티클 오브젝트 끄기 + 블랙바/플래시 숨기기
        SetChestSprite(closedBoxSprite);
        boxTransform.gameObject.SetActive(false);
        HideAllParticles();
        HideBlackBarsImmediate();
        HideFlashImmediate(0f);

        if (button1 != null) button1.onClick.AddListener(OnClickButton1);
        else Debug.LogWarning("ParticleScene: button1이 연결되지 않았습니다.");

        if (button2 != null) button2.onClick.AddListener(OnClickButton2);
        else Debug.LogWarning("ParticleScene: button2가 연결되지 않았습니다.");

        if (button3 != null) button3.onClick.AddListener(OnClickButton3);
    }

    // ===== Button 1: 위에서 떨어지며 쫀득 착지 + 먼지 + 부들부들 =====
    public void OnClickButton1()
    {
        if (!TryBeginAnimation()) return;

        HideAllParticles();
        HideBlackBarsImmediate();
        HideFlashImmediate(0f);

        ResetChest(
            closedBoxSprite,
            new Vector2(originalPos.x, originalPos.y + dropStartY),
            Vector3.one,
            Quaternion.identity,
            true
        );

        // 1) 위에서 떨어지기 (OutBounce → 쫀득한 착지)
        PlayChestMove(originalPos.y, dropDuration, Ease.OutBounce)
            .OnComplete(() =>
            {
                // 2) 착지 → 먼지 파티클 + 부들부들 떨림
                PlayParticle(dustParticle);
                PlayChestShake(shakeDuration, 10f, 20, 90f, false, true)
                    .OnComplete(EndAnimation);
            });
    }

    // ===== Button 2: 블랙바 + 차징 → 강렬한 떨림 → 폭발 → 이미지 변경 =====
    public void OnClickButton2()
    {
        if (!TryBeginAnimation()) return;

        PlayOpenSequence(
            startSprite: closedBoxSprite,
            poweredSprite: shakingBoxSprite,
            openedSprite: openedBoxSprite,
            chargeParticles: new ParticleSystem[] { chargeParticle, chargeChild },
            transitionParticles: null,
            explodeParticles: new ParticleSystem[] { explodeParticle, explodeParticle2 },
            loopParticle: glowLoopParticle,
            chargeScale: 1.15f,
            chargeTime: chargeDuration,
            chargeShakeStrength: 12f,
            chargeShakeVibrato: 25,
            chargeShakeRandomness: 90f,
            chargeRotationStrength: 6f,
            chargeRotationVibrato: 25,
            chargeRotationRandomness: 90f,
            poweredShakeTime: shakeDuration,
            poweredShakeStrength: 30f,
            poweredShakeVibrato: 50,
            poweredShakeRandomness: 90f,
            poweredRotationStrength: 20f,
            poweredRotationVibrato: 50,
            poweredRotationRandomness: 90f,
            burstScale: 1.4f,
            burstScaleTime: 0.08f,
            settleScale: 1f,
            settleScaleTime: 0.35f,
            flashColor: Color.white,
            flashMaxAlpha: 0f,
            flashFadeInTime: 0f,
            flashHoldTime: 0f,
            flashFadeOutTime: 0f,
            playFlash: false,
            useBlackBars: true,
            blackBarHiddenY: 1920f,
            blackBarVisibleY: 800f
        );
    }

    // ===== Button 3: 일반 차징 → 스페셜 전환 → 강화 폭발 + 화면 플래시 =====
    public void OnClickButton3()
    {
        if (!TryBeginAnimation()) return;

        PlayOpenSequence(
            startSprite: closedBoxSprite,
            poweredSprite: specialBoxSprite,
            openedSprite: specialOpenedBoxSprite,
            chargeParticles: new ParticleSystem[] { chargeParticle, chargeChild },
            transitionParticles: new ParticleSystem[] { specialChargeParticle },
            explodeParticles: new ParticleSystem[] { explodeParticle, explodeParticle2, specialExplodeParticle, specialFlashParticle },
            loopParticle: glowLoopParticle,
            chargeScale: 1.2f,
            chargeTime: chargeDuration,
            chargeShakeStrength: 16f,
            chargeShakeVibrato: 30,
            chargeShakeRandomness: 100f,
            chargeRotationStrength: 8f,
            chargeRotationVibrato: 30,
            chargeRotationRandomness: 100f,
            poweredShakeTime: shakeDuration * 1.25f,
            poweredShakeStrength: 48f,
            poweredShakeVibrato: 70,
            poweredShakeRandomness: 120f,
            poweredRotationStrength: 30f,
            poweredRotationVibrato: 70,
            poweredRotationRandomness: 120f,
            burstScale: 1.7f,
            burstScaleTime: 0.1f,
            settleScale: 1.08f,
            settleScaleTime: 0.45f,
            flashColor: Color.white,
            flashMaxAlpha: 0.85f,
            flashFadeInTime: 0.04f,
            flashHoldTime: 0.05f,
            flashFadeOutTime: 0.3f,
            playFlash: true,
            useBlackBars: true,
            blackBarHiddenY: 1920f,
            blackBarVisibleY: 800f
        );
    }

    // ========== 공통 오픈 시퀀스 ==========

    void PlayOpenSequence(
        Sprite startSprite,
        Sprite poweredSprite,
        Sprite openedSprite,
        ParticleSystem[] chargeParticles,
        ParticleSystem[] transitionParticles,
        ParticleSystem[] explodeParticles,
        ParticleSystem loopParticle,
        float chargeScale,
        float chargeTime,
        float chargeShakeStrength,
        int chargeShakeVibrato,
        float chargeShakeRandomness,
        float chargeRotationStrength,
        int chargeRotationVibrato,
        float chargeRotationRandomness,
        float poweredShakeTime,
        float poweredShakeStrength,
        int poweredShakeVibrato,
        float poweredShakeRandomness,
        float poweredRotationStrength,
        int poweredRotationVibrato,
        float poweredRotationRandomness,
        float burstScale,
        float burstScaleTime,
        float settleScale,
        float settleScaleTime,
        Color flashColor,
        float flashMaxAlpha,
        float flashFadeInTime,
        float flashHoldTime,
        float flashFadeOutTime,
        bool playFlash,
        bool useBlackBars,
        float blackBarHiddenY,
        float blackBarVisibleY
    )
    {
        HideAllParticles();
        HideFlashImmediate(0f);

        ResetChest(startSprite, originalPos, Vector3.one, Quaternion.identity, true);

        Sequence seq = DOTween.Sequence();

        // 0) 블랙바 등장 (시네마틱 강조)
        if (useBlackBars)
        {
            seq.AppendCallback(() => ShowBlackBars(barDuration, blackBarHiddenY, blackBarVisibleY));
            seq.AppendInterval(barDuration);
        }

        // 1) 일반 차징 파티클 시작 + 스케일 키우기 + 흔들림
        seq.AppendCallback(() => PlayParticles(chargeParticles));
        seq.Append(PlayChestScale(chargeScale, chargeTime, Ease.InQuad));
        seq.Join(PlayChestShake(chargeTime, chargeShakeStrength, chargeShakeVibrato, chargeShakeRandomness, false, true));
        seq.Join(PlayChestRotationShake(chargeTime, chargeRotationStrength, chargeRotationVibrato, chargeRotationRandomness));

        // 2) 터지기 직전 또는 스페셜 전환 — 스프라이트 변경 + 추가 파티클 + 더 강한 흔들림
        seq.AppendCallback(() =>
        {
            SetChestSprite(poweredSprite);
            PlayParticles(transitionParticles);
        });
        seq.Append(PlayChestShake(poweredShakeTime, poweredShakeStrength, poweredShakeVibrato, poweredShakeRandomness, false, true));
        seq.Join(PlayChestRotationShake(poweredShakeTime, poweredRotationStrength, poweredRotationVibrato, poweredRotationRandomness));

        // 3) 폭발! → 차징 끄고 + 폭발 파티클 + 이미지 변경 + 플래시
        seq.AppendCallback(() =>
        {
            StopParticles(chargeParticles);
            StopParticles(transitionParticles);
            PlayParticles(explodeParticles);
            SetChestSprite(openedSprite);
            PlayFlash(flashColor, flashMaxAlpha, flashFadeInTime, flashHoldTime, flashFadeOutTime, playFlash);
        });

        // 4) 폭발 스케일 연출 (팡! 커졌다가 지정 스케일로 정착)
        seq.Append(PlayChestScale(burstScale, burstScaleTime, Ease.OutQuad));
        seq.Append(PlayChestScale(settleScale, settleScaleTime, Ease.OutBack));

        // 5) 열린 상자 반짝이 루프 시작
        seq.AppendCallback(() => PlayParticle(loopParticle));

        // 6) 블랙바 퇴장
        if (useBlackBars)
        {
            seq.AppendCallback(() => HideBlackBars(barDuration, blackBarHiddenY));
        }

        seq.OnComplete(EndAnimation);
    }

    // ========== 상자 공통 모듈 ==========

    void ResetChest(Sprite sprite, Vector2 anchoredPosition, Vector3 localScale, Quaternion localRotation, bool active)
    {
        if (boxTransform == null || boxImage == null)
        {
            Debug.LogWarning("ParticleScene: boxTransform 또는 boxImage가 연결되지 않아 상자를 초기화할 수 없습니다.");
            return;
        }

        boxTransform.DOKill();
        SetChestSprite(sprite);
        boxTransform.gameObject.SetActive(active);
        boxTransform.localScale = localScale;
        boxTransform.localRotation = localRotation;
        boxTransform.anchoredPosition = anchoredPosition;
    }

    void SetChestSprite(Sprite sprite)
    {
        if (boxImage == null)
        {
            Debug.LogWarning("ParticleScene: boxImage가 연결되지 않아 스프라이트를 변경할 수 없습니다.");
            return;
        }

        if (sprite == null)
        {
            Debug.LogWarning("ParticleScene: 변경하려는 상자 스프라이트가 null입니다.");
            return;
        }

        boxImage.sprite = sprite;
    }

    Tweener PlayChestMove(float targetY, float duration, Ease ease)
    {
        return boxTransform.DOAnchorPosY(targetY, duration).SetEase(ease);
    }

    Tweener PlayChestScale(float targetScale, float duration, Ease ease)
    {
        return boxTransform.DOScale(targetScale, duration).SetEase(ease);
    }

    Tweener PlayChestShake(float duration, float strength, int vibrato, float randomness, bool snapping, bool fadeOut)
    {
        return boxTransform.DOShakePosition(duration, strength, vibrato, randomness, snapping, fadeOut);
    }

    Tweener PlayChestRotationShake(float duration, float strength, int vibrato, float randomness)
    {
        return boxTransform.DOShakeRotation(duration, strength, vibrato, randomness);
    }

    // ========== 화면 플래시 ==========

    void PlayFlash(Color color, float maxAlpha, float fadeInTime, float holdTime, float fadeOutTime, bool shouldPlay)
    {
        if (!shouldPlay) return;

        if (flashImage == null)
        {
            Debug.LogWarning("ParticleScene: flashImage가 연결되지 않아 화면 플래시를 건너뜁니다.");
            return;
        }

        flashImage.DOKill();
        flashImage.gameObject.SetActive(true);

        Color transparentColor = color;
        transparentColor.a = 0f;
        Color visibleColor = color;
        visibleColor.a = maxAlpha;

        flashImage.color = transparentColor;

        Sequence flashSeq = DOTween.Sequence();
        flashSeq.Append(flashImage.DOColor(visibleColor, fadeInTime));
        flashSeq.AppendInterval(holdTime);
        flashSeq.Append(flashImage.DOColor(transparentColor, fadeOutTime));
        flashSeq.OnComplete(() => flashImage.gameObject.SetActive(false));
    }

    void HideFlashImmediate(float alpha)
    {
        if (flashImage == null) return;

        flashImage.DOKill();
        Color color = flashImage.color;
        color.a = alpha;
        flashImage.color = color;
        flashImage.gameObject.SetActive(false);
    }

    // ========== 블랙바 ==========

    void ShowBlackBars(float duration, float hiddenY, float visibleY)
    {
        if (blackBarTop != null)
        {
            blackBarTop.DOKill();
            blackBarTop.gameObject.SetActive(true);
            blackBarTop.anchoredPosition = new Vector2(0, hiddenY);
            blackBarTop.DOAnchorPosY(visibleY, duration).SetEase(Ease.OutQuad);
        }
        if (blackBarBottom != null)
        {
            blackBarBottom.DOKill();
            blackBarBottom.gameObject.SetActive(true);
            blackBarBottom.anchoredPosition = new Vector2(0, -hiddenY);
            blackBarBottom.DOAnchorPosY(-visibleY, duration).SetEase(Ease.OutQuad);
        }
    }

    void HideBlackBars(float duration, float hiddenY)
    {
        if (blackBarTop != null)
            blackBarTop.DOAnchorPosY(hiddenY, duration).SetEase(Ease.InQuad)
                .OnComplete(() => blackBarTop.gameObject.SetActive(false));
        if (blackBarBottom != null)
            blackBarBottom.DOAnchorPosY(-hiddenY, duration).SetEase(Ease.InQuad)
                .OnComplete(() => blackBarBottom.gameObject.SetActive(false));
    }

    void HideBlackBarsImmediate()
    {
        if (blackBarTop != null)
        {
            blackBarTop.DOKill();
            blackBarTop.anchoredPosition = new Vector2(0, barSize);
            blackBarTop.gameObject.SetActive(false);
        }
        if (blackBarBottom != null)
        {
            blackBarBottom.DOKill();
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

    void PlayParticles(ParticleSystem[] particles)
    {
        if (particles == null) return;

        for (int i = 0; i < particles.Length; i++)
        {
            PlayParticle(particles[i]);
        }
    }

    // 정지 + 오브젝트 끄기
    void StopParticle(ParticleSystem ps)
    {
        if (ps == null) return;
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ps.gameObject.SetActive(false);
    }

    void StopParticles(ParticleSystem[] particles)
    {
        if (particles == null) return;

        for (int i = 0; i < particles.Length; i++)
        {
            StopParticle(particles[i]);
        }
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
        StopParticle(specialChargeParticle);
        StopParticle(specialExplodeParticle);
        StopParticle(specialFlashParticle);
    }

    // ========== 애니메이션 상태 ==========

    bool TryBeginAnimation()
    {
        if (isAnimating) return false;

        if (!HasCoreReferences()) return false;

        isAnimating = true;
        SetButtonsInteractable(false);
        return true;
    }

    void EndAnimation()
    {
        isAnimating = false;
        SetButtonsInteractable(true);
    }

    void SetButtonsInteractable(bool interactable)
    {
        if (button1 != null) button1.interactable = interactable;
        if (button2 != null) button2.interactable = interactable;
        if (button3 != null) button3.interactable = interactable;
    }

    bool HasCoreReferences()
    {
        bool hasReferences = true;

        if (boxTransform == null)
        {
            Debug.LogWarning("ParticleScene: boxTransform이 연결되지 않았습니다.");
            hasReferences = false;
        }

        if (boxImage == null)
        {
            Debug.LogWarning("ParticleScene: boxImage가 연결되지 않았습니다.");
            hasReferences = false;
        }

        return hasReferences;
    }
}

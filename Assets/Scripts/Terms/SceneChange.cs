using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// 메뉴 전환 방식 비교 (교육용)
/// Button A: SetActive 방식 — 즉시 전환 (전환 효과 없음)
/// Button B: DOFade 방식  — 페이드 애니메이션 전환
/// </summary>
public class MenuChanger : MonoBehaviour
{
    [Header("패널")]
    public GameObject panelSceneA;
    public GameObject panelSceneB;

    [Header("페이드용 CanvasGroup")]
    public CanvasGroup canvasGroupA;
    public CanvasGroup canvasGroupB;

    [Header("버튼")]
    public Button buttonA;  // SetActive 전환 버튼
    public Button buttonB;  // DOFade 전환 버튼

    [Header("설정")]
    public float fadeDuration = 0.3f;

    private bool isShowingA = true;
    private bool isTransitioning = false;

    void Start()
    {
        // 초기 상태: SceneA만 보이기
        panelSceneA.SetActive(true);
        panelSceneB.SetActive(false);

        if (canvasGroupA != null) canvasGroupA.alpha = 1f;
        if (canvasGroupB != null) canvasGroupB.alpha = 0f;

        // 버튼 이벤트 연결
        buttonA.onClick.AddListener(ChangeBySetActive);
        buttonB.onClick.AddListener(ChangeByDOFade);
    }

    // ===== Button A: SetActive 방식 (즉시 전환) =====
    public void ChangeBySetActive()
    {
        isShowingA = !isShowingA;
        panelSceneA.SetActive(isShowingA);
        panelSceneB.SetActive(!isShowingA);

        // CanvasGroup alpha도 맞춰줌 (DOFade 이후 상태 보정)
        if (canvasGroupA != null) canvasGroupA.alpha = 1f;
        if (canvasGroupB != null) canvasGroupB.alpha = 1f;
    }

    // ===== Button B: DOFade 방식 (페이드 전환) =====
    public void ChangeByDOFade()
    {
        if (isTransitioning) return;
        isTransitioning = true;

        CanvasGroup current = isShowingA ? canvasGroupA : canvasGroupB;
        CanvasGroup next = isShowingA ? canvasGroupB : canvasGroupA;
        GameObject currentObj = isShowingA ? panelSceneA : panelSceneB;
        GameObject nextObj = isShowingA ? panelSceneB : panelSceneA;

        // 현재 패널 페이드아웃 → 다음 패널 페이드인
        current.DOFade(0f, fadeDuration).OnComplete(() =>
        {
            currentObj.SetActive(false);
            nextObj.SetActive(true);
            next.alpha = 0f;
            next.DOFade(1f, fadeDuration)
                .OnComplete(() => isTransitioning = false);
        });

        isShowingA = !isShowingA;
    }
}

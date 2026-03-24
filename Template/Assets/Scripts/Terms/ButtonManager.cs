using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// 버튼 클릭 효과 비교 (교육용)
/// A: 기본 버튼 (효과 없음)
/// B: OutBounce — 통통 튀는 느낌
/// C: OutBack — 쫀득하게 눌렸다 튀어오르는 느낌
/// D: InOutCubic — 부드러운 눌림
/// </summary>
public class ButtonMaanger : MonoBehaviour
{
    [Header("버튼")]
    public Button buttonA;  // 기본
    public Button buttonB;  // OutBounce
    public Button buttonC;  // OutBack
    public Button buttonD;  // InOutCubic


    

    void Start()
    {
        buttonA.onClick.AddListener(OnClickA);
        buttonB.onClick.AddListener(OnClickB);
        buttonC.onClick.AddListener(OnClickC);
        buttonD.onClick.AddListener(OnClickD);
    }

    // ===== A: 기본 버튼 (효과 없음) =====
    public void OnClickA()
    {
        Debug.Log("Button A: 기본 클릭");
    }

    // ===== B: OutBounce — 통통 튀는 느낌 =====
    public void OnClickB()
    {
        buttonB.transform.DOScale(0.85f, 0.08f)
            .SetEase(Ease.InQuad)
            .OnComplete(() =>
                buttonB.transform.DOScale(1f, 0.35f)
                    .SetEase(Ease.OutBounce));
    }

    // ===== C: OutBack — 쫀득한 느낌 =====
    public void OnClickC()
    {
        buttonC.transform.DOScale(0.85f, 0.08f)
            .SetEase(Ease.InQuad)
            .OnComplete(() =>
                buttonC.transform.DOScale(1f, 0.35f)
                    .SetEase(Ease.OutBack));
    }

    // ===== D: InOutCubic — 부드러운 눌림 =====
    public void OnClickD()
    {
        buttonD.transform.DOPunchScale(Vector3.one * 0.2f, 0.4f, 5, 0.5f);
    }
}

# Unity UI Template (교육용)

DOTween을 활용한 UI 연출 비교 학습용 프로젝트입니다.

## 씬 구성

### 1. SceneTransition (메뉴 전환)
패널 전환 방식의 차이를 비교합니다.

| 버튼 | 방식 | 설명 |
|------|------|------|
| Button A | `SetActive` | 즉시 전환 (효과 없음) |
| Button B | `DOFade` | 페이드 애니메이션 전환 |

- 스크립트: `Assets/Scripts/Terms/SceneChange.cs`
- 패널에 **CanvasGroup** 컴포넌트 필요

### 2. Button (버튼 클릭 효과)
DOTween Ease 타입별 클릭 효과를 비교합니다.

| 버튼 | 효과 | 설명 |
|------|------|------|
| A | 없음 | 기본 클릭 (비교 대조군) |
| B | `OutBounce` | 통통 튀는 느낌 |
| C | `OutBack` | 쫀득하게 눌렸다 튀어오름 |
| D | `DOPunchScale` | 부드러운 펑 눌림 |

- 스크립트: `Assets/Scripts/Terms/ButtonManager.cs`

### 3. EffectScene (파티클 + DOTween 연출)
파티클과 DOTween을 조합한 연출을 보여줍니다.

**Button 1 — 등장 연출**
```
상자가 위에서 떨어짐 (OutBounce) → 착지 먼지 파티클 → 부들부들 떨림
```

**Button 2 — 폭발 연출**
```
블랙바 등장 (시네마틱)
→ 차징 파티클 + 스케일 커지면서 부들부들
→ 스프라이트 변경 (금 간 상자) + 더 강하게 떨림
→ 폭발 파티클 2종 + 스프라이트 변경 (열린 상자)
→ 팡! 스케일 펀치 + 글로우 루프 파티클
→ 블랙바 퇴장
```

- 스크립트: `Assets/Scripts/Terms/ParticleScene.cs`
- Canvas를 **Screen Space - Camera** 모드로 설정해야 파티클이 UI 위에 보임
- 스프라이트 3종 필요: 닫힌 상자 / 금 간 상자 / 열린 상자
- 파티클 5종: 차징, 차징 자식, 먼지, 폭발(파편), 폭발(섬광), 글로우 루프

## 사전 요구사항

- Unity 6 (6000.x)
- [DOTween](https://assetstore.unity.com/packages/tools/animation/dotween-hotween-v2-27676) (무료) — import 후 Setup 필요

# CodingRules.md — Three Card Survival

## 네이밍

- 클래스명 / 메서드명 / 변수명: **영어**
- Odin 버튼 라벨, 인스펙터 Title, 한국어 주석: **한국어**
- private 필드: camelCase (언더스코어 없음)
- public 프로퍼티: PascalCase

## 아키텍처 원칙

- **데이터** → `ScriptableObject` (NodeData, EnemyData, SkillData, PlayerData)
- **씬 동작** → `MonoBehaviour` (NodeView, MapManager, MonsterView, BattleManager 등)
- ScriptableObject에 씬 오브젝트 참조(`NodeView`, `MonsterView`) 절대 금지 — MonoBehaviour에 둔다

## Unity 렌더링

- 이 프로젝트는 **URP + 2D Renderer** 사용
- 스프라이트 머티리얼: **Sprite-Lit-Default** (`Sprites/Default` 금지)
- LineRenderer: URP 호환 머티리얼 (`Universal Render Pipeline/Unlit`)
- 에디터 전용 코드는 반드시 `#if UNITY_EDITOR` 감싸기

## Odin Inspector

- 인스펙터 UI에 Odin 적극 활용: `[Required]`, `[ShowIf]`, `[BoxGroup]`, `[Title]`, `[ReadOnly]`, `[EnumToggleButtons]`, `[Button]`
- `[Button]` 라벨은 한국어로 작성

## 코딩 스타일

- Update() 사용 최소화 — 복잡한 상태 머신은 Coroutine 또는 UniTask 선호
- 주석은 WHY가 비명백할 때만 작성. WHAT 설명 주석 금지
- 매직 넘버는 const 또는 SerializeField로 추출
- 불필요한 null 체크 / 방어 코드 지양 — 내부 코드는 신뢰

## 프로젝트 규칙

- 새 기능 구현 전 `ClaudeRules/기획서.txt` 확인 필수
- 기획서와 충돌하는 코드 작성 금지
- 진행 현황은 `ClaudeRules/TODO.md` 참조
- 클래스 구조는 `ClaudeRules/Architecture.md` 참조
- 변경 사항은 TODO.md에 즉시 반영

# Architecture — Three Card Survival

스펙 출처: `ThreeCardSurvival_Spec.md`

---

## 클래스 구조

```
GameManager (MonoBehaviour)
  ├── 상태 머신: PlayerTurn / DayEndRoutine / GameOver / GameClear
  ├── DeckManager
  ├── CardSelector
  ├── EffectResolver
  ├── DayEndRoutine
  └── EscapeSystem

GameState (순수 C# 클래스)
  └── 모든 게임 데이터 보관 (자원, 스탯, 덱, 진행 상태)

CardData (ScriptableObject)
  └── 카드 정의 데이터 (비용, 조건, 효과)
```

---

## 클래스별 책임

### CardData (ScriptableObject)
- 카드 정의 데이터. 씬 참조 금지.
- 핵심 필드: cardType, costResource/costAmount, requiredStat/requiredStatAmount, effectResource/effectAmount, effectStat/effectStatAmount, onRefuseCopyToDesk

### GameState (순수 C#)
- 자원: hp, food, water, stone, wood
- 스탯: strength, agility, intelligence
- 덱: `List<CardData> deck`, `usedCards`, `carriedOver`
- 진행: day, escapeChance, isGameOver, isClear

### DeckManager (MonoBehaviour or 순수 C#)
- GetThreeCards(): carriedOver + 덱 상단 보충 → 최대 3장 반환
- SelectCard(int index): 선택 카드 → usedCards, 나머지 → carriedOver
- AddCardToDeck / RemoveCardFromDeck / MergeCards

### CardSelector (MonoBehaviour)
- UI에 3장 표시
- 선택 가능 여부 판단 (비용 / 스탯 조건 vs GameState)
- 선택 불가 카드 어둡게 표시
- 전원 불가 → GameManager.OnAllCardsUnselectable()

### EffectResolver (MonoBehaviour or 순수 C#)
- ApplyCard(CardData, GameState): 자원/스탯 증감 적용
- 시련 거부 시 OnRefuseCopyToDesk → DeckManager.AddCardToDeck
- SOS: escapeChance 누적
- DayEnd: 세 가지 선택지 분기

### DayEndRoutine (MonoBehaviour)
- 순서 고정: 카드 보상 → 합성 → 강화 → 시련 추가 → 마일스톤 체크 → day+1

### EscapeSystem (MonoBehaviour or 순수 C#)
- TryEscape(GameState): 확률 계산 → 성공/실패 처리
- 실패 패널티: deck 전체 순회 → costAmount +1

### GameManager (MonoBehaviour)
- 상태 전환 제어
- GameState 소유
- 각 시스템 초기화 및 연결

---

## 씬 구성 (MainScene)

```
[Canvas]
  ├── TopHUD          — Day / 자원 / 스탯 / 탈출확률
  ├── CardArea        — 카드 3장 (CardSelector 관할)
  └── BottomInfo      — 덱 수 / 이월 수

[GameManager]           — 게임 흐름 제어
[DeckManager]
[CardSelector]
[EffectResolver]
[DayEndRoutineController]
[EscapeSystem]
```

---

## 데이터 흐름

```
CardData (SO) ──► DeckManager ──► CardSelector (UI)
                                        │
                                   플레이어 선택
                                        │
                                  EffectResolver
                                        │
                                   GameState 갱신
                                        │
                                   GameManager
                            (상태 전환 / 게임오버 체크)
```

---

## 열거형 정의

```csharp
enum CardType    { Resource, Stat, Trial, DayEnd, SOS }
enum ResourceType { None, HP, Food, Water, Stone, Wood }
enum StatType    { None, Strength, Agility, Intelligence }
```

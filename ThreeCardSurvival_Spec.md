# Three Card Survival — Unity 프로토타입 스펙

> 이 문서는 Claude Code에서 Unity 프로토타입을 구현하기 위한 개발 스펙입니다.  
> GDD 확정 항목만 구현합니다. UI는 최소한으로, 동작 검증이 목표입니다.

---

## 1. 프로젝트 세팅

- **Unity 버전**: 2022.3 LTS 이상
- **렌더 파이프라인**: URP (또는 Built-in, 무관)
- **씬 구성**: 씬 1개로 전체 게임 구성 (MainScene)
- **UI**: Unity uGUI 사용 (TextMeshPro 포함)

---

## 2. 데이터 구조

### 2.1 CardData (ScriptableObject)

```csharp
public enum CardType
{
    Resource,   // 자원 카드
    Stat,       // 스탯 카드
    Trial,      // 시련 카드
    DayEnd,     // 하루의 끝 카드
    SOS         // SOS 카드
}

public enum ResourceType
{
    None,
    HP, Food, Water, Stone, Wood
}

public enum StatType
{
    None,
    Strength, Agility, Intelligence
}

[CreateAssetMenu]
public class CardData : ScriptableObject
{
    public string cardName;
    public CardType cardType;
    public string description;

    // 비용 (선택 불가 조건 — 비용 방식)
    public ResourceType costResource;
    public int costAmount;

    // 조건 (선택 불가 조건 — 조건 방식)
    public StatType requiredStat;
    public int requiredStatAmount;

    // 효과
    public ResourceType effectResource;
    public int effectAmount;       // 양수: 획득, 음수: 소모
    public StatType effectStat;
    public int effectStatAmount;

    // 시련 카드 전용: 거부 시 패널티
    public bool onRefuseCopyToDesk;  // 거부 시 덱에 복사
}
```

### 2.2 GameState

```csharp
public class GameState
{
    // 자원
    public int hp;
    public int food;
    public int water;
    public int stone;
    public int wood;

    // 스탯
    public int strength;
    public int agility;
    public int intelligence;

    // 덱 상태
    public List<CardData> deck;         // 현재 덱
    public List<CardData> usedCards;    // 사용한 카드 (셔플 대상 아님 — 덱 소진 시 이월만으로 진행)
    public List<CardData> carriedOver;  // 이월 카드 (선택 안 한 2장)

    // 진행 상태
    public int day;
    public float escapeChance;          // 탈출 확률 (0.0 ~ 1.0)
    public bool isGameOver;
    public bool isClear;
}
```

---

## 3. 시스템 구성

### 3.1 DeckManager

덱과 이월 카드를 관리한다.

```
책임:
- 매 턴 3장 공개 (이월 카드 + 덱에서 새 카드로 채움)
- 덱이 비었으면 이월 카드만으로 구성
- 카드 선택 시: 선택한 카드 → usedCards, 나머지 2장 → carriedOver
- 덱 소진 판단

공개 메서드:
- List<CardData> GetThreeCards()
- void SelectCard(int index)         // 0, 1, 2 중 선택
- void AddCardToDeck(CardData card)
- void RemoveCardFromDeck(CardData card)
- void MergeCards(CardData a, CardData b, CardData result)
```

**GetThreeCards() 로직:**
```
1. hand = carriedOver (이월 카드)
2. 덱에 카드가 남아있으면: (3 - hand.Count)장을 덱 상단에서 뽑아 hand에 추가
3. 덱이 비었으면: hand에 있는 카드만으로 구성
4. hand를 반환 (최대 3장, 최소 1장)
```

### 3.2 CardSelector

매 턴 UI에 3장을 표시하고 플레이어 선택을 받는다.

```
책임:
- 3장의 카드 UI 표시
- 각 카드의 선택 가능 여부 판단 (비용/조건 체크)
- 선택 불가 카드 시각적 비활성화 표시
- 3장 모두 선택 불가 시 → GameManager에 게임오버 신호
- 카드 선택 시 → EffectResolver에 효과 처리 요청

선택 가능 조건 체크:
- costResource가 None이 아니면: GameState의 해당 자원 >= costAmount
- requiredStat이 None이 아니면: GameState의 해당 스탯 >= requiredStatAmount
```

### 3.3 EffectResolver

선택된 카드의 효과를 GameState에 반영한다.

```
책임:
- 자원 증감 처리
- 스탯 증감 처리
- 시련 카드 거부 패널티 처리 (덱에 복사)
- SOS 카드 효과 처리 (탈출 확률 누적)
- 하루의 끝 카드 효과 처리

하루의 끝 카드 선택지:
  1. 하루를 끝내기   → DayEndRoutine 실행
  2. 계속 진행하기   → 하루의 끝 카드를 덱에 다시 섞고 턴 계속
  3. 탈출 시도       → EscapeSystem.TryEscape() 호출
```

### 3.4 DayEndRoutine

하루 종료 시 3단계 루틴을 순서대로 진행한다.

```
순서:
1. 카드 보상   — 새 카드 3장 제시 → 삼중택일 → 선택한 카드 덱에 추가
2. 카드 합성   — 덱에서 카드 2장 선택 → 1장으로 합성 → 결과 카드 덱에 추가
3. 카드 강화   — 덱에서 카드 1장 선택 → 강화 (미정: 효과량 +1 등으로 임시 구현)
4. 시련 추가   — 랜덤 시련 카드 1장 덱에 추가
5. 마일스톤 체크 — 해당 일차 특수 카드 추가 (5일차: SOS 카드)
6. day + 1 → 다음 날 시작
```

### 3.5 EscapeSystem

탈출 시도를 처리한다.

```
TryEscape():
1. 현재 탈출 확률(escapeChance) 계산
   - 기반: GameState.escapeChance (SOS 카드 누적치)
   - 보정: 자원 합산 / 스탯 합산에 비례 (임시 공식 사용)
2. Random.value < escapeChance 이면 → 게임 클리어
3. 실패 시 → 이후 모든 카드 costAmount +1 (리스크 부여, 임시 구현)

임시 탈출 확률 공식 (프로토타입용):
escapeChance += (strength + agility + intelligence) * 0.01f
escapeChance += (food + water) * 0.005f
```

### 3.6 GameManager

전체 게임 흐름을 관리한다.

```
상태 머신:
- PlayerTurn       : 삼중택일 진행
- DayEndRoutine    : 하루 종료 루틴
- GameOver         : 게임오버 화면
- GameClear        : 클리어 화면

흐름:
Start → 초기 덱 생성 → PlayerTurn
PlayerTurn → 카드 선택 → 효과 처리 → 게임오버 체크 → PlayerTurn 반복
              └─ 하루의 끝 선택 시 → DayEndRoutine or EscapeSystem
DayEndRoutine 완료 → PlayerTurn
```

---

## 4. 초기 덱 구성 (스타터 덱)

| 카드명 | 타입 | 수량 | 효과 |
|---|---|---|---|
| 식량 수집 | Resource | 3 | Food +2 |
| 물 수집 | Resource | 3 | Water +2 |
| 채석 | Resource | 2 | Stone +2 |
| 벌목 | Resource | 2 | Wood +2 |
| 하루의 끝 | DayEnd | 1 | 하루 종료 트리거 |

**총 11장으로 시작**

---

## 5. 카드 목록 (프로토타입)

### 자원 카드

| 카드명 | 비용 | 효과 |
|---|---|---|
| 식량 수집 | 없음 | Food +2 |
| 물 수집 | 없음 | Water +2 |
| 채석 | 없음 | Stone +2 |
| 벌목 | 없음 | Wood +2 |

### 시련 카드

| 카드명 | 비용(선택 비용) | 거부 시 패널티 |
|---|---|---|
| 허기 | Food -3 | 덱에 복사 |
| 갈증 | Water -3 | 덱에 복사 |
| 부상 | HP -3 | 덱에 복사 |

### 특수 카드

| 카드명 | 타입 | 효과 |
|---|---|---|
| 하루의 끝 | DayEnd | 삼중택일 (끝내기 / 계속 / 탈출시도) |
| SOS | SOS | Stone -2, Wood -2 소모 → escapeChance +0.1 |

### 카드 보상 풀 (하루 종료 시 제시)

| 카드명 | 타입 | 효과 |
|---|---|---|
| 사냥 | Resource | Food +4 (Strength >= 2 조건) |
| 낚시 | Resource | Food +3, Water +1 |
| 빗물 수집 | Resource | Water +4 |
| 힘 훈련 | Stat | Strength +1 |
| 민첩 훈련 | Stat | Agility +1 |
| 관찰 | Stat | Intelligence +1 |

---

## 6. 초기값

```
HP      : 10
Food    : 5
Water   : 5
Stone   : 0
Wood    : 0

Strength    : 1
Agility     : 1
Intelligence: 1

Day         : 1
EscapeChance: 0.0f
```

---

## 7. UI 구성 (최소)

```
[ 상단 ]
- 현재 날짜 (Day N)
- 자원 현황: HP / Food / Water / Stone / Wood
- 스탯 현황: STR / AGI / INT
- 탈출 확률: N%

[ 중앙 ]
- 카드 3장 표시 (카드명 + 효과 텍스트 + 선택 가능 여부)
- 선택 불가 카드: 어둡게 표시

[ 하단 ]
- 현재 덱 카드 수 표시
- 이월 카드 수 표시
```

---

## 8. 구현 순서 (권장)

1. `CardData` ScriptableObject 정의 및 카드 에셋 생성
2. `GameState` 클래스 구현
3. `DeckManager` 구현 및 테스트
4. `CardSelector` UI 연결 및 선택 가능 여부 판단
5. `EffectResolver` 구현
6. `GameManager` 상태 머신 구현
7. `DayEndRoutine` 구현
8. `EscapeSystem` 구현
9. 게임오버 / 클리어 화면
10. 전체 루프 통합 테스트

---

## 9. 프로토타입 제외 항목

- 카드 애니메이션 / 비주얼 이펙트
- 사운드
- 카드 강화 상세 구현 (임시: 효과량 +1)
- 스탯 공식 밸런싱
- 10일차, 15일차 마일스톤
- 메타 진행 시스템
- 세이브/로드

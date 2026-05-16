# TODO — Three Card Survival

구현 순서는 `ThreeCardSurvival_Spec.md` 섹션 8 기준.

---

## 구현 체크리스트

- [ ] 1. `CardData` ScriptableObject 정의 및 카드 에셋 생성
  - CardType, ResourceType, StatType 열거형
  - 스타터 덱 11장 에셋 생성
  - 시련 카드 3종 / SOS 카드 / 보상 카드 6종 에셋 생성

- [ ] 2. `GameState` 클래스 구현
  - 자원 (HP, Food, Water, Stone, Wood)
  - 스탯 (Strength, Agility, Intelligence)
  - 덱 상태 (deck, usedCards, carriedOver)
  - 진행 상태 (day, escapeChance, isGameOver, isClear)

- [ ] 3. `DeckManager` 구현 및 테스트
  - GetThreeCards() — 이월 카드 + 덱 보충 로직
  - SelectCard(int index)
  - AddCardToDeck / RemoveCardFromDeck / MergeCards

- [ ] 4. `CardSelector` UI 연결
  - 3장 카드 UI 표시
  - 선택 가능 여부 판단 (비용 / 조건 체크)
  - 선택 불가 카드 시각적 비활성화
  - 3장 모두 불가 시 GameManager에 게임오버 신호

- [ ] 5. `EffectResolver` 구현
  - 자원 / 스탯 증감
  - 시련 카드 거부 패널티 (덱에 복사)
  - SOS 카드 처리 (escapeChance 누적)
  - 하루의 끝 선택지 처리 (끝내기 / 계속 / 탈출)

- [ ] 6. `GameManager` 상태 머신 구현
  - PlayerTurn / DayEndRoutine / GameOver / GameClear 상태
  - 게임오버 체크 (HP <= 0 / 전원 선택 불가)

- [ ] 7. `DayEndRoutine` 구현
  - 카드 보상 삼중택일
  - 카드 합성 (2장 → 1장)
  - 카드 강화 (효과량 +1, 임시)
  - 시련 카드 추가
  - 마일스톤 체크 (5일차: SOS 추가)

- [ ] 8. `EscapeSystem` 구현
  - TryEscape() — 확률 계산 및 성공/실패 처리
  - 실패 패널티: 모든 카드 costAmount +1

- [ ] 9. 게임오버 / 클리어 화면

- [ ] 10. 전체 루프 통합 테스트

---

## 완료된 작업

(없음 — 프로젝트 시작)

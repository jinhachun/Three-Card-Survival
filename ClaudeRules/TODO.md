# TODO — Three Card Survival

---

## 완료된 작업

### 코어 시스템
- [x] CardData ScriptableObject + 카드 에셋 생성 (CardDataCreator / CsvCardImporter)
- [x] GameState (자원, 스탯, 덱 상태, 진행 상태, 건물 상태, costEscalations)
- [x] DeckManager (GetThreeCards, SelectCard, 이월 로직, 중첩 표시)
- [x] CardSelector (3장 표시, 비용/조건 체크, 딜 애니메이션)
- [x] EffectResolver (자원/스탯/건물 효과, 시련 패널티, 탈출 패널티)
- [x] GameManager 상태 머신 (PlayerTurn / DayEndChoice / DayEndRoutine / GameOver / GameClear)
- [x] DayEndRoutine (카드 보상 삼중택일, 시련 추가, 마일스톤, HP 회복)
- [x] EscapeSystem (확률 계산, 실패 패널티 누적)
- [x] CardView UI (카드 이름/타입/비용/조건/효과 표시, 중첩 배지, 딜 애니메이션, 호버)
- [x] HUDController (자원/스탯/일차 표시, 변화 애니메이션)
- [x] DeckPileView (덱 카드 수 표시, 카드 추가 토스트 애니메이션)

### 카드 시스템
- [x] Cards.csv 단일 소스 관리 + CsvCardImporter 에디터 툴
- [x] CardType: Resource / Building / Trial / DayEnd / SOS
- [x] minDay 필드 (최소 등장 일차)
- [x] requiredBuilding 필드 (건물 완성 후 보상 풀 등장)
- [x] escalatesOnDraw 필드 (손패 등장 시 비용 영구 +1)
- [x] Sprite 필드 (CSV Sprite 컬럼 → Assets/Resources/Sprites/ 자동 연결)
- [x] 시련 카드 미처리 시 HP -1/card (DayEnd 선택 시 패널티 없음)
- [x] 보상 풀 필터링 (minDay + requiredBuilding + 완성 건물 카드 제외)
- [x] 시련 풀도 minDay 필터링 적용
- [x] Starvation / Dehydration 에스컬레이팅 시련 카드 (minDay=3)

### 건물 시스템
- [x] BuildingData ScriptableObject (buildCosts, progressPerUse, completionStatGain, passiveResource, unlocksCards)
- [x] BuildingRegistry ScriptableObject
- [x] Buildings.csv + CsvBuildingImporter (BuildingData 에셋 + Build X 카드 자동 생성, 단일 임포트로 정상 동작)
- [x] 6개 건물: Well / Campfire / Infirmary (패시브) + Gym / Training Ground / Library (스탯+해금)
- [x] 건물 완성 시 카드 덱에서 제거
- [x] GameState: buildingProgress (Dictionary<string,int>) + completedBuildings (HashSet<string>)

### 난이도 시스템
- [x] 하루 종료마다 시련 카드 추가 수 일차별 증가 (Day 1~3: 1장, Day 4~6: 2장, Day 7+: 3장)
- [x] EndOfDay 카드는 항상 선택 가능 (게임오버 버그 수정)
- [x] costEscalations: 에스컬레이팅 카드 비용 영구 증가 추적

### 비주얼 피드백
- [x] 카드 딜 애니메이션 (덱에서 날아오는 효과)
- [x] 건물 완성 팝업 (BuildingCompletePopup + DOTween)
- [x] 패시브 발동 플로팅 텍스트 (PassiveNotifier)
- [x] HUD 수치 변화 애니메이션 (Flash + DOPunchScale)
- [x] Day 전환 알림 텍스트
- [x] 게임오버 / 클리어 화면 UI + 리스타트

### 기타
- [x] CLAUDE.md 인게임 텍스트 영어 규칙 명시
- [x] PanelAnimator (패널 오픈/클로즈 DOTween)
- [x] GitRules.md + dev 브랜치 운영

---

## 남은 작업

### 밸런싱
- [ ] 시련 카드 누적 속도 플레이테스트 (Day 4부터 2장 추가)
- [ ] 건물 비용 및 진행도 밸런싱
- [ ] 탈출 확률 체감 검토

### 콘텐츠
- [ ] 카드 이미지 (Assets/Resources/Sprites/ 에 배치 후 CSV Sprite 컬럼 입력)

### 추후 고려
- [ ] 카드 합성 / 강화 UI
- [ ] 사운드
- [ ] 10일차 / 15일차 마일스톤
- [ ] 세이브 / 로드

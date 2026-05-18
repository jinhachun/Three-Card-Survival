# TODO — Three Card Survival

---

## 완료된 작업

### 코어 시스템
- [x] CardData ScriptableObject + 카드 에셋 생성 (CardDataCreator / CsvCardImporter)
- [x] GameState (자원, 스탯, 덱 상태, 진행 상태, 건물 상태)
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
- [x] 시련 카드 미처리 시 HP -1/card (덱 복사 방식 대체)
- [x] 보상 풀 필터링 (minDay + requiredBuilding + 완성 건물 카드 제외)

### 건물 시스템
- [x] BuildingData ScriptableObject (buildCosts, progressPerUse, completionStatGain, passiveResource, unlocksCards)
- [x] BuildingRegistry ScriptableObject
- [x] Buildings.csv + CsvBuildingImporter (BuildingData 에셋 + Build X 카드 자동 생성)
- [x] 6개 건물: Well / Campfire / Infirmary (패시브) + Gym / Training Ground / Library (스탯+해금)
- [x] 건물 완성 시 카드 덱에서 제거
- [x] GameState: buildingProgress (Dictionary<string,int>) + completedBuildings (HashSet<string>)

### 비주얼 피드백
- [x] 카드 딜 애니메이션 (덱에서 날아오는 효과)
- [x] 건물 완성 팝업 (BuildingCompletePopup + DOTween)
- [x] 패시브 발동 플로팅 텍스트 (PassiveNotifier)
- [x] HUD 수치 변화 애니메이션 (Flash + DOPunchScale)
- [x] Day 전환 알림 텍스트

### 기타
- [x] CLAUDE.md 인게임 텍스트 영어 규칙 명시
- [x] PanelAnimator (패널 오픈/클로즈 DOTween)

---

## 남은 작업

### 필수
- [ ] BuildingCompletePopup 씬 배치 및 GameManager 연결
- [ ] PassiveNotifier 씬 배치 및 GameManager 연결
- [ ] EffectResolver / HUDController 인스펙터에 BuildingRegistry 연결

### 게임플레이 개선
- [ ] 탈출 확률 공식 재설계 (현재 SOS만으로 누적)
- [ ] 게임오버 / 클리어 화면 UI 완성
- [ ] 카드 이미지 (현재 플레이스홀더)

### 밸런싱
- [ ] 건물 비용 및 진행도 밸런싱 플레이테스트
- [ ] 시련 카드 HP 드레인 속도 검토
- [ ] 보상 카드 등장 빈도 조정

### 추후 고려
- [ ] 카드 합성 / 강화 UI
- [ ] 사운드
- [ ] 10일차 / 15일차 마일스톤
- [ ] 세이브 / 로드

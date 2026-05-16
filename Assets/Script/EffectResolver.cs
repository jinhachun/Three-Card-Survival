using UnityEngine;

public class EffectResolver : MonoBehaviour
{
    private DeckManager _deckManager;

    public void Init(DeckManager deckManager) => _deckManager = deckManager;

    // 모든 카드 타입에 동일하게 적용: 비용 지불 → 효과 실행
    // DayEnd 카드는 GameManager가 직접 처리하므로 여기로 오지 않음
    public void ApplyCard(CardData card, int count, GameState state)
    {
        foreach (var cost in card.costs)
            state.AddResource(cost.resource, -(cost.amount * count) - state.costPenalty);

        for (int i = 0; i < count; i++)
            foreach (var effect in card.effects)
                effect.Apply(state);
    }

    // 시련 카드 거부 시: 덱에 복사본 추가
    public void OnRefuseTrial(CardData card, GameState state)
    {
        if (card.onRefuseCopyToDeck)
            _deckManager.AddCardToDeck(CardUtils.Clone(card));
    }

    // 탈출 실패 패널티: SO 수정 없이 전역 패널티 누적
    public void ApplyEscapeFailurePenalty(GameState state) => state.costPenalty += 1;
}

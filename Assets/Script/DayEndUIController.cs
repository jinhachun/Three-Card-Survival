using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

// DayEndRoutineController 이벤트를 받아 UI 패널을 제어.
public class DayEndUIController : MonoBehaviour
{
    [Required] [SerializeField] private DayEndRoutineController dayEndRoutine;
    [Required] [SerializeField] private CardListPanel rewardPanel;
    [Required] [SerializeField] private CardListPanel mergePanel;
    [Required] [SerializeField] private CardListPanel enhancePanel;

    void OnEnable()
    {
        dayEndRoutine.OnRequestRewardChoice  += ShowRewardPanel;
        dayEndRoutine.OnRequestMergeChoice   += ShowMergePanel;
        dayEndRoutine.OnRequestEnhanceChoice += ShowEnhancePanel;
    }

    void OnDisable()
    {
        dayEndRoutine.OnRequestRewardChoice  -= ShowRewardPanel;
        dayEndRoutine.OnRequestMergeChoice   -= ShowMergePanel;
        dayEndRoutine.OnRequestEnhanceChoice -= ShowEnhancePanel;
    }

    private void ShowRewardPanel(List<CardData> cards, System.Action<int> onSelected)
        => rewardPanel.Show("Reward — Pick 1", cards, 1, indices => onSelected(indices[0]));

    private void ShowMergePanel(List<CardData> cards, System.Action<int, int> onSelected)
        => mergePanel.Show("Merge — Pick 2", cards, 2, indices => onSelected(indices[0], indices[1]));

    private void ShowEnhancePanel(List<CardData> cards, System.Action<int> onSelected)
        => enhancePanel.Show("Enhance — Pick 1", cards, 1, indices => onSelected(indices[0]));
}

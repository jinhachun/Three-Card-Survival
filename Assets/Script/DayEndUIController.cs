using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class DayEndUIController : MonoBehaviour
{
    [Required] [SerializeField] private DayEndRoutineController dayEndRoutine;
    [Required] [SerializeField] private CardListPanel rewardPanel;
    [Required] [SerializeField] private CardListPanel mergePanel;

    void OnEnable()  => dayEndRoutine.OnRequestRewardChoice += ShowRewardPanel;
    void OnDisable() => dayEndRoutine.OnRequestRewardChoice -= ShowRewardPanel;

    private void ShowRewardPanel(List<CardData> cards, System.Action<int> onSelected)
        => rewardPanel.Show("Reward — Pick 1", cards, 1, indices => onSelected(indices[0]));
}

using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class DeckPileView : MonoBehaviour
{
    [Required] [SerializeField] private TextMeshProUGUI countText;

    public void UpdateCount(int count)
    {
        countText.text = count.ToString();
    }
}

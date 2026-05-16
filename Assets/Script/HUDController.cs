using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class HUDController : MonoBehaviour
{
    [BoxGroup("진행"), Required] [SerializeField] private TMP_Text dayText;
    [BoxGroup("진행"), Required] [SerializeField] private TMP_Text escapeChanceText;

    [BoxGroup("자원"), Required] [SerializeField] private TMP_Text hpText;
    [BoxGroup("자원"), Required] [SerializeField] private TMP_Text foodText;
    [BoxGroup("자원"), Required] [SerializeField] private TMP_Text waterText;
    [BoxGroup("자원"), Required] [SerializeField] private TMP_Text stoneText;
    [BoxGroup("자원"), Required] [SerializeField] private TMP_Text woodText;

    [BoxGroup("스탯"), Required] [SerializeField] private TMP_Text strengthText;
    [BoxGroup("스탯"), Required] [SerializeField] private TMP_Text agilityText;
    [BoxGroup("스탯"), Required] [SerializeField] private TMP_Text intelligenceText;

    [BoxGroup("덱"), Required] [SerializeField] private TMP_Text deckCountText;
    [BoxGroup("덱"), Required] [SerializeField] private TMP_Text carriedOverCountText;

    private GameState _state;

    public void Init(GameState state) => _state = state;

    public void Refresh()
    {
        if (_state == null) return;

        dayText.text          = $"Day {_state.day}";
        escapeChanceText.text = $"Escape {_state.escapeChance:P0}";

        hpText.text    = $"HP {_state.hp}";
        foodText.text  = $"Food {_state.food}";
        waterText.text = $"Water {_state.water}";
        stoneText.text = $"Stone {_state.stone}";
        woodText.text  = $"Wood {_state.wood}";

        strengthText.text     = $"STR {_state.strength}";
        agilityText.text      = $"AGI {_state.agility}";
        intelligenceText.text = $"INT {_state.intelligence}";

        deckCountText.text        = $"Deck {_state.deck.Count}";
        carriedOverCountText.text = $"Carry {_state.carriedOver.Count}";
    }
}

using System.Text;
using DG.Tweening;
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

    // 건물 패시브 표시 — 씬에 배치 후 연결 (미연결 시 무시)
    [BoxGroup("건물")] [SerializeField] private TMP_Text       buildingsText;
    [BoxGroup("건물")] [SerializeField] private BuildingRegistry buildingRegistry;

    private static readonly Color ColGain = new(0.35f, 1f, 0.40f);
    private static readonly Color ColLoss = new(1f,    0.28f, 0.28f);

    private GameState _state;

    // 이전 프레임 값 — 변화 감지용
    private int   _prevHp, _prevFood, _prevWater, _prevStone, _prevWood;
    private int   _prevStr, _prevAgi, _prevInt, _prevDay;
    private float _prevEscape;

    public void Init(GameState state)
    {
        _state = state;
        // 첫 Refresh에서 애니메이션이 트리거되지 않도록 prev 초기화
        _prevHp     = state.hp;     _prevFood  = state.food;  _prevWater = state.water;
        _prevStone  = state.stone;  _prevWood  = state.wood;
        _prevStr    = state.strength; _prevAgi = state.agility; _prevInt = state.intelligence;
        _prevDay    = state.day;    _prevEscape = state.escapeChance;
    }

    public void Refresh()
    {
        if (_state == null) return;

        // Day — 새 날이면 크게 튀김
        AnimInt(dayText, $"Day {_state.day}", _prevDay, _state.day, big: true);
        _prevDay = _state.day;

        // Escape — float 변화 감지
        float esc = _state.escapeChance;
        escapeChanceText.text = $"Escape {esc:P0}";
        if (!Mathf.Approximately(esc, _prevEscape))
            Flash(escapeChanceText, esc > _prevEscape);
        _prevEscape = esc;

        // 자원
        AnimInt(hpText,    $"HP {_state.hp}",      _prevHp,    _state.hp);    _prevHp    = _state.hp;
        AnimInt(foodText,  $"Food {_state.food}",   _prevFood,  _state.food);  _prevFood  = _state.food;
        AnimInt(waterText, $"Water {_state.water}",  _prevWater, _state.water); _prevWater = _state.water;
        AnimInt(stoneText, $"Stone {_state.stone}",  _prevStone, _state.stone); _prevStone = _state.stone;
        AnimInt(woodText,  $"Wood {_state.wood}",    _prevWood,  _state.wood);  _prevWood  = _state.wood;

        // 스탯
        AnimInt(strengthText,     $"STR {_state.strength}",     _prevStr, _state.strength); _prevStr = _state.strength;
        AnimInt(agilityText,      $"AGI {_state.agility}",      _prevAgi, _state.agility);  _prevAgi = _state.agility;
        AnimInt(intelligenceText, $"INT {_state.intelligence}", _prevInt, _state.intelligence); _prevInt = _state.intelligence;

        deckCountText.text        = $"Deck {_state.deck.Count}";
        carriedOverCountText.text = $"Carry {_state.carriedOver.Count}";

        if (buildingsText != null && buildingRegistry != null)
            buildingsText.text = BuildBuildingsText(_state);
    }

    private string BuildBuildingsText(GameState state)
    {
        if (buildingRegistry == null) return "";
        var sb = new StringBuilder();
        foreach (var b in buildingRegistry.buildings)
        {
            if (state.IsBuildingComplete(b.buildingName))
            {
                string desc = b.passiveResource != ResourceType.None
                    ? $"{b.passiveResource} +{b.passiveAmount}/turn"
                    : b.completionStatGain != StatType.None
                        ? $"{b.completionStatGain} +{b.completionStatAmount}"
                        : "";
                sb.AppendLine($"{b.buildingName}: {desc}");
            }
            else if (state.GetBuildingProgress(b.buildingName) > 0)
                sb.AppendLine($"{b.buildingName}: {state.GetBuildingProgress(b.buildingName)}%");
        }
        return sb.Length > 0 ? sb.ToString().TrimEnd() : "";
    }

    // ── helpers ──────────────────────────────────────────────────────

    private void AnimInt(TMP_Text text, string str, int prev, int cur, bool big = false)
    {
        text.text = str;
        if (prev == cur) return;
        Flash(text, cur > prev, big);
    }

    private void Flash(TMP_Text text, bool gain, bool big = false)
    {
        DOTween.Kill(text.transform);
        DOTween.Kill(text);

        text.color = gain ? ColGain : ColLoss;
        text.transform.DOPunchScale(Vector3.one * (big ? 0.55f : 0.3f), 0.38f, big ? 8 : 5, 0.5f);
        DOTween.To(() => text.color, c => text.color = c, Color.white, 0.5f).SetTarget(text);
    }
}

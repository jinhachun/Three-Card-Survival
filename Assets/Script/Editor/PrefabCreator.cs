#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class PrefabCreator
{
    private const string PrefabPath = "Assets/Prefabs";

    // ──────────────────────────────────────────────
    // CardView
    // ──────────────────────────────────────────────
    [MenuItem("ThreeCardSurvival/프리팹 생성/CardView")]
    public static void CreateCardViewPrefab()
    {
        // 루트: 컨테이너만 (Image 없음 → StackShadow가 CardFace 뒤에 렌더링되도록)
        var root = new GameObject("CardView");
        var rootRect = root.AddComponent<RectTransform>();
        rootRect.sizeDelta = new Vector2(160, 260);
        var canvasGroup = root.AddComponent<CanvasGroup>();
        var button      = root.AddComponent<Button>();
        root.AddComponent<CardView>();

        // ── StackShadow (first child → CardFace 뒤에 렌더링)
        var shadowObj = new GameObject("StackShadow");
        shadowObj.transform.SetParent(root.transform, false);
        var shadowImg = shadowObj.AddComponent<Image>();
        shadowImg.color = new Color(0.60f, 0.65f, 0.80f, 1f);
        var shadowRect = shadowObj.GetComponent<RectTransform>();
        shadowRect.anchorMin = Vector2.zero;
        shadowRect.anchorMax = Vector2.one;
        shadowRect.offsetMin = new Vector2(6, -6);
        shadowRect.offsetMax = new Vector2(6, -6); // +6 right, -6 down → 왼쪽·위 카드 뒤에 비어져 보임
        shadowObj.SetActive(false);

        // ── CardFace (second child → 컨텐츠 배경)
        var face = new GameObject("CardFace");
        face.transform.SetParent(root.transform, false);
        var faceImg = face.AddComponent<Image>();
        faceImg.color = Color.white;
        var faceRect = face.GetComponent<RectTransform>();
        faceRect.anchorMin = Vector2.zero;
        faceRect.anchorMax = Vector2.one;
        faceRect.offsetMin = Vector2.zero;
        faceRect.offsetMax = Vector2.zero;

        // 이름 (상단 14%)
        var nameObj  = CreateTMP(face, "NameText");
        var nameText = nameObj.GetComponent<TextMeshProUGUI>();
        nameText.text      = "Card Name";
        nameText.alignment = TextAlignmentOptions.Center;
        nameText.fontSize  = 16;
        nameText.fontStyle = FontStyles.Bold;
        nameText.color     = Color.black;
        SetAnchors(nameObj, 0f, 0.86f, 1f, 1f, 8, 2, -8, -4);

        // 종류 배지 (78-86%)
        var badgeObj = new GameObject("TypeBadge");
        badgeObj.transform.SetParent(face.transform, false);
        var typeBg = badgeObj.AddComponent<Image>();
        typeBg.color = new Color(0.22f, 0.55f, 0.22f);
        SetAnchors(badgeObj, 0f, 0.78f, 1f, 0.86f, 0, 0, 0, 0);

        var typeTextObj = CreateTMP(badgeObj, "TypeText");
        var typeText    = typeTextObj.GetComponent<TextMeshProUGUI>();
        typeText.text      = "";
        typeText.alignment = TextAlignmentOptions.Center;
        typeText.fontSize  = 11;
        typeText.color     = Color.white;
        SetAnchors(typeTextObj, 0f, 0f, 1f, 1f, 4, 0, -4, 0);

        // 카드 이미지 (50-78%)
        var imgObj  = new GameObject("CardImage");
        imgObj.transform.SetParent(face.transform, false);
        var cardImg = imgObj.AddComponent<Image>();
        cardImg.color   = new Color(0.85f, 0.85f, 0.85f);
        cardImg.enabled = false;
        SetAnchors(imgObj, 0.05f, 0.50f, 0.95f, 0.78f, 0, 0, 0, 0);

        // 비용 (빨강, 33-50%)
        var costObj  = CreateTMP(face, "CostText");
        var costText = costObj.GetComponent<TextMeshProUGUI>();
        costText.text      = "";
        costText.alignment = TextAlignmentOptions.Center;
        costText.fontSize  = 12;
        costText.color     = new Color(0.85f, 0.15f, 0.15f);
        SetAnchors(costObj, 0f, 0.33f, 1f, 0.50f, 8, 2, -8, -2);

        // 요구조건 (빨강, 17-33%)
        var condObj  = CreateTMP(face, "ConditionText");
        var condText = condObj.GetComponent<TextMeshProUGUI>();
        condText.text      = "";
        condText.alignment = TextAlignmentOptions.Center;
        condText.fontSize  = 12;
        condText.color     = new Color(0.85f, 0.15f, 0.15f);
        SetAnchors(condObj, 0f, 0.17f, 1f, 0.33f, 8, 2, -8, -2);

        // 효과 (0-17%)
        var effectObj  = CreateTMP(face, "EffectText");
        var effectText = effectObj.GetComponent<TextMeshProUGUI>();
        effectText.text      = "";
        effectText.alignment = TextAlignmentOptions.Center;
        effectText.fontSize  = 12;
        effectText.color     = Color.black;
        SetAnchors(effectObj, 0f, 0f, 1f, 0.17f, 8, 4, -8, -2);

        // ── CountBadge (×2 표시, 루트 기준 우상단)
        var badgeCountObj = CreateTMP(root, "CountBadge");
        var countBadge    = badgeCountObj.GetComponent<TextMeshProUGUI>();
        countBadge.text      = "×2";
        countBadge.alignment = TextAlignmentOptions.Center;
        countBadge.fontSize  = 18;
        countBadge.fontStyle = FontStyles.Bold;
        countBadge.color     = new Color(1f, 0.75f, 0f);
        SetAnchors(badgeCountObj, 0.55f, 0.78f, 1f, 1f, 0, 0, -4, -4);
        badgeCountObj.SetActive(false);

        // Button target graphic → CardFace
        button.targetGraphic = faceImg;

        // SerializeField 연결
        var so = new SerializedObject(root.GetComponent<CardView>());
        so.FindProperty("nameText").objectReferenceValue       = nameText;
        so.FindProperty("typeBackground").objectReferenceValue = typeBg;
        so.FindProperty("typeText").objectReferenceValue       = typeText;
        so.FindProperty("cardImage").objectReferenceValue      = cardImg;
        so.FindProperty("costText").objectReferenceValue       = costText;
        so.FindProperty("conditionText").objectReferenceValue  = condText;
        so.FindProperty("effectText").objectReferenceValue     = effectText;
        so.FindProperty("stackShadow").objectReferenceValue    = shadowImg;
        so.FindProperty("countBadge").objectReferenceValue     = countBadge;
        so.FindProperty("button").objectReferenceValue         = button;
        so.FindProperty("canvasGroup").objectReferenceValue    = canvasGroup;
        so.ApplyModifiedProperties();

        string path = $"{PrefabPath}/CardView.prefab";
        PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);

        AssetDatabase.Refresh();
        Debug.Log($"[PrefabCreator] CardView 프리팹 생성 완료 → {path}");
    }

    // ──────────────────────────────────────────────
    // DeckPileView
    // ──────────────────────────────────────────────
    [MenuItem("ThreeCardSurvival/프리팹 생성/DeckPileView")]
    public static void CreateDeckPileViewPrefab()
    {
        const int   n    = 15;    // 카드 장 수 (뒤에서 앞으로)
        const float W    = 160f;
        const float H    = 260f;
        const float step = 2f;   // 장당 Y 오프셋(px)

        // 루트를 CardView와 동일한 크기(160×260)로 고정 → HLG 기준점 일치
        // 뒤 카드들은 하단으로 overflow되어 보이지만 레이아웃 계산엔 영향 없음
        var root     = new GameObject("DeckPileView");
        var rootRect = root.AddComponent<RectTransform>();
        rootRect.sizeDelta = new Vector2(W, H);
        root.AddComponent<DeckPileView>();

        // 카드 장 추가 — sibling 0이 맨 뒤, sibling n-1이 맨 앞
        // 앞면 카드(i=n-1): 루트를 정확히 채움
        // 뒤 카드들: step*(n-1-i)px 아래로 내려가 루트 하단 밖으로 삐져나옴
        for (int i = 0; i < n; i++)
        {
            var go  = new GameObject($"StackCard{i}");
            go.transform.SetParent(root.transform, false);
            var img = go.AddComponent<Image>();

            float t = i / (float)(n - 1);
            img.color = Color.Lerp(new Color(0.06f, 0.12f, 0.32f),
                                   new Color(0.18f, 0.30f, 0.62f), t);

            var r = go.GetComponent<RectTransform>();
            r.anchorMin = r.anchorMax = new Vector2(0.5f, 0f); // 하단 중앙 앵커
            r.pivot     = new Vector2(0.5f, 0.5f);
            r.sizeDelta = new Vector2(W, H);
            // 앞면(i=n-1): anchoredPosition.y = H/2 → 루트를 꽉 채움
            // 뒤 카드들: step*(n-1-i)px 아래 → overflow
            r.anchoredPosition = new Vector2(0f, H / 2f - step * (n - 1 - i));
        }

        // 카드 수 텍스트 — 앞면 카드와 동일 위치 (루트 전체 커버)
        var countGo  = CreateTMP(root, "CountText");
        var countTmp = countGo.GetComponent<TextMeshProUGUI>();
        countTmp.text      = "0";
        countTmp.alignment = TextAlignmentOptions.Center;
        countTmp.fontSize  = 36;
        countTmp.fontStyle = FontStyles.Bold;
        countTmp.color     = Color.white;
        var cr = countGo.GetComponent<RectTransform>();
        cr.anchorMin = Vector2.zero;
        cr.anchorMax = Vector2.one;
        cr.offsetMin = Vector2.zero;
        cr.offsetMax = Vector2.zero;

        var so = new SerializedObject(root.GetComponent<DeckPileView>());
        so.FindProperty("countText").objectReferenceValue = countTmp;
        so.ApplyModifiedProperties();

        string path = $"{PrefabPath}/DeckPileView.prefab";
        PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);

        AssetDatabase.Refresh();
        Debug.Log($"[PrefabCreator] DeckPileView 프리팹 생성 완료 → {path}");
    }

    // ──────────────────────────────────────────────
    // 유틸
    // ──────────────────────────────────────────────
    private static GameObject CreateTMP(GameObject parent, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        go.AddComponent<TextMeshProUGUI>();
        return go;
    }

    private static void SetAnchors(GameObject go,
        float minX, float minY, float maxX, float maxY,
        float offLeft, float offBottom, float offRight, float offTop)
    {
        var r       = go.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(minX, minY);
        r.anchorMax = new Vector2(maxX, maxY);
        r.offsetMin = new Vector2(offLeft, offBottom);
        r.offsetMax = new Vector2(offRight, offTop);
    }
}
#endif

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AutoBattlerSpire.UI;

public static class BuildCombatUI_Minimal
{
    [MenuItem("Tools/Build/Setup Combat UI (minimal)")]
    public static void Setup()
    {
        var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var ev = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));
        }

        var uiGo = new GameObject("CombatUI"); uiGo.transform.SetParent(canvasGo.transform, false);
        var ui = uiGo.AddComponent<CombatUI>();

        TextMeshProUGUI MakeTMP(string name, Transform parent, int size, TextAlignmentOptions align, Color color)
        {
            var go = new GameObject(name, typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<TextMeshProUGUI>();
            t.fontSize = size; t.alignment = align; t.text = name; t.color = color;
            var rt = t.rectTransform; rt.anchorMin = new Vector2(0,0); rt.anchorMax = new Vector2(1,1); rt.offsetMin = rt.offsetMax = Vector2.zero;
            return t;
        }
        Button MakeButton(string name, Transform parent, string label)
        {
            var go = new GameObject(name, typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>(); img.color = new Color(0.9f,0.9f,0.9f,1);
            var rt = go.GetComponent<RectTransform>(); rt.sizeDelta = new Vector2(260, 60);
            var tmp = MakeTMP("Label", go.transform, 28, TextAlignmentOptions.Center, Color.black); tmp.text = label;
            return go.GetComponent<Button>();
        }
        Slider MakeSlider(string name, Transform parent, Vector2 size)
        {
            var go = new GameObject(name, typeof(Slider));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = size;
            return go.GetComponent<Slider>();
        }
        RectTransform Panel(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.GetComponent<RectTransform>();
        }

        var top = Panel("Top", canvasGo.transform);
        var rtTop = top; rtTop.anchorMin = new Vector2(0,1); rtTop.anchorMax = new Vector2(1,1); rtTop.pivot = new Vector2(0.5f,1);
        rtTop.anchoredPosition = new Vector2(0,-20); rtTop.sizeDelta = new Vector2(0, 200);

        var left = Panel("PlayerPanel", top); var rtL = left; rtL.anchorMin = new Vector2(0,0); rtL.anchorMax = new Vector2(0.5f,1);
        var right = Panel("EnemyPanel", top); var rtR = right; rtR.anchorMin = new Vector2(0.5f,0); rtR.anchorMax = new Vector2(1,1);

        ui.PlayerName = MakeTMP("PlayerName", left, 36, TextAlignmentOptions.Left, Color.white);
        ui.PlayerHp   = MakeSlider("PlayerHp", left, new Vector2(400, 22));
        ui.PlayerHpText = MakeTMP("PlayerHpText", left, 24, TextAlignmentOptions.Left, Color.white); ui.PlayerHpText.text = "HP: 0/0";
        ui.PlayerBlock = MakeSlider("PlayerBlock", left, new Vector2(360, 18));
        ui.PlayerBlockText = MakeTMP("PlayerBlockText", left, 22, TextAlignmentOptions.Left, Color.white); ui.PlayerBlockText.text = "Block: 0";

        ui.EnemyName = MakeTMP("EnemyName", right, 36, TextAlignmentOptions.Right, Color.white);
        ui.EnemyHp   = MakeSlider("EnemyHp", right, new Vector2(400, 22));
        ui.EnemyHpText = MakeTMP("EnemyHpText", right, 24, TextAlignmentOptions.Right, Color.white); ui.EnemyHpText.text = "HP: 0/0";
        ui.EnemyBlock = MakeSlider("EnemyBlock", right, new Vector2(360, 18));
        ui.EnemyBlockText = MakeTMP("EnemyBlockText", right, 22, TextAlignmentOptions.Right, Color.white); ui.EnemyBlockText.text = "Block: 0";

        var bottom = Panel("Bottom", canvasGo.transform);
        var rtB = bottom; rtB.anchorMin = new Vector2(0,0); rtB.anchorMax = new Vector2(1,0); rtB.pivot = new Vector2(0.5f,0);
        rtB.anchoredPosition = new Vector2(0,20); rtB.sizeDelta = new Vector2(0, 120);

        ui.ButtonEndTurn = MakeButton("ButtonEndTurn", bottom, "Конец хода");
        ui.ButtonEndTurn.transform.localPosition = new Vector3(0, 0, 0);

        ui.LogText = MakeTMP("Log", canvasGo.transform, 24, TextAlignmentOptions.TopLeft, Color.white);
        var rtLog = ui.LogText.rectTransform; rtLog.anchorMin = new Vector2(0.1f, 0.2f); rtLog.anchorMax = new Vector2(0.9f, 0.75f);
        rtLog.offsetMin = rtLog.offsetMax = Vector2.zero; ui.LogText.enableWordWrapping = true; ui.LogText.text = "";

        Selection.activeObject = canvasGo;
        Debug.Log("[BuildCombatUI] Минимальный UI создан. Привяжи CombatUI к полю UI на CombatRunner.");
    }
}
#endif

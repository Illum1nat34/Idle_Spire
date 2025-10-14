#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AutoBattlerSpire.UI;

public static class BuildCombatUI_Alt
{
    [MenuItem("Tools/Build/Setup Combat UI")]
    [MenuItem("GameObject/Create UI/Setup Combat UI", priority = 10)]
    public static void Setup()
    {
        // Canvas
        var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // EventSystem
        if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var ev = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));
        }

        // Root for UI script
        var uiGo = new GameObject("CombatUI"); uiGo.transform.SetParent(canvasGo.transform, false);
        var ui = uiGo.AddComponent<CombatUI>();

        // Helpers
        TextMeshProUGUI MakeTMP(string name, Transform parent, int size, TextAlignmentOptions align)
        {
            var go = new GameObject(name, typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<TextMeshProUGUI>();
            t.fontSize = size; t.alignment = align; t.text = name;
            return t;
        }
        Button MakeButton(string name, Transform parent, string label)
        {
            var go = new GameObject(name, typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var txt = MakeTMP("Label", go.transform, 28, TextAlignmentOptions.Center);
            txt.text = label;
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(260, 60);
            return go.GetComponent<Button>();
        }
        Slider MakeSlider(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(Slider));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>(); rt.sizeDelta = new Vector2(400, 20);
            return go.GetComponent<Slider>();
        }
        RectTransform Panel(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.GetComponent<RectTransform>();
        }

        // Top panels
        var top = Panel("Top", canvasGo.transform);
        var rtTop = top; rtTop.anchorMin = new Vector2(0,1); rtTop.anchorMax = new Vector2(1,1); rtTop.pivot = new Vector2(0.5f,1);
        rtTop.anchoredPosition = new Vector2(0,-20); rtTop.sizeDelta = new Vector2(0, 160);

        var left = Panel("PlayerPanel", top); var rtL = left; rtL.anchorMin = new Vector2(0,0); rtL.anchorMax = new Vector2(0.5f,1);
        var right = Panel("EnemyPanel", top); var rtR = right; rtR.anchorMin = new Vector2(0.5f,0); rtR.anchorMax = new Vector2(1,1);

        // Player widgets
        ui.PlayerName = MakeTMP("PlayerName", left, 36, TextAlignmentOptions.Left);
        ui.PlayerHp   = MakeSlider("PlayerHp", left);
        ui.PlayerBlock= MakeTMP("PlayerBlock", left, 28, TextAlignmentOptions.Left);

        // Enemy widgets
        ui.EnemyName = MakeTMP("EnemyName", right, 36, TextAlignmentOptions.Right);
        ui.EnemyHp   = MakeSlider("EnemyHp", right);
        ui.EnemyBlock= MakeTMP("EnemyBlock", right, 28, TextAlignmentOptions.Right);

        // Bottom controls
        var bottom = Panel("Bottom", canvasGo.transform);
        var rtB = bottom; rtB.anchorMin = new Vector2(0,0); rtB.anchorMax = new Vector2(1,0); rtB.pivot = new Vector2(0.5f,0);
        rtB.anchoredPosition = new Vector2(0,20); rtB.sizeDelta = new Vector2(0, 120);

        ui.ButtonEndTurn = MakeButton("ButtonEndTurn", bottom, "Конец хода");
        ui.ButtonScry    = MakeButton("ButtonScry", bottom, "Просмотр 2");
        ui.ButtonShift   = MakeButton("ButtonShift", bottom, "Сдвиг слота");

        // place buttons
        ui.ButtonEndTurn.transform.localPosition = new Vector3(-300, 0, 0);
        ui.ButtonScry.transform.localPosition    = new Vector3(   0, 0, 0);
        ui.ButtonShift.transform.localPosition   = new Vector3( 300, 0, 0);

        // Log
        ui.LogText = MakeTMP("Log", canvasGo.transform, 24, TextAlignmentOptions.TopLeft);
        var rtLog = ui.LogText.rectTransform;
        rtLog.anchorMin = new Vector2(0.1f, 0.2f); rtLog.anchorMax = new Vector2(0.9f, 0.8f);
        rtLog.offsetMin = rtLog.offsetMax = Vector2.zero;
        ui.LogText.enableWordWrapping = true;
        ui.LogText.text = "";

        Selection.activeObject = canvasGo;
        Debug.Log("[BuildCombatUI] UI создан. Привяжи ссылку CombatUI к полю в CombatRunner.");
    }
}
#endif

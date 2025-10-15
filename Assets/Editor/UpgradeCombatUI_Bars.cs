#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AutoBattlerSpire.UI;

public static class UpgradeCombatUI_Bars
{
    [MenuItem("Tools/Build/Upgrade Combat UI Bars")] 
    public static void Upgrade()
    {
        var ui = Object.FindObjectOfType<CombatUI>();
        if (ui == null)
        {
            EditorUtility.DisplayDialog("Upgrade Combat UI Bars", "Не найден объект с компонентом CombatUI на сцене.", "OK");
            return;
        }

        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Upgrade Combat UI Bars");
        int group = Undo.GetCurrentGroup();

        var uiTransform = ui.transform;
        var playerPanel = GetPanelFromRef(ui.PlayerName) ?? GetPanelFromRef(ui.PlayerHp) ?? FindChildRect(uiTransform, "PlayerPanel");
        var enemyPanel  = GetPanelFromRef(ui.EnemyName) ?? GetPanelFromRef(ui.EnemyHp) ?? FindChildRect(uiTransform, "EnemyPanel");

        if (playerPanel == null || enemyPanel == null)
        {
            EditorUtility.DisplayDialog("Upgrade Combat UI Bars", "Не найдены PlayerPanel или EnemyPanel под CombatUI.", "OK");
            Undo.CollapseUndoOperations(group);
            return;
        }

        var playerHp = EnsureSlider(playerPanel, "PlayerHp", new Vector2(420f, 24f));
        var playerHpText = EnsureText(playerPanel, "PlayerHpText", "HP: 0/0", 24, TextAlignmentOptions.Left);
        var playerBlock = EnsureSlider(playerPanel, "PlayerBlock", new Vector2(420f, 20f));
        var playerBlockText = EnsureText(playerPanel, "PlayerBlockText", "Block: 0", 22, TextAlignmentOptions.Left);

        var enemyHp = EnsureSlider(enemyPanel, "EnemyHp", new Vector2(420f, 24f));
        var enemyHpText = EnsureText(enemyPanel, "EnemyHpText", "HP: 0/0", 24, TextAlignmentOptions.Right);
        var enemyBlock = EnsureSlider(enemyPanel, "EnemyBlock", new Vector2(420f, 20f));
        var enemyBlockText = EnsureText(enemyPanel, "EnemyBlockText", "Block: 0", 22, TextAlignmentOptions.Right);

        LayoutBars(playerPanel, playerHp, playerHpText.rectTransform, playerBlock, playerBlockText.rectTransform, true);
        LayoutBars(enemyPanel, enemyHp, enemyHpText.rectTransform, enemyBlock, enemyBlockText.rectTransform, false);

        Undo.RecordObject(ui, "Assign CombatUI refs");
        ui.PlayerHp = playerHp;
        ui.PlayerHpText = playerHpText;
        ui.PlayerBlock = playerBlock;
        ui.PlayerBlockText = playerBlockText;
        ui.EnemyHp = enemyHp;
        ui.EnemyHpText = enemyHpText;
        ui.EnemyBlock = enemyBlock;
        ui.EnemyBlockText = enemyBlockText;
        EditorUtility.SetDirty(ui);

        EditorSceneManager.MarkSceneDirty(ui.gameObject.scene);
        Undo.CollapseUndoOperations(group);
        Selection.activeObject = ui.gameObject;
        Debug.Log("[UpgradeCombatUI] Полосы HP/Block и тексты обновлены.");
    }

    enum SliderAnchor { LeftTop, RightTop }

    static RectTransform GetPanelFromRef(Component comp)
    {
        if (comp == null) return null;
        return comp.transform.parent as RectTransform;
    }

    static RectTransform FindChildRect(Transform root, string name)
    {
        if (root == null) return null;
        foreach (Transform child in root)
        {
            if (child.name == name) return child as RectTransform;
            var nested = FindChildRect(child, name);
            if (nested != null) return nested;
        }
        return null;
    }

    static Slider EnsureSlider(RectTransform parent, string name, Vector2 size)
    {
        var child = parent.Find(name);
        Slider slider = null;

        if (child != null)
        {
            slider = child.GetComponent<Slider>();
            if (slider == null)
            {
                Undo.DestroyObjectImmediate(child.gameObject);
                child = null;
            }
        }

        if (slider == null)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Slider));
            Undo.RegisterCreatedObjectUndo(go, "Create Slider");
            go.transform.SetParent(parent, false);
            slider = go.GetComponent<Slider>();
            slider.interactable = false;
            slider.transition = Selectable.Transition.None;
            slider.navigation = new Navigation { mode = Navigation.Mode.None };
            slider.value = 0f;
        }

        var rect = slider.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = size;
        return slider;
    }

    static TextMeshProUGUI EnsureText(RectTransform parent, string name, string defaultText, int fontSize, TextAlignmentOptions alignment)
    {
        var child = parent.Find(name);
        TextMeshProUGUI tmp = null;

        if (child != null)
        {
            tmp = child.GetComponent<TextMeshProUGUI>();
            if (tmp == null)
            {
                Undo.DestroyObjectImmediate(child.gameObject);
                child = null;
            }
        }

        if (tmp == null)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            Undo.RegisterCreatedObjectUndo(go, "Create TMP");
            go.transform.SetParent(parent, false);
            tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = defaultText;
        }

        tmp.fontSize = fontSize;
        tmp.alignment = alignment;
        tmp.raycastTarget = false;
        tmp.enableAutoSizing = false;
        tmp.rectTransform.anchorMin = new Vector2(0f, 1f);
        tmp.rectTransform.anchorMax = new Vector2(0f, 1f);
        tmp.rectTransform.pivot = new Vector2(0f, 1f);
        tmp.rectTransform.anchoredPosition = Vector2.zero;
        tmp.rectTransform.sizeDelta = new Vector2(420f, fontSize * 1.6f);
        return tmp;
    }

    static void LayoutBars(RectTransform panel, Slider hpSlider, RectTransform hpText, Slider blockSlider, RectTransform blockText, bool left)
    {
        float dir = left ? 1f : -1f;

        PositionRect(hpSlider?.GetComponent<RectTransform>(), panel, dir * 20f, -90f);
        PositionRect(hpText, panel, dir * 20f, -118f);
        PositionRect(blockSlider?.GetComponent<RectTransform>(), panel, dir * 20f, -40f);
        PositionRect(blockText, panel, dir * 20f, -68f);
    }

    static void PositionRect(RectTransform rect, RectTransform panel, float offsetX, float offsetY)
    {
        if (rect == null) return;
        bool left = offsetX >= 0f;
        rect.anchorMin = rect.anchorMax = new Vector2(left ? 0f : 1f, 1f);
        rect.pivot = new Vector2(left ? 0f : 1f, 1f);
        rect.SetParent(panel, false);
        rect.anchoredPosition = new Vector2(offsetX, offsetY);
    }
}
#endif


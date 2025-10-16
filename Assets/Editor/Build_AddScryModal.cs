#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AutoBattlerSpire.UI;

public static class Build_AddScryModal
{
    [MenuItem("Tools/Build/Add Scry Modal")]
    public static void AddScryModal()
    {
        var canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("Add Scry Modal", "Canvas не найден на сцене.", "OK");
            return;
        }

        var ui = Object.FindObjectOfType<CombatUI>();
        if (ui == null)
        {
            EditorUtility.DisplayDialog("Add Scry Modal", "CombatUI не найден на сцене.", "OK");
            return;
        }

        if (ui.ScryModal != null)
        {
            var rebuild = EditorUtility.DisplayDialog("Add Scry Modal", "У CombatUI уже настроена ScryModal. Пересоздать?", "Пересоздать", "Отмена");
            if (!rebuild) return;

            Undo.RegisterFullObjectHierarchyUndo(ui.ScryModal.gameObject, "Remove ScryModal");
            Object.DestroyImmediate(ui.ScryModal.gameObject);
            ui.ScryModal = null;
        }

        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Add Scry Modal");
        int group = Undo.GetCurrentGroup();

        var modalGo = new GameObject("ScryModal", typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(modalGo, "Create ScryModal");
        modalGo.transform.SetParent(canvas.transform, false);

        var modal = modalGo.AddComponent<ScryModal>();

        var panel = BuildPanel(modalGo.transform);
        modal.PanelRoot = panel.gameObject;

        BuildWindow(panel.transform, modal);

        modalGo.SetActive(false);
        panel.gameObject.SetActive(false);

        ui.ScryModal = modal;
        EditorUtility.SetDirty(ui);
        EditorUtility.SetDirty(modal);

        Undo.CollapseUndoOperations(group);
        EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);

        Selection.activeObject = modalGo;
        Debug.Log("[Build] ScryModal создан и назначен в CombatUI.");
    }

    static RectTransform BuildPanel(Transform parent)
    {
        var go = new GameObject("PanelRoot", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        Undo.RegisterCreatedObjectUndo(go, "Create PanelRoot");
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0.6f);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        go.SetActive(false);
        return rect;
    }

    static void BuildWindow(Transform parent, ScryModal modal)
    {
        var window = new GameObject("Window", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        Undo.RegisterCreatedObjectUndo(window, "Create Window");
        window.transform.SetParent(parent, false);
        var rect = window.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(700f, 480f);
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        window.GetComponent<Image>().color = new Color(0.18f, 0.18f, 0.18f, 0.95f);

        modal.Header = BuildHeader(rect);
        modal.ListRoot = BuildList(rect);
        modal.FooterArea = BuildFooter(rect, modal);
        modal.ItemPrefab = BuildItemPrefab(modal.ListRoot);
    }

    static TextMeshProUGUI BuildHeader(RectTransform parent)
    {
        var go = new GameObject("Header", typeof(RectTransform), typeof(TextMeshProUGUI));
        Undo.RegisterCreatedObjectUndo(go, "Create Header");
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.offsetMin = new Vector2(32f, -80f);
        rect.offsetMax = new Vector2(-32f, -24f);

        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = "Просмотр";
        tmp.fontSize = 34f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableWordWrapping = false;
        tmp.color = Color.white;
        return tmp;
    }

    static RectTransform BuildList(RectTransform parent)
    {
        var go = new GameObject("List", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        Undo.RegisterCreatedObjectUndo(go, "Create List");
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0.25f);
        rect.anchorMax = new Vector2(1f, 0.78f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = new Vector2(32f, 0f);
        rect.offsetMax = new Vector2(-32f, 0f);

        var layout = go.GetComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(0, 0, 8, 12);
        layout.spacing = 10f;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        var fitter = go.GetComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        return rect;
    }

    static RectTransform BuildFooter(RectTransform parent, ScryModal modal)
    {
        var go = new GameObject("Footer", typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(go, "Create Footer");
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(1f, 0.2f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = new Vector2(48f, 20f);
        rect.offsetMax = new Vector2(-48f, 96f);

        modal.ButtonCancel = CreateButton("ButtonCancel", rect, "Отмена");
        modal.ButtonConfirm = CreateButton("ButtonConfirm", rect, "Принять");

        return rect;
    }

    static Button CreateButton(string name, RectTransform parent, string label)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        Undo.RegisterCreatedObjectUndo(go, "Create Button");
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(240f, 64f);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);

        var img = go.GetComponent<Image>();
        img.color = new Color(0.85f, 0.85f, 0.85f, 1f);

        var labelTmp = CreateText("Label", rect, 28, TextAlignmentOptions.Center, Color.black);
        labelTmp.text = label;

        var layout = go.GetComponent<LayoutElement>() ?? go.AddComponent<LayoutElement>();
        layout.preferredWidth = 240f;
        layout.minHeight = 56f;

        return go.GetComponent<Button>();
    }

    static TextMeshProUGUI CreateText(string name, RectTransform parent, int size, TextAlignmentOptions align, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        Undo.RegisterCreatedObjectUndo(go, "Create TMP");
        go.transform.SetParent(parent, false);
        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.fontSize = size;
        tmp.alignment = align;
        tmp.color = color;
        tmp.text = name;
        tmp.enableWordWrapping = false;
        var rect = tmp.rectTransform;
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        return tmp;
    }

    static GameObject BuildItemPrefab(RectTransform parent)
    {
        var go = new GameObject("ItemPrefab", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        Undo.RegisterCreatedObjectUndo(go, "Create ItemPrefab");
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(0f, 72f);

        var img = go.GetComponent<Image>();
        img.color = new Color(0.14f, 0.14f, 0.14f, 0.95f);

        var layout = go.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(18, 18, 14, 14);
        layout.spacing = 16f;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlHeight = true;
        layout.childForceExpandHeight = false;

        var orderSlot = CreateSlot(go.transform, 50f);
        var orderText = CreateText("Order", orderSlot, 28, TextAlignmentOptions.Center, Color.white);
        orderText.text = "1";

        var titleSlot = CreateSlot(go.transform, -1f, true);
        var titleText = CreateText("Title", titleSlot, 28, TextAlignmentOptions.Left, Color.white);
        titleText.text = "Название";

        var toggleSlot = CreateSlot(go.transform, 150f);
        var toggle = CreateToggle(toggleSlot);

        var buttonsSlot = CreateSlot(go.transform, 110f);
        var upBtn = CreateMiniButton("BtnUp", buttonsSlot, "↑");
        var downBtn = CreateMiniButton("BtnDown", buttonsSlot, "↓");

        var chip = go.AddComponent<CardChip>();
        chip.Order = orderText;
        chip.Title = titleText;
        chip.ToggleBottom = toggle;
        chip.BtnUp = upBtn;
        chip.BtnDown = downBtn;

        go.SetActive(false);
        return go;
    }

    static RectTransform CreateSlot(Transform parent, float preferredWidth, bool flexible = false)
    {
        var slot = new GameObject("Slot", typeof(RectTransform), typeof(LayoutElement));
        Undo.RegisterCreatedObjectUndo(slot, "Create Slot");
        slot.transform.SetParent(parent, false);
        var rect = slot.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        var layout = slot.GetComponent<LayoutElement>();
        layout.minHeight = 50f;
        if (preferredWidth > 0f) layout.preferredWidth = preferredWidth;
        if (flexible) layout.flexibleWidth = 1f;
        return rect;
    }

    static Toggle CreateToggle(RectTransform parent)
    {
        var toggleGo = new GameObject("ToggleBottom", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Toggle));
        Undo.RegisterCreatedObjectUndo(toggleGo, "Create Toggle");
        toggleGo.transform.SetParent(parent, false);
        var rect = toggleGo.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(32f, 32f);

        var bg = toggleGo.GetComponent<Image>();
        bg.color = new Color(0.85f, 0.85f, 0.85f, 1f);

        var checkmark = new GameObject("Checkmark", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        Undo.RegisterCreatedObjectUndo(checkmark, "Create Checkmark");
        checkmark.transform.SetParent(toggleGo.transform, false);
        var checkRect = checkmark.GetComponent<RectTransform>();
        checkRect.anchorMin = new Vector2(0.2f, 0.2f);
        checkRect.anchorMax = new Vector2(0.8f, 0.8f);
        checkRect.offsetMin = checkRect.offsetMax = Vector2.zero;
        checkRect.GetComponent<Image>().color = new Color(0.1f, 0.6f, 0.25f, 1f);

        var toggle = toggleGo.GetComponent<Toggle>();
        toggle.graphic = checkRect.GetComponent<Image>();
        toggle.targetGraphic = bg;

        var label = CreateText("Label", parent, 22, TextAlignmentOptions.Left, Color.white);
        label.text = "Вниз";
        label.rectTransform.offsetMin = new Vector2(42f, 0f);
        label.rectTransform.offsetMax = new Vector2(0f, 0f);

        return toggle;
    }

    static Button CreateMiniButton(string name, RectTransform parent, string label)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        Undo.RegisterCreatedObjectUndo(go, "Create MiniButton");
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(110f, 36f);
        var img = go.GetComponent<Image>();
        img.color = new Color(0.32f, 0.32f, 0.32f, 1f);
        var labelTmp = CreateText("Lbl", rect, 24, TextAlignmentOptions.Center, Color.white);
        labelTmp.text = label;
        labelTmp.enableWordWrapping = false;
        return go.GetComponent<Button>();
    }
}
#endif


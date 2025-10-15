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
            EditorUtility.DisplayDialog("Add Scry Modal", "У CombatUI уже настроена ScryModal.", "OK");
            return;
        }

        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Add Scry Modal");
        int group = Undo.GetCurrentGroup();

        var modalGo = new GameObject("ScryModal", typeof(RectTransform), typeof(CanvasRenderer));
        Undo.RegisterCreatedObjectUndo(modalGo, "Create ScryModal");
        modalGo.transform.SetParent(canvas.transform, false);

        var modal = modalGo.AddComponent<ScryModal>();

        var panelRoot = CreatePanelRoot(modalGo.transform);
        modal.PanelRoot = panelRoot.gameObject;

        var window = CreateWindow(panelRoot);

        var header = CreateText("Header", window, 36, TextAlignmentOptions.Center, new Color(1f, 1f, 1f, 1f));
        modal.Header = header;
        header.text = "Просмотр";

        var listRoot = new GameObject("List", typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(listRoot, "Create List");
        listRoot.transform.SetParent(window, false);
        var listRect = listRoot.GetComponent<RectTransform>();
        listRect.anchorMin = new Vector2(0.05f, 0.2f);
        listRect.anchorMax = new Vector2(0.95f, 0.75f);
        listRect.offsetMin = listRect.offsetMax = Vector2.zero;
        modal.ListRoot = listRect;

        var confirm = CreateButton("ButtonConfirm", window, "Принять");
        var confirmRect = confirm.GetComponent<RectTransform>();
        confirmRect.anchorMin = new Vector2(0.5f, 0.05f);
        confirmRect.anchorMax = new Vector2(0.9f, 0.15f);
        confirmRect.offsetMin = confirmRect.offsetMax = Vector2.zero;
        modal.ButtonConfirm = confirm.GetComponent<Button>();

        var cancel = CreateButton("ButtonCancel", window, "Отмена");
        var cancelRect = cancel.GetComponent<RectTransform>();
        cancelRect.anchorMin = new Vector2(0.1f, 0.05f);
        cancelRect.anchorMax = new Vector2(0.4f, 0.15f);
        cancelRect.offsetMin = cancelRect.offsetMax = Vector2.zero;
        modal.ButtonCancel = cancel.GetComponent<Button>();

        var itemPrefab = CreateItemPrefab(window, modal);
        modal.ItemPrefab = itemPrefab;

        modalGo.SetActive(false);
        modal.PanelRoot.SetActive(false);

        ui.ScryModal = modal;
        EditorUtility.SetDirty(ui);
        EditorUtility.SetDirty(modal);

        Undo.CollapseUndoOperations(group);
        EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);

        Selection.activeObject = modalGo;
        Debug.Log("[Build] ScryModal создан и назначен в CombatUI.");
    }

    static GameObject CreatePanelRoot(Transform parent)
    {
        var go = new GameObject("PanelRoot", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        Undo.RegisterCreatedObjectUndo(go, "Create PanelRoot");
        go.transform.SetParent(parent, false);
        var image = go.GetComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0.6f);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        go.SetActive(false);
        return go;
    }

    static Transform CreateWindow(GameObject panelRoot)
    {
        var window = new GameObject("Window", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        Undo.RegisterCreatedObjectUndo(window, "Create Window");
        window.transform.SetParent(panelRoot.transform, false);
        var rect = window.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(700f, 500f);
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        var image = window.GetComponent<Image>();
        image.color = new Color(0.2f, 0.2f, 0.2f, 0.95f);
        return window.transform;
    }

    static TextMeshProUGUI CreateText(string name, Transform parent, int size, TextAlignmentOptions align, Color color)
    {
        var go = new GameObject(name, typeof(TextMeshProUGUI));
        Undo.RegisterCreatedObjectUndo(go, "Create TMP");
        go.transform.SetParent(parent, false);
        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.fontSize = size;
        tmp.alignment = align;
        tmp.color = color;
        tmp.text = name;
        var rect = tmp.rectTransform;
        rect.anchorMin = new Vector2(0.05f, 0.8f);
        rect.anchorMax = new Vector2(0.95f, 0.95f);
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        return tmp;
    }

    static GameObject CreateButton(string name, Transform parent, string label)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        Undo.RegisterCreatedObjectUndo(go, "Create Button");
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        img.color = new Color(0.85f, 0.85f, 0.85f, 1f);
        var tmp = CreateText("Label", go.transform, 28, TextAlignmentOptions.Center, Color.black);
        tmp.text = label;
        var rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(240f, 60f);
        return go;
    }

    static GameObject CreateItemPrefab(Transform parent, ScryModal modal)
    {
        var go = new GameObject("ItemPrefab", typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(go, "Create ItemPrefab");
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(600f, 80f);

        var bg = go.AddComponent<Image>();
        bg.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);

        var chip = go.AddComponent<CardChip>();

        var orderGo = new GameObject("Order", typeof(TextMeshProUGUI));
        Undo.RegisterCreatedObjectUndo(orderGo, "Create Order");
        orderGo.transform.SetParent(go.transform, false);
        var orderTmp = orderGo.GetComponent<TextMeshProUGUI>();
        orderTmp.fontSize = 32;
        orderTmp.alignment = TextAlignmentOptions.MidlineLeft;
        orderTmp.color = Color.white;
        var orderRect = orderTmp.rectTransform;
        orderRect.anchorMin = new Vector2(0f, 0f);
        orderRect.anchorMax = new Vector2(0.1f, 1f);
        orderRect.offsetMin = new Vector2(10f, 0f);
        orderRect.offsetMax = new Vector2(-10f, 0f);
        chip.Order = orderTmp;

        var titleGo = new GameObject("Title", typeof(TextMeshProUGUI));
        Undo.RegisterCreatedObjectUndo(titleGo, "Create Title");
        titleGo.transform.SetParent(go.transform, false);
        var titleTmp = titleGo.GetComponent<TextMeshProUGUI>();
        titleTmp.fontSize = 28;
        titleTmp.alignment = TextAlignmentOptions.MidlineLeft;
        titleTmp.color = Color.white;
        var titleRect = titleTmp.rectTransform;
        titleRect.anchorMin = new Vector2(0.1f, 0f);
        titleRect.anchorMax = new Vector2(0.55f, 1f);
        titleRect.offsetMin = new Vector2(10f, 0f);
        titleRect.offsetMax = new Vector2(-10f, 0f);
        chip.Title = titleTmp;

        var toggleGo = new GameObject("ToggleBottom", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Toggle));
        Undo.RegisterCreatedObjectUndo(toggleGo, "Create Toggle");
        toggleGo.transform.SetParent(go.transform, false);
        var toggleRect = toggleGo.GetComponent<RectTransform>();
        toggleRect.anchorMin = new Vector2(0.55f, 0.2f);
        toggleRect.anchorMax = new Vector2(0.65f, 0.8f);
        toggleRect.offsetMin = toggleRect.offsetMax = Vector2.zero;
        var toggleImage = toggleGo.GetComponent<Image>();
        toggleImage.color = new Color(0.8f, 0.8f, 0.8f, 1f);
        var toggle = toggleGo.GetComponent<Toggle>();

        var background = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        Undo.RegisterCreatedObjectUndo(background, "Create Toggle Background");
        background.transform.SetParent(toggleGo.transform, false);
        var bgRect = background.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0f, 0f);
        bgRect.anchorMax = new Vector2(0.4f, 1f);
        bgRect.offsetMin = bgRect.offsetMax = Vector2.zero;
        var bgImage = background.GetComponent<Image>();
        bgImage.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        var checkmark = new GameObject("Checkmark", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        Undo.RegisterCreatedObjectUndo(checkmark, "Create Toggle Checkmark");
        checkmark.transform.SetParent(background.transform, false);
        var checkRect = checkmark.GetComponent<RectTransform>();
        checkRect.anchorMin = new Vector2(0.2f, 0.2f);
        checkRect.anchorMax = new Vector2(0.8f, 0.8f);
        checkRect.offsetMin = checkRect.offsetMax = Vector2.zero;
        var checkImage = checkmark.GetComponent<Image>();
        checkImage.color = new Color(0.1f, 0.6f, 0.1f, 1f);

        toggle.targetGraphic = background.GetComponent<Image>();
        toggle.graphic = checkImage;
        toggle.isOn = false;

        var toggleLabel = CreateText("Label", toggleGo.transform, 22, TextAlignmentOptions.Center, Color.black);
        toggleLabel.text = "Вниз";
        var toggleLabelRect = toggleLabel.rectTransform;
        toggleLabelRect.anchorMin = new Vector2(0.45f, 0f);
        toggleLabelRect.anchorMax = new Vector2(1f, 1f);
        toggleLabelRect.offsetMin = new Vector2(10f, 0f);
        toggleLabelRect.offsetMax = new Vector2(-10f, 0f);

        chip.ToggleBottom = toggle;

        var upButton = CreateButton("BtnUp", go.transform, "↑");
        var upRect = upButton.GetComponent<RectTransform>();
        upRect.anchorMin = new Vector2(0.7f, 0.2f);
        upRect.anchorMax = new Vector2(0.8f, 0.8f);
        upRect.offsetMin = upRect.offsetMax = Vector2.zero;
        chip.BtnUp = upButton.GetComponent<Button>();

        var downButton = CreateButton("BtnDown", go.transform, "↓");
        var downRect = downButton.GetComponent<RectTransform>();
        downRect.anchorMin = new Vector2(0.82f, 0.2f);
        downRect.anchorMax = new Vector2(0.92f, 0.8f);
        downRect.offsetMin = downRect.offsetMax = Vector2.zero;
        chip.BtnDown = downButton.GetComponent<Button>();

        go.SetActive(false);
        return go;
    }
}
#endif


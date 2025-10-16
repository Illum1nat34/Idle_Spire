using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using AutoBattlerSpire.Combat;

namespace AutoBattlerSpire.UI
{
    public class ScryModal : MonoBehaviour
    {
        [Header("Refs")]
        public GameObject PanelRoot;
        public RectTransform WindowRoot;
        public TextMeshProUGUI Header;
        public RectTransform ListRoot;
        public RectTransform FooterArea;
        public Button ButtonConfirm;
        public Button ButtonCancel;

        [Header("Item Template")]
        public GameObject ItemPrefab;

        public bool IsOpen => _isOpen;

        private readonly List<CardChip> _chips = new();
        private readonly List<GameObject> _spawned = new();
        private Deck _deck;
        private Action<bool> _onClosed;
        private bool _isOpen;
        private bool _layoutConfigured;

        internal int VisibleCount => _chips.Count;

        void Awake()
        {
            EnsureLayoutSetup();
            SetupButtonListeners();
            Hide();
        }

        void OnEnable()
        {
            EnsureLayoutSetup();
            SetupButtonListeners();
        }

        void OnDisable()
        {
            RemoveButtonListeners();
        }

        void OnDestroy()
        {
            RemoveButtonListeners();
        }

        void OnValidate()
        {
            EnsureLayoutSetup();
            SetupButtonListeners();
        }

        void SetupButtonListeners()
        {
            EnsureLayoutSetup();

            if (ButtonConfirm != null)
            {
                ButtonConfirm.onClick.RemoveListener(OnConfirmClicked);
                ButtonConfirm.onClick.AddListener(OnConfirmClicked);
            }

            if (ButtonCancel != null)
            {
                ButtonCancel.onClick.RemoveListener(OnCancelClicked);
                ButtonCancel.onClick.AddListener(OnCancelClicked);
            }
        }

        void RemoveButtonListeners()
        {
            if (ButtonConfirm != null) ButtonConfirm.onClick.RemoveListener(OnConfirmClicked);
            if (ButtonCancel != null) ButtonCancel.onClick.RemoveListener(OnCancelClicked);
        }

        void EnsureLayoutSetup()
        {
            if (_layoutConfigured) return;

            if (ListRoot != null)
            {
                var vertical = ListRoot.GetComponent<VerticalLayoutGroup>();
                if (vertical == null) vertical = ListRoot.gameObject.AddComponent<VerticalLayoutGroup>();
                vertical.padding = new RectOffset(0, 0, 8, 12);
                vertical.spacing = 10f;
                vertical.childAlignment = TextAnchor.UpperCenter;
                vertical.childControlWidth = true;
                vertical.childControlHeight = true;
                vertical.childForceExpandWidth = true;
                vertical.childForceExpandHeight = false;

                var fitter = ListRoot.GetComponent<ContentSizeFitter>();
                if (fitter == null) fitter = ListRoot.gameObject.AddComponent<ContentSizeFitter>();
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            }

            ConfigureButton(ButtonCancel, "Отмена");
            ConfigureButton(ButtonConfirm, "Принять");

            if (FooterArea != null)
            {
                var horizontal = FooterArea.GetComponent<HorizontalLayoutGroup>();
                if (horizontal == null) horizontal = FooterArea.gameObject.AddComponent<HorizontalLayoutGroup>();
                horizontal.padding = new RectOffset(48, 48, 0, 10);
                horizontal.spacing = 28f;
                horizontal.childAlignment = TextAnchor.MiddleCenter;
                horizontal.childControlWidth = true;
                horizontal.childControlHeight = true;
                horizontal.childForceExpandWidth = false;
                horizontal.childForceExpandHeight = false;
            }

            _layoutConfigured = true;
        }

        void ConfigureButton(Button button, string labelText)
        {
            if (button == null) return;
            var rect = button.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.sizeDelta = new Vector2(240f, 64f);
                rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
            }

            var img = button.GetComponent<Image>();
            if (img != null)
            {
                img.color = new Color(0.85f, 0.85f, 0.85f, 1f);
            }

            var le = button.GetComponent<LayoutElement>() ?? button.gameObject.AddComponent<LayoutElement>();
            le.preferredWidth = 240f;
            le.preferredHeight = 64f;
            le.minHeight = 52f;

            var label = button.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
            {
                label.text = labelText;
                label.enableAutoSizing = false;
                label.fontSize = 30f;
                label.alignment = TextAlignmentOptions.Center;
                label.enableWordWrapping = false;
            }
        }

        void ConfigureItem(CardChip chip)
        {
            if (chip == null) return;

            var rootRect = chip.GetComponent<RectTransform>();
            if (rootRect != null)
            {
                rootRect.sizeDelta = new Vector2(0f, 72f);
                var le = chip.GetComponent<LayoutElement>() ?? chip.gameObject.AddComponent<LayoutElement>();
                le.preferredHeight = 72f;
                le.minHeight = 72f;
            }

            var layout = chip.GetComponent<HorizontalLayoutGroup>() ?? chip.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(18, 18, 14, 14);
            layout.spacing = 16f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;

            ConfigureSlot(chip.Order?.rectTransform?.parent, 48f, false);
            ConfigureSlot(chip.Title?.rectTransform?.parent, 0f, true);
            ConfigureSlot(chip.ToggleBottom?.transform?.parent, 150f, false);
            ConfigureSlot(chip.BtnUp?.transform?.parent, 110f, false);
            ConfigureSlot(chip.BtnDown?.transform?.parent, 110f, false);

            if (chip.Title != null)
            {
                chip.Title.fontSize = 26f;
                chip.Title.enableWordWrapping = false;
                chip.Title.alignment = TextAlignmentOptions.Left;
            }

            if (chip.Order != null)
            {
                chip.Order.fontSize = 26f;
                chip.Order.alignment = TextAlignmentOptions.Center;
                chip.Order.enableWordWrapping = false;
            }

            if (chip.ToggleBottom != null)
            {
                var toggleRect = chip.ToggleBottom.GetComponent<RectTransform>();
                toggleRect.sizeDelta = new Vector2(30f, 30f);

                var toggleLabel = FindSiblingLabel(chip.ToggleBottom.transform);
                if (toggleLabel != null)
                {
                    toggleLabel.text = "Вниз";
                    toggleLabel.fontSize = 22f;
                    toggleLabel.enableWordWrapping = false;
                    toggleLabel.alignment = TextAlignmentOptions.Left;
                }
            }

            ConfigureMiniButton(chip.BtnUp, "↑");
            ConfigureMiniButton(chip.BtnDown, "↓");
        }

        void ConfigureSlot(Transform slotTransform, float preferredWidth, bool flexible)
        {
            if (slotTransform == null) return;
            var slot = slotTransform as RectTransform;
            if (slot == null) return;

            var le = slot.GetComponent<LayoutElement>() ?? slot.gameObject.AddComponent<LayoutElement>();
            le.minHeight = 50f;
            if (preferredWidth > 0f) le.preferredWidth = preferredWidth;
            le.flexibleWidth = flexible ? 1f : 0f;
        }

        TextMeshProUGUI FindSiblingLabel(Transform toggleTransform)
        {
            if (toggleTransform == null || toggleTransform.parent == null) return null;
            foreach (Transform child in toggleTransform.parent)
            {
                if (child == toggleTransform) continue;
                var tmp = child.GetComponent<TextMeshProUGUI>();
                if (tmp != null) return tmp;
            }
            return null;
        }

        void ConfigureMiniButton(Button button, string label)
        {
            if (button == null) return;
            var rect = button.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(100f, 36f);

            var le = button.GetComponent<LayoutElement>() ?? button.gameObject.AddComponent<LayoutElement>();
            le.preferredWidth = 100f;
            le.preferredHeight = 36f;
            le.minHeight = 36f;

            var img = button.GetComponent<Image>();
            if (img != null)
            {
                img.color = new Color(0.32f, 0.32f, 0.32f, 1f);
            }

            var tmp = button.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.text = label;
                tmp.fontSize = 24f;
                tmp.enableWordWrapping = false;
                tmp.alignment = TextAlignmentOptions.Center;
            }
        }

        public void Open(Deck deck, int x, Action<bool> onClosed)
        {
            if (deck == null)
            {
                onClosed?.Invoke(false);
                return;
            }

            if (_isOpen)
            {
                Close(false);
            }

            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            _deck = deck;
            _onClosed = onClosed;
            _isOpen = true;

            ClearItems();

            int count = Mathf.Max(1, x);
            var snapshot = deck.PeekTop(count);

            if (Header != null)
            {
                Header.text = $"Просмотр {count}";
            }

            if (PanelRoot == null) PanelRoot = gameObject;
            if (PanelRoot != null) PanelRoot.SetActive(true);
            if (WindowRoot != null) WindowRoot.gameObject.SetActive(true);

            EnsureLayoutSetup();

            if (ListRoot != null && ItemPrefab != null)
            {
                for (int i = 0; i < snapshot.Count; i++)
                {
                    var card = snapshot[i];
                    var go = Instantiate(ItemPrefab, ListRoot);
                    go.SetActive(true);
                    _spawned.Add(go);

                    var chip = go.GetComponent<CardChip>();
                    if (chip == null)
                    {
                        chip = go.AddComponent<CardChip>();
                    }

                    string title = card?.Data?.Title;
                    if (string.IsNullOrEmpty(title)) title = $"Карта {i + 1}";
                    chip.Bind(title, i, this);
                    ConfigureItem(chip);
                    _chips.Add(chip);
                }
            }

            RefreshOrder();
        }

        void OnConfirmClicked()
        {
            if (!_isOpen) return;

            var orderedTop = new List<int>();

            for (int i = 0; i < _chips.Count; i++)
            {
                var chip = _chips[i];
                if (chip.SendToBottom) continue;
                orderedTop.Add(chip.SourceIndex);
            }

            var toBottom = new List<int>();

            foreach (var chip in _chips)
            {
                if (chip.SendToBottom)
                {
                    toBottom.Add(chip.SourceIndex);
                }
            }

            _deck?.ApplyScry(toBottom, orderedTop);
            Close(true);
        }

        void OnCancelClicked()
        {
            if (!_isOpen) return;
            Close(false);
        }

        void Close(bool accepted)
        {
            _isOpen = false;
            if (PanelRoot != null) PanelRoot.SetActive(false);
            if (WindowRoot != null) WindowRoot.gameObject.SetActive(false);
            if (gameObject.activeSelf) gameObject.SetActive(false);

            ClearItems();

            var cb = _onClosed;
            _onClosed = null;
            _deck = null;
            cb?.Invoke(accepted);
        }

        void ClearItems()
        {
            foreach (var go in _spawned)
            {
                if (go != null)
                {
                    var chip = go.GetComponent<CardChip>();
                    chip?.Cleanup();
                    Destroy(go);
                }
            }

            _spawned.Clear();
            _chips.Clear();
        }

        internal void MoveChip(CardChip chip, int direction)
        {
            int index = _chips.IndexOf(chip);
            if (index < 0) return;

            int newIndex = Mathf.Clamp(index + direction, 0, _chips.Count - 1);
            if (index == newIndex) return;

            _chips.RemoveAt(index);
            _chips.Insert(newIndex, chip);

            if (chip != null)
            {
                chip.transform.SetSiblingIndex(newIndex);
            }

            RefreshOrder();
        }

        internal void RefreshOrder()
        {
            for (int i = 0; i < _chips.Count; i++)
            {
                var chip = _chips[i];
                chip.transform.SetSiblingIndex(i);
                chip.SetOrder(i + 1);
            }
        }

        void Hide()
        {
            if (PanelRoot != null)
            {
                PanelRoot.SetActive(false);
            }
        }
    }

    public class CardChip : MonoBehaviour
    {
        public TextMeshProUGUI Title;
        public TextMeshProUGUI Order;
        public Toggle ToggleBottom;
        public Button BtnUp;
        public Button BtnDown;

        public bool SendToBottom => ToggleBottom != null && ToggleBottom.isOn;
        public int SourceIndex { get; private set; }

        ScryModal _modal;

        void Awake()
        {
            if (BtnUp != null) BtnUp.onClick.AddListener(OnUpClicked);
            if (BtnDown != null) BtnDown.onClick.AddListener(OnDownClicked);
        }

        void OnDestroy()
        {
            if (BtnUp != null) BtnUp.onClick.RemoveListener(OnUpClicked);
            if (BtnDown != null) BtnDown.onClick.RemoveListener(OnDownClicked);
        }

        public void Cleanup()
        {
            if (BtnUp != null) BtnUp.onClick.RemoveListener(OnUpClicked);
            if (BtnDown != null) BtnDown.onClick.RemoveListener(OnDownClicked);
            _modal = null;
        }

        public void Bind(string title, int orderIndex, ScryModal modal)
        {
            _modal = modal;
            SourceIndex = orderIndex;

            if (Title != null) Title.text = title;
            SetOrder(orderIndex + 1);

            if (ToggleBottom != null)
            {
                ToggleBottom.isOn = false;
            }
        }

        public void SetOrder(int displayOrder)
        {
            if (Order != null)
            {
                Order.text = displayOrder.ToString();
            }

            if (BtnUp != null)
            {
                BtnUp.interactable = displayOrder > 1;
            }

            if (BtnDown != null)
            {
                BtnDown.interactable = _modal != null && displayOrder < _modal.VisibleCount;
            }
        }

        void OnUpClicked()
        {
            _modal?.MoveChip(this, -1);
        }

        void OnDownClicked()
        {
            _modal?.MoveChip(this, +1);
        }
    }
}


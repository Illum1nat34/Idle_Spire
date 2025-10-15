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
        public Transform ListRoot;
        public Button ButtonConfirm;
        public Button ButtonCancel;
        public TextMeshProUGUI Header;

        [Header("Item Template")]
        public GameObject ItemPrefab;

        public bool IsOpen => _isOpen;

        private readonly List<CardChip> _chips = new();
        private readonly List<GameObject> _spawned = new();
        private Deck _deck;
        private Action<bool> _onClosed;
        private bool _isOpen;

        internal int VisibleCount => _chips.Count;

        void Awake()
        {
            if (ButtonConfirm != null) ButtonConfirm.onClick.AddListener(OnConfirmClicked);
            if (ButtonCancel != null) ButtonCancel.onClick.AddListener(OnCancelClicked);
            Hide();
        }

        void OnDestroy()
        {
            if (ButtonConfirm != null) ButtonConfirm.onClick.RemoveListener(OnConfirmClicked);
            if (ButtonCancel != null) ButtonCancel.onClick.RemoveListener(OnCancelClicked);
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


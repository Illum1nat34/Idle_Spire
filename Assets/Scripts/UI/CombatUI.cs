using TMPro;
using UnityEngine;
using UnityEngine.UI;
using AutoBattlerSpire.Combat;

namespace AutoBattlerSpire.UI
{
    public class CombatUI : MonoBehaviour
    {
        [Header("Player")]
        public TextMeshProUGUI PlayerName;
        public Slider PlayerHp;
        public TextMeshProUGUI PlayerHpText;
        public Slider PlayerBlock;
        public TextMeshProUGUI PlayerBlockText;

        [Header("Player Colors")]
        public Color PlayerHpBackground = new Color(0.15f, 0.2f, 0.4f, 0.8f);
        public Color PlayerHpFill = new Color(0.3f, 0.6f, 1f, 0.9f);
        public Color PlayerBlockBackground = new Color(0.1f, 0.3f, 0.25f, 0.8f);
        public Color PlayerBlockFill = new Color(0.3f, 0.85f, 0.7f, 0.9f);

        [Header("Enemy")]
        public TextMeshProUGUI EnemyName;
        public Slider EnemyHp;
        public TextMeshProUGUI EnemyHpText;
        public Slider EnemyBlock;
        public TextMeshProUGUI EnemyBlockText;

        [Header("Enemy Colors")]
        public Color EnemyHpBackground = new Color(0.4f, 0.15f, 0.15f, 0.8f);
        public Color EnemyHpFill = new Color(1f, 0.4f, 0.3f, 0.9f);
        public Color EnemyBlockBackground = new Color(0.35f, 0.15f, 0.35f, 0.8f);
        public Color EnemyBlockFill = new Color(0.9f, 0.5f, 1f, 0.9f);

        [Header("Controls")]
        public Button ButtonEndTurn;

        [Header("Log")]
        public TextMeshProUGUI LogText;

        [Header("Optional")]
        public ScryModal ScryModal;

        private System.Text.StringBuilder _sb = new System.Text.StringBuilder();

        void Awake()
        {
            EnsureSliderGraphics(PlayerHp, PlayerHpBackground, PlayerHpFill);
            EnsureSliderGraphics(PlayerBlock, PlayerBlockBackground, PlayerBlockFill);
            EnsureSliderGraphics(EnemyHp, EnemyHpBackground, EnemyHpFill);
            EnsureSliderGraphics(EnemyBlock, EnemyBlockBackground, EnemyBlockFill);
        }

        public void Bind(CombatRunner runner)
        {
            ButtonEndTurn.onClick.AddListener(runner.OnEndTurnClicked);
        }

        public void SetNames(string p, string e)
        {
            PlayerName.text = p;
            EnemyName.text  = e;
        }

        public void UpdateBars(Fighter player, Fighter enemy)
        {
            if (PlayerHp != null) PlayerHp.maxValue = player.MaxHp;
            if (PlayerBlock != null) PlayerBlock.maxValue = Mathf.Max(1f, Mathf.Max(player.MaxHp, player.Block));
            if (EnemyHp != null) EnemyHp.maxValue = enemy.MaxHp;
            if (EnemyBlock != null) EnemyBlock.maxValue = Mathf.Max(1f, Mathf.Max(enemy.MaxHp, enemy.Block));

#if DOTWEEN || DOTWEEN_PRESENT
            if (PlayerHp != null) AutoBattlerSpire.FX.CombatFx.TweenHp(PlayerHp, player.Hp);
            if (EnemyHp != null) AutoBattlerSpire.FX.CombatFx.TweenHp(EnemyHp, enemy.Hp);
            if (PlayerBlock != null) AutoBattlerSpire.FX.CombatFx.TweenBlockSlider(PlayerBlock, player.Block);
            if (EnemyBlock != null) AutoBattlerSpire.FX.CombatFx.TweenBlockSlider(EnemyBlock, enemy.Block);
#else
            if (PlayerHp != null) PlayerHp.value = player.Hp;
            if (EnemyHp != null) EnemyHp.value = enemy.Hp;
            if (PlayerBlock != null) PlayerBlock.value = player.Block;
            if (EnemyBlock != null) EnemyBlock.value = enemy.Block;
#endif

            if (PlayerHpText != null) PlayerHpText.text = $"HP: {player.Hp}/{player.MaxHp}";
            if (EnemyHpText != null) EnemyHpText.text = $"HP: {enemy.Hp}/{enemy.MaxHp}";

            AutoBattlerSpire.FX.CombatFx.TweenBlockText(PlayerBlockText, player.Block);
            AutoBattlerSpire.FX.CombatFx.TweenBlockText(EnemyBlockText, enemy.Block);
        }

        public void Log(string line)
        {
            _sb.AppendLine(line);
            LogText.text = _sb.ToString();
        }

        public void SetEndTurnEnabled(bool enabled)
        {
            if (ButtonEndTurn != null)
            {
                ButtonEndTurn.interactable = enabled;
            }
        }

        void EnsureSliderGraphics(Slider slider, Color backgroundColor, Color fillColor)
        {
            if (slider == null || slider.fillRect != null) return;

            slider.direction = Slider.Direction.LeftToRight;
            slider.transition = Selectable.Transition.None;
            slider.navigation = new Navigation { mode = Navigation.Mode.None };
            slider.handleRect = null;

            var background = new GameObject("Background", typeof(RectTransform), typeof(Image));
            background.transform.SetParent(slider.transform, false);
            var bgRect = background.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = new Vector2(0f, 0f);
            bgRect.offsetMax = new Vector2(0f, 0f);
            var bgImage = background.GetComponent<Image>();
            bgImage.color = backgroundColor;

            var fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(slider.transform, false);
            var fillAreaRect = fillArea.GetComponent<RectTransform>();
            fillAreaRect.anchorMin = new Vector2(0.02f, 0.15f);
            fillAreaRect.anchorMax = new Vector2(0.98f, 0.85f);
            fillAreaRect.offsetMin = fillAreaRect.offsetMax = Vector2.zero;

            var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(fillArea.transform, false);
            var fillRect = fill.GetComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0f, 0f);
            fillRect.anchorMax = new Vector2(1f, 1f);
            fillRect.offsetMin = fillRect.offsetMax = Vector2.zero;
            var fillImage = fill.GetComponent<Image>();
            fillImage.color = fillColor;

            slider.fillRect = fillRect;
            slider.targetGraphic = fillImage;
        }
    }
}

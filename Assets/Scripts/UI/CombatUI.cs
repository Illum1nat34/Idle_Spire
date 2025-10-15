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
        public TextMeshProUGUI PlayerBlock;

        [Header("Enemy")]
        public TextMeshProUGUI EnemyName;
        public Slider EnemyHp;
        public TextMeshProUGUI EnemyBlock;

        [Header("Controls")]
        public Button ButtonEndTurn;

        [Header("Log")]
        public TextMeshProUGUI LogText;

        [Header("Optional")]
        public ScryModal ScryModal;

        private System.Text.StringBuilder _sb = new System.Text.StringBuilder();

        void Awake()
        {
            EnsureSliderGraphics(PlayerHp, new Color(0.15f, 0.2f, 0.4f, 0.8f), new Color(0.3f, 0.6f, 1f, 0.9f));
            EnsureSliderGraphics(EnemyHp, new Color(0.4f, 0.15f, 0.15f, 0.8f), new Color(1f, 0.4f, 0.3f, 0.9f));
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
            PlayerHp.maxValue = player.MaxHp;
            EnemyHp.maxValue = enemy.MaxHp;

#if DOTWEEN || DOTWEEN_PRESENT
            AutoBattlerSpire.FX.CombatFx.TweenHp(PlayerHp, player.Hp);
            AutoBattlerSpire.FX.CombatFx.TweenHp(EnemyHp, enemy.Hp);
            AutoBattlerSpire.FX.CombatFx.TweenBlock(PlayerBlock, player.Block);
            AutoBattlerSpire.FX.CombatFx.TweenBlock(EnemyBlock, enemy.Block);
#else
            PlayerHp.value = player.Hp;
            EnemyHp.value = enemy.Hp;
            PlayerBlock.text = $"Block: {player.Block}";
            EnemyBlock.text = $"Block: {enemy.Block}";
#endif
        }

        public void Log(string line)
        {
            _sb.AppendLine(line);
            LogText.text = _sb.ToString();
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

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

        private System.Text.StringBuilder _sb = new System.Text.StringBuilder();

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
            PlayerHp.maxValue = player.MaxHp; PlayerHp.value = player.Hp;
            EnemyHp .maxValue = enemy .MaxHp; EnemyHp .value = enemy .Hp;
            PlayerBlock.text = $"Block: {player.Block}";
            EnemyBlock .text = $"Block: {enemy.Block}";
        }

        public void Log(string line)
        {
            _sb.AppendLine(line);
            LogText.text = _sb.ToString();
        }
    }
}

using UnityEngine;

namespace AutoBattlerSpire.Data
{
    [CreateAssetMenu(fileName="Relic_", menuName="Game/Relic")]
    public class RelicData : ScriptableObject
    {
        public string Id;
        public string Title;
        [TextArea] public string Description;

        // Триггеры/логика добавим в следующем спринте.
    }
}

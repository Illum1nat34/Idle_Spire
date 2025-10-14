using UnityEditor;
using UnityEngine;

namespace AutoBattlerSpire.Tools
{
    public static class SoMenuShortcuts
    {
#if UNITY_EDITOR
        [MenuItem("Assets/Game/Create/Card (Quick)", false, 10)]
        public static void CreateCardQuick()
        {
            var a = ScriptableObject.CreateInstance<AutoBattlerSpire.Data.CardData>();
            ProjectWindowUtil.CreateAsset(a, "Card_New.asset");
        }
#endif
    }
}

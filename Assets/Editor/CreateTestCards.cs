#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using AutoBattlerSpire.Data;
using AutoBattlerSpire.Core;

public static class CreateTestCards
{
    private const string TargetFolder = "Assets/Data/ScriptableObjects/Cards";

    [MenuItem("Game/Generate/Test Cards (10 items)")]
    public static void Generate()
    {
        // Ensure folder exists
        if (!AssetDatabase.IsValidFolder("Assets/Data"))
            AssetDatabase.CreateFolder("Assets", "Data");
        if (!AssetDatabase.IsValidFolder("Assets/Data/ScriptableObjects"))
            AssetDatabase.CreateFolder("Assets/Data", "ScriptableObjects");
        if (!AssetDatabase.IsValidFolder(TargetFolder))
            AssetDatabase.CreateFolder("Assets/Data/ScriptableObjects", "Cards");

        CreateCard("Card_01_Spark", "Искра", CardType.Attack, Rarity.Common, 1,
            adaptive: SlotMask.None,
            s1Mult:1f, s2Mult:1f, s3Mult:1f,
            effects: new EffectDef[]{ new EffectDef{ Kind=EffectKind.Damage, V1=5 } }
        );

        CreateCard("Card_02_JumpStrike", "Удар в прыжке", CardType.Attack, Rarity.Common, 2,
            SlotMask.None, 1f,1f,1f,
            new EffectDef[]{ new EffectDef{ Kind=EffectKind.Damage, V1=9 } }
        );

        CreateCard("Card_03_FireBurst", "Огненный взрыв", CardType.Attack, Rarity.Uncommon, 3,
            SlotMask.None, 1f,1f,1f,
            new EffectDef[]{ new EffectDef{ Kind=EffectKind.Damage, V1=14 } }
        );

        CreateCard("Card_04_Plate", "Пластина", CardType.Skill, Rarity.Common, 2,
            SlotMask.None, 1f,1f,1f,
            new EffectDef[]{ new EffectDef{ Kind=EffectKind.Block, V1=7 } }
        );

        CreateCard("Card_05_IronWill", "Железная воля", CardType.Skill, Rarity.Uncommon, 3,
            SlotMask.None, 1f,1f,1f,
            new EffectDef[]{ new EffectDef{ Kind=EffectKind.Block, V1=10 } }
        );

        CreateCard("Card_06_FlexPush", "Гибкий толчок", CardType.Attack, Rarity.Uncommon, 2,
            SlotMask.S1S2, 0.9f,1.0f,1.1f,
            new EffectDef[]{ new EffectDef{ Kind=EffectKind.Damage, V1=8 } }
        );

        CreateCard("Card_07_ParryHit", "Парирующий удар", CardType.Attack, Rarity.Common, 2,
            SlotMask.None, 1f,1f,1f,
            new EffectDef[]{
                new EffectDef{ Kind=EffectKind.Damage, V1=6 },
                new EffectDef{ Kind=EffectKind.Block, V1=4 }
            }
        );

        CreateCard("Card_08_Adrenaline", "Адреналин", CardType.Skill, Rarity.Common, 1,
            SlotMask.None, 1f,1f,1f,
            new EffectDef[]{ new EffectDef{ Kind=EffectKind.Block, V1=4 } }
        );

        CreateCard("Card_09_Shock", "Удар током", CardType.Attack, Rarity.Common, 1,
            SlotMask.None, 1f,1f,1f,
            new EffectDef[]{ new EffectDef{ Kind=EffectKind.Damage, V1=4 } }
        );

        CreateCard("Card_10_BladeEcho", "Эхо клинка", CardType.Attack, Rarity.Uncommon, 2,
            SlotMask.S2S3, 0.8f,1.0f,1.15f,
            new EffectDef[]{ new EffectDef{ Kind=EffectKind.Damage, V1=7 } }
        );

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[CreateTestCards] 10 карточек сгенерировано в " + TargetFolder);
    }

    private static void CreateCard(string fileName, string title, CardType type, Rarity rarity, int baseCost,
                                   SlotMask adaptive, float s1Mult, float s2Mult, float s3Mult,
                                   EffectDef[] effects)
    {
        var assetPath = Path.Combine(TargetFolder, fileName + ".asset").Replace("\\","/");
        var asset = ScriptableObject.CreateInstance<CardData>();
        asset.Id = fileName;
        asset.Title = title;
        asset.Description = "";
        asset.Type = type;
        asset.Rarity = rarity;
        asset.BaseCost = Mathf.Clamp(baseCost, 1, 3);
        asset.AdaptiveMask = adaptive;
        asset.S1Mult = s1Mult;
        asset.S2Mult = s2Mult;
        asset.S3Mult = s3Mult;
        asset.Effects = effects;
        asset.UpgradeTo = null;

        AssetDatabase.CreateAsset(asset, assetPath);
    }
}
#endif

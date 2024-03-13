﻿using R2API;
using RoR2;
using Starstorm2Unofficial.Modules;
using Starstorm2Unofficial.Modules.Achievements;
using UnityEngine;

namespace Starstorm2Unofficial.Cores.Unlockables
{
    public static class VanillaSurvivorUnlockables
    {
        public static UnlockableDef commandoGrandMastery;
        public static UnlockableDef toolbotGrandMastery;
        public static UnlockableDef acridGrandMastery;

        public static void RegisterUnlockables()
        {
            // todo: make a base class for mastery achievements and simply inherit from it for each character 


            //LanguageAPI.Add("CROCO_GRANDMASTERYUNLOCKABLE_ACHIEVEMENT_NAME", "Acrid: Grand Mastery");
            //LanguageAPI.Add("CROCO_GRANDMASTERYUNLOCKABLE_ACHIEVEMENT_DESC", "As Acrid, beat the game or obliterate on Typhoon.");
            //LanguageAPI.Add("CROCO_GRANDMASTERYUNLOCKABLE_UNLOCKABLE_NAME", "Acrid: Grand Mastery");

            if (Modules.Config.EnableGrandMasteryCommando.Value)
            {
                LanguageAPI.Add("ACHIEVEMENT_SS2UCOMMANDOCLEARGAMETYPHOON_NAME", "Commando: Grand Mastery");
                LanguageAPI.Add("ACHIEVEMENT_SS2UCOMMANDOCLEARGAMETYPHOON_DESCRIPTION", "As Commando, beat the game or obliterate on Typhoon.");
                commandoGrandMastery = ScriptableObject.CreateInstance<UnlockableDef>();
                commandoGrandMastery.cachedName = "Skins.SS2UCommando.GrandMastery";
                commandoGrandMastery.nameToken = "ACHIEVEMENT_SS2UCOMMANDOCLEARGAMETYPHOON_NAME";
                commandoGrandMastery.achievementIcon = Assets.mainAssetBundle.LoadAsset<Sprite>("texCommandoSkinGrandMaster");
                Modules.Unlockables.unlockableDefs.Add(commandoGrandMastery);

                AchievementHider.unlockableRewardIdentifiers.Remove(commandoGrandMastery.cachedName);
            }

            if (Modules.Config.EnableGrandMasteryToolbot.Value)
            {
                LanguageAPI.Add("ACHIEVEMENT_SS2UTOOLBOTCLEARGAMETYPHOON_NAME", "MUL-T: Grand Mastery");
                LanguageAPI.Add("ACHIEVEMENT_SS2UTOOLBOTCLEARGAMETYPHOON_DESCRIPTION", "As MUL-T, beat the game or obliterate on Typhoon.");
                toolbotGrandMastery = ScriptableObject.CreateInstance<UnlockableDef>();
                toolbotGrandMastery.cachedName = "Skins.SS2UToolbot.GrandMastery";
                toolbotGrandMastery.nameToken = "ACHIEVEMENT_SS2UTOOLBOTCLEARGAMETYPHOON_NAME";
                toolbotGrandMastery.achievementIcon = Assets.mainAssetBundle.LoadAsset<Sprite>("texToolbotSkinGrandMaster");
                Modules.Unlockables.unlockableDefs.Add(toolbotGrandMastery);

                AchievementHider.unlockableRewardIdentifiers.Remove(toolbotGrandMastery.cachedName);
            }

            //acridGrandMastery = Modules.Unlockables.AddUnlockable<Achievements.CrocoGrandMasteryAchievement>(true);//Assets.mainAssetBundle.LoadAsset<Sprite>("texAcridSkinGrandMaster");
        }
    }
}
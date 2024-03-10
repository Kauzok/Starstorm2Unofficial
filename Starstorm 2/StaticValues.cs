﻿using BepInEx.Configuration;

namespace Starstorm2Unofficial
{
    public static class StaticValues
    {
        public static float maliceRangeValue;
        public static float maliceRangeStackValue;
        public static float maliceDmgReductionValue;
        public static float maliceProcCoefValue;

        public static float trematodeDamage;
        public static float trematodeDuration;
        public static float trematodeCritical;

        public static float testerGold;
        public static float testerHealing;

        public static float bootsBase;
        public static float bootsStack;
        public static float bootsRadius;
        public static float bootsProc;
        public static JetBootsEffectQuality timbsQuality;

        public static float hottestSusRadius;
        public static float hottestSusHit;
        public static float hottestSusDuration;
        public static float hottestSusDamage;

        public static float sekiroArmor;
        public static float sekiroArmorStack;
        public static float sekiroCrit;
        public static float sekiroCritStack;

        internal static void InitValues()
        {
            sekiroArmor = 15f;
            sekiroArmorStack = 10f;
            sekiroCrit = 25f;
            sekiroCritStack = 20f;

            hottestSusHit = 1.5f;
            hottestSusRadius = 30f;
            hottestSusDuration = 6f;
            hottestSusDamage = 1f;

            bootsBase = 1.5f;
            bootsStack = 1f;
            bootsRadius = 7.5f;
            bootsProc = 0f;
            timbsQuality = JetBootsEffectQuality.Default;

            maliceRangeValue = 9f;
            maliceRangeStackValue = 1f;
            maliceDmgReductionValue = 0.55f;
            maliceProcCoefValue = 0f;

            testerGold = 5f;
            testerHealing = 15f;
        }

        // helper for ez item stat config
        internal static float ItemStatConfigValue(string itemName, string configName, string desc, float defaultValue)
        {
            ConfigEntry<float> config = StarstormPlugin.instance.Config.Bind<float>("Starstorm 2 :: Items :: " + itemName, configName, defaultValue, new ConfigDescription(desc));
            return config.Value;
        }

        internal static float ItemStatStupidConfigValue(string itemName, string configName, string desc, int defaultValue)
        {
            ConfigEntry<float> config = StarstormPlugin.instance.Config.Bind<float>("Starstorm 2 :: Items :: " + itemName, configName, defaultValue, new ConfigDescription(desc));
            return config.Value;
        }

        public enum JetBootsEffectQuality
        {
            None,
            Light,
            Default
        };
    }
}
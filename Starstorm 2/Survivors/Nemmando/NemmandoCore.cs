﻿using BepInEx.Configuration;
using EntityStates;
using EntityStates.SS2UStates.Nemmando;
using EntityStates.SS2UStates.Nemmando.Taunt;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.CharacterAI;
using RoR2.Projectile;
using RoR2.Skills;
using Starstorm2Unofficial.Cores;
using Starstorm2Unofficial.Cores.NemesisInvasion;
using Starstorm2Unofficial.Modules;
using Starstorm2Unofficial.Modules.Achievements;
using Starstorm2Unofficial.Modules.Survivors;
using Starstorm2Unofficial.Survivors.Nemmando.Components;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Starstorm2Unofficial.Survivors.Nemmando
{
    internal class NemmandoCore : SurvivorBase
    {
        public static BodyIndex bodyIndex;

        internal override string bodyName { get; set; } = "SS2UNemmando";
        internal override string modelName { get; set; } = "mdlNemmando";
        internal override string displayName { get; set; } = "NemmandoDisplay";

        internal override GameObject bodyPrefab { get; set; }
        internal override GameObject displayPrefab { get; set; }

        internal static GameObject bossMasterPrefab { get; set; }
        internal GameObject bossBodyPrefab { get; set; }

        internal override float sortPosition { get; set; } = 1.001f;

        internal override StarstormBodyInfo bodyInfo { get; set; } = new StarstormBodyInfo
        {
            armor = 20f,
            bodyName = "SS2UNemmandoBody",
            bodyNameToken = "SS2UNEMMANDO_NAME",
            bodyColor = new Color(1.0f, 0.20f, 0.20f),
            characterPortrait = Modules.Assets.LoadCharacterIcon("Nemmando"),
            crosshair = Modules.Assets.LoadCrosshair("Standard"),
            damage = 12f,
            healthGrowth = 33f,
            healthRegen = 2.5f,
            jumpCount = 1,
            maxHealth = 110f,
            subtitleNameToken = "SS2UNEMMANDO_SUBTITLE",
            menuSoundString = "SS2UNemmandoCSS"
        };

        internal static Material nemmandoMat = Modules.Assets.CreateMaterial("matNemmando");
        internal static Material swordMat = Modules.Assets.CreateMaterial("matNemmando");
        internal override int mainRendererIndex { get; set; } = 0;

        internal override CustomRendererInfo[] customRendererInfos { get; set; } = new CustomRendererInfo[] {
                new CustomRendererInfo
                {
                    childName = "Model",
                    material = nemmandoMat
                },
                new CustomRendererInfo
                {
                    childName = "SwordModel",
                    material = swordMat,
                    ignoreOverlays = true
                },
                new CustomRendererInfo
                {
                    childName = "GunModel",
                    material = nemmandoMat,
                    ignoreOverlays = true
                },
                new CustomRendererInfo
                {
                    childName = "CoatModel",
                    material = Modules.Assets.CreateMaterial("matVergil", 1f)
                }};

        internal override Type characterMainState { get; set; } = typeof(NemmandoMain);

        // item display stuffs
        internal override ItemDisplayRuleSet itemDisplayRuleSet { get; set; }
        internal override List<ItemDisplayRuleSet.KeyAssetRuleGroup> itemDisplayRules { get; set; }

        internal override UnlockableDef characterUnlockableDef { get; set; } = CreateUnlockableDef();
        public static UnlockableDef survivorUnlock;
        public static UnlockableDef killSelfUnlockableDef;

        private static void CreateKillSelfUnlockable()
        {
            if (!NemmandoCore.killSelfUnlockableDef)
            {
                NemmandoCore.killSelfUnlockableDef = ScriptableObject.CreateInstance<UnlockableDef>();
                NemmandoCore.killSelfUnlockableDef.cachedName = "Skins.SS2UNemmando.Commando";
                NemmandoCore.killSelfUnlockableDef.nameToken = "ACHIEVEMENT_SS2UNEMMANDOKILLSELF_NAME";
                NemmandoCore.killSelfUnlockableDef.achievementIcon = Assets.mainAssetBundle.LoadAsset<Sprite>("texSingleTapUnlockIcon");
                AchievementHider.unlockableRewardIdentifiers.Remove(NemmandoCore.killSelfUnlockableDef.cachedName);

                SkinDef sd = Addressables.LoadAssetAsync<SkinDef>("RoR2/Base/Commando/skinCommandoDefault.asset").WaitForCompletion();
                if (sd && sd.icon) NemmandoCore.killSelfUnlockableDef.achievementIcon = sd.icon;

                Modules.Unlockables.unlockableDefs.Add(NemmandoCore.killSelfUnlockableDef);
            }
        }

        private static UnlockableDef CreateUnlockableDef()
        {
            if (!survivorUnlock)
            {
                survivorUnlock = ScriptableObject.CreateInstance<UnlockableDef>();
                survivorUnlock.cachedName = "Characters.SS2UNemmando";
                survivorUnlock.nameToken = "ACHIEVEMENT_SS2UNEMMANDOUNLOCK_NAME";
                survivorUnlock.achievementIcon = Assets.mainAssetBundle.LoadAsset<Sprite>("texNemmandoIconUnlock");
                AchievementHider.unlockableRewardIdentifiers.Remove(survivorUnlock.cachedName);

                Modules.Unlockables.unlockableDefs.Add(survivorUnlock);
            }

            if (Modules.Config.EnableUnlockAll.Value || !Modules.Config.EnableVoid.Value) return null;

            return survivorUnlock;
        }

        private static UnlockableDef masterySkinUnlockableDef;
        private static UnlockableDef grandMasterySkinUnlockableDef;

        private static UnlockableDef singleTapUnlockableDef;    //todo: reimplement
        private static UnlockableDef decisiveStrikeUnlockableDef;   //todo: reimplement

        // ugh
        internal static SkillDef decisiveStrikeSkillDef;
        internal static SkillDef scepterDecisiveStrikeSkillDef;
        internal static SkillDef scepterSubmissionSkillDef;

        //Used by NemmandoController. Todo: organize and rename
        public static SkillDef secondaryDistantGash;
        public static SkillDef specialDecisiveStrike;
        public static SkillDef specialSubmission;

        private void SetBodyIndex()
        {
            bodyIndex = BodyCatalog.FindBodyIndex("SS2UNemmandoBody");

            BodyIndex monsterBodyIndex = BodyCatalog.FindBodyIndex("SS2UNemmandoMonsterBody");
            if (monsterBodyIndex != BodyIndex.None)
            {
                NemesisInvasionCore.prioritizePlayersList.Add(monsterBodyIndex);
            }
        }
        internal override void InitializeCharacter()
        {
            base.InitializeCharacter();
            R2API.ItemAPI.DoNotAutoIDRSFor(bodyPrefab);

            SetupSwordProjectile();
            SetupLaserTracer();

            RoR2.RoR2Application.onLoad += SetBodyIndex;
            Modules.Assets.LoadNemmandoEffects();

            bodyPrefab.AddComponent<Components.NemmandoController>();
            bodyPrefab.GetComponent<EntityStateMachine>().initialStateType = new SerializableEntityStateType(typeof(EntityStates.SS2UStates.Common.NemmandoSpawnState));
            bodyPrefab.AddComponent<CustomEffectComponent>();

            bodyPrefab.GetComponent<ModelLocator>().modelTransform.gameObject.AddComponent<Components.SS2CharacterAnimationEvents>();

            bodyPrefab.GetComponentInChildren<ChildLocator>().FindChild("GunReloadEffect").GetChild(0).GetComponent<ParticleSystemRenderer>().material = Modules.Assets.CreateMaterial("matAmmo");

            InitializeBoss();
            if (StarstormPlugin.emoteAPILoaded) EmoteAPICompat();
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private void EmoteAPICompat()
        {
            On.RoR2.SurvivorCatalog.Init += (orig) =>
            {
                orig();

                foreach (var item in SurvivorCatalog.allSurvivorDefs)
                {
                    if (item.bodyPrefab.name == "SS2UNemmandoBody")
                    {
                        var skele = Modules.Assets.mainAssetBundle.LoadAsset<UnityEngine.GameObject>("animNemmandoEmote.prefab");

                        EmotesAPI.CustomEmotesAPI.ImportArmature(item.bodyPrefab, skele);
                        skele.GetComponentInChildren<BoneMapper>().scale = 1.5f;
                        break;
                    }
                }
            };
        }

        private void SetupSwordProjectile()
        {

            GameObject swordBeam = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/FMJ"), "SS2UNemmandoSwordBeam", true);
            swordBeam.transform.localScale = new Vector3(4.5f, 2.5f, 2.5f);

            GameObject swordBeamGhost = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/EvisProjectile").GetComponent<ProjectileController>().ghostPrefab, "SS2UNemmandoSwordBeamGhost", false);
            foreach (ParticleSystemRenderer i in swordBeamGhost.GetComponentsInChildren<ParticleSystemRenderer>())
            {
                if (i)
                {
                    Material mat = UnityEngine.Object.Instantiate<Material>(i.material);
                    mat.SetColor("_TintColor", Color.red);
                    i.material = mat;
                }
            }
            foreach (Light i in swordBeamGhost.GetComponentsInChildren<Light>())
            {
                if (i)
                {
                    i.color = Color.red;
                }
            }

            swordBeam.GetComponent<ProjectileController>().ghostPrefab = swordBeamGhost;
            swordBeam.GetComponent<ProjectileDamage>().damageType = DamageType.Generic;

            DamageAPI.ModdedDamageTypeHolderComponent moddedDamage = swordBeam.AddComponent<DamageAPI.ModdedDamageTypeHolderComponent>();
            moddedDamage.Add(Cores.DamageTypeCore.ModdedDamageTypes.GougeOnHit);

            StarstormPlugin.Destroy(swordBeam.transform.Find("SweetSpotBehavior").gameObject);

            Modules.Prefabs.projectilePrefabs.Add(swordBeam);

            FireSwordBeam.projectilePrefab = swordBeam;
        }

        private void SetupLaserTracer()
        {
            GameObject laserTracer = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/Tracers/TracerCommandoShotgun").InstantiateClone("SS2UNemmandoLaserTracer", true);

            foreach (LineRenderer i in laserTracer.GetComponentsInChildren<LineRenderer>())
            {
                if (i)
                {
                    Material material = UnityEngine.Object.Instantiate<Material>(i.material);
                    material.SetColor("_TintColor", Color.red);
                    i.material = material;
                    i.startColor = new Color(0.8f, 0.19f, 0.19f);
                    i.endColor = new Color(0.8f, 0.19f, 0.19f);
                }
            }

            Modules.Assets.AddEffect(laserTracer);
            ChargeBarrageFire.tracerEffectPrefab = laserTracer;
            ScepterBarrageFire.tracerEffectPrefab = laserTracer;
        }

        private void InitializeBoss()
        {
            bossBodyPrefab = PrefabAPI.InstantiateClone(bodyPrefab, "SS2UNemmandoMonsterBody", true);

            var body = bossBodyPrefab.GetComponent<CharacterBody>();
            body.baseMaxHealth = 3600f;
            body.levelMaxHealth =  1080f;
            body.baseRegen = 0;
            body.levelRegen = 0;
            body.baseDamage = 3f;
            body.levelDamage = 0.6f;
            body.isChampion = true;

            body.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;
            body.bodyFlags |= CharacterBody.BodyFlags.OverheatImmune;
            body.bodyFlags |= CharacterBody.BodyFlags.Void;
            body.bodyFlags |= CharacterBody.BodyFlags.ImmuneToVoidDeath;

            R2API.ItemAPI.DoNotAutoIDRSFor(bossBodyPrefab);

            //does this even work?
            if (body.mainHurtBox)
            {
                (body.mainHurtBox.collider as CapsuleCollider).radius *= 1.5f;
            }

            ModelLocator ml = bossBodyPrefab.GetComponent<ModelLocator>();
            if (ml && ml.modelTransform)
            {
                ml.modelTransform.localScale *= 2f;
            }

            bossMasterPrefab = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterMasters/MercMonsterMaster"), "SS2UNemmandoBossMaster", true);
            bossMasterPrefab.GetComponent<CharacterMaster>().bodyPrefab = bossBodyPrefab;

            GameObject bossEffect = GameObject.Instantiate(Modules.Assets.nemmandoBossFX);
            bossEffect.transform.parent = bossBodyPrefab.GetComponentInChildren<ChildLocator>().FindChild("Chest");
            bossEffect.transform.localPosition = Vector3.zero;
            bossEffect.transform.GetChild(0).localPosition = Vector3.zero;
            bossEffect.transform.localScale = Vector3.one * 2f;
            bossEffect.transform.GetChild(0).localScale = Vector3.one * 2f;

            bossBodyPrefab.AddComponent<Components.NemmandoUnlockComponent>();
            bossBodyPrefab.AddComponent<Components.NemmandoBossSpecialSwapComponent>();

            Modules.Prefabs.bodyPrefabs.Add(bossBodyPrefab);
            Modules.Prefabs.masterPrefabs.Add(bossMasterPrefab);

            Modules.Prefabs.RemoveAISkillDrivers(bossMasterPrefab);
            Modules.Prefabs.AddAISkillDriver(bossMasterPrefab, "Special", SkillSlot.Special, null,
                  true, false,
                  Mathf.NegativeInfinity, Mathf.Infinity,
                  Mathf.NegativeInfinity, Mathf.Infinity,
                  0f, 30f,
                  true, false, false, -1,
                  AISkillDriver.TargetType.CurrentEnemy,
                  true, false, false,
                  AISkillDriver.MovementType.ChaseMoveTarget, 1f,
                  AISkillDriver.AimType.AtCurrentEnemy,
                  true,
                  false,
                  false,
                  AISkillDriver.ButtonPressType.Hold,
                  3f,
                  false,
                  true,
                  null);

            Modules.Prefabs.AddAISkillDriver(bossMasterPrefab, "Secondary", SkillSlot.Secondary, null,
                 true, false,
                 Mathf.NegativeInfinity, Mathf.Infinity,
                 Mathf.NegativeInfinity, Mathf.Infinity,
                 10f, 40f,
                 true, false, true, -1,
                 AISkillDriver.TargetType.CurrentEnemy,
                 true, true, true,
                 AISkillDriver.MovementType.ChaseMoveTarget, 1f,
                 AISkillDriver.AimType.AtCurrentEnemy,
                 false,
                 true,
                 false,
                 AISkillDriver.ButtonPressType.Hold,
                 1f,
                 false,
                 false,
                 null);

            Modules.Prefabs.AddAISkillDriver(bossMasterPrefab, "Roll", SkillSlot.Utility, null,
                false, false,
                Mathf.NegativeInfinity, Mathf.Infinity,
                Mathf.NegativeInfinity, Mathf.Infinity,
                15f, Mathf.Infinity,
                false, false, false, -1,
                AISkillDriver.TargetType.CurrentEnemy,
                false, false, false,
                AISkillDriver.MovementType.ChaseMoveTarget, 1f,
                AISkillDriver.AimType.AtCurrentEnemy,
                false,
                true,
                false,
                AISkillDriver.ButtonPressType.Hold,
                0.5f,
                false,
                true,
                null);

            Modules.Prefabs.AddAISkillDriver(bossMasterPrefab, "Primary", SkillSlot.Primary, null,
                true, false,
                Mathf.NegativeInfinity, Mathf.Infinity,
                Mathf.NegativeInfinity, Mathf.Infinity,
                0f, 12f,
                true, false, false, -1,
                AISkillDriver.TargetType.CurrentEnemy,
                true, false, false,
                AISkillDriver.MovementType.ChaseMoveTarget, 1f,
                AISkillDriver.AimType.AtCurrentEnemy,
                false,
                true,
                false,
                AISkillDriver.ButtonPressType.Hold,
                -1f,
                false,
                false,
                null);

            Modules.Prefabs.AddAISkillDriver(bossMasterPrefab, "Chase", SkillSlot.None, null,
                false, false,
                Mathf.NegativeInfinity, Mathf.Infinity,
                Mathf.NegativeInfinity, Mathf.Infinity,
                0f, Mathf.Infinity,
                false, false, false, -1,
                AISkillDriver.TargetType.CurrentEnemy,
                false, false, false,
                AISkillDriver.MovementType.ChaseMoveTarget, 1f,
                AISkillDriver.AimType.AtCurrentEnemy,
                false,
                true,
                false,
                AISkillDriver.ButtonPressType.Abstain,
                -1,
                false,
                false,
                null);

            NemesisInvasionCore.AddNemesisBoss(bossMasterPrefab, null, "SS2U_StirringSoul", true);
        }

        internal override void InitializeUnlockables()
        {
            masterySkinUnlockableDef = ScriptableObject.CreateInstance<UnlockableDef>();
            masterySkinUnlockableDef.cachedName = "Skins.SS2UNemmando.Mastery";
            masterySkinUnlockableDef.nameToken = "ACHIEVEMENT_SS2UNEMMANDOCLEARGAMEMONSOON_NAME";
            masterySkinUnlockableDef.achievementIcon = Assets.mainAssetBundle.LoadAsset<Sprite>("texNemmandoSkinMaster");
            Unlockables.unlockableDefs.Add(masterySkinUnlockableDef);
            AchievementHider.unlockableRewardIdentifiers.Remove(masterySkinUnlockableDef.cachedName);

            grandMasterySkinUnlockableDef = ScriptableObject.CreateInstance<UnlockableDef>();
            grandMasterySkinUnlockableDef.cachedName = "Skins.SS2UNemmando.GrandMastery";
            grandMasterySkinUnlockableDef.nameToken = "ACHIEVEMENT_SS2UNEMMANDOCLEARGAMETYPHOON_NAME";
            grandMasterySkinUnlockableDef.achievementIcon = Assets.mainAssetBundle.LoadAsset<Sprite>("texNemmandoSkinGrandMaster");
            Unlockables.unlockableDefs.Add(grandMasterySkinUnlockableDef);
            AchievementHider.unlockableRewardIdentifiers.Remove(grandMasterySkinUnlockableDef.cachedName);

            //Assets.mainAssetBundle.LoadAsset<Sprite>("texNemmandoIconUnlock");
            //Assets.mainAssetBundle.LoadAsset<Sprite>("texSingleTapUnlockIcon");
            //Assets.mainAssetBundle.LoadAsset<Sprite>("texDecisiveStrike");
        }

        internal override void InitializeDoppelganger()
        {
            GameObject masterPrefab = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterMasters/CommandoMonsterMaster"), "SS2UNemmandoMonsterMaster", true);
            masterPrefab.GetComponent<CharacterMaster>().bodyPrefab = bodyPrefab;
            Modules.Prefabs.masterPrefabs.Add(masterPrefab);

            Modules.Prefabs.RemoveAISkillDrivers(masterPrefab);
            Modules.Prefabs.AddAISkillDriver(masterPrefab, "Special", SkillSlot.Special, null,
                  true, false,
                  Mathf.NegativeInfinity, Mathf.Infinity,
                  Mathf.NegativeInfinity, Mathf.Infinity,
                  0f, 30f,
                  true, false, false, -1,
                  AISkillDriver.TargetType.CurrentEnemy,
                  true, false, false,
                  AISkillDriver.MovementType.ChaseMoveTarget, 1f,
                  AISkillDriver.AimType.AtCurrentEnemy,
                  true,
                  false,
                  false,
                  AISkillDriver.ButtonPressType.Hold,
                  3f,
                  false,
                  true,
                  null);

            Modules.Prefabs.AddAISkillDriver(masterPrefab, "Secondary", SkillSlot.Secondary, null,
                 true, false,
                 Mathf.NegativeInfinity, Mathf.Infinity,
                 Mathf.NegativeInfinity, Mathf.Infinity,
                 10f, 40f,
                 true, false, true, -1,
                 AISkillDriver.TargetType.CurrentEnemy,
                 true, true, true,
                 AISkillDriver.MovementType.ChaseMoveTarget, 1f,
                 AISkillDriver.AimType.AtCurrentEnemy,
                 false,
                 true,
                 false,
                 AISkillDriver.ButtonPressType.Hold,
                 1f,
                 false,
                 false,
                 null);

            Modules.Prefabs.AddAISkillDriver(masterPrefab, "Roll", SkillSlot.Utility, null,
                false, false,
                Mathf.NegativeInfinity, Mathf.Infinity,
                Mathf.NegativeInfinity, Mathf.Infinity,
                15f, Mathf.Infinity,
                false, false, false, -1,
                AISkillDriver.TargetType.CurrentEnemy,
                false, false, false,
                AISkillDriver.MovementType.ChaseMoveTarget, 1f,
                AISkillDriver.AimType.AtCurrentEnemy,
                false,
                true,
                false,
                AISkillDriver.ButtonPressType.Hold,
                0.5f,
                false,
                true,
                null);

            Modules.Prefabs.AddAISkillDriver(masterPrefab, "Primary", SkillSlot.Primary, null,
                true, false,
                Mathf.NegativeInfinity, Mathf.Infinity,
                Mathf.NegativeInfinity, Mathf.Infinity,
                0f, 12f,
                true, false, false, -1,
                AISkillDriver.TargetType.CurrentEnemy,
                true, false, false,
                AISkillDriver.MovementType.ChaseMoveTarget, 1f,
                AISkillDriver.AimType.AtCurrentEnemy,
                false,
                true,
                false,
                AISkillDriver.ButtonPressType.Hold,
                -1f,
                false,
                false,
                null);

            Modules.Prefabs.AddAISkillDriver(masterPrefab, "Chase", SkillSlot.None, null,
                false, false,
                Mathf.NegativeInfinity, Mathf.Infinity,
                Mathf.NegativeInfinity, Mathf.Infinity,
                0f, Mathf.Infinity,
                false, false, false, -1,
                AISkillDriver.TargetType.CurrentEnemy,
                false, false, false,
                AISkillDriver.MovementType.ChaseMoveTarget, 1f,
                AISkillDriver.AimType.AtCurrentEnemy,
                false,
                true,
                false,
                AISkillDriver.ButtonPressType.Abstain,
                -1,
                false,
                false,
                null);
        }

        internal override void InitializeHitboxes()
        {
            ChildLocator childLocator = bodyPrefab.GetComponentInChildren<ChildLocator>();
            GameObject model = childLocator.gameObject;

            Transform hitboxTransform = childLocator.FindChild("SwordHitbox");
            Modules.Prefabs.SetupHitbox(model, hitboxTransform, "SwordHitbox");

            hitboxTransform = childLocator.FindChild("SwordHitboxLarge");
            Modules.Prefabs.SetupHitbox(model, hitboxTransform, "SwordHitboxLarge");

            hitboxTransform = childLocator.FindChild("SwordHitboxWide");
            Modules.Prefabs.SetupHitbox(model, hitboxTransform, "SwordHitboxWide");
        }

        internal override void InitializeSkills()
        {
            RegisterStates();

            Modules.Skills.CreateSkillFamilies(bodyPrefab);

            #region Primary
            SkillDef m1SkillDef = Modules.Skills.CreatePrimarySkillDef(new SerializableEntityStateType(typeof(BladeOfCessation2)), "Weapon", "SS2UNEMMANDO_PRIMARY_BLADE_NAME", "SS2UNEMMANDO_PRIMARY_BLADE_DESCRIPTION", Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("texBladeOfCessation"), true);
            m1SkillDef.keywordTokens = new string[] { "KEYWORD_SS2U_GOUGE", "KEYWORD_AGILE" };

            Modules.Skills.AddPrimarySkill(bodyPrefab, m1SkillDef);
            #endregion

            #region Secondary
            SkillDef gashSkillDef = Modules.Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "SS2UNEMMANDO_SECONDARY_CONCUSSION_NAME",
                skillNameToken = "SS2UNEMMANDO_SECONDARY_CONCUSSION_NAME",
                skillDescriptionToken = "SS2UNEMMANDO_SECONDARY_CONCUSSION_DESCRIPTION",
                skillIcon = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("texPhaseCharge"),
                activationState = new SerializableEntityStateType(typeof(ChargeSwordBeam)),
                activationStateMachineName = "Weapon",
                baseMaxStock = 1,
                baseRechargeInterval = 4f,
                beginSkillCooldownOnSkillEnd = false,
                canceledFromSprinting = false,
                forceSprintDuringState = false,
                fullRestockOnAssign = true,
                interruptPriority = InterruptPriority.Skill,
                resetCooldownTimerOnUse = false,
                isCombatSkill = true,
                mustKeyPress = false,
                cancelSprintingOnActivation = false,
                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,
                keywordTokens = new string[] { "KEYWORD_SS2U_GOUGE" }
            });
            secondaryDistantGash = gashSkillDef;

            SkillDef gunSkillDef = Modules.Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "SS2UNEMMANDO_SECONDARY_SHOOT_NAME",
                skillNameToken = "SS2UNEMMANDO_SECONDARY_SHOOT_NAME",
                skillDescriptionToken = "SS2UNEMMANDO_SECONDARY_SHOOT_DESCRIPTION",
                skillIcon = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("texSubmission"),
                activationState = new SerializableEntityStateType(typeof(ShootGun)),
                activationStateMachineName = "Slide",
                baseMaxStock = 6,
                baseRechargeInterval = 2f,
                beginSkillCooldownOnSkillEnd = false,
                canceledFromSprinting = false,
                forceSprintDuringState = false,
                fullRestockOnAssign = true,
                interruptPriority = InterruptPriority.Skill,
                resetCooldownTimerOnUse = true,
                isCombatSkill = true,
                mustKeyPress = false,
                cancelSprintingOnActivation = false,
                rechargeStock = 6,
                requiredStock = 1,
                stockToConsume = 1,
                keywordTokens = new string[] {}
            });

            Modules.Skills.AddSecondarySkill(bodyPrefab, gashSkillDef);
            Modules.Skills.AddSecondarySkill(bodyPrefab, gunSkillDef);//, singleTapUnlockableDef
            #endregion

            #region Utility
            SkillDef dashSkillDef = Modules.Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "SS2UNEMMANDO_UTILITY_DODGE_NAME",
                skillNameToken = "SS2UNEMMANDO_UTILITY_DODGE_NAME",
                skillDescriptionToken = "SS2UNEMMANDO_UTILITY_DODGE_DESCRIPTION",
                skillIcon = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("texTacticalRoll"),
                activationState = new SerializableEntityStateType(typeof(DodgeState)),
                activationStateMachineName = "Body",
                baseMaxStock = 1,
                baseRechargeInterval = 4f,
                beginSkillCooldownOnSkillEnd = false,
                canceledFromSprinting = false,
                forceSprintDuringState = true,
                fullRestockOnAssign = true,
                interruptPriority = InterruptPriority.PrioritySkill,
                resetCooldownTimerOnUse = false,
                isCombatSkill = false,
                mustKeyPress = false,
                cancelSprintingOnActivation = false,
                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,
            });

            Modules.Skills.AddUtilitySkills(bodyPrefab, dashSkillDef);
            #endregion

            #region Special
            SkillDef submissionSkillDef = Modules.Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "SS2UNEMMANDO_SPECIAL_SUBMISSION_NAME",
                skillNameToken = "SS2UNEMMANDO_SPECIAL_SUBMISSION_NAME",
                skillDescriptionToken = "SS2UNEMMANDO_SPECIAL_SUBMISSION_DESCRIPTION",
                skillIcon = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("texSubmission"),
                activationState = new SerializableEntityStateType(typeof(ChargeBarrageCharge)),
                activationStateMachineName = "Slide",
                baseMaxStock = 1,
                baseRechargeInterval = 12f,
                beginSkillCooldownOnSkillEnd = true,
                canceledFromSprinting = false,
                forceSprintDuringState = false,
                fullRestockOnAssign = true,
                interruptPriority = InterruptPriority.PrioritySkill,
                resetCooldownTimerOnUse = false,
                isCombatSkill = true,
                mustKeyPress = false,
                cancelSprintingOnActivation = false,
                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1
            });
            specialSubmission = submissionSkillDef;

            decisiveStrikeSkillDef = Modules.Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "SS2UNEMMANDO_SPECIAL_EPIC_NAME",
                skillNameToken = "SS2UNEMMANDO_SPECIAL_EPIC_NAME",
                skillDescriptionToken = "SS2UNEMMANDO_SPECIAL_EPIC_DESCRIPTION",
                skillIcon = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("texDecisiveStrike"),
                activationState = new SerializableEntityStateType(typeof(ChargedSlashCharge)),
                activationStateMachineName = "Body",
                baseMaxStock = 1,
                baseRechargeInterval = 12f,
                beginSkillCooldownOnSkillEnd = true,
                canceledFromSprinting = false,
                forceSprintDuringState = false,
                fullRestockOnAssign = true,
                interruptPriority = InterruptPriority.PrioritySkill,
                resetCooldownTimerOnUse = false,
                isCombatSkill = true,
                mustKeyPress = false,
                cancelSprintingOnActivation = true,
                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,
                keywordTokens = new string[] { "KEYWORD_SS2U_GOUGE" }
            });
            specialDecisiveStrike = decisiveStrikeSkillDef;

            Modules.Skills.AddSpecialSkill(bodyPrefab, submissionSkillDef);
            Modules.Skills.AddSpecialSkill(bodyPrefab, decisiveStrikeSkillDef);//, decisiveStrikeUnlockableDef
            #endregion

            SetupScepterSkills();
            if (StarstormPlugin.scepterPluginLoaded)
            {
                ScepterSetup();
            }
            if (StarstormPlugin.classicItemsLoaded)
            {
                ClassicScepterSetup();
            }
        }

        private void RegisterStates()
        {
            Modules.States.AddState(typeof(EntityStates.SS2UStates.Common.NemmandoSpawnState));
            Modules.States.AddState(typeof(NemmandoMain));
            Modules.States.AddState(typeof(BladeOfCessation2));
            Modules.States.AddState(typeof(ChargeSwordBeam));
            Modules.States.AddState(typeof(DodgeState));
            Modules.States.AddState(typeof(Submission));
            Modules.States.AddState(typeof(ChargedSlashCharge));
            Modules.States.AddState(typeof(ScepterSlashCharge));
            Modules.States.AddState(typeof(ScepterSlashEntry));
            Modules.States.AddState(typeof(ScepterSlashAttack));
            Modules.States.AddState(typeof(ScepterBarrageCharge));
            Modules.States.AddState(typeof(ScepterBarrageFire));

            Modules.States.AddState(typeof(NemmandoRestEmote));
            Modules.States.AddState(typeof(NemmandoTauntEmote));
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private void ScepterSetup()
        {

            AncientScepter.AncientScepterItem.instance.RegisterScepterSkill(scepterSubmissionSkillDef, bodyInfo.bodyName, specialSubmission);
            AncientScepter.AncientScepterItem.instance.RegisterScepterSkill(scepterDecisiveStrikeSkillDef, bodyInfo.bodyName, specialDecisiveStrike);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private void ClassicScepterSetup()
        {
            ThinkInvisible.ClassicItems.Scepter.instance.RegisterScepterSkill(scepterSubmissionSkillDef, bodyInfo.bodyName, SkillSlot.Special, specialSubmission);
            ThinkInvisible.ClassicItems.Scepter.instance.RegisterScepterSkill(scepterDecisiveStrikeSkillDef, bodyInfo.bodyName, SkillSlot.Special, specialDecisiveStrike);
        }

        private static void SetupScepterSkills()
        {
            scepterSubmissionSkillDef = Modules.Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "SS2UNEMMANDO_SPECIAL_SCEPSUBMISSION_NAME",
                skillNameToken = "SS2UNEMMANDO_SPECIAL_SCEPSUBMISSION_NAME",
                skillDescriptionToken = "SS2UNEMMANDO_SPECIAL_SCEPSUBMISSION_DESCRIPTION",
                skillIcon = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("texSubmissionScepter"),
                activationState = new SerializableEntityStateType(typeof(ScepterBarrageCharge)),
                activationStateMachineName = "Slide",
                baseMaxStock = 1,
                baseRechargeInterval = 12f,
                beginSkillCooldownOnSkillEnd = true,
                canceledFromSprinting = false,
                forceSprintDuringState = false,
                fullRestockOnAssign = true,
                interruptPriority = InterruptPriority.PrioritySkill,
                resetCooldownTimerOnUse = false,
                isCombatSkill = true,
                mustKeyPress = false,
                cancelSprintingOnActivation = false,
                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1
            });

            scepterDecisiveStrikeSkillDef = Modules.Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "SS2UNEMMANDO_SPECIAL_SCEPEPIC_NAME",
                skillNameToken = "SS2UNEMMANDO_SPECIAL_SCEPEPIC_NAME",
                skillDescriptionToken = "SS2UNEMMANDO_SPECIAL_SCEPEPIC_DESCRIPTION",
                skillIcon = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("texDecisiveStrikeScepter"),
                activationState = new SerializableEntityStateType(typeof(ScepterSlashCharge)),
                activationStateMachineName = "Body",
                baseMaxStock = 1,
                baseRechargeInterval = 12f,
                beginSkillCooldownOnSkillEnd = true,
                canceledFromSprinting = false,
                forceSprintDuringState = false,
                fullRestockOnAssign = true,
                interruptPriority = InterruptPriority.PrioritySkill,
                resetCooldownTimerOnUse = false,
                isCombatSkill = true,
                mustKeyPress = false,
                cancelSprintingOnActivation = true,
                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,
                keywordTokens = new string[] { "KEYWORD_SS2U_GOUGE" }
            });
        }

        internal override void RegisterTokens()
        {
            LanguageAPI.Add("SS2UNEMMANDO_NAME", "Nemesis Commando");
            LanguageAPI.Add("SS2UNEMMANDO_SUBTITLE", "Lone Echo");
            LanguageAPI.Add("SS2UNEMMANDO_DESCRIPTION", "The Nemesis Commando's origins are unknown, but something is clear: he's no longer who he once was.<style=cSub>\r\n\r\n" +
                "< ! > Blade of Cessation is risky but can build up a lot of damage by stacking Gouge.\r\n\r\n" +
                "< ! > Distant Gash can be used to safely apply stacks of Gouge to groups of enemies - take advantage of its piercing!\r\n\r\n" +
                "< ! > Proper balance of melee and ranged combat is key, and Tactical Roll grants high armor to help enter and exit battles.\r\n\r\n" +
                "< ! > Submission deals massive damage when fully charged.\r\n\r\n");
            LanguageAPI.Add("SS2UNEMMANDO_OUTRO_FLAVOR", "..and so he left, newfound sense of humanity within.");
            LanguageAPI.Add("SS2UNEMMANDO_OUTRO_FAILURE", "..and so he returned, concluding his endless struggle.");
            LanguageAPI.Add("SS2UNEMMANDO_LORE", "He lays to rest by a fire, setting the weapons he carries within an arm's reach. Checking his surroundings, hoping the cave he hid in would be enough shelter. Some would say, it isn't wise to be overly cautious. He would say, <color=#FFCCED>they haven't seen enough.</color>\n\n" +
                "The service pistol he got when he enlisted. Well used, dirtied, and loved. It's brought him through thick and thin, no matter which mission he took on. Some would say, only a fool would trust their weapon. He would say, <color=#FFCCED>your weapon is all you can trust.</color>\n\n" +
                "The blade of a fallen brother of war. Picked from what he could scavenge of the last of humanity he's seen. Some would say, it keeps their spirits united. He would say, <color=#FFCCED>it's to keep their spirits from joining for as long as possible.</color>\n\n" +
                "The loads of cargo that landed with him. Each item picked from a chest, keeping him alive in one way or another. Dozens of shipments to anyone but him. Some would say, they aren't his items to take. He would say, <color=#FFCCED>if he doesn't take them, then no one will.</color>\n\n" +
                "The glow of a violet light coming from deeper within the cave. Calming, warming... beckoning. Welcoming him deeper and deeper, promising safety. Some would say, what sounds too good to be true probably is. He would say, <color=#FFCCED>Faust was a wise man.</color>");

            LanguageAPI.Add("SS2UNEMMANDO_DEFAULT_SKIN_NAME", "Default");
            LanguageAPI.Add("SS2UNEMMANDO_MASTERY_SKIN_NAME", "Minuano");
            LanguageAPI.Add("SS2UNEMMANDO_CLASSIC_SKIN_NAME", "Classic");
            LanguageAPI.Add("SS2UNEMMANDO_COMMANDO_SKIN_NAME", "Commando");
            LanguageAPI.Add("SS2UNEMMANDO_VERGIL_SKIN_NAME", "Motivator");

            float gougeDuration = 2f;
            string gougeDamage = $"<style=cIsDamage>{gougeDuration * DamageTypeCore.gougeDamageCoefficient * 100}%</style> base damage";
            string bladeDamage = $"<style=cIsDamage>{BladeOfCessation2.damageCoefficient * 100}% damage</style>";

            LanguageAPI.Add("KEYWORD_SS2U_GOUGE", $"<style=cKeywordName>Gouge</style><style=cSub>Deal {gougeDamage} over {gougeDuration}s. <i>Capable of triggering item effects.</i>");

            LanguageAPI.Add("SS2UNEMMANDO_PRIMARY_BLADE_NAME", "Blade of Cessation");
            LanguageAPI.Add("SS2UNEMMANDO_PRIMARY_BLADE_DESCRIPTION", $@"<style=cIsHealth>Gouging</style>. <style=cIsUtility>Agile.</style> Slice enemies for {bladeDamage}.");

            LanguageAPI.Add("SS2UNEMMANDO_SECONDARY_CONCUSSION_NAME", "Distant Gash");

            string chargeDamage = $"<style=cIsDamage>{FireSwordBeam.minDamageCoeffficient * 100}%-{FireSwordBeam.maxDamageCoefficient * 100}% damage</style>";
            LanguageAPI.Add("SS2UNEMMANDO_SECONDARY_CONCUSSION_DESCRIPTION", $"<style=cIsHealth>Gouging</style>. Charge a piercing sword beam for {chargeDamage}.");

            LanguageAPI.Add("SS2UNEMMANDO_SECONDARY_SHOOT_NAME", "Single Tap");

            chargeDamage = $"<style=cIsDamage>{ShootGun.damageCoefficient * 100}% damage</style>";
            LanguageAPI.Add("SS2UNEMMANDO_SECONDARY_SHOOT_DESCRIPTION", $"Fire your gun for {chargeDamage}.");

            LanguageAPI.Add("SS2UNEMMANDO_UTILITY_DODGE_NAME", "Tactical Roll");
            LanguageAPI.Add("SS2UNEMMANDO_UTILITY_DODGE_DESCRIPTION", "<style=cIsUtility>Roll</style> a short distance and gain <style=cIsUtility>100 armor</style>.");

            LanguageAPI.Add("SS2UNEMMANDO_SPECIAL_SUBMISSION_NAME", "Submission");
            LanguageAPI.Add("SS2UNEMMANDO_SPECIAL_SUBMISSION_DESCRIPTION", $"Charge up a barrage of <style=cIsDamage>5 shotgun blasts</style> that deal <style=cIsDamage>4x{ChargeBarrageFire.damageCoefficient * 100}% damage</style> each. The number of shots increases with attack speed.");

            LanguageAPI.Add("SS2UNEMMANDO_SPECIAL_EPIC_NAME", "Decisive Strike");
            string chargedSlashDamage = $"<style=cIsDamage>{ChargedSlashAttack.maxHits}x{ChargedSlashAttack.maxDamageCoefficient * 100}% damage</style>";
            LanguageAPI.Add("SS2UNEMMANDO_SPECIAL_EPIC_DESCRIPTION", $"<style=cIsHealth>Gouging</style>. Charge up a devastating barrage of <style=cIsDamage>slashes</style> that deals up to {chargedSlashDamage}.");

            string desc = $"Charge up a barrage of <style=cIsDamage>5 shotgun blasts</style> that deal <style=cIsDamage>4x{ChargeBarrageFire.damageCoefficient * 100}% damage</style> each. The number of shots increases with attack speed.";
            desc += Helpers.ScepterDescription($"Fire lasers for {100f * ScepterBarrageFire.laserDamageCoefficient}% damage.");

            LanguageAPI.Add("SS2UNEMMANDO_SPECIAL_SCEPSUBMISSION_NAME", "Subjunction");
            LanguageAPI.Add("SS2UNEMMANDO_SPECIAL_SCEPSUBMISSION_DESCRIPTION", desc);

            chargedSlashDamage = $"<style=cIsDamage>{ChargedSlashAttack.maxHits}x{ChargedSlashAttack.maxDamageCoefficient * 100}% damage</style>";
            desc = $"<style=cIsHealth>Gouging</style>. Charge up a devastating barrage of <style=cIsDamage>slashes</style> that deals up to {chargedSlashDamage}.";
            desc += Helpers.ScepterDescription("Briefly disappear and then unleash all your strikes at once. Hit count increases the more attack speed you have.");

            LanguageAPI.Add("SS2UNEMMANDO_SPECIAL_SCEPEPIC_NAME", "Judgement Cut End");
            LanguageAPI.Add("SS2UNEMMANDO_SPECIAL_SCEPEPIC_DESCRIPTION", desc);

            // todo: make a base class for mastery achievements and simply inherit from it for each character 
            LanguageAPI.Add("ACHIEVEMENT_SS2UNEMMANDOCLEARGAMEMONSOON_NAME", "Nemesis Commando: Mastery");
            LanguageAPI.Add("ACHIEVEMENT_SS2UNEMMANDOCLEARGAMEMONSOON_DESCRIPTION", "As Nemesis Commando, beat the game or obliterate on Monsoon.");

            LanguageAPI.Add("ACHIEVEMENT_SS2UNEMMANDOCLEARGAMETYPHOON_NAME", "Nemesis Commando: Grand Mastery");
            LanguageAPI.Add("ACHIEVEMENT_SS2UNEMMANDOCLEARGAMETYPHOON_DESCRIPTION", "As Nemesis Commando, beat the game or obliterate on Typhoon.");

            LanguageAPI.Add("ACHIEVEMENT_SS2UNEMMANDOKILLSELF_NAME", "Nemesis Commando: Nemesis Nemesis");
            LanguageAPI.Add("ACHIEVEMENT_SS2UNEMMANDOKILLSELF_DESCRIPTION", "As Nemesis Commando, defeat Commando's vestige.");

            LanguageAPI.Add("SS2UNEMMANDO_EPICUNLOCKABLE_ACHIEVEMENT_NAME", "Nemesis Commando: Zandatsu");
            LanguageAPI.Add("SS2UNEMMANDO_EPICUNLOCKABLE_ACHIEVEMENT_DESC", "As Nemesis Commando, inflict 50 stacks of Gouge on one enemy.");
            LanguageAPI.Add("SS2UNEMMANDO_EPICUNLOCKABLE_UNLOCKABLE_NAME", "Nemesis Commando: Zandatsu");

            LanguageAPI.Add("ACHIEVEMENT_SS2UNEMMANDOUNLOCK_NAME", "???");
            LanguageAPI.Add("ACHIEVEMENT_SS2UNEMMANDOUNLOCK_DESCRIPTION", "Defeat Commando's vestige.");
        }

        #region Hooks
        internal override void Hook()
        {
            On.RoR2.MapZone.TryZoneStart += MapZone_TryZoneStart;
            On.RoR2.HealthComponent.Suicide += HealthComponent_Suicide;
            On.EntityStates.GlobalSkills.LunarDetonator.Detonate.OnEnter += PlayRuinAnimation;
        }

        private void PlayRuinAnimation(On.EntityStates.GlobalSkills.LunarDetonator.Detonate.orig_OnEnter orig, EntityStates.GlobalSkills.LunarDetonator.Detonate self)
        {
            orig(self);

            if (self.characterBody.bodyIndex == NemmandoCore.bodyIndex)
            {
                self.PlayAnimation("Gesture, Override", "CastRuin");
                self.StartAimMode(self.duration + 0.5f);

                EffectManager.SimpleMuzzleFlash(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/MuzzleFlashes/MuzzleflashLunarNeedle"), self.gameObject, "Head", false);
            }
        }

        private void MapZone_TryZoneStart(On.RoR2.MapZone.orig_TryZoneStart orig, MapZone self, Collider other)
        {
            if (other.gameObject)
            {
                CharacterBody body = other.GetComponent<CharacterBody>();
                if (body)
                {
                    if (body.bodyIndex == NemmandoCore.bodyIndex)
                    {
                        var teamComponent = body.teamComponent;
                        if (teamComponent)
                        {
                            if (teamComponent.teamIndex != TeamIndex.Player)
                            {
                                TeamIndex origIndex = teamComponent.teamIndex;
                                teamComponent.teamIndex = TeamIndex.Player;
                                orig(self, other);
                                teamComponent.teamIndex = origIndex;
                                return;
                            }
                        }
                    }
                }
            }
            orig(self, other);
        }

        private void HealthComponent_Suicide(On.RoR2.HealthComponent.orig_Suicide orig, HealthComponent self, GameObject killerOverride, GameObject inflictorOverride, DamageType damageType)
        {

            if (damageType == DamageType.VoidDeath)
            {
                if (self.body.bodyIndex == NemmandoCore.bodyIndex)
                {
                    if (self.body.teamComponent.teamIndex != TeamIndex.Player)
                    {
                        ChatMessage.SendColored("He laughs in the face of the void.", new Color(0.149f, 0.0039f, 0.2117f));
                        return;
                    }
                }
            }
            orig(self, killerOverride, inflictorOverride, damageType);
        }
        #endregion

        public static class SkinDefs
        {
            public static SkinDef Default, Mastery, Grandmastery, Commando, CommandoJoke, Vergil;
        }

        internal override void InitializeSkins()
        {
            GameObject model = bodyPrefab.GetComponentInChildren<ModelLocator>().modelTransform.gameObject;
            CharacterModel characterModel = model.GetComponent<CharacterModel>();

            ModelSkinController skinController = model.AddComponent<ModelSkinController>();
            ChildLocator childLocator = model.GetComponent<ChildLocator>();

            SkinnedMeshRenderer mainRenderer = characterModel.mainSkinnedMeshRenderer;

            CharacterModel.RendererInfo[] defaultRenderers = characterModel.baseRendererInfos;

            List<SkinDef> skins = new List<SkinDef>();

            GameObject coat = childLocator.FindChild("Coat").gameObject;

            SkinDef.GameObjectActivation[] defaultActivations = new SkinDef.GameObjectActivation[]
            {
                new SkinDef.GameObjectActivation
                {
                    gameObject = coat,
                    shouldActivate = false
                }
            };

            SkinDef.GameObjectActivation[] coatActivations = new SkinDef.GameObjectActivation[]
            {
                new SkinDef.GameObjectActivation
                {
                    gameObject = coat,
                    shouldActivate = true
                }
            };

            #region DefaultSkin
            SkinDef defaultSkin = Modules.Skins.CreateSkinDef("SS2UNEMMANDO_DEFAULT_SKIN_NAME",
                Assets.mainAssetBundle.LoadAsset<Sprite>("texNemmandoSkin"),
                defaultRenderers,
                mainRenderer,
                model);

            defaultSkin.meshReplacements = new SkinDef.MeshReplacement[]
            {
                new SkinDef.MeshReplacement
                {
                    mesh = Modules.Assets.mainAssetBundle.LoadAsset<Mesh>("meshNemmando"),
                    renderer = defaultRenderers[0].renderer
                },
                new SkinDef.MeshReplacement
                {
                    mesh = Modules.Assets.mainAssetBundle.LoadAsset<Mesh>("meshNemmandoSword"),
                    renderer = defaultRenderers[1].renderer
                },
                new SkinDef.MeshReplacement
                {
                    mesh = Modules.Assets.mainAssetBundle.LoadAsset<Mesh>("meshNemmandoGun"),
                    renderer = defaultRenderers[2].renderer
                }
            };

            defaultSkin.gameObjectActivations = defaultActivations;

            skins.Add(defaultSkin);
            SkinDefs.Default = defaultSkin;
            #endregion

            #region MasterySkin
            Material masteryMat = Modules.Assets.CreateMaterial("matNemmandoMGS", 1f);
            Material masterySword = Modules.Assets.CreateMaterial("matNemmandoMGS", 0f);
            Material masteryMatNoEmission = Modules.Assets.CreateMaterial("matNemmandoMGS", 0f);
            CharacterModel.RendererInfo[] masteryRendererInfos = SkinRendererInfos(defaultRenderers, new Material[]
            {
                masteryMat,
                masterySword,
                masteryMatNoEmission
            });

            SkinDef masterySkin = Modules.Skins.CreateSkinDef("SS2UNEMMANDO_MASTERY_SKIN_NAME",
                Assets.mainAssetBundle.LoadAsset<Sprite>("texNemmandoSkinMaster"),
                masteryRendererInfos,
                mainRenderer,
                model,
                Modules.Config.ForceUnlockSkins.Value ? null : masterySkinUnlockableDef);

            masterySkin.meshReplacements = new SkinDef.MeshReplacement[]
            {
                new SkinDef.MeshReplacement
                {
                    mesh = Modules.Assets.mainAssetBundle.LoadAsset<Mesh>("meshNemmandoMGS"),
                    renderer = defaultRenderers[0].renderer
                },
                new SkinDef.MeshReplacement
                {
                    mesh = Modules.Assets.mainAssetBundle.LoadAsset<Mesh>("meshNemmandoSwordMGS"),
                    renderer = defaultRenderers[1].renderer
                },
                new SkinDef.MeshReplacement
                {
                    mesh = Modules.Assets.mainAssetBundle.LoadAsset<Mesh>("meshNemmandoClassicGun"),
                    renderer = defaultRenderers[2].renderer
                }
            };

            masterySkin.gameObjectActivations = defaultActivations;

            skins.Add(masterySkin);
            SkinDefs.Mastery = masterySkin;
            #endregion

            #region GrandMasterySkin
            CharacterModel.RendererInfo[] grandMasteryRendererInfos = SkinRendererInfos(defaultRenderers, new Material[]
            {
                Modules.Assets.CreateMaterial("matNemmandoClassic", 1, Color.white, 1),
                Modules.Assets.CreateMaterial("matNemmando", 70, Color.white),
                Modules.Assets.CreateMaterial("matNemmandoClassic", 0, Color.white)
            });

            SkinDef grandMasterySkin = Modules.Skins.CreateSkinDef("SS2UNEMMANDO_CLASSIC_SKIN_NAME",
                Assets.mainAssetBundle.LoadAsset<Sprite>("texNemmandoSkinGrandMaster"),
                grandMasteryRendererInfos,
                mainRenderer,
                model,
                Modules.Config.ForceUnlockSkins.Value ? null : grandMasterySkinUnlockableDef);

            grandMasterySkin.meshReplacements = new SkinDef.MeshReplacement[]
            {
                new SkinDef.MeshReplacement
                {
                    mesh = Modules.Assets.mainAssetBundle.LoadAsset<Mesh>("meshNemmandoClassic"),
                    renderer = defaultRenderers[0].renderer
                },
                new SkinDef.MeshReplacement
                {
                    mesh = Modules.Assets.mainAssetBundle.LoadAsset<Mesh>("meshNemmandoSword"),
                    renderer = defaultRenderers[1].renderer
                },
                new SkinDef.MeshReplacement
                {
                    mesh = Modules.Assets.mainAssetBundle.LoadAsset<Mesh>("meshNemmandoClassicGun"),
                    renderer = defaultRenderers[2].renderer
                }
            };

            grandMasterySkin.gameObjectActivations = defaultActivations;

            skins.Add(grandMasterySkin);
            SkinDefs.Grandmastery = grandMasterySkin;
            #endregion

            #region CommandoSkin
            CharacterModel.RendererInfo[] commandoRendererInfos = SkinRendererInfos(defaultRenderers, new Material[]
            {
                Modules.Assets.CreateMaterial("matNemmandoAlt", 1f),
                Modules.Assets.CreateMaterial("matNemmandoAlt", 1f),
                Modules.Assets.commandoMat
            });

            CreateKillSelfUnlockable();

            SkinDef commandoSkin = Modules.Skins.CreateSkinDef("SS2UNEMMANDO_COMMANDO_SKIN_NAME",
                 Assets.mainAssetBundle.LoadAsset<Sprite>("texNemmandoCommandoSkin"),
                commandoRendererInfos,
                mainRenderer,
                model,
                (Modules.Config.EnableVoid.Value && !Modules.Config.ForceUnlockSkins.Value) ? NemmandoCore.killSelfUnlockableDef : null);

            commandoSkin.meshReplacements = new SkinDef.MeshReplacement[]
            {
                new SkinDef.MeshReplacement
                {
                    mesh = Modules.Assets.mainAssetBundle.LoadAsset<Mesh>("meshNemmando"),
                    renderer = defaultRenderers[0].renderer
                },
                new SkinDef.MeshReplacement
                {
                    mesh = Modules.Assets.mainAssetBundle.LoadAsset<Mesh>("meshNemmandoSword"),
                    renderer = defaultRenderers[1].renderer
                },
                new SkinDef.MeshReplacement
                {
                    mesh = Modules.Assets.mainAssetBundle.LoadAsset<Mesh>("meshNemmandoGun"),
                    renderer = defaultRenderers[2].renderer
                }
            };

            //Causes console errors
            /*commandoSkin.projectileGhostReplacements = new SkinDef.ProjectileGhostReplacement[]
                {
                    new SkinDef.ProjectileGhostReplacement
                    {
                        projectilePrefab = FireSwordBeam.projectilePrefab,
                        projectileGhostReplacementPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/ProjectileGhosts/EvisProjectileGhost")
                    }
                };*/

            commandoSkin.gameObjectActivations = defaultActivations;

            skins.Add(commandoSkin);
            SkinDefs.Commando = commandoSkin;
            #endregion

            #region Cursed
            if (Modules.Config.cursed.Value)
            {
                #region CommandoJokeSkin
                Material mercSwordMat = LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterBodies/MercBody").GetComponentInChildren<CharacterModel>().baseRendererInfos[1].defaultMaterial;
                CharacterModel.RendererInfo[] commandoJokeRendererInfos = SkinRendererInfos(defaultRenderers, new Material[]
                {
                Modules.Assets.commandoMat,
                mercSwordMat,
                Modules.Assets.commandoMat
                });

                SkinDef commandoJokeSkin = Modules.Skins.CreateSkinDef("SS2UNEMMANDO_COMMANDO_SKIN_NAME",
                    Assets.mainAssetBundle.LoadAsset<Sprite>("texNemmandoCommandoSkin"),
                    commandoJokeRendererInfos,
                    mainRenderer,
                    model,
                    (Modules.Config.EnableVoid.Value && !Modules.Config.ForceUnlockSkins.Value) ? NemmandoCore.killSelfUnlockableDef : null);

                commandoJokeSkin.meshReplacements = new SkinDef.MeshReplacement[]
                {
                new SkinDef.MeshReplacement
                {
                    mesh = Modules.Assets.mainAssetBundle.LoadAsset<Mesh>("meshCommandoMesh"),
                    renderer = defaultRenderers[0].renderer
                },
                new SkinDef.MeshReplacement
                {
                    mesh = Modules.Assets.mainAssetBundle.LoadAsset<Mesh>("meshMercSword"),
                    renderer = defaultRenderers[1].renderer
                },
                new SkinDef.MeshReplacement
                {
                    mesh = Modules.Assets.mainAssetBundle.LoadAsset<Mesh>("meshNemmandoGun"),
                    renderer = defaultRenderers[2].renderer
                }
                };

                commandoJokeSkin.gameObjectActivations = defaultActivations;

                skins.Add(commandoJokeSkin);
                SkinDefs.CommandoJoke = commandoJokeSkin;
                #endregion

                #region VergilSkin
                CharacterModel.RendererInfo[] vergilRendererInfos = SkinRendererInfos(defaultRenderers, new Material[]
                {
                    Modules.Assets.CreateMaterial("matVergil", 3f),
                    Modules.Assets.CreateMaterial("matYamato", 5f),
                    Modules.Assets.commandoMat
                });

                SkinDef vergilSkin = Modules.Skins.CreateSkinDef("SS2UNEMMANDO_VERGIL_SKIN_NAME",
                    Assets.mainAssetBundle.LoadAsset<Sprite>("texVergilSkin"),
                    vergilRendererInfos,
                    mainRenderer,
                    model,
                    decisiveStrikeUnlockableDef);

                vergilSkin.meshReplacements = new SkinDef.MeshReplacement[]
                {
                new SkinDef.MeshReplacement
                {
                    mesh = Modules.Assets.mainAssetBundle.LoadAsset<Mesh>("meshVergil"),
                    renderer = defaultRenderers[0].renderer
                },
                new SkinDef.MeshReplacement
                {
                    mesh = Modules.Assets.mainAssetBundle.LoadAsset<Mesh>("meshYamato"),
                    renderer = defaultRenderers[1].renderer
                },
                new SkinDef.MeshReplacement
                {
                    mesh = Modules.Assets.mainAssetBundle.LoadAsset<Mesh>("meshNemmandoGun"),
                    renderer = defaultRenderers[2].renderer
                }
                };

                vergilSkin.gameObjectActivations = coatActivations;

                //Causes console errors
                /*vergilSkin.projectileGhostReplacements = new SkinDef.ProjectileGhostReplacement[]
                {
                    new SkinDef.ProjectileGhostReplacement
                    {
                        projectilePrefab = FireSwordBeam.projectilePrefab,
                        projectileGhostReplacementPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/ProjectileGhosts/EvisProjectileGhost")
                    }
                };*/

                skins.Add(vergilSkin);
                SkinDefs.Vergil = vergilSkin;
                #endregion
            }
            #endregion

            skinController.skins = skins.ToArray();
        }

        internal override void SetItemDisplays()
        {
            instance.itemDisplayRules = new List<ItemDisplayRuleSet.KeyAssetRuleGroup>();

            NemmandoItemDisplays.RegisterDisplays();

            itemDisplayRuleSet.keyAssetRuleGroups = instance.itemDisplayRules.ToArray();
            //itemDisplayRuleSet.GenerateRuntimeValues();
        }

        private static CharacterModel.RendererInfo[] SkinRendererInfos(CharacterModel.RendererInfo[] defaultRenderers, Material[] materials)
        {
            CharacterModel.RendererInfo[] newRendererInfos = new CharacterModel.RendererInfo[defaultRenderers.Length];
            defaultRenderers.CopyTo(newRendererInfos, 0);

            newRendererInfos[0].defaultMaterial = materials[0];
            newRendererInfos[1].defaultMaterial = materials[1];
            newRendererInfos[2].defaultMaterial = materials[2];

            return newRendererInfos;
        }
    }
}

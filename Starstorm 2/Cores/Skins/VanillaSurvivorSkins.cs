﻿using System;
using System.Linq;
using UnityEngine;
using R2API;
using RoR2;
using R2API.Utils;
using Starstorm2.Cores.Unlockables;
using System.Collections.Generic;

namespace Starstorm2.Cores.Skins
{
    public static class VanillaSurvivorSkins
    {
        public static Mesh meshCommandoClassic;
        public static Mesh meshCommandoClassicGun;
        public static Mesh meshArmoredAcrid;

        public static void LoadMeshes(AssetBundle vanillaSurvivorsAssetBundle)
        {
            meshCommandoClassic = vanillaSurvivorsAssetBundle.LoadAsset<Mesh>("meshCommandoClassic");
            meshCommandoClassicGun = vanillaSurvivorsAssetBundle.LoadAsset<Mesh>("meshCommandoClassicGun");
        }

        public static void RegisterVanillaSurvivorSkins()
        {
            RegisterCommandoSkins();
            RegisterMultSkins();
            //RegisterAcridSkins();
        }

        private static void RegisterCommandoSkins()
        {
            GameObject bodyPrefab = BodyCatalog.FindBodyPrefab("CommandoBody");
            ModelSkinController skinController = bodyPrefab.GetComponentInChildren<ModelSkinController>();
            GameObject model = skinController.gameObject;
            CharacterModel characterModel = model.GetComponent<CharacterModel>();
            SkinnedMeshRenderer mainRenderer = Reflection.GetFieldValue<SkinnedMeshRenderer>(characterModel, "mainSkinnedMeshRenderer");

            CharacterModel.RendererInfo[] defaultRenderers = characterModel.baseRendererInfos;

            //List<SkinDef> skinDefs = new List<SkinDef>();

            #region Commando Classic
            CharacterModel.RendererInfo[] commandoRendererInfos = new CharacterModel.RendererInfo[defaultRenderers.Length];
            defaultRenderers.CopyTo(commandoRendererInfos, 0);


            Material commandoClassicMaterial = Modules.Assets.CreateMaterial("matCommandoClassic", 1, Color.white, 0);

            commandoRendererInfos[0].defaultMaterial = commandoClassicMaterial;
            commandoRendererInfos[1].defaultMaterial = commandoClassicMaterial;
            commandoRendererInfos[2].defaultMaterial = commandoClassicMaterial;

            LanguageAPI.Add("COMMANDO_GRANDMASTERY_SKIN_NAME", "Classic");
            SkinDef classicSkin = SkinsCore.CreateSkinDef("COMMANDO_GRANDMASTERY_SKIN_NAME",
                                                          Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("texCommandoSkinGrandMaster"),
                                                          commandoRendererInfos,
                                                          mainRenderer,
                                                          model,
                                                          VanillaSurvivorUnlockables.commandoGrandMastery);

            classicSkin.meshReplacements = SkinsCore.CreateMeshReplacements(commandoRendererInfos,
                                                                            Modules.Assets.mainAssetBundle.LoadAsset<Mesh>("meshCommandoClassicGun"),
                                                                            Modules.Assets.mainAssetBundle.LoadAsset<Mesh>("meshCommandoClassicGun"),
                                                                            Modules.Assets.mainAssetBundle.LoadAsset<Mesh>("meshCommandoClassic"));

            //skinDefs.Add(classicSkin);
            #endregion
            List<SkinDef> skinDefs = skinController.skins.ToList();
            skinDefs.Insert(2, classicSkin);

            skinController.skins = skinDefs.ToArray();

            SkinDef[][] skinsField = Reflection.GetFieldValue<SkinDef[][]>(typeof(BodyCatalog), "skins");
            skinsField[(int)BodyCatalog.FindBodyIndex(bodyPrefab)] = skinController.skins;
            BodyCatalog.skins = skinsField;
        }

        private static void RegisterMultSkins()
        {
            GameObject bodyPrefab = BodyCatalog.FindBodyPrefab("ToolbotBody");
            ModelSkinController skinController = bodyPrefab.GetComponentInChildren<ModelSkinController>();
            GameObject model = skinController.gameObject;
            CharacterModel characterModel = model.GetComponent<CharacterModel>();
            SkinnedMeshRenderer mainRenderer = Reflection.GetFieldValue<SkinnedMeshRenderer>(characterModel, "mainSkinnedMeshRenderer");

            CharacterModel.RendererInfo[] defaultRenderers = characterModel.baseRendererInfos;

            //List<SkinDef> skinDefs = new List<SkinDef>();

            #region Lunar MUL-T
            CharacterModel.RendererInfo[] toolbotRendererInfos = new CharacterModel.RendererInfo[defaultRenderers.Length];
            defaultRenderers.CopyTo(toolbotRendererInfos, 0);

            toolbotRendererInfos[1].defaultMaterial = Modules.Assets.matLunarGolem;

            LanguageAPI.Add("TOOLBOT_GRANDMASTERY_SKIN_NAME", "Lunar");
            SkinDef lunarSkin = SkinsCore.CreateSkinDef("TOOLBOT_GRANDMASTERY_SKIN_NAME",
                                                          Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("texToolbotSkinGrandMaster"),
                                                          toolbotRendererInfos,
                                                          mainRenderer,
                                                          model,
                                                          VanillaSurvivorUnlockables.toolbotGrandMastery);

            lunarSkin.meshReplacements = new SkinDef.MeshReplacement[]
            {
                new SkinDef.MeshReplacement
                {
                    mesh = Modules.Assets.mainAssetBundle.LoadAsset<Mesh>("meshToolbotGM"),
                    renderer = lunarSkin.rendererInfos[1].renderer.GetComponent<SkinnedMeshRenderer>()
                }
            };

            lunarSkin.projectileGhostReplacements = new SkinDef.ProjectileGhostReplacement[]
            {
                new SkinDef.ProjectileGhostReplacement
                {
                    projectilePrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/CryoCanisterProjectile"),
                    projectileGhostReplacementPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/ProjectileGhosts/LunarWispTrackingBombGhost")
                },
                new SkinDef.ProjectileGhostReplacement
                {
                    projectilePrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/CryoCanisterBombletsProjectile"),
                    projectileGhostReplacementPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/ProjectileGhosts/LunarGolemTwinShotProjectileGhost")
                },
                new SkinDef.ProjectileGhostReplacement
                {
                    projectilePrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/ToolbotGrenadeLauncherProjectile"),
                    projectileGhostReplacementPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/ProjectileGhosts/LunarExploderShardGhost")
                }
            };

            #endregion
            List<SkinDef> skinDefs = skinController.skins.ToList();
            skinDefs.Insert(2, lunarSkin);

            skinController.skins = skinDefs.ToArray();

            SkinDef[][] skinsField = Reflection.GetFieldValue<SkinDef[][]>(typeof(BodyCatalog), "skins");
            skinsField[(int)BodyCatalog.FindBodyIndex(bodyPrefab)] = skinController.skins;
            BodyCatalog.skins = skinsField;

            // special vfx

            On.EntityStates.Toolbot.BaseNailgunState.FireBullet += BaseNailgunState_FireBullet;
            On.EntityStates.Toolbot.FireSpear.FireBullet += FireSpear_FireBullet;
            On.EntityStates.Toolbot.ToolbotDualWield.OnEnter += ToolbotDualWield_OnEnter;
        }

        private static void RegisterAcridSkins()
        {
            GameObject bodyPrefab = BodyCatalog.FindBodyPrefab("CrocoBody");
            ModelSkinController skinController = bodyPrefab.GetComponentInChildren<ModelSkinController>();
            GameObject model = skinController.gameObject;
            CharacterModel characterModel = model.GetComponent<CharacterModel>();
            SkinnedMeshRenderer mainRenderer = Reflection.GetFieldValue<SkinnedMeshRenderer>(characterModel, "mainSkinnedMeshRenderer");

            CharacterModel.RendererInfo[] defaultRenderers = characterModel.baseRendererInfos;

            //List<SkinDef> skinDefs = new List<SkinDef>();

            #region Armored Acrid
            CharacterModel.RendererInfo[] crocoRendererInfos = new CharacterModel.RendererInfo[defaultRenderers.Length];
            defaultRenderers.CopyTo(crocoRendererInfos, 0);

            //I leave it in your hands, Rob.
            //Godspeed.
            // :bombardier:


            Material crocoClassicMaterial = Modules.Assets.CreateMaterial("matAcridArmor", 1, Color.white, 0);

            crocoRendererInfos[0].defaultMaterial = crocoClassicMaterial;

            LanguageAPI.Add("ACRID_GRANDMASTERY_SKIN_NAME", "Armored");
            SkinDef armoredSkin = SkinsCore.CreateSkinDef("ACRID_GRANDMASTERY_SKIN_NAME",
                                                          Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("texAcridSkinGrandMaster"),
                                                          crocoRendererInfos,
                                                          mainRenderer,
                                                          model,
                                                          VanillaSurvivorUnlockables.acridGrandMastery);

            armoredSkin.meshReplacements = new SkinDef.MeshReplacement[]
            {
                new SkinDef.MeshReplacement
                {
                    mesh = Modules.Assets.mainAssetBundle.LoadAsset<Mesh>("meshAcridGM"),
                    renderer = armoredSkin.rendererInfos[0].renderer.GetComponent<SkinnedMeshRenderer>()
                }
            };
            #endregion
            List<SkinDef> skinDefs = skinController.skins.ToList();
            skinDefs.Insert(2, armoredSkin);

            skinController.skins = skinDefs.ToArray();

            SkinDef[][] skinsField = Reflection.GetFieldValue<SkinDef[][]>(typeof(BodyCatalog), "skins");
            skinsField[(int)BodyCatalog.FindBodyIndex(bodyPrefab)] = skinController.skins;
            BodyCatalog.skins = skinsField;
        }

        private static void ToolbotDualWield_OnEnter(On.EntityStates.Toolbot.ToolbotDualWield.orig_OnEnter orig, EntityStates.Toolbot.ToolbotDualWield self)
        {
            orig(self);

            if (self.GetModelTransform().GetComponentInChildren<ModelSkinController>().skins[self.characterBody.skinIndex].nameToken == "TOOLBOT_GRANDMASTERY_SKIN_NAME")
            {
                self.coverLeftInstance.GetComponentInChildren<SkinnedMeshRenderer>().material = Modules.Assets.matLunarGolem;
                self.coverRightInstance.GetComponentInChildren<SkinnedMeshRenderer>().material = Modules.Assets.matLunarGolem;
            }
        }

        private static void FireSpear_FireBullet(On.EntityStates.Toolbot.FireSpear.orig_FireBullet orig, EntityStates.Toolbot.FireSpear self, Ray aimRay)
        {
            bool isLunar = false;
            GameObject oldTracer = self.tracerEffectPrefab;

            if (self.GetModelTransform().GetComponentInChildren<ModelSkinController>().skins[self.characterBody.skinIndex].nameToken == "TOOLBOT_GRANDMASTERY_SKIN_NAME")
            {
                isLunar = true;
                self.tracerEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/Tracers/TracerHuntressSnipe");
            }

            orig(self, aimRay);

            if (isLunar)
            {
                self.tracerEffectPrefab = oldTracer;
            }
        }

        private static void BaseNailgunState_FireBullet(On.EntityStates.Toolbot.BaseNailgunState.orig_FireBullet orig, EntityStates.Toolbot.BaseNailgunState self, Ray aimRay, int bulletCount, float spreadPitchScale, float spreadYawScale)
        {
            bool isLunar = false;
            GameObject oldTracer = EntityStates.Toolbot.BaseNailgunState.tracerEffectPrefab;

            if (self.GetModelTransform().GetComponentInChildren<ModelSkinController>().skins[self.characterBody.skinIndex].nameToken == "TOOLBOT_GRANDMASTERY_SKIN_NAME")
            {
                isLunar = true;
                EntityStates.Toolbot.BaseNailgunState.tracerEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/Tracers/TracerLunarWispMinigun");
            }

            orig(self, aimRay, bulletCount, spreadPitchScale, spreadYawScale);

            if (isLunar)
            {
                EntityStates.Toolbot.BaseNailgunState.tracerEffectPrefab = oldTracer;
            }
        }
    }
}
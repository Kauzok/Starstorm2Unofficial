﻿using RoR2;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace EntityStates.SS2UStates.Nucleator.Primary
{
    public class FireIrradiateOvercharge : BaseState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = baseDuration;// this.attackSpeedStat;
            this.damageStat *= this.attackSpeedStat;

            Ray aimRay = base.GetAimRay();
            base.StartAimMode(aimRay, 2f, false);

            //Util.PlaySound("SS2UNucleatorSkill1", base.gameObject);
            //Util.PlaySound("SS2UNucleatorSkill1c", base.gameObject);
            Util.PlaySound("SS2UCyborgUtility", base.gameObject);
            base.PlayAnimation("Gesture, Override", "PrimaryBig", "Primary.playbackRate", this.duration);

            if (muzzleflashEffectPrefab) EffectManager.SimpleMuzzleFlash(muzzleflashEffectPrefab, base.gameObject, "MuzzleR", false);

            if (base.isAuthority)
            {
                float chargeScaled = (charge - BaseChargeState.overchargeFraction) / (1f - BaseChargeState.overchargeFraction);

                float damageCoefficient = Mathf.Lerp(minDamageCoefficient, maxDamageCoefficient, chargeScaled);
                float force = Mathf.Lerp(minForce, maxForce, chargeScaled);
                ProjectileManager.instance.FireProjectile(projectilePrefab, aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction), base.gameObject, this.damageStat * damageCoefficient, force, base.RollCrit(), DamageColorIndex.Default, null, Mathf.Lerp(minProjectileSpeed, maxProjectileSpeed, chargeScaled));
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge >= this.duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        public static GameObject muzzleflashEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Croco/MuzzleflashCroco.prefab").WaitForCompletion();
        public static GameObject projectilePrefab;

        public static float minDamageCoefficient = 6.5f;
        public static float maxDamageCoefficient = 10f;

        public static float minForce = 3000f;
        public static float maxForce = 3000f;

        public static float minProjectileSpeed = 20f;
        public static float maxProjectileSpeed = 20f;

        public static float baseDuration = 0.4f;

        public float charge = 0f;
        private float duration;
    }
}

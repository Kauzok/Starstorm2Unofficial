﻿using RoR2;
using UnityEngine;
using RoR2.Projectile;
using UnityEngine.AddressableAssets;
using Starstorm2Unofficial.Survivors.Cyborg.Components;

namespace EntityStates.SS2UStates.Cyborg
{
    public class ShockCore : BaseState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = ShockCore.baseDuration / this.attackSpeedStat;

            Ray aimRay = base.GetAimRay();
            base.StartAimMode(aimRay, 2f, false);

            Util.PlaySound("SS2UCyborgUtility", base.gameObject);
            base.PlayAnimation("Gesture, Override", "FireSpecial", "FireArrow.playbackRate", this.duration);

            EffectManager.SimpleMuzzleFlash(muzzleflashEffectPrefab, base.gameObject, "Lowerarm.L_end", false);

            if (base.isAuthority)
            {
                ProjectileManager.instance.FireProjectile(GetProjectilePrefab(), aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction), base.gameObject, this.damageStat * GetDamageCoefficient(), 0f, base.RollCrit(), DamageColorIndex.Default, null, -1f);
            }
            ApplySelfKnockback();

            energyComponent = base.GetComponent<CyborgEnergyComponent>();
            if (energyComponent)
            {
                energyComponent.ConsumeEnergy(0.3f);
                energyComponent.energySkillsActive++;
            }
        }

        public virtual float GetDamageCoefficient() { return ShockCore.damageCoefficient;  }
        public virtual GameObject GetProjectilePrefab() { return ShockCore.projectilePrefab; }

        public void ApplySelfKnockback()
        {
            if (base.isAuthority && base.characterBody && base.characterMotor && !base.characterMotor.isGrounded)
            {
                EntityStateMachine jetMachine = EntityStateMachine.FindByCustomName(base.gameObject, "Jetpack");
                if (!(jetMachine && jetMachine.state.GetType() == typeof(EntityStates.Idle)))
                {
                    return;
                }

                if (base.characterMotor.velocity.y < 0f) base.characterMotor.velocity.y = 0f;
                base.characterMotor.ApplyForce(-2400f * base.GetAimRay().direction, true, false);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.isAuthority)
            {
                if (!buttonReleased && base.inputBank)
                {
                    if (!base.inputBank.skill2.down) buttonReleased = true;
                }
                if (base.fixedAge >= this.duration)
                {
                    this.outer.SetNextStateToMain();
                    return;
                }
            }
        }

        public override void OnExit()
        {
            if (energyComponent)
            {
                energyComponent.energySkillsActive--;
            }
            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return buttonReleased ? InterruptPriority.Any : InterruptPriority.PrioritySkill;
        }

        public static GameObject muzzleflashEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Captain/CaptainChargeTazer.prefab").WaitForCompletion();
        public static GameObject projectilePrefab;
        public static float damageCoefficient = 4f;
        public static float baseDuration = 0.5f;

        private float duration;
        private bool buttonReleased = false;
        private CyborgEnergyComponent energyComponent;
    }
}

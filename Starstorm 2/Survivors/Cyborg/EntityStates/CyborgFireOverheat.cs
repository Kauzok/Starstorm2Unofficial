﻿using RoR2;
using RoR2.Projectile;
using UnityEngine;
using Starstorm2Unofficial.Cores;
using Starstorm2Unofficial.Survivors.Cyborg.Components;

namespace EntityStates.SS2UStates.Cyborg
{
    public class CyborgFireOverheat : BaseSkillState
    {
        public static float damageCoefficient = 12f;
        public static float baseDuration = 0.5f;
        public static float recoil = 1f;
        public static GameObject projectilePrefab;

        private float duration;
        private bool hasFired;
        private Animator animator;
        private string muzzleString;

        public float damageCoefficientInternal;
        public GameObject projectilePrefabInternal;
        private CyborgEnergyComponent energyComponent;

        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = CyborgFireOverheat.baseDuration / this.attackSpeedStat;
            base.characterBody.SetAimTimer(2f);
            this.animator = base.GetModelAnimator();
            this.muzzleString = "Muzzle";

            Util.PlaySound("SS2UCyborgUtility", base.gameObject);
            base.PlayAnimation("Gesture, Override", "FireSpecial", "FireArrow.playbackRate", this.duration);

            damageCoefficientInternal = CyborgFireOverheat.damageCoefficient;
            projectilePrefabInternal = CyborgFireOverheat.projectilePrefab;
            OverrideStats();

            FireBFG();
            ApplySelfKnockback();

            energyComponent = base.GetComponent<CyborgEnergyComponent>();
            if (energyComponent)
            {
                energyComponent.ConsumeEnergy(0.6f);
                energyComponent.energySkillsActive++;
            }
        }

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

        public virtual void OverrideStats() { }

        public override void OnExit()
        {
            if (energyComponent)
            {
                energyComponent.energySkillsActive--;
            }
            base.OnExit();
        }

        private void FireBFG()
        {
            if (!this.hasFired)
            {
                this.hasFired = true;

                base.characterBody.AddSpreadBloom(0.75f);
                Ray aimRay = base.GetAimRay();
                EffectManager.SimpleMuzzleFlash(Commando.CommandoWeapon.FirePistol2.muzzleEffectPrefab, base.gameObject, this.muzzleString, false);

                if (base.isAuthority)
                {
                    ProjectileManager.instance.FireProjectile(projectilePrefabInternal,
                        aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction),
                        base.gameObject, base.damageStat * damageCoefficientInternal,
                        0f,
                        Util.CheckRoll(this.characterBody.crit,
                        this.characterBody.master),
                        DamageColorIndex.Default, null, -1f);
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.fixedAge >= this.duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
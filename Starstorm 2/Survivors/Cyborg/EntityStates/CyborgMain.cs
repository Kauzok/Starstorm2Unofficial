﻿using RoR2;
using Starstorm2.Survivors.Cyborg.Components;
using UnityEngine;

namespace EntityStates.Starstorm2States.Cyborg
{
    public class CyborgMain : GenericCharacterMain
    {
        private float hoverVelocity = -1f;    //was -1.1
        private float hoverAcceleration = 60f;  //was 25f
        private CyborgController cyborgController;

        private Transform thrusterEffectL;
        private Transform thrusterEffectR;
        private bool inJetpackState = false;

        private EntityStateMachine jetpackStateMachine;

        public override void OnEnter()
        {
            base.OnEnter();
            jetpackStateMachine = EntityStateMachine.FindByCustomName(base.gameObject, "Jetpack");
            cyborgController = base.GetComponent<CyborgController>();

            ChildLocator cl = base.GetModelChildLocator();
            if (cl)
            {
                thrusterEffectL = cl.FindChild("ThrusterEffectL");
                if (thrusterEffectL)
                {
                    thrusterEffectL.gameObject.SetActive(false);
                }

                thrusterEffectR = cl.FindChild("ThrusterEffectR");
                if (thrusterEffectR)
                {
                    thrusterEffectR.gameObject.SetActive(false);
                }
            }
        }

        public override void ProcessJump()
        {
            base.ProcessJump();
            inJetpackState = this.jetpackStateMachine.state.GetType() == typeof(JetpackOn);
            if (this.hasCharacterMotor && this.hasInputBank && base.isAuthority)
            {
                bool inputPressed = base.inputBank.jump.down && base.characterMotor.velocity.y < 0f && !base.characterMotor.isGrounded;
                if (inputPressed && !inJetpackState && cyborgController.allowJetpack)
                {
                    this.jetpackStateMachine.SetNextState(new JetpackOn());
                }
                if (inJetpackState &&(!inputPressed || !cyborgController.allowJetpack))
                {
                    this.jetpackStateMachine.SetNextState(new Idle());
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            inJetpackState = this.jetpackStateMachine.state.GetType() == typeof(JetpackOn);
            bool shouldShowThruster = (inJetpackState || (base.characterBody && base.characterBody.isSprinting));
            if (shouldShowThruster)
            {
                if (thrusterEffectL)
                {
                    thrusterEffectL.gameObject.SetActive(true);
                }

                if (thrusterEffectR)
                {
                    thrusterEffectR.gameObject.SetActive(true);
                }
            }
            else
            {
                if (thrusterEffectL)
                {
                    thrusterEffectL.gameObject.SetActive(false);
                }

                if (thrusterEffectR)
                {
                    thrusterEffectR.gameObject.SetActive(false);
                }
            }

            // rest idle!!
            //if (this.animator) this.animator.SetBool("inCombat", (!base.characterBody.outOfCombat || !base.characterBody.outOfDanger));
        }

        public override void Update()
        {
            base.Update();

            /*if (base.isAuthority && base.characterMotor.isGrounded)
            {
                if (Input.GetKeyDown(Starstorm.restKeybind))
                {
                    this.outer.SetInterruptState(EntityState.Instantiate(new SerializableEntityStateType(typeof(Emotes.RestEmote))), InterruptPriority.Any);
                    return;
                }
                else if (Input.GetKeyDown(Starstorm.tauntKeybind))
                {
                    this.outer.SetInterruptState(EntityState.Instantiate(new SerializableEntityStateType(typeof(Emotes.TauntEmote))), InterruptPriority.Any);
                    return;
                }
            }*/
            // I ll need those eventually
        }
    }
}
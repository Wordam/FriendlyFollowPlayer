using System;
using System.Reflection;
using RoR2;
using RoR2.CharacterAI;
using RoR2.Navigation;
using UnityEngine;

namespace EntityStates.AI.Walker
{
    // Token: 0x020001E6 RID: 486
    public class Rush : BaseAIState
    {
        // Token: 0x0600097B RID: 2427 RVA: 0x0002F465 File Offset: 0x0002D665
        public override void OnEnter()
        {
            base.OnEnter();
            this.pathUpdateTimer = 0f;
        }

        // Token: 0x0600097C RID: 2428 RVA: 0x0002F48D File Offset: 0x0002D68D
        public override void OnExit()
        {
            base.OnExit();
        }

        // Token: 0x0600097D RID: 2429 RVA: 0x0002F498 File Offset: 0x0002D698
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.ai && base.body)
            {
                if(base.body.teamComponent.teamIndex != TeamIndex.Player)
                {
                    outer.SetNextState(new Wander());
                    return;
                }

                NetworkUser n = NetworkUser.readOnlyInstancesList[0];
                if (!n.master)
                {
                    outer.SetNextState(new Wander());
                    return;
                }
                CharacterBody cb = n.master.GetBody();
                if(!cb)
                {
                    outer.SetNextState(new Wander());
                    return;
                }

                Vector3 footPosition = base.body.footPosition;
                Vector3 targetPosition = cb.footPosition;
                base.ai.pathFollower.UpdatePosition(footPosition);
                this.updateTimer -= Time.fixedDeltaTime;
                this.pathUpdateTimer -= Time.fixedDeltaTime;

                // If the distance is greater than a threshold
                if(Vector3.Distance(footPosition, targetPosition) <= this.max_distance)
                {
                    outer.SetNextState(new Wander());
                    return;
                }
                if (this.updateTimer <= 0f)
                {
                    this.updateTimer = UnityEngine.Random.Range(0.16666667f, 0.2f);
                    Vector3 position = base.body.transform.position;
                    
                    if (this.pathUpdateTimer <= 0f)
                    {
                        base.ai.RefreshPath(footPosition, targetPosition);
                        this.pathUpdateTimer = this.pathUpdateInterval;
                    }

                    base.ai.localNavigator.targetPosition = position + (base.ai.pathFollower.GetNextPosition() - footPosition).normalized;
                    base.ai.localNavigator.Update(Time.fixedDeltaTime);
                    Vector3 vector5 = base.ai.localNavigator.moveVector;
                    vector5 *= this.scale_move;

                    if (base.bodyInputBank)
                    {
                        bool newState5 = true;
                        if (base.bodyCharacterMotor && base.bodyCharacterMotor.isGrounded && Mathf.Max(base.ai.pathFollower.CalculateJumpVelocityNeededToReachNextWaypoint(base.body.moveSpeed), base.ai.localNavigator.jumpSpeed) > 0f)
                        {
                            newState5 = true;
                        }
                        base.bodyInputBank.jump.PushState(newState5);
                        base.bodyInputBank.moveVector = vector5;
                    }
                }
            }
        }

        public Rush()
        {
        }

        public float max_distance = 20f;
        private float scale_move = 1.5f;
        private float pathUpdateTimer;
        private float pathUpdateInterval = 0.5f;
        private float updateTimer;
    }
}


using System;
using System.Reflection;
using BepInEx;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using EntityStates.AI;
using RoR2.CharacterAI;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace FriendlyFollowPlayer
{

    class Utils
    {
        public static bool isPlayerTooFar(CharacterBody body, float max_distance)
        {
            if (!body)
            {
                return false;
            }

            NetworkUser n = NetworkUser.readOnlyInstancesList[0];
            if (!n.master)
            {
                return false;
            }
            CharacterBody cb = n.master.GetBody();
            if (!cb)
            {
                return false;
            }

            Vector3 footPosition = body.footPosition;
            Vector3 targetPosition = cb.footPosition;

            // If the distance is greater than a threshold
            if (Vector3.Distance(footPosition, targetPosition) > max_distance)
            {
                return true;
            }
            return false;
        }

        public static bool isAIPlayerTeam(CharacterBody body)
        {
            if (body)
            {
                if (body.teamComponent)
                {
                    return body.teamComponent.teamIndex == TeamIndex.Player;
                }
            }
            return false;
        }
    }


    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("dev.wordam.ror2.friendlyfollowplayer", "FriendlyFollowPlayer", "1.0.0")]
    public class FriendlyFollowPlayer : BaseUnityPlugin
    {
        private static PropertyInfo wbody = typeof(EntityStates.AI.Walker.Wander).GetProperty("body", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        private static PropertyInfo lbody = typeof(EntityStates.AI.Walker.LookBusy).GetProperty("body", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        private static PropertyInfo cbody = typeof(EntityStates.AI.Walker.Combat).GetProperty("body", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        private float max_distance_from_player = 30.0f;

        private void Awake()
        {
            // Add a shortcut to every state to get to player when the AI is "too" far

            IL.EntityStates.AI.Walker.Wander.FixedUpdate += (il1) =>
            {
                var c2 = new ILCursor(il1);
                c2.Emit(OpCodes.Ldarg_0);

                c2.EmitDelegate<Action<EntityStates.AI.Walker.Wander>>((w) =>
                {
                    if(w.outer)
                    {
                        CharacterBody b = ((CharacterBody)wbody.GetValue(w));
                        if (b)
                        {
                            if (Utils.isAIPlayerTeam(b) && Utils.isPlayerTooFar(b, max_distance_from_player))
                            {
                                w.outer.SetNextState(new EntityStates.AI.Walker.Rush());
                                return;
                            }
                        }
                    }
                });
            };

            IL.EntityStates.AI.Walker.LookBusy.FixedUpdate += (il1) =>
            {
                var c2 = new ILCursor(il1);
                c2.Emit(OpCodes.Ldarg_0);

                c2.EmitDelegate<Action<EntityStates.AI.Walker.LookBusy>>((w) =>
                {
                    if (w.outer)
                    {
                        CharacterBody b = ((CharacterBody)wbody.GetValue(w));
                        if (b)
                        {
                            if (Utils.isAIPlayerTeam(b) && Utils.isPlayerTooFar(b, max_distance_from_player))
                            {
                                w.outer.SetNextState(new EntityStates.AI.Walker.Rush());
                                return;
                            }
                        }
                    }
                });
            };

            IL.EntityStates.AI.Walker.Combat.FixedUpdate += (il1) =>
            {
                var c2 = new ILCursor(il1);
                c2.Emit(OpCodes.Ldarg_0);

                c2.EmitDelegate<Action<EntityStates.AI.Walker.Combat>>((w) =>
                {
                    if (w.outer)
                    {
                        CharacterBody b = ((CharacterBody)cbody.GetValue(w));
                        if (b)
                        {
                            if (Utils.isAIPlayerTeam(b) && Utils.isPlayerTooFar(b, max_distance_from_player))
                            {
                                w.outer.SetNextState(new EntityStates.AI.Walker.Rush());
                                return;
                            }
                        }
                    }
                });
            };
        }
    }
}
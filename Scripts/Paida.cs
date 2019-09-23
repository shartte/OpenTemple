
using System;
using System.Collections.Generic;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.Dialog;
using SpicyTemple.Core.Systems.Feats;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.Script;
using SpicyTemple.Core.Systems.Spells;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Systems.D20.Conditions;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Systems.ObjScript;
using SpicyTemple.Core.Ui;
using System.Linq;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace Scripts
{
    [ObjectScript(143)]
    public class Paida : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetLeader() != null))
            {
                triggerer.BeginDialog(attachee, 110); // paida in party
            }
            else if ((GetGlobalVar(902) == 32 && attachee.GetMap() != 5044))
            {
                triggerer.BeginDialog(attachee, 240); // have attacked 3 or more farm animals with paida in party and paida not at home
            }
            else if ((GetGlobalFlag(148)) && (!GetGlobalFlag(932)))
            {
                triggerer.BeginDialog(attachee, 70); // paida dispelled and has not talked to you yet since
            }
            else if ((!GetGlobalFlag(148)))
            {
                if ((GetGlobalFlag(325)))
                {
                    triggerer.BeginDialog(attachee, 230); // paida not dispelled and hedrack killed in front of her
                }
                else
                {
                    triggerer.BeginDialog(attachee, 1); // paida not dispelled
                }

            }
            else if ((GetGlobalFlag(38)))
            {
                triggerer.BeginDialog(attachee, 170); // paida has been returned to valden
            }
            else
            {
                triggerer.BeginDialog(attachee, 160); // none of the above - will fire angry dialog like you left her in temple
            }

            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (((GetGlobalFlag(149)) || (GetGlobalFlag(38))))
            {
                if ((attachee.GetMap() == 5044))
                {
                    if ((GetGlobalVar(501) == 4 || GetGlobalVar(501) == 5 || GetGlobalVar(501) == 6 || GetGlobalVar(510) == 2))
                    {
                        attachee.SetObjectFlag(ObjectFlag.OFF);
                    }
                    else
                    {
                        attachee.ClearObjectFlag(ObjectFlag.OFF);
                    }

                }

                if ((attachee.GetMap() == 5080))
                {
                    attachee.SetObjectFlag(ObjectFlag.OFF);
                }

            }
            else if ((attachee.GetMap() == 5044 && attachee.GetLeader() == null))
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
            }

            return RunDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            SetGlobalFlag(153, true);
            if ((!GetGlobalFlag(238)))
            {
                attachee.FloatLine(12014, triggerer);
                SetGlobalVar(23, GetGlobalVar(23) + 1);
                if (GetGlobalVar(23) >= 2)
                {
                    PartyLeader.AddReputation(92);
                }

            }
            else
            {
                SetGlobalVar(29, GetGlobalVar(29) + 1);
                attachee.FloatLine(12014, triggerer);
            }

            return RunDefault;
        }
        public override bool OnEnterCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            CombatStandardRoutines.ProtectTheInnocent(attachee, triggerer);
            attachee.FloatLine(12057, triggerer);
            return RunDefault;
        }
        public override bool OnResurrect(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(153, false);
            return RunDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((!GameSystems.Combat.IsCombatActive()))
            {
                Co8.StopCombat(attachee, 1);
                if ((GetQuestState(20) == QuestState.Completed))
                {
                    foreach (var pc in GameSystems.Party.PartyMembers)
                    {
                        if (pc.HasFollowerByName(8001))
                        {
                            pc.RemoveFollower(attachee);
                            DetachScript();
                        }

                    }

                }
                else if ((GetGlobalVar(902) >= 3))
                {
                    if ((attachee != null))
                    {
                        var leader = attachee.GetLeader();
                        if ((leader != null))
                        {
                            leader.RemoveFollower(attachee);
                            attachee.FloatLine(22000, triggerer);
                            if ((attachee.GetMap() == 5001))
                            {
                                SetQuestState(20, QuestState.Completed);
                                SetGlobalFlag(38, true);
                            }

                        }

                    }

                }

            }

            return RunDefault;
        }
        public override bool OnJoin(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(238, true);
            return RunDefault;
        }
        public override bool OnDisband(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(238, false);
            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                attachee.AIRemoveFromShitlist(pc);
                attachee.SetReaction(pc, 50);
            }

            return RunDefault;
        }
        public override bool OnSpellCast(GameObjectBody attachee, GameObjectBody triggerer, SpellPacketBody spell)
        {
            if ((spell.spellEnum == WellKnownSpells.DispelMagic || spell.spellEnum == WellKnownSpells.BreakEnchantment || spell.spellEnum == WellKnownSpells.DispelEvil))
            {
                SetGlobalFlag(148, true);
                triggerer.BeginDialog(attachee, 70);
                foreach (var pc in GameSystems.Party.PartyMembers)
                {
                    attachee.AIRemoveFromShitlist(pc);
                }

            }

            return RunDefault;
        }
        public static bool run_off(GameObjectBody attachee, GameObjectBody triggerer)
        {
            // attachee.standpoint_set( STANDPOINT_NIGHT, 257 )
            // attachee.standpoint_set( STANDPOINT_DAY, 257 )
            attachee.RunOff(attachee.GetLocation() - 6);
            return RunDefault;
        }
        public static bool LookHedrack(GameObjectBody attachee, GameObjectBody triggerer, int line)
        {
            var npc = Utilities.find_npc_near(attachee, 8032);
            if ((npc != null) && (!GetGlobalFlag(146)))
            {
                triggerer.BeginDialog(npc, line);
                npc.TurnTowards(attachee);
                attachee.TurnTowards(npc);
            }
            else
            {
                triggerer.BeginDialog(attachee, 10);
            }

            return SkipDefault;
        }
        public static bool get_rep(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (!triggerer.HasReputation(7))
            {
                triggerer.AddReputation(7);
            }

            SetGlobalVar(25, GetGlobalVar(25) + 1);
            if ((GetGlobalVar(25) >= 3 && !triggerer.HasReputation(8)))
            {
                triggerer.AddReputation(8);
            }

            return RunDefault;
        }

    }
}


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
    [ObjectScript(321)]
    public class CaptainAbiram : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.TurnTowards(triggerer);
            if ((GetGlobalVar(979) == 2) && (GetGlobalFlag(980)))
            {
                triggerer.BeginDialog(attachee, 120);
            }
            else if ((GetGlobalVar(979) == 1) && (Utilities.is_daytime()))
            {
                triggerer.BeginDialog(attachee, 50);
            }
            else if ((GetGlobalVar(979) == 1) && (!Utilities.is_daytime()))
            {
                triggerer.BeginDialog(attachee, 60);
            }
            else if ((attachee.HasMet(triggerer)))
            {
                triggerer.BeginDialog(attachee, 70);
            }
            else
            {
                triggerer.BeginDialog(attachee, 1);
            }

            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetMap() == 5170 || attachee.GetMap() == 5135))
            {
                if ((GetGlobalVar(948) == 1))
                {
                    attachee.ClearObjectFlag(ObjectFlag.OFF);
                }
                else if ((GetGlobalVar(948) == 2))
                {
                    attachee.SetObjectFlag(ObjectFlag.OFF);
                }

            }
            else if ((attachee.GetMap() == 5169))
            {
                if ((GetGlobalVar(948) == 2) && ScriptDaemon.tpsts("abiram_off_to_arrest", 1 * 60 * 60) == 0)
                {
                    attachee.SetObjectFlag(ObjectFlag.OFF);
                }
                else if ((GetGlobalVar(948) == 3) || ScriptDaemon.tpsts("abiram_off_to_arrest", 1 * 60 * 60))
                {
                    attachee.ClearObjectFlag(ObjectFlag.OFF);
                }

            }

            return RunDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(550, true);
            return RunDefault;
        }
        public override bool OnResurrect(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(550, false);
            return RunDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (((attachee.GetMap() == 5170 || attachee.GetMap() == 5135) && GetGlobalVar(948) == 1))
            {
                if ((!GameSystems.Combat.IsCombatActive()))
                {
                    foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                    {
                        if ((is_groovier_to_talk(attachee, obj)))
                        {
                            StartTimer(2000, () => start_talking(attachee, triggerer));
                            DetachScript();
                        }

                    }

                }

            }
            else
            {
                if ((PartyLeader.HasReputation(34) || PartyLeader.HasReputation(35) || PartyLeader.HasReputation(42) || PartyLeader.HasReputation(44) || PartyLeader.HasReputation(35) || PartyLeader.HasReputation(43) || PartyLeader.HasReputation(46) || (GetGlobalVar(993) == 5 && !GetGlobalFlag(870))))
                {
                    if (((GetGlobalVar(969) == 0) && (!GetGlobalFlag(955))))
                    {
                        if ((!GameSystems.Combat.IsCombatActive()))
                        {
                            foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                            {
                                if ((is_better_to_talk(attachee, obj)))
                                {
                                    attachee.TurnTowards(obj);
                                    obj.BeginDialog(attachee, 140);
                                    SetGlobalVar(969, 1);
                                }

                            }

                        }

                    }

                }

            }

            return RunDefault;
        }
        public static bool is_better_to_talk(GameObjectBody speaker, GameObjectBody listener)
        {
            if ((speaker.DistanceTo(listener) <= 50))
            {
                return true;
            }

            return false;
        }
        public static bool is_groovier_to_talk(GameObjectBody speaker, GameObjectBody listener)
        {
            if ((speaker.HasLineOfSight(listener)))
            {
                if ((speaker.DistanceTo(listener) <= 40))
                {
                    return true;
                }

            }

            return false;
        }
        public static bool start_talking(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var npc = Utilities.find_npc_near(attachee, 8703);
            attachee.TurnTowards(npc);
            PartyLeader.BeginDialog(attachee, 270);
            return RunDefault;
        }
        public static bool switch_to_wilfrick(GameObjectBody attachee, GameObjectBody triggerer, int line)
        {
            var npc = Utilities.find_npc_near(attachee, 8703);
            if ((npc != null))
            {
                triggerer.BeginDialog(npc, line);
                npc.TurnTowards(triggerer);
            }

            return SkipDefault;
        }
        public static bool run_off(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.RunOff();
            return RunDefault;
        }
        public static bool remove_panathaes(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8791)))
            {
                foreach (var npc in PartyLeader.GetPartyMembers())
                {
                    if ((npc.GetNameId() == 8791 && npc.GetLeader() != null))
                    {
                        triggerer.RemoveFollower(npc);
                        foreach (var pc in GameSystems.Party.PartyMembers)
                        {
                            npc.AIRemoveFromShitlist(pc);
                            npc.SetReaction(pc, 50);
                        }

                    }

                }

            }
            else if (triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8792)))
            {
                foreach (var npc in PartyLeader.GetPartyMembers())
                {
                    if ((npc.GetNameId() == 8792 && npc.GetLeader() != null))
                    {
                        triggerer.RemoveFollower(npc);
                        foreach (var pc in GameSystems.Party.PartyMembers)
                        {
                            npc.AIRemoveFromShitlist(pc);
                            npc.SetReaction(pc, 50);
                        }

                    }

                }

            }
            else if (triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8793)))
            {
                foreach (var npc in PartyLeader.GetPartyMembers())
                {
                    if ((npc.GetNameId() == 8793 && npc.GetLeader() != null))
                    {
                        triggerer.RemoveFollower(npc);
                        foreach (var pc in GameSystems.Party.PartyMembers)
                        {
                            npc.AIRemoveFromShitlist(pc);
                            npc.SetReaction(pc, 50);
                        }

                    }

                }

            }
            else if (triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8794)))
            {
                foreach (var npc in PartyLeader.GetPartyMembers())
                {
                    if ((npc.GetNameId() == 8794 && npc.GetLeader() != null))
                    {
                        triggerer.RemoveFollower(npc);
                        foreach (var pc in GameSystems.Party.PartyMembers)
                        {
                            npc.AIRemoveFromShitlist(pc);
                            npc.SetReaction(pc, 50);
                        }

                    }

                }

            }
            else if (triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8795)))
            {
                foreach (var npc in PartyLeader.GetPartyMembers())
                {
                    if ((npc.GetNameId() == 8795 && npc.GetLeader() != null))
                    {
                        triggerer.RemoveFollower(npc);
                        foreach (var pc in GameSystems.Party.PartyMembers)
                        {
                            npc.AIRemoveFromShitlist(pc);
                            npc.SetReaction(pc, 50);
                        }

                    }

                }

            }
            else if (triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8796)))
            {
                foreach (var npc in PartyLeader.GetPartyMembers())
                {
                    if ((npc.GetNameId() == 8796 && npc.GetLeader() != null))
                    {
                        triggerer.RemoveFollower(npc);
                        foreach (var pc in GameSystems.Party.PartyMembers)
                        {
                            npc.AIRemoveFromShitlist(pc);
                            npc.SetReaction(pc, 50);
                        }

                    }

                }

            }
            else if (triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8797)))
            {
                foreach (var npc in PartyLeader.GetPartyMembers())
                {
                    if ((npc.GetNameId() == 8797 && npc.GetLeader() != null))
                    {
                        triggerer.RemoveFollower(npc);
                        foreach (var pc in GameSystems.Party.PartyMembers)
                        {
                            npc.AIRemoveFromShitlist(pc);
                            npc.SetReaction(pc, 50);
                        }

                    }

                }

            }
            else if (triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8798)))
            {
                foreach (var npc in PartyLeader.GetPartyMembers())
                {
                    if ((npc.GetNameId() == 8798 && npc.GetLeader() != null))
                    {
                        triggerer.RemoveFollower(npc);
                        foreach (var pc in GameSystems.Party.PartyMembers)
                        {
                            npc.AIRemoveFromShitlist(pc);
                            npc.SetReaction(pc, 50);
                        }

                    }

                }

            }

            SetGlobalVar(551, 2);
            // game.global_vars[549] = 2
            return RunDefault;
        }
        public static bool remove_rakham(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8766)))
            {
                foreach (var npc in PartyLeader.GetPartyMembers())
                {
                    if ((npc.GetNameId() == 8766 && npc.GetLeader() != null))
                    {
                        triggerer.RemoveFollower(npc);
                        foreach (var pc in GameSystems.Party.PartyMembers)
                        {
                            npc.AIRemoveFromShitlist(pc);
                            npc.SetReaction(pc, 50);
                        }

                    }

                }

            }

            SetGlobalVar(549, 3);
            return RunDefault;
        }
        public static bool remove_boroquin(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8767)))
            {
                foreach (var npc in PartyLeader.GetPartyMembers())
                {
                    if ((npc.GetNameId() == 8767 && npc.GetLeader() != null))
                    {
                        triggerer.RemoveFollower(npc);
                        foreach (var pc in GameSystems.Party.PartyMembers)
                        {
                            npc.AIRemoveFromShitlist(pc);
                            npc.SetReaction(pc, 50);
                        }

                    }

                }

            }

            SetGlobalVar(549, 1);
            return RunDefault;
        }
        public static void rep_routine(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((PartyLeader.HasReputation(75)))
            {
                PartyLeader.AddReputation(78);
                PartyLeader.RemoveReputation(75);
            }
            else if ((PartyLeader.HasReputation(76)))
            {
                PartyLeader.AddReputation(78);
                PartyLeader.RemoveReputation(76);
            }
            else if ((PartyLeader.HasReputation(77)))
            {
                PartyLeader.AddReputation(78);
                PartyLeader.RemoveReputation(77);
            }

            return;
        }

    }
}

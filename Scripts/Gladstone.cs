
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
    [ObjectScript(353)]
    public class Gladstone : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetLeader() != null))
            {
                triggerer.BeginDialog(attachee, 260);
            }
            else if ((GetGlobalFlag(957)) || (GetGlobalFlag(958)))
            {
                triggerer.BeginDialog(attachee, 210);
            }
            else
            {
                triggerer.BeginDialog(attachee, 1);
            }

            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetQuestState(79) == QuestState.Accepted && attachee.GetMap() == 5152))
            {
                attachee.ClearObjectFlag(ObjectFlag.OFF);
            }
            else if ((GetGlobalVar(951) == 1 && attachee.GetMap() == 5172))
            {
                var obj = attachee.GetLeader();
                if ((obj != null))
                {
                    obj.RemoveFollower(attachee);
                    SetGlobalVar(951, 2);
                }

            }
            else if ((GetGlobalVar(951) == 3 && attachee.GetMap() == 5172))
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
                SetGlobalVar(951, 4);
            }

            return RunDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            SetGlobalFlag(956, true);
            if ((attachee.GetMap() == 5152))
            {
                SetGlobalVar(961, 1);
            }

            return RunDefault;
        }
        public override bool OnResurrect(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(956, false);
            return RunDefault;
        }
        public override bool OnEnterCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((!attachee.HasEquippedByName(4161) || !attachee.HasEquippedByName(4126)))
            {
                attachee.WieldBestInAllSlots();
            }

            if ((attachee.GetMap() == 5057 || attachee.GetMap() == 5152 || attachee.GetMap() == 5008))
            {
                var kendrew = Utilities.find_npc_near(attachee, 8717);
                if ((kendrew != null))
                {
                    var leader = kendrew.GetLeader();
                    if ((leader != null))
                    {
                        leader.RemoveFollower(kendrew);
                    }

                    kendrew.Attack(triggerer);
                }

                var quintus = Utilities.find_npc_near(attachee, 8718);
                if ((quintus != null))
                {
                    var leader = quintus.GetLeader();
                    if ((leader != null))
                    {
                        leader.RemoveFollower(quintus);
                    }

                    quintus.Attack(triggerer);
                }

            }

            return RunDefault;
        }
        public override bool OnStartCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            while ((attachee.FindItemByName(8903) != null))
            {
                attachee.FindItemByName(8903).Destroy();
            }

            // if (attachee.d20_query(Q_Is_BreakFree_Possible)): # workaround no longer necessary!
            // create_item_in_inventory( 8903, attachee )
            if ((!attachee.HasEquippedByName(4161) || !attachee.HasEquippedByName(4126)))
            {
                attachee.WieldBestInAllSlots();
            }

            return RunDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((!attachee.HasEquippedByName(4161) || !attachee.HasEquippedByName(4126)))
            {
                attachee.WieldBestInAllSlots();
                DetachScript();
            }

            return RunDefault;
        }
        public override bool OnJoin(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var boots = attachee.FindItemByName(6202);
            boots.SetItemFlag(ItemFlag.NO_TRANSFER);
            var gloves = attachee.FindItemByName(6046);
            gloves.SetItemFlag(ItemFlag.NO_TRANSFER);
            var coat = attachee.FindItemByName(6343);
            coat.SetItemFlag(ItemFlag.NO_TRANSFER);
            var armor = attachee.FindItemByName(6306);
            armor.SetItemFlag(ItemFlag.NO_TRANSFER);
            var sword2 = attachee.FindItemByName(4161);
            sword2.SetItemFlag(ItemFlag.NO_TRANSFER);
            var sword1 = attachee.FindItemByName(4126);
            sword1.SetItemFlag(ItemFlag.NO_TRANSFER);
            return RunDefault;
        }
        public override bool OnDisband(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var boots = attachee.FindItemByName(6202);
            boots.ClearItemFlag(ItemFlag.NO_TRANSFER);
            var gloves = attachee.FindItemByName(6046);
            gloves.ClearItemFlag(ItemFlag.NO_TRANSFER);
            var coat = attachee.FindItemByName(6343);
            coat.ClearItemFlag(ItemFlag.NO_TRANSFER);
            var armor = attachee.FindItemByName(6306);
            armor.ClearItemFlag(ItemFlag.NO_TRANSFER);
            var sword2 = attachee.FindItemByName(4161);
            sword2.ClearItemFlag(ItemFlag.NO_TRANSFER);
            var sword1 = attachee.FindItemByName(4126);
            sword1.ClearItemFlag(ItemFlag.NO_TRANSFER);
            return RunDefault;
        }
        public override bool OnNewMap(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetLeader() != null))
            {
                if ((attachee.GetMap() == 5169) || (attachee.GetMap() == 5171))
                {
                    if ((!attachee.IsUnconscious()))
                    {
                        attachee.FloatLine(2000, triggerer);
                        attachee.RunOff();
                        StartTimer(5000, () => go_away(attachee));
                    }
                    else if ((attachee.IsUnconscious()))
                    {
                        SetGlobalVar(951, 1);
                    }

                }
                else if ((attachee.GetMap() == 5121))
                {
                    if ((GetGlobalVar(940) == 0))
                    {
                        SetGlobalVar(940, 1);
                        StartTimer(432000000, () => stopwatch_for_time_in_party(attachee)); // 5 days
                    }

                }
                else if ((GetGlobalVar(940) == 2))
                {
                    attachee.FloatLine(3000, triggerer);
                    attachee.RunOff();
                    StartTimer(5000, () => go_away(attachee));
                }

            }

            return RunDefault;
        }
        public static bool go_away(GameObjectBody attachee)
        {
            attachee.SetObjectFlag(ObjectFlag.OFF);
            return RunDefault;
        }
        public static bool stopwatch_for_time_in_party(GameObjectBody attachee)
        {
            SetGlobalVar(940, 2);
            return RunDefault;
        }

    }
}

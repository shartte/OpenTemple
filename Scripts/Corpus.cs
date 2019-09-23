
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
    [ObjectScript(355)]
    public class Corpus : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetLeader() != null))
            {
                triggerer.BeginDialog(attachee, 260);
            }
            else if ((GetGlobalFlag(956)) || (GetGlobalFlag(957)))
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
            if ((GetQuestState(81) == QuestState.Accepted && attachee.GetMap() == 5057))
            {
                attachee.ClearObjectFlag(ObjectFlag.OFF);
            }
            else if ((GetGlobalVar(953) == 1 && attachee.GetMap() == 5172))
            {
                var obj = attachee.GetLeader();
                if ((obj != null))
                {
                    obj.RemoveFollower(attachee);
                    SetGlobalVar(953, 2);
                }

            }
            else if ((GetGlobalVar(953) == 3 && attachee.GetMap() == 5172))
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
                SetGlobalVar(953, 4);
            }

            return RunDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            SetGlobalFlag(958, true);
            if ((attachee.GetMap() == 5057))
            {
                SetGlobalVar(961, 5);
            }

            return RunDefault;
        }
        public override bool OnResurrect(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(958, false);
            return RunDefault;
        }
        public override bool OnEnterCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
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

                var guntur = Utilities.find_npc_near(attachee, 8716);
                if ((guntur != null))
                {
                    var leader = guntur.GetLeader();
                    if ((leader != null))
                    {
                        leader.RemoveFollower(guntur);
                    }

                    guntur.Attack(triggerer);
                }

            }

            return RunDefault;
        }
        public override bool OnJoin(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var boots = attachee.FindItemByName(6040);
            boots.SetItemFlag(ItemFlag.NO_TRANSFER);
            var gloves = attachee.FindItemByName(6021);
            gloves.SetItemFlag(ItemFlag.NO_TRANSFER);
            var kilt = attachee.FindItemByName(6438);
            kilt.SetItemFlag(ItemFlag.NO_TRANSFER);
            var armor = attachee.FindItemByName(6229);
            armor.SetItemFlag(ItemFlag.NO_TRANSFER);
            var sword = attachee.FindItemByName(4443);
            sword.SetItemFlag(ItemFlag.NO_TRANSFER);
            return RunDefault;
        }
        public override bool OnDisband(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var boots = attachee.FindItemByName(6040);
            boots.ClearItemFlag(ItemFlag.NO_TRANSFER);
            var gloves = attachee.FindItemByName(6021);
            gloves.ClearItemFlag(ItemFlag.NO_TRANSFER);
            var kilt = attachee.FindItemByName(6438);
            kilt.ClearItemFlag(ItemFlag.NO_TRANSFER);
            var armor = attachee.FindItemByName(6229);
            armor.ClearItemFlag(ItemFlag.NO_TRANSFER);
            var sword = attachee.FindItemByName(4443);
            sword.ClearItemFlag(ItemFlag.NO_TRANSFER);
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
                        SetGlobalVar(953, 1);
                    }

                }
                else if ((attachee.GetMap() == 5051))
                {
                    if ((GetGlobalVar(942) == 0))
                    {
                        SetGlobalVar(942, 1);
                        StartTimer(432000000, () => stopwatch_for_time_in_party(attachee)); // 5 days
                    }

                }
                else if ((GetGlobalVar(942) == 2))
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
            SetGlobalVar(942, 2);
            return RunDefault;
        }

    }
}

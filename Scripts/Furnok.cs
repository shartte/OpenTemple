
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
    [ObjectScript(70)]
    public class Furnok : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((SelectedPartyLeader.HasReputation(32) || SelectedPartyLeader.HasReputation(30) || SelectedPartyLeader.HasReputation(29)))
            {
                attachee.FloatLine(11004, triggerer);
            }
            else if ((GetGlobalFlag(61)))
            {
                triggerer.BeginDialog(attachee, 500);
            }
            else if ((attachee.GetLeader() != null))
            {
                triggerer.BeginDialog(attachee, 300);
            }
            else if ((GetGlobalFlag(51)))
            {
                if ((GetQuestState(18) == QuestState.Completed))
                {
                    triggerer.BeginDialog(attachee, 220);
                }
                else
                {
                    triggerer.BeginDialog(attachee, 210);
                }

            }
            else
            {
                triggerer.BeginDialog(attachee, 1);
            }

            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetLeader() == null))
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

            return RunDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            SetGlobalFlag(58, true);
            attachee.FloatLine(12014, triggerer);
            if ((!GetGlobalFlag(235)))
            {
                SetGlobalVar(23, GetGlobalVar(23) + 1);
                if ((GetGlobalVar(23) >= 2))
                {
                    PartyLeader.AddReputation(92);
                }

            }
            else
            {
                SetGlobalVar(29, GetGlobalVar(29) + 1);
            }

            return RunDefault;
        }
        public override bool OnResurrect(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(58, false);
            return RunDefault;
        }
        public override bool OnJoin(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(235, true);
            var ring = attachee.FindItemByName(6088);
            if ((ring != null))
            {
                ring.SetItemFlag(ItemFlag.NO_TRANSFER);
            }

            var dagger = attachee.FindItemByName(4058);
            if ((dagger != null))
            {
                dagger.SetItemFlag(ItemFlag.NO_TRANSFER);
            }

            return RunDefault;
        }
        public override bool OnDisband(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(235, false);
            var ring = attachee.FindItemByName(6088);
            if ((ring != null))
            {
                ring.ClearItemFlag(ItemFlag.NO_TRANSFER);
            }

            var dagger = attachee.FindItemByName(4058);
            if ((dagger != null))
            {
                dagger.ClearItemFlag(ItemFlag.NO_TRANSFER);
            }

            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                attachee.AIRemoveFromShitlist(pc);
                attachee.SetReaction(pc, 50);
            }

            return RunDefault;
        }
        public override bool OnNewMap(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (((attachee.GetArea() == 2) || (attachee.GetArea() == 4)))
            {
                SetGlobalFlag(60, true);
            }
            else if (((attachee.GetArea() == 1) && (GetGlobalFlag(60))))
            {
                SetGlobalFlag(60, false);
                if ((attachee.GetMoney() >= 200000))
                {
                    var leader = attachee.GetLeader();
                    if ((leader != null))
                    {
                        leader.BeginDialog(attachee, 400);
                    }

                }

            }

            return RunDefault;
        }

    }
}

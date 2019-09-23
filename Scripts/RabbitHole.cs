
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
    [ObjectScript(398)]
    public class RabbitHole : BaseObjectScript
    {
        public override bool OnUse(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalVar(937) == 0 && GetQuestState(89) >= QuestState.Accepted))
            {
                SetGlobalVar(937, 1);
            }

            return RunDefault;
        }
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalVar(937) >= 4))
            {
                triggerer.BeginDialog(attachee, 1);
            }
            else if ((GetGlobalVar(937) == 3))
            {
                triggerer.BeginDialog(attachee, 10);
            }
            else
            {
                return SkipDefault;
            }

            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalVar(937) == 1))
            {
                attachee.ClearObjectFlag(ObjectFlag.OFF);
                SetGlobalVar(937, 2);
            }

            return RunDefault;
        }
        public override bool OnStartCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var leader = PartyLeader;
            Co8.StopCombat(attachee, 0);
            leader.BeginDialog(attachee, 4000);
            return RunDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetMap() == 5143 && GetGlobalVar(937) == 2))
            {
                if ((!GameSystems.Combat.IsCombatActive()))
                {
                    foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                    {
                        if ((close_enough(attachee, obj)))
                        {
                            if ((obj.GetSkillLevel(attachee, SkillId.spot) >= 15))
                            {
                                obj.BeginDialog(attachee, 20);
                                SetGlobalVar(937, 3);
                                attachee.ClearObjectFlag(ObjectFlag.DONTDRAW);
                            }

                        }

                    }

                }

            }

            return RunDefault;
        }
        public static bool close_enough(GameObjectBody speaker, GameObjectBody listener)
        {
            if ((speaker.DistanceTo(listener) <= 5))
            {
                return true;
            }

            return false;
        }

    }
}

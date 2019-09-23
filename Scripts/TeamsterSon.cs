
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
    [ObjectScript(21)]
    public class TeamsterSon : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetMap() == 5001))
            {
                triggerer.BeginDialog(attachee, 1);
            }
            else if ((GetGlobalFlag(6)))
            {
                triggerer.BeginDialog(attachee, 80);
            }
            else
            {
                return RunDefault;
            }

            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetMap() == 5032))
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

            if ((attachee.GetMap() == 5001))
            {
                SetGlobalFlag(5, true);
                Utilities.create_item_in_inventory(12004, attachee);
                if ((!triggerer.HasReputation(9)))
                {
                    triggerer.AddReputation(9);
                }

            }
            else
            {
                SetGlobalFlag(300, true);
            }

            return RunDefault;
        }
        public override bool OnResurrect(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(5, false);
            SetGlobalFlag(300, false);
            return RunDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetMap() == 5001))
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
        public static bool discovered_and_leaves_field(GameObjectBody attachee, GameObjectBody triggerer)
        {
            // attachee.standpoint_set( STANDPOINT_NIGHT, 164 )
            attachee.RunOff();
            SetGlobalFlag(230, true);
            // game.timevent_add( turn_back_on, ( attachee, ), 43200000 )
            return RunDefault;
        }
        public static bool turn_back_on(GameObjectBody attachee)
        {
            attachee.ClearObjectFlag(ObjectFlag.OFF);
            // game.timevent_add( assassinated, ( attachee, ), 86400000 )
            return RunDefault;
        }
        public static bool turn_back_on2(GameObjectBody attachee)
        {
            StartTimer(86400000, () => assassinated(attachee));
            return RunDefault;
        }
        public static bool assassinated(GameObjectBody attachee)
        {
            attachee.SetObjectFlag(ObjectFlag.OFF);
            SetGlobalFlag(7, true);
            return RunDefault;
        }

    }
}

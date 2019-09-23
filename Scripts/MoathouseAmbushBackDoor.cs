
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
    [ObjectScript(564)]
    public class MoathouseAmbushBackDoor : BaseObjectScript
    {
        public override bool OnUse(GameObjectBody attachee, GameObjectBody triggerer)
        {
            // used by door in Moathouse Interior to Moathouse Back Door to tell which Ambush to turn on
            SetGlobalVar(710, 3);
            return RunDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalVar(765) == 1 || GetGlobalVar(765) == 2 || GetGlobalFlag(283)))
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
                return SkipDefault;
            }
            else if (((attachee.GetMap() == 5002) && (GetGlobalVar(765) == 0) && (PartyAlignment != Alignment.LAWFUL_EVIL) && (GetGlobalFlag(977)) && (GetGlobalVar(710) == 3)) && ((GetGlobalVar(450) & Math.Pow(2, 0)) == 0) && ((GetGlobalVar(450) & Math.Pow(2, 11)) == 0))
            {
                if (!SelectedPartyLeader.GetPartyMembers().Any(o => o.HasFollowerByName(8002)) && !SelectedPartyLeader.GetPartyMembers().Any(o => o.HasFollowerByName(8004)) && !SelectedPartyLeader.GetPartyMembers().Any(o => o.HasFollowerByName(8005)) && !SelectedPartyLeader.GetPartyMembers().Any(o => o.HasFollowerByName(8010)))
                {
                    if (((GetGlobalFlag(44) != 1) && (GetGlobalFlag(45) != 1) && (GetGlobalFlag(700) != 1) && (GetGlobalFlag(37)) && (!GetGlobalFlag(283))))
                    {
                        attachee.ClearObjectFlag(ObjectFlag.OFF);
                    }

                }

            }

            attachee.SetScriptId(ObjScriptEvent.StartCombat, 2); // san_start_combat
            return RunDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            SetGlobalVar(765, 3);
            return RunDefault;
        }

    }
}

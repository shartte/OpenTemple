
using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.Dialog;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.Script;
using OpenTemple.Core.Systems.Spells;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.Systems.D20.Conditions;
using OpenTemple.Core.Location;
using OpenTemple.Core.Systems.ObjScript;
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace Scripts
{
    [ObjectScript(288)]
    public class AhFong : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            triggerer.BeginDialog(attachee, 1);
            attachee.TurnTowards(triggerer);
            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((!GetGlobalFlag(914)))
            {
                StartTimer(604800000, () => respawn(attachee)); // 604800000ms is 1 week
                SetGlobalFlag(914, true);
            }

            return RunDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            return RunDefault;
        }
        public static void respawn(GameObjectBody attachee)
        {
            var box = Utilities.find_container_near(attachee, 1001);
            InventoryRespawn.RespawnInventory(box);
            StartTimer(604800000, () => respawn(attachee)); // 604800000ms is 1 week
            return;
        }

    }
}

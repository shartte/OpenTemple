
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace VanillaScripts.Dialog
{
    [DialogScript(81)]
    public class SpugnoirDialog : Spugnoir, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 3:
                    originalScript = "not npc.has_met(pc)";
                    return !npc.HasMet(pc);
                case 4:
                case 5:
                    originalScript = "npc.has_met(pc)";
                    return npc.HasMet(pc);
                case 21:
                case 22:
                    originalScript = "game.story_state < 2";
                    return StoryState < 2;
                case 23:
                case 24:
                    originalScript = "game.story_state >= 2";
                    return StoryState >= 2;
                case 31:
                case 32:
                    originalScript = "game.areas[2] == 1 and npc.leader_get() == OBJ_HANDLE_NULL";
                    return IsAreaKnown(2) && npc.GetLeader() == null;
                case 33:
                case 34:
                case 104:
                case 109:
                    originalScript = "game.areas[2] == 0";
                    return !IsAreaKnown(2);
                case 35:
                case 36:
                    originalScript = "game.areas[2] == 1 and npc.leader_get() != OBJ_HANDLE_NULL";
                    return IsAreaKnown(2) && npc.GetLeader() != null;
                case 37:
                case 38:
                    originalScript = "game.areas[5] == 0";
                    return !IsAreaKnown(5);
                case 41:
                case 42:
                    originalScript = "not pc.follower_atmax()";
                    return !pc.HasMaxFollowers();
                case 45:
                case 46:
                    originalScript = "pc.follower_atmax()";
                    return pc.HasMaxFollowers();
                case 71:
                case 72:
                case 81:
                case 82:
                case 101:
                case 106:
                case 121:
                case 122:
                case 131:
                case 132:
                    originalScript = "npc.leader_get() == OBJ_HANDLE_NULL";
                    return npc.GetLeader() == null;
                case 83:
                case 84:
                case 105:
                case 110:
                    originalScript = "game.story_state == 2";
                    return StoryState == 2;
                case 102:
                case 107:
                    originalScript = "npc.leader_get() != OBJ_HANDLE_NULL";
                    return npc.GetLeader() != null;
                case 111:
                case 112:
                    originalScript = "game.story_state < 2 and game.areas[5] == 0";
                    return StoryState < 2 && !IsAreaKnown(5);
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 33:
                case 34:
                case 104:
                case 109:
                    originalScript = "game.story_state = 1; game.areas[2] = 1";
                    StoryState = 1;
                    MakeAreaKnown(2);
                    ;
                    break;
                case 41:
                case 42:
                    originalScript = "pc.follower_add(npc)";
                    pc.AddFollower(npc);
                    break;
                case 75:
                case 76:
                    originalScript = "game.worldmap_travel_by_dialog(2)";
                    // FIXME: worldmap_travel_by_dialog;
                    break;
                case 83:
                case 84:
                case 105:
                case 110:
                    originalScript = "game.story_state = 3; game.areas[3] = 1";
                    StoryState = 3;
                    MakeAreaKnown(3);
                    ;
                    break;
                case 91:
                case 92:
                    originalScript = "pc.follower_remove(npc)";
                    pc.RemoveFollower(npc);
                    break;
                case 125:
                case 126:
                    originalScript = "game.worldmap_travel_by_dialog(3)";
                    // FIXME: worldmap_travel_by_dialog;
                    break;
                case 130:
                    originalScript = "game.areas[5] = 1";
                    MakeAreaKnown(5);
                    break;
                case 135:
                case 136:
                    originalScript = "game.worldmap_travel_by_dialog(5)";
                    // FIXME: worldmap_travel_by_dialog;
                    break;
                default:
                    originalScript = null;
                    return;
            }
        }
        public bool TryGetSkillChecks(int lineNumber, out DialogSkillChecks skillChecks)
        {
            switch (lineNumber)
            {
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}

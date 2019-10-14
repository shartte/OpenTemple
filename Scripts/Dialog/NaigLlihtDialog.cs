
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

namespace Scripts.Dialog
{
    [DialogScript(600)]
    public class NaigLlihtDialog : NaigLliht, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 11:
                case 21:
                case 31:
                case 41:
                case 51:
                case 61:
                case 71:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_diplomacy) >= 0");
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 0;
                case 3:
                case 12:
                case 22:
                case 32:
                case 42:
                case 52:
                case 62:
                case 72:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_bluff) >= 0");
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 0;
                case 4:
                case 13:
                case 23:
                case 33:
                case 43:
                case 53:
                case 63:
                case 73:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_intimidate) >= 0");
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 0;
                case 5:
                case 14:
                case 24:
                case 34:
                case 44:
                case 54:
                case 64:
                case 74:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_gather_information) >= 0");
                    return pc.GetSkillLevel(npc, SkillId.gather_information) >= 0;
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 10:
                    Trace.Assert(originalScript == "game.global_vars[802] = game.global_vars[802] + 1; increment_rep(npc,pc)");
                    SetGlobalVar(802, GetGlobalVar(802) + 1);
                    increment_rep(npc, pc);
                    ;
                    break;
                case 80:
                    Trace.Assert(originalScript == "set_alarm(npc,pc); game.global_vars[980] = 1");
                    set_alarm(npc, pc);
                    SetGlobalVar(980, 1);
                    ;
                    break;
                default:
                    Trace.Assert(originalScript == null);
                    return;
            }
        }
        public bool TryGetSkillChecks(int lineNumber, out DialogSkillChecks skillChecks)
        {
            switch (lineNumber)
            {
                case 2:
                case 11:
                case 21:
                case 31:
                case 41:
                case 51:
                case 61:
                case 71:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 0);
                    return true;
                case 3:
                case 12:
                case 22:
                case 32:
                case 42:
                case 52:
                case 62:
                case 72:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 0);
                    return true;
                case 4:
                case 13:
                case 23:
                case 33:
                case 43:
                case 53:
                case 63:
                case 73:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 0);
                    return true;
                case 5:
                case 14:
                case 24:
                case 34:
                case 44:
                case 54:
                case 64:
                case 74:
                    skillChecks = new DialogSkillChecks(SkillId.gather_information, 0);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
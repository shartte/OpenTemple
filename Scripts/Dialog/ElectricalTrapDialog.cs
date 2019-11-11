
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
    [DialogScript(314)]
    public class ElectricalTrapDialog : ElectricalTrap, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 1001:
                case 1002:
                case 1003:
                case 1012:
                case 1013:
                case 1014:
                case 1504:
                case 1505:
                case 1506:
                case 1602:
                case 1603:
                    originalScript = "game.global_flags[813] == 1";
                    return GetGlobalFlag(813);
                case 1004:
                case 1015:
                    originalScript = "pc.skill_level_get(skill_disable_device) >= 5 and game.global_flags[820] == 0";
                    throw new NotSupportedException("Conversion failed.");
                case 1005:
                case 1016:
                case 1507:
                case 1604:
                    originalScript = "game.global_flags[813] == 0";
                    return !GetGlobalFlag(813);
                case 1501:
                    originalScript = "pc.skill_level_get(skill_disable_device) <= 11";
                    throw new NotSupportedException("Conversion failed.");
                case 1502:
                    originalScript = "pc.skill_level_get(skill_disable_device) <= 15 and pc.skill_level_get(skill_disable_device) >= 12";
                    throw new NotSupportedException("Conversion failed.");
                case 1503:
                    originalScript = "pc.skill_level_get(skill_disable_device) >= 16";
                    throw new NotSupportedException("Conversion failed.");
                case 1801:
                    originalScript = "game.global_flags[820] == 0";
                    return !GetGlobalFlag(820);
                case 1802:
                    originalScript = "game.global_flags[820] == 1";
                    return GetGlobalFlag(820);
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 1:
                case 1700:
                case 1801:
                    originalScript = "game.global_flags[820] = 1";
                    SetGlobalFlag(820, true);
                    break;
                case 2:
                case 1006:
                case 1017:
                case 1508:
                case 1605:
                case 1803:
                    originalScript = "game.global_vars[707] = 3";
                    SetGlobalVar(707, 3);
                    break;
                case 1501:
                    originalScript = "zap(npc,pc); game.global_vars[707] = 3";
                    zap(npc, pc);
                    SetGlobalVar(707, 3);
                    ;
                    break;
                case 1802:
                    originalScript = "game.global_flags[820] = 0; game.global_vars[707] = 3";
                    SetGlobalFlag(820, false);
                    SetGlobalVar(707, 3);
                    ;
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


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
    [DialogScript(144)]
    public class MurrayaDialog : Murraya, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 13:
                case 14:
                    Trace.Assert(originalScript == "game.global_flags[146] == 1");
                    return GetGlobalFlag(146);
                case 15:
                case 16:
                    Trace.Assert(originalScript == "game.global_flags[146] == 0");
                    return !GetGlobalFlag(146);
                case 17:
                case 18:
                    Trace.Assert(originalScript == "pc.stat_level_get( stat_gender ) == gender_male");
                    return pc.GetGender() == Gender.Male;
                case 19:
                case 20:
                    Trace.Assert(originalScript == "game.global_flags[148] == 1");
                    return GetGlobalFlag(148);
                case 21:
                case 22:
                    Trace.Assert(originalScript == "game.global_flags[158] == 1 and game.global_flags[148] == 0");
                    return GetGlobalFlag(158) && !GetGlobalFlag(148);
                case 23:
                case 24:
                    Trace.Assert(originalScript == "game.global_flags[158] == 1 and game.global_flags[148] == 1");
                    return GetGlobalFlag(158) && GetGlobalFlag(148);
                case 81:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_charisma) >= 16 and game.global_flags[148] == 0");
                    return pc.GetStat(Stat.charisma) >= 16 && !GetGlobalFlag(148);
                case 82:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_charisma) >= 11 and pc.stat_level_get(stat_charisma) <= 15 and game.global_flags[148] == 0");
                    return pc.GetStat(Stat.charisma) >= 11 && pc.GetStat(Stat.charisma) <= 15 && !GetGlobalFlag(148);
                case 83:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_charisma) <= 10 and game.global_flags[148] == 0");
                    return pc.GetStat(Stat.charisma) <= 10 && !GetGlobalFlag(148);
                case 86:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_charisma) >= 16 and game.global_flags[148] == 1");
                    return pc.GetStat(Stat.charisma) >= 16 && GetGlobalFlag(148);
                case 87:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_charisma) >= 11 and pc.stat_level_get(stat_charisma) <= 15 and game.global_flags[148] == 1");
                    return pc.GetStat(Stat.charisma) >= 11 && pc.GetStat(Stat.charisma) <= 15 && GetGlobalFlag(148);
                case 88:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_charisma) <= 10 and game.global_flags[148] == 1");
                    return pc.GetStat(Stat.charisma) <= 10 && GetGlobalFlag(148);
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 1:
                    Trace.Assert(originalScript == "game.global_flags[159] = 1");
                    SetGlobalFlag(159, true);
                    break;
                case 2:
                case 3:
                    Trace.Assert(originalScript == "LookHedrack(npc,pc,300)");
                    LookHedrack(npc, pc, 300);
                    break;
                case 11:
                case 12:
                case 31:
                case 32:
                case 251:
                case 252:
                    Trace.Assert(originalScript == "npc.attack( pc )");
                    npc.Attack(pc);
                    break;
                case 130:
                case 140:
                case 210:
                    Trace.Assert(originalScript == "game.global_flags[161] = 1");
                    SetGlobalFlag(161, true);
                    break;
                case 201:
                    Trace.Assert(originalScript == "get_rep( npc, pc )");
                    get_rep(npc, pc);
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
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
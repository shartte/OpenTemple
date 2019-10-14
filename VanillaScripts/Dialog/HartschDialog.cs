
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
    [DialogScript(123)]
    public class HartschDialog : Hartsch, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 3:
                    Trace.Assert(originalScript == "game.quests[43].state == qs_mentioned");
                    return GetQuestState(43) == QuestState.Mentioned;
                case 4:
                case 5:
                    Trace.Assert(originalScript == "game.quests[43].state >= qs_accepted");
                    return GetQuestState(43) >= QuestState.Accepted;
                case 25:
                case 26:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_sense_motive) >= 9");
                    return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 9;
                case 205:
                case 206:
                    Trace.Assert(originalScript == "game.quests[43].state == qs_accepted");
                    return GetQuestState(43) == QuestState.Accepted;
                case 207:
                case 208:
                    Trace.Assert(originalScript == "game.global_flags[119] == 0 and game.quests[44].state == qs_accepted and ( not anyone( pc.group_list(), \"has_item\", 5807 ) ) and game.global_flags[120] == 0");
                    return !GetGlobalFlag(119) && GetQuestState(44) == QuestState.Accepted && (!pc.GetPartyMembers().Any(o => o.HasItemByName(5807))) && !GetGlobalFlag(120);
                case 209:
                case 210:
                    Trace.Assert(originalScript == "game.global_flags[115] == 0 and game.global_flags[116] == 0 and game.global_flags[119] == 1 and game.quests[44].state == qs_accepted");
                    return !GetGlobalFlag(115) && !GetGlobalFlag(116) && GetGlobalFlag(119) && GetQuestState(44) == QuestState.Accepted;
                case 211:
                case 212:
                    Trace.Assert(originalScript == "game.quests[45].state == qs_accepted");
                    return GetQuestState(45) == QuestState.Accepted;
                case 263:
                case 264:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_bluff) >= 10");
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 10;
                case 283:
                case 284:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_sense_motive) >= 6");
                    return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 6;
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 41:
                case 51:
                case 52:
                    Trace.Assert(originalScript == "npc.attack( pc )");
                    npc.Attack(pc);
                    break;
                case 250:
                    Trace.Assert(originalScript == "game.map_flags( 5066, 2, 1 )");
                    // FIXME: map_flags;
                    break;
                case 263:
                case 264:
                    Trace.Assert(originalScript == "game.global_flags[124] = 1");
                    SetGlobalFlag(124, true);
                    break;
                case 270:
                    Trace.Assert(originalScript == "game.map_flags( 5067, 0, 1 )");
                    // FIXME: map_flags;
                    break;
                case 280:
                    Trace.Assert(originalScript == "game.map_flags( 5067, 1, 1 )");
                    // FIXME: map_flags;
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
                case 25:
                case 26:
                    skillChecks = new DialogSkillChecks(SkillId.sense_motive, 9);
                    return true;
                case 263:
                case 264:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 10);
                    return true;
                case 283:
                case 284:
                    skillChecks = new DialogSkillChecks(SkillId.sense_motive, 6);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
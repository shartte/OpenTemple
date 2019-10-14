
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
    [DialogScript(175)]
    public class AshremDialog : Ashrem, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 61:
                case 62:
                    Trace.Assert(originalScript == "( game.quests[52].state >= qs_mentioned ) and ( not pc.follower_atmax() ) and ( anyone( pc.group_list(), \"has_follower\", 8039 ) ) and ( pc.stat_level_get(stat_level_paladin) == 0 )");
                    return (GetQuestState(52) >= QuestState.Mentioned) && (!pc.HasMaxFollowers()) && (pc.GetPartyMembers().Any(o => o.HasFollowerByName(8039))) && (pc.GetStat(Stat.level_paladin) == 0);
                case 63:
                case 64:
                case 135:
                case 136:
                case 153:
                case 154:
                case 161:
                case 162:
                    Trace.Assert(originalScript == "pc.follower_atmax() and pc.stat_level_get(stat_level_paladin) == 0");
                    return pc.HasMaxFollowers() && pc.GetStat(Stat.level_paladin) == 0;
                case 65:
                case 66:
                case 191:
                case 192:
                    Trace.Assert(originalScript == "game.global_flags[107] == 1");
                    return GetGlobalFlag(107);
                case 67:
                case 68:
                    Trace.Assert(originalScript == "( game.quests[52].state >= qs_mentioned ) and ( not pc.follower_atmax() ) and ( not anyone( pc.group_list(), \"has_follower\", 8039 ) ) and (pc.stat_level_get(stat_level_paladin) == 0)");
                    return (GetQuestState(52) >= QuestState.Mentioned) && (!pc.HasMaxFollowers()) && (!pc.GetPartyMembers().Any(o => o.HasFollowerByName(8039))) && (pc.GetStat(Stat.level_paladin) == 0);
                case 69:
                case 70:
                    Trace.Assert(originalScript == "( game.quests[52].state == qs_unknown ) and ( not pc.follower_atmax() ) and ( anyone( pc.group_list(), \"has_follower\", 8039 ) ) and (pc.stat_level_get(stat_level_paladin) == 0)");
                    return (GetQuestState(52) == QuestState.Unknown) && (!pc.HasMaxFollowers()) && (pc.GetPartyMembers().Any(o => o.HasFollowerByName(8039))) && (pc.GetStat(Stat.level_paladin) == 0);
                case 71:
                case 72:
                    Trace.Assert(originalScript == "( game.quests[52].state == qs_unknown ) and ( not pc.follower_atmax() ) and ( not anyone( pc.group_list(), \"has_follower\", 8039 ) ) and (pc.stat_level_get(stat_level_paladin) == 0)");
                    return (GetQuestState(52) == QuestState.Unknown) && (!pc.HasMaxFollowers()) && (!pc.GetPartyMembers().Any(o => o.HasFollowerByName(8039))) && (pc.GetStat(Stat.level_paladin) == 0);
                case 75:
                case 76:
                    Trace.Assert(originalScript == "(pc.stat_level_get(stat_level_paladin) == 1)");
                    return (pc.GetStat(Stat.level_paladin) == 1);
                case 81:
                case 82:
                    Trace.Assert(originalScript == "(game.party_alignment != LAWFUL_EVIL) and (game.party_alignment != NEUTRAL_EVIL) and (game.party_alignment != CHAOTIC_EVIL) and anyone( pc.group_list(), \"has_follower\", 8040 )");
                    return (PartyAlignment != Alignment.LAWFUL_EVIL) && (PartyAlignment != Alignment.NEUTRAL_EVIL) && (PartyAlignment != Alignment.CHAOTIC_EVIL) && pc.GetPartyMembers().Any(o => o.HasFollowerByName(8040));
                case 107:
                case 108:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_sense_motive) >= 10");
                    return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 10;
                case 131:
                case 132:
                case 155:
                case 156:
                case 163:
                case 164:
                    Trace.Assert(originalScript == "( not pc.follower_atmax() ) and ( anyone( pc.group_list(), \"has_follower\", 8039 ) ) and (pc.stat_level_get(stat_level_paladin) == 0)");
                    return (!pc.HasMaxFollowers()) && (pc.GetPartyMembers().Any(o => o.HasFollowerByName(8039))) && (pc.GetStat(Stat.level_paladin) == 0);
                case 133:
                case 134:
                case 157:
                case 158:
                case 165:
                case 166:
                    Trace.Assert(originalScript == "( not pc.follower_atmax() ) and ( not anyone( pc.group_list(), \"has_follower\", 8039 ) ) and (pc.stat_level_get(stat_level_paladin) == 0)");
                    return (!pc.HasMaxFollowers()) && (!pc.GetPartyMembers().Any(o => o.HasFollowerByName(8039))) && (pc.GetStat(Stat.level_paladin) == 0);
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                    Trace.Assert(originalScript == "talk_Taki( npc, pc, 10 )");
                    talk_Taki(npc, pc, 10);
                    break;
                case 11:
                    Trace.Assert(originalScript == "talk_Taki( npc, pc, 180 )");
                    talk_Taki(npc, pc, 180);
                    break;
                case 30:
                    Trace.Assert(originalScript == "game.global_vars[141] = 1");
                    SetGlobalVar(141, 1);
                    break;
                case 31:
                case 32:
                case 75:
                case 76:
                    Trace.Assert(originalScript == "npc.attack(pc)");
                    npc.Attack(pc);
                    break;
                case 91:
                    Trace.Assert(originalScript == "talk_Taki( npc, pc, 230 )");
                    talk_Taki(npc, pc, 230);
                    break;
                case 140:
                    Trace.Assert(originalScript == "pc.follower_add(npc)");
                    pc.AddFollower(npc);
                    break;
                case 180:
                    Trace.Assert(originalScript == "pc.follower_remove(npc)");
                    pc.RemoveFollower(npc);
                    break;
                case 230:
                    Trace.Assert(originalScript == "game.global_flags[192] = 1");
                    SetGlobalFlag(192, true);
                    break;
                case 231:
                    Trace.Assert(originalScript == "talk_Alrrem( npc, pc, 710 )");
                    talk_Alrrem(npc, pc, 710);
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
                case 107:
                case 108:
                    skillChecks = new DialogSkillChecks(SkillId.sense_motive, 10);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
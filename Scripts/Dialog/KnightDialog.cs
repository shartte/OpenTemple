
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
    [DialogScript(185)]
    public class KnightDialog : Knight, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 41:
                case 42:
                    Trace.Assert(originalScript == "anyone( pc.group_list(), \"item_find\", 3014 )");
                    return pc.GetPartyMembers().Any(o => o.FindItemByName(3014) != null);
                case 43:
                case 44:
                    Trace.Assert(originalScript == "(not anyone( pc.group_list(), \"item_find\", 3014 )) and ( game.party_alignment & ALIGNMENT_GOOD != 0 )");
                    return (!pc.GetPartyMembers().Any(o => o.FindItemByName(3014) != null)) && (PartyAlignment.IsGood());
                case 45:
                case 46:
                    Trace.Assert(originalScript == "(not anyone( pc.group_list(), \"item_find\", 3014 )) and ( game.party_alignment & ALIGNMENT_GOOD == 0 ) and ( pc.stat_level_get(stat_alignment) & ALIGNMENT_GOOD != 0 )");
                    return (!pc.GetPartyMembers().Any(o => o.FindItemByName(3014) != null)) && (!PartyAlignment.IsGood()) && (pc.GetAlignment().IsGood());
                case 47:
                case 48:
                    Trace.Assert(originalScript == "(not anyone( pc.group_list(), \"item_find\", 3014 )) and ( game.party_alignment & ALIGNMENT_GOOD == 0 ) and ( pc.stat_level_get(stat_alignment) & ALIGNMENT_GOOD == 0 )");
                    return (!pc.GetPartyMembers().Any(o => o.FindItemByName(3014) != null)) && (!PartyAlignment.IsGood()) && (!pc.GetAlignment().IsGood());
                case 81:
                case 82:
                    Trace.Assert(originalScript == "((game.party_alignment == LAWFUL_GOOD) or (game.party_alignment == NEUTRAL_GOOD) or (game.party_alignment == CHAOTIC_GOOD))");
                    return ((PartyAlignment == Alignment.LAWFUL_GOOD) || (PartyAlignment == Alignment.NEUTRAL_GOOD) || (PartyAlignment == Alignment.CHAOTIC_GOOD));
                case 83:
                case 84:
                    Trace.Assert(originalScript == "(game.party_alignment != LAWFUL_GOOD) and (game.party_alignment != NEUTRAL_GOOD) and (game.party_alignment != CHAOTIC_GOOD) and ((pc.stat_level_get(stat_alignment) == LAWFUL_GOOD) or (pc.stat_level_get(stat_alignment) == NEUTRAL_GOOD) or (pc.stat_level_get(stat_alignment) == CHAOTIC_GOOD))");
                    return (PartyAlignment != Alignment.LAWFUL_GOOD) && (PartyAlignment != Alignment.NEUTRAL_GOOD) && (PartyAlignment != Alignment.CHAOTIC_GOOD) && ((pc.GetAlignment() == Alignment.LAWFUL_GOOD) || (pc.GetAlignment() == Alignment.NEUTRAL_GOOD) || (pc.GetAlignment() == Alignment.CHAOTIC_GOOD));
                case 85:
                case 86:
                    Trace.Assert(originalScript == "(game.party_alignment != LAWFUL_GOOD) and (game.party_alignment != NEUTRAL_GOOD) and (game.party_alignment != CHAOTIC_GOOD) and (pc.stat_level_get(stat_alignment) != LAWFUL_GOOD) and (pc.stat_level_get(stat_alignment) != NEUTRAL_GOOD) and (pc.stat_level_get(stat_alignment) != CHAOTIC_GOOD)");
                    return (PartyAlignment != Alignment.LAWFUL_GOOD) && (PartyAlignment != Alignment.NEUTRAL_GOOD) && (PartyAlignment != Alignment.CHAOTIC_GOOD) && (pc.GetAlignment() != Alignment.LAWFUL_GOOD) && (pc.GetAlignment() != Alignment.NEUTRAL_GOOD) && (pc.GetAlignment() != Alignment.CHAOTIC_GOOD);
                case 91:
                case 92:
                    Trace.Assert(originalScript == "(pc.stat_level_get(stat_alignment) == LAWFUL_GOOD) or (pc.stat_level_get(stat_alignment) == NEUTRAL_GOOD) or (pc.stat_level_get(stat_alignment) == CHAOTIC_GOOD)");
                    return (pc.GetAlignment() == Alignment.LAWFUL_GOOD) || (pc.GetAlignment() == Alignment.NEUTRAL_GOOD) || (pc.GetAlignment() == Alignment.CHAOTIC_GOOD);
                case 93:
                case 94:
                    Trace.Assert(originalScript == "(pc.stat_level_get(stat_alignment) != LAWFUL_GOOD) and (pc.stat_level_get(stat_alignment) != NEUTRAL_GOOD) and (pc.stat_level_get(stat_alignment) != CHAOTIC_GOOD)");
                    return (pc.GetAlignment() != Alignment.LAWFUL_GOOD) && (pc.GetAlignment() != Alignment.NEUTRAL_GOOD) && (pc.GetAlignment() != Alignment.CHAOTIC_GOOD);
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 40:
                    Trace.Assert(originalScript == "distribute_magic_items(npc,pc)");
                    distribute_magic_items(npc, pc);
                    break;
                case 61:
                    Trace.Assert(originalScript == "run_off(npc,pc)");
                    run_off(npc, pc);
                    break;
                case 71:
                case 75:
                    Trace.Assert(originalScript == "transfer_scrolls(npc,pc)");
                    transfer_scrolls(npc, pc);
                    break;
                case 72:
                case 76:
                    Trace.Assert(originalScript == "npc.item_transfer_to_by_proto( pc, 6127)");
                    npc.TransferItemByProtoTo(pc, 6127);
                    break;
                case 73:
                case 77:
                    Trace.Assert(originalScript == "npc.item_transfer_to_by_proto( pc, 6050)");
                    npc.TransferItemByProtoTo(pc, 6050);
                    break;
                case 74:
                case 78:
                    Trace.Assert(originalScript == "npc.item_transfer_to_by_proto( pc, 6086)");
                    npc.TransferItemByProtoTo(pc, 6086);
                    break;
                case 90:
                    Trace.Assert(originalScript == "knight_party(npc,pc)");
                    knight_party(npc, pc);
                    break;
                case 100:
                    Trace.Assert(originalScript == "npc.item_transfer_to_by_proto( pc, 4016)");
                    npc.TransferItemByProtoTo(pc, 4016);
                    break;
                case 200:
                    Trace.Assert(originalScript == "npc.scripts[19] = 0");
                    npc.RemoveScript(ObjScriptEvent.Heartbeat);
                    break;
                case 201:
                    Trace.Assert(originalScript == "call_good_pc( npc, pc )");
                    call_good_pc(npc, pc);
                    break;
                case 202:
                    Trace.Assert(originalScript == "call_good_pc(npc, pc)");
                    call_good_pc(npc, pc);
                    break;
                default:
                    Trace.Assert(originalScript == null);
                    return;
            }
        }
        public bool TryGetSkillCheck(int lineNumber, out DialogSkillChecks skillChecks)
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

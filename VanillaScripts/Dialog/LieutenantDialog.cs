
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
    [DialogScript(76)]
    public class LieutenantDialog : Lieutenant, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 3:
                    originalScript = "game.global_flags[48] == 1";
                    return GetGlobalFlag(48);
                case 4:
                case 5:
                    originalScript = "game.quests[16].state == qs_completed and (game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == CHAOTIC_GOOD or game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == TRUE_NEUTRAL or game.party_alignment == CHAOTIC_NEUTRAL)";
                    return GetQuestState(16) == QuestState.Completed && (PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD || PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL);
                case 6:
                case 7:
                    originalScript = "game.quests[16].state == qs_completed and (game.party_alignment == CHAOTIC_EVIL or game.party_alignment == NEUTRAL_EVIL or game.party_alignment == LAWFUL_EVIL)";
                    return GetQuestState(16) == QuestState.Completed && (PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.LAWFUL_EVIL);
                case 31:
                case 32:
                    originalScript = "pc.skill_level_get(npc,skill_bluff) >= 7";
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 7;
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 11:
                case 12:
                case 81:
                    originalScript = "npc.attack( pc )";
                    npc.Attack(pc);
                    break;
                case 40:
                    originalScript = "game.global_flags[52] = 1";
                    SetGlobalFlag(52, true);
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
                case 31:
                case 32:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 7);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}

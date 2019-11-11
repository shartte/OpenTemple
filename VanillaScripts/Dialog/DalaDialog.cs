
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
    [DialogScript(109)]
    public class DalaDialog : Dala, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 62:
                case 64:
                    originalScript = "pc.stat_level_get(stat_charisma) >= 6";
                    return pc.GetStat(Stat.charisma) >= 6;
                case 65:
                case 66:
                    originalScript = "pc.stat_level_get(stat_charisma) <= 5";
                    return pc.GetStat(Stat.charisma) <= 5;
                case 71:
                case 73:
                    originalScript = "pc.money_get() >= 5";
                    return pc.GetMoney() >= 5;
                case 72:
                case 74:
                    originalScript = "pc.money_get() < 5";
                    return pc.GetMoney() < 5;
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 71:
                case 73:
                    originalScript = "pc.money_adj(-5)";
                    pc.AdjustMoney(-5);
                    break;
                case 120:
                    originalScript = "game.quests[37].state == qs_completed";
                    SetQuestState(37, QuestState.Completed);
                    break;
                case 121:
                case 122:
                    originalScript = "make_dick_talk(npc,pc,1)";
                    make_dick_talk(npc, pc, 1);
                    break;
                case 133:
                case 136:
                    originalScript = "npc.attack(pc)";
                    npc.Attack(pc);
                    break;
                case 150:
                    originalScript = "create_item_in_inventory( 8004, pc )";
                    Utilities.create_item_in_inventory(8004, pc);
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

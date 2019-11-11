
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
    [DialogScript(156)]
    public class FarmerPrisonerWifeDialog : FarmerPrisonerWife, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 33:
                    originalScript = "game.party_alignment == LAWFUL_GOOD or game.party_alignment == CHAOTIC_GOOD or game.party_alignment == NEUTRAL_GOOD and pc.money_get() >= 100000";
                    return PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD && pc.GetMoney() >= 100000;
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 3:
                    originalScript = "banter(npc,pc,10)";
                    banter(npc, pc, 10);
                    break;
                case 11:
                case 31:
                case 32:
                case 52:
                case 53:
                case 61:
                case 62:
                case 71:
                case 72:
                case 81:
                case 82:
                case 93:
                case 94:
                case 101:
                case 113:
                case 114:
                    originalScript = "game.global_flags[169] = 1; run_off( npc, pc ); get_rep( npc, pc )";
                    SetGlobalFlag(169, true);
                    run_off(npc, pc);
                    get_rep(npc, pc);
                    ;
                    break;
                case 21:
                case 22:
                    originalScript = "banter(npc,pc,20)";
                    banter(npc, pc, 20);
                    break;
                case 33:
                    originalScript = "pc.money_adj(-100000)";
                    pc.AdjustMoney(-100000);
                    break;
                case 41:
                    originalScript = "run_off( npc, pc ); get_rep( npc, pc )";
                    run_off(npc, pc);
                    get_rep(npc, pc);
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

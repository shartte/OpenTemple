
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
    [DialogScript(326)]
    public class PanathaesDialog : Panathaes, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 22:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_diplomacy) >= 19");
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 19;
                case 21:
                    Trace.Assert(originalScript == "game.party_size() >= 2");
                    return GameSystems.Party.PartySize >= 2;
                case 23:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_diplomacy) <= 18");
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) <= 18;
                case 41:
                case 151:
                case 241:
                    Trace.Assert(originalScript == "game.global_flags[530] == 0");
                    return !GetGlobalFlag(530);
                case 42:
                case 51:
                case 242:
                    Trace.Assert(originalScript == "not npc_get(npc,1)");
                    return !ScriptDaemon.npc_get(npc, 1);
                case 44:
                case 62:
                    Trace.Assert(originalScript == "game.global_flags[530] == 1 and pc.follower_atmax() == 0");
                    return GetGlobalFlag(530) && !pc.HasMaxFollowers();
                case 71:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_bluff) >= 19");
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 19;
                case 72:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_bluff) <= 18");
                    return pc.GetSkillLevel(npc, SkillId.bluff) <= 18;
                case 81:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_intimidate) >= 19");
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 19;
                case 82:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_intimidate) <= 18");
                    return pc.GetSkillLevel(npc, SkillId.intimidate) <= 18;
                case 152:
                    Trace.Assert(originalScript == "game.global_flags[530] == 1");
                    return GetGlobalFlag(530);
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 4:
                case 11:
                case 24:
                case 73:
                case 83:
                case 91:
                case 102:
                case 112:
                case 122:
                case 132:
                case 142:
                case 153:
                case 162:
                case 172:
                case 182:
                case 192:
                case 202:
                case 212:
                case 222:
                case 232:
                    Trace.Assert(originalScript == "npc.attack( pc )");
                    npc.Attack(pc);
                    break;
                case 10:
                case 50:
                case 250:
                    Trace.Assert(originalScript == "game.global_flags[530] = 1");
                    SetGlobalFlag(530, true);
                    break;
                case 42:
                case 51:
                case 242:
                    Trace.Assert(originalScript == "npc_set(npc,1)");
                    ScriptDaemon.npc_set(npc, 1);
                    break;
                case 44:
                case 62:
                    Trace.Assert(originalScript == "pc.follower_add( npc ); go_to_sleep(npc,pc); game.global_vars[551] = 1");
                    pc.AddFollower(npc);
                    go_to_sleep(npc, pc);
                    SetGlobalVar(551, 1);
                    ;
                    break;
                case 61:
                    Trace.Assert(originalScript == "go_to_sleep(npc,pc)");
                    go_to_sleep(npc, pc);
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
                case 2:
                case 22:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 19);
                    return true;
                case 71:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 19);
                    return true;
                case 81:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 19);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}

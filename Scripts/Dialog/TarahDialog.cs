
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
    [DialogScript(342)]
    public class TarahDialog : Tarah, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 11:
                case 351:
                    Trace.Assert(originalScript == "pc.stat_level_get( stat_charisma ) <= 14");
                    return pc.GetStat(Stat.charisma) <= 14;
                case 12:
                case 352:
                    Trace.Assert(originalScript == "pc.stat_level_get( stat_charisma ) >= 15 and pc.stat_level_get( stat_charisma ) <= 17");
                    return pc.GetStat(Stat.charisma) >= 15 && pc.GetStat(Stat.charisma) <= 17;
                case 13:
                case 353:
                    Trace.Assert(originalScript == "pc.stat_level_get( stat_charisma ) >= 18");
                    return pc.GetStat(Stat.charisma) >= 18;
                case 111:
                    Trace.Assert(originalScript == "game.party[2].stat_level_get( stat_gender ) == gender_male");
                    return GameSystems.Party.GetPartyGroupMemberN(2).GetGender() == Gender.Male;
                case 112:
                    Trace.Assert(originalScript == "game.party[2].stat_level_get( stat_gender ) == gender_female");
                    return GameSystems.Party.GetPartyGroupMemberN(2).GetGender() == Gender.Female;
                case 171:
                    Trace.Assert(originalScript == "game.party_size() == 3");
                    return GameSystems.Party.PartySize == 3;
                case 172:
                    Trace.Assert(originalScript == "game.party_size() == 4");
                    return GameSystems.Party.PartySize == 4;
                case 173:
                    Trace.Assert(originalScript == "game.party_size() == 5");
                    return GameSystems.Party.PartySize == 5;
                case 174:
                    Trace.Assert(originalScript == "game.party_size() == 6");
                    return GameSystems.Party.PartySize == 6;
                case 175:
                    Trace.Assert(originalScript == "game.party_size() == 7");
                    return GameSystems.Party.PartySize == 7;
                case 176:
                    Trace.Assert(originalScript == "game.party_size() == 8");
                    return GameSystems.Party.PartySize == 8;
                case 221:
                case 291:
                case 381:
                case 431:
                    Trace.Assert(originalScript == "(game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == CHAOTIC_GOOD or game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == TRUE_NEUTRAL)");
                    return (PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD || PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.NEUTRAL);
                case 222:
                case 382:
                    Trace.Assert(originalScript == "game.global_flags[801] == 1 and (game.party_alignment == CHAOTIC_NEUTRAL or game.party_alignment == LAWFUL_EVIL)");
                    return GetGlobalFlag(801) && (PartyAlignment == Alignment.CHAOTIC_NEUTRAL || PartyAlignment == Alignment.LAWFUL_EVIL);
                case 223:
                case 383:
                    Trace.Assert(originalScript == "(game.party_alignment == NEUTRAL_EVIL or game.party_alignment == CHAOTIC_EVIL)");
                    return (PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL);
                case 281:
                    Trace.Assert(originalScript == "game.global_vars[992] == 1");
                    return GetGlobalVar(992) == 1;
                case 282:
                    Trace.Assert(originalScript == "game.global_vars[992] == 2");
                    return GetGlobalVar(992) == 2;
                case 283:
                    Trace.Assert(originalScript == "game.global_vars[992] == 3");
                    return GetGlobalVar(992) == 3;
                case 284:
                    Trace.Assert(originalScript == "game.global_vars[992] == 4");
                    return GetGlobalVar(992) == 4;
                case 285:
                    Trace.Assert(originalScript == "game.global_vars[992] == 5");
                    return GetGlobalVar(992) == 5;
                case 286:
                    Trace.Assert(originalScript == "game.global_vars[992] == 6");
                    return GetGlobalVar(992) == 6;
                case 292:
                case 432:
                    Trace.Assert(originalScript == "(game.party_alignment == CHAOTIC_NEUTRAL or game.party_alignment == LAWFUL_EVIL or game.party_alignment == NEUTRAL_EVIL or game.party_alignment == CHAOTIC_EVIL)");
                    return (PartyAlignment == Alignment.CHAOTIC_NEUTRAL || PartyAlignment == Alignment.LAWFUL_EVIL || PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL);
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
                    Trace.Assert(originalScript == "game.sound( 4012 ); dom_mon( npc, pc )");
                    Sound(4012);
                    dom_mon(npc, pc);
                    ;
                    break;
                case 21:
                case 61:
                    Trace.Assert(originalScript == "pick_to_grope( npc, pc )");
                    pick_to_grope(npc, pc);
                    break;
                case 70:
                    Trace.Assert(originalScript == "game.global_vars[992] = 1");
                    SetGlobalVar(992, 1);
                    break;
                case 71:
                    Trace.Assert(originalScript == "meleny_see_tarah( npc, pc ); game.sound( 4018 ); switch_to_meleny( npc, pc, 340)");
                    meleny_see_tarah(npc, pc);
                    Sound(4018);
                    switch_to_meleny(npc, pc, 340);
                    ;
                    break;
                case 80:
                    Trace.Assert(originalScript == "game.global_vars[992] = 3");
                    SetGlobalVar(992, 3);
                    break;
                case 81:
                    Trace.Assert(originalScript == "fruella_see_tarah( npc, pc ); game.sound( 4018 ); switch_to_fruella( npc, pc, 410)");
                    fruella_see_tarah(npc, pc);
                    Sound(4018);
                    switch_to_fruella(npc, pc, 410);
                    ;
                    break;
                case 90:
                    Trace.Assert(originalScript == "game.global_vars[992] = 4");
                    SetGlobalVar(992, 4);
                    break;
                case 91:
                    Trace.Assert(originalScript == "serena_see_tarah( npc, pc ); game.sound( 4018 ); switch_to_serena( npc, pc, 250)");
                    serena_see_tarah(npc, pc);
                    Sound(4018);
                    switch_to_serena(npc, pc, 250);
                    ;
                    break;
                case 100:
                    Trace.Assert(originalScript == "game.global_vars[992] = 2");
                    SetGlobalVar(992, 2);
                    break;
                case 101:
                    Trace.Assert(originalScript == "riana_see_tarah( npc, pc ); game.sound( 4018 ); switch_to_riana( npc, pc, 200)");
                    riana_see_tarah(npc, pc);
                    Sound(4018);
                    switch_to_riana(npc, pc, 200);
                    ;
                    break;
                case 111:
                case 112:
                    Trace.Assert(originalScript == "pc2_see_tarah( npc, pc ); game.sound( 4018 )");
                    pc2_see_tarah(npc, pc);
                    Sound(4018);
                    ;
                    break;
                case 121:
                    Trace.Assert(originalScript == "switch_to_kenan( npc, pc, 90)");
                    switch_to_kenan(npc, pc, 90);
                    break;
                case 131:
                    Trace.Assert(originalScript == "switch_to_sharar( npc, pc, 10)");
                    switch_to_sharar(npc, pc, 10);
                    break;
                case 141:
                    Trace.Assert(originalScript == "switch_to_gadham( npc, pc, 10)");
                    switch_to_gadham(npc, pc, 10);
                    break;
                case 151:
                    Trace.Assert(originalScript == "switch_to_abaddon( npc, pc, 10)");
                    switch_to_abaddon(npc, pc, 10);
                    break;
                case 161:
                    Trace.Assert(originalScript == "switch_to_gershom( npc, pc, 10)");
                    switch_to_gershom(npc, pc, 10);
                    break;
                case 171:
                case 173:
                    Trace.Assert(originalScript == "game.particles( 'cast-Necromancy-cast', npc ); game.sound( 4014 ); kill_pc_3( npc, pc )");
                    AttachParticles("cast-Necromancy-cast", npc);
                    Sound(4014);
                    kill_pc_3(npc, pc);
                    ;
                    break;
                case 172:
                    Trace.Assert(originalScript == "game.particles( 'cast-Necromancy-cast', npc ); game.sound( 4014 ); kill_pc_4( npc, pc )");
                    AttachParticles("cast-Necromancy-cast", npc);
                    Sound(4014);
                    kill_pc_4(npc, pc);
                    ;
                    break;
                case 174:
                    Trace.Assert(originalScript == "game.particles( 'cast-Necromancy-cast', npc ); game.sound( 4014 ); kill_pc_5( npc, pc )");
                    AttachParticles("cast-Necromancy-cast", npc);
                    Sound(4014);
                    kill_pc_5(npc, pc);
                    ;
                    break;
                case 175:
                    Trace.Assert(originalScript == "game.particles( 'cast-Necromancy-cast', npc ); game.sound( 4014 ); kill_pc_7( npc, pc )");
                    AttachParticles("cast-Necromancy-cast", npc);
                    Sound(4014);
                    kill_pc_7(npc, pc);
                    ;
                    break;
                case 176:
                    Trace.Assert(originalScript == "game.particles( 'cast-Necromancy-cast', npc ); game.sound( 4014 ); kill_pc_6( npc, pc )");
                    AttachParticles("cast-Necromancy-cast", npc);
                    Sound(4014);
                    kill_pc_6(npc, pc);
                    ;
                    break;
                case 181:
                    Trace.Assert(originalScript == "switch_to_gershom( npc, pc, 20)");
                    switch_to_gershom(npc, pc, 20);
                    break;
                case 191:
                case 371:
                    Trace.Assert(originalScript == "daniel_see_tarah( npc, pc ); game.sound( 4018 )");
                    daniel_see_tarah(npc, pc);
                    Sound(4018);
                    ;
                    break;
                case 201:
                    Trace.Assert(originalScript == "switch_to_daniel( npc, pc, 10)");
                    switch_to_daniel(npc, pc, 10);
                    break;
                case 231:
                case 241:
                case 391:
                case 401:
                    Trace.Assert(originalScript == "create_skel( npc, pc ); game.sound( 4015 )");
                    create_skel(npc, pc);
                    Sound(4015);
                    ;
                    break;
                case 251:
                case 421:
                    Trace.Assert(originalScript == "dom_mon_end( npc, pc ); game.sound( 4016 )");
                    dom_mon_end(npc, pc);
                    Sound(4016);
                    ;
                    break;
                case 260:
                    Trace.Assert(originalScript == "game.global_vars[992] = 5");
                    SetGlobalVar(992, 5);
                    break;
                case 261:
                    Trace.Assert(originalScript == "pishella_see_tarah( npc, pc ); game.sound( 4018 ); switch_to_pishella( npc, pc, 240)");
                    pishella_see_tarah(npc, pc);
                    Sound(4018);
                    switch_to_pishella(npc, pc, 240);
                    ;
                    break;
                case 270:
                    Trace.Assert(originalScript == "game.global_vars[992] = 6");
                    SetGlobalVar(992, 6);
                    break;
                case 271:
                    Trace.Assert(originalScript == "kella_see_tarah( npc, pc ); game.sound( 4018 ); switch_to_kella( npc, pc, 230)");
                    kella_see_tarah(npc, pc);
                    Sound(4018);
                    switch_to_kella(npc, pc, 230);
                    ;
                    break;
                case 281:
                    Trace.Assert(originalScript == "switch_to_meleny( npc, pc, 350)");
                    switch_to_meleny(npc, pc, 350);
                    break;
                case 282:
                    Trace.Assert(originalScript == "switch_to_riana( npc, pc, 210)");
                    switch_to_riana(npc, pc, 210);
                    break;
                case 283:
                    Trace.Assert(originalScript == "switch_to_fruella( npc, pc, 420)");
                    switch_to_fruella(npc, pc, 420);
                    break;
                case 284:
                    Trace.Assert(originalScript == "switch_to_serena( npc, pc, 260)");
                    switch_to_serena(npc, pc, 260);
                    break;
                case 285:
                    Trace.Assert(originalScript == "switch_to_pishella( npc, pc, 320)");
                    switch_to_pishella(npc, pc, 320);
                    break;
                case 286:
                    Trace.Assert(originalScript == "switch_to_kella( npc, pc, 240)");
                    switch_to_kella(npc, pc, 240);
                    break;
                case 290:
                case 430:
                    Trace.Assert(originalScript == "npc.cast_spell( spell_improved_invisibility, npc )");
                    npc.CastSpell(WellKnownSpells.ImprovedInvisibility, npc);
                    break;
                case 291:
                case 292:
                case 431:
                case 432:
                    Trace.Assert(originalScript == "destroy_skel( npc, pc ); game.sound( 4015 )");
                    destroy_skel(npc, pc);
                    Sound(4015);
                    ;
                    break;
                case 311:
                    Trace.Assert(originalScript == "switch_to_daniel( npc, pc, 1)");
                    switch_to_daniel(npc, pc, 1);
                    break;
                case 321:
                case 322:
                    Trace.Assert(originalScript == "switch_to_abaddon( npc, pc, 20)");
                    switch_to_abaddon(npc, pc, 20);
                    break;
                case 330:
                case 440:
                    Trace.Assert(originalScript == "npc.cast_spell( spell_lesser_globe_of_invulnerability, npc )");
                    npc.CastSpell(WellKnownSpells.LesserGlobeOfInvulnerability, npc);
                    break;
                case 331:
                    Trace.Assert(originalScript == "start_fight( npc, pc )");
                    start_fight(npc, pc);
                    break;
                case 441:
                    Trace.Assert(originalScript == "npc.attack( pc )");
                    npc.Attack(pc);
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

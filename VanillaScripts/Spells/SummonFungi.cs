
using System;
using System.Collections.Generic;
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
using SpicyTemple.Core.Ui;
using System.Linq;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts
{
    [SpellScript(606)]
    public class SummonFungi : BaseSpellScript
    {

        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("summon fungi OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-conjuration-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("summon fungi OnSpellEffect");
            spell.duration = 5;

            var dice = Dice.D4;

            var proto = dice.Roll();

            int proto_id; // DECL_PULL_UP
            if (proto == 1)
            {
                proto_id = 14281;

            }
            else if (proto == 2)
            {
                proto_id = 14284;

            }
            else if (proto == 3)
            {
                proto_id = 14283;

            }
            else
            {
                proto_id = 14277;

            }

            spell.SummonMonsters(true, proto_id);
            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("summon fungi OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("summon fungi OnEndSpellCast");
        }


    }
}
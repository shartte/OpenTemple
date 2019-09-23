
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
using SpicyTemple.Core.Systems.ObjScript;
using SpicyTemple.Core.Ui;
using System.Linq;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace Scripts.Spells
{
    [SpellScript(5)]
    public class AnalyzeDweomer : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Analyze Dweomer OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-divination-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Analyze Dweomer OnSpellEffect");
            var remove_list = new List<GameObjectBody>();
            spell.duration = spell.casterLevel * 10;
            if (spell.Targets.Length == 1 && spell.Targets[0].Object == spell.caster)
            {

            }
            else
            {
                if (spell.caster.GetMoney() >= 150000)
                {
                    foreach (var t in spell.Targets)
                    {
                        AttachParticles("sp-Identify", t.Object);
                        t.Object.IdentifyAll();
                    }

                }
                else
                {
                    spell.caster.FloatMesFileLine("mes/spell.mes", 16009);
                }

            }

            foreach (var target_item in spell.Targets)
            {
                remove_list.Add(target_item.Object);
            }

            spell.RemoveTargets(remove_list);
            if (Co8.find_spell_obj_with_flag(spell.caster, 6400, Co8SpellFlag.AnalyzeDweomer) == null)
            {
                var spell_obj = GameSystems.MapObject.CreateObject(6400, spell.caster.GetLocation());
                Co8.set_spell_flag(spell_obj, Co8SpellFlag.AnalyzeDweomer);
                spell_obj.AddConditionToItem("Skill Circumstance Bonus", (int) SkillId.spellcraft, 20);
                spell.caster.GetItem(spell_obj);
                spell.ClearTargets();
                spell.AddTarget(spell.caster);
                spell.caster.AddCondition("sp-Endurance", spell.spellId, spell.duration, 0);
            }
            else
            {
                spell.caster.FloatMesFileLine("mes/spell.mes", 16007);
                spell.EndSpell();
            }

        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Analyze Dweomer OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Analyze Dweomer OnEndSpellCast");
            Co8.destroy_spell_obj_with_flag(spell.caster, 6400, Co8SpellFlag.AnalyzeDweomer);
        }

    }
}

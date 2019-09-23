
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
    [SpellScript(299)]
    public class MassHeal : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Heal OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-conjuration-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Heal OnSpellEffect");
            var remove_list = new List<GameObjectBody>();
            var has_tensers_list = new List<GameObjectBody>();
            var is_tensers = 0;
            foreach (var target in spell.Targets)
            {
                is_tensers = check_for_tensers(target.Object);
                AttachParticles("sp-Heal", target.Object);
                remove_list.Add(target.Object);
                // check if target is undead
                if (target.Object.IsMonsterCategory(MonsterCategory.undead))
                {
                    // Harm target
                    target.Object.AddCondition("sp-Harm", spell.spellId, spell.duration, 0);
                }
                else
                {
                    // Heal undead
                    target.Object.AddCondition("sp-Heal", spell.spellId, spell.duration, 0);
                }

                if (is_tensers)
                {
                    replace_tensers(target.Object);
                    is_tensers = 0;
                }

            }

            spell.RemoveTargets(remove_list);
            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Heal OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Heal OnEndSpellCast");
        }
        public static bool check_for_tensers(GameObjectBody target)
        {
            if (Co8.find_spell_obj_with_flag(target, 6400, Co8SpellFlag.TensersTransformation) != null)
            {
                Co8.destroy_spell_obj_with_flag(target, 6400, Co8SpellFlag.TensersTransformation);
                return true;
            }
            else
            {
                return false;
            }

        }
        public static void replace_tensers(GameObjectBody target)
        {
            var spell_obj = GameSystems.MapObject.CreateObject(6400, target.GetLocation());
            Co8.set_spell_flag(spell_obj, Co8SpellFlag.TensersTransformation);
            spell_obj.AddConditionToItem("sp-Feeblemind", 0, 0, 0); // yeah it works, you can put spell effects on items and they will affect the holder
            spell_obj.AddConditionToItem("Saving Throw Resistance Bonus", 0, 9);
            spell_obj.AddConditionToItem("Saving Throw Resistance Bonus", 1, 4);
            spell_obj.AddConditionToItem("Saving Throw Resistance Bonus", 2, 4);
            spell_obj.AddConditionToItem("Attribute Enhancement Bonus", 0, -2); // divine power gives too much strength bonus
            spell_obj.AddConditionToItem("Attribute Enhancement Bonus", 1, 4);
            spell_obj.AddConditionToItem("Attribute Enhancement Bonus", 2, 4);
            spell_obj.AddConditionToItem("Amulet of Natural Armor", 4, 0);
            target.GetItem(spell_obj);
        }

    }
}

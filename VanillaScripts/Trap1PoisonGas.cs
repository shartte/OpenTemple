
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
    [ObjectScript(32003)]
    public class Trap1PoisonGas : BaseObjectScript
    {

        public override bool OnTrap(TrapSprungEvent trap, GameObjectBody triggerer)
        {
            AttachParticles(trap.Type.ParticleSystemId, trap.Object);
            foreach (var obj in ObjList.ListVicinity(triggerer.GetLocation(), ObjectListFilter.OLC_CRITTERS))
            {
                if ((obj.DistanceTo(trap.Object) <= 10))
                {
                    if ((obj.HasLineOfSight(trap.Object)))
                    {
                        foreach (var dmg in trap.Type.Damage)
                        {
                            if ((dmg.Type == DamageType.Poison))
                            {
                                if ((!obj.SavingThrow(15, SavingThrowType.Fortitude, D20SavingThrowFlag.POISON, trap.Object)))
                                {
                                    obj.AddCondition("Poisoned", dmg.Dice.Modifier, 0);
                                }

                            }
                            else
                            {
                                obj.Damage(trap.Object, dmg.Type, dmg.Dice);
                            }

                        }

                    }

                }

            }

            return SkipDefault;
        }


    }
}
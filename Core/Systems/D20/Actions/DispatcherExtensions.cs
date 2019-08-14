using System;
using System.Security.Cryptography.X509Certificates;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems.Spells;
using SpicyTemple.Core.Utils;

namespace SpicyTemple.Core.Systems.D20.Actions
{
    public static class DispatcherExtensions
    {
        public static MetaMagicData DispatchMetaMagicModify(this GameObjectBody critter, MetaMagicData data)
        {
            var dispatcher = critter.GetDispatcher();
            if (dispatcher == null)
            {
                return data;
            }

            var dispIo = EvtObjMetaMagic.Default;
            dispIo.mmData = data;
            dispatcher.Process(DispatcherType.MetaMagicMod, D20DispatcherKey.NONE, dispIo);
            return dispIo.mmData;
        }

        public static bool Dispatch64ImmunityCheck(this GameObjectBody critter, DispIoImmunity dispIo)
        {
            var dispatcher = critter.GetDispatcher();
            if (dispatcher == null)
            {
                return false;
            }

            dispatcher.Process(DispatcherType.SpellImmunityCheck, D20DispatcherKey.NONE, dispIo);

            return dispIo.returnVal != 0;
        }

        public static void DispatchSpellResistanceCasterLevelCheck(this GameObjectBody critter,
            GameObjectBody target, BonusList casterLvlBonus, SpellPacketBody spell)
        {
            var dispatcher = critter.GetDispatcher();
            if (dispatcher == null)
            {
                return;
            }

            var dispIo = EvtObjSpellTargetBonus.Default;
            dispIo.spellPkt = spell;
            dispIo.target = target;
            dispIo.bonusList = casterLvlBonus;
            dispatcher.Process(DispatcherType.SpellResistanceCasterLevelCheck, D20DispatcherKey.NONE, dispIo);
        }

        public static int Dispatch45SpellResistanceMod(this GameObjectBody critter, SpellEntry spell)
        {
            var dispatcher = critter.GetDispatcher();
            if (dispatcher == null)
            {
                return 0;
            }

            var dispIo = DispIoBonusAndSpellEntry.Default;
            dispIo.bonList = BonusList.Default;
            dispIo.spellEntry = spell;
            dispatcher.Process(DispatcherType.SpellResistanceMod, D20DispatcherKey.NONE, dispIo);

            return dispIo.bonList.OverallBonus;
        }

        public static int DispatchSpellDcBase(this GameObjectBody critter, SpellEntry spell)
        {
            var dispatcher = critter.GetDispatcher();
            if (dispatcher == null)
            {
                return 0;
            }

            var dispIo = DispIoBonusAndSpellEntry.Default;
            dispIo.bonList = BonusList.Default;
            dispIo.spellEntry = spell;
            dispatcher.Process(DispatcherType.SpellDcBase, D20DispatcherKey.NONE, dispIo);

            return dispIo.bonList.OverallBonus;
        }

        public static int DispatchSpellDcMod(this GameObjectBody critter, SpellEntry spell)
        {
            var dispatcher = critter.GetDispatcher();
            if (dispatcher == null)
            {
                return 0;
            }

            var dispIo = DispIoBonusAndSpellEntry.Default;
            dispIo.bonList = BonusList.Default;
            dispIo.spellEntry = spell;
            dispatcher.Process(DispatcherType.SpellDcMod, D20DispatcherKey.NONE, dispIo);

            return dispIo.bonList.OverallBonus;
        }

        [TempleDllLocation(0x1004dd00)]
        public static int DispatchForCritter(this GameObjectBody obj, DispIoBonusList eventObj, DispatcherType dispType,
            D20DispatcherKey dispKey)
        {
            if (obj == null || !obj.IsCritter())
            {
                return 0;
            }

            var dispatcher = obj.GetDispatcher();
            if (dispatcher == null)
            {
                return 0;
            }

            if (eventObj == null)
            {
                eventObj = DispIoBonusList.Default;
            }

            dispatcher.Process(dispType, dispKey, eventObj);

            return eventObj.bonlist.OverallBonus;
        }

        [TempleDllLocation(0x1004e7f0)]
        public static int Dispatch10AbilityScoreLevelGet(this GameObjectBody obj, Stat stat, DispIoBonusList arg)
        {
            return DispatchForCritter(obj, arg, DispatcherType.AbilityScoreLevel, (D20DispatcherKey) (stat + 1));
        }

        [TempleDllLocation(0x1004d620)]
        public static int Dispatch35CasterLevelModify(this GameObjectBody critter, SpellPacketBody spellPkt)
        {
            var dispatcher = critter.GetDispatcher();
            if (dispatcher == null)
            {
                return 0;
            }

            var dispIo = DispIoD20Query.Default;
            dispIo.return_val = spellPkt.casterLevel;
            dispIo.obj = spellPkt;

            dispatcher.Process(DispatcherType.BaseCasterLevelMod, D20DispatcherKey.NONE, dispIo);

            return dispIo.return_val;
        }

        public static int DispatchActionCostMod(this GameObjectBody critter,
            ActionCostPacket acp, TurnBasedStatus tbStatus, D20Action action)
        {
            var dispatcher = critter.GetDispatcher();
            if (dispatcher == null)
            {
                return acp.hourglassCost;
            }

            EvtObjActionCost dispIo = new EvtObjActionCost(acp, tbStatus, action);
            dispatcher.Process(DispatcherType.ActionCostMod, D20DispatcherKey.NONE, dispIo);
            return dispIo.acpCur.hourglassCost;
        }

        [TempleDllLocation(0x1004cfb0)]
        public static float Dispatch40GetMoveSpeedBase(this GameObjectBody critter, out BonusList bonusList,
            out float factor)
        {
            var dispatcher = critter.GetDispatcher();
            if (dispatcher != null)
            {
                var dispIo = DispIoMoveSpeed.Default;
                dispatcher.Process(DispatcherType.GetMoveSpeedBase, D20DispatcherKey.NONE, dispIo);

                bonusList = dispIo.bonlist;
                factor = dispIo.factor;
                return dispIo.bonlist.OverallBonus * dispIo.factor;
            }
            else
            {
                bonusList = BonusList.Default;
                factor = 1.0f;
                return 30.0f;
            }
        }

        [TempleDllLocation(0x1004d080)]
        public static float Dispatch41GetMoveSpeed(this GameObjectBody critter, out BonusList bonusList)
        {
            var dispatcher = critter.GetDispatcher();
            var dispIo = DispIoMoveSpeed.Default;
            if (dispatcher != null)
            {
                critter.Dispatch40GetMoveSpeedBase(out dispIo.bonlist, out dispIo.factor);
                dispatcher.Process(DispatcherType.GetMoveSpeed, D20DispatcherKey.NONE, dispIo);
                var movement = dispIo.bonlist.OverallBonus;
                if (movement < 0)
                {
                    movement = 0;
                }

                if (dispIo.factor < 0.0)
                {
                    dispIo.factor = 0.0f;
                }

                bonusList = dispIo.bonlist;

                if (dispIo.factor > 0.0f && dispIo.factor != 1.0f)
                {
                    // Only return full 5 foot increments
                    var remainingMovement = dispIo.factor * movement;
                    return MathF.Floor(remainingMovement / 5.0f) * 5.0f;
                }

                return movement;
            }
            else
            {
                bonusList = BonusList.Default;
                return 30.0f;
            }
        }

        [TempleDllLocation(0x1004d1d0)]
        public static float DispatchGetRunSpeed(this GameObjectBody critter, out BonusList bonusList)
        {
            return Dispatch41GetMoveSpeed(critter, out bonusList) * 4.0f;
        }

        [TempleDllLocation(0x1004ED70)]
        public static int dispatch1ESkillLevel(this GameObjectBody critter, SkillId skill, ref BonusList bonusList,
            GameObjectBody opposingObj, int flag)
        {
            var dispatcher = critter.GetDispatcher();
            if (dispatcher == null)
            {
                return 0;
            }

            DispIoObjBonus dispIO = DispIoObjBonus.Default;
            dispIO.flags = flag;
            dispIO.obj = opposingObj;
            dispIO.bonlist = bonusList;
            dispatcher.Process(DispatcherType.SkillLevel, (D20DispatcherKey) (skill + 20), dispIO);
            bonusList = dispIO.bonlist;
            return dispIO.bonlist.OverallBonus;
        }

        [TempleDllLocation(0x1004ED70)]
        public static int dispatch1ESkillLevel(this GameObjectBody critter, SkillId skill, GameObjectBody opposingObj,
            int flag)
        {
            var noBonus = BonusList.Default;
            return dispatch1ESkillLevel(critter, skill, ref noBonus, opposingObj, flag);
        }

        [TempleDllLocation(0x1004f330)]
        public static int DispatchProjectileCreated(this GameObjectBody attacker, GameObjectBody projectile, D20CAF caf)
        {
            DispIoAttackBonus dispIo = DispIoAttackBonus.Default;
            dispIo.attackPacket.dispKey = 1;
            dispIo.attackPacket.d20ActnType = D20ActionType.STANDARD_ATTACK;
            dispIo.attackPacket.attacker = attacker;
            GameObjectBody weapon;
            if (caf.HasFlag(D20CAF.SECONDARY_WEAPON))
            {
                weapon = GameSystems.Item.ItemWornAt(attacker, EquipSlot.WeaponSecondary);
            }
            else
            {
                weapon = GameSystems.Item.ItemWornAt(attacker, EquipSlot.WeaponPrimary);
            }

            if (weapon != null && weapon.type == ObjectType.weapon)
            {
                dispIo.attackPacket.weaponUsed = weapon;
            }

            dispIo.attackPacket.ammoItem = projectile;
            dispIo.attackPacket.flags = caf;

            return GameSystems.Stat.DispatchAttackBonus(attacker, null, ref dispIo, DispatcherType.ProjectileCreated,
                0);
        }

        [TempleDllLocation(0x1004f420)]
        public static int DispatchProjectileDestroyed(this GameObjectBody attacker, GameObjectBody projectile,
            D20CAF caf)
        {
            DispIoAttackBonus dispIo = DispIoAttackBonus.Default;
            dispIo.attackPacket.dispKey = 1;
            dispIo.attackPacket.d20ActnType = D20ActionType.STANDARD_ATTACK;
            dispIo.attackPacket.attacker = attacker;

            GameObjectBody weapon;
            if (caf.HasFlag(D20CAF.SECONDARY_WEAPON))
            {
                weapon = GameSystems.Item.GetItemAtInvIdx(attacker, 204);
            }
            else
            {
                weapon = GameSystems.Item.GetItemAtInvIdx(attacker, 203);
            }

            if (weapon != null && weapon.type == ObjectType.weapon)
            {
                dispIo.attackPacket.weaponUsed = weapon;
            }

            dispIo.attackPacket.ammoItem = projectile;
            dispIo.attackPacket.flags = caf;
            return GameSystems.Stat.DispatchAttackBonus(attacker, null, ref dispIo, DispatcherType.ProjectileDestroyed,
                0);
        }

        [TempleDllLocation(0x1004e990)]
        public static int DispatchGetToHitModifiersFromDefender(this GameObjectBody attacker, DispIoAttackBonus dispIo)
        {
            return GameSystems.Stat.DispatchAttackBonus(attacker, null, ref dispIo,
                DispatcherType.ToHitBonusFromDefenderCondition, 0);
        }

        [TempleDllLocation(0x1004e920)]
        public static int DispatchGetAcAdjustedByAttacker(this GameObjectBody a1, DispIoAttackBonus a2)
        {
            return GameSystems.Stat.DispatchAttackBonus(a1, null, ref a2, DispatcherType.AcModifyByAttacker, 0);
        }

        // TemplePlus extension
        public static void DispatchSpellDamage(this GameObjectBody obj, DamagePacket damage, GameObjectBody target, SpellPacketBody spellPkt)
        {
            var dispatcher = obj.GetDispatcher();
            if (dispatcher != null)
            {
                EvtObjDealingSpellDamage dispIo = new EvtObjDealingSpellDamage
                {
                    damage = damage,
                    spellPkt = spellPkt,
                    target = target
                };
                dispatcher.Process(DispatcherType.DealingDamageSpell, 0, dispIo);
            }
        }

        [TempleDllLocation(0x1004e940)]
        public static int DispatchToHitBonusBase(this GameObjectBody attacker)
        {
            var dispIo = DispIoAttackBonus.Default;
            return GameSystems.Stat.DispatchAttackBonus(attacker, null, ref dispIo, DispatcherType.ToHitBonusBase, D20DispatcherKey.NONE);
        }

        [TempleDllLocation(0x1004e940)]
        public static int DispatchToHitBonusBase(this GameObjectBody attacker, ref DispIoAttackBonus dispIo)
        {
            var dispKey = (D20DispatcherKey) dispIo.attackPacket.dispKey;
            return GameSystems.Stat.DispatchAttackBonus(attacker, null, ref dispIo, DispatcherType.ToHitBonusBase,
                dispKey);
        }

        [TempleDllLocation(0x1004ee50)]
        public static int DispatchGetLevel(this GameObjectBody critter, int classOffset, BonusList bonlist, GameObjectBody contextObj)
        {
            var dispatcher = critter.GetDispatcher();
            if (dispatcher != null)
            {
                DispIoObjBonus dispIo = DispIoObjBonus.Default;
                dispIo.bonlist = bonlist;
                dispIo.obj = contextObj;
                dispatcher.Process(DispatcherType.GetLevel, (D20DispatcherKey.CL_Level + classOffset), dispIo);
                return dispIo.bonOut.OverallBonus;
            }
            else
            {
                return 0;
            }
        }

        [TempleDllLocation(0x1004d270)]
        public static void DispatchDispelCheck(this GameObjectBody critter, DispIoDispelCheck dispIo)
        {
            var dispatcher = critter.GetDispatcher();
            dispatcher?.Process(DispatcherType.DispelCheck, D20DispatcherKey.NONE, dispIo);
        }

        [TempleDllLocation(0x1004d480)]
        public static int DispatchGetAbilityLoss(this GameObjectBody critter, DispIoAbilityLoss dispIo)
        {
            dispIo.flags |= 8;

            var dispatcher = critter.GetDispatcher();
            if (dispatcher != null)
            {
                dispatcher.Process(DispatcherType.GetAbilityLoss, D20DispatcherKey.NONE, dispIo);
                return dispIo.result;
            }
            else
            {
                return 0;
            }
        }

        [TempleDllLocation(0x1004d3e0)]
        public static D20DispatcherKey DispatchHasImmunityTrigger(this GameObjectBody critter,
            DispIoTypeImmunityTrigger dispIo)
        {
            var dispatcher = critter.GetDispatcher();
            if (dispatcher == null)
            {
                return D20DispatcherKey.NONE;
            }

            for (var i = D20DispatcherKey.IMMUNITY_SPELL; i <= D20DispatcherKey.IMMUNITY_SPECIAL; i++)
            {
                dispatcher.Process(DispatcherType.ImmunityTrigger, i, dispIo);
                if (dispIo.interrupt == 1)
                {
                    return i;
                }
            }

            return D20DispatcherKey.NONE;
        }

        [TempleDllLocation(0x1004d500)]
        public static Dice DispatchGetAttackDice(this GameObjectBody critter, DispIoAttackDice dispIo)
        {
            var dispatcher = critter.GetDispatcher();
            if (dispatcher != null)
            {
                var weapon = dispIo.weapon;
                if (weapon != null)
                {
                    var damageDice = Dice.Unpack(weapon.GetUInt32(obj_f.weapon_damage_dice));
                    dispIo.dicePacked = damageDice;
                    dispIo.attackDamageType = (DamageType) weapon.GetInt32(obj_f.weapon_attacktype);
                }
                dispatcher.Process(DispatcherType.GetAttackDice, D20DispatcherKey.NONE, dispIo);

                var bonus = dispIo.bonlist.OverallBonus;
                return dispIo.dicePacked.WithAdjustedModifer(bonus);
            }

            return Dice.Zero;
        }

        [TempleDllLocation(0x1004eaa0)]
        public static void DispatchHealing(this GameObjectBody critter, DispIoDamage dispIo)
        {
            var dispatcher = critter.GetDispatcher();
            dispatcher?.Process(DispatcherType.ReceiveHealing, D20DispatcherKey.NONE, dispIo);
        }

    }
}
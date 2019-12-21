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
using SpicyTemple.Core.Startup.Discovery;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace SpicyTemple.Core.Systems.D20.Conditions.TemplePlus
{
    public static class TurnUndead
    {

        // Called when a turn undead is performed:  The data is the turn undead type
        public static void TurnUndeadPerform(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Signal();
            if (dispIo.data1 == 7)
            {
                // Greater Turning:  Also deduct a turn undead charge
                var NewCharges = evt.GetConditionArg2() - 1;
                if (NewCharges > 0)
                {
                    evt.SetConditionArg2(NewCharges);
                }
                else
                {
                    evt.SetConditionArg2(0);
                }
            }
            else if (dispIo.data1 == 0)
            {
                // Standard Turn Undead
                var charges = evt.GetConditionArg2();
                if (charges == 0)
                {
                    evt.objHndCaller.D20SendSignal("Deduct Greater Turn Undead Charge"); // Zero out greater turning
                }
            }
        }

        // Gets the number of turn undead charges
        public static void GetTurnUndeadCharges(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            dispIo.resultData = (ulong) evt.GetConditionArg2();
        }

        // Called to deduct a turn undead charge
        public static void DeductTurnUndeadCharge(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Signal();
            // Deduct one turn undead charge
            var NewCharges = evt.GetConditionArg2() - 1;
            if (NewCharges > 0)
            {
                evt.SetConditionArg2(NewCharges);
            }
            else
            {
                evt.SetConditionArg2(0);
                evt.objHndCaller.D20SendSignal("Deduct Greater Turn Undead Charge"); // Zero out greater turning
            }
        }

        // Note:  Arg 0 = turn undead type, Arg 1 =  is the number of chrages
        [AutoRegister]
        public static readonly ConditionSpec TurnUndeadExtension = ConditionSpec.Extend(DomainConditions.TurnUndead)
            .AddSignalHandler("Turn Undead Perform", TurnUndeadPerform)
            .AddSignalHandler("Deduct Turn Undead Charge", DeductTurnUndeadCharge)
            .AddQueryHandler("Turn Undead Charges", GetTurnUndeadCharges)
            .Build();

        public static int GetTurnUndeadCharges(this GameObjectBody critter)
        {
            return (int) GameSystems.D20.D20QueryReturnData(critter, "Turn Undead Charges");
        }
    }
}
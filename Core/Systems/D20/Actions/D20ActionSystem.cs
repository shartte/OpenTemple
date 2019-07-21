using System;
using System.Collections.Generic;
using System.Linq;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems.Anim;
using SpicyTemple.Core.Systems.D20.Conditions;
using SpicyTemple.Core.Systems.Feats;
using SpicyTemple.Core.Systems.ObjScript;
using SpicyTemple.Core.Systems.Pathfinding;
using SpicyTemple.Core.Systems.Spells;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Ui.InGameSelect;

namespace SpicyTemple.Core.Systems.D20.Actions
{
    public enum CursorType
    {
        Sword = 1,
        Arrow = 2,
        FeetGreen = 3,
        FeetYellow = 4,
        SlidePortraits = 5,
        Locked = 6,
        HaveKey = 7,
        UseSkill = 8,
        UsePotion = 9,
        UseSpell = 10,
        UseTeleportIcon = 11,
        HotKeySelection = 12,
        Talk = 13,
        IdentifyCursor = 14,
        IdentifyCursor2 = 15,
        ArrowInvalid = 16,
        SwordInvalid = 17,
        ArrowInvalid2 = 18,
        FeetRed = 19,
        FeetRed2 = 20,
        InvalidSelection = 21,
        Locked2 = 22,
        HaveKey2 = 23,
        UseSkillInvalid = 24,
        UsePotionInvalid = 25,
        UseSpellInvalid = 26,
        PlaceFlag = 27,
        HotKeySelectionInvalid = 28,
        InvalidSelection2 = 29,
        InvalidSelection3 = 30,
        InvalidSelection4 = 31,
        AttackOfOpportunity = 32,
        AttackOfOpportunityGrey = 33,
    }

    public class D20ActionSystem : IDisposable
    {
        public const int INV_IDX_INVALID = -1;

        private static readonly ILogger Logger = new ConsoleLogger();

        private static readonly Dictionary<CursorType, string> CursorPaths = new Dictionary<CursorType, string>
        {
            {CursorType.AttackOfOpportunity, "art/interface/combat_ui/attack-of-opportunity.tga"},
            {CursorType.AttackOfOpportunityGrey, "art/interface/combat_ui/attack-of-opportunity-grey.tga"},
            {CursorType.Sword, "art/interface/cursors/sword.tga"},
            {CursorType.Arrow, "art/interface/cursors/arrow.tga"},
            {CursorType.FeetGreen, "art/interface/cursors/feet_green.tga"},
            {CursorType.FeetYellow, "art/interface/cursors/feet_yellow.tga"},
            {CursorType.SlidePortraits, "art/interface/cursors/SlidePortraits.tga"},
            {CursorType.Locked, "art/interface/cursors/Locked.tga"},
            {CursorType.HaveKey, "art/interface/cursors/havekey.tga"},
            {CursorType.UseSkill, "art/interface/cursors/useskill.tga"},
            {CursorType.UsePotion, "art/interface/cursors/usepotion.tga"},
            {CursorType.UseSpell, "art/interface/cursors/usespell.tga"},
            {CursorType.UseTeleportIcon, "art/interface/cursors/useteleporticon.tga"},
            {CursorType.HotKeySelection, "art/interface/cursors/hotkeyselection.tga"},
            {CursorType.Talk, "art/interface/cursors/talk.tga"},
            {CursorType.IdentifyCursor, "art/interface/cursors/IdentifyCursor.tga"},
            {CursorType.IdentifyCursor2, "art/interface/cursors/IdentifyCursor.tga"},
            {CursorType.ArrowInvalid, "art/interface/cursors/arrow_invalid.tga"},
            {CursorType.SwordInvalid, "art/interface/cursors/sword_invalid.tga"},
            {CursorType.ArrowInvalid2, "art/interface/cursors/arrow_invalid.tga"},
            {CursorType.FeetRed, "art/interface/cursors/feet_red.tga"},
            {CursorType.FeetRed2, "art/interface/cursors/feet_red.tga"},
            {CursorType.InvalidSelection, "art/interface/cursors/InvalidSelection.tga"},
            {CursorType.Locked2, "art/interface/cursors/locked.tga"},
            {CursorType.HaveKey2, "art/interface/cursors/havekey.tga"},
            {CursorType.UseSkillInvalid, "art/interface/cursors/useskill_invalid.tga"},
            {CursorType.UsePotionInvalid, "art/interface/cursors/usepotion_invalid.tga"},
            {CursorType.UseSpellInvalid, "art/interface/cursors/usespell_invalid.tga"},
            {CursorType.PlaceFlag, "art/interface/cursors/placeflagcursor.tga"},
            {CursorType.HotKeySelectionInvalid, "art/interface/cursors/hotkeyselection_invalid.tga"},
            {CursorType.InvalidSelection2, "art/interface/cursors/invalidSelection.tga"},
            {CursorType.InvalidSelection3, "art/interface/cursors/invalidSelection.tga"},
            {CursorType.InvalidSelection4, "art/interface/cursors/invalidSelection.tga"},
        };

        private readonly Dictionary<int, PythonActionSpec> _pythonActions = new Dictionary<int, PythonActionSpec>();

        public D20Action globD20Action = new D20Action();

        private D20DispatcherKey globD20ActionKey;

        [TempleDllLocation(0x118A09A0)]
        private List<ActionSequence> actSeqArray = new List<ActionSequence>();

        [TempleDllLocation(0x118CD574)]
        private ActionSequence actSeqInterrupt;

        [TempleDllLocation(0x1186A8F0)]
        internal ActionSequence CurrentSequence { get; private set; }

        [TempleDllLocation(0x118CD2A0)]
        internal int actSeqTargetsIdx { get; set; }

        [TempleDllLocation(0x118CD2A8)]
        internal List<GameObjectBody> actSeqTargets;

        [TempleDllLocation(0x118CD3A8)]
        internal LocAndOffsets actSeqSpellLoc;

        [TempleDllLocation(0x118CD400)]
        private D20Action actSeqPickerAction;

        [TempleDllLocation(0x10B3D5A0)]
        internal bool actSeqPickerActive { get; private set; }

        [TempleDllLocation(0x1186A900)]
        private List<ReadiedActionPacket> _readiedActions = new List<ReadiedActionPacket>();

        [TempleDllLocation(0x10B3BF48)]
        private Dictionary<int, string> _translations;

        /// <summary>
        /// This is from vanilla. In general we could go as high as we want.
        /// </summary>
        private const int MaxSimultPerformers = 8;

        [TempleDllLocation(0x10B3D5B8)]
        private int numSimultPerformers;

        [TempleDllLocation(0x118A06C0)]
        private List<GameObjectBody> _simultPerformerQueue = new List<GameObjectBody>();

        [TempleDllLocation(0x10B3D5BC)]
        private int simulsIdx;

        [TempleDllLocation(0x118CD3C0)]
        private TurnBasedStatus tbStatus118CD3C0;

        [TempleDllLocation(0x10B3D59C)]
        private int seqSthg_10B3D59C;

        [TempleDllLocation(0x10092800)]
        public D20ActionSystem()
        {
            _translations = Tig.FS.ReadMesFile("mes/action.mes");
            Stub.TODO();
        }

        [TempleDllLocation(0x10089ef0)]
        public void Dispose()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10097be0)]
        public void ActionSequencesResetOnCombatEnd()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10095fd0)]
        public bool TurnBasedStatusInit(GameObjectBody actor)
        {
            if (GameSystems.Combat.IsCombatActive())
            {
                return GameSystems.D20.Initiative.CurrentActor == actor;
            }

            if (IsCurrentlyPerforming(actor))
            {
                return false;
            }

            globD20ActnSetPerformer(actor);

            CurrentSequence = null;
            AssignSeq(actor);
            var curSeq = CurrentSequence;

            var tbStatus = new TurnBasedStatus();
            curSeq.tbStatus = tbStatus;
            tbStatus.hourglassState = HourglassState.FULL;
            tbStatus.tbsFlags = default;
            tbStatus.idxSthg = -1;
            tbStatus.baseAttackNumCode = 0;
            tbStatus.attackModeCode = 0;
            tbStatus.numBonusAttacks = 0;
            tbStatus.numAttacks = 0;
            tbStatus.errCode = 0;
            tbStatus.surplusMoveDistance = 0;

            var dispIOtB = new DispIOTurnBasedStatus();
            dispIOtB.tbStatus = tbStatus;
            dispatchTurnBasedStatusInit(actor, dispIOtB);
            curSeq.seqOccupied &= ~SequenceFlags.PERFORMING; // unset "occupied" byte flag
            return true;
        }

        [TempleDllLocation(0x10094eb0)]
        public void AssignSeq(GameObjectBody performer)
        {
            var prevSeq = CurrentSequence;
            AllocSeq(performer);
            if (GameSystems.Combat.IsCombatActive())
            {
                if (prevSeq != null)
                {
                    Logger.Debug("Pushing sequence from {0} to {1}", prevSeq.performer, performer);
                }
                else
                {
                    Logger.Debug("Allocated sequence for {0}", performer);
                }
            }

            CurrentSequence.prevSeq = prevSeq;
            CurrentSequence.IsPerforming = true;
        }

        [TempleDllLocation(0x10094e20)]
        private void AllocSeq(GameObjectBody performer)
        {
            var newSequence = new ActionSequence();
            actSeqArray.Add(newSequence);
            CurrentSequence = newSequence;
            if (GameSystems.Combat.IsCombatActive())
                Logger.Debug("AllocSeq: \t Sequence Allocate[{0}]({1}): Resetting Sequence. ", actSeqArray.Count,
                    performer);
            CurSeqReset(performer);
        }

        [TempleDllLocation(0x1008a530)]
        public void globD20ActnSetPerformer(GameObjectBody performer)
        {
            if (performer != globD20Action.d20APerformer)
            {
                seqPickerTargetingType = D20TargetClassification.Invalid;
                seqPickerD20ActnType = D20ActionType.UNSPECIFIED_ATTACK;
                seqPickerD20ActnData1 = 0;
            }

            globD20Action.d20APerformer = performer;
        }

        [TempleDllLocation(0x10089f70)]
        public TurnBasedStatus curSeqGetTurnBasedStatus()
        {
            throw new NotImplementedException();
            return null;
        }

        // describes the new hourglass state when current state is i after doing an action that costs j
        [TempleDllLocation(0x102CC538)]
        private static readonly int[,] TurnBasedStatusTransitionMatrix = new int[7, 5]
        {
            {0, -1, -1, -1, -1},
            {1, 0, -1, -1, -1},
            {2, 0, 0, -1, -1},
            {3, 0, 0, 0, -1},
            {4, 2, 1, -1, 0},
            {5, 2, 2, -1, 0},
            {6, 5, 2, -1, 3}
        };

        [TempleDllLocation(0x1008b020)]
        public HourglassState GetHourglassTransition(HourglassState hourglassCurrent, int hourglassCost)
        {
            if (hourglassCurrent == HourglassState.INVALID)
            {
                return hourglassCurrent;
            }

            return (HourglassState) TurnBasedStatusTransitionMatrix[(int) hourglassCurrent, hourglassCost];
        }

        // initializes the sequence pointed to by actSeqCur and assigns it to objHnd
        [TempleDllLocation(0x10094A00)]
        public void CurSeqReset(GameObjectBody performer)
        {
            var curSeq = CurrentSequence;

            // release path finding queries
            foreach (var action in curSeq.d20ActArray)
            {
                ReleasePooledPathQueryResult(ref action.path);
            }

            curSeq.d20ActArray.Clear();
            curSeq.d20aCurIdx = -1;
            curSeq.prevSeq = null;
            curSeq.interruptSeq = null;
            curSeq.seqOccupied = default;

            globD20ActnSetPerformer(performer);
            GlobD20ActnInit();
            curSeq.performer = performer;
            curSeq.targetObj = null;
            curSeq.performerLoc = performer.GetLocationFull();
            curSeq.ignoreLos = 0;
            performingDefaultAction = false;
        }

        [TempleDllLocation(0x100949e0)]
        public void GlobD20ActnInit()
        {
            globD20Action.Reset(globD20Action.d20APerformer);
        }

        [TempleDllLocation(0x10089f80)]
        public void GlobD20ActnSetTypeAndData1(D20ActionType type, int data1)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10097C20)]
        public ActionErrorCode ActionAddToSeq()
        {
            var curSeq = CurrentSequence;
            var d20ActnType = globD20Action.d20ActType;
            TurnBasedStatus tbStatus = curSeq.tbStatus;

            if (d20ActnType == D20ActionType.CAST_SPELL
                && curSeq.spellPktBody.spellEnum >= 600)
            {
                curSeq.tbStatus.tbsFlags |=
                    TurnBasedStatusFlags.CritterSpell; // perhaps bug that it's not affecting the local copy?? TODO
                curSeq.spellPktBody.spellEnumOriginal = curSeq.spellPktBody.spellEnum;
            }

            if (d20ActnType == D20ActionType.PYTHON_ACTION)
            {
                globD20ActionKey = (D20DispatcherKey) globD20Action.data1;
            }

            var actnCheckFunc = D20ActionDefs.GetActionDef(d20ActnType).actionCheckFunc;

            var actnCheckResult = ActionErrorCode.AEC_OK;
            if (actnCheckFunc != null)
            {
                actnCheckResult = actnCheckFunc(globD20Action, tbStatus);
            }

            if (GameSystems.Party.IsPlayerControlled(curSeq.performer))
            {
                if (actnCheckResult != ActionErrorCode.AEC_OK)
                {
                    if (actnCheckResult == ActionErrorCode.AEC_TARGET_INVALID)
                    {
                        ActSeqGetPicker();
                        return ActionErrorCode.AEC_TARGET_INVALID;
                    }

                    if (actnCheckResult != ActionErrorCode.AEC_TARGET_TOO_FAR)
                    {
                        var errorString = ActionErrorString(actnCheckResult);
                        GameSystems.TextFloater.FloatLine(curSeq.performer, TextFloaterCategory.Generic,
                            TextFloaterColor.Red, errorString);
                        return actnCheckResult;
                    }
                }
                else if (!TargetCheck(globD20Action))
                {
                    ActSeqGetPicker();
                    return ActionErrorCode.AEC_TARGET_INVALID;
                }
            }
            else
            {
                if (actnCheckResult == ActionErrorCode.AEC_OUT_OF_CHARGES)
                {
                    return actnCheckResult;
                }

                TargetCheck(globD20Action);
            }

            return AddActionToSequence(globD20Action, curSeq);
        }

        [TempleDllLocation(0x1008a100)]
        private ActionErrorCode AddActionToSequence(D20Action action, ActionSequence sequence)
        {
            var type = action.d20ActType;
            if (type == D20ActionType.NONE)
            {
                return ActionErrorCode.AEC_INVALID_ACTION;
            }

            var d20Def = D20ActionDefs.GetActionDef(type);
            actnProcState = d20Def.addToSeqFunc(action, sequence, sequence.tbStatus);

            // TODO: Why is this done if the addToSeqFunc might have failed???
            var curSeq = CurrentSequence;
            if (sequence.tbStatus.tbsFlags.HasFlag(TurnBasedStatusFlags.FullAttack))
            {
                for (var i = curSeq.d20aCurIdx + 1; i < curSeq.d20ActArray.Count; i++)
                {
                    curSeq.d20ActArray[i].d20Caf |= D20CAF.FULL_ATTACK;
                }
            }

            return actnProcState;
        }

        [TempleDllLocation(0x10096570)]
        private void ChooseTargetCallback(ref PickerResult result, object callbackArg)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x100977a0)]
        private void ActSeqGetPicker()
        {
            var tgtClassif = TargetClassification(globD20Action);

            var d20adflags = GetActionFlags(globD20Action.d20ActType);

            if (tgtClassif == D20TargetClassification.ItemInteraction)
            {
                if (d20adflags.HasFlag(D20ADF.D20ADF_UseCursorForPicking))
                {
                    seqPickerD20ActnType = globD20Action.d20ActType; // seqPickerD20ActnType
                    seqPickerD20ActnData1 = globD20Action.data1;
                    seqPickerTargetingType = D20TargetClassification.ItemInteraction;
                    return;
                }

                var actSeqPicker = new PickerArgs();
                actSeqPicker.flagsTarget = UiPickerFlagsTarget.None;
                actSeqPicker.modeTarget = UiPickerType.Single;
                actSeqPicker.incFlags = UiPickerIncFlags.UIPI_NonCritter;
                actSeqPicker.excFlags = UiPickerIncFlags.UIPI_None;
                actSeqPicker.callback = ChooseTargetCallback;
                actSeqPicker.caster = globD20Action.d20APerformer;
                actSeqPickerActive = true;
                GameUiBridge.ShowPicker(actSeqPicker);
                actSeqPickerAction = globD20Action;
                return;
            }

            if (tgtClassif == D20TargetClassification.SingleIncSelf)
            {
                if (d20adflags.HasFlag(D20ADF.D20ADF_UseCursorForPicking))
                {
                    seqPickerD20ActnType = globD20Action.d20ActType; //seqPickerD20ActnType
                    seqPickerD20ActnData1 = globD20Action.data1;
                    seqPickerTargetingType = D20TargetClassification.SingleIncSelf;
                    return;
                }

                var actSeqPicker = new PickerArgs();
                actSeqPicker.flagsTarget = UiPickerFlagsTarget.None;
                actSeqPicker.modeTarget = UiPickerType.Single;
                actSeqPicker.incFlags =
                    UiPickerIncFlags.UIPI_Self | UiPickerIncFlags.UIPI_Other | UiPickerIncFlags.UIPI_Dead;
                actSeqPicker.excFlags = UiPickerIncFlags.UIPI_NonCritter;
                actSeqPicker.callback = ChooseTargetCallback;
                actSeqPicker.spellEnum = 0;
                actSeqPicker.caster = globD20Action.d20APerformer;

                actSeqPickerActive = true;
                GameUiBridge.ShowPicker(actSeqPicker);
                actSeqPickerAction = globD20Action;
                return;
            }

            if (tgtClassif == D20TargetClassification.SingleExcSelf)
            {
                if (d20adflags.HasFlag(D20ADF.D20ADF_UseCursorForPicking))
                {
                    seqPickerD20ActnType = globD20Action.d20ActType;
                    seqPickerD20ActnData1 = globD20Action.data1;
                    seqPickerTargetingType = D20TargetClassification.SingleExcSelf;
                    return;
                }

                var actSeqPicker = new PickerArgs();
                actSeqPicker.flagsTarget = UiPickerFlagsTarget.None;
                actSeqPicker.modeTarget = UiPickerType.Single;
                actSeqPicker.incFlags = UiPickerIncFlags.UIPI_Other | UiPickerIncFlags.UIPI_Dead;
                actSeqPicker.excFlags = UiPickerIncFlags.UIPI_Self | UiPickerIncFlags.UIPI_NonCritter;
                actSeqPicker.callback = ChooseTargetCallback;
                actSeqPicker.spellEnum = 0;
                actSeqPicker.caster = globD20Action.d20APerformer;
                actSeqPickerActive = true;
                GameUiBridge.ShowPicker(actSeqPicker);
                actSeqPickerAction = globD20Action;
                return;
            }

            if (tgtClassif == D20TargetClassification.CallLightning)
            {
                var actSeqPicker = new PickerArgs();
                if (globD20Action.d20ActType == D20ActionType.SPELL_CALL_LIGHTNING)
                {
                    var callLightningId = (int) GameSystems.D20.D20QueryReturnData(globD20Action.d20APerformer,
                        D20DispatcherKey.QUE_Critter_Can_Call_Lightning);
                    var spellPkt = GameSystems.Spell.GetActiveSpell(callLightningId);
                    var baseCasterLevelMod = globD20Action.d20APerformer.Dispatch35CasterLevelModify(spellPkt);
                    actSeqPicker.range = GameSystems.Spell.GetSpellRangeExact(SpellRangeType.SRT_Medium,
                        baseCasterLevelMod, globD20Action.d20APerformer);
                    actSeqPicker.radiusTarget = 5;
                }
                else
                {
                    actSeqPicker.range = 0;
                    actSeqPicker.radiusTarget = 0;
                }

                actSeqPicker.flagsTarget = UiPickerFlagsTarget.Range;
                actSeqPicker.modeTarget = UiPickerType.Area;
                actSeqPicker.incFlags = UiPickerIncFlags.UIPI_Self | UiPickerIncFlags.UIPI_Other;
                actSeqPicker.excFlags = UiPickerIncFlags.UIPI_Dead | UiPickerIncFlags.UIPI_NonCritter;
                actSeqPicker.callback = ChooseTargetCallback;
                actSeqPicker.spellEnum = 0;
                actSeqPicker.caster = globD20Action.d20APerformer;
                actSeqPickerActive = true;
                GameUiBridge.ShowPicker(actSeqPicker);
                actSeqPickerAction = globD20Action;
                return;
            }

            if (tgtClassif == D20TargetClassification.CastSpell)
            {
                var spellEnum = globD20Action.d20SpellData.SpellEnum;
                var metaMagicData = globD20Action.d20SpellData.metaMagicData;

                //Modify metamagic data for enlarge and widen
                metaMagicData = globD20Action.d20APerformer.DispatchMetaMagicModify(metaMagicData);

                var curSeq = CurrentSequence;
                curSeq.spellPktBody.spellRange *= metaMagicData.metaMagicEnlargeSpellCount + 1;
                var spellEntry = GameSystems.Spell.GetSpellEntry(spellEnum);
                var radiusTarget = spellEntry.radiusTarget * metaMagicData.metaMagicWidenSpellCount + 1;
                var pickArgs = new PickerArgs();
                GameSystems.Spell.PickerArgsFromSpellEntry(spellEntry, pickArgs, curSeq.spellPktBody.caster,
                    curSeq.spellPktBody.casterLevel, radiusTarget);
                pickArgs.spellEnum = spellEnum;
                pickArgs.callback = (ref PickerResult result, object cbArgs) =>
                {
                    SpellPickerCallback(ref result, curSeq);
                };

                // Modify the PickerArgs
                if (globD20Action.d20ActType == D20ActionType.PYTHON_ACTION)
                {
                    GameSystems.Script.Actions.ModifyPicker(globD20Action.data1, pickArgs);
                }

                actSeqPickerActive = true;
                GameUiBridge.ShowPicker(pickArgs);
                actSeqPickerAction = globD20Action;
            }
        }

        [TempleDllLocation(0x10090af0)]
        private void ActSeqTargetsClear()
        {
            actSeqTargetsIdx = -1;
            actSeqTargets.Clear();
            actSeqSpellLoc = LocAndOffsets.Zero;
        }

        [TempleDllLocation(0x10096cc0)]
        private void SpellPickerCallback(ref PickerResult result, ActionSequence sequence)
        {
            if (result.flags == default)
            {
                actSeqPickerActive = true;
                return;
            }

            actSeqPickerActive = false;

            if (result.flags.HasFlag(PickerResultFlags.PRF_CANCELLED))
            {
                GameUiBridge.FreeCurrentPicker();
                CurrentSequence?.spellPktBody.Reset();
                ActSeqTargetsClear();
                GlobD20ActnInit();
                return;
            }

            Logger.Info("SpellPickerCallback(): Resetting sequence");
            var performer = actSeqPickerAction.d20APerformer;

            if (!SequenceSwitch(performer) && CurrentSequence.IsPerforming)
                return;

            CurSeqReset(performer);
            globD20Action = actSeqPickerAction;

            var spellPacket = sequence.spellPktBody;
            var d20a = sequence.d20Action;
            if (result.flags.HasFlag(PickerResultFlags.PRF_HAS_SINGLE_OBJ))
            {
                spellPacket.targetListHandles = new[] {result.handle};
                spellPacket.orgTargetCount = 1;
                d20a.d20ATarget = result.handle;
                d20a.destLoc = result.handle.GetLocationFull();
            }
            else
            {
                spellPacket.targetListHandles = Array.Empty<GameObjectBody>();
                spellPacket.orgTargetCount = 0;
            }

            if (result.flags.HasFlag(PickerResultFlags.PRF_HAS_MULTI_OBJ))
            {
                spellPacket.targetListHandles = result.objList.ToArray();
                spellPacket.orgTargetCount = spellPacket.targetCount;
            }

            if (result.flags.HasFlag(PickerResultFlags.PRF_HAS_LOCATION))
            {
                spellPacket.aoeCenter = result.location;
                spellPacket.aoeCenterZ = result.offsetz;
                d20a.destLoc = result.location;
            }
            else
            {
                spellPacket.aoeCenter = LocAndOffsets.Zero;
                spellPacket.aoeCenterZ = 0.0f;
                d20a.destLoc = LocAndOffsets.Zero;
            }

            if (result.flags.HasFlag(PickerResultFlags.PRF_UNK8))
            {
                Logger.Warn("SpellPickerCallback: not implemented - BECAME_TOUCH_ATTACK");
            }

            spellPacket.pickerResult = result;
            GameUiBridge.FreeCurrentPicker();
            ActionAddToSeq();
            sequencePerform();
            ActSeqTargetsClear();
        }

        [TempleDllLocation(0x1008b140)]
        private bool SequenceSwitch(GameObjectBody performer)
        {
            int seqIdx = -1;
            for (int i = 0; i < actSeqArray.Count; i++)
            {
                var seq = actSeqArray[i];
                if (seq.IsPerforming)
                {
                    if (seq.performer == performer)
                    {
                        seqIdx = i;
                    }
                }
                else
                {
                    if (seq.prevSeq != null && seq.prevSeq.performer == performer)
                        return false;
                    if (seq.interruptSeq != null)
                    {
                        if (seq.interruptSeq.performer == performer)
                            return false;
                    }
                }
            }

            if (seqIdx >= 0)
            {
                Logger.Debug("SequenceSwitch: \t doing for {0}. Previous Current Seq: {1}", performer, CurrentSequence);
                CurrentSequence = actSeqArray[seqIdx];
                Logger.Debug("SequenceSwitch: \t new Current Seq: {0}", CurrentSequence);
                return true;
            }

            return false;
        }

        [TempleDllLocation(0x1008a1b0)]
        private string ActionErrorString(ActionErrorCode actionErrorCode)
        {
            return _translations[1000 + (int) actionErrorCode];
        }

        [TempleDllLocation(0x100961C0)]
        public void sequencePerform()
        {
            // check if OK to perform
            if (actSeqPickerActive)
            {
                // should solve the issue when casting a spell in the presence of prebuffing NPCs
                // (i.e non-party members can now cast spells while the player's picker is active, so there shouldn't
                // be a lingering spell cast sequence fucking your shit up)
                if (CurrentSequence == null || GameSystems.Party.IsInParty(CurrentSequence.performer))
                    return;
            }

            // is curSeq ok to perform?
            if (!actSeqOkToPerform())
            {
                Logger.Debug("SequencePerform: \t Sequence given while performing previous action - aborted.");
                GlobD20ActnInit();
                return;
            }

            // try to perform the sequence and its actions
            ActionSequence curSeq = CurrentSequence;
            if (GameSystems.Combat.IsCombatActive() || !ShouldTriggerCombat(curSeq) || !combatTriggerSthg(curSeq))
            {
                if (curSeq != CurrentSequence)
                {
                    Logger.Debug("SequencePerform: Switched sequence slot from combat trigger!");
                    curSeq = CurrentSequence;
                }

                Logger.Debug("SequencePerform: \t {0} performing sequence ({1})...",
                    GameSystems.MapObject.GetDisplayName(curSeq.performer), curSeq);
                if (isSimultPerformer(curSeq.performer))
                {
                    Logger.Debug("simultaneously...");
                    if (!simulsOk(curSeq))
                    {
                        if (simulsAbort(curSeq.performer))
                            Logger.Debug("sequence not allowed... aborting simuls (pending).");
                        else Logger.Debug("sequence not allowed... aborting subsequent simuls.");
                        return;
                    }

                    Logger.Debug("succeeded...");
                }
                else
                {
                    Logger.Debug("independently.");
                }

                actnProcState = ActionErrorCode.AEC_OK;
                curSeq.IsPerforming = true;
                ActionPerform();
                // I think actionPerform can modify the sequence, so better be safe
                for (curSeq = CurrentSequence; IsCurrentlyPerforming(curSeq.performer); curSeq = CurrentSequence)
                {
                    if (curSeq.IsPerforming)
                    {
                        var curIdx = curSeq.d20aCurIdx;
                        if (curIdx >= 0 && curIdx < curSeq.d20ActArrayNum)
                        {
                            var caflags = curSeq.d20ActArray[curIdx].d20Caf;
                            if (caflags.HasFlag(D20CAF.NEED_PROJECTILE_HIT) ||
                                caflags.HasFlag(D20CAF.NEED_ANIM_COMPLETED))
                            {
                                break;
                            }
                        }
                    }

                    ActionPerform();
                }
            }
        }

        [TempleDllLocation(0x100996e0)]
        private void ActionPerform()
        {
            // in principle this cycles through actions,
            // but if it succeeds in performing one it will return
            // note: the number of actions can generally change due to processing (e.g. if you cleave or incur AoOs)
            while (true)
            {
                var curSeq = CurrentSequence;
                ref var curIdx = ref curSeq.d20aCurIdx; // the action idx is inited to -1 for fresh sequences
                ++curIdx;

                void RemoveRemainingActions()
                {
                    curSeq.d20ActArray.RemoveRange(curSeq.d20aCurIdx, curSeq.d20ActArray.Count - curSeq.d20aCurIdx);
                }

                // if the performer has become unconscious, abort
                GameObjectBody performer = curSeq.performer;
                if (GameSystems.Critter.IsDeadOrUnconscious(performer))
                {
                    RemoveRemainingActions();
                    Logger.Debug("ActionPerform: \t Unconscious actor {0} - cutting sequence", performer);
                }

                // if have finished up the actions - do CurSeqNext
                if (curSeq.d20aCurIdx >= curSeq.d20ActArrayNum)
                    break;

                var tbStatus = curSeq.tbStatus;
                var d20a = curSeq.d20ActArray[curIdx];

                var errCode = CheckActionPreconditions(curSeq, curIdx, tbStatus);
                if (errCode != ActionErrorCode.AEC_OK)
                {
                    var errorText = ActionErrorString(errCode);

                    Logger.Debug("ActionPerform: \t Action unavailable for {0} ({1}): {2}",
                        GameSystems.MapObject.GetDisplayName(d20a.d20APerformer), d20a.d20APerformer, errorText);
                    actnProcState = errCode;
                    curSeq.tbStatus.errCode = errCode;
                    GameSystems.TextFloater.FloatLine(performer, TextFloaterCategory.Generic, TextFloaterColor.Red,
                        errorText);
                    RemoveRemainingActions();
                    break;
                }

                if (performingDefaultAction)
                {
                    if (d20a.d20ActType == D20ActionType.STANDARD_ATTACK ||
                        d20a.d20ActType == D20ActionType.STANDARD_RANGED_ATTACK)
                    {
                        if (d20a.d20ATarget != null && GameSystems.Critter.IsDeadOrUnconscious(d20a.d20ATarget))
                        {
                            performedDefaultAction = false;
                            RemoveRemainingActions();
                            //curSeq.tbStatus.tbsFlags |= TurnBasedStatusFlags.FullAttack;
                            break;
                        }
                    }

                    performedDefaultAction = true;
                }

                D20aTriggerCombatCheck(curSeq, curIdx);

                if (d20a.d20ActType == D20ActionType.AOO_MOVEMENT)
                {
                    if (CheckAooIncurRegardTumble(d20a))
                    {
                        DoAoo(d20a.d20APerformer, d20a.d20ATarget);
                        curSeq = CurrentSequence; // DoAoo updates the curSeq
                        curSeq.d20ActArray[curSeq.d20ActArrayNum - 1].d20Caf |= D20CAF.AOO_MOVEMENT;
                        sequencePerform();
                        return;
                    }
                }

                else
                {
                    if (D20ActionTriggersAoO(d20a, tbStatus) && DoAoosByAdjcentEnemies(d20a.d20APerformer))
                    {
                        Logger.Debug("ActionPerform: \t Sequence Preempted {0}", d20a.d20APerformer);
                        --curIdx;
                        sequencePerform();
                    }
                    else
                    {
                        curSeq.tbStatus = tbStatus;
                        curSeq.tbStatus.tbsFlags |= TurnBasedStatusFlags.HasActedThisRound;
                        InterruptCounterspell(d20a);
                        Logger.Debug("ActionPerform: \t Performing action for {0}: {1}", d20a.d20APerformer,
                            d20a.d20ActType);

                        D20ActionDefs.GetActionDef(d20a.d20ActType).performFunc(d20a);
                        InterruptNonCounterspell(d20a);
                    }

                    return;
                }
            }

            if (projectileCheckBeforeNextAction())
            {
                curSeqNext();
            }
        }

        [TempleDllLocation(0x10096450)]
        private ActionErrorCode CheckActionPreconditions(ActionSequence actSeq, int idx, TurnBasedStatus tbStat)
        {
            if (idx < 0 || idx >= actSeq.d20ActArrayNum)
            {
                return ActionErrorCode.AEC_INVALID_ACTION;
            }

            var action = actSeq.d20ActArray[idx];
            var performerLoc = action.d20APerformer.GetLocationFull();
            var actionDef = D20ActionDefs.GetActionDef(action.d20ActType);

            // target
            var tgtChecker = actionDef.tgtCheckFunc;
            if (tgtChecker != null)
            {
                var targetCheckResult = tgtChecker(action, tbStat);
                if (targetCheckResult != ActionErrorCode.AEC_OK)
                {
                    return targetCheckResult;
                }
            }

            // hourglass & action check
            var actionCheckResult = seqCheckAction(action, tbStat);
            if (actionCheckResult != ActionErrorCode.AEC_OK)
            {
                return actionCheckResult;
            }

            // location
            var locChecker = actionDef.locCheckFunc;
            if (locChecker != null)
            {
                var locCheckResult = locChecker(action, tbStat, performerLoc);
                if (locCheckResult != ActionErrorCode.AEC_OK)
                {
                    return locCheckResult;
                }
            }

            foreach (var d20Action in actSeq.d20ActArray)
            {
                var otherActionDef = D20ActionDefs.GetActionDef(d20Action.d20ActType);
                if (otherActionDef.flags.HasFlag(D20ADF.D20ADF_DoLocationCheckAtDestination))
                {
                    var path = action.path;
                    if (path != null)
                    {
                        performerLoc = path.to;
                    }

                    return ActionSequenceChecksRegardLoc(performerLoc, tbStat, idx + 1, actSeq);
                }
            }

            return ActionErrorCode.AEC_OK;
        }

        [TempleDllLocation(0x100960b0)]
        private ActionErrorCode ActionSequenceChecksRegardLoc(LocAndOffsets loc, TurnBasedStatus tbStatus,
            int startIndex, ActionSequence actSeq)
        {
            TurnBasedStatus tbStatCopy = tbStatus.Copy();
            LocAndOffsets locCopy = loc;
            ActionErrorCode result = ActionErrorCode.AEC_OK;

            for (int i = startIndex; i < actSeq.d20ActArrayNum; i++)
            {
                var d20a = actSeq.d20ActArray[i];
                var d20aType = d20a.d20ActType;
                var actionDef = D20ActionDefs.GetActionDef(d20aType);

                var tgtCheckFunc = actionDef.tgtCheckFunc;
                if (tgtCheckFunc != null)
                {
                    result = tgtCheckFunc(d20a, tbStatCopy);
                    if (result != ActionErrorCode.AEC_OK)
                    {
                        return result;
                    }
                }

                result = TurnBasedStatusUpdate(d20a, tbStatCopy);
                if (result != ActionErrorCode.AEC_OK)
                {
                    tbStatCopy.errCode = result; // ??? WTF maybe debug leftover
                    return result;
                }

                var actionCheckFunc = actionDef.actionCheckFunc;
                if (actionCheckFunc != null)
                {
                    result = actionCheckFunc(d20a, tbStatCopy);
                    if (result != ActionErrorCode.AEC_OK)
                    {
                        return result;
                    }
                }

                var locCheckFunc = actionDef.locCheckFunc;
                if (locCheckFunc != null)
                {
                    result = locCheckFunc(d20a, tbStatCopy, locCopy);
                    if (result != ActionErrorCode.AEC_OK)
                    {
                        return result;
                    }
                }

                var path = d20a.path;
                if (path != null)
                {
                    locCopy = path.to;
                }
            }

            return result;
        }

        [TempleDllLocation(0x10093890)]
        private bool combatTriggerSthg(ActionSequence actSeq)
        {
            var performer = actSeq.performer;
            // TODO: This is a rather incomplete copy, really...
            var seqCopy = actSeq.Copy();

            GameSystems.Combat.EnterCombat(performer);

            for (var i = 0; i < actSeq.d20ActArrayNum; i++)
            {
                D20aTriggerCombatCheck(actSeq, i);
            }

            GameSystems.Combat.StartCombat(performer, 1);

            if (GameSystems.Party.IsPlayerControlled(performer))
            {
                CurrentSequence = seqCopy;
                return false;
            }

            return true;
        }

        [TempleDllLocation(0x1008AE90)]
        private void D20aTriggerCombatCheck(ActionSequence actSeq, int idx)
        {
            var performer = actSeq.performer;
            var d20a = actSeq.d20ActArray[idx];
            var tgt = d20a.d20ATarget;
            var flags = d20a.GetActionDefinitionFlags();
            if (flags.HasFlag(D20ADF.D20ADF_TriggersCombat))
            {
                GameSystems.Combat.EnterCombat(actSeq.performer);
                if (tgt != null)
                {
                    GameSystems.Combat.EnterCombat(tgt);
                    GameSystems.AI.ProvokeHostility(performer, tgt, 1, 0);
                }
            }

            if (idx == 0 && (d20a.d20ActType == D20ActionType.CAST_SPELL || d20a.d20ActType == D20ActionType.USE_ITEM))
            {
                var spellPkt = actSeq.spellPktBody;
                var spEnum = spellPkt.spellEnum;
                if (spEnum == 0)
                    return;

                var caster = spellPkt.caster;
                foreach (var spTgt in spellPkt.targetListHandles)
                {
                    if (!spTgt.IsCritter())
                        continue;
                    if (GameSystems.Spell.IsSpellHarmful(spEnum, caster, spTgt))
                    {
                        GameSystems.Combat.EnterCombat(performer);
                        GameSystems.Combat.EnterCombat(spTgt);
                    }
                }
            }
        }

        [TempleDllLocation(0x1008ad10)]
        private bool ShouldTriggerCombat(ActionSequence actSeq)
        {
            // check if any of the actions triggers combat
            for (var i = 0; i < actSeq.d20ActArrayNum; i++)
            {
                var d20a = actSeq.d20ActArray[i];
                var flags = d20a.GetActionDefinitionFlags();
                if (flags.HasFlag(D20ADF.D20ADF_TriggersCombat))
                {
                    if (GameSystems.Party.IsInParty(actSeq.performer))
                        return true;
                    if (actSeq.targetObj != null && GameSystems.Party.IsInParty(actSeq.targetObj))
                        return true;
                }
            }

            // check spell targets
            var spPkt = actSeq.spellPktBody;
            if (spPkt.spellEnum == 0)
                return false;
            foreach (var spTgt in spPkt.targetListHandles)
            {
                if (spTgt == null)
                    continue;
                if (!spTgt.IsCritter())
                    continue;
                if (GameSystems.Spell.IsSpellHarmful(spPkt.spellEnum, spPkt.caster, spTgt))
                {
                    if (GameSystems.Party.IsInParty(spTgt) || GameSystems.Party.IsInParty(actSeq.performer))
                        return true;
                }
            }

            return false;
        }

        private bool isSimultPerformer(GameObjectBody objHnd)
        {
            return _simultPerformerQueue.Contains(objHnd);
        }

        private bool simulsOk(ActionSequence actSeq)
        {
            var numd20as = actSeq.d20ActArrayNum;
            if (numd20as <= 0)
            {
                return true;
            }

            var numStdAttkActns = 0;
            foreach (var d20a in actSeq.d20ActArray)
            {
                if (!d20a.GetActionDefinitionFlags().HasFlag(D20ADF.D20ADF_SimulsCompatible))
                {
                    ++numStdAttkActns;
                    if (numStdAttkActns >= numd20as)
                    {
                        return true;
                    }
                }
            }

            if (isSomeoneAlreadyActingSimult(actSeq.performer))
            {
                return false;
            }
            else
            {
                _simultPerformerQueue.Clear();
                Logger.Debug("first simul actor, proceeding");
            }

            return true;
        }

        private bool simulsAbort(GameObjectBody objHnd)
        {
            // aborts sequence; returns 1 if objHnd is not the first in queue
            if (!GameSystems.Combat.IsCombatActive()) return false;
            var isFirstInQueue = true;

            foreach (var simultPerformer in _simultPerformerQueue)
            {
                if (objHnd == simultPerformer)
                {
                    if (isFirstInQueue)
                    {
                        _simultPerformerQueue.Clear();
                        return false;
                    }
                    else
                    {
                        numSimultPerformers = simulsIdx;
                        tbStatus118CD3C0 = CurrentSequence.tbStatus.Copy();
                        Logger.Debug("Simul aborted {0} ({1})", objHnd, simulsIdx);
                        return true;
                    }
                }

                if (IsCurrentlyPerforming(simultPerformer))
                    isFirstInQueue = false;
            }

            return false;
        }

        private bool isSomeoneAlreadyActingSimult(GameObjectBody objHnd)
        {
            if (numSimultPerformers == 0)
            {
                return false;
            }

            foreach (var simultPerformer in _simultPerformerQueue)
            {
                if (objHnd == simultPerformer)
                {
                    return false;
                }

                foreach (var actionSequence in actSeqArray)
                {
                    if (actionSequence.IsPerforming && actionSequence.performer == simultPerformer)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        [TempleDllLocation(0x1008a980)]
        private bool actSeqOkToPerform()
        {
            var curSeq = CurrentSequence;
            if ((curSeq.IsPerforming) && (curSeq.d20aCurIdx >= 0) && curSeq.d20aCurIdx < curSeq.d20ActArrayNum)
            {
                var caflags = curSeq.d20ActArray[curSeq.d20aCurIdx].d20Caf;
                if (caflags.HasFlag(D20CAF.NEED_PROJECTILE_HIT))
                {
                    return false;
                }

                return !caflags.HasFlag(D20CAF.NEED_ANIM_COMPLETED);
            }

            return true;
        }

        [TempleDllLocation(0x10098c90)]
        public bool DoUseItemAction(GameObjectBody holder, GameObjectBody aiObj, GameObjectBody item)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10099cf0)]
        public void PerformOnAnimComplete(GameObjectBody obj, int uniqueId)
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x100933F0)]
        public void ActionFrameProcess(GameObjectBody obj)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10089f00)]
        public bool WasInterrupted(GameObjectBody obj)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1004e790)]
        public void dispatchTurnBasedStatusInit(GameObjectBody obj, DispIOTurnBasedStatus dispIo)
        {
            var dispatcher = obj.GetDispatcher();
            if (dispatcher != null)
            {
                dispatcher.Process(DispatcherType.TurnBasedStatusInit, D20DispatcherKey.NONE, dispIo);
                if (dispIo.tbStatus != null)
                {
                    if (dispIo.tbStatus.hourglassState > 0)
                    {
                        GameSystems.D20.D20SendSignal(obj, D20DispatcherKey.SIG_BeginTurn);
                    }
                }
            }
        }

        [TempleDllLocation(0x1004ED70)]
        public int dispatch1ESkillLevel(GameObjectBody critter, SkillId skill, ref BonusList bonusList,
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
        public int dispatch1ESkillLevel(GameObjectBody critter, SkillId skill, GameObjectBody opposingObj, int flag)
        {
            var noBonus = BonusList.Default;
            return dispatch1ESkillLevel(critter, skill, ref noBonus, opposingObj, flag);
        }

        [TempleDllLocation(0x10099b10)]
        public void ProjectileHit(GameObjectBody projectile, GameObjectBody critter)
        {
            throw new NotImplementedException();
        }


        [TempleDllLocation(0x118CD3B8)]
        internal D20TargetClassification seqPickerTargetingType; // init to -1

        [TempleDllLocation(0x118A0980)]
        internal D20ActionType seqPickerD20ActnType; // init to 1

        [TempleDllLocation(0x118CD570)]
        internal int seqPickerD20ActnData1; // init to 0

        [TempleDllLocation(0x1008a0f0)]
        public bool SeqPickerHasTargetingType()
        {
            return seqPickerTargetingType != D20TargetClassification.Invalid;
        }

        [TempleDllLocation(0x1008a0b0)]
        public void SeqPickerTargetingTypeReset()
        {
            seqPickerTargetingType = D20TargetClassification.Invalid;
            seqPickerD20ActnType = D20ActionType.UNSPECIFIED_ATTACK;
            seqPickerD20ActnData1 = 0;
        }

        [TempleDllLocation(0x1008a050)]
        public bool IsCurrentlyPerforming(GameObjectBody performer) => IsCurrentlyPerforming(performer, out _);

        [TempleDllLocation(0x1008a050)]
        public bool IsCurrentlyPerforming(GameObjectBody performer, out ActionSequence sequenceOut)
        {
            foreach (var actionSequence in actSeqArray)
            {
                if (actionSequence.performer == performer && actionSequence.IsPerforming)
                {
                    sequenceOut = actionSequence;
                    return true;
                }
            }

            sequenceOut = null;
            return false;
        }

        [TempleDllLocation(0x1008a4b0)]
        public D20TargetClassification TargetClassification(D20Action action)
        {
            // direct access to action flags - intentional
            var d20DefFlags = D20ActionDefs.GetActionDef(action.d20ActType).flags;

            if (d20DefFlags.HasFlag(D20ADF.D20ADF_Python))
            {
                return GameSystems.D20.Actions.GetPythonAction(action.data1).tgtClass;
            }

            if (d20DefFlags.HasFlag(D20ADF.D20ADF_Movement))
            {
                return D20TargetClassification.Movement;
            }

            if (d20DefFlags.HasFlag(D20ADF.D20ADF_TargetSingleIncSelf))
                return D20TargetClassification.SingleIncSelf;
            if (d20DefFlags.HasFlag(D20ADF.D20ADF_TargetSingleExcSelf))
                return D20TargetClassification.SingleExcSelf;
            if (d20DefFlags.HasFlag(D20ADF.D20ADF_MagicEffectTargeting))
                return D20TargetClassification.CastSpell;
            if (d20DefFlags.HasFlag(D20ADF.D20ADF_CallLightningTargeting))
                return D20TargetClassification.CallLightning;
            if (d20DefFlags.HasFlag(D20ADF.D20ADF_TargetContainer))
                return D20TargetClassification.ItemInteraction;
            if (d20DefFlags.HasFlag(D20ADF.D20ADF_TargetingBasedOnD20Data))
            {
                switch ((BardicMusicSongType) action.data1)
                {
                    case BardicMusicSongType.BM_FASCINATE:
                    case BardicMusicSongType.BM_INSPIRE_COMPETENCE:
                    case BardicMusicSongType.BM_SUGGESTION:
                    case BardicMusicSongType.BM_INSPIRE_GREATNESS:
                        return D20TargetClassification.SingleExcSelf;
                    case BardicMusicSongType.BM_INSPIRE_HEROICS:
                        return D20TargetClassification.SingleIncSelf;
                    default:
                        return D20TargetClassification.Target0;
                }
            }

            return D20TargetClassification.Target0;
        }

        [TempleDllLocation(0x10092e50)]
        public ActionErrorCode GlobD20ActnSetTarget(GameObjectBody target, LocAndOffsets? location)
        {
            switch (TargetClassification(globD20Action))
            {
                case D20TargetClassification.Movement:
                    if (location.HasValue)
                    {
                        globD20Action.destLoc = location.Value;
                    }
                    else
                    {
                        globD20Action.destLoc = target.GetLocationFull();
                        globD20Action.d20ATarget = target;
                    }

                    globD20Action.distTraversed = GameSystems.D20.Combat.EstimateDistance(globD20Action.d20APerformer,
                        globD20Action.destLoc.location, 0, 0.0);
                    return TargetCheck(globD20Action) ? ActionErrorCode.AEC_OK : ActionErrorCode.AEC_TARGET_INVALID;
                default:
                    return TargetCheck(globD20Action) ? ActionErrorCode.AEC_OK : ActionErrorCode.AEC_TARGET_INVALID;
                case D20TargetClassification.SingleExcSelf:
                case D20TargetClassification.SingleIncSelf:
                case D20TargetClassification.ItemInteraction:
                    if (location.HasValue)
                    {
                        globD20Action.destLoc = location.Value;
                    }
                    else if (target != null)
                    {
                        globD20Action.destLoc = target.GetLocationFull();
                    }

                    globD20Action.d20ATarget = target;
                    return TargetCheck(globD20Action) ? ActionErrorCode.AEC_OK : ActionErrorCode.AEC_TARGET_INVALID;
                case D20TargetClassification.Target0:
                    globD20Action.d20ATarget = target;
                    if (target != null)
                    {
                        globD20Action.destLoc = target.GetLocationFull();
                        globD20Action.distTraversed = globD20Action.d20APerformer.DistanceToObjInFeet(target);
                    }
                    else
                    {
                        if (location.HasValue)
                        {
                            globD20Action.destLoc = location.Value;
                        }

                        globD20Action.distTraversed = 0;
                    }

                    return TargetCheck(globD20Action) ? ActionErrorCode.AEC_OK : ActionErrorCode.AEC_TARGET_INVALID;
                case D20TargetClassification.CastSpell:
                    if (location.HasValue)
                    {
                        globD20Action.destLoc = location.Value;
                    }
                    else if (target != null)
                    {
                        globD20Action.destLoc = target.GetLocationFull();
                    }

                    globD20Action.d20ATarget = target;
                    return ActionErrorCode.AEC_OK;
            }
        }

        [TempleDllLocation(0x1008a580)]
        public bool TargetCheck(D20Action action)
        {
            var target = action.d20ATarget;

            var curSeq = GameSystems.D20.Actions.CurrentSequence;
            switch (GameSystems.D20.Actions.TargetClassification(action))
            {
                case D20TargetClassification.SingleExcSelf:
                    if (target == action.d20APerformer)
                        return false;
                    goto case D20TargetClassification.SingleIncSelf;
                case D20TargetClassification.SingleIncSelf:
                    if (target == null)
                        return false;
                    return target.IsCritter();
                case D20TargetClassification.ItemInteraction:
                    if (target == null)
                        return false;
                    if (target.type == ObjectType.container)
                        return true;
                    if (target.IsCritter())
                        return GameSystems.Critter.IsDeadNullDestroyed(target);
                    if (target.type == ObjectType.portal)
                        return true;
                    return false;
                case D20TargetClassification.CallLightning:
                    return GameSystems.D20.Actions.actSeqTargetsIdx >= 0;

                case D20TargetClassification.CastSpell:
                    curSeq.d20Action = action;
                    if (curSeq.spellPktBody.caster != null || curSeq.spellPktBody.spellEnum != 0)
                        return true;

                    curSeq.spellPktBody.Reset();
                    var spellEnum = action.d20SpellData.SpellEnum;
                    curSeq.spellPktBody.spellEnum = spellEnum;
                    curSeq.spellPktBody.spellEnumOriginal = action.d20SpellData.spellEnumOrg;
                    curSeq.spellPktBody.caster = action.d20APerformer;
                    curSeq.spellPktBody.spellClass = action.d20SpellData.spellClassCode;
                    var spellSlotLevel = action.d20SpellData.spellSlotLevel;
                    curSeq.spellPktBody.spellKnownSlotLevel = spellSlotLevel;
                    curSeq.spellPktBody.metaMagicData = action.d20SpellData.metaMagicData;
                    curSeq.spellPktBody.invIdx = action.d20SpellData.itemSpellData;

                    if (!GameSystems.Spell.TryGetSpellEntry(spellEnum, out var spellEntry))
                    {
                        Logger.Warn("Perform Cast Spell: failed to retrieve spell entry {0}!", spellEnum);
                        return true;
                    }

                    // set caster level
                    if (curSeq.spellPktBody.invIdx == INV_IDX_INVALID)
                    {
                        GameSystems.Spell.SpellPacketSetCasterLevel(curSeq.spellPktBody);
                    }
                    else
                    {
                        // item spell
                        curSeq.spellPktBody.casterLevel =
                            Math.Max(1, 2 * spellSlotLevel - 1); // todo special handling for Magic domain
                    }

                    curSeq.spellPktBody.spellRange = GameSystems.Spell.GetSpellRange(spellEntry,
                        curSeq.spellPktBody.casterLevel, curSeq.spellPktBody.caster);

                    if ((spellEntry.modeTargetSemiBitmask & 0xFF) != (int) UiPickerType.Personal
                        || spellEntry.radiusTarget < 0
                        || spellEntry.flagsTargetBitmask.HasFlag(UiPickerFlagsTarget.Radius))
                        return false;
                    curSeq.spellPktBody.orgTargetCount = 1;
                    curSeq.spellPktBody.targetListHandles = new[] {curSeq.spellPktBody.caster};
                    curSeq.spellPktBody.aoeCenter = curSeq.spellPktBody.caster.GetLocationFull();
                    curSeq.spellPktBody.aoeCenterZ = curSeq.spellPktBody.caster.OffsetZ;
                    if (spellEntry.radiusTarget > 0)
                        curSeq.spellPktBody.spellRange = spellEntry.radiusTarget;
                    return true;

                default:
                    return true;
            }
        }

        public void ActionTypeAutomatedSelection(GameObjectBody obj)
        {
            void SetGlobD20Action(D20ActionType actType, int data1)
            {
                globD20Action.d20ActType = actType;
                globD20Action.data1 = data1;
            }

            D20TargetClassification targetingType = D20TargetClassification.Movement; // default
            if (obj != null)
            {
                switch (obj.type)
                {
                    case ObjectType.portal:
                    case ObjectType.scenery:
                        targetingType = D20TargetClassification.Movement;
                        break;

                    case ObjectType.container:
                    case ObjectType.projectile:
                    case ObjectType.weapon:
                    case ObjectType.ammo:
                    case ObjectType.armor:
                    case ObjectType.money:
                    case ObjectType.food:
                    case ObjectType.scroll:
                    case ObjectType.key:
                    case ObjectType.written:
                    case ObjectType.generic:
                    case ObjectType.trap:
                    case ObjectType.bag:
                        targetingType = D20TargetClassification.ItemInteraction;
                        break;
                    case ObjectType.pc:
                    case ObjectType.npc:
                        targetingType = D20TargetClassification.SingleExcSelf;
                        break;
                    default:
                        targetingType = D20TargetClassification.Movement;
                        break;
                }
            }

            if (seqPickerTargetingType == targetingType
                || seqPickerTargetingType == D20TargetClassification.SingleIncSelf &&
                targetingType == D20TargetClassification.SingleExcSelf
                || seqPickerTargetingType != D20TargetClassification.Invalid)
            {
                SetGlobD20Action(seqPickerD20ActnType, seqPickerD20ActnData1);
                return;
            }


            // if no targeting type defined for picker:
            if (obj == null)
            {
                SetGlobD20Action(D20ActionType.UNSPECIFIED_MOVE, 0);
                return;
            }

            switch (obj.type)
            {
                case ObjectType.portal:
                case ObjectType.scenery:
                    SetGlobD20Action(D20ActionType.UNSPECIFIED_MOVE, 0);
                    return;
                case ObjectType.projectile:
                case ObjectType.weapon:
                case ObjectType.ammo:
                case ObjectType.armor:
                case ObjectType.money:
                case ObjectType.food:
                case ObjectType.scroll:
                case ObjectType.key:
                case ObjectType.written:
                case ObjectType.generic:
                case ObjectType.trap:
                case ObjectType.bag:
                    SetGlobD20Action(D20ActionType.PICKUP_OBJECT, 0);
                    return;
                case ObjectType.container:
                    SetGlobD20Action(D20ActionType.OPEN_CONTAINER, 0);
                    return;
                case ObjectType.pc:
                case ObjectType.npc:
                    break;
                default:
                    return;
            }

            // Critters

            var d20a = globD20Action;
            var performer = d20a.d20APerformer;
            if (GameSystems.Critter.IsFriendly(obj, performer))
            {
                // || GameSystems.Critter.NpcAllegianceShared(handle, performer)){ // NpcAllegianceShared check is currently not a good idea since ToEE has poor Friend or Foe tracking when it comes to faction mates...

                var performerLeader = GameSystems.Critter.GetLeader(performer);
                if (performerLeader == null || GameSystems.Critter.IsFriendly(obj, performerLeader))
                {
                    if (GameSystems.D20.D20Query(performer, D20DispatcherKey.QUE_HoldingCharge) == 0)
                    {
                        SetGlobD20Action(D20ActionType.UNSPECIFIED_MOVE, 0);
                        return;
                    }
                }
            }
            else if (GameSystems.Critter.IsDeadNullDestroyed(obj) && GameSystems.Critter.IsLootableCorpse(obj))
            {
                SetGlobD20Action(D20ActionType.OPEN_CONTAINER, 0);
                return;
            }

            SetGlobD20Action(D20ActionType.UNSPECIFIED_ATTACK, 0);
        }

        private struct ProjectileEntry
        {
            public D20Action d20a;
            public int pad4;
            public GameObjectBody projectile;
            public GameObjectBody ammoItem;
        }

        [TempleDllLocation(0x118A0720)]
        private List<ProjectileEntry> _projectiles = new List<ProjectileEntry>();

        [TempleDllLocation(0x10B3D5A4)]
        private ActionErrorCode actnProcState;

        [TempleDllLocation(0x10B3D5C4)]
        private bool performedDefaultAction;

        [TempleDllLocation(0x1008B1E0)]
        public bool ProjectileAppend(D20Action action, GameObjectBody projHndl, GameObjectBody thrownItem)
        {
            if (projHndl == null)
            {
                return false;
            }

            _projectiles.Add(new ProjectileEntry
            {
                d20a = action,
                ammoItem = thrownItem,
                projectile = projHndl
            });

            return true;
        }

        public D20ADF GetActionFlags(D20ActionType d20ActionType)
        {
            var flags = D20ActionDefs.GetActionDef(d20ActionType).flags;
            if (flags.HasFlag(D20ADF.D20ADF_Python))
            {
                // enforce python flag in case it's not defined elsewhere
                return _pythonActions[(int) globD20ActionKey].flags | D20ADF.D20ADF_Python;
            }

            return flags;
        }

        public int DispatchD20ActionCheck(D20Action action, TurnBasedStatus turnBasedStatus,
            DispatcherType dispType)
        {
            var dispatcherKey = D20DispatcherKey.D20A_UNSPECIFIED_MOVE + (int) action.d20ActType;

            var dispatcher = action.d20APerformer.GetDispatcher();
            if (dispatcher != null)
            {
                var dispIo = new DispIoD20ActionTurnBased(action);
                dispIo.tbStatus = turnBasedStatus;
                if (dispType == DispatcherType.GetBonusAttacks)
                {
                    dispIo.bonlist = BonusList.Default;
                    dispatcher.Process(dispType, dispatcherKey, dispIo);
                    return (int) (dispIo.returnVal + dispIo.bonlist.OverallBonus);
                }
                else
                {
                    dispatcher.Process(dispType, dispatcherKey, dispIo);
                    return (int) dispIo.returnVal;
                }
            }

            return 0;
        }

        public PythonActionSpec GetPythonAction(int id) => _pythonActions[id];

        [TempleDllLocation(0x10095450)]
        public ActionErrorCode AddToSeqWithTarget(D20Action action, ActionSequence sequence, TurnBasedStatus tbStatus)
        {
            var target = action.d20ATarget;
            var actNum = sequence.d20ActArrayNum;
            if (target == null)
            {
                return ActionErrorCode.AEC_TARGET_INVALID;
            }

            // check if target is within reach
            var reach = action.d20APerformer.GetReach(action.d20ActType);
            if (action.d20APerformer.DistanceToObjInFeet(action.d20ATarget) < reach)
            {
                sequence.d20ActArray.Add(action);
                return ActionErrorCode.AEC_OK;
            }

            // if not, add a move sequence
            var d20aCopy = action.Copy();
            d20aCopy.d20ActType = D20ActionType.UNSPECIFIED_MOVE;
            d20aCopy.destLoc = target.GetLocationFull();
            var result = MoveSequenceParse(d20aCopy, sequence, tbStatus, 0.0f, reach, true);
            if (result == ActionErrorCode.AEC_OK)
            {
                var tbStatusCopy = tbStatus.Copy();
                sequence.d20ActArray.Add(action);
                if (actNum < sequence.d20ActArrayNum)
                {
                    for (; actNum < sequence.d20ActArrayNum; actNum++)
                    {
                        var otherAction = sequence.d20ActArray[actNum];
                        result = TurnBasedStatusUpdate(otherAction, tbStatusCopy);
                        if (result != ActionErrorCode.AEC_OK)
                        {
                            tbStatusCopy.errCode = result;
                            return result;
                        }

                        var actionType = otherAction.d20ActType;
                        var actionCheckFunc = D20ActionDefs.GetActionDef(actionType).actionCheckFunc;
                        if (actionCheckFunc != null)
                        {
                            result = actionCheckFunc(otherAction, tbStatusCopy);
                            if (result != ActionErrorCode.AEC_OK)
                            {
                                return result;
                            }
                        }
                    }

                    if (actNum >= sequence.d20ActArrayNum)
                        return ActionErrorCode.AEC_OK;
                    tbStatusCopy.errCode = result;
                    if (result != ActionErrorCode.AEC_OK)
                        return result;
                }

                return ActionErrorCode.AEC_OK;
            }

            return result;
        }

        [TempleDllLocation(0x10B3D5C8)]
        [TempleDllLocation(0x100927f0)]
        public bool rollbackSequenceFlag { get; private set; }

        [TempleDllLocation(0x10B3D5C0)]
        public bool performingDefaultAction { get; set; }

        [TempleDllLocation(0x10094f70)]
        internal ActionErrorCode MoveSequenceParse(D20Action d20aIn, ActionSequence actSeq, TurnBasedStatus tbStat,
            float distToTgtMin, float reach, bool nonspecificMoveType)
        {
            D20Action d20aCopy;
            TurnBasedStatus tbStatCopy = tbStat.Copy();
            LocAndOffsets locAndOffCopy = d20aIn.destLoc;

            seqCheckFuncs(tbStatCopy);

            // if prone, add a standup action
            if (GameSystems.D20.D20Query(d20aIn.d20APerformer, D20DispatcherKey.QUE_Prone) != 0)
            {
                d20aCopy = d20aIn.Copy();
                d20aCopy.d20ActType = D20ActionType.STAND_UP;
                actSeq.d20ActArray.Add(d20aCopy);
            }

            d20aCopy = d20aIn.Copy();
            var d20a = d20aIn;

            var pathQ = new PathQuery();
            pathQ.critter = d20aCopy.d20APerformer;
            pathQ.from = actSeq.performerLoc;

            if (d20a.d20ATarget != null)
            {
                pathQ.targetObj = d20a.d20ATarget;
                if (pathQ.critter == pathQ.targetObj)
                    return ActionErrorCode.AEC_TARGET_INVALID;

                pathQ.flags = PathQueryFlags.PQF_TO_EXACT | PathQueryFlags.PQF_HAS_CRITTER | PathQueryFlags.PQF_800
                              | PathQueryFlags.PQF_TARGET_OBJ | PathQueryFlags.PQF_ADJUST_RADIUS |
                              PathQueryFlags.PQF_ADJ_RADIUS_REQUIRE_LOS;

                if (reach < 0.1f)
                {
                    reach = 3.0f; // 3 feet minimum reach
                }

                actSeq.targetObj = d20a.d20ATarget;
                pathQ.distanceToTargetMin = distToTgtMin * 12.0f; // Feet to inches
                var fourPointSevenPlusEight = locXY.INCH_PER_SUBTILE / 2 + 8.0f;
                pathQ.tolRadius = reach * 12.0f - fourPointSevenPlusEight; // Feet to inches
            }
            else
            {
                pathQ.to = d20aIn.destLoc;
                pathQ.flags = PathQueryFlags.PQF_TO_EXACT | PathQueryFlags.PQF_HAS_CRITTER | PathQueryFlags.PQF_800
                              | PathQueryFlags.PQF_ALLOW_ALTERNATIVE_TARGET_TILE;
            }

            if (d20a.d20Caf.HasFlag(D20CAF.UNNECESSARY))
            {
                pathQ.flags |= PathQueryFlags.PQF_A_STAR_TIME_CAPPED;
            }

            // frees the last path used in the d20a
            ReleasePooledPathQueryResult(ref d20aCopy.path);

            // get new path result slot
            // find path
            rollbackSequenceFlag = false;
            if ((d20aCopy.d20Caf.HasFlag(D20CAF.CHARGE)) || d20a.d20ActType == D20ActionType.RUN ||
                d20a.d20ActType == D20ActionType.DOUBLE_MOVE)
            {
                pathQ.flags |= PathQueryFlags.PQF_DONT_USE_PATHNODES; // so it runs in a straight line
            }

            var pathResult = GameSystems.PathX.FindPath(pathQ, out d20aCopy.path);
            var pqResult = d20aCopy.path;
            if (!pathResult)
            {
                if (pqResult.flags.HasFlag(PathFlags.PF_TIMED_OUT))
                {
                    rollbackSequenceFlag = true;
                }

                if (pathQ.targetObj != null)
                    Logger.Debug("MoveSequenceParse: FAILED PATH... {0} attempted from {1} to {2} ({3})",
                        GameSystems.MapObject.GetDisplayName(pqResult.mover), pqResult.from, pqResult.to,
                        GameSystems.MapObject.GetDisplayName(pathQ.targetObj));
                else
                    Logger.Debug("MoveSequenceParse: FAILED PATH... {0} attempted from {1} to {2}",
                        GameSystems.MapObject.GetDisplayName(pqResult.mover), pqResult.from, pqResult.to);

                ReleasePooledPathQueryResult(ref d20aCopy.path);
                return ActionErrorCode.AEC_TARGET_INVALID;
            }

            var pathLength = pqResult.GetPathResultLength();
            d20aCopy.destLoc = pqResult.to;
            d20aCopy.distTraversed = pathLength;

            if (pathLength < 0.1f)
            {
                ReleasePooledPathQueryResult(ref d20aCopy.path);
                return ActionErrorCode.AEC_OK;
            }

            if (!GameSystems.Combat.IsCombatActive())
            {
                d20aCopy.distTraversed = 0;
                pathLength = 0.0f;
            }

            // deducting moves that have already been spent, but also a raw calculation (not taking 5' step and such into account)
            if (GetRemainingMaxMoveLength(d20a, tbStatCopy, out var remainingMaxMoveLength))
            {
                if (remainingMaxMoveLength < 0.1)
                {
                    ReleasePooledPathQueryResult(ref d20aCopy.path);
                    return ActionErrorCode.AEC_TARGET_TOO_FAR;
                }

                if (remainingMaxMoveLength < pathLength)
                {
                    TrimPathToRemainingMoveLength(d20aCopy, remainingMaxMoveLength, pathQ);
                    pqResult = d20aCopy.path;
                    pathLength = remainingMaxMoveLength;
                }
            }

            /*
            this is 0 for specific move action types like 5' step, Move, Run, Withdraw;
            */
            if (nonspecificMoveType)
            {
                var chosenActionType = D20ActionType.MOVE;

                float baseMoveDist = d20aCopy.d20APerformer.Dispatch41GetMoveSpeed(out _);
                if (!d20aCopy.d20Caf.HasFlag(D20CAF.CHARGE))
                {
                    if (pathLength > tbStatCopy.surplusMoveDistance)
                    {
                        // distance greater than twice base distance . cannot do
                        if (2 * baseMoveDist + tbStatCopy.surplusMoveDistance < pathLength)
                        {
                            ReleasePooledPathQueryResult(ref d20aCopy.path);
                            return ActionErrorCode.AEC_TARGET_TOO_FAR;
                        }

                        // distance greater than base move distance . do double move
                        if (tbStatCopy.surplusMoveDistance + baseMoveDist < pathLength)
                        {
                            chosenActionType = D20ActionType.DOUBLE_MOVE;
                        }

                        // check if under 5'
                        else if (pathLength <= 5.0f)
                        {
                            if (d20a.d20ActType != D20ActionType.UNSPECIFIED_MOVE)
                            {
                                var actCost = new ActionCostPacket();
                                D20ActionDefs.GetActionDef(d20a.d20ActType).actionCost(d20a, tbStatCopy, actCost);

                                if (actCost.hourglassCost == 4 ||
                                    tbStatCopy.hourglassState == HourglassState.EMPTY)
                                {
                                    chosenActionType = D20ActionType.FIVEFOOTSTEP;
                                }
                            }
                            else
                            {
                                // added for AI to take 5' steps when it still has full round action to exploit
                                if (tbStatCopy.hourglassState == HourglassState.EMPTY ||
                                    tbStatCopy.hourglassState == HourglassState.FULL &&
                                    !GameSystems.Party.IsPlayerControlled(d20a.d20APerformer))
                                {
                                    chosenActionType = D20ActionType.FIVEFOOTSTEP;
                                    if ((tbStatCopy.tbsFlags &
                                         (TurnBasedStatusFlags.Movement | TurnBasedStatusFlags.Movement2)) != 0)
                                    {
                                        chosenActionType = D20ActionType.MOVE;
                                    }
                                }
                            }
                        }
                    }

                    d20aCopy.d20ActType = chosenActionType;
                }
                else if (2 * baseMoveDist >= pathLength)
                {
                    chosenActionType = D20ActionType.RUN;
                }
                else
                {
                    ReleasePooledPathQueryResult(ref d20aCopy.path);
                    return ActionErrorCode.AEC_TARGET_TOO_FAR;
                }

                d20aCopy.d20ActType = chosenActionType;
            }

            actSeq.performerLoc = pqResult.to;

            ProcessPathForReadiedActions(d20aCopy, out var readiedAction);
            ProcessSequenceForAoOs(actSeq, d20aCopy);
            if (readiedAction != null)
            {
                AddReadiedInterrupt(actSeq, readiedAction);
            }

            UpdateDistTraversed(actSeq);
            return 0;
        }

        [TempleDllLocation(0x1008bb40)]
        private void ProcessSequenceForAoOs(ActionSequence actSeq, D20Action action)
        {
            float distFeet = 0.0f;
            var pqr = action.path;
            var d20ActionTypePostAoO = action.d20ActType;
            bool addingAoOStatus = true;
            if (d20ActionTypePostAoO == D20ActionType.FIVEFOOTSTEP)
            {
                actSeq.d20ActArray.Add(action.Copy());
                return;
            }

            if (d20ActionTypePostAoO == D20ActionType.DOUBLE_MOVE)
            {
                distFeet = 5.0f;
                d20ActionTypePostAoO = D20ActionType.MOVE;
            }

            if (!GameSystems.D20.Combat.FindAttacksOfOpportunity(action.d20APerformer, action.path, distFeet,
                out var attacks))
            {
                actSeq.d20ActArray.Add(action.Copy());
                return;
            }

            var d20aAoOMovement = new D20Action(D20ActionType.AOO_MOVEMENT);
            var startDistFeet = 0.0f;
            var endDistFeet = action.path.GetPathResultLength();
            foreach (var attack in attacks)
            {
                if (attack.Distance != startDistFeet)
                {
                    d20aAoOMovement = action.Copy();
                    var aooDistFeet = attack.Distance;

                    GameSystems.PathX.GetPartialPath(pqr, out var pqrTrunc, startDistFeet, aooDistFeet);
                    d20aAoOMovement.path = pqrTrunc;

                    startDistFeet = attack.Distance;
                    d20aAoOMovement.destLoc = attack.Location;
                    d20aAoOMovement.distTraversed = pqrTrunc.GetPathResultLength();
                    if (!addingAoOStatus)
                    {
                        d20aAoOMovement.d20ActType = d20ActionTypePostAoO;
                    }

                    addingAoOStatus = false;
                    actSeq.d20ActArray.Add(d20aAoOMovement);
                }

                d20aAoOMovement.d20APerformer = attack.Interrupter;
                d20aAoOMovement.d20ATarget = action.d20APerformer;
                d20aAoOMovement.destLoc = attack.Location;
                d20aAoOMovement.d20Caf = 0;
                d20aAoOMovement.distTraversed = 0;
                d20aAoOMovement.path = null;
                d20aAoOMovement.d20ActType = D20ActionType.AOO_MOVEMENT;
                d20aAoOMovement.data1 = 1;
                actSeq.d20ActArray.Add(d20aAoOMovement.Copy());
            }

            d20aAoOMovement = action.Copy();

            GameSystems.PathX.GetPartialPath(pqr, out var pqrLastStretch, startDistFeet, endDistFeet);
            d20aAoOMovement.path = pqrLastStretch;

            d20aAoOMovement.destLoc = action.destLoc;
            d20aAoOMovement.distTraversed = pqrLastStretch.GetPathResultLength();
            if (!addingAoOStatus)
                d20aAoOMovement.d20ActType = d20ActionTypePostAoO;
            actSeq.d20ActArray.Add(d20aAoOMovement);
            ReleasePooledPathQueryResult(ref action.path);
        }

        [TempleDllLocation(0x1008bac0)]
        private void AddReadiedInterrupt(ActionSequence actSeq, ReadiedActionPacket readied)
        {
            var readiedAction = new D20Action();
            readiedAction.d20ATarget = actSeq.performer;
            readiedAction.d20Caf = default;
            readiedAction.d20ActType = D20ActionType.READIED_INTERRUPT;
            readiedAction.data1 = 1;
            readiedAction.path = null;
            readiedAction.d20APerformer = readied.interrupter;
            actSeq.d20ActArray.Add(readiedAction);
        }

        // The size of increments in which we search for interrupts along the path
        private const float InterruptSearchIncrement = 4.0f;

        [TempleDllLocation(0x100939d0)]
        private void ProcessPathForReadiedActions(D20Action action, out ReadiedActionPacket triggeredReadiedAction)
        {
            Span<int> actionIndices = stackalloc int[_readiedActions.Count];
            FindHostileReadyVsMoveActions(action.d20APerformer, ref actionIndices);
            if (actionIndices.Length == 0)
            {
                triggeredReadiedAction = null;
                return;
            }

            var pathLength = action.path.GetPathResultLength();
            var startLocation = action.path.from;

            // Search along the path and look for changes of interrupts
            for (var dist = 0.0f; dist < pathLength + InterruptSearchIncrement / 2; dist += InterruptSearchIncrement)
            {
                var truncateLengthFeet = MathF.Min(dist, pathLength);
                GameSystems.PathX.TruncatePathToDistance(action.path, out var newLocation, truncateLengthFeet);

                // Process every approach/withdraw action along the path
                foreach (var readiedActionIdx in actionIndices)
                {
                    var readiedAction = _readiedActions[readiedActionIdx];

                    var wasInRange =
                        GameSystems.D20.Combat.CanMeleeTargetAtLocation(readiedAction.interrupter, action.d20APerformer,
                            startLocation);
                    var willBeInRange =
                        GameSystems.D20.Combat.CanMeleeTargetAtLocation(readiedAction.interrupter, action.d20APerformer,
                            newLocation);

                    if (readiedAction.readyType == ReadyVsTypeEnum.RV_Approach)
                    {
                        // This essentially checks if the action performer moves from a spot that the interrupter
                        // can attack it into a spot that it cannot attack it.
                        if (wasInRange)
                        {
                            continue;
                        }

                        if (!willBeInRange && !GameSystems.D20.Combat.IsWithinThreeFeet(readiedAction.interrupter,
                                action.d20APerformer,
                                newLocation))
                        {
                            continue;
                        }
                    }
                    else if (readiedAction.readyType == ReadyVsTypeEnum.RV_Withdrawal)
                    {
                        if (!wasInRange)
                        {
                            continue;
                        }

                        if (willBeInRange)
                        {
                            if (!GameSystems.D20.Combat.IsWithinThreeFeet(readiedAction.interrupter,
                                action.d20APerformer,
                                newLocation))
                            {
                                continue;
                            }
                        }
                    }
                    else
                    {
                        continue;
                    }

                    triggeredReadiedAction = readiedAction;

                    // Truncate the path so it ends where the interrupt happens
                    GameSystems.PathX.GetPartialPath(action.path, out var trimmedPath, 0, dist);
                    ReleasePooledPathQueryResult(ref action.path);
                    action.path = trimmedPath;
                    return;
                }
            }

            triggeredReadiedAction = null;
        }

        [TempleDllLocation(0x10091580)]
        private void FindHostileReadyVsMoveActions(GameObjectBody mover, ref Span<int> actionIndices)
        {
            int count = 0;
            for (var index = 0; index < _readiedActions.Count; index++)
            {
                var readiedAction = _readiedActions[index];
                if (readiedAction.flags == 1 && (readiedAction.readyType == ReadyVsTypeEnum.RV_Approach ||
                                                 readiedAction.readyType == ReadyVsTypeEnum.RV_Withdrawal))
                {
                    // Ignore actions that would not actually trigger against the moving critter
                    if (readiedAction.interrupter == mover ||
                        GameSystems.Critter.IsFriendly(readiedAction.interrupter, mover))
                    {
                        continue;
                    }

                    actionIndices[count++] = index;
                }
            }

            actionIndices = actionIndices.Slice(0, count);
        }

        [TempleDllLocation(0x1008ba80)]
        private void UpdateDistTraversed(ActionSequence actSeq)
        {
            foreach (var d20Action in actSeq.d20ActArray)
            {
                var path = d20Action.path;
                if (path != null)
                {
                    d20Action.distTraversed = path.GetPathResultLength();
                }
            }
        }

        [TempleDllLocation(0x10091580)]
        int ReadyVsApproachOrWithdrawalCount(GameObjectBody mover)
        {
            int result = 0;
            for (int i = 0; i < _readiedActions.Count; i++)
            {
                if (_readiedActions[i].flags == 1
                    && _readiedActions[i].interrupter != null
                    && (_readiedActions[i].readyType == ReadyVsTypeEnum.RV_Approach
                        || _readiedActions[i].readyType == ReadyVsTypeEnum.RV_Withdrawal))
                    result++;
            }

            return result;
        }

        [TempleDllLocation(0x10091500)]
        void ReadyVsRemoveForObj(GameObjectBody obj)
        {
            for (int i = 0; i < _readiedActions.Count; i++)
            {
                if (_readiedActions[i].flags == 1 && _readiedActions[i].interrupter == obj)
                {
                    _readiedActions[i].flags = 0;
                    _readiedActions[i].interrupter = null;
                }
            }
        }

        [TempleDllLocation(0x10091650)]
        private ReadiedActionPacket ReadiedActionGetNext(ReadiedActionPacket prevReadiedAction, D20Action d20a)
        {
            int i0 = 0;

            // find the prevReadiedAction in the array (kinda retarded since it's a pointer???)
            if (prevReadiedAction != null)
            {
                for (i0 = 0; i0 < _readiedActions.Count; i0++)
                {
                    if (_readiedActions[i0] == prevReadiedAction)
                    {
                        i0++;
                        break;
                    }
                }

                if (i0 >= _readiedActions.Count)
                    return null;
            }

            for (int i = i0; i < _readiedActions.Count; i++)
            {
                if ((_readiedActions[i].flags & 1) == 0)
                    continue;
                switch (_readiedActions[i].readyType)
                {
                    case ReadyVsTypeEnum.RV_Spell:
                    case ReadyVsTypeEnum.RV_Counterspell:
                        if (GameSystems.Critter.IsFriendly(d20a.d20APerformer, _readiedActions[i].interrupter))
                            continue;
                        if (d20a.d20ActType == D20ActionType.CAST_SPELL)
                            return _readiedActions[i];
                        break;
                    case ReadyVsTypeEnum.RV_Approach:
                    case ReadyVsTypeEnum.RV_Withdrawal:
                    default:
                        if (d20a.d20ActType != D20ActionType.READIED_INTERRUPT)
                            continue;
                        if (d20a.d20APerformer != _readiedActions[i].interrupter)
                            continue;
                        return _readiedActions[i];
                }
            }

            return null;
        }

        [TempleDllLocation(0x1008B9A0)]
        private void TrimPathToRemainingMoveLength(D20Action d20a, float remainingMoveLength, PathQuery pathQ)
        {
            var pathLengthTrimmed = remainingMoveLength - 0.1f;

            GameSystems.PathX.GetPartialPath(d20a.path, out var pqrTrimmed, 0.0f, pathLengthTrimmed);

            ReleasePooledPathQueryResult(ref d20a.path);
            d20a.path = pqrTrimmed;
            var destination = pqrTrimmed.to;
            if (!GameSystems.PathX.PathDestIsClear(pathQ, d20a.d20APerformer, destination))
            {
                d20a.d20Caf |= D20CAF.ALTERNATE;
            }

            d20a.d20Caf |= D20CAF.TRUNCATED;
        }

        internal bool GetRemainingMaxMoveLength(D20Action d20a, TurnBasedStatus tbStat, out float moveLen)
        {
            var surplusMoves = tbStat.surplusMoveDistance;
            var moveSpeed = d20a.d20APerformer.Dispatch41GetMoveSpeed(out _);
            if (d20a.d20ActType == D20ActionType.UNSPECIFIED_MOVE)
            {
                if (tbStat.hourglassState >= HourglassState.FULL)
                {
                    moveLen = 2 * moveSpeed + surplusMoves;
                    return true;
                }

                if (tbStat.hourglassState >= HourglassState.MOVE)
                {
                    moveLen = moveSpeed + surplusMoves;
                    return true;
                }

                moveLen = surplusMoves;
                return true;
            }

            if (d20a.d20ActType == D20ActionType.FIVEFOOTSTEP
                && (tbStat.tbsFlags & (TurnBasedStatusFlags.Movement | TurnBasedStatusFlags.Movement2)) == default)
            {
                if (surplusMoves <= 0.001f)
                {
                    moveLen = 5.0f;
                    return true;
                }

                moveLen = surplusMoves;
                return true;
            }

            if (d20a.d20ActType == D20ActionType.MOVE)
            {
                moveLen = moveSpeed + surplusMoves;
                return true;
            }

            if (d20a.d20ActType == D20ActionType.DOUBLE_MOVE)
            {
                moveLen = 2 * moveSpeed + surplusMoves;
                return true;
            }

            moveLen = 0;
            return false;
        }

        [TempleDllLocation(0x1008b850)]
        internal void ReleasePooledPathQueryResult(ref PathQueryResult result)
        {
            if (result == null)
            {
                return;
            }

            result = null;
            Stub.TODO(); // Will not be implemented either
        }

        [TempleDllLocation(0x1008b810)]
        public PathQueryResult GetPooledPathQueryResult()
        {
            throw new NotImplementedException(); // Will not be implemented either
        }

        [TempleDllLocation(0x10094ca0)]
        private ActionErrorCode seqCheckFuncs(TurnBasedStatus tbStatus)
        {
            var curSeq = CurrentSequence;
            var result = ActionErrorCode.AEC_OK;

            if (curSeq == null)
            {
                tbStatus.Clear();
                return 0;
            }

            tbStatus = curSeq.tbStatus;
            var seqPerfLoc = curSeq.performer.GetLocationFull();

            for (int i = 0; i < curSeq.d20ActArray.Count; i++)
            {
                var d20a = curSeq.d20ActArray[i];
                if (curSeq.d20ActArrayNum <= 0) return 0;

                var d20Def = D20ActionDefs.GetActionDef(d20a.d20ActType);
                var tgtCheckFunc = d20Def.tgtCheckFunc;
                if (tgtCheckFunc != null)
                {
                    result = (ActionErrorCode) tgtCheckFunc(d20a, tbStatus);
                    if (result != ActionErrorCode.AEC_OK)
                    {
                        break;
                    }
                }

                result = TurnBasedStatusUpdate(d20a, tbStatus);
                if (result != ActionErrorCode.AEC_OK)
                {
                    tbStatus.errCode = result;
                    break;
                }

                var actCheckFunc = d20Def.actionCheckFunc;
                if (actCheckFunc != null)
                {
                    result = actCheckFunc(d20a, tbStatus);
                    if (result != ActionErrorCode.AEC_OK)
                    {
                        break;
                    }
                }

                var locCheckFunc = d20Def.locCheckFunc;
                if (locCheckFunc != null)
                {
                    result = locCheckFunc(d20a, tbStatus, seqPerfLoc);
                    if (result != 0)
                    {
                        break;
                    }
                }

                var path = curSeq.d20ActArray[i].path;
                if (path != null)
                {
                    seqPerfLoc = path.to;
                }
            }

            if (result != ActionErrorCode.AEC_OK)
            {
                if (CurrentSequence == null)
                {
                    tbStatus.Clear();
                }
                else
                {
                    CurrentSequence.tbStatus.CopyTo(tbStatus);
                }
            }

            return result;
        }

        [TempleDllLocation(0x10093950)]
        internal ActionErrorCode TurnBasedStatusUpdate(D20Action action, TurnBasedStatus tbStatus)
        {
            var tbStatCopy = tbStatus.Copy();

            var actProcResult = ActionCostProcess(tbStatCopy, action);
            if (actProcResult == ActionErrorCode.AEC_OK)
            {
                tbStatCopy.CopyTo(tbStatus);
                return actProcResult;
            }

            var tbsCheckFunc = D20ActionDefs.GetActionDef(action.d20ActType).turnBasedStatusCheck;
            if (tbsCheckFunc != null)
            {
                if (tbsCheckFunc(action, tbStatCopy) != ActionErrorCode.AEC_OK)
                {
                    return actProcResult;
                }

                actProcResult = ActionCostProcess(tbStatCopy, action);
            }

            if (actProcResult == ActionErrorCode.AEC_OK)
            {
                tbStatCopy.CopyTo(tbStatus);
                return actProcResult;
            }

            return actProcResult;
        }

        [TempleDllLocation(0x1008b040)]
        private ActionErrorCode ActionCostProcess(TurnBasedStatus tbStat, D20Action action)
        {
            var actCost = new ActionCostPacket();

            var result = D20ActionDefs.GetActionDef(action.d20ActType).actionCost(action, tbStat, actCost);
            if (result != ActionErrorCode.AEC_OK)
            {
                return result;
            }

            // Adding action cost modification facility
            actCost.hourglassCost = action.d20APerformer.DispatchActionCostMod(actCost, tbStat, action);

            var hourglassCost = actCost.hourglassCost;
            tbStat.hourglassState = GetHourglassTransition(tbStat.hourglassState, actCost.hourglassCost);

            if (tbStat.hourglassState == HourglassState.INVALID) // this is an error state I think
            {
                if (hourglassCost <= 4)
                {
                    switch (hourglassCost)
                    {
                        case 1:
                            tbStat.errCode = ActionErrorCode.AEC_NOT_ENOUGH_TIME2;
                            return ActionErrorCode.AEC_NOT_ENOUGH_TIME2;
                        case 2:
                            tbStat.errCode = ActionErrorCode.AEC_NOT_ENOUGH_TIME1;
                            return ActionErrorCode.AEC_NOT_ENOUGH_TIME1;
                        case 4:
                            tbStat.errCode = ActionErrorCode.AEC_NOT_ENOUGH_TIME3;
                            return ActionErrorCode.AEC_NOT_ENOUGH_TIME3;
                        case 3:
                            tbStat.errCode = ActionErrorCode.AEC_NOT_ENOUGH_TIME3;
                            break;
                    }
                }
            }

            if (tbStat.surplusMoveDistance >= actCost.moveDistCost)
            {
                tbStat.surplusMoveDistance -= actCost.moveDistCost;
                if (actCost.chargeAfterPicker <= 0
                    || actCost.chargeAfterPicker + tbStat.attackModeCode <=
                    tbStat.baseAttackNumCode + tbStat.numBonusAttacks)
                {
                    if (tbStat.numBonusAttacks < actCost.chargeAfterPicker)
                        tbStat.attackModeCode += actCost.chargeAfterPicker;
                    else
                        tbStat.numBonusAttacks -= actCost.chargeAfterPicker;
                    if (tbStat.attackModeCode == tbStat.baseAttackNumCode && tbStat.numBonusAttacks == 0)
                        tbStat.tbsFlags &= ~TurnBasedStatusFlags.FullAttack;
                    result = ActionErrorCode.AEC_OK;
                }
                else
                {
                    result = ActionErrorCode.AEC_NOT_ENOUGH_TIME1;
                }
            }
            else
            {
                result = (tbStat.tbsFlags & TurnBasedStatusFlags.Movement2) != 0
                    ? ActionErrorCode.AEC_ALREADY_MOVED
                    : ActionErrorCode.AEC_TARGET_OUT_OF_RANGE;
            }

            return result;
        }

        [TempleDllLocation(0x10094c60)]
        public ActionErrorCode seqCheckAction(D20Action action, TurnBasedStatus tbStat)
        {
            ActionErrorCode errorCode = TurnBasedStatusUpdate(action, tbStat);
            if (errorCode != ActionErrorCode.AEC_OK)
            {
                tbStat.errCode = errorCode;
                return errorCode;
            }

            var actionCheckFunc = D20ActionDefs.GetActionDef(action.d20ActType).actionCheckFunc;
            if (actionCheckFunc != null)
            {
                return actionCheckFunc(action, tbStat);
            }

            return ActionErrorCode.AEC_OK;
        }

        [TempleDllLocation(0x1008c580)]
        public void FullAttackCostCalculate(D20Action d20a, TurnBasedStatus tbStatus, out int baseAttackNumCode,
            out int bonusAttacks, out int numAttacks, out int attackModeCode)
        {
            GameObjectBody performer = d20a.d20APerformer;
            int usingOffhand = 0;
            int _attackTypeCodeHigh = 1;
            int _attackTypeCodeLow = 0;
            int numAttacksBase = 0;
            var mainWeapon = GameSystems.Item.ItemWornAt(performer, EquipSlot.WeaponPrimary);
            var offhand = GameSystems.Item.ItemWornAt(performer, EquipSlot.WeaponSecondary);

            if (offhand != null)
            {
                if (mainWeapon != null)
                {
                    if (offhand.type != ObjectType.armor)
                    {
                        _attackTypeCodeHigh = AttackPacket.ATTACK_CODE_OFFHAND + 1; // originally 5
                        _attackTypeCodeLow = AttackPacket.ATTACK_CODE_OFFHAND; // originally 4
                        usingOffhand = 1;
                    }
                }
                else
                {
                    mainWeapon = offhand;
                }
            }

            if (mainWeapon != null && GameSystems.Item.IsRangedWeapon(mainWeapon))
            {
                d20a.d20Caf |= D20CAF.RANGED;
            }

            // if unarmed check natural attacks (for monsters)
            if (mainWeapon == null && offhand == null)
            {
                numAttacksBase = DispatchD20ActionCheck(d20a, tbStatus, DispatcherType.GetCritterNaturalAttacksNum);

                if (numAttacksBase > 0)
                {
                    _attackTypeCodeHigh = AttackPacket.ATTACK_CODE_NATURAL_ATTACK + 1; // originally 10
                    _attackTypeCodeLow = AttackPacket.ATTACK_CODE_NATURAL_ATTACK; // originally 9
                }
            }

            if (numAttacksBase <= 0)
            {
                numAttacksBase = DispatchD20ActionCheck(d20a, tbStatus, DispatcherType.GetNumAttacksBase);
            }

            bonusAttacks = DispatchD20ActionCheck(d20a, tbStatus, DispatcherType.GetBonusAttacks);
            numAttacks = usingOffhand + numAttacksBase + bonusAttacks;
            attackModeCode = _attackTypeCodeLow;
            baseAttackNumCode = numAttacksBase + _attackTypeCodeHigh - 1 + usingOffhand;
        }

        public bool UsingSecondaryWeapon(D20Action d20a)
        {
            return UsingSecondaryWeapon(d20a.d20APerformer, d20a.data1);
        }

        public bool UsingSecondaryWeapon(GameObjectBody obj, int attackCode)
        {
            if (attackCode == AttackPacket.ATTACK_CODE_OFFHAND + 2
                || attackCode == AttackPacket.ATTACK_CODE_OFFHAND + 4
                || attackCode == AttackPacket.ATTACK_CODE_OFFHAND + 6)
            {
                if (attackCode == AttackPacket.ATTACK_CODE_OFFHAND + 2)
                {
                    return true;
                }

                if (attackCode == AttackPacket.ATTACK_CODE_OFFHAND + 4)
                {
                    if (GameSystems.Feat.HasFeatCount(obj, FeatId.IMPROVED_TWO_WEAPON_FIGHTING) != 0
                        || GameSystems.Feat.HasFeatCountByClass(obj, FeatId.IMPROVED_TWO_WEAPON_FIGHTING_RANGER,
                            (Stat) 0, 0) != 0)
                        return true;
                }
                else if (attackCode == AttackPacket.ATTACK_CODE_OFFHAND + 6)
                {
                    if (GameSystems.Feat.HasFeatCount(obj, FeatId.GREATER_TWO_WEAPON_FIGHTING) != 0
                        || GameSystems.Feat.HasFeatCountByClass(obj, FeatId.GREATER_TWO_WEAPON_FIGHTING_RANGER,
                            (Stat) 0, 0) != 0)
                        return true;
                }
            }

            return false;
        }

        [TempleDllLocation(0x10092da0)]
        public void ResetAll(GameObjectBody critter)
        {
            // resets the readied action cache
            // Clear TBUiIntgameFocus
            // Set handle to initiative list first entry
            // Free all act seq array occupancies
            // Set Turn Based Actor to Initiative List first entry, and use their initiative
            // Clear the picker targeting type, action type and data
            // Clear pathfinding cache occupancies

            _readiedActions.Clear();
            GameUiBridge.ResetSelectionInput();

            if (critter != null && (GameSystems.Party.IsPlayerControlled(critter) ||
                                    GameSystems.Critter.IsConcealed(critter)))
            {
                GameSystems.D20.Initiative.Move(critter, 0);
            }

            actSeqArray.Clear();

            GameSystems.D20.Initiative.RewindCurrentActor();
            seqPickerTargetingType = D20TargetClassification.Invalid;
            seqPickerD20ActnType = D20ActionType.UNSPECIFIED_ATTACK;
            seqPickerD20ActnData1 = 0;
        }

        [TempleDllLocation(0x10099430)]
        public void TurnStart(GameObjectBody obj)
        {
            Logger.Debug("*** NEXT TURN *** starting for {0}. CurSeq: {1}", obj, CurrentSequence);

            // NOTE: Previously wrote 0 to 0x1189FB60, but found no other uses for it
            seqSthg_10B3D59C = 0;
            performedDefaultAction = false;

            if (globD20Action.d20APerformer != null && globD20Action.d20APerformer != obj)
            {
                GameSystems.D20.D20SendSignal(globD20Action.d20APerformer, D20DispatcherKey.SIG_EndTurn);
            }

            if (!GameSystems.Combat.IsCombatActive())
                return;

            // check for interrupter sequence
            if (actSeqInterrupt != null)
            {
                // switch sequences
                var actSeq = actSeqInterrupt;
                CurrentSequence = actSeq;
                actSeqInterrupt = actSeq.interruptSeq;
                var curIdx = actSeq.d20aCurIdx;
                Logger.Info("Switching to Interrupt sequence, actor {0}",
                    GameSystems.MapObject.GetDisplayName(actSeq.performer));
                if (curIdx < actSeq.d20ActArrayNum)
                {
                    var d20a = actSeq.d20ActArray[curIdx];

                    if (InterruptNonCounterspell(d20a))
                        return;

                    if (d20a.d20ActType == D20ActionType.CAST_SPELL)
                    {
                        var d20SpellData = d20a.d20SpellData;
                        if (GameSystems.D20.D20QueryWithObject(actSeq.performer, D20DispatcherKey.QUE_SpellInterrupted,
                                d20SpellData) != 0)
                        {
                            d20a.d20Caf &= ~D20CAF.NEED_ANIM_COMPLETED;
                            GameSystems.Anim.Interrupt(actSeq.performer, AnimGoalPriority.AGP_5);
                        }
                    }
                }

                sequencePerform();
                return;
            }

            // clean readied actions for obj
            ReadyVsRemoveForObj(obj);

            // clean previously occupied sequences
            globD20ActnSetPerformer(obj);

            foreach (var actionSequence in actSeqArray)
            {
                if (actionSequence.IsPerforming && actionSequence.performer == obj)
                {
                    // TODO: Remove from array
                    actionSequence.IsPerforming = false;
                    Logger.Info("Clearing outstanding sequence [{0}]", actionSequence);
                }
            }

            // allocate new sequence
            AllocSeq(obj);

            // apply newround condition and do BeginRound stuff
            if (GameSystems.D20.D20Query(obj, D20DispatcherKey.QUE_NewRound_This_Turn) == 0)
            {
                GameSystems.Combat.DispatchBeginRound(obj, 1);
                obj.AddCondition("NewRound_This_Turn");
            }

            // dispatch TurnBasedStatusInit
            var tbStat = CurrentSequence.tbStatus;
            tbStat.hourglassState = HourglassState.FULL;
            tbStat.tbsFlags = 0;
            tbStat.idxSthg = -1;
            tbStat.surplusMoveDistance = 0;
            tbStat.attackModeCode = 0;
            tbStat.baseAttackNumCode = 0;
            tbStat.numBonusAttacks = 0;
            tbStat.numAttacks = 0;
            tbStat.errCode = ActionErrorCode.AEC_OK;

            DispIOTurnBasedStatus dispIo = new DispIOTurnBasedStatus();
            dispIo.tbStatus = tbStat;
            dispatchTurnBasedStatusInit(obj, dispIo);

            // Enqueue simuls
            GameSystems.D20.Actions.SimulsEnqueue();

            if (GameSystems.Party.IsPlayerControlled(obj) && GameSystems.Critter.IsDeadOrUnconscious(obj))
            {
                Logger.Info("Action for {0} ending turn (unconscious)...",
                    GameSystems.MapObject.GetDisplayName(globD20Action.d20APerformer));
                GameSystems.Combat.AdvanceTurn(obj);
                return;
            }

            if (obj.HasFlag(ObjectFlag.OFF))
            {
                Logger.Info("Action for {0} ending turn (ObjectFlag.OFF)",
                    GameSystems.MapObject.GetDisplayName(globD20Action.d20APerformer));
                GameSystems.Combat.AdvanceTurn(obj);
                return;
            }

            if (GameSystems.D20.D20Query(obj, D20DispatcherKey.QUE_Prone) != 0)
            {
                if (CurrentSequence.tbStatus.hourglassState >= HourglassState.MOVE)
                {
                    GlobD20ActnInit();
                    globD20Action.d20ActType = D20ActionType.STAND_UP;
                    globD20Action.data1 = 0;
                    ActionAddToSeq();
                    sequencePerform();
                }
            }
        }

        private static bool HasCustomCombatScript(GameObjectBody obj)
        {
            if (obj == null)
            {
                return false;
            }

            var combatStartScript = obj.GetScript(obj_f.scripts_idx, (int) ObjScriptEvent.StartCombat);
            return combatStartScript.scriptId != 0;
        }

        [TempleDllLocation(0x100922B0)]
        private void SimulsEnqueue()
        {
            if (!Globals.Config.ConcurrentTurnsEnabled)
            {
                return;
            }

            var obj = GameSystems.D20.Initiative.CurrentActor;

            if (numSimultPerformers > 0 && _simultPerformerQueue.Contains(obj))
            {
                return;
            }

            numSimultPerformers = 0;
            _simultPerformerQueue = new List<GameObjectBody> {null};

            // If any actions are readied, do not allow simultaneous turns
            if (_readiedActions.Count > 0)
            {
                return;
            }

            if (actSeqInterrupt == null && !GameSystems.Party.IsInParty(obj) && !HasCustomCombatScript(obj))
            {
                Logger.Info("Building List: ");
                Logger.Info("\t{0}\n", obj);
                _simultPerformerQueue[numSimultPerformers] = obj;
                numSimultPerformers++;

                var currentInitiativeIndex = GameSystems.D20.Initiative.CurrentActorIndex;
                var iVar2 = GameSystems.D20.Initiative.Count;

                // Iterate through the initiative list starting at the position of the current actor
                // and find anyone in order of their initiative who might go at the same time as the actor.
                var idx = (currentInitiativeIndex + 1) % GameSystems.D20.Initiative.Count;

                while (idx != currentInitiativeIndex)
                {
                    var nextInInitiative = GameSystems.D20.Initiative[idx];
                    if (GameSystems.Party.IsInParty(nextInInitiative))
                    {
                        // Seek through the initiative until we find the next party member
                        break;
                    }

                    // If the critter is hostile towards any of the currently queued simulatenous performers,
                    // he cannot go at the same time.
                    for (var i = 0; i < numSimultPerformers; i++)
                    {
                        if (!GameSystems.Critter.IsFriendly(_simultPerformerQueue[i], nextInInitiative))
                        {
                            break;
                        }
                    }

                    // Custom combat scripts prevent simultaneous turns
                    if (HasCustomCombatScript(nextInInitiative))
                    {
                        break;
                    }

                    Logger.Info("\t{0}\n", nextInInitiative);

                    _simultPerformerQueue[numSimultPerformers++] = nextInInitiative;

                    if (numSimultPerformers > MaxSimultPerformers)
                        break;

                    // Wrap around to the start of the initiative list, if needed
                    idx = (idx + 1) % GameSystems.D20.Initiative.Count;
                }

                if (numSimultPerformers == 1)
                {
                    numSimultPerformers = 0;
                }

                _simultPerformerQueue[numSimultPerformers] = null;
                simulsIdx = 0;
            }
        }

        [TempleDllLocation(0x10099320)]
        private bool InterruptNonCounterspell(D20Action action)
        {
            var readiedAction = ReadiedActionGetNext(null, action);
            if (readiedAction == null)
            {
                return false;
            }

            while (true)
            {
                if (readiedAction.readyType != ReadyVsTypeEnum.RV_Counterspell)
                {
                    var isInterruptibleAction = action.d20ActType == D20ActionType.CAST_SPELL;
                    // added interruption to use of scrolls and such
                    if (action.d20ActType == D20ActionType.USE_ITEM && action.d20SpellData.spellEnumOrg != 0)
                    {
                        isInterruptibleAction = true;
                    }

                    if (isInterruptibleAction)
                    {
                        if (action.d20APerformer != null && readiedAction.interrupter != null)
                        {
                            var isFriendly =
                                GameSystems.Critter.IsFriendly(readiedAction.interrupter, action.d20APerformer);
                            var sharedAlleg =
                                GameSystems.Critter.NpcAllegianceShared(readiedAction.interrupter,
                                    action.d20APerformer);
                            if (!sharedAlleg && !isFriendly)
                                break;
                        }
                    }

                    if (action.d20ActType == D20ActionType.READIED_INTERRUPT)
                    {
                        if (action.d20ATarget != null && readiedAction.interrupter != null)
                        {
                            var isFriendly =
                                GameSystems.Critter.IsFriendly(readiedAction.interrupter, action.d20ATarget);
                            var sharedAlleg =
                                GameSystems.Critter.NpcAllegianceShared(readiedAction.interrupter, action.d20ATarget);
                            if (!sharedAlleg && !isFriendly)
                                break;
                        }
                    }
                }

                readiedAction = ReadiedActionGetNext(readiedAction, action);
                if (readiedAction == null)
                {
                    return false;
                }
            }

            InterruptSwitchActionSequence(readiedAction);
            return true;
        }

        [TempleDllLocation(0x10098ac0)]
        void InterruptSwitchActionSequence(ReadiedActionPacket readiedAction)
        {
            if (readiedAction.readyType == ReadyVsTypeEnum.RV_Counterspell)
            {
                return addresses.Counterspell_sthg(readiedAction);
            }

            CurrentSequence.interruptSeq = actSeqInterrupt;
            actSeqInterrupt = CurrentSequence;
            GameSystems.D20.Combat.FloatCombatLine(CurrentSequence.performer, 158); // Action Interrupted
            Logger.Debug("{0} interrupted by {1}!", GameSystems.MapObject.GetDisplayName(CurrentSequence.performer),
                GameSystems.MapObject.GetDisplayName(readiedAction.interrupter));
            GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(7, readiedAction.interrupter,
                actSeqInterrupt.performer);
            AssignSeq(readiedAction.interrupter);
            CurrentSequence.prevSeq = null;
            var curActor = GameSystems.D20.Initiative.CurrentActor;
            GameSystems.D20.Initiative.SetInitiativeTo(readiedAction.interrupter, curActor);
            GameSystems.D20.Initiative.CurrentActor = readiedAction.interrupter;
            CurrentSequence.performer = readiedAction.interrupter;
            globD20ActnSetPerformer(readiedAction.interrupter);
            CurrentSequence.tbStatus.hourglassState = HourglassState.PARTIAL;
            CurrentSequence.tbStatus.tbsFlags = 0;
            CurrentSequence.tbStatus.idxSthg = -1;
            CurrentSequence.tbStatus.surplusMoveDistance = 0;
            CurrentSequence.tbStatus.attackModeCode = 0;
            CurrentSequence.tbStatus.baseAttackNumCode = 0;
            CurrentSequence.tbStatus.numBonusAttacks = 0;
            CurrentSequence.tbStatus.numAttacks = 0;
            CurrentSequence.tbStatus.errCode = 0;
            CurrentSequence.IsPerforming = false;
            GlobD20ActnInit();
            readiedAction.flags = 0;
            GameSystems.Party.ClearSelection();
            if (GameSystems.Party.IsPlayerControlled(readiedAction.interrupter))
            {
                GameSystems.Party.AddToSelection(readiedAction.interrupter);
            }
            else
            {
                GameSystems.Combat.TurnProcessAi(readiedAction.interrupter);
            }

            if (GameSystems.D20.D20Query(readiedAction.interrupter, D20DispatcherKey.QUE_Prone) != 0)
            {
                if (CurrentSequence.tbStatus.hourglassState >= HourglassState.MOVE)
                {
                    GlobD20ActnInit();
                    globD20Action.d20ActType = D20ActionType.STAND_UP;
                    globD20Action.data1 = 0;
                    ActionAddToSeq();
                    sequencePerform();
                }
            }

            GameUiBridge.UpdateCombatUi();
        }

        [TempleDllLocation(0x100920e0)]
        public GameObjectBody getNextSimulsPerformer()
        {
            if (simulsIdx + 1 < numSimultPerformers)
            {
                return _simultPerformerQueue[simulsIdx + 1];
            }
            else
            {
                return null;
            }
        }

        [TempleDllLocation(0x10092110)]
        public bool IsSimulsCompleted()
        {
            var currentActor = GameSystems.D20.Initiative.CurrentActor;
            for (var index = 0; index < numSimultPerformers; index++)
            {
                var performer = _simultPerformerQueue[index];
                if (currentActor == performer && index < numSimultPerformers - 1)
                {
                    var actorName = GameSystems.MapObject.GetDisplayName(performer);
                    Logger.Info("Actor {0} not completed issuing instructions", actorName);
                    return false;
                }

                foreach (var actionSequence in actSeqArray)
                {
                    if (actionSequence.performer == performer)
                    {
                        var actorName = GameSystems.MapObject.GetDisplayName(performer);
                        Logger.Info("Actor {0} not completed", actorName);
                        return false;
                    }
                }
            }

            return true;
        }

        [TempleDllLocation(0x100925b0)]
        public bool IsLastSimultPopped(GameObjectBody obj)
        {
            return obj == _simultPerformerQueue[numSimultPerformers];
        }
    }
}
using System;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.Combat
{
    public class InitiativePortraitButton : WidgetButtonBase
    {
        private readonly InitiativeMetrics _metrics;

        private readonly GameObjectBody _combatant;

        private readonly bool _smallMode;

        private readonly WidgetImage _frame;

        private readonly WidgetImage _portrait;

        private readonly WidgetImage _highlight;

        // Was previously a property of mButton. No idea what it does!
        private int field8C = -1;

        private InitiativeUi InitiativeUi => UiSystems.Combat.Initiative;

        public InitiativePortraitButton(GameObjectBody combatant, bool smallMode)
        {
            _combatant = combatant;
            _smallMode = smallMode;
            _metrics = smallMode ? InitiativeMetrics.Small : InitiativeMetrics.Normal;

            SetPos(_metrics.Button.Location);
            SetSize(_metrics.Button.Size);

            _portrait = new WidgetImage(GetPortraitPath());
            AddContent(_portrait); // This is for automated cleanup

            string frameTexture;
            if (_smallMode)
            {
                frameTexture = "art/interface/COMBAT_UI/COMBAT_INITIATIVE_UI/PortraitFrame_Mini.tga";
            }
            else
            {
                frameTexture = "art/interface/COMBAT_UI/COMBAT_INITIATIVE_UI/PortraitFrame.tga";
            }

            _frame = new WidgetImage(frameTexture);
            AddContent(_frame); // This is for automated cleanup

            string highlightTexture;
            if (_smallMode)
            {
                highlightTexture = "art/interface/COMBAT_UI/COMBAT_INITIATIVE_UI/Highlight_Mini.tga";
            }
            else
            {
                highlightTexture = "art/interface/COMBAT_UI/COMBAT_INITIATIVE_UI/Highlight_Red.tga";
            }

            _highlight = new WidgetImage(highlightTexture);
            AddContent(_highlight); // This is for automated cleanup
        }

        [TempleDllLocation(0x10141a50)]
        public override void RenderTooltip(int x, int y)
        {
            if (ButtonState == LgcyButtonState.Disabled)
            {
                return;
            }

            TooltipContent = UiSystems.Tooltip.GetObjectDescriptionContent(_combatant, null);
            base.RenderTooltip(x, y);
        }

        [TempleDllLocation(0x10141810)]
        public override void Render()
        {
            if (!Visible)
            {
                return;
            }

            RenderFrame();
            RenderPortrait();

            if (GameSystems.D20.Actions.IsCurrentlyActing(_combatant))
            {
                RenderHighlight();
            }

            // TODO: This should be on mouseover, not on render
            if (!GameSystems.Critter.IsConcealed(_combatant))
            {
                if (ButtonState == LgcyButtonState.Hovered)
                {
                    UiSystems.InGameSelect.Focus = _combatant;
                }
                else if (ButtonState == LgcyButtonState.Down)
                {
                    UiSystems.InGameSelect.AddToFocusGroup(_combatant);
                }
            }
        }

        [TempleDllLocation(0x101428d0)]
        public override bool HandleMessage(Message msg)
        {
            var initiativeIndex = GameSystems.D20.Initiative.IndexOf(_combatant);
            if (msg.type == MessageType.WIDGET)
            {
                var widgetArgs = msg.WidgetArgs;
                var evt = widgetArgs.widgetEventType;
                switch (evt)
                {
                    case TigMsgWidgetEvent.Clicked:
                        if (!InitiativeUi.uiPortraitState1)
                        {
                            InitiativeUi.initiativeSwapSourceIndex = initiativeIndex;
                            InitiativeUi.initiativeSwapTargetIndex = initiativeIndex;
                            InitiativeUi.uiPortraitState1 = true;
                            InitiativeUi.actorCanChangeInitiative =
                                GameSystems.D20.Actions.ActorCanChangeInitiative(_combatant);
                        }

                        return true;
                    case TigMsgWidgetEvent.MouseReleased:
                    case TigMsgWidgetEvent.MouseReleasedAtDifferentButton:
                        if (InitiativeUi.uiPortraitState1 && InitiativeUi.draggingPortrait)
                        {
                            InitiativeUi.draggingPortrait = false;
                        }

                        InitiativeUi.uiPortraitState1 = false;
                        if (InitiativeUi.swapPortraitsForDragAndDrop)
                        {
                            var swapTarget = InitiativeUi.initiativeSwapTargetIndex;
                            var swapSourceCritter = GameSystems.D20.Initiative[InitiativeUi.initiativeSwapSourceIndex];
                            GameSystems.D20.Actions.SwapInitiativeWith(swapSourceCritter, swapTarget);
                            InitiativeUi.swapPortraitsForDragAndDrop = false;
                            if (field8C == -1)
                            {
                                UiSystems.Combat.Initiative.UpdateIfNeeded();
                                return false;
                            }
                        }
                        else
                        {
                            if (evt == TigMsgWidgetEvent.MouseReleasedAtDifferentButton)
                            {
                                InitiativeUi.UpdateIfNeeded();
                                UiSystems.TurnBased.sub_101749D0();
                                return true;
                            }

                            if (!GameSystems.Critter.IsConcealed(_combatant))
                            {
                                GameSystems.Scroll.CenterOnSmooth(_combatant);
                            }

                            UiSystems.TurnBased.sub_101749D0();
                        }

                        return true;
                    case TigMsgWidgetEvent.Entered:
                        if (InitiativeUi.uiPortraitState1 && InitiativeUi.actorCanChangeInitiative)
                        {
                            InitiativeUi.initiativeSwapTargetIndex = initiativeIndex;
                            InitiativeUi.swapPortraitsForDragAndDrop =
                                initiativeIndex != InitiativeUi.initiativeSwapSourceIndex;
                            UiSystems.Combat.Initiative.UpdateIfNeeded();
                        }

                        if (!GameSystems.Critter.IsConcealed(_combatant))
                        {
                            UiSystems.TurnBased.TargetFromPortrait(_combatant);
                        }

                        return true;
                    case TigMsgWidgetEvent.Exited:
                        InitiativeUi.initiativeSwapTargetIndex = InitiativeUi.initiativeSwapSourceIndex;
                        InitiativeUi.swapPortraitsForDragAndDrop = false;
                        UiSystems.TurnBased.TargetFromPortrait(null);
                        return true;
                    default:
                        UiSystems.Combat.Initiative.UpdateIfNeeded();
                        return false;
                }
            }

            if (msg.type == MessageType.MOUSE)
            {
                var mouseArgs = msg.MouseArgs;
                if ((mouseArgs.flags & MouseEventFlag.LeftDown) != 0
                    && InitiativeUi.uiPortraitState1 && !InitiativeUi.draggingPortrait)
                {
                    InitiativeUi.draggingPortrait = true;
                }

                return true;
            }

            UiSystems.Combat.Initiative.UpdateIfNeeded();
            return false;
        }

        [TempleDllLocation(0x10141780)]
        private void RenderFrame()
        {
            var contentArea = GetContentArea();
            // This was previously drawn in the context of the parent container
            contentArea.Offset(-X, -Y);
            contentArea.Offset(1, 1);
            contentArea.Size = _metrics.FrameSize;

            _frame.SetBounds(contentArea);
            _frame.Render();
        }

        private void RenderPortrait()
        {
            var contentArea = GetContentArea();

            var portraitRect = contentArea;
            portraitRect.Offset(_metrics.PortraitOffset);

            _portrait.SetBounds(portraitRect);
            _portrait.Render();
        }

        private void RenderHighlight()
        {
            var contentArea = GetContentArea();
            var highlightRect = _metrics.HighlightFrame;
            highlightRect.Offset(contentArea.Location);

            _highlight.SetBounds(highlightRect);
            _highlight.Render();
        }

        private string GetPortraitPath()
        {
            var isDead = GameSystems.Critter.IsDeadOrUnconscious(_combatant);
            var portraitId = _combatant.GetInt32(obj_f.critter_portrait);

            PortraitVariant variant;
            if (_smallMode)
            {
                variant = isDead ? PortraitVariant.MediumGrey : PortraitVariant.Medium;
            }

            else
            {
                variant = isDead ? PortraitVariant.SmallGrey : PortraitVariant.Small;
            }

            return GameSystems.UiArtManager.GetPortraitPath(portraitId, variant);
        }
    }
}
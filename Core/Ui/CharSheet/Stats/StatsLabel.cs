using System.Drawing;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.CharSheet.Stats
{
    public class StatsLabel : WidgetButtonBase
    {
        private readonly WidgetImage _hoverImage;

        private readonly WidgetImage _downImage;

        private readonly WidgetText _label;

        private readonly Stat _stat;

        public StatsLabel(
            Stat stat,
            string helpTopic,
            Rectangle rect,
            StatsUiTexture downImage,
            StatsUiTexture hoverImage,
            StatsUiParams uiParams) : base(rect)
        {
            _stat = stat;
            _downImage = new WidgetImage(uiParams.TexturePaths[downImage]);
            _hoverImage = new WidgetImage(uiParams.TexturePaths[hoverImage]);

            var statName = GetStatName(uiParams, stat);

            _label = new WidgetText(statName, "char-ui-stat-label");
            SetWidgetMsgHandler(msg =>
            {
                if (msg.widgetEventType == TigMsgWidgetEvent.Exited)
                {
                    UiSystems.CharSheet.Help.ClearHelpText();
                    return true;
                }

                return false;
            });
            if (helpTopic != null)
            {
                SetClickHandler(() => { GameSystems.Help.ShowTopic(helpTopic); });
            }
        }

        private static string GetStatName(StatsUiParams uiParams, Stat stat)
        {
            if (stat == Stat.initiative_bonus || stat == Stat.movement_speed)
            {
                return GameSystems.Stat.GetStatName(stat);
            }
            else if (stat == Stat.melee_attack_bonus)
            {
                return uiParams.PrimaryAtkLabelText;
            }
            else if (stat == Stat.ranged_attack_bonus)
            {
                return uiParams.SecondaryAtkLabelText;
            }
            else
            {
                return GameSystems.Stat.GetStatShortName(stat);
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                _downImage.Dispose();
                _hoverImage.Dispose();
            }
        }

        public override void Render()
        {
            WidgetImage renderImage = null;
            if (ButtonState == LgcyButtonState.Hovered)
            {
                renderImage = _hoverImage;
            }
            else if (ButtonState == LgcyButtonState.Down)
            {
                renderImage = _downImage;
            }

            var contentArea = GetContentArea();

            if (renderImage != null)
            {
                renderImage.SetBounds(
                    new Rectangle(
                        contentArea.X,
                        contentArea.Y - 1,
                        renderImage.GetPreferredSize().Width,
                        renderImage.GetPreferredSize().Height
                    )
                );
                renderImage.Render();
            }

            var labelSize = _label.GetPreferredSize();
            // Center horizontally and vertically within the content area
            var labelArea = new Rectangle(
                contentArea.X + (contentArea.Width - labelSize.Width) / 2,
                contentArea.Y + (contentArea.Height - labelSize.Height) / 2,
                contentArea.Width = labelSize.Width,
                contentArea.Height = labelSize.Height
            );

            // Special cases for certain labels
            if (_stat == Stat.level)
            {
                labelArea.X = contentArea.X + 2;
            }
            else if (_stat == Stat.initiative_bonus)
            {
                labelArea.X = contentArea.X + 1;
            }

            _label.SetBounds(labelArea);
            _label.Render();
        }
    }
}
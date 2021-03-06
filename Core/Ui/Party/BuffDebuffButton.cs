using System;
using System.Drawing;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.Party
{
    public class BuffDebuffButton : WidgetButtonBase
    {
        private readonly WidgetImage _image;

        private readonly WidgetTooltipRenderer _tooltipRenderer = new ();

        public BuffDebuffType Type { get; }

        public int Index { get; }

        public string Tooltip
        {
            set => _tooltipRenderer.TooltipText = value;
        }

        public string HelpTopic { get; set; }

        public string IconPath
        {
            set => _image.SetTexture(value);
        }

        public BuffDebuffButton(Rectangle rect, BuffDebuffType type, int index) : base(rect)
        {
            _image = new WidgetImage(null);
            _image.SourceRect = new Rectangle(Point.Empty, rect.Size);
            AddContent(_image);
            Type = type;
            Index = index;
            SetClickHandler(ShowHelpTopic);

            _tooltipRenderer.TooltipStyle = "buffdebuff-tooltip";
        }

        [TempleDllLocation(0x101323d0)]
        private void ShowHelpTopic()
        {
            if (HelpTopic != null)
            {
                GameSystems.Help.ShowTopic(HelpTopic);
            }
        }

        [TempleDllLocation(0x10131ea0)]
        public override void RenderTooltip(int x, int y)
        {
            if (ButtonState == LgcyButtonState.Disabled)
            {
                return;
            }

            _tooltipRenderer.Render(x, y);
        }
    }
}
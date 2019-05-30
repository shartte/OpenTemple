using System.Drawing;

namespace SpicyTemple.Core.Ui.WidgetDocs
{
    public class WidgetTooltipRenderer
    {

        private TooltipStyle _tooltipStyle;
        private string _tooltipText;
        private WidgetLegacyText _tooltipLabel;

        public TooltipStyle TooltipStyle
        {
            get => _tooltipStyle;
            set
            {
                _tooltipStyle = value;
                UpdateTooltipLabel();
            }
        }

        public string TooltipText
        {
            get => _tooltipText;
            set
            {
                _tooltipText = value;
                UpdateTooltipLabel();
            }
        }

        private void UpdateTooltipLabel()
        {
            if (_tooltipStyle == null || _tooltipText == null)
            {
                _tooltipLabel = null;
                return;
            }

            if (_tooltipLabel == null)
            {
                _tooltipLabel = new WidgetLegacyText(_tooltipText, _tooltipStyle.Font, _tooltipStyle.TextStyle);
            }
            else
            {
                _tooltipLabel.Text = _tooltipText;
            }
        }


        public void Render(int x, int y)
        {
            if (TooltipStyle != null && TooltipText != null)
            {
                var preferredSize = _tooltipLabel.GetPreferredSize();
                var contentArea = new Rectangle(
                    x, y - preferredSize.Height,
                    preferredSize.Width, preferredSize.Height
                );
                _tooltipLabel.SetContentArea(contentArea);
                _tooltipLabel.Render();
            }
        }
    }
}
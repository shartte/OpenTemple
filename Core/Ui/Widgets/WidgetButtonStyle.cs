#nullable enable

namespace OpenTemple.Core.Ui.Widgets
{
    public sealed class WidgetButtonStyle
    {
        public string? Id;
        public string? Inherits;
        public string? NormalImagePath;
        public string? ActivatedImagePath;
        public string? HoverImagePath;
        public string? PressedImagePath;
        public string? DisabledImagePath;
        public string? FrameImagePath;

        public string? TextStyleId;
        public string? HoverTextStyleId;
        public string? PressedTextStyleId;
        public string? DisabledTextStyleId;
        public int SoundEnter = -1;
        public int SoundLeave = -1;
        public int SoundDown = -1;
        public int SoundClick = -1;

        public WidgetButtonStyle Copy()
        {
            return (WidgetButtonStyle) MemberwiseClone();
        }

        // TODO: Trace all uses of this function in Vanilla and make the same call in the proper places in C#
        [TempleDllLocation(0x101f9660)]
        public WidgetButtonStyle UseDefaultSounds()
        {
            SoundEnter = 3010;
            SoundLeave = 3011;
            SoundClick = 3013;
            SoundDown = 3012;
            return this;
        }
    }

}
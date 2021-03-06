#nullable enable
using System;
using System.Collections.Immutable;
using System.Drawing;

namespace OpenTemple.Core.Ui.Widgets
{
    public class WidgetButton : WidgetButtonBase
    {
        private readonly WidgetText _label;

        private WidgetImage _activatedImage;
        private WidgetImage _disabledImage;
        private WidgetImage _frameImage;
        private WidgetImage _hoverImage;

        protected WidgetImage _normalImage;
        private WidgetImage _pressedImage;

        private WidgetButtonStyle _style;

        // is the state associated with the button active? Note: this is separate from mDisabled, which determines if the button itself is disabled or not
        protected bool _active = false;

        public void SetActive(bool isActive)
        {
            _active = isActive;
        }

        public bool IsActive()
        {
            return _active;
        }

        public WidgetButton()
        {
            _label = new WidgetText {Parent = this};
        }

        public WidgetButton(Rectangle rect) : this()
        {
            SetPos(rect.Location);
            SetSize(rect.Size);
        }

        /*
         central style definitions:
         templeplus/button_styles.json
         */
        public void SetStyle(WidgetButtonStyle style)
        {
            _style = style;
            sndHoverOn = style.SoundEnter;
            sndHoverOff = style.SoundLeave;
            sndDown = style.SoundDown;
            sndClick = style.SoundClick;
            UpdateContent();
        }

        /*
         directly fetch style from Globals.WidgetButtonStyles
         */
        public void SetStyle(string styleName)
        {
            SetStyle(Globals.WidgetButtonStyles.GetStyle(styleName));
        }

        public WidgetButtonStyle GetStyle()
        {
            return _style;
        }

        public override void Render()
        {
            InvokeOnBeforeRender();
            ApplyAutomaticSizing();

            base.Render();

            var contentArea = GetContentArea();

            string? labelStyle = null;

            if (mDisabled)
            {
                if (_style.DisabledTextStyleId != null)
                {
                    labelStyle = _style.DisabledTextStyleId;
                }
                else
                {
                    labelStyle = _style.TextStyleId;
                }
            }
            else
            {
                if (ButtonState == LgcyButtonState.Down)
                {
                    if (_style.PressedTextStyleId != null)
                    {
                        labelStyle = _style.PressedTextStyleId;
                    }
                    else if (_style.HoverTextStyleId != null)
                    {
                        labelStyle = _style.HoverTextStyleId;
                    }
                    else
                    {
                        labelStyle = _style.TextStyleId;
                    }
                }
                else if (IsActive())
                {
                    if (ButtonState == LgcyButtonState.Hovered
                        || ButtonState == LgcyButtonState.Released)
                    {
                        if (_style.HoverTextStyleId != null)
                        {
                            labelStyle = _style.HoverTextStyleId;
                        }
                        else
                        {
                            labelStyle = _style.TextStyleId;
                        }
                    }
                    else
                    {
                        labelStyle = _style.TextStyleId;
                    }
                }
                else if (ButtonState == LgcyButtonState.Hovered
                         || ButtonState == LgcyButtonState.Released)
                {
                    if (_style.HoverTextStyleId != null)
                    {
                        labelStyle = _style.HoverTextStyleId;
                    }
                    else
                    {
                        labelStyle = _style.TextStyleId;
                    }
                }
                else
                {
                    labelStyle = _style.TextStyleId;
                }
            }

            _label.StyleIds = labelStyle != null ? ImmutableList.Create(labelStyle) : ImmutableList<string>.Empty;

            var fr = _frameImage;
            if (fr != null)
            {
                var contentAreaWithMargins = GetContentArea(true);
                fr.SetBounds(contentAreaWithMargins);
                fr.Render();
            }

            var image = GetCurrentImage();
            if (image != null)
            {
                image.SetBounds(contentArea);
                image.Render();
            }

            _label.SetBounds(contentArea);
            _label.Render();
        }

        protected virtual WidgetImage GetCurrentImage()
        {
            // Always fall back to the default
            var image = _normalImage;

            if (mDisabled)
            {
                if (_disabledImage != null)
                {
                    image = _disabledImage;
                }
                else
                {
                    image = _normalImage;
                }
            }
            else
            {
                if (ButtonState == LgcyButtonState.Down)
                {
                    if (_pressedImage != null)
                    {
                        image = _pressedImage;
                    }
                    else if (_hoverImage != null)
                    {
                        image = _hoverImage;
                    }
                    else
                    {
                        image = _normalImage;
                    }
                }
                else if (IsActive())
                {
                    // Activated, else Pressed, else Hovered, (else Normal)
                    if (_activatedImage != null)
                    {
                        image = _activatedImage;
                    }
                    else if (_pressedImage != null)
                    {
                        image = _pressedImage;
                    }
                    else if (_hoverImage != null)
                    {
                        image = _hoverImage;
                    }
                }
                else if (ButtonState == LgcyButtonState.Hovered
                         || ButtonState == LgcyButtonState.Released)
                {
                    if (_hoverImage != null)
                    {
                        image = _hoverImage;
                    }
                    else
                    {
                        image = _normalImage;
                    }
                }
                else
                {
                    image = _normalImage;
                }
            }

            return image;
        }

        public string Text
        {
            get => _label.Text;
            set
            {
                _label.Text = value;
                UpdateAutoSize();
            }
        }

        /*
          1. updates the WidgetImage pointers below, using WidgetButtonStyle file paths
          2. Updates mLabel
         */
        private void UpdateContent()
        {
            if (_style.NormalImagePath != null)
            {
                _normalImage = new WidgetImage(_style.NormalImagePath);
            }
            else
            {
                _normalImage?.Dispose();
                _normalImage = null;
            }

            if (_style.ActivatedImagePath != null)
            {
                _activatedImage = new WidgetImage(_style.ActivatedImagePath);
            }
            else
            {
                _activatedImage?.Dispose();
                _activatedImage = null;
            }

            if (_style.HoverImagePath != null)
            {
                _hoverImage = new WidgetImage(_style.HoverImagePath);
            }
            else
            {
                _hoverImage?.Dispose();
                _hoverImage = null;
            }

            if (_style.PressedImagePath != null)
            {
                _pressedImage = new WidgetImage(_style.PressedImagePath);
            }
            else
            {
                _pressedImage?.Dispose();
                _pressedImage = null;
            }

            if (_style.DisabledImagePath != null)
            {
                _disabledImage = new WidgetImage(_style.DisabledImagePath);
            }
            else
            {
                _disabledImage?.Dispose();
                _disabledImage = null;
            }

            if (_style.FrameImagePath != null)
            {
                _frameImage = new WidgetImage(_style.FrameImagePath);
            }
            else
            {
                _frameImage?.Dispose();
                _frameImage = null;
            }

            if (_style.TextStyleId != null)
            {
                _label.AddStyle(_style.TextStyleId);
            }

            UpdateAutoSize();
        }

        private void UpdateAutoSize()
        {
            // Try to var-size
            if (mAutoSizeWidth || mAutoSizeHeight)
            {
                Size prefSize;
                if (_normalImage != null)
                {
                    prefSize = _normalImage.GetPreferredSize();
                }
                else
                {
                    prefSize = _label.GetPreferredSize();
                }

                if (_frameImage != null)
                {
                    // update margins from frame size
                    var framePrefSize = _frameImage.GetPreferredSize();
                    var marginW = framePrefSize.Width - prefSize.Width;
                    var marginH = framePrefSize.Height - prefSize.Height;
                    if (marginW > 0)
                    {
                        mMargins.Right = marginW / 2;
                        mMargins.Left = marginW - mMargins.Right;
                    }

                    if (marginH > 0)
                    {
                        mMargins.Bottom = marginH / 2;
                        mMargins.Top = marginH - mMargins.Bottom;
                    }
                }

                prefSize.Height += mMargins.Bottom + mMargins.Top;
                prefSize.Width += mMargins.Left + mMargins.Right;

                if (mAutoSizeWidth && mAutoSizeHeight)
                {
                    SetSize(prefSize);
                }
                else if (mAutoSizeWidth)
                {
                    Width = prefSize.Width;
                }
                else if (mAutoSizeHeight)
                {
                    Height = prefSize.Height;
                }
            }
        }

    }
}
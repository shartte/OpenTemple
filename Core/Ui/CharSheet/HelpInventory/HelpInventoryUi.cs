using OpenTemple.Core.GameObject;
using OpenTemple.Core.Ui.FlowModel;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.CharSheet.HelpInventory
{
    public class CharSheetHelpUi
    {
        private WidgetScrollView _scrollView;

        private WidgetContainer _textContainer;

        private WidgetText _textLabel;

        public CharSheetHelpUi()
        {
            var widgetDoc = WidgetDoc.Load("ui/char_help.json");
            _scrollView = (WidgetScrollView) widgetDoc.TakeRootWidget();

            _textContainer = new WidgetContainer(0, 0,
                _scrollView.GetInnerWidth(), _scrollView.GetInnerHeight());
            _textLabel = new WidgetText();
            _textContainer.AddContent(_textLabel);
            _scrollView.Add(_textContainer);
            _scrollView.AddStyle("char-help-text");
        }

        [TempleDllLocation(0x10BF0BC0)]
        public bool Shown { get; set; }

        public WidgetBase Container => _scrollView;

        public void Hide()
        {
            Stub.TODO();
            Shown = false;
        }

        [TempleDllLocation(0x101627a0)]
        public void Show()
        {
            Stub.TODO();
            Shown = true;
        }

        [TempleDllLocation(0x10162c00)]
        public void SetHelpText(string text)
        {
            _textLabel.Text = text;
        }
        public void SetHelpText(InlineElement content)
        {
            _textLabel.Content = content;
        }

        [TempleDllLocation(0x101628D0)]
        public InlineElement GetObjectHelp(GameObjectBody obj, GameObjectBody observer)
        {
            return UiSystems.Tooltip.GetObjectDescriptionContent(obj, observer);
        }

        public void ShowItemDescription(GameObjectBody item, GameObjectBody observer)
        {
            var text = UiSystems.CharSheet.Help.GetObjectHelp(item, observer);
            SetHelpText(text);
        }

        public void ClearHelpText() => SetHelpText("");

        [TempleDllLocation(0x10162730)]
        public void Reset()
        {
            ClearHelpText();
        }
    }
}
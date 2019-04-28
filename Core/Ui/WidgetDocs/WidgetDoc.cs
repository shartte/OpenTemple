using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Ui.Styles;

namespace SpicyTemple.Core.Ui.WidgetDocs
{
    internal class WidgetDocLoader
    {
        private readonly string _path;

        public WidgetDocLoader(string path)
        {
            _path = path;
        }

        public Dictionary<string, WidgetBase> Registry { get; } = new Dictionary<string, WidgetBase>();

        private void LoadContent(in JsonElement contentList, WidgetBase widget)
        {
            foreach (var contentJson in contentList.EnumerateArray())
            {
                var type = contentJson.GetProperty("type").GetString();

                WidgetContent content;
                switch (type)
                {
                    case "image":
                    {
                        var path = contentJson.GetProperty("path").GetString();
                        content = new WidgetImage(path);
                        break;
                    }

                    case "text":
                    {
                        var text = contentJson.GetProperty("text").GetString();
                        var styleId = contentJson.GetProperty("style").GetString();
                        var textContent = new WidgetText();
                        textContent.SetStyleId(styleId);
                        textContent.SetText(text);
                        content = textContent;
                        break;
                    }

                    default:
                        throw new Exception($"Unknown widget content type: '{type}'");
                }

                // Generic properties
                if (contentJson.TryGetProperty("x", out var xNode))
                {
                    content.SetX(xNode.GetInt32());
                }

                if (contentJson.TryGetProperty("y", out var yNode))
                {
                    content.SetY(yNode.GetInt32());
                }

                if (contentJson.TryGetProperty("width", out var widthNode))
                {
                    content.SetFixedWidth(widthNode.GetInt32());
                }

                if (contentJson.TryGetProperty("height", out var heightNode))
                {
                    content.SetFixedHeight(heightNode.GetInt32());
                }

                widget.AddContent(content);
            }
        }

        private void LoadWidgetBase(JsonElement jsonObj, WidgetBase widget)
        {
            if (jsonObj.TryGetProperty("__styleFiles", out var textStyleFiles))
            {
                foreach (var styleSheetName in textStyleFiles.EnumerateArray())
                {
                    Globals.WidgetTextStyles.LoadStylesFile(styleSheetName.GetString());
                }
            }

            if (jsonObj.TryGetProperty("__styles", out var inlineTextStyles))
            {
                Globals.WidgetTextStyles.LoadStyles(inlineTextStyles);
            }

            if (jsonObj.TryGetProperty("__buttonStyleFiles", out var buttonStyleFiles))
            {
                foreach (var style in buttonStyleFiles.EnumerateArray())
                {
                    Globals.WidgetButtonStyles.LoadStylesFile(style.GetString());
                }
            }

            if (jsonObj.TryGetProperty("__buttonStyles", out var inlineButtonStyles))
            {
                Globals.WidgetButtonStyles.LoadStyles(inlineButtonStyles);
            }

            var x = jsonObj.GetInt32Prop("x", 0);
            var y = jsonObj.GetInt32Prop("y", 0);
            widget.SetPos(x, y);

            var size = widget.GetSize();
            if (jsonObj.TryGetProperty("width", out var widthNode))
            {
                size.Width = widthNode.GetInt32();
                widget.SetAutoSizeWidth(false);
            }

            if (jsonObj.TryGetProperty("height", out var heightNode))
            {
                size.Height = heightNode.GetInt32();
                widget.SetAutoSizeHeight(false);
            }

            widget.SetSize(size);

            if (jsonObj.TryGetProperty("centerHorizontally", out var centerHorizontallyNode))
            {
                widget.SetCenterHorizontally(centerHorizontallyNode.GetBoolean());
            }

            if (jsonObj.TryGetProperty("centerVertically", out var centerVerticallyNode))
            {
                widget.SetCenterVertically(centerVerticallyNode.GetBoolean());
            }

            if (jsonObj.TryGetProperty("sizeToParent", out var sizeToParentNode))
            {
                widget.SetSizeToParent(sizeToParentNode.GetBoolean());
            }
        }

        private void LoadChildren(JsonElement jsonObj, WidgetContainer container)
        {
            foreach (var childJson in jsonObj.EnumerateArray())
            {
                var childWidget = LoadWidgetTree(childJson);
                childWidget.SetParent(container);
                container.Add(childWidget);
            }
        }

        private void LoadWidgetBaseWithContent(JsonElement jsonObj, WidgetContainer result)
        {
            LoadWidgetBase(jsonObj, result);

            if (jsonObj.TryGetProperty("content", out var contentNode))
            {
                LoadContent(contentNode, result);
            }

            if (jsonObj.TryGetProperty("children", out var childrenNode))
            {
                LoadChildren(childrenNode, result);
            }
        }

        private WidgetBase LoadWidgetScrollView(JsonElement jsonObj)
        {
            var width = jsonObj.GetInt32Prop("width", 0);
            var height = jsonObj.GetInt32Prop("height", 0);

            var result = new WidgetScrollView(width, height);

            LoadWidgetBaseWithContent(jsonObj, result);

            return result;
        }

        private WidgetBase LoadWidgetContainer(JsonElement jsonObj)
        {
            var width = jsonObj.GetInt32Prop("width", 0);
            var height = jsonObj.GetInt32Prop("height", 0);

            var result = new WidgetContainer(width, height);

            LoadWidgetBaseWithContent(jsonObj, result);

            return result;
        }

        private WidgetBase LoadWidgetButton(JsonElement jsonObj)
        {
            var result = new WidgetButton();

            LoadWidgetBase(jsonObj, result);

            result.SetText(jsonObj.GetProperty("text").GetString());

            WidgetButtonStyle buttonStyle;
            if (jsonObj.TryGetProperty("style", out var styleNode))
            {
                buttonStyle = Globals.WidgetButtonStyles.GetStyle(styleNode.GetString()).Copy();
            }
            else
            {
                buttonStyle = new WidgetButtonStyle();
            }

            // Allow local overrides
            foreach (var jsonProperty in jsonObj.EnumerateObject())
            {
                var key = jsonProperty.Name;
                switch (key)
                {
                    case "disabledImage":
                        buttonStyle.disabledImagePath = jsonProperty.Value.GetString();
                        break;
                    case "activatedImage":
                        buttonStyle.activatedImagePath = jsonProperty.Value.GetString();
                        break;
                    case "normalImage":
                        buttonStyle.normalImagePath = jsonProperty.Value.GetString();
                        break;
                    case "hoverImage":
                        buttonStyle.hoverImagePath = jsonProperty.Value.GetString();
                        break;
                    case "pressedImage":
                        buttonStyle.pressedImagePath = jsonProperty.Value.GetString();
                        break;
                    case "frameImage":
                        buttonStyle.frameImagePath = jsonProperty.Value.GetString();
                        break;
                    case "textStyle":
                        buttonStyle.textStyleId = jsonProperty.Value.GetString();
                        break;
                    case "hoverTextStyle":
                        buttonStyle.hoverTextStyleId = jsonProperty.Value.GetString();
                        break;
                    case "pressedTextStyle":
                        buttonStyle.pressedTextStyleId = jsonProperty.Value.GetString();
                        break;
                    case "disabledTextStyle":
                        buttonStyle.disabledTextStyleId = jsonProperty.Value.GetString();
                        break;
                }
            }

            result.SetStyle(buttonStyle);

            return result;
        }

        private WidgetBase LoadWidgetScrollBar(JsonElement jsonObj)
        {
            var result = new WidgetScrollBar();

            LoadWidgetBase(jsonObj, result);

            return result;
        }

        public WidgetBase LoadWidgetTree(JsonElement jsonObj)
        {
            var type = jsonObj.GetProperty("type").GetString();

            // Is there a factory for the type?
            WidgetBase widget;
            switch (type)
            {
                case "container":
                    widget = LoadWidgetContainer(jsonObj);
                    break;
                case "button":
                    widget = LoadWidgetButton(jsonObj);
                    break;
                case "scrollBar":
                    widget = LoadWidgetScrollBar(jsonObj);
                    break;
                case "scrollView":
                    widget = LoadWidgetScrollView(jsonObj);
                    break;
                default:
                    throw new Exception($"Cannot process unknown widget type: '{type}'");
            }

            widget.SetSourceURI(_path);

            // If the widget had an ID, put it into the registry
            if (jsonObj.TryGetProperty("id", out var idNode))
            {
                var id = idNode.GetString();
                if (Registry.ContainsKey(id))
                {
                    throw new Exception($"Duplicate widget id: {id}");
                }

                Registry[id] = widget;
                widget.SetId(id);
            }

            return widget;
        }
    }

/**
 * Contains a definition for a grabbag of widgets.
 */
    internal class WidgetDoc
    {
        private readonly string _path;
        private readonly WidgetBase _rootWidget;
        private readonly Dictionary<string, WidgetBase> _widgetsById;

        private WidgetDoc(string path, WidgetBase root, Dictionary<string, WidgetBase> registry)
        {
            _path = path;
            _rootWidget = root;
            _widgetsById = registry;
        }

        public static WidgetDoc Load(string path)
        {
            var json = Tig.FS.ReadBinaryFile(path);
            using var root = JsonDocument.Parse(json);

            try
            {
                var loader = new WidgetDocLoader(path);
                var rootWidget = loader.LoadWidgetTree(root.RootElement);

                return new WidgetDoc(path, rootWidget, loader.Registry);
            }
            catch (Exception e)
            {
                throw new Exception($"Unable to load widget doc '{path}'.", e);
            }
        }

        /**
         * Returns the root widget defined in the widget doc. The caller takes ownership of the widget.
         * This function can only be called once per widget doc instance!
         */
        public WidgetBase TakeRootWidget()
        {
            Trace.Assert(_rootWidget != null);
            return _rootWidget;
        }

        /**
          * Returns the root widget defined in the widget doc, assuming it is a container widget.
         * If the root widget is NOT a container, this method will throw an exception.
         * The caller takes ownership of the widget.
         * This function can only be called once per widget doc instance!
         */
        public WidgetContainer TakeRootContainer()
        {
            Trace.Assert(_rootWidget != null);
            if (!_rootWidget.IsContainer())
            {
                throw new Exception($"Expected root widget in '{_path}' to be a container.");
            }

            return (WidgetContainer) _rootWidget;
        }

        public WidgetBase GetWidget(string id)
        {
            if (!_widgetsById.TryGetValue(id, out var widget))
            {
                throw new Exception($"Couldn't find required widget id '{id}' in widget doc '{_path}'");
            }

            return widget;
        }

        public WidgetContainer GetWindow(string id)
        {
            var widget = GetWidget(id);
            if (!widget.IsContainer())
            {
                throw new Exception($"Expected widget with id '{id}' in doc '{_path}' to be a container!");
            }

            return (WidgetContainer) widget;
        }

        public WidgetButtonBase GetButton(string id)
        {
            var widget = GetWidget(id);
            if (!widget.IsButton())
            {
                throw new Exception($"Expected widget with id '{id}' in doc '{_path}' to be a button!");
            }

            return (WidgetButton) widget;
        }

        public WidgetScrollView GetScrollView(string id)
        {
            var widget = GetWidget(id);
            if (!widget.IsScrollView())
            {
                throw new Exception($"Expected widget with id '{id}' in doc '{_path}' to be a scroll view!");
            }

            return (WidgetScrollView) widget;
        }
    }
}
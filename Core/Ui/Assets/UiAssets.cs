using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.IO.Images;
using SpicyTemple.Core.IO.MesFiles;
using SpicyTemple.Core.TigSubsystems;

namespace SpicyTemple.Core.Ui.Assets
{
    public enum UiAssetType
    {
        Portraits = 0,
        Inventory,
        Generic, // Textures
        GenericLarge // IMG files
    };

    public enum UiGenericAsset
    {
        AcceptHover = 0,
        AcceptNormal,
        AcceptPressed,
        DeclineHover,
        DeclineNormal,
        DeclinePressed,
        DisabledNormal,
        GenericDialogueCheck
    };

    public class UiAssets
    {
        public UiAssets()
        {
            mTranslationFiles["main_menu"] = Tig.FS.ReadMesFile("mes/mainmenu.mes");
        }

        /**
         * Replaces placeholders of the form #{main_menu:123} with the key 123 from the mes file registered
         * as main_menu.
         */
        public string ApplyTranslation(string text)
        {
            StringBuilder result = new StringBuilder(text.Length);
            StringBuilder mesFilename = new StringBuilder();
            for (int i = 0; i < text.Length; i++)
            {
                if (!IsStartOfTranslation(text, i))
                {
                    result.Append(text[i]);
                    continue;
                }

                var firstToken = i; // If parsing fails, we append the original
                mesFilename.Clear();

                // Start pushing back tokens until we reach the marker or end of translation
                bool terminated = false;
                for (i = i + 2; i < text.Length; i++)
                {
                    if (text[i] == ':' || text[i] == '}')
                    {
                        terminated = text[i] == ':';
                        break;
                    }
                    else
                    {
                        mesFilename.Append(text[i]);
                    }
                }

                if (!terminated)
                {
                    result.Append(text.Substring(firstToken, i - firstToken));
                    continue;
                }


                if (!mTranslationFiles.TryGetValue(mesFilename.ToString(), out var translationDict))
                {
                    result.Append(text.Substring(firstToken, i - firstToken));
                    continue;
                }

                // Parse the mes id now
                terminated = false;
                StringBuilder mesLine = new StringBuilder();
                for (i = i + 1; i < text.Length; i++)
                {
                    if (text[i] == '}')
                    {
                        terminated = true;
                        break;
                    }
                    else
                    {
                        mesLine.Append(text[i]);
                    }
                }

                if (!terminated)
                {
                    result.Append(text.Substring(firstToken, i - firstToken));
                    continue;
                }

                if (!int.TryParse(mesLine.ToString(), out var mesLineNo))
                {
                    result.Append(text.Substring(firstToken, i - firstToken));
                    continue;
                }

                result.Append(translationDict[mesLineNo]);
            }

            return result.ToString();
        }

        /* TODO
        public bool GetAsset(UiAssetType assetType, UiGenericAsset assetIndex, out int textureIdOut) {
            static var ui_get_common_texture_id = temple.GetPointer<signed int(UiAssetType assetType, UiGenericAsset assetIdx, int& textureIdOut, int a4)>(0x1004a360);
            return ui_get_common_texture_id(assetType, assetIndex, textureIdOut, 0) == 0;
        }*/

        // Loads a .img file.
        [TempleDllLocation(0x101e8320)]
        public ResourceRef<ITexture> LoadImg(string filename)
        {
            return Tig.Textures.Resolve(filename, false);
        }

        /*
        public string GetTooltipString(int line)
        {
            var getTooltipString = temple.GetRef<const CHAR* (__cdecl)(int)>(0x10122DA0);
            return getTooltipString(line);
        }

        public string GetStatShortName(Stat stat)
        {
            return temple.GetRef<const CHAR* (__cdecl)(Stat)>(0x10074980)(stat);
        }

        public string GetStatMesLine(int line)
        {
            var mesHandle = temple.GetRef<MesHandle>(0x10AAF1F4);
            MesLine line(lineNumber);
            mesFuncs.GetLine_Safe(mesHandle, line);
            return line.value;
        }*/

        public static bool IsStartOfTranslation(ReadOnlySpan<char> text, int pos)
        {
            // #{} Is minimal
            if (pos + 2 >= text.Length)
            {
                return false;
            }

            return (text[pos] == '#' && text[pos + 1] == '{');
        }

        private readonly Dictionary<string, Dictionary<int, string>> mTranslationFiles
            = new Dictionary<string, Dictionary<int, string>>();
    };
}
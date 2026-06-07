using System;
using G_Formatter.Models;

namespace G_Formatter.Services
{
    public static class TextFormatter
    {
        public static string ApplyFormatToggle(string text, BBCodeTag formatTag)
        {
            return BBCodeParser.ToggleTag(text, formatTag);
        }

        public static string ApplyColor(string text, BBCodeTag colorTag)
        {
            return BBCodeParser.ChangeColor(text, colorTag);
        }

        public static string ClearAllFormat(string text)
        {
            return BBCodeParser.StripAllTags(text);
        }

        public static string ClearOnlyColor(string text)
        {
            return BBCodeParser.StripColorTags(text);
        }

        public static string ClearOnlyFormat(string text)
        {
            return BBCodeParser.StripFormatTags(text);
        }
    }
}
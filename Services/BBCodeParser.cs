using System.Collections.Generic;
using System.Text.RegularExpressions;
using G_Formatter.Models;

namespace G_Formatter.Services
{
    public static class BBCodeParser
    {
        public static string WrapWithTag(string text, BBCodeTag tag)
        {
            if (string.IsNullOrEmpty(text)) return text;
            if (HasTag(text, tag)) return text;
            return tag.OpenTag + text + tag.CloseTag;
        }

        public static string RemoveTag(string text, BBCodeTag tag)
        {
            if (string.IsNullOrEmpty(text)) return text;
            string pattern = Regex.Escape(tag.OpenTag) + "(.*?)" + Regex.Escape(tag.CloseTag);
            var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return text.Remove(match.Index, match.Length).Insert(match.Index, match.Groups[1].Value);
            }
            return text;
        }

        public static bool HasTag(string text, BBCodeTag tag)
        {
            if (string.IsNullOrEmpty(text)) return false;
            string pattern = Regex.Escape(tag.OpenTag) + ".*?" + Regex.Escape(tag.CloseTag);
            return Regex.IsMatch(text, pattern, RegexOptions.IgnoreCase);
        }

        public static string StripColorTags(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            string result = text;
            foreach (var tag in BBCodeTag.ColorTags)
            {
                result = StripTag(result, tag);
            }
            return result;
        }

        public static string StripFormatTags(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            string result = text;
            foreach (var tag in BBCodeTag.FormatTags)
            {
                result = StripTag(result, tag);
            }
            return result;
        }

        public static string ChangeColor(string text, BBCodeTag newColorTag)
        {
            if (string.IsNullOrEmpty(text)) return text;
            text = StripColorTags(text);
            return WrapWithTag(text, newColorTag);
        }

        public static string ToggleTag(string text, BBCodeTag tag)
        {
            if (HasTag(text, tag)) return RemoveTag(text, tag);
            else return WrapWithTag(text, tag);
        }

        private static string StripTag(string text, BBCodeTag tag)
        {
            string pattern = Regex.Escape(tag.OpenTag) + "(.*?)" + Regex.Escape(tag.CloseTag);
            while (Regex.IsMatch(text, pattern, RegexOptions.IgnoreCase))
            {
                text = Regex.Replace(text, pattern, "$1", RegexOptions.IgnoreCase);
            }
            return text;
        }

        public static string StripAllTags(string text)
        {
            return StripColorTags(StripFormatTags(text));
        }
    }
}
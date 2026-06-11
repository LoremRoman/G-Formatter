using System.Collections.Generic;
using System.Text.RegularExpressions;
using G_Formatter.Models;
using System.Linq;

namespace G_Formatter.Services
{
    public static class BBCodeParser
    {

        private static string RebuildWithHierarchy(string rawText, bool hasB, bool hasU, bool hasI, BBCodeTag activeColor)
        {
            string result = rawText;

            if (activeColor != null) result = activeColor.OpenTag + result + activeColor.CloseTag;
            if (hasI) result = BBCodeTag.FormatTags[2].OpenTag + result + BBCodeTag.FormatTags[2].CloseTag;
            if (hasU) result = BBCodeTag.FormatTags[1].OpenTag + result + BBCodeTag.FormatTags[1].CloseTag;
            if (hasB) result = BBCodeTag.FormatTags[0].OpenTag + result + BBCodeTag.FormatTags[0].CloseTag;

            return result;
        }

        public static string ToggleFormatAndRebuild(string text, BBCodeTag toggleTag)
        {
            if (string.IsNullOrEmpty(text)) return text;

            bool hasB = HasTag(text, BBCodeTag.FormatTags[0]);
            bool hasU = HasTag(text, BBCodeTag.FormatTags[1]);
            bool hasI = HasTag(text, BBCodeTag.FormatTags[2]);

            BBCodeTag activeColor = BBCodeTag.ColorTags.FirstOrDefault(t => HasTag(text, t));

            if (toggleTag == BBCodeTag.FormatTags[0]) hasB = !hasB;
            else if (toggleTag == BBCodeTag.FormatTags[1]) hasU = !hasU;
            else if (toggleTag == BBCodeTag.FormatTags[2]) hasI = !hasI;

            string rawText = StripAllTags(text);
            return RebuildWithHierarchy(rawText, hasB, hasU, hasI, activeColor);
        }

        public static string ChangeColorAndRebuild(string text, BBCodeTag newColorTag)
        {
            if (string.IsNullOrEmpty(text)) return text;

            bool hasB = HasTag(text, BBCodeTag.FormatTags[0]);
            bool hasU = HasTag(text, BBCodeTag.FormatTags[1]);
            bool hasI = HasTag(text, BBCodeTag.FormatTags[2]);

            string rawText = StripAllTags(text);
            return RebuildWithHierarchy(rawText, hasB, hasU, hasI, newColorTag);
        }
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
            var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
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
            return Regex.IsMatch(text, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
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
            while (Regex.IsMatch(text, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline))
            {
                text = Regex.Replace(text, pattern, "$1", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            }
            return text;
        }

        public static string StripAllTags(string text)
        {
            return StripColorTags(StripFormatTags(text));
        }
    }
}
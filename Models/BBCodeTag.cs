using System.Collections.Generic;

namespace G_Formatter.Models
{
    public enum TagType
    {
        Format,
        Color
    }

    public class BBCodeTag
    {
        public string OpenTag { get; set; }
        public string CloseTag { get; set; }
        public string DisplayName { get; set; }
        public TagType Type { get; set; }

        public BBCodeTag(string openTag, string closeTag, string displayName, TagType type)
        {
            OpenTag = openTag;
            CloseTag = closeTag;
            DisplayName = displayName;
            Type = type;
        }

        public static List<BBCodeTag> FormatTags = new List<BBCodeTag>
        {
            new BBCodeTag("[b]", "[/b]", "bold", TagType.Format),
            new BBCodeTag("[u]", "[/u]", "underlined", TagType.Format),
            new BBCodeTag("[i]", "[/i]", "italic", TagType.Format)
        };

        public static List<BBCodeTag> ColorTags = new List<BBCodeTag>
        {
            new BBCodeTag("[red]", "[/red]", "red", TagType.Color),
            new BBCodeTag("[green]", "[/green]", "green", TagType.Color),
            new BBCodeTag("[blue]", "[/blue]", "blue", TagType.Color),
            new BBCodeTag("[purple]", "[/purple]", "purple", TagType.Color),
            new BBCodeTag("[cyan]", "[/cyan]", "cyan", TagType.Color)
        };
    }
}
using System.Collections.Generic;

namespace G_Formatter.Models
{
    public class EmojiData
    {
        public string Symbol { get; set; }
        public string Name { get; set; }
        public string VisualEmoji { get; set; }

        public EmojiData(string symbol, string name, string visualEmoji)
        {
            Symbol = symbol;
            Name = name;
            VisualEmoji = visualEmoji;
        }

        private static List<EmojiData> _allEmojis;
        public static List<EmojiData> AllEmojis
        {
            get
            {
                if (_allEmojis == null)
                {
                    _allEmojis = new List<EmojiData>
                    {
                        new EmojiData("—", "Music", "🎵"),
                        new EmojiData("¥", "Star", "⭐"),
                        new EmojiData("|", "Heart", "❤️"),
                        new EmojiData("¬", "Fish", "🐟"),
                        new EmojiData("¶", "Bulb", "💡"),
                        new EmojiData("√", "Check", "✅"),
                        new EmojiData("ƒ", "Black Heart", "🖤"),
                        new EmojiData("÷", "Down Hand", "👎"),
                        new EmojiData("†", "Bomb", "💣"),
                        new EmojiData("‡", "Prohibited", "🚫"),
                        new EmojiData("Ø", "Alien", "👽"),
                        new EmojiData("»", "Clover", "♣️"),
                        new EmojiData("±", "Phone", "📱"),
                        new EmojiData("•", "Up Hand", "👍"),
                        new EmojiData("ª", "Skull", "💀"),
                        new EmojiData("º", "Ray", "⚡"),
                        new EmojiData("µ", "Coffe", "☕"),
                        new EmojiData("°", "Sun", "☀️"),
                        new EmojiData("½", "Medium", "🌙"),
                        new EmojiData("¼", "Quarter", "🌔"),
                        new EmojiData("¾", "Three-quarters", "🌖"),
                        new EmojiData("¹", "One", "1️⃣"),
                        new EmojiData("²", "Two", "2️⃣"),
                        new EmojiData("³", "Three", "3️⃣"),
                        new EmojiData("©", "Copyright", "©️"),
                        new EmojiData("®", "Registered", "®️"),
                        new EmojiData("™", "Trademark", "™️")
                    };
                }
                return _allEmojis;
            }
        }
    }
}
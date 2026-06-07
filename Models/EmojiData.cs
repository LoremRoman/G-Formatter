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
                        new EmojiData("—", "Música", "🎵"),
                        new EmojiData("¥", "Estrella", "⭐"),
                        new EmojiData("|", "Corazón", "❤️"),
                        new EmojiData("¬", "Pez", "🐟"),
                        new EmojiData("¶", "Bombillo", "💡"),
                        new EmojiData("√", "Check", "✅"),
                        new EmojiData("ƒ", "Corazón negro", "🖤"),
                        new EmojiData("÷", "Mano abajo", "👎"),
                        new EmojiData("†", "Bomba", "💣"),
                        new EmojiData("‡", "Prohibido", "🚫"),
                        new EmojiData("Ø", "Alien", "👽"),
                        new EmojiData("»", "Trébol", "♣️"),
                        new EmojiData("±", "Teléfono", "📱"),
                        new EmojiData("•", "Mano arriba", "👍"),
                        new EmojiData("ª", "Calavera", "💀"),
                        new EmojiData("º", "Rayo", "⚡"),
                        new EmojiData("µ", "Café", "☕"),
                        new EmojiData("°", "Sol", "☀️"),
                        new EmojiData("½", "Media luna", "🌙"),
                        new EmojiData("¼", "Cuarto creciente", "🌔"),
                        new EmojiData("¾", "Tres cuartos", "🌖"),
                        new EmojiData("¹", "Uno", "1️⃣"),
                        new EmojiData("²", "Dos", "2️⃣"),
                        new EmojiData("³", "Tres", "3️⃣"),
                        new EmojiData("©", "Copyright", "©️"),
                        new EmojiData("®", "Registrado", "®️"),
                        new EmojiData("™", "Trademark", "™️")
                    };
                }
                return _allEmojis;
            }
        }
    }
}
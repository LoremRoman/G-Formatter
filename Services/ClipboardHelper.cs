using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace G_Formatter.Services
{
    public static class ClipboardHelper
    {
        public static string GetText()
        {
            try
            {
                if (Clipboard.ContainsText()) return Clipboard.GetText();
            }
            catch { }
            return string.Empty;
        }

        public static void SetText(string text)
        {
            try { Clipboard.SetText(text); } catch { }
        }

        public static async Task<string> GetSelectedTextAsync()
        {
            Clipboard.Clear();
            SendKeys.SendWait("^(c)");
            await Task.Delay(80);
            return GetText();
        }

        public static async Task PasteAndReselectAsync(string text, bool reselect)
        {
            if (string.IsNullOrEmpty(text)) return;

            SetText(text);
            SendKeys.SendWait("^(v)");
            await Task.Delay(80);

            if (reselect)
            {
                SendKeys.SendWait($"+({{LEFT {text.Length}}})");
                await Task.Delay(50);
            }
        }
    }
}
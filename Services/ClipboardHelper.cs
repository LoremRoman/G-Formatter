using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace G_Formatter.Services
{
    public static class ClipboardHelper
    {
        public static string GetText()
        {
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    if (Clipboard.ContainsText()) return Clipboard.GetText();
                    return string.Empty;
                }
                catch (System.Runtime.InteropServices.ExternalException)
                {
                    System.Threading.Thread.Sleep(20);
                }
            }
            return string.Empty;
        }

        public static void SetText(string text)
        {
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    Clipboard.SetText(text);
                    return;
                }
                catch (System.Runtime.InteropServices.ExternalException)
                {
                    System.Threading.Thread.Sleep(20);
                }
            }
        }

        public static async Task<string> GetSelectedTextAsync()
        {
            string originalText = GetText();
            SendKeys.SendWait("^(c)");
            await Task.Delay(60);
            string selectedText = GetText();

            if (selectedText == originalText) { }
            return selectedText;
        }

        public static async Task PasteAndReselectAsync(string text, bool reselect)
        {
            if (string.IsNullOrEmpty(text)) return;

            SetText(text);
            SendKeys.SendWait("^(v)");
            await Task.Delay(80);

            if (reselect)
            {
                int visualLength = text.Replace("\r\n", "\n").Length;
                SendKeys.SendWait($"+({{LEFT {visualLength}}})");
                await Task.Delay(50);
            }
        }
    }
}
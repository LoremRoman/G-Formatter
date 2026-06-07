using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using G_Formatter.Models;
using G_Formatter.Services;

namespace G_Formatter.UI
{
    public class FormatMenu : Form
    {
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        private const int BUTTON_SIZE = 36;

        private EmojiPanel? _emojiPanel;

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x08000000;
                return cp;
            }
        }

        public FormatMenu()
        {
            InitializeForm();
            CreateTopBar();
            CreateButtons();
        }

        private void InitializeForm()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(45, 45, 48);
            this.AutoSize = false;
            this.Size = new Size(405, 72);

            this.Paint += (s, e) =>
            {
                ControlPaint.DrawBorder(e.Graphics, this.ClientRectangle, Color.FromArgb(100, 100, 100), ButtonBorderStyle.Solid);
            };
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            SetWindowPos(this.Handle, new IntPtr(-1), 0, 0, 0, 0, 0x0001 | 0x0002 | 0x0010);
        }

        private void CreateTopBar()
        {
            Panel topBar = new Panel
            {
                Height = 22,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(30, 30, 30),
                Cursor = Cursors.SizeAll
            };

            Label title = new Label
            {
                Text = "≡ G-Formatter",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };

            title.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    ReleaseCapture();
                    SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
                }
            };

            topBar.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    ReleaseCapture();
                    SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
                }
            };

            topBar.Controls.Add(title);
            this.Controls.Add(topBar);
        }

        private void CreateButtons()
        {
            Panel btnPanel = new Panel
            {
                Location = new Point(0, 22),
                Size = new Size(this.Width, 48),
                BackColor = Color.FromArgb(45, 45, 48)
            };

            int x = 6;
            int y = 6;

            var btnBold = CreateFormatButton("B", "bold", x, y, Color.FromArgb(60, 60, 60), btnPanel);
            btnBold.Click += async (s, e) => await ApplyFormatAsync(BBCodeTag.FormatTags[0]);
            x += BUTTON_SIZE + 2;

            var btnUnderline = CreateFormatButton("U", "underlined", x, y, Color.FromArgb(60, 60, 60), btnPanel);
            btnUnderline.Click += async (s, e) => await ApplyFormatAsync(BBCodeTag.FormatTags[1]);
            x += BUTTON_SIZE + 2;

            var btnItalic = CreateFormatButton("I", "italics", x, y, Color.FromArgb(60, 60, 60), btnPanel);
            btnItalic.Click += async (s, e) => await ApplyFormatAsync(BBCodeTag.FormatTags[2]);
            x += BUTTON_SIZE + 4;

            AddSeparator(x, y, 24, btnPanel);
            x += 5;

            var btnRed = CreateColorButton(x, y, Color.FromArgb(220, 50, 50), "Red", btnPanel);
            btnRed.Click += async (s, e) => await ApplyColorAsync(BBCodeTag.ColorTags[0]);
            x += 24 + 2;

            var btnGreen = CreateColorButton(x, y, Color.FromArgb(50, 180, 50), "Green", btnPanel);
            btnGreen.Click += async (s, e) => await ApplyColorAsync(BBCodeTag.ColorTags[1]);
            x += 24 + 2;

            var btnBlue = CreateColorButton(x, y, Color.FromArgb(50, 120, 220), "Blue", btnPanel);
            btnBlue.Click += async (s, e) => await ApplyColorAsync(BBCodeTag.ColorTags[2]);
            x += 24 + 2;

            var btnPurple = CreateColorButton(x, y, Color.FromArgb(160, 50, 200), "Purple", btnPanel);
            btnPurple.Click += async (s, e) => await ApplyColorAsync(BBCodeTag.ColorTags[3]);
            x += 24 + 2;

            var btnCyan = CreateColorButton(x, y, Color.FromArgb(50, 190, 190), "Cyan", btnPanel);
            btnCyan.Click += async (s, e) => await ApplyColorAsync(BBCodeTag.ColorTags[4]);
            x += 24 + 4;

            AddSeparator(x, y, 24, btnPanel);
            x += 5;

            var btnClearFormat = CreateSmallButton("Fx", "Delete Format", x, y, btnPanel);
            btnClearFormat.Click += async (s, e) => await ClearSpecificAsync(false);
            x += 28 + 2;

            var btnClearColor = CreateSmallButton("C", "Remove Color", x, y, btnPanel);
            btnClearColor.Click += async (s, e) => await ClearSpecificAsync(true);
            x += 28 + 2;

            var btnClearAll = CreateSmallButton("X", "Delete All", x, y, btnPanel, Color.FromArgb(180, 60, 60));
            btnClearAll.Click += async (s, e) => await ClearAllAsync();
            x += 28 + 4; // Espacio extra

            AddSeparator(x, y, 24, btnPanel);
            x += 5;

            var btnEmoji = CreateFormatButton("🖤", "Emojis", x, y, Color.FromArgb(60, 60, 60), btnPanel);
            btnEmoji.Click += (s, e) => ToggleEmojiPanel();

            this.Controls.Add(btnPanel);
        }

        private async Task ApplyFormatAsync(BBCodeTag tag)
        {
            string txt = await ClipboardHelper.GetSelectedTextAsync();
            if (string.IsNullOrEmpty(txt)) return;
            string formatted = TextFormatter.ApplyFormatToggle(txt, tag);
            await ClipboardHelper.PasteAndReselectAsync(formatted, true);
        }

        private async Task ApplyColorAsync(BBCodeTag tag)
        {
            string txt = await ClipboardHelper.GetSelectedTextAsync();
            if (string.IsNullOrEmpty(txt)) return;
            string formatted = TextFormatter.ApplyColor(txt, tag);
            await ClipboardHelper.PasteAndReselectAsync(formatted, true);
        }

        private async Task ClearSpecificAsync(bool colorsOnly)
        {
            string txt = await ClipboardHelper.GetSelectedTextAsync();
            if (string.IsNullOrEmpty(txt)) return;
            string clean = colorsOnly ? TextFormatter.ClearOnlyColor(txt) : TextFormatter.ClearOnlyFormat(txt);
            await ClipboardHelper.PasteAndReselectAsync(clean, true);
        }

        private async Task ClearAllAsync()
        {
            string txt = await ClipboardHelper.GetSelectedTextAsync();
            if (string.IsNullOrEmpty(txt)) return;
            string clean = TextFormatter.ClearAllFormat(txt);
            await ClipboardHelper.PasteAndReselectAsync(clean, true);
        }

        private void ToggleEmojiPanel()
        {
            if (_emojiPanel != null && !_emojiPanel.IsDisposed)
            {
                _emojiPanel.Close();
                return;
            }

            _emojiPanel = new EmojiPanel(this);
            _emojiPanel.Show();
        }

        private void AddSeparator(int x, int y, int height, Panel parent)
        {
            var sep = new Panel { Location = new Point(x, y + 6), Size = new Size(1, height), BackColor = Color.FromArgb(100, 100, 100) };
            parent.Controls.Add(sep);
        }

        private Button CreateFormatButton(string text, string tooltip, int x, int y, Color backColor, Panel parent)
        {
            var btn = new Button { Text = text, FlatStyle = FlatStyle.Flat, Size = new Size(BUTTON_SIZE, BUTTON_SIZE), Location = new Point(x, y), BackColor = backColor, ForeColor = Color.White, Font = new Font("Segoe UI Emoji", 10, FontStyle.Bold), Cursor = Cursors.Hand };
            btn.FlatAppearance.BorderSize = 0;
            new ToolTip().SetToolTip(btn, tooltip);
            parent.Controls.Add(btn);
            return btn;
        }

        private Button CreateColorButton(int x, int y, Color color, string name, Panel parent)
        {
            var btn = new Button { Text = "", FlatStyle = FlatStyle.Flat, Size = new Size(24, 24), Location = new Point(x, y + 6), BackColor = color, Cursor = Cursors.Hand };
            btn.FlatAppearance.BorderSize = 1; btn.FlatAppearance.BorderColor = Color.FromArgb(80, 80, 80);
            new ToolTip().SetToolTip(btn, name);
            parent.Controls.Add(btn);
            return btn;
        }

        private Button CreateSmallButton(string text, string tooltip, int x, int y, Panel parent, Color? backColor = null)
        {
            var btn = new Button { Text = text, FlatStyle = FlatStyle.Flat, Size = new Size(28, 24), Location = new Point(x, y + 6), BackColor = backColor ?? Color.FromArgb(60, 60, 60), ForeColor = Color.White, Font = new Font("Segoe UI", 7, FontStyle.Bold), Cursor = Cursors.Hand };
            btn.FlatAppearance.BorderSize = 0;
            new ToolTip().SetToolTip(btn, tooltip);
            parent.Controls.Add(btn);
            return btn;
        }
    }
}
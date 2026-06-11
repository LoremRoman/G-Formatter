using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using G_Formatter.Models;
using G_Formatter.Services;

namespace G_Formatter.UI
{
    public class FormatMenu : Form
    {
        public bool IsSuspended = false;
        public float IdleOpacity = 0.5f;
        private SettingsForm _settingsForm;
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        private const int WH_MOUSE_LL = 14;
        private const int WM_RBUTTONDOWN = 0x0204;
        private const int WM_LBUTTONDOWN = 0x0201;

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const byte VK_CONTROL = 0x11;
        private const uint KEYEVENTF_KEYUP = 0x0002;

        private IntPtr _hookID = IntPtr.Zero;
        private LowLevelKeyboardProc _proc;

        private IntPtr _mouseHookID = IntPtr.Zero;
        private LowLevelMouseProc _mouseProc;

        private const int BUTTON_SIZE = 36;
        private EmojiPanel? _emojiPanel;

        private ToolTip _mainToolTip;
        private Button _btnMainEmoji;

        private BBCodeTag _currentDefaultColor = BBCodeTag.ColorTags[0];
        private Button _btnMainColor;
        private System.Windows.Forms.Timer _colorHoverTimer;
        private Form _colorSubMenu;

        private Button _btnMainClear;
        private System.Windows.Forms.Timer _clearHoverTimer;
        private Form _clearSubMenu;
        private int _defaultClearMode = 0;
        private Button _btnMainClipboard;
        private System.Windows.Forms.Timer _clipboardHoverTimer;
        private Form _clipboardSubMenu;

        private System.Windows.Forms.Timer _emojiHoverTimer;

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
            var config = ConfigManager.Load();
            this.IsSuspended = config.IsSuspended;
            this.IdleOpacity = config.IdleOpacity;

            InitializeForm();
            CreateButtons();

            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(-10000, -10000);

            _proc = HookCallback;
            _hookID = SetHook(_proc);
            _mouseProc = MouseHookCallback;
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                _mouseHookID = SetWindowsHookEx(WH_MOUSE_LL, _mouseProc, GetModuleHandle(curModule.ModuleName), 0);
            }

            SetupOpacityHover();
        }

        private IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private bool IsHabboActive()
        {
            IntPtr hwnd = GetForegroundWindow();
            if (hwnd == IntPtr.Zero) return false;

            StringBuilder sb = new StringBuilder(256);
            if (GetWindowText(hwnd, sb, 256) > 0)
            {
                string title = sb.ToString().ToLower();
                return title.Contains("habbo");
            }
            return false;
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN))
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Keys key = (Keys)vkCode;

                bool ctrlPressed = (ModifierKeys & Keys.Control) == Keys.Control;
                bool shiftPressed = (ModifierKeys & Keys.Shift) == Keys.Shift;

                if (ctrlPressed && shiftPressed && key == Keys.S)
                {
                    if (IsHabboActive())
                    {
                        this.Invoke((MethodInvoker)delegate
                        {
                            OpenSettings();
                        });
                    }
                    return (IntPtr)1;
                }

                if (IsSuspended) return CallNextHookEx(_hookID, nCode, wParam, lParam);

                if (ctrlPressed && !shiftPressed && IsHabboActive())
                {
                    BBCodeTag? tag = null;
                    if (key == Keys.B) tag = BBCodeTag.FormatTags[0];
                    else if (key == Keys.U) tag = BBCodeTag.FormatTags[1];
                    else if (key == Keys.I) tag = BBCodeTag.FormatTags[2];

                    if (tag != null)
                    {
                        keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, 0);
                        _ = ApplyFormatAsync(tag);
                        return (IntPtr)1;
                    }
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        public void OpenSettings()
        {
            if (_settingsForm == null || _settingsForm.IsDisposed)
            {
                _settingsForm = new SettingsForm(this);
                _settingsForm.Show();
            }
            else
            {
                _settingsForm.BringToFront();
            }
        }

        private IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                if (wParam == (IntPtr)WM_RBUTTONDOWN && IsHabboActive())
                {
                    if (IsSuspended) return CallNextHookEx(_mouseHookID, nCode, wParam, lParam);

                    _ = HandleRightClickAsync();
                    return (IntPtr)1;
                }

                if (wParam == (IntPtr)WM_LBUTTONDOWN)
                {
                    bool inMainMenu = this.Bounds.Contains(Cursor.Position);
                    bool inEmojiMenu = _emojiPanel != null && !_emojiPanel.IsDisposed && _emojiPanel.Bounds.Contains(Cursor.Position);
                    bool inColorMenu = _colorSubMenu != null && !_colorSubMenu.IsDisposed && _colorSubMenu.Bounds.Contains(Cursor.Position);
                    bool inClearMenu = _clearSubMenu != null && !_clearSubMenu.IsDisposed && _clearSubMenu.Bounds.Contains(Cursor.Position);
                    bool inClipboardMenu = _clipboardSubMenu != null && !_clipboardSubMenu.IsDisposed && _clipboardSubMenu.Bounds.Contains(Cursor.Position);

                    if (!inMainMenu && !inEmojiMenu && !inColorMenu && !inClearMenu && !inClipboardMenu)
                    {
                        this.Invoke((MethodInvoker)delegate
                        {
                            this.Location = new Point(-10000, -10000);
                            if (_emojiPanel != null && !_emojiPanel.IsDisposed) _emojiPanel.Close();
                            if (_colorSubMenu != null && !_colorSubMenu.IsDisposed) _colorSubMenu.Close();
                            if (_clearSubMenu != null && !_clearSubMenu.IsDisposed) _clearSubMenu.Close();
                        });
                    }
                }
            }
            return CallNextHookEx(_mouseHookID, nCode, wParam, lParam);
        }

        private async Task HandleRightClickAsync()
        {
            string txt = await ClipboardHelper.GetSelectedTextAsync();
            bool hasText = !string.IsNullOrEmpty(txt);

            this.Invoke((MethodInvoker)delegate
            {
                UpdateMenuState(hasText);

                int targetX = Cursor.Position.X;
                int targetY = Cursor.Position.Y;

                Rectangle workingArea = Screen.FromPoint(Cursor.Position).WorkingArea;

                if (targetX + this.Width > workingArea.Right)
                    targetX = workingArea.Right - this.Width - 5;

                if (targetY + this.Height > workingArea.Bottom)
                    targetY = Cursor.Position.Y - this.Height - 5;
                else
                    targetY = Cursor.Position.Y + 5;

                this.Location = new Point(targetX, targetY);
                SetWindowPos(this.Handle, new IntPtr(-1), this.Left, this.Top, this.Width, this.Height, 0x0040);
            });
        }

        private void UpdateMenuState(bool hasText)
        {
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is Panel pnl)
                {
                    foreach (Control item in pnl.Controls)
                    {
                        if (item is Button btn)
                        {
                            bool shouldBeEnabled = (btn.Tag?.ToString() == "AlwaysOn") || hasText;

                            btn.Enabled = shouldBeEnabled;
                            btn.ForeColor = shouldBeEnabled ? Color.White : Color.FromArgb(100, 100, 100);
                        }
                    }
                }
            }
        }

        private void InitializeForm()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.BackColor = Color.FromArgb(30, 30, 32);
            this.Opacity = IdleOpacity;
            this.Size = new Size(520, 48);

            this.Paint += (s, e) =>
            {
                ControlPaint.DrawBorder(e.Graphics, this.ClientRectangle, Color.FromArgb(80, 80, 80), ButtonBorderStyle.Solid);
            };

            _mainToolTip = new ToolTip();
            _mainToolTip.ShowAlways = true;
            _mainToolTip.InitialDelay = 200;
            _mainToolTip.ReshowDelay = 100;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            SetWindowPos(this.Handle, new IntPtr(-1), 0, 0, 0, 0, 0x0001 | 0x0002 | 0x0010);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            if (_hookID != IntPtr.Zero) UnhookWindowsHookEx(_hookID);
            if (_mouseHookID != IntPtr.Zero) UnhookWindowsHookEx(_mouseHookID);
            base.OnFormClosed(e);
        }

        private void CreateButtons()
        {
            Panel btnPanel = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(this.Width, this.Height),
                BackColor = Color.Transparent
            };

            int x = 6;
            int y = 6;

            _btnMainClipboard = CreateSmallButton("📋", "Clipboard (Hold for options)", x, y, btnPanel);
            _btnMainClipboard.Tag = "AlwaysOn";
            _btnMainClipboard.Click += (s, e) => { if (IsHabboActive()) SendKeys.SendWait("^(v)"); };
            _btnMainClipboard.MouseEnter += (s, e) => StartClipboardHoverTimer();
            _btnMainClipboard.MouseLeave += (s, e) => StopClipboardHoverTimer();
            x += 28 + 4;

            AddSeparator(x, y, 24, btnPanel);
            x += 5;

            var btnBold = CreateFormatButton("B", "Bold - Ctrl+B", x, y, Color.FromArgb(60, 60, 60), btnPanel);
            btnBold.Click += async (s, e) => await ApplyFormatAsync(BBCodeTag.FormatTags[0]);
            x += BUTTON_SIZE + 2;

            var btnUnderline = CreateFormatButton("U", "Underlined - Ctrl+U", x, y, Color.FromArgb(60, 60, 60), btnPanel);
            btnUnderline.Click += async (s, e) => await ApplyFormatAsync(BBCodeTag.FormatTags[1]);
            x += BUTTON_SIZE + 2;

            var btnItalic = CreateFormatButton("I", "Italics - Ctrl+I", x, y, Color.FromArgb(60, 60, 60), btnPanel);
            btnItalic.Click += async (s, e) => await ApplyFormatAsync(BBCodeTag.FormatTags[2]);
            x += BUTTON_SIZE + 4;

            AddSeparator(x, y, 24, btnPanel);
            x += 5;

            _btnMainColor = CreateColorButton(x, y, Color.FromArgb(220, 50, 50), "Color (Hold to open palette)", btnPanel);
            _btnMainColor.Click += async (s, e) => await ApplyColorAsync(_currentDefaultColor);
            _btnMainColor.MouseEnter += (s, e) => StartColorHoverTimer();
            _btnMainColor.MouseLeave += (s, e) => StopColorHoverTimer();
            x += 24 + 4;

            AddSeparator(x, y, 24, btnPanel);
            x += 5;

            _btnMainClear = CreateSmallButton("Fx", "Clear (Hold for options)", x, y, btnPanel);
            _btnMainClear.Click += async (s, e) => await ExecuteClearActionAsync(_defaultClearMode);
            _btnMainClear.MouseEnter += (s, e) => StartClearHoverTimer();
            _btnMainClear.MouseLeave += (s, e) => StopClearHoverTimer();
            x += 28 + 4;

            AddSeparator(x, y, 24, btnPanel);
            x += 5;

            _btnMainEmoji = CreateFormatButton("🖤", "Emoji (Hold for panel)", x, y, Color.FromArgb(60, 60, 60), btnPanel);
            _btnMainEmoji.Tag = "AlwaysOn";
            _btnMainEmoji.Click += async (s, e) =>
            {
                if (IsHabboActive()) await ClipboardHelper.PasteAndReselectAsync(_btnMainEmoji.Text, false);
            };
            _btnMainEmoji.MouseEnter += (s, e) => StartEmojiHoverTimer();
            _btnMainEmoji.MouseLeave += (s, e) => StopEmojiHoverTimer();
            x += BUTTON_SIZE + 6;

            this.Width = x;
            btnPanel.Width = x;

            this.Controls.Add(btnPanel);
        }

        private async Task ApplyFormatAsync(BBCodeTag tag)
        {
            if (!IsHabboActive()) return;
            string txt = await ClipboardHelper.GetSelectedTextAsync();
            if (string.IsNullOrEmpty(txt)) return;
            string formatted = TextFormatter.ApplyFormatToggle(txt, tag);
            await ClipboardHelper.PasteAndReselectAsync(formatted, true);
        }

        private async Task ApplyColorAsync(BBCodeTag tag)
        {
            if (!IsHabboActive()) return;
            string txt = await ClipboardHelper.GetSelectedTextAsync();
            if (string.IsNullOrEmpty(txt)) return;
            string formatted = TextFormatter.ApplyColor(txt, tag);
            await ClipboardHelper.PasteAndReselectAsync(formatted, true);
        }

        private async Task ClearSpecificAsync(bool colorsOnly)
        {
            if (!IsHabboActive()) return;
            string txt = await ClipboardHelper.GetSelectedTextAsync();
            if (string.IsNullOrEmpty(txt)) return;
            string clean = colorsOnly ? TextFormatter.ClearOnlyColor(txt) : TextFormatter.ClearOnlyFormat(txt);
            await ClipboardHelper.PasteAndReselectAsync(clean, true);
        }

        private async Task CreateAllAsync()
        {
            if (!IsHabboActive()) return;
            string txt = await ClipboardHelper.GetSelectedTextAsync();
            if (string.IsNullOrEmpty(txt)) return;
            string clean = TextFormatter.ClearAllFormat(txt);
            await ClipboardHelper.PasteAndReselectAsync(clean, true);
        }

        private async Task ClearAllAsync()
        {
            if (!IsHabboActive()) return;
            string txt = await ClipboardHelper.GetSelectedTextAsync();
            if (string.IsNullOrEmpty(txt)) return;
            string clean = TextFormatter.ClearAllFormat(txt);
            await ClipboardHelper.PasteAndReselectAsync(clean, true);
        }

        private void ToggleEmojiPanel()
        {
            if (!IsHabboActive()) return;
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

        private void AutoSizeSubMenu(Form subMenu, FlowLayoutPanel pnl)
        {
            int totalWidth = pnl.Padding.Left;
            foreach (Control ctrl in pnl.Controls)
            {
                totalWidth += ctrl.Width + ctrl.Margin.Left + ctrl.Margin.Right;
            }
            subMenu.Width = totalWidth + pnl.Padding.Right + 8;
        }

        private Button CreateFormatButton(string text, string tooltip, int x, int y, Color backColor, Panel parent)
        {
            var btn = new Button { Text = text, FlatStyle = FlatStyle.Flat, Size = new Size(BUTTON_SIZE, BUTTON_SIZE), Location = new Point(x, y), BackColor = backColor, ForeColor = Color.White, Font = new Font("Segoe UI Emoji", 10, FontStyle.Bold), Cursor = Cursors.Hand };
            btn.FlatAppearance.BorderSize = 0;
            _mainToolTip.SetToolTip(btn, tooltip);
            parent.Controls.Add(btn);
            return btn;
        }

        private Button CreateColorButton(int x, int y, Color color, string name, Panel parent)
        {
            var btn = new Button { Text = "", FlatStyle = FlatStyle.Flat, Size = new Size(24, 24), Location = new Point(x, y + 6), BackColor = color, Cursor = Cursors.Hand };
            btn.FlatAppearance.BorderSize = 1; btn.FlatAppearance.BorderColor = Color.FromArgb(80, 80, 80);
            _mainToolTip.SetToolTip(btn, name);
            parent.Controls.Add(btn);
            return btn;
        }

        private Button CreateSmallButton(string text, string tooltip, int x, int y, Panel parent, Color? backColor = null)
        {
            var btn = new Button { Text = text, FlatStyle = FlatStyle.Flat, Size = new Size(28, 24), Location = new Point(x, y + 6), BackColor = backColor ?? Color.FromArgb(60, 60, 60), ForeColor = Color.White, Font = new Font("Segoe UI", 7, FontStyle.Bold), Cursor = Cursors.Hand };
            btn.FlatAppearance.BorderSize = 0;
            _mainToolTip.SetToolTip(btn, tooltip);
            parent.Controls.Add(btn);
            return btn;
        }

        private void SetupOpacityHover()
        {
            System.Windows.Forms.Timer opacityTimer = new System.Windows.Forms.Timer { Interval = 50 };
            opacityTimer.Tick += (s, e) =>
            {
                if (this.Location.X <= -10000) return;

                if (this.Bounds.Contains(Cursor.Position) ||
                   (_emojiPanel != null && _emojiPanel.Bounds.Contains(Cursor.Position)) ||
                   (_colorSubMenu != null && _colorSubMenu.Bounds.Contains(Cursor.Position)) ||
                   (_clearSubMenu != null && _clearSubMenu.Bounds.Contains(Cursor.Position)) ||
                   (_clipboardSubMenu != null && _clipboardSubMenu.Bounds.Contains(Cursor.Position)))
                {
                    if (this.Opacity < 1.0f) this.Opacity = 1.0f;
                }
                else
                {
                    if (this.Opacity > IdleOpacity) this.Opacity = IdleOpacity;
                    else if (this.Opacity < IdleOpacity) this.Opacity = IdleOpacity;
                }
            };
            opacityTimer.Start();
        }

        public void UpdateMainEmojiIcon(string visualEmoji)
        {
            if (_btnMainEmoji != null)
            {
                _btnMainEmoji.Text = visualEmoji;
            }
        }

        private void StartColorHoverTimer()
        {
            if (_colorHoverTimer == null)
            {
                _colorHoverTimer = new System.Windows.Forms.Timer { Interval = 400 };
                _colorHoverTimer.Tick += (s, e) => ShowColorSubMenu();
            }
            _colorHoverTimer.Start();
        }

        private void StopColorHoverTimer()
        {
            _colorHoverTimer?.Stop();
            System.Windows.Forms.Timer closeTimer = new System.Windows.Forms.Timer { Interval = 200 };
            closeTimer.Tick += (s, ev) =>
            {
                if (_colorSubMenu != null && !_colorSubMenu.IsDisposed)
                {
                    if (!_colorSubMenu.Bounds.Contains(Cursor.Position) && !_btnMainColor.Bounds.Contains(Cursor.Position))
                    {
                        _colorSubMenu.Close();
                        closeTimer.Stop();
                    }
                }
                else closeTimer.Stop();
            };
            closeTimer.Start();
        }

        private void ShowColorSubMenu()
        {
            _colorHoverTimer.Stop();
            if (_colorSubMenu != null && !_colorSubMenu.IsDisposed) return;

            _colorSubMenu = new NoFocusForm
            {
                FormBorderStyle = FormBorderStyle.None,
                ShowInTaskbar = false,
                TopMost = true,
                StartPosition = FormStartPosition.Manual,
                BackColor = Color.FromArgb(40, 40, 42),
                Size = new Size(136, 32)
            };

            _colorSubMenu.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, _colorSubMenu.ClientRectangle, Color.FromArgb(100, 100, 100), ButtonBorderStyle.Solid);

            Point btnScreenPos = _btnMainColor.PointToScreen(Point.Empty);
            _colorSubMenu.Location = new Point(btnScreenPos.X - 50, btnScreenPos.Y - 36);

            FlowLayoutPanel pnl = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(4, 4, 0, 0) };

            for (int i = 0; i < BBCodeTag.ColorTags.Count; i++)
            {
                var tag = BBCodeTag.ColorTags[i];
                Color btnColor = i == 0 ? Color.FromArgb(220, 50, 50) :
                                 i == 1 ? Color.FromArgb(50, 180, 50) :
                                 i == 2 ? Color.FromArgb(50, 120, 220) :
                                 i == 3 ? Color.FromArgb(160, 50, 200) : Color.FromArgb(50, 190, 190);

                Button btn = new Button { Size = new Size(22, 22), BackColor = btnColor, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, Margin = new Padding(2, 0, 2, 0) };
                btn.FlatAppearance.BorderSize = 0;

                string colorName = i == 0 ? "Red" : i == 1 ? "Green" : i == 2 ? "Blue" : i == 3 ? "Purple" : "Cyan";
                _mainToolTip.SetToolTip(btn, colorName);

                btn.Click += async (s, e) =>
                {
                    _currentDefaultColor = tag;
                    _btnMainColor.BackColor = btnColor;
                    _colorSubMenu.Close();
                    await ApplyColorAsync(tag);
                };

                pnl.Controls.Add(btn);
            }

            _colorSubMenu.Controls.Add(pnl);
            _colorSubMenu.MouseLeave += (s, e) =>
            {
                if (!_colorSubMenu.Bounds.Contains(Cursor.Position) && !_btnMainColor.Bounds.Contains(Cursor.Position))
                    _colorSubMenu.Close();
            };

            _colorSubMenu.Show();
        }

        private async Task ExecuteClearActionAsync(int mode)
        {
            if (mode == 0) await ClearSpecificAsync(false);
            else if (mode == 1) await ClearSpecificAsync(true);
            else await ClearAllAsync();
        }

        private void StartClearHoverTimer()
        {
            if (_clearHoverTimer == null)
            {
                _clearHoverTimer = new System.Windows.Forms.Timer { Interval = 400 };
                _clearHoverTimer.Tick += (s, e) => ShowClearSubMenu();
            }
            _clearHoverTimer.Start();
        }

        private void StopClearHoverTimer()
        {
            _clearHoverTimer?.Stop();
            System.Windows.Forms.Timer closeTimer = new System.Windows.Forms.Timer { Interval = 200 };
            closeTimer.Tick += (s, ev) =>
            {
                if (_clearSubMenu != null && !_clearSubMenu.IsDisposed)
                {
                    if (!_clearSubMenu.Bounds.Contains(Cursor.Position) && !_btnMainClear.Bounds.Contains(Cursor.Position))
                    {
                        _clearSubMenu.Close();
                        closeTimer.Stop();
                    }
                }
                else closeTimer.Stop();
            };
            closeTimer.Start();
        }

        private void ShowClearSubMenu()
        {
            _clearHoverTimer.Stop();
            if (_clearSubMenu != null && !_clearSubMenu.IsDisposed) return;

            _clearSubMenu = new NoFocusForm
            {
                FormBorderStyle = FormBorderStyle.None,
                ShowInTaskbar = false,
                TopMost = true,
                StartPosition = FormStartPosition.Manual,
                BackColor = Color.FromArgb(40, 40, 42),
                Size = new Size(100, 32)
            };

            _clearSubMenu.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, _clearSubMenu.ClientRectangle, Color.FromArgb(100, 100, 100), ButtonBorderStyle.Solid);

            Point btnScreenPos = _btnMainClear.PointToScreen(Point.Empty);
            _clearSubMenu.Location = new Point(btnScreenPos.X - 35, btnScreenPos.Y - 36);

            FlowLayoutPanel pnl = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(4, 4, 0, 0) };
            string[] texts = { "Fx", "C", "X" };
            Color[] colors = { Color.FromArgb(60, 60, 60), Color.FromArgb(60, 60, 60), Color.FromArgb(180, 60, 60) };

            for (int i = 0; i < 3; i++)
            {
                int mode = i;
                Button btn = new Button { Text = texts[i], Size = new Size(24, 22), BackColor = colors[i], ForeColor = Color.White, Font = new Font("Segoe UI", 7, FontStyle.Bold), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, Margin = new Padding(2, 0, 2, 0) };
                btn.FlatAppearance.BorderSize = 0;

                string tip = i == 0 ? "Clear Format" : i == 1 ? "Clear Color" : "Clear All";
                _mainToolTip.SetToolTip(btn, tip);

                btn.Click += async (s, e) =>
                {
                    _defaultClearMode = mode;
                    _btnMainClear.Text = texts[mode];
                    _btnMainClear.BackColor = colors[mode];
                    _clearSubMenu.Close();
                    await ExecuteClearActionAsync(mode);
                };
                pnl.Controls.Add(btn);
            }
            _clearSubMenu.Controls.Add(pnl);
            _clearSubMenu.Show();
        }

        private void StartEmojiHoverTimer()
        {
            if (_emojiHoverTimer == null)
            {
                _emojiHoverTimer = new System.Windows.Forms.Timer { Interval = 400 };
                _emojiHoverTimer.Tick += (s, e) => ShowEmojiPanelOnHover();
            }
            _emojiHoverTimer.Start();
        }

        private void StopEmojiHoverTimer()
        {
            _emojiHoverTimer?.Stop();
            System.Windows.Forms.Timer closeTimer = new System.Windows.Forms.Timer { Interval = 200 };
            closeTimer.Tick += (s, ev) =>
            {
                if (_emojiPanel != null && !_emojiPanel.IsDisposed)
                {
                    if (!_emojiPanel.Bounds.Contains(Cursor.Position) && !_btnMainEmoji.Bounds.Contains(Cursor.Position))
                    {
                        _emojiPanel.Close();
                        closeTimer.Stop();
                    }
                }
                else closeTimer.Stop();
            };
            closeTimer.Start();
        }

        private void ShowEmojiPanelOnHover()
        {
            _emojiHoverTimer.Stop();
            if (!IsHabboActive() || (_emojiPanel != null && !_emojiPanel.IsDisposed)) return;

            _emojiPanel = new EmojiPanel(this);
            _emojiPanel.Show();
        }

        private void StartClipboardHoverTimer()
        {
            if (_clipboardHoverTimer == null)
            {
                _clipboardHoverTimer = new System.Windows.Forms.Timer { Interval = 400 };
                _clipboardHoverTimer.Tick += (s, e) => ShowClipboardSubMenu();
            }
            _clipboardHoverTimer.Start();
        }

        private void StopClipboardHoverTimer()
        {
            _clipboardHoverTimer?.Stop();
            System.Windows.Forms.Timer closeTimer = new System.Windows.Forms.Timer { Interval = 200 };
            closeTimer.Tick += (s, ev) =>
            {
                if (_clipboardSubMenu != null && !_clipboardSubMenu.IsDisposed)
                {
                    if (!_clipboardSubMenu.Bounds.Contains(Cursor.Position) && !_btnMainClipboard.Bounds.Contains(Cursor.Position))
                    {
                        _clipboardSubMenu.Close();
                        closeTimer.Stop();
                    }
                }
                else closeTimer.Stop();
            };
            closeTimer.Start();
        }

        private void ShowClipboardSubMenu()
        {
            _clipboardHoverTimer.Stop();
            if (_clipboardSubMenu != null && !_clipboardSubMenu.IsDisposed) return;

            _clipboardSubMenu = new NoFocusForm
            {
                FormBorderStyle = FormBorderStyle.None,
                ShowInTaskbar = false,
                TopMost = true,
                StartPosition = FormStartPosition.Manual,
                BackColor = Color.FromArgb(40, 40, 42),
                Height = 32
            };

            _clipboardSubMenu.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, _clipboardSubMenu.ClientRectangle, Color.FromArgb(100, 100, 100), ButtonBorderStyle.Solid);

            Point btnScreenPos = _btnMainClipboard.PointToScreen(Point.Empty);

            FlowLayoutPanel pnl = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(4, 4, 0, 0), WrapContents = false };

            string[] texts = { "Cut", "Copy", "Paste", "All" };
            string[] keys = { "^(x)", "^(c)", "^(v)", "^(a)" };

            for (int i = 0; i < 4; i++)
            {
                string keyCommand = keys[i];
                Button btn = new Button { Text = texts[i], Size = new Size(34, 22), BackColor = Color.FromArgb(60, 60, 60), ForeColor = Color.White, Font = new Font("Segoe UI", 7, FontStyle.Bold), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, Margin = new Padding(2, 0, 2, 0), Tag = "AlwaysOn" };
                btn.FlatAppearance.BorderSize = 0;

                _mainToolTip.SetToolTip(btn, $"Shortcut: {keyCommand.Replace("^(", "Ctrl+").Replace(")", "")}");

                btn.Click += async (s, e) =>
                {
                    if (IsHabboActive())
                    {
                        SendKeys.SendWait(keyCommand);
                        if (keyCommand == "^(a)") await Task.Delay(50);
                    }
                    _clipboardSubMenu.Close();
                };
                pnl.Controls.Add(btn);
            }

            _clipboardSubMenu.Controls.Add(pnl);

            AutoSizeSubMenu(_clipboardSubMenu, pnl);

            _clipboardSubMenu.Location = new Point(btnScreenPos.X - 10, btnScreenPos.Y - 36);
            _clipboardSubMenu.Show();
        }
    }

    public class NoFocusForm : Form
    {
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x08000000;
                return cp;
            }
        }
    }
}
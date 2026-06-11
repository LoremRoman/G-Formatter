using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace G_Formatter.UI
{
    public class SettingsForm : Form
    {
        private FormatMenu _mainMenu;
        private Button _btnSave;
        private Button _btnCancel;
        private Label _lblOpacityValue;
        private ToggleSwitch _toggleSuspend;
        private bool _isDragging;
        private Point _dragStart;
        private bool _initialSuspended;
        private float _initialOpacity;

        public SettingsForm(FormatMenu mainMenu)
        {
            _mainMenu = mainMenu;
            _initialSuspended = _mainMenu.IsSuspended;
            _initialOpacity = _mainMenu.IdleOpacity;

            InitializeForm();
            CreateTitleBar();
            CreateControls();
        }

        private void InitializeForm()
        {
            this.Text = "Settings";
            this.Size = new Size(340, 290);
            this.BackColor = Color.FromArgb(28, 28, 32);
            this.ForeColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.TopMost = true;
            this.Paint += (s, e) =>
            {
                ControlPaint.DrawBorder(e.Graphics, this.ClientRectangle,
                    Color.FromArgb(60, 60, 70), 1, ButtonBorderStyle.Solid,
                    Color.FromArgb(60, 60, 70), 1, ButtonBorderStyle.Solid,
                    Color.FromArgb(60, 60, 70), 1, ButtonBorderStyle.Solid,
                    Color.FromArgb(60, 60, 70), 1, ButtonBorderStyle.Solid);
            };
        }

        private void CreateTitleBar()
        {
            Panel titleBar = new Panel
            {
                Height = 32,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(38, 38, 42),
                Cursor = Cursors.SizeAll
            };

            titleBar.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left) { _isDragging = true; _dragStart = new Point(e.X, e.Y); }
            };
            titleBar.MouseMove += (s, e) =>
            {
                if (_isDragging) { this.Location = new Point(this.Left + e.X - _dragStart.X, this.Top + e.Y - _dragStart.Y); }
            };
            titleBar.MouseUp += (s, e) => _isDragging = false;

            Label title = new Label
            {
                Text = "G-Formatter 1.1.0 Settings",
                Font = new Font("Segoe UI Semibold", 9.5f),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Location = new Point(12, 6),
                AutoSize = true
            };

            Button btnClose = new Button
            {
                Text = "✕",
                FlatStyle = FlatStyle.Flat,
                Size = new Size(32, 32),
                Location = new Point(this.Width - 32, 0),
                BackColor = Color.Transparent,
                ForeColor = Color.FromArgb(150, 150, 160),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.FlatAppearance.MouseOverBackColor = Color.FromArgb(220, 50, 50);
            btnClose.FlatAppearance.MouseDownBackColor = Color.FromArgb(180, 40, 40);
            btnClose.MouseEnter += (s, e) => btnClose.ForeColor = Color.White;
            btnClose.MouseLeave += (s, e) => btnClose.ForeColor = Color.FromArgb(150, 150, 160);
            btnClose.Click += (s, e) => CancelAndClose();

            titleBar.Controls.Add(title);
            titleBar.Controls.Add(btnClose);
            this.Controls.Add(titleBar);
        }

        private void CreateControls()
        {
            Panel content = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
            };

            int y = 50;

            Label lblSuspend = new Label
            {
                Text = "Pause Extension",
                Font = new Font("Segoe UI", 10f),
                ForeColor = Color.White,
                Location = new Point(24, y + 2),
                AutoSize = true
            };
            content.Controls.Add(lblSuspend);

            _toggleSuspend = new ToggleSwitch
            {
                Location = new Point(this.Width - 24 - 46, y),
                Checked = _mainMenu.IsSuspended
            };
            _toggleSuspend.CheckedChanged += (s, e) =>
            {
                _mainMenu.IsSuspended = _toggleSuspend.Checked;
            };
            content.Controls.Add(_toggleSuspend);

            y += 40;

            content.Controls.Add(CreateSeparator(y));

            y += 20;

            Label lblOpacity = new Label
            {
                Text = "Idle Opacity",
                Font = new Font("Segoe UI", 10f),
                ForeColor = Color.White,
                Location = new Point(24, y),
                AutoSize = true
            };
            content.Controls.Add(lblOpacity);

            _lblOpacityValue = new Label
            {
                Text = $"{_mainMenu.IdleOpacity * 100:F0}%",
                Location = new Point(this.Width - 65, y),
                AutoSize = false,
                Size = new Size(41, 20),
                TextAlign = ContentAlignment.TopRight,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 150, 210)
            };
            content.Controls.Add(_lblOpacityValue);

            y += 25;

            TrackBar trkOpacity = new TrackBar
            {
                Minimum = 10,
                Maximum = 100,
                Value = (int)(_mainMenu.IdleOpacity * 100),
                Location = new Point(20, y),
                Size = new Size(300, 45),
                TickStyle = TickStyle.None,
                BackColor = Color.FromArgb(28, 28, 32)
            };
            trkOpacity.Scroll += (s, e) =>
            {
                _mainMenu.IdleOpacity = trkOpacity.Value / 100f;
                _lblOpacityValue.Text = $"{trkOpacity.Value}%";
            };
            content.Controls.Add(trkOpacity);

            y += 45;

            content.Controls.Add(CreateSeparator(y));

            y += 20;

            Label lblCredit = new Label
            {
                Text = "By: LoremRoman · HABBO.ES: Bigbenitocamelo",
                Font = new Font("Segoe UI", 8f, FontStyle.Italic),
                ForeColor = Color.FromArgb(120, 120, 130),
                Location = new Point(24, y),
                AutoSize = true
            };
            content.Controls.Add(lblCredit);

            y += 35;

            Button btnHelp = new Button
            {
                Text = "How to use?",
                Size = new Size(100, 30),
                Location = new Point(24, y),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(210, 100, 40),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnHelp.FlatAppearance.BorderSize = 0;
            btnHelp.Click += (s, e) =>
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "https://github.com/LoremRoman/G-Formatter#readme",
                    UseShellExecute = true
                });
            };
            content.Controls.Add(btnHelp);

            _btnCancel = new Button
            {
                Text = "Cancel",
                Size = new Size(85, 30),
                Location = new Point(this.Width - 24 - 85 - 10 - 85, y),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(55, 55, 60),
                ForeColor = Color.FromArgb(220, 220, 230),
                Font = new Font("Segoe UI", 9f),
                Cursor = Cursors.Hand
            };
            _btnCancel.FlatAppearance.BorderSize = 0;
            _btnCancel.Click += (s, e) => CancelAndClose();

            _btnSave = new Button
            {
                Text = "Save",
                Size = new Size(85, 30),
                Location = new Point(this.Width - 24 - 85, y),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 120, 200),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            _btnSave.FlatAppearance.BorderSize = 0;
            _btnSave.Click += (s, e) =>
            {
                G_Formatter.Services.ConfigManager.Save(_mainMenu.IsSuspended, _mainMenu.IdleOpacity);
                this.Close();
            };

            content.Controls.Add(_btnCancel);
            content.Controls.Add(_btnSave);

            this.Controls.Add(content);
        }

        private Panel CreateSeparator(int y)
        {
            return new Panel
            {
                Location = new Point(24, y),
                Size = new Size(292, 1),
                BackColor = Color.FromArgb(50, 50, 55)
            };
        }

        private void CancelAndClose()
        {
            _mainMenu.IsSuspended = _initialSuspended;
            _mainMenu.IdleOpacity = _initialOpacity;
            this.Close();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                CancelAndClose();
            base.OnKeyDown(e);
        }
    }
    public class ToggleSwitch : Control
    {
        public event EventHandler CheckedChanged;

        private bool _checked = false;
        public bool Checked
        {
            get => _checked;
            set
            {
                if (_checked != value)
                {
                    _checked = value;
                    this.Invalidate();
                    CheckedChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public ToggleSwitch()
        {
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
            this.BackColor = Color.Transparent;
            this.Size = new Size(46, 24);
            this.Cursor = Cursors.Hand;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            e.Graphics.Clear(this.Parent != null ? this.Parent.BackColor : Color.FromArgb(28, 28, 32));

            Color bgColor = _checked ? Color.FromArgb(0, 130, 200) : Color.FromArgb(80, 80, 90);

            Rectangle rect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
            int radius = this.Height - 1;

            using (GraphicsPath path = GetRoundedPath(rect, radius))
            using (SolidBrush brush = new SolidBrush(bgColor))
            {
                e.Graphics.FillPath(brush, path);
            }

            int circleSize = this.Height - 6;
            int circleX = _checked ? this.Width - circleSize - 3 : 3;
            int circleY = 3;

            using (SolidBrush circleBrush = new SolidBrush(Color.White))
            {
                e.Graphics.FillEllipse(circleBrush, circleX, circleY, circleSize, circleSize);
            }
        }

        protected override void OnClick(EventArgs e)
        {
            Checked = !Checked;
            base.OnClick(e);
        }

        private GraphicsPath GetRoundedPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int d = radius;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
using System;
using System.Drawing;
using System.Windows.Forms;
using G_Formatter.Models;
using G_Formatter.Services;

namespace G_Formatter.UI
{
    public class EmojiPanel : Form
    {
        private const int EMOJI_SIZE = 38;

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x08000000;
                return cp;
            }
        }

        public EmojiPanel(Form parentMenu)
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.StartPosition = FormStartPosition.Manual;
            this.BackColor = Color.FromArgb(45, 45, 48);

            this.Size = new Size(parentMenu.Width, 140);

            this.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, this.ClientRectangle, Color.DarkGray, ButtonBorderStyle.Solid);

            CreateGrid();
            CalculatePosition(parentMenu);
        }

        private void CalculatePosition(Form parent)
        {
            int screenHeight = Screen.FromControl(parent).WorkingArea.Height;

            int targetY = parent.Bottom + 5;

            if (targetY + this.Height > screenHeight)
            {
                targetY = parent.Top - this.Height - 5;
            }

            this.Location = new Point(parent.Left, targetY);
        }

        private void CreateGrid()
        {
            FlowLayoutPanel panel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(8, 8, 0, 0),
                BackColor = Color.FromArgb(50, 50, 53)
            };

            foreach (var emoji in EmojiData.AllEmojis)
            {
                Button btn = new Button
                {
                    Text = emoji.VisualEmoji,
                    FlatStyle = FlatStyle.Flat,
                    Size = new Size(EMOJI_SIZE, EMOJI_SIZE),
                    BackColor = Color.FromArgb(60, 60, 60),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI Emoji", 14),
                    Cursor = Cursors.Hand,
                    Margin = new Padding(2)
                };
                btn.FlatAppearance.BorderSize = 0;
                new ToolTip().SetToolTip(btn, emoji.Name);

                btn.Click += async (s, e) =>
                {
                    await ClipboardHelper.PasteAndReselectAsync(emoji.Symbol, false);
                };

                btn.MouseEnter += (s, e) => btn.BackColor = Color.FromArgb(0, 120, 215);
                btn.MouseLeave += (s, e) => btn.BackColor = Color.FromArgb(60, 60, 60);

                panel.Controls.Add(btn);
            }

            this.Controls.Add(panel);
        }
    }
}
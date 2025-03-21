using System;
using System.Drawing;
using System.Windows.Forms;

namespace ZoneMaker
{
    public class MainForm : Form
    {
        private Button changeBackgroundButton;
        private Button addIconButton;
        private PictureBox iconPictureBox;
        private OpenFileDialog openFileDialog;

        public MainForm()
        {
            this.Icon = new Icon("icon.ico");
            // Initialize components
            changeBackgroundButton = new Button { Text = "Change Background", Dock = DockStyle.Top };
            addIconButton = new Button { Text = "Add Icon", Dock = DockStyle.Top };
            iconPictureBox = new PictureBox { Size = new Size(50, 50), Visible = false, BackColor = Color.Transparent };
            openFileDialog = new OpenFileDialog();

            // Add components to the form
            Controls.Add(changeBackgroundButton);
            Controls.Add(addIconButton);
            Controls.Add(iconPictureBox);

            // Event handlers
            changeBackgroundButton.Click += ChangeBackgroundButton_Click;
            addIconButton.Click += AddIconButton_Click;
            iconPictureBox.MouseMove += IconPictureBox_MouseMove;
        }

        private void ChangeBackgroundButton_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                Image backgroundImage = Image.FromFile(openFileDialog.FileName);
                BackgroundImage = backgroundImage;
                BackgroundImageLayout = ImageLayout.Stretch;
                ClientSize = backgroundImage.Size;
            }
        }

        private void AddIconButton_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                Image originalImage = Image.FromFile(openFileDialog.FileName);

                iconPictureBox.Image = originalImage;
                iconPictureBox.Size = originalImage.Size;
                iconPictureBox.Visible = true;
                iconPictureBox.Location = new Point(ClientSize.Width / 2 - iconPictureBox.Width / 2, ClientSize.Height / 2 - iconPictureBox.Height / 2);
                iconPictureBox.BringToFront();
            }
        }

        private void IconPictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                iconPictureBox.Left += e.X - iconPictureBox.Width / 2;
                iconPictureBox.Top += e.Y - iconPictureBox.Height / 2;

                double xPercent = Math.Round((double)iconPictureBox.Left / ClientSize.Width * 100, 1);
                double yPercent = Math.Round((double)iconPictureBox.Top / ClientSize.Height * 100, 1);

                Text = $"Icon Position: X = {xPercent}%, Y = {yPercent}%";
            }
        }
    }
}
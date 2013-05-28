using System.Windows.Forms;
using System.Drawing;

namespace Fysiologie
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            int h = Screen.PrimaryScreen.Bounds.Height;
            int w = Screen.PrimaryScreen.Bounds.Width;
            int fontsize = 100;
            string fontfam = "Cambria";
            int toppadding = (h / 2 - fontsize) / 2;

            this.panelTeam1 = new Panel();
            this.panelTeam2 = new Panel();
            this.nameTeam1 = new Label();
            this.nameTeam2 = new Label();
            this.comboBoxTeam1 = new ComboBox();
            this.comboBoxTeam2 = new ComboBox();
            this.timerTeam1 = new Label();
            this.timerTeam2 = new Label();
            this.timesBox = new ListView();
            this.SuspendLayout();
            //
            // panelTeam1
            //
            this.panelTeam1.Size = new Size(w, h / 2);
            this.panelTeam1.Location = new Point(0, 0);
            this.panelTeam1.Name = "panelTeam1";
            //
            // panelTeam2
            //
            this.panelTeam2.Size = new Size(w, h / 2);
            this.panelTeam2.Location = new Point(0, h / 2);
            this.panelTeam2.Name = "panelTeam2";
            // 
            // nameTeam1
            // 
            this.nameTeam1.AutoSize = true;
            this.nameTeam1.Location = new Point(80, toppadding);
            this.nameTeam1.Name = "nameTeam1";
            this.nameTeam1.Size = new Size(w / 2 - 20, h / 2 - 20);
            this.nameTeam1.TabIndex = 0;
            this.nameTeam1.Text = "";
            this.nameTeam1.Font = new Font(fontfam, fontsize);
            // 
            // nameTeam2
            // 
            this.nameTeam2.AutoSize = true;
            this.nameTeam2.Location = new Point(80, h / 2 + toppadding);
            this.nameTeam2.Name = "nameTeam2";
            this.nameTeam2.Size = new Size(w / 2 - 20, h / 2 - 20);
            this.nameTeam2.TabIndex = 2;
            this.nameTeam2.Text = "";
            this.nameTeam2.Font = new Font(fontfam, fontsize);
            //
            // comboBoxTeam1
            //
            this.comboBoxTeam1.Location = new Point(80, toppadding);
            this.comboBoxTeam1.Name = "comboBoxTeam1";
            this.comboBoxTeam1.Size = new Size(w / 2 - 20, h / 2 - 20);
            this.comboBoxTeam1.Font = new Font(fontfam, 25);
            this.comboBoxTeam1.DropDownStyle = ComboBoxStyle.DropDownList;
            this.comboBoxTeam1.KeyPress += new KeyPressEventHandler(Form1_KeyPress);
            //
            // comboBoxTeam2
            //
            this.comboBoxTeam2.Location = new Point(80, h / 2 + toppadding);
            this.comboBoxTeam2.Name = "comboBoxTeam2";
            this.comboBoxTeam2.Size = new Size(w / 2 - 20, h / 2 - 20);
            this.comboBoxTeam2.Font = new Font(fontfam, 25);
            this.comboBoxTeam2.DropDownStyle = ComboBoxStyle.DropDownList;
            this.comboBoxTeam2.KeyPress += new KeyPressEventHandler(Form1_KeyPress);

            // 
            // timerTeam1
            // 
            this.timerTeam1.AutoSize = true;
            this.timerTeam1.Location = new Point(w / 2 + 80, toppadding);
            this.timerTeam1.Name = "timerTeam1";
            this.timerTeam1.Size = new Size(w / 2 - 20, h / 2 - 20);
            this.timerTeam1.TabIndex = 1;
            this.timerTeam1.Text = "";
            this.timerTeam1.Font = new Font(fontfam, fontsize);
            // 
            // timerTeam2
            // 
            this.timerTeam2.AutoSize = true;
            this.timerTeam2.Location = new Point(w / 2 + 80, h / 2 + toppadding);
            this.timerTeam2.Name = "timerTeam2";
            this.timerTeam2.Size = new Size(w / 2 - 20, h / 2 - 20);
            this.timerTeam2.TabIndex = 3;
            this.timerTeam2.Text = "";
            this.timerTeam2.Font = new Font(fontfam, fontsize);
            //
            // timesBox
            //
            this.timesBox.Location = new Point(80, 80);
            this.timesBox.Name = "timesBox";
            this.timesBox.Size = new Size(w - 160, h - 160);
            this.timesBox.Visible = false;
            ColumnHeader c1 = new ColumnHeader();
            ColumnHeader c2 = new ColumnHeader();
            ColumnHeader c3 = new ColumnHeader();
            ColumnHeader c4 = new ColumnHeader();
            c1.Text = "Team 1";
            c2.Text = "Time 1";
            c3.Text = "Team 2";
            c4.Text = "Time 2";
            c1.Width = (w - 160) / 4;
            c2.Width = (w - 160) / 4;
            c3.Width = (w - 160) / 4;
            c4.Width = (w - 160) / 4;
            this.timesBox.Columns.AddRange(new ColumnHeader[] {c1, c2, c3, c4});
            this.timesBox.FullRowSelect = true;
            this.timesBox.AllowColumnReorder = false;
            this.timesBox.View = View.Details;
            this.timesBox.Font = new Font(fontfam, 20);
            this.timesBox.KeyPress += new KeyPressEventHandler(Form1_KeyPress);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;

            this.TopMost = true; 
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;

            this.Controls.Add(this.nameTeam1);
            this.Controls.Add(this.nameTeam2);
            this.Controls.Add(this.comboBoxTeam1);
            this.Controls.Add(this.comboBoxTeam2);
            this.Controls.Add(this.timerTeam1);
            this.Controls.Add(this.timerTeam2);
            this.Controls.Add(this.panelTeam2);
            this.Controls.Add(this.panelTeam1);
            this.Controls.Add(this.timesBox);

            this.panelTeam1.SendToBack();
            this.panelTeam2.SendToBack();

            this.Load += new System.EventHandler(Form1_Load);
            this.KeyPress += new KeyPressEventHandler(Form1_KeyPress);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private Panel panelTeam1;
        private Panel panelTeam2;
        private Label nameTeam1;
        private Label nameTeam2;
        private ComboBox comboBoxTeam1;
        private ComboBox comboBoxTeam2;
        private Label timerTeam1;
        private Label timerTeam2;
        private ListView timesBox;
    }
}


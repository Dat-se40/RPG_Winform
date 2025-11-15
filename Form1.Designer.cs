namespace BTLT04
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            lbState = new Label();
            btnPlay = new Button();
            lbWaveCount = new Label();
            lbCurrHp = new Label();
            panel1 = new Panel();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // lbState
            // 
            lbState.AutoSize = true;
            lbState.Dock = DockStyle.Left;
            lbState.Location = new Point(0, 0);
            lbState.Name = "lbState";
            lbState.Size = new Size(38, 15);
            lbState.TabIndex = 0;
            lbState.Text = "label1";
            // 
            // btnPlay
            // 
            btnPlay.BackColor = Color.Silver;
            btnPlay.Dock = DockStyle.Right;
            btnPlay.Location = new Point(723, 0);
            btnPlay.Name = "btnPlay";
            btnPlay.Size = new Size(75, 26);
            btnPlay.TabIndex = 1;
            btnPlay.Text = "Stop";
            btnPlay.UseVisualStyleBackColor = false;
            btnPlay.Click += button1_Click;
            // 
            // lbWaveCount
            // 
            lbWaveCount.AutoSize = true;
            lbWaveCount.Location = new Point(54, 0);
            lbWaveCount.Name = "lbWaveCount";
            lbWaveCount.Size = new Size(78, 15);
            lbWaveCount.TabIndex = 2;
            lbWaveCount.Text = "Wave hiện tại";
            lbWaveCount.Click += lbWaveCount_Click;
            // 
            // lbCurrHp
            // 
            lbCurrHp.AutoSize = true;
            lbCurrHp.Location = new Point(163, 0);
            lbCurrHp.Name = "lbCurrHp";
            lbCurrHp.Size = new Size(19, 15);
            lbCurrHp.TabIndex = 3;
            lbCurrHp.Text = "Hi";
            // 
            // panel1
            // 
            panel1.BackColor = Color.FromArgb(224, 224, 224);
            panel1.BorderStyle = BorderStyle.FixedSingle;
            panel1.Controls.Add(lbState);
            panel1.Controls.Add(lbWaveCount);
            panel1.Controls.Add(btnPlay);
            panel1.Controls.Add(lbCurrHp);
            panel1.Location = new Point(0, 1);
            panel1.Name = "panel1";
            panel1.Size = new Size(800, 28);
            panel1.TabIndex = 4;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(panel1);
            Name = "Form1";
            Text = "Wizard vs Zombie";
            Load += Form1_Load;
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Label lbState;
        private Button btnPlay;
        private Label lbWaveCount;
        private Label lbCurrHp;
        private Panel panel1;
    }
}

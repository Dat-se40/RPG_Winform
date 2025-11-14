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
            lbZombieCount = new Label();
            SuspendLayout();
            // 
            // lbState
            // 
            lbState.AutoSize = true;
            lbState.Location = new Point(67, 35);
            lbState.Name = "lbState";
            lbState.Size = new Size(38, 15);
            lbState.TabIndex = 0;
            lbState.Text = "label1";
            // 
            // btnPlay
            // 
            btnPlay.Location = new Point(131, 27);
            btnPlay.Name = "btnPlay";
            btnPlay.Size = new Size(75, 23);
            btnPlay.TabIndex = 1;
            btnPlay.Text = "Stop";
            btnPlay.UseVisualStyleBackColor = true;
            btnPlay.Click += button1_Click;
            // 
            // lbWaveCount
            // 
            lbWaveCount.AutoSize = true;
            lbWaveCount.Location = new Point(502, 35);
            lbWaveCount.Name = "lbWaveCount";
            lbWaveCount.Size = new Size(78, 15);
            lbWaveCount.TabIndex = 2;
            lbWaveCount.Text = "Wave hiện tại";
            // 
            // lbZombieCount
            // 
            lbZombieCount.AutoSize = true;
            lbZombieCount.Location = new Point(626, 35);
            lbZombieCount.Name = "lbZombieCount";
            lbZombieCount.Size = new Size(106, 15);
            lbZombieCount.TabIndex = 3;
            lbZombieCount.Text = "Số Zombie hiện tại";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(lbZombieCount);
            Controls.Add(lbWaveCount);
            Controls.Add(btnPlay);
            Controls.Add(lbState);
            Name = "Form1";
            Text = "Go To Hell, Bitch!";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lbState;
        private Button btnPlay;
        private Label lbWaveCount;
        private Label lbZombieCount;
    }
}

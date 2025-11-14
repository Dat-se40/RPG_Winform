namespace BTLT04
{
    partial class OpeningForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnStart = new Button();
            btnIntruc = new Button();
            btnExit = new Button();
            SuspendLayout();
            // 
            // btnStart
            // 
            btnStart.Location = new Point(244, 52);
            btnStart.Name = "btnStart";
            btnStart.Size = new Size(137, 29);
            btnStart.TabIndex = 0;
            btnStart.Text = "Bắt đầu";
            btnStart.UseVisualStyleBackColor = true;
            // 
            // btnIntruc
            // 
            btnIntruc.Location = new Point(244, 141);
            btnIntruc.Name = "btnIntruc";
            btnIntruc.Size = new Size(137, 31);
            btnIntruc.TabIndex = 1;
            btnIntruc.Text = "Hướng dẫn";
            btnIntruc.UseVisualStyleBackColor = true;
            // 
            // btnExit
            // 
            btnExit.Location = new Point(244, 226);
            btnExit.Name = "btnExit";
            btnExit.Size = new Size(137, 31);
            btnExit.TabIndex = 2;
            btnExit.Text = "Thoát";
            btnExit.UseVisualStyleBackColor = true;
            // 
            // OpeningForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(651, 402);
            Controls.Add(btnExit);
            Controls.Add(btnIntruc);
            Controls.Add(btnStart);
            Name = "OpeningForm";
            Text = "Wizard vs Zombie";
            ResumeLayout(false);
        }

        #endregion

        private Button btnStart;
        private Button btnIntruc;
        private Button btnExit;
    }
}
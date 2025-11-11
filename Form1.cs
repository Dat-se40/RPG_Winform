using BTLT04.Components;
using BTLT04.Sources;
using System;
using System.Drawing;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace BTLT04
{
    public partial class Form1 : Form
    {
        private Bitmap backBuffer;
        private Graphics graphics;
        private Timer timer;
        private Player mainPlayer; 
        private int targetFPS = 20; 
        public Form1()
        {
            InitializeComponent();
            InitGame();
        }

        private void InitGame()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            graphics = CreateGraphics();

            backBuffer = new Bitmap(ClientSize.Width, ClientSize.Height);
            mainPlayer = new Player();  
            // Timer cho game loop
            timer = new Timer { Interval = 1000/targetFPS, Enabled = true };
            timer.Tick += Update;
        }

        private void Update(object sender, EventArgs e)
        { 
            RenderGame();
            mainPlayer.Update();
        }

        private void RenderGame()
        {
            using (Graphics g = Graphics.FromImage(backBuffer))
            {
                g.Clear(Color.White);
                mainPlayer.spriteRenderer.Draw(g);
            }

            graphics.DrawImageUnscaled(backBuffer, 0, 0);
        }
    }
}

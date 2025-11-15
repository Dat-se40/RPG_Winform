#region Form1.cs - Game Main Loop & Rendering
using BTLT04.Components;
using BTLT04.Sources;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using BTLT04.Sources;
using Timer = System.Windows.Forms.Timer;
using System.Security.Cryptography;

namespace BTLT04
{
    public partial class Form1 : Form
    {
        // Back-buffer để vẽ offline → chống flicker
        private Bitmap _backBuffer;

        // Đo thời gian thực tế giữa các frame → delta time
        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

        // Tích lũy thời gian để cố định logic update (60 FPS)
        private double _accumulator = 0;
        private const double TargetFrameTimeMs = 1000.0 / 60.0; // 60 FPS cố định cho logic

        // Game objects
        private Player _mainPlayer;
        private ZombieSpawner _zombieSpawner;
        private Timer _gameTimer;
        private Tower _rmTower; 
        // Giới hạn vùng chơi (trừ viền sprite)
        private Rectangle PlayArea => ClientRectangle;
        // Nút hoạt động
        private bool _playing = true;
        public Form1()
        {
            InitializeComponent();
            InitGame();
            this.KeyPreview = true;
        }

        /// <summary>
        /// Khởi tạo toàn bộ game: style, buffer, player, timer
        /// </summary>
        private void InitGame()
        {
            // [Đạt] mấy cái này tui ko tự viết được hihi, cho ko hiểu rõ cái WinForm
            // Tối ưu WinForms rendering
            SetStyle(
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.ResizeRedraw,
                true);

            // Tạo back-buffer ban đầu
            RecreateBackBuffer();

            // Khởi tạo nhân vật với vùng chơi
            _mainPlayer = new Player(PlayArea);
            // Khởi tạo zombie
            _zombieSpawner = new ZombieSpawner(PlayArea);

            _rmTower = new Tower();
            _rmTower.Transform.Position = new PointF(PlayArea.X, PlayArea.Y + PlayArea.Height/2);
            
            // Timer chạy liên tục (interval nhỏ nhất → chính xác hơn)
            _gameTimer = new Timer { Interval = 1 };
            _gameTimer.Tick += GameLoop;
            _gameTimer.Start();

            // Đăng ký sự kiện resize
            SizeChanged += OnFormResized;
        }

        /// <summary>
        /// Game loop chính: Update → Render → Invalidate
        /// </summary>
        private void GameLoop(object sender, EventArgs e)
        {
            if (!_playing) return; 
            // Tính delta time (ms)
            double deltaMs = _stopwatch.Elapsed.TotalMilliseconds;
            _stopwatch.Restart();
            _accumulator += deltaMs;

            // Cập nhật logic với fixed timestep (tránh giật lag)
            while (_accumulator >= TargetFrameTimeMs)
            {
                double dt = TargetFrameTimeMs / 1000.0;
                _mainPlayer.Update(dt);
                _zombieSpawner.Update((float)dt);
                CheckCollisions();
                _rmTower.Update(dt);
                _accumulator -= TargetFrameTimeMs;
            }
            RenderFrame();
            Invalidate();
        }

        /// <summary>
        /// Kiểm tra va chạm giữa player, zombie, và đạn
        /// </summary>
        private void CheckCollisions()
        {
            var playerPos = _mainPlayer.Transform.Position;
            var playerRenderer = _mainPlayer.StateMachine.SpriteRenderer;

            Rectangle playerRect = playerRenderer.GetHitbox();
            Graphics g = CreateGraphics();
            // Kiểm tra va chạm với zombies
            foreach (var zombie in _zombieSpawner.Zombies)
            {
                if (!zombie.IsAlive || zombie.State == Zombie.ZombieState.Dead)
                    continue;

                var zombiePos = zombie.Transform.Position;
                var zombieRenderer = zombie.StateMachine.SpriteRenderer;

                Rectangle zombieRect = zombieRenderer.GetHitbox();  

                // Player chạm zombie
                if (playerRect.IntersectsWith(zombieRect))
                {
                    zombie.State = Zombie.ZombieState.Attacking;
                    // using (Pen pen = new Pen(Color.Blue, 2))
                    // {
                    //     g.DrawRectangle(pen, zombieRect);
                    //     g.DrawRectangle(pen, playerRect);
                    // }
                    // TODO: Gây damage cho player
                    // _mainPlayer.TakeDamage(zombie.Data.Damage);
                }
                else
                {
                    zombie.State = Zombie.ZombieState.Walking; // Cập nhật di chuyển sau khi người chơi rời 
                }
                
            }
        }

        /// <summary>
        /// Vẽ toàn bộ scene vào back-buffer
        /// </summary>
        private void RenderFrame()
        {
            using (Graphics g = Graphics.FromImage(_backBuffer))
            {
                g.Clear(Color.CornflowerBlue); // nền

                DrawLanes(g);

                // Vẽ zombies TRƯỚC (để player ở trên)
                _mainPlayer.Draw(g);
                _zombieSpawner.Draw(g);
                _rmTower.Draw(g);
            }
        }

        /// <summary>
        /// Vẽ các lanes như PvZ (optional)
        /// </summary>
        private void DrawLanes(Graphics g)
        {
            const int TotalLanes = 5;
            float laneHeight = ClientSize.Height / (float)TotalLanes;

            using (Pen lanePen = new Pen(Color.FromArgb(50, Color.Black), 2))
            {
                for (int i = 1; i < TotalLanes; i++)
                {
                    float y = i * laneHeight;
                    g.DrawLine(lanePen, 0, y, PlayArea.Width, y);
                }
            }
        }

        /// <summary>
        /// WinForms gọi khi cần vẽ → copy back-buffer ra màn hình
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            if (_backBuffer != null)
            {
                e.Graphics.DrawImageUnscaled(_backBuffer, 0, 0);
            }
            base.OnPaint(e);
        }

        /// <summary>
        /// Xử lý resize: tạo lại back-buffer, cập nhật vùng chơi
        /// </summary>
        private void OnFormResized(object sender, EventArgs e)
        {
            if (ClientSize.Width <= 0 || ClientSize.Height <= 0) return;

            RecreateBackBuffer();
            _mainPlayer.UpdatePlayArea(PlayArea);
            _zombieSpawner.UpdatePlayArea(PlayArea);
        }

        /// <summary>
        /// Tạo lại back-buffer với kích thước mới
        /// </summary>
        private void RecreateBackBuffer()
        {
            if (_backBuffer != null && !_backBuffer.Size.IsEmpty)
            {
                // Copy nội dung cũ (nếu có)
                var temp = new Bitmap(ClientSize.Width, ClientSize.Height);
                using (var g = Graphics.FromImage(temp))
                {
                    g.DrawImage(_backBuffer, 0, 0);
                }
                _backBuffer.Dispose();
                _backBuffer = temp;
            }
            else
            {
                _backBuffer?.Dispose();
                _backBuffer = new Bitmap(ClientSize.Width, ClientSize.Height);
            }
        }

        // === INPUT HANDLING ===
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            _mainPlayer.OnKeyDown(e.KeyCode);

            // Debug: hiển thị phím
            this.InvokeIfRequired(() => lbState.Text = e.KeyCode.ToString());
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            _mainPlayer.OnKeyUp(e.KeyCode);
        }

        /// <summary>
        /// Dọn dẹp tài nguyên khi form đóng
        /// </summary>
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _gameTimer?.Stop();
            _gameTimer?.Dispose();
            _backBuffer?.Dispose();
            Content.UnloadAll(); // giải phóng tất cả bitmap
            base.OnFormClosed(e);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _playing = !_playing;
            if (_playing) btnPlay.Text = "Stop";
            else btnPlay.Text = "Continue";
        }
    }

    /// <summary>
    /// Helper: Invoke nếu cần (tránh cross-thread)
    /// </summary>
    public static class ControlExtensions
    {
        public static void InvokeIfRequired(this Control control, Action action)
        {
            if (control.InvokeRequired)
                control.Invoke(action);
            else
                action();
        }
    }
}
#endregion
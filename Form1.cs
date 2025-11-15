#region Form1.cs - Game Main Loop & Rendering with Health System
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

        // ⭐ HEALTH SYSTEM
        private int _currentHealth = 100;
        private int _maxHealth = 100;
        private const int HealthPerZombie = 10; // Mỗi zombie qua trừ 10 máu

        // Giới hạn vùng chơi (trừ viền sprite)
        private Rectangle PlayArea => ClientRectangle;

        // Nút hoạt động
        private bool _playing = true;
        private bool _gameOver = false;

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

            // ⭐ Cập nhật health UI
            UpdateHealthUI();

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
            if (!_playing || _gameOver) return;

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
                lbWaveCount.Text = $"Đợi tấn công: {_zombieSpawner._currentWaveIndex + 1} / {_zombieSpawner.TotalWaves} ";
                CheckCollisions();
                CheckZombiesPassedLeft(); // ⭐ Kiểm tra zombie qua bên trái
                _accumulator -= TargetFrameTimeMs;
            }

            RenderFrame();
            Invalidate();
        }

        /// <summary>
        /// ⭐ Kiểm tra zombie đi qua bên trái màn hình
        /// </summary>
        private void CheckZombiesPassedLeft()
        {
            foreach (var zombie in _zombieSpawner.Zombies.ToList())
            {
                // Bỏ qua zombie đã chết
                if (!zombie.IsAlive || zombie.State == Zombie.ZombieState.Dead)
                    continue;

                // Kiểm tra zombie đã qua bên trái
                if (zombie.Transform.Position.X < -50) // Cho phép đi qua 1 chút
                {
                    TakeDamage(HealthPerZombie);

                    // Đánh dấu zombie để remove (tránh trừ máu nhiều lần)
                    zombie.TakeDamage(999999); // Kill luôn

                    Debug.WriteLine($"⚠️ Zombie escaped! Health: {_currentHealth}/{_maxHealth}");
                }
            }
        }

        /// <summary>
        /// ⭐ Trừ máu player
        /// </summary>
        private void TakeDamage(int damage)
        {
            _currentHealth -= damage;
            if (_currentHealth < 0) _currentHealth = 0;

            UpdateHealthUI();

            // Game Over
            if (_currentHealth <= 0)
            {
                GameOver();
            }
        }

        /// <summary>
        /// ⭐ Cập nhật UI thanh máu
        /// </summary>
        private void UpdateHealthUI()
        {
            this.InvokeIfRequired(() =>
            {
                lbCurrHp.Text = $" HP nhà chính: {_currentHealth}/{_maxHealth}";

                // Đổi màu label theo máu
                if (_currentHealth > 60)
                    lbCurrHp.ForeColor = Color.Green;
                else if (_currentHealth > 30)
                    lbCurrHp.ForeColor = Color.Orange;
                else
                    lbCurrHp.ForeColor = Color.Red;
            });
        }

        /// <summary>
        /// ⭐ Xử lý Game Over
        /// </summary>
        private void GameOver()
        {
            _gameOver = true;
            _playing = false;
            _gameTimer?.Stop();

            var result = MessageBox.Show(
                $"Game Over!\n\nBạn đã hết máu!\nZombies spawned: {_zombieSpawner._zombiesSpawned}\n\nChơi lại?",
                "Game Over",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Error
            );

            if (result == DialogResult.Yes)
            {
                RestartGame();
            }
            else
            {
                this.Close();
            }
        }

        /// <summary>
        /// ⭐ Restart game
        /// </summary>
        private void RestartGame()
        {
            // Reset health
            _currentHealth = _maxHealth;
            UpdateHealthUI();

            // Reset game state
            _gameOver = false;
            _playing = true;

            // Reset zombies
            _zombieSpawner.Reset();

            // Reset player position
            _mainPlayer = new Player(PlayArea);

            // Restart timer
            _stopwatch.Restart();
            _accumulator = 0;
            _gameTimer.Start();

            Debug.WriteLine("🔄 Game Restarted!");
        }

        /// <summary>
        /// Kiểm tra va chạm giữa player, zombie, và đạn
        /// </summary>
        private void CheckCollisions()
        {
            var playerPos = _mainPlayer.Transform.Position;
            var playerRenderer = _mainPlayer.StateMachine.SpriteRenderer;
            Rectangle playerRect = playerRenderer.GetHitbox();

            // Kiểm tra va chạm với zombies
            foreach (var zombie in _zombieSpawner.Zombies)
            {
                // Bỏ qua zombie đã chết
                if (!zombie.IsAlive || zombie.State == Zombie.ZombieState.Dead)
                    continue;

                var zombiePos = zombie.Transform.Position;
                var zombieRenderer = zombie.StateMachine.SpriteRenderer;
                Rectangle zombieRect = zombieRenderer.GetHitbox();

                // Player chạm zombie
                if (playerRect.IntersectsWith(zombieRect))
                {
                    zombie.State = Zombie.ZombieState.Attacking;
                    // TODO: Gây damage cho player (va chạm trực tiếp)
                    // TakeDamage(1); 
                }
                else
                {
                    // Chỉ chuyển về Walking nếu không phải Dead
                    if (zombie.State != Zombie.ZombieState.Dead)
                    {
                        zombie.State = Zombie.ZombieState.Walking;
                    }
                }
            }

            // === Kiểm tra va chạm giữa đạn và zombie ===
            foreach (var proj in _mainPlayer.Projectiles.ToList())
            {
                if (proj.IsExpired) continue;

                Rectangle projRect = proj.GetHitbox();
                bool hitSomething = false;

                foreach (var zombie in _zombieSpawner.Zombies.ToList())
                {
                    if (!zombie.IsAlive || zombie.State == Zombie.ZombieState.Dead)
                        continue;

                    Rectangle zombieRect = zombie.StateMachine.SpriteRenderer.GetHitbox();

                    if (projRect.IntersectsWith(zombieRect))
                    {
                        zombie.TakeDamage(proj.Damage);
                        proj.Expire();
                        hitSomething = true;
                        break;
                    }
                }

                if (hitSomething) break;
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
                _zombieSpawner.Draw(g);
                _mainPlayer.Draw(g);
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

            // ⭐ Cheat code: Nhấn R để restart
            if (e.KeyCode == Keys.R && _gameOver)
            {
                RestartGame();
            }
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
            if (_gameOver) return; // Không cho pause khi game over

            _playing = !_playing;
            if (_playing) btnPlay.Text = "Stop";
            else btnPlay.Text = "Continue";
        }

        private void lbWaveCount_Click(object sender, EventArgs e)
        {
            if (_gameOver) return;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

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
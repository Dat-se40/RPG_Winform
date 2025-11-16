#region Form1.cs - Game Main Loop & Rendering with Health System
using BTLT04.Components;
using BTLT04.Sources;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
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
        private int _maxHealth = 200;
        private int _currentHealth = 200;
        private const int HealthPerZombie = 10; // Mỗi zombie qua trừ 10 máu

        // Giới hạn vùng chơi (trừ viền sprite), lấy lbCurrHP làm móc
        private Rectangle PlayArea => CalcPlayArea(); 

        // Nút hoạt động
        private bool _playing = true;
        private bool _gameOver = false;
        // Background
        string path;
        public Form1()
        {
            InitializeComponent();
            InitGame();
            this.KeyPreview = true;
            this.BackgroundImage = Image.FromFile(AbsPath(rePath: @"Sources\\Background\\R.png"));
            this.BackgroundImageLayout = ImageLayout.Stretch;
            
        }
        private Rectangle CalcPlayArea() 
        {
            return new Rectangle(
                ClientRectangle.X + lbCurrHp.Bounds.X,
                ClientRectangle.Y,
                ClientRectangle.Width - lbCurrHp.Bounds.X,
                ClientRectangle.Height
            );
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
                // Skip zombie chết
                if (!zombie.IsAlive || zombie.State == Zombie.ZombieState.Dead)
                    continue;

                // Khi zombie đi qua bên trái màn hình
                if (zombie.Transform.Position.X < lbCurrHp.Bounds.X)
                {
                    TakeDamage(HealthPerZombie);

                    // Xóa zombie khỏi game ngay lập tức
                    _zombieSpawner.RemoveZombie(zombie);

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
                lbCurrHp.Text = $" Sinh mệnh hiện tại: {_currentHealth}/{_maxHealth}";

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
                $"Game Over!\nChơi lại?",
                "Game Over",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
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
            foreach (var zombie in _zombieSpawner.Zombies.ToList())
            {
                // Bỏ qua zombie đã chết
                if (!zombie.IsAlive || zombie.State == Zombie.ZombieState.Dead)
                    continue;

                var zombiePos = zombie.Transform.Position;
                var zombieRenderer = zombie.StateMachine.SpriteRenderer;
                Rectangle zombieRect = zombieRenderer.GetHitbox();
                
                // Đòn tấn công cuối cùng của zombie mới gây damage
                if (zombie.State == Zombie.ZombieState.Attacking && zombie.StateMachine.isLastFrame) TakeDamage(5);
                // Player chạm zombie
                if (playerRect.IntersectsWith(zombieRect) )
                {
                    zombie.State = Zombie.ZombieState.Attacking;
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
                g.Clear(Color.Transparent); 

                //DrawLanes(g);

                // Vẽ zombies TRƯỚC (để player ở trên)
                _zombieSpawner.Draw(g);
                _mainPlayer.Draw(g);
            }
        }

        /// <summary>
        /// Vẽ các lanes như PvZ (optional)
        /// </summary>
        //private void DrawLanes(Graphics g)
        //{
        //    const int TotalLanes = 5;
        //    float laneHeight = ClientSize.Height / (float)TotalLanes;

        //    using (Pen lanePen = new Pen(Color.FromArgb(50, Color.Black), 2))
        //    {
        //        for (int i = 1; i < TotalLanes; i++)
        //        {
        //            float y = i * laneHeight;
        //            g.DrawLine(lanePen, 0, y, PlayArea.Width, y);
        //        }
        //    }
        //}

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
            if (_playing) btnPlay.Text = "Dừng";
            else btnPlay.Text = "Tiếp tục";
        }

        private void lbWaveCount_Click(object sender, EventArgs e)
        {
            if (_gameOver) return;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            SoundManager.Instance.PlayBackground(
                AbsPath(@"Sources\Sound\background.mp3"), loop: true);
        }

        private void btnGuide_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Nhiệm vụ của bản là bảo vể bản thân và không cho zombie tấn công nhà bạn!\n" +
             "W/S/D/A: lên/xuống/trái phải \n" +
             "Q/E: kích hoạt kĩ năng tấn công \n",
             "Hướng dẫn", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        public static string AbsPath(string rePath)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\" + rePath);
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
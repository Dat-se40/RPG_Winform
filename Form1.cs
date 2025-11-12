#region Form1.cs - Game Main Loop & Rendering
using BTLT04.Components;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

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
        private Timer _gameTimer;

        // Giới hạn vùng chơi (trừ viền sprite)
        private Rectangle PlayArea => ClientRectangle;

        public Form1()
        {
            InitializeComponent();
            InitGame();
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
            // Tính delta time (ms)
            double deltaMs = _stopwatch.Elapsed.TotalMilliseconds;
            _stopwatch.Restart();
            _accumulator += deltaMs;

            // Cập nhật logic với fixed timestep (tránh giật lag)
            while (_accumulator >= TargetFrameTimeMs)
            {
                _mainPlayer.Update(TargetFrameTimeMs / 1000.0);
                _accumulator -= TargetFrameTimeMs;
            }
            RenderFrame();
            Invalidate();
        }

        /// <summary>
        /// Vẽ toàn bộ scene vào back-buffer
        /// </summary>
        private void RenderFrame()
        {
            using (Graphics g = Graphics.FromImage(_backBuffer))
            {
                g.Clear(Color.CornflowerBlue); // nền
                _mainPlayer.Draw(g);

                // [Tùy chọn] Vẽ debug info
                // g.DrawString($"FPS: ~{1000 / _stopwatch.Elapsed.TotalMilliseconds:F1}", 
                //              new Font("Consolas", 10), Brushes.White, 10, 10);
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
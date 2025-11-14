#region SpriteRenderer.cs - Frame Animation with Delta Time
using System;
using System.Collections.Generic;
using System.Drawing;
// Có lẽ sẽ xóa bớt mất cái ngoại lệ
namespace BTLT04.Components
{
    /// <summary>
    /// Quản lý sprite sheet: chia frame, cập nhật animation theo thời gian thực
    /// </summary>
    public class SpriteRenderer
    {
        public int FrameCount { get; }
        public int CurrentFrame { get; private set; } = 0;
        public int FrameWidth { get; private set; }
        public int FrameHeight { get; private set; }

        private readonly Transform _transform; // Reference
        private readonly Bitmap _spriteSheet;
        private float _frameTimer = 0f;
        private const float FrameDuration = 0.1f; // 10 FPS

        public SpriteRenderer(Bitmap sheet, int frameCount, Transform transform)
        {
            _spriteSheet = sheet ?? throw new ArgumentNullException(nameof(sheet));
            if (frameCount <= 0) throw new ArgumentOutOfRangeException(nameof(frameCount));
            _transform = transform ?? throw new ArgumentNullException(nameof(transform));

            FrameCount = frameCount;
            SetupFrames();
        }

        private void SetupFrames()
        {
            FrameWidth = _spriteSheet.Width / FrameCount;
            FrameHeight = _spriteSheet.Height;
            if (FrameWidth <= 0) throw new InvalidOperationException("Sprite sheet không hợp lệ.");
        }

        public void Update(float deltaTime)
        {
            _frameTimer += deltaTime;
            if (_frameTimer >= FrameDuration)
            {
                CurrentFrame = (CurrentFrame + 1) % FrameCount;
                _frameTimer -= FrameDuration;
            }
        }

        public void Draw(Graphics g)
        {
            if (g == null) return;
            var pos = _transform.Position;
            var src = new Rectangle(CurrentFrame * FrameWidth, 0, FrameWidth, FrameHeight);
            //var dest = new Rectangle((int)pos.X, (int)pos.Y, FrameWidth, FrameHeight);

            //scale ảnh
            int scaledWidth = (int)(FrameWidth * _transform.Scale);
            int scaledHeight = (int)(FrameHeight * _transform.Scale);

            // Dựa theo scale để vẽ sprite to/nhỏ
            var dest = new Rectangle(
                (int)pos.X, (int)pos.Y,
                scaledWidth, scaledHeight
            );

            g.DrawImage(_spriteSheet, dest, src, GraphicsUnit.Pixel);
        }
        public bool isLastFrame() 
        {
            if (CurrentFrame == FrameCount - 1) return true;
            return false;
        }
    }

    /// <summary>
    /// Content Manager: cache bitmap, tránh tải lại, tự động dispose
    /// </summary>
    public static class Content
    {
        private static readonly Dictionary<string, Bitmap> Cache = new();

        /// <summary>
        /// Tải bitmap từ đường dẫn tương đối (dựa trên exe)
        /// </summary>
        public static Bitmap Load(string relativePath)
        {
            string fullPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\\" + relativePath);

            if (Cache.TryGetValue(fullPath, out var bmp))
                return bmp;

            if (!System.IO.File.Exists(fullPath))
                throw new System.IO.FileNotFoundException($"Không tìm thấy file: {relativePath}", fullPath);

            bmp = new Bitmap(fullPath);
            Cache[fullPath] = bmp;
            return bmp;
        }

        /// <summary>
        /// Giải phóng toàn bộ bitmap
        /// </summary>
        public static void UnloadAll()
        {
            foreach (var bmp in Cache.Values)
            {
                bmp.Dispose();
            }
            Cache.Clear();
        }
    }
}
#endregion
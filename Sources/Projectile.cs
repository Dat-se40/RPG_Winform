using BTLT04.Components;
using System;
using System.Collections.Generic;
using System.Drawing;

/// <summary>
/// Đại diện cho viên đạn / chiêu thức tấn công
/// </summary>

namespace BTLT04.Sources
{
    internal class Projectile
    {
        public Transform Transform { get; }
        public SpriteRenderer Sprite { get; }

        private readonly float _speed;      // pixel/giây
        private readonly float _range;      // phạm vi tối đa
        private readonly PointF _direction; // hướng bay
        private readonly PointF _origin;    // vị trí bắt đầu

        public bool IsExpired { get; private set; } = false;
        private readonly bool _rotate;
        public Projectile(PointF start, PointF direction, string spritePath,
                          int frames, float speed, float range, float scale = 1f, bool rotate = false)
        {
            _rotate = rotate; //xoay ảnh đạn (đạn 2)

            Transform = new Transform
            {
                Position = start,
                Velocity = new PointF(direction.X * speed, direction.Y * speed)
            };
            Transform.Scale = scale; //thay đổi kích cỡ đạn

            Sprite = new SpriteRenderer(Content.Load(spritePath), frames, Transform);
            _origin = start;
            _direction = direction;
            _speed = speed;
            _range = range;
        }

        public void Update(float dt)
        {
            if (IsExpired) return;
            Transform.Update(dt);
            Sprite.Update(dt);

            float traveled = Distance(_origin, Transform.Position);
            if (traveled >= _range)
                IsExpired = true;
        }

        public void Draw(Graphics g)
        {
            if (IsExpired) return;

            if (_rotate)
            {
                var pos = Transform.Position;
                g.TranslateTransform(pos.X + Sprite.FrameWidth / 2, pos.Y + Sprite.FrameHeight / 2);
                g.RotateTransform(-90);
                g.TranslateTransform(-(pos.X + Sprite.FrameWidth / 2), -(pos.Y + Sprite.FrameHeight / 2));
            }

            Sprite.Draw(g);  // Vẽ một lần, sau khi xoay nếu cần

            if (_rotate)
                g.ResetTransform();
        }


        private static float Distance(PointF a, PointF b)
        {
            float dx = a.X - b.X, dy = a.Y - b.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }
    }
}


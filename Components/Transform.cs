using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTLT04.Components
{
    public class Transform
    {
        public PointF Position { get; set; } = new PointF(100, 100);
        public float Rotation { get; set; } = 0f;        // độ (chưa dùng, để mở rộng)
        public float Scale { get; set; } = 1f;           // tỉ lệ (chưa dùng)

        public PointF Velocity { get; set; } = PointF.Empty; // pixel/giây

        /// <summary>
        /// Cập nhật vị trí theo vận tốc và delta time
        /// </summary>
        public void Update(float deltaTime)
        {
            if (Velocity.IsEmpty) return;

            Position = new PointF(
                Position.X + Velocity.X * deltaTime,
                Position.Y + Velocity.Y * deltaTime
            );
        }

        /// <summary>
        /// Reset vận tốc về 0
        /// </summary>
        public void Stop() => Velocity = PointF.Empty;
    }
}

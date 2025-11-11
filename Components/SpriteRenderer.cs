using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTLT04.Components
{
    public class SpriteRenderer
    {
        private Bitmap spriteSheet;
        private int frameCount;
        private int currentFrame;
        private int frameWidth;
        private int frameHeight;

        public Point Position { get; set; }

        public SpriteRenderer(string spritePath, int frameCount)
        {
            spriteSheet = new Bitmap(spritePath);
            this.frameCount = frameCount;
            currentFrame = 0;
            frameWidth = spriteSheet.Width / frameCount;
            frameHeight = spriteSheet.Height;
            Position = new Point(100, 100);
        }

        public void Update()
        {
            // Cập nhật frame
            currentFrame = (currentFrame + 1) % frameCount;
        }
           

        public void Draw(Graphics g)
        {
            Rectangle srcRect = new Rectangle(currentFrame * frameWidth, 0, frameWidth, frameHeight);
            Rectangle destRect = new Rectangle(Position.X, Position.Y, frameWidth, frameHeight);
            g.DrawImage(spriteSheet, destRect, srcRect, GraphicsUnit.Pixel);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTLT04.Components
{
    public class SpriteRenderer
    {
        public int frameCount;
        public int currentFrame;
        private int frameWidth;
        private int frameHeight;

        public Point Position { get; set; }
        private Bitmap spriteSheet;

        public Bitmap SpriteSheet
        {
            get { return spriteSheet; }
            set { 
                spriteSheet = value;
                SetUpStat();
            }
        }


        public SpriteRenderer(Bitmap image , int frameCount) 
        {
            this.frameCount = frameCount;
            Position = new Point(100, 100);
            SpriteSheet = image;
        }

        public void SetUpStat() 
        {
            currentFrame = 0;
            frameWidth = SpriteSheet.Width / frameCount;
            frameHeight = SpriteSheet.Height; 
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
            g.DrawImage(SpriteSheet, destRect, srcRect, GraphicsUnit.Pixel);
        }
    }
}

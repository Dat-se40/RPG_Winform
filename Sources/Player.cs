using BTLT04.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTLT04.Sources
{
    internal class Player
    {
        public StateMachine stateMachine;
        private HashSet<Keys> pressedKeys = new HashSet<Keys>(); // Lưu các phím người chơi nhấn 
        int speed = 10; 
        public Player() 
        {
            this.stateMachine = new StateMachine();
            stateMachine.AddState("Idle", "D:\\third\\M\\IT008\\BTLT04\\Sources\\Player\\Idle.png", 6);
            stateMachine.AddState("Walk", "D:\\third\\M\\IT008\\BTLT04\\Sources\\Player\\Walk.png", 7);
            stateMachine.AddState("Attack2", "D:\\third\\M\\IT008\\BTLT04\\Sources\\Player\\Attack_2.png", 4);
            stateMachine.ChangeState("Idle"); // Mặc định đứng yên
        }
        public void Update() 
        {
            Move();
            stateMachine.Update();
        }
        public void OnKeyUp(Keys key)
        {
            pressedKeys.Remove(key);
        }
        public void OnKeyDown(Keys key)
        {
            pressedKeys.Add(key);
        }

        public void Move()
        {
            var pos = new Point(stateMachine.SpritePostion.X, stateMachine.SpritePostion.Y); // Copy giá trị

            if (pressedKeys.Contains(Keys.W)) pos.Y -= speed; 
            if (pressedKeys.Contains(Keys.S)) pos.Y += speed;
            if (pressedKeys.Contains(Keys.A)) pos.X -= speed;
            if (pressedKeys.Contains(Keys.D)) pos.X += speed;
          
            if (stateMachine.SpritePostion != pos)
            {
                stateMachine.ChangeState("Walk");
                stateMachine.SpritePostion = pos;
            }
            else
            {
                stateMachine.ChangeState("Idle");
            }
        }
    }
    internal class StateMachine
    {
        public SpriteRenderer spriteRenderer;
        private SpriteRenderer prevSpriteRenderer; 
        private Dictionary<string, KeyValuePair<Bitmap,int>> states;
        public Point SpritePostion
        {
            get { return spriteRenderer.Position; }
            set { spriteRenderer.Position  = value; }
        }
        public StateMachine() 
        {
            states = new Dictionary<string, KeyValuePair<Bitmap, int>>() ;
            spriteRenderer = new SpriteRenderer(new Bitmap(1, 1), 1);
            prevSpriteRenderer = new SpriteRenderer(new Bitmap(1, 1), 1);
        }
        // Set FrameCount truoc roi toi Bit map ; 
        public void ChangeState(string name) 
        {
           if (states.ContainsKey(name)) 
           {
                var newImage = states[name].Key; 
                
                if (newImage != prevSpriteRenderer.SpriteSheet) 
                {
                    prevSpriteRenderer = spriteRenderer;
                    spriteRenderer.frameCount = states[name].Value;
                    spriteRenderer.SpriteSheet = newImage;
                }
           }
        }
        public void Update() 
        {
            spriteRenderer.Update(); 
        }   
        public void AddState(string name, string path, int frameCount)
        {
            if (!states.ContainsKey(name))
            {
                var pair = new KeyValuePair<Bitmap, int>(new Bitmap(path), frameCount); 
                states.Add(name, pair);
            } 
        }
      
    }
}

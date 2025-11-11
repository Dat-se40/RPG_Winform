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
        public SpriteRenderer spriteRenderer;
        public Player() 
        {
            spriteRenderer = new SpriteRenderer("D:\\third\\M\\IT008\\BTLT04\\Sources\\Player\\Attack_2.png", 4);
        }
        public void Update() 
        {
            spriteRenderer.Update();    
        }
    }
}

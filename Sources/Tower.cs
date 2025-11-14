using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transform = BTLT04.Components.Transform;

namespace BTLT04.Sources
{
    internal class Tower
    {
        public StateMachine StateMachine { get; }
        public Transform Transform { get; } = new Transform(); // MỚI

        public Tower() 
        {   
            StateMachine = new StateMachine(Transform) { };
            StateMachine.AddState("Idle",@"Sources\Building\RedMoonTower.png",11);
            StateMachine.ChangeState("Idle");
        }
        public void Update(double dt) 
        {
            StateMachine.Update((float)dt); 
        }
        public void Draw(Graphics g) => StateMachine.SpriteRenderer.Draw(g);
    }
}

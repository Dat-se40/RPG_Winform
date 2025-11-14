using BTLT04.Components;
using BTLT04.Sources;
using Transform = BTLT04.Components.Transform;

internal class Zombie
{
    public StateMachine StateMachine { get; }
    public Transform Transform { get; } = new Transform();
    public ZombieData Data { get; private set; }
        
    public int Health { get; private set; }
    public int LaneIndex { get; set; }
    public bool IsAlive => Health > 0;
    public ZombieState State { get; set; } = ZombieState.Walking;
        
    private float _deadTimer = 0f;
    private const float DeadDuration = 1f; // Thời gian hiển thị animation chết
        
    public enum ZombieState
    {
        Idle,
        Walking,
        Attacking,
        Dead
    }
    
    public Zombie(ZombieData data, PointF startPosition, int laneIndex)
    {
        Data = data;
        Health = data.MaxHealth;
        LaneIndex = laneIndex;
        Transform.Position = startPosition;
            
        StateMachine = new StateMachine(Transform);
            
        StateMachine.AddState("Walk", data.WalkSpritePath, data.WalkFrameCount);
        StateMachine.AddState("Attack", data.AttackSpritePath, data.AttackFrameCount);
        StateMachine.AddState("Dead", data.DeadSpritePath, data.DeadFrameCount);
            
        StateMachine.ChangeState("Idle");
        SetHixBox();
    }
    void SetHixBox()
    {
        // Cập nhật các công thức tính 
        void SetHixBox()
        {
            // Xác định phần thân thực của zombie (bỏ khoảng trắng xung quanh).

            var renderer = StateMachine.SpriteRenderer;

            // Ví dụ sẽ thay số sau
            renderer.HitboxOffsetX = 12;  // pixel tính từ cạnh trái của frame
            renderer.HitboxOffsetY = 5;   // pixel tính từ cạnh trên frame
            renderer.HitboxWidth = 45;  // chiều rộng phần thân zombie
            renderer.HitboxHeight = 85;  // chiều cao thực tế
        }
    }
    public void Update(float deltaTime)
    {
        // Xử lý animation chết
        if (State == ZombieState.Dead)
        {
            _deadTimer += deltaTime;
            Transform.Velocity = PointF.Empty;
            StateMachine.Update(deltaTime);
            return;
        }
            
        if (!IsAlive) return;
            
        // Di chuyển về bên trái
        if (State == ZombieState.Walking)
        {
            Transform.Velocity = new PointF(-Data.Speed, 0);
        }
        else if (State == ZombieState.Attacking)
        {
            Transform.Velocity = PointF.Empty; // Dừng lại khi attack
        }
            
        Transform.Update(deltaTime);
        StateMachine.Update(deltaTime);
        UpdateAnimation();
    }
    
    private void UpdateAnimation()
    {
        string newState = State switch
        {
            ZombieState.Walking => "Walk",
            ZombieState.Attacking => "Attack",
            ZombieState.Dead => "Dead",
            _ => "Idle"
        };
            
        StateMachine.ChangeState(newState);
    }
    
    public void TakeDamage(int damage)
    {
        if (!IsAlive) return;
            
        Health -= damage;
            
        if (Health <= 0)
        {
            Health = 0;
            State = ZombieState.Dead;
        }
    }
    
    public void Draw(Graphics g)
    {
        // Không vẽ nếu đã chết quá lâu
        if (State == ZombieState.Dead && _deadTimer > DeadDuration)
            return;
            
        StateMachine.SpriteRenderer.Draw(g);
    }
    
    public bool IsOffScreen()
    {
        return Transform.Position.X < -100;
    }
        
    public bool ShouldBeRemoved()
    {
        return (!IsAlive && _deadTimer > DeadDuration) || IsOffScreen();
    }



}
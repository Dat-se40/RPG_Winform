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
        
        StateMachine.OnStateChanged += UpdateHitBox;
        UpdateHitBox();
        // SetHixBox();
    }
    
    void UpdateHitBox()
    {
        var renderer = StateMachine.SpriteRenderer;
            
        // Điều chỉnh hitbox theo type của zombie
        switch (Data.Type)
        {
            case ZombieType.Normal:
                renderer.HitboxOffsetX = 60;
                renderer.HitboxOffsetY = 45;
                renderer.HitboxWidth = 36;
                renderer.HitboxHeight = 90;
                break;
                    
            case ZombieType.Fast:
                renderer.HitboxOffsetX = 30;
                renderer.HitboxOffsetY = 5;
                renderer.HitboxWidth = 50;
                renderer.HitboxHeight = 80;
                break;
        }
    }
    
    // void SetHixBox()
    // {
    //     // Cập nhật các công thức tính 
    //     var sprite = StateMachine.SpriteRenderer;
    //
    //     // Offset: vị trí bắt đầu của hitbox trong frame
    //     sprite.HitboxOffsetX = 60; 
    //     sprite.HitboxOffsetY = 60;
    //
    //     // Width/Height: kích thước vùng hitbox (không trừ offset!)
    //     sprite.HitboxWidth = 36;   // Ví dụ: từ x=100 đến x=180
    //     sprite.HitboxHeight = 90;
    // }
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
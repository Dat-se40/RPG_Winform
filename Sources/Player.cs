using BTLT04.Components;
using BTLT04.Sources;
using Transform = BTLT04.Components.Transform; 
internal class Player
{
    //attack
    private readonly List<Projectile> _projectiles = new();
    public IEnumerable<Projectile> Projectiles => _projectiles;
    //
    public StateMachine StateMachine { get; }
    public Transform Transform { get; } = new Transform(); // MỚI
    private readonly HashSet<Keys> _pressedKeys = new();
    private const float Speed = 360f; // pixel/giây
    private Rectangle _playArea;

    public Player(Rectangle playArea)
    {
        _playArea = playArea;
        StateMachine = new StateMachine(Transform); // Truyền Transform vào

        StateMachine.AddState("Idle", @"Sources\Player\Idle.png", 6);
        StateMachine.AddState("Walk", @"Sources\Player\Walk.png", 7);
        StateMachine.AddState("Attack2", @"Sources\Player\Attack_2.png", 4);
        StateMachine.AddState("Attack1", @"Sources\Player\Attack_1.png", 10);
        StateMachine.ChangeState("Idle");
        
        StateMachine.OnStateChanged += UpdateHitBox;
        UpdateHitBox();
        // SetHitBox(); 
    }
    
    void UpdateHitBox()
    {
        var renderer = StateMachine.SpriteRenderer;
        
        renderer.HitboxOffsetX = 50;
        renderer.HitboxOffsetY = 60;
        renderer.HitboxWidth = 28;
        renderer.HitboxHeight = 65;
        
        // switch (StateMachine._currentState)
        // {
        //     case "Idle":
        //         renderer.HitboxOffsetX = 50;
        //         renderer.HitboxOffsetY = 60;
        //         renderer.HitboxWidth = 28;
        //         renderer.HitboxHeight = 65;
        //         break;
        //             
        //     case "Walk":
        //         renderer.HitboxOffsetX = 50;
        //         renderer.HitboxOffsetY = 60;
        //         renderer.HitboxWidth = 28;
        //         renderer.HitboxHeight = 65;
        //         break;
        //             
        //     case "Attack1":
        //     case "Attack2":
        //         renderer.HitboxOffsetX = 50;
        //         renderer.HitboxOffsetY = 60;
        //         renderer.HitboxWidth = 28;
        //         renderer.HitboxHeight = 65;
        //         break;
        //             
        //     default:
        //         renderer.HitboxOffsetX = 50;
        //         renderer.HitboxOffsetY = 60;
        //         renderer.HitboxWidth = 28;
        //         renderer.HitboxHeight = 65;
        //         break;
        // }
    }
    
    // void SetHitBox()
    // {
    //     // Cập nhập các công thức tính sau
    //     var sprite = StateMachine.SpriteRenderer;
    //
    //     // Offset: vị trí bắt đầu của hitbox trong frame
    //     sprite.HitboxOffsetX = 50; 
    //     sprite.HitboxOffsetY = 70;
    //
    //     // Width/Height: kích thước vùng hitbox (không trừ offset!)
    //     sprite.HitboxWidth = 28;   // Ví dụ: từ x=100 đến x=180
    //     sprite.HitboxHeight = 28;
    // }
    public void Update(double deltaTime)
    {
        float dt = (float)deltaTime;
        HandleInput(dt);
        UpdateState();
        Transform.Update(dt); // Tự động di chuyển theo velocity
        ClampToBounds();
        StateMachine.Update(dt);

        // === Update projectiles ===
        foreach (var prj in _projectiles)
            prj.Update(dt);

        // Loại bỏ đạn đã hết phạm vi
        _projectiles.RemoveAll(p => p.IsExpired);

    }

    private void HandleInput(float dt)
    {
        var vel = PointF.Empty;
        if (_pressedKeys.Contains(Keys.W)) vel.Y -= Speed;
        if (_pressedKeys.Contains(Keys.S)) vel.Y += Speed;
        if (_pressedKeys.Contains(Keys.A)) vel.X -= Speed;
        if (_pressedKeys.Contains(Keys.D)) vel.X += Speed;

        Transform.Velocity = vel; // Gán vận tốc
    }

    private void ClampToBounds()
    {
        var renderer = StateMachine.SpriteRenderer;
        int w = renderer.FrameWidth, h = renderer.FrameHeight;
        var pos = Transform.Position;

        pos.X = Math.Max(_playArea.X, Math.Min(_playArea.Right - w, pos.X));
        pos.Y = Math.Max(_playArea.Y, Math.Min(_playArea.Bottom - h, pos.Y));

        Transform.Position = pos;
        Transform.Velocity = PointF.Empty; // Dừng nếu chạm biên
    }

    private void UpdateState()
    {
        if ((StateMachine._currentState == "Attack1" || StateMachine._currentState == "Attack2") &&
              !StateMachine.SpriteRenderer.isLastFrame()) return;
        StateMachine.ChangeState(Transform.Velocity.IsEmpty ? "Idle" : "Walk");
    }

    // Input
    public void OnKeyDown(Keys key)
    {
        _pressedKeys.Add(key); 

        //attack
        if (key == Keys.Q)
            Attack(1);
        else if (key == Keys.E)
            Attack(2);
    }


    public void OnKeyUp(Keys key) => _pressedKeys.Remove(key);

    public void Draw(Graphics g)
    {
        // Vẽ nhân vật
        StateMachine.SpriteRenderer.Draw(g);

        // Vẽ tất cả đạn
        foreach (var prj in _projectiles)
            prj.Draw(g);
    }

    public void UpdatePlayArea(Rectangle area) => _playArea = area;

    //Add attack Q E
    private void Attack(int type)
    {
        string spritePath;
        float speed;
        float range;

        // Căn giữa nhân vật, bắn ra từ giữa thân – thấp hơn một chút
        var projPos = new PointF(
            Transform.Position.X + StateMachine.SpriteRenderer.FrameWidth * 0.65f,
            Transform.Position.Y + StateMachine.SpriteRenderer.FrameHeight * 0.65f
            );


        if (type == 1)
        {
            spritePath = @"Sources\Projectile\PlayerAttack1Prj.png";
            speed = 300f;
            range = 350f; //phạm vi bay
        }
        else
        {
            spritePath = @"Sources\Projectile\PlayerAttack2Prj.png";
            speed = 220f;
            range = 200f; //phạm vi bay
        }

        // Chiều bắn
        var direction = new PointF(1, 0);

        // Nếu là loại 2 (E) xoay sprite -90 độ
        bool rotate = (type == 2);

        var proj = new Projectile(projPos, direction, spritePath, 6, speed, range, 1.5f, rotate);
        _projectiles.Add(proj);
        StateMachine.ChangeState("Attack" + type.ToString());
    }

}

// === StateMachine cập nhật ===
// internal class StateMachine
// {
//     public SpriteRenderer SpriteRenderer { get; private set; }
//     private readonly Transform _transform; // Giữ reference
//     private readonly Dictionary<string, (Bitmap sheet, int frameCount)> _states = new();
//     public string _currentState = "";
//     
//     public event Action OnStateChanged;
//
//     public StateMachine(Transform transform)
//     {
//         _transform = transform;
//         SpriteRenderer = new SpriteRenderer(new Bitmap(1, 1), 1, transform);
//     }
//
//     public void AddState(string name, string path, int frameCount)
//     {
//         if (_states.ContainsKey(name)) return;
//         _states[name] = (Content.Load(path), frameCount);
//     }
//
//     public void ChangeState(string name)
//     {
//         if (_currentState == name || !_states.ContainsKey(name)) return;
//         var (sheet, count) = _states[name];
//         SpriteRenderer = new SpriteRenderer(sheet, count, _transform);
//         _currentState = name;
//         
//         //
//         OnStateChanged?.Invoke(); 
//     }
//
//     public void Update(float dt) => SpriteRenderer.Update(dt);
// }

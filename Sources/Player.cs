using BTLT04.Components;
using Transform = BTLT04.Components.Transform; 
internal class Player
{
    public StateMachine StateMachine { get; }
    public Transform Transform { get; } = new Transform(); // MỚI
    private readonly HashSet<Keys> _pressedKeys = new();
    private const float Speed = 180f; // pixel/giây

    private Rectangle _playArea;

    public Player(Rectangle playArea)
    {
        _playArea = playArea;
        StateMachine = new StateMachine(Transform); // Truyền Transform vào

        StateMachine.AddState("Idle", @"Sources\Player\Idle.png", 6);
        StateMachine.AddState("Walk", @"Sources\Player\Walk.png", 7);
        StateMachine.AddState("Attack2", @"Sources\Player\Attack_2.png", 4);
        StateMachine.ChangeState("Idle");
    }

    public void Update(double deltaTime)
    {
        float dt = (float)deltaTime;
        HandleInput(dt);
        UpdateState();
        Transform.Update(dt); // Tự động di chuyển theo velocity
        ClampToBounds();
        StateMachine.Update(dt);
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
        StateMachine.ChangeState(Transform.Velocity.IsEmpty ? "Idle" : "Walk");
    }

    // Input
    public void OnKeyDown(Keys key) => _pressedKeys.Add(key);
    public void OnKeyUp(Keys key) => _pressedKeys.Remove(key);

    public void Draw(Graphics g) => StateMachine.SpriteRenderer.Draw(g);
    public void UpdatePlayArea(Rectangle area) => _playArea = area;
}

// === StateMachine cập nhật ===
internal class StateMachine
{
    public SpriteRenderer SpriteRenderer { get; private set; }
    private readonly Transform _transform; // Giữ reference
    private readonly Dictionary<string, (Bitmap sheet, int frameCount)> _states = new();
    private string _currentState = "";

    public StateMachine(Transform transform)
    {
        _transform = transform;
        SpriteRenderer = new SpriteRenderer(new Bitmap(1, 1), 1, transform);
    }

    public void AddState(string name, string path, int frameCount)
    {
        if (_states.ContainsKey(name)) return;
        _states[name] = (Content.Load(path), frameCount);
    }

    public void ChangeState(string name)
    {
        if (_currentState == name || !_states.ContainsKey(name)) return;
        var (sheet, count) = _states[name];
        SpriteRenderer = new SpriteRenderer(sheet, count, _transform);
        _currentState = name;
    }

    public void Update(float dt) => SpriteRenderer.Update(dt);
}

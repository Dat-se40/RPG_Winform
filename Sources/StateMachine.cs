using BTLT04.Components;

namespace BTLT04.Sources;

internal class StateMachine
{
    public SpriteRenderer SpriteRenderer { get; private set; }
    private readonly Transform _transform; // Giữ reference
    private readonly Dictionary<string, (Bitmap sheet, int frameCount, float speed)> _states = new();
    public string _currentState = "";
    public string _currentType = "";
    
    public event Action OnStateChanged;

    public StateMachine(Transform transform)
    {
        _transform = transform;
        SpriteRenderer = new SpriteRenderer(new Bitmap(1, 1), 1, transform);
    }

    public void AddState(string name, string path, int frameCount, float speed = 1f)
    {
        if (_states.ContainsKey(name)) return;
        _states[name] = (Content.Load(path), frameCount, speed);
    }

    public void ChangeState(string name)
    {
        if (_currentState == name || !_states.ContainsKey(name)) return;
        var (sheet, count, speed) = _states[name];
        SpriteRenderer = new SpriteRenderer(sheet, count, _transform, speed);
        _currentState = name;
        
        //
        OnStateChanged?.Invoke(); 
    }

    public void Update(float dt) => SpriteRenderer.Update(dt);
}
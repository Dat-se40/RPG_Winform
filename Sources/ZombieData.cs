namespace BTLT04.Sources;

internal enum ZombieType
{
    Normal,
    Fast
}

internal class ZombieData
{
    public ZombieType Type { get; set; }
    public string Name { get; set; }
    public int MaxHealth { get; set; }
    public float Speed { get; set; }
    public int Damage { get; set; }
    public float SpawnWeight { get; set; } // Tỷ lệ spawn (0-1)
    public float AnimationFPS { get; set; } = 12f; // FPS của animation
    
    public string AppearSpritePath { get; set; }
    public int AppearFrameCount { get; set; }
    
    public string IdleSpritePath { get; set; }
    public int IdleFrameCount { get; set; }
    
    public string WalkSpritePath { get; set; }
    public int WalkFrameCount { get; set; }
    
    public string AttackSpritePath { get; set; }
    public int AttackFrameCount { get; set; }
    
    public string DeadSpritePath { get; set; }
    public int DeadFrameCount { get; set; }
    
    // Preset zombie types 
    public static ZombieData NormalZombie => new ZombieData
    {
        Type = ZombieType.Normal,
        Name = "Normal",
        MaxHealth = 100,
        Speed = 30f,
        Damage = 10,
        SpawnWeight = 0.6f,
        AnimationFPS = 10f, // Animation bình thường
        AppearSpritePath = @"Sources\Enemy\newAppear.png",
        AppearFrameCount = 11,
        IdleSpritePath = @"Sources\Enemy\newIdle.png",
        IdleFrameCount = 6,
        WalkSpritePath = @"Sources\Enemy\newWalk.png", 
        WalkFrameCount = 10,
        AttackSpritePath = @"Sources\Enemy\newAttack.png",
        AttackFrameCount = 7,
        DeadSpritePath = @"Sources\Enemy\newDie.png",
        DeadFrameCount = 8
    };
    
    public static ZombieData FastZombie => new ZombieData
    {
        Type = ZombieType.Fast,
        Name = "Fast",
        MaxHealth = 60,
        Speed = 60f, // Di chuyển nhanh
        Damage = 5,
        SpawnWeight = 0.25f,
        AnimationFPS = 18f, // Animation NHANH HƠN (tăng FPS)
        IdleSpritePath = @"Sources\Enemy\miniIdle.png",
        IdleFrameCount = 10,
        WalkSpritePath = @"Sources\Enemy\miniWalk.png", 
        WalkFrameCount = 10,
        AttackSpritePath = @"Sources\Enemy\miniAttack.png",
        AttackFrameCount = 8,
        DeadSpritePath = @"Sources\Enemy\miniDead.png",
        DeadFrameCount = 8
    };
}
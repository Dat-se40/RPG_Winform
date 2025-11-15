namespace BTLT04.Sources;

internal class ZombieSpawner
{
    private readonly List<Zombie> _zombies = new List<Zombie>();
    private readonly List<Wave> _waves = new List<Wave>();
    private readonly Random _random = new Random();

    // Lane configuration
    private const int TotalLanes = 5;
    private float _laneHeight;
    private float _firstLaneY;
    private float _spawnX;

    // Wave system
    public int _currentWaveIndex = 0;
    private Wave _currentWave;
    public  int _zombiesSpawned = 0;
    private bool _isWaveActive = false;

    // Spawn timing
    private float _spawnTimer = 0f;
    private float _waveTimer = 0f;

    // Lane tracking
    private readonly int[] _zombiesInLane = new int[TotalLanes];
    private const int MaxZombiesPerLane = 8;

    public IReadOnlyList<Zombie> Zombies => _zombies;
    public int CurrentWave => _currentWaveIndex + 1;
    public bool IsWaveActive => _isWaveActive;
    public int TotalWaves => _waves.Count;
    public bool IsGameComplete => _currentWaveIndex >= _waves.Count && _zombies.Count == 0;

    public ZombieSpawner(Rectangle playArea)
    {
        UpdatePlayArea(playArea);
        InitializeWaves();
    }

    /// <summary>
    /// Cấu hình các wave theo phong cách PvZ
    /// </summary>
    private void InitializeWaves()
    {
        // Wave 1: 
        _waves.Add(new Wave(1)
        {
            SpawnInterval = 3f,
            WaveCooldown = 10f
        }
        .AddZombieType(ZombieData.NormalZombie, 10));

        // Wave 2: 
        _waves.Add(new Wave(2)
        {
            SpawnInterval = 2.5f,
            WaveCooldown = 12f
        }
        .AddZombieType(ZombieData.NormalZombie, 12)
        .AddZombieType(ZombieData.FastZombie, 3));

        // Wave 3: 
        _waves.Add(new Wave(3)
        {
            SpawnInterval = 2.2f,
            WaveCooldown = 15f
        }
        .AddZombieType(ZombieData.NormalZombie, 10)
        .AddZombieType(ZombieData.FastZombie, 8));

        // Wave 4: 
        _waves.Add(new Wave(4)
        {
            SpawnInterval = 2f,
            WaveCooldown = 15f
        }
        .AddZombieType(ZombieData.NormalZombie, 8)
        .AddZombieType(ZombieData.FastZombie, 12));

        // Wave 5: 
        _waves.Add(new Wave(5)
        {
            SpawnInterval = 1.5f,
            WaveCooldown = 20f
        }
        .AddZombieType(ZombieData.NormalZombie, 15)
        .AddZombieType(ZombieData.FastZombie, 15));

    }

    public void UpdatePlayArea(Rectangle playArea)
    {
        _laneHeight = playArea.Height / (float)TotalLanes;
        _firstLaneY = playArea.Y + (_laneHeight / 2f);
        _spawnX = playArea.Right + 50f;
    }

    public void Update(float deltaTime)
    {
        // Update zombies
        foreach (var zombie in _zombies.ToList())
        {
            zombie.Update(deltaTime);

            if (zombie.ShouldBeRemoved())
            {
                RemoveZombie(zombie);
            }
        }

        // Update wave system
        UpdateWaveSystem(deltaTime);
    }

    private void UpdateWaveSystem(float deltaTime)
    {
        // Kiểm tra xem đã hết wave chưa
        if (_currentWaveIndex >= _waves.Count)
        {
            if (_zombies.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("\n=== ALL WAVES COMPLETED! YOU WIN! ===\n");
            }
            return;
        }

        // Nếu wave chưa bắt đầu, đếm cooldown
        if (!_isWaveActive)
        {
            _waveTimer += deltaTime;

            if (_waveTimer >= (_currentWave?.WaveCooldown ?? 5f))
            {
                StartNewWave();
            }
            return;
        }

        // Nếu đã spawn đủ zombie trong wave
        if (_zombiesSpawned >= _currentWave.TotalZombies)
        {
            // Chờ tất cả zombie chết mới kết thúc wave
            if (_zombies.Count == 0)
            {
                EndWave();
            }
            return;
        }

        // Spawn zombie theo interval
        _spawnTimer += deltaTime;

        if (_spawnTimer >= _currentWave.SpawnInterval)
        {
            SpawnZombieInRandomLane();
            _spawnTimer = 0f;
            _zombiesSpawned++;
        }
    }

    private void StartNewWave()
    {
        _currentWave = _waves[_currentWaveIndex];
        _zombiesSpawned = 0;
        _isWaveActive = true;
        _waveTimer = 0f;
        _spawnTimer = 0f;

        System.Diagnostics.Debug.WriteLine(
            $"\n=== WAVE {_currentWave.WaveNumber} START - {_currentWave.TotalZombies} ZOMBIES ===");

        // In thông tin chi tiết về wave
        foreach (var spawn in _currentWave.Spawns)
        {
            System.Diagnostics.Debug.WriteLine(
                $"  - {spawn.Count}x {spawn.ZombieData.Name} Zombie");
        }
        System.Diagnostics.Debug.WriteLine("");
    }

    private void EndWave()
    {
        System.Diagnostics.Debug.WriteLine(
            $"\n>>> Wave {_currentWave.WaveNumber} Complete!");

        _currentWaveIndex++;

        if (_currentWaveIndex < _waves.Count)
        {
            var nextWave = _waves[_currentWaveIndex];
            System.Diagnostics.Debug.WriteLine(
                $">>> Next wave in {nextWave.WaveCooldown}s\n");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine(">>> That was the final wave!\n");
        }

        _isWaveActive = false;
        _waveTimer = 0f;
    }

    public void SpawnZombieInRandomLane()
    {
        var availableLanes = new List<int>();
        for (int i = 0; i < TotalLanes; i++)
        {
            if (_zombiesInLane[i] < MaxZombiesPerLane)
            {
                availableLanes.Add(i);
            }
        }

        if (availableLanes.Count == 0) return;

        int laneIndex = availableLanes[_random.Next(availableLanes.Count)];
        SpawnZombieInLane(laneIndex);
    }

    public void SpawnZombieInLane(int laneIndex)
    {
        if (laneIndex < 0 || laneIndex >= TotalLanes) return;
        if (_zombiesInLane[laneIndex] >= MaxZombiesPerLane) return;
        if (_currentWave == null) return;

        var zombieData = _currentWave.GetRandomZombieType(_random);
        float laneY = _firstLaneY + (laneIndex * _laneHeight);
        PointF spawnPos = new PointF(_spawnX, laneY);

        var zombie = new Zombie(zombieData, spawnPos, laneIndex);
        _zombies.Add(zombie);
        _zombiesInLane[laneIndex]++;

        System.Diagnostics.Debug.WriteLine(
            $"[Wave {_currentWave.WaveNumber}] Spawned {zombieData.Name} Zombie in Lane {laneIndex + 1} ({_zombiesSpawned + 1}/{_currentWave.TotalZombies})");
    }

    /// <summary>
    /// Spawn một nhóm zombie cùng lúc (có thể dùng cho special events)
    /// </summary>
    public void SpawnZombieGroup(int laneIndex, int count, ZombieData zombieData)
    {
        for (int i = 0; i < count; i++)
        {
            SpawnZombieInLaneWithOffset(laneIndex, i * 80f, zombieData);
        }
    }

    private void SpawnZombieInLaneWithOffset(int laneIndex, float offsetX, ZombieData zombieData)
    {
        if (laneIndex < 0 || laneIndex >= TotalLanes) return;

        float laneY = _firstLaneY + (laneIndex * _laneHeight);
        PointF spawnPos = new PointF(_spawnX + offsetX, laneY);

        var zombie = new Zombie(zombieData, spawnPos, laneIndex);
        _zombies.Add(zombie);
        _zombiesInLane[laneIndex]++;
    }

    /// <summary>
    /// Thêm wave tùy chỉnh vào cuối danh sách
    /// </summary>
    public void AddCustomWave(Wave wave)
    {
        _waves.Add(wave);
    }

    private void RemoveZombie(Zombie zombie)
    {
        _zombiesInLane[zombie.LaneIndex]--;
        _zombies.Remove(zombie);
    }

    public void Draw(Graphics g)
    {
        foreach (var zombie in _zombies)
        {
            zombie.Draw(g);
        }
    }

    public void Clear()
    {
        _zombies.Clear();
        Array.Clear(_zombiesInLane, 0, _zombiesInLane.Length);
    }

    /// <summary>
    /// Reset về wave đầu tiên
    /// </summary>
    public void Reset()
    {
        Clear();
        _currentWaveIndex = 0;
        _zombiesSpawned = 0;
        _isWaveActive = false;
        _spawnTimer = 0f;
        _waveTimer = 0f;
    }

    /// <summary>
    /// Lấy thông tin wave hiện tại
    /// </summary>
    public string GetWaveInfo()
    {
        if (_currentWave == null)
            return "Waiting for wave to start...";

        return $"Wave {_currentWave.WaveNumber}/{_waves.Count} - " +
               $"{_zombiesSpawned}/{_currentWave.TotalZombies} spawned - " +
               $"{_zombies.Count} alive";
    }
}
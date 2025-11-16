
using BTLT04.Components;

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
    public int _zombiesSpawned = 0;
    private bool _isWaveActive = false;

    // Spawn timing
    private float _spawnTimer = 0f;
    private float _waveTimer = 0f;

    // Lane tracking
    private readonly int[] _zombiesInLane = new int[TotalLanes];
    private const int MaxZombiesPerLane = 6; // ⭐ Giảm từ 8 → 6 để tránh nghẽn

    // ⭐ Thêm retry mechanism
    private int _spawnRetries = 0;
    private const int MaxSpawnRetries = 10;

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
        // Wave 1: Tutorial
        _waves.Add(new Wave(1)
        {
            SpawnInterval = 3f,
            WaveCooldown = 10f
        }
        .AddZombieType(ZombieData.NormalZombie, 10));

        // Wave 2: Thêm fast zombie
        _waves.Add(new Wave(2)
        {
            SpawnInterval = 2.5f,
            WaveCooldown = 10f
        }
        .AddZombieType(ZombieData.NormalZombie, 12)
        .AddZombieType(ZombieData.FastZombie, 3));

        // Wave 3: Tăng số lượng
        _waves.Add(new Wave(3)
        {
            SpawnInterval = 2.2f,
            WaveCooldown = 10f
        }
        .AddZombieType(ZombieData.NormalZombie, 10)
        .AddZombieType(ZombieData.FastZombie, 8));

        // Wave 4: ⭐ Fix spawn interval
        _waves.Add(new Wave(4)
        {
            SpawnInterval = 2.5f, // Tăng từ 2f → 2.5f
            WaveCooldown = 10f
        }
        .AddZombieType(ZombieData.NormalZombie, 8)
        .AddZombieType(ZombieData.FastZombie, 12));

        // Wave 5: Final wave
        _waves.Add(new Wave(5)
        {
            SpawnInterval = 2f, // Tăng từ 1.5f → 2f
            WaveCooldown = 10f
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
                System.Diagnostics.Debug.WriteLine("\n🎉 === ALL WAVES COMPLETED! YOU WIN! === 🎉\n");
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

        // ⭐ FIX: Kiểm tra wave hoàn thành
        if (_zombiesSpawned >= _currentWave.TotalZombies)
        {
            // Debug log
            if (_zombies.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"⏳ Wave {_currentWave.WaveNumber}: Waiting for {_zombies.Count} zombies to die...");
            }

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
            // ⭐ FIX: Dùng bool để biết spawn thành công hay không
            bool spawned = TrySpawnZombie();

            if (spawned)
            {
                _zombiesSpawned++;
                _spawnRetries = 0; // Reset retry counter
                _spawnTimer = 0f;

                System.Diagnostics.Debug.WriteLine(
                    $"✅ [Wave {_currentWave.WaveNumber}] Spawned zombie {_zombiesSpawned}/{_currentWave.TotalZombies} | Alive: {_zombies.Count}");
            }
            else
            {
                // ⭐ Retry sau 0.5s thay vì bỏ qua
                _spawnRetries++;
                _spawnTimer = _currentWave.SpawnInterval - 0.5f; // Thử lại sau 0.5s

                if (_spawnRetries >= MaxSpawnRetries)
                {
                    // Nếu retry quá nhiều, force spawn vào lane ít zombie nhất
                    ForceSpawnZombie();
                    _zombiesSpawned++;
                    _spawnRetries = 0;
                    _spawnTimer = 0f;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"⚠️ All lanes full! Retry {_spawnRetries}/{MaxSpawnRetries}...");
                }
            }
        }
    }

    private void StartNewWave()
    {
        _currentWave = _waves[_currentWaveIndex];
        _zombiesSpawned = 0;
        _isWaveActive = true;
        _waveTimer = 0f;
        _spawnTimer = 0f;
        _spawnRetries = 0; // ⭐ Reset retry counter

        System.Diagnostics.Debug.WriteLine(
            $"\n🧟 === WAVE {_currentWave.WaveNumber} START - {_currentWave.TotalZombies} ZOMBIES === 🧟");

        try
        {
            SoundManager.Instance.PlayEffectDirect(Form1.AbsPath(@"Sources\Sound\zombie.ogg"));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Sound error: {ex.Message}");
        }

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
            $"\n✅ >>> Wave {_currentWave.WaveNumber} Complete!");

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

    /// <summary>
    /// ⭐ Thử spawn zombie, trả về true nếu thành công
    /// </summary>
    private bool TrySpawnZombie()
    {
        var availableLanes = new List<int>();
        for (int i = 0; i < TotalLanes; i++)
        {
            if (_zombiesInLane[i] < MaxZombiesPerLane)
            {
                availableLanes.Add(i);
            }
        }

        if (availableLanes.Count == 0)
        {
            return false; // Không có lane khả dụng
        }

        int laneIndex = availableLanes[_random.Next(availableLanes.Count)];
        return SpawnZombieInLane(laneIndex);
    }

    /// <summary>
    /// ⭐ Force spawn vào lane ít zombie nhất
    /// </summary>
    private void ForceSpawnZombie()
    {
        // Tìm lane có ít zombie nhất
        int minLane = 0;
        int minCount = _zombiesInLane[0];

        for (int i = 1; i < TotalLanes; i++)
        {
            if (_zombiesInLane[i] < minCount)
            {
                minCount = _zombiesInLane[i];
                minLane = i;
            }
        }

        System.Diagnostics.Debug.WriteLine(
            $"🔴 Force spawning in lane {minLane + 1} (has {minCount} zombies)");

        SpawnZombieInLane(minLane);
    }

    /// <summary>
    /// ⭐ Spawn zombie trong lane, trả về bool
    /// </summary>
    public bool SpawnZombieInLane(int laneIndex)
    {
        if (laneIndex < 0 || laneIndex >= TotalLanes) return false;
        if (_currentWave == null) return false;

        var zombieData = _currentWave.GetRandomZombieType(_random);
        float laneY = _firstLaneY + (laneIndex * _laneHeight);
        PointF spawnPos = new PointF(_spawnX, laneY);

        var zombie = new Zombie(zombieData, spawnPos, laneIndex);
        _zombies.Add(zombie);
        _zombiesInLane[laneIndex]++;

        return true;
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

    /// <summary>
    /// ⭐ Xóa zombie và cập nhật lane counter
    /// </summary>
    public void RemoveZombie(Zombie zombie)
    {
        if (_zombies.Remove(zombie))
        {
            // Giảm counter của lane
            int lane = zombie.LaneIndex;
            if (lane >= 0 && lane < TotalLanes)
            {
                _zombiesInLane[lane]--;
                if (_zombiesInLane[lane] < 0) _zombiesInLane[lane] = 0;
            }
        }
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
        _spawnRetries = 0; // ⭐ Reset retry counter

        System.Diagnostics.Debug.WriteLine("🔄 ZombieSpawner Reset!");
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
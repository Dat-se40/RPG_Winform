namespace BTLT04.Sources;

internal class ZombieSpawner
    {
        private readonly List<Zombie> _zombies = new List<Zombie>();
        private readonly List<ZombieData> _zombieTypes = new List<ZombieData>();
        private readonly Random _random = new Random();
        
        // Lane configuration
        private const int TotalLanes = 5;
        private float _laneHeight;
        private float _firstLaneY;
        private float _spawnX;
        
        // Wave system
        private int _currentWave = 1;
        private int _zombiesPerWave = 10;
        private int _zombiesSpawned = 0;
        private bool _isWaveActive = true;
        
        // Spawn timing
        private float _spawnTimer = 0f;
        private float _spawnInterval = 3f;
        private float _waveCooldown = 10f;
        private float _waveTimer = 0f;
        
        // Lane tracking
        private readonly int[] _zombiesInLane = new int[TotalLanes];
        private const int MaxZombiesPerLane = 8;
        
        public IReadOnlyList<Zombie> Zombies => _zombies;
        public int CurrentWave => _currentWave;
        public bool IsWaveActive => _isWaveActive;
        
        public ZombieSpawner(Rectangle playArea)
        {
            UpdatePlayArea(playArea);
            
            // Load zombie types
            _zombieTypes.Add(ZombieData.NormalZombie);
            _zombieTypes.Add(ZombieData.FastZombie);
        }
        
        public void UpdatePlayArea(Rectangle playArea)
        {
            _laneHeight = playArea.Height / (float)TotalLanes;
            _firstLaneY = playArea.Y + (_laneHeight / 2f); // Center trong lane
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
            if (!_isWaveActive)
            {
                _waveTimer += deltaTime;
                
                if (_waveTimer >= _waveCooldown)
                {
                    StartNewWave();
                }
                return;
            }
            
            if (_zombiesSpawned >= _zombiesPerWave)
            {
                if (_zombies.Count == 0)
                {
                    EndWave();
                }
                return;
            }
            
            _spawnTimer += deltaTime;
            
            if (_spawnTimer >= _spawnInterval)
            {
                SpawnZombieInRandomLane();
                _spawnTimer = 0f;
                _zombiesSpawned++;
            }
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
            
            var zombieData = GetRandomZombieType();
            float laneY = _firstLaneY + (laneIndex * _laneHeight);
            PointF spawnPos = new PointF(_spawnX, laneY);
            
            var zombie = new Zombie(zombieData, spawnPos, laneIndex);
            _zombies.Add(zombie);
            _zombiesInLane[laneIndex]++;
            
            System.Diagnostics.Debug.WriteLine(
                $"[Wave {_currentWave}] Spawned {zombieData.Name} Zombie in Lane {laneIndex + 1}");
        }
        
        public void SpawnZombieGroup(int laneIndex, int count)
        {
            for (int i = 0; i < count; i++)
            {
                SpawnZombieInLaneWithOffset(laneIndex, i * 80f);
            }
        }
        // Sẽ có một vài zombie sinh ra bên ngoài màn hình  
        private void SpawnZombieInLaneWithOffset(int laneIndex, float offsetX)
        {
            if (laneIndex < 0 || laneIndex >= TotalLanes) return;
            
            var zombieData = GetRandomZombieType();
            float laneY = _firstLaneY + (laneIndex * _laneHeight);
            PointF spawnPos = new PointF(_spawnX + offsetX, laneY);
            
            var zombie = new Zombie(zombieData, spawnPos, laneIndex);
            _zombies.Add(zombie);
            _zombiesInLane[laneIndex]++;
        }
        
        // public void SpawnHugeWave()
        // {
        //     System.Diagnostics.Debug.WriteLine("=== HUGE WAVE OF ZOMBIES! ===");
        //     
        //     for (int lane = 0; lane < TotalLanes; lane++)
        //     {
        //         int zombieCount = _random.Next(3, 6);
        //         SpawnZombieGroup(lane, zombieCount);
        //     }
        // }
        
        private ZombieData GetRandomZombieType()
        {
            float totalWeight = 0f;
            foreach (var type in _zombieTypes)
            {
                totalWeight += type.SpawnWeight;
            }
            
            float randomValue = (float)_random.NextDouble() * totalWeight;
            float cumulative = 0f;
            
            foreach (var type in _zombieTypes)
            {
                cumulative += type.SpawnWeight;
                if (randomValue <= cumulative)
                {
                    return type;
                }
            }
            
            return _zombieTypes[0];
        }
        
        private void RemoveZombie(Zombie zombie)
        {
            _zombiesInLane[zombie.LaneIndex]--;
            _zombies.Remove(zombie);
        }
        
        private void StartNewWave()
        {
            _currentWave++;
            _zombiesSpawned = 0;
            _zombiesPerWave += 5;
            _spawnInterval = Math.Max(1f, _spawnInterval - 0.2f);
            _isWaveActive = true;
            _waveTimer = 0f;
            
            System.Diagnostics.Debug.WriteLine(
                $"\n=== WAVE {_currentWave} START - {_zombiesPerWave} ZOMBIES ===\n");
        }
        
        private void EndWave()
        {
            _isWaveActive = false;
            _waveTimer = 0f;
            
            System.Diagnostics.Debug.WriteLine(
                $"\n>>> Wave {_currentWave} Complete! Next wave in {_waveCooldown}s\n");
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
        
        // public int GetZombieCount() => _zombies.Count;
        //
        // public int GetZombiesInLane(int laneIndex)
        // {
        //     return laneIndex >= 0 && laneIndex < TotalLanes 
        //         ? _zombiesInLane[laneIndex] 
        //         : 0;
        // }
    }
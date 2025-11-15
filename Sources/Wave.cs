using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTLT04.Sources
{
    internal class Wave
    {
        public int WaveNumber { get; set; }
        public List<WaveSpawn> Spawns { get; set; } = new List<WaveSpawn>();
        public float SpawnInterval { get; set; } = 3f; // Thời gian giữa mỗi lần spawn
        public float WaveCooldown { get; set; } = 10f; // Thời gian nghỉ giữa các wave

        /// <summary>
        /// Tổng số zombie trong wave này
        /// </summary>
        public int TotalZombies => Spawns.Sum(s => s.Count);

        public Wave(int waveNumber)
        {
            WaveNumber = waveNumber;
        }

        /// <summary>
        /// Thêm một loại zombie vào wave
        /// </summary>
        public Wave AddZombieType(ZombieData zombieData, int count)
        {
            Spawns.Add(new WaveSpawn
            {
                ZombieData = zombieData,
                Count = count
            });
            return this;
        }

        /// <summary>
        /// Lấy zombie type ngẫu nhiên dựa trên cấu hình wave
        /// </summary>
        public ZombieData GetRandomZombieType(Random random)
        {
            // Tính tổng weight
            float totalWeight = 0f;
            foreach (var spawn in Spawns)
            {
                totalWeight += spawn.ZombieData.SpawnWeight * spawn.Count;
            }

            // Random theo weight
            float randomValue = (float)random.NextDouble() * totalWeight;
            float cumulative = 0f;

            foreach (var spawn in Spawns)
            {
                cumulative += spawn.ZombieData.SpawnWeight * spawn.Count;
                if (randomValue <= cumulative)
                {
                    return spawn.ZombieData;
                }
            }

            return Spawns[0].ZombieData;
        }
    }

    /// <summary>
    /// Cấu hình spawn cho một loại zombie trong wave
    /// </summary>
    internal class WaveSpawn
    {
        public ZombieData ZombieData { get; set; }
        public int Count { get; set; }
    }

}

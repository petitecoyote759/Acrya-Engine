using ILGPU.Util;
using ShortTools.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Acrya.ECSComponents
{
    /// <summary>
    /// Component for an entity that constantly spawns entities. Creates a new entity using a given function every given duration. 
    /// Spawned enemies must have the <see cref="EC_SpawnedLogic"/> component.
    /// </summary>
    internal struct EC_SpawnerLogic : IEntityComponent
    {
        public bool Active { readonly get => active; set => active = value; }
        private bool active = true;

        public readonly int Max => max;
        private readonly int max = 10000;
        public readonly int CurrentSpawned => spawnedUids.Count;

        public HashSet<int> spawnedUids = new HashSet<int>();

        public float MaxSpawnsPerSecond 
        { 
            readonly get => maxSpawnsPerSecond; 
            set 
            { 
                maxSpawnsPerSecond = value;
                secondsPerSpawn = 1f / value;
            } 
        }
        private float maxSpawnsPerSecond = 10f; // should only be set via the property

        private readonly Func<int> creatorFunc;
        private float spawnTimer = 0;
        private float secondsPerSpawn = 0;

        public EC_SpawnerLogic(Func<int> creatorFunc, float maxSpawnsPerSecond, int maxEntities) 
        { 
            this.creatorFunc = creatorFunc;
            this.MaxSpawnsPerSecond = maxSpawnsPerSecond;
            this.max = maxEntities;
        }

        public void Action(float dt, int uid)
        {
            spawnTimer += dt;
            if (CurrentSpawned >= max) { return; }
            while (spawnTimer > secondsPerSpawn)
            {
                spawnTimer -= secondsPerSpawn;
                int spawnedUid = creatorFunc();
                _ = spawnedUids.Add(spawnedUid);
            }
        }

        public readonly void Cleanup(int uid)
        {
            // Spawned entities references are automatically closed on cleanup via EC_Entity

            spawnedUids.Clear();
        }
    }
}

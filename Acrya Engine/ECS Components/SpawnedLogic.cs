using ShortTools.General;
using System.Runtime.CompilerServices;
using Acrya.ECSHandlers;



namespace Acrya.ECSComponents
{
    /// <summary>
    /// Component that is used by an entity spawned via an entity that uses the <see cref="EC_SpawnedLogic"/> component.
    /// </summary>
    internal struct EC_SpawnedLogic : IEntityComponent
    {
        // <<Public Variables>> //
        public bool Active { readonly get => active; set => active = value; }
        private bool active = false;

        public EntityReference spawnerReference;


        public EC_SpawnedLogic(int uid, int spawnerUid) 
        { 
            this.spawnerReference = new EntityReference(spawnerUid, uid); 
            if (spawnerUid == -1) { ECSHandler.debugger.AddLog($"Given spawner UID was -1, something went wrong", WarningLevel.Error); return; }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public readonly void Action(float dt, int uid) { }


        public readonly void Cleanup(int uid)
        {
            if (spawnerReference.Closed) 
            { ECSHandler.debugger.AddLog($"Attempted to access closed spawner.", WarningLevel.Debug); return; } // spawner is closed

            ECSHandler.GetEntityComponent(spawnerReference.Target, out EC_SpawnerLogic spawnerLogic);

            spawnerLogic.spawnedUids.Remove(uid);

            ECSHandler.SetEntitiyComponent(spawnerReference.Target, spawnerLogic);
        }
    }
}

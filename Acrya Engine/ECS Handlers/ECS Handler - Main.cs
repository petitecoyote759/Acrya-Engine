using Acrya.ECSComponents;
using Acrya.Renderer;
using ShortTools.General;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;


#pragma warning disable CA1708 // Names of members should vary more than by case, this isnt the case for fields relating to the variables.



namespace Acrya.ECSHandlers
{
    public static partial class ECSHandler
    {
        // <<Public Variables>> //
        internal static List<bool> entities = new List<bool>();
        internal static Dictionary<Type, List<IEntityComponent?>> ECSs = CreateECSs();


        // <<<Entity Grid Variables>>> //

        /// <summary>
        /// The size of each entity grid in tiles.
        /// </summary>
        public static int EntityGridSize => entityGridSize;
        /// <inheritdoc cref="EntityGridSize"/>
        internal static int entityGridSize = 32;

        public static int EntityGridWidth => entityGridWidth;
        internal static int entityGridWidth;

        public static int EntityGridHeight => entityGridHeight;
        internal static int entityGridHeight;




        /// <summary>
        /// Last frame time of the ECS system in ms, obtained via <see cref="DateTimeOffset.Now"/>, and then <see cref="DateTimeOffset.ToUnixTimeMilliseconds"/>
        /// </summary>
        public static long CurrentTime => LFT;
        private static long LFT = DateTimeOffset.Now.ToUnixTimeMilliseconds();// last frame time
        private const int MaxFPS = 60;
        internal static int currentFPS = 0;
        private const long MaxMsPerFrame = 1000 / MaxFPS;
        private const int secondsPerFPSUpdate = 2;
        private const long ticksPerFPSUpdate = secondsPerFPSUpdate * 1000;
        private static int frameCount = 0;
        private static long FPSUpateTimer = 0;




        internal static Thread controllerThread = new Thread(new ThreadStart(RunLoop));

        internal static HashSet<(int, int)> updatedGrids = new HashSet<(int, int)>();

        internal static bool running = true;

        internal static Debugger debugger = new Debugger("ECS",
#if DEBUG
                WarningLevel.Debug,
#else
                WarningLevel.Info,
#endif
                DebuggerFlag.PrintLogs, DebuggerFlag.WriteLogsToFile, DebuggerFlag.DisplayThread);




        // <<Entity Management Functions>> //
        private static Dictionary<Type, List<IEntityComponent?>> CreateECSs()
        {
            return new Dictionary<Type, List<IEntityComponent?>>()
            {
              { typeof(EC_SpawnerLogic), new List<IEntityComponent?>() },
              { typeof(EC_SpawnedLogic), new List<IEntityComponent?>() },
              { typeof(EC_Entity), new List<IEntityComponent?>() },
              { typeof(EC_PathFinding), new List<IEntityComponent?>() },
              { typeof(EC_Render), new List<IEntityComponent?>() },
            };
        }
        


        // <<Init Functions>> //

        public static void Setup()
        {
            if (AcryaEngine.map is null) { debugger.AddLog($"Attempted to setup the ECS when the map was null, cancelling setup...", WarningLevel.Error); return; }

            entityGridWidth = AcryaEngine.map.Width / entityGridSize;
            entityGridHeight = AcryaEngine.map.Height / entityGridSize;

            for (int uid = 0; uid < entities.Count; uid++)
            {
                if (entities[uid] == false) { continue; } // already disposed

                FreeUID(uid);
            }

            entities = new List<bool>();

            if (controllerThread.ThreadState == ThreadState.Running) 
            { 
                debugger.AddLog($"ECS Thread running already, joining..."); 
                running = false; 
                controllerThread.Join(); 
            }

            controllerThread = new Thread(new ThreadStart(RunLoop));
            controllerThread.Name = "ECS Controller Thread";
        }
        public static bool Start()
        {
            if (controllerThread.ThreadState == ThreadState.Running || controllerThread.ThreadState == ThreadState.Stopped) { return false; }


            controllerThread.Start();


            return true;
        }












        // <<Main Functions>> //
        
        private static void RunLoop()
        {
            while (running)
            {
                long now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                long delta = now - LFT;
                int makeupTime = (int)(MaxMsPerFrame - delta); // the amount of time 
                if (makeupTime > 0)
                {
                    Thread.Sleep(makeupTime);
                }
                float dt = delta / 1000f;
                LFT = now;

                frameCount++;
                FPSUpateTimer += delta;
                if (FPSUpateTimer > ticksPerFPSUpdate)
                {
                    FPSUpateTimer -= ticksPerFPSUpdate;
                    currentFPS = frameCount / secondsPerFPSUpdate;
                    debugger.AddLog($"ECS Frame Count {frameCount} over {secondsPerFPSUpdate} giving {currentFPS} FPS", WarningLevel.Debug);
                    frameCount = 0;
                }

                Run(dt);
            }
            debugger.AddLog($"Shutting down ECS", WarningLevel.Info);
            debugger.Dispose(true);
        }
        
        
        
        
        // Called by renderer thread for synchronisation
        public static void DoEntityRenderTasks(float dt)
        {
            int length = entities.Count;

            for (int uid = 0; uid < length; uid++)
            {
                if (entities[uid] == false) { continue; } // entity is closed

                if (ECSs[renderType][uid] is null) { continue; } // Entity does not have the component.
                if (ECSs[renderType][uid]?.Active == false) { continue; } // Module is disabled
                ECSs[renderType][uid]?.Action(dt, uid);
            }

            lock (updatedGrids)
            {
                foreach ((int, int) coordinate in updatedGrids)
                {
                    RendererTools.RequestDrawGrid(coordinate.Item1, coordinate.Item2);
                }
                updatedGrids.Clear();
            }
        }

        public static void Run(float dt)
        {
            updatedGrids = new HashSet<(int, int)>();

            int length = entities.Count;

            for (int i = 0; i < length; i++)
            {
                if (entities[i] == false) { continue; } // entity is closed

                RunEntitiy(i, dt);
            }
        }


        private static readonly Type renderType = typeof(EC_Render);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void RunEntitiy(int uid, float dt)
        {
            foreach (KeyValuePair<Type, List<IEntityComponent?>> pair in ECSs)
            {
                if (pair.Value[uid] is null) { continue; } // Entity does not have the component.
                if (pair.Value[uid]?.Active == false) { continue; } // Module is disabled
                if (pair.Key == renderType) { continue; } // Render is done on a seperate thread
                pair.Value[uid]?.Action(dt, uid);
            }
        }




        
    }

}

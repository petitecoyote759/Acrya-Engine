using ShortTools.AStar;
using ShortTools.General;
using System.Numerics;
using CacheCell = System.Collections.Generic.List<System.Collections.Generic.Queue<System.Numerics.Vector2>>;
using Path = System.Collections.Generic.Queue<System.Numerics.Vector2>; // Queue<Vector2>
using Acrya.ECSHandlers;


namespace Acrya.ECSComponents
{
    /// <summary>
    /// Pathfinding module, when there is no path it generates a path from the current position to 
    /// the coordinates at (<see cref="targetX"/>, <see cref="targetY"/>). <br/><br/>
    /// <b><u>REQURES:</u></b><br/>
    /// <see cref="EC_Entity"/>
    /// </summary>
    internal struct EC_PathFinding : IEntityComponent
    {
        // <<Requires>> //
        // EC_Entity //

        // <<Public Variables>> //
        private readonly static List<PathFinder> pathers = new List<PathFinder>();
        private static PathFinder? intraGridPather;
        /// <summary>
        /// Path of the module stored as a <see cref="Queue{T}"/> of <see cref="Vector2"/> coordinates.
        /// </summary>
        public Path? path;

        public bool Active { readonly get => active; set => active = value; }
        private bool active = true;

        public static bool UsesGPU => false;


        /// <summary>
        /// The target x coordinate.
        /// </summary>
        public int targetX = 0;
        /// <summary>
        /// The target y coordinate.
        /// </summary>
        public int targetY = 0;




        // <<Private Variables>> //

        // Cached Paths
        /// <summary>
        /// Path cache used to save paths to improve performance, if an entity is close to the start of a path it will move towards it.
        /// </summary>
        internal static CacheCell[][] cachedPaths = Array.Empty<CacheCell[]>();

        public readonly Func<int, int, bool> Walkable;
        private readonly int patherID = -1;

        // <<Constants>> //
        const float startPathDistanceRatio = 0.4f; // what portion of a boidGrid the leaders would be willing to walk to find a preset path
        const int pathCacheMax = 5;


        // <<Modified Constants> //
        static readonly int startPathDistance = (int)(startPathDistanceRatio * ECSHandler.entityGridSize);




        public EC_PathFinding(Func<int, int, bool> Walkable, int patherID = 0, 
            int patherRange = 1000, bool patherUseDiagonals = true, bool useCaches = false) : this(Walkable, useCaches)
        { 
            if (patherID > pathers.Count) { ECSHandler.debugger.AddLog($"Attempted to create pather with too large patherID : {patherID}", WarningLevel.Error); }
            this.patherID = patherID;
            CreatePather(patherID, patherRange, patherUseDiagonals);
        }
        public EC_PathFinding(Func<int, int, bool> Walkable, bool useCaches = false)
        {
            this.Walkable = Walkable;


            intraGridPather ??= new PathFinder(Walkable, maxDist: startPathDistance, useDiagonals: true);

            if (pathers.Count == 0)
            {
                CreatePather(0, 1000, true);
            }
            if (useCaches && cachedPaths.Length == 0)
            {
                // Create Cached paths variables.
                cachedPaths = new CacheCell[ECSHandler.entityGridWidth][];
                for (int x = 0; x < ECSHandler.entityGridWidth; x++)
                {
                    cachedPaths[x] = new CacheCell[ECSHandler.entityGridHeight];
                    for (int y = 0; y < ECSHandler.entityGridHeight; y++)
                    {
                        cachedPaths[x][y] = new CacheCell(pathCacheMax);
                    }
                }
            }

            path = null;
        }


        private readonly void CreatePather(int id, int maxDist, bool useDiagonals)
        {
            PathFinder pather = new PathFinder(Walkable, maxDist: maxDist, useDiagonals: useDiagonals);

            if (id == pathers.Count) { pathers.Add(pather); return; }
            pathers[id] = pather;
        }




        public void Action(float dt, int uid)
        {
            ECSHandler.GetEntityComponent(uid, out EC_Entity Me);

            int tileX = Me.intX;
            int tileY = Me.intY;

            int gridX = Me.gridX;
            int gridY = Me.gridY;


            // Pathfind, and set path if required
            // Simply path to centre
            if (path is null || path.Count == 0)
            {
                // <<Get Path From Cache>> //
                CacheCell currentCache = cachedPaths[gridX][gridY];

                for (int i = 0; i < currentCache.Count; i++)
                {
                    Path cachedPath = new Path(currentCache[i]); // makes a deep copy

                    Vector2 pathStart = cachedPath.Peek();
                    int pathStartX = (int)pathStart.X;
                    int pathStartY = (int)pathStart.Y;

                    Path? toStartPath = intraGridPather!.GetPath(tileX, tileY, pathStartX, pathStartY);

                    if (toStartPath is null) { continue; }
                    if (!PathIsValid(cachedPath)) { currentCache.RemoveAt(i); i--; continue; }

                    path = toStartPath;
                    int length = cachedPath.Count;
                    for (int j = 0; j < length; j++)
                    {
                        path.Enqueue(cachedPath.Dequeue());
                    }
                    break;
                }
                // <<Generate New Path>> //
                if (path is null || path.Count == 0)
                {
                    ECSHandler.debugger.AddLog($"Creating new path...", WarningLevel.Debug);
                    // create new path if none there
                    path = pathers[patherID].GetPath(tileX, tileY, targetX, targetY);
                    if (path is null || path.Count == 0) 
                    { 
                        ECSHandler.debugger.AddLog($"No path could be found from ({tileX},{tileY}) to ({targetX},{targetY}), self destructing...", WarningLevel.Debug); 
                        ECSHandler.FreeUID(uid); 
                        return; 
                    } // no path could be found, so it should not be there.
                    // path is not null
                    if (currentCache.Count < pathCacheMax)
                    {
                        cachedPaths[gridX][gridY].Add(new Path(path));
                    }
                }
            }
        }


        private readonly bool PathIsValid(Path path)
        {
            Path testPath = new Path(path);
            while (testPath.Count != 0)
            {
                Vector2 node = testPath.Dequeue();
                int x = (int)node.X;
                int y = (int)node.Y;
                if (0 > x || x >= AcryaEngine.map!.Width) { return false; }
                if (0 > y || y >= AcryaEngine.map!.Height) { return false; }
                if (Walkable(x, y) == false) { return false; }
            }
            return true;
        }


        public void Cleanup(int uid) { path = null; }
    }
}

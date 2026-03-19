




/*

How will the user use this package?
Create entities like this


internal static class Walker
{
    public const float walkSpeed = 2f;
    public const int deletionRadius = 10;

    private const int deletionRadiusSquared = deletionRadius * deletionRadius;

    public static int CreateWalker(Vector2 position, int spawnerReference = -1)
    {
        int uid = ECSHandler.GetUID();

        ECSHandler.entities[uid] = true;



        // <<Set Variables>> //
        EC_Entity Me = new EC_Entity();
        Me.position = position;


        // <<EAF Creation>> //
        ECSHandler.ECSs[typeof(EC_Entity)][uid] = Me;
        ECSHandler.ECSs[typeof(EC_Despawning)][uid] = new EC_Despawning(deletionRadius);
        ECSHandler.ECSs[typeof(EC_BoidLogic)][uid] = new EC_BoidLogic();
        ECSHandler.ECSs[typeof(EC_PathFinding)][uid] = new EC_PathFinding(Walkable);
        ECSHandler.ECSs[typeof(EC_Render)][uid] = new EC_Render(Path.Combine("Images", "Walker Temp.png"), 2, 2);
        ECSHandler.ECSs[typeof(EC_SpawnedLogic)][uid] = new EC_SpawnedLogic(uid, spawnerReference);

        // <<Disable the EAF modules used for leaders>> //
#pragma warning disable CS8602 // This should absolutely not be null as it is assigned just a few lines up
        ECSHandler.ECSs[typeof(EC_PathFinding)][uid].Active = false;
#pragma warning restore CS8602



        return uid;
    }
}






Setup via

Renderer.Setup();
Renderer.Start();

ECS.Setup();
ECS.Start();


and then functionality for .Stop, .Pause, and .Resume




*/

namespace Acrya
{
    public static class AcryaEngine
    {
        public static IMap? map = null;
    }



    public interface IMap
    {
        public int Width { get; }
        public int Height { get; }

        public IntPtr GenerateMapImage(int screenWidth, int screenHeight, IntPtr SDLRenderer);
    }
}
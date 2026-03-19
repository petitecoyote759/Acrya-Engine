

namespace Acrya.ECSComponents
{
    /// <summary>
    /// Interface all Entity Components should inherit from.
    /// </summary>
    public interface IEntityComponent
    {
        /// <summary>
        /// The active state of the component. If false the <see cref="Action(float, int)"/> function will not be called.
        /// </summary>
        public bool Active { get; set; }
        /// <summary>
        /// Bool representing if this component uses the GPU, if it does, then inherit from the IAcceleratedComponent instead.
        /// </summary>
        public static bool UsesGPU { get; } = false;
        /// <summary>
        /// The main running action of the component, called every time the ECS Handler itterates.
        /// </summary>
        /// <param name="dt">Time in ms since the last call.</param>
        /// <param name="uid">The uid of the entity being called.</param>
        public void Action(float dt, int uid);
        /// <summary>
        /// Cleanup function, called when the entity is deleted.
        /// </summary>
        /// <param name="uid">The uid of the entity being deleted.</param>
        public void Cleanup(int uid);
    }

    /// <summary>
    /// Entity component that uses the GPU to accelerate running, uses an overrided <see cref="HandleEntities"/> function 
    /// instead of being called normally by the ECS Handler. Defaults UsesGPU to true.
    /// </summary>
    public interface IAcceleratedComponent : IEntityComponent
    {
        public static new bool UsesGPU => true;
        public static void HandleEntities() => throw new NotImplementedException("Accelerated Component HandleEntities has not been overriden!");
    }
}

using Acrya.ECSHandlers;
using System.Diagnostics.CodeAnalysis;

namespace Acrya.ECSComponents
{
    /// <summary>
    /// Entity Component. Every entity has this, with basic functionality of storing the position of a entity and managing bounds checks, as well as handling references.
    /// </summary>
    public struct EC_Entity : IEntityComponent, IEquatable<EC_Entity>
    {
        public bool Active { readonly get => active; set => active = value; }
        private bool active = true;

        // <<Variables>> //

        public int uid = -1;

        public float x = 0;
        public float y = 0;

        public int intX = 0;
        public int intY = 0;

        public int gridX = 0;
        public int gridY = 0;
        
        // <<<Bounds Checks Variables>> //
        public float? minX = null;
        public float? maxX = null;
        public float? minY = null;
        public float? maxY = null;

        private readonly bool boundsCheckXMin = false;
        private readonly bool boundsCheckYMin = false;
        private readonly bool boundsCheckXMax = false;
        private readonly bool boundsCheckYMax = false;


        public HashSet<EntityReference> selfReferences = new HashSet<EntityReference>(); // all references to me



        public EC_Entity(int uid, float? minX = null, float? minY = null, float? maxX = null, float? maxY = null)
        {
            this.uid = uid;

            if (minX is not null) { this.minX = minX.Value; boundsCheckXMin = true; }
            if (maxX is not null) { this.maxX = maxX.Value; boundsCheckXMax = true; }

            if (minY is not null) { this.minY = minY.Value; boundsCheckYMin = true; }
            if (maxY is not null) { this.maxY = maxY.Value; boundsCheckYMax = true; }
        }

        public void Action(float dt, int uid) 
        {
            // <<Update Positions>> //

            // Bounds checks
            if (boundsCheckXMin) { x = MathF.Max(x, minX!.Value); }
            else if (boundsCheckXMax) { x = MathF.Min(x, maxX!.Value); }

            if (boundsCheckYMin) { y = MathF.Max(y, minY!.Value); }
            else if (boundsCheckYMax) { y = MathF.Min(y, maxY!.Value); }

            intX = (int)x;
            intY = (int)y;

            gridX = intX / ECSHandler.entityGridSize;
            gridY = intY / ECSHandler.entityGridSize;
        }

        public readonly void Cleanup(int uid) 
        { 
            foreach (EntityReference reference in selfReferences)
            {
                reference.Close(true);
            }
            selfReferences.Clear();
        }






        // <<Overrides>> //
        public readonly override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (obj is not EC_Entity entity) { return false; }

            return entity.uid == this.uid;
        }
        public readonly bool Equals(EC_Entity entity) { return this.uid == entity.uid; }
        public readonly override int GetHashCode() => base.GetHashCode();

        public static bool operator ==(EC_Entity left, EC_Entity right) { return left.Equals(right); }
        public static bool operator !=(EC_Entity left, EC_Entity right) { return !left.Equals(right); }
    }
}

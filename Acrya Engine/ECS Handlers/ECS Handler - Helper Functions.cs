using Acrya.ECSComponents;
using ShortTools.General;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acrya.ECSHandlers
{
    public static partial class ECSHandler
    {
        // <UID Functions> //
        public static int GetUID()
        {
            int length = entities.Count;

            for (int i = 0; i < length; i++)
            {
                if (entities[i] == false) { return i; } // see if that space is free
            }
            entities.Add(true);
            foreach (KeyValuePair<Type, List<IEntityComponent?>> pair in ECSs)
            {
                pair.Value.Add(null);
            }
            return length;
        }
        /// <summary>
        /// Destroys the given entity and calls the cleanup on all of the components.
        /// </summary>
        /// <param name="uid"></param>
        public static void FreeUID(int uid)
        {
            if (0 < uid || uid >= entities.Count) { debugger.AddLog($"Attempted to delete uid {uid} that was out of range", WarningLevel.Warning); return; }

            entities[uid] = false;
            foreach (KeyValuePair<Type, List<IEntityComponent?>> pair in ECSs)
            {
                pair.Value[uid]?.Cleanup(uid);
            }
        }





        public static bool IsClosed(int uid)
        {
            if (uid < 0 || uid >= entities.Count) { return false; }
            return entities[uid];
        }


        public static void CreateEntity([NotNull] IEntityComponent[] components)
        {
            int uid = GetUID();

            if (components.Any((component) => component.GetType() == typeof(EC_Entity)) == false) // No entity component in given, should absolutely not be one though
            {
                SetEntitiyComponent(uid, new EC_Entity(uid));
            }


            foreach (IEntityComponent component in components)
            {
                SetEntitiyComponent(uid, component);
            }
        }




#pragma warning disable CS8601 // component being null is okay as it should not be accessed if false is returned
        public static bool GetEntityComponent<T>(int uid, out T component) where T : IEntityComponent
        {
            T? nullableComponent = (T?)ECSs[typeof(T)][uid];
            if (nullableComponent is null) { component = default; return false; }

            component = (T)nullableComponent;
            return true;
        }
#pragma warning restore CS8601

        public static bool SetEntitiyComponent<T>(int uid, T component) where T : IEntityComponent
        {
            Type componentType = typeof(T);

            if (ECSs[componentType] is null) { return false; }

            ECSs[typeof(T)][uid] = component;
            return true;
        }
    }
}

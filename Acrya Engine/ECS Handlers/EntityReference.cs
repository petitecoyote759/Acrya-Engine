using Acrya.ECSComponents;
using ShortTools.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acrya.ECSHandlers
{
    /// <summary>
    /// Creates a reference between 2 entities, which is automatically closed when the target is destroyed.
    /// </summary>
    public class EntityReference
    {
        // <<Public Variables>> //
        public int Target => targetUid;
        private int targetUid;
        public int Referer => refereeUid;
        private int refereeUid;
        public bool Closed => closed;
        private bool closed;



        // NoReference Constructor
        private EntityReference() { closed = true; targetUid = -1; refereeUid = -1; }
        public EntityReference(int targetUid, int refereeUid)
        {
            this.targetUid = targetUid;
            this.refereeUid = refereeUid;
            this.closed = false;
        }

        public void Close(bool calledFromTarget)
        {
            if (calledFromTarget == false)
            {
                bool success = ECSHandler.GetEntityComponent(targetUid, out EC_Entity entityData);
                if (!success) { ECSHandler.debugger.AddLog($"Entity {targetUid} had no entity data!", WarningLevel.Warning); return; }
                _ = entityData.selfReferences.Remove(this);
            }

            targetUid = -1;
            refereeUid = -1;
            closed = true;
        }


        public static EntityReference NoReference => new EntityReference();
    }
}

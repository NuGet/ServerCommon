using System;

namespace NuGet.Services.Status.Table
{
    public class ChildComponentAffectingEntity<T> : ComponentAffectingEntity, IAggregatedEntity<T>
        where T : ComponentAffectingEntity
    {
        public ChildComponentAffectingEntity()
        {
        }

        public ChildComponentAffectingEntity(
            string partitionKey, 
            string rowKey,
            T entity,
            string affectedComponentPath,
            DateTime startTime,
            ComponentStatus affectedComponentStatus = ComponentStatus.Up,
            DateTime? endTime = null)
            : base(
                  partitionKey, 
                  rowKey, 
                  affectedComponentPath, 
                  startTime, 
                  affectedComponentStatus, 
                  endTime)
        {
            _parent = new ChildEntity<T>(partitionKey, rowKey, entity);
        }

        private readonly ChildEntity<T> _parent;
        public string ParentRowKey => _parent.ParentRowKey;
        public bool IsLinked => _parent.IsLinked;
    }
}

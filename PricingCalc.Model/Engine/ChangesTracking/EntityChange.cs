using System;
using System.Diagnostics;
using PricingCalc.Model.Engine.Core;

namespace PricingCalc.Model.Engine.ChangesTracking
{
    [DebuggerDisplay("Action = {Action}")]
    internal class EntityChange<TEntity, TData> : IEntityChange<TEntity, TData>
        where TEntity : IEntity
        where TData : ICopy<TData>
    {
        public EntityAction Action { get; }

        public TEntity Entity { get; }

        public TData OldData { get; }

        public TData NewData { get; }

        public EntityChange(EntityAction action, TEntity entity, TData oldData, TData newData)
        {
            Action = action;
            Entity = entity;
            OldData = oldData;
            NewData = newData;
        }

        public IEntityChange<TEntity, TData> Invert()
        {
            return new EntityChange<TEntity, TData>(InvertAction(Action), Entity, NewData, OldData);
        }

        private static EntityAction InvertAction(EntityAction action)
        {
            return action switch
            {
                EntityAction.Add => EntityAction.Remove,
                EntityAction.Remove => EntityAction.Add,
                EntityAction.Modify => EntityAction.Modify,
                _ => throw new NotSupportedException($"Action type {action} is not supported")
            };
        }
    }
}

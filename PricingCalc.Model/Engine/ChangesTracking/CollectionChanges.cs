using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using PricingCalc.Model.Engine.Core;

namespace PricingCalc.Model.Engine.ChangesTracking
{
    [DebuggerDisplay("HasChanges = {HasChanges()}")]
    public class CollectionChanges<TEntity, TData> : ICollectionChanges<TEntity, TData>
        where TEntity : IEntity, ICopy<TEntity>
        where TData : IEntityProperties, ICopy<TData>
    {
        private readonly IList<IEntityChange<TEntity, TData>> _changes;

        public CollectionChanges() : this(new List<IEntityChange<TEntity, TData>>())
        {
        }

        private CollectionChanges(IList<IEntityChange<TEntity, TData>> changes)
        {
            _changes = changes;
        }

        public void Add(EntityAction action, TEntity entity, TData oldData, TData newData)
        {
            _changes.Add(new EntityChange<TEntity, TData>(action, entity, oldData, newData));
        }

        public ICollectionChanges<TEntity, TData> Invert()
        {
            var inverted = _changes.Reverse().Select(x => x.Invert()).ToList();
            return new CollectionChanges<TEntity, TData>(inverted);
        }

        public bool HasChanges() => _changes.Count > 0;

        public void Apply(ICollection<TEntity, TData> collection)
        {
            var col = (ICollectionInternal<TEntity, TData>)collection;
            foreach (var change in _changes)
            {
                switch (change.Action)
                {
                    case EntityAction.Add:
                        col.Add(change.Entity, change.NewData);
                        break;
                    case EntityAction.Remove:
                        col.Remove(change.Entity);
                        break;
                    case EntityAction.Modify:
                        col.Modify(change.Entity, d =>
                        {
                            var bag = new PropertiesBag();
                            change.NewData.WriteTo(bag);
                            d.ReadFrom(bag);
                        });
                        break;
                    default:
                        break;
                }
            }
        }

        public IEnumerator<IEntityChange<TEntity, TData>> GetEnumerator()
        {
            return _changes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _changes.GetEnumerator();
        }
    }
}

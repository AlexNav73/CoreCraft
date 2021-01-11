using System.Collections.Generic;
using PricingCalc.Model.Engine.ChangesTracking;
using PricingCalc.Model.Engine.Core;

namespace PricingCalc.Model.Engine.Persistence
{
    public abstract class ModelShardStorage<TShard, TFrame> : IModelShardStorage
        where TShard : IModelShard
        where TFrame : class, IChangesFrame
    {
        public void Save(string path, IRepository repository, IModelChanges changes)
        {
            if (changes.TryGetFrame<TFrame>(out var frame))
            {
                SaveInternal(path, repository, frame);
            }
        }

        public void Load(string path, IRepository repository, IModel model)
        {
            var modelShard = model.Shard<TShard>();
            if (modelShard != null)
            {
                LoadInternal(path, repository, modelShard);
            }
        }

        protected abstract void SaveInternal(string path, IRepository repository, TFrame changes);

        protected abstract void LoadInternal(string path, IRepository repository, TShard shard);

        protected void Save<TEntity, TData>(IRepository repository, string name, ICollectionChanges<TEntity, TData> changes, Scheme scheme)
            where TEntity : IEntity, ICopy<TEntity>
            where TData : IEntityProperties, ICopy<TData>
        {
            if (!changes.HasChanges())
            {
                return;
            }

            var added = new List<KeyValuePair<TEntity, TData>>();
            var modified = new List<KeyValuePair<TEntity, TData>>();
            var removed = new List<TEntity>();

            foreach (var change in changes)
            {
                switch (change.Action)
                {
                    case EntityAction.Add:
                        added.Add(new KeyValuePair<TEntity, TData>(change.Entity, change.NewData));
                        break;

                    case EntityAction.Modify:
                        modified.Add(new KeyValuePair<TEntity, TData>(change.Entity, change.NewData));
                        break;

                    case EntityAction.Remove:
                        removed.Add(change.Entity);
                        break;
                }
            }

            if (added.Count > 0)
            {
                repository.Insert(name, added, scheme);
            }
            if (modified.Count > 0)
            {
                repository.Update(name, modified, scheme);
            }
            if (removed.Count > 0)
            {
                repository.Delete(name, removed);
            }
        }

        protected void Save<TParent, TChild>(IRepository repository, string name, IRelationCollectionChanges<TParent, TChild> changes)
            where TParent : IEntity
            where TChild : IEntity
        {
            if (!changes.HasChanges())
            {
                return;
            }

            var linked = new List<KeyValuePair<TParent, TChild>>();
            var unlinked = new List<KeyValuePair<TParent, TChild>>();

            foreach (var change in changes)
            {
                switch (change.Action)
                {
                    case RelationAction.Linked:
                        linked.Add(new KeyValuePair<TParent, TChild>(change.Parent, change.Child));
                        break;
                    case RelationAction.Unlinked:
                        unlinked.Add(new KeyValuePair<TParent, TChild>(change.Parent, change.Child));
                        break;
                }
            }

            if (linked.Count > 0)
            {
                repository.Insert(name, linked);
            }
            if (unlinked.Count > 0)
            {
                repository.Delete(name, unlinked);
            }
        }

        protected void Load<TEntity, TData>(IRepository repository, string name, ICollection<TEntity, TData> collection, Scheme scheme)
            where TEntity : IEntity, ICopy<TEntity>
            where TData : IEntityProperties, ICopy<TData>
        {
            repository.Select(name, collection, scheme);
        }

        protected void Load<TParent, TChild>(IRepository repository, string name, IRelation<TParent, TChild> relation, IEntityCollection<TParent> parents, IEntityCollection<TChild> children)
            where TParent : IEntity
            where TChild : IEntity
        {
            repository.Select(name, relation, parents, children);
        }
    }
}

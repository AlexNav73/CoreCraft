using System;
using System.Diagnostics;
using PricingCalc.Model.Engine.Core;

namespace PricingCalc.Model.Engine.ChangesTracking
{
    [DebuggerDisplay("Action = {Action}")]
    public class RelationChange<TParent, TChild> : IRelationChange<TParent, TChild>
        where TParent : IEntity
        where TChild : IEntity
    {
        public RelationAction Action { get; }

        public TParent Parent { get; }

        public TChild Child { get; }

        public RelationChange(RelationAction action, TParent parent, TChild child)
        {
            Action = action;
            Parent = parent;
            Child = child;
        }

        public IRelationChange<TParent, TChild> Invert()
        {
            return new RelationChange<TParent, TChild>(InvertAction(Action), Parent, Child);
        }

        private static RelationAction InvertAction(RelationAction action)
        {
            return action switch
            {
                RelationAction.Linked => RelationAction.Unlinked,
                RelationAction.Unlinked => RelationAction.Linked,
                _ => throw new NotSupportedException($"Action {action} is not supported")
            };
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using PricingCalc.Model.Engine.ChangesTracking;

namespace PricingCalc.Model.Engine.Core
{
    internal class ModelChanges : IModelChanges
    {
        protected readonly IList<IChangesFrame> Frames;

        public ModelChanges()
            : this(new List<IChangesFrame>())
        {
        }

        public ModelChanges(IList<IChangesFrame> frames)
        {
            Frames = frames;
        }

        public bool TryGetFrame<T>(out T frame)
            where T : class, IChangesFrame
        {
            frame = Frames.OfType<T>().SingleOrDefault()!;

            return frame != null;
        }

        public bool HasChanges()
        {
            return Frames.Any(x => x.HasChanges());
        }
    }
}

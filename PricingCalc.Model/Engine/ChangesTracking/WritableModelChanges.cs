using System.Collections.Generic;
using System.Linq;

namespace PricingCalc.Model.Engine.ChangesTracking
{
    internal class WritableModelChanges : ModelChanges, IWritableModelChanges
    {
        public WritableModelChanges()
        {
        }

        private WritableModelChanges(IList<IChangesFrame> frames)
            : base(frames)
        {
        }

        public T Add<T>(T newChanges) where T : IWritableChangesFrame
        {
            var frame = Frames.OfType<T>().SingleOrDefault();
            if (frame != null)
            {
                return frame;
            }

            Frames.Add(newChanges);
            return newChanges;
        }

        public IWritableModelChanges Invert()
        {
            var frames = Frames.Cast<IWritableChangesFrame>().Select(x => x.Invert()).ToArray();
            return new WritableModelChanges(frames);
        }

        public void Apply(IModel model)
        {
            foreach (var frame in Frames.Cast<IWritableChangesFrame>())
            {
                frame.Apply(model);
            }
        }
    }
}

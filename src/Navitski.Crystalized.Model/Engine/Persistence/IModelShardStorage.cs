using Navitski.Crystalized.Model.Engine.ChangesTracking;

namespace Navitski.Crystalized.Model.Engine.Persistence;

public interface IModelShardStorage
{
    void Save(IRepository repository, IModel model, IModelChanges changes);

    void Save(IRepository repository, IModel model);

    void Load(IRepository repository, IModel model);
}

using Navitski.Crystalized.Model.Engine.ChangesTracking;

namespace Navitski.Crystalized.Model.Engine.Persistence;

public interface IStorage
{
    void Save(string path, IModel model, IReadOnlyList<IModelChanges> changes);

    void Save(string path, IModel model);

    void Load(string path, IModel model);
}

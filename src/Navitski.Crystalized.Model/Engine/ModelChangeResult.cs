using Navitski.Crystalized.Model.Engine.ChangesTracking;

namespace Navitski.Crystalized.Model.Engine;

internal record ModelChangeResult(IModel OldModel, IModel NewModel, IWritableModelChanges Changes);

﻿using Navitski.Crystalized.Model.Engine.ChangesTracking;

namespace Navitski.Crystalized.Model.Engine.Core;

internal record ModelChangeResult(IModel OldModel, IModel NewModel, IWritableModelChanges Changes);
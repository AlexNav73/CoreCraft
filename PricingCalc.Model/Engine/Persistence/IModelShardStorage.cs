﻿using PricingCalc.Model.Engine.ChangesTracking;

namespace PricingCalc.Model.Engine.Persistence;

public interface IModelShardStorage
{
    void Save(IRepository repository, IModel model, IModelChanges changes);

    void Save(IRepository repository, IModel model);

    void Load(IRepository repository, IModel model);
}

using System;
using System.Collections.Generic;

namespace PricingCalc.Model.Engine.Persistence
{
    public record Property(string Name, Type Type, bool IsNullable);

    // TODO: Could we use PropertiesBag to generate queries?
    public record Scheme(IList<Property> Properties);
}

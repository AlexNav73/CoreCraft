﻿using System.Threading.Tasks;

namespace PricingCalc.Model.Engine.Commands
{
    public interface IModelCommand
    {
        Task Execute();
    }
}

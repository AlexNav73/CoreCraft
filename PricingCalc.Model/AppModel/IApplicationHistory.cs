using System;

namespace PricingCalc.Model.AppModel
{
    public interface IApplicationHistory
    {
        event EventHandler Changed;

        void Save(string path);

        void Load(string path);

        void Clear();

        void Undo();

        void Redo();

        bool HasChanges();
    }
}

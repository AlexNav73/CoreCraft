using System;
using System.Threading.Tasks;

namespace PricingCalc.Model.AppModel
{
    public interface IApplicationHistory
    {
        event EventHandler Changed;

        Task Save(string path);

        Task Load(string path);

        Task Clear();

        Task Undo();

        Task Redo();

        bool HasChanges();
    }
}

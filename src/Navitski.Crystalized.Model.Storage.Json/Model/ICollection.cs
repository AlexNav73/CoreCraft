namespace Navitski.Crystalized.Model.Storage.Json.Model;

internal interface ICollection
{
    string Name { get; set; }

    void Delete(Guid id);
}

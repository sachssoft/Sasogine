namespace sachssoft.Sasogine.Elements;

public interface IGameObject : IIdentifiable
{
    string? Name { get; set; }

    string? Class { get; set; }

    object? DataContext { get; set; }
}

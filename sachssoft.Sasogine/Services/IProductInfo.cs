namespace sachssoft.Sasogine.Services;
public interface IProductInfo
{
    int Id { get; }
    string Title { get; }
    string Description { get; }
    float Price { get; }
}
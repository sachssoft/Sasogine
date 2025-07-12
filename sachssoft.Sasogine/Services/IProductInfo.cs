namespace sachssoft.Sasogine.Services;

public interface IProductInfo
{
    string ProductId { get; }
    string Title { get; }
    string Description { get; }
    string Price { get; }
}
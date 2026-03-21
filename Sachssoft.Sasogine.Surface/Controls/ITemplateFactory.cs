namespace Sachssoft.Sasogine.Surface.Controls
{
    public interface ITemplateFactory<T> where T : Widget
    {
        T Create();
    }
}

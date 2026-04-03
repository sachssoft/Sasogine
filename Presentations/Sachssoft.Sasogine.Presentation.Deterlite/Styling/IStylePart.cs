namespace Sachssoft.Sasogine.Presentation.Deterlite.Styling
{
    public interface IStylePart
    {
        IStylePart Create(Skin sheet, PropertyMap properties);
    }
}

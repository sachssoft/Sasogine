namespace Sachssoft.Sasogine.Surface.Controls.Primitives
{
    public interface IPresenterTemplateFactory<TPresenter, TSource>
    {
        TPresenter CreatePresenter(TPresenter? template, TSource source);
    }
}

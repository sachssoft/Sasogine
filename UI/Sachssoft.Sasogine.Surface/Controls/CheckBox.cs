using Sachssoft.Sasogine.Surface.Basic;
using Sachssoft.Sasogine.Surface.Controls.Primitives;
using Sachssoft.Sasogine.Surface.Layouts;
using Sachssoft.Sasogine.Surface.Styles;
using Sachssoft.Sasogine.Surface.Visuals;

namespace Sachssoft.Sasogine.Surface.Controls;

public class CheckBox : CheckableButtonBase
{
    private StyleProperty<int> _checkContentSpacing = new StyleProperty<int>(0, isUserSet: false);
    private StyleProperty<IndicatorPosition> _checkPosition = new StyleProperty<IndicatorPosition>(Primitives.IndicatorPosition.Left, isUserSet: false);

    private readonly StackPanelLayout _layout = new StackPanelLayout(Orientation.Horizontal);

    public CheckBox()
    {
        LayoutContainer  = _layout;
    }

    #region Style Properties

    public StyleProperty<IndicatorPosition> CheckPosition
    {
        get => _checkPosition;
        set
        {
            if (SetAndNotify(ref _checkPosition, value))
            {
                UpdateLayout();
            }
        }
    }

    public StyleProperty<int> CheckContentSpacing
    {
        get => _checkContentSpacing;
        set
        {
            if (SetAndNotify(ref _checkContentSpacing, value))
            {
                UpdateLayoutProperties();
            }
        }
    }

    #endregion

    #region Style

    public override void ApplyFromStyle(Style? style)
    {
        base.ApplyFromStyle(style);

        var imageStyle = style?.FindStyle(typeof(Image));
        if (imageStyle != null)
        {
            StateIndicator.ApplyFromStyle(imageStyle);
        }

        var labelStyle = style?.FindStyle(typeof(Label));
        if (labelStyle != null && ContentPresenter is Label label)
        {
            label.ApplyFromStyle(labelStyle);
        }

        style?.Apply(this, (target, sheet, property, value) =>
        {
            switch (property)
            {
                case nameof(CheckContentSpacing):
                    target.CheckContentSpacing = target.CheckContentSpacing.Override(value.ConvertTo<int>());
                    break;
            }
        });
    }

    public override void ApplyFrom(ElementBase other)
    {
        base.ApplyFrom(other);

        if (other is not CheckBox source)
            return;

        CheckContentSpacing = source.CheckContentSpacing;
        CheckPosition = source.CheckPosition;

        // Unterelemente übernehmen
        StateIndicator.ApplyFrom(source.StateIndicator);

        UpdateLayout();
        UpdateLayoutProperties();
    }

    protected override ElementBase CreateCloneInstance()
    {
        return new CheckBox();
    }

    #endregion

    protected override void OnLayoutChanged(PresenterEventArgs e)
    {
        base.OnLayoutChanged(e);

        Children.Clear();

        switch (_checkPosition.Value)
        {
            case Primitives.IndicatorPosition.Left:
                Children.Add(StateIndicator);
                if (e.NewPresenter != null)
                {
                    Children.Add(e.NewPresenter);
                }

                break;

            case Primitives.IndicatorPosition.Right:
                if (e.NewPresenter != null)
                {
                    Children.Add(e.NewPresenter);
                }
                Children.Add(StateIndicator);

                break;
        }
    }

    private void UpdateLayoutProperties()
    {
        _layout.Spacing = _checkContentSpacing.Value;
    }
}

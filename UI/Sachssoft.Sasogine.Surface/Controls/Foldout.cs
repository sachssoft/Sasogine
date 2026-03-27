using Sachssoft.Sasogine.Surface.Basic;
using Sachssoft.Sasogine.Surface.Controls.Primitives;
using Sachssoft.Sasogine.Surface.Layouts;
using Sachssoft.Sasogine.Surface.Styles;
using Sachssoft.Sasogine.Surface.Visuals;
using Sachssoft.Sasogine.Surface.Visuals.Regions;
using System;
using System.Net.Http.Headers;

namespace Sachssoft.Sasogine.Surface.Controls
{
    public class Foldout : ContentControl, IGroup
    {
        private StyleProperty<int> _iconHeaderSpacing = new StyleProperty<int>(0, isUserSet: false);
        private StyleProperty<IndicatorPosition> _markPosition = new StyleProperty<IndicatorPosition>(Primitives.IndicatorPosition.Left, isUserSet: false);
        private StyleProperty<bool> _isExpanded = new StyleProperty<bool>(true, isUserSet: false);

        private readonly CheckBox _headerCheckBox = new CheckBox();
        private readonly GridLayout _layout = new GridLayout();
        private bool _expandedByCheckBox;

        public Foldout()
        {
            _headerCheckBox.HorizontalAlignment = Visuals.HorizontalAlignment.Stretch;
            _headerCheckBox.IsChecked = true;
            _headerCheckBox.IsCheckedChanged += HeaderCheckBox_IsCheckedChanged;

            _layout.RowsProportions.Add(new Proportion(ProportionType.Auto));
            _layout.RowsProportions.Add(new Proportion(ProportionType.Fill));

            LayoutContainer  = _layout;

            HorizontalAlignment = HorizontalAlignment.Override(Visuals.HorizontalAlignment.Stretch);
            VerticalAlignment = VerticalAlignment.Override(Visuals.VerticalAlignment.Top);
        }

        #region Style Properties

        public StyleProperty<IndicatorPosition> IconPosition
        {
            get => _markPosition;
            set
            {
                if (SetAndNotify(ref _markPosition, value))
                {
                    UpdateLayout();
                }
            }
        }

        public StyleProperty<bool> IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (SetAndNotify(ref _isExpanded, value))
                {
                    if (!_expandedByCheckBox)
                    {
                        _headerCheckBox.IsChecked = value;
                        return;
                    }

                    UpdateLayout();
                }
            }
        }

        public StyleProperty<int> IconHeaderSpacing
        {
            get => _iconHeaderSpacing;
            set
            {
                if (SetAndNotify(ref _iconHeaderSpacing, value))
                {
                    UpdateLayoutProperties();
                }
            }
        }

        public StyleProperty<ITextureRegion?> UnexpandedIcon
        {
            get => _headerCheckBox.UncheckedImage;
            set => _headerCheckBox.UncheckedImage = value;
        }

        public StyleProperty<ITextureRegion?> ExpandedIcon
        {
            get => _headerCheckBox.CheckedImage;
            set => _headerCheckBox.CheckedImage = value;
        }

        public StyleProperty<object?> Header
        {
            get => _headerCheckBox.Content;
            set => _headerCheckBox.Content = value;
        }

        #endregion

        #region Direct Properties

        public ContentControl HeaderPresenter
        {
            get => _headerCheckBox;
        }

        #endregion

        #region Style

        public override void ApplyFromStyle(Style? style)
        {
            base.ApplyFromStyle(style);

            var headertyle = style?.FindStyle(typeof(CheckBox));
            _headerCheckBox.ApplyFromStyle(headertyle);

            //if (imageStyle != null)
            //{
            //    imageStyle.Apply(_headerCheckBox, (target, sheet, property, value) =>
            //    {
            //        switch (property)
            //        {
            //            case nameof(Image.Visual):
            //                target.CheckedImage = target.CheckedImage.Override(sheet.FindRegion(value.RawValue));
            //                break;
            //            case nameof(Image.PressedVisual):
            //                target.UncheckedImage = target.UncheckedImage.Override(sheet.FindRegion(value.RawValue));
            //                break;
            //        }
            //    });
            //}

            var contentStyle = style?.FindStyle(typeof(Label));
            if (contentStyle != null && Content.Value is Label contentLabel)
            {
                contentLabel.ApplyFromStyle(contentStyle);
            }

            var headerStyle = style?.FindStyle(typeof(Label), "header");
            if (headerStyle != null && Content.Value is Label headerLabel)
            {
                headerLabel.ApplyFromStyle(headerStyle);
            }

            style?.Apply(this, (target, sheet, property, value) =>
            {
                switch (property)
                {
                    case nameof(IconHeaderSpacing):
                        target.IconHeaderSpacing = target.IconHeaderSpacing.Override(value.ConvertTo<int>());
                        break;
                }
            });
        }

        public override void ApplyFrom(ElementBase other)
        {
            base.ApplyFrom(other);

            if (other is not Foldout source)
                return;

            IconPosition = source.IconPosition;
            IconHeaderSpacing = source.IconHeaderSpacing;
            IsExpanded = source.IsExpanded;

            UnexpandedIcon = source.UnexpandedIcon;
            ExpandedIcon = source.ExpandedIcon;
            Header = source.Header;

            _headerCheckBox.ApplyFrom(source._headerCheckBox);

            if (Content.Value is Widget content && source.Content.Value is Widget srcContent)
                content.ApplyFrom(srcContent);

            if (Header.Value is Widget header && source.Header.Value is Widget srcHeader)
                header.ApplyFrom(srcHeader);

            UpdateLayout();
            UpdateLayoutProperties();
        }

        protected override ElementBase CreateCloneInstance()
        {
            return new Foldout();
        }

        #endregion

        protected override void OnLayoutChanged(PresenterEventArgs e)
        {
            base.OnLayoutChanged(e);

            Children.Clear();

            Grid.SetRow(_headerCheckBox, 0);
            Children.Add(_headerCheckBox);

            if (e.NewPresenter != null && _isExpanded.Value)
            {
                Grid.SetColumn(e.NewPresenter, 0);
                Grid.SetRow(e.NewPresenter, 1);
                Children.Add(e.NewPresenter);
            }
        }

        private void UpdateLayoutProperties()
        {
            _layout.ColumnSpacing = _iconHeaderSpacing.Value;
        }

        private void HeaderCheckBox_IsCheckedChanged(object? sender, EventArgs e)
        {
            _expandedByCheckBox = true;
            IsExpanded = _headerCheckBox.IsChecked;
            _expandedByCheckBox = false;
        }

        #region IGroup

        object? IGroup.Header
        {
            get => Header.Value;
            set => Header = value;
        }

        object? IGroup.Content
        {
            get => Content.Value;
            set => Content = value;
        }

        #endregion
    }
}

using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Surface.Layouts;
using Sachssoft.Sasogine.Surface.Utility;
using Sachssoft.Sasogine.Surface.Visuals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Surface.Controls
{
    public partial class Desktop
    {
        private readonly LayoutWidgetCollection _layoutChildren = new LayoutWidgetCollection();

        private Rectangle _bounds;
        private Transform? _transform;
        private Matrix _inverseMatrix;
        private bool _inverseMatrixDirty = true;
        private bool _layoutDirty = true;

        #region Bounds

        public static Rectangle DefaultBoundsFetcher()
        {
            var size = CrossEngineStuff.ViewSize;
            return new Rectangle(0, 0, size.X, size.Y);
        }

        internal Rectangle LayoutBounds => new Rectangle(0, 0, InternalBounds.Width, InternalBounds.Height);


        public Func<Rectangle> BoundsFetcher = DefaultBoundsFetcher;

        internal Rectangle InternalBounds
        {
            get => _bounds;

            set
            {
                if (_bounds == value)
                {
                    return;
                }

                _bounds = value;


                InvalidateTransform();
            }
        }

        #endregion

        #region Transform

        private void InvalidateTransform()
        {
            _transform = null;
            _inverseMatrixDirty = true;

            foreach (var child in LayoutChildren)
            {
                child.InvalidateTransform();
            }
        }

        #endregion

        #region Layout

        public void InvalidateLayout()
        {
            _layoutDirty = true;
        }

        public void UpdateLayout()
        {
            var newBounds = BoundsFetcher();
            if (InternalBounds != newBounds)
            {
                InvalidateLayout();
            }

            InternalBounds = newBounds;

            if (InternalBounds.IsEmpty)
            {
                return;
            }

            if (!_layoutDirty)
            {
                return;
            }

            foreach (var child in LayoutChildren)
            {
                if (child.IsVisible)
                {
                    child.Arrange(LayoutBounds);
                }
            }

            // Rest processing
            //MenuBar = null;

            var childrenCopy = LayoutChildren;
            for (var i = childrenCopy.Count - 1; i >= 0; --i)
            {
                var w = childrenCopy[i];
                if (!w.IsVisible)
                {
                    continue;
                }

                //MenuBar = w.FindChild<HorizontalMenu>();
                //if (MenuBar != null)
                //{
                //    break;
                //}
            }

            UpdateRecursiveLayout(LayoutChildren);
            _layoutDirty = false;
            LayoutChanged?.Invoke(this, EventArgs.Empty);
        }

        private void UpdateRecursiveLayout(IEnumerable<Widget> widgets)
        {
            foreach (var i in widgets)
            {
                UpdateRecursiveLayout(i.LayoutChildren);
            }
        }

        private void UpdateLayoutChildren()
        {
            if (!_widgetsDirty)
                return;

            _layoutChildren.Clear();
            _layoutChildren.AddRange(Widgets);

            _layoutChildren.SortZIndex();

            _widgetsDirty = false;
        }

        internal LayoutWidgetCollection LayoutChildren
        {
            get
            {
                UpdateLayoutChildren();
                return _layoutChildren;
            }
        }

        #endregion

        #region Local/Global

        public Vector2 ToGlobal(Vector2 pos) => Transform.Apply(pos);

        public Point ToGlobal(Point pos) => Transform.Apply(pos);

        public Vector2 ToLocal(Vector2 source)
        {
            if (_inverseMatrixDirty)
            {
                _inverseMatrix = Matrix.Invert(Transform.Matrix);
                _inverseMatrixDirty = false;
            }

            return source.Transform(ref _inverseMatrix);
        }

        public Point ToLocal(Point pos) => ToLocal(new Vector2(pos.X, pos.Y)).ToPoint();

        #endregion
    }
}

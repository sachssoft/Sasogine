using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Components.Tools.Selection
{
    public abstract class SelectionToolLayer
    {
        private readonly List<SelectionToolNode> _nodes;
        private ISelectionTarget? _selectionTarget;

        protected SelectionToolLayer()
        {
            _nodes = new List<SelectionToolNode>();
        }

        public ISelectionTarget? SelectionTarget
        {
            get => _selectionTarget;
            internal set
            {
                if (ReferenceEquals(_selectionTarget, value))
                    return;

                _selectionTarget = value;
                OnTargetEnter(value);
            }
        }

        protected IList<SelectionToolNode> Nodes => _nodes;

        protected virtual void OnTargetEnter(
            ISelectionTarget? target)
        {
        }

        protected internal virtual void OnTargetInvalidated(
            ISelectionTarget? target)
        {
        }

        protected internal virtual void OnNodeInteract(
            SelectionToolNode node)
        {
        }

        internal IReadOnlyList<SelectionToolNode> GetNodes()
        {
            return _nodes;
        }
    }
}
using Sachssoft.Sasogine.Components;
using Sachssoft.Sasogine.Components.Tools;
using Sachssoft.Sasogine.Scenes;
using System;
using System.Collections.Generic;


namespace Sachssoft.Sasogine.Components.Tools
{
    public abstract class ToolComponentBase : ResourceComponentBase, IUpdatableComponent, IDrawableComponent
    {
        private readonly List<ToolBase> _tools = new();

        protected IList<ToolBase> Tools => _tools;

        public void Update(SceneUpdateContext context)
        {
            for (int i = 0; i < _tools.Count; i++)
            {
                ToolBase tool = _tools[i];

                if (tool.IsActive)
                    tool.Update(context);
            }
        }

        public void Draw(SceneDrawContext context)
        {
            for (int i = 0; i < _tools.Count; i++)
            {
                ToolBase tool = _tools[i];

                if (tool.IsActive)
                    tool.Draw(context);
            }
        }
    }
}
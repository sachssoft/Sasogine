using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Graphics.Text
{
    public class TextLayoutOptions
    {

        public Alignment HorizontalAlignment { get; init; }

        public Alignment VerticalAlignment {  get; init; }

        public TextAlignment TextAlignment { get; init; }

        public TextWrap Wrap { get; init; }

        public FlowDirection FlowDirection { get; init; }
        
        public bool IsMultiline { get; init; } = false;


    }
}

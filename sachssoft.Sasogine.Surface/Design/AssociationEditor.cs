using System;
using System.Collections.Generic;
using Sachssoft.Sasogine.Elements;
using Sachssoft.Sasogine.Surface.Visuals.Controls;

namespace Sachssoft.Sasogine.Surface.Design;

public class AssociationEditor : PropertyEditorBase
{
    public AssociationEditor()
    {
    }

    //public override bool ForType(Type type)
    //{
    //    return type.IsAssignableTo(typeof(IAssociation)) && !type.IsAbstract;
    //}

    //public override Widget CreateControl()
    //{
    //    var cbo = new ComboView()
    //    {
    //        HorizontalAlignment = HorizontalAlignment.Stretch
    //    };

    //    var selected_item = (IAssociation?)Value;
    //    var d = PropertyName;
    //    var index = 0;

    //    cbo.Widgets.Add(new Label()
    //    {
    //        Text = ""
    //    });
    //    cbo.SelectedIndex = 0;

    //    var prop_type = GetActuallyPropertyType();

    //    if (Items != null)
    //    {
    //        foreach (var item in Items)
    //        {
    //            var inst = (IAssociation)Activator.CreateInstance(GetActuallyPropertyType())!;

    //            if (!(inst.Type == item.GetType() || inst.Type.IsAssignableFrom(item.GetType())))
    //            {
    //                continue;
    //            }

    //            index++;
    //            inst.ID = item.ID;

    //            cbo.Widgets.Add(new Label()
    //            {
    //                Text = item.ID,
    //                Tag = inst
    //            });

    //            if (selected_item?.ID == item.ID)
    //            {
    //                cbo.SelectedIndex = index;
    //            }
    //        }
    //    }

    //    cbo.SelectedIndexChanged += (s, e) =>
    //    {
    //        if (cbo.SelectedIndex == 0)
    //        {
    //            if (IsNullable())
    //                Value = null;
    //            else
    //                Value = default;
    //        }
    //        else
    //        {
    //            Value = cbo.SelectedItem.Tag;
    //        }
    //    };

    //    return cbo;
    //}

    public IEnumerable<GameObject>? Items
    {
        get;
        set;
    }
}
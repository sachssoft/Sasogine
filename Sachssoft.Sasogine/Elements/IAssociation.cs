/* 
 * © 2024 Tobias Sachs
 * IAssociation
 * 11.07.2024 
*/

using System;
using System.Collections;

namespace Sachssoft.Sasogine.Elements;

public interface IAssociation
{
    string? ID { get; set; }

    Type Type { get; }

    bool ContainsAmbiguous(IEnumerable collection);

    object? Find(IEnumerable collection);
}

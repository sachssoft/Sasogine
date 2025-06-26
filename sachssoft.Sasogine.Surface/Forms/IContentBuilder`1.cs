/* 
 * © 2024 Tobias Sachs
 * IContentBuilder
 * 11.07.2024 
*/

using System;

namespace sachssoft.Sasogine.Surface.Forms;

public interface IContentBuilder<T>
{

    void Build(T owner);

}
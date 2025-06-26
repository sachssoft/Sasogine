/* 
 * © 2024 Tobias Sachs
 * IWebBrowserProvider
 * 11.07.2024 
*/

using System;

namespace sachssoft.Sasogine.Providers;

public interface IWebBrowserProvider
{

    void Open(Uri uri);

}
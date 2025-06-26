/* 
 * © 2024 Tobias Sachs
 * GameException
 * 11.07.2024 
*/

using System;
using System.IO;

namespace sachssoft.Sasogine;

public class GameException : Exception
{

    public GameException(string message) : base(message)
    {
    }

}

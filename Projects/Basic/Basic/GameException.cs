/* 
 * © 2024 Tobias Sachs
 * GameException
 * 11.07.2024 
*/

using System;

namespace Sachssoft.Sasogine;

public class GameException : Exception
{

    public GameException(string message) : base(message)
    {
    }

}

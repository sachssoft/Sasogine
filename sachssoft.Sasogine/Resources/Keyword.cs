/* 
 * © 2024 Tobias Sachs
 * Keyword
 * 11.07.2024 
*/

namespace Sachssoft.Sasogine.Resources
{
    // Stichwort
    public readonly struct Keyword
    {
        public Keyword() { }

        public required string Name
        {
            get;
            init;
        }

        public required string TextKey
        {
            get;
            init;
        }

        public required string TextDefaultText
        {
            get;
            init;
        }

        public string? TextDefaultDescription
        {
            get;
            init;
        }
    }
}

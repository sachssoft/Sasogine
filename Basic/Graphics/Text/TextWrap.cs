namespace Sachssoft.Sasogine.Graphics.Text
{
    public enum TextWrap
    {
        None,      // Eine Zeile, schneidet ab, wenn Text länger als Width
        Word,    // Zeilenumbruch an Wortgrenzen
        Character // Zeilenumbruch auch mitten in einem Wort
    }
}

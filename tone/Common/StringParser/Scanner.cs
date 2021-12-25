using System.IO;

namespace tone.Common.StringParser;
public class Scanner:StringReader
{
    private readonly string _input;
    private readonly int _maxIndex;
    public int Index { get; private set; }
    
    // https://stackoverflow.com/questions/289792/int-to-char-in-c-sharp
    // todo: change to stream? Additional ctor: https://stackoverflow.com/questions/45351430/converting-string-to-stream
    // MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
    // + disposable?
    /* https://stackoverflow.com/questions/1879395/how-do-i-generate-a-stream-from-a-string
  public static Stream GenerateStreamFromString(string s)
{
    var stream = new MemoryStream();
    var writer = new StreamWriter(stream);
    writer.Write(s);
    writer.Flush();
    stream.Position = 0;
    return stream;
}
using (var stream = GenerateStreamFromString("a,b \n c,d"))
{
    // ... Do stuff to stream
}
     */
    public Scanner(string input) : base(input)
    {
        _input = input;
        _maxIndex = input.Length - 1;
        Index = 0;
    }
    
    public char? Peek()
    {
        return Index < _maxIndex ? _input[Index] : null;
    }
    
    public char? Poke()
    {
        var value = Peek();
        if(value != null)
        {
            Index++;
        }
        return value;
    }
    
    public bool HasNext()
    {
        return Peek() != null;
    }
    
    // todo: readline
}
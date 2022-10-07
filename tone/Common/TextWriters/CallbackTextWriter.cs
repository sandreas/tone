using System;
using System.IO;
using System.Linq;
using System.Text;

namespace tone.Common.TextWriters;

public sealed class CallbackTextWriter : TextWriter
{
    private readonly Action<string>? _callback;
    private readonly StringBuilder _builder = new();
    private readonly char _lastNewLineChar;
    private readonly string _newLineString;

    public CallbackTextWriter(Action<string> callback)
    {
        _callback = callback;
        _lastNewLineChar = NewLine.Last();
        _newLineString = string.Join("", NewLine);
    }

    public override void Write(string? value)
    {
        if (value != null)
        {
            _callback?.Invoke(value);
        }
    }

    public override void Write(char value)
    {
        _builder.Append(value);
        if (value != _lastNewLineChar)
        {
            return;
        }

        var str = _builder.ToString();
        if (str.EndsWith(_newLineString))
        {
            _flushString(str);
        }
    }

    private void _flushString(string logString)
    {
        _callback?.Invoke(logString);
        _builder.Clear();
    }

    protected override void Dispose(bool disposing)
    {
        if (_builder.Length > 0)
        {
            _flushString(_builder.ToString());    
        }
        base.Dispose(disposing);
    }

    public override Encoding Encoding => Encoding.Default;
}
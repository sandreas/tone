using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;

namespace tone.Directives;

public class LimitDirective : IDirective<IEnumerable<IFileInfo>>
{
    private int Skip { get; }
    private int Take { get; } = int.MaxValue;

    public LimitDirective(string limit)
    {
        var parts = limit.Split(",");
        if (parts.Length == 0)
        {
            return;
        }

        if (!int.TryParse(parts[0], out var firstNumber))
        {
            return;
        }

        if (parts.Length == 2)
        {
            if (!int.TryParse(parts[1], out var secondNumber))
            {
                return;
            }

            Skip = firstNumber;
            Take = secondNumber;
        }
        else
        {
            Take = firstNumber;
        }
    }


    public IEnumerable<IFileInfo> Apply(IEnumerable<IFileInfo> subject)
    {
        if (Skip != 0)
        {
            if (Take < int.MaxValue)
            {
                return subject.Skip(Skip).Take(Take);
            }
            return subject.Skip(Skip);
        }

        return Take < int.MaxValue ? subject.Take(Take) : subject;
    }
}
using System.Collections.Generic;

namespace tone.Metadata.Taggers;

public class ExtraFieldsTagger : ITagger
{
    private readonly IReadOnlyList<string> _extraFields;

    public ExtraFieldsTagger(IReadOnlyList<string> extraFields)
    {
        _extraFields = extraFields;
    }

    public void Update(IMetadata metadata)
    {
        if (metadata.AdditionalFields == null)
        {
            metadata.AdditionalFields = new Dictionary<string, string>();
        }

        if (_extraFields.Count % 2 != 0)
        {
            return;
            // await console.Error.WriteLineAsync(
            //     "--meta-additional-fields has to contain an even number of values (e.g. --meta-additional-fields <fieldname> <fieldvalue>)");
        }

        var index = 0;
        var key = "";
        foreach (var field in _extraFields)
        {
            if (index++ % 2 == 0)
            {
                key = field;
                continue;
            }

            if (metadata.AdditionalFields.ContainsKey(key))
            {
                continue;
            }

            metadata.AdditionalFields[key] = field;
        }
    }
}
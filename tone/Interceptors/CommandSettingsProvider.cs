using System;
using System.Collections.Generic;
using System.Linq;
using Spectre.Console.Cli;
using tone.Metadata.Taggers;

namespace tone.Interceptors;

public class CommandSettingsProvider
{
    public CommandSettings? Settings { get; set; }

    public T? Get<T>() where T: class
    {
        if (Settings is T s)
        {
            return s;
        }
        return null;
    }
    
    public TReturn? Build<TSettings,TReturn>(Func<TSettings, TReturn> func) where TSettings: class
    {
        var settings = Get<TSettings>();
        return settings != null ? func(settings) : default;
    }    
    
    public CommandSettingsProvider Append<TReturn, TSettings>(ref List<TReturn> taggers, Func<TSettings, TReturn> func)
    {
        if (Settings is TSettings s)
        {
            taggers.Add(func(s));
        }

        return this;
    }
}
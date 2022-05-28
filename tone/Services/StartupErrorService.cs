using System;
using System.Collections.Generic;

namespace tone.Services;

public class StartupErrorService
{
    public List<(ReturnCode,string)> Errors { get; set; } = new();

    public bool HasErrors => Errors.Count > 0;

    public int ShowErrors(Action< string> showErrorCallback)
    {
        var returnCode = ReturnCode.Success;
        foreach (var (errorCode, errorMessage) in Errors)
        {
            showErrorCallback(errorMessage);
            if (returnCode < errorCode)
            {
                returnCode = errorCode;
            }
        }

        return (int)returnCode;
    }
}
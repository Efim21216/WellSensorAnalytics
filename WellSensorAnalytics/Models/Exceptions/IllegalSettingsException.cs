using System;

namespace WellSensorAnalytics.Models.Exceptions;

public class IllegalSettingsException : Exception
{
    public IllegalSettingsException()
    {
    }

    public IllegalSettingsException(string message)
        : base(message)
    {
    }

    public IllegalSettingsException(string message, Exception inner)
        : base(message, inner)
    {
    }
}

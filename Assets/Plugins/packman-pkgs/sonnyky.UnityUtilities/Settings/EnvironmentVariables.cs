using System;
using System.IO;
using UnityEngine;

public static class EnvironmentVariables
{
    public static string GetVariable(string variable)
    {
        string value = "";

        // Check whether the environment variable exists.
        value = Environment.GetEnvironmentVariable(variable, EnvironmentVariableTarget.User);

        if (value == null)
        {
            Environment.SetEnvironmentVariable(variable, "None");

            // Now retrieve it.
            value = Environment.GetEnvironmentVariable(variable);
        }
        return value;
    }
}

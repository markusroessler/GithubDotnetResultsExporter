using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GithubDotnetResultsExporter.Model;

internal interface IEnvironment
{
    string CurrentDirectory { get; }

    string? GetEnvironmentVariable(string variable);
}

internal sealed class DefaultEnvironment : IEnvironment
{
    public string CurrentDirectory => Environment.CurrentDirectory;

    public string? GetEnvironmentVariable(string variable) => Environment.GetEnvironmentVariable(variable);
}

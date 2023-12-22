using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using GithubDotnetResultsExporter.Model.Vstst;
using Microsoft.Extensions.Logging;

namespace GithubDotnetResultsExporter.Model;

internal sealed class TestRunProvider
{
    private readonly ILogger _logger;

    public TestRunProvider(ILogger<TestRunProvider> logger)
    {
        _logger = logger;
    }

    public IEnumerable<TestRunType> LoadTestRuns(IEnumerable<string> files)
    {
        var serializer = new XmlSerializer(typeof(TestRunType));
        foreach (var file in files)
        {
            _logger.LogInformation("Loading trx file: {file}", file);
            using var myFileStream = new FileStream(file, FileMode.Open);
            yield return (TestRunType?)serializer.Deserialize(myFileStream) ?? throw new Exception("XmlSerializer.Deserialize returned null");
        }
    }
}

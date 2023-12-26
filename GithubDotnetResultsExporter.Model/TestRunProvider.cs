using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
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

    public IList<TestRunType> LoadTestRuns(IEnumerable<string> files)
    {
        var serializer = new XmlSerializer(typeof(TestRunType));
        return files.Select(file =>
        {
            _logger.LogInformation("Loading trx file: {File}", file);
            using var fileStream = new FileStream(file, FileMode.Open);
            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Ignore,
                XmlResolver = null
            };
            var xmlReader = XmlReader.Create(fileStream, settings);
            return (TestRunType?)serializer.Deserialize(xmlReader) ?? throw new InvalidOperationException("XmlSerializer.Deserialize returned null");
        }).ToList();
    }
}

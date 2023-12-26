using Microsoft.CodeAnalysis.Sarif;

namespace GithubDotnetResultsExporter.Model.Test;

public class GithubDotnetResultsExporterModelOpsTest
{

    [Test]
    public void Test_MapToAnnotationRequests()
    {
        var sarifResults = new List<Result>
        {
            new Result
            {
                Level = FailureLevel.Warning,
                Message = new Message {Text = "Warning Message" },
                Locations = new List<Location>
                {
                    new Location
                    {
                        PhysicalLocation = new PhysicalLocation
                        {
                            ArtifactLocation = new ArtifactLocation { Uri = new Uri("file:///repo/project/Foobar.cs") },
                            Region = new Region
                            {
                                StartLine = 1,
                                StartColumn = 2,
                                EndLine = 3,
                                EndColumn = 4
                            }
                        }
                    }
                }
            },
            new Result
            {
                Level = FailureLevel.Error,
                Message = new Message {Text = "Error Message" },
                Locations = new List<Location>
                {
                    new Location
                    {
                        PhysicalLocation = new PhysicalLocation
                        {
                            ArtifactLocation = new ArtifactLocation { Uri = new Uri("file:///repo/project/Blub.cs") },
                            Region = new Region
                            {
                                StartLine = 1,
                                StartColumn = 2,
                                EndLine = 3,
                                EndColumn = 4
                            }
                        }
                    }
                }
            }
        };

        var collectorRequest = new GithubDotnetResultsExporterRequest(true, true, "https://github.com", "markusroessler/GithubDotnetResultsExporter", "develop");

        var requests = GithubDotnetResultsExporterModelOps.MapToAnnotationRequests(sarifResults, collectorRequest, "/repo");

        Assert.That(requests, Has.Count.EqualTo(2));

        var request = requests[0];
        Assert.Multiple(() =>
        {
            Assert.That(request.Path, Is.EqualTo($"project{Path.DirectorySeparatorChar}Foobar.cs"));
            Assert.That(request.RawDetails, Is.EqualTo("https://github.com/markusroessler/GithubDotnetResultsExporter/blob/develop/project/Foobar.cs#L1"));
            Assert.That(request.StartLine, Is.EqualTo(1));
            Assert.That(request.StartColumn, Is.EqualTo(2));
            Assert.That(request.EndLine, Is.EqualTo(3));
            Assert.That(request.EndColumn, Is.EqualTo(4));
            Assert.That(request.Message, Is.EqualTo("Warning Message"));
            Assert.That(request.AnnotationLevel, Is.EqualTo("warning"));
        });

        request = requests[1];
        Assert.Multiple(() =>
        {
            Assert.That(request.Path, Is.EqualTo($"project{Path.DirectorySeparatorChar}Blub.cs"));
            Assert.That(request.RawDetails, Is.EqualTo("https://github.com/markusroessler/GithubDotnetResultsExporter/blob/develop/project/Blub.cs#L1"));
            Assert.That(request.StartLine, Is.EqualTo(1));
            Assert.That(request.StartColumn, Is.EqualTo(2));
            Assert.That(request.EndLine, Is.EqualTo(3));
            Assert.That(request.EndColumn, Is.EqualTo(4));
            Assert.That(request.Message, Is.EqualTo("Error Message"));
            Assert.That(request.AnnotationLevel, Is.EqualTo("failure"));
        });
    }

    [Test]
    public void Test_CreateSummaryMarkdown()
    {
        var sarifResults = new List<Result>
        {
            new Result
            {
                Level = FailureLevel.Warning,
                Message = new Message {Text = "Warning Message" },
                Locations = new List<Location>
                {
                    new Location
                    {
                        PhysicalLocation = new PhysicalLocation
                        {
                            ArtifactLocation = new ArtifactLocation { Uri = new Uri("file:///repo/project/Foobar.cs") },
                            Region = new Region
                            {
                                StartLine = 1,
                                StartColumn = 2,
                                EndLine = 3,
                                EndColumn = 4
                            }
                        }
                    }
                }
            },
            new Result
            {
                Level = FailureLevel.Warning,
                Message = new Message {Text = "Warning without location" },
            },
            new Result
            {
                Level = FailureLevel.Error,
                Message = new Message {Text = "Error Message" },
                Locations = new List<Location>
                {
                    new Location
                    {
                        PhysicalLocation = new PhysicalLocation
                        {
                            ArtifactLocation = new ArtifactLocation { Uri = new Uri("file:///repo/project/Blub.cs") },
                            Region = new Region
                            {
                                StartLine = 1,
                                StartColumn = 2,
                                EndLine = 3,
                                EndColumn = 4
                            }
                        }
                    }
                }
            }
        };

        var collectorRequest = new GithubDotnetResultsExporterRequest(true, true, "https://github.com", "markusroessler/GithubDotnetResultsExporter", "develop");

        var markdown = GithubDotnetResultsExporterModelOps.CreateSummaryMarkdown(sarifResults, collectorRequest, "/repo");
        Console.WriteLine(markdown);

        Assert.That(markdown, Is.EqualTo(
            """
            ## Build Results
            :warning: [Foobar.cs#L1](https://github.com/markusroessler/GithubDotnetResultsExporter/blob/develop/project/Foobar.cs#L1)  
            Warning Message  

            :warning: 
            Warning without location  

            :x: [Blub.cs#L1](https://github.com/markusroessler/GithubDotnetResultsExporter/blob/develop/project/Blub.cs#L1)  
            Error Message  


            """));
    }

}
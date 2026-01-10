using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GithubDotnetResultsExporter.IntegrationTest;

[Ignore("enable to generate SampleTestResults.trx")]
public class SampleTests
{
    [Test]
    public void Test_Pass()
    {
        Assert.Pass();
    }

    [Test]
    public void Test_Fail()
    {
        Assert.Fail("foobar");
    }

    [Test]
    [Ignore("foobar")]
    public void Test_Skipped()
    {
        Assert.Fail("foobar");
    }

    [Test]
    [Platform(Exclude = "Win")]
    public void Test_SkippedOnPlatform()
    {
        Assert.Fail("foobar");
    }

    [Test]
    public void Test_SkippedUsingAssume()
    {
        Assume.That(false);
    }

    [Test]
    [CancelAfter(100)]
    public void Test_Timeout(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            Thread.Sleep(10);
        }
        cancellationToken.ThrowIfCancellationRequested();
    }

    [Test]
    public void Test_StdOut()
    {
        Console.WriteLine("<b>Hello from Test_StdOut</b>");
        Console.Error.WriteLine("<b>Error from Test_StdOut</b>");
    }
}

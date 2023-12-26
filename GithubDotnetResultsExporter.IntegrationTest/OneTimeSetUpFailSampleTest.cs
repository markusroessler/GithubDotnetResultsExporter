using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GithubDotnetResultsExporter.IntegrationTest;

[Ignore("enable to generate OneTimeSetUpFailSampleTestResults.trx")]
public class OneTimeSetUpFailSampleTest
{
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        Assert.Fail("OneTimeSetUp failed");
    }

    [Test]
    public void Test_Pass()
    {
        Assert.Pass();
    }
}

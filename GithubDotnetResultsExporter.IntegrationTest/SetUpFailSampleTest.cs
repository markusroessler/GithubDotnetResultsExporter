using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GithubDotnetResultsExporter.IntegrationTest;

[Ignore("enable to generate SetUpFailSampleTestResults.trx")]
public class SetUpFailSampleTest
{
    [SetUp]
    public void SetUp()
    {
        Assert.Fail("SetUp failed");
    }

    [Test]
    public void Test_Pass()
    {
        Assert.Pass();
    }

    [Test]
    public void Test_Pass_2()
    {
        Assert.Pass();
    }
}

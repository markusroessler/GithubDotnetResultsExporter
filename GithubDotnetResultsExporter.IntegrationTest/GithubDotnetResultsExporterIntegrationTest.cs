namespace GithubDotnetResultsExporter.IntegrationTest;

using GithubDotnetResultsExporter.Csl;
using GithubDotnetResultsExporter.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

public class GithubDotnetResultsExporterIntegrationTest
{

    [Test]
    public void Test_ExportResults()
    {
        var slnDir = Path.Combine(Environment.CurrentDirectory, "TestSln");
        if (Directory.Exists(slnDir))
            Directory.Delete(slnDir, recursive: true);

        var testResultsDir = Path.Combine(slnDir, "TestResults");
        Directory.CreateDirectory(testResultsDir);

        {
            var assembly = typeof(GithubDotnetResultsExporterIntegrationTest).Assembly;
            using var testResultsStream = assembly.GetManifestResourceStream("GithubDotnetResultsExporter.IntegrationTest.SampleTestResults.trx")!;
            using var outputStream = File.OpenWrite(Path.Combine(testResultsDir, "TestResults.trx"));
            testResultsStream.CopyTo(outputStream);
        }

        var args = new string[]
        {
            "--github-server-url", "https://github.com",
            "--github-repo", "markusroessler/GithubDotnetResultsExporter",
            "--github-ref-name", "develop",
            "--export-step-summary", "true"
        };

        var environment = new TestEnvironment();

        Program.ExportResults(args, services =>
        {
            services.Replace(ServiceDescriptor.Singleton<IEnvironment>(environment));
        });

        var summaryText = File.ReadAllText(environment.GithubStepSummaryFile);

        Assert.That(summaryText.Replace("\r\n", "\n"), Is.EqualTo("""
        ## Build Results
        ## Test Results
        passed: 2  
        failed: 2  
        skipped: 3
        
        <details><summary>:x: Test_Timeout</summary>

        **Error**  
        ```
        Test exceeded Timeout value of 100ms

        ```

        </details>
        <details><summary>:x: Test_Fail</summary>

        **Error**  
        ```
        foobar
           at GithubDotnetResultsExporter.IntegrationTest.GithubDotnetResultsExporterIntegrationTest.Test_Fail() in D:\Entwicklung\DotNet\GithubDotnetResultsExporter\GithubDotnetResultsExporter.IntegrationTest\GithubDotnetResultsExporterIntegrationTest.cs:line 140

        1)    at GithubDotnetResultsExporter.IntegrationTest.GithubDotnetResultsExporterIntegrationTest.Test_Fail() in D:\Entwicklung\DotNet\GithubDotnetResultsExporter\GithubDotnetResultsExporter.IntegrationTest\GithubDotnetResultsExporterIntegrationTest.cs:line 140


        ```

        </details>
        <details><summary>:zzz: Test_SkippedUsingAssume</summary>

        **Error**  
        ```
          Expected: True
          But was:  False

        
        ```
        **StdOut**  
        ```
        Expected: True
          But was:  False
        ```

        </details>
        <details><summary>:zzz: Test_SkippedOnPlatform</summary>

        **Error**  
        ```
        Not supported on Win

        ```
        **StdOut**  
        ```
        Not supported on Win
        ```

        </details>
        <details><summary>:zzz: Test_Skipped</summary>

        **Error**  
        ```
        foobar

        ```
        **StdOut**  
        ```
        foobar
        ```

        </details>
        <details><summary>:heavy_check_mark: Test_Pass</summary>


        </details>
        <details><summary>:heavy_check_mark: Test_StdOut</summary>
        
        **StdOut**  
        ```
        <b>Hello from Test_StdOut</b>
        ```

        </details>

        """.Replace("\r\n", "\n")));
    }

    // [Test]
    // public void Test_Pass()
    // {
    //     Assert.Pass();
    // }

    // [Test]
    // public void Test_Fail()
    // {
    //     Assert.Fail("foobar");
    // }

    // [Test]
    // [Ignore("foobar")]
    // public void Test_Skipped()
    // {
    //     Assert.Fail("foobar");
    // }

    // [Test]
    // [Platform(Exclude = "Win")]
    // public void Test_SkippedOnPlatform()
    // {
    //     Assert.Fail("foobar");
    // }

    // [Test]
    // public void Test_SkippedUsingAssume()
    // {
    //     Assume.That(false);
    // }

    // [Test]
    // [Timeout(100)]
    // public void Test_Timeout()
    // {
    //     Thread.Sleep(1000);
    // }

    // [Test]
    // public void Test_StdOut()
    // {
    //     Console.WriteLine("<b>Hello from Test_StdOut</b>");
    // }
}
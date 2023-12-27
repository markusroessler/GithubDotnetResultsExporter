namespace GithubDotnetResultsExporter.IntegrationTest;

using GithubDotnetResultsExporter.Csl;
using GithubDotnetResultsExporter.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

public class GithubDotnetResultsExporterIntegrationTest
{

    private string _tempDir;
    private string _slnDir;
    private string _testResultsDir;
    private TestEnvironment _environment;

    [SetUp]
    public void Setup()
    {
        _tempDir = Path.Combine(Environment.CurrentDirectory, "test-temp", TestContext.CurrentContext.Test.MethodName);
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
        Directory.CreateDirectory(_tempDir);

        _slnDir = Path.Combine(_tempDir, "TestSln");
        if (Directory.Exists(_slnDir))
            Directory.Delete(_slnDir, recursive: true);

        _testResultsDir = Path.Combine(_slnDir, "TestResults");
        Directory.CreateDirectory(_testResultsDir);

        _environment = new TestEnvironment
        {
            CurrentDirectory = _slnDir
        };
    }

    [Test]
    public void Test_ExportResults()
    {
        CopyBuildResultsSarifToSlnDir("GithubDotnetResultsExporter.IntegrationTest.sample-compiler-diagnostics.sarif");
        CopyTestResultsTrxToTestDir("GithubDotnetResultsExporter.IntegrationTest.SampleTestResults.trx");

        var args = new string[]
        {
            "--github-server-url", "https://github.com",
            "--github-repo", "markusroessler/GithubDotnetResultsExporter",
            "--github-ref-name", "develop",
            "--export-step-summary", "true"
        };

        Program.ExportResults(args, services =>
        {
            services.Replace(ServiceDescriptor.Singleton<IEnvironment>(_environment));
        });

        var summaryText = File.ReadAllText(_environment.GithubStepSummaryFile);
        // Console.WriteLine(summaryText);

        Assert.That(summaryText.Replace("\r\n", "\n"), Is.EqualTo("""
        ## Build Results
        |||
        |:---|---:|
        | Errors | 1 |
        | Warnings | 2 |
        | Notes | 3 |

        :x: [FileProvider.cs#L20](https://github.com/markusroessler/GithubDotnetResultsExporter/blob/develop/GithubDotnetResultsExporter.Model/FileProvider.cs#L20)  
        Blabla failure ([CS8618](https://www.google.com/search?q=CS8618))  

        :warning: [FileProvider.cs#L20](https://github.com/markusroessler/GithubDotnetResultsExporter/blob/develop/GithubDotnetResultsExporter.Model/FileProvider.cs#L20)  
        Non-nullable field '_foobar' must contain a non-null value when exiting constructor. Consider declaring the field as nullable. ([CS8618](https://www.google.com/search?q=CS8618))  

        :warning: [FileProvider.cs#L18](https://github.com/markusroessler/GithubDotnetResultsExporter/blob/develop/GithubDotnetResultsExporter.Model/FileProvider.cs#L18)  
        The field 'FileProvider._foobar' is never used ([CS0169](https://www.google.com/search?q=CS0169))  

        🛈 [FileProvider.cs#L25](https://github.com/markusroessler/GithubDotnetResultsExporter/blob/develop/GithubDotnetResultsExporter.Model/FileProvider.cs#L25)  
        Member 'EnumerateSarifFiles' does not access instance data and can be marked as static ([CA1822](https://www.google.com/search?q=CA1822))  

        🛈 [FileProvider.cs#L30](https://github.com/markusroessler/GithubDotnetResultsExporter/blob/develop/GithubDotnetResultsExporter.Model/FileProvider.cs#L30)  
        Member 'EnumerateTrxFiles' does not access instance data and can be marked as static ([CA1822](https://www.google.com/search?q=CA1822))  

        🛈 [FileProvider.cs#L35](https://github.com/markusroessler/GithubDotnetResultsExporter/blob/develop/GithubDotnetResultsExporter.Model/FileProvider.cs#L35)  
        Member 'AppendTextToFile' does not access instance data and can be marked as static ([CA1822](https://www.google.com/search?q=CA1822))  

        ## Test Results
        |||
        |:---|---:|
        | Failed | 2 |
        | Skipped | 3 |
        | Passed | 2 |
        
        <details><summary>:x: GithubDotnetResultsExporter.IntegrationTest.GithubDotnetResultsExporterIntegrationTest.Test_Fail</summary>

        **Error**  
        ```
        foobar
           at GithubDotnetResultsExporter.IntegrationTest.GithubDotnetResultsExporterIntegrationTest.Test_Fail() in D:\Entwicklung\DotNet\GithubDotnetResultsExporter\GithubDotnetResultsExporter.IntegrationTest\GithubDotnetResultsExporterIntegrationTest.cs:line 141

        1)    at GithubDotnetResultsExporter.IntegrationTest.GithubDotnetResultsExporterIntegrationTest.Test_Fail() in D:\Entwicklung\DotNet\GithubDotnetResultsExporter\GithubDotnetResultsExporter.IntegrationTest\GithubDotnetResultsExporterIntegrationTest.cs:line 141


        ```

        </details>
        <details><summary>:x: GithubDotnetResultsExporter.IntegrationTest.GithubDotnetResultsExporterIntegrationTest.Test_Timeout</summary>

        **Error**  
        ```
        Test exceeded Timeout value of 100ms

        ```

        </details>
        <details><summary>:zzz: GithubDotnetResultsExporter.IntegrationTest.GithubDotnetResultsExporterIntegrationTest.Test_Skipped</summary>

        **Error**  
        ```
        foobar

        ```
        **StdOut**  
        ```
        foobar
        ```

        </details>
        <details><summary>:zzz: GithubDotnetResultsExporter.IntegrationTest.GithubDotnetResultsExporterIntegrationTest.Test_SkippedOnPlatform</summary>

        **Error**  
        ```
        Not supported on Win

        ```
        **StdOut**  
        ```
        Not supported on Win
        ```

        </details>
        <details><summary>:zzz: GithubDotnetResultsExporter.IntegrationTest.GithubDotnetResultsExporterIntegrationTest.Test_SkippedUsingAssume</summary>

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
        <details><summary>:heavy_check_mark: GithubDotnetResultsExporter.IntegrationTest.GithubDotnetResultsExporterIntegrationTest.Test_Pass</summary>


        </details>
        <details><summary>:heavy_check_mark: GithubDotnetResultsExporter.IntegrationTest.GithubDotnetResultsExporterIntegrationTest.Test_StdOut</summary>

        **StdOut**  
        ```
        <b>Hello from Test_StdOut</b>
        ```
        **StdErr**  
        ```
        <b>Error from Test_StdOut</b>
        ```

        </details>

        """.Replace("\r\n", "\n")));
    }

    [Test]
    public void Test_ExportResults_OnetimeSetupFail()
    {
        CopyTestResultsTrxToTestDir("GithubDotnetResultsExporter.IntegrationTest.OneTimeSetUpFailSampleTestResults.trx");

        var args = new string[]
        {
            "--github-server-url", "https://github.com",
            "--github-repo", "markusroessler/GithubDotnetResultsExporter",
            "--github-ref-name", "develop",
            "--export-step-summary", "true"
        };

        Program.ExportResults(args, services =>
        {
            services.Replace(ServiceDescriptor.Singleton<IEnvironment>(_environment));
        });

        var summaryText = File.ReadAllText(_environment.GithubStepSummaryFile);

        Assert.That(summaryText.Replace("\r\n", "\n"), Is.EqualTo("""
        ## Build Results
        |||
        |:---|---:|
        | Errors | 0 |
        | Warnings | 0 |
        | Notes | 0 |

        ## Test Results
        |||
        |:---|---:|
        | Failed | 1 |
        | Skipped | 0 |
        | Passed | 0 |
        
        <details><summary>:x: GithubDotnetResultsExporter.IntegrationTest.SetupFailSampleTest.Test_Pass</summary>

        **Error**  
        ```
        OneTimeSetUp: OneTimeSetUp failed
           at GithubDotnetResultsExporter.IntegrationTest.SetupFailSampleTest.OneTimeSetUp() in D:\Entwicklung\DotNet\GithubDotnetResultsExporter\GithubDotnetResultsExporter.IntegrationTest\SetupFailSampleTest.cs:line 13


        ```

        </details>

        """.Replace("\r\n", "\n")));
    }

    [Test]
    public void Test_ExportResults_SetupFail()
    {
        CopyTestResultsTrxToTestDir("GithubDotnetResultsExporter.IntegrationTest.SetUpFailSampleTestResults.trx");

        var args = new string[]
        {
            "--github-server-url", "https://github.com",
            "--github-repo", "markusroessler/GithubDotnetResultsExporter",
            "--github-ref-name", "develop",
            "--export-step-summary", "true"
        };

        Program.ExportResults(args, services =>
        {
            services.Replace(ServiceDescriptor.Singleton<IEnvironment>(_environment));
        });

        var summaryText = File.ReadAllText(_environment.GithubStepSummaryFile);
        // Console.WriteLine(summaryText);

        // note: don't know why the stacktrace of the second test method differs from the first one ("at InvokeStub_SetUpFailSampleTest")
        Assert.That(summaryText.Replace("\r\n", "\n"), Is.EqualTo("""
        ## Build Results
        |||
        |:---|---:|
        | Errors | 0 |
        | Warnings | 0 |
        | Notes | 0 |

        ## Test Results
        |||
        |:---|---:|
        | Failed | 2 |
        | Skipped | 0 |
        | Passed | 0 |
        
        <details><summary>:x: GithubDotnetResultsExporter.IntegrationTest.SetUpFailSampleTest.Test_Pass</summary>

        **Error**  
        ```
        SetUp failed
           at GithubDotnetResultsExporter.IntegrationTest.SetUpFailSampleTest.SetUp() in D:\Entwicklung\DotNet\GithubDotnetResultsExporter\GithubDotnetResultsExporter.IntegrationTest\SetUpFailSampleTest.cs:line 14

        1)    at GithubDotnetResultsExporter.IntegrationTest.SetUpFailSampleTest.SetUp() in D:\Entwicklung\DotNet\GithubDotnetResultsExporter\GithubDotnetResultsExporter.IntegrationTest\SetUpFailSampleTest.cs:line 14


        ```

        </details>
        <details><summary>:x: GithubDotnetResultsExporter.IntegrationTest.SetUpFailSampleTest.Test_Pass_2</summary>

        **Error**  
        ```
        SetUp failed
           at GithubDotnetResultsExporter.IntegrationTest.SetUpFailSampleTest.SetUp() in D:\Entwicklung\DotNet\GithubDotnetResultsExporter\GithubDotnetResultsExporter.IntegrationTest\SetUpFailSampleTest.cs:line 14
           at InvokeStub_SetUpFailSampleTest.SetUp(Object, Object, IntPtr*)

        1)    at GithubDotnetResultsExporter.IntegrationTest.SetUpFailSampleTest.SetUp() in D:\Entwicklung\DotNet\GithubDotnetResultsExporter\GithubDotnetResultsExporter.IntegrationTest\SetUpFailSampleTest.cs:line 14
           at InvokeStub_SetUpFailSampleTest.SetUp(Object, Object, IntPtr*)


        ```

        </details>

        """.Replace("\r\n", "\n")));
    }

    private void CopyTestResultsTrxToTestDir(string trxName)
    {
        var assembly = typeof(GithubDotnetResultsExporterIntegrationTest).Assembly;
        using var testResultsStream = assembly.GetManifestResourceStream(trxName);
        using var outputStream = File.OpenWrite(Path.Combine(_testResultsDir, "TestResults.trx"));
        testResultsStream.CopyTo(outputStream);
    }

    private void CopyBuildResultsSarifToSlnDir(string sarifName)
    {
        var assembly = typeof(GithubDotnetResultsExporterIntegrationTest).Assembly;
        using var reader = new StreamReader(assembly.GetManifestResourceStream(sarifName));
        var content = reader.ReadToEnd().Replace("{SLN_DIR}", _slnDir.Replace("\\", "/"));
        File.WriteAllText(Path.Combine(_slnDir, "compiler-diagnostics.sarif"), content);
    }
}
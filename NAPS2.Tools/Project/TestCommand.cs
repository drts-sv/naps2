using System.Runtime.InteropServices;

namespace NAPS2.Tools.Project;

public class TestCommand : ICommand<TestOptions>
{
    public int Run(TestOptions opts)
    {
        var arch = RuntimeInformation.OSArchitecture.ToString().ToLowerInvariant();
        var depsRootPath = OperatingSystem.IsMacOS()
            ? $"NAPS2.App.Mac/bin/Debug/net9-macos/osx-{arch}"
            : OperatingSystem.IsLinux()
                ? $"NAPS2.App.Gtk/bin/Debug/net9/linux-{arch}"
                : $"NAPS2.App.WinForms/bin/Debug/net9-windows/win-{arch}";
        var frameworkArg = OperatingSystem.IsWindows() ? "" : "-f net9";

        void RunTests(string project, bool isRetry = false)
        {
            try {

                Cli.Run("dotnet", $"test -l \"console;verbosity=normal\" {frameworkArg} {project}", new()
                {
                    { "NAPS2_TEST_DEPS", Path.Combine(Paths.SolutionRoot, depsRootPath) },
                    { "NAPS2_TEST_NOGUI", opts.NoGui ? "1" : "0" },
                    { "NAPS2_TEST_NONETWORK", opts.NoNetwork ? "1" : "0" }
                });
            }
            catch (Exception)
            {
                if (isRetry) throw;
                Output.Info("Tests failed, retrying once");
                RunTests(project, true);
            }
        }

        Output.Info("Running tests");
        RunTests("NAPS2.Sdk.Tests");
        RunTests("NAPS2.Lib.Tests");
        RunTests("NAPS2.App.Tests");
        Output.Info("Tests passed.");
        return 0;
    }
}
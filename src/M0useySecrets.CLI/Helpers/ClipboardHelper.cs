using System.Diagnostics;
using System.Runtime.InteropServices;

namespace M0useySecrets.CLI.Helpers;

/// <summary>
/// Provides methods for copying text to the system clipboard using platform-specific commands, with support for automatic clearing after a specified duration.
/// </summary>
public static class ClipboardHelper
{
    /// <summary>
    /// Copies the specified value to the system clipboard using platform-specific commands.
    /// </summary>
    /// <param name="value">The value to copy to the clipboard.</param>
    /// <exception cref="PlatformNotSupportedException">Thrown if the operating system is not supported.</exception>
    public static void CopyToClipboard(string value)
    {
        var (tool, args) = true switch
        {
            _ when RuntimeInformation.IsOSPlatform(OSPlatform.Windows) => ("clip", ""),
            _ when RuntimeInformation.IsOSPlatform(OSPlatform.OSX) => ("pbcopy", ""),
            _ when RuntimeInformation.IsOSPlatform(OSPlatform.Linux) => ("xclip", "-selection clipboard"),
            _ => throw new PlatformNotSupportedException("Unsupported OS")
        };

        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = tool,
                    Arguments = args,
                    RedirectStandardInput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            process.StandardInput.Write(value);
            process.StandardInput.Close();
            process.WaitForExit();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error copying to clipboard: {ex.Message}");
        }
    }

    /// <summary>
    /// Copies the specified value to the clipboard and automatically clears it after a specified number of seconds, unless the user presses a key to skip the clearing.
    /// </summary>
    /// <param name="value">The value to copy to the clipboard.</param>
    /// <param name="clearAfterSeconds">The number of seconds to wait before automatically clearing the clipboard.</param>
    public static void CopyWithAutoClear(string value, int clearAfterSeconds = 15)
    {
        CopyToClipboard(value);

        Console.WriteLine($"Copied! Clipboard will clear in {clearAfterSeconds}s. Press any key to skip...");

        int elapsed = 0;
        int totalMs = clearAfterSeconds * 1000;

        while (elapsed < totalMs)
        {
            if (Console.KeyAvailable)
            {
                Console.ReadKey(true);
                break;
            }
            Thread.Sleep(500);
            elapsed += 500;
        }

        CopyToClipboard("");
        Console.WriteLine("Clipboard cleared.");
    }
}

using M0useySecrets.Core.Models;

namespace M0useySecrets.CLI.Helpers;

/// <summary>
/// Provides methods for printing decrypted secrets and formatted messages to the console, with support for colored output to indicate status (e.g., success, error, warning) and tabular display of multiple secrets. 
/// This class is designed to enhance the user experience by making it easier to read and interpret the output of secret-related operations in the CLI application.
/// </summary>
public static class ConsoleOutput
{
    /// <summary>
    /// Prints the details of a single decrypted secret to the console in a formatted manner.
    /// </summary>
    /// <param name="secret">The decrypted secret to print.</param>
    public static void PrintSecret(DecryptedSecret secret)
    {
        // calculate label width for alignment (based on longest property name + padding)
        int labelWidth = typeof(DecryptedSecret).GetProperties().Max(p => p.Name.Length) + 2;

        // print each property with label and value, aligning labels to the right
        foreach (var (label, value) in new[]
        {
            ("Name", secret.Name),
            ("Namespace", secret.Namespace),
            ("Created", secret.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")),
            ("Expires", secret.ExpiresAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Never"),
            ("Status", secret.IsExpired ? "EXPIRED" : "Active"),
            ("Value", secret.Value ?? "[hidden]"),
            ("Notes", secret.Notes ?? "-")
        })
        {
            if (label == "Status" && value == "EXPIRED")
                PrintWarning($"{label.PadRight(labelWidth)}: {value}");
            else
                Console.WriteLine($"{label.PadRight(labelWidth)}: {value}");
            Console.ResetColor();
        }
    }

    /// <summary>
    /// Prints a list of decrypted secrets in a tabular format to the console. Expired secrets are highlighted in yellow.
    /// </summary>
    /// <param name="secrets">The list of decrypted secrets to print.</param>
    public static void PrintSecretTable(List<DecryptedSecret> secrets)
    {
        if (!secrets.Any())
        {
            PrintWarning("No secrets found.");
            return;
        }

        // calculate column widths
        var nameWidth = Math.Max("NAME".Length, secrets.Max(s => s.Name.Length)) + 2;
        var nsWidth = Math.Max("NAMESPACE".Length, secrets.Max(s => s.Namespace.Length)) + 2;
        var createdWidth = Math.Max("CREATED".Length, secrets.Max(s => s.CreatedAt.ToString("yyyy-MM-dd")?.Length ?? "Never".Length)) + 2;
        var expiresWidth = Math.Max("EXPIRES".Length, secrets.Max(s => s.ExpiresAt?.ToString("yyyy-MM-dd")?.Length ?? "Never".Length)) + 2;
        var statusWidth = "STATUS".Length + 2;
        var valueWidth = Math.Max("VALUE".Length, secrets.Max(s => s.Value?.Length ?? "[hidden]".Length)) + 2;
        var notesWidth = Math.Max("NOTES".Length, secrets.Max(s => s.Notes?.Length ?? "-".Length)) + 2;

        // header
        Console.WriteLine(
            $"{"NAME".PadRight(nameWidth)}" +
            $"{"NAMESPACE".PadRight(nsWidth)}" +
            $"{"CREATED".PadRight(createdWidth)}" +
            $"{"EXPIRES".PadRight(expiresWidth)}" +
            $"{"STATUS".PadRight(statusWidth)}" +
            $"{"VALUE".PadRight(valueWidth)}" +
            $"{"NOTES".PadRight(notesWidth)}");

        // rows
        int count = secrets.Count;
        for (int i = 0; i < count; i++)
        {
            string status = secrets[i].IsExpired ? "EXPIRED" : "Active";
            string row = $"{secrets[i].Name.PadRight(nameWidth)}" +
                         $"{secrets[i].Namespace.PadRight(nsWidth)}" +
                         $"{secrets[i].CreatedAt.ToString("yyyy-MM-dd").PadRight(createdWidth)}" +
                         $"{(secrets[i].ExpiresAt?.ToString("yyyy-MM-dd") ?? "Never").PadRight(expiresWidth)}" +
                         $"{status.PadRight(statusWidth)}" +
                         $"{(secrets[i].Value ?? "[hidden]").PadRight(valueWidth)}" +
                         $"{(secrets[i].Notes ?? "-").PadRight(notesWidth)}";

            if (secrets[i].IsExpired)
                PrintWarning(row);
            else
                Console.WriteLine(row);
        }
    }

    /// <summary>
    /// Prints a success message to the console in green color to indicate successful operations or positive outcomes.
    /// </summary>
    /// <param name="message">The success message to print.</param>
    public static void PrintSuccess(string message)
    {
        SetConsoleColor(ConsoleColor.Green, message);
    }

    /// <summary>
    /// Prints an error message to the console in red color to indicate failed operations or negative outcomes.
    /// </summary>
    /// <param name="message">The error message to print.</param>
    public static void PrintError(string message)
    {
        SetConsoleColor(ConsoleColor.Red, message);
    }

    /// <summary>
    /// Prints a warning message to the console in yellow color to indicate potential issues or cautionary information.
    /// </summary>
    /// <param name="message">The warning message to print.</param>
    public static void PrintWarning(string message)
    {
        SetConsoleColor(ConsoleColor.Yellow, message);
    }

    /// <summary>
    /// Sets the console text color, prints the provided message, and then resets the console color to its default. 
    /// This method is used internally by the PrintSuccess, PrintError, and PrintWarning methods to ensure consistent formatting of colored messages.
    /// </summary>
    /// <param name="color">The color to set for the console text.</param>
    /// <param name="message">The message to print to the console.</param>
    private static void SetConsoleColor(ConsoleColor color, string message)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ResetColor();
    }
}

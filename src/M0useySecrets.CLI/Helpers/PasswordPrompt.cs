namespace M0useySecrets.CLI.Helpers;

/// <summary>
/// Provides a method to securely read a password from the console without echoing the input, and with support for backspace handling.
/// </summary>
public static class PasswordPrompt
{
    /// <summary>
    /// Reads a password from the console without echoing the input.
    /// </summary>
    /// <param name="prompt">The prompt to display to the user.</param>
    /// <returns>The password entered by the user.</returns>
    public static string ReadPassword(string prompt = "Master password: ")
    {
        Console.Write(prompt);
        char[] buffer = new char[128]; // fixed-size buffer for password input
        int bufferIndex = 0; // index to track current position in buffer

        do
        {
            ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);
            if (keyInfo.Key == ConsoleKey.Enter)
            {
                break;
            }
            else if (keyInfo.Key == ConsoleKey.Backspace)
            {
                // handle backspace
                if (bufferIndex > 0)
                {
                    bufferIndex--;
                    Console.Write("\b \b"); // move back, write space to erase, move back again
                }
            }
            else
            {
                // append to buffer and print '*'
                if (bufferIndex < buffer.Length)
                {
                    buffer[bufferIndex++] = keyInfo.KeyChar;
                    Console.Write('*');
                }
            }
        } while (true);

        // move to next line after Enter
        Console.WriteLine();

        // return the collected string
        string password = new string(buffer, 0, bufferIndex);
        Array.Clear(buffer, 0, buffer.Length); // clear the buffer for security
        return password;
    }
}

Just calls dir command and colorizes the output
<img width="1920" height="1080" alt="snapshot" src="https://github.com/user-attachments/assets/6ead284f-beb0-487f-88ff-9726ace3660e" />


```
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

class Program
{
    static void CPrint(string text, ConsoleColor fg, ConsoleColor? bg = null)
    {
        var oldFg = Console.ForegroundColor;
        var oldBg = Console.BackgroundColor;

        Console.ForegroundColor = fg;
        if (bg.HasValue)
            Console.BackgroundColor = bg.Value;

        Console.WriteLine(text);

        Console.ForegroundColor = oldFg;
        Console.BackgroundColor = oldBg;
    }

    static string[] GetExecutableExtensions()
    {
        string pathext = Environment.GetEnvironmentVariable("PATHEXT") ?? "";
        return pathext.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                      .Select(e => e.Trim().ToLowerInvariant())
                      .ToArray();
    }

    static bool IsExecutableFile(string line, string[] exeExts)
    {
        // Ignore directory entries
        if (line.IndexOf("<DIR>", StringComparison.OrdinalIgnoreCase) >= 0)
            return false;

        string trimLine = line.Trim();
        if (string.IsNullOrEmpty(trimLine))
            return false;

        // Try to extract the "filename" part from dir output
        string[] parts = trimLine.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
            return false;

        string filename = parts[parts.Length - 1]; // last part is usually the name
        string ext = Path.GetExtension(filename).ToLowerInvariant();

        return exeExts.Contains(ext);
    }

    static void CmdList(string args)
    {
        string[] exeExts = GetExecutableExtensions();

        ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", "/c dir /o " + args)
        {
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (Process proc = Process.Start(psi))
        using (var reader = proc.StandardOutput)
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                string trimLine = line.Trim();

                if (line.IndexOf("<DIR>", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    CPrint(line, ConsoleColor.Yellow);
                }
                else if (line.IndexOf("<SYMLINK", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    CPrint(line, ConsoleColor.Green);
                }
                else if (line.IndexOf(":$DATA", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    CPrint(line, ConsoleColor.Red);
                }
                else if (trimLine.StartsWith("volume", StringComparison.OrdinalIgnoreCase))
                {
                    CPrint(line, ConsoleColor.Gray);
                }
                else if (trimLine.StartsWith("directory of", StringComparison.OrdinalIgnoreCase))
                {
                    CPrint(line, ConsoleColor.Green);
                }
                else if (IsExecutableFile(line, exeExts))
                {
                    CPrint(line, ConsoleColor.Cyan);
                }
                else if (line.StartsWith("   "))
                {
                    CPrint(line, ConsoleColor.Gray);
                }
                else if (string.IsNullOrWhiteSpace(line))
                {
                    CPrint(line, ConsoleColor.Gray);
                }
                else
                {
                    CPrint(line, ConsoleColor.White);
                }
            }
        }
    }

    static void Main(string[] args)
    {
        string arguments = args.Length > 0 ? string.Join(" ", args) : "";
        CmdList(arguments);
    }
}

using System;
using System.Diagnostics;

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

    static void CmdList(string args)
    {
        ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", "/c dir " + args)
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
                string lowerLine = trimLine.ToLowerInvariant();

                if (line.Contains("<DIR>"))
                {
                    CPrint(line, ConsoleColor.Yellow);
                }
                else if (line.Contains("<SYMLINK"))
                {
                    CPrint(line, ConsoleColor.Green);
                }
                else if (line.Contains(":$DATA"))
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

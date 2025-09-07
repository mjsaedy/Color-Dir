using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

class Program
{
    static bool UseAnsi = false;

    // Enable ANSI escape sequences on Windows 10+
    static void EnableAnsi()
    {
        const int STD_OUTPUT_HANDLE = -11;
        const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;

        IntPtr handle = GetStdHandle(STD_OUTPUT_HANDLE);
        if (GetConsoleMode(handle, out uint mode))
        {
            mode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING;
            UseAnsi = SetConsoleMode(handle, mode);
        }
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr GetStdHandle(int nStdHandle);

    [DllImport("kernel32.dll")]
    static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

    [DllImport("kernel32.dll")]
    static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

    static void CPrint(string text, ConsoleColor fg, ConsoleColor? bg = null)
    {
        if (UseAnsi)
        {
            string fgAnsi = fg switch
            {
                ConsoleColor.Black => "0;0;0",
                ConsoleColor.DarkBlue => "0;0;139",
                ConsoleColor.DarkGreen => "0;100;0",
                ConsoleColor.DarkCyan => "0;139;139",
                ConsoleColor.DarkRed => "139;0;0",
                ConsoleColor.DarkMagenta => "139;0;139",
                ConsoleColor.DarkYellow => "184;134;11",
                ConsoleColor.Gray => "192;192;192",
                ConsoleColor.DarkGray => "128;128;128",
                ConsoleColor.Blue => "0;0;255",
                ConsoleColor.Green => "0;255;0",      // vibrant green
                ConsoleColor.Cyan => "0;255;255",     // bright cyan
                ConsoleColor.Red => "255;0;0",
                ConsoleColor.Magenta => "255;0;255",
                ConsoleColor.Yellow => "255;255;0",
                ConsoleColor.White => "255;255;255",
                _ => "255;255;255"
            };

            string bgAnsi = bg.HasValue ? (bg.Value switch
            {
                ConsoleColor.Black => "0;0;0",
                ConsoleColor.DarkBlue => "0;0;139",
                ConsoleColor.DarkGreen => "0;100;0",
                ConsoleColor.DarkCyan => "0;139;139",
                ConsoleColor.DarkRed => "139;0;0",
                ConsoleColor.DarkMagenta => "139;0;139",
                ConsoleColor.DarkYellow => "184;134;11",
                ConsoleColor.Gray => "192;192;192",
                ConsoleColor.DarkGray => "128;128;128",
                ConsoleColor.Blue => "0;0;255",
                ConsoleColor.Green => "0;255;0",
                ConsoleColor.Cyan => "0;255;255",
                ConsoleColor.Red => "255;0;0",
                ConsoleColor.Magenta => "255;0;255",
                ConsoleColor.Yellow => "255;255;0",
                ConsoleColor.White => "255;255;255",
                _ => "0;0;0"
            }) : null;

            string seq = $"\x1b[38;2;{fgAnsi}m";
            if (bgAnsi != null)
                seq += $"\x1b[48;2;{bgAnsi}m";

            Console.WriteLine($"{seq}{text}\x1b[0m");
        }
        else
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
        if (line.IndexOf("<DIR>", StringComparison.OrdinalIgnoreCase) >= 0)
            return false;

        string trimLine = line.Trim();
        if (string.IsNullOrEmpty(trimLine))
            return false;

        string[] parts = trimLine.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
            return false;

        string filename = parts[parts.Length - 1];
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
        EnableAnsi(); // enable ANSI sequences
        string arguments = args.Length > 0 ? string.Join(" ", args) : "";
        CmdList(arguments);
    }
}

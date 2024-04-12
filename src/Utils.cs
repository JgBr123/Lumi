using System.Security.Cryptography;

namespace Lumi
{
    class Utils
    {
        public const int GIGABYTE = 1000000000;
        public static Dictionary<string, ConsoleColor[]> paletteDict = new Dictionary<string, ConsoleColor[]>()
        {
            { "0", new ConsoleColor[] { ConsoleColor.Black, ConsoleColor.White, ConsoleColor.Yellow } },
            { "1", new ConsoleColor[] { ConsoleColor.White, ConsoleColor.Black, ConsoleColor.Black } },
            { "2", new ConsoleColor[] { ConsoleColor.Black, ConsoleColor.Magenta, ConsoleColor.DarkMagenta } },
            { "3", new ConsoleColor[] { ConsoleColor.Black, ConsoleColor.Green, ConsoleColor.DarkGreen } },
            { "4", new ConsoleColor[] { ConsoleColor.Black, ConsoleColor.Cyan, ConsoleColor.DarkCyan } }
        };
        public static void Print(string text)
        {
            var colors = paletteDict[GetPalette()];
            ConsoleColor background_color = colors[0];
            ConsoleColor text_color = colors[1];
            ConsoleColor extra_color = colors[2];

            Console.ResetColor();
            Console.BackgroundColor = background_color;

            bool usingExtraColor = false;
            foreach (char chr in text)
            {
                if (chr == '*') usingExtraColor = !usingExtraColor; //Toggling extra color mode
                else if (chr == '\n') //Do not style breakline
                {
                    Console.ResetColor();
                    Console.Write('\n');
                    Console.BackgroundColor = background_color;
                }
                else //Styling
                {
                    if (usingExtraColor) Console.ForegroundColor = extra_color;
                    else Console.ForegroundColor = text_color;
                    Console.Write(chr);
                }
            }
            Console.ResetColor();
        }
        public static void PrintLine(string text)
        {
            Print(text + "\n");
        }
        public static (string[] args, string[] options) ParseArgs(string[] args, out string[] invalidOptions)
        {
            var parsedArgs = new List<string>();
            var options = new List<string>();
            var _invalidOptions = new List<string>();

            foreach (string arg in args)
            {
                if (arg.StartsWith("--") || arg.StartsWith("-"))
                {
                    if (arg == "--force" || arg == "-f") options.Add("force");
                    else if (arg == "--keep" || arg == "-k") options.Add("keep");
                    switch (arg)
                    {
                        case "--force":
                        case "-f":
                            options.Add("force");
                            break;

                        case "--keep":
                        case "-k":
                            options.Add("keep");
                            break;

                        case "--compress":
                        case "-c":
                            options.Add("compress");
                            break;

                        default:
                            _invalidOptions.Add(arg);
                            break;
                    }
                }
                else
                {
                    parsedArgs.Add(arg);
                }
            }

            invalidOptions = _invalidOptions.ToArray();
            return (parsedArgs.ToArray(), options.ToArray());
        }
        public static string ReadLine()
        {
            var colors = paletteDict[GetPalette()];
            ConsoleColor background_color = colors[0];
            ConsoleColor extra_color = colors[2];

            Console.ResetColor();
            Console.BackgroundColor = background_color;
            Console.ForegroundColor = extra_color;

            var input = Console.ReadLine();
            Console.ResetColor();
            return input ?? "";
        }
        public static string Hash(byte[] data)
        {
            return BytesToString(SHA256.HashData(data));
        }
        public static string HashStream(Stream stream)
        {
            return BytesToString(SHA256.HashData(stream));
        }
        public static string HashFile(string path)
        {
            using (var stream = File.Open(path, FileMode.Open))
            {
                return HashStream(stream);
            }
        }
        public static void CreateEmptyFile(string path)
        {
            File.Create(path).Dispose();
        }
        public static bool CanCreateFile(string path)
        {
            try
            {
                CreateEmptyFile(path);
                File.Delete(path);
                return true;
            }
            catch { return false; }
        }
        public static bool IsValidFilename(string filename)
        {
            return !string.IsNullOrEmpty(filename) && filename.IndexOfAny(Path.GetInvalidFileNameChars()) < 0 && !filename.StartsWith(".");
        }
        public static string RemoveQuotes(string arg)
        {
            return arg.Replace("\"", "").Replace("'", "");
        }
        public static string[] GetFiles(string path)
        {
            var files = new List<string>();
            foreach (var file in Directory.GetFiles(path, "*", SearchOption.AllDirectories).ToList())
            {
                if (file.StartsWith(".\\.lumi\\")) continue; //Ignore files inside lumi folder
                files.Add(file);
            }
            return files.ToArray();
        }
        public static string[] GetDirectories(string path)
        {
            var directories = new List<string>();
            foreach (var directory in Directory.GetDirectories(path, "*", SearchOption.AllDirectories).ToList())
            {
                if (directory.StartsWith(".\\.lumi")) continue; //Ignore folders inside lumi folder
                directories.Add(directory);
            }
            return directories.ToArray();
        }
        public static void WipeRepository()
        {
            Directory.Delete(".lumi\\", true);
        }
        public static void WipeCurrentFiles()
        {
            foreach (string directory in Utils.GetDirectories("."))
            {
                if (Directory.Exists(directory)) Directory.Delete(directory, true);
            }
            foreach (string file in Utils.GetFiles("."))
            {
                File.Delete(file);
            }
        }
        public static bool IsInitialized()
        {
            if (Directory.Exists(".lumi\\") && Directory.Exists(".lumi\\packages") && File.Exists(".lumi\\filebase") && File.Exists(".lumi\\hashes")) return true;
            else return false;
        }
        public static bool ConfirmationPrompt()
        {
            switch (Utils.ReadLine().ToLower())
            {
                case "y":
                    return false;
                default:
                    Utils.Print("The operation was cancelled by the user.");
                    return true;
            }
        }
        public static string GetPalette()
        {
            string palettePath = ".lumi\\palette";
            if (File.Exists(palettePath))
            {
                var data = File.ReadAllText(palettePath);
                if (IsValidPalette(data)) return data;
                else return "0";
            }
            else return "0";
        }
        public static void SetPalette(string palette)
        {
            string palettePath = ".lumi\\palette";
            if (palette == "0") File.Delete(palettePath); //Deletes palette file if its 0 (default)
            else File.WriteAllText(palettePath, palette);
        }
        public static bool IsValidPalette(string palette)
        {
            return paletteDict.ContainsKey(palette);
        }
        public static int GetDigits(int number)
        {
            if (number == 0) return 0;
            else return (int)Math.Floor(Math.Log10(Math.Abs(number)) + 1);
        }
        public static string IndentString(string str, int targetIndentation, bool reverseIndentation = false)
        {
            string indentedString = str;
            while (indentedString.Length < targetIndentation)
            {
                if (reverseIndentation) indentedString = " " + indentedString;
                else indentedString += " ";
            }
            return indentedString;
        }
        public static FileInfo? GetPackageByName(DirectoryInfo packagesDirectory, string name)
        {
            string packagePath = Path.Combine(packagesDirectory.FullName, name);
            if (File.Exists(packagePath)) //If specific package exists, return it
            {
                return new FileInfo(packagePath);
            }
            else //Else, tries to get the package by id
            {
                if (int.TryParse(name, out int id))
                {
                    var packages = packagesDirectory.GetFiles("*.pkg").OrderBy(f => f.LastWriteTime);
                    return packages.ElementAtOrDefault(id);
                }
                else return default;
            }
        }
        public static long GetDataLeftToRead(Stream stream)
        {
            return stream.Length - stream.Position;
        }
        public static string BytesToString(byte[] data)
        {
            return Convert.ToBase64String(data);
        }
        public static byte[] StringToBytes(string data)
        {
            return Convert.FromBase64String(data);
        }
    }
}

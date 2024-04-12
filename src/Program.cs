using System.IO.Compression;
using System.Text;

namespace Lumi
{
    class Program
    {
        const string version = "LUMI v1.0.0 .NET 8.0";

        //
        // MAIN
        //
        static void Main(string[] args)
        {
            Console.CancelKeyPress += new ConsoleCancelEventHandler(Handlers.CancelHandler);

            Console.Title = "Lumi";
            Console.OutputEncoding = Encoding.UTF8;
            var parsedArgs = Utils.ParseArgs(args, out string[] invalidOptions);
            if (Checks.InvalidOptionsCheck(invalidOptions)) return;

            if (parsedArgs.args.Length > 0)
            {
                switch (parsedArgs.args[0])
                {
                    case "init":
                        Init();
                        break;
                    case "version":
                        Utils.PrintLine(version);
                        break;
                    case "save":
                        Save(parsedArgs);
                        break;
                    case "load":
                        Load(parsedArgs);
                        break;
                    case "list":
                        List();
                        break;
                    case "history":
                        History(parsedArgs);
                        break;
                    case "import":
                        Import(parsedArgs);
                        break;
                    case "export":
                        Export(parsedArgs);
                        break;
                    case "wipe":
                        Wipe(parsedArgs);
                        break;
                    case "palette":
                        Palette(parsedArgs);
                        break;
                    case "delete":
                        Delete(parsedArgs);
                        break;
                    case "cleanup":
                        Cleanup();
                        break;
                    case "tree":
                        Tree();
                        break;
                    default:
                        Utils.PrintLine("Invalid LUMI command.");
                        break;
                }
            }
            else Utils.PrintLine("Please enter a LUMI command.");
        }

        //
        // INIT
        //
        static void Init()
        {
            try
            {
                if (Utils.IsInitialized())
                {
                    Utils.PrintLine("The repository is already initialized.");
                    return;
                }

                DirectoryInfo rep = Directory.CreateDirectory(".lumi");
                rep.Attributes = FileAttributes.Directory | FileAttributes.Hidden;

                Directory.CreateDirectory(".lumi\\packages");
                Utils.CreateEmptyFile(".lumi\\filebase");
                Utils.CreateEmptyFile(".lumi\\hashes");

                Utils.PrintLine($"Initialized new LUMI repository at *{rep.FullName}*.");
            }
            catch (Exception ex)
            {
                Utils.PrintLine($"Couldn't initialize LUMI repository *[{ex.Message}]*.");
            }
        }

        //
        // SAVE
        //
        static void Save((string[] args, string[] options) parsedArgs)
        {
            try
            {
                var args = parsedArgs.args;
                if (Checks.InitializedCheck()) return;
                if (Checks.FilenameCheck_Save(args)) return;

                LoadingAnimation.Start("Saving...");

                //Reads hashes file
                var writtenHashes = new List<string>();
                using (var stream = File.Open(".lumi\\hashes", FileMode.Open))
                {
                    using (var reader = new BinaryReader(stream, Encoding.UTF8, false))
                    {
                        while (reader.BaseStream.Position != reader.BaseStream.Length) //Reads till the end of file
                        {
                            writtenHashes.Add(reader.ReadString());
                        }
                    }
                }

                //Creates a map of files and its corresponding hashes
                var files_hashes = new Dictionary<string, string>();

                //Create writers for filebase and hashes 
                {
                    using var filebase_streamWriter = File.Open(".lumi\\filebase", FileMode.Append);
                    using var filebase_writer = new BinaryWriter(filebase_streamWriter, Encoding.UTF8, false);

                    using var hashes_streamWriter = File.Open(".lumi\\hashes", FileMode.Append);
                    using var hashes_writer = new BinaryWriter(hashes_streamWriter, Encoding.UTF8, false);

                    //Hash files and write them to filebase if needed
                    foreach (string file in Utils.GetFiles("."))
                    {
                        using (var fileStream = File.Open(file, FileMode.Open))
                        {
                            var hash = Utils.HashStream(fileStream);
                            files_hashes.Add(file, hash);

                            if (!writtenHashes.Contains(hash)) //Checks if hash was already written to filebase
                            {
                                fileStream.Seek(0, SeekOrigin.Begin); //Goes to the start of the stream before doing anything
                                long fileLength = fileStream.Length;

                                //Header of data (adds the hashes to filebase and hash file)
                                hashes_writer.Write(hash);
                                writtenHashes.Add(hash);
                                filebase_writer.Write(hash);

                                if (fileLength > 2 * Utils.GIGABYTE)
                                {
                                    //Saving files bigger than 2gb (now will append data to the header)
                                    while (fileStream.Position != fileStream.Length) //Reads till the end of file
                                    {
                                        var dataLeftToRead = Utils.GetDataLeftToRead(fileStream);

                                        byte[] dataBuffer;
                                        if (dataLeftToRead > 2 * Utils.GIGABYTE) dataBuffer = new byte[2 * Utils.GIGABYTE];
                                        else dataBuffer = new byte[dataLeftToRead];

                                        //Adds data
                                        fileStream.Read(dataBuffer, 0, dataBuffer.Length);
                                        filebase_writer.Write(dataBuffer.Length);
                                        filebase_writer.Write(dataBuffer);

                                        //Check if there is more data to write, if yes then set bool to true, otherwise set bool to false
                                        dataLeftToRead = Utils.GetDataLeftToRead(fileStream);
                                        if (dataLeftToRead != 0) filebase_writer.Write(true);
                                        else filebase_writer.Write(false);
                                    }
                                }
                                else
                                {
                                    //Saving files less than 2gb (now will append data to the header)
                                    byte[] data = new byte[(int)fileLength];
                                    fileStream.Read(data, 0, (int)fileLength);
                                    //Adds data
                                    filebase_writer.Write(data.Length);
                                    filebase_writer.Write(data);
                                    filebase_writer.Write(false);
                                }
                            }
                        }
                    }
                }

                //Assign package file location
                var packagePath = $".lumi\\packages\\{args[1]}\\{Guid.NewGuid()}.pkg";
                Directory.CreateDirectory(Path.GetDirectoryName(packagePath)!); //Makes sure the folder exists

                //Create package file
                using (var stream = File.Open(packagePath, FileMode.Create))
                {
                    using (var writer = new BinaryWriter(stream, Encoding.UTF8, false))
                    {
                        //Write directories to package (step 1)
                        foreach (string directory in Utils.GetDirectories("."))
                        {
                            writer.Write(directory);
                        }
                        writer.Write("-"); //Separator

                        //Write files to package (step 2)
                        foreach ((var file, var hash) in files_hashes)
                        {
                            writer.Write(hash);
                            writer.Write(file);
                        }
                    }
                }
                LoadingAnimation.Stop("The package was successfully saved and written in the repository.");
            }
            catch (Exception ex)
            {
                LoadingAnimation.Stop($"Couldn't save package to LUMI repository *[{ex.Message}]*.");
            }
        }

        //
        // LOAD
        //
        static void Load((string[] args, string[] options) parsedArgs)
        {
            try
            {
                var args = parsedArgs.args;
                var options = parsedArgs.options;
                if (Checks.InitializedCheck()) return;
                if (Checks.FilenameCheck_Load(args)) return;

                var packagesDirectory = new DirectoryInfo($".lumi\\packages\\{args[1]}");

                if (!packagesDirectory.Exists) //If there is no folder with that name
                {
                    Utils.PrintLine("That package doesn't exist in the repository.");
                    return;
                }

                //Gets the package specified by the user or the newest one
                FileInfo? package = default;
                if (args.Length > 2)
                {
                    if (Checks.FilenameCheck_Save(args, true)) return;

                    package = Utils.GetPackageByName(packagesDirectory, args[2]);
                    if (package == default) //If the package is invalid
                    {
                        Utils.PrintLine("That package doesn't exist in the repository.");
                        return;
                    }
                }
                else
                {
                    package = packagesDirectory.GetFiles("*.pkg").OrderByDescending(f => f.LastWriteTime).FirstOrDefault();
                    if (package == default) //If the package is empty
                    {
                        Utils.PrintLine("That package is empty.");
                        return;
                    }
                }

                if (!options.Contains("force")) //Only promps for confirmation if operation is not forced
                {
                    if (options.Contains("keep"))
                    {
                        Utils.PrintLine("Loading a package will potentialy overwrite similar files currently in the repository. *Continue? (Y/N)*.");
                        Utils.PrintLine("Package to be loaded: *" + package.FullName);
                    }
                    else
                    {
                        Utils.PrintLine("Loading a package will delete all the files currently in the repository. *Continue? (Y/N)*.");
                        Utils.PrintLine("Package to be loaded: *" + package.FullName);
                    }

                    if (Utils.ConfirmationPrompt()) return;
                }

                LoadingAnimation.Start("Loading...");

                //Delete everything before loading the package
                if (!options.Contains("keep")) //Ignore deletion if operation wants to keep the files
                {
                    Utils.WipeCurrentFiles();
                }

                //Creates a map of hashes and its corresponding files
                var hashes_files = new Dictionary<string, string[]>();
                var directories = new List<string>(); //Creates list of directories

                //Reads package file
                using (var stream = File.Open(package.FullName, FileMode.Open))
                {
                    using (var reader = new BinaryReader(stream, Encoding.UTF8, false))
                    {
                        int step = 1;
                        while (reader.BaseStream.Position != reader.BaseStream.Length) //Reads till the end of file
                        {
                            //Read directories from package (step 1)
                            if (step == 1)
                            {
                                var directory = reader.ReadString();
                                if (directory == "-") step = 2; //If its the separator, go to step 2
                                else directories.Add(directory); //If not, then add the directory to the list
                                continue;
                            }

                            //Read files from package (step 2)
                            if (step == 2)
                            {
                                var hash = reader.ReadString();
                                var file = reader.ReadString();
                                if (hashes_files.ContainsKey(hash))
                                {
                                    var files = hashes_files[hash];
                                    Array.Resize(ref files, files.Length + 1); //Resizes the file array to create one empty space
                                    files[^1] = file; //Adds the new file at the end of the array
                                    hashes_files[hash] = files; //Edit the dictionary with the new file array
                                }
                                else hashes_files.Add(hash, [file]); //Creates an array with one file and adds it to the dictionary
                            }
                        }
                    }
                }

                //Create directories for the repository
                foreach (var directory in directories) Directory.CreateDirectory(directory);

                //Read from filebase and write to repository
                using (var stream = File.Open(".lumi\\filebase", FileMode.Open))
                {
                    using (var reader = new BinaryReader(stream, Encoding.UTF8, false))
                    {
                        while (reader.BaseStream.Position != reader.BaseStream.Length) //Reads till the end of file
                        {
                            var hash = reader.ReadString();
                            if (hashes_files.ContainsKey(hash)) //If hash is included in the package, write it to the repository
                            {
                                while (true)
                                {
                                    var length = reader.ReadInt32();
                                    var data = reader.ReadBytes(length);
                                    foreach (string file in hashes_files[hash])
                                    {
                                        using (var fileStream = File.Open(file, FileMode.Append))
                                        {
                                            fileStream.Write(data, 0, data.Length);
                                        }
                                    }

                                    var moreData = reader.ReadBoolean();
                                    if (!moreData) break;
                                }
                            }
                            else //Skips data if not included in the package
                            {
                                while (true)
                                {
                                    var length = reader.ReadInt32();
                                    reader.BaseStream.Seek(length, SeekOrigin.Current); //Skips data forwarding the position

                                    var moreData = reader.ReadBoolean();
                                    if (!moreData) break;
                                }
                            }
                        }
                    }
                }
                LoadingAnimation.Stop("The package was successfully loaded and written in the repository.");
            }
            catch (Exception ex)
            {
                LoadingAnimation.Stop($"Couldn't load package from LUMI repository *[{ex.Message}]*.");
            }
        }

        //
        // LIST
        //
        static void List()
        {
            try
            {
                if (Checks.InitializedCheck()) return;

                var packagesDirectory = new DirectoryInfo($".lumi\\packages");
                var packages = packagesDirectory.GetDirectories().OrderByDescending(f => f.LastWriteTime);

                if (!packages.Any()) //If there is no packages
                {
                    Utils.PrintLine("The repository is empty.");
                    return;
                }

                foreach (var package in packages)
                {
                    Utils.PrintLine($"*{package.Name}* │ {package.LastWriteTime}");
                }
            }
            catch (Exception ex)
            {
                Utils.PrintLine($"Couldn't list packages from LUMI repository *[{ex.Message}]*.");
            }
        }

        //
        // HISTORY
        //
        static void History((string[] args, string[] options) parsedArgs)
        {
            try
            {
                var args = parsedArgs.args;
                if (Checks.InitializedCheck()) return;
                if (Checks.FilenameCheck_Load(args)) return;

                var packagesDirectory = new DirectoryInfo($".lumi\\packages\\{args[1]}");

                if (!packagesDirectory.Exists) //If there is no folder with that name
                {
                    Utils.PrintLine("That package doesn't exist in the repository.");
                    return;
                }

                if (!packagesDirectory.GetFiles("*.pkg").Any()) //If there is no packages inside the folder
                {
                    Utils.PrintLine("That package is empty.");
                    return;
                }

                var packages = packagesDirectory.GetFiles("*.pkg").OrderBy(f => f.LastWriteTime);
                int i = 0;
                foreach (var package in packages)
                {
                    string id = Utils.IndentString($"[{i}]", Utils.GetDigits(packages.Count() - 1) + 2);
                    Utils.PrintLine($"{id} *{package.Name}* │ {package.LastWriteTime}");
                    i++;
                }
            }
            catch (Exception ex)
            {
                Utils.PrintLine($"Couldn't load history from LUMI repository *[{ex.Message}]*.");
            }
        }

        //
        // IMPORT
        //
        static void Import((string[] args, string[] options) parsedArgs)
        {
            try
            {
                var args = parsedArgs.args;
                var options = parsedArgs.options;
                if (Checks.InitializedCheck()) return;

                var path = Path.GetFullPath(args[1]);

                if (!File.Exists(path))
                {
                    if (String.IsNullOrEmpty(Path.GetExtension(path)))
                    {
                        path += ".zip";
                        if (!File.Exists(path))
                        {
                            Utils.PrintLine("The file to import the repository doesn't exist or is inaccessible.");
                            return;
                        }
                    }
                    else
                    {
                        Utils.PrintLine("The file to import the repository doesn't exist or is inaccessible.");
                        return;
                    }
                }

                if (!options.Contains("force")) //Only promps for confirmation if operation is not forced
                {
                    Utils.PrintLine("Importing a repository will overwrite all the files currently in the repository. *Continue? (Y/N)*.");
                    Utils.PrintLine("Repository to be imported: *" + path);
                    if (Utils.ConfirmationPrompt()) return;
                }

                LoadingAnimation.Start("Importing...");

                if (Directory.Exists(".lumi")) Directory.Delete(".lumi", true);
                ZipFile.ExtractToDirectory(path, ".lumi");
                LoadingAnimation.Stop($"Imported LUMI repository from *{path}*.");
            }
            catch (Exception ex)
            {
                LoadingAnimation.Stop($"Couldn't import LUMI repository *[{ex.Message}]*.");
            }
        }

        //
        // EXPORT
        //
        static void Export((string[] args, string[] options) parsedArgs)
        {
            try
            {
                var args = parsedArgs.args;
                var options = parsedArgs.options;
                if (Checks.InitializedCheck()) return;

                var path = Path.GetFullPath(args[1]);
                if (String.IsNullOrEmpty(Path.GetExtension(path))) path += ".zip"; //If file has no extension, add .zip extesion to it

                if (!Utils.CanCreateFile(path))
                {
                    Utils.PrintLine("The path to export the repository doesn't exist or is inaccessible.");
                    return;
                }

                if (!options.Contains("force")) //Only promps for confirmation if operation is not forced
                {
                    Utils.PrintLine("Exporting will create a single file containing a copy of the current repository. *Continue? (Y/N)*.");
                    Utils.PrintLine("File to be created: *" + path);
                    if (Utils.ConfirmationPrompt()) return;
                }

                LoadingAnimation.Start("Exporting...");

                CompressionLevel compressionLevel = options.Contains("compress") ? CompressionLevel.Optimal : CompressionLevel.NoCompression; //Adds compression to the zip or not
                ZipFile.CreateFromDirectory(".lumi", path, compressionLevel, false);
                LoadingAnimation.Stop($"Exported LUMI repository to *{path}*.");
            }
            catch (Exception ex)
            {
                LoadingAnimation.Stop($"Couldn't export LUMI repository *[{ex.Message}]*.");
            }
        }

        //
        // WIPE
        //
        static void Wipe((string[] args, string[] options) parsedArgs)
        {
            try
            {
                var options = parsedArgs.options;

                if (!Utils.IsInitialized())
                {
                    Utils.PrintLine("There isn't a repository to wipe.");
                    return;
                }

                if (!options.Contains("force")) //Only promps for confirmation if operation is not forced
                {
                    Utils.PrintLine("Wiping will delete all the packages currently in the repository. *Continue? (Y/N)*.");
                    Utils.PrintLine("*Warning: Make sure you have exported the repository if you don't wanna lose all the packages.*");
                    if (Utils.ConfirmationPrompt()) return;
                }

                Utils.WipeRepository();
                Utils.PrintLine("The LUMI repository was successfully wiped.");
            }
            catch (Exception ex)
            {
                Utils.PrintLine($"Couldn't wipe LUMI repository *[{ex.Message}]*.");
            }
        }

        //
        // PALETTE
        //
        static void Palette((string[] args, string[] options) parsedArgs)
        {
            try
            {
                var args = parsedArgs.args;
                if (Checks.InitializedCheck()) return;

                if (args.Length > 1)
                {
                    if (Utils.IsValidPalette(args[1]))
                    {
                        Utils.SetPalette(args[1]);
                        Utils.PrintLine("The palette was setted successfully in the repository.");
                    }
                    else Utils.PrintLine("The specified palette is not valid.");
                }
                else
                {
                    string[] palettes = ["Default palette", "High contrast", "Magenta", "Green", "Cyan"];
                    var currentPalette = Utils.GetPalette();

                    int i = 0;
                    foreach (string palette in palettes)
                    {
                        if (i.ToString() == currentPalette) Utils.PrintLine($"*[{i}] {palette} (Currently selected)*");
                        else Utils.PrintLine($"[{i}] {palette}");
                        i++;
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.PrintLine($"Couldn't load color palettes from LUMI repository *[{ex.Message}]*.");
            }
        }

        //
        // DELETE
        //
        static void Delete((string[] args, string[] options) parsedArgs)
        {
            try
            {
                var args = parsedArgs.args;
                var options = parsedArgs.options;
                if (Checks.InitializedCheck()) return;
                if (Checks.FilenameCheck_Delete(args)) return;

                var packagesDirectory = new DirectoryInfo($".lumi\\packages\\{args[1]}");

                if (!packagesDirectory.Exists) //If there is no folder with that name
                {
                    Utils.PrintLine("That package doesn't exist in the repository.");
                    return;
                }

                //Gets the package to be deleted
                string packagePath = "";
                if (args.Length > 2)
                {
                    if (Checks.FilenameCheck_Save(args, true)) return;

                    var package = Utils.GetPackageByName(packagesDirectory, args[2]);
                    if (package == default) //If the package is invalid
                    {
                        Utils.PrintLine("That package doesn't exist in the repository.");
                        return;
                    }
                    packagePath = package.FullName;
                }
                else packagePath = packagesDirectory.FullName; //Gets the package directory for deletion

                if (!options.Contains("force")) //Only promps for confirmation if operation is not forced
                {
                    Utils.PrintLine("Deleting a package will also delete all the files saved inside it. *Continue? (Y/N)*.");
                    Utils.PrintLine("Package to be deleted: *" + packagePath);
                    if (Utils.ConfirmationPrompt()) return;
                }

                if (Directory.Exists(packagePath)) Directory.Delete(packagePath, true);
                if (File.Exists(packagePath)) File.Delete(packagePath);
                Utils.PrintLine("The package was successfully deleted from the repository.");
            }
            catch (Exception ex)
            {
                Utils.PrintLine($"Couldn't delete package from LUMI repository *[{ex.Message}]*.");
            }
        }

        //
        // CLEANUP
        //
        static void Cleanup()
        {
            try
            {
                if (Checks.InitializedCheck()) return;

                LoadingAnimation.Start("Cleaning up...");

                //Read hashes from all the packages
                var fileHashes = new List<string>();
                foreach (var packageDirectory in Directory.GetDirectories(".lumi\\packages"))
                {
                    foreach (var package in Directory.GetFiles(packageDirectory, "*.pkg"))
                    {
                        //Reads package file
                        using (var stream = File.Open(package, FileMode.Open))
                        {
                            using (var reader = new BinaryReader(stream, Encoding.UTF8, false))
                            {
                                int step = 1;
                                while (reader.BaseStream.Position != reader.BaseStream.Length) //Reads till the end of file
                                {
                                    //Read directories from package (step 1)
                                    if (step == 1)
                                    {
                                        var directory = reader.ReadString();
                                        if (directory == "-") step = 2; //If its the separator, go to step 2
                                        continue;
                                    }

                                    //Read files from package (step 2)
                                    if (step == 2)
                                    {
                                        var hash = reader.ReadString();
                                        var file = reader.ReadString();
                                        if (!fileHashes.Contains(hash)) fileHashes.Add(hash);
                                    }
                                }
                            }
                        }
                    }
                }

                //Create reader for filebase and writer for new filebase
                {
                    using var streamReader = File.Open(".lumi\\filebase", FileMode.Open);
                    using var reader = new BinaryReader(streamReader, Encoding.UTF8, false);

                    using var streamWriter = File.Open(".lumi\\cleanup", FileMode.Create);
                    using var writer = new BinaryWriter(streamWriter, Encoding.UTF8, false);

                    //Read from old filebase and create new one
                    while (reader.BaseStream.Position != reader.BaseStream.Length) //Reads till the end of file
                    {
                        var hash = reader.ReadString();
                        if (fileHashes.Contains(hash)) //If hash is used by the packages, write it to the new filebase
                        {
                            writer.Write(hash);
                            while (true)
                            {
                                var length = reader.ReadInt32();
                                var data = reader.ReadBytes(length);
                                var moreData = reader.ReadBoolean();

                                writer.Write(length);
                                writer.Write(data);
                                writer.Write(moreData);

                                if (!moreData) break;
                            }
                        }
                        else //Skips data if not included by the packages
                        {
                            while (true)
                            {
                                var length = reader.ReadInt32();
                                reader.BaseStream.Seek(length, SeekOrigin.Current); //Skips data forwarding the position

                                var moreData = reader.ReadBoolean();
                                if (!moreData) break;
                            }
                        }
                    }
                }

                File.Delete(".lumi\\filebase"); //Deletes old filebase
                File.Move(".lumi\\cleanup", ".lumi\\filebase"); //Makes the cleanup filebase the new filebase

                LoadingAnimation.Stop("The filebase was successfully cleaned up.");
            }
            catch (Exception ex)
            {
                LoadingAnimation.Stop($"Couldn't cleanup LUMI repository *[{ex.Message}]*.");
            }
        }

        //
        // TREE
        //
        static void Tree()
        {
            try
            {
                if (Checks.InitializedCheck()) return;

                var packagesDirectory = new DirectoryInfo($".lumi\\packages");
                var packages = packagesDirectory.GetDirectories().OrderBy(f => f.LastWriteTime);

                if (!packages.Any()) //If there is no packages
                {
                    Utils.PrintLine("The repository is empty.");
                    return;
                }

                int packagesRead = 0;
                foreach (var package in packages)
                {
                    packagesRead++;
                    var packagesLeft = packages.Count() - packagesRead;

                    Utils.Print(" ");
                    if (packagesLeft > 0) Utils.PrintLine("├─" + $"*{package.Name}*");
                    else Utils.PrintLine("└─" + $"*{package.Name}*");

                    var subPackages = package.GetFiles("*.pkg").OrderBy(f => f.LastWriteTime);
                    int subPackagesRead = 0;
                    foreach (var subPackage in subPackages)
                    {
                        subPackagesRead++;
                        var subPackagesLeft = subPackages.Count() - subPackagesRead;
                        string id = Utils.IndentString($"[{subPackagesRead-1}]", Utils.GetDigits(subPackages.Count() - 1) + 2);

                        if (packagesLeft > 0) Utils.Print(" │  ");
                        else Utils.Print("    ");

                        if (subPackagesLeft > 0) Utils.PrintLine("├─" + $"*{subPackage.Name}* {id} {subPackage.LastWriteTime}");
                        else Utils.PrintLine("└─" + $"*{subPackage.Name}* {id} {subPackage.LastWriteTime}");
                    }
                }
            }
            catch (Exception ex)
            {
                LoadingAnimation.Stop($"Couldn't show tree from LUMI repository *[{ex.Message}]*.");
            }
        }
    }
}

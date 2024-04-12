namespace Lumi
{
    class Checks
    {
        public static bool FilenameCheck_Save(string[] args, bool checkingSecondArgument = false)
        {
            return FilenameCheck(args, "You have to specify a name for the package.", "The name for the package is invalid.", checkingSecondArgument);
        }
        public static bool FilenameCheck_Load(string[] args, bool checkingSecondArgument = false)
        {
            return FilenameCheck(args, "You have to specify the name of the package.", "The name of the package is invalid.", checkingSecondArgument);
        }
        public static bool FilenameCheck_Delete(string[] args, bool checkingSecondArgument = false)
        {
            return FilenameCheck(args, "You have to specify the name of the package.", "The name of the package is invalid.", checkingSecondArgument);
        }
        public static bool FilenameCheck(string[] args, string prompt1, string prompt2, bool twoFilenames)
        {
            if (args.Length < 2) //If there is no name argument, interrupt
            {
                Utils.PrintLine(prompt1);
                return true;
            }

            if (!Utils.IsValidFilename(args[1])) //Checks if filename is valid
            {
                Utils.PrintLine(prompt2);
                return true;
            }

            if (!twoFilenames) return false;
            else return FilenameCheck(args.Skip(1).ToArray(), prompt1, prompt2, false); //Rechecks the second argument
        }
        public static bool InitializedCheck()
        {
            if (!Utils.IsInitialized())
            {
                Utils.PrintLine("You have to initialize a repository to perform this action.");
                return true;
            }

            return false;
        }
        public static bool InvalidOptionsCheck(string[] invalidOptions)
        {
            if (invalidOptions.Any()) //If there are invalid options
            {
                if (invalidOptions.Length == 1) Utils.PrintLine($"Invalid option *\"{invalidOptions[0]}\"*."); //One invalid option
                else if (invalidOptions.Length == 2) Utils.PrintLine($"Invalid options *\"{invalidOptions[0]}\"* and *\"{invalidOptions[1]}\"*."); //Two invalid options
                else //Multiple invalid options
                {
                    var invalidOptionsMiddle = invalidOptions.Skip(1).SkipLast(1);
                    Utils.Print($"Invalid options *\"{invalidOptions[0]}\"*"); //Writes the first invalid option

                    foreach (var invalidOption in invalidOptionsMiddle) //Writes the invalid options in the middle
                    {
                        Utils.Print($", *\"{invalidOption}\"*");
                    }

                    Utils.PrintLine($" and *\"{invalidOptions.Last()}\"*"); //Writes the last invalid option
                }

                return true;
            }
            else return false;
        }
    }
}

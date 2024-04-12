# Lumi

## Only compatible with Windows (10+) on x64 architecture!
Lumi is a simple version control command line program for Windows, developed by me to be used in my own projects. Made for educational purposes and for my own use cases, it should not be used in production environments or in complex projects. <br/><br/>Focused on high local perfomance and compact way of storing files in the repository, while also being extremely easy to use by begginers who doesn't want to learn something more advanced like Git. Can be used to backup notes, switching between game configs or mods, coding version control, and many other applications.

## Setup
Lumi can be easily installed on a Windows machine by running [this setup](https://github.com/JgBr123/Lumi/releases/tag/setup). It will download and install the latest version on the machine, ready to be used.

## Usage

- `lumi init` : Initializes a new LUMI repository.
- `lumi version` : Gets the current LUMI version installed on your machine.
- `lumi save [package]` : Saves all the files into the repository, creating a package with the specified name.
- `lumi load [package] (subpackage)` : Loads the specified package or subpackage, and writes all the files into the project folder.
- `lumi list` : Lists all the packages in the repository.
- `lumi history [name]` : Lists all the subpackages inside the specified package.
- `lumi import [path]` : Imports a LUMI repository from the specified file.
- `lumi export [path]` : Exports the LUMI repository to the specified file.
- `lumi wipe` : Wipes the entire LUMI repository and all the packages.
- `lumi palette (palette)` : Checks the available color palettes for the repository or sets one.
- `lumi delete [package] (subpackage)` : Deletes the specified package or subpackage.
- `lumi cleanup` : Cleans up the filebase of the repository. This will delete unnecessary files and free up space.
- `lumi tree` : Shows all the packages and subpackages in the repository in a tree style.
#### Keep in mind [ ] are required parameters of the command, and ( ) are optional parameters.

## Useful info

- Packages store all of the files inside a repository. When you save into an existing package, a subpackage will be created. That means packages are never replaced by other packages, unless you manually delete them.
- The filebase is where all of the data of the repository is saved. The packages only save a reference to the data inside the filebase.
- After deleting a package or subpackage, some data may remain inside the filebase. To free up space and remove that data, you can use the cleanup command.
- The entire repository can be exported and imported to a single file, using the export and import commands. They are useful to share the repository to another machine or to backup the repository.

## Repository Structure

### Filebase (For each file)
- Hash (Hash of the data, using SHA-256)
- Data length (Number of bytes of data)
- Data (The data itself)
- Boolean (True if the file is bigger than 2gb. The program will split the file into many 2gb files)

### Package (For each file)
- Directories names (The path of each directory that should be created when loading this package)
- File hash (The SHA-256 hash of the data that should be written to a file. It will look into the filebase to try to find this hash and retrieve the data)
- File path (The path of where the file should be written)

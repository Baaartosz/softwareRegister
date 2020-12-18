using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using IWshRuntimeLibrary;
using System.Text.Json;
using File = System.IO.File;

// okay so rewrite the code to create a object that contains
// the executable data and creates a shortcut instead of modifying the registry
// it creates shortscuts to the exe in the right places for windows to search.
// also look into %Appdata%
namespace softwareRegister
{
    /// <summary>
    ///  This class holds the executable object methods to
    ///  register the file executable in the Windows and
    ///  tracks the current state of the current object.
    /// </summary>
    public class ExecutableObject
    {
        // Note to self
        // ---
        // create a custom json file / class array custom that holds all the history of
        // that desktop and changes it made to the Windows Registry.
        // EDIT - that will go to %programdata%
        private string _executablePath;
        private string _fileName;
        private bool _isRegistered = false;
        private const string TestPath = "C:\\Projects\\softwareRegister\\softwareRegister\\";
        private List<string> _modfiedLocations = new List<string>();

        /// <summary>
        /// Uses a ?? operator that basically means that it will try to get the
        /// primary property and if it returns null then it will use the backup
        /// property which is "Object name unset".
        ///
        /// => (primary) ?? (backup);
        ///
        /// In reality this should never be null.
        /// </summary>
        /// <returns>String executablePath</returns>
        private string GetObjectExecutableLocation() => _executablePath ?? "Executable location unset.";

        private string GetExecutableFileName() => _fileName ?? "Executable name unset";

        private bool GetIfRegistered() => _isRegistered;

        public List<string> GetModifedLocations() => _modfiedLocations;

        /// <summary>
        /// Constructor, set the executablePath and fileName
        /// </summary>
        /// <param name="executablePath">The executable path</param>
        /// <param name="fileName">The executable full name</param>
        public ExecutableObject(string executablePath, string fileName)
        {
            _executablePath = executablePath;
            _fileName = fileName;
        }

        // This can be tracked based upon development of the JSON file class to be made.
        private void SaveToAppdata()
        {
            var exeObject = new ExeObjectSeralised()
            {
                ExecutableName = GetExecutableFileName(),
                ExecutablePath = GetObjectExecutableLocation(),
                IsRegistered = GetIfRegistered(),
                ModfiedLocations = GetModifedLocations(),
                TimeMade = DateTime.Now
            };

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };
            var jsonString = JsonSerializer.Serialize(exeObject, options);
            File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
                              + "\\" + GetExecutableFileName() + ".sr", jsonString);
        }

        private void GetSaveFromAppdata()
        {
            var currentJson = File.ReadAllText(File.ReadAllText(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
                + "\\" + GetExecutableFileName() + ".sr"));
            var executableObject = JsonSerializer.Deserialize<ExeObjectSeralised>(currentJson);

            if (executableObject == null || DateTime.Compare(DateTime.Now, executableObject.TimeMade) < 0) return;
            _fileName = executableObject.ExecutableName;
            _executablePath = executableObject.ExecutablePath;
            _isRegistered = executableObject.IsRegistered;
            _modfiedLocations = executableObject.ModfiedLocations;
        }


        /// <summary>
        /// Creates a shortcut in Windows.
        /// </summary>
        /// <param name="specialFolder">Location to be saved in.</param>
        /// <param name="shortcutTargetLocationPath">Location where shortcut will be saved in.</param>
        /// <returns>false == failed, true == success</returns>
        private bool CreateShortcut(Environment.SpecialFolder specialFolder, string shortcutTargetLocationPath)
        {
            if (!_isRegistered)
            {
                var wsh = new IWshShell_Class();
                if (wsh.CreateShortcut(PathLink:
                    Environment.GetFolderPath(specialFolder)
                    + "\\" + GetExecutableFileName() + ".lnk") is IWshShortcut shortcut)
                {
                    shortcut.TargetPath = GetObjectExecutableLocation();
                    shortcut.Save();
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Deletes the shortcut created by the program undo changes.
        /// </summary>
        /// <param name="targetShortcutPath"></param>
        /// <returns>false == failed, true == success</returns>
        private bool DeleteShortcut(string targetShortcutPath)
        {
            try
            {
                // Check if file exists with its full path    
                if (!File.Exists(targetShortcutPath)) return false;
                File.Delete(targetShortcutPath);
                _modfiedLocations.Remove(targetShortcutPath);
                return true;
            }
            // Should really be catching and finding what the exception is and address it 
            // from there. :3 *hover over the file.delete*
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Holds a bunch of special folder to iterate through
        /// </summary>
        private readonly Environment.SpecialFolder[] _folders =
        {
            Environment.SpecialFolder.Startup,
            Environment.SpecialFolder.StartMenu
        };

        public bool RegisterInWindows()
        {
            // Go through a array and add the shortcut to those folders.
            for (var index = 0; index < _folders.Length; index++)
            {
                var f = _folders[index];
                if (!CreateShortcut(f, TestPath)) continue;
                var finalLocation = Environment.GetFolderPath(f) + "\\" + GetExecutableFileName() + ".lnk";
                _modfiedLocations.Add(finalLocation);
                if (index == _folders.Length - 1) return true;
            }

            return false;
        }

        public bool DeregisterInWindows()
        {
            // Add a way to update the locations array before proceeding with the 
            // Foreach String in 'ModifiedLocations' Delete all.
            return _modfiedLocations.Where(DeleteShortcut).Any();
        }

        public void Close()
        {
            SaveToAppdata();
        }
        
    }
}
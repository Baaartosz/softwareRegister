using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using IWshRuntimeLibrary;
using System.Text.Json;
using System.Windows;
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
        private string _softwareFolderName = "SoftwareReg";
        private static string _dataFolderPath;
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

        //private string GetExecutableFileName() => _fileName ?? "Executable name unset";

        private bool GetIfRegistered() => _isRegistered;

        private List<string> GetModfiedLocations() => _modfiedLocations;

        /// <summary>
        /// Constructor, set the executablePath and fileName
        /// </summary>
        /// <param name="executablePath">The executable path</param>
        /// <param name="fileName">The executable full name</param>
        public ExecutableObject(string executablePath, string fileName)
        {
            _executablePath = executablePath;
            _fileName = fileName;
            _dataFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), _softwareFolderName);
            MessageBox.Show(_dataFolderPath);
            Directory.CreateDirectory(_dataFolderPath);
            //NOTE TO SELF
            // Create a sepererate constructor for attmpting to find if it already exsists if not create new object.
        }

        // This can be tracked based upon development of the JSON file class to be made.
        private void SaveToAppdata()
        {
            var exeObject = new ExeObjectSeralised()
            {
                ExecutableName = _fileName,
                ExecutablePath = GetObjectExecutableLocation(),
                IsRegistered = GetIfRegistered(),
                ModfiedLocations = GetModfiedLocations(),
                TimeMade = DateTime.Now
            };

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };
            var jsonString = JsonSerializer.Serialize(exeObject, options);
            File.WriteAllText(_dataFolderPath + "\\" + _fileName + ".sr", jsonString);
        }

        private void GetSaveFromAppdata()
        {
            var currentJson = File.ReadAllText(_dataFolderPath + "\\" + _fileName + ".sr");
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
            try
            {
                if (!_isRegistered)
                {
                    var wsh = new IWshShell_Class();
                    if (wsh.CreateShortcut(PathLink:
                        Environment.GetFolderPath(specialFolder)
                        + "\\" + _fileName + ".lnk") is IWshShortcut shortcut)
                    {
                        shortcut.TargetPath = shortcutTargetLocationPath;
                        shortcut.Save();
                        _isRegistered = true;
                        SaveToAppdata();
                    }
                }
                if (_isRegistered)
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
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
                SaveToAppdata();
                return true;
            }
            // Should really be catching and finding what the exception is and address it 
            // from there. :3 *hover over the file.delete*
            catch
            {
                return false;
            }
        }

        // TODO
        // Needs to accept dynamic changes in the user interface from the
        // tick boxes of possible places.
        
        /// <summary>
        /// Holds a bunch of special folder to iterate through
        /// </summary>
        private readonly Environment.SpecialFolder[] _folders =
        {
            Environment.SpecialFolder.Startup,
            Environment.SpecialFolder.StartMenu
        };

        public void RegisterInWindows()
        {
            // Go through a array and add the shortcut to those folders.
            foreach (var f in _folders)
            {
                if (CreateShortcut(f, _executablePath))
                {
                    var finalLocation = Environment.GetFolderPath(f) + "\\" + _fileName + ".lnk";
                    _modfiedLocations.Add(finalLocation);
                    MessageBox.Show($"Operation successful on {_fileName}");
                    SaveToAppdata();
                } else
                    MessageBox.Show($"Operation failed on {_fileName}");
            }
        }

        public void DeregisterInWindows()
        {
            // Add a way to update the locations array before proceeding with the 
            // Foreach String in 'ModifiedLocations' Delete all.
            GetSaveFromAppdata();
            MessageBox.Show(_modfiedLocations.Where(DeleteShortcut).Any()
                ? $"Operation successful : {_fileName}"
                : $"Unregistering failed on {_fileName}");
        }

        public void Close()
        {
            SaveToAppdata();
        }

        private void CleanUp()
        {
            
        }
        
    }
}
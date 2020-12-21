using System;
using System.Collections.Generic;
using System.IO;
using IWshRuntimeLibrary;
using System.Text.Json;
using System.Windows;
using File = System.IO.File;

namespace softwareRegister
{
    /// <summary>
    ///  This class holds the executable object methods to
    ///  register the file executable in the Windows and
    ///  tracks the current state of the current object.
    /// </summary>
    public class ExecutableObject
    {
        private string _executablePath;
        private string _fileName;
        private bool _hasExe = false; // TODO _hasExe should be used to prevent unwanted crashes when no object is selected.
        private bool _isRegistered = false;
        private const string SoftwareFolderName = "SoftwareReg";
        private readonly string _dataFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), SoftwareFolderName);
        private List<string> _modfiedLocations = new List<string>();

        
        /// <summary>
        /// Constructor, set the executablePath and fileName
        /// </summary>
        /// <param name="executablePath">The executable path</param>
        /// <param name="fileName">The executable full name</param>
        public ExecutableObject(string executablePath, string fileName)
        {
            _executablePath = executablePath;
            _fileName = fileName;
            
            if (Directory.Exists(_dataFolderPath)) GetSaveFromAppdata();
            if(_dataFolderPath != null && !Directory.Exists(_dataFolderPath))
                Directory.CreateDirectory(_dataFolderPath);
            
            MessageBox.Show(Privilege.IsElevated
                ? "Admin : True"
                : "Admin : False");
        }
        
        private void SaveToAppdata()
        {
            var exeObject = new ExeObjectSeralised()
            {
                ExecutableName = _fileName,
                ExecutablePath = _executablePath,
                IsRegistered = _isRegistered, 
                ModfiedLocations = _modfiedLocations, // todo final bug wrong spelling 'ModifiedLocations' instead.
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

            // Check current date is newer then old file date.
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
        { // TODO clean up
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
                } else if (_isRegistered) return true;
               
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
            MessageBox.Show(targetShortcutPath);
            try
            {
                // Check if file exists with its full path
                MessageBox.Show("I am going to modify: " + targetShortcutPath);
                if (File.Exists(targetShortcutPath))
                {
                    MessageBox.Show("Shortcut to exists.");
                    File.Delete(targetShortcutPath);
                    _modfiedLocations.Remove(targetShortcutPath);
                    return true;
                }

                MessageBox.Show("Shortcut to delete doesnt exist.");
                _modfiedLocations.Remove(targetShortcutPath);
                if (_modfiedLocations.Count == 0)
                {
                    CleanUp();
                    // TODO check if _modifiedLocations is empty if it is run the cleanup function.
                }
                else
                {
                    SaveToAppdata();
                }

                return false;
            }
            // Should really be catching and finding what the exception is and address it 
            // from there. :3 *hover over the file.delete*
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return false;
            }
        }

        // TODO low new features
        // Needs to accept dynamic changes in the user interface from the
        // tick boxes of possible places.
        
        /// <summary>
        /// Holds a bunch of special folder to iterate through
        /// </summary>
        private readonly Environment.SpecialFolder[] _folders =
        {
            // Currently Limited to Startup due to removal errors in other locations.
            Environment.SpecialFolder.Startup
        };

        public void RegisterInWindows() // works as far as I know more problems with remove.
        {
            // Go through a array and add the shortcut to those folders.
            foreach (var f in _folders)
            {
                if (CreateShortcut(f, _executablePath))
                {
                    var finalLocation = Environment.GetFolderPath(f) + "\\" + _fileName + ".lnk";
                    _modfiedLocations.Add(finalLocation);
                    _isRegistered = true;
                    MessageBox.Show($"Operation successful on {_fileName}");
                } else
                    MessageBox.Show($"Operation failed on {_fileName}");
            }
            SaveToAppdata();
        }

        // This function is not removing shortcuts correctly. Leaving a mess tbf
        // Remove function does not work in area other than startup.
        //     OR
        // Remove function MAY remove the shortcuts but fails to update the appdata file?
        public void DeregisterInWindows()
        {
            _isRegistered = false;
            GetSaveFromAppdata();
            foreach (var modfiedLocation in _modfiedLocations) MessageBox.Show(modfiedLocation);
            foreach (string location in _modfiedLocations)
            {
                MessageBox.Show(DeleteShortcut(location)
                    ? $"Operation successful : {_fileName}"
                    : $"Unregistering failed on {_fileName}");
            }
            Close();
        }

        private void Close()
        {
            SaveToAppdata();
        }

        /// <summary>
        /// When the program finds that the program is not longer registed it will cleanup after itself.
        /// </summary>
        private void CleanUp()
        {
            
        }
        
    }
}
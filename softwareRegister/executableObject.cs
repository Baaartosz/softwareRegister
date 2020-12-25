using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private bool _isRegistered = false;
        private const string SoftwareFolderName = "SoftwareReg";
        private readonly string _dataFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), SoftwareFolderName);
        private List<string> _modifiedLocations = new List<string>();

        
        /// <summary>
        /// Constructor, set the executablePath and fileName
        /// </summary>
        /// <param name="executablePath">The executable path</param>
        /// <param name="fileName">The executable full name</param>
        public ExecutableObject(string executablePath, string fileName)
        {
            _executablePath = executablePath;
            _fileName = fileName;

            // Check if executable has all ready been registered.
            if (Directory.Exists(_dataFolderPath)) GetSaveFromAppdata(); 
           
            // Check if directory is available for program.
            if(_dataFolderPath != null && !Directory.Exists(_dataFolderPath)) 
                Directory.CreateDirectory(_dataFolderPath);

            // MessageBox.Show(Privilege.IsElevated
            //     ? "Admin : True"
            //     : "Admin : False");
        }
        
        private void SaveToAppdata()
        {
            var exeObject = new ExeObject()
            {
                ExecutableName = _fileName,
                ExecutablePath = _executablePath,
                IsRegistered = _isRegistered, 
                ModifiedLocations = _modifiedLocations,
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
            var executableObject = JsonSerializer.Deserialize<ExeObject>(currentJson);

            // Check current date is newer then old file date.
            if (executableObject == null || DateTime.Compare(DateTime.Now, executableObject.TimeMade) < 0) return;
            _fileName = executableObject.ExecutableName;
            _executablePath = executableObject.ExecutablePath;
            _isRegistered = executableObject.IsRegistered;
            _modifiedLocations = executableObject.ModifiedLocations;
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
                    _modifiedLocations.Remove(targetShortcutPath);
                    return true;
                }

                MessageBox.Show("Shortcut to delete doesnt exist.");
                _modifiedLocations.Remove(targetShortcutPath);
                if (_modifiedLocations.Count == 0)
                {
                    CleanUp();
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
                    _modifiedLocations.Add(finalLocation);
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
            foreach (var modfiedLocation in _modifiedLocations) MessageBox.Show(modfiedLocation);
            foreach (string location in _modifiedLocations)
            {
                MessageBox.Show(DeleteShortcut(location)
                    ? $"Operation successful : {_fileName}"
                    : $"Unregistering failed on {_fileName}");
            }
        }

        /// <summary>
        /// When the program finds that the executable is not longer registed it will cleanup after itself.
        /// </summary>
        private void CleanUp()
        {
            if (Directory.GetFiles(_dataFolderPath, "*.sr").Length != 0)
            {
                foreach (var path in Directory.GetFiles(_dataFolderPath, "*.sr"))
                {
                    File.Delete(path);
                }
            }
        }
        
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                IsRegistered = _isRegistered, // todo bug wrong spelling 'ModifiedLocations' instead.
                ModfiedLocations = _modfiedLocations,
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
            Environment.SpecialFolder.CommonStartMenu
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
            _isRegistered = false;
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
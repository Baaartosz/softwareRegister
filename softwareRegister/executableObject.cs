using System;
using Shell32;
using IWshRuntimeLibrary;


// okay so rewrite the code to create a object that contains
// the executable data and creates a shortcut instead of modifying the registry
// it creates shortscuts to the exe in the right places for windows to search.
// also look into %programdata%
namespace softwareRegister
{   

    /// <summary>
    ///  This class holds the executable object methods to
    ///  register the file executable in the Windows and
    ///  tracks the current state of the current object.
    /// </summary>
    public class executableObject
    {
        // Note to self
        // ---
        // create a custom json file / class array custom that holds all the history of
        // that desktop and changes it made to the Windows Registry.
        // EDIT - that will go to %programdata%
        private readonly String _executablePath;
        private readonly String _fileName;
        private bool _isRegistered = false;

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
        public String GetObjectExecutableLocation() => _executablePath ?? "Executable location unset.";

        public String GetExecutableFileName() => _fileName ?? "Executable name unset";

        public String GetIfRegistered() => _isRegistered.ToString();

        /// <summary>
        /// Constructor, set the executablePath and fileName
        /// </summary>
        /// <param name="executablePath">The executable path</param>
        /// <param name="fileName">The executable full name</param>
        public executableObject(string executablePath, string fileName)
        {
            this._executablePath = executablePath;
            this._fileName = fileName;
        }
        
        // Note to self
        // ---
        // A additional problem that might occur is if the exe is missing from the locations and
        // it tries to access it, it will show an error that it doesnt exists.
        // Could try and find a way to double check the executables exist and if not rollback the changes
        // that were made.
        // 
        // This can be tracked based upon development of the JSON file class to be made.
        
        private bool CreateShortcut(string targetLocationPath)
        {
            var wsh = new IWshShell_Class();
            if (wsh.CreateShortcut(PathLink:
                Environment.GetFolderPath(Environment.SpecialFolder.StartMenu)
                + "\\" + GetExecutableFileName() + ".lnk") is IWshShortcut shortcut)
            {
                shortcut.TargetPath = GetObjectExecutableLocation();
                shortcut.Save();
                return true;
            }
            return false;
        }
        
        public bool RegisterInWindows()
        {
            // Access the registry 
            // add all necessary keys into the registry to find the applcation
            // and index it in file explorer
            // RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\WinRegistry");
            // key.SetValue("value", 180);
            // key.Close(); 
            return false;
        } 
        
        public bool DeregisterInWindows()
        {
            // check the file containing the data regarding registry edits.
            // 
            // RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\WinRegistry");  
            // if (key != null)  
            // {  
            //     int value = int.Parse(key.GetValue("value").ToString());  
            // } 
            return false;
        } 
    }
}
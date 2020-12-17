using System;

namespace softwareRegister
{   

    /// <summary>
    ///  This class holds the executable object methods to
    ///  register the file executable in the registry and
    ///  tracks the current state of the current object.
    /// </summary>
    public class RegisteredObject
    {
        // Note to self
        // ---
        // create a custom json file / class array custom that holds all the history of
        // that desktop and changes it made to the Windows Registry.
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
        public RegisteredObject(string executablePath, string fileName)
        {
            this._executablePath = executablePath;
            this._fileName = fileName;
        }
        
        public bool RegisterObjectInRegistry(string fileLocation)
        {
            // Access the registry 
            // add all necessary keys into the registry to find the applcation
            // and index it in the 
            return false;
        } 
        
        public bool DeregisterObjectInRegistry()
        {
            return false;
        } 
    }
}
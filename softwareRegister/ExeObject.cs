using System;
using System.Collections.Generic;

namespace softwareRegister
{
    public class ExeObject
    {
        public List<string> ModifiedLocations { get; set; }
        public string ExecutableName { get; set; }
        public string ExecutablePath { get; set; }
        public bool IsRegistered { get; set; }
        public DateTime TimeMade { get; set; }
    }
}
using System;
using System.Collections.Generic;

namespace softwareRegister
{
    public class ExeObjectSeralised
    {
        public List<string> ModfiedLocations { get; set; }
        public string ExecutableName { get; set; }
        public string ExecutablePath { get; set; }
        public bool IsRegistered { get; set; }
        public DateTime TimeMade { get; set; }
    }
}
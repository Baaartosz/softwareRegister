using System.Security.Principal;

namespace softwareRegister
{
    public static class Privilege
    {
        // TODO
        // Yeah you gotta check for Administrator Privilege, as restricted users may experience problems, windows *roll eyes*
        public static bool IsElevated => new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
    }
}
using System.Security.Principal;

namespace softwareRegister
{
    public class Privilege
    {
        static bool IsElevated => new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
    }
}
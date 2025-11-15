using System.Runtime.InteropServices;

namespace AUTOCAP.Core.Utilities;

/// <summary>
/// Runtime information utilities
/// </summary>
public static class RuntimeInformation
{
    public static string FrameworkDescription
    {
        get
        {
#if NET8_0
            return ".NET 8.0";
#else
            return ".NET (Version Unknown)";
#endif
        }
    }

    public static Architecture ProcessArchitecture
    {
        get => System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture;
    }
}

using System.Runtime.InteropServices;
using Xunit;

namespace NativeLibraryLoader.Tests
{
    public class SystemLibrary
    {
        private delegate void BeepFunctionType(uint frequency, uint duration);
        [Fact]
        public void Kernel32_Beep()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                NativeLibrary k32 = new NativeLibrary("kernel32");
                k32.LoadFunction<BeepFunctionType>("Beep");
            }
        }
    }
}

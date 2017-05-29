using System;
using Xunit;

namespace NativeLibraryLoader.Tests
{
    public class NativeLibInPackage
    {
        [Fact]
        public void LoadLibraryAndFunction()
        {
            NativeLibrary nl = new NativeLibrary("cimgui");
            var functionPtr = nl.LoadFunction<Action>("igNewFrame");
        }
    }
}

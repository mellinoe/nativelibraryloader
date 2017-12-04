using Microsoft.DotNet.PlatformAbstractions;
using Microsoft.Extensions.DependencyModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace NativeLibraryLoader
{
    /// <summary>
    /// Enumerates possible library load targets.
    /// </summary>
    public abstract class PathResolver
    {
        /// <summary>
        /// Returns an enumerator which yieilds possible library load targets, in priority order.
        /// </summary>
        /// <param name="name">The name of the library to load.</param>
        /// <returns>An enumerator yielding load targets.</returns>
        public abstract IEnumerable<string> EnumeratePossibleLibraryLoadTargets(string name);

        /// <summary>
        /// Gets a default path resolver.
        /// </summary>
        public static PathResolver Default { get; } = new DefaultPathResolver();
    }

    /// <summary>
    /// Enumerates possible library load targets. This default implementation returns the following load targets:
    /// First: The library contained in the applications base folder.
    /// Second: The simple name, unchanged.
    /// Third: The library as resolved via the default DependencyContext, in the default nuget package cache folder.
    /// </summary>
    public class DefaultPathResolver : PathResolver
    {
        /// <summary>
        /// Returns an enumerator which yieilds possible library load targets, in priority order.
        /// </summary>
        /// <param name="name">The name of the library to load.</param>
        /// <returns>An enumerator yielding load targets.</returns>
        public override IEnumerable<string> EnumeratePossibleLibraryLoadTargets(string name)
        {
            yield return Path.Combine(AppContext.BaseDirectory, name);
            yield return name;
            if (TryLocateNativeAssetFromDeps(name, out string depsResolvedPath))
            {
                yield return depsResolvedPath;
            }
        }

        private bool TryLocateNativeAssetFromDeps(string name, out string depsResolvedPath)
        {
            DependencyContext defaultContext = DependencyContext.Default;
            string runtimeIdentifier = Microsoft.DotNet.PlatformAbstractions.RuntimeEnvironment.GetRuntimeIdentifier();
            foreach (var runtimeLib in defaultContext.RuntimeLibraries)
            {
                foreach (var nativeAsset in runtimeLib.GetRuntimeNativeAssets(defaultContext, runtimeIdentifier))
                {
                    if (Path.GetFileName(nativeAsset) == name || Path.GetFileNameWithoutExtension(nativeAsset) == name)
                    {
                        string fullPath = Path.Combine(
                            GetNugetPackagesRootDirectory(),
                            runtimeLib.Name.ToLowerInvariant(),
                            runtimeLib.Version, nativeAsset);
                        fullPath = Path.GetFullPath(fullPath);
                        depsResolvedPath = fullPath;
                        return true;
                    }
                }
            }

            depsResolvedPath = null;
            return false;
        }

        private string GetNugetPackagesRootDirectory()
        {
            // TODO: Handle alternative package directories, if they are configured.
            return Path.Combine(GetUserDirectory(), ".nuget", "packages");
        }

        private string GetUserDirectory()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return Environment.GetEnvironmentVariable("USERPROFILE");
            }
            else
            {
                return Environment.GetEnvironmentVariable("HOME");
            }
        }
    }
}

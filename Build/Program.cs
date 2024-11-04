using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CopyBuildAssembly;

// ReSharper disable ClassNeverInstantiated.Global

namespace Build
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var arg = args.ElementAtOrDefault(0);
            var sha = Copy.GetTipSha(args.ElementAtOrDefault(1));

            const string gamePath = @"R:\Battlestate Games\Client.0.15.5.1.33420";

            var modPath = $@"{gamePath}\BepInEx\plugins\kmyuhkyuk-SkinService";

            const string versionName = "1.2.1";

            var releaseName = $"{new DirectoryInfo(modPath).Name}-(Release_{versionName}).7z";

            try
            {
                var skinServiceServerModPath = Path.Combine(baseDirectory, "SkinService_ServerMod");

                Copy.CopyFolder(arg, "Release", Path.Combine(skinServiceServerModPath),
                    gamePath);

                Copy.CopyFolder(arg, "Release", Path.Combine(baseDirectory, "localized"),
                    Path.Combine(modPath, "localized"));

                Copy.CopyAssembly(arg, "Release", baseDirectory, modPath, new[]
                {
                    "SkinService"
                }, sha);

                Copy.GenerateSevenZip(arg, "Release", modPath, releaseName, @"BepInEx\plugins", Array.Empty<string>(),
                    Array.Empty<string>(), new[] { Path.Combine(baseDirectory, "ReadMe.txt") },
                    new[] { skinServiceServerModPath });

                //Unity

                const string unityEditorPath = @"C:\Users\24516\Documents\SkinService\Assets\Managed";

                Copy.CopyAssembly(arg, "UNITY_EDITOR", baseDirectory, unityEditorPath, new[]
                {
                    "SkinService"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

                Console.ReadKey();

                Process.GetCurrentProcess().Kill();
            }
        }
    }
}
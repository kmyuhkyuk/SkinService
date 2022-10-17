using HarmonyLib;
using System;
using System.Reflection;

namespace SkinService.Utils
{
    public class LocalizedHelp
    {
        private static Func<string, string, string> RefLocalized;

        public static void Init()
        {
            BindingFlags flags = BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;

            RefLocalized = AccessTools.MethodDelegate<Func<string, string, string>>(RefHelp.GetEftMethod(x => x.GetMethod("ParseLocalization", flags) != null, flags, 
                x => x.Name == "Localized"
                && x.GetParameters().Length == 2
                && x.GetParameters()[0].ParameterType == typeof(string)
                && x.GetParameters()[1].ParameterType == typeof(string)));
        }

        public static string localized(string id, string prefix)
        {
            return RefLocalized(id, prefix);
        }
    }
}

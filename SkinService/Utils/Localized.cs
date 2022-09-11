using Aki.Reflection.Utils;
using System;
using System.Linq;
using System.Reflection;
using EFT;

namespace SkinService.Utils
{
    public class Localized
    {
        private static Type LocalizedType;

        private static MethodInfo LocalizedMethod;

        public static void Init()
        {
            LocalizedType = PatchConstants.EftTypes.Single(x =>
            x.GetMethod("ParseLocalization", BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance) != null);

            LocalizedMethod = LocalizedType.GetMethods().Single(x => x.Name == "Localized"
            && x.GetParameters().Length == 2
            && x.GetParameters()[0].ParameterType == typeof(string)
            && x.GetParameters()[1].ParameterType == typeof(string));
        }

        public static string localized(string id, string prefix)
        {
            return (string)LocalizedMethod.Invoke(null, new object[] { id, prefix });
        }
    }
}

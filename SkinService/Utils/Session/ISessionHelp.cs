using System;
using System.Reflection;
using Comfort.Common;

namespace SkinService.Utils.Session
{
    public static class ISessionHelp
    {
        private static ISession Session;

        private static Action<object, string, Callback> RefChangeVoice;

        static ISessionHelp()
        {
            BindingFlags flags = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance;

            RefChangeVoice = RefHelp.ObjectMethodDelegate<Action<object, string, Callback>>(RefHelp.GetEftType(x => x.GetMethod("ChangeVoice", flags) != null && !x.IsInterface && !x.GetMethod("ChangeVoice", flags).GetParameters()[1].HasDefaultValue).GetMethod("ChangeVoice", flags));
        }

        public static void Init(ISession session)
        {
            Session = session;
        }

        public static void ChangeVoice(string voice, Callback callback)
        {
            RefChangeVoice(Session, voice, callback);
        }

        public static void SendOperationRightNow(object operation, Callback callback)
        {
            Session.SendOperationRightNow(operation, callback);
        }
    }
}

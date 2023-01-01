using HarmonyLib;
using System;
using System.Reflection;
using Comfort.Common;

namespace SkinService.Utils.Session
{
    public class ISessionHelp
    {
        private static ISession Session;

        private static Action<string, Callback> RefChangeVoice;

        //Only Init Once
        public static void Init(ISession session)
        {
            Session = session;

            Type sessionType = Session.GetType();

            RefChangeVoice = AccessTools.MethodDelegate<Action<string, Callback>>(sessionType.GetMethod("ChangeVoice", BindingFlags.Public | BindingFlags.Instance), Session);
        }

        public static void ChangeVoice(string voice, Callback callback)
        {
            RefChangeVoice(voice, callback);
        }

        public static void SendOperationRightNow(object operation, Callback callback)
        {
            Session.SendOperationRightNow(operation, callback);
        }
    }
}

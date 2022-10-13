using CitiesHarmony.API;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace QCommonLib
{
    public class QPatcher
    {
        private string HarmonyId;
        private bool patched = false;
        private bool IsDebug = false;
        private DEarlyRevert EarlyRevert;

        private Harmony instance = null;
        public Harmony Instance
        {
            get {
                if (instance == null)
                {
                    instance = new Harmony(HarmonyId);
                    Harmony.DEBUG = IsDebug;
                }
                return instance;
            }
            set { instance = value; }
        }

        public QPatcher(string id, DEarlyDeploy earlyDeploy = null, DEarlyRevert earlyRevert = null, bool isDebug = false)
        {
            HarmonyId = id;
            IsDebug = isDebug;
            EarlyRevert = earlyRevert;
            if (earlyDeploy != null) earlyDeploy(this);
        }

        public delegate void DEarlyDeploy(QPatcher patcher);
        public delegate void DEarlyRevert(QPatcher patcher);

        public void PatchAll()
        {
            if (patched) return;

            patched = true;
            HarmonyHelper.DoOnHarmonyReady(() => Instance.PatchAll());
        }

        public void UnpatchAll()
        {
            if (!patched) return;

            if (EarlyRevert != null) EarlyRevert(this);
            HarmonyHelper.DoOnHarmonyReady(() => Instance.UnpatchAll());
            patched = false;
        }

        public void Prefix(MethodInfo original, MethodInfo replacement)
        {
            Instance.Patch(original, prefix: new HarmonyMethod(replacement));
        }

        public void Postfix(MethodInfo original, MethodInfo replacement)
        {
            Instance.Patch(original, postfix: new HarmonyMethod(replacement));
        }

        public void Unpatch(MethodInfo original, MethodInfo replacement)
        {
            Instance.Unpatch(original, replacement);
        }
    }
}

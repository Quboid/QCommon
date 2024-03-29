﻿using CitiesHarmony.API;
using HarmonyLib;
using System;
using System.Reflection;

namespace QCommonLib
{
    public class QPatcher
    {
        private readonly Assembly caller;
        private readonly string HarmonyId;
        private readonly bool IsDebug = false;
        private readonly DEarlyRevert EarlyRevert;
        private bool patched = false;

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

        /// <summary>
        /// Create instance. Should be called early in mod lifecycle, typically during OnEnable
        /// </summary>
        /// <param name="id">The mod's Harmony ID</param>
        /// <param name="earlyDeploy">Optional method to call for patches that need to be deployed before loading</param>
        /// <param name="earlyRevert">Optional method to clean up the early deployed patches</param>
        /// <param name="isDebug">Should QLogger output extra debug info?</param>
        public QPatcher(string id, DEarlyDeploy earlyDeploy = null, DEarlyRevert earlyRevert = null, bool isDebug = false)
        {
            caller = Assembly.GetCallingAssembly();
            HarmonyId = id;
            IsDebug = isDebug;
            EarlyRevert = earlyRevert;
            earlyDeploy?.Invoke(this);
            QLoggerStatic.Debug($"QPatcher initialised, EarlyDeploy applied", "[Q04]");
        }

        public delegate void DEarlyDeploy(QPatcher patcher);
        public delegate void DEarlyRevert(QPatcher patcher);

        /// <summary>
        /// Deploys all patches marked with annotation
        /// </summary>
        public void PatchAll()
        {
            if (patched) return;

            patched = true;
            HarmonyHelper.DoOnHarmonyReady(() => Instance.PatchAll(caller));
            QLoggerStatic.Debug($"QPatcher PatchAll applied", "[Q05]");
        }

        /// <summary>
        /// Reverts all patches including ones deployed early
        /// </summary>
        public void UnpatchAll()
        {
            if (!patched) return;

            EarlyRevert?.Invoke(this);
            HarmonyHelper.DoOnHarmonyReady(() => Instance.UnpatchAll(HarmonyId));
            patched = false;
            QLoggerStatic.Debug($"QPatcher EarlyRevert and UnpatchAll applied", "[Q06]");
        }

        /// <summary>
        /// Deploy a prefix patch
        /// </summary>
        /// <param name="original">The method to patch</param>
        /// <param name="replacement">The method to prefix <paramref name="original"/></param>
        public void Prefix(MethodInfo original, MethodInfo replacement)
        {
            try
            {
                Instance.Patch(original, prefix: new HarmonyMethod(replacement));
            }
            catch (Exception e)
            {
                throw new QPatcherPrefixException(original, replacement, e);
            }
        }


        /// <summary>
        /// Deploy a postfix patch
        /// </summary>
        /// <param name="original">The method to patch</param>
        /// <param name="replacement">The method to postfix <paramref name="original"/></param>
        public void Postfix(MethodInfo original, MethodInfo replacement)
        {
            try
            {
                Instance.Patch(original, postfix: new HarmonyMethod(replacement));
            }
            catch (Exception e)
            {
                throw new QPatcherPostfixException(original, replacement, e);
            }
        }

        /// <summary>
        /// Revert a patched method
        /// </summary>
        /// <param name="original">The method that was matched</param>
        /// <param name="replacement">The method used to patch <paramref name="original"/></param>
        public void Unpatch(MethodInfo original, MethodInfo replacement)
        {
            try
            {
                Instance.Unpatch(original, replacement);
            }
            catch (Exception e)
            {
                throw new QPatcherUnpatchException(original, replacement, e);
            }
        }
    }

    public class QPatcherException : Exception
    {
        protected string message;

        public override string ToString()
        {
            return message;
        }

        //public QPatcherException() : base()
        //{
        //}

        //public QPatcherException(string message) : base(message)
        //{
        //}

        //public QPatcherException(string message, Exception innerException) : base(message, innerException)
        //{
        //}

        //public QPatcherException(string type, MethodInfo original, MethodInfo replacement)
        //{
        //}
    }

    public class QPatcherPrefixException : QPatcherException
    {
        public QPatcherPrefixException(MethodInfo original, MethodInfo replacement, Exception exception)
        {
            string orig = (original != null) ? original.Name : "<null>";
            string replace = (replacement != null) ? replacement.Name : "<null>";
            message = $"Failed prefix patch:\n{orig} -> {replace}\n{exception.ToString()}";
        }
    }

    public class QPatcherPostfixException : QPatcherException
    {
        public QPatcherPostfixException(MethodInfo original, MethodInfo replacement, Exception exception)
        {
            string orig = (original != null) ? original.Name : "<null>";
            string replace = (replacement != null) ? replacement.Name : "<null>";
            message = $"Failed postfix patch:\n{orig} -> {replace}\n{exception.ToString()}";
        }
    }

    public class QPatcherUnpatchException : QPatcherException
    {
        public QPatcherUnpatchException(MethodInfo original, MethodInfo replacement, Exception exception)
        {
            string orig = (original != null) ? original.Name : "<null>";
            string replace = (replacement != null) ? replacement.Name : "<null>";
            message = $"Failed to unpatch:\n{orig} -> {replace}\n{exception.ToString()}";
        }
    }
}

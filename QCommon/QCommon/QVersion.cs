using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace QCommonLib
{
    public class QVersion
    {
        public static string MinorVersion(Assembly ass) => MajorVersion(ass) + "." + ass.GetName().Version.Build;
        public static string MajorVersion(Assembly ass) => ass.GetName().Version.Major + "." + ass.GetName().Version.Minor;
        public static string FullVersion(Assembly ass) => MinorVersion(ass) + " r" + ass.GetName().Version.Revision;

        public static string Version()
        {
            Assembly ass = Assembly.GetExecutingAssembly();
            if (ass.GetName().Version.Minor == 0 && ass.GetName().Version.Build == 0)
            {
                return ass.GetName().Version.Major.ToString() + ".0";
            }
            if (ass.GetName().Version.Build > 0)
            {
                return MinorVersion(ass);
            }
            else
            {
                return MajorVersion(ass);
            }
        }
    }
}

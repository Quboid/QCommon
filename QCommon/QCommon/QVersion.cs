using System.Reflection;

namespace QCommonLib
{
    public class QVersion
    {
        public static string MinorVersion(Assembly ass) => MajorVersion(ass) + "." + ass.GetName().Version.Build; // 1.0.0.23456 becomes 1.0.0
        public static string MajorVersion(Assembly ass) => ass.GetName().Version.Major + "." + ass.GetName().Version.Minor; // 1.0.0.23456 becomes 1.0
        public static string FullVersion(Assembly ass) => MinorVersion(ass) + " r" + ass.GetName().Version.Revision; // 1.0.0.23456 becomes 1.0.0.23456

        /// <summary>
        /// Get the calling mod's version in Major.Minor.Build format (eg 1.2.3.45678 becomes 1.2.3)
        /// If build is 0, it is excluded (eg 1.2.0.34567 becomes 1.2)
        /// </summary>
        /// <returns>Version as string</returns>
        public static string Version()
        {
            return Version(Assembly.GetCallingAssembly());
        }

        /// <summary>
        /// Get the calling mod's version in Major.Minor.Build format (eg 1.2.3.45678 becomes 1.2.3)
        /// If build is 0, it is excluded (eg 1.2.0.34567 becomes 1.2)
        /// </summary>
        /// <param name="ass">The assembly to get version from</param>
        /// <returns>Version as string</returns>
        public static string Version(Assembly ass)
        {
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

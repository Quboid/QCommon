namespace QCommonLib.Lang
{
	public class QStr
	{
		public static System.Globalization.CultureInfo Culture {get; set;}
		public static QCommonLib.Lang.LocalizeManager LocaleManager {get;} = new QCommonLib.Lang.LocalizeManager("QStr", typeof(QStr).Assembly, "Common");

		/// <summary>
		/// Cancel
		/// </summary>
		public static string QCommon_cancel => LocaleManager.GetString("QCommon_cancel", Culture);

		/// <summary>
		/// No
		/// </summary>
		public static string QCommon_no => LocaleManager.GetString("QCommon_no", Culture);

		/// <summary>
		/// OK
		/// </summary>
		public static string QCommon_ok => LocaleManager.GetString("QCommon_ok", Culture);

		/// <summary>
		/// Press Any Key
		/// </summary>
		public static string QCommon_PressAnyKey => LocaleManager.GetString("QCommon_PressAnyKey", Culture);

		/// <summary>
		/// English
		/// </summary>
		public static string QCommon_ThisLanguage => LocaleManager.GetString("QCommon_ThisLanguage", Culture);

		/// <summary>
		/// Yes
		/// </summary>
		public static string QCommon_yes => LocaleManager.GetString("QCommon_yes", Culture);

		/// <summary>
		/// Delete Mod
		/// </summary>
		public static string QIncompatible_DeleteMod => LocaleManager.GetString("QIncompatible_DeleteMod", Culture);

		/// <summary>
		/// List of mods changed (deleted or unsubscribed).
		/// </summary>
		public static string QIncompatible_RestartBlurb => LocaleManager.GetString("QIncompatible_RestartBlurb", Culture);

		/// <summary>
		/// Game Restart Required
		/// </summary>
		public static string QIncompatible_RestartTitle => LocaleManager.GetString("QIncompatible_RestartTitle", Culture);

		/// <summary>
		/// Incompatible Mods Check
		/// </summary>
		public static string QIncompatible_Title => LocaleManager.GetString("QIncompatible_Title", Culture);

		/// <summary>
		/// Unsubscribe Mod
		/// </summary>
		public static string QIncompatible_UnsubMod => LocaleManager.GetString("QIncompatible_UnsubMod", Culture);
	}
}
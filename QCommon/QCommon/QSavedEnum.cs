using ColossalFramework;
using System;

namespace QCommonLib
{
    /// <summary>
    /// Wrapper for SavedString so it can load and save Enum values
    /// </summary>
    /// <typeparam name="T">Enum type to save</typeparam>
    public class QSavedEnum<T> where T : Enum
    {
        protected SavedString m_savedString;

        public QSavedEnum(string name, string fileName, T def, bool autoUpdate)
        {
            m_savedString = new SavedString(name, fileName, GetString(def), autoUpdate);
        }

        public T Value
        {
            get
            {
                return GetEnum(m_savedString.value);
            }
            set
            {
                m_savedString.value = GetString(value);
            }
        }

        /// <summary>
        /// Get the index of current value (0-based)
        /// </summary>
        /// <returns>Index number</returns>
        public int GetIndex()
        {
            int i = 0;
            foreach (string s in Enum.GetNames(typeof(T)))
            {
                if (s == m_savedString.value) break;
                i++;
            }
            return i;
        }

        /// <summary>
        /// Set the current value to the enum entry with the given index (0-based)
        /// </summary>
        /// <param name="i">The index to set</param>
        public void SetIndex(int i)
        {
            m_savedString.value = Enum.GetName(typeof(T), i);
        }

        /// <summary>
        /// Get the string of the given value
        /// </summary>
        /// <param name="e">The choosen enum entry</param>
        /// <returns>The enum's string</returns>
        protected static string GetString(T e)
        {
            return e.ToString();
        }

        /// <summary>
        /// Get the enum entry of the given string
        /// </summary>
        /// <param name="s">The string to find</param>
        /// <returns>The enum entry</returns>
        protected static T GetEnum(string s)
        {
            return (T)Enum.Parse(typeof(T), s);
        }
    }
}

using System;

namespace StarWorld.FrameWork
{
    /// <summary>
    /// Provide a single instance of the specified type T.
    /// </summary>
    /// <typeparam name="T">Specified type.</typeparam>
    public class Singleton<T>
        where T : new()
    {
        #region Nested Class
        /// <summary>
        /// Inner singleton provide instance.
        /// </summary>
        private class InnerSingleton
        {
            #region Property
            /// <summary>
            /// Single instance of the specified type T created by that type's default constructor.
            /// </summary>
            internal static readonly T _Instance = new T();
            #endregion

            #region Static Method
            /// <summary>
            /// Explicit static constructor to tell C# compiler not to mark type as beforefieldinit.
            /// </summary>
            static InnerSingleton() { }
            #endregion
        }
        #endregion

        #region Property
        /// <summary>
        /// Single instance of the specified type T.
        /// </summary>
        public static T Instance
        {
            get { return InnerSingleton._Instance; }
        }
        #endregion
    }
}

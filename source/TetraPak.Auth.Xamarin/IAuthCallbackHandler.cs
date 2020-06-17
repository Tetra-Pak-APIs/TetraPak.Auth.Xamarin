using System;

namespace TetraPak.Auth.Xamarin
{
    /// <summary>
    ///   For internal use by platform code.
    /// </summary>
    public interface IAuthCallbackHandler
    {
        /// <summary>
        ///   For internal use by platform code.
        /// </summary>
        /// <param name="uri"></param>
        void HandleUrlCallback(Uri uri);
    }
}
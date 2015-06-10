using System;
using System.Runtime.InteropServices;
using System.Security;

namespace IndigoWord.Utility
{
	/// <summary>
	/// Wrapper around Win32 functions.
	/// </summary>
	static class Win32
	{
		/// <summary>
		/// Gets the caret blink time.
		/// </summary>
		public static TimeSpan CaretBlinkTime {
			get { return TimeSpan.FromMilliseconds(SafeNativeMethods.GetCaretBlinkTime()); }
		}
		
		[SuppressUnmanagedCodeSecurity]
		static class SafeNativeMethods
		{
			[DllImport("user32.dll")]
			public static extern int GetCaretBlinkTime();
		}
	}
}

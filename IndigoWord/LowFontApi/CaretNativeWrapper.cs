using System;
using System.Runtime.InteropServices;
using System.Security;

namespace IndigoWord.LowFontApi
{
	/// <summary>
	/// Wrapper around CaretNativeWrapper functions.
	/// </summary>
	static class CaretNativeWrapper
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

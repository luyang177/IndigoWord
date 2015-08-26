using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using Draw = System.Drawing;

namespace IndigoWord.LowFontApi
{
	/// <summary>
	/// Native API required for IME support.
	/// </summary>
	static class ImeNativeWrapper
	{
		[StructLayout(LayoutKind.Sequential)]
		struct CompositionForm
		{
			public int dwStyle;
			public POINT ptCurrentPos;
			public RECT rcArea;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		struct POINT
		{
			public int x;
			public int y;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		struct RECT
		{
			public int left;
			public int top;
			public int right;
			public int bottom;
		}
		
		[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
		struct LOGFONT
		{
			public int lfHeight;
			public int lfWidth;
			public int lfEscapement;
			public int lfOrientation;
			public int lfWeight;
			public byte lfItalic;
			public byte lfUnderline;
			public byte lfStrikeOut;
			public byte lfCharSet;
			public byte lfOutPrecision;
			public byte lfClipPrecision;
			public byte lfQuality;
			public byte lfPitchAndFamily;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst=32)] public string lfFaceName;
		}
		
		const int CPS_CANCEL = 0x4;
		const int NI_COMPOSITIONSTR = 0x15;
		const int GCS_COMPSTR = 0x0008;
		
		public const int WM_IME_COMPOSITION = 0x10F;
		public const int WM_IME_SETCONTEXT = 0x281;
		public const int WM_INPUTLANGCHANGE = 0x51;
		
		[DllImport("imm32.dll")]
		public static extern IntPtr ImmAssociateContext(IntPtr hWnd, IntPtr hIMC);
		[DllImport("imm32.dll")]
		internal static extern IntPtr ImmGetContext(IntPtr hWnd);
		[DllImport("imm32.dll")]
		internal static extern IntPtr ImmGetDefaultIMEWnd(IntPtr hWnd);
		[DllImport("imm32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool ImmReleaseContext(IntPtr hWnd, IntPtr hIMC);
		[DllImport("imm32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool ImmNotifyIME(IntPtr hIMC, int dwAction, int dwIndex, int dwValue = 0);
		[DllImport("imm32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool ImmSetCompositionWindow(IntPtr hIMC, ref CompositionForm form);
		[DllImport("imm32.dll", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool ImmSetCompositionFont(IntPtr hIMC, ref LOGFONT font);
		[DllImport("imm32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool ImmGetCompositionFont(IntPtr hIMC, out LOGFONT font);
		
		[DllImport("msctf.dll")]
		static extern int TF_CreateThreadMgr(out ITfThreadMgr threadMgr);
		
		[ThreadStatic] static bool textFrameworkThreadMgrInitialized;
		[ThreadStatic] static ITfThreadMgr textFrameworkThreadMgr;
		
		public static ITfThreadMgr GetTextFrameworkThreadManager()
		{
			if (!textFrameworkThreadMgrInitialized) {
				textFrameworkThreadMgrInitialized = true;
				TF_CreateThreadMgr(out textFrameworkThreadMgr);
			}
			return textFrameworkThreadMgr;
		}
		
		public static bool NotifyIme(IntPtr hIMC)
		{
			return ImmNotifyIME(hIMC, NI_COMPOSITIONSTR, CPS_CANCEL);
		}

        public static bool SetCompositionWindow(HwndSource source, IntPtr hIMC, Point point)
        {
            //https://msdn.microsoft.com/en-us/library/windows/desktop/dd317764(v=vs.85).aspx
            var form = new CompositionForm();
            form.dwStyle = 0x0020; //CFS_FORCE_POSITION 
            form.ptCurrentPos.x = (int)point.X;
            form.ptCurrentPos.y = (int)point.Y;
            return ImmSetCompositionWindow(hIMC, ref form);
        }

        /*
         * For the ime which doesn't provide font itself, like windows default, however, the ime like suogou doesn't need this method.
         */
        public static bool SetCompositionFont(HwndSource source, IntPtr hIMC, string faceName, int fontHeight)
        {
            var lf = new LOGFONT();
            lf.lfFaceName = faceName;
            lf.lfHeight = fontHeight;
            return ImmSetCompositionFont(hIMC, ref lf);
        }
		
		static readonly Rect EMPTY_RECT = new Rect(0, 0, 0, 0);
	}
	
	[ComImport, Guid("aa80e801-2021-11d2-93e0-0060b067b86e"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	interface ITfThreadMgr
	{
		void Activate(out int clientId);
		void Deactivate();
		void CreateDocumentMgr(out IntPtr docMgr);
		void EnumDocumentMgrs(out IntPtr enumDocMgrs);
		void GetFocus(out IntPtr docMgr);
		void SetFocus(IntPtr docMgr);
		void AssociateFocus(IntPtr hwnd, IntPtr newDocMgr, out IntPtr prevDocMgr);
		void IsThreadFocus([MarshalAs(UnmanagedType.Bool)] out bool isFocus);
		void GetFunctionProvider(ref Guid classId, out IntPtr funcProvider);
		void EnumFunctionProviders(out IntPtr enumProviders);
		void GetGlobalCompartment(out IntPtr compartmentMgr);
	}
}

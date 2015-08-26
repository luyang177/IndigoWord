using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using IndigoWord.LowFontApi;

namespace IndigoWord.Core
{
    /*
     * Method CreateContext, WndProc and UpdateCompositionWindow are important.
     */
    class ImeSupport
	{
        readonly FrameworkElement _view;
		IntPtr currentContext;
		IntPtr previousContext;
		IntPtr defaultImeWnd;
		HwndSource hwndSource;
		EventHandler requerySuggestedHandler; // we need to keep the event handler instance alive because CommandManager.RequerySuggested uses weak references
		//bool isReadOnly;

	    private double _heightOffset;
	    private ICaretPosition _caretPosition;
	    private FontRendering _fontRendering;

        public ImeSupport(FrameworkElement view, 
                          double heightOffset,
                          ICaretPosition caretPosition,
                          FontRendering fontRendering)
		{
            if (view == null)
                throw new ArgumentNullException("view");
            _view = view;

            _heightOffset = heightOffset;

            if (caretPosition == null)
                throw new ArgumentNullException("caretPosition");
            _caretPosition = caretPosition;

            if (fontRendering == null)
                throw new ArgumentNullException("fontRendering");
            _fontRendering = fontRendering;

            InputMethod.SetIsInputMethodSuspended(view, true);
			// We listen to CommandManager.RequerySuggested for both caret offset changes and changes to the set of read-only sections.
			// This is because there's no dedicated event for read-only section changes; but RequerySuggested needs to be raised anyways
			// to invalidate the Paste command.
			requerySuggestedHandler = OnRequerySuggested;
			CommandManager.RequerySuggested += requerySuggestedHandler;
			//textArea.OptionChanged += TextAreaOptionChanged;
		}

		void OnRequerySuggested(object sender, EventArgs e)
		{
			UpdateImeEnabled();
		}
		
		void TextAreaOptionChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "EnableImeSupport") {
                InputMethod.SetIsInputMethodSuspended(_view, true);
				UpdateImeEnabled();
			}
		}
		
		public void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
		{
			UpdateImeEnabled();
		}
		
		public void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
		{
			if (e.OldFocus == _view && currentContext != IntPtr.Zero)
				ImeNativeWrapper.NotifyIme(currentContext);
			ClearContext();
		}
		
		void UpdateImeEnabled()
		{
			//if (_view.IsKeyboardFocused) {
            if (true)
            {
				if (hwndSource == null) {
					ClearContext(); // clear existing context (on read-only change)
					//isReadOnly = newReadOnly;
					CreateContext();
				}
			} else {
				ClearContext();
			}
		}
		
		void ClearContext()
		{
			if (hwndSource != null) {
				ImeNativeWrapper.ImmAssociateContext(hwndSource.Handle, previousContext);
				ImeNativeWrapper.ImmReleaseContext(defaultImeWnd, currentContext);
				currentContext = IntPtr.Zero;
				defaultImeWnd = IntPtr.Zero;
				hwndSource.RemoveHook(WndProc);
				hwndSource = null;
			}
		}
		
		void CreateContext()
		{
			hwndSource = (HwndSource)PresentationSource.FromVisual(_view);
			if (hwndSource != null) {
				//if (isReadOnly) {
                if (false)
                {
					defaultImeWnd = IntPtr.Zero;
					currentContext = IntPtr.Zero;
				} else {
					defaultImeWnd = ImeNativeWrapper.ImmGetDefaultIMEWnd(IntPtr.Zero);
					currentContext = ImeNativeWrapper.ImmGetContext(defaultImeWnd);
				}
				previousContext = ImeNativeWrapper.ImmAssociateContext(hwndSource.Handle, currentContext);
				hwndSource.AddHook(WndProc);
				// UpdateCompositionWindow() will be called by the caret becoming visible
				
				var threadMgr = ImeNativeWrapper.GetTextFrameworkThreadManager();
				if (threadMgr != null) {
					// Even though the docu says passing null is invalid, this seems to help
					// activating the IME on the default input context that is shared with WPF
					threadMgr.SetFocus(IntPtr.Zero);
				}
			}
		}
		
		IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			switch (msg) {
				case ImeNativeWrapper.WM_INPUTLANGCHANGE:
					// Don't mark the message as handled; other windows
					// might want to handle it as well.
					
					// If we have a context, recreate it
					if (hwndSource != null) {
						ClearContext();
						CreateContext();
					}
					break;
				case ImeNativeWrapper.WM_IME_COMPOSITION:
					UpdateCompositionWindow();
					break;
			}
			return IntPtr.Zero;
		}
		
		public void UpdateCompositionWindow()
		{
            if (currentContext != IntPtr.Zero)
            {
                var faceName = _fontRendering.Typeface.FontFamily.ToString();
                var fontHeight = (int) _caretPosition.CaretRect.Height;
                ImeNativeWrapper.SetCompositionFont(hwndSource, currentContext, faceName, fontHeight);

                var x = _caretPosition.CaretRect.Left;
                var y = _caretPosition.CaretRect.Top + _heightOffset;
                var point = new Point(x, y);
                ImeNativeWrapper.SetCompositionWindow(hwndSource, currentContext, point);
            }
		}
	}
}

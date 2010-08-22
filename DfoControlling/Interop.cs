using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Dfo.Controlling
{
	internal static class Interop
	{
		[DllImport( "user32.dll", SetLastError = true )]
		public static extern IntPtr FindWindow( string lpClassName, string lpWindowName );

		[DllImport( "user32.dll", SetLastError = true )]
		[return: MarshalAs( UnmanagedType.Bool )]
		public static extern bool IsWindowVisible( IntPtr hWnd );

		[DllImport( "user32.dll", SetLastError = true )]
		[return: MarshalAs( UnmanagedType.Bool )]
		static extern bool GetWindowRect( IntPtr hWnd, out RECT lpRect );

		[StructLayout( LayoutKind.Sequential )]
		public struct RECT
		{
			public int Left;        // x position of upper-left corner
			public int Top;         // y position of upper-left corner
			public int Right;       // x position of lower-right corner
			public int Bottom;      // y position of lower-right corner
		}

		[DllImport( "user32.dll", SetLastError = true )]
		[return: MarshalAs( UnmanagedType.Bool )]
		public static extern bool SetWindowPos( IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy,
			uint uFlags );

		public static readonly IntPtr HWND_TOPMOST = new IntPtr( -1 );
		public static readonly IntPtr HWND_NOTOPMOST = new IntPtr( -2 );
		public static readonly IntPtr HWND_TOP = new IntPtr( 0 );
		public static readonly IntPtr HWND_BOTTOM = new IntPtr( 1 );

		public const UInt32 SWP_NOSIZE = 0x0001;
		public const UInt32 SWP_NOMOVE = 0x0002;
		public const UInt32 SWP_NOZORDER = 0x0004;
		public const UInt32 SWP_NOREDRAW = 0x0008;
		public const UInt32 SWP_NOACTIVATE = 0x0010;
		public const UInt32 SWP_FRAMECHANGED = 0x0020;  /* The frame changed: send WM_NCCALCSIZE */
		public const UInt32 SWP_DRAWFRAME = 0x0020;
		public const UInt32 SWP_SHOWWINDOW = 0x0040;
		public const UInt32 SWP_HIDEWINDOW = 0x0080;
		public const UInt32 SWP_NOCOPYBITS = 0x0100;
		public const UInt32 SWP_NOREPOSITION = 0x0200;
		public const UInt32 SWP_NOOWNERZORDER = 0x0200;  /* Don't do owner Z ordering */
		public const UInt32 SWP_NOSENDCHANGING = 0x0400;  /* Don't send WM_WINDOWPOSCHANGING */
		public const UInt32 SWP_DEFERERASE = 0x2000;
		public const UInt32 SWP_ASYNCWINDOWPOS = 0x4000;
	}
}

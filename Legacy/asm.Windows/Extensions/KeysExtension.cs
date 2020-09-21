using System;
using System.Collections.Generic;
using System.Windows.Forms;

// ReSharper disable once CheckNamespace
namespace asm.Extensions
{
	public static class KeysExtension
	{
		private static readonly ISet<Keys> KEYS_FUNCTION = new HashSet<Keys>
		{
			Keys.LButton,
			Keys.RButton,
			Keys.Cancel,
			Keys.MButton,
			Keys.XButton1,
			Keys.XButton2,
			Keys.Tab,
			Keys.ShiftKey,
			Keys.ControlKey,
			Keys.Menu,
			Keys.Pause,
			Keys.Capital,
			Keys.CapsLock,
			Keys.LineFeed,
			Keys.KanaMode,
			Keys.HanguelMode,
			Keys.HangulMode,
			Keys.JunjaMode,
			Keys.FinalMode,
			Keys.HanjaMode,
			Keys.KanjiMode,
			Keys.Escape,
			Keys.IMEConvert,
			Keys.IMENonconvert,
			Keys.IMEAccept,
			Keys.IMEAceept,
			Keys.IMEModeChange,
			Keys.Prior,
			Keys.Select,
			Keys.Print,
			Keys.Execute,
			Keys.Snapshot,
			Keys.PrintScreen,
			Keys.Insert,
			Keys.Help,
			Keys.LWin,
			Keys.RWin,
			Keys.Apps,
			Keys.Sleep,
			Keys.F1,
			Keys.F2,
			Keys.F3,
			Keys.F4,
			Keys.F5,
			Keys.F6,
			Keys.F7,
			Keys.F8,
			Keys.F9,
			Keys.F10,
			Keys.F11,
			Keys.F12,
			Keys.F13,
			Keys.F14,
			Keys.F15,
			Keys.F16,
			Keys.F17,
			Keys.F18,
			Keys.F19,
			Keys.F20,
			Keys.F21,
			Keys.F22,
			Keys.F23,
			Keys.F24,
			Keys.NumLock,
			Keys.Scroll,
			Keys.LShiftKey,
			Keys.RShiftKey,
			Keys.LControlKey,
			Keys.RControlKey,
			Keys.LMenu,
			Keys.RMenu,
			Keys.BrowserBack,
			Keys.BrowserForward,
			Keys.BrowserRefresh,
			Keys.BrowserStop,
			Keys.BrowserSearch,
			Keys.BrowserFavorites,
			Keys.BrowserHome,
			Keys.VolumeMute,
			Keys.VolumeDown,
			Keys.VolumeUp,
			Keys.MediaNextTrack,
			Keys.MediaPreviousTrack,
			Keys.MediaStop,
			Keys.MediaPlayPause,
			Keys.LaunchMail,
			Keys.SelectMedia,
			Keys.LaunchApplication1,
			Keys.LaunchApplication2,
			Keys.Oem102,
			Keys.ProcessKey,
			Keys.Packet,
			Keys.Attn,
			Keys.Crsel,
			Keys.Exsel,
			Keys.Play,
			Keys.Zoom,
			Keys.NoName,
			Keys.Pa1,
			Keys.Shift,
			Keys.Control,
			Keys.Alt
		};

		private static readonly ISet<Keys> KEYS_NAVIGATION = new HashSet<Keys>
		{
			Keys.PageUp,
			Keys.Next,
			Keys.PageDown,
			Keys.End,
			Keys.Home,
			Keys.Left,
			Keys.Up,
			Keys.Right,
			Keys.Down
		};

		private static readonly ISet<Keys> KEYS_TYPING = new HashSet<Keys>
		{
			Keys.Space,
			Keys.D0,
			Keys.D1,
			Keys.D2,
			Keys.D3,
			Keys.D4,
			Keys.D5,
			Keys.D6,
			Keys.D7,
			Keys.D8,
			Keys.D9,
			Keys.A,
			Keys.B,
			Keys.C,
			Keys.D,
			Keys.E,
			Keys.F,
			Keys.G,
			Keys.H,
			Keys.I,
			Keys.J,
			Keys.K,
			Keys.L,
			Keys.M,
			Keys.N,
			Keys.O,
			Keys.P,
			Keys.Q,
			Keys.R,
			Keys.S,
			Keys.T,
			Keys.U,
			Keys.V,
			Keys.W,
			Keys.X,
			Keys.Y,
			Keys.Z,
			Keys.NumPad0,
			Keys.NumPad1,
			Keys.NumPad2,
			Keys.NumPad3,
			Keys.NumPad4,
			Keys.NumPad5,
			Keys.NumPad6,
			Keys.NumPad7,
			Keys.NumPad8,
			Keys.NumPad9,
			Keys.Multiply,
			Keys.Add,
			Keys.Separator,
			Keys.Subtract,
			Keys.Decimal,
			Keys.Divide,
			Keys.OemSemicolon,
			Keys.Oem1,
			Keys.Oemplus,
			Keys.Oemcomma,
			Keys.OemMinus,
			Keys.OemPeriod,
			Keys.OemQuestion,
			Keys.Oem2,
			Keys.Oemtilde,
			Keys.Oem3,
			Keys.OemOpenBrackets,
			Keys.Oem4,
			Keys.OemPipe,
			Keys.Oem5,
			Keys.OemCloseBrackets,
			Keys.Oem6,
			Keys.OemQuotes,
			Keys.Oem7,
			Keys.Oem8,
			Keys.OemBackslash
		};

		private static readonly ISet<Keys> KEYS_REMOVE = new HashSet<Keys>
		{
			Keys.Space,
			Keys.Delete,
			Keys.EraseEof,
			Keys.OemClear
		};

		public static char ToChar(this Keys thisValue)
		{
			int nonVirtualKey = Win32.MapVirtualKey((uint)thisValue, 2);
			return Convert.ToChar(nonVirtualKey);
		}

		public static bool IsFunction(this Keys thisValue) { return KEYS_FUNCTION.Contains(thisValue); }

		public static bool IsNavigation(this Keys thisValue) { return KEYS_NAVIGATION.Contains(thisValue); }

		public static bool IsTyping(this Keys thisValue) { return KEYS_TYPING.Contains(thisValue); }

		public static bool IsRemove(this Keys thisValue) { return KEYS_REMOVE.Contains(thisValue); }

		public static bool IsBackspace(this Keys thisValue) { return thisValue == Keys.Back; }

		public static bool IsDelete(this Keys thisValue) { return thisValue == Keys.Delete; }

		public static bool IsEnter(this Keys thisValue) { return thisValue == Keys.Return; }
	}
}
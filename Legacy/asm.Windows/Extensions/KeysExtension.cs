using System;
using System.Windows.Forms;

// ReSharper disable once CheckNamespace
namespace asm.Extensions
{
	public static class KeysExtension
	{
		public static char ToChar(this Keys thisValue)
		{
			int nonVirtualKey = Win32.MapVirtualKey((uint)thisValue, 2);
			return Convert.ToChar(nonVirtualKey);
		}

		public static bool IsFunction(this Keys thisValue)
		{
			switch (thisValue)
			{
				case Keys.LButton:
				case Keys.RButton:
				case Keys.Cancel:
				case Keys.MButton:
				case Keys.XButton1:
				case Keys.XButton2:
				case Keys.Tab:
				case Keys.ShiftKey:
				case Keys.ControlKey:
				case Keys.Menu:
				case Keys.Pause:
				case Keys.CapsLock:
				case Keys.LineFeed:
				case Keys.HangulMode:
				case Keys.JunjaMode:
				case Keys.FinalMode:
				case Keys.KanjiMode:
				case Keys.Escape:
				case Keys.IMEConvert:
				case Keys.IMENonconvert:
				case Keys.IMEAccept:
				case Keys.IMEModeChange:
				case Keys.Prior:
				case Keys.Select:
				case Keys.Print:
				case Keys.Execute:
				case Keys.PrintScreen:
				case Keys.Insert:
				case Keys.Help:
				case Keys.LWin:
				case Keys.RWin:
				case Keys.Apps:
				case Keys.Sleep:
				case Keys.F1:
				case Keys.F2:
				case Keys.F3:
				case Keys.F4:
				case Keys.F5:
				case Keys.F6:
				case Keys.F7:
				case Keys.F8:
				case Keys.F9:
				case Keys.F10:
				case Keys.F11:
				case Keys.F12:
				case Keys.F13:
				case Keys.F14:
				case Keys.F15:
				case Keys.F16:
				case Keys.F17:
				case Keys.F18:
				case Keys.F19:
				case Keys.F20:
				case Keys.F21:
				case Keys.F22:
				case Keys.F23:
				case Keys.F24:
				case Keys.NumLock:
				case Keys.Scroll:
				case Keys.LShiftKey:
				case Keys.RShiftKey:
				case Keys.LControlKey:
				case Keys.RControlKey:
				case Keys.LMenu:
				case Keys.RMenu:
				case Keys.BrowserBack:
				case Keys.BrowserForward:
				case Keys.BrowserRefresh:
				case Keys.BrowserStop:
				case Keys.BrowserSearch:
				case Keys.BrowserFavorites:
				case Keys.BrowserHome:
				case Keys.VolumeMute:
				case Keys.VolumeDown:
				case Keys.VolumeUp:
				case Keys.MediaNextTrack:
				case Keys.MediaPreviousTrack:
				case Keys.MediaStop:
				case Keys.MediaPlayPause:
				case Keys.LaunchMail:
				case Keys.SelectMedia:
				case Keys.LaunchApplication1:
				case Keys.LaunchApplication2:
				case Keys.Oem102:
				case Keys.ProcessKey:
				case Keys.Packet:
				case Keys.Attn:
				case Keys.Crsel:
				case Keys.Exsel:
				case Keys.Play:
				case Keys.Zoom:
				case Keys.NoName:
				case Keys.Pa1:
				case Keys.Shift:
				case Keys.Control:
				case Keys.Alt:
					return true;
				default:
					return false;
			}
		}

		public static bool IsNavigation(this Keys thisValue)
		{
			switch (thisValue)
			{
				case Keys.PageUp:
				case Keys.PageDown:
				case Keys.End:
				case Keys.Home:
				case Keys.Left:
				case Keys.Up:
				case Keys.Right:
				case Keys.Down:
					return true;
				default:
					return false;
			}
		}

		public static bool IsTyping(this Keys thisValue)
		{
			switch (thisValue)
			{
				case Keys.Space:
				case Keys.D0:
				case Keys.D1:
				case Keys.D2:
				case Keys.D3:
				case Keys.D4:
				case Keys.D5:
				case Keys.D6:
				case Keys.D7:
				case Keys.D8:
				case Keys.D9:
				case Keys.A:
				case Keys.B:
				case Keys.C:
				case Keys.D:
				case Keys.E:
				case Keys.F:
				case Keys.G:
				case Keys.H:
				case Keys.I:
				case Keys.J:
				case Keys.K:
				case Keys.L:
				case Keys.M:
				case Keys.N:
				case Keys.O:
				case Keys.P:
				case Keys.Q:
				case Keys.R:
				case Keys.S:
				case Keys.T:
				case Keys.U:
				case Keys.V:
				case Keys.W:
				case Keys.X:
				case Keys.Y:
				case Keys.Z:
				case Keys.NumPad0:
				case Keys.NumPad1:
				case Keys.NumPad2:
				case Keys.NumPad3:
				case Keys.NumPad4:
				case Keys.NumPad5:
				case Keys.NumPad6:
				case Keys.NumPad7:
				case Keys.NumPad8:
				case Keys.NumPad9:
				case Keys.Multiply:
				case Keys.Add:
				case Keys.Separator:
				case Keys.Subtract:
				case Keys.Decimal:
				case Keys.Divide:
				case Keys.Oem1:
				case Keys.Oemplus:
				case Keys.Oemcomma:
				case Keys.OemMinus:
				case Keys.OemPeriod:
				case Keys.Oem2:
				case Keys.Oem3:
				case Keys.Oem4:
				case Keys.Oem5:
				case Keys.Oem6:
				case Keys.Oem7:
				case Keys.Oem8:
				case Keys.OemBackslash:
					return true;
				default:
					return false;
			}
		}

		public static bool IsRemove(this Keys thisValue)
		{
			switch (thisValue)
			{
				case Keys.Space:
				case Keys.Delete:
				case Keys.EraseEof:
				case Keys.OemClear:
					return true;
				default:
					return false;
			}
		}

		public static bool IsBackspace(this Keys thisValue) { return thisValue == Keys.Back; }

		public static bool IsDelete(this Keys thisValue) { return thisValue == Keys.Delete; }

		public static bool IsEnter(this Keys thisValue) { return thisValue == Keys.Return; }
	}
}
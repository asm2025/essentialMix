using System;
using System.Globalization;
using System.IO;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.AccessControl;
using System.Text;
using essentialMix.Network;
using JetBrains.Annotations;

// ReSharper disable IdentifierTypo
namespace essentialMix;

#region delegates
public delegate bool EnumWindowsProcessor(IntPtr hWnd, IntPtr lParam);

public delegate bool EnumThreadWindowsProcessor(IntPtr hWnd, IntPtr lParam);

public delegate bool EnumDesktopProc(string lpszDesktop, IntPtr lParam);

public delegate bool EnumDesktopWindowsProc(IntPtr desktopHandle, IntPtr lParam);

public delegate bool ConsoleCtrlDelegate(CtrlTypes ctrlType);

public delegate void TimerCallback(uint uTimerID, uint uMsg, UIntPtr dwUser, UIntPtr dw1, UIntPtr dw2);

[UnmanagedFunctionPointer(CallingConvention.StdCall)]
public delegate int CreateObjectDelegate([In] ref Guid classID, [In] ref Guid interfaceID, [MarshalAs(UnmanagedType.Interface)] out object outObject);
#endregion

#region enums
[Flags]
public enum ErrorModesEnum : uint
{
	SYSTEM_DEFAULT = 0x0,
	SEM_FAILCRITICALERRORS = 0x0001,
	SEM_NOALIGNMENTFAULTEXCEPT = 0x0004,
	SEM_NOGPFAULTERRORBOX = 0x0002,
	SEM_NOOPENFILEERRORBOX = 0x8000
}

public enum CtrlTypes : uint
{
	CTRL_C_EVENT = 0,
	CTRL_BREAK_EVENT,
	CTRL_CLOSE_EVENT,
	CTRL_LOGOFF_EVENT = 5,
	CTRL_SHUTDOWN_EVENT
}

public enum JobObjectInfoTypeEnum
{
	AssociateCompletionPortInformation = 7,
	BasicLimitInformation = 2,
	BasicUIRestrictions = 4,
	EndOfJobTimeInformation = 6,
	ExtendedLimitInformation = 9,
	SecurityLimitInformation = 5,
	GroupInformation = 11
}

[Flags]
public enum JobObjectLimitEnum : uint
{
	JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE = 0x2000
}

[Flags]
public enum ConsoleModesEnum : uint
{
	NONE = 0x0000,
	ENABLE_PROCESSED_INPUT = 0x0001,
	ENABLE_PROCESSED_OUTPUT = 0x0001,
	ENABLE_LINE_INPUT = 0x0002,
	ENABLE_WRAP_AT_EOL_OUTPUT = 0x0002,
	ENABLE_ECHO_INPUT = 0x0004,
	ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004,
	ENABLE_WINDOW_INPUT = 0x0008,
	DISABLE_NEWLINE_AUTO_RETURN = 0x0008,
	ENABLE_MOUSE_INPUT = 0x0010,
	ENABLE_LVB_GRID_WORLDWIDE = 0x0010,
	ENABLE_INSERT_MODE = 0x0020,
	ENABLE_QUICK_EDIT_MODE = 0x0040,
	ENABLE_EXTENDED_FLAGS = 0x0080,
	ENABLE_AUTO_POSITION = 0x0100,
	ENABLE_VIRTUAL_TERMINAL_INPUT = 0x0200
}

[Flags]
public enum ControlKeyStatesEnum
{
	RIGHT_ALT_PRESSED = 0x1,
	LEFT_ALT_PRESSED = 0x2,
	RIGHT_CTRL_PRESSED = 0x4,
	LEFT_CTRL_PRESSED = 0x8,
	SHIFT_PRESSED = 0x10,
	NUMLOCK_ON = 0x20,
	SCROLLLOCK_ON = 0x40,
	CAPSLOCK_ON = 0x80,
	ENHANCED_KEY = 0x100
}

public enum VirtualKeysEnum : ushort
{
	LeftButton = 0x01,
	RightButton = 0x02,
	Cancel = 0x03,
	MiddleButton = 0x04,
	ExtraButton1 = 0x05,
	ExtraButton2 = 0x06,
	Back = 0x08,
	Tab = 0x09,
	Clear = 0x0C,
	Return = 0x0D,
	Shift = 0x10,
	Control = 0x11,
	Menu = 0x12,
	Pause = 0x13,
	CapsLock = 0x14,
	Kana = 0x15,
	Hangeul = 0x15,
	Hangul = 0x15,
	Junja = 0x17,
	Final = 0x18,
	Hanja = 0x19,
	Kanji = 0x19,
	Escape = 0x1B,
	Convert = 0x1C,
	NonConvert = 0x1D,
	Accept = 0x1E,
	ModeChange = 0x1F,
	Space = 0x20,
	Prior = 0x21,
	Next = 0x22,
	End = 0x23,
	Home = 0x24,
	Left = 0x25,
	Up = 0x26,
	Right = 0x27,
	Down = 0x28,
	Select = 0x29,
	Print = 0x2A,
	Execute = 0x2B,
	Snapshot = 0x2C,
	Insert = 0x2D,
	Delete = 0x2E,
	Help = 0x2F,
	N0 = 0x30,
	N1 = 0x31,
	N2 = 0x32,
	N3 = 0x33,
	N4 = 0x34,
	N5 = 0x35,
	N6 = 0x36,
	N7 = 0x37,
	N8 = 0x38,
	N9 = 0x39,
	A = 0x41,
	B = 0x42,
	C = 0x43,
	D = 0x44,
	E = 0x45,
	F = 0x46,
	G = 0x47,
	H = 0x48,
	I = 0x49,
	J = 0x4A,
	K = 0x4B,
	L = 0x4C,
	M = 0x4D,
	N = 0x4E,
	O = 0x4F,
	P = 0x50,
	Q = 0x51,
	R = 0x52,
	S = 0x53,
	T = 0x54,
	U = 0x55,
	V = 0x56,
	W = 0x57,
	X = 0x58,
	Y = 0x59,
	Z = 0x5A,
	LeftWindows = 0x5B,
	RightWindows = 0x5C,
	Application = 0x5D,
	Sleep = 0x5F,
	Numpad0 = 0x60,
	Numpad1 = 0x61,
	Numpad2 = 0x62,
	Numpad3 = 0x63,
	Numpad4 = 0x64,
	Numpad5 = 0x65,
	Numpad6 = 0x66,
	Numpad7 = 0x67,
	Numpad8 = 0x68,
	Numpad9 = 0x69,
	Multiply = 0x6A,
	Add = 0x6B,
	Separator = 0x6C,
	Subtract = 0x6D,
	Decimal = 0x6E,
	Divide = 0x6F,
	F1 = 0x70,
	F2 = 0x71,
	F3 = 0x72,
	F4 = 0x73,
	F5 = 0x74,
	F6 = 0x75,
	F7 = 0x76,
	F8 = 0x77,
	F9 = 0x78,
	F10 = 0x79,
	F11 = 0x7A,
	F12 = 0x7B,
	F13 = 0x7C,
	F14 = 0x7D,
	F15 = 0x7E,
	F16 = 0x7F,
	F17 = 0x80,
	F18 = 0x81,
	F19 = 0x82,
	F20 = 0x83,
	F21 = 0x84,
	F22 = 0x85,
	F23 = 0x86,
	F24 = 0x87,
	NumLock = 0x90,
	ScrollLock = 0x91,
	NEC_Equal = 0x92,
	Fujitsu_Jisho = 0x92,
	Fujitsu_Masshou = 0x93,
	Fujitsu_Touroku = 0x94,
	Fujitsu_Loya = 0x95,
	Fujitsu_Roya = 0x96,
	LeftShift = 0xA0,
	RightShift = 0xA1,
	LeftControl = 0xA2,
	RightControl = 0xA3,
	LeftMenu = 0xA4,
	RightMenu = 0xA5,
	BrowserBack = 0xA6,
	BrowserForward = 0xA7,
	BrowserRefresh = 0xA8,
	BrowserStop = 0xA9,
	BrowserSearch = 0xAA,
	BrowserFavorites = 0xAB,
	BrowserHome = 0xAC,
	VolumeMute = 0xAD,
	VolumeDown = 0xAE,
	VolumeUp = 0xAF,
	MediaNextTrack = 0xB0,
	MediaPrevTrack = 0xB1,
	MediaStop = 0xB2,
	MediaPlayPause = 0xB3,
	LaunchMail = 0xB4,
	LaunchMediaSelect = 0xB5,
	LaunchApplication1 = 0xB6,
	LaunchApplication2 = 0xB7,
	OEM1 = 0xBA,
	OEMPlus = 0xBB,
	OEMComma = 0xBC,
	OEMMinus = 0xBD,
	OEMPeriod = 0xBE,
	OEM2 = 0xBF,
	OEM3 = 0xC0,
	OEM4 = 0xDB,
	OEM5 = 0xDC,
	OEM6 = 0xDD,
	OEM7 = 0xDE,
	OEM8 = 0xDF,
	OEMAX = 0xE1,
	OEM102 = 0xE2,
	ICOHelp = 0xE3,
	ICO00 = 0xE4,
	ProcessKey = 0xE5,
	ICOClear = 0xE6,
	Packet = 0xE7,
	OEMReset = 0xE9,
	OEMJump = 0xEA,
	OEMPA1 = 0xEB,
	OEMPA2 = 0xEC,
	OEMPA3 = 0xED,
	OEMWSCtrl = 0xEE,
	OEMCUSel = 0xEF,
	OEMATTN = 0xF0,
	OEMFinish = 0xF1,
	OEMCopy = 0xF2,
	OEMAuto = 0xF3,
	OEMENLW = 0xF4,
	OEMBackTab = 0xF5,
	ATTN = 0xF6,
	CRSel = 0xF7,
	EXSel = 0xF8,
	EREOF = 0xF9,
	Play = 0xFA,
	Zoom = 0xFB,
	Noname = 0xFC,
	PA1 = 0xFD,
	OEMClear = 0xFE
}

public enum InputRecordEventTypeEnum : ushort
{
	KEY_EVENT = 0x0001,
	MOUSE_EVENT = 0x0002,
	WINDOW_BUFFER_SIZE_EVENT = 0x0004,
	MENU_EVENT = 0x0008,
	FOCUS_EVENT = 0x0010
}

[Flags]
public enum HandleFlagsEnum : uint
{
	None = 0,
	INHERIT = 1,
	PROTECT_FROM_CLOSE = 2
}

[Flags]
public enum StartupInfoFlagsEnum : uint
{
	STARTF_USESHOWWINDOW = 0x00000001,
	STARTF_USESIZE = 0x00000002,
	STARTF_USEPOSITION = 0x00000004,
	STARTF_USECOUNTCHARS = 0x00000008,
	STARTF_USEFILLATTRIBUTE = 0x00000010,
	STARTF_RUNFULLSCREEN = 0x00000020, // ignored for non-x86 platforms
	STARTF_FORCEONFEEDBACK = 0x00000040,
	STARTF_FORCEOFFFEEDBACK = 0x00000080,
	STARTF_USESTDHANDLES = 0x00000100
}

[Flags]
public enum CreateProcessFlagsEnum : uint
{
	DEBUG_PROCESS = 0x00000001,
	DEBUG_ONLY_THIS_PROCESS = 0x00000002,
	CREATE_SUSPENDED = 0x00000004,
	DETACHED_PROCESS = 0x00000008,
	CREATE_NEW_CONSOLE = 0x00000010,
	NORMAL_PRIORITY_CLASS = 0x00000020,
	IDLE_PRIORITY_CLASS = 0x00000040,
	HIGH_PRIORITY_CLASS = 0x00000080,
	REALTIME_PRIORITY_CLASS = 0x00000100,
	CREATE_NEW_PROCESS_GROUP = 0x00000200,
	CREATE_UNICODE_ENVIRONMENT = 0x00000400,
	CREATE_SEPARATE_WOW_VDM = 0x00000800,
	CREATE_SHARED_WOW_VDM = 0x00001000,
	CREATE_FORCEDOS = 0x00002000,
	BELOW_NORMAL_PRIORITY_CLASS = 0x00004000,
	ABOVE_NORMAL_PRIORITY_CLASS = 0x00008000,
	INHERIT_PARENT_AFFINITY = 0x00010000,
	INHERIT_CALLER_PRIORITY = 0x00020000,
	CREATE_PROTECTED_PROCESS = 0x00040000,
	EXTENDED_STARTUPINFO_PRESENT = 0x00080000,
	PROCESS_MODE_BACKGROUND_BEGIN = 0x00100000,
	PROCESS_MODE_BACKGROUND_END = 0x00200000,
	CREATE_BREAKAWAY_FROM_JOB = 0x01000000,
	CREATE_PRESERVE_CODE_AUTHZ_LEVEL = 0x02000000,
	CREATE_DEFAULT_ERROR_MODE = 0x04000000,
	CREATE_NO_WINDOW = 0x08000000,
	PROFILE_USER = 0x10000000,
	PROFILE_KERNEL = 0x20000000,
	PROFILE_SERVER = 0x40000000,
	CREATE_IGNORE_SYSTEM_DEFAULT = 0x80000000
}

[Flags]
public enum PipeOpenModeFlagsEnum : uint
{
	PIPE_ACCESS_DUPLEX = 0x00000003,
	PIPE_ACCESS_INBOUND = 0x00000001,
	PIPE_ACCESS_OUTBOUND = 0x00000002,
	FILE_FLAG_FIRST_PIPE_INSTANCE = 0x00080000,
	FILE_FLAG_WRITE_THROUGH = 0x80000000,
	FILE_FLAG_OVERLAPPED = 0x40000000,
	WRITE_DAC = 0x00040000,
	WRITE_OWNER = 0x00080000,
	ACCESS_SYSTEM_SECURITY = 0x01000000
}

[Flags]
public enum PipeModeFlagsEnum : uint
{
	//One of the following type modes can be specified. The same type mode must be specified for each instance of the pipe.
	PIPE_TYPE_BYTE = 0x00000000,
	PIPE_TYPE_MESSAGE = 0x00000004,

	//One of the following read modes can be specified. Different instances of the same pipe can specify different read modes
	PIPE_READMODE_BYTE = 0x00000000,
	PIPE_READMODE_MESSAGE = 0x00000002,

	//One of the following wait modes can be specified. Different instances of the same pipe can specify different wait modes.
	PIPE_WAIT = 0x00000000,
	PIPE_NOWAIT = 0x00000001,

	//One of the following remote-client modes can be specified. Different instances of the same pipe can specify different remote-client modes.
	PIPE_ACCEPT_REMOTE_CLIENTS = 0x00000000,
	PIPE_REJECT_REMOTE_CLIENTS = 0x00000008
}

public enum HitTestEnum
{
	HTERROR = -2,
	HTTRANSPARENT = -1,
	HTNOWHERE = 0,
	HTCLIENT = 1,
	HTCAPTION = 2,
	HTSYSMENU = 3,
	HTGROWBOX = 4,
	HTSIZE = HTGROWBOX,
	HTMENU = 5,
	HTHSCROLL = 6,
	HTVSCROLL = 7,
	HTMINBUTTON = 8,
	HTMAXBUTTON = 9,
	HTLEFT = 10,
	HTRIGHT = 11,
	HTTOP = 12,
	HTTOPLEFT = 13,
	HTTOPRIGHT = 14,
	HTBOTTOM = 15,
	HTBOTTOMLEFT = 16,
	HTBOTTOMRIGHT = 17,
	HTBORDER = 18,
	HTREDUCE = HTMINBUTTON,
	HTZOOM = HTMAXBUTTON,
	HTSIZEFIRST = HTLEFT,
	HTSIZELAST = HTBOTTOMRIGHT,

	HTOBJECT = 19,
	HTCLOSE = 20,
	HTHELP = 21
}

public enum WindowsMessagesEnum
{
	WM_MOUSEMOVE = 0x0200,
	WM_NCMOUSEMOVE = 0x00A0,
	WM_NCLBUTTONDOWN = 0x00A1,
	WM_NCLBUTTONUP = 0x00A2,
	WM_NCLBUTTONDBLCLK = 0x00A3,
	WM_LBUTTONDOWN = 0x0201,
	WM_LBUTTONUP = 0x0202,
	WM_KEYDOWN = 0x0100,
	WM_CLOSE = 0x0010,
	WM_SHOWWINDOW = 0x0010
}

public enum hWndInsertAfterEnum
{
	HWND_TOPMOST = -1,
	HWND_NOTOPMOST = -2,
	HWND_TOP = 0,
	HWND_BOTTOM = 1
}

public enum ShowWindowEnum
{
	SW_HIDE = 0,
	SW_SHOWNORMAL = 1,
	SW_NORMAL = 1,
	SW_SHOWMINIMIZED = 2,
	SW_SHOWMAXIMIZED = 3,
	SW_MAXIMIZE = 3,
	SW_SHOWNOACTIVATE = 4,
	SW_SHOW = 5,
	SW_MINIMIZE = 6,
	SW_SHOWMINNOACTIVE = 7,
	SW_SHOWNA = 8,
	SW_RESTORE = 9,
	SW_SHOWDEFAULT = 10,
	SW_FORCEMINIMIZE = 11,
	SW_MAX = 11
}

[Flags]
public enum WindowPositionFlagsEnum
{
	SWP_NOSIZE = 0x1,
	SWP_NOMOVE = 0x2,
	SWP_NOZORDER = 0x4,
	SWP_NOREDRAW = 0x0008,
	SWP_NOACTIVATE = 0x0010,
	SWP_DRAWFRAME = 0x0020,
	SWP_FRAMECHANGED = 0x0020,
	SWP_SHOWWINDOW = 0x0040,
	SWP_HIDEWINDOW = 0x0080,
	SWP_NOCOPYBITS = 0x0100,
	SWP_NOOWNERZORDER = 0x0200,
	SWP_NOREPOSITION = 0x0200,
	SWP_NOSENDCHANGING = 0x0400,
	SWP_DEFERERASE = 0x2000,
	SWP_ASYNCWINDOWPOS = 0x4000
}

public enum WindowLongSettingIndexEnum
{
	GWL_EXSTYLE = -20,
	GWL_STYLE = -16,
	GWL_WNDPROC = -4,
	GWL_HINSTANCE = -6,
	GWL_HWNDPARENT = -8,
	GWL_ID = -12,
	GWL_USERDATA = -21,
	DWL_DLGPROC = 4,
	DWL_MSGRESULT = 0,
	DWL_USER = 8
}

[Flags]
public enum WindowStylesEnum : uint
{
	WS_OVERLAPPED = 0x00000000,
	WS_POPUP = 0x80000000,
	WS_CHILD = 0x40000000,
	WS_MINIMIZE = 0x20000000,
	WS_VISIBLE = 0x10000000,
	WS_DISABLED = 0x08000000,
	WS_CLIPSIBLINGS = 0x04000000,
	WS_CLIPCHILDREN = 0x02000000,
	WS_MAXIMIZE = 0x01000000,
	WS_BORDER = 0x00800000,
	WS_DLGFRAME = 0x00400000,
	WS_VSCROLL = 0x00200000,
	WS_HSCROLL = 0x00100000,
	WS_SYSMENU = 0x00080000,
	WS_THICKFRAME = 0x00040000,
	WS_GROUP = 0x00020000,
	WS_TABSTOP = 0x00010000,

	WS_MINIMIZEBOX = 0x00020000,
	WS_MAXIMIZEBOX = 0x00010000,

	WS_CAPTION = WS_BORDER | WS_DLGFRAME,
	WS_TILED = WS_OVERLAPPED,
	WS_ICONIC = WS_MINIMIZE,
	WS_SIZEBOX = WS_THICKFRAME,
	WS_TILEDWINDOW = WS_OVERLAPPEDWINDOW,

	WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX,
	WS_POPUPWINDOW = WS_POPUP | WS_BORDER | WS_SYSMENU,
	WS_CHILDWINDOW = WS_CHILD,

	//Extended Window Styles

	WS_EX_DLGMODALFRAME = 0x00000001,
	WS_EX_NOPARENTNOTIFY = 0x00000004,
	WS_EX_TOPMOST = 0x00000008,
	WS_EX_ACCEPTFILES = 0x00000010,
	WS_EX_TRANSPARENT = 0x00000020,

	//#if(WINVER >= 0x0400)

	WS_EX_MDICHILD = 0x00000040,
	WS_EX_TOOLWINDOW = 0x00000080,
	WS_EX_WINDOWEDGE = 0x00000100,
	WS_EX_CLIENTEDGE = 0x00000200,
	WS_EX_CONTEXTHELP = 0x00000400,

	WS_EX_RIGHT = 0x00001000,
	WS_EX_LEFT = 0x00000000,
	WS_EX_RTLREADING = 0x00002000,
	WS_EX_LTRREADING = 0x00000000,
	WS_EX_LEFTSCROLLBAR = 0x00004000,
	WS_EX_RIGHTSCROLLBAR = 0x00000000,

	WS_EX_CONTROLPARENT = 0x00010000,
	WS_EX_STATICEDGE = 0x00020000,
	WS_EX_APPWINDOW = 0x00040000,

	WS_EX_OVERLAPPEDWINDOW = WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE,
	WS_EX_PALETTEWINDOW = WS_EX_WINDOWEDGE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST,
	//#endif /* WINVER >= 0x0400 */

	//#if(WIN32WINNT >= 0x0500)

	WS_EX_LAYERED = 0x00080000,
	//#endif /* WIN32WINNT >= 0x0500 */

	//#if(WINVER >= 0x0500)

	WS_EX_NOINHERITLAYOUT = 0x00100000, // Disable inheritence of mirroring by children
	WS_EX_LAYOUTRTL = 0x00400000, // Right to left mirroring
								  //#endif /* WINVER >= 0x0500 */

	//#if(WIN32WINNT >= 0x0500)

	WS_EX_COMPOSITED = 0x02000000,
	WS_EX_NOACTIVATE = 0x08000000,
	//#endif /* WIN32WINNT >= 0x0500 */

	WS_EMBEDED = WS_CHILD | WS_MAXIMIZE
}

[Flags]
public enum DialogStylesEnum : uint
{
	DS_ABSALIGN = 0x00000001,
	DS_SYSMODAL = 0x00000002,
	DS_LOCALEDIT = 0x00000020, /* Edit items get Local storage. */
	DS_SETFONT = 0x00000040, /* User specified font for Dlg controls */
	DS_MODALFRAME = 0x00000080, /* Can be combined with WS_CAPTION  */
	DS_NOIDLEMSG = 0x00000100, /* WM_ENTERIDLE message will not be sent */
	DS_SETFOREGROUND = 0x00000200 /* not in win3.1 */
}

[Flags]
public enum ShellExecuteMaskFlagsEnum
{
	SEE_MASK_DEFAULT = 0x00000000,
	SEE_MASK_CLASSNAME = 0x00000001,
	SEE_MASK_CLASSKEY = 0x00000003,
	SEE_MASK_IDLIST = 0x00000004,
	SEE_MASK_INVOKEIDLIST = 0x0000000c, // Note SEE_MASK_INVOKEIDLIST(0xC) implies SEE_MASK_IDLIST(0x04) 
	SEE_MASK_ICON = 0x00000010,
	SEE_MASK_HOTKEY = 0x00000020,
	SEE_MASK_NOCLOSEPROCESS = 0x00000040,
	SEE_MASK_CONNECTNETDRV = 0x00000080,
	SEE_MASK_FLAG_DDEWAIT = 0x00000100,
	SEE_MASK_NOASYNC = SEE_MASK_FLAG_DDEWAIT,
	SEE_MASK_DOENVSUBST = 0x00000200,
	SEE_MASK_FLAG_NO_UI = 0x00000400,
	SEE_MASK_UNICODE = 0x00004000,
	SEE_MASK_NO_CONSOLE = 0x00008000,
	SEE_MASK_ASYNCOK = 0x00100000,
	SEE_MASK_HMONITOR = 0x00200000,
	SEE_MASK_NOZONECHECKS = 0x00800000,
	SEE_MASK_NOQUERYCLASSSTORE = 0x01000000,
	SEE_MASK_WAITFORINPUTIDLE = 0x02000000,
	SEE_MASK_FLAG_LOG_USAGE = 0x04000000
}

[Flags]
public enum DesktopAccessRightsEnum : long
{
	DESKTOP_CREATEWINDOW = 0x0002L,
	DESKTOP_ENUMERATE = 0x0040L,
	DESKTOP_WRITEOBJECTS = 0x0080L,
	DESKTOP_SWITCHDESKTOP = 0x0100L,
	DESKTOP_CREATEMENU = 0x0004L,
	DESKTOP_HOOKCONTROL = 0x0008L,
	DESKTOP_READOBJECTS = 0x0001L,
	DESKTOP_JOURNALRECORD = 0x0010L,
	DESKTOP_JOURNALPLAYBACK = 0x0020L,

	DESKTOP_ALL = DESKTOP_JOURNALRECORD |
				DESKTOP_JOURNALPLAYBACK |
				DESKTOP_CREATEWINDOW |
				DESKTOP_ENUMERATE |
				DESKTOP_WRITEOBJECTS |
				DESKTOP_SWITCHDESKTOP |
				DESKTOP_CREATEMENU |
				DESKTOP_HOOKCONTROL |
				DESKTOP_READOBJECTS
}

[Flags]
public enum InternetGetConnectedStateFlags
{
	INTERNET_CONNECTION_MODEM = 0x01,
	INTERNET_CONNECTION_LAN = 0x02,
	INTERNET_CONNECTION_PROXY = 0x04,
	INTERNET_CONNECTION_RAS_INSTALLED = 0x10,
	INTERNET_CONNECTION_OFFLINE = 0x20,
	INTERNET_CONNECTION_CONFIGURED = 0x40
}

[Flags]
public enum CONSOLE_SELECTION_ENUM : uint
{
	CONSOLE_NO_SELECTION = 0x0000, //No selection
	CONSOLE_SELECTION_IN_PROGRESS = 0x0001, //Selection has begun
	CONSOLE_SELECTION_NOT_EMPTY = 0x0002, //Selection rectangle is not empty
	CONSOLE_MOUSE_SELECTION = 0x0004, //Selecting with the mouse
	CONSOLE_MOUSE_DOWN = 0x0008 // Mouse is down
}

[Flags]
public enum FontMask
{
	RASTER_FONTTYPE = 0x0001,
	DEVICE_FONTTYPE = 0x0002,
	TRUETYPE_FONTTYPE = 0x0004
}

[Flags]
public enum FontWeight
{
	FW_DONTCARE = 0,
	FW_THIN = 100,
	FW_EXTRALIGHT = 200,
	FW_LIGHT = 300,
	FW_NORMAL = 400,
	FW_MEDIUM = 500,
	FW_SEMIBOLD = 600,
	FW_BOLD = 700,
	FW_EXTRABOLD = 800,
	FW_HEAVY = 900
}

public enum FontPitch
{
	DEFAULT_PITCH = 0,
	FIXED_PITCH = 1,
	VARIABLE_PITCH = 2,
	MONO_FONT = 8
}

public enum FontFamily
{
	FF_DONTCARE = 0,
	FF_ROMAN = 1 << 4,
	FF_SWISS = 2 << 4,
	FF_MODERN = 3 << 4,
	FF_SCRIPT = 4 << 4,
	FF_DECORATIVE = 5 << 4
}

[Flags]
public enum FileAccessEnum : uint
{
	// Standard Section
	AccessSystemSecurity = 0x1000000, // AccessSystemAcl access type
	MaximumAllowed = 0x2000000, // MaximumAllowed access type

	Delete = 0x10000,
	ReadControl = 0x20000,
	WriteDac = 0x40000,
	WriteOwner = 0x80000,
	Synchronize = 0x100000,

	StandardRightsRequired = 0xF0000,
	StandardRightsRead = ReadControl,
	StandardRightsWrite = ReadControl,
	StandardRightsExecute = ReadControl,
	StandardRightsAll = 0x1F0000,
	SpecificRightsAll = 0xFFFF,

	FILE_READ_DATA = 0x0001, // file & pipe
	FILE_LIST_DIRECTORY = 0x0001, // directory
	FILE_WRITE_DATA = 0x0002, // file & pipe
	FILE_ADD_FILE = 0x0002, // directory
	FILE_APPEND_DATA = 0x0004, // file
	FILE_ADD_SUBDIRECTORY = 0x0004, // directory
	FILE_CREATE_PIPE_INSTANCE = 0x0004, // named pipe
	FILE_READ_EA = 0x0008, // file & directory
	FILE_WRITE_EA = 0x0010, // file & directory
	FILE_EXECUTE = 0x0020, // file
	FILE_TRAVERSE = 0x0020, // directory
	FILE_DELETE_CHILD = 0x0040, // directory
	FILE_READ_ATTRIBUTES = 0x0080, // all
	FILE_WRITE_ATTRIBUTES = 0x0100, // all

	// Generic Section
	GenericRead = 0x80000000,
	GenericWrite = 0x40000000,
	GenericExecute = 0x20000000,
	GenericAll = 0x10000000,

	SPECIFIC_RIGHTS_ALL = 0x00FFFF,

	FILE_ALL_ACCESS =
		StandardRightsRequired |
		Synchronize |
		0x1FF,

	FILE_GENERIC_READ =
		StandardRightsRead |
		FILE_READ_DATA |
		FILE_READ_ATTRIBUTES |
		FILE_READ_EA |
		Synchronize,

	FILE_GENERIC_WRITE =
		StandardRightsWrite |
		FILE_WRITE_DATA |
		FILE_WRITE_ATTRIBUTES |
		FILE_WRITE_EA |
		FILE_APPEND_DATA |
		Synchronize,

	FILE_GENERIC_EXECUTE =
		StandardRightsExecute |
		FILE_READ_ATTRIBUTES |
		FILE_EXECUTE |
		Synchronize
}

[Flags]
public enum FileShareEnum : uint
{
	/// <summary>
	/// </summary>
	None = 0x00000000,

	/// <summary>
	///     Enables subsequent open operations on an object to request read access.
	///     Otherwise, other processes cannot open the object if they request read access.
	///     If this flag is not specified, but the object has been opened for read access, the function fails.
	/// </summary>
	Read = 0x00000001,

	/// <summary>
	///     Enables subsequent open operations on an object to request write access.
	///     Otherwise, other processes cannot open the object if they request write access.
	///     If this flag is not specified, but the object has been opened for write access, the function fails.
	/// </summary>
	Write = 0x00000002,

	/// <summary>
	///     Enables subsequent open operations on an object to request delete access.
	///     Otherwise, other processes cannot open the object if they request delete access.
	///     If this flag is not specified, but the object has been opened for delete access, the function fails.
	/// </summary>
	Delete = 0x00000004
}

public enum CreationDispositionEnum : uint
{
	/// <summary>
	///     Creates a new file. The function fails if a specified file exists.
	/// </summary>
	New = 1,

	/// <summary>
	///     Creates a new file, always.
	///     If a file exists, the function overwrites the file, clears the existing attributes, combines the specified file
	///     attributes,
	///     and flags with FILE_ATTRIBUTE_ARCHIVE, but does not set the security descriptor that the SECURITY_ATTRIBUTES
	///     structure specifies.
	/// </summary>
	CreateAlways = 2,

	/// <summary>
	///     Opens a file. The function fails if the file does not exist.
	/// </summary>
	OpenExisting = 3,

	/// <summary>
	///     Opens a file, always.
	///     If a file does not exist, the function creates a file as if dwCreationDisposition is CREATE_NEW.
	/// </summary>
	OpenAlways = 4,

	/// <summary>
	///     Opens a file and truncates it so that its size is 0 (zero) bytes. The function fails if the file does not exist.
	///     The calling process must open the file with the GENERIC_WRITE access right.
	/// </summary>
	TruncateExisting = 5
}

[Flags]
public enum FileAttributesEnum : uint
{
	Readonly = 0x00000001,
	Hidden = 0x00000002,
	System = 0x00000004,
	Directory = 0x00000010,
	Archive = 0x00000020,
	Device = 0x00000040,
	Normal = 0x00000080,
	Temporary = 0x00000100,
	SparseFile = 0x00000200,
	ReparsePoint = 0x00000400,
	Compressed = 0x00000800,
	Offline = 0x00001000,
	NotContentIndexed = 0x00002000,
	Encrypted = 0x00004000,
	WriteThrough = 0x80000000,
	Overlapped = 0x40000000,
	NoBuffering = 0x20000000,
	RandomAccess = 0x10000000,
	SequentialScan = 0x08000000,
	DeleteOnClose = 0x04000000,
	BackupSemantics = 0x02000000,
	PosixSemantics = 0x01000000,
	OpenReparsePoint = 0x00200000,
	OpenNoRecall = 0x00100000,
	FirstPipeInstance = 0x00080000
}

public enum FileType
{
	Unknown,
	Disk,
	Char,
	Pipe
}

[Flags]
public enum FormatMessageFlags : uint
{
	FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x00000100,
	FORMAT_MESSAGE_IGNORE_INSERTS = 0x00000200,
	FORMAT_MESSAGE_FROM_SYSTEM = 0x00001000,
	FORMAT_MESSAGE_ARGUMENT_ARRAY = 0x00002000,
	FORMAT_MESSAGE_FROM_HMODULE = 0x00000800,
	FORMAT_MESSAGE_FROM_STRING = 0x00000400
}

[Flags]
public enum ProcessAccessFlags : uint
{
	All = 0x001F0FFF,
	Terminate = 0x00000001,
	CreateThread = 0x00000002,
	VirtualMemoryOperation = 0x00000008,
	VirtualMemoryRead = 0x00000010,
	VirtualMemoryWrite = 0x00000020,
	DuplicateHandle = 0x00000040,
	CreateProcess = 0x000000080,
	SetQuota = 0x00000100,
	SetInformation = 0x00000200,
	QueryInformation = 0x00000400,
	QueryLimitedInformation = 0x00001000,
	Synchronize = 0x00100000
}

public enum CTRL_EVENT : uint
{
	/// <summary>
	///     Generates a CTRL+C signal. This signal cannot be generated for process groups. If dwProcessGroupId is nonzero, this
	///     function will succeed, but the CTRL+C signal will not be received by processes within the specified process group.
	/// </summary>
	CTRL_C_EVENT = 0,

	/// <summary>
	///     Generates a CTRL+BREAK signal.
	/// </summary>
	CTRL_BREAK_EVENT = 1
}

//Timer type definitions
[Flags]
public enum fuEvent : uint
{
	TIME_ONESHOT = 0, //Event occurs once, after uDelay milliseconds. 
	TIME_PERIODIC = 1,

	TIME_CALLBACK_FUNCTION = 0x0000 /* callback is function */
	//TIME_CALLBACK_EVENT_SET = 0x0010, /* callback is event - use SetEvent */
	//TIME_CALLBACK_EVENT_PULSE = 0x0020  /* callback is event - use PulseEvent */
}

public enum LogonType
{
	/// <summary>
	///     This logon type is intended for users who will be interactively using the computer, such as a user being logged
	///     on by a terminal server, remote shell, or similar process. This logon type has the additional expense of caching
	///     logon information for disconnected operations; therefore, it is inappropriate for some client/server applications,
	///     such as a mail server.
	/// </summary>
	Interactive = 2,

	/// <summary>
	///     This logon type is intended for high performance servers to authenticate plaintext passwords.
	///     The LogonUser function does not cache credentials for this logon type.
	/// </summary>
	Network = 3,

	/// <summary>
	///     This logon type is intended for batch servers, where processes may be executing on behalf of a user
	///     without their direct intervention. This type is also for higher performance servers that process many
	///     plaintext authentication attempts at a time, such as mail or web servers.
	/// </summary>
	Batch = 4,

	/// <summary>
	///     Indicates a service-type logon. The account provided must have the service privilege enabled.
	/// </summary>
	Service = 5,

	/// <summary>
	///     GINAs are no longer supported.
	///     Windows Server 2003 and Windows XP:  This logon type is for GINA DLLs that log on users who will be
	///     interactively using the computer. This logon type can generate a unique audit record that shows when
	///     the workstation was unlocked.
	/// </summary>
	Unlock = 7,

	/// <summary>
	///     This logon type preserves the name and password in the authentication package, which allows the server
	///     to make connections to other network servers while impersonating the client. A server can accept plaintext
	///     credentials from a client, call LogonUser, verify that the user can access the system across the network,
	///     and still communicate with other servers.
	/// </summary>
	NetworkClearText = 8,

	/// <summary>
	///     This logon type allows the caller to clone its current token and specify new credentials for outbound connections.
	///     The new logon session has the same local identifier but uses different credentials for other network connections.
	///     This logon type is supported only by the LOGON32_PROVIDER_WINNT50 logon provider.
	/// </summary>
	NewCredentials = 9
}

public enum SecurityImpersonationLevel
{
	SecurityAnonymous = 0,
	SecurityIdentification = 1,
	SecurityImpersonation = 2,
	SecurityDelegation = 3
}

public enum TokenInformationClass
{
	TokenUser = 1,
	TokenGroups,
	TokenPrivileges,
	TokenOwner,
	TokenPrimaryGroup,
	TokenDefaultDacl,
	TokenSource,
	TokenType,
	TokenImpersonationLevel,
	TokenStatistics,
	TokenRestrictedSids,
	TokenSessionId,
	TokenGroupsAndPrivileges,
	TokenSessionReference,
	TokenSandBoxInert,
	TokenAuditPolicy,
	TokenOrigin,
	TokenElevationType,
	TokenLinkedToken,
	TokenElevation,
	TokenHasRestrictions,
	TokenAccessInformation,
	TokenVirtualizationAllowed,
	TokenVirtualizationEnabled,
	TokenIntegrityLevel,
	TokenUiAccess,
	TokenMandatoryPolicy,
	TokenLogonSid,
	MaxTokenInfoClass
}

public enum TokenElevationType
{
	/// <summary>
	///     User is not using a split token, so they cannot elevate.
	/// </summary>
	TokenElevationTypeDefault = 1,

	/// <summary>
	///     User has a split token, and the process is running elevated. Assuming they're an administrator.
	/// </summary>
	TokenElevationTypeFull,

	/// <summary>
	///     User has a split token, but the process is not running elevated. Assuming they're an administrator.
	/// </summary>
	TokenElevationTypeLimited
}

public enum MULTIPLE_TRUSTEE_OPERATION
{
	NO_MULTIPLE_TRUSTEE,
	TRUSTEE_IS_IMPERSONATE
}

public enum TRUSTEE_FORM
{
	TRUSTEE_IS_SID,
	TRUSTEE_IS_NAME,
	TRUSTEE_BAD_FORM,
	TRUSTEE_IS_OBJECTS_AND_SID,
	TRUSTEE_IS_OBJECTS_AND_NAME
}

public enum TRUSTEE_TYPE
{
	TRUSTEE_IS_UNKNOWN,
	TRUSTEE_IS_USER,
	TRUSTEE_IS_GROUP,
	TRUSTEE_IS_DOMAIN,
	TRUSTEE_IS_ALIAS,
	TRUSTEE_IS_WELL_KNOWN_GROUP,
	TRUSTEE_IS_DELETED,
	TRUSTEE_IS_INVALID,
	TRUSTEE_IS_COMPUTER
}

[Flags]
public enum ServiceAccessRights : uint
{
	QueryConfig = 1,
	ChangeConfig = 1 << 1,
	QueryStatus = 1 << 2,
	EnumerateDependents = 1 << 3,
	Start = 1 << 4,
	Stop = 1 << 5,
	PauseContinue = 1 << 6,
	Interrogate = 1 << 7,
	UserDefinedControl = 1 << 8,
	Delete = 1 << 9,
	ReadControl = 1 << 10,
	WriteDac = 1 << 11,
	WriteOwner = 1 << 12,
	Synchronize = 1 << 13,
	AccessSystemSecurity = 1 << 14,
	GenericAll = 1 << 15,
	GenericExecute = 1 << 16,
	GenericWrite = 1 << 17,
	GenericRead = 1 << 18
}

public enum DBT_DeviceType
{
	OEM = 0x00000000,
	Volume = 0x00000002,
	Port = 0x00000003,
	DeviceInterface = 0x00000005,
	Handle = 0x00000006
}

public enum DBT
{
	DBT_DEVNODES_CHANGED = 0x0007,
	DBT_QUERYCHANGECONFIG = 0x0017,
	DBT_CONFIGCHANGED = 0x0018,
	DBT_CONFIGCHANGECANCELED = 0x0019,
	DBT_DEVICEARRIVAL = 0x8000,
	DBT_DEVICEQUERYREMOVE = 0x8001,
	DBT_DEVICEQUERYREMOVEFAILED = 0x8002,
	DBT_DEVICEREMOVEPENDING = 0x8003,
	DBT_DEVICEREMOVECOMPLETE = 0x8004,
	DBT_DEVICETYPESPECIFIC = 0x8005,
	DBT_CUSTOMEVENT = 0x8006,
	DBT_USERDEFINED = 0xFFFF
}

public enum SHSTOCKICONID : uint
{
	/// <summary>Document of a type with no associated application.</summary>
	SIID_DOCNOASSOC = 0,
	/// <summary>Document of a type with an associated application.</summary>
	SIID_DOCASSOC = 1,
	/// <summary>Generic application with no custom icon.</summary>
	SIID_APPLICATION = 2,
	/// <summary>Folder (generic, unspecified state).</summary>
	SIID_FOLDER = 3,
	/// <summary>Folder (open).</summary>
	SIID_FOLDEROPEN = 4,
	/// <summary>5.25-inch disk drive.</summary>
	SIID_DRIVE525 = 5,
	/// <summary>3.5-inch disk drive.</summary>
	SIID_DRIVE35 = 6,
	/// <summary>Removable drive.</summary>
	SIID_DRIVEREMOVE = 7,
	/// <summary>Fixed drive (hard disk).</summary>
	SIID_DRIVEFIXED = 8,
	/// <summary>Network drive (connected).</summary>
	SIID_DRIVENET = 9,
	/// <summary>Network drive (disconnected).</summary>
	SIID_DRIVENETDISABLED = 10,
	/// <summary>CD drive.</summary>
	SIID_DRIVECD = 11,
	/// <summary>RAM disk drive.</summary>
	SIID_DRIVERAM = 12,
	/// <summary>The entire network.</summary>
	SIID_WORLD = 13,
	/// <summary>A computer on the network.</summary>
	SIID_SERVER = 15,
	/// <summary>A local printer or print destination.</summary>
	SIID_PRINTER = 16,
	/// <summary>The Network virtual folder (FOLDERID_NetworkFolder/CSIDL_NETWORK).</summary>
	SIID_MYNETWORK = 17,
	/// <summary>The Search feature.</summary>
	SIID_FIND = 22,
	/// <summary>The Help and Support feature.</summary>
	SIID_HELP = 23,
	/// <summary>Overlay for a shared item.</summary>
	SIID_SHARE = 28,
	/// <summary>Overlay for a shortcut.</summary>
	SIID_LINK = 29,
	/// <summary>Overlay for items that are expected to be slow to access.</summary>
	SIID_SLOWFILE = 30,
	/// <summary>The Recycle Bin (empty).</summary>
	SIID_RECYCLER = 31,
	/// <summary>The Recycle Bin (not empty).</summary>
	SIID_RECYCLERFULL = 32,
	/// <summary>Audio CD media.</summary>
	SIID_MEDIACDAUDIO = 40,
	/// <summary>Security lock.</summary>
	SIID_LOCK = 47,
	/// <summary>A virtual folder that contains the results of a search.</summary>
	SIID_AUTOLIST = 49,
	/// <summary>A network printer.</summary>
	SIID_PRINTERNET = 50,
	/// <summary>A server shared on a network.</summary>
	SIID_SERVERSHARE = 51,
	/// <summary>A local fax printer.</summary>
	SIID_PRINTERFAX = 52,
	/// <summary>A network fax printer.</summary>
	SIID_PRINTERFAXNET = 53,
	/// <summary>A file that receives the output of a Print to file operation.</summary>
	SIID_PRINTERFILE = 54,
	/// <summary>A category that results from a Stack by command to organize the contents of a folder.</summary>
	SIID_STACK = 55,
	/// <summary>Super Video CD (SVCD) media.</summary>
	SIID_MEDIASVCD = 56,
	/// <summary>A folder that contains only subfolders as child items.</summary>
	SIID_STUFFEDFOLDER = 57,
	/// <summary>Unknown drive type.</summary>
	SIID_DRIVEUNKNOWN = 58,
	/// <summary>DVD drive.</summary>
	SIID_DRIVEDVD = 59,
	/// <summary>DVD media.</summary>
	SIID_MEDIADVD = 60,
	/// <summary>DVD-RAM media.</summary>
	SIID_MEDIADVDRAM = 61,
	/// <summary>DVD-RW media.</summary>
	SIID_MEDIADVDRW = 62,
	/// <summary>DVD-R media.</summary>
	SIID_MEDIADVDR = 63,
	/// <summary>DVD-ROM media.</summary>
	SIID_MEDIADVDROM = 64,
	/// <summary>CD+ (enhanced audio CD) media.</summary>
	SIID_MEDIACDAUDIOPLUS = 65,
	/// <summary>CD-RW media.</summary>
	SIID_MEDIACDRW = 66,
	/// <summary>CD-R media.</summary>
	SIID_MEDIACDR = 67,
	/// <summary>A writable CD in the process of being burned.</summary>
	SIID_MEDIACDBURN = 68,
	/// <summary>Blank writable CD media.</summary>
	SIID_MEDIABLANKCD = 69,
	/// <summary>CD-ROM media.</summary>
	SIID_MEDIACDROM = 70,
	/// <summary>An audio file.</summary>
	SIID_AUDIOFILES = 71,
	/// <summary>An image file.</summary>
	SIID_IMAGEFILES = 72,
	/// <summary>A video file.</summary>
	SIID_VIDEOFILES = 73,
	/// <summary>A mixed file.</summary>
	SIID_MIXEDFILES = 74,
	/// <summary>Folder back.</summary>
	SIID_FOLDERBACK = 75,
	/// <summary>Folder front.</summary>
	SIID_FOLDERFRONT = 76,
	/// <summary>Security shield. Use for UAC prompts only.</summary>
	SIID_SHIELD = 77,
	/// <summary>Warning.</summary>
	SIID_WARNING = 78,
	/// <summary>Informational.</summary>
	SIID_INFO = 79,
	/// <summary>Error.</summary>
	SIID_ERROR = 80,
	/// <summary>Key.</summary>
	SIID_KEY = 81,
	/// <summary>Software.</summary>
	SIID_SOFTWARE = 82,
	/// <summary>A UI item, such as a button, that issues a rename command.</summary>
	SIID_RENAME = 83,
	/// <summary>A UI item, such as a button, that issues a delete command.</summary>
	SIID_DELETE = 84,
	/// <summary>Audio DVD media.</summary>
	SIID_MEDIAAUDIODVD = 85,
	/// <summary>Movie DVD media.</summary>
	SIID_MEDIAMOVIEDVD = 86,
	/// <summary>Enhanced CD media.</summary>
	SIID_MEDIAENHANCEDCD = 87,
	/// <summary>Enhanced DVD media.</summary>
	SIID_MEDIAENHANCEDDVD = 88,
	/// <summary>High definition DVD media in the HD DVD format.</summary>
	SIID_MEDIAHDDVD = 89,
	/// <summary>High definition DVD media in the Blu-ray Disc™ format.</summary>
	SIID_MEDIABLURAY = 90,
	/// <summary>Video CD (VCD) media.</summary>
	SIID_MEDIAVCD = 91,
	/// <summary>DVD+R media.</summary>
	SIID_MEDIADVDPLUSR = 92,
	/// <summary>DVD+RW media.</summary>
	SIID_MEDIADVDPLUSRW = 93,
	/// <summary>A desktop computer.</summary>
	SIID_DESKTOPPC = 94,
	/// <summary>A mobile computer (laptop).</summary>
	SIID_MOBILEPC = 95,
	/// <summary>The User Accounts Control Panel item.</summary>
	SIID_USERS = 96,
	/// <summary>Smart media.</summary>
	SIID_MEDIASMARTMEDIA = 97,
	/// <summary>CompactFlash media.</summary>
	SIID_MEDIACOMPACTFLASH = 98,
	/// <summary>A cell phone.</summary>
	SIID_DEVICECELLPHONE = 99,
	/// <summary>A digital camera.</summary>
	SIID_DEVICECAMERA = 100,
	/// <summary>A digital video camera.</summary>
	SIID_DEVICEVIDEOCAMERA = 101,
	/// <summary>An audio player.</summary>
	SIID_DEVICEAUDIOPLAYER = 102,
	/// <summary>Connect to network.</summary>
	SIID_NETWORKCONNECT = 103,
	/// <summary>The Network and Internet Control Panel item.</summary>
	SIID_INTERNET = 104,
	/// <summary>A compressed file with a .zip file name extension.</summary>
	SIID_ZIPFILE = 105,
	/// <summary>The Additional Options Control Panel item.</summary>
	SIID_SETTINGS = 106,
	/// <summary>High definition DVD drive (any type - HD DVD-ROM, HD DVD-R, HD-DVD-RAM) that uses the HD DVD format.</summary>
	/// <remarks>Windows Vista with SP1 and later.</remarks>
	SIID_DRIVEHDDVD = 132,
	/// <summary>High definition DVD drive (any type - BD-ROM, BD-R, BD-RE) that uses the Blu-ray Disc format.</summary>
	/// <remarks>Windows Vista with SP1 and later.</remarks>
	SIID_DRIVEBD = 133,
	/// <summary>High definition DVD-ROM media in the HD DVD-ROM format.</summary>
	/// <remarks>Windows Vista with SP1 and later.</remarks>
	SIID_MEDIAHDDVDROM = 134,
	/// <summary>High definition DVD-R media in the HD DVD-R format.</summary>
	/// <remarks>Windows Vista with SP1 and later.</remarks>
	SIID_MEDIAHDDVDR = 135,
	/// <summary>High definition DVD-RAM media in the HD DVD-RAM format.</summary>
	/// <remarks>Windows Vista with SP1 and later.</remarks>
	SIID_MEDIAHDDVDRAM = 136,
	/// <summary>High definition DVD-ROM media in the Blu-ray Disc BD-ROM format.</summary>
	/// <remarks>Windows Vista with SP1 and later.</remarks>
	SIID_MEDIABDROM = 137,
	/// <summary>High definition write-once media in the Blu-ray Disc BD-R format.</summary>
	/// <remarks>Windows Vista with SP1 and later.</remarks>
	SIID_MEDIABDR = 138,
	/// <summary>High definition read/write media in the Blu-ray Disc BD-RE format.</summary>
	/// <remarks>Windows Vista with SP1 and later.</remarks>
	SIID_MEDIABDRE = 139,
	/// <summary>A cluster disk array.</summary>
	/// <remarks>Windows Vista with SP1 and later.</remarks>
	SIID_CLUSTEREDDRIVE = 140,
	/// <summary>The highest valid value in the enumeration.</summary>
	/// <remarks>Values over 160 are Windows 7-only icons.</remarks>
	SIID_MAX_ICONS = 175
}

[Flags]
public enum SHGSI : uint
{
	/// <summary>The szPath and iIcon members of the SHSTOCKICONINFO structure receive the path and icon index of the requested icon, in a format suitable for passing to the ExtractIcon function. The numerical value of this flag is zero, so you always get the icon location regardless of other flags.</summary>
	SHGSI_ICONLOCATION = 0,
	/// <summary>The hIcon member of the SHSTOCKICONINFO structure receives a handle to the specified icon.</summary>
	SHGSI_ICON = 0x000000100,
	/// <summary>The iSysImageImage member of the SHSTOCKICONINFO structure receives the index of the specified icon in the system imagelist.</summary>
	SHGSI_SYSICONINDEX = 0x000004000,
	/// <summary>Modifies the SHGSI_ICON value by causing the function to add the link overlay to the file's icon.</summary>
	SHGSI_LINKOVERLAY = 0x000008000,
	/// <summary>Modifies the SHGSI_ICON value by causing the function to blend the icon with the system highlight color.</summary>
	SHGSI_SELECTED = 0x000010000,
	/// <summary>Modifies the SHGSI_ICON value by causing the function to retrieve the large version of the icon, as specified by the SM_CXICON and SM_CYICON system metrics.</summary>
	SHGSI_LARGEICON = 0x000000000,
	/// <summary>Modifies the SHGSI_ICON value by causing the function to retrieve the small version of the icon, as specified by the SM_CXSMICON and SM_CYSMICON system metrics.</summary>
	SHGSI_SMALLICON = 0x000000001,
	/// <summary>Modifies the SHGSI_LARGEICON or SHGSI_SMALLICON values by causing the function to retrieve the Shell-sized icons rather than the sizes specified by the system metrics.</summary>
	SHGSI_SHELLICONSIZE = 0x000000004
}


public enum MessageBoxButtons : long
{
	OK = 0x00000000L,
	OKCancel = 0x00000001L,
	AbortRetryIgnore = 0x00000002L,
	YesNoCancel = 0x00000003L,
	YesNo = 0x00000004L,
	RetryCancel = 0x00000005L,
	CancelTryContinue = 0x00000006L
}

public enum MessageBoxIcons : long
{
	Error = 0x00000010L,
	Question = 0x00000020L,
	Warning = 0x00000030L,
	Information = 0x00000040L
}

public enum MessageBoxDefaultButton : long
{
	Button1 = 0x00000000L,
	Button2 = 0x00000100L,
	Button3 = 0x00000200L,
	Button4 = 0x00000300L
}

public enum MessageBoxModalType : long
{
	Application = 0x00000000L,
	System = 0x00001000L,
	Task = 0x00002000L
}

[Flags]
public enum MessageBoxFlags : long
{
	Help = 0x00004000L,
	DefaultDesktopOnly = 0x00020000L,
	Right = 0x00080000L,
	RTLReading = 0x00100000L,
	SetForeground = 0x00010000L,
	TopMost = 0x00040000L,
	ServiceNotification = 0x00200000L
}

public enum MessageReturnValue
{
	OK = 1,
	Cancel = 2,
	Abort = 3,
	Retry = 4,
	Ignore = 5,
	Yes = 6,
	No = 7,
	TryAgain = 10,
	Continue = 11
}
#endregion

#region struct
[StructLayout(LayoutKind.Explicit)]
public struct CHAR_INFO
{
	[FieldOffset(0)]
	public char UnicodeChar;

	[FieldOffset(0)]
	public char AsciiChar;

	[FieldOffset(2)] //2 bytes seems to work properly
	public ushort Attributes;
}

[StructLayout(LayoutKind.Sequential)]
public struct COLORREF
{
	public uint Value;

	public COLORREF(uint color) { Value = color; }
}

[StructLayout(LayoutKind.Sequential)]
public struct CONSOLE_CURSOR_INFO
{
	public uint Size;
	public bool Visible;
}

[StructLayout(LayoutKind.Sequential)]
public struct CONSOLE_FONT_INFO
{
	public int nFont;
	public COORD dwFontSize;

	public static explicit operator ConsoleFont(CONSOLE_FONT_INFO f)
	{
		return new ConsoleFont
		{
			Index = (uint)f.nFont,
			SizeX = f.dwFontSize.X,
			SizeY = f.dwFontSize.Y
		};
	}

	public static explicit operator CONSOLE_FONT_INFO(ConsoleFont f)
	{
		return new CONSOLE_FONT_INFO
		{
			nFont = (int)f.Index,
			dwFontSize = new COORD(f.SizeX, f.SizeY)
		};
	}
}

public struct CONSOLE_SCREEN_BUFFER_INFO
{
	public COORD dwSize;
	public COORD dwCursorPosition;
	public short wAttributes;
	public SMALL_RECT srWindow;
	public COORD dwMaximumWindowSize;
}

[StructLayout(LayoutKind.Sequential)]
public struct CONSOLE_SELECTION_INFO
{
	public CONSOLE_SELECTION_ENUM Flags;
	public COORD SelectionAnchor;
	public SMALL_RECT Selection;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ConsoleFont
{
	public uint Index;
	public short SizeX, SizeY;

	public static explicit operator ConsoleFont(CONSOLE_FONT_INFO f)
	{
		return new ConsoleFont
		{
			Index = (uint)f.nFont,
			SizeX = f.dwFontSize.X,
			SizeY = f.dwFontSize.Y
		};
	}

	public static explicit operator CONSOLE_FONT_INFO(ConsoleFont f)
	{
		return new CONSOLE_FONT_INFO
		{
			nFont = (int)f.Index,
			dwFontSize = new COORD(f.SizeX, f.SizeY)
		};
	}
}

[StructLayout(LayoutKind.Sequential)]
public struct COORD
{
	public short X;
	public short Y;

	public COORD(short x, short y)
	{
		X = x;
		Y = y;
	}

	public COORD(POINT pt)
		: this((short)pt.X, (short)pt.Y)
	{
	}

	public static explicit operator POINT(COORD c) { return new POINT(c.X, c.Y); }

	public static explicit operator COORD(POINT p) { return new COORD((short)p.X, (short)p.Y); }
}

[StructLayout(LayoutKind.Sequential)]
public struct FOCUS_EVENT_RECORD
{
	public uint bSetFocus;
}

[StructLayout(LayoutKind.Explicit)]
public struct INPUT_RECORD
{
	[FieldOffset(0)]
	public InputRecordEventTypeEnum EventType;

	[FieldOffset(4)]
	public KEY_EVENT_RECORD KeyEvent;

	[FieldOffset(4)]
	public MOUSE_EVENT_RECORD MouseEvent;

	[FieldOffset(4)]
	public WINDOW_BUFFER_SIZE_RECORD WindowBufferSizeEvent;

	[FieldOffset(4)]
	public MENU_EVENT_RECORD MenuEvent;

	[FieldOffset(4)]
	public FOCUS_EVENT_RECORD FocusEvent;
}

[StructLayout(LayoutKind.Sequential)]
public struct IO_COUNTERS
{
	public ulong ReadOperationCount;
	public ulong WriteOperationCount;
	public ulong OtherOperationCount;
	public ulong ReadTransferCount;
	public ulong WriteTransferCount;
	public ulong OtherTransferCount;
}

[StructLayout(LayoutKind.Sequential)]
public struct JOBOBJECT_BASIC_LIMIT_INFORMATION
{
	public long PerProcessUserTimeLimit;
	public long PerJobUserTimeLimit;
	public JobObjectLimitEnum LimitFlags;
	public UIntPtr MinimumWorkingSetSize;
	public UIntPtr MaximumWorkingSetSize;
	public uint ActiveProcessLimit;
	public long Affinity;
	public uint PriorityClass;
	public uint SchedulingClass;
}

[StructLayout(LayoutKind.Sequential)]
public struct JOBOBJECT_EXTENDED_LIMIT_INFORMATION
{
	public JOBOBJECT_BASIC_LIMIT_INFORMATION BasicLimitInformation;
	public IO_COUNTERS IoInfo;
	public UIntPtr ProcessMemoryLimit;
	public UIntPtr JobMemoryLimit;
	public UIntPtr PeakProcessMemoryUsed;
	public UIntPtr PeakJobMemoryUsed;
}

[StructLayout(LayoutKind.Explicit)]
public struct KEY_EVENT_RECORD
{
	[FieldOffset(0)]
	[MarshalAs(UnmanagedType.Bool)]
	public bool bKeyDown;

	[FieldOffset(4)]
	[MarshalAs(UnmanagedType.U2)]
	public ushort wRepeatCount;

	[FieldOffset(6)]
	[MarshalAs(UnmanagedType.U2)]
	public VirtualKeysEnum wVirtualKeyCode;

	[FieldOffset(8)]
	[MarshalAs(UnmanagedType.U2)]
	public ushort wVirtualScanCode;

	[FieldOffset(10)]
	public char UnicodeChar;

	[FieldOffset(12)]
	[MarshalAs(UnmanagedType.U4)]
	public ControlKeyStatesEnum dwControlKeyState;
}

[StructLayout(LayoutKind.Sequential)]
public struct MENU_EVENT_RECORD
{
	public uint dwCommandId;
}

[StructLayout(LayoutKind.Explicit)]
public struct MOUSE_EVENT_RECORD
{
	[FieldOffset(0)]
	public COORD dwMousePosition;

	[FieldOffset(4)]
	public uint dwButtonState;

	[FieldOffset(8)]
	public uint dwControlKeyState;

	[FieldOffset(12)]
	public uint dwEventFlags;
}

[StructLayout(LayoutKind.Sequential)]
public struct POINT
{
	public int X;
	public int Y;

	public POINT(int x, int y)
	{
		X = x;
		Y = y;
	}
}

[StructLayout(LayoutKind.Sequential)]
public struct SIZE
{
	public int CX;
	public int CY;

	public SIZE(int cx, int cy)
	{
		CX = cx;
		CY = cy;
	}
}

[StructLayout(LayoutKind.Sequential)]
public struct PROCESS_INFORMATION
{
	public IntPtr hProcess;
	public IntPtr hThread;
	public int dwProcessId;
	public int dwThreadId;
}

[StructLayout(LayoutKind.Sequential)]
public struct RECT
{
	public int Left, Top, Right, Bottom;

	public RECT(int left, int top, int right, int bottom)
	{
		Left = left;
		Top = top;
		Right = right;
		Bottom = bottom;
	}

	public int X
	{
		get => Left;
		set
		{
			Right -= Left - value;
			Left = value;
		}
	}

	public int Y
	{
		get => Top;
		set
		{
			Bottom -= Top - value;
			Top = value;
		}
	}

	public int Height { get => Bottom - Top; set => Bottom = value + Top; }

	public int Width { get => Right - Left; set => Right = value + Left; }

	public POINT Location
	{
		get => new POINT(Left, Top);
		set
		{
			X = value.X;
			Y = value.Y;
		}
	}

	public SIZE Size
	{
		get => new SIZE(Width, Height);
		set
		{
			Width = value.CX;
			Height = value.CY;
		}
	}
}

public struct SMALL_RECT
{
	public short Left, Top, Right, Bottom;

	public SMALL_RECT(short left, short top, short right, short bottom)
	{
		Left = left;
		Top = top;
		Right = right;
		Bottom = bottom;
	}

	public SMALL_RECT(SMALL_RECT r)
		: this(r.Left, r.Top, r.Right, r.Bottom)
	{
	}

	public short X
	{
		get => Left;
		set
		{
			Right -= (short)(Left - value);
			Left = value;
		}
	}

	public short Y
	{
		get => Top;
		set
		{
			Bottom -= (short)(Top - value);
			Top = value;
		}
	}

	public short Height { get => (short)(Bottom - Top); set => Bottom = (short)(value + Top); }

	public short Width { get => (short)(Right - Left); set => Right = (short)(value + Left); }

	public POINT Location
	{
		get => new POINT(Left, Top);
		set
		{
			X = (short)value.X;
			Y = (short)value.Y;
		}
	}

	public SIZE Size
	{
		get => new SIZE(Width, Height);
		set
		{
			Width = (short)value.CX;
			Height = (short)value.CY;
		}
	}

	[NotNull]
	public override string ToString() { return string.Format(CultureInfo.CurrentCulture, "{{Left={0},Top={1},Right={2},Bottom={3}}}", Left, Top, Right, Bottom); }

	public bool Equals(SMALL_RECT r) { return r.Left == Left && r.Top == Top && r.Right == Right && r.Bottom == Bottom; }
}

[StructLayout(LayoutKind.Sequential)]
public struct SystemTime
{
	public ushort Year;
	public ushort Month;
	public ushort DayOfWeek;
	public ushort Day;
	public ushort Hour;
	public ushort Minute;
	public ushort Second;
	public ushort Millisecond;
}

public struct WINDOW_BUFFER_SIZE_RECORD
{
	public COORD dwSize;

	public WINDOW_BUFFER_SIZE_RECORD(short x, short y) { dwSize = new COORD(x, y); }
}

public struct TRUSTEE
{
	public IntPtr pMultipleTrustee;
	public MULTIPLE_TRUSTEE_OPERATION MultipleTrusteeOperation;
	public TRUSTEE_FORM TrusteeForm;
	public TRUSTEE_TYPE TrusteeType;
	public IntPtr ptstrName;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
public struct DEV_BROADCAST_HDR
{
	public int Size;
	public DBT_DeviceType DeviceType;
	public int Reserved;
}

[StructLayout(LayoutKind.Sequential)]
public struct DEV_BROADCAST_OEM
{
	public int Size;
	public DBT_DeviceType DeviceType;
	public int Reserved;
	public int Identifier;
	public int SuppFunc;
}

[StructLayout(LayoutKind.Sequential)]
public struct DEV_BROADCAST_VOLUME
{
	public int Size;
	public DBT_DeviceType DeviceType;
	public int Reserved;
	public int UnitMask;
	public short Flags;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
public struct DEV_BROADCAST_PORT
{
	public int Size;
	public DBT_DeviceType DeviceType;
	public int Reserved;
	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
	public string Name;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
public struct DEV_BROADCAST_DEVICEINTERFACE
{
	public int Size;
	public DBT_DeviceType DeviceType;
	public int Reserved;
	public Guid ClassGuid;
	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
	public string Name;
}

[StructLayout(LayoutKind.Sequential)]
public struct DEV_BROADCAST_HANDLE
{
	public int Size;
	public DBT_DeviceType DeviceType;
	public int Reserved;
	public IntPtr Handle;
	public IntPtr HDevNotify;
	public Guid EventGuid;
	public long NameOffset;
	public byte Data;
	public byte Data1;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
public struct DEV_BROADCAST_USERDEFINED
{
	public DEV_BROADCAST_HDR dbh;
	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
	public string szName;
}

[StructLayout(LayoutKind.Sequential)]
public struct MONITORINFO
{
	public int cbSize;
	public RECT rcMonitor;
	public RECT rcWork;
	public uint dwFlags;
}

[StructLayout(LayoutKind.Sequential)]
public struct MINMAXINFO
{
	public POINT ptReserved;
	public POINT ptMaxSize;
	public POINT ptMaxPosition;
	public POINT ptMinTrackSize;
	public POINT ptMaxTrackSize;
}

[StructLayout(LayoutKind.Sequential)]
public struct DWMCOLORIZATIONPARAMS
{
	public uint colorizationColor;
	public uint colorizationAfterglow;
	public uint colorizationColorBalance; // Ranging from 0 to 100
	public uint colorizationAfterglowBalance;
	public uint colorizationBlurBalance;
	public uint colorizationGlassReflectionIntensity;
	public uint colorizationOpaqueBlend;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct SHSTOCKICONINFO
{
	public uint cbSize;
	public IntPtr hIcon;
	public int iSysIconIndex;
	public int iIcon;
	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = Win32.MAX_PATH)]
	public string szPath;
}
#endregion

#region classes
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public class CONSOLE_FONT_INFO_EX
{
	public CONSOLE_FONT_INFO_EX() { cbSize = (uint)Marshal.SizeOf(typeof(CONSOLE_FONT_INFO_EX)); }

	public uint cbSize;
	public int nIndex;
	public short nWidth;
	public short nHeight;
	public FontMask nFamily;
	public FontWeight nWeight;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
	public string lpszFaceName;
}

[StructLayout(LayoutKind.Sequential)]
public class CONSOLE_HISTORY_INFO
{
	public CONSOLE_HISTORY_INFO() { cbSize = (ushort)Marshal.SizeOf(typeof(CONSOLE_HISTORY_INFO)); }

	public ushort cbSize;
	public ushort HistoryBufferSize;
	public ushort NumberOfHistoryBuffers;
	public uint dwFlags;
}

[StructLayout(LayoutKind.Sequential)]
public class CONSOLE_SCREEN_BUFFER_INFO_EX
{
	public CONSOLE_SCREEN_BUFFER_INFO_EX() { cbSize = (uint)Marshal.SizeOf(typeof(CONSOLE_SCREEN_BUFFER_INFO_EX)); }

	public uint cbSize;
	public COORD dwSize;
	public COORD dwCursorPosition;
	public short wAttributes;
	public SMALL_RECT srWindow;
	public COORD dwMaximumWindowSize;

	public ushort wPopupAttributes;
	public bool bFullscreenSupported;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
	public COLORREF[] ColorTable;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
public class LOGFONT
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

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
	public string lfFaceName;
}

[StructLayout(LayoutKind.Sequential)]
public class SECURITY_ATTRIBUTES
{
	public SECURITY_ATTRIBUTES() { nLength = Marshal.SizeOf((object)this); }

	public int nLength;
	public IntPtr lpSecurityDescriptor;
	public bool bInheritHandle;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
public class SHELLEXECUTEINFO
{
	public SHELLEXECUTEINFO() { cbSize = Marshal.SizeOf((object)this); }

	public int cbSize;
	public int fMask;
	public IntPtr hWnd = IntPtr.Zero;

	[MarshalAs(UnmanagedType.LPTStr)]
	public string lpVerb = null;

	[MarshalAs(UnmanagedType.LPTStr)]
	public string lpFile = null;

	[MarshalAs(UnmanagedType.LPTStr)]
	public string lpParameters = null;

	[MarshalAs(UnmanagedType.LPTStr)]
	public string lpDirectory = null;

	public int nShow = (int)ShowWindowEnum.SW_SHOW;
	public IntPtr hInstApp = IntPtr.Zero;
	public IntPtr lpIDList = IntPtr.Zero;

	[MarshalAs(UnmanagedType.LPTStr)]
	public string lpClass = null;

	public IntPtr hKeyClass = IntPtr.Zero;
	public uint dwHotKey = 0;
	public IntPtr hIcon = IntPtr.Zero;
	public IntPtr hProcess = IntPtr.Zero;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
public class STARTUPINFO
{
	public STARTUPINFO() { cb = Marshal.SizeOf((object)this); }

	public int cb;
	public string lpReserved = null;
	public string lpDesktop = null;
	public string lpTitle = null;
	public int dwX;
	public int dwY;
	public int dwXSize;
	public int dwYSize;
	public int dwXCountChars;
	public int dwYCountChars;
	public int dwFillAttribute;
	public uint dwFlags;
	public short wShowWindow;
	public short cbReserved2;
	public IntPtr lpReserved2 = IntPtr.Zero;
	public IntPtr hStdInput = IntPtr.Zero;
	public IntPtr hStdOutput = IntPtr.Zero;
	public IntPtr hStdError = IntPtr.Zero;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
public class STARTUPINFOEX
{
	public STARTUPINFO StartupInfo = new STARTUPINFO();
	public IntPtr lpAttributeList = IntPtr.Zero;
}
#endregion

#region static classes
/// <summary>
/// See <see href="https://github.com/MicrosoftDocs/win32/blob/docs/desktop-src/com/com-error-codes.md">COM Error Codes</see>
/// </summary>
public static class ResultCom
{
	#region Generic
	/// <summary>
	/// Operation successful
	/// </summary>
	public const int S_OK = 0x00000000;
	/// <summary>
	/// Operation successful
	/// </summary>
	public const int S_FALSE = 0x00000001;
	/// <summary>
	/// Catastrophic failure
	/// </summary>
	public const int E_UNEXPECTED = unchecked((int)0x8000FFFF);
	/// <summary>
	/// Not implemented
	/// </summary>
	public const int E_NOTIMPL = unchecked((int)0x80004001);
	/// <summary>
	/// Ran out of memory
	/// </summary>
	public const int E_OUTOFMEMORY = unchecked((int)0x8007000E);
	/// <summary>
	/// One or more arguments are invalid
	/// </summary>
	public const int E_INVALIDARG = unchecked((int)0x80070057);
	/// <summary>
	/// No such interface supported
	/// </summary>
	public const int E_NOINTERFACE = unchecked((int)0x80004002);
	/// <summary>
	/// Invalid pointer
	/// </summary>
	public const int E_POINTER = unchecked((int)0x80004003);
	/// <summary>
	/// Invalid handle
	/// </summary>
	public const int E_HANDLE = unchecked((int)0x80070006);
	/// <summary>
	/// Operation aborted
	/// </summary>
	public const int E_ABORT = unchecked((int)0x80004004);
	/// <summary>
	/// Unspecified error
	/// </summary>
	public const int E_FAIL = unchecked((int)0x80004005);
	/// <summary>
	/// General access denied error
	/// </summary>
	public const int E_ACCESSDENIED = unchecked((int)0x80070005);
	/// <summary>
	/// The data necessary to complete this operation is not yet available.
	/// </summary>
	public const int E_PENDING = unchecked((int)0x8000000A);
	/// <summary>
	/// The operation attempted to access data outside the valid range
	/// </summary>
	public const int E_BOUNDS = unchecked((int)0x8000000B);
	/// <summary>
	/// A concurrent or interleaved operation changed the state of the object, invalidating this operation.
	/// </summary>
	public const int E_CHANGED_STATE = unchecked((int)0x8000000C);
	/// <summary>
	/// An illegal state change was requested.
	/// </summary>
	public const int E_ILLEGAL_STATE_CHANGE = unchecked((int)0x8000000D);
	/// <summary>
	/// A method was called at an unexpected time.
	/// </summary>
	public const int E_ILLEGAL_METHOD_CALL = unchecked((int)0x8000000E);
	/// <summary>
	/// Typename or Namespace was not found in metadata file.
	/// </summary>
	public const int RO_E_METADATA_NAME_NOT_FOUND = unchecked((int)0x8000000F);
	/// <summary>
	/// Name is an existing namespace rather than a typename.
	/// </summary>
	public const int RO_E_METADATA_NAME_IS_NAMESPACE = unchecked((int)0x80000010);
	/// <summary>
	/// Typename has an invalid format.
	/// </summary>
	public const int RO_E_METADATA_INVALID_TYPE_FORMAT = unchecked((int)0x80000011);
	/// <summary>
	/// Metadata file is invalid or corrupted.
	/// </summary>
	public const int RO_E_INVALID_METADATA_FILE = unchecked((int)0x80000012);
	/// <summary>
	/// The object has been closed.
	/// </summary>
	public const int RO_E_CLOSED = unchecked((int)0x80000013);
	/// <summary>
	/// Only one thread may access the object during a write operation.
	/// </summary>
	public const int RO_E_EXCLUSIVE_WRITE = unchecked((int)0x80000014);
	/// <summary>
	/// Operation is prohibited during change notification.
	/// </summary>
	public const int RO_E_CHANGE_NOTIFICATION_IN_PROGRESS = unchecked((int)0x80000015);
	/// <summary>
	/// The text associated with this error code could not be found.
	/// </summary>
	public const int RO_E_ERROR_STRING_NOT_FOUND = unchecked((int)0x80000016);
	/// <summary>
	/// String not null terminated.
	/// </summary>
	public const int E_STRING_NOT_NULL_TERMINATED = unchecked((int)0x80000017);
	/// <summary>
	/// A delegate was assigned when not allowed.
	/// </summary>
	public const int E_ILLEGAL_DELEGATE_ASSIGNMENT = unchecked((int)0x80000018);
	/// <summary>
	/// An async operation was not properly started.
	/// </summary>
	public const int E_ASYNC_OPERATION_NOT_STARTED = unchecked((int)0x80000019);
	/// <summary>
	/// The application is exiting and cannot service this request.
	/// </summary>
	public const int E_APPLICATION_EXITING = unchecked((int)0x8000001A);
	/// <summary>
	/// The application view is exiting and cannot service this request.
	/// </summary>
	public const int E_APPLICATION_VIEW_EXITING = unchecked((int)0x8000001B);
	/// <summary>
	/// The object must support the IAgileObject interface.
	/// </summary>
	public const int RO_E_MUST_BE_AGILE = unchecked((int)0x8000001C);
	/// <summary>
	/// Activating a single-threaded class from MTA is not supported.
	/// </summary>
	public const int RO_E_UNSUPPORTED_FROM_MTA = unchecked((int)0x8000001D);
	/// <summary>
	/// The object has been committed.
	/// </summary>
	public const int RO_E_COMMITTED = unchecked((int)0x8000001E);
	/// <summary>
	/// Thread local storage failure
	/// </summary>
	public const int CO_E_INIT_TLS = unchecked((int)0x80004006);
	/// <summary>
	/// Get shared memory allocator failure
	/// </summary>
	public const int CO_E_INIT_SHARED_ALLOCATOR = unchecked((int)0x80004007);
	/// <summary>
	/// Get memory allocator failure
	/// </summary>
	public const int CO_E_INIT_MEMORY_ALLOCATOR = unchecked((int)0x80004008);
	/// <summary>
	/// Unable to initialize class cache
	/// </summary>
	public const int CO_E_INIT_CLASS_CACHE = unchecked((int)0x80004009);
	/// <summary>
	/// Unable to initialize RPC services
	/// </summary>
	public const int CO_E_INIT_RPC_CHANNEL = unchecked((int)0x8000400A);
	/// <summary>
	/// Cannot set thread local storage channel control
	/// </summary>
	public const int CO_E_INIT_TLS_SET_CHANNEL_CONTROL = unchecked((int)0x8000400B);
	/// <summary>
	/// Could not allocate thread local storage channel control
	/// </summary>
	public const int CO_E_INIT_TLS_CHANNEL_CONTROL = unchecked((int)0x8000400C);
	/// <summary>
	/// The user supplied memory allocator is unacceptable
	/// </summary>
	public const int CO_E_INIT_UNACCEPTED_USER_ALLOCATOR = unchecked((int)0x8000400D);
	/// <summary>
	/// The OLE service mutex already exists
	/// </summary>
	public const int CO_E_INIT_SCM_MUTEX_EXISTS = unchecked((int)0x8000400E);
	/// <summary>
	/// The OLE service file mapping already exists
	/// </summary>
	public const int CO_E_INIT_SCM_FILE_MAPPING_EXISTS = unchecked((int)0x8000400F);
	/// <summary>
	/// Unable to map view of file for OLE service
	/// </summary>
	public const int CO_E_INIT_SCM_MAP_VIEW_OF_FILE = unchecked((int)0x80004010);
	/// <summary>
	/// Failure attempting to launch OLE service
	/// </summary>
	public const int CO_E_INIT_SCM_EXEC_FAILURE = unchecked((int)0x80004011);
	/// <summary>
	/// There was an attempt to call CoInitialize a second time while single threaded
	/// </summary>
	public const int CO_E_INIT_ONLY_SINGLE_THREADED = unchecked((int)0x80004012);
	/// <summary>
	/// A Remote activation was necessary but was not allowed
	/// </summary>
	public const int CO_E_CANT_REMOTE = unchecked((int)0x80004013);
	/// <summary>
	/// A Remote activation was necessary but the server name provided was invalid
	/// </summary>
	public const int CO_E_BAD_SERVER_NAME = unchecked((int)0x80004014);
	/// <summary>
	/// The class is configured to run as a security id different from the caller
	/// </summary>
	public const int CO_E_WRONG_SERVER_IDENTITY = unchecked((int)0x80004015);
	/// <summary>
	/// Use of Ole1 services requiring DDE windows is disabled
	/// </summary>
	public const int CO_E_OLE1DDE_DISABLED = unchecked((int)0x80004016);
	/// <summary>
	/// A RunAs specification must be \ or simply . 
	/// </summary>
	public const int CO_E_RUNAS_SYNTAX = unchecked((int)0x80004017);
	/// <summary>
	/// The server process could not be started. The pathname may be incorrect.
	/// </summary>
	public const int CO_E_CREATEPROCESS_FAILURE = unchecked((int)0x80004018);
	/// <summary>
	/// The server process could not be started as the configured identity. The pathname may be incorrect or unavailable.
	/// </summary>
	public const int CO_E_RUNAS_CREATEPROCESS_FAILURE = unchecked((int)0x80004019);
	/// <summary>
	/// The server process could not be started because the configured identity is incorrect. Check the user name and password.
	/// </summary>
	public const int CO_E_RUNAS_LOGON_FAILURE = unchecked((int)0x8000401A);
	/// <summary>
	/// The client is not allowed to launch this server.
	/// </summary>
	public const int CO_E_LAUNCH_PERMSSION_DENIED = unchecked((int)0x8000401B);
	/// <summary>
	/// The service providing this server could not be started.
	/// </summary>
	public const int CO_E_START_SERVICE_FAILURE = unchecked((int)0x8000401C);
	/// <summary>
	/// This computer was unable to communicate with the computer providing the server.
	/// </summary>
	public const int CO_E_REMOTE_COMMUNICATION_FAILURE = unchecked((int)0x8000401D);
	/// <summary>
	/// The server did not respond after being launched.
	/// </summary>
	public const int CO_E_SERVER_START_TIMEOUT = unchecked((int)0x8000401E);
	/// <summary>
	/// The registration information for this server is inconsistent or incomplete.
	/// </summary>
	public const int CO_E_CLSREG_INCONSISTENT = unchecked((int)0x8000401F);
	/// <summary>
	/// The registration information for this interface is inconsistent or incomplete.
	/// </summary>
	public const int CO_E_IIDREG_INCONSISTENT = unchecked((int)0x80004020);
	/// <summary>
	/// The operation attempted is not supported.
	/// </summary>
	public const int CO_E_NOT_SUPPORTED = unchecked((int)0x80004021);
	/// <summary>
	/// A dll must be loaded.
	/// </summary>
	public const int CO_E_RELOAD_DLL = unchecked((int)0x80004022);
	/// <summary>
	/// A Microsoft Software Installer error was encountered.
	/// </summary>
	public const int CO_E_MSI_ERROR = unchecked((int)0x80004023);
	/// <summary>
	/// The specified activation could not occur in the client context as specified.
	/// </summary>
	public const int CO_E_ATTEMPT_TO_CREATE_OUTSIDE_CLIENT_CONTEXT = unchecked((int)0x80004024);
	/// <summary>
	/// Activations on the server are paused.
	/// </summary>
	public const int CO_E_SERVER_PAUSED = unchecked((int)0x80004025);
	/// <summary>
	/// Activations on the server are not paused.
	/// </summary>
	public const int CO_E_SERVER_NOT_PAUSED = unchecked((int)0x80004026);
	/// <summary>
	/// The component or application containing the component has been disabled.
	/// </summary>
	public const int CO_E_CLASS_DISABLED = unchecked((int)0x80004027);
	/// <summary>
	/// The common language runtime is not available
	/// </summary>
	public const int CO_E_CLRNOTAVAILABLE = unchecked((int)0x80004028);
	/// <summary>
	/// The thread-pool rejected the submitted asynchronous work.
	/// </summary>
	public const int CO_E_ASYNC_WORK_REJECTED = unchecked((int)0x80004029);
	/// <summary>
	/// The server started, but did not finish initializing in a timely fashion.
	/// </summary>
	public const int CO_E_SERVER_INIT_TIMEOUT = unchecked((int)0x8000402A);
	/// <summary>
	/// Unable to complete the call since there is no COM+ security context inside IObjectControl.Activate.
	/// </summary>
	public const int CO_E_NO_SECCTX_IN_ACTIVATE = unchecked((int)0x8000402B);
	/// <summary>
	/// The provided tracker configuration is invalid
	/// </summary>
	public const int CO_E_TRACKER_CONFIG = unchecked((int)0x80004030);
	/// <summary>
	/// The provided thread pool configuration is invalid
	/// </summary>
	public const int CO_E_THREADPOOL_CONFIG = unchecked((int)0x80004031);
	/// <summary>
	/// The provided side-by-side configuration is invalid
	/// </summary>
	public const int CO_E_SXS_CONFIG = unchecked((int)0x80004032);
	/// <summary>
	/// The server principal name (SPN) obtained during security negotiation is malformed.
	/// </summary>
	public const int CO_E_MALFORMED_SPN = unchecked((int)0x80004033);
	/// <summary>
	/// Invalid OLEVERB structure
	/// </summary>
	public const int OLE_E_OLEVERB = unchecked((int)0x80040000);
	/// <summary>
	/// Invalid advise flags
	/// </summary>
	public const int OLE_E_ADVF = unchecked((int)0x80040001);
	/// <summary>
	/// Can't enumerate any more, because the associated data is missing
	/// </summary>
	public const int OLE_E_ENUM_NOMORE = unchecked((int)0x80040002);
	/// <summary>
	/// This implementation doesn't take advises
	/// </summary>
	public const int OLE_E_ADVISENOTSUPPORTED = unchecked((int)0x80040003);
	/// <summary>
	/// There is no connection for this connection ID
	/// </summary>
	public const int OLE_E_NOCONNECTION = unchecked((int)0x80040004);
	/// <summary>
	/// Need to run the object to perform this operation
	/// </summary>
	public const int OLE_E_NOTRUNNING = unchecked((int)0x80040005);
	/// <summary>
	/// There is no cache to operate on
	/// </summary>
	public const int OLE_E_NOCACHE = unchecked((int)0x80040006);
	/// <summary>
	/// Uninitialized object
	/// </summary>
	public const int OLE_E_BLANK = unchecked((int)0x80040007);
	/// <summary>
	/// Linked object's source class has changed
	/// </summary>
	public const int OLE_E_CLASSDIFF = unchecked((int)0x80040008);
	/// <summary>
	/// Not able to get the moniker of the object
	/// </summary>
	public const int OLE_E_CANT_GETMONIKER = unchecked((int)0x80040009);
	/// <summary>
	/// Not able to bind to the source
	/// </summary>
	public const int OLE_E_CANT_BINDTOSOURCE = unchecked((int)0x8004000A);
	/// <summary>
	/// Object is static; operation not allowed
	/// </summary>
	public const int OLE_E_STATIC = unchecked((int)0x8004000B);
	/// <summary>
	/// User canceled out of save dialog
	/// </summary>
	public const int OLE_E_PROMPTSAVECANCELLED = unchecked((int)0x8004000C);
	/// <summary>
	/// Invalid rectangle
	/// </summary>
	public const int OLE_E_INVALIDRECT = unchecked((int)0x8004000D);
	/// <summary>
	/// compobj.dll is too old for the ole2.dll initialized
	/// </summary>
	public const int OLE_E_WRONGCOMPOBJ = unchecked((int)0x8004000E);
	/// <summary>
	/// Invalid window handle
	/// </summary>
	public const int OLE_E_INVALIDHWND = unchecked((int)0x8004000F);
	/// <summary>
	/// Object is not in any of the inplace active states
	/// </summary>
	public const int OLE_E_NOT_INPLACEACTIVE = unchecked((int)0x80040010);
	/// <summary>
	/// Not able to convert object
	/// </summary>
	public const int OLE_E_CANTCONVERT = unchecked((int)0x80040011);
	/// <summary>
	/// Not able to perform the operation because object is not given storage yet
	/// </summary>
	public const int OLE_E_NOSTORAGE = unchecked((int)0x80040012);
	/// <summary>
	/// Invalid FORMATETC structure
	/// </summary>
	public const int DV_E_FORMATETC = unchecked((int)0x80040064);
	/// <summary>
	/// Invalid DVTARGETDEVICE structure
	/// </summary>
	public const int DV_E_DVTARGETDEVICE = unchecked((int)0x80040065);
	/// <summary>
	/// Invalid STDGMEDIUM structure
	/// </summary>
	public const int DV_E_STGMEDIUM = unchecked((int)0x80040066);
	/// <summary>
	/// Invalid STATDATA structure
	/// </summary>
	public const int DV_E_STATDATA = unchecked((int)0x80040067);
	/// <summary>
	/// Invalid lindex
	/// </summary>
	public const int DV_E_LINDEX = unchecked((int)0x80040068);
	/// <summary>
	/// Invalid tymed
	/// </summary>
	public const int DV_E_TYMED = unchecked((int)0x80040069);
	/// <summary>
	/// Invalid clipboard format
	/// </summary>
	public const int DV_E_CLIPFORMAT = unchecked((int)0x8004006A);
	/// <summary>
	/// Invalid aspect(s)
	/// </summary>
	public const int DV_E_DVASPECT = unchecked((int)0x8004006B);
	/// <summary>
	/// tdSize parameter of the DVTARGETDEVICE structure is invalid
	/// </summary>
	public const int DV_E_DVTARGETDEVICE_SIZE = unchecked((int)0x8004006C);
	/// <summary>
	/// Object doesn't support IViewObject interface
	/// </summary>
	public const int DV_E_NOIVIEWOBJECT = unchecked((int)0x8004006D);
	/// <summary>
	/// Trying to revoke a drop target that has not been registered
	/// </summary>
	public const int DRAGDROP_E_NOTREGISTERED = unchecked((int)0x80040100);
	/// <summary>
	/// This window has already been registered as a drop target
	/// </summary>
	public const int DRAGDROP_E_ALREADYREGISTERED = unchecked((int)0x80040101);
	/// <summary>
	/// Invalid window handle
	/// </summary>
	public const int DRAGDROP_E_INVALIDHWND = unchecked((int)0x80040102);
	/// <summary>
	/// Class does not support aggregation (or class object is remote)
	/// </summary>
	public const int CLASS_E_NOAGGREGATION = unchecked((int)0x80040110);
	/// <summary>
	/// ClassFactory cannot supply requested class
	/// </summary>
	public const int CLASS_E_CLASSNOTAVAILABLE = unchecked((int)0x80040111);
	/// <summary>
	/// Class is not licensed for use
	/// </summary>
	public const int CLASS_E_NOTLICENSED = unchecked((int)0x80040112);
	/// <summary>
	/// Error drawing view
	/// </summary>
	public const int VIEW_E_DRAW = unchecked((int)0x80040140);
	/// <summary>
	/// Could not read key from registry
	/// </summary>
	public const int REGDB_E_READREGDB = unchecked((int)0x80040150);
	/// <summary>
	/// Could not write key to registry
	/// </summary>
	public const int REGDB_E_WRITEREGDB = unchecked((int)0x80040151);
	/// <summary>
	/// Could not find the key in the registry
	/// </summary>
	public const int REGDB_E_KEYMISSING = unchecked((int)0x80040152);
	/// <summary>
	/// Invalid value for registry
	/// </summary>
	public const int REGDB_E_INVALIDVALUE = unchecked((int)0x80040153);
	/// <summary>
	/// Class not registered
	/// </summary>
	public const int REGDB_E_CLASSNOTREG = unchecked((int)0x80040154);
	/// <summary>
	/// Interface not registered
	/// </summary>
	public const int REGDB_E_IIDNOTREG = unchecked((int)0x80040155);
	/// <summary>
	/// Threading model entry is not valid
	/// </summary>
	public const int REGDB_E_BADTHREADINGMODEL = unchecked((int)0x80040156);
	/// <summary>
	/// CATID does not exist
	/// </summary>
	public const int CAT_E_CATIDNOEXIST = unchecked((int)0x80040160);
	/// <summary>
	/// Description not found
	/// </summary>
	public const int CAT_E_NODESCRIPTION = unchecked((int)0x80040161);
	/// <summary>
	/// No package in the software installation data in the Active Directory meets this criteria.
	/// </summary>
	public const int CS_E_PACKAGE_NOTFOUND = unchecked((int)0x80040164);
	/// <summary>
	/// Deleting this will break the referential integrity of the software installation data in the Active Directory.
	/// </summary>
	public const int CS_E_NOT_DELETABLE = unchecked((int)0x80040165);
	/// <summary>
	/// The CLSID was not found in the software installation data in the Active Directory.
	/// </summary>
	public const int CS_E_CLASS_NOTFOUND = unchecked((int)0x80040166);
	/// <summary>
	/// The software installation data in the Active Directory is corrupt.
	/// </summary>
	public const int CS_E_INVALID_VERSION = unchecked((int)0x80040167);
	/// <summary>
	/// There is no software installation data in the Active Directory.
	/// </summary>
	public const int CS_E_NO_CLASSSTORE = unchecked((int)0x80040168);
	/// <summary>
	/// There is no software installation data object in the Active Directory.
	/// </summary>
	public const int CS_E_OBJECT_NOTFOUND = unchecked((int)0x80040169);
	/// <summary>
	/// The software installation data object in the Active Directory already exists.
	/// </summary>
	public const int CS_E_OBJECT_ALREADY_EXISTS = unchecked((int)0x8004016A);
	/// <summary>
	/// The path to the software installation data in the Active Directory is not correct.
	/// </summary>
	public const int CS_E_INVALID_PATH = unchecked((int)0x8004016B);
	/// <summary>
	/// A network error interrupted the operation.
	/// </summary>
	public const int CS_E_NETWORK_ERROR = unchecked((int)0x8004016C);
	/// <summary>
	/// The size of this object exceeds the maximum size set by the Administrator.
	/// </summary>
	public const int CS_E_ADMIN_LIMIT_EXCEEDED = unchecked((int)0x8004016D);
	/// <summary>
	/// The schema for the software installation data in the Active Directory does not match the required schema.
	/// </summary>
	public const int CS_E_SCHEMA_MISMATCH = unchecked((int)0x8004016E);
	/// <summary>
	/// An error occurred in the software installation data in the Active Directory.
	/// </summary>
	public const int CS_E_INTERNAL_ERROR = unchecked((int)0x8004016F);
	/// <summary>
	/// Cache not updated
	/// </summary>
	public const int CACHE_E_NOCACHE_UPDATED = unchecked((int)0x80040170);
	/// <summary>
	/// No verbs for OLE object
	/// </summary>
	public const int OLEOBJ_E_NOVERBS = unchecked((int)0x80040180);
	/// <summary>
	/// Invalid verb for OLE object
	/// </summary>
	public const int OLEOBJ_E_INVALIDVERB = unchecked((int)0x80040181);
	/// <summary>
	/// Undo is not available
	/// </summary>
	public const int INPLACE_E_NOTUNDOABLE = unchecked((int)0x800401A0);
	/// <summary>
	/// Space for tools is not available
	/// </summary>
	public const int INPLACE_E_NOTOOLSPACE = unchecked((int)0x800401A1);
	/// <summary>
	/// OLESTREAM Get method failed
	/// </summary>
	public const int CONVERT10_E_OLESTREAM_GET = unchecked((int)0x800401C0);
	/// <summary>
	/// OLESTREAM Put method failed
	/// </summary>
	public const int CONVERT10_E_OLESTREAM_PUT = unchecked((int)0x800401C1);
	/// <summary>
	/// Contents of the OLESTREAM not in correct format
	/// </summary>
	public const int CONVERT10_E_OLESTREAM_FMT = unchecked((int)0x800401C2);
	/// <summary>
	/// There was an error in a Windows GDI call while converting the bitmap to a DIB
	/// </summary>
	public const int CONVERT10_E_OLESTREAM_BITMAP_TO_DIB = unchecked((int)0x800401C3);
	/// <summary>
	/// Contents of the IStorage not in correct format
	/// </summary>
	public const int CONVERT10_E_STG_FMT = unchecked((int)0x800401C4);
	/// <summary>
	/// Contents of IStorage is missing one of the standard streams
	/// </summary>
	public const int CONVERT10_E_STG_NO_STD_STREAM = unchecked((int)0x800401C5);
	/// <summary>
	/// There was an error in a Windows GDI call while converting the DIB to a bitmap.
	/// </summary>
	public const int CONVERT10_E_STG_DIB_TO_BITMAP = unchecked((int)0x800401C6);
	/// <summary>
	/// OpenClipboard Failed
	/// </summary>
	public const int CLIPBRD_E_CANT_OPEN = unchecked((int)0x800401D0);
	/// <summary>
	/// EmptyClipboard Failed
	/// </summary>
	public const int CLIPBRD_E_CANT_EMPTY = unchecked((int)0x800401D1);
	/// <summary>
	/// SetClipboard Failed
	/// </summary>
	public const int CLIPBRD_E_CANT_SET = unchecked((int)0x800401D2);
	/// <summary>
	/// Data on clipboard is invalid
	/// </summary>
	public const int CLIPBRD_E_BAD_DATA = unchecked((int)0x800401D3);
	/// <summary>
	/// CloseClipboard Failed
	/// </summary>
	public const int CLIPBRD_E_CANT_CLOSE = unchecked((int)0x800401D4);
	/// <summary>
	/// Moniker needs to be connected manually
	/// </summary>
	public const int MK_E_CONNECTMANUALLY = unchecked((int)0x800401E0);
	/// <summary>
	/// Operation exceeded deadline
	/// </summary>
	public const int MK_E_EXCEEDEDDEADLINE = unchecked((int)0x800401E1);
	/// <summary>
	/// Moniker needs to be generic
	/// </summary>
	public const int MK_E_NEEDGENERIC = unchecked((int)0x800401E2);
	/// <summary>
	/// Operation unavailable
	/// </summary>
	public const int MK_E_UNAVAILABLE = unchecked((int)0x800401E3);
	/// <summary>
	/// Invalid syntax
	/// </summary>
	public const int MK_E_SYNTAX = unchecked((int)0x800401E4);
	/// <summary>
	/// No object for moniker
	/// </summary>
	public const int MK_E_NOOBJECT = unchecked((int)0x800401E5);
	/// <summary>
	/// Bad extension for file
	/// </summary>
	public const int MK_E_INVALIDEXTENSION = unchecked((int)0x800401E6);
	/// <summary>
	/// Intermediate operation failed
	/// </summary>
	public const int MK_E_INTERMEDIATEINTERFACENOTSUPPORTED = unchecked((int)0x800401E7);
	/// <summary>
	/// Moniker is not bindable
	/// </summary>
	public const int MK_E_NOTBINDABLE = unchecked((int)0x800401E8);
	/// <summary>
	/// Moniker is not bound
	/// </summary>
	public const int MK_E_NOTBOUND = unchecked((int)0x800401E9);
	/// <summary>
	/// Moniker cannot open file
	/// </summary>
	public const int MK_E_CANTOPENFILE = unchecked((int)0x800401EA);
	/// <summary>
	/// User input required for operation to succeed
	/// </summary>
	public const int MK_E_MUSTBOTHERUSER = unchecked((int)0x800401EB);
	/// <summary>
	/// Moniker class has no inverse
	/// </summary>
	public const int MK_E_NOINVERSE = unchecked((int)0x800401EC);
	/// <summary>
	/// Moniker does not refer to storage
	/// </summary>
	public const int MK_E_NOSTORAGE = unchecked((int)0x800401ED);
	/// <summary>
	/// No common prefix
	/// </summary>
	public const int MK_E_NOPREFIX = unchecked((int)0x800401EE);
	/// <summary>
	/// Moniker could not be enumerated
	/// </summary>
	public const int MK_E_ENUMERATION_FAILED = unchecked((int)0x800401EF);
	/// <summary>
	/// CoInitialize has not been called.
	/// </summary>
	public const int CO_E_NOTINITIALIZED = unchecked((int)0x800401F0);
	/// <summary>
	/// CoInitialize has already been called.
	/// </summary>
	public const int CO_E_ALREADYINITIALIZED = unchecked((int)0x800401F1);
	/// <summary>
	/// Class of object cannot be determined
	/// </summary>
	public const int CO_E_CANTDETERMINECLASS = unchecked((int)0x800401F2);
	/// <summary>
	/// Invalid class string
	/// </summary>
	public const int CO_E_CLASSSTRING = unchecked((int)0x800401F3);
	/// <summary>
	/// Invalid interface string
	/// </summary>
	public const int CO_E_IIDSTRING = unchecked((int)0x800401F4);
	/// <summary>
	/// Application not found
	/// </summary>
	public const int CO_E_APPNOTFOUND = unchecked((int)0x800401F5);
	/// <summary>
	/// Application cannot be run more than once
	/// </summary>
	public const int CO_E_APPSINGLEUSE = unchecked((int)0x800401F6);
	/// <summary>
	/// Some error in application program
	/// </summary>
	public const int CO_E_ERRORINAPP = unchecked((int)0x800401F7);
	/// <summary>
	/// DLL for class not found
	/// </summary>
	public const int CO_E_DLLNOTFOUND = unchecked((int)0x800401F8);
	/// <summary>
	/// Error in the DLL
	/// </summary>
	public const int CO_E_ERRORINDLL = unchecked((int)0x800401F9);
	/// <summary>
	/// Wrong operating system or operating system version for the application
	/// </summary>
	public const int CO_E_WRONGOSFORAPP = unchecked((int)0x800401FA);
	/// <summary>
	/// Object is not registered
	/// </summary>
	public const int CO_E_OBJNOTREG = unchecked((int)0x800401FB);
	/// <summary>
	/// Object is already registered
	/// </summary>
	public const int CO_E_OBJISREG = unchecked((int)0x800401FC);
	/// <summary>
	/// Object is not connected to server
	/// </summary>
	public const int CO_E_OBJNOTCONNECTED = unchecked((int)0x800401FD);
	/// <summary>
	/// Application was launched but it didn't register a class factory
	/// </summary>
	public const int CO_E_APPDIDNTREG = unchecked((int)0x800401FE);
	/// <summary>
	/// Object has been released
	/// </summary>
	public const int CO_E_RELEASED = unchecked((int)0x800401FF);
	/// <summary>
	/// An event was able to invoke some but not all of the subscribers
	/// </summary>
	public const int EVENT_S_SOME_SUBSCRIBERS_FAILED = 0x00040200;
	/// <summary>
	/// An event was unable to invoke any of the subscribers
	/// </summary>
	public const int EVENT_E_ALL_SUBSCRIBERS_FAILED = unchecked((int)0x80040201);
	/// <summary>
	/// An event was delivered but there were no subscribers
	/// </summary>
	public const int EVENT_S_NOSUBSCRIBERS = 0x00040202;
	/// <summary>
	/// A syntax error occurred trying to evaluate a query string
	/// </summary>
	public const int EVENT_E_QUERYSYNTAX = unchecked((int)0x80040203);
	/// <summary>
	/// An invalid field name was used in a query string
	/// </summary>
	public const int EVENT_E_QUERYFIELD = unchecked((int)0x80040204);
	/// <summary>
	/// An unexpected exception was raised
	/// </summary>
	public const int EVENT_E_INTERNALEXCEPTION = unchecked((int)0x80040205);
	/// <summary>
	/// An unexpected internal error was detected
	/// </summary>
	public const int EVENT_E_INTERNALERROR = unchecked((int)0x80040206);
	/// <summary>
	/// The owner SID on a per-user subscription doesn't exist
	/// </summary>
	public const int EVENT_E_INVALID_PER_USER_SID = unchecked((int)0x80040207);
	/// <summary>
	/// A user-supplied component or subscriber raised an exception
	/// </summary>
	public const int EVENT_E_USER_EXCEPTION = unchecked((int)0x80040208);
	/// <summary>
	/// An interface has too many methods to fire events from
	/// </summary>
	public const int EVENT_E_TOO_MANY_METHODS = unchecked((int)0x80040209);
	/// <summary>
	/// A subscription cannot be stored unless its event class already exists
	/// </summary>
	public const int EVENT_E_MISSING_EVENTCLASS = unchecked((int)0x8004020A);
	/// <summary>
	/// Not all the objects requested could be removed
	/// </summary>
	public const int EVENT_E_NOT_ALL_REMOVED = unchecked((int)0x8004020B);
	/// <summary>
	/// COM+ is required for this operation, but is not installed
	/// </summary>
	public const int EVENT_E_COMPLUS_NOT_INSTALLED = unchecked((int)0x8004020C);
	/// <summary>
	/// Cannot modify or delete an object that was not added using the COM+ Admin SDK
	/// </summary>
	public const int EVENT_E_CANT_MODIFY_OR_DELETE_UNCONFIGURED_OBJECT = unchecked((int)0x8004020D);
	/// <summary>
	/// Cannot modify or delete an object that was added using the COM+ Admin SDK
	/// </summary>
	public const int EVENT_E_CANT_MODIFY_OR_DELETE_CONFIGURED_OBJECT = unchecked((int)0x8004020E);
	/// <summary>
	/// The event class for this subscription is in an invalid partition
	/// </summary>
	public const int EVENT_E_INVALID_EVENT_CLASS_PARTITION = unchecked((int)0x8004020F);
	/// <summary>
	/// The owner of the PerUser subscription is not logged on to the system specified
	/// </summary>
	public const int EVENT_E_PER_USER_SID_NOT_LOGGED_ON = unchecked((int)0x80040210);
	#endregion

	#region XACT, SCHED, OLE
	/// <summary>
	/// Another single phase resource manager has already been enlisted in this transaction.
	/// </summary>
	public const int XACT_E_ALREADYOTHERSINGLEPHASE = unchecked((int)0x8004D000);
	/// <summary>
	/// A retaining commit or abort is not supported
	/// </summary>
	public const int XACT_E_CANTRETAIN = unchecked((int)0x8004D001);
	/// <summary>
	/// The transaction failed to commit for an unknown reason. The transaction was aborted.
	/// </summary>
	public const int XACT_E_COMMITFAILED = unchecked((int)0x8004D002);
	/// <summary>
	/// Cannot call commit on this transaction object because the calling application did not initiate the transaction.
	/// </summary>
	public const int XACT_E_COMMITPREVENTED = unchecked((int)0x8004D003);
	/// <summary>
	/// Instead of committing, the resource heuristically aborted.
	/// </summary>
	public const int XACT_E_HEURISTICABORT = unchecked((int)0x8004D004);
	/// <summary>
	/// Instead of aborting, the resource heuristically committed.
	/// </summary>
	public const int XACT_E_HEURISTICCOMMIT = unchecked((int)0x8004D005);
	/// <summary>
	/// Some of the states of the resource were committed while others were aborted, likely because of heuristic decisions.
	/// </summary>
	public const int XACT_E_HEURISTICDAMAGE = unchecked((int)0x8004D006);
	/// <summary>
	/// Some of the states of the resource may have been committed while others may have been aborted, likely because of heuristic decisions.
	/// </summary>
	public const int XACT_E_HEURISTICDANGER = unchecked((int)0x8004D007);
	/// <summary>
	/// The requested isolation level is not valid or supported.
	/// </summary>
	public const int XACT_E_ISOLATIONLEVEL = unchecked((int)0x8004D008);
	/// <summary>
	/// The transaction manager doesn't support an asynchronous operation for this method.
	/// </summary>
	public const int XACT_E_NOASYNC = unchecked((int)0x8004D009);
	/// <summary>
	/// Unable to enlist in the transaction.
	/// </summary>
	public const int XACT_E_NOENLIST = unchecked((int)0x8004D00A);
	/// <summary>
	/// The requested semantics of retention of isolation across retaining commit and abort boundaries cannot be supported by this transaction implementation, or isoFlags was not equal to zero.
	/// </summary>
	public const int XACT_E_NOISORETAIN = unchecked((int)0x8004D00B);
	/// <summary>
	/// There is no resource presently associated with this enlistment
	/// </summary>
	public const int XACT_E_NORESOURCE = unchecked((int)0x8004D00C);
	/// <summary>
	/// The transaction failed to commit due to the failure of optimistic concurrency control in at least one of the resource managers.
	/// </summary>
	public const int XACT_E_NOTCURRENT = unchecked((int)0x8004D00D);
	/// <summary>
	/// The transaction has already been implicitly or explicitly committed or aborted
	/// </summary>
	public const int XACT_E_NOTRANSACTION = unchecked((int)0x8004D00E);
	/// <summary>
	/// An invalid combination of flags was specified
	/// </summary>
	public const int XACT_E_NOTSUPPORTED = unchecked((int)0x8004D00F);
	/// <summary>
	/// The resource manager id is not associated with this transaction or the transaction manager.
	/// </summary>
	public const int XACT_E_UNKNOWNRMGRID = unchecked((int)0x8004D010);
	/// <summary>
	/// This method was called in the wrong state
	/// </summary>
	public const int XACT_E_WRONGSTATE = unchecked((int)0x8004D011);
	/// <summary>
	/// The indicated unit of work does not match the unit of work expected by the resource manager.
	/// </summary>
	public const int XACT_E_WRONGUOW = unchecked((int)0x8004D012);
	/// <summary>
	/// An enlistment in a transaction already exists.
	/// </summary>
	public const int XACT_E_XTIONEXISTS = unchecked((int)0x8004D013);
	/// <summary>
	/// An import object for the transaction could not be found.
	/// </summary>
	public const int XACT_E_NOIMPORTOBJECT = unchecked((int)0x8004D014);
	/// <summary>
	/// The transaction cookie is invalid.
	/// </summary>
	public const int XACT_E_INVALIDCOOKIE = unchecked((int)0x8004D015);
	/// <summary>
	/// The transaction status is in doubt. A communication failure occurred, or a transaction manager or resource manager has failed
	/// </summary>
	public const int XACT_E_INDOUBT = unchecked((int)0x8004D016);
	/// <summary>
	/// A time-out was specified, but time-outs are not supported.
	/// </summary>
	public const int XACT_E_NOTIMEOUT = unchecked((int)0x8004D017);
	/// <summary>
	/// The requested operation is already in progress for the transaction.
	/// </summary>
	public const int XACT_E_ALREADYINPROGRESS = unchecked((int)0x8004D018);
	/// <summary>
	/// The transaction has already been aborted.
	/// </summary>
	public const int XACT_E_ABORTED = unchecked((int)0x8004D019);
	/// <summary>
	/// The Transaction Manager returned a log full error.
	/// </summary>
	public const int XACT_E_LOGFULL = unchecked((int)0x8004D01A);
	/// <summary>
	/// The Transaction Manager is not available.
	/// </summary>
	public const int XACT_E_TMNOTAVAILABLE = unchecked((int)0x8004D01B);
	/// <summary>
	/// A connection with the transaction manager was lost.
	/// </summary>
	public const int XACT_E_CONNECTION_DOWN = unchecked((int)0x8004D01C);
	/// <summary>
	/// A request to establish a connection with the transaction manager was denied.
	/// </summary>
	public const int XACT_E_CONNECTION_DENIED = unchecked((int)0x8004D01D);
	/// <summary>
	/// Resource manager reenlistment to determine transaction status timed out.
	/// </summary>
	public const int XACT_E_REENLISTTIMEOUT = unchecked((int)0x8004D01E);
	/// <summary>
	/// This transaction manager failed to establish a connection with another TIP transaction manager.
	/// </summary>
	public const int XACT_E_TIP_CONNECT_FAILED = unchecked((int)0x8004D01F);
	/// <summary>
	/// This transaction manager encountered a protocol error with another TIP transaction manager.
	/// </summary>
	public const int XACT_E_TIP_PROTOCOL_ERROR = unchecked((int)0x8004D020);
	/// <summary>
	/// This transaction manager could not propagate a transaction from another TIP transaction manager.
	/// </summary>
	public const int XACT_E_TIP_PULL_FAILED = unchecked((int)0x8004D021);
	/// <summary>
	/// The Transaction Manager on the destination machine is not available.
	/// </summary>
	public const int XACT_E_DEST_TMNOTAVAILABLE = unchecked((int)0x8004D022);
	/// <summary>
	/// The Transaction Manager has disabled its support for TIP.
	/// </summary>
	public const int XACT_E_TIP_DISABLED = unchecked((int)0x8004D023);
	/// <summary>
	/// The transaction manager has disabled its support for remote/network transactions.
	/// </summary>
	public const int XACT_E_NETWORK_TX_DISABLED = unchecked((int)0x8004D024);
	/// <summary>
	/// The partner transaction manager has disabled its support for remote/network transactions.
	/// </summary>
	public const int XACT_E_PARTNER_NETWORK_TX_DISABLED = unchecked((int)0x8004D025);
	/// <summary>
	/// The transaction manager has disabled its support for XA transactions.
	/// </summary>
	public const int XACT_E_XA_TX_DISABLED = unchecked((int)0x8004D026);
	/// <summary>
	/// MSDTC was unable to read its configuration information.
	/// </summary>
	public const int XACT_E_UNABLE_TO_READ_DTC_CONFIG = unchecked((int)0x8004D027);
	/// <summary>
	/// MSDTC was unable to load the dtc proxy dll.
	/// </summary>
	public const int XACT_E_UNABLE_TO_LOAD_DTC_PROXY = unchecked((int)0x8004D028);
	/// <summary>
	/// The local transaction has aborted.
	/// </summary>
	public const int XACT_E_ABORTING = unchecked((int)0x8004D029);
	/// <summary>
	/// The MSDTC transaction manager was unable to push the transaction to the destination transaction manager due to communication problems. Possible causes are: a firewall is present and it doesn't have an exception for the MSDTC process, the two machines cannot find each other by their NetBIOS names, or the support for network transactions is not enabled for one of the two transaction managers.
	/// </summary>
	public const int XACT_E_PUSH_COMM_FAILURE = unchecked((int)0x8004D02A);
	/// <summary>
	/// The MSDTC transaction manager was unable to pull the transaction from the source transaction manager due to communication problems. Possible causes are: a firewall is present and it doesn't have an exception for the MSDTC process, the two machines cannot find each other by their NetBIOS names, or the support for network transactions is not enabled for one of the two transaction managers.
	/// </summary>
	public const int XACT_E_PULL_COMM_FAILURE = unchecked((int)0x8004D02B);
	/// <summary>
	/// The MSDTC transaction manager has disabled its support for SNA LU 6.2 transactions.
	/// </summary>
	public const int XACT_E_LU_TX_DISABLED = unchecked((int)0x8004D02C);
	/// <summary>
	/// XACT_E_CLERKNOTFOUND
	/// </summary>
	public const int XACT_E_CLERKNOTFOUND = unchecked((int)0x8004D080);
	/// <summary>
	/// XACT_E_CLERKEXISTS
	/// </summary>
	public const int XACT_E_CLERKEXISTS = unchecked((int)0x8004D081);
	/// <summary>
	/// XACT_E_RECOVERYINPROGRESS
	/// </summary>
	public const int XACT_E_RECOVERYINPROGRESS = unchecked((int)0x8004D082);
	/// <summary>
	/// XACT_E_TRANSACTIONCLOSED
	/// </summary>
	public const int XACT_E_TRANSACTIONCLOSED = unchecked((int)0x8004D083);
	/// <summary>
	/// XACT_E_INVALIDLSN
	/// </summary>
	public const int XACT_E_INVALIDLSN = unchecked((int)0x8004D084);
	/// <summary>
	/// XACT_E_REPLAYREQUEST
	/// </summary>
	public const int XACT_E_REPLAYREQUEST = unchecked((int)0x8004D085);
	/// <summary>
	/// An asynchronous operation was specified. The operation has begun, but its outcome is not known yet.
	/// </summary>
	public const int XACT_S_ASYNC = 0x0004D000;
	/// <summary>
	/// XACT_S_DEFECT
	/// </summary>
	public const int XACT_S_DEFECT = 0x0004D001;
	/// <summary>
	/// The method call succeeded because the transaction was read-only.
	/// </summary>
	public const int XACT_S_READONLY = 0x0004D002;
	/// <summary>
	/// The transaction was successfully aborted. However, this is a coordinated transaction, and some number of enlisted resources were aborted outright because they could not support abort-retaining semantics
	/// </summary>
	public const int XACT_S_SOMENORETAIN = 0x0004D003;
	/// <summary>
	/// No changes were made during this call, but the sink wants another chance to look if any other sinks make further changes.
	/// </summary>
	public const int XACT_S_OKINFORM = 0x0004D004;
	/// <summary>
	/// The sink is content and wishes the transaction to proceed. Changes were made to one or more resources during this call.
	/// </summary>
	public const int XACT_S_MADECHANGESCONTENT = 0x0004D005;
	/// <summary>
	/// The sink is for the moment and wishes the transaction to proceed, but if other changes are made following this return by other event sinks then this sink wants another chance to look
	/// </summary>
	public const int XACT_S_MADECHANGESINFORM = 0x0004D006;
	/// <summary>
	/// The transaction was successfully aborted. However, the abort was non-retaining.
	/// </summary>
	public const int XACT_S_ALLNORETAIN = 0x0004D007;
	/// <summary>
	/// An abort operation was already in progress.
	/// </summary>
	public const int XACT_S_ABORTING = 0x0004D008;
	/// <summary>
	/// The resource manager has performed a single-phase commit of the transaction.
	/// </summary>
	public const int XACT_S_SINGLEPHASE = 0x0004D009;
	/// <summary>
	/// The local transaction has not aborted.
	/// </summary>
	public const int XACT_S_LOCALLY_OK = 0x0004D00A;
	/// <summary>
	/// The resource manager has requested to be the coordinator (last resource manager) for the transaction.
	/// </summary>
	public const int XACT_S_LASTRESOURCEMANAGER = 0x0004D010;
	/// <summary>
	/// The root transaction wanted to commit, but transaction aborted
	/// </summary>
	public const int CONTEXT_E_ABORTED = unchecked((int)0x8004E002);
	/// <summary>
	/// You made a method call on a COM+ component that has a transaction that has already aborted or in the process of aborting.
	/// </summary>
	public const int CONTEXT_E_ABORTING = unchecked((int)0x8004E003);
	/// <summary>
	/// There is no MTS object context
	/// </summary>
	public const int CONTEXT_E_NOCONTEXT = unchecked((int)0x8004E004);
	/// <summary>
	/// The component is configured to use synchronization and this method call would cause a deadlock to occur.
	/// </summary>
	public const int CONTEXT_E_WOULD_DEADLOCK = unchecked((int)0x8004E005);
	/// <summary>
	/// The component is configured to use synchronization and a thread has timed out waiting to enter the context.
	/// </summary>
	public const int CONTEXT_E_SYNCH_TIMEOUT = unchecked((int)0x8004E006);
	/// <summary>
	/// You made a method call on a COM+ component that has a transaction that has already committed or aborted.
	/// </summary>
	public const int CONTEXT_E_OLDREF = unchecked((int)0x8004E007);
	/// <summary>
	/// The specified role was not configured for the application
	/// </summary>
	public const int CONTEXT_E_ROLENOTFOUND = unchecked((int)0x8004E00C);
	/// <summary>
	/// COM+ was unable to talk to the Microsoft Distributed Transaction Coordinator
	/// </summary>
	public const int CONTEXT_E_TMNOTAVAILABLE = unchecked((int)0x8004E00F);
	/// <summary>
	/// An unexpected error occurred during COM+ Activation.
	/// </summary>
	public const int CO_E_ACTIVATIONFAILED = unchecked((int)0x8004E021);
	/// <summary>
	/// COM+ Activation failed. Check the event log for more information
	/// </summary>
	public const int CO_E_ACTIVATIONFAILED_EVENTLOGGED = unchecked((int)0x8004E022);
	/// <summary>
	/// COM+ Activation failed due to a catalog or configuration error.
	/// </summary>
	public const int CO_E_ACTIVATIONFAILED_CATALOGERROR = unchecked((int)0x8004E023);
	/// <summary>
	/// COM+ activation failed because the activation could not be completed in the specified amount of time.
	/// </summary>
	public const int CO_E_ACTIVATIONFAILED_TIMEOUT = unchecked((int)0x8004E024);
	/// <summary>
	/// COM+ Activation failed because an initialization function failed. Check the event log for more information.
	/// </summary>
	public const int CO_E_INITIALIZATIONFAILED = unchecked((int)0x8004E025);
	/// <summary>
	/// The requested operation requires that JIT be in the current context and it is not
	/// </summary>
	public const int CONTEXT_E_NOJIT = unchecked((int)0x8004E026);
	/// <summary>
	/// The requested operation requires that the current context have a Transaction, and it does not
	/// </summary>
	public const int CONTEXT_E_NOTRANSACTION = unchecked((int)0x8004E027);
	/// <summary>
	/// The components threading model has changed after install into a COM+ Application. Please re-install component.
	/// </summary>
	public const int CO_E_THREADINGMODEL_CHANGED = unchecked((int)0x8004E028);
	/// <summary>
	/// IIS intrinsics not available. Start your work with IIS.
	/// </summary>
	public const int CO_E_NOIISINTRINSICS = unchecked((int)0x8004E029);
	/// <summary>
	/// An attempt to write a cookie failed.
	/// </summary>
	public const int CO_E_NOCOOKIES = unchecked((int)0x8004E02A);
	/// <summary>
	/// An attempt to use a database generated a database specific error.
	/// </summary>
	public const int CO_E_DBERROR = unchecked((int)0x8004E02B);
	/// <summary>
	/// The COM+ component you created must use object pooling to work.
	/// </summary>
	public const int CO_E_NOTPOOLED = unchecked((int)0x8004E02C);
	/// <summary>
	/// The COM+ component you created must use object construction to work correctly.
	/// </summary>
	public const int CO_E_NOTCONSTRUCTED = unchecked((int)0x8004E02D);
	/// <summary>
	/// The COM+ component requires synchronization, and it is not configured for it.
	/// </summary>
	public const int CO_E_NOSYNCHRONIZATION = unchecked((int)0x8004E02E);
	/// <summary>
	/// The TxIsolation Level property for the COM+ component being created is stronger than the TxIsolationLevel for the "root" component for the transaction. The creation failed.
	/// </summary>
	public const int CO_E_ISOLEVELMISMATCH = unchecked((int)0x8004E02F);
	/// <summary>
	/// The component attempted to make a cross-context call between invocations of EnterTransactionScopeand ExitTransactionScope. This is not allowed. Cross-context calls cannot be made while inside of a transaction scope.
	/// </summary>
	public const int CO_E_CALL_OUT_OF_TX_SCOPE_NOT_ALLOWED = unchecked((int)0x8004E030);
	/// <summary>
	/// The component made a call to EnterTransactionScope, but did not make a corresponding call to ExitTransactionScope before returning.
	/// </summary>
	public const int CO_E_EXIT_TRANSACTION_SCOPE_NOT_CALLED = unchecked((int)0x8004E031);
	/// <summary>
	/// Use the registry database to provide the requested information
	/// </summary>
	public const int OLE_S_USEREG = 0x00040000;
	/// <summary>
	/// Success, but static
	/// </summary>
	public const int OLE_S_STATIC = 0x00040001;
	/// <summary>
	/// Macintosh clipboard format
	/// </summary>
	public const int OLE_S_MAC_CLIPFORMAT = 0x00040002;
	/// <summary>
	/// Successful drop took place
	/// </summary>
	public const int DRAGDROP_S_DROP = 0x00040100;
	/// <summary>
	/// Drag-drop operation canceled
	/// </summary>
	public const int DRAGDROP_S_CANCEL = 0x00040101;
	/// <summary>
	/// Use the default cursor
	/// </summary>
	public const int DRAGDROP_S_USEDEFAULTCURSORS = 0x00040102;
	/// <summary>
	/// Data has same FORMATETC
	/// </summary>
	public const int DATA_S_SAMEFORMATETC = 0x00040130;
	/// <summary>
	/// View is already frozen
	/// </summary>
	public const int VIEW_S_ALREADY_FROZEN = 0x00040140;
	/// <summary>
	/// FORMATETC not supported
	/// </summary>
	public const int CACHE_S_FORMATETC_NOTSUPPORTED = 0x00040170;
	/// <summary>
	/// Same cache
	/// </summary>
	public const int CACHE_S_SAMECACHE = 0x00040171;
	/// <summary>
	/// Some cache(s) not updated
	/// </summary>
	public const int CACHE_S_SOMECACHES_NOTUPDATED = 0x00040172;
	/// <summary>
	/// Invalid verb for OLE object
	/// </summary>
	public const int OLEOBJ_S_INVALIDVERB = 0x00040180;
	/// <summary>
	/// Verb number is valid but verb cannot be done now
	/// </summary>
	public const int OLEOBJ_S_CANNOT_DOVERB_NOW = 0x00040181;
	/// <summary>
	/// Invalid window handle passed
	/// </summary>
	public const int OLEOBJ_S_INVALIDHWND = 0x00040182;
	/// <summary>
	/// Message is too long; some of it had to be truncated before displaying
	/// </summary>
	public const int INPLACE_S_TRUNCATED = 0x000401A0;
	/// <summary>
	/// Unable to convert OLESTREAM to IStorage
	/// </summary>
	public const int CONVERT10_S_NO_PRESENTATION = 0x000401C0;
	/// <summary>
	/// Moniker reduced to itself
	/// </summary>
	public const int MK_S_REDUCED_TO_SELF = 0x000401E2;
	/// <summary>
	/// Common prefix is this moniker
	/// </summary>
	public const int MK_S_ME = 0x000401E4;
	/// <summary>
	/// Common prefix is input moniker
	/// </summary>
	public const int MK_S_HIM = 0x000401E5;
	/// <summary>
	/// Common prefix is both monikers
	/// </summary>
	public const int MK_S_US = 0x000401E6;
	/// <summary>
	/// Moniker is already registered in running object table
	/// </summary>
	public const int MK_S_MONIKERALREADYREGISTERED = 0x000401E7;
	/// <summary>
	/// The task is ready to run at its next scheduled time.
	/// </summary>
	public const int SCHED_S_TASK_READY = 0x00041300;
	/// <summary>
	/// The task is currently running.
	/// </summary>
	public const int SCHED_S_TASK_RUNNING = 0x00041301;
	/// <summary>
	/// The task will not run at the scheduled times because it has been disabled.
	/// </summary>
	public const int SCHED_S_TASK_DISABLED = 0x00041302;
	/// <summary>
	/// The task has not yet run.
	/// </summary>
	public const int SCHED_S_TASK_HAS_NOT_RUN = 0x00041303;
	/// <summary>
	/// There are no more runs scheduled for this task.
	/// </summary>
	public const int SCHED_S_TASK_NO_MORE_RUNS = 0x00041304;
	/// <summary>
	/// One or more of the properties that are needed to run this task on a schedule have not been set.
	/// </summary>
	public const int SCHED_S_TASK_NOT_SCHEDULED = 0x00041305;
	/// <summary>
	/// The last run of the task was terminated by the user.
	/// </summary>
	public const int SCHED_S_TASK_TERMINATED = 0x00041306;
	/// <summary>
	/// Either the task has no triggers or the existing triggers are disabled or not set.
	/// </summary>
	public const int SCHED_S_TASK_NO_VALID_TRIGGERS = 0x00041307;
	/// <summary>
	/// Event triggers don't have set run times.
	/// </summary>
	public const int SCHED_S_EVENT_TRIGGER = 0x00041308;
	/// <summary>
	/// Trigger not found.
	/// </summary>
	public const int SCHED_E_TRIGGER_NOT_FOUND = unchecked((int)0x80041309);
	/// <summary>
	/// One or more of the properties that are needed to run this task have not been set.
	/// </summary>
	public const int SCHED_E_TASK_NOT_READY = unchecked((int)0x8004130A);
	/// <summary>
	/// There is no running instance of the task.
	/// </summary>
	public const int SCHED_E_TASK_NOT_RUNNING = unchecked((int)0x8004130B);
	/// <summary>
	/// The Task Scheduler Service is not installed on this computer.
	/// </summary>
	public const int SCHED_E_SERVICE_NOT_INSTALLED = unchecked((int)0x8004130C);
	/// <summary>
	/// The task object could not be opened.
	/// </summary>
	public const int SCHED_E_CANNOT_OPEN_TASK = unchecked((int)0x8004130D);
	/// <summary>
	/// The object is either an invalid task object or is not a task object.
	/// </summary>
	public const int SCHED_E_INVALID_TASK = unchecked((int)0x8004130E);
	/// <summary>
	/// No account information could be found in the Task Scheduler security database for the task indicated.
	/// </summary>
	public const int SCHED_E_ACCOUNT_INFORMATION_NOT_SET = unchecked((int)0x8004130F);
	/// <summary>
	/// Unable to establish existence of the account specified.
	/// </summary>
	public const int SCHED_E_ACCOUNT_NAME_NOT_FOUND = unchecked((int)0x80041310);
	/// <summary>
	/// Corruption was detected in the Task Scheduler security database; the database has been reset.
	/// </summary>
	public const int SCHED_E_ACCOUNT_DBASE_CORRUPT = unchecked((int)0x80041311);
	/// <summary>
	/// Task Scheduler security services are not available.
	/// </summary>
	public const int SCHED_E_NO_SECURITY_SERVICES = unchecked((int)0x80041312);
	/// <summary>
	/// The task object version is either unsupported or invalid.
	/// </summary>
	public const int SCHED_E_UNKNOWN_OBJECT_VERSION = unchecked((int)0x80041313);
	/// <summary>
	/// The task has been configured with an unsupported combination of account settings and run time options.
	/// </summary>
	public const int SCHED_E_UNSUPPORTED_ACCOUNT_OPTION = unchecked((int)0x80041314);
	/// <summary>
	/// The Task Scheduler Service is not running.
	/// </summary>
	public const int SCHED_E_SERVICE_NOT_RUNNING = unchecked((int)0x80041315);
	/// <summary>
	/// The task XML contains an unexpected node.
	/// </summary>
	public const int SCHED_E_UNEXPECTEDNODE = unchecked((int)0x80041316);
	/// <summary>
	/// The task XML contains an element or attribute from an unexpected namespace.
	/// </summary>
	public const int SCHED_E_NAMESPACE = unchecked((int)0x80041317);
	/// <summary>
	/// The task XML contains a value which is incorrectly formatted or out of range.
	/// </summary>
	public const int SCHED_E_INVALIDVALUE = unchecked((int)0x80041318);
	/// <summary>
	/// The task XML is missing a required element or attribute.
	/// </summary>
	public const int SCHED_E_MISSINGNODE = unchecked((int)0x80041319);
	/// <summary>
	/// The task XML is malformed.
	/// </summary>
	public const int SCHED_E_MALFORMEDXML = unchecked((int)0x8004131A);
	/// <summary>
	/// The task is registered, but not all specified triggers will start the task.
	/// </summary>
	public const int SCHED_S_SOME_TRIGGERS_FAILED = 0x0004131B;
	/// <summary>
	/// The task is registered, but may fail to start. Batch logon privilege needs to be enabled for the task principal.
	/// </summary>
	public const int SCHED_S_BATCH_LOGON_PROBLEM = 0x0004131C;
	/// <summary>
	/// The task XML contains too many nodes of the same type.
	/// </summary>
	public const int SCHED_E_TOO_MANY_NODES = unchecked((int)0x8004131D);
	/// <summary>
	/// The task cannot be started after the trigger's end boundary.
	/// </summary>
	public const int SCHED_E_PAST_END_BOUNDARY = unchecked((int)0x8004131E);
	/// <summary>
	/// An instance of this task is already running.
	/// </summary>
	public const int SCHED_E_ALREADY_RUNNING = unchecked((int)0x8004131F);
	/// <summary>
	/// The task will not run because the user is not logged on.
	/// </summary>
	public const int SCHED_E_USER_NOT_LOGGED_ON = unchecked((int)0x80041320);
	/// <summary>
	/// The task image is corrupt or has been tampered with.
	/// </summary>
	public const int SCHED_E_INVALID_TASK_HASH = unchecked((int)0x80041321);
	/// <summary>
	/// The Task Scheduler service is not available.
	/// </summary>
	public const int SCHED_E_SERVICE_NOT_AVAILABLE = unchecked((int)0x80041322);
	/// <summary>
	/// The Task Scheduler service is too busy to handle your request. Please try again later.
	/// </summary>
	public const int SCHED_E_SERVICE_TOO_BUSY = unchecked((int)0x80041323);
	/// <summary>
	/// The Task Scheduler service attempted to run the task, but the task did not run due to one of the constraints in the task definition.
	/// </summary>
	public const int SCHED_E_TASK_ATTEMPTED = unchecked((int)0x80041324);
	/// <summary>
	/// The Task Scheduler service has asked the task to run.
	/// </summary>
	public const int SCHED_S_TASK_QUEUED = 0x00041325;
	/// <summary>
	/// The task is disabled.
	/// </summary>
	public const int SCHED_E_TASK_DISABLED = unchecked((int)0x80041326);
	/// <summary>
	/// The task has properties that are not compatible with previous versions of Windows.
	/// </summary>
	public const int SCHED_E_TASK_NOT_V1_COMPAT = unchecked((int)0x80041327);
	/// <summary>
	/// The task settings do not allow the task to start on demand.
	/// </summary>
	public const int SCHED_E_START_ON_DEMAND = unchecked((int)0x80041328);
	/// <summary>
	/// The combination of properties that task is using is not compatible with the scheduling engine.
	/// </summary>
	public const int SCHED_E_TASK_NOT_UBPM_COMPAT = unchecked((int)0x80041329);
	/// <summary>
	/// Attempt to create a class object failed
	/// </summary>
	public const int CO_E_CLASS_CREATE_FAILED = unchecked((int)0x80080001);
	/// <summary>
	/// OLE service could not bind object
	/// </summary>
	public const int CO_E_SCM_ERROR = unchecked((int)0x80080002);
	/// <summary>
	/// RPC communication failed with OLE service
	/// </summary>
	public const int CO_E_SCM_RPC_FAILURE = unchecked((int)0x80080003);
	/// <summary>
	/// Bad path to object
	/// </summary>
	public const int CO_E_BAD_PATH = unchecked((int)0x80080004);
	/// <summary>
	/// Server execution failed
	/// </summary>
	public const int CO_E_SERVER_EXEC_FAILURE = unchecked((int)0x80080005);
	/// <summary>
	/// OLE service could not communicate with the object server
	/// </summary>
	public const int CO_E_OBJSRV_RPC_FAILURE = unchecked((int)0x80080006);
	/// <summary>
	/// Moniker path could not be normalized
	/// </summary>
	public const int MK_E_NO_NORMALIZED = unchecked((int)0x80080007);
	/// <summary>
	/// Object server is stopping when OLE service contacts it
	/// </summary>
	public const int CO_E_SERVER_STOPPING = unchecked((int)0x80080008);
	/// <summary>
	/// An invalid root block pointer was specified
	/// </summary>
	public const int MEM_E_INVALID_ROOT = unchecked((int)0x80080009);
	/// <summary>
	/// An allocation chain contained an invalid link pointer
	/// </summary>
	public const int MEM_E_INVALID_LINK = unchecked((int)0x80080010);
	/// <summary>
	/// The requested allocation size was too large
	/// </summary>
	public const int MEM_E_INVALID_SIZE = unchecked((int)0x80080011);
	/// <summary>
	/// Not all the requested interfaces were available
	/// </summary>
	public const int CO_S_NOTALLINTERFACES = 0x00080012;
	/// <summary>
	/// The specified machine name was not found in the cache.
	/// </summary>
	public const int CO_S_MACHINENAMENOTFOUND = 0x00080013;
	/// <summary>
	/// The activation requires a display name to be present under the CLSID key.
	/// </summary>
	public const int CO_E_MISSING_DISPLAYNAME = unchecked((int)0x80080015);
	/// <summary>
	/// The activation requires that the RunAs value for the application is Activate As Activator.
	/// </summary>
	public const int CO_E_RUNAS_VALUE_MUST_BE_AAA = unchecked((int)0x80080016);
	/// <summary>
	/// The class is not configured to support Elevated activation.
	/// </summary>
	public const int CO_E_ELEVATION_DISABLED = unchecked((int)0x80080017);
	/// <summary>
	/// Unknown interface.
	/// </summary>
	public const int DISP_E_UNKNOWNINTERFACE = unchecked((int)0x80020001);
	/// <summary>
	/// Member not found.
	/// </summary>
	public const int DISP_E_MEMBERNOTFOUND = unchecked((int)0x80020003);
	/// <summary>
	/// Parameter not found.
	/// </summary>
	public const int DISP_E_PARAMNOTFOUND = unchecked((int)0x80020004);
	/// <summary>
	/// Type mismatch.
	/// </summary>
	public const int DISP_E_TYPEMISMATCH = unchecked((int)0x80020005);
	/// <summary>
	/// Unknown name.
	/// </summary>
	public const int DISP_E_UNKNOWNNAME = unchecked((int)0x80020006);
	/// <summary>
	/// No named arguments.
	/// </summary>
	public const int DISP_E_NONAMEDARGS = unchecked((int)0x80020007);
	/// <summary>
	/// Bad variable type.
	/// </summary>
	public const int DISP_E_BADVARTYPE = unchecked((int)0x80020008);
	/// <summary>
	/// Exception occurred.
	/// </summary>
	public const int DISP_E_EXCEPTION = unchecked((int)0x80020009);
	/// <summary>
	/// Out of present range.
	/// </summary>
	public const int DISP_E_OVERFLOW = unchecked((int)0x8002000A);
	/// <summary>
	/// Invalid index.
	/// </summary>
	public const int DISP_E_BADINDEX = unchecked((int)0x8002000B);
	/// <summary>
	/// Unknown language.
	/// </summary>
	public const int DISP_E_UNKNOWNLCID = unchecked((int)0x8002000C);
	/// <summary>
	/// Memory is locked.
	/// </summary>
	public const int DISP_E_ARRAYISLOCKED = unchecked((int)0x8002000D);
	/// <summary>
	/// Invalid number of parameters.
	/// </summary>
	public const int DISP_E_BADPARAMCOUNT = unchecked((int)0x8002000E);
	/// <summary>
	/// Parameter not optional.
	/// </summary>
	public const int DISP_E_PARAMNOTOPTIONAL = unchecked((int)0x8002000F);
	/// <summary>
	/// Invalid callee.
	/// </summary>
	public const int DISP_E_BADCALLEE = unchecked((int)0x80020010);
	/// <summary>
	/// Does not support a collection.
	/// </summary>
	public const int DISP_E_NOTACOLLECTION = unchecked((int)0x80020011);
	/// <summary>
	/// Division by zero.
	/// </summary>
	public const int DISP_E_DIVBYZERO = unchecked((int)0x80020012);
	/// <summary>
	/// Buffer too small
	/// </summary>
	public const int DISP_E_BUFFERTOOSMALL = unchecked((int)0x80020013);
	/// <summary>
	/// Buffer too small.
	/// </summary>
	public const int TYPE_E_BUFFERTOOSMALL = unchecked((int)0x80028016);
	/// <summary>
	/// Field name not defined in the record.
	/// </summary>
	public const int TYPE_E_FIELDNOTFOUND = unchecked((int)0x80028017);
	/// <summary>
	/// Old format or invalid type library.
	/// </summary>
	public const int TYPE_E_INVDATAREAD = unchecked((int)0x80028018);
	/// <summary>
	/// Old format or invalid type library.
	/// </summary>
	public const int TYPE_E_UNSUPFORMAT = unchecked((int)0x80028019);
	/// <summary>
	/// Error accessing the OLE registry.
	/// </summary>
	public const int TYPE_E_REGISTRYACCESS = unchecked((int)0x8002801C);
	/// <summary>
	/// Library not registered.
	/// </summary>
	public const int TYPE_E_LIBNOTREGISTERED = unchecked((int)0x8002801D);
	/// <summary>
	/// Bound to unknown type.
	/// </summary>
	public const int TYPE_E_UNDEFINEDTYPE = unchecked((int)0x80028027);
	/// <summary>
	/// Qualified name disallowed.
	/// </summary>
	public const int TYPE_E_QUALIFIEDNAMEDISALLOWED = unchecked((int)0x80028028);
	/// <summary>
	/// Invalid forward reference, or reference to uncompiled type.
	/// </summary>
	public const int TYPE_E_INVALIDSTATE = unchecked((int)0x80028029);
	/// <summary>
	/// Type mismatch.
	/// </summary>
	public const int TYPE_E_WRONGTYPEKIND = unchecked((int)0x8002802A);
	/// <summary>
	/// Element not found.
	/// </summary>
	public const int TYPE_E_ELEMENTNOTFOUND = unchecked((int)0x8002802B);
	/// <summary>
	/// Ambiguous name.
	/// </summary>
	public const int TYPE_E_AMBIGUOUSNAME = unchecked((int)0x8002802C);
	/// <summary>
	/// Name already exists in the library.
	/// </summary>
	public const int TYPE_E_NAMECONFLICT = unchecked((int)0x8002802D);
	/// <summary>
	/// Unknown LCID.
	/// </summary>
	public const int TYPE_E_UNKNOWNLCID = unchecked((int)0x8002802E);
	/// <summary>
	/// Function not defined in specified DLL.
	/// </summary>
	public const int TYPE_E_DLLFUNCTIONNOTFOUND = unchecked((int)0x8002802F);
	/// <summary>
	/// Wrong module kind for the operation.
	/// </summary>
	public const int TYPE_E_BADMODULEKIND = unchecked((int)0x800288BD);
	/// <summary>
	/// Size may not exceed 64K.
	/// </summary>
	public const int TYPE_E_SIZETOOBIG = unchecked((int)0x800288C5);
	/// <summary>
	/// Duplicate ID in inheritance hierarchy.
	/// </summary>
	public const int TYPE_E_DUPLICATEID = unchecked((int)0x800288C6);
	/// <summary>
	/// Incorrect inheritance depth in standard OLE hmember.
	/// </summary>
	public const int TYPE_E_INVALIDID = unchecked((int)0x800288CF);
	/// <summary>
	/// Type mismatch.
	/// </summary>
	public const int TYPE_E_TYPEMISMATCH = unchecked((int)0x80028CA0);
	/// <summary>
	/// Invalid number of arguments.
	/// </summary>
	public const int TYPE_E_OUTOFBOUNDS = unchecked((int)0x80028CA1);
	/// <summary>
	/// I/O Error.
	/// </summary>
	public const int TYPE_E_IOERROR = unchecked((int)0x80028CA2);
	/// <summary>
	/// Error creating unique tmp file.
	/// </summary>
	public const int TYPE_E_CANTCREATETMPFILE = unchecked((int)0x80028CA3);
	/// <summary>
	/// Error loading type library/DLL.
	/// </summary>
	public const int TYPE_E_CANTLOADLIBRARY = unchecked((int)0x80029C4A);
	/// <summary>
	/// Inconsistent property functions.
	/// </summary>
	public const int TYPE_E_INCONSISTENTPROPFUNCS = unchecked((int)0x80029C83);
	/// <summary>
	/// Circular dependency between types/modules.
	/// </summary>
	public const int TYPE_E_CIRCULARTYPE = unchecked((int)0x80029C84);
	#endregion

	#region STG, RPC
	/// <summary>
	/// Unable to perform requested operation.
	/// </summary>
	public const int STG_E_INVALIDFUNCTION = unchecked((int)0x80030001);
	/// <summary>
	/// could not be found.
	/// </summary>
	public const int STG_E_FILENOTFOUND = unchecked((int)0x80030002);
	/// <summary>
	/// The path %1 could not be found.
	/// </summary>
	public const int STG_E_PATHNOTFOUND = unchecked((int)0x80030003);
	/// <summary>
	/// There are insufficient resources to open another file.
	/// </summary>
	public const int STG_E_TOOMANYOPENFILES = unchecked((int)0x80030004);
	/// <summary>
	/// Access Denied.
	/// </summary>
	public const int STG_E_ACCESSDENIED = unchecked((int)0x80030005);
	/// <summary>
	/// Attempted an operation on an invalid object.
	/// </summary>
	public const int STG_E_INVALIDHANDLE = unchecked((int)0x80030006);
	/// <summary>
	/// There is insufficient memory available to complete operation.
	/// </summary>
	public const int STG_E_INSUFFICIENTMEMORY = unchecked((int)0x80030008);
	/// <summary>
	/// Invalid pointer error.
	/// </summary>
	public const int STG_E_INVALIDPOINTER = unchecked((int)0x80030009);
	/// <summary>
	/// There are no more entries to return.
	/// </summary>
	public const int STG_E_NOMOREFILES = unchecked((int)0x80030012);
	/// <summary>
	/// Disk is write-protected.
	/// </summary>
	public const int STG_E_DISKISWRITEPROTECTED = unchecked((int)0x80030013);
	/// <summary>
	/// An error occurred during a seek operation.
	/// </summary>
	public const int STG_E_SEEKERROR = unchecked((int)0x80030019);
	/// <summary>
	/// A disk error occurred during a write operation.
	/// </summary>
	public const int STG_E_WRITEFAULT = unchecked((int)0x8003001D);
	/// <summary>
	/// A disk error occurred during a read operation.
	/// </summary>
	public const int STG_E_READFAULT = unchecked((int)0x8003001E);
	/// <summary>
	/// A share violation has occurred.
	/// </summary>
	public const int STG_E_SHAREVIOLATION = unchecked((int)0x80030020);
	/// <summary>
	/// A lock violation has occurred.
	/// </summary>
	public const int STG_E_LOCKVIOLATION = unchecked((int)0x80030021);
	/// <summary>
	/// already exists.
	/// </summary>
	public const int STG_E_FILEALREADYEXISTS = unchecked((int)0x80030050);
	/// <summary>
	/// Invalid parameter error.
	/// </summary>
	public const int STG_E_INVALIDPARAMETER = unchecked((int)0x80030057);
	/// <summary>
	/// There is insufficient disk space to complete operation.
	/// </summary>
	public const int STG_E_MEDIUMFULL = unchecked((int)0x80030070);
	/// <summary>
	/// Illegal write of non-simple property to simple property set.
	/// </summary>
	public const int STG_E_PROPSETMISMATCHED = unchecked((int)0x800300F0);
	/// <summary>
	/// An API call exited abnormally.
	/// </summary>
	public const int STG_E_ABNORMALAPIEXIT = unchecked((int)0x800300FA);
	/// <summary>
	/// The file %1 is not a valid compound file.
	/// </summary>
	public const int STG_E_INVALIDHEADER = unchecked((int)0x800300FB);
	/// <summary>
	/// The name %1 is not valid.
	/// </summary>
	public const int STG_E_INVALIDNAME = unchecked((int)0x800300FC);
	/// <summary>
	/// An unexpected error occurred.
	/// </summary>
	public const int STG_E_UNKNOWN = unchecked((int)0x800300FD);
	/// <summary>
	/// That function is not implemented.
	/// </summary>
	public const int STG_E_UNIMPLEMENTEDFUNCTION = unchecked((int)0x800300FE);
	/// <summary>
	/// Invalid flag error.
	/// </summary>
	public const int STG_E_INVALIDFLAG = unchecked((int)0x800300FF);
	/// <summary>
	/// Attempted to use an object that is busy.
	/// </summary>
	public const int STG_E_INUSE = unchecked((int)0x80030100);
	/// <summary>
	/// The storage has been changed since the last commit.
	/// </summary>
	public const int STG_E_NOTCURRENT = unchecked((int)0x80030101);
	/// <summary>
	/// Attempted to use an object that has ceased to exist.
	/// </summary>
	public const int STG_E_REVERTED = unchecked((int)0x80030102);
	/// <summary>
	/// Can't save.
	/// </summary>
	public const int STG_E_CANTSAVE = unchecked((int)0x80030103);
	/// <summary>
	/// The compound file %1 was produced with an incompatible version of storage.
	/// </summary>
	public const int STG_E_OLDFORMAT = unchecked((int)0x80030104);
	/// <summary>
	/// The compound file %1 was produced with a newer version of storage.
	/// </summary>
	public const int STG_E_OLDDLL = unchecked((int)0x80030105);
	/// <summary>
	/// Share.exe or equivalent is required for operation.
	/// </summary>
	public const int STG_E_SHAREREQUIRED = unchecked((int)0x80030106);
	/// <summary>
	/// Illegal operation called on non-file based storage.
	/// </summary>
	public const int STG_E_NOTFILEBASEDSTORAGE = unchecked((int)0x80030107);
	/// <summary>
	/// Illegal operation called on object with extant marshallings.
	/// </summary>
	public const int STG_E_EXTANTMARSHALLINGS = unchecked((int)0x80030108);
	/// <summary>
	/// The docfile has been corrupted.
	/// </summary>
	public const int STG_E_DOCFILECORRUPT = unchecked((int)0x80030109);
	/// <summary>
	/// OLE32.DLL has been loaded at the wrong address.
	/// </summary>
	public const int STG_E_BADBASEADDRESS = unchecked((int)0x80030110);
	/// <summary>
	/// The compound file is too large for the current implementation
	/// </summary>
	public const int STG_E_DOCFILETOOLARGE = unchecked((int)0x80030111);
	/// <summary>
	/// The compound file was not created with the STGM_SIMPLE flag
	/// </summary>
	public const int STG_E_NOTSIMPLEFORMAT = unchecked((int)0x80030112);
	/// <summary>
	/// The file download was aborted abnormally. The file is incomplete.
	/// </summary>
	public const int STG_E_INCOMPLETE = unchecked((int)0x80030201);
	/// <summary>
	/// The file download has been terminated.
	/// </summary>
	public const int STG_E_TERMINATED = unchecked((int)0x80030202);
	/// <summary>
	/// The underlying file was converted to compound file format.
	/// </summary>
	public const int STG_S_CONVERTED = 0x00030200;
	/// <summary>
	/// The storage operation should block until more data is available.
	/// </summary>
	public const int STG_S_BLOCK = 0x00030201;
	/// <summary>
	/// The storage operation should retry immediately.
	/// </summary>
	public const int STG_S_RETRYNOW = 0x00030202;
	/// <summary>
	/// The notified event sink will not influence the storage operation.
	/// </summary>
	public const int STG_S_MONITORING = 0x00030203;
	/// <summary>
	/// Multiple opens prevent consolidated. (commit succeeded).
	/// </summary>
	public const int STG_S_MULTIPLEOPENS = 0x00030204;
	/// <summary>
	/// Consolidation of the storage file failed. (commit succeeded).
	/// </summary>
	public const int STG_S_CONSOLIDATIONFAILED = 0x00030205;
	/// <summary>
	/// Consolidation of the storage file is inappropriate. (commit succeeded).
	/// </summary>
	public const int STG_S_CANNOTCONSOLIDATE = 0x00030206;
	/// <summary>
	/// Generic Copy Protection Error.
	/// </summary>
	public const int STG_E_STATUS_COPY_PROTECTION_FAILURE = unchecked((int)0x80030305);
	/// <summary>
	/// Copy Protection Error - DVD CSS Authentication failed.
	/// </summary>
	public const int STG_E_CSS_AUTHENTICATION_FAILURE = unchecked((int)0x80030306);
	/// <summary>
	/// Copy Protection Error - The given sector does not have a valid CSS key.
	/// </summary>
	public const int STG_E_CSS_KEY_NOT_PRESENT = unchecked((int)0x80030307);
	/// <summary>
	/// Copy Protection Error - DVD session key not established.
	/// </summary>
	public const int STG_E_CSS_KEY_NOT_ESTABLISHED = unchecked((int)0x80030308);
	/// <summary>
	/// Copy Protection Error - The read failed because the sector is encrypted.
	/// </summary>
	public const int STG_E_CSS_SCRAMBLED_SECTOR = unchecked((int)0x80030309);
	/// <summary>
	/// Copy Protection Error - The current DVD's region does not correspond to the region setting of the drive.
	/// </summary>
	public const int STG_E_CSS_REGION_MISMATCH = unchecked((int)0x8003030A);
	/// <summary>
	/// Copy Protection Error - The drive's region setting may be permanent or the number of user resets has been exhausted.
	/// </summary>
	public const int STG_E_RESETS_EXHAUSTED = unchecked((int)0x8003030B);
	/// <summary>
	/// Call was rejected by callee.
	/// </summary>
	public const int RPC_E_CALL_REJECTED = unchecked((int)0x80010001);
	/// <summary>
	/// Call was canceled by the message filter.
	/// </summary>
	public const int RPC_E_CALL_CANCELED = unchecked((int)0x80010002);
	/// <summary>
	/// The caller is dispatching an intertask SendMessage call and cannot call out via PostMessage.
	/// </summary>
	public const int RPC_E_CANTPOST_INSENDCALL = unchecked((int)0x80010003);
	/// <summary>
	/// The caller is dispatching an asynchronous call and cannot make an outgoing call on behalf of this call.
	/// </summary>
	public const int RPC_E_CANTCALLOUT_INASYNCCALL = unchecked((int)0x80010004);
	/// <summary>
	/// It is illegal to call out while inside message filter.
	/// </summary>
	public const int RPC_E_CANTCALLOUT_INEXTERNALCALL = unchecked((int)0x80010005);
	/// <summary>
	/// The connection terminated or is in a bogus state and cannot be used any more. Other connections are still valid.
	/// </summary>
	public const int RPC_E_CONNECTION_TERMINATED = unchecked((int)0x80010006);
	/// <summary>
	/// The callee (server [not server application]) is not available and disappeared; all connections are invalid. The call may have executed.
	/// </summary>
	public const int RPC_E_SERVER_DIED = unchecked((int)0x80010007);
	/// <summary>
	/// The caller (client) disappeared while the callee (server) was processing a call.
	/// </summary>
	public const int RPC_E_CLIENT_DIED = unchecked((int)0x80010008);
	/// <summary>
	/// The data packet with the marshalled parameter data is incorrect.
	/// </summary>
	public const int RPC_E_INVALID_DATAPACKET = unchecked((int)0x80010009);
	/// <summary>
	/// The call was not transmitted properly; the message queue was full and was not emptied after yielding.
	/// </summary>
	public const int RPC_E_CANTTRANSMIT_CALL = unchecked((int)0x8001000A);
	/// <summary>
	/// The client (caller) cannot marshal the parameter data - low memory, etc.
	/// </summary>
	public const int RPC_E_CLIENT_CANTMARSHAL_DATA = unchecked((int)0x8001000B);
	/// <summary>
	/// The client (caller) cannot unmarshal the return data - low memory, etc.
	/// </summary>
	public const int RPC_E_CLIENT_CANTUNMARSHAL_DATA = unchecked((int)0x8001000C);
	/// <summary>
	/// The server (callee) cannot marshal the return data - low memory, etc.
	/// </summary>
	public const int RPC_E_SERVER_CANTMARSHAL_DATA = unchecked((int)0x8001000D);
	/// <summary>
	/// The server (callee) cannot unmarshal the parameter data - low memory, etc.
	/// </summary>
	public const int RPC_E_SERVER_CANTUNMARSHAL_DATA = unchecked((int)0x8001000E);
	/// <summary>
	/// Received data is invalid; could be server or client data.
	/// </summary>
	public const int RPC_E_INVALID_DATA = unchecked((int)0x8001000F);
	/// <summary>
	/// A particular parameter is invalid and cannot be (un)marshalled.
	/// </summary>
	public const int RPC_E_INVALID_PARAMETER = unchecked((int)0x80010010);
	/// <summary>
	/// There is no second outgoing call on same channel in DDE conversation.
	/// </summary>
	public const int RPC_E_CANTCALLOUT_AGAIN = unchecked((int)0x80010011);
	/// <summary>
	/// The callee (server [not server application]) is not available and disappeared; all connections are invalid. The call did not execute.
	/// </summary>
	public const int RPC_E_SERVER_DIED_DNE = unchecked((int)0x80010012);
	/// <summary>
	/// System call failed.
	/// </summary>
	public const int RPC_E_SYS_CALL_FAILED = unchecked((int)0x80010100);
	/// <summary>
	/// Could not allocate some required resource (memory, events, ...)
	/// </summary>
	public const int RPC_E_OUT_OF_RESOURCES = unchecked((int)0x80010101);
	/// <summary>
	/// Attempted to make calls on more than one thread in single threaded mode.
	/// </summary>
	public const int RPC_E_ATTEMPTED_MULTITHREAD = unchecked((int)0x80010102);
	/// <summary>
	/// The requested interface is not registered on the server object.
	/// </summary>
	public const int RPC_E_NOT_REGISTERED = unchecked((int)0x80010103);
	/// <summary>
	/// RPC could not call the server or could not return the results of calling the server.
	/// </summary>
	public const int RPC_E_FAULT = unchecked((int)0x80010104);
	/// <summary>
	/// The server threw an exception.
	/// </summary>
	public const int RPC_E_SERVERFAULT = unchecked((int)0x80010105);
	/// <summary>
	/// Cannot change thread mode after it is set.
	/// </summary>
	public const int RPC_E_CHANGED_MODE = unchecked((int)0x80010106);
	/// <summary>
	/// The method called does not exist on the server.
	/// </summary>
	public const int RPC_E_INVALIDMETHOD = unchecked((int)0x80010107);
	/// <summary>
	/// The object invoked has disconnected from its clients.
	/// </summary>
	public const int RPC_E_DISCONNECTED = unchecked((int)0x80010108);
	/// <summary>
	/// The object invoked chose not to process the call now. Try again later.
	/// </summary>
	public const int RPC_E_RETRY = unchecked((int)0x80010109);
	/// <summary>
	/// The message filter indicated that the application is busy.
	/// </summary>
	public const int RPC_E_SERVERCALL_RETRYLATER = unchecked((int)0x8001010A);
	/// <summary>
	/// The message filter rejected the call.
	/// </summary>
	public const int RPC_E_SERVERCALL_REJECTED = unchecked((int)0x8001010B);
	/// <summary>
	/// A call control interfaces was called with invalid data.
	/// </summary>
	public const int RPC_E_INVALID_CALLDATA = unchecked((int)0x8001010C);
	/// <summary>
	/// An outgoing call cannot be made since the application is dispatching an input-synchronous call.
	/// </summary>
	public const int RPC_E_CANTCALLOUT_ININPUTSYNCCALL = unchecked((int)0x8001010D);
	/// <summary>
	/// The application called an interface that was marshalled for a different thread.
	/// </summary>
	public const int RPC_E_WRONG_THREAD = unchecked((int)0x8001010E);
	/// <summary>
	/// CoInitialize has not been called on the current thread.
	/// </summary>
	public const int RPC_E_THREAD_NOT_INIT = unchecked((int)0x8001010F);
	/// <summary>
	/// The version of OLE on the client and server machines does not match.
	/// </summary>
	public const int RPC_E_VERSION_MISMATCH = unchecked((int)0x80010110);
	/// <summary>
	/// OLE received a packet with an invalid header.
	/// </summary>
	public const int RPC_E_INVALID_HEADER = unchecked((int)0x80010111);
	/// <summary>
	/// OLE received a packet with an invalid extension.
	/// </summary>
	public const int RPC_E_INVALID_EXTENSION = unchecked((int)0x80010112);
	/// <summary>
	/// The requested object or interface does not exist.
	/// </summary>
	public const int RPC_E_INVALID_IPID = unchecked((int)0x80010113);
	/// <summary>
	/// The requested object does not exist.
	/// </summary>
	public const int RPC_E_INVALID_OBJECT = unchecked((int)0x80010114);
	/// <summary>
	/// OLE has sent a request and is waiting for a reply.
	/// </summary>
	public const int RPC_S_CALLPENDING = unchecked((int)0x80010115);
	/// <summary>
	/// OLE is waiting before retrying a request.
	/// </summary>
	public const int RPC_S_WAITONTIMER = unchecked((int)0x80010116);
	/// <summary>
	/// Call context cannot be accessed after call completed.
	/// </summary>
	public const int RPC_E_CALL_COMPLETE = unchecked((int)0x80010117);
	/// <summary>
	/// Impersonate on unsecure calls is not supported.
	/// </summary>
	public const int RPC_E_UNSECURE_CALL = unchecked((int)0x80010118);
	/// <summary>
	/// Security must be initialized before any interfaces are marshalled or unmarshalled. It cannot be changed once initialized.
	/// </summary>
	public const int RPC_E_TOO_LATE = unchecked((int)0x80010119);
	/// <summary>
	/// No security packages are installed on this machine or the user is not logged on or there are no compatible security packages between the client and server.
	/// </summary>
	public const int RPC_E_NO_GOOD_SECURITY_PACKAGES = unchecked((int)0x8001011A);
	/// <summary>
	/// Access is denied.
	/// </summary>
	public const int RPC_E_ACCESS_DENIED = unchecked((int)0x8001011B);
	/// <summary>
	/// Remote calls are not allowed for this process.
	/// </summary>
	public const int RPC_E_REMOTE_DISABLED = unchecked((int)0x8001011C);
	/// <summary>
	/// The marshaled interface data packet (OBJREF) has an invalid or unknown format.
	/// </summary>
	public const int RPC_E_INVALID_OBJREF = unchecked((int)0x8001011D);
	/// <summary>
	/// No context is associated with this call. This happens for some custom marshalled calls and on the client side of the call.
	/// </summary>
	public const int RPC_E_NO_CONTEXT = unchecked((int)0x8001011E);
	/// <summary>
	/// This operation returned because the timeout period expired.
	/// </summary>
	public const int RPC_E_TIMEOUT = unchecked((int)0x8001011F);
	/// <summary>
	/// There are no synchronize objects to wait on.
	/// </summary>
	public const int RPC_E_NO_SYNC = unchecked((int)0x80010120);
	/// <summary>
	/// Full subject issuer chain SSL principal name expected from the server.
	/// </summary>
	public const int RPC_E_FULLSIC_REQUIRED = unchecked((int)0x80010121);
	/// <summary>
	/// Principal name is not a valid MSSTD name.
	/// </summary>
	public const int RPC_E_INVALID_STD_NAME = unchecked((int)0x80010122);
	/// <summary>
	/// Unable to impersonate DCOM client
	/// </summary>
	public const int CO_E_FAILEDTOIMPERSONATE = unchecked((int)0x80010123);
	/// <summary>
	/// Unable to obtain server's security context
	/// </summary>
	public const int CO_E_FAILEDTOGETSECCTX = unchecked((int)0x80010124);
	/// <summary>
	/// Unable to open the access token of the current thread
	/// </summary>
	public const int CO_E_FAILEDTOOPENTHREADTOKEN = unchecked((int)0x80010125);
	/// <summary>
	/// Unable to obtain user info from an access token
	/// </summary>
	public const int CO_E_FAILEDTOGETTOKENINFO = unchecked((int)0x80010126);
	/// <summary>
	/// The client who called IAccessControl::IsAccessPermitted was not the trustee provided to the method
	/// </summary>
	public const int CO_E_TRUSTEEDOESNTMATCHCLIENT = unchecked((int)0x80010127);
	/// <summary>
	/// Unable to obtain the client's security blanket
	/// </summary>
	public const int CO_E_FAILEDTOQUERYCLIENTBLANKET = unchecked((int)0x80010128);
	/// <summary>
	/// Unable to set a discretionary ACL into a security descriptor
	/// </summary>
	public const int CO_E_FAILEDTOSETDACL = unchecked((int)0x80010129);
	/// <summary>
	/// The system function, AccessCheck, returned false
	/// </summary>
	public const int CO_E_ACCESSCHECKFAILED = unchecked((int)0x8001012A);
	/// <summary>
	/// Either NetAccessDel or NetAccessAdd returned an error code.
	/// </summary>
	public const int CO_E_NETACCESSAPIFAILED = unchecked((int)0x8001012B);
	/// <summary>
	/// One of the trustee strings provided by the user did not conform to the &lt;Domain&gt;\&lt;Name&gt; syntax and it was not the "*" string
	/// </summary>
	public const int CO_E_WRONGTRUSTEENAMESYNTAX = unchecked((int)0x8001012C);
	/// <summary>
	/// One of the security identifiers provided by the user was invalid
	/// </summary>
	public const int CO_E_INVALIDSID = unchecked((int)0x8001012D);
	/// <summary>
	/// Unable to convert a wide character trustee string to a multibyte trustee string
	/// </summary>
	public const int CO_E_CONVERSIONFAILED = unchecked((int)0x8001012E);
	/// <summary>
	/// Unable to find a security identifier that corresponds to a trustee string provided by the user
	/// </summary>
	public const int CO_E_NOMATCHINGSIDFOUND = unchecked((int)0x8001012F);
	/// <summary>
	/// The system function, LookupAccountSID, failed
	/// </summary>
	public const int CO_E_LOOKUPACCSIDFAILED = unchecked((int)0x80010130);
	/// <summary>
	/// Unable to find a trustee name that corresponds to a security identifier provided by the user
	/// </summary>
	public const int CO_E_NOMATCHINGNAMEFOUND = unchecked((int)0x80010131);
	/// <summary>
	/// The system function, LookupAccountName, failed
	/// </summary>
	public const int CO_E_LOOKUPACCNAMEFAILED = unchecked((int)0x80010132);
	/// <summary>
	/// Unable to set or reset a serialization handle
	/// </summary>
	public const int CO_E_SETSERLHNDLFAILED = unchecked((int)0x80010133);
	/// <summary>
	/// Unable to obtain the Windows directory
	/// </summary>
	public const int CO_E_FAILEDTOGETWINDIR = unchecked((int)0x80010134);
	/// <summary>
	/// Path too long
	/// </summary>
	public const int CO_E_PATHTOOLONG = unchecked((int)0x80010135);
	/// <summary>
	/// Unable to generate a uuid.
	/// </summary>
	public const int CO_E_FAILEDTOGENUUID = unchecked((int)0x80010136);
	/// <summary>
	/// Unable to create file
	/// </summary>
	public const int CO_E_FAILEDTOCREATEFILE = unchecked((int)0x80010137);
	/// <summary>
	/// Unable to close a serialization handle or a file handle.
	/// </summary>
	public const int CO_E_FAILEDTOCLOSEHANDLE = unchecked((int)0x80010138);
	/// <summary>
	/// The number of ACEs in an ACL exceeds the system limit.
	/// </summary>
	public const int CO_E_EXCEEDSYSACLLIMIT = unchecked((int)0x80010139);
	/// <summary>
	/// Not all the DENY_ACCESS ACEs are arranged in front of the GRANT_ACCESS ACEs in the stream.
	/// </summary>
	public const int CO_E_ACESINWRONGORDER = unchecked((int)0x8001013A);
	/// <summary>
	/// The version of ACL format in the stream is not supported by this implementation of IAccessControl
	/// </summary>
	public const int CO_E_INCOMPATIBLESTREAMVERSION = unchecked((int)0x8001013B);
	/// <summary>
	/// Unable to open the access token of the server process
	/// </summary>
	public const int CO_E_FAILEDTOOPENPROCESSTOKEN = unchecked((int)0x8001013C);
	/// <summary>
	/// Unable to decode the ACL in the stream provided by the user
	/// </summary>
	public const int CO_E_DECODEFAILED = unchecked((int)0x8001013D);
	/// <summary>
	/// The COM IAccessControl object is not initialized
	/// </summary>
	public const int CO_E_ACNOTINITIALIZED = unchecked((int)0x8001013F);
	/// <summary>
	/// Call Cancellation is disabled
	/// </summary>
	public const int CO_E_CANCEL_DISABLED = unchecked((int)0x80010140);
	/// <summary>
	/// An internal error occurred.
	/// </summary>
	public const int RPC_E_UNEXPECTED = unchecked((int)0x8001FFFF);
	#endregion

	#region Security and Setup
	/// <summary>
	/// The specified event is currently not being audited.
	/// </summary>
	public const int ERROR_AUDITING_DISABLED = unchecked((int)0xC0090001);
	/// <summary>
	/// The SID filtering operation removed all SIDs.
	/// </summary>
	public const int ERROR_ALL_SIDS_FILTERED = unchecked((int)0xC0090002);
	/// <summary>
	/// Business rule scripts are disabled for the calling application.
	/// </summary>
	public const int ERROR_BIZRULES_NOT_ENABLED = unchecked((int)0xC0090003);
	/// <summary>
	/// The packaging API has encountered an internal error.
	/// </summary>
	public const int APPX_E_PACKAGING_INTERNAL = unchecked((int)0x80080200);
	/// <summary>
	/// The file is not a valid package because its contents are interleaved.
	/// </summary>
	public const int APPX_E_INTERLEAVING_NOT_ALLOWED = unchecked((int)0x80080201);
	/// <summary>
	/// The file is not a valid package because it contains OPC relationships.
	/// </summary>
	public const int APPX_E_RELATIONSHIPS_NOT_ALLOWED = unchecked((int)0x80080202);
	/// <summary>
	/// The file is not a valid package because it is missing a manifest or block map, or missing a signature file when the code integrity file is present.
	/// </summary>
	public const int APPX_E_MISSING_REQUIRED_FILE = unchecked((int)0x80080203);
	/// <summary>
	/// The package's manifest is invalid.
	/// </summary>
	public const int APPX_E_INVALID_MANIFEST = unchecked((int)0x80080204);
	/// <summary>
	/// The package's block map is invalid.
	/// </summary>
	public const int APPX_E_INVALID_BLOCKMAP = unchecked((int)0x80080205);
	/// <summary>
	/// The package's content cannot be read because it is corrupt.
	/// </summary>
	public const int APPX_E_CORRUPT_CONTENT = unchecked((int)0x80080206);
	/// <summary>
	/// The computed hash value of the block does not match the one stored in the block map.
	/// </summary>
	public const int APPX_E_BLOCK_HASH_INVALID = unchecked((int)0x80080207);
	/// <summary>
	/// The requested byte range is over 4GB when translated to byte range of blocks.
	/// </summary>
	public const int APPX_E_REQUESTED_RANGE_TOO_LARGE = unchecked((int)0x80080208);
	/// <summary>
	/// The SIP_SUBJECTINFO structure used to sign the package didn't contain the required data.
	/// </summary>
	public const int APPX_E_INVALID_SIP_CLIENT_DATA = unchecked((int)0x80080209);
	/// <summary>
	/// The app didn't start in the required time.
	/// </summary>
	public const int E_APPLICATION_ACTIVATION_TIMED_OUT = unchecked((int)0x8027025A);
	/// <summary>
	/// The app didn't start.
	/// </summary>
	public const int E_APPLICATION_ACTIVATION_EXEC_FAILURE = unchecked((int)0x8027025B);
	/// <summary>
	/// This app failed to launch because of an issue with its license. Please try again in a moment.
	/// </summary>
	public const int E_APPLICATION_TEMPORARY_LICENSE_ERROR = unchecked((int)0x8027025C);
	/// <summary>
	/// Bad UID.
	/// </summary>
	public const int NTE_BAD_UID = unchecked((int)0x80090001);
	/// <summary>
	/// Bad Hash.
	/// </summary>
	public const int NTE_BAD_HASH = unchecked((int)0x80090002);
	/// <summary>
	/// Bad Key.
	/// </summary>
	public const int NTE_BAD_KEY = unchecked((int)0x80090003);
	/// <summary>
	/// Bad Length.
	/// </summary>
	public const int NTE_BAD_LEN = unchecked((int)0x80090004);
	/// <summary>
	/// Bad Data.
	/// </summary>
	public const int NTE_BAD_DATA = unchecked((int)0x80090005);
	/// <summary>
	/// Invalid Signature.
	/// </summary>
	public const int NTE_BAD_SIGNATURE = unchecked((int)0x80090006);
	/// <summary>
	/// Bad Version of provider.
	/// </summary>
	public const int NTE_BAD_VER = unchecked((int)0x80090007);
	/// <summary>
	/// Invalid algorithm specified.
	/// </summary>
	public const int NTE_BAD_ALGID = unchecked((int)0x80090008);
	/// <summary>
	/// Invalid flags specified.
	/// </summary>
	public const int NTE_BAD_FLAGS = unchecked((int)0x80090009);
	/// <summary>
	/// Invalid type specified.
	/// </summary>
	public const int NTE_BAD_TYPE = unchecked((int)0x8009000A);
	/// <summary>
	/// Key not valid for use in specified state.
	/// </summary>
	public const int NTE_BAD_KEY_STATE = unchecked((int)0x8009000B);
	/// <summary>
	/// Hash not valid for use in specified state.
	/// </summary>
	public const int NTE_BAD_HASH_STATE = unchecked((int)0x8009000C);
	/// <summary>
	/// Key does not exist.
	/// </summary>
	public const int NTE_NO_KEY = unchecked((int)0x8009000D);
	/// <summary>
	/// Insufficient memory available for the operation.
	/// </summary>
	public const int NTE_NO_MEMORY = unchecked((int)0x8009000E);
	/// <summary>
	/// Object already exists.
	/// </summary>
	public const int NTE_EXISTS = unchecked((int)0x8009000F);
	/// <summary>
	/// Access denied.
	/// </summary>
	public const int NTE_PERM = unchecked((int)0x80090010);
	/// <summary>
	/// Object was not found.
	/// </summary>
	public const int NTE_NOT_FOUND = unchecked((int)0x80090011);
	/// <summary>
	/// Data already encrypted.
	/// </summary>
	public const int NTE_DOUBLE_ENCRYPT = unchecked((int)0x80090012);
	/// <summary>
	/// Invalid provider specified.
	/// </summary>
	public const int NTE_BAD_PROVIDER = unchecked((int)0x80090013);
	/// <summary>
	/// Invalid provider type specified.
	/// </summary>
	public const int NTE_BAD_PROV_TYPE = unchecked((int)0x80090014);
	/// <summary>
	/// Provider's public key is invalid.
	/// </summary>
	public const int NTE_BAD_PUBLIC_KEY = unchecked((int)0x80090015);
	/// <summary>
	/// Keyset does not exist
	/// </summary>
	public const int NTE_BAD_KEYSET = unchecked((int)0x80090016);
	/// <summary>
	/// Provider type not defined.
	/// </summary>
	public const int NTE_PROV_TYPE_NOT_DEF = unchecked((int)0x80090017);
	/// <summary>
	/// Provider type as registered is invalid.
	/// </summary>
	public const int NTE_PROV_TYPE_ENTRY_BAD = unchecked((int)0x80090018);
	/// <summary>
	/// The keyset is not defined.
	/// </summary>
	public const int NTE_KEYSET_NOT_DEF = unchecked((int)0x80090019);
	/// <summary>
	/// Keyset as registered is invalid.
	/// </summary>
	public const int NTE_KEYSET_ENTRY_BAD = unchecked((int)0x8009001A);
	/// <summary>
	/// Provider type does not match registered value.
	/// </summary>
	public const int NTE_PROV_TYPE_NO_MATCH = unchecked((int)0x8009001B);
	/// <summary>
	/// The digital signature file is corrupt.
	/// </summary>
	public const int NTE_SIGNATURE_FILE_BAD = unchecked((int)0x8009001C);
	/// <summary>
	/// Provider DLL failed to initialize correctly.
	/// </summary>
	public const int NTE_PROVIDER_DLL_FAIL = unchecked((int)0x8009001D);
	/// <summary>
	/// Provider DLL could not be found.
	/// </summary>
	public const int NTE_PROV_DLL_NOT_FOUND = unchecked((int)0x8009001E);
	/// <summary>
	/// The Keyset parameter is invalid.
	/// </summary>
	public const int NTE_BAD_KEYSET_PARAM = unchecked((int)0x8009001F);
	/// <summary>
	/// An internal error occurred.
	/// </summary>
	public const int NTE_FAIL = unchecked((int)0x80090020);
	/// <summary>
	/// A base error occurred.
	/// </summary>
	public const int NTE_SYS_ERR = unchecked((int)0x80090021);
	/// <summary>
	/// Provider could not perform the action since the context was acquired as silent.
	/// </summary>
	public const int NTE_SILENT_CONTEXT = unchecked((int)0x80090022);
	/// <summary>
	/// The security token does not have storage space available for an additional container.
	/// </summary>
	public const int NTE_TOKEN_KEYSET_STORAGE_FULL = unchecked((int)0x80090023);
	/// <summary>
	/// The profile for the user is a temporary profile.
	/// </summary>
	public const int NTE_TEMPORARY_PROFILE = unchecked((int)0x80090024);
	/// <summary>
	/// The key parameters could not be set because the CSP uses fixed parameters.
	/// </summary>
	public const int NTE_FIXEDPARAMETER = unchecked((int)0x80090025);
	/// <summary>
	/// The supplied handle is invalid.
	/// </summary>
	public const int NTE_INVALID_HANDLE = unchecked((int)0x80090026);
	/// <summary>
	/// The parameter is incorrect.
	/// </summary>
	public const int NTE_INVALID_PARAMETER = unchecked((int)0x80090027);
	/// <summary>
	/// The buffer supplied to a function was too small.
	/// </summary>
	public const int NTE_BUFFER_TOO_SMALL = unchecked((int)0x80090028);
	/// <summary>
	/// The requested operation is not supported.
	/// </summary>
	public const int NTE_NOT_SUPPORTED = unchecked((int)0x80090029);
	/// <summary>
	/// No more data is available.
	/// </summary>
	public const int NTE_NO_MORE_ITEMS = unchecked((int)0x8009002A);
	/// <summary>
	/// The supplied buffers overlap incorrectly.
	/// </summary>
	public const int NTE_BUFFERS_OVERLAP = unchecked((int)0x8009002B);
	/// <summary>
	/// The specified data could not be decrypted.
	/// </summary>
	public const int NTE_DECRYPTION_FAILURE = unchecked((int)0x8009002C);
	/// <summary>
	/// An internal consistency check failed.
	/// </summary>
	public const int NTE_INTERNAL_ERROR = unchecked((int)0x8009002D);
	/// <summary>
	/// This operation requires input from the user.
	/// </summary>
	public const int NTE_UI_REQUIRED = unchecked((int)0x8009002E);
	/// <summary>
	/// The cryptographic provider does not support HMAC.
	/// </summary>
	public const int NTE_HMAC_NOT_SUPPORTED = unchecked((int)0x8009002F);
	/// <summary>
	/// The device that is required by this cryptographic provider is not ready for use.
	/// </summary>
	public const int NTE_DEVICE_NOT_READY = unchecked((int)0x80090030);
	/// <summary>
	/// The dictionary attack mitigation is triggered and the provided authorization was ignored by the provider.
	/// </summary>
	public const int NTE_AUTHENTICATION_IGNORED = unchecked((int)0x80090031);
	/// <summary>
	/// The validation of the provided data failed the integrity or signature validation.
	/// </summary>
	public const int NTE_VALIDATION_FAILED = unchecked((int)0x80090032);
	/// <summary>
	/// Incorrect password.
	/// </summary>
	public const int NTE_INCORRECT_PASSWORD = unchecked((int)0x80090033);
	/// <summary>
	/// Encryption failed.
	/// </summary>
	public const int NTE_ENCRYPTION_FAILURE = unchecked((int)0x80090034);
	/// <summary>
	/// Not enough memory is available to complete this request
	/// </summary>
	public const int SEC_E_INSUFFICIENT_MEMORY = unchecked((int)0x80090300);
	/// <summary>
	/// The handle specified is invalid
	/// </summary>
	public const int SEC_E_INVALID_HANDLE = unchecked((int)0x80090301);
	/// <summary>
	/// The function requested is not supported
	/// </summary>
	public const int SEC_E_UNSUPPORTED_FUNCTION = unchecked((int)0x80090302);
	/// <summary>
	/// The specified target is unknown or unreachable
	/// </summary>
	public const int SEC_E_TARGET_UNKNOWN = unchecked((int)0x80090303);
	/// <summary>
	/// The Local Security Authority cannot be contacted
	/// </summary>
	public const int SEC_E_INTERNAL_ERROR = unchecked((int)0x80090304);
	/// <summary>
	/// The requested security package does not exist
	/// </summary>
	public const int SEC_E_SECPKG_NOT_FOUND = unchecked((int)0x80090305);
	/// <summary>
	/// The caller is not the owner of the desired credentials
	/// </summary>
	public const int SEC_E_NOT_OWNER = unchecked((int)0x80090306);
	/// <summary>
	/// The security package failed to initialize, and cannot be installed
	/// </summary>
	public const int SEC_E_CANNOT_INSTALL = unchecked((int)0x80090307);
	/// <summary>
	/// The token supplied to the function is invalid
	/// </summary>
	public const int SEC_E_INVALID_TOKEN = unchecked((int)0x80090308);
	/// <summary>
	/// The security package is not able to marshal the logon buffer, so the logon attempt has failed
	/// </summary>
	public const int SEC_E_CANNOT_PACK = unchecked((int)0x80090309);
	/// <summary>
	/// The per-message Quality of Protection is not supported by the security package
	/// </summary>
	public const int SEC_E_QOP_NOT_SUPPORTED = unchecked((int)0x8009030A);
	/// <summary>
	/// The security context does not allow impersonation of the client
	/// </summary>
	public const int SEC_E_NO_IMPERSONATION = unchecked((int)0x8009030B);
	/// <summary>
	/// The logon attempt failed
	/// </summary>
	public const int SEC_E_LOGON_DENIED = unchecked((int)0x8009030C);
	/// <summary>
	/// The credentials supplied to the package were not recognized
	/// </summary>
	public const int SEC_E_UNKNOWN_CREDENTIALS = unchecked((int)0x8009030D);
	/// <summary>
	/// No credentials are available in the security package
	/// </summary>
	public const int SEC_E_NO_CREDENTIALS = unchecked((int)0x8009030E);
	/// <summary>
	/// The message or signature supplied for verification has been altered
	/// </summary>
	public const int SEC_E_MESSAGE_ALTERED = unchecked((int)0x8009030F);
	/// <summary>
	/// The message supplied for verification is out of sequence
	/// </summary>
	public const int SEC_E_OUT_OF_SEQUENCE = unchecked((int)0x80090310);
	/// <summary>
	/// No authority could be contacted for authentication.
	/// </summary>
	public const int SEC_E_NO_AUTHENTICATING_AUTHORITY = unchecked((int)0x80090311);
	/// <summary>
	/// The function completed successfully, but must be called again to complete the context
	/// </summary>
	public const int SEC_I_CONTINUE_NEEDED = 0x00090312;
	/// <summary>
	/// The function completed successfully, but CompleteToken must be called
	/// </summary>
	public const int SEC_I_COMPLETE_NEEDED = 0x00090313;
	/// <summary>
	/// The function completed successfully, but both CompleteToken and this function must be called to complete the context
	/// </summary>
	public const int SEC_I_COMPLETE_AND_CONTINUE = 0x00090314;
	/// <summary>
	/// The logon was completed, but no network authority was available. The logon was made using locally known information
	/// </summary>
	public const int SEC_I_LOCAL_LOGON = 0x00090315;
	/// <summary>
	/// The requested security package does not exist
	/// </summary>
	public const int SEC_E_BAD_PKGID = unchecked((int)0x80090316);
	/// <summary>
	/// The context has expired and can no longer be used.
	/// </summary>
	public const int SEC_E_CONTEXT_EXPIRED = unchecked((int)0x80090317);
	/// <summary>
	/// The context has expired and can no longer be used.
	/// </summary>
	public const int SEC_I_CONTEXT_EXPIRED = 0x00090317;
	/// <summary>
	/// The supplied message is incomplete. The signature was not verified.
	/// </summary>
	public const int SEC_E_INCOMPLETE_MESSAGE = unchecked((int)0x80090318);
	/// <summary>
	/// The credentials supplied were not complete, and could not be verified. The context could not be initialized.
	/// </summary>
	public const int SEC_E_INCOMPLETE_CREDENTIALS = unchecked((int)0x80090320);
	/// <summary>
	/// The buffers supplied to a function was too small.
	/// </summary>
	public const int SEC_E_BUFFER_TOO_SMALL = unchecked((int)0x80090321);
	/// <summary>
	/// The credentials supplied were not complete, and could not be verified. Additional information can be returned from the context.
	/// </summary>
	public const int SEC_I_INCOMPLETE_CREDENTIALS = 0x00090320;
	/// <summary>
	/// The context data must be renegotiated with the peer.
	/// </summary>
	public const int SEC_I_RENEGOTIATE = 0x00090321;
	/// <summary>
	/// The target principal name is incorrect.
	/// </summary>
	public const int SEC_E_WRONG_PRINCIPAL = unchecked((int)0x80090322);
	/// <summary>
	/// There is no LSA mode context associated with this context.
	/// </summary>
	public const int SEC_I_NO_LSA_CONTEXT = 0x00090323;
	/// <summary>
	/// The clocks on the client and server machines are skewed.
	/// </summary>
	public const int SEC_E_TIME_SKEW = unchecked((int)0x80090324);
	/// <summary>
	/// The certificate chain was issued by an authority that is not trusted.
	/// </summary>
	public const int SEC_E_UNTRUSTED_ROOT = unchecked((int)0x80090325);
	/// <summary>
	/// The message received was unexpected or badly formatted.
	/// </summary>
	public const int SEC_E_ILLEGAL_MESSAGE = unchecked((int)0x80090326);
	/// <summary>
	/// An unknown error occurred while processing the certificate.
	/// </summary>
	public const int SEC_E_CERT_UNKNOWN = unchecked((int)0x80090327);
	/// <summary>
	/// The received certificate has expired.
	/// </summary>
	public const int SEC_E_CERT_EXPIRED = unchecked((int)0x80090328);
	/// <summary>
	/// The specified data could not be encrypted.
	/// </summary>
	public const int SEC_E_ENCRYPT_FAILURE = unchecked((int)0x80090329);
	/// <summary>
	/// The specified data could not be decrypted.
	/// </summary>
	public const int SEC_E_DECRYPT_FAILURE = unchecked((int)0x80090330);
	/// <summary>
	/// The client and server cannot communicate, because they do not possess a common algorithm.
	/// </summary>
	public const int SEC_E_ALGORITHM_MISMATCH = unchecked((int)0x80090331);
	/// <summary>
	/// The security context could not be established due to a failure in the requested quality of service (e.g. mutual authentication or delegation).
	/// </summary>
	public const int SEC_E_SECURITY_QOS_FAILED = unchecked((int)0x80090332);
	/// <summary>
	/// A security context was deleted before the context was completed. This is considered a logon failure.
	/// </summary>
	public const int SEC_E_UNFINISHED_CONTEXT_DELETED = unchecked((int)0x80090333);
	/// <summary>
	/// The client is trying to negotiate a context and the server requires user-to-user but didn't send a TGT reply.
	/// </summary>
	public const int SEC_E_NO_TGT_REPLY = unchecked((int)0x80090334);
	/// <summary>
	/// Unable to accomplish the requested task because the local machine does not have any IP addresses.
	/// </summary>
	public const int SEC_E_NO_IP_ADDRESSES = unchecked((int)0x80090335);
	/// <summary>
	/// The supplied credential handle does not match the credential associated with the security context.
	/// </summary>
	public const int SEC_E_WRONG_CREDENTIAL_HANDLE = unchecked((int)0x80090336);
	/// <summary>
	/// The crypto system or checksum function is invalid because a required function is unavailable.
	/// </summary>
	public const int SEC_E_CRYPTO_SYSTEM_INVALID = unchecked((int)0x80090337);
	/// <summary>
	/// The number of maximum ticket referrals has been exceeded.
	/// </summary>
	public const int SEC_E_MAX_REFERRALS_EXCEEDED = unchecked((int)0x80090338);
	/// <summary>
	/// The local machine must be a Kerberos KDC (domain controller) and it is not.
	/// </summary>
	public const int SEC_E_MUST_BE_KDC = unchecked((int)0x80090339);
	/// <summary>
	/// The other end of the security negotiation is requires strong crypto but it is not supported on the local machine.
	/// </summary>
	public const int SEC_E_STRONG_CRYPTO_NOT_SUPPORTED = unchecked((int)0x8009033A);
	/// <summary>
	/// The KDC reply contained more than one principal name.
	/// </summary>
	public const int SEC_E_TOO_MANY_PRINCIPALS = unchecked((int)0x8009033B);
	/// <summary>
	/// Expected to find PA data for a hint of what etype to use, but it was not found.
	/// </summary>
	public const int SEC_E_NO_PA_DATA = unchecked((int)0x8009033C);
	/// <summary>
	/// The client certificate does not contain a valid UPN, or does not match the client name in the logon request. Please contact your administrator.
	/// </summary>
	public const int SEC_E_PKINIT_NAME_MISMATCH = unchecked((int)0x8009033D);
	/// <summary>
	/// Smartcard logon is required and was not used.
	/// </summary>
	public const int SEC_E_SMARTCARD_LOGON_REQUIRED = unchecked((int)0x8009033E);
	/// <summary>
	/// A system shutdown is in progress.
	/// </summary>
	public const int SEC_E_SHUTDOWN_IN_PROGRESS = unchecked((int)0x8009033F);
	/// <summary>
	/// An invalid request was sent to the KDC.
	/// </summary>
	public const int SEC_E_KDC_INVALID_REQUEST = unchecked((int)0x80090340);
	/// <summary>
	/// The KDC was unable to generate a referral for the service requested.
	/// </summary>
	public const int SEC_E_KDC_UNABLE_TO_REFER = unchecked((int)0x80090341);
	/// <summary>
	/// The encryption type requested is not supported by the KDC.
	/// </summary>
	public const int SEC_E_KDC_UNKNOWN_ETYPE = unchecked((int)0x80090342);
	/// <summary>
	/// An unsupported preauthentication mechanism was presented to the Kerberos package.
	/// </summary>
	public const int SEC_E_UNSUPPORTED_PREAUTH = unchecked((int)0x80090343);
	/// <summary>
	/// The requested operation cannot be completed. The computer must be trusted for delegation and the current user account must be configured to allow delegation.
	/// </summary>
	public const int SEC_E_DELEGATION_REQUIRED = unchecked((int)0x80090345);
	/// <summary>
	/// Client's supplied SSPI channel bindings were incorrect.
	/// </summary>
	public const int SEC_E_BAD_BINDINGS = unchecked((int)0x80090346);
	/// <summary>
	/// The received certificate was mapped to multiple accounts.
	/// </summary>
	public const int SEC_E_MULTIPLE_ACCOUNTS = unchecked((int)0x80090347);
	/// <summary>
	/// SEC_E_NO_KERB_KEY
	/// </summary>
	public const int SEC_E_NO_KERB_KEY = unchecked((int)0x80090348);
	/// <summary>
	/// The certificate is not valid for the requested usage.
	/// </summary>
	public const int SEC_E_CERT_WRONG_USAGE = unchecked((int)0x80090349);
	/// <summary>
	/// The system cannot contact a domain controller to service the authentication request. Please try again later.
	/// </summary>
	public const int SEC_E_DOWNGRADE_DETECTED = unchecked((int)0x80090350);
	/// <summary>
	/// The smartcard certificate used for authentication has been revoked. Please contact your system administrator. There may be additional information in the event log.
	/// </summary>
	public const int SEC_E_SMARTCARD_CERT_REVOKED = unchecked((int)0x80090351);
	/// <summary>
	/// An untrusted certificate authority was detected While processing the smartcard certificate used for authentication. Please contact your system administrator.
	/// </summary>
	public const int SEC_E_ISSUING_CA_UNTRUSTED = unchecked((int)0x80090352);
	/// <summary>
	/// The revocation status of the smartcard certificate used for authentication could not be determined. Please contact your system administrator.
	/// </summary>
	public const int SEC_E_REVOCATION_OFFLINE_C = unchecked((int)0x80090353);
	/// <summary>
	/// The smartcard certificate used for authentication was not trusted. Please contact your system administrator.
	/// </summary>
	public const int SEC_E_PKINIT_CLIENT_FAILURE = unchecked((int)0x80090354);
	/// <summary>
	/// The smartcard certificate used for authentication has expired. Please contact your system administrator.
	/// </summary>
	public const int SEC_E_SMARTCARD_CERT_EXPIRED = unchecked((int)0x80090355);
	/// <summary>
	/// The Kerberos subsystem encountered an error. A service for user protocol request was made against a domain controller which does not support service for user.
	/// </summary>
	public const int SEC_E_NO_S4U_PROT_SUPPORT = unchecked((int)0x80090356);
	/// <summary>
	/// An attempt was made by this server to make a Kerberos constrained delegation request for a target outside of the server's realm. This is not supported, and indicates a misconfiguration on this server's allowed to delegate to list. Please contact your administrator.
	/// </summary>
	public const int SEC_E_CROSSREALM_DELEGATION_FAILURE = unchecked((int)0x80090357);
	/// <summary>
	/// The revocation status of the domain controller certificate used for smartcard authentication could not be determined. There is additional information in the system event log. Please contact your system administrator.
	/// </summary>
	public const int SEC_E_REVOCATION_OFFLINE_KDC = unchecked((int)0x80090358);
	/// <summary>
	/// An untrusted certificate authority was detected while processing the domain controller certificate used for authentication. There is additional information in the system event log. Please contact your system administrator.
	/// </summary>
	public const int SEC_E_ISSUING_CA_UNTRUSTED_KDC = unchecked((int)0x80090359);
	/// <summary>
	/// The domain controller certificate used for smartcard logon has expired. Please contact your system administrator with the contents of your system event log.
	/// </summary>
	public const int SEC_E_KDC_CERT_EXPIRED = unchecked((int)0x8009035A);
	/// <summary>
	/// The domain controller certificate used for smartcard logon has been revoked. Please contact your system administrator with the contents of your system event log.
	/// </summary>
	public const int SEC_E_KDC_CERT_REVOKED = unchecked((int)0x8009035B);
	/// <summary>
	/// A signature operation must be performed before the user can authenticate.
	/// </summary>
	public const int SEC_I_SIGNATURE_NEEDED = 0x0009035C;
	/// <summary>
	/// One or more of the parameters passed to the function was invalid.
	/// </summary>
	public const int SEC_E_INVALID_PARAMETER = unchecked((int)0x8009035D);
	/// <summary>
	/// Client policy does not allow credential delegation to target server.
	/// </summary>
	public const int SEC_E_DELEGATION_POLICY = unchecked((int)0x8009035E);
	/// <summary>
	/// Client policy does not allow credential delegation to target server with NLTM only authentication.
	/// </summary>
	public const int SEC_E_POLICY_NLTM_ONLY = unchecked((int)0x8009035F);
	/// <summary>
	/// The recipient rejected the renegotiation request.
	/// </summary>
	public const int SEC_I_NO_RENEGOTIATION = 0x00090360;
	/// <summary>
	/// The required security context does not exist.
	/// </summary>
	public const int SEC_E_NO_CONTEXT = unchecked((int)0x80090361);
	/// <summary>
	/// The PKU2U protocol encountered an error while attempting to utilize the associated certificates.
	/// </summary>
	public const int SEC_E_PKU2U_CERT_FAILURE = unchecked((int)0x80090362);
	/// <summary>
	/// The identity of the server computer could not be verified.
	/// </summary>
	public const int SEC_E_MUTUAL_AUTH_FAILED = unchecked((int)0x80090363);
	/// <summary>
	/// The returned buffer is only a fragment of the message. More fragments need to be returned.
	/// </summary>
	public const int SEC_I_MESSAGE_FRAGMENT = 0x00090364;
	/// <summary>
	/// Only https scheme is allowed.
	/// </summary>
	public const int SEC_E_ONLY_HTTPS_ALLOWED = unchecked((int)0x80090365);
	/// <summary>
	/// The function completed successfully, but must be called again to complete the context. Early start can be used.
	/// </summary>
	public const int SEC_I_CONTINUE_NEEDED_MESSAGE_OK = unchecked((int)0x80090366);
	/// <summary>
	/// An error occurred while performing an operation on a cryptographic message.
	/// </summary>
	public const int CRYPT_E_MSG_ERROR = unchecked((int)0x80091001);
	/// <summary>
	/// Unknown cryptographic algorithm.
	/// </summary>
	public const int CRYPT_E_UNKNOWN_ALGO = unchecked((int)0x80091002);
	/// <summary>
	/// The object identifier is poorly formatted.
	/// </summary>
	public const int CRYPT_E_OID_FORMAT = unchecked((int)0x80091003);
	/// <summary>
	/// Invalid cryptographic message type.
	/// </summary>
	public const int CRYPT_E_INVALID_MSG_TYPE = unchecked((int)0x80091004);
	/// <summary>
	/// Unexpected cryptographic message encoding.
	/// </summary>
	public const int CRYPT_E_UNEXPECTED_ENCODING = unchecked((int)0x80091005);
	/// <summary>
	/// The cryptographic message does not contain an expected authenticated attribute.
	/// </summary>
	public const int CRYPT_E_AUTH_ATTR_MISSING = unchecked((int)0x80091006);
	/// <summary>
	/// The hash value is not correct.
	/// </summary>
	public const int CRYPT_E_HASH_VALUE = unchecked((int)0x80091007);
	/// <summary>
	/// The index value is not valid.
	/// </summary>
	public const int CRYPT_E_INVALID_INDEX = unchecked((int)0x80091008);
	/// <summary>
	/// The content of the cryptographic message has already been decrypted.
	/// </summary>
	public const int CRYPT_E_ALREADY_DECRYPTED = unchecked((int)0x80091009);
	/// <summary>
	/// The content of the cryptographic message has not been decrypted yet.
	/// </summary>
	public const int CRYPT_E_NOT_DECRYPTED = unchecked((int)0x8009100A);
	/// <summary>
	/// The enveloped-data message does not contain the specified recipient.
	/// </summary>
	public const int CRYPT_E_RECIPIENT_NOT_FOUND = unchecked((int)0x8009100B);
	/// <summary>
	/// Invalid control type.
	/// </summary>
	public const int CRYPT_E_CONTROL_TYPE = unchecked((int)0x8009100C);
	/// <summary>
	/// Invalid issuer and/or serial number.
	/// </summary>
	public const int CRYPT_E_ISSUER_SERIALNUMBER = unchecked((int)0x8009100D);
	/// <summary>
	/// Cannot find the original signer.
	/// </summary>
	public const int CRYPT_E_SIGNER_NOT_FOUND = unchecked((int)0x8009100E);
	/// <summary>
	/// The cryptographic message does not contain all of the requested attributes.
	/// </summary>
	public const int CRYPT_E_ATTRIBUTES_MISSING = unchecked((int)0x8009100F);
	/// <summary>
	/// The streamed cryptographic message is not ready to return data.
	/// </summary>
	public const int CRYPT_E_STREAM_MSG_NOT_READY = unchecked((int)0x80091010);
	/// <summary>
	/// The streamed cryptographic message requires more data to complete the decode operation.
	/// </summary>
	public const int CRYPT_E_STREAM_INSUFFICIENT_DATA = unchecked((int)0x80091011);
	/// <summary>
	/// The protected data needs to be re-protected.
	/// </summary>
	public const int CRYPT_I_NEW_PROTECTION_REQUIRED = 0x00091012;
	/// <summary>
	/// The length specified for the output data was insufficient.
	/// </summary>
	public const int CRYPT_E_BAD_LEN = unchecked((int)0x80092001);
	/// <summary>
	/// An error occurred during encode or decode operation.
	/// </summary>
	public const int CRYPT_E_BAD_ENCODE = unchecked((int)0x80092002);
	/// <summary>
	/// An error occurred while reading or writing to a file.
	/// </summary>
	public const int CRYPT_E_FILE_ERROR = unchecked((int)0x80092003);
	/// <summary>
	/// Cannot find object or property.
	/// </summary>
	public const int CRYPT_E_NOT_FOUND = unchecked((int)0x80092004);
	/// <summary>
	/// The object or property already exists.
	/// </summary>
	public const int CRYPT_E_EXISTS = unchecked((int)0x80092005);
	/// <summary>
	/// No provider was specified for the store or object.
	/// </summary>
	public const int CRYPT_E_NO_PROVIDER = unchecked((int)0x80092006);
	/// <summary>
	/// The specified certificate is self signed.
	/// </summary>
	public const int CRYPT_E_SELF_SIGNED = unchecked((int)0x80092007);
	/// <summary>
	/// The previous certificate or CRL context was deleted.
	/// </summary>
	public const int CRYPT_E_DELETED_PREV = unchecked((int)0x80092008);
	/// <summary>
	/// Cannot find the requested object.
	/// </summary>
	public const int CRYPT_E_NO_MATCH = unchecked((int)0x80092009);
	/// <summary>
	/// The certificate does not have a property that references a private key.
	/// </summary>
	public const int CRYPT_E_UNEXPECTED_MSG_TYPE = unchecked((int)0x8009200A);
	/// <summary>
	/// Cannot find the certificate and private key for decryption.
	/// </summary>
	public const int CRYPT_E_NO_KEY_PROPERTY = unchecked((int)0x8009200B);
	/// <summary>
	/// Cannot find the certificate and private key to use for decryption.
	/// </summary>
	public const int CRYPT_E_NO_DECRYPT_CERT = unchecked((int)0x8009200C);
	/// <summary>
	/// Not a cryptographic message or the cryptographic message is not formatted correctly.
	/// </summary>
	public const int CRYPT_E_BAD_MSG = unchecked((int)0x8009200D);
	/// <summary>
	/// The signed cryptographic message does not have a signer for the specified signer index.
	/// </summary>
	public const int CRYPT_E_NO_SIGNER = unchecked((int)0x8009200E);
	/// <summary>
	/// Final closure is pending until additional frees or closes.
	/// </summary>
	public const int CRYPT_E_PENDING_CLOSE = unchecked((int)0x8009200F);
	/// <summary>
	/// The certificate is revoked.
	/// </summary>
	public const int CRYPT_E_REVOKED = unchecked((int)0x80092010);
	/// <summary>
	/// No Dll or exported function was found to verify revocation.
	/// </summary>
	public const int CRYPT_E_NO_REVOCATION_DLL = unchecked((int)0x80092011);
	/// <summary>
	/// The revocation function was unable to check revocation for the certificate.
	/// </summary>
	public const int CRYPT_E_NO_REVOCATION_CHECK = unchecked((int)0x80092012);
	/// <summary>
	/// The revocation function was unable to check revocation because the revocation server was offline.
	/// </summary>
	public const int CRYPT_E_REVOCATION_OFFLINE = unchecked((int)0x80092013);
	/// <summary>
	/// The certificate is not in the revocation server's database.
	/// </summary>
	public const int CRYPT_E_NOT_IN_REVOCATION_DATABASE = unchecked((int)0x80092014);
	/// <summary>
	/// The string contains a non-numeric character.
	/// </summary>
	public const int CRYPT_E_INVALID_NUMERIC_STRING = unchecked((int)0x80092020);
	/// <summary>
	/// The string contains a non-printable character.
	/// </summary>
	public const int CRYPT_E_INVALID_PRINTABLE_STRING = unchecked((int)0x80092021);
	/// <summary>
	/// The string contains a character not in the 7 bit ASCII character set.
	/// </summary>
	public const int CRYPT_E_INVALID_IA5_STRING = unchecked((int)0x80092022);
	/// <summary>
	/// The string contains an invalid X500 name attribute key, oid, value or delimiter.
	/// </summary>
	public const int CRYPT_E_INVALID_X500_STRING = unchecked((int)0x80092023);
	/// <summary>
	/// The dwValueType for the CERT_NAME_VALUE is not one of the character strings. Most likely it is either a CERT_RDN_ENCODED_BLOB or CERT_RDN_OCTET_STRING.
	/// </summary>
	public const int CRYPT_E_NOT_CHAR_STRING = unchecked((int)0x80092024);
	/// <summary>
	/// The Put operation cannot continue. The file needs to be resized. However, there is already a signature present. A complete signing operation must be done.
	/// </summary>
	public const int CRYPT_E_FILERESIZED = unchecked((int)0x80092025);
	/// <summary>
	/// The cryptographic operation failed due to a local security option setting.
	/// </summary>
	public const int CRYPT_E_SECURITY_SETTINGS = unchecked((int)0x80092026);
	/// <summary>
	/// No DLL or exported function was found to verify subject usage.
	/// </summary>
	public const int CRYPT_E_NO_VERIFY_USAGE_DLL = unchecked((int)0x80092027);
	/// <summary>
	/// The called function was unable to do a usage check on the subject.
	/// </summary>
	public const int CRYPT_E_NO_VERIFY_USAGE_CHECK = unchecked((int)0x80092028);
	/// <summary>
	/// Since the server was offline, the called function was unable to complete the usage check.
	/// </summary>
	public const int CRYPT_E_VERIFY_USAGE_OFFLINE = unchecked((int)0x80092029);
	/// <summary>
	/// The subject was not found in a Certificate Trust List (CTL).
	/// </summary>
	public const int CRYPT_E_NOT_IN_CTL = unchecked((int)0x8009202A);
	/// <summary>
	/// None of the signers of the cryptographic message or certificate trust list is trusted.
	/// </summary>
	public const int CRYPT_E_NO_TRUSTED_SIGNER = unchecked((int)0x8009202B);
	/// <summary>
	/// The public key's algorithm parameters are missing.
	/// </summary>
	public const int CRYPT_E_MISSING_PUBKEY_PARA = unchecked((int)0x8009202C);
	/// <summary>
	/// An object could not be located using the object locator infrastructure with the given name.
	/// </summary>
	public const int CRYPT_E_OBJECT_LOCATOR_NOT_FOUND = unchecked((int)0x8009202d);
	/// <summary>
	/// OSS Certificate encode/decode error code base See asn1code.h for a definition of the OSS runtime errors. The OSS error values are offset by CRYPT_E_OSS_ERROR.
	/// </summary>
	public const int CRYPT_E_OSS_ERROR = unchecked((int)0x80093000);
	/// <summary>
	/// OSS ASN.1 Error: Output Buffer is too small.
	/// </summary>
	public const int OSS_MORE_BUF = unchecked((int)0x80093001);
	/// <summary>
	/// OSS ASN.1 Error: Signed integer is encoded as a unsigned integer.
	/// </summary>
	public const int OSS_NEGATIVE_UINTEGER = unchecked((int)0x80093002);
	/// <summary>
	/// OSS ASN.1 Error: Unknown ASN.1 data type.
	/// </summary>
	public const int OSS_PDU_RANGE = unchecked((int)0x80093003);
	/// <summary>
	/// OSS ASN.1 Error: Output buffer is too small, the decoded data has been truncated.
	/// </summary>
	public const int OSS_MORE_INPUT = unchecked((int)0x80093004);
	/// <summary>
	/// OSS ASN.1 Error: Invalid data.
	/// </summary>
	public const int OSS_DATA_ERROR = unchecked((int)0x80093005);
	/// <summary>
	/// OSS ASN.1 Error: Invalid argument.
	/// </summary>
	public const int OSS_BAD_ARG = unchecked((int)0x80093006);
	/// <summary>
	/// OSS ASN.1 Error: Encode/Decode version mismatch.
	/// </summary>
	public const int OSS_BAD_VERSION = unchecked((int)0x80093007);
	/// <summary>
	/// OSS ASN.1 Error: Out of memory.
	/// </summary>
	public const int OSS_OUT_MEMORY = unchecked((int)0x80093008);
	/// <summary>
	/// OSS ASN.1 Error: Encode/Decode Error.
	/// </summary>
	public const int OSS_PDU_MISMATCH = unchecked((int)0x80093009);
	/// <summary>
	/// OSS ASN.1 Error: Internal Error.
	/// </summary>
	public const int OSS_LIMITED = unchecked((int)0x8009300A);
	/// <summary>
	/// OSS ASN.1 Error: Invalid data.
	/// </summary>
	public const int OSS_BAD_PTR = unchecked((int)0x8009300B);
	/// <summary>
	/// OSS ASN.1 Error: Invalid data.
	/// </summary>
	public const int OSS_BAD_TIME = unchecked((int)0x8009300C);
	/// <summary>
	/// OSS ASN.1 Error: Unsupported BER indefinite-length encoding.
	/// </summary>
	public const int OSS_INDEFINITE_NOT_SUPPORTED = unchecked((int)0x8009300D);
	/// <summary>
	/// OSS ASN.1 Error: Access violation.
	/// </summary>
	public const int OSS_MEM_ERROR = unchecked((int)0x8009300E);
	/// <summary>
	/// OSS ASN.1 Error: Invalid data.
	/// </summary>
	public const int OSS_BAD_TABLE = unchecked((int)0x8009300F);
	/// <summary>
	/// OSS ASN.1 Error: Invalid data.
	/// </summary>
	public const int OSS_TOO_LONG = unchecked((int)0x80093010);
	/// <summary>
	/// OSS ASN.1 Error: Invalid data.
	/// </summary>
	public const int OSS_CONSTRAINT_VIOLATED = unchecked((int)0x80093011);
	/// <summary>
	/// OSS ASN.1 Error: Internal Error.
	/// </summary>
	public const int OSS_FATAL_ERROR = unchecked((int)0x80093012);
	/// <summary>
	/// OSS ASN.1 Error: Multi-threading conflict.
	/// </summary>
	public const int OSS_ACCESS_SERIALIZATION_ERROR = unchecked((int)0x80093013);
	/// <summary>
	/// OSS ASN.1 Error: Invalid data.
	/// </summary>
	public const int OSS_NULL_TBL = unchecked((int)0x80093014);
	/// <summary>
	/// OSS ASN.1 Error: Invalid data.
	/// </summary>
	public const int OSS_NULL_FCN = unchecked((int)0x80093015);
	/// <summary>
	/// OSS ASN.1 Error: Invalid data.
	/// </summary>
	public const int OSS_BAD_ENCRULES = unchecked((int)0x80093016);
	/// <summary>
	/// OSS ASN.1 Error: Encode/Decode function not implemented.
	/// </summary>
	public const int OSS_UNAVAIL_ENCRULES = unchecked((int)0x80093017);
	/// <summary>
	/// OSS ASN.1 Error: Trace file error.
	/// </summary>
	public const int OSS_CANT_OPEN_TRACE_WINDOW = unchecked((int)0x80093018);
	/// <summary>
	/// OSS ASN.1 Error: Function not implemented.
	/// </summary>
	public const int OSS_UNIMPLEMENTED = unchecked((int)0x80093019);
	/// <summary>
	/// OSS ASN.1 Error: Program link error.
	/// </summary>
	public const int OSS_OID_DLL_NOT_LINKED = unchecked((int)0x8009301A);
	/// <summary>
	/// OSS ASN.1 Error: Trace file error.
	/// </summary>
	public const int OSS_CANT_OPEN_TRACE_FILE = unchecked((int)0x8009301B);
	/// <summary>
	/// OSS ASN.1 Error: Trace file error.
	/// </summary>
	public const int OSS_TRACE_FILE_ALREADY_OPEN = unchecked((int)0x8009301C);
	/// <summary>
	/// OSS ASN.1 Error: Invalid data.
	/// </summary>
	public const int OSS_TABLE_MISMATCH = unchecked((int)0x8009301D);
	/// <summary>
	/// OSS ASN.1 Error: Invalid data.
	/// </summary>
	public const int OSS_TYPE_NOT_SUPPORTED = unchecked((int)0x8009301E);
	/// <summary>
	/// OSS ASN.1 Error: Program link error.
	/// </summary>
	public const int OSS_REAL_DLL_NOT_LINKED = unchecked((int)0x8009301F);
	/// <summary>
	/// OSS ASN.1 Error: Program link error.
	/// </summary>
	public const int OSS_REAL_CODE_NOT_LINKED = unchecked((int)0x80093020);
	/// <summary>
	/// OSS ASN.1 Error: Program link error.
	/// </summary>
	public const int OSS_OUT_OF_RANGE = unchecked((int)0x80093021);
	/// <summary>
	/// OSS ASN.1 Error: Program link error.
	/// </summary>
	public const int OSS_COPIER_DLL_NOT_LINKED = unchecked((int)0x80093022);
	/// <summary>
	/// OSS ASN.1 Error: Program link error.
	/// </summary>
	public const int OSS_CONSTRAINT_DLL_NOT_LINKED = unchecked((int)0x80093023);
	/// <summary>
	/// OSS ASN.1 Error: Program link error.
	/// </summary>
	public const int OSS_COMPARATOR_DLL_NOT_LINKED = unchecked((int)0x80093024);
	/// <summary>
	/// OSS ASN.1 Error: Program link error.
	/// </summary>
	public const int OSS_COMPARATOR_CODE_NOT_LINKED = unchecked((int)0x80093025);
	/// <summary>
	/// OSS ASN.1 Error: Program link error.
	/// </summary>
	public const int OSS_MEM_MGR_DLL_NOT_LINKED = unchecked((int)0x80093026);
	/// <summary>
	/// OSS ASN.1 Error: Program link error.
	/// </summary>
	public const int OSS_PDV_DLL_NOT_LINKED = unchecked((int)0x80093027);
	/// <summary>
	/// OSS ASN.1 Error: Program link error.
	/// </summary>
	public const int OSS_PDV_CODE_NOT_LINKED = unchecked((int)0x80093028);
	/// <summary>
	/// OSS ASN.1 Error: Program link error.
	/// </summary>
	public const int OSS_API_DLL_NOT_LINKED = unchecked((int)0x80093029);
	/// <summary>
	/// OSS ASN.1 Error: Program link error.
	/// </summary>
	public const int OSS_BERDER_DLL_NOT_LINKED = unchecked((int)0x8009302A);
	/// <summary>
	/// OSS ASN.1 Error: Program link error.
	/// </summary>
	public const int OSS_PER_DLL_NOT_LINKED = unchecked((int)0x8009302B);
	/// <summary>
	/// OSS ASN.1 Error: Program link error.
	/// </summary>
	public const int OSS_OPEN_TYPE_ERROR = unchecked((int)0x8009302C);
	/// <summary>
	/// OSS ASN.1 Error: System resource error.
	/// </summary>
	public const int OSS_MUTEX_NOT_CREATED = unchecked((int)0x8009302D);
	/// <summary>
	/// OSS ASN.1 Error: Trace file error.
	/// </summary>
	public const int OSS_CANT_CLOSE_TRACE_FILE = unchecked((int)0x8009302E);
	/// <summary>
	/// ASN1 Certificate encode/decode error code base. The ASN1 error values are offset by CRYPT_E_ASN1_ERROR.
	/// </summary>
	public const int CRYPT_E_ASN1_ERROR = unchecked((int)0x80093100);
	/// <summary>
	/// ASN1 internal encode or decode error.
	/// </summary>
	public const int CRYPT_E_ASN1_INTERNAL = unchecked((int)0x80093101);
	/// <summary>
	/// ASN1 unexpected end of data.
	/// </summary>
	public const int CRYPT_E_ASN1_EOD = unchecked((int)0x80093102);
	/// <summary>
	/// ASN1 corrupted data.
	/// </summary>
	public const int CRYPT_E_ASN1_CORRUPT = unchecked((int)0x80093103);
	/// <summary>
	/// ASN1 value too large.
	/// </summary>
	public const int CRYPT_E_ASN1_LARGE = unchecked((int)0x80093104);
	/// <summary>
	/// ASN1 constraint violated.
	/// </summary>
	public const int CRYPT_E_ASN1_CONSTRAINT = unchecked((int)0x80093105);
	/// <summary>
	/// ASN1 out of memory.
	/// </summary>
	public const int CRYPT_E_ASN1_MEMORY = unchecked((int)0x80093106);
	/// <summary>
	/// ASN1 buffer overflow.
	/// </summary>
	public const int CRYPT_E_ASN1_OVERFLOW = unchecked((int)0x80093107);
	/// <summary>
	/// ASN1 function not supported for this PDU.
	/// </summary>
	public const int CRYPT_E_ASN1_BADPDU = unchecked((int)0x80093108);
	/// <summary>
	/// ASN1 bad arguments to function call.
	/// </summary>
	public const int CRYPT_E_ASN1_BADARGS = unchecked((int)0x80093109);
	/// <summary>
	/// ASN1 bad real value.
	/// </summary>
	public const int CRYPT_E_ASN1_BADREAL = unchecked((int)0x8009310A);
	/// <summary>
	/// ASN1 bad tag value met.
	/// </summary>
	public const int CRYPT_E_ASN1_BADTAG = unchecked((int)0x8009310B);
	/// <summary>
	/// ASN1 bad choice value.
	/// </summary>
	public const int CRYPT_E_ASN1_CHOICE = unchecked((int)0x8009310C);
	/// <summary>
	/// ASN1 bad encoding rule.
	/// </summary>
	public const int CRYPT_E_ASN1_RULE = unchecked((int)0x8009310D);
	/// <summary>
	/// ASN1 bad unicode (UTF8).
	/// </summary>
	public const int CRYPT_E_ASN1_UTF8 = unchecked((int)0x8009310E);
	/// <summary>
	/// ASN1 bad PDU type.
	/// </summary>
	public const int CRYPT_E_ASN1_PDU_TYPE = unchecked((int)0x80093133);
	/// <summary>
	/// ASN1 not yet implemented.
	/// </summary>
	public const int CRYPT_E_ASN1_NYI = unchecked((int)0x80093134);
	/// <summary>
	/// ASN1 skipped unknown extension(s).
	/// </summary>
	public const int CRYPT_E_ASN1_EXTENDED = unchecked((int)0x80093201);
	/// <summary>
	/// ASN1 end of data expected
	/// </summary>
	public const int CRYPT_E_ASN1_NOEOD = unchecked((int)0x80093202);
	/// <summary>
	/// The request subject name is invalid or too long.
	/// </summary>
	public const int CERTSRV_E_BAD_REQUESTSUBJECT = unchecked((int)0x80094001);
	/// <summary>
	/// The request does not exist.
	/// </summary>
	public const int CERTSRV_E_NO_REQUEST = unchecked((int)0x80094002);
	/// <summary>
	/// The request's current status does not allow this operation.
	/// </summary>
	public const int CERTSRV_E_BAD_REQUESTSTATUS = unchecked((int)0x80094003);
	/// <summary>
	/// The requested property value is empty.
	/// </summary>
	public const int CERTSRV_E_PROPERTY_EMPTY = unchecked((int)0x80094004);
	/// <summary>
	/// The certification authority's certificate contains invalid data.
	/// </summary>
	public const int CERTSRV_E_INVALID_CA_CERTIFICATE = unchecked((int)0x80094005);
	/// <summary>
	/// Certificate service has been suspended for a database restore operation.
	/// </summary>
	public const int CERTSRV_E_SERVER_SUSPENDED = unchecked((int)0x80094006);
	/// <summary>
	/// The certificate contains an encoded length that is potentially incompatible with older enrollment software.
	/// </summary>
	public const int CERTSRV_E_ENCODING_LENGTH = unchecked((int)0x80094007);
	/// <summary>
	/// The operation is denied. The user has multiple roles assigned and the certification authority is configured to enforce role separation.
	/// </summary>
	public const int CERTSRV_E_ROLECONFLICT = unchecked((int)0x80094008);
	/// <summary>
	/// The operation is denied. It can only be performed by a certificate manager that is allowed to manage certificates for the current requester.
	/// </summary>
	public const int CERTSRV_E_RESTRICTEDOFFICER = unchecked((int)0x80094009);
	/// <summary>
	/// Cannot archive private key. The certification authority is not configured for key archival.
	/// </summary>
	public const int CERTSRV_E_KEY_ARCHIVAL_NOT_CONFIGURED = unchecked((int)0x8009400A);
	/// <summary>
	/// Cannot archive private key. The certification authority could not verify one or more key recovery certificates.
	/// </summary>
	public const int CERTSRV_E_NO_VALID_KRA = unchecked((int)0x8009400B);
	/// <summary>
	/// The request is incorrectly formatted. The encrypted private key must be in an unauthenticated attribute in an outermost signature.
	/// </summary>
	public const int CERTSRV_E_BAD_REQUEST_KEY_ARCHIVAL = unchecked((int)0x8009400C);
	/// <summary>
	/// At least one security principal must have the permission to manage this CA.
	/// </summary>
	public const int CERTSRV_E_NO_CAADMIN_DEFINED = unchecked((int)0x8009400D);
	/// <summary>
	/// The request contains an invalid renewal certificate attribute.
	/// </summary>
	public const int CERTSRV_E_BAD_RENEWAL_CERT_ATTRIBUTE = unchecked((int)0x8009400E);
	/// <summary>
	/// An attempt was made to open a Certification Authority database session, but there are already too many active sessions. The server may need to be configured to allow additional sessions.
	/// </summary>
	public const int CERTSRV_E_NO_DB_SESSIONS = unchecked((int)0x8009400F);
	/// <summary>
	/// A memory reference caused a data alignment fault.
	/// </summary>
	public const int CERTSRV_E_ALIGNMENT_FAULT = unchecked((int)0x80094010);
	/// <summary>
	/// The permissions on this certification authority do not allow the current user to enroll for certificates.
	/// </summary>
	public const int CERTSRV_E_ENROLL_DENIED = unchecked((int)0x80094011);
	/// <summary>
	/// The permissions on the certificate template do not allow the current user to enroll for this type of certificate.
	/// </summary>
	public const int CERTSRV_E_TEMPLATE_DENIED = unchecked((int)0x80094012);
	/// <summary>
	/// The contacted domain controller cannot support signed LDAP traffic. Update the domain controller or configure Certificate Services to use SSL for Active Directory access.
	/// </summary>
	public const int CERTSRV_E_DOWNLEVEL_DC_SSL_OR_UPGRADE = unchecked((int)0x80094013);
	/// <summary>
	/// The request was denied by a certificate manager or CA administrator.
	/// </summary>
	public const int CERTSRV_E_ADMIN_DENIED_REQUEST = unchecked((int)0x80094014);
	/// <summary>
	/// An enrollment policy server cannot be located.
	/// </summary>
	public const int CERTSRV_E_NO_POLICY_SERVER = unchecked((int)0x80094015);
	/// <summary>
	/// The requested certificate template is not supported by this CA.
	/// </summary>
	public const int CERTSRV_E_UNSUPPORTED_CERT_TYPE = unchecked((int)0x80094800);
	/// <summary>
	/// The request contains no certificate template information.
	/// </summary>
	public const int CERTSRV_E_NO_CERT_TYPE = unchecked((int)0x80094801);
	/// <summary>
	/// The request contains conflicting template information.
	/// </summary>
	public const int CERTSRV_E_TEMPLATE_CONFLICT = unchecked((int)0x80094802);
	/// <summary>
	/// The request is missing a required Subject Alternate name extension.
	/// </summary>
	public const int CERTSRV_E_SUBJECT_ALT_NAME_REQUIRED = unchecked((int)0x80094803);
	/// <summary>
	/// The request is missing a required private key for archival by the server.
	/// </summary>
	public const int CERTSRV_E_ARCHIVED_KEY_REQUIRED = unchecked((int)0x80094804);
	/// <summary>
	/// The request is missing a required SMIME capabilities extension.
	/// </summary>
	public const int CERTSRV_E_SMIME_REQUIRED = unchecked((int)0x80094805);
	/// <summary>
	/// The request was made on behalf of a subject other than the caller. The certificate template must be configured to require at least one signature to authorize the request.
	/// </summary>
	public const int CERTSRV_E_BAD_RENEWAL_SUBJECT = unchecked((int)0x80094806);
	/// <summary>
	/// The request template version is newer than the supported template version.
	/// </summary>
	public const int CERTSRV_E_BAD_TEMPLATE_VERSION = unchecked((int)0x80094807);
	/// <summary>
	/// The template is missing a required signature policy attribute.
	/// </summary>
	public const int CERTSRV_E_TEMPLATE_POLICY_REQUIRED = unchecked((int)0x80094808);
	/// <summary>
	/// The request is missing required signature policy information.
	/// </summary>
	public const int CERTSRV_E_SIGNATURE_POLICY_REQUIRED = unchecked((int)0x80094809);
	/// <summary>
	/// The request is missing one or more required signatures.
	/// </summary>
	public const int CERTSRV_E_SIGNATURE_COUNT = unchecked((int)0x8009480A);
	/// <summary>
	/// One or more signatures did not include the required application or issuance policies. The request is missing one or more required valid signatures.
	/// </summary>
	public const int CERTSRV_E_SIGNATURE_REJECTED = unchecked((int)0x8009480B);
	/// <summary>
	/// The request is missing one or more required signature issuance policies.
	/// </summary>
	public const int CERTSRV_E_ISSUANCE_POLICY_REQUIRED = unchecked((int)0x8009480C);
	/// <summary>
	/// The UPN is unavailable and cannot be added to the Subject Alternate name.
	/// </summary>
	public const int CERTSRV_E_SUBJECT_UPN_REQUIRED = unchecked((int)0x8009480D);
	/// <summary>
	/// The Active Directory GUID is unavailable and cannot be added to the Subject Alternate name.
	/// </summary>
	public const int CERTSRV_E_SUBJECT_DIRECTORY_GUID_REQUIRED = unchecked((int)0x8009480E);
	/// <summary>
	/// The DNS name is unavailable and cannot be added to the Subject Alternate name.
	/// </summary>
	public const int CERTSRV_E_SUBJECT_DNS_REQUIRED = unchecked((int)0x8009480F);
	/// <summary>
	/// The request includes a private key for archival by the server, but key archival is not enabled for the specified certificate template.
	/// </summary>
	public const int CERTSRV_E_ARCHIVED_KEY_UNEXPECTED = unchecked((int)0x80094810);
	/// <summary>
	/// The public key does not meet the minimum size required by the specified certificate template.
	/// </summary>
	public const int CERTSRV_E_KEY_LENGTH = unchecked((int)0x80094811);
	/// <summary>
	/// The EMail name is unavailable and cannot be added to the Subject or Subject Alternate name.
	/// </summary>
	public const int CERTSRV_E_SUBJECT_EMAIL_REQUIRED = unchecked((int)0x80094812);
	/// <summary>
	/// One or more certificate templates to be enabled on this certification authority could not be found.
	/// </summary>
	public const int CERTSRV_E_UNKNOWN_CERT_TYPE = unchecked((int)0x80094813);
	/// <summary>
	/// The certificate template renewal period is longer than the certificate validity period. The template should be reconfigured or the CA certificate renewed.
	/// </summary>
	public const int CERTSRV_E_CERT_TYPE_OVERLAP = unchecked((int)0x80094814);
	/// <summary>
	/// The certificate template requires too many RA signatures. Only one RA signature is allowed.
	/// </summary>
	public const int CERTSRV_E_TOO_MANY_SIGNATURES = unchecked((int)0x80094815);
	/// <summary>
	/// The certificate template requires renewal with the same public key, but the request uses a different public key.
	/// </summary>
	public const int CERTSRV_E_RENEWAL_BAD_PUBLIC_KEY = unchecked((int)0x80094816);
	/// <summary>
	/// The key is not exportable.
	/// </summary>
	public const int XENROLL_E_KEY_NOT_EXPORTABLE = unchecked((int)0x80095000);
	/// <summary>
	/// You cannot add the root CA certificate into your local store.
	/// </summary>
	public const int XENROLL_E_CANNOT_ADD_ROOT_CERT = unchecked((int)0x80095001);
	/// <summary>
	/// The key archival hash attribute was not found in the response.
	/// </summary>
	public const int XENROLL_E_RESPONSE_KA_HASH_NOT_FOUND = unchecked((int)0x80095002);
	/// <summary>
	/// An unexpected key archival hash attribute was found in the response.
	/// </summary>
	public const int XENROLL_E_RESPONSE_UNEXPECTED_KA_HASH = unchecked((int)0x80095003);
	/// <summary>
	/// There is a key archival hash mismatch between the request and the response.
	/// </summary>
	public const int XENROLL_E_RESPONSE_KA_HASH_MISMATCH = unchecked((int)0x80095004);
	/// <summary>
	/// Signing certificate cannot include SMIME extension.
	/// </summary>
	public const int XENROLL_E_KEYSPEC_SMIME_MISMATCH = unchecked((int)0x80095005);
	/// <summary>
	/// A system-level error occurred while verifying trust.
	/// </summary>
	public const int TRUST_E_SYSTEM_ERROR = unchecked((int)0x80096001);
	/// <summary>
	/// The certificate for the signer of the message is invalid or not found.
	/// </summary>
	public const int TRUST_E_NO_SIGNER_CERT = unchecked((int)0x80096002);
	/// <summary>
	/// One of the counter signatures was invalid.
	/// </summary>
	public const int TRUST_E_COUNTER_SIGNER = unchecked((int)0x80096003);
	/// <summary>
	/// The signature of the certificate cannot be verified.
	/// </summary>
	public const int TRUST_E_CERT_SIGNATURE = unchecked((int)0x80096004);
	/// <summary>
	/// The timestamp signature and/or certificate could not be verified or is malformed.
	/// </summary>
	public const int TRUST_E_TIME_STAMP = unchecked((int)0x80096005);
	/// <summary>
	/// The digital signature of the object did not verify.
	/// </summary>
	public const int TRUST_E_BAD_DIGEST = unchecked((int)0x80096010);
	/// <summary>
	/// A certificate's basic constraint extension has not been observed.
	/// </summary>
	public const int TRUST_E_BASIC_CONSTRAINTS = unchecked((int)0x80096019);
	/// <summary>
	/// The certificate does not meet or contain the Authenticode(tm) financial extensions.
	/// </summary>
	public const int TRUST_E_FINANCIAL_CRITERIA = unchecked((int)0x8009601E);
	/// <summary>
	/// Tried to reference a part of the file outside the proper range.
	/// </summary>
	public const int MSSIPOTF_E_OUTOFMEMRANGE = unchecked((int)0x80097001);
	/// <summary>
	/// Could not retrieve an object from the file.
	/// </summary>
	public const int MSSIPOTF_E_CANTGETOBJECT = unchecked((int)0x80097002);
	/// <summary>
	/// Could not find the head table in the file.
	/// </summary>
	public const int MSSIPOTF_E_NOHEADTABLE = unchecked((int)0x80097003);
	/// <summary>
	/// The magic number in the head table is incorrect.
	/// </summary>
	public const int MSSIPOTF_E_BAD_MAGICNUMBER = unchecked((int)0x80097004);
	/// <summary>
	/// The offset table has incorrect values.
	/// </summary>
	public const int MSSIPOTF_E_BAD_OFFSET_TABLE = unchecked((int)0x80097005);
	/// <summary>
	/// Duplicate table tags or tags out of alphabetical order.
	/// </summary>
	public const int MSSIPOTF_E_TABLE_TAGORDER = unchecked((int)0x80097006);
	/// <summary>
	/// A table does not start on a long word boundary.
	/// </summary>
	public const int MSSIPOTF_E_TABLE_LONGWORD = unchecked((int)0x80097007);
	/// <summary>
	/// First table does not appear after header information.
	/// </summary>
	public const int MSSIPOTF_E_BAD_FIRST_TABLE_PLACEMENT = unchecked((int)0x80097008);
	/// <summary>
	/// Two or more tables overlap.
	/// </summary>
	public const int MSSIPOTF_E_TABLES_OVERLAP = unchecked((int)0x80097009);
	/// <summary>
	/// Too many pad bytes between tables or pad bytes are not 0.
	/// </summary>
	public const int MSSIPOTF_E_TABLE_PADBYTES = unchecked((int)0x8009700A);
	/// <summary>
	/// File is too small to contain the last table.
	/// </summary>
	public const int MSSIPOTF_E_FILETOOSMALL = unchecked((int)0x8009700B);
	/// <summary>
	/// A table checksum is incorrect.
	/// </summary>
	public const int MSSIPOTF_E_TABLE_CHECKSUM = unchecked((int)0x8009700C);
	/// <summary>
	/// The file checksum is incorrect.
	/// </summary>
	public const int MSSIPOTF_E_FILE_CHECKSUM = unchecked((int)0x8009700D);
	/// <summary>
	/// The signature does not have the correct attributes for the policy.
	/// </summary>
	public const int MSSIPOTF_E_FAILED_POLICY = unchecked((int)0x80097010);
	/// <summary>
	/// The file did not pass the hints check.
	/// </summary>
	public const int MSSIPOTF_E_FAILED_HINTS_CHECK = unchecked((int)0x80097011);
	/// <summary>
	/// The file is not an OpenType file.
	/// </summary>
	public const int MSSIPOTF_E_NOT_OPENTYPE = unchecked((int)0x80097012);
	/// <summary>
	/// Failed on a file operation (open, map, read, write).
	/// </summary>
	public const int MSSIPOTF_E_FILE = unchecked((int)0x80097013);
	/// <summary>
	/// A call to a CryptoAPI function failed.
	/// </summary>
	public const int MSSIPOTF_E_CRYPT = unchecked((int)0x80097014);
	/// <summary>
	/// There is a bad version number in the file.
	/// </summary>
	public const int MSSIPOTF_E_BADVERSION = unchecked((int)0x80097015);
	/// <summary>
	/// The structure of the DSIG table is incorrect.
	/// </summary>
	public const int MSSIPOTF_E_DSIG_STRUCTURE = unchecked((int)0x80097016);
	/// <summary>
	/// A check failed in a partially constant table.
	/// </summary>
	public const int MSSIPOTF_E_PCONST_CHECK = unchecked((int)0x80097017);
	/// <summary>
	/// Some kind of structural error.
	/// </summary>
	public const int MSSIPOTF_E_STRUCTURE = unchecked((int)0x80097018);
	/// <summary>
	/// The requested credential requires confirmation.
	/// </summary>
	public const int ERROR_CRED_REQUIRES_CONFIRMATION = unchecked((int)0x80097019);
	/// <summary>
	/// Unknown trust provider.
	/// </summary>
	public const int TRUST_E_PROVIDER_UNKNOWN = unchecked((int)0x800B0001);
	/// <summary>
	/// The trust verification action specified is not supported by the specified trust provider.
	/// </summary>
	public const int TRUST_E_ACTION_UNKNOWN = unchecked((int)0x800B0002);
	/// <summary>
	/// The form specified for the subject is not one supported or known by the specified trust provider.
	/// </summary>
	public const int TRUST_E_SUBJECT_FORM_UNKNOWN = unchecked((int)0x800B0003);
	/// <summary>
	/// The subject is not trusted for the specified action.
	/// </summary>
	public const int TRUST_E_SUBJECT_NOT_TRUSTED = unchecked((int)0x800B0004);
	/// <summary>
	/// Error due to problem in ASN.1 encoding process.
	/// </summary>
	public const int DIGSIG_E_ENCODE = unchecked((int)0x800B0005);
	/// <summary>
	/// Error due to problem in ASN.1 decoding process.
	/// </summary>
	public const int DIGSIG_E_DECODE = unchecked((int)0x800B0006);
	/// <summary>
	/// Reading / writing Extensions where Attributes are appropriate, and visa versa.
	/// </summary>
	public const int DIGSIG_E_EXTENSIBILITY = unchecked((int)0x800B0007);
	/// <summary>
	/// Unspecified cryptographic failure.
	/// </summary>
	public const int DIGSIG_E_CRYPTO = unchecked((int)0x800B0008);
	/// <summary>
	/// The size of the data could not be determined.
	/// </summary>
	public const int PERSIST_E_SIZEDEFINITE = unchecked((int)0x800B0009);
	/// <summary>
	/// The size of the indefinite-sized data could not be determined.
	/// </summary>
	public const int PERSIST_E_SIZEINDEFINITE = unchecked((int)0x800B000A);
	/// <summary>
	/// This object does not read and write self-sizing data.
	/// </summary>
	public const int PERSIST_E_NOTSELFSIZING = unchecked((int)0x800B000B);
	/// <summary>
	/// No signature was present in the subject.
	/// </summary>
	public const int TRUST_E_NOSIGNATURE = unchecked((int)0x800B0100);
	/// <summary>
	/// A required certificate is not within its validity period when verifying against the current system clock or the timestamp in the signed file.
	/// </summary>
	public const int CERT_E_EXPIRED = unchecked((int)0x800B0101);
	/// <summary>
	/// The validity periods of the certification chain do not nest correctly.
	/// </summary>
	public const int CERT_E_VALIDITYPERIODNESTING = unchecked((int)0x800B0102);
	/// <summary>
	/// A certificate that can only be used as an end-entity is being used as a CA or visa versa.
	/// </summary>
	public const int CERT_E_ROLE = unchecked((int)0x800B0103);
	/// <summary>
	/// A path length constraint in the certification chain has been violated.
	/// </summary>
	public const int CERT_E_PATHLENCONST = unchecked((int)0x800B0104);
	/// <summary>
	/// A certificate contains an unknown extension that is marked 'critical'.
	/// </summary>
	public const int CERT_E_CRITICAL = unchecked((int)0x800B0105);
	/// <summary>
	/// A certificate being used for a purpose other than the ones specified by its CA.
	/// </summary>
	public const int CERT_E_PURPOSE = unchecked((int)0x800B0106);
	/// <summary>
	/// A parent of a given certificate in fact did not issue that child certificate.
	/// </summary>
	public const int CERT_E_ISSUERCHAINING = unchecked((int)0x800B0107);
	/// <summary>
	/// A certificate is missing or has an empty value for an important field, such as a subject or issuer name.
	/// </summary>
	public const int CERT_E_MALFORMED = unchecked((int)0x800B0108);
	/// <summary>
	/// A certificate chain processed, but terminated in a root certificate which is not trusted by the trust provider.
	/// </summary>
	public const int CERT_E_UNTRUSTEDROOT = unchecked((int)0x800B0109);
	/// <summary>
	/// A certificate chain could not be built to a trusted root authority.
	/// </summary>
	public const int CERT_E_CHAINING = unchecked((int)0x800B010A);
	/// <summary>
	/// Generic trust failure.
	/// </summary>
	public const int TRUST_E_FAIL = unchecked((int)0x800B010B);
	/// <summary>
	/// A certificate was explicitly revoked by its issuer.
	/// </summary>
	public const int CERT_E_REVOKED = unchecked((int)0x800B010C);
	/// <summary>
	/// The certification path terminates with the test root which is not trusted with the current policy settings.
	/// </summary>
	public const int CERT_E_UNTRUSTEDTESTROOT = unchecked((int)0x800B010D);
	/// <summary>
	/// The revocation process could not continue - the certificate(s) could not be checked.
	/// </summary>
	public const int CERT_E_REVOCATION_FAILURE = unchecked((int)0x800B010E);
	/// <summary>
	/// The certificate's CN name does not match the passed value.
	/// </summary>
	public const int CERT_E_CN_NO_MATCH = unchecked((int)0x800B010F);
	/// <summary>
	/// The certificate is not valid for the requested usage.
	/// </summary>
	public const int CERT_E_WRONG_USAGE = unchecked((int)0x800B0110);
	/// <summary>
	/// The certificate was explicitly marked as untrusted by the user.
	/// </summary>
	public const int TRUST_E_EXPLICIT_DISTRUST = unchecked((int)0x800B0111);
	/// <summary>
	/// A certification chain processed correctly, but one of the CA certificates is not trusted by the policy provider.
	/// </summary>
	public const int CERT_E_UNTRUSTEDCA = unchecked((int)0x800B0112);
	/// <summary>
	/// The certificate has invalid policy.
	/// </summary>
	public const int CERT_E_INVALID_POLICY = unchecked((int)0x800B0113);
	/// <summary>
	/// The certificate has an invalid name. The name is not included in the permitted list or is explicitly excluded.
	/// </summary>
	public const int CERT_E_INVALID_NAME = unchecked((int)0x800B0114);
	/// <summary>
	/// A non-empty line was encountered in the INF before the start of a section.
	/// </summary>
	public const int SPAPI_E_EXPECTED_SECTION_NAME = unchecked((int)0x800F0000);
	/// <summary>
	/// A section name marker in the INF is not complete, or does not exist on a line by itself.
	/// </summary>
	public const int SPAPI_E_BAD_SECTION_NAME_LINE = unchecked((int)0x800F0001);
	/// <summary>
	/// An INF section was encountered whose name exceeds the maximum section name length.
	/// </summary>
	public const int SPAPI_E_SECTION_NAME_TOO_LONG = unchecked((int)0x800F0002);
	/// <summary>
	/// The syntax of the INF is invalid.
	/// </summary>
	public const int SPAPI_E_GENERAL_SYNTAX = unchecked((int)0x800F0003);
	/// <summary>
	/// The style of the INF is different than what was requested.
	/// </summary>
	public const int SPAPI_E_WRONG_INF_STYLE = unchecked((int)0x800F0100);
	/// <summary>
	/// The required section was not found in the INF.
	/// </summary>
	public const int SPAPI_E_SECTION_NOT_FOUND = unchecked((int)0x800F0101);
	/// <summary>
	/// The required line was not found in the INF.
	/// </summary>
	public const int SPAPI_E_LINE_NOT_FOUND = unchecked((int)0x800F0102);
	/// <summary>
	/// The files affected by the installation of this file queue have not been backed up for uninstall.
	/// </summary>
	public const int SPAPI_E_NO_BACKUP = unchecked((int)0x800F0103);
	/// <summary>
	/// The INF or the device information set or element does not have an associated install class.
	/// </summary>
	public const int SPAPI_E_NO_ASSOCIATED_CLASS = unchecked((int)0x800F0200);
	/// <summary>
	/// The INF or the device information set or element does not match the specified install class.
	/// </summary>
	public const int SPAPI_E_CLASS_MISMATCH = unchecked((int)0x800F0201);
	/// <summary>
	/// An existing device was found that is a duplicate of the device being manually installed.
	/// </summary>
	public const int SPAPI_E_DUPLICATE_FOUND = unchecked((int)0x800F0202);
	/// <summary>
	/// There is no driver selected for the device information set or element.
	/// </summary>
	public const int SPAPI_E_NO_DRIVER_SELECTED = unchecked((int)0x800F0203);
	/// <summary>
	/// The requested device registry key does not exist.
	/// </summary>
	public const int SPAPI_E_KEY_DOES_NOT_EXIST = unchecked((int)0x800F0204);
	/// <summary>
	/// The device instance name is invalid.
	/// </summary>
	public const int SPAPI_E_INVALID_DEVINST_NAME = unchecked((int)0x800F0205);
	/// <summary>
	/// The install class is not present or is invalid.
	/// </summary>
	public const int SPAPI_E_INVALID_CLASS = unchecked((int)0x800F0206);
	/// <summary>
	/// The device instance cannot be created because it already exists.
	/// </summary>
	public const int SPAPI_E_DEVINST_ALREADY_EXISTS = unchecked((int)0x800F0207);
	/// <summary>
	/// The operation cannot be performed on a device information element that has not been registered.
	/// </summary>
	public const int SPAPI_E_DEVINFO_NOT_REGISTERED = unchecked((int)0x800F0208);
	/// <summary>
	/// The device property code is invalid.
	/// </summary>
	public const int SPAPI_E_INVALID_REG_PROPERTY = unchecked((int)0x800F0209);
	/// <summary>
	/// The INF from which a driver list is to be built does not exist.
	/// </summary>
	public const int SPAPI_E_NO_INF = unchecked((int)0x800F020A);
	/// <summary>
	/// The device instance does not exist in the hardware tree.
	/// </summary>
	public const int SPAPI_E_NO_SUCH_DEVINST = unchecked((int)0x800F020B);
	/// <summary>
	/// The icon representing this install class cannot be loaded.
	/// </summary>
	public const int SPAPI_E_CANT_LOAD_CLASS_ICON = unchecked((int)0x800F020C);
	/// <summary>
	/// The class installer registry entry is invalid.
	/// </summary>
	public const int SPAPI_E_INVALID_CLASS_INSTALLER = unchecked((int)0x800F020D);
	/// <summary>
	/// The class installer has indicated that the default action should be performed for this installation request.
	/// </summary>
	public const int SPAPI_E_DI_DO_DEFAULT = unchecked((int)0x800F020E);
	/// <summary>
	/// The operation does not require any files to be copied.
	/// </summary>
	public const int SPAPI_E_DI_NOFILECOPY = unchecked((int)0x800F020F);
	/// <summary>
	/// The specified hardware profile does not exist.
	/// </summary>
	public const int SPAPI_E_INVALID_HWPROFILE = unchecked((int)0x800F0210);
	/// <summary>
	/// There is no device information element currently selected for this device information set.
	/// </summary>
	public const int SPAPI_E_NO_DEVICE_SELECTED = unchecked((int)0x800F0211);
	/// <summary>
	/// The operation cannot be performed because the device information set is locked.
	/// </summary>
	public const int SPAPI_E_DEVINFO_LIST_LOCKED = unchecked((int)0x800F0212);
	/// <summary>
	/// The operation cannot be performed because the device information element is locked.
	/// </summary>
	public const int SPAPI_E_DEVINFO_DATA_LOCKED = unchecked((int)0x800F0213);
	/// <summary>
	/// The specified path does not contain any applicable device INFs.
	/// </summary>
	public const int SPAPI_E_DI_BAD_PATH = unchecked((int)0x800F0214);
	/// <summary>
	/// No class installer parameters have been set for the device information set or element.
	/// </summary>
	public const int SPAPI_E_NO_CLASSINSTALL_PARAMS = unchecked((int)0x800F0215);
	/// <summary>
	/// The operation cannot be performed because the file queue is locked.
	/// </summary>
	public const int SPAPI_E_FILEQUEUE_LOCKED = unchecked((int)0x800F0216);
	/// <summary>
	/// A service installation section in this INF is invalid.
	/// </summary>
	public const int SPAPI_E_BAD_SERVICE_INSTALLSECT = unchecked((int)0x800F0217);
	/// <summary>
	/// There is no class driver list for the device information element.
	/// </summary>
	public const int SPAPI_E_NO_CLASS_DRIVER_LIST = unchecked((int)0x800F0218);
	/// <summary>
	/// The installation failed because a function driver was not specified for this device instance.
	/// </summary>
	public const int SPAPI_E_NO_ASSOCIATED_SERVICE = unchecked((int)0x800F0219);
	/// <summary>
	/// There is presently no default device interface designated for this interface class.
	/// </summary>
	public const int SPAPI_E_NO_DEFAULT_DEVICE_INTERFACE = unchecked((int)0x800F021A);
	/// <summary>
	/// The operation cannot be performed because the device interface is currently active.
	/// </summary>
	public const int SPAPI_E_DEVICE_INTERFACE_ACTIVE = unchecked((int)0x800F021B);
	/// <summary>
	/// The operation cannot be performed because the device interface has been removed from the system.
	/// </summary>
	public const int SPAPI_E_DEVICE_INTERFACE_REMOVED = unchecked((int)0x800F021C);
	/// <summary>
	/// An interface installation section in this INF is invalid.
	/// </summary>
	public const int SPAPI_E_BAD_INTERFACE_INSTALLSECT = unchecked((int)0x800F021D);
	/// <summary>
	/// This interface class does not exist in the system.
	/// </summary>
	public const int SPAPI_E_NO_SUCH_INTERFACE_CLASS = unchecked((int)0x800F021E);
	/// <summary>
	/// The reference string supplied for this interface device is invalid.
	/// </summary>
	public const int SPAPI_E_INVALID_REFERENCE_STRING = unchecked((int)0x800F021F);
	/// <summary>
	/// The specified machine name does not conform to UNC naming conventions.
	/// </summary>
	public const int SPAPI_E_INVALID_MACHINENAME = unchecked((int)0x800F0220);
	/// <summary>
	/// A general remote communication error occurred.
	/// </summary>
	public const int SPAPI_E_REMOTE_COMM_FAILURE = unchecked((int)0x800F0221);
	/// <summary>
	/// The machine selected for remote communication is not available at this time.
	/// </summary>
	public const int SPAPI_E_MACHINE_UNAVAILABLE = unchecked((int)0x800F0222);
	/// <summary>
	/// The Plug and Play service is not available on the remote machine.
	/// </summary>
	public const int SPAPI_E_NO_CONFIGMGR_SERVICES = unchecked((int)0x800F0223);
	/// <summary>
	/// The property page provider registry entry is invalid.
	/// </summary>
	public const int SPAPI_E_INVALID_PROPPAGE_PROVIDER = unchecked((int)0x800F0224);
	/// <summary>
	/// The requested device interface is not present in the system.
	/// </summary>
	public const int SPAPI_E_NO_SUCH_DEVICE_INTERFACE = unchecked((int)0x800F0225);
	/// <summary>
	/// The device's co-installer has additional work to perform after installation is complete.
	/// </summary>
	public const int SPAPI_E_DI_POSTPROCESSING_REQUIRED = unchecked((int)0x800F0226);
	/// <summary>
	/// The device's co-installer is invalid.
	/// </summary>
	public const int SPAPI_E_INVALID_COINSTALLER = unchecked((int)0x800F0227);
	/// <summary>
	/// There are no compatible drivers for this device.
	/// </summary>
	public const int SPAPI_E_NO_COMPAT_DRIVERS = unchecked((int)0x800F0228);
	/// <summary>
	/// There is no icon that represents this device or device type.
	/// </summary>
	public const int SPAPI_E_NO_DEVICE_ICON = unchecked((int)0x800F0229);
	/// <summary>
	/// A logical configuration specified in this INF is invalid.
	/// </summary>
	public const int SPAPI_E_INVALID_INF_LOGCONFIG = unchecked((int)0x800F022A);
	/// <summary>
	/// The class installer has denied the request to install or upgrade this device.
	/// </summary>
	public const int SPAPI_E_DI_DONT_INSTALL = unchecked((int)0x800F022B);
	/// <summary>
	/// One of the filter drivers installed for this device is invalid.
	/// </summary>
	public const int SPAPI_E_INVALID_FILTER_DRIVER = unchecked((int)0x800F022C);
	/// <summary>
	/// The driver selected for this device does not support this version of Windows.
	/// </summary>
	public const int SPAPI_E_NON_WINDOWS_NT_DRIVER = unchecked((int)0x800F022D);
	/// <summary>
	/// The driver selected for this device does not support Windows.
	/// </summary>
	public const int SPAPI_E_NON_WINDOWS_DRIVER = unchecked((int)0x800F022E);
	/// <summary>
	/// The third-party INF does not contain digital signature information.
	/// </summary>
	public const int SPAPI_E_NO_CATALOG_FOR_OEM_INF = unchecked((int)0x800F022F);
	/// <summary>
	/// An invalid attempt was made to use a device installation file queue for verification of digital signatures relative to other platforms.
	/// </summary>
	public const int SPAPI_E_DEVINSTALL_QUEUE_NONNATIVE = unchecked((int)0x800F0230);
	/// <summary>
	/// The device cannot be disabled.
	/// </summary>
	public const int SPAPI_E_NOT_DISABLEABLE = unchecked((int)0x800F0231);
	/// <summary>
	/// The device could not be dynamically removed.
	/// </summary>
	public const int SPAPI_E_CANT_REMOVE_DEVINST = unchecked((int)0x800F0232);
	/// <summary>
	/// Cannot copy to specified target.
	/// </summary>
	public const int SPAPI_E_INVALID_TARGET = unchecked((int)0x800F0233);
	/// <summary>
	/// Driver is not intended for this platform.
	/// </summary>
	public const int SPAPI_E_DRIVER_NONNATIVE = unchecked((int)0x800F0234);
	/// <summary>
	/// Operation not allowed in WOW64.
	/// </summary>
	public const int SPAPI_E_IN_WOW64 = unchecked((int)0x800F0235);
	/// <summary>
	/// The operation involving unsigned file copying was rolled back, so that a system restore point could be set.
	/// </summary>
	public const int SPAPI_E_SET_SYSTEM_RESTORE_POINT = unchecked((int)0x800F0236);
	/// <summary>
	/// An INF was copied into the Windows INF directory in an improper manner.
	/// </summary>
	public const int SPAPI_E_INCORRECTLY_COPIED_INF = unchecked((int)0x800F0237);
	/// <summary>
	/// The Security Configuration Editor (SCE) APIs have been disabled on this Embedded product.
	/// </summary>
	public const int SPAPI_E_SCE_DISABLED = unchecked((int)0x800F0238);
	/// <summary>
	/// An unknown exception was encountered.
	/// </summary>
	public const int SPAPI_E_UNKNOWN_EXCEPTION = unchecked((int)0x800F0239);
	/// <summary>
	/// A problem was encountered when accessing the Plug and Play registry database.
	/// </summary>
	public const int SPAPI_E_PNP_REGISTRY_ERROR = unchecked((int)0x800F023A);
	/// <summary>
	/// The requested operation is not supported for a remote machine.
	/// </summary>
	public const int SPAPI_E_REMOTE_REQUEST_UNSUPPORTED = unchecked((int)0x800F023B);
	/// <summary>
	/// The specified file is not an installed OEM INF.
	/// </summary>
	public const int SPAPI_E_NOT_AN_INSTALLED_OEM_INF = unchecked((int)0x800F023C);
	/// <summary>
	/// One or more devices are presently installed using the specified INF.
	/// </summary>
	public const int SPAPI_E_INF_IN_USE_BY_DEVICES = unchecked((int)0x800F023D);
	/// <summary>
	/// The requested device install operation is obsolete.
	/// </summary>
	public const int SPAPI_E_DI_FUNCTION_OBSOLETE = unchecked((int)0x800F023E);
	/// <summary>
	/// A file could not be verified because it does not have an associated catalog signed via Authenticode(tm).
	/// </summary>
	public const int SPAPI_E_NO_AUTHENTICODE_CATALOG = unchecked((int)0x800F023F);
	/// <summary>
	/// Authenticode(tm) signature verification is not supported for the specified INF.
	/// </summary>
	public const int SPAPI_E_AUTHENTICODE_DISALLOWED = unchecked((int)0x800F0240);
	/// <summary>
	/// The INF was signed with an Authenticode(tm) catalog from a trusted publisher.
	/// </summary>
	public const int SPAPI_E_AUTHENTICODE_TRUSTED_PUBLISHER = unchecked((int)0x800F0241);
	/// <summary>
	/// The publisher of an Authenticode(tm) signed catalog has not yet been established as trusted.
	/// </summary>
	public const int SPAPI_E_AUTHENTICODE_TRUST_NOT_ESTABLISHED = unchecked((int)0x800F0242);
	/// <summary>
	/// The publisher of an Authenticode(tm) signed catalog was not established as trusted.
	/// </summary>
	public const int SPAPI_E_AUTHENTICODE_PUBLISHER_NOT_TRUSTED = unchecked((int)0x800F0243);
	/// <summary>
	/// The software was tested for compliance with Windows Logo requirements on a different version of Windows, and may not be compatible with this version.
	/// </summary>
	public const int SPAPI_E_SIGNATURE_OSATTRIBUTE_MISMATCH = unchecked((int)0x800F0244);
	/// <summary>
	/// The file may only be validated by a catalog signed via Authenticode(tm).
	/// </summary>
	public const int SPAPI_E_ONLY_VALIDATE_VIA_AUTHENTICODE = unchecked((int)0x800F0245);
	/// <summary>
	/// One of the installers for this device cannot perform the installation at this time.
	/// </summary>
	public const int SPAPI_E_DEVICE_INSTALLER_NOT_READY = unchecked((int)0x800F0246);
	/// <summary>
	/// A problem was encountered while attempting to add the driver to the store.
	/// </summary>
	public const int SPAPI_E_DRIVER_STORE_ADD_FAILED = unchecked((int)0x800F0247);
	/// <summary>
	/// The installation of this device is forbidden by system policy. Contact your system administrator.
	/// </summary>
	public const int SPAPI_E_DEVICE_INSTALL_BLOCKED = unchecked((int)0x800F0248);
	/// <summary>
	/// The installation of this driver is forbidden by system policy. Contact your system administrator.
	/// </summary>
	public const int SPAPI_E_DRIVER_INSTALL_BLOCKED = unchecked((int)0x800F0249);
	/// <summary>
	/// The specified INF is the wrong type for this operation.
	/// </summary>
	public const int SPAPI_E_WRONG_INF_TYPE = unchecked((int)0x800F024A);
	/// <summary>
	/// The hash for the file is not present in the specified catalog file. The file is likely corrupt or the victim of tampering.
	/// </summary>
	public const int SPAPI_E_FILE_HASH_NOT_IN_CATALOG = unchecked((int)0x800F024B);
	/// <summary>
	/// A problem was encountered while attempting to delete the driver from the store.
	/// </summary>
	public const int SPAPI_E_DRIVER_STORE_DELETE_FAILED = unchecked((int)0x800F024C);
	/// <summary>
	/// An unrecoverable stack overflow was encountered.
	/// </summary>
	public const int SPAPI_E_UNRECOVERABLE_STACK_OVERFLOW = unchecked((int)0x800F0300);
	/// <summary>
	/// No installed components were detected.
	/// </summary>
	public const int SPAPI_E_ERROR_NOT_INSTALLED = unchecked((int)0x800F1000);
	/// <summary>
	/// An internal consistency check failed.
	/// </summary>
	public const int SCARD_F_INTERNAL_ERROR = unchecked((int)0x80100001);
	/// <summary>
	/// The action was canceled by an SCardCancel request.
	/// </summary>
	public const int SCARD_E_CANCELLED = unchecked((int)0x80100002);
	/// <summary>
	/// The supplied handle was invalid.
	/// </summary>
	public const int SCARD_E_INVALID_HANDLE = unchecked((int)0x80100003);
	/// <summary>
	/// One or more of the supplied parameters could not be properly interpreted.
	/// </summary>
	public const int SCARD_E_INVALID_PARAMETER = unchecked((int)0x80100004);
	/// <summary>
	/// Registry startup information is missing or invalid.
	/// </summary>
	public const int SCARD_E_INVALID_TARGET = unchecked((int)0x80100005);
	/// <summary>
	/// Not enough memory available to complete this command.
	/// </summary>
	public const int SCARD_E_NO_MEMORY = unchecked((int)0x80100006);
	/// <summary>
	/// An internal consistency timer has expired.
	/// </summary>
	public const int SCARD_F_WAITED_TOO_LONG = unchecked((int)0x80100007);
	/// <summary>
	/// The data buffer to receive returned data is too small for the returned data.
	/// </summary>
	public const int SCARD_E_INSUFFICIENT_BUFFER = unchecked((int)0x80100008);
	/// <summary>
	/// The specified reader name is not recognized.
	/// </summary>
	public const int SCARD_E_UNKNOWN_READER = unchecked((int)0x80100009);
	/// <summary>
	/// The user-specified timeout value has expired.
	/// </summary>
	public const int SCARD_E_TIMEOUT = unchecked((int)0x8010000A);
	/// <summary>
	/// The smart card cannot be accessed because of other connections outstanding.
	/// </summary>
	public const int SCARD_E_SHARING_VIOLATION = unchecked((int)0x8010000B);
	/// <summary>
	/// The operation requires a Smart Card, but no Smart Card is currently in the device.
	/// </summary>
	public const int SCARD_E_NO_SMARTCARD = unchecked((int)0x8010000C);
	/// <summary>
	/// The specified smart card name is not recognized.
	/// </summary>
	public const int SCARD_E_UNKNOWN_CARD = unchecked((int)0x8010000D);
	/// <summary>
	/// The system could not dispose of the media in the requested manner.
	/// </summary>
	public const int SCARD_E_CANT_DISPOSE = unchecked((int)0x8010000E);
	/// <summary>
	/// The requested protocols are incompatible with the protocol currently in use with the smart card.
	/// </summary>
	public const int SCARD_E_PROTO_MISMATCH = unchecked((int)0x8010000F);
	/// <summary>
	/// The reader or smart card is not ready to accept commands.
	/// </summary>
	public const int SCARD_E_NOT_READY = unchecked((int)0x80100010);
	/// <summary>
	/// One or more of the supplied parameters values could not be properly interpreted.
	/// </summary>
	public const int SCARD_E_INVALID_VALUE = unchecked((int)0x80100011);
	/// <summary>
	/// The action was canceled by the system, presumably to log off or shut down.
	/// </summary>
	public const int SCARD_E_SYSTEM_CANCELLED = unchecked((int)0x80100012);
	/// <summary>
	/// An internal communications error has been detected.
	/// </summary>
	public const int SCARD_F_COMM_ERROR = unchecked((int)0x80100013);
	/// <summary>
	/// An internal error has been detected, but the source is unknown.
	/// </summary>
	public const int SCARD_F_UNKNOWN_ERROR = unchecked((int)0x80100014);
	/// <summary>
	/// An ATR obtained from the registry is not a valid ATR string.
	/// </summary>
	public const int SCARD_E_INVALID_ATR = unchecked((int)0x80100015);
	/// <summary>
	/// An attempt was made to end a non-existent transaction.
	/// </summary>
	public const int SCARD_E_NOT_TRANSACTED = unchecked((int)0x80100016);
	/// <summary>
	/// The specified reader is not currently available for use.
	/// </summary>
	public const int SCARD_E_READER_UNAVAILABLE = unchecked((int)0x80100017);
	/// <summary>
	/// The operation has been aborted to allow the server application to exit.
	/// </summary>
	public const int SCARD_P_SHUTDOWN = unchecked((int)0x80100018);
	/// <summary>
	/// The PCI Receive buffer was too small.
	/// </summary>
	public const int SCARD_E_PCI_TOO_SMALL = unchecked((int)0x80100019);
	/// <summary>
	/// The reader driver does not meet minimal requirements for support.
	/// </summary>
	public const int SCARD_E_READER_UNSUPPORTED = unchecked((int)0x8010001A);
	/// <summary>
	/// The reader driver did not produce a unique reader name.
	/// </summary>
	public const int SCARD_E_DUPLICATE_READER = unchecked((int)0x8010001B);
	/// <summary>
	/// The smart card does not meet minimal requirements for support.
	/// </summary>
	public const int SCARD_E_CARD_UNSUPPORTED = unchecked((int)0x8010001C);
	/// <summary>
	/// The Smart card resource manager is not running.
	/// </summary>
	public const int SCARD_E_NO_SERVICE = unchecked((int)0x8010001D);
	/// <summary>
	/// The Smart card resource manager has shut down.
	/// </summary>
	public const int SCARD_E_SERVICE_STOPPED = unchecked((int)0x8010001E);
	/// <summary>
	/// An unexpected card error has occurred.
	/// </summary>
	public const int SCARD_E_UNEXPECTED = unchecked((int)0x8010001F);
	/// <summary>
	/// No Primary Provider can be found for the smart card.
	/// </summary>
	public const int SCARD_E_ICC_INSTALLATION = unchecked((int)0x80100020);
	/// <summary>
	/// The requested order of object creation is not supported.
	/// </summary>
	public const int SCARD_E_ICC_CREATEORDER = unchecked((int)0x80100021);
	/// <summary>
	/// This smart card does not support the requested feature.
	/// </summary>
	public const int SCARD_E_UNSUPPORTED_FEATURE = unchecked((int)0x80100022);
	/// <summary>
	/// The identified directory does not exist in the smart card.
	/// </summary>
	public const int SCARD_E_DIR_NOT_FOUND = unchecked((int)0x80100023);
	/// <summary>
	/// The identified file does not exist in the smart card.
	/// </summary>
	public const int SCARD_E_FILE_NOT_FOUND = unchecked((int)0x80100024);
	/// <summary>
	/// The supplied path does not represent a smart card directory.
	/// </summary>
	public const int SCARD_E_NO_DIR = unchecked((int)0x80100025);
	/// <summary>
	/// The supplied path does not represent a smart card file.
	/// </summary>
	public const int SCARD_E_NO_FILE = unchecked((int)0x80100026);
	/// <summary>
	/// Access is denied to this file.
	/// </summary>
	public const int SCARD_E_NO_ACCESS = unchecked((int)0x80100027);
	/// <summary>
	/// The smartcard does not have enough memory to store the information.
	/// </summary>
	public const int SCARD_E_WRITE_TOO_MANY = unchecked((int)0x80100028);
	/// <summary>
	/// There was an error trying to set the smart card file object pointer.
	/// </summary>
	public const int SCARD_E_BAD_SEEK = unchecked((int)0x80100029);
	/// <summary>
	/// The supplied PIN is incorrect.
	/// </summary>
	public const int SCARD_E_INVALID_CHV = unchecked((int)0x8010002A);
	/// <summary>
	/// An unrecognized error code was returned from a layered component.
	/// </summary>
	public const int SCARD_E_UNKNOWN_RES_MNG = unchecked((int)0x8010002B);
	/// <summary>
	/// The requested certificate does not exist.
	/// </summary>
	public const int SCARD_E_NO_SUCH_CERTIFICATE = unchecked((int)0x8010002C);
	/// <summary>
	/// The requested certificate could not be obtained.
	/// </summary>
	public const int SCARD_E_CERTIFICATE_UNAVAILABLE = unchecked((int)0x8010002D);
	/// <summary>
	/// Cannot find a smart card reader.
	/// </summary>
	public const int SCARD_E_NO_READERS_AVAILABLE = unchecked((int)0x8010002E);
	/// <summary>
	/// A communications error with the smart card has been detected. Retry the operation.
	/// </summary>
	public const int SCARD_E_COMM_DATA_LOST = unchecked((int)0x8010002F);
	/// <summary>
	/// The requested key container does not exist on the smart card.
	/// </summary>
	public const int SCARD_E_NO_KEY_CONTAINER = unchecked((int)0x80100030);
	/// <summary>
	/// The Smart card resource manager is too busy to complete this operation.
	/// </summary>
	public const int SCARD_E_SERVER_TOO_BUSY = unchecked((int)0x80100031);
	/// <summary>
	/// The smart card PIN cache has expired.
	/// </summary>
	public const int SCARD_E_PIN_CACHE_EXPIRED = unchecked((int)0x80100032);
	/// <summary>
	/// The smart card PIN cannot be cached.
	/// </summary>
	public const int SCARD_E_NO_PIN_CACHE = unchecked((int)0x80100033);
	/// <summary>
	/// The smart card is read only and cannot be written to.
	/// </summary>
	public const int SCARD_E_READ_ONLY_CARD = unchecked((int)0x80100034);
	/// <summary>
	/// The reader cannot communicate with the smart card, due to ATR configuration conflicts.
	/// </summary>
	public const int SCARD_W_UNSUPPORTED_CARD = unchecked((int)0x80100065);
	/// <summary>
	/// The smart card is not responding to a reset.
	/// </summary>
	public const int SCARD_W_UNRESPONSIVE_CARD = unchecked((int)0x80100066);
	/// <summary>
	/// Power has been removed from the smart card, so that further communication is not possible.
	/// </summary>
	public const int SCARD_W_UNPOWERED_CARD = unchecked((int)0x80100067);
	/// <summary>
	/// The smart card has been reset, so any shared state information is invalid.
	/// </summary>
	public const int SCARD_W_RESET_CARD = unchecked((int)0x80100068);
	/// <summary>
	/// The smart card has been removed, so that further communication is not possible.
	/// </summary>
	public const int SCARD_W_REMOVED_CARD = unchecked((int)0x80100069);
	/// <summary>
	/// Access was denied because of a security violation.
	/// </summary>
	public const int SCARD_W_SECURITY_VIOLATION = unchecked((int)0x8010006A);
	/// <summary>
	/// The card cannot be accessed because the wrong PIN was presented.
	/// </summary>
	public const int SCARD_W_WRONG_CHV = unchecked((int)0x8010006B);
	/// <summary>
	/// The card cannot be accessed because the maximum number of PIN entry attempts has been reached.
	/// </summary>
	public const int SCARD_W_CHV_BLOCKED = unchecked((int)0x8010006C);
	/// <summary>
	/// The end of the smart card file has been reached.
	/// </summary>
	public const int SCARD_W_EOF = unchecked((int)0x8010006D);
	/// <summary>
	/// The action was canceled by the user.
	/// </summary>
	public const int SCARD_W_CANCELLED_BY_USER = unchecked((int)0x8010006E);
	/// <summary>
	/// No PIN was presented to the smart card.
	/// </summary>
	public const int SCARD_W_CARD_NOT_AUTHENTICATED = unchecked((int)0x8010006F);
	/// <summary>
	/// The requested item could not be found in the cache.
	/// </summary>
	public const int SCARD_W_CACHE_ITEM_NOT_FOUND = unchecked((int)0x80100070);
	/// <summary>
	/// The requested cache item is too old and was deleted from the cache.
	/// </summary>
	public const int SCARD_W_CACHE_ITEM_STALE = unchecked((int)0x80100071);
	/// <summary>
	/// The new cache item exceeds the maximum per-item size defined for the cache.
	/// </summary>
	public const int SCARD_W_CACHE_ITEM_TOO_BIG = unchecked((int)0x80100072);
	/// <summary>
	/// Authentication target is invalid or not configured correctly.
	/// </summary>
	public const int ONL_E_INVALID_AUTHENTICATION_TARGET = unchecked((int)0x8A020001);
	/// <summary>
	/// Your application cannot get the Online Id properties due to the Terms of Use accepted by the user.
	/// </summary>
	public const int ONL_E_ACCESS_DENIED_BY_TOU = unchecked((int)0x8A020002);
	#endregion

	#region COMADMIN, FILTER, GRAPHICS
	/// <summary>
	/// Errors occurred accessing one or more objects - the ErrorInfo collection may have more detail
	/// </summary>
	public const int COMADMIN_E_OBJECTERRORS = unchecked((int)0x80110401);
	/// <summary>
	/// One or more of the object's properties are missing or invalid
	/// </summary>
	public const int COMADMIN_E_OBJECTINVALID = unchecked((int)0x80110402);
	/// <summary>
	/// The object was not found in the catalog
	/// </summary>
	public const int COMADMIN_E_KEYMISSING = unchecked((int)0x80110403);
	/// <summary>
	/// The object is already registered
	/// </summary>
	public const int COMADMIN_E_ALREADYINSTALLED = unchecked((int)0x80110404);
	/// <summary>
	/// Error occurred writing to the application file
	/// </summary>
	public const int COMADMIN_E_APP_FILE_WRITEFAIL = unchecked((int)0x80110407);
	/// <summary>
	/// Error occurred reading the application file
	/// </summary>
	public const int COMADMIN_E_APP_FILE_READFAIL = unchecked((int)0x80110408);
	/// <summary>
	/// Invalid version number in application file
	/// </summary>
	public const int COMADMIN_E_APP_FILE_VERSION = unchecked((int)0x80110409);
	/// <summary>
	/// The file path is invalid
	/// </summary>
	public const int COMADMIN_E_BADPATH = unchecked((int)0x8011040A);
	/// <summary>
	/// The application is already installed
	/// </summary>
	public const int COMADMIN_E_APPLICATIONEXISTS = unchecked((int)0x8011040B);
	/// <summary>
	/// The role already exists
	/// </summary>
	public const int COMADMIN_E_ROLEEXISTS = unchecked((int)0x8011040C);
	/// <summary>
	/// An error occurred copying the file
	/// </summary>
	public const int COMADMIN_E_CANTCOPYFILE = unchecked((int)0x8011040D);
	/// <summary>
	/// One or more users are not valid
	/// </summary>
	public const int COMADMIN_E_NOUSER = unchecked((int)0x8011040F);
	/// <summary>
	/// One or more users in the application file are not valid
	/// </summary>
	public const int COMADMIN_E_INVALIDUSERIDS = unchecked((int)0x80110410);
	/// <summary>
	/// The component's CLSID is missing or corrupt
	/// </summary>
	public const int COMADMIN_E_NOREGISTRYCLSID = unchecked((int)0x80110411);
	/// <summary>
	/// The component's progID is missing or corrupt
	/// </summary>
	public const int COMADMIN_E_BADREGISTRYPROGID = unchecked((int)0x80110412);
	/// <summary>
	/// Unable to set required authentication level for update request
	/// </summary>
	public const int COMADMIN_E_AUTHENTICATIONLEVEL = unchecked((int)0x80110413);
	/// <summary>
	/// The identity or password set on the application is not valid
	/// </summary>
	public const int COMADMIN_E_USERPASSWDNOTVALID = unchecked((int)0x80110414);
	/// <summary>
	/// Application file CLSIDs or IIDs do not match corresponding DLLs
	/// </summary>
	public const int COMADMIN_E_CLSIDORIIDMISMATCH = unchecked((int)0x80110418);
	/// <summary>
	/// Interface information is either missing or changed
	/// </summary>
	public const int COMADMIN_E_REMOTEINTERFACE = unchecked((int)0x80110419);
	/// <summary>
	/// DllRegisterServer failed on component install
	/// </summary>
	public const int COMADMIN_E_DLLREGISTERSERVER = unchecked((int)0x8011041A);
	/// <summary>
	/// No server file share available
	/// </summary>
	public const int COMADMIN_E_NOSERVERSHARE = unchecked((int)0x8011041B);
	/// <summary>
	/// DLL could not be loaded
	/// </summary>
	public const int COMADMIN_E_DLLLOADFAILED = unchecked((int)0x8011041D);
	/// <summary>
	/// The registered TypeLib ID is not valid
	/// </summary>
	public const int COMADMIN_E_BADREGISTRYLIBID = unchecked((int)0x8011041E);
	/// <summary>
	/// Application install directory not found
	/// </summary>
	public const int COMADMIN_E_APPDIRNOTFOUND = unchecked((int)0x8011041F);
	/// <summary>
	/// Errors occurred while in the component registrar
	/// </summary>
	public const int COMADMIN_E_REGISTRARFAILED = unchecked((int)0x80110423);
	/// <summary>
	/// The file does not exist
	/// </summary>
	public const int COMADMIN_E_COMPFILE_DOESNOTEXIST = unchecked((int)0x80110424);
	/// <summary>
	/// The DLL could not be loaded
	/// </summary>
	public const int COMADMIN_E_COMPFILE_LOADDLLFAIL = unchecked((int)0x80110425);
	/// <summary>
	/// GetClassObject failed in the DLL
	/// </summary>
	public const int COMADMIN_E_COMPFILE_GETCLASSOBJ = unchecked((int)0x80110426);
	/// <summary>
	/// The DLL does not support the components listed in the TypeLib
	/// </summary>
	public const int COMADMIN_E_COMPFILE_CLASSNOTAVAIL = unchecked((int)0x80110427);
	/// <summary>
	/// The TypeLib could not be loaded
	/// </summary>
	public const int COMADMIN_E_COMPFILE_BADTLB = unchecked((int)0x80110428);
	/// <summary>
	/// The file does not contain components or component information
	/// </summary>
	public const int COMADMIN_E_COMPFILE_NOTINSTALLABLE = unchecked((int)0x80110429);
	/// <summary>
	/// Changes to this object and its sub-objects have been disabled
	/// </summary>
	public const int COMADMIN_E_NOTCHANGEABLE = unchecked((int)0x8011042A);
	/// <summary>
	/// The delete function has been disabled for this object
	/// </summary>
	public const int COMADMIN_E_NOTDELETEABLE = unchecked((int)0x8011042B);
	/// <summary>
	/// The server catalog version is not supported
	/// </summary>
	public const int COMADMIN_E_SESSION = unchecked((int)0x8011042C);
	/// <summary>
	/// The component move was disallowed, because the source or destination application is either a system application or currently locked against changes
	/// </summary>
	public const int COMADMIN_E_COMP_MOVE_LOCKED = unchecked((int)0x8011042D);
	/// <summary>
	/// The component move failed because the destination application no longer exists
	/// </summary>
	public const int COMADMIN_E_COMP_MOVE_BAD_DEST = unchecked((int)0x8011042E);
	/// <summary>
	/// The system was unable to register the TypeLib
	/// </summary>
	public const int COMADMIN_E_REGISTERTLB = unchecked((int)0x80110430);
	/// <summary>
	/// This operation cannot be performed on the system application
	/// </summary>
	public const int COMADMIN_E_SYSTEMAPP = unchecked((int)0x80110433);
	/// <summary>
	/// The component registrar referenced in this file is not available
	/// </summary>
	public const int COMADMIN_E_COMPFILE_NOREGISTRAR = unchecked((int)0x80110434);
	/// <summary>
	/// A component in the same DLL is already installed
	/// </summary>
	public const int COMADMIN_E_COREQCOMPINSTALLED = unchecked((int)0x80110435);
	/// <summary>
	/// The service is not installed
	/// </summary>
	public const int COMADMIN_E_SERVICENOTINSTALLED = unchecked((int)0x80110436);
	/// <summary>
	/// One or more property settings are either invalid or in conflict with each other
	/// </summary>
	public const int COMADMIN_E_PROPERTYSAVEFAILED = unchecked((int)0x80110437);
	/// <summary>
	/// The object you are attempting to add or rename already exists
	/// </summary>
	public const int COMADMIN_E_OBJECTEXISTS = unchecked((int)0x80110438);
	/// <summary>
	/// The component already exists
	/// </summary>
	public const int COMADMIN_E_COMPONENTEXISTS = unchecked((int)0x80110439);
	/// <summary>
	/// The registration file is corrupt
	/// </summary>
	public const int COMADMIN_E_REGFILE_CORRUPT = unchecked((int)0x8011043B);
	/// <summary>
	/// The property value is too large
	/// </summary>
	public const int COMADMIN_E_PROPERTY_OVERFLOW = unchecked((int)0x8011043C);
	/// <summary>
	/// Object was not found in registry
	/// </summary>
	public const int COMADMIN_E_NOTINREGISTRY = unchecked((int)0x8011043E);
	/// <summary>
	/// This object is not poolable
	/// </summary>
	public const int COMADMIN_E_OBJECTNOTPOOLABLE = unchecked((int)0x8011043F);
	/// <summary>
	/// A CLSID with the same GUID as the new application ID is already installed on this machine
	/// </summary>
	public const int COMADMIN_E_APPLID_MATCHES_CLSID = unchecked((int)0x80110446);
	/// <summary>
	/// A role assigned to a component, interface, or method did not exist in the application
	/// </summary>
	public const int COMADMIN_E_ROLE_DOES_NOT_EXIST = unchecked((int)0x80110447);
	/// <summary>
	/// You must have components in an application in order to start the application
	/// </summary>
	public const int COMADMIN_E_START_APP_NEEDS_COMPONENTS = unchecked((int)0x80110448);
	/// <summary>
	/// This operation is not enabled on this platform
	/// </summary>
	public const int COMADMIN_E_REQUIRES_DIFFERENT_PLATFORM = unchecked((int)0x80110449);
	/// <summary>
	/// Application Proxy is not exportable
	/// </summary>
	public const int COMADMIN_E_CAN_NOT_EXPORT_APP_PROXY = unchecked((int)0x8011044A);
	/// <summary>
	/// Failed to start application because it is either a library application or an application proxy
	/// </summary>
	public const int COMADMIN_E_CAN_NOT_START_APP = unchecked((int)0x8011044B);
	/// <summary>
	/// System application is not exportable
	/// </summary>
	public const int COMADMIN_E_CAN_NOT_EXPORT_SYS_APP = unchecked((int)0x8011044C);
	/// <summary>
	/// Cannot subscribe to this component (the component may have been imported)
	/// </summary>
	public const int COMADMIN_E_CANT_SUBSCRIBE_TO_COMPONENT = unchecked((int)0x8011044D);
	/// <summary>
	/// An event class cannot also be a subscriber component
	/// </summary>
	public const int COMADMIN_E_EVENTCLASS_CANT_BE_SUBSCRIBER = unchecked((int)0x8011044E);
	/// <summary>
	/// Library applications and application proxies are incompatible
	/// </summary>
	public const int COMADMIN_E_LIB_APP_PROXY_INCOMPATIBLE = unchecked((int)0x8011044F);
	/// <summary>
	/// This function is valid for the base partition only
	/// </summary>
	public const int COMADMIN_E_BASE_PARTITION_ONLY = unchecked((int)0x80110450);
	/// <summary>
	/// You cannot start an application that has been disabled
	/// </summary>
	public const int COMADMIN_E_START_APP_DISABLED = unchecked((int)0x80110451);
	/// <summary>
	/// The specified partition name is already in use on this computer
	/// </summary>
	public const int COMADMIN_E_CAT_DUPLICATE_PARTITION_NAME = unchecked((int)0x80110457);
	/// <summary>
	/// The specified partition name is invalid. Check that the name contains at least one visible character
	/// </summary>
	public const int COMADMIN_E_CAT_INVALID_PARTITION_NAME = unchecked((int)0x80110458);
	/// <summary>
	/// The partition cannot be deleted because it is the default partition for one or more users
	/// </summary>
	public const int COMADMIN_E_CAT_PARTITION_IN_USE = unchecked((int)0x80110459);
	/// <summary>
	/// The partition cannot be exported, because one or more components in the partition have the same file name
	/// </summary>
	public const int COMADMIN_E_FILE_PARTITION_DUPLICATE_FILES = unchecked((int)0x8011045A);
	/// <summary>
	/// Applications that contain one or more imported components cannot be installed into a non-base partition
	/// </summary>
	public const int COMADMIN_E_CAT_IMPORTED_COMPONENTS_NOT_ALLOWED = unchecked((int)0x8011045B);
	/// <summary>
	/// The application name is not unique and cannot be resolved to an application id
	/// </summary>
	public const int COMADMIN_E_AMBIGUOUS_APPLICATION_NAME = unchecked((int)0x8011045C);
	/// <summary>
	/// The partition name is not unique and cannot be resolved to a partition id
	/// </summary>
	public const int COMADMIN_E_AMBIGUOUS_PARTITION_NAME = unchecked((int)0x8011045D);
	/// <summary>
	/// The COM+ registry database has not been initialized
	/// </summary>
	public const int COMADMIN_E_REGDB_NOTINITIALIZED = unchecked((int)0x80110472);
	/// <summary>
	/// The COM+ registry database is not open
	/// </summary>
	public const int COMADMIN_E_REGDB_NOTOPEN = unchecked((int)0x80110473);
	/// <summary>
	/// The COM+ registry database detected a system error
	/// </summary>
	public const int COMADMIN_E_REGDB_SYSTEMERR = unchecked((int)0x80110474);
	/// <summary>
	/// The COM+ registry database is already running
	/// </summary>
	public const int COMADMIN_E_REGDB_ALREADYRUNNING = unchecked((int)0x80110475);
	/// <summary>
	/// This version of the COM+ registry database cannot be migrated
	/// </summary>
	public const int COMADMIN_E_MIG_VERSIONNOTSUPPORTED = unchecked((int)0x80110480);
	/// <summary>
	/// The schema version to be migrated could not be found in the COM+ registry database
	/// </summary>
	public const int COMADMIN_E_MIG_SCHEMANOTFOUND = unchecked((int)0x80110481);
	/// <summary>
	/// There was a type mismatch between binaries
	/// </summary>
	public const int COMADMIN_E_CAT_BITNESSMISMATCH = unchecked((int)0x80110482);
	/// <summary>
	/// A binary of unknown or invalid type was provided
	/// </summary>
	public const int COMADMIN_E_CAT_UNACCEPTABLEBITNESS = unchecked((int)0x80110483);
	/// <summary>
	/// There was a type mismatch between a binary and an application
	/// </summary>
	public const int COMADMIN_E_CAT_WRONGAPPBITNESS = unchecked((int)0x80110484);
	/// <summary>
	/// The application cannot be paused or resumed
	/// </summary>
	public const int COMADMIN_E_CAT_PAUSE_RESUME_NOT_SUPPORTED = unchecked((int)0x80110485);
	/// <summary>
	/// The COM+ Catalog Server threw an exception during execution
	/// </summary>
	public const int COMADMIN_E_CAT_SERVERFAULT = unchecked((int)0x80110486);
	/// <summary>
	/// Only COM+ Applications marked "queued" can be invoked using the "queue" moniker
	/// </summary>
	public const int COMQC_E_APPLICATION_NOT_QUEUED = unchecked((int)0x80110600);
	/// <summary>
	/// At least one interface must be marked "queued" in order to create a queued component instance with the "queue" moniker
	/// </summary>
	public const int COMQC_E_NO_QUEUEABLE_INTERFACES = unchecked((int)0x80110601);
	/// <summary>
	/// MSMQ is required for the requested operation and is not installed
	/// </summary>
	public const int COMQC_E_QUEUING_SERVICE_NOT_AVAILABLE = unchecked((int)0x80110602);
	/// <summary>
	/// Unable to marshal an interface that does not support IPersistStream
	/// </summary>
	public const int COMQC_E_NO_IPERSISTSTREAM = unchecked((int)0x80110603);
	/// <summary>
	/// The message is improperly formatted or was damaged in transit
	/// </summary>
	public const int COMQC_E_BAD_MESSAGE = unchecked((int)0x80110604);
	/// <summary>
	/// An unauthenticated message was received by an application that accepts only authenticated messages
	/// </summary>
	public const int COMQC_E_UNAUTHENTICATED = unchecked((int)0x80110605);
	/// <summary>
	/// The message was requeued or moved by a user not in the "QC Trusted User" role
	/// </summary>
	public const int COMQC_E_UNTRUSTED_ENQUEUER = unchecked((int)0x80110606);
	/// <summary>
	/// Cannot create a duplicate resource of type Distributed Transaction Coordinator
	/// </summary>
	public const int MSDTC_E_DUPLICATE_RESOURCE = unchecked((int)0x80110701);
	/// <summary>
	/// One of the objects being inserted or updated does not belong to a valid parent collection
	/// </summary>
	public const int COMADMIN_E_OBJECT_PARENT_MISSING = unchecked((int)0x80110808);
	/// <summary>
	/// One of the specified objects cannot be found
	/// </summary>
	public const int COMADMIN_E_OBJECT_DOES_NOT_EXIST = unchecked((int)0x80110809);
	/// <summary>
	/// The specified application is not currently running
	/// </summary>
	public const int COMADMIN_E_APP_NOT_RUNNING = unchecked((int)0x8011080A);
	/// <summary>
	/// The partition(s) specified are not valid.
	/// </summary>
	public const int COMADMIN_E_INVALID_PARTITION = unchecked((int)0x8011080B);
	/// <summary>
	/// COM+ applications that run as NT service may not be pooled or recycled
	/// </summary>
	public const int COMADMIN_E_SVCAPP_NOT_POOLABLE_OR_RECYCLABLE = unchecked((int)0x8011080D);
	/// <summary>
	/// One or more users are already assigned to a local partition set.
	/// </summary>
	public const int COMADMIN_E_USER_IN_SET = unchecked((int)0x8011080E);
	/// <summary>
	/// Library applications may not be recycled.
	/// </summary>
	public const int COMADMIN_E_CANTRECYCLELIBRARYAPPS = unchecked((int)0x8011080F);
	/// <summary>
	/// Applications running as NT services may not be recycled.
	/// </summary>
	public const int COMADMIN_E_CANTRECYCLESERVICEAPPS = unchecked((int)0x80110811);
	/// <summary>
	/// The process has already been recycled.
	/// </summary>
	public const int COMADMIN_E_PROCESSALREADYRECYCLED = unchecked((int)0x80110812);
	/// <summary>
	/// A paused process may not be recycled.
	/// </summary>
	public const int COMADMIN_E_PAUSEDPROCESSMAYNOTBERECYCLED = unchecked((int)0x80110813);
	/// <summary>
	/// Library applications may not be NT services.
	/// </summary>
	public const int COMADMIN_E_CANTMAKEINPROCSERVICE = unchecked((int)0x80110814);
	/// <summary>
	/// The ProgID provided to the copy operation is invalid. The ProgID is in use by another registered CLSID.
	/// </summary>
	public const int COMADMIN_E_PROGIDINUSEBYCLSID = unchecked((int)0x80110815);
	/// <summary>
	/// The partition specified as default is not a member of the partition set.
	/// </summary>
	public const int COMADMIN_E_DEFAULT_PARTITION_NOT_IN_SET = unchecked((int)0x80110816);
	/// <summary>
	/// A recycled process may not be paused.
	/// </summary>
	public const int COMADMIN_E_RECYCLEDPROCESSMAYNOTBEPAUSED = unchecked((int)0x80110817);
	/// <summary>
	/// Access to the specified partition is denied.
	/// </summary>
	public const int COMADMIN_E_PARTITION_ACCESSDENIED = unchecked((int)0x80110818);
	/// <summary>
	/// Only Application Files (*.MSI files) can be installed into partitions.
	/// </summary>
	public const int COMADMIN_E_PARTITION_MSI_ONLY = unchecked((int)0x80110819);
	/// <summary>
	/// Applications containing one or more legacy components may not be exported to 1.0 format.
	/// </summary>
	public const int COMADMIN_E_LEGACYCOMPS_NOT_ALLOWED_IN_1_0_FORMAT = unchecked((int)0x8011081A);
	/// <summary>
	/// Legacy components may not exist in non-base partitions.
	/// </summary>
	public const int COMADMIN_E_LEGACYCOMPS_NOT_ALLOWED_IN_NONBASE_PARTITIONS = unchecked((int)0x8011081B);
	/// <summary>
	/// A component cannot be moved (or copied) from the System Application, an application proxy or a non-changeable application
	/// </summary>
	public const int COMADMIN_E_COMP_MOVE_SOURCE = unchecked((int)0x8011081C);
	/// <summary>
	/// A component cannot be moved (or copied) to the System Application, an application proxy or a non-changeable application
	/// </summary>
	public const int COMADMIN_E_COMP_MOVE_DEST = unchecked((int)0x8011081D);
	/// <summary>
	/// A private component cannot be moved (or copied) to a library application or to the base partition
	/// </summary>
	public const int COMADMIN_E_COMP_MOVE_PRIVATE = unchecked((int)0x8011081E);
	/// <summary>
	/// The Base Application Partition exists in all partition sets and cannot be removed.
	/// </summary>
	public const int COMADMIN_E_BASEPARTITION_REQUIRED_IN_SET = unchecked((int)0x8011081F);
	/// <summary>
	/// Event Class components cannot be aliased.
	/// </summary>
	public const int COMADMIN_E_CANNOT_ALIAS_EVENTCLASS = unchecked((int)0x80110820);
	/// <summary>
	/// Access is denied because the component is private.
	/// </summary>
	public const int COMADMIN_E_PRIVATE_ACCESSDENIED = unchecked((int)0x80110821);
	/// <summary>
	/// The specified SAFER level is invalid.
	/// </summary>
	public const int COMADMIN_E_SAFERINVALID = unchecked((int)0x80110822);
	/// <summary>
	/// The specified user cannot write to the system registry
	/// </summary>
	public const int COMADMIN_E_REGISTRY_ACCESSDENIED = unchecked((int)0x80110823);
	/// <summary>
	/// COM+ partitions are currently disabled.
	/// </summary>
	public const int COMADMIN_E_PARTITIONS_DISABLED = unchecked((int)0x80110824);
	/// <summary>
	/// The IO was completed by a filter.
	/// </summary>
	public const int ERROR_FLT_IO_COMPLETE = 0x001F0001;
	/// <summary>
	/// A handler was not defined by the filter for this operation.
	/// </summary>
	public const int ERROR_FLT_NO_HANDLER_DEFINED = unchecked((int)0x801F0001);
	/// <summary>
	/// A context is already defined for this object.
	/// </summary>
	public const int ERROR_FLT_CONTEXT_ALREADY_DEFINED = unchecked((int)0x801F0002);
	/// <summary>
	/// Asynchronous requests are not valid for this operation.
	/// </summary>
	public const int ERROR_FLT_INVALID_ASYNCHRONOUS_REQUEST = unchecked((int)0x801F0003);
	/// <summary>
	/// Disallow the Fast IO path for this operation.
	/// </summary>
	public const int ERROR_FLT_DISALLOW_FAST_IO = unchecked((int)0x801F0004);
	/// <summary>
	/// An invalid name request was made. The name requested cannot be retrieved at this time.
	/// </summary>
	public const int ERROR_FLT_INVALID_NAME_REQUEST = unchecked((int)0x801F0005);
	/// <summary>
	/// Posting this operation to a worker thread for further processing is not safe at this time because it could lead to a system deadlock.
	/// </summary>
	public const int ERROR_FLT_NOT_SAFE_TO_POST_OPERATION = unchecked((int)0x801F0006);
	/// <summary>
	/// The Filter Manager was not initialized when a filter tried to register. Make sure that the Filter Manager is getting loaded as a driver.
	/// </summary>
	public const int ERROR_FLT_NOT_INITIALIZED = unchecked((int)0x801F0007);
	/// <summary>
	/// The filter is not ready for attachment to volumes because it has not finished initializing (FltStartFiltering has not been called).
	/// </summary>
	public const int ERROR_FLT_FILTER_NOT_READY = unchecked((int)0x801F0008);
	/// <summary>
	/// The filter must cleanup any operation specific context at this time because it is being removed from the system before the operation is completed by the lower drivers.
	/// </summary>
	public const int ERROR_FLT_POST_OPERATION_CLEANUP = unchecked((int)0x801F0009);
	/// <summary>
	/// The Filter Manager had an internal error from which it cannot recover, therefore the operation has been failed. This is usually the result of a filter returning an invalid value from a pre-operation callback.
	/// </summary>
	public const int ERROR_FLT_INTERNAL_ERROR = unchecked((int)0x801F000A);
	/// <summary>
	/// The object specified for this action is in the process of being deleted, therefore the action requested cannot be completed at this time.
	/// </summary>
	public const int ERROR_FLT_DELETING_OBJECT = unchecked((int)0x801F000B);
	/// <summary>
	/// Non-paged pool must be used for this type of context.
	/// </summary>
	public const int ERROR_FLT_MUST_BE_NONPAGED_POOL = unchecked((int)0x801F000C);
	/// <summary>
	/// A duplicate handler definition has been provided for an operation.
	/// </summary>
	public const int ERROR_FLT_DUPLICATE_ENTRY = unchecked((int)0x801F000D);
	/// <summary>
	/// The callback data queue has been disabled.
	/// </summary>
	public const int ERROR_FLT_CBDQ_DISABLED = unchecked((int)0x801F000E);
	/// <summary>
	/// Do not attach the filter to the volume at this time.
	/// </summary>
	public const int ERROR_FLT_DO_NOT_ATTACH = unchecked((int)0x801F000F);
	/// <summary>
	/// Do not detach the filter from the volume at this time.
	/// </summary>
	public const int ERROR_FLT_DO_NOT_DETACH = unchecked((int)0x801F0010);
	/// <summary>
	/// An instance already exists at this altitude on the volume specified.
	/// </summary>
	public const int ERROR_FLT_INSTANCE_ALTITUDE_COLLISION = unchecked((int)0x801F0011);
	/// <summary>
	/// An instance already exists with this name on the volume specified.
	/// </summary>
	public const int ERROR_FLT_INSTANCE_NAME_COLLISION = unchecked((int)0x801F0012);
	/// <summary>
	/// The system could not find the filter specified.
	/// </summary>
	public const int ERROR_FLT_FILTER_NOT_FOUND = unchecked((int)0x801F0013);
	/// <summary>
	/// The system could not find the volume specified.
	/// </summary>
	public const int ERROR_FLT_VOLUME_NOT_FOUND = unchecked((int)0x801F0014);
	/// <summary>
	/// The system could not find the instance specified.
	/// </summary>
	public const int ERROR_FLT_INSTANCE_NOT_FOUND = unchecked((int)0x801F0015);
	/// <summary>
	/// No registered context allocation definition was found for the given request.
	/// </summary>
	public const int ERROR_FLT_CONTEXT_ALLOCATION_NOT_FOUND = unchecked((int)0x801F0016);
	/// <summary>
	/// An invalid parameter was specified during context registration.
	/// </summary>
	public const int ERROR_FLT_INVALID_CONTEXT_REGISTRATION = unchecked((int)0x801F0017);
	/// <summary>
	/// The name requested was not found in Filter Manager's name cache and could not be retrieved from the file system.
	/// </summary>
	public const int ERROR_FLT_NAME_CACHE_MISS = unchecked((int)0x801F0018);
	/// <summary>
	/// The requested device object does not exist for the given volume.
	/// </summary>
	public const int ERROR_FLT_NO_DEVICE_OBJECT = unchecked((int)0x801F0019);
	/// <summary>
	/// The specified volume is already mounted.
	/// </summary>
	public const int ERROR_FLT_VOLUME_ALREADY_MOUNTED = unchecked((int)0x801F001A);
	/// <summary>
	/// The specified Transaction Context is already enlisted in a transaction
	/// </summary>
	public const int ERROR_FLT_ALREADY_ENLISTED = unchecked((int)0x801F001B);
	/// <summary>
	/// The specified context is already attached to another object
	/// </summary>
	public const int ERROR_FLT_CONTEXT_ALREADY_LINKED = unchecked((int)0x801F001C);
	/// <summary>
	/// No waiter is present for the filter's reply to this message.
	/// </summary>
	public const int ERROR_FLT_NO_WAITER_FOR_REPLY = unchecked((int)0x801F0020);
	/// <summary>
	/// The filesystem database resource is in use. Registration cannot complete at this time.
	/// </summary>
	public const int ERROR_FLT_REGISTRATION_BUSY = unchecked((int)0x801F0023);
	/// <summary>
	/// Display Driver Stopped Responding} The %hs display driver has stopped working normally. Save your work and reboot the system to restore full display functionality. The next time you reboot the machine a dialog will be displayed giving you a chance to report this failure to Microsoft.
	/// </summary>
	public const int ERROR_HUNG_DISPLAY_DRIVER_THREAD = unchecked((int)0x80260001);
	/// <summary>
	/// Desktop composition is disabled} The operation could not be completed because desktop composition is disabled.
	/// </summary>
	public const int DWM_E_COMPOSITIONDISABLED = unchecked((int)0x80263001);
	/// <summary>
	/// Some desktop composition APIs are not supported while remoting} The operation is not supported while running in a remote session.
	/// </summary>
	public const int DWM_E_REMOTING_NOT_SUPPORTED = unchecked((int)0x80263002);
	/// <summary>
	/// No DWM redirection surface is available} The DWM was unable to provide a redireciton surface to complete the DirectX present.
	/// </summary>
	public const int DWM_E_NO_REDIRECTION_SURFACE_AVAILABLE = unchecked((int)0x80263003);
	/// <summary>
	/// DWM is not queuing presents for the specified window} The window specified is not currently using queued presents.
	/// </summary>
	public const int DWM_E_NOT_QUEUING_PRESENTS = unchecked((int)0x80263004);
	/// <summary>
	/// The adapter specified by the LUID is not found} DWM cannot find the adapter specified by the LUID.
	/// </summary>
	public const int DWM_E_ADAPTER_NOT_FOUND = unchecked((int)0x80263005);
	/// <summary>
	/// GDI redirection surface was returned} GDI redirection surface of the top level window was returned.
	/// </summary>
	public const int DWM_S_GDI_REDIRECTION_SURFACE = 0x00263005;
	/// <summary>
	/// Redirection surface can not be created. The size of the surface is larger than what is supported on this machine} Redirection surface can not be created. The size of the surface is larger than what is supported on this machine.
	/// </summary>
	public const int DWM_E_TEXTURE_TOO_LARGE = unchecked((int)0x80263007);
	/// <summary>
	/// Monitor descriptor could not be obtained.
	/// </summary>
	public const int ERROR_MONITOR_NO_DESCRIPTOR = unchecked((int)0x80261001);
	/// <summary>
	/// Format of the obtained monitor descriptor is not supported by this release.
	/// </summary>
	public const int ERROR_MONITOR_UNKNOWN_DESCRIPTOR_FORMAT = unchecked((int)0x80261002);
	/// <summary>
	/// Checksum of the obtained monitor descriptor is invalid.
	/// </summary>
	public const int ERROR_MONITOR_INVALID_DESCRIPTOR_CHECKSUM = unchecked((int)0xC0261003);
	/// <summary>
	/// Monitor descriptor contains an invalid standard timing block.
	/// </summary>
	public const int ERROR_MONITOR_INVALID_STANDARD_TIMING_BLOCK = unchecked((int)0xC0261004);
	/// <summary>
	/// WMI data block registration failed for one of the MSMonitorClass WMI subclasses.
	/// </summary>
	public const int ERROR_MONITOR_WMI_DATABLOCK_REGISTRATION_FAILED = unchecked((int)0xC0261005);
	/// <summary>
	/// Provided monitor descriptor block is either corrupted or does not contain monitor's detailed serial number.
	/// </summary>
	public const int ERROR_MONITOR_INVALID_SERIAL_NUMBER_MONDSC_BLOCK = unchecked((int)0xC0261006);
	/// <summary>
	/// Provided monitor descriptor block is either corrupted or does not contain monitor's user friendly name.
	/// </summary>
	public const int ERROR_MONITOR_INVALID_USER_FRIENDLY_MONDSC_BLOCK = unchecked((int)0xC0261007);
	/// <summary>
	/// There is no monitor descriptor data at the specified (offset, size) region.
	/// </summary>
	public const int ERROR_MONITOR_NO_MORE_DESCRIPTOR_DATA = unchecked((int)0xC0261008);
	/// <summary>
	/// Monitor descriptor contains an invalid detailed timing block.
	/// </summary>
	public const int ERROR_MONITOR_INVALID_DETAILED_TIMING_BLOCK = unchecked((int)0xC0261009);
	/// <summary>
	/// Monitor descriptor contains invalid manufacture date.
	/// </summary>
	public const int ERROR_MONITOR_INVALID_MANUFACTURE_DATE = unchecked((int)0xC026100A);
	/// <summary>
	/// Exclusive mode ownership is needed to create unmanaged primary allocation.
	/// </summary>
	public const int ERROR_GRAPHICS_NOT_EXCLUSIVE_MODE_OWNER = unchecked((int)0xC0262000);
	/// <summary>
	/// The driver needs more DMA buffer space in order to complete the requested operation.
	/// </summary>
	public const int ERROR_GRAPHICS_INSUFFICIENT_DMA_BUFFER = unchecked((int)0xC0262001);
	/// <summary>
	/// Specified display adapter handle is invalid.
	/// </summary>
	public const int ERROR_GRAPHICS_INVALID_DISPLAY_ADAPTER = unchecked((int)0xC0262002);
	/// <summary>
	/// Specified display adapter and all of its state has been reset.
	/// </summary>
	public const int ERROR_GRAPHICS_ADAPTER_WAS_RESET = unchecked((int)0xC0262003);
	/// <summary>
	/// The driver stack doesn't match the expected driver model.
	/// </summary>
	public const int ERROR_GRAPHICS_INVALID_DRIVER_MODEL = unchecked((int)0xC0262004);
	/// <summary>
	/// Present happened but ended up in the changed desktop
	/// </summary>
	public const int ERROR_GRAPHICS_PRESENT_MODE_CHANGED = unchecked((int)0xC0262005);
	/// <summary>
	/// Nothing to present due to desktop occlusion
	/// </summary>
	public const int ERROR_GRAPHICS_PRESENT_OCCLUDED = unchecked((int)0xC0262006);
	/// <summary>
	/// Not able to present due to denial of desktop access
	/// </summary>
	public const int ERROR_GRAPHICS_PRESENT_DENIED = unchecked((int)0xC0262007);
	/// <summary>
	/// Not able to present with color convertion
	/// </summary>
	public const int ERROR_GRAPHICS_CANNOTCOLORCONVERT = unchecked((int)0xC0262008);
	/// <summary>
	/// The kernel driver detected a version mismatch between it and the user mode driver.
	/// </summary>
	public const int ERROR_GRAPHICS_DRIVER_MISMATCH = unchecked((int)0xC0262009);
	/// <summary>
	/// Specified buffer is not big enough to contain entire requested dataset. Partial data populated up to the size of the buffer. Caller needs to provide buffer of size as specified in the partially populated buffer's content (interface specific).
	/// </summary>
	public const int ERROR_GRAPHICS_PARTIAL_DATA_POPULATED = 0x4026200A;
	/// <summary>
	/// Present redirection is disabled (desktop windowing management subsystem is off).
	/// </summary>
	public const int ERROR_GRAPHICS_PRESENT_REDIRECTION_DISABLED = unchecked((int)0xC026200B);
	/// <summary>
	/// Previous exclusive VidPn source owner has released its ownership
	/// </summary>
	public const int ERROR_GRAPHICS_PRESENT_UNOCCLUDED = unchecked((int)0xC026200C);
	/// <summary>
	/// Window DC is not available for presentation
	/// </summary>
	public const int ERROR_GRAPHICS_WINDOWDC_NOT_AVAILABLE = unchecked((int)0xC026200D);
	/// <summary>
	/// Not enough video memory available to complete the operation.
	/// </summary>
	public const int ERROR_GRAPHICS_NO_VIDEO_MEMORY = unchecked((int)0xC0262100);
	/// <summary>
	/// Couldn't probe and lock the underlying memory of an allocation.
	/// </summary>
	public const int ERROR_GRAPHICS_CANT_LOCK_MEMORY = unchecked((int)0xC0262101);
	/// <summary>
	/// The allocation is currently busy.
	/// </summary>
	public const int ERROR_GRAPHICS_ALLOCATION_BUSY = unchecked((int)0xC0262102);
	/// <summary>
	/// An object being referenced has reach the maximum reference count already and can't be reference further.
	/// </summary>
	public const int ERROR_GRAPHICS_TOO_MANY_REFERENCES = unchecked((int)0xC0262103);
	/// <summary>
	/// A problem couldn't be solved due to some currently existing condition. The problem should be tried again later.
	/// </summary>
	public const int ERROR_GRAPHICS_TRY_AGAIN_LATER = unchecked((int)0xC0262104);
	/// <summary>
	/// A problem couldn't be solved due to some currently existing condition. The problem should be tried again immediately.
	/// </summary>
	public const int ERROR_GRAPHICS_TRY_AGAIN_NOW = unchecked((int)0xC0262105);
	/// <summary>
	/// The allocation is invalid.
	/// </summary>
	public const int ERROR_GRAPHICS_ALLOCATION_INVALID = unchecked((int)0xC0262106);
	/// <summary>
	/// No more unswizzling aperture are currently available.
	/// </summary>
	public const int ERROR_GRAPHICS_UNSWIZZLING_APERTURE_UNAVAILABLE = unchecked((int)0xC0262107);
	/// <summary>
	/// The current allocation can't be unswizzled by an aperture.
	/// </summary>
	public const int ERROR_GRAPHICS_UNSWIZZLING_APERTURE_UNSUPPORTED = unchecked((int)0xC0262108);
	/// <summary>
	/// The request failed because a pinned allocation can't be evicted.
	/// </summary>
	public const int ERROR_GRAPHICS_CANT_EVICT_PINNED_ALLOCATION = unchecked((int)0xC0262109);
	/// <summary>
	/// The allocation can't be used from its current segment location for the specified operation.
	/// </summary>
	public const int ERROR_GRAPHICS_INVALID_ALLOCATION_USAGE = unchecked((int)0xC0262110);
	/// <summary>
	/// A locked allocation can't be used in the current command buffer.
	/// </summary>
	public const int ERROR_GRAPHICS_CANT_RENDER_LOCKED_ALLOCATION = unchecked((int)0xC0262111);
	/// <summary>
	/// The allocation being referenced has been closed permanently.
	/// </summary>
	public const int ERROR_GRAPHICS_ALLOCATION_CLOSED = unchecked((int)0xC0262112);
	/// <summary>
	/// An invalid allocation instance is being referenced.
	/// </summary>
	public const int ERROR_GRAPHICS_INVALID_ALLOCATION_INSTANCE = unchecked((int)0xC0262113);
	/// <summary>
	/// An invalid allocation handle is being referenced.
	/// </summary>
	public const int ERROR_GRAPHICS_INVALID_ALLOCATION_HANDLE = unchecked((int)0xC0262114);
	/// <summary>
	/// The allocation being referenced doesn't belong to the current device.
	/// </summary>
	public const int ERROR_GRAPHICS_WRONG_ALLOCATION_DEVICE = unchecked((int)0xC0262115);
	/// <summary>
	/// The specified allocation lost its content.
	/// </summary>
	public const int ERROR_GRAPHICS_ALLOCATION_CONTENT_LOST = unchecked((int)0xC0262116);
	/// <summary>
	/// GPU exception is detected on the given device. The device is not able to be scheduled.
	/// </summary>
	public const int ERROR_GRAPHICS_GPU_EXCEPTION_ON_DEVICE = unchecked((int)0xC0262200);
	/// <summary>
	/// Skip preparation of allocations referenced by the DMA buffer.
	/// </summary>
	public const int ERROR_GRAPHICS_SKIP_ALLOCATION_PREPARATION = 0x40262201;
	/// <summary>
	/// Specified VidPN topology is invalid.
	/// </summary>
	public const int ERROR_GRAPHICS_INVALID_VIDPN_TOPOLOGY = unchecked((int)0xC0262300);
	/// <summary>
	/// Specified VidPN topology is valid but is not supported by this model of the display adapter.
	/// </summary>
	public const int ERROR_GRAPHICS_VIDPN_TOPOLOGY_NOT_SUPPORTED = unchecked((int)0xC0262301);
	/// <summary>
	/// Specified VidPN topology is valid but is not supported by the display adapter at this time, due to current allocation of its resources.
	/// </summary>
	public const int ERROR_GRAPHICS_VIDPN_TOPOLOGY_CURRENTLY_NOT_SUPPORTED = unchecked((int)0xC0262302);
	/// <summary>
	/// Specified VidPN handle is invalid.
	/// </summary>
	public const int ERROR_GRAPHICS_INVALID_VIDPN = unchecked((int)0xC0262303);
	/// <summary>
	/// Specified video present source is invalid.
	/// </summary>
	public const int ERROR_GRAPHICS_INVALID_VIDEO_PRESENT_SOURCE = unchecked((int)0xC0262304);
	/// <summary>
	/// Specified video present target is invalid.
	/// </summary>
	public const int ERROR_GRAPHICS_INVALID_VIDEO_PRESENT_TARGET = unchecked((int)0xC0262305);
	/// <summary>
	/// Specified VidPN modality is not supported (e.g. at least two of the pinned modes are not cofunctional).
	/// </summary>
	public const int ERROR_GRAPHICS_VIDPN_MODALITY_NOT_SUPPORTED = unchecked((int)0xC0262306);
	/// <summary>
	/// No mode is pinned on the specified VidPN source/target.
	/// </summary>
	public const int ERROR_GRAPHICS_MODE_NOT_PINNED = 0x00262307;
	/// <summary>
	/// Specified VidPN source mode set is invalid.
	/// </summary>
	public const int ERROR_GRAPHICS_INVALID_VIDPN_SOURCEMODESET = unchecked((int)0xC0262308);
	/// <summary>
	/// Specified VidPN target mode set is invalid.
	/// </summary>
	public const int ERROR_GRAPHICS_INVALID_VIDPN_TARGETMODESET = unchecked((int)0xC0262309);
	/// <summary>
	/// Specified video signal frequency is invalid.
	/// </summary>
	public const int ERROR_GRAPHICS_INVALID_FREQUENCY = unchecked((int)0xC026230A);
	/// <summary>
	/// Specified video signal active region is invalid.
	/// </summary>
	public const int ERROR_GRAPHICS_INVALID_ACTIVE_REGION = unchecked((int)0xC026230B);
	/// <summary>
	/// Specified video signal total region is invalid.
	/// </summary>
	public const int ERROR_GRAPHICS_INVALID_TOTAL_REGION = unchecked((int)0xC026230C);
	/// <summary>
	/// Specified video present source mode is invalid.
	/// </summary>
	public const int ERROR_GRAPHICS_INVALID_VIDEO_PRESENT_SOURCE_MODE = unchecked((int)0xC0262310);
	/// <summary>
	/// Specified video present target mode is invalid.
	/// </summary>
	public const int ERROR_GRAPHICS_INVALID_VIDEO_PRESENT_TARGET_MODE = unchecked((int)0xC0262311);
	/// <summary>
	/// Pinned mode must remain in the set on VidPN's cofunctional modality enumeration.
	/// </summary>
	public const int ERROR_GRAPHICS_PINNED_MODE_MUST_REMAIN_IN_SET = unchecked((int)0xC0262312);
	/// <summary>
	/// Specified video present path is already in VidPN's topology.
	/// </summary>
	public const int ERROR_GRAPHICS_PATH_ALREADY_IN_TOPOLOGY = unchecked((int)0xC0262313);
	/// <summary>
	/// Specified mode is already in the mode set.
	/// </summary>
	public const int ERROR_GRAPHICS_MODE_ALREADY_IN_MODESET = unchecked((int)0xC0262314);
	/// <summary>
	/// Specified video present source set is invalid.
	/// </summary>
	public const int ERROR_GRAPHICS_INVALID_VIDEOPRESENTSOURCESET = unchecked((int)0xC0262315);
	/// <summary>
	/// Specified video present target set is invalid.
	/// </summary>
	public const int ERROR_GRAPHICS_INVALID_VIDEOPRESENTTARGETSET = unchecked((int)0xC0262316);
	/// <summary>
	/// Specified video present source is already in the video present source set.
	/// </summary>
	public const int ERROR_GRAPHICS_SOURCE_ALREADY_IN_SET = unchecked((int)0xC0262317);
	/// <summary>
	/// Specified video present target is already in the video present target set.
	/// </summary>
	public const int ERROR_GRAPHICS_TARGET_ALREADY_IN_SET = unchecked((int)0xC0262318);
	/// <summary>
	/// Specified VidPN present path is invalid.
	/// </summary>
	public const int ERROR_GRAPHICS_INVALID_VIDPN_PRESENT_PATH = unchecked((int)0xC0262319);
	/// <summary>
	/// Miniport has no recommendation for augmentation of the specified VidPN's topology.
	/// </summary>
	public const int ERROR_GRAPHICS_NO_RECOMMENDED_VIDPN_TOPOLOGY = unchecked((int)0xC026231A);
	/// <summary>
	/// Specified monitor frequency range set is invalid.
	/// </summary>
	public const int ERROR_GRAPHICS_INVALID_MONITOR_FREQUENCYRANGESET = unchecked((int)0xC026231B);
	/// <summary>
	/// Specified monitor frequency range is invalid.
	/// </summary>
	public const int ERROR_GRAPHICS_INVALID_MONITOR_FREQUENCYRANGE = unchecked((int)0xC026231C);
	/// <summary>
	/// Specified frequency range is not in the specified monitor frequency range set.
	/// </summary>
	public const int ERROR_GRAPHICS_FREQUENCYRANGE_NOT_IN_SET = unchecked((int)0xC026231D);
	/// <summary>
	/// Specified mode set does not specify preference for one of its modes.
	/// </summary>
	public const int ERROR_GRAPHICS_NO_PREFERRED_MODE = 0x0026231E;
	/// <summary>
	/// Specified frequency range is already in the specified monitor frequency range set.
	/// </summary>
	public const int ERROR_GRAPHICS_FREQUENCYRANGE_ALREADY_IN_SET = unchecked((int)0xC026231F);
	/// <summary>
	/// Specified mode set is stale. Please reacquire the new mode set.
	/// </summary>
	public const int ERROR_GRAPHICS_STALE_MODESET = unchecked((int)0xC0262320);
	/// <summary>
	/// Specified monitor source mode set is invalid.
	/// </summary>
	public const int ERROR_GRAPHICS_INVALID_MONITOR_SOURCEMODESET = unchecked((int)0xC0262321);
	/// <summary>
	/// Specified monitor source mode is invalid.
	/// </summary>
	public const int ERROR_GRAPHICS_INVALID_MONITOR_SOURCE_MODE = unchecked((int)0xC0262322);
	/// <summary>
	/// Miniport does not have any recommendation regarding the request to provide a functional VidPN given the current display adapter configuration.
	/// </summary>
	public const int ERROR_GRAPHICS_NO_RECOMMENDED_FUNCTIONAL_VIDPN = unchecked((int)0xC0262323);
	/// <summary>
	/// ID of the specified mode is already used by another mode in the set.
	/// </summary>
	public const int ERROR_GRAPHICS_MODE_ID_MUST_BE_UNIQUE = unchecked((int)0xC0262324);
	/// <summary>
	/// System failed to determine a mode that is supported by both the display adapter and the monitor connected to it.
	/// </summary>
	public const int ERROR_GRAPHICS_EMPTY_ADAPTER_MONITOR_MODE_SUPPORT_INTERSECTION = unchecked((int)0xC0262325);
	/// <summary>
	/// Number of video present targets must be greater than or equal to the number of video present sources.
	/// </summary>
	public const int ERROR_GRAPHICS_VIDEO_PRESENT_TARGETS_LESS_THAN_SOURCES = unchecked((int)0xC0262326);
	/// <summary>
	/// Specified present path is not in VidPN's topology.
	/// </summary>
	public const int ERROR_GRAPHICS_PATH_NOT_IN_TOPOLOGY = unchecked((int)0xC0262327);
	/// <summary>
	/// Display adapter must have at least one video present source.
	/// </summary>
	public const int ERROR_GRAPHICS_ADAPTER_MUST_HAVE_AT_LEAST_ONE_SOURCE = unchecked((int)0xC0262328);
	/// <summary>
	/// Display adapter must have at least one video present target.
	/// </summary>
	public const int ERROR_GRAPHICS_ADAPTER_MUST_HAVE_AT_LEAST_ONE_TARGET = unchecked((int)0xC0262329);
	/// <summary>
	/// Specified monitor descriptor set is invalid.
	/// </summary>
	public const int ERROR_GRAPHICS_INVALID_MONITORDESCRIPTORSET = unchecked((int)0xC026232A);
	/// <summary>
	/// Specified monitor descriptor is invalid.
	/// </summary>
	public const int ERROR_GRAPHICS_INVALID_MONITORDESCRIPTOR = unchecked((int)0xC026232B);
	/// <summary>
	/// Specified descriptor is not in the specified monitor descriptor set.
	/// </summary>
	public const int ERROR_GRAPHICS_MONITORDESCRIPTOR_NOT_IN_SET = unchecked((int)0xC026232C);
	/// <summary>
	/// Specified descriptor is already in the specified monitor descriptor set.
	/// </summary>
	public const int ERROR_GRAPHICS_MONITORDESCRIPTOR_ALREADY_IN_SET = unchecked((int)0xC026232D);
	/// <summary>
	/// ID of the specified monitor descriptor is already used by another descriptor in the set.
	/// </summary>
	public const int ERROR_GRAPHICS_MONITORDESCRIPTOR_ID_MUST_BE_UNIQUE = unchecked((int)0xC026232E);
	/// <summary>
	/// Specified video present target subset type is invalid.
	/// </summary>
	public const int ERROR_GRAPHICS_INVALID_VIDPN_TARGET_SUBSET_TYPE = unchecked((int)0xC026232F);
	/// <summary>
	/// Two or more of the specified resources are not related to each other, as defined by the interface semantics.
	/// </summary>
	public const int ERROR_GRAPHICS_RESOURCES_NOT_RELATED = unchecked((int)0xC0262330);
	/// <summary>
	/// ID of the specified video present source is already used by another source in the set.
	/// </summary>
	public const int ERROR_GRAPHICS_SOURCE_ID_MUST_BE_UNIQUE = unchecked((int)0xC0262331);
	/// <summary>
	/// ID of the specified video present target is already used by another target in the set.
	/// </summary>
	public const int ERROR_GRAPHICS_TARGET_ID_MUST_BE_UNIQUE = unchecked((int)0xC0262332);
	/// <summary>
	/// Specified VidPN source cannot be used because there is no available VidPN target to connect it to.
	/// </summary>
	public const int ERROR_GRAPHICS_NO_AVAILABLE_VIDPN_TARGET = unchecked((int)0xC0262333);
	/// <summary>
	/// Newly arrived monitor could not be associated with a display adapter.
	/// </summary>
	public const int ERROR_GRAPHICS_MONITOR_COULD_NOT_BE_ASSOCIATED_WITH_ADAPTER = unchecked((int)0xC0262334);
	/// <summary>
	/// Display adapter in question does not have an associated VidPN manager.
	/// </summary>
	public const int ERROR_GRAPHICS_NO_VIDPNMGR = unchecked((int)0xC0262335);
	/// <summary>
	/// VidPN manager of the display adapter in question does not have an active VidPN.
	/// </summary>
	public const int ERROR_GRAPHICS_NO_ACTIVE_VIDPN = unchecked((int)0xC0262336);
	/// <summary>
	/// Specified VidPN topology is stale. Please reacquire the new topology.
	/// </summary>
	public const int ERROR_GRAPHICS_STALE_VIDPN_TOPOLOGY = unchecked((int)0xC0262337);
	/// <summary>
	/// There is no monitor connected on the specified video present target.
	/// </summary>
	public const int ERROR_GRAPHICS_MONITOR_NOT_CONNECTED = unchecked((int)0xC0262338);
	/// <summary>
	/// Specified source is not part of the specified VidPN's topology.
	/// </summary>
	public const int ERROR_GRAPHICS_SOURCE_NOT_IN_TOPOLOGY = unchecked((int)0xC0262339);
	/// <summary>
	/// Specified primary surface size is invalid.
	/// </summary>
	public const int ERROR_GRAPHICS_INVALID_PRIMARYSURFACE_SIZE = unchecked((int)0xC026233A);
	/// <summary>
	/// Specified visible region size is invalid.
	/// </summary>
	public const int ERROR_GRAPHICS_INVALID_VISIBLEREGION_SIZE = unchecked((int)0xC026233B);
	/// <summary>
	/// Specified stride is invalid.
	/// </summary>
	public const int ERROR_GRAPHICS_INVALID_STRIDE = unchecked((int)0xC026233C);
	/// <summary>
	/// Specified pixel format is invalid.
	/// </summary>
	public const int ERROR_GRAPHICS_INVALID_PIXELFORMAT = unchecked((int)0xC026233D);
	/// <summary>
	/// Specified color basis is invalid.
	/// </summary>
	public const int ERROR_GRAPHICS_INVALID_COLORBASIS = unchecked((int)0xC026233E);
	/// <summary>
	/// Specified pixel value access mode is invalid.
	/// </summary>
	public const int ERROR_GRAPHICS_INVALID_PIXELVALUEACCESSMODE = unchecked((int)0xC026233F);
	/// <summary>
	/// Specified target is not part of the specified VidPN's topology.
	/// </summary>
	public const int ERROR_GRAPHICS_TARGET_NOT_IN_TOPOLOGY = unchecked((int)0xC0262340);
	/// <summary>
	/// Failed to acquire display mode management interface.
	/// </summary>
	public const int ERROR_GRAPHICS_NO_DISPLAY_MODE_MANAGEMENT_SUPPORT = unchecked((int)0xC0262341);
	/// <summary>
	/// Specified VidPN source is already owned by a DMM client and cannot be used until that client releases it.
	/// </summary>
	public const int ERROR_GRAPHICS_VIDPN_SOURCE_IN_USE = unchecked((int)0xC0262342);
	/// <summary>
	/// Specified VidPN is active and cannot be accessed.
	/// </summary>
	public const int ERROR_GRAPHICS_CANT_ACCESS_ACTIVE_VIDPN = unchecked((int)0xC0262343);
	/// <summary>
	/// Specified VidPN present path importance ordinal is invalid.
	/// </summary>
	public const int ERROR_GRAPHICS_INVALID_PATH_IMPORTANCE_ORDINAL = unchecked((int)0xC0262344);
	/// <summary>
	/// Specified VidPN present path content geometry transformation is invalid.
	/// </summary>
	public const int ERROR_GRAPHICS_INVALID_PATH_CONTENT_GEOMETRY_TRANSFORMATION = unchecked((int)0xC0262345);
	/// <summary>
	/// Specified content geometry transformation is not supported on the respective VidPN present path.
	/// </summary>
	public const int ERROR_GRAPHICS_PATH_CONTENT_GEOMETRY_TRANSFORMATION_NOT_SUPPORTED = unchecked((int)0xC0262346);
	/// <summary>
	/// Specified gamma ramp is invalid.
	/// </summary>
	public const int ERROR_GRAPHICS_INVALID_GAMMA_RAMP = unchecked((int)0xC0262347);
	/// <summary>
	/// Specified gamma ramp is not supported on the respective VidPN present path.
	/// </summary>
	public const int ERROR_GRAPHICS_GAMMA_RAMP_NOT_SUPPORTED = unchecked((int)0xC0262348);
	/// <summary>
	/// Multi-sampling is not supported on the respective VidPN present path.
	/// </summary>
	public const int ERROR_GRAPHICS_MULTISAMPLING_NOT_SUPPORTED = unchecked((int)0xC0262349);
	/// <summary>
	/// Specified mode is not in the specified mode set.
	/// </summary>
	public const int ERROR_GRAPHICS_MODE_NOT_IN_MODESET = unchecked((int)0xC026234A);
	/// <summary>
	/// Specified data set (e.g. mode set, frequency range set, descriptor set, topology, etc.) is empty.
	/// </summary>
	public const int ERROR_GRAPHICS_DATASET_IS_EMPTY = 0x0026234B;
	/// <summary>
	/// Specified data set (e.g. mode set, frequency range set, descriptor set, topology, etc.) does not contain any more elements.
	/// </summary>
	public const int ERROR_GRAPHICS_NO_MORE_ELEMENTS_IN_DATASET = 0x0026234C;
	/// <summary>
	/// Specified VidPN topology recommendation reason is invalid.
	/// </summary>
	public const int ERROR_GRAPHICS_INVALID_VIDPN_TOPOLOGY_RECOMMENDATION_REASON = unchecked((int)0xC026234D);
	/// <summary>
	/// Specified VidPN present path content type is invalid.
	/// </summary>
	public const int ERROR_GRAPHICS_INVALID_PATH_CONTENT_TYPE = unchecked((int)0xC026234E);
	/// <summary>
	/// Specified VidPN present path copy protection type is invalid.
	/// </summary>
	public const int ERROR_GRAPHICS_INVALID_COPYPROTECTION_TYPE = unchecked((int)0xC026234F);
	/// <summary>
	/// No more than one unassigned mode set can exist at any given time for a given VidPN source/target.
	/// </summary>
	public const int ERROR_GRAPHICS_UNASSIGNED_MODESET_ALREADY_EXISTS = unchecked((int)0xC0262350);
	/// <summary>
	/// Specified content transformation is not pinned on the specified VidPN present path.
	/// </summary>
	public const int ERROR_GRAPHICS_PATH_CONTENT_GEOMETRY_TRANSFORMATION_NOT_PINNED = 0x00262351;
	/// <summary>
	/// Specified scanline ordering type is invalid.
	/// </summary>
	public const int ERROR_GRAPHICS_INVALID_SCANLINE_ORDERING = unchecked((int)0xC0262352);
	/// <summary>
	/// Topology changes are not allowed for the specified VidPN.
	/// </summary>
	public const int ERROR_GRAPHICS_TOPOLOGY_CHANGES_NOT_ALLOWED = unchecked((int)0xC0262353);
	/// <summary>
	/// All available importance ordinals are already used in specified topology.
	/// </summary>
	public const int ERROR_GRAPHICS_NO_AVAILABLE_IMPORTANCE_ORDINALS = unchecked((int)0xC0262354);
	/// <summary>
	/// Specified primary surface has a different private format attribute than the current primary surface
	/// </summary>
	public const int ERROR_GRAPHICS_INCOMPATIBLE_PRIVATE_FORMAT = unchecked((int)0xC0262355);
	/// <summary>
	/// Specified mode pruning algorithm is invalid
	/// </summary>
	public const int ERROR_GRAPHICS_INVALID_MODE_PRUNING_ALGORITHM = unchecked((int)0xC0262356);
	/// <summary>
	/// Specified monitor capability origin is invalid.
	/// </summary>
	public const int ERROR_GRAPHICS_INVALID_MONITOR_CAPABILITY_ORIGIN = unchecked((int)0xC0262357);
	/// <summary>
	/// Specified monitor frequency range constraint is invalid.
	/// </summary>
	public const int ERROR_GRAPHICS_INVALID_MONITOR_FREQUENCYRANGE_CONSTRAINT = unchecked((int)0xC0262358);
	/// <summary>
	/// Maximum supported number of present paths has been reached.
	/// </summary>
	public const int ERROR_GRAPHICS_MAX_NUM_PATHS_REACHED = unchecked((int)0xC0262359);
	/// <summary>
	/// Miniport requested that augmentation be canceled for the specified source of the specified VidPN's topology.
	/// </summary>
	public const int ERROR_GRAPHICS_CANCEL_VIDPN_TOPOLOGY_AUGMENTATION = unchecked((int)0xC026235A);
	/// <summary>
	/// Specified client type was not recognized.
	/// </summary>
	public const int ERROR_GRAPHICS_INVALID_CLIENT_TYPE = unchecked((int)0xC026235B);
	/// <summary>
	/// Client VidPN is not set on this adapter (e.g. no user mode initiated mode changes took place on this adapter yet).
	/// </summary>
	public const int ERROR_GRAPHICS_CLIENTVIDPN_NOT_SET = unchecked((int)0xC026235C);
	/// <summary>
	/// Specified display adapter child device already has an external device connected to it.
	/// </summary>
	public const int ERROR_GRAPHICS_SPECIFIED_CHILD_ALREADY_CONNECTED = unchecked((int)0xC0262400);
	/// <summary>
	/// Specified display adapter child device does not support descriptor exposure.
	/// </summary>
	public const int ERROR_GRAPHICS_CHILD_DESCRIPTOR_NOT_SUPPORTED = unchecked((int)0xC0262401);
	/// <summary>
	/// Child device presence was not reliably detected.
	/// </summary>
	public const int ERROR_GRAPHICS_UNKNOWN_CHILD_STATUS = 0x4026242F;
	/// <summary>
	/// The display adapter is not linked to any other adapters.
	/// </summary>
	public const int ERROR_GRAPHICS_NOT_A_LINKED_ADAPTER = unchecked((int)0xC0262430);
	/// <summary>
	/// Lead adapter in a linked configuration was not enumerated yet.
	/// </summary>
	public const int ERROR_GRAPHICS_LEADLINK_NOT_ENUMERATED = unchecked((int)0xC0262431);
	/// <summary>
	/// Some chain adapters in a linked configuration were not enumerated yet.
	/// </summary>
	public const int ERROR_GRAPHICS_CHAINLINKS_NOT_ENUMERATED = unchecked((int)0xC0262432);
	/// <summary>
	/// The chain of linked adapters is not ready to start because of an unknown failure.
	/// </summary>
	public const int ERROR_GRAPHICS_ADAPTER_CHAIN_NOT_READY = unchecked((int)0xC0262433);
	/// <summary>
	/// An attempt was made to start a lead link display adapter when the chain links were not started yet.
	/// </summary>
	public const int ERROR_GRAPHICS_CHAINLINKS_NOT_STARTED = unchecked((int)0xC0262434);
	/// <summary>
	/// An attempt was made to power up a lead link display adapter when the chain links were powered down.
	/// </summary>
	public const int ERROR_GRAPHICS_CHAINLINKS_NOT_POWERED_ON = unchecked((int)0xC0262435);
	/// <summary>
	/// The adapter link was found to be in an inconsistent state. Not all adapters are in an expected PNP/Power state.
	/// </summary>
	public const int ERROR_GRAPHICS_INCONSISTENT_DEVICE_LINK_STATE = unchecked((int)0xC0262436);
	/// <summary>
	/// Starting the leadlink adapter has been deferred temporarily.
	/// </summary>
	public const int ERROR_GRAPHICS_LEADLINK_START_DEFERRED = 0x40262437;
	/// <summary>
	/// The driver trying to start is not the same as the driver for the POSTed display adapter.
	/// </summary>
	public const int ERROR_GRAPHICS_NOT_POST_DEVICE_DRIVER = unchecked((int)0xC0262438);
	/// <summary>
	/// The display adapter is being polled for children too frequently at the same polling level.
	/// </summary>
	public const int ERROR_GRAPHICS_POLLING_TOO_FREQUENTLY = 0x40262439;
	/// <summary>
	/// Starting the adapter has been deferred temporarily.
	/// </summary>
	public const int ERROR_GRAPHICS_START_DEFERRED = 0x4026243A;
	/// <summary>
	/// An operation is being attempted that requires the display adapter to be in a quiescent state.
	/// </summary>
	public const int ERROR_GRAPHICS_ADAPTER_ACCESS_NOT_EXCLUDED = unchecked((int)0xC026243B);
	/// <summary>
	/// The driver does not support OPM.
	/// </summary>
	public const int ERROR_GRAPHICS_OPM_NOT_SUPPORTED = unchecked((int)0xC0262500);
	/// <summary>
	/// The driver does not support COPP.
	/// </summary>
	public const int ERROR_GRAPHICS_COPP_NOT_SUPPORTED = unchecked((int)0xC0262501);
	/// <summary>
	/// The driver does not support UAB.
	/// </summary>
	public const int ERROR_GRAPHICS_UAB_NOT_SUPPORTED = unchecked((int)0xC0262502);
	/// <summary>
	/// The specified encrypted parameters are invalid.
	/// </summary>
	public const int ERROR_GRAPHICS_OPM_INVALID_ENCRYPTED_PARAMETERS = unchecked((int)0xC0262503);
	/// <summary>
	/// The GDI display device passed to this function does not have any active video outputs.
	/// </summary>
	public const int ERROR_GRAPHICS_OPM_NO_VIDEO_OUTPUTS_EXIST = unchecked((int)0xC0262505);
	/// <summary>
	/// An internal error caused this operation to fail.
	/// </summary>
	public const int ERROR_GRAPHICS_OPM_INTERNAL_ERROR = unchecked((int)0xC026250B);
	/// <summary>
	/// The function failed because the caller passed in an invalid OPM user mode handle.
	/// </summary>
	public const int ERROR_GRAPHICS_OPM_INVALID_HANDLE = unchecked((int)0xC026250C);
	/// <summary>
	/// A certificate could not be returned because the certificate buffer passed to the function was too small.
	/// </summary>
	public const int ERROR_GRAPHICS_PVP_INVALID_CERTIFICATE_LENGTH = unchecked((int)0xC026250E);
	/// <summary>
	/// A video output could not be created because the frame buffer is in spanning mode.
	/// </summary>
	public const int ERROR_GRAPHICS_OPM_SPANNING_MODE_ENABLED = unchecked((int)0xC026250F);
	/// <summary>
	/// A video output could not be created because the frame buffer is in theater mode.
	/// </summary>
	public const int ERROR_GRAPHICS_OPM_THEATER_MODE_ENABLED = unchecked((int)0xC0262510);
	/// <summary>
	/// The function failed because the display adapter's Hardware Functionality Scan failed to validate the graphics hardware.
	/// </summary>
	public const int ERROR_GRAPHICS_PVP_HFS_FAILED = unchecked((int)0xC0262511);
	/// <summary>
	/// The HDCP System Renewability Message passed to this function did not comply with section 5 of the HDCP 1.1 specification.
	/// </summary>
	public const int ERROR_GRAPHICS_OPM_INVALID_SRM = unchecked((int)0xC0262512);
	/// <summary>
	/// The video output cannot enable the High-bandwidth Digital Content Protection (HDCP) System because it does not support HDCP.
	/// </summary>
	public const int ERROR_GRAPHICS_OPM_OUTPUT_DOES_NOT_SUPPORT_HDCP = unchecked((int)0xC0262513);
	/// <summary>
	/// The video output cannot enable Analog Copy Protection (ACP) because it does not support ACP.
	/// </summary>
	public const int ERROR_GRAPHICS_OPM_OUTPUT_DOES_NOT_SUPPORT_ACP = unchecked((int)0xC0262514);
	/// <summary>
	/// The video output cannot enable the Content Generation Management System Analog (CGMS-A) protection technology because it does not support CGMS-A.
	/// </summary>
	public const int ERROR_GRAPHICS_OPM_OUTPUT_DOES_NOT_SUPPORT_CGMSA = unchecked((int)0xC0262515);
	/// <summary>
	/// The IOPMVideoOutput::GetInformation method cannot return the version of the SRM being used because the application never successfully passed an SRM to the video output.
	/// </summary>
	public const int ERROR_GRAPHICS_OPM_HDCP_SRM_NEVER_SET = unchecked((int)0xC0262516);
	/// <summary>
	/// The IOPMVideoOutput::Configure method cannot enable the specified output protection technology because the output's screen resolution is too high.
	/// </summary>
	public const int ERROR_GRAPHICS_OPM_RESOLUTION_TOO_HIGH = unchecked((int)0xC0262517);
	/// <summary>
	/// The IOPMVideoOutput::Configure method cannot enable HDCP because the display adapter's HDCP hardware is already being used by other physical outputs.
	/// </summary>
	public const int ERROR_GRAPHICS_OPM_ALL_HDCP_HARDWARE_ALREADY_IN_USE = unchecked((int)0xC0262518);
	/// <summary>
	/// The operating system asynchronously destroyed this OPM video output because the operating system's state changed. This error typically occurs because the monitor PDO associated with this video output was removed, the monitor PDO associated with this video output was stopped, the video output's session became a non-console session or the video output's desktop became an inactive desktop.
	/// </summary>
	public const int ERROR_GRAPHICS_OPM_VIDEO_OUTPUT_NO_LONGER_EXISTS = unchecked((int)0xC026251A);
	/// <summary>
	/// The method failed because the session is changing its type. No IOPMVideoOutput methods can be called when a session is changing its type. There are currently three types of sessions: console, disconnected and remote.
	/// </summary>
	public const int ERROR_GRAPHICS_OPM_SESSION_TYPE_CHANGE_IN_PROGRESS = unchecked((int)0xC026251B);
	/// <summary>
	/// Either the IOPMVideoOutput::COPPCompatibleGetInformation, IOPMVideoOutput::GetInformation, or IOPMVideoOutput::Configure method failed. This error is returned when the caller tries to use a COPP specific command while the video output has OPM semantics only.
	/// </summary>
	public const int ERROR_GRAPHICS_OPM_VIDEO_OUTPUT_DOES_NOT_HAVE_COPP_SEMANTICS = unchecked((int)0xC026251C);
	/// <summary>
	/// The IOPMVideoOutput::GetInformation and IOPMVideoOutput::COPPCompatibleGetInformation methods return this error if the passed in sequence number is not the expected sequence number or the passed in OMAC value is invalid.
	/// </summary>
	public const int ERROR_GRAPHICS_OPM_INVALID_INFORMATION_REQUEST = unchecked((int)0xC026251D);
	/// <summary>
	/// The method failed because an unexpected error occurred inside of a display driver.
	/// </summary>
	public const int ERROR_GRAPHICS_OPM_DRIVER_INTERNAL_ERROR = unchecked((int)0xC026251E);
	/// <summary>
	/// Either the IOPMVideoOutput::COPPCompatibleGetInformation, IOPMVideoOutput::GetInformation, or IOPMVideoOutput::Configure method failed. This error is returned when the caller tries to use an OPM specific command while the video output has COPP semantics only.
	/// </summary>
	public const int ERROR_GRAPHICS_OPM_VIDEO_OUTPUT_DOES_NOT_HAVE_OPM_SEMANTICS = unchecked((int)0xC026251F);
	/// <summary>
	/// The IOPMVideoOutput::COPPCompatibleGetInformation or IOPMVideoOutput::Configure method failed because the display driver does not support the OPM_GET_ACP_AND_CGMSA_SIGNALING and OPM_SET_ACP_AND_CGMSA_SIGNALING GUIDs.
	/// </summary>
	public const int ERROR_GRAPHICS_OPM_SIGNALING_NOT_SUPPORTED = unchecked((int)0xC0262520);
	/// <summary>
	/// The IOPMVideoOutput::Configure function returns this error code if the passed in sequence number is not the expected sequence number or the passed in OMAC value is invalid.
	/// </summary>
	public const int ERROR_GRAPHICS_OPM_INVALID_CONFIGURATION_REQUEST = unchecked((int)0xC0262521);
	/// <summary>
	/// The monitor connected to the specified video output does not have an I2C bus.
	/// </summary>
	public const int ERROR_GRAPHICS_I2C_NOT_SUPPORTED = unchecked((int)0xC0262580);
	/// <summary>
	/// No device on the I2C bus has the specified address.
	/// </summary>
	public const int ERROR_GRAPHICS_I2C_DEVICE_DOES_NOT_EXIST = unchecked((int)0xC0262581);
	/// <summary>
	/// An error occurred while transmitting data to the device on the I2C bus.
	/// </summary>
	public const int ERROR_GRAPHICS_I2C_ERROR_TRANSMITTING_DATA = unchecked((int)0xC0262582);
	/// <summary>
	/// An error occurred while receiving data from the device on the I2C bus.
	/// </summary>
	public const int ERROR_GRAPHICS_I2C_ERROR_RECEIVING_DATA = unchecked((int)0xC0262583);
	/// <summary>
	/// The monitor does not support the specified VCP code.
	/// </summary>
	public const int ERROR_GRAPHICS_DDCCI_VCP_NOT_SUPPORTED = unchecked((int)0xC0262584);
	/// <summary>
	/// The data received from the monitor is invalid.
	/// </summary>
	public const int ERROR_GRAPHICS_DDCCI_INVALID_DATA = unchecked((int)0xC0262585);
	/// <summary>
	/// The function failed because a monitor returned an invalid Timing Status byte when the operating system used the DDC/CI Get Timing Report &amp; Timing Message command to get a timing report from a monitor.
	/// </summary>
	public const int ERROR_GRAPHICS_DDCCI_MONITOR_RETURNED_INVALID_TIMING_STATUS_BYTE = unchecked((int)0xC0262586);
	/// <summary>
	/// The monitor returned a DDC/CI capabilities string which did not comply with the ACCESS.bus 3.0, DDC/CI 1.1, or MCCS 2 Revision 1 specification.
	/// </summary>
	public const int ERROR_GRAPHICS_MCA_INVALID_CAPABILITIES_STRING = unchecked((int)0xC0262587);
	/// <summary>
	/// An internal Monitor Configuration API error occurred.
	/// </summary>
	public const int ERROR_GRAPHICS_MCA_INTERNAL_ERROR = unchecked((int)0xC0262588);
	/// <summary>
	/// An operation failed because a DDC/CI message had an invalid value in its command field.
	/// </summary>
	public const int ERROR_GRAPHICS_DDCCI_INVALID_MESSAGE_COMMAND = unchecked((int)0xC0262589);
	/// <summary>
	/// An error occurred because the field length of a DDC/CI message contained an invalid value.
	/// </summary>
	public const int ERROR_GRAPHICS_DDCCI_INVALID_MESSAGE_LENGTH = unchecked((int)0xC026258A);
	/// <summary>
	/// An error occurred because the checksum field in a DDC/CI message did not match the message's computed checksum value. This error implies that the data was corrupted while it was being transmitted from a monitor to a computer.
	/// </summary>
	public const int ERROR_GRAPHICS_DDCCI_INVALID_MESSAGE_CHECKSUM = unchecked((int)0xC026258B);
	/// <summary>
	/// This function failed because an invalid monitor handle was passed to it.
	/// </summary>
	public const int ERROR_GRAPHICS_INVALID_PHYSICAL_MONITOR_HANDLE = unchecked((int)0xC026258C);
	/// <summary>
	/// The operating system asynchronously destroyed the monitor which corresponds to this handle because the operating system's state changed. This error typically occurs because the monitor PDO associated with this handle was removed, the monitor PDO associated with this handle was stopped, or a display mode change occurred. A display mode change occurs when windows sends a WM_DISPLAYCHANGE windows message to applications.
	/// </summary>
	public const int ERROR_GRAPHICS_MONITOR_NO_LONGER_EXISTS = unchecked((int)0xC026258D);
	/// <summary>
	/// A continuous VCP code's current value is greater than its maximum value. This error code indicates that a monitor returned an invalid value.
	/// </summary>
	public const int ERROR_GRAPHICS_DDCCI_CURRENT_CURRENT_VALUE_GREATER_THAN_MAXIMUM_VALUE = unchecked((int)0xC02625D8);
	/// <summary>
	/// The monitor's VCP Version (0xDF) VCP code returned an invalid version value.
	/// </summary>
	public const int ERROR_GRAPHICS_MCA_INVALID_VCP_VERSION = unchecked((int)0xC02625D9);
	/// <summary>
	/// The monitor does not comply with the MCCS specification it claims to support.
	/// </summary>
	public const int ERROR_GRAPHICS_MCA_MONITOR_VIOLATES_MCCS_SPECIFICATION = unchecked((int)0xC02625DA);
	/// <summary>
	/// The MCCS version in a monitor's mccs_ver capability does not match the MCCS version the monitor reports when the VCP Version (0xDF) VCP code is used.
	/// </summary>
	public const int ERROR_GRAPHICS_MCA_MCCS_VERSION_MISMATCH = unchecked((int)0xC02625DB);
	/// <summary>
	/// The Monitor Configuration API only works with monitors which support the MCCS 1.0 specification, MCCS 2.0 specification or the MCCS 2.0 Revision 1 specification.
	/// </summary>
	public const int ERROR_GRAPHICS_MCA_UNSUPPORTED_MCCS_VERSION = unchecked((int)0xC02625DC);
	/// <summary>
	/// The monitor returned an invalid monitor technology type. CRT, Plasma and LCD (TFT) are examples of monitor technology types. This error implies that the monitor violated the MCCS 2.0 or MCCS 2.0 Revision 1 specification.
	/// </summary>
	public const int ERROR_GRAPHICS_MCA_INVALID_TECHNOLOGY_TYPE_RETURNED = unchecked((int)0xC02625DE);
	/// <summary>
	/// SetMonitorColorTemperature()'s caller passed a color temperature to it which the current monitor did not support. This error implies that the monitor violated the MCCS 2.0 or MCCS 2.0 Revision 1 specification.
	/// </summary>
	public const int ERROR_GRAPHICS_MCA_UNSUPPORTED_COLOR_TEMPERATURE = unchecked((int)0xC02625DF);
	/// <summary>
	/// This function can only be used if a program is running in the local console session. It cannot be used if the program is running on a remote desktop session or on a terminal server session.
	/// </summary>
	public const int ERROR_GRAPHICS_ONLY_CONSOLE_SESSION_SUPPORTED = unchecked((int)0xC02625E0);
	/// <summary>
	/// This function cannot find an actual GDI display device which corresponds to the specified GDI display device name.
	/// </summary>
	public const int ERROR_GRAPHICS_NO_DISPLAY_DEVICE_CORRESPONDS_TO_NAME = unchecked((int)0xC02625E1);
	/// <summary>
	/// The function failed because the specified GDI display device was not attached to the Windows desktop.
	/// </summary>
	public const int ERROR_GRAPHICS_DISPLAY_DEVICE_NOT_ATTACHED_TO_DESKTOP = unchecked((int)0xC02625E2);
	/// <summary>
	/// This function does not support GDI mirroring display devices because GDI mirroring display devices do not have any physical monitors associated with them.
	/// </summary>
	public const int ERROR_GRAPHICS_MIRRORING_DEVICES_NOT_SUPPORTED = unchecked((int)0xC02625E3);
	/// <summary>
	/// The function failed because an invalid pointer parameter was passed to it. A pointer parameter is invalid if it is <strong>NULL</strong>, points to an invalid address, points to a kernel mode address, or is not correctly aligned.
	/// </summary>
	public const int ERROR_GRAPHICS_INVALID_POINTER = unchecked((int)0xC02625E4);
	/// <summary>
	/// The function failed because the specified GDI device did not have any monitors associated with it.
	/// </summary>
	public const int ERROR_GRAPHICS_NO_MONITORS_CORRESPOND_TO_DISPLAY_DEVICE = unchecked((int)0xC02625E5);
	/// <summary>
	/// An array passed to the function cannot hold all of the data that the function must copy into the array.
	/// </summary>
	public const int ERROR_GRAPHICS_PARAMETER_ARRAY_TOO_SMALL = unchecked((int)0xC02625E6);
	/// <summary>
	/// An internal error caused an operation to fail.
	/// </summary>
	public const int ERROR_GRAPHICS_INTERNAL_ERROR = unchecked((int)0xC02625E7);
	/// <summary>
	/// The function failed because the current session is changing its type. This function cannot be called when the current session is changing its type. There are currently three types of sessions: console, disconnected and remote.
	/// </summary>
	public const int ERROR_GRAPHICS_SESSION_TYPE_CHANGE_IN_PROGRESS = unchecked((int)0xC02605E8);
	#endregion

	#region TPM, PLA, FVE
	/// <summary>
	/// This is an error mask to convert TPM hardware errors to win errors.
	/// </summary>
	public const int TPM_E_ERROR_MASK = unchecked((int)0x80280000);
	/// <summary>
	/// Authentication failed.
	/// </summary>
	public const int TPM_E_AUTHFAIL = unchecked((int)0x80280001);
	/// <summary>
	/// The index to a PCR, DIR or other register is incorrect.
	/// </summary>
	public const int TPM_E_BADINDEX = unchecked((int)0x80280002);
	/// <summary>
	/// One or more parameter is bad.
	/// </summary>
	public const int TPM_E_BAD_PARAMETER = unchecked((int)0x80280003);
	/// <summary>
	/// An operation completed successfully but the auditing of that operation failed.
	/// </summary>
	public const int TPM_E_AUDITFAILURE = unchecked((int)0x80280004);
	/// <summary>
	/// The clear disable flag is set and all clear operations now require physical access.
	/// </summary>
	public const int TPM_E_CLEAR_DISABLED = unchecked((int)0x80280005);
	/// <summary>
	/// Activate the Trusted Platform Module (TPM).
	/// </summary>
	public const int TPM_E_DEACTIVATED = unchecked((int)0x80280006);
	/// <summary>
	/// Enable the Trusted Platform Module (TPM).
	/// </summary>
	public const int TPM_E_DISABLED = unchecked((int)0x80280007);
	/// <summary>
	/// The target command has been disabled.
	/// </summary>
	public const int TPM_E_DISABLED_CMD = unchecked((int)0x80280008);
	/// <summary>
	/// The operation failed.
	/// </summary>
	public const int TPM_E_FAIL = unchecked((int)0x80280009);
	/// <summary>
	/// The ordinal was unknown or inconsistent.
	/// </summary>
	public const int TPM_E_BAD_ORDINAL = unchecked((int)0x8028000A);
	/// <summary>
	/// The ability to install an owner is disabled.
	/// </summary>
	public const int TPM_E_INSTALL_DISABLED = unchecked((int)0x8028000B);
	/// <summary>
	/// The key handle cannot be interpreted.
	/// </summary>
	public const int TPM_E_INVALID_KEYHANDLE = unchecked((int)0x8028000C);
	/// <summary>
	/// The key handle points to an invalid key.
	/// </summary>
	public const int TPM_E_KEYNOTFOUND = unchecked((int)0x8028000D);
	/// <summary>
	/// Unacceptable encryption scheme.
	/// </summary>
	public const int TPM_E_INAPPROPRIATE_ENC = unchecked((int)0x8028000E);
	/// <summary>
	/// Migration authorization failed.
	/// </summary>
	public const int TPM_E_MIGRATEFAIL = unchecked((int)0x8028000F);
	/// <summary>
	/// PCR information could not be interpreted.
	/// </summary>
	public const int TPM_E_INVALID_PCR_INFO = unchecked((int)0x80280010);
	/// <summary>
	/// No room to load key.
	/// </summary>
	public const int TPM_E_NOSPACE = unchecked((int)0x80280011);
	/// <summary>
	/// There is no Storage Root Key (SRK) set.
	/// </summary>
	public const int TPM_E_NOSRK = unchecked((int)0x80280012);
	/// <summary>
	/// An encrypted blob is invalid or was not created by this TPM.
	/// </summary>
	public const int TPM_E_NOTSEALED_BLOB = unchecked((int)0x80280013);
	/// <summary>
	/// The Trusted Platform Module (TPM) already has an owner.
	/// </summary>
	public const int TPM_E_OWNER_SET = unchecked((int)0x80280014);
	/// <summary>
	/// The TPM has insufficient internal resources to perform the requested action.
	/// </summary>
	public const int TPM_E_RESOURCES = unchecked((int)0x80280015);
	/// <summary>
	/// A random string was too short.
	/// </summary>
	public const int TPM_E_SHORTRANDOM = unchecked((int)0x80280016);
	/// <summary>
	/// The TPM does not have the space to perform the operation.
	/// </summary>
	public const int TPM_E_SIZE = unchecked((int)0x80280017);
	/// <summary>
	/// The named PCR value does not match the current PCR value.
	/// </summary>
	public const int TPM_E_WRONGPCRVAL = unchecked((int)0x80280018);
	/// <summary>
	/// The paramSize argument to the command has the incorrect value .
	/// </summary>
	public const int TPM_E_BAD_PARAM_SIZE = unchecked((int)0x80280019);
	/// <summary>
	/// There is no existing SHA-1 thread.
	/// </summary>
	public const int TPM_E_SHA_THREAD = unchecked((int)0x8028001A);
	/// <summary>
	/// The calculation is unable to proceed because the existing SHA-1 thread has already encountered an error.
	/// </summary>
	public const int TPM_E_SHA_ERROR = unchecked((int)0x8028001B);
	/// <summary>
	/// The TPM hardware device reported a failure during its internal self test. Try restarting the computer to resolve the problem. If the problem continues, you might need to replace your TPM hardware or motherboard.
	/// </summary>
	public const int TPM_E_FAILEDSELFTEST = unchecked((int)0x8028001C);
	/// <summary>
	/// The authorization for the second key in a 2 key function failed authorization.
	/// </summary>
	public const int TPM_E_AUTH2FAIL = unchecked((int)0x8028001D);
	/// <summary>
	/// The tag value sent to for a command is invalid.
	/// </summary>
	public const int TPM_E_BADTAG = unchecked((int)0x8028001E);
	/// <summary>
	/// An IO error occurred transmitting information to the TPM.
	/// </summary>
	public const int TPM_E_IOERROR = unchecked((int)0x8028001F);
	/// <summary>
	/// The encryption process had a problem.
	/// </summary>
	public const int TPM_E_ENCRYPT_ERROR = unchecked((int)0x80280020);
	/// <summary>
	/// The decryption process did not complete.
	/// </summary>
	public const int TPM_E_DECRYPT_ERROR = unchecked((int)0x80280021);
	/// <summary>
	/// An invalid handle was used.
	/// </summary>
	public const int TPM_E_INVALID_AUTHHANDLE = unchecked((int)0x80280022);
	/// <summary>
	/// The TPM does not have an Endorsement Key (EK) installed.
	/// </summary>
	public const int TPM_E_NO_ENDORSEMENT = unchecked((int)0x80280023);
	/// <summary>
	/// The usage of a key is not allowed.
	/// </summary>
	public const int TPM_E_INVALID_KEYUSAGE = unchecked((int)0x80280024);
	/// <summary>
	/// The submitted entity type is not allowed.
	/// </summary>
	public const int TPM_E_WRONG_ENTITYTYPE = unchecked((int)0x80280025);
	/// <summary>
	/// The command was received in the wrong sequence relative to TPM_Init and a subsequent TPM_Startup.
	/// </summary>
	public const int TPM_E_INVALID_POSTINIT = unchecked((int)0x80280026);
	/// <summary>
	/// Signed data cannot include additional DER information.
	/// </summary>
	public const int TPM_E_INAPPROPRIATE_SIG = unchecked((int)0x80280027);
	/// <summary>
	/// The key properties in TPM_KEY_PARMs are not supported by this TPM.
	/// </summary>
	public const int TPM_E_BAD_KEY_PROPERTY = unchecked((int)0x80280028);
	/// <summary>
	/// The migration properties of this key are incorrect.
	/// </summary>
	public const int TPM_E_BAD_MIGRATION = unchecked((int)0x80280029);
	/// <summary>
	/// The signature or encryption scheme for this key is incorrect or not permitted in this situation.
	/// </summary>
	public const int TPM_E_BAD_SCHEME = unchecked((int)0x8028002A);
	/// <summary>
	/// The size of the data (or blob) parameter is bad or inconsistent with the referenced key.
	/// </summary>
	public const int TPM_E_BAD_DATASIZE = unchecked((int)0x8028002B);
	/// <summary>
	/// A mode parameter is bad, such as capArea or subCapArea for TPM_GetCapability, phsicalPresence parameter for TPM_PhysicalPresence, or migrationType for TPM_CreateMigrationBlob.
	/// </summary>
	public const int TPM_E_BAD_MODE = unchecked((int)0x8028002C);
	/// <summary>
	/// Either the physicalPresence or physicalPresenceLock bits have the wrong value.
	/// </summary>
	public const int TPM_E_BAD_PRESENCE = unchecked((int)0x8028002D);
	/// <summary>
	/// The TPM cannot perform this version of the capability.
	/// </summary>
	public const int TPM_E_BAD_VERSION = unchecked((int)0x8028002E);
	/// <summary>
	/// The TPM does not allow for wrapped transport sessions.
	/// </summary>
	public const int TPM_E_NO_WRAP_TRANSPORT = unchecked((int)0x8028002F);
	/// <summary>
	/// TPM audit construction failed and the underlying command was returning a failure code also.
	/// </summary>
	public const int TPM_E_AUDITFAIL_UNSUCCESSFUL = unchecked((int)0x80280030);
	/// <summary>
	/// TPM audit construction failed and the underlying command was returning success.
	/// </summary>
	public const int TPM_E_AUDITFAIL_SUCCESSFUL = unchecked((int)0x80280031);
	/// <summary>
	/// Attempt to reset a PCR register that does not have the resettable attribute.
	/// </summary>
	public const int TPM_E_NOTRESETABLE = unchecked((int)0x80280032);
	/// <summary>
	/// Attempt to reset a PCR register that requires locality and locality modifier not part of command transport.
	/// </summary>
	public const int TPM_E_NOTLOCAL = unchecked((int)0x80280033);
	/// <summary>
	/// Make identity blob not properly typed.
	/// </summary>
	public const int TPM_E_BAD_TYPE = unchecked((int)0x80280034);
	/// <summary>
	/// When saving context identified resource type does not match actual resource.
	/// </summary>
	public const int TPM_E_INVALID_RESOURCE = unchecked((int)0x80280035);
	/// <summary>
	/// The TPM is attempting to execute a command only available when in FIPS mode.
	/// </summary>
	public const int TPM_E_NOTFIPS = unchecked((int)0x80280036);
	/// <summary>
	/// The command is attempting to use an invalid family ID.
	/// </summary>
	public const int TPM_E_INVALID_FAMILY = unchecked((int)0x80280037);
	/// <summary>
	/// The permission to manipulate the NV storage is not available.
	/// </summary>
	public const int TPM_E_NO_NV_PERMISSION = unchecked((int)0x80280038);
	/// <summary>
	/// The operation requires a signed command.
	/// </summary>
	public const int TPM_E_REQUIRES_SIGN = unchecked((int)0x80280039);
	/// <summary>
	/// Wrong operation to load an NV key.
	/// </summary>
	public const int TPM_E_KEY_NOTSUPPORTED = unchecked((int)0x8028003A);
	/// <summary>
	/// NV_LoadKey blob requires both owner and blob authorization.
	/// </summary>
	public const int TPM_E_AUTH_CONFLICT = unchecked((int)0x8028003B);
	/// <summary>
	/// The NV area is locked and not writtable.
	/// </summary>
	public const int TPM_E_AREA_LOCKED = unchecked((int)0x8028003C);
	/// <summary>
	/// The locality is incorrect for the attempted operation.
	/// </summary>
	public const int TPM_E_BAD_LOCALITY = unchecked((int)0x8028003D);
	/// <summary>
	/// The NV area is read only and can't be written to.
	/// </summary>
	public const int TPM_E_READ_ONLY = unchecked((int)0x8028003E);
	/// <summary>
	/// There is no protection on the write to the NV area.
	/// </summary>
	public const int TPM_E_PER_NOWRITE = unchecked((int)0x8028003F);
	/// <summary>
	/// The family count value does not match.
	/// </summary>
	public const int TPM_E_FAMILYCOUNT = unchecked((int)0x80280040);
	/// <summary>
	/// The NV area has already been written to.
	/// </summary>
	public const int TPM_E_WRITE_LOCKED = unchecked((int)0x80280041);
	/// <summary>
	/// The NV area attributes conflict.
	/// </summary>
	public const int TPM_E_BAD_ATTRIBUTES = unchecked((int)0x80280042);
	/// <summary>
	/// The structure tag and version are invalid or inconsistent.
	/// </summary>
	public const int TPM_E_INVALID_STRUCTURE = unchecked((int)0x80280043);
	/// <summary>
	/// The key is under control of the TPM Owner and can only be evicted by the TPM Owner.
	/// </summary>
	public const int TPM_E_KEY_OWNER_CONTROL = unchecked((int)0x80280044);
	/// <summary>
	/// The counter handle is incorrect.
	/// </summary>
	public const int TPM_E_BAD_COUNTER = unchecked((int)0x80280045);
	/// <summary>
	/// The write is not a complete write of the area.
	/// </summary>
	public const int TPM_E_NOT_FULLWRITE = unchecked((int)0x80280046);
	/// <summary>
	/// The gap between saved context counts is too large.
	/// </summary>
	public const int TPM_E_CONTEXT_GAP = unchecked((int)0x80280047);
	/// <summary>
	/// The maximum number of NV writes without an owner has been exceeded.
	/// </summary>
	public const int TPM_E_MAXNVWRITES = unchecked((int)0x80280048);
	/// <summary>
	/// No operator AuthData value is set.
	/// </summary>
	public const int TPM_E_NOOPERATOR = unchecked((int)0x80280049);
	/// <summary>
	/// The resource pointed to by context is not loaded.
	/// </summary>
	public const int TPM_E_RESOURCEMISSING = unchecked((int)0x8028004A);
	/// <summary>
	/// The delegate administration is locked.
	/// </summary>
	public const int TPM_E_DELEGATE_LOCK = unchecked((int)0x8028004B);
	/// <summary>
	/// Attempt to manage a family other than the delegated family.
	/// </summary>
	public const int TPM_E_DELEGATE_FAMILY = unchecked((int)0x8028004C);
	/// <summary>
	/// Delegation table management not enabled.
	/// </summary>
	public const int TPM_E_DELEGATE_ADMIN = unchecked((int)0x8028004D);
	/// <summary>
	/// There was a command executed outside of an exclusive transport session.
	/// </summary>
	public const int TPM_E_TRANSPORT_NOTEXCLUSIVE = unchecked((int)0x8028004E);
	/// <summary>
	/// Attempt to context save a owner evict controlled key.
	/// </summary>
	public const int TPM_E_OWNER_CONTROL = unchecked((int)0x8028004F);
	/// <summary>
	/// The DAA command has no resources availble to execute the command.
	/// </summary>
	public const int TPM_E_DAA_RESOURCES = unchecked((int)0x80280050);
	/// <summary>
	/// The consistency check on DAA parameter inputData0 has failed.
	/// </summary>
	public const int TPM_E_DAA_INPUT_DATA0 = unchecked((int)0x80280051);
	/// <summary>
	/// The consistency check on DAA parameter inputData1 has failed.
	/// </summary>
	public const int TPM_E_DAA_INPUT_DATA1 = unchecked((int)0x80280052);
	/// <summary>
	/// The consistency check on DAA_issuerSettings has failed.
	/// </summary>
	public const int TPM_E_DAA_ISSUER_SETTINGS = unchecked((int)0x80280053);
	/// <summary>
	/// The consistency check on DAA_tpmSpecific has failed.
	/// </summary>
	public const int TPM_E_DAA_TPM_SETTINGS = unchecked((int)0x80280054);
	/// <summary>
	/// The atomic process indicated by the submitted DAA command is not the expected process.
	/// </summary>
	public const int TPM_E_DAA_STAGE = unchecked((int)0x80280055);
	/// <summary>
	/// The issuer's validity check has detected an inconsistency.
	/// </summary>
	public const int TPM_E_DAA_ISSUER_VALIDITY = unchecked((int)0x80280056);
	/// <summary>
	/// The consistency check on w has failed.
	/// </summary>
	public const int TPM_E_DAA_WRONG_W = unchecked((int)0x80280057);
	/// <summary>
	/// The handle is incorrect.
	/// </summary>
	public const int TPM_E_BAD_HANDLE = unchecked((int)0x80280058);
	/// <summary>
	/// Delegation is not correct.
	/// </summary>
	public const int TPM_E_BAD_DELEGATE = unchecked((int)0x80280059);
	/// <summary>
	/// The context blob is invalid.
	/// </summary>
	public const int TPM_E_BADCONTEXT = unchecked((int)0x8028005A);
	/// <summary>
	/// Too many contexts held by the TPM.
	/// </summary>
	public const int TPM_E_TOOMANYCONTEXTS = unchecked((int)0x8028005B);
	/// <summary>
	/// Migration authority signature validation failure.
	/// </summary>
	public const int TPM_E_MA_TICKET_SIGNATURE = unchecked((int)0x8028005C);
	/// <summary>
	/// Migration destination not authenticated.
	/// </summary>
	public const int TPM_E_MA_DESTINATION = unchecked((int)0x8028005D);
	/// <summary>
	/// Migration source incorrect.
	/// </summary>
	public const int TPM_E_MA_SOURCE = unchecked((int)0x8028005E);
	/// <summary>
	/// Incorrect migration authority.
	/// </summary>
	public const int TPM_E_MA_AUTHORITY = unchecked((int)0x8028005F);
	/// <summary>
	/// Attempt to revoke the EK and the EK is not revocable.
	/// </summary>
	public const int TPM_E_PERMANENTEK = unchecked((int)0x80280061);
	/// <summary>
	/// Bad signature of CMK ticket.
	/// </summary>
	public const int TPM_E_BAD_SIGNATURE = unchecked((int)0x80280062);
	/// <summary>
	/// There is no room in the context list for additional contexts.
	/// </summary>
	public const int TPM_E_NOCONTEXTSPACE = unchecked((int)0x80280063);
	/// <summary>
	/// The command was blocked.
	/// </summary>
	public const int TPM_E_COMMAND_BLOCKED = unchecked((int)0x80280400);
	/// <summary>
	/// The specified handle was not found.
	/// </summary>
	public const int TPM_E_INVALID_HANDLE = unchecked((int)0x80280401);
	/// <summary>
	/// The TPM returned a duplicate handle and the command needs to be resubmitted.
	/// </summary>
	public const int TPM_E_DUPLICATE_VHANDLE = unchecked((int)0x80280402);
	/// <summary>
	/// The command within the transport was blocked.
	/// </summary>
	public const int TPM_E_EMBEDDED_COMMAND_BLOCKED = unchecked((int)0x80280403);
	/// <summary>
	/// The command within the transport is not supported.
	/// </summary>
	public const int TPM_E_EMBEDDED_COMMAND_UNSUPPORTED = unchecked((int)0x80280404);
	/// <summary>
	/// The TPM is too busy to respond to the command immediately, but the command could be resubmitted at a later time.
	/// </summary>
	public const int TPM_E_RETRY = unchecked((int)0x80280800);
	/// <summary>
	/// SelfTestFull has not been run.
	/// </summary>
	public const int TPM_E_NEEDS_SELFTEST = unchecked((int)0x80280801);
	/// <summary>
	/// The TPM is currently executing a full selftest.
	/// </summary>
	public const int TPM_E_DOING_SELFTEST = unchecked((int)0x80280802);
	/// <summary>
	/// The TPM is defending against dictionary attacks and is in a time-out period.
	/// </summary>
	public const int TPM_E_DEFEND_LOCK_RUNNING = unchecked((int)0x80280803);
	/// <summary>
	/// TPM 2.0: Inconsistent attributes.
	/// </summary>
	public const int TPM_20_E_ATTRIBUTES = unchecked((int)0x80280082);
	/// <summary>
	/// TPM 2.0: Hash algorithm not supported or not appropriate.
	/// </summary>
	public const int TPM_20_E_HASH = unchecked((int)0x80280083);
	/// <summary>
	/// TPM 2.0: Value is out of range or is not correct for the context.
	/// </summary>
	public const int TPM_20_E_VALUE = unchecked((int)0x80280084);
	/// <summary>
	/// TPM 2.0: Hierarchy is not enabled or is not correct for the use.
	/// </summary>
	public const int TPM_20_E_HIERARCHY = unchecked((int)0x80280085);
	/// <summary>
	/// TPM 2.0: Key size is not supported.
	/// </summary>
	public const int TPM_20_E_KEY_SIZE = unchecked((int)0x80280086);
	/// <summary>
	/// TPM 2.0: Mask generation function not supported.
	/// </summary>
	public const int TPM_20_E_MGF = unchecked((int)0x80280087);
	/// <summary>
	/// TPM 2.0: Mode of operation not supported.
	/// </summary>
	public const int TPM_20_E_MODE = unchecked((int)0x80280089);
	/// <summary>
	/// TPM 2.0: The type of the value is not appropriate for the use.
	/// </summary>
	public const int TPM_20_E_TYPE = unchecked((int)0x8028008A);
	/// <summary>
	/// TPM 2.0: The Handle is not correct for the use.
	/// </summary>
	public const int TPM_20_E_HANDLE = unchecked((int)0x8028008B);
	/// <summary>
	/// TPM 2.0: Unsupported key derivation function or function not appropriate for use.
	/// </summary>
	public const int TPM_20_E_KDF = unchecked((int)0x8028008C);
	/// <summary>
	/// TPM 2.0: Value was out of allowed range.
	/// </summary>
	public const int TPM_20_E_RANGE = unchecked((int)0x8028008D);
	/// <summary>
	/// TPM 2.0: The authorization HMAC check failed and DA counter incremented.
	/// </summary>
	public const int TPM_20_E_AUTH_FAIL = unchecked((int)0x8028008E);
	/// <summary>
	/// TPM 2.0: Invalid nonce size.
	/// </summary>
	public const int TPM_20_E_NONCE = unchecked((int)0x8028008F);
	/// <summary>
	/// TPM 2.0: Unsupported or incompatible scheme.
	/// </summary>
	public const int TPM_20_E_SCHEME = unchecked((int)0x80280092);
	/// <summary>
	/// TPM 2.0: Structure is wrong size..
	/// </summary>
	public const int TPM_20_E_SIZE = unchecked((int)0x80280095);
	/// <summary>
	/// TPM 2.0: Incorrect structure tag.
	/// </summary>
	public const int TPM_20_E_TAG = unchecked((int)0x80280097);
	/// <summary>
	/// TPM 2.0: Union selector is incorrect.
	/// </summary>
	public const int TPM_20_E_SELECTOR = unchecked((int)0x80280098);
	/// <summary>
	/// TPM 2.0: The signature is not valid.
	/// </summary>
	public const int TPM_20_E_SIGNATURE = unchecked((int)0x8028009B);
	/// <summary>
	/// TPM 2.0: Key fields are not compatible with the selected use.
	/// </summary>
	public const int TPM_20_E_KEY = unchecked((int)0x80280087);
	/// <summary>
	/// TPM 2.0: A policy check failed.
	/// </summary>
	public const int TPM_20_E_POLICY_FAIL = unchecked((int)0x8028009D);
	/// <summary>
	/// TPM 2.0: Integrity check failed.
	/// </summary>
	public const int TPM_20_E_INTEGRITY = unchecked((int)0x8028009F);
	/// <summary>
	/// TPM 2.0: Invalid ticket.
	/// </summary>
	public const int TPM_20_E_TICKET = unchecked((int)0x802800A0);
	/// <summary>
	/// TPM 2.0: Reserved bits not set to zero as required.
	/// </summary>
	public const int TPM_20_E_RESERVED_BITS = unchecked((int)0x802800A1);
	/// <summary>
	/// TPM 2.0: Authorization failure without DA implications.
	/// </summary>
	public const int TPM_20_E_BAD_AUTH = unchecked((int)0x802800A2);
	/// <summary>
	/// TPM 2.0: The policy has expired.
	/// </summary>
	public const int TPM_20_E_EXPIRED = unchecked((int)0x802800A3);
	/// <summary>
	/// TPM 2.0: The command code in the policy is not the command code of the command or the command code in a policy command references a command that is not implemented.
	/// </summary>
	public const int TPM_20_E_POLICY_CC = unchecked((int)0x802800A4);
	/// <summary>
	/// TPM 2.0: Public and sensitive portions of an object are not cryptographically bound.
	/// </summary>
	public const int TPM_20_E_BINDING = unchecked((int)0x802800A5);
	/// <summary>
	/// TPM 2.0: Curve not supported.
	/// </summary>
	public const int TPM_20_E_CURVE = unchecked((int)0x802800A6);
	/// <summary>
	/// TPM 2.0: Point is not on the required curve.
	/// </summary>
	public const int TPM_20_E_ECC_POINT = unchecked((int)0x802800A7);
	/// <summary>
	/// TPM 2.0: TPM not initialized.
	/// </summary>
	public const int TPM_20_E_INITIALIZE = unchecked((int)0x80280100);
	/// <summary>
	/// TPM 2.0: Commands not being accepted because of a TPM failure.
	/// </summary>
	public const int TPM_20_E_FAILURE = unchecked((int)0x80280101);
	/// <summary>
	/// TPM 2.0: Improper use of a sequence handle.
	/// </summary>
	public const int TPM_20_E_SEQUENCE = unchecked((int)0x80280103);
	/// <summary>
	/// TPM 2.0: TPM_RC_PRIVATE error.
	/// </summary>
	public const int TPM_20_E_PRIVATE = unchecked((int)0x80280010B);
	/// <summary>
	/// TPM 2.0: TPM_RC_HMAC.
	/// </summary>
	public const int TPM_20_E_HMAC = unchecked((int)0x80280119);
	/// <summary>
	/// TPM 2.0: TPM_RC_DISABLED.
	/// </summary>
	public const int TPM_20_E_DISABLED = unchecked((int)0x80280120);
	/// <summary>
	/// TPM 2.0: Command failed because audit sequence required exclusivity.
	/// </summary>
	public const int TPM_20_E_EXCLUSIVE = unchecked((int)0x80280121);
	/// <summary>
	/// TPM 2.0: Unsupported ECC curve.
	/// </summary>
	public const int TPM_20_E_ECC_CURVE = unchecked((int)0x80280123);
	/// <summary>
	/// TPM 2.0: Authorization handle is not correct for command.
	/// </summary>
	public const int TPM_20_E_AUTH_TYPE = unchecked((int)0x80280124);
	/// <summary>
	/// TPM 2.0: Command requires an authorization session for handle and is not present.
	/// </summary>
	public const int TPM_20_E_AUTH_MISSING = unchecked((int)0x80280125);
	/// <summary>
	/// TPM 2.0: Policy failure in Math Operation or an invalid authPolicy value.
	/// </summary>
	public const int TPM_20_E_POLICY = unchecked((int)0x80280126);
	/// <summary>
	/// TPM 2.0: PCR check fail.
	/// </summary>
	public const int TPM_20_E_PCR = unchecked((int)0x80280127);
	/// <summary>
	/// TPM 2.0: PCR have changed since checked.
	/// </summary>
	public const int TPM_20_E_PCR_CHANGED = unchecked((int)0x80280128);
	/// <summary>
	/// TPM 2.0: The TPM is not in the right mode for upgrade.
	/// </summary>
	public const int TPM_20_E_UPGRADE = unchecked((int)0x8028012D);
	/// <summary>
	/// TPM 2.0: Context ID counter is at maximum.
	/// </summary>
	public const int TPM_20_E_TOO_MANY_CONTEXTS = unchecked((int)0x8028012E);
	/// <summary>
	/// TPM 2.0: authValue or authPolicy is not available for selected entity.
	/// </summary>
	public const int TPM_20_E_AUTH_UNAVAILABLE = unchecked((int)0x8028012F);
	/// <summary>
	/// TPM 2.0: A _TPM_Init and Startup(CLEAR) is required before the TPM can resume operation.
	/// </summary>
	public const int TPM_20_E_REBOOT = unchecked((int)0x80280130);
	/// <summary>
	/// TPM 2.0: The protection algorithms (hash and symmetric) are not reasonably balanced. The digest size of the hash must be larger than the key size of the symmetric algorithm.
	/// </summary>
	public const int TPM_20_E_UNBALANCED = unchecked((int)0x80280131);
	/// <summary>
	/// TPM 2.0: The TPM command's commandSize value is inconsistent with contents of the command buffer; either the size is not the same as the bytes loaded by the hardware interface layer or the value is not large enough to hold a command header.
	/// </summary>
	public const int TPM_20_E_COMMAND_SIZE = unchecked((int)0x80280142);
	/// <summary>
	/// TPM 2.0: Command code not supported.
	/// </summary>
	public const int TPM_20_E_COMMAND_CODE = unchecked((int)0x80280143);
	/// <summary>
	/// TPM 2.0: The value of authorizationSize is out of range or the number of octets in the authorization Area is greater than required.
	/// </summary>
	public const int TPM_20_E_AUTHSIZE = unchecked((int)0x80280144);
	/// <summary>
	/// TPM 2.0: Use of an authorization session with a context command or another command that cannot have an authorization session.
	/// </summary>
	public const int TPM_20_E_AUTH_CONTEXT = unchecked((int)0x80280145);
	/// <summary>
	/// TPM 2.0: NV offset+size is out of range.
	/// </summary>
	public const int TPM_20_E_NV_RANGE = unchecked((int)0x80280146);
	/// <summary>
	/// TPM 2.0: Requested allocation size is larger than allowed.
	/// </summary>
	public const int TPM_20_E_NV_SIZE = unchecked((int)0x80280147);
	/// <summary>
	/// TPM 2.0: NV access locked.
	/// </summary>
	public const int TPM_20_E_NV_LOCKED = unchecked((int)0x80280148);
	/// <summary>
	/// TPM 2.0: NV access authorization fails in command actions
	/// </summary>
	public const int TPM_20_E_NV_AUTHORIZATION = unchecked((int)0x80280149);
	/// <summary>
	/// TPM 2.0: An NV index is used before being initialized or the state saved by TPM2_Shutdown(STATE) could not be restored.
	/// </summary>
	public const int TPM_20_E_NV_UNINITIALIZED = unchecked((int)0x8028014A);
	/// <summary>
	/// TPM 2.0: Insufficient space for NV allocation.
	/// </summary>
	public const int TPM_20_E_NV_SPACE = unchecked((int)0x8028014B);
	/// <summary>
	/// TPM 2.0: NV index or persistent object already defined.
	/// </summary>
	public const int TPM_20_E_NV_DEFINED = unchecked((int)0x8028014C);
	/// <summary>
	/// TPM 2.0: Context in TPM2_ContextLoad() is not valid.
	/// </summary>
	public const int TPM_20_E_BAD_CONTEXT = unchecked((int)0x80280150);
	/// <summary>
	/// TPM 2.0: chHash value already set or not correct for use.
	/// </summary>
	public const int TPM_20_E_CPHASH = unchecked((int)0x80280151);
	/// <summary>
	/// TPM 2.0: Handle for parent is not a valid parent.
	/// </summary>
	public const int TPM_20_E_PARENT = unchecked((int)0x80280152);
	/// <summary>
	/// TPM 2.0: Some function needs testing.
	/// </summary>
	public const int TPM_20_E_NEEDS_TEST = unchecked((int)0x80280153);
	/// <summary>
	/// TPM 2.0: returned when an internal function cannot process a request due to an unspecified problem. This code is usually related to invalid parameters that are not properly filtered by the input unmarshaling code.
	/// </summary>
	public const int TPM_20_E_NO_RESULT = unchecked((int)0x80280154);
	/// <summary>
	/// TPM 2.0: The sensitive area did not unmarshal correctly after decryption - this code is used in lieu of the other unmarshaling errors so that an attacker cannot determine where the unmarshaling error occurred.
	/// </summary>
	public const int TPM_20_E_SENSITIVE = unchecked((int)0x80280155);
	/// <summary>
	/// TPM 2.0: Gap for context ID is too large.
	/// </summary>
	public const int TPM_20_E_CONTEXT_GAP = unchecked((int)0x80280901);
	/// <summary>
	/// TPM 2.0: Out of memory for object contexts.
	/// </summary>
	public const int TPM_20_E_OBJECT_MEMORY = unchecked((int)0x80280902);
	/// <summary>
	/// TPM 2.0: Out of memory for session contexts.
	/// </summary>
	public const int TPM_20_E_SESSION_MEMORY = unchecked((int)0x80280903);
	/// <summary>
	/// TPM 2.0: Out of shared object/session memory or need space for internal operations.
	/// </summary>
	public const int TPM_20_E_MEMORY = unchecked((int)0x80280904);
	/// <summary>
	/// TPM 2.0: Out of session handles - a session must be flushed before a new session may be created.
	/// </summary>
	public const int TPM_20_E_SESSION_HANDLES = unchecked((int)0x80280905);
	/// <summary>
	/// TPM 2.0: Out of object handles - the handle space for objects is depleted and a reboot is required.
	/// </summary>
	public const int TPM_20_E_OBJECT_HANDLES = unchecked((int)0x80280906);
	/// <summary>
	/// TPM 2.0: Bad locality.
	/// </summary>
	public const int TPM_20_E_LOCALITY = unchecked((int)0x80280907);
	/// <summary>
	/// TPM 2.0: The TPM has suspended operation on the command; forward progress was made and the command may be retried.
	/// </summary>
	public const int TPM_20_E_YIELDED = unchecked((int)0x80280908);
	/// <summary>
	/// TPM 2.0: The command was canceled.
	/// </summary>
	public const int TPM_20_E_CANCELED = unchecked((int)0x80280909);
	/// <summary>
	/// TPM 2.0: TPM is performing self-tests.
	/// </summary>
	public const int TPM_20_E_TESTING = unchecked((int)0x8028090A);
	/// <summary>
	/// TPM 2.0: The TPM is rate-limiting accesses to prevent wearout of NV.
	/// </summary>
	public const int TPM_20_E_NV_RATE = unchecked((int)0x80280920);
	/// <summary>
	/// TPM 2.0: Authorization for objects subject to DA protection are not allowed at this time because the TPM is in DA lockout mode.
	/// </summary>
	public const int TPM_20_E_LOCKOUT = unchecked((int)0x80280921);
	/// <summary>
	/// TPM 2.0: The TPM was not able to start the command.
	/// </summary>
	public const int TPM_20_E_RETRY = unchecked((int)0x80280922);
	/// <summary>
	/// TPM 2.0: the command may require writing of NV and NV is not current accessible..
	/// </summary>
	public const int TPM_20_E_NV_UNAVAILABLE = unchecked((int)0x80280923);
	/// <summary>
	/// An internal software error has been detected.
	/// </summary>
	public const int TBS_E_INTERNAL_ERROR = unchecked((int)0x80284001);
	/// <summary>
	/// One or more input parameters is bad.
	/// </summary>
	public const int TBS_E_BAD_PARAMETER = unchecked((int)0x80284002);
	/// <summary>
	/// A specified output pointer is bad.
	/// </summary>
	public const int TBS_E_INVALID_OUTPUT_POINTER = unchecked((int)0x80284003);
	/// <summary>
	/// The specified context handle does not refer to a valid context.
	/// </summary>
	public const int TBS_E_INVALID_CONTEXT = unchecked((int)0x80284004);
	/// <summary>
	/// A specified output buffer is too small.
	/// </summary>
	public const int TBS_E_INSUFFICIENT_BUFFER = unchecked((int)0x80284005);
	/// <summary>
	/// An error occurred while communicating with the TPM.
	/// </summary>
	public const int TBS_E_IOERROR = unchecked((int)0x80284006);
	/// <summary>
	/// One or more context parameters is invalid.
	/// </summary>
	public const int TBS_E_INVALID_CONTEXT_PARAM = unchecked((int)0x80284007);
	/// <summary>
	/// The TBS service is not running and could not be started.
	/// </summary>
	public const int TBS_E_SERVICE_NOT_RUNNING = unchecked((int)0x80284008);
	/// <summary>
	/// A new context could not be created because there are too many open contexts.
	/// </summary>
	public const int TBS_E_TOO_MANY_TBS_CONTEXTS = unchecked((int)0x80284009);
	/// <summary>
	/// A new virtual resource could not be created because there are too many open virtual resources.
	/// </summary>
	public const int TBS_E_TOO_MANY_RESOURCES = unchecked((int)0x8028400A);
	/// <summary>
	/// The TBS service has been started but is not yet running.
	/// </summary>
	public const int TBS_E_SERVICE_START_PENDING = unchecked((int)0x8028400B);
	/// <summary>
	/// The physical presence interface is not supported.
	/// </summary>
	public const int TBS_E_PPI_NOT_SUPPORTED = unchecked((int)0x8028400C);
	/// <summary>
	/// The command was canceled.
	/// </summary>
	public const int TBS_E_COMMAND_CANCELED = unchecked((int)0x8028400D);
	/// <summary>
	/// The input or output buffer is too large.
	/// </summary>
	public const int TBS_E_BUFFER_TOO_LARGE = unchecked((int)0x8028400E);
	/// <summary>
	/// A compatible Trusted Platform Module (TPM) Security Device cannot be found on this computer.
	/// </summary>
	public const int TBS_E_TPM_NOT_FOUND = unchecked((int)0x8028400F);
	/// <summary>
	/// The TBS service has been disabled.
	/// </summary>
	public const int TBS_E_SERVICE_DISABLED = unchecked((int)0x80284010);
	/// <summary>
	/// No TCG event log is available.
	/// </summary>
	public const int TBS_E_NO_EVENT_LOG = unchecked((int)0x80284011);
	/// <summary>
	/// The caller does not have the appropriate rights to perform the requested operation.
	/// </summary>
	public const int TBS_E_ACCESS_DENIED = unchecked((int)0x80284012);
	/// <summary>
	/// The TPM provisioning action is not allowed by the specified flags. For provisioning to be successful, one of several actions may be required. The TPM management console (tpm.msc) action to make the TPM Ready may help. For further information, see the documentation for the Win32_Tpm WMI method 'Provision'. (The actions that may be required include importing the TPM Owner Authorization value into the system, calling the Win32_Tpm WMI method for provisioning the TPM and specifying TRUE for either 'ForceClear_Allowed' or 'PhysicalPresencePrompts_Allowed' (as indicated by the value returned in the Additional Information), or enabling the TPM in the system BIOS.)
	/// </summary>
	public const int TBS_E_PROVISIONING_NOT_ALLOWED = unchecked((int)0x80284013);
	/// <summary>
	/// The Physical Presence Interface of this firmware does not support the requested method.
	/// </summary>
	public const int TBS_E_PPI_FUNCTION_UNSUPPORTED = unchecked((int)0x80284014);
	/// <summary>
	/// The requested TPM OwnerAuth value was not found.
	/// </summary>
	public const int TBS_E_OWNERAUTH_NOT_FOUND = unchecked((int)0x80284015);
	/// <summary>
	/// The TPM provisioning did not complete. For more information on completing the provisioning, call the Win32_Tpm WMI method for provisioning the TPM ('Provision') and check the returned Information.
	/// </summary>
	public const int TBS_E_PROVISIONING_INCOMPLETE = unchecked((int)0x80284016);
	/// <summary>
	/// The command buffer is not in the correct state.
	/// </summary>
	public const int TPMAPI_E_INVALID_STATE = unchecked((int)0x80290100);
	/// <summary>
	/// The command buffer does not contain enough data to satisfy the request.
	/// </summary>
	public const int TPMAPI_E_NOT_ENOUGH_DATA = unchecked((int)0x80290101);
	/// <summary>
	/// The command buffer cannot contain any more data.
	/// </summary>
	public const int TPMAPI_E_TOO_MUCH_DATA = unchecked((int)0x80290102);
	/// <summary>
	/// One or more output parameters was <strong>NULL</strong> or invalid.
	/// </summary>
	public const int TPMAPI_E_INVALID_OUTPUT_POINTER = unchecked((int)0x80290103);
	/// <summary>
	/// One or more input parameters is invalid.
	/// </summary>
	public const int TPMAPI_E_INVALID_PARAMETER = unchecked((int)0x80290104);
	/// <summary>
	/// Not enough memory was available to satisfy the request.
	/// </summary>
	public const int TPMAPI_E_OUT_OF_MEMORY = unchecked((int)0x80290105);
	/// <summary>
	/// The specified buffer was too small.
	/// </summary>
	public const int TPMAPI_E_BUFFER_TOO_SMALL = unchecked((int)0x80290106);
	/// <summary>
	/// An internal error was detected.
	/// </summary>
	public const int TPMAPI_E_INTERNAL_ERROR = unchecked((int)0x80290107);
	/// <summary>
	/// The caller does not have the appropriate rights to perform the requested operation.
	/// </summary>
	public const int TPMAPI_E_ACCESS_DENIED = unchecked((int)0x80290108);
	/// <summary>
	/// The specified authorization information was invalid.
	/// </summary>
	public const int TPMAPI_E_AUTHORIZATION_FAILED = unchecked((int)0x80290109);
	/// <summary>
	/// The specified context handle was not valid.
	/// </summary>
	public const int TPMAPI_E_INVALID_CONTEXT_HANDLE = unchecked((int)0x8029010A);
	/// <summary>
	/// An error occurred while communicating with the TBS.
	/// </summary>
	public const int TPMAPI_E_TBS_COMMUNICATION_ERROR = unchecked((int)0x8029010B);
	/// <summary>
	/// The TPM returned an unexpected result.
	/// </summary>
	public const int TPMAPI_E_TPM_COMMAND_ERROR = unchecked((int)0x8029010C);
	/// <summary>
	/// The message was too large for the encoding scheme.
	/// </summary>
	public const int TPMAPI_E_MESSAGE_TOO_LARGE = unchecked((int)0x8029010D);
	/// <summary>
	/// The encoding in the blob was not recognized.
	/// </summary>
	public const int TPMAPI_E_INVALID_ENCODING = unchecked((int)0x8029010E);
	/// <summary>
	/// The key size is not valid.
	/// </summary>
	public const int TPMAPI_E_INVALID_KEY_SIZE = unchecked((int)0x8029010F);
	/// <summary>
	/// The encryption operation failed.
	/// </summary>
	public const int TPMAPI_E_ENCRYPTION_FAILED = unchecked((int)0x80290110);
	/// <summary>
	/// The key parameters structure was not valid
	/// </summary>
	public const int TPMAPI_E_INVALID_KEY_PARAMS = unchecked((int)0x80290111);
	/// <summary>
	/// The requested supplied data does not appear to be a valid migration authorization blob.
	/// </summary>
	public const int TPMAPI_E_INVALID_MIGRATION_AUTHORIZATION_BLOB = unchecked((int)0x80290112);
	/// <summary>
	/// The specified PCR index was invalid
	/// </summary>
	public const int TPMAPI_E_INVALID_PCR_INDEX = unchecked((int)0x80290113);
	/// <summary>
	/// The data given does not appear to be a valid delegate blob.
	/// </summary>
	public const int TPMAPI_E_INVALID_DELEGATE_BLOB = unchecked((int)0x80290114);
	/// <summary>
	/// One or more of the specified context parameters was not valid.
	/// </summary>
	public const int TPMAPI_E_INVALID_CONTEXT_PARAMS = unchecked((int)0x80290115);
	/// <summary>
	/// The data given does not appear to be a valid key blob
	/// </summary>
	public const int TPMAPI_E_INVALID_KEY_BLOB = unchecked((int)0x80290116);
	/// <summary>
	/// The specified PCR data was invalid.
	/// </summary>
	public const int TPMAPI_E_INVALID_PCR_DATA = unchecked((int)0x80290117);
	/// <summary>
	/// The format of the owner auth data was invalid.
	/// </summary>
	public const int TPMAPI_E_INVALID_OWNER_AUTH = unchecked((int)0x80290118);
	/// <summary>
	/// The random number generated did not pass FIPS RNG check.
	/// </summary>
	public const int TPMAPI_E_FIPS_RNG_CHECK_FAILED = unchecked((int)0x80290119);
	/// <summary>
	/// The TCG Event Log does not contain any data.
	/// </summary>
	public const int TPMAPI_E_EMPTY_TCG_LOG = unchecked((int)0x8029011A);
	/// <summary>
	/// An entry in the TCG Event Log was invalid.
	/// </summary>
	public const int TPMAPI_E_INVALID_TCG_LOG_ENTRY = unchecked((int)0x8029011B);
	/// <summary>
	/// A TCG Separator was not found.
	/// </summary>
	public const int TPMAPI_E_TCG_SEPARATOR_ABSENT = unchecked((int)0x8029011C);
	/// <summary>
	/// A digest value in a TCG Log entry did not match hashed data.
	/// </summary>
	public const int TPMAPI_E_TCG_INVALID_DIGEST_ENTRY = unchecked((int)0x8029011D);
	/// <summary>
	/// The requested operation was blocked by current TPM policy. Please contact your system administrator for assistance.
	/// </summary>
	public const int TPMAPI_E_POLICY_DENIES_OPERATION = unchecked((int)0x8029011E);
	/// <summary>
	/// The specified buffer was too small.
	/// </summary>
	public const int TBSIMP_E_BUFFER_TOO_SMALL = unchecked((int)0x80290200);
	/// <summary>
	/// The context could not be cleaned up.
	/// </summary>
	public const int TBSIMP_E_CLEANUP_FAILED = unchecked((int)0x80290201);
	/// <summary>
	/// The specified context handle is invalid.
	/// </summary>
	public const int TBSIMP_E_INVALID_CONTEXT_HANDLE = unchecked((int)0x80290202);
	/// <summary>
	/// An invalid context parameter was specified.
	/// </summary>
	public const int TBSIMP_E_INVALID_CONTEXT_PARAM = unchecked((int)0x80290203);
	/// <summary>
	/// An error occurred while communicating with the TPM
	/// </summary>
	public const int TBSIMP_E_TPM_ERROR = unchecked((int)0x80290204);
	/// <summary>
	/// No entry with the specified key was found.
	/// </summary>
	public const int TBSIMP_E_HASH_BAD_KEY = unchecked((int)0x80290205);
	/// <summary>
	/// The specified virtual handle matches a virtual handle already in use.
	/// </summary>
	public const int TBSIMP_E_DUPLICATE_VHANDLE = unchecked((int)0x80290206);
	/// <summary>
	/// The pointer to the returned handle location was <strong>NULL</strong> or invalid
	/// </summary>
	public const int TBSIMP_E_INVALID_OUTPUT_POINTER = unchecked((int)0x80290207);
	/// <summary>
	/// One or more parameters is invalid
	/// </summary>
	public const int TBSIMP_E_INVALID_PARAMETER = unchecked((int)0x80290208);
	/// <summary>
	/// The RPC subsystem could not be initialized.
	/// </summary>
	public const int TBSIMP_E_RPC_INIT_FAILED = unchecked((int)0x80290209);
	/// <summary>
	/// The TBS scheduler is not running.
	/// </summary>
	public const int TBSIMP_E_SCHEDULER_NOT_RUNNING = unchecked((int)0x8029020A);
	/// <summary>
	/// The command was canceled.
	/// </summary>
	public const int TBSIMP_E_COMMAND_CANCELED = unchecked((int)0x8029020B);
	/// <summary>
	/// There was not enough memory to fulfill the request
	/// </summary>
	public const int TBSIMP_E_OUT_OF_MEMORY = unchecked((int)0x8029020C);
	/// <summary>
	/// The specified list is empty, or the iteration has reached the end of the list.
	/// </summary>
	public const int TBSIMP_E_LIST_NO_MORE_ITEMS = unchecked((int)0x8029020D);
	/// <summary>
	/// The specified item was not found in the list.
	/// </summary>
	public const int TBSIMP_E_LIST_NOT_FOUND = unchecked((int)0x8029020E);
	/// <summary>
	/// The TPM does not have enough space to load the requested resource.
	/// </summary>
	public const int TBSIMP_E_NOT_ENOUGH_SPACE = unchecked((int)0x8029020F);
	/// <summary>
	/// There are too many TPM contexts in use.
	/// </summary>
	public const int TBSIMP_E_NOT_ENOUGH_TPM_CONTEXTS = unchecked((int)0x80290210);
	/// <summary>
	/// The TPM command failed.
	/// </summary>
	public const int TBSIMP_E_COMMAND_FAILED = unchecked((int)0x80290211);
	/// <summary>
	/// The TBS does not recognize the specified ordinal.
	/// </summary>
	public const int TBSIMP_E_UNKNOWN_ORDINAL = unchecked((int)0x80290212);
	/// <summary>
	/// The requested resource is no longer available.
	/// </summary>
	public const int TBSIMP_E_RESOURCE_EXPIRED = unchecked((int)0x80290213);
	/// <summary>
	/// The resource type did not match.
	/// </summary>
	public const int TBSIMP_E_INVALID_RESOURCE = unchecked((int)0x80290214);
	/// <summary>
	/// No resources can be unloaded.
	/// </summary>
	public const int TBSIMP_E_NOTHING_TO_UNLOAD = unchecked((int)0x80290215);
	/// <summary>
	/// No new entries can be added to the hash table.
	/// </summary>
	public const int TBSIMP_E_HASH_TABLE_FULL = unchecked((int)0x80290216);
	/// <summary>
	/// A new TBS context could not be created because there are too many open contexts.
	/// </summary>
	public const int TBSIMP_E_TOO_MANY_TBS_CONTEXTS = unchecked((int)0x80290217);
	/// <summary>
	/// A new virtual resource could not be created because there are too many open virtual resources.
	/// </summary>
	public const int TBSIMP_E_TOO_MANY_RESOURCES = unchecked((int)0x80290218);
	/// <summary>
	/// The physical presence interface is not supported.
	/// </summary>
	public const int TBSIMP_E_PPI_NOT_SUPPORTED = unchecked((int)0x80290219);
	/// <summary>
	/// TBS is not compatible with the version of TPM found on the system.
	/// </summary>
	public const int TBSIMP_E_TPM_INCOMPATIBLE = unchecked((int)0x8029021A);
	/// <summary>
	/// No TCG event log is available.
	/// </summary>
	public const int TBSIMP_E_NO_EVENT_LOG = unchecked((int)0x8029021B);
	/// <summary>
	/// A general error was detected when attempting to acquire the BIOS's response to a Physical Presence command.
	/// </summary>
	public const int TPM_E_PPI_ACPI_FAILURE = unchecked((int)0x80290300);
	/// <summary>
	/// The user failed to confirm the TPM operation request.
	/// </summary>
	public const int TPM_E_PPI_USER_ABORT = unchecked((int)0x80290301);
	/// <summary>
	/// The BIOS failure prevented the successful execution of the requested TPM operation (e.g. invalid TPM operation request, BIOS communication error with the TPM).
	/// </summary>
	public const int TPM_E_PPI_BIOS_FAILURE = unchecked((int)0x80290302);
	/// <summary>
	/// The BIOS does not support the physical presence interface.
	/// </summary>
	public const int TPM_E_PPI_NOT_SUPPORTED = unchecked((int)0x80290303);
	/// <summary>
	/// The Physical Presence command was blocked by current BIOS settings. The system owner may be able to reconfigure the BIOS settings to allow the command.
	/// </summary>
	public const int TPM_E_PPI_BLOCKED_IN_BIOS = unchecked((int)0x80290304);
	/// <summary>
	/// This is an error mask to convert Platform Crypto Provider errors to win errors.
	/// </summary>
	public const int TPM_E_PCP_ERROR_MASK = unchecked((int)0x80290400);
	/// <summary>
	/// The Platform Crypto Device is currently not ready. It needs to be fully provisioned to be operational.
	/// </summary>
	public const int TPM_E_PCP_DEVICE_NOT_READY = unchecked((int)0x80290401);
	/// <summary>
	/// The handle provided to the Platform Crypto Provider is invalid.
	/// </summary>
	public const int TPM_E_PCP_INVALID_HANDLE = unchecked((int)0x80290402);
	/// <summary>
	/// A parameter provided to the Platform Crypto Provider is invalid.
	/// </summary>
	public const int TPM_E_PCP_INVALID_PARAMETER = unchecked((int)0x80290403);
	/// <summary>
	/// A provided flag to the Platform Crypto Provider is not supported.
	/// </summary>
	public const int TPM_E_PCP_FLAG_NOT_SUPPORTED = unchecked((int)0x80290404);
	/// <summary>
	/// The requested operation is not supported by this Platform Crypto Provider.
	/// </summary>
	public const int TPM_E_PCP_NOT_SUPPORTED = unchecked((int)0x80290405);
	/// <summary>
	/// The buffer is too small to contain all data. No information has been written to the buffer.
	/// </summary>
	public const int TPM_E_PCP_BUFFER_TOO_SMALL = unchecked((int)0x80290406);
	/// <summary>
	/// An unexpected internal error has occurred in the Platform Crypto Provider.
	/// </summary>
	public const int TPM_E_PCP_INTERNAL_ERROR = unchecked((int)0x80290407);
	/// <summary>
	/// The authorization to use a provider object has failed.
	/// </summary>
	public const int TPM_E_PCP_AUTHENTICATION_FAILED = unchecked((int)0x80290408);
	/// <summary>
	/// The Platform Crypto Device has ignored the authorization for the provider object, to mitigate against a dictionary attack.
	/// </summary>
	public const int TPM_E_PCP_AUTHENTICATION_IGNORED = unchecked((int)0x80290409);
	/// <summary>
	/// The referenced policy was not found.
	/// </summary>
	public const int TPM_E_PCP_POLICY_NOT_FOUND = unchecked((int)0x8029040A);
	/// <summary>
	/// The referenced profile was not found.
	/// </summary>
	public const int TPM_E_PCP_PROFILE_NOT_FOUND = unchecked((int)0x8029040B);
	/// <summary>
	/// The validation was not successful.
	/// </summary>
	public const int TPM_E_PCP_VALIDATION_FAILED = unchecked((int)0x8029040C);
	/// <summary>
	/// An attempt was made to import or load a key under an incorrect storage parent.
	/// </summary>
	public const int TPM_E_PCP_WRONG_PARENT = unchecked((int)0x8029040E);
	/// <summary>
	/// The TPM key is not loaded.
	/// </summary>
	public const int TPM_E_KEY_NOT_LOADED = unchecked((int)0x8029040F);
	/// <summary>
	/// The TPM key certification has not been generated.
	/// </summary>
	public const int TPM_E_NO_KEY_CERTIFICATION = unchecked((int)0x80290410);
	/// <summary>
	/// The TPM key is not yet finalized.
	/// </summary>
	public const int TPM_E_KEY_NOT_FINALIZED = unchecked((int)0x80290411);
	/// <summary>
	/// The TPM attestation challenge is not set.
	/// </summary>
	public const int TPM_E_ATTESTATION_CHALLENGE_NOT_SET = unchecked((int)0x80290412);
	/// <summary>
	/// The TPM PCR info is not available.
	/// </summary>
	public const int TPM_E_NOT_PCR_BOUND = unchecked((int)0x80290413);
	/// <summary>
	/// The TPM key is already finalized.
	/// </summary>
	public const int TPM_E_KEY_ALREADY_FINALIZED = unchecked((int)0x80290414);
	/// <summary>
	/// The TPM key usage policy is not supported.
	/// </summary>
	public const int TPM_E_KEY_USAGE_POLICY_NOT_SUPPORTED = unchecked((int)0x80290415);
	/// <summary>
	/// The TPM key usage policy is invalid.
	/// </summary>
	public const int TPM_E_KEY_USAGE_POLICY_INVALID = unchecked((int)0x80290416);
	/// <summary>
	/// There was a problem with the software key being imported into the TPM.
	/// </summary>
	public const int TPM_E_SOFT_KEY_ERROR = unchecked((int)0x80290417);
	/// <summary>
	/// The TPM key is not authenticated.
	/// </summary>
	public const int TPM_E_KEY_NOT_AUTHENTICATED = unchecked((int)0x80290418);
	/// <summary>
	/// The TPM key is not an AIK.
	/// </summary>
	public const int TPM_E_PCP_KEY_NOT_AIK = unchecked((int)0x80290419);
	/// <summary>
	/// The TPM key is not a signing key.
	/// </summary>
	public const int TPM_E_KEY_NOT_SIGNING_KEY = unchecked((int)0x8029041A);
	/// <summary>
	/// The TPM is locked out.
	/// </summary>
	public const int TPM_E_LOCKED_OUT = unchecked((int)0x8029041B);
	/// <summary>
	/// The claim type requested is not supported.
	/// </summary>
	public const int TPM_E_CLAIM_TYPE_NOT_SUPPORTED = unchecked((int)0x8029041C);
	/// <summary>
	/// TPM version is not supported.
	/// </summary>
	public const int TPM_E_VERSION_NOT_SUPPORTED = unchecked((int)0x8029041D);
	/// <summary>
	/// The buffer lengths do not match.
	/// </summary>
	public const int TPM_E_BUFFER_LENGTH_MISMATCH = unchecked((int)0x8029041E);
	/// <summary>
	/// The RSA key creation is blocked on this TPM due to known security vulnerabilities.
	/// </summary>
	public const int TPM_E_PCP_IFX_RSA_KEY_CREATION_BLOCKED = unchecked((int)0x8029041F);
	/// <summary>
	/// A ticket required to use a key was not provided.
	/// </summary>
	public const int TPM_E_PCP_TICKET_MISSING = unchecked((int)0x80290420);
	/// <summary>
	/// This key has a raw policy so the KSP can't authenticate against it.
	/// </summary>
	public const int TPM_E_PCP_RAW_POLICY_NOT_SUPPORTED = unchecked((int)0x80290421);
	/// <summary>
	/// The TPM key's handle was unexpectedly invalidated due to a hardware or firmware issue.
	/// </summary>
	public const int TPM_E_PCP_KEY_HANDLE_INVALIDATED = unchecked((int)0x80290422);
	/// <summary>
	/// The requested salt size for signing with RSAPSS does not match what the TPM uses.
	/// </summary>
	public const int TPM_E_PCP_UNSUPPORTED_PSS_SALT = 0x40290423;
	/// <summary>
	/// Validation of the platform claim failed.
	/// </summary>
	public const int TPM_E_PCP_PLATFORM_CLAIM_MAY_BE_OUTDATED = 0x40290424;
	/// <summary>
	/// The requested platform claim is for a previous boot.
	/// </summary>
	public const int TPM_E_PCP_PLATFORM_CLAIM_OUTDATED = 0x40290425;
	/// <summary>
	/// The platform claim is for a previous boot, and cannot be created without reboot.
	/// </summary>
	public const int TPM_E_PCP_PLATFORM_CLAIM_REBOOT = 0x40290426;
	/// <summary>
	/// TPM related network operations are blocked as Zero Exhaust mode is enabled on client.
	/// </summary>
	public const int TPM_E_EXHAUST_ENABLED = unchecked((int)0x80290500);
	/// <summary>
	/// TPM provisioning did not run to completion.
	/// </summary>
	public const int TPM_E_PROVISIONING_INCOMPLETE = unchecked((int)0x80290600);
	/// <summary>
	/// An invalid owner authorization value was specified.
	/// </summary>
	public const int TPM_E_INVALID_OWNER_AUTH = unchecked((int)0x80290601);
	/// <summary>
	/// TPM command returned too much data.
	/// </summary>
	public const int TPM_E_TOO_MUCH_DATA = unchecked((int)0x80290602);
	/// <summary>
	/// Data Collector Set was not found.
	/// </summary>
	public const int PLA_E_DCS_NOT_FOUND = unchecked((int)0x80300002);
	/// <summary>
	/// The Data Collector Set or one of its dependencies is already in use.
	/// </summary>
	public const int PLA_E_DCS_IN_USE = unchecked((int)0x803000AA);
	/// <summary>
	/// Unable to start Data Collector Set because there are too many folders.
	/// </summary>
	public const int PLA_E_TOO_MANY_FOLDERS = unchecked((int)0x80300045);
	/// <summary>
	/// Not enough free disk space to start Data Collector Set.
	/// </summary>
	public const int PLA_E_NO_MIN_DISK = unchecked((int)0x80300070);
	/// <summary>
	/// Data Collector Set already exists.
	/// </summary>
	public const int PLA_E_DCS_ALREADY_EXISTS = unchecked((int)0x803000B7);
	/// <summary>
	/// Property value will be ignored.
	/// </summary>
	public const int PLA_S_PROPERTY_IGNORED = 0x00300100;
	/// <summary>
	/// Property value conflict.
	/// </summary>
	public const int PLA_E_PROPERTY_CONFLICT = unchecked((int)0x80300101);
	/// <summary>
	/// The current configuration for this Data Collector Set requires that it contain exactly one Data Collector.
	/// </summary>
	public const int PLA_E_DCS_SINGLETON_REQUIRED = unchecked((int)0x80300102);
	/// <summary>
	/// A user account is required in order to commit the current Data Collector Set properties.
	/// </summary>
	public const int PLA_E_CREDENTIALS_REQUIRED = unchecked((int)0x80300103);
	/// <summary>
	/// Data Collector Set is not running.
	/// </summary>
	public const int PLA_E_DCS_NOT_RUNNING = unchecked((int)0x80300104);
	/// <summary>
	/// A conflict was detected in the list of include/exclude APIs. Do not specify the same API in both the include list and the exclude list.
	/// </summary>
	public const int PLA_E_CONFLICT_INCL_EXCL_API = unchecked((int)0x80300105);
	/// <summary>
	/// The executable path you have specified refers to a network share or UNC path.
	/// </summary>
	public const int PLA_E_NETWORK_EXE_NOT_VALID = unchecked((int)0x80300106);
	/// <summary>
	/// The executable path you have specified is already configured for API tracing.
	/// </summary>
	public const int PLA_E_EXE_ALREADY_CONFIGURED = unchecked((int)0x80300107);
	/// <summary>
	/// The executable path you have specified does not exist. Verify that the specified path is correct.
	/// </summary>
	public const int PLA_E_EXE_PATH_NOT_VALID = unchecked((int)0x80300108);
	/// <summary>
	/// Data Collector already exists.
	/// </summary>
	public const int PLA_E_DC_ALREADY_EXISTS = unchecked((int)0x80300109);
	/// <summary>
	/// The wait for the Data Collector Set start notification has timed out.
	/// </summary>
	public const int PLA_E_DCS_START_WAIT_TIMEOUT = unchecked((int)0x8030010A);
	/// <summary>
	/// The wait for the Data Collector to start has timed out.
	/// </summary>
	public const int PLA_E_DC_START_WAIT_TIMEOUT = unchecked((int)0x8030010B);
	/// <summary>
	/// The wait for the report generation tool to finish has timed out.
	/// </summary>
	public const int PLA_E_REPORT_WAIT_TIMEOUT = unchecked((int)0x8030010C);
	/// <summary>
	/// Duplicate items are not allowed.
	/// </summary>
	public const int PLA_E_NO_DUPLICATES = unchecked((int)0x8030010D);
	/// <summary>
	/// When specifying the executable that you want to trace, you must specify a full path to the executable and not just a filename.
	/// </summary>
	public const int PLA_E_EXE_FULL_PATH_REQUIRED = unchecked((int)0x8030010E);
	/// <summary>
	/// The session name provided is invalid.
	/// </summary>
	public const int PLA_E_INVALID_SESSION_NAME = unchecked((int)0x8030010F);
	/// <summary>
	/// The Event Log channel Microsoft-Windows-Diagnosis-PLA/Operational must be enabled to perform this operation.
	/// </summary>
	public const int PLA_E_PLA_CHANNEL_NOT_ENABLED = unchecked((int)0x80300110);
	/// <summary>
	/// The Event Log channel Microsoft-Windows-TaskScheduler must be enabled to perform this operation.
	/// </summary>
	public const int PLA_E_TASKSCHED_CHANNEL_NOT_ENABLED = unchecked((int)0x80300111);
	/// <summary>
	/// The execution of the Rules Manager failed.
	/// </summary>
	public const int PLA_E_RULES_MANAGER_FAILED = unchecked((int)0x80300112);
	/// <summary>
	/// An error occurred while attempting to compress or extract the data.
	/// </summary>
	public const int PLA_E_CABAPI_FAILURE = unchecked((int)0x80300113);
	/// <summary>
	/// This drive is locked by BitLocker Drive Encryption. You must unlock this drive from Control Panel.
	/// </summary>
	public const int FVE_E_LOCKED_VOLUME = unchecked((int)0x80310000);
	/// <summary>
	/// The drive is not encrypted.
	/// </summary>
	public const int FVE_E_NOT_ENCRYPTED = unchecked((int)0x80310001);
	/// <summary>
	/// The BIOS did not correctly communicate with the Trusted Platform Module (TPM). Contact the computer manufacturer for BIOS upgrade instructions.
	/// </summary>
	public const int FVE_E_NO_TPM_BIOS = unchecked((int)0x80310002);
	/// <summary>
	/// The BIOS did not correctly communicate with the master boot record (MBR). Contact the computer manufacturer for BIOS upgrade instructions.
	/// </summary>
	public const int FVE_E_NO_MBR_METRIC = unchecked((int)0x80310003);
	/// <summary>
	/// A required TPM measurement is missing. If there is a bootable CD or DVD in your computer, remove it, restart the computer, and turn on BitLocker again. If the problem persists, ensure the master boot record is up to date.
	/// </summary>
	public const int FVE_E_NO_BOOTSECTOR_METRIC = unchecked((int)0x80310004);
	/// <summary>
	/// The boot sector of this drive is not compatible with BitLocker Drive Encryption. Use the Bootrec.exe tool in the Windows Recovery Environment to update or repair the boot manager (BOOTMGR).
	/// </summary>
	public const int FVE_E_NO_BOOTMGR_METRIC = unchecked((int)0x80310005);
	/// <summary>
	/// The boot manager of this operating system is not compatible with BitLocker Drive Encryption. Use the Bootrec.exe tool in the Windows Recovery Environment to update or repair the boot manager (BOOTMGR).
	/// </summary>
	public const int FVE_E_WRONG_BOOTMGR = unchecked((int)0x80310006);
	/// <summary>
	/// At least one secure key protector is required for this operation to be performed.
	/// </summary>
	public const int FVE_E_SECURE_KEY_REQUIRED = unchecked((int)0x80310007);
	/// <summary>
	/// BitLocker Drive Encryption is not enabled on this drive. Turn on BitLocker.
	/// </summary>
	public const int FVE_E_NOT_ACTIVATED = unchecked((int)0x80310008);
	/// <summary>
	/// BitLocker Drive Encryption cannot perform requested action. This condition may occur when two requests are issued at the same time. Wait a few moments and then try the action again.
	/// </summary>
	public const int FVE_E_ACTION_NOT_ALLOWED = unchecked((int)0x80310009);
	/// <summary>
	/// The Active Directory Domain Services forest does not contain the required attributes and classes to host BitLocker Drive Encryption or Trusted Platform Module information. Contact your domain administrator to verify that any required BitLocker Active Directory schema extensions have been installed.
	/// </summary>
	public const int FVE_E_AD_SCHEMA_NOT_INSTALLED = unchecked((int)0x8031000A);
	/// <summary>
	/// The type of the data obtained from Active Directory was not expected. The BitLocker recovery information may be missing or corrupted.
	/// </summary>
	public const int FVE_E_AD_INVALID_DATATYPE = unchecked((int)0x8031000B);
	/// <summary>
	/// The size of the data obtained from Active Directory was not expected. The BitLocker recovery information may be missing or corrupted.
	/// </summary>
	public const int FVE_E_AD_INVALID_DATASIZE = unchecked((int)0x8031000C);
	/// <summary>
	/// The attribute read from Active Directory does not contain any values. The BitLocker recovery information may be missing or corrupted.
	/// </summary>
	public const int FVE_E_AD_NO_VALUES = unchecked((int)0x8031000D);
	/// <summary>
	/// The attribute was not set. Verify that you are logged on with a domain account that has the ability to write information to Active Directory objects.
	/// </summary>
	public const int FVE_E_AD_ATTR_NOT_SET = unchecked((int)0x8031000E);
	/// <summary>
	/// The specified attribute cannot be found in Active Directory Domain Services. Contact your domain administrator to verify that any required BitLocker Active Directory schema extensions have been installed.
	/// </summary>
	public const int FVE_E_AD_GUID_NOT_FOUND = unchecked((int)0x8031000F);
	/// <summary>
	/// The BitLocker metadata for the encrypted drive is not valid. You can attempt to repair the drive to restore access.
	/// </summary>
	public const int FVE_E_BAD_INFORMATION = unchecked((int)0x80310010);
	/// <summary>
	/// The drive cannot be encrypted because it does not have enough free space. Delete any unnecessary data on the drive to create additional free space and then try again.
	/// </summary>
	public const int FVE_E_TOO_SMALL = unchecked((int)0x80310011);
	/// <summary>
	/// The drive cannot be encrypted because it contains system boot information. Create a separate partition for use as the system drive that contains the boot information and a second partition for use as the operating system drive and then encrypt the operating system drive.
	/// </summary>
	public const int FVE_E_SYSTEM_VOLUME = unchecked((int)0x80310012);
	/// <summary>
	/// The drive cannot be encrypted because the file system is not supported.
	/// </summary>
	public const int FVE_E_FAILED_WRONG_FS = unchecked((int)0x80310013);
	/// <summary>
	/// The file system size is larger than the partition size in the partition table. This drive may be corrupt or may have been tampered with. To use it with BitLocker, you must reformat the partition.
	/// </summary>
	public const int FVE_E_BAD_PARTITION_SIZE = unchecked((int)0x80310014);
	/// <summary>
	/// This drive cannot be encrypted.
	/// </summary>
	public const int FVE_E_NOT_SUPPORTED = unchecked((int)0x80310015);
	/// <summary>
	/// The data is not valid.
	/// </summary>
	public const int FVE_E_BAD_DATA = unchecked((int)0x80310016);
	/// <summary>
	/// The data drive specified is not set to automatically unlock on the current computer and cannot be unlocked automatically.
	/// </summary>
	public const int FVE_E_VOLUME_NOT_BOUND = unchecked((int)0x80310017);
	/// <summary>
	/// You must initialize the Trusted Platform Module (TPM) before you can use BitLocker Drive Encryption.
	/// </summary>
	public const int FVE_E_TPM_NOT_OWNED = unchecked((int)0x80310018);
	/// <summary>
	/// The operation attempted cannot be performed on an operating system drive. 
	/// </summary>
	public const int FVE_E_NOT_DATA_VOLUME = unchecked((int)0x80310019);
	/// <summary>
	/// The buffer supplied to a function was insufficient to contain the returned data. Increase the buffer size before running the function again.
	/// </summary>
	public const int FVE_E_AD_INSUFFICIENT_BUFFER = unchecked((int)0x8031001A);
	/// <summary>
	/// A read operation failed while converting the drive. The drive was not converted. Please re-enable BitLocker.
	/// </summary>
	public const int FVE_E_CONV_READ = unchecked((int)0x8031001B);
	/// <summary>
	/// A write operation failed while converting the drive. The drive was not converted. Please re-enable BitLocker.
	/// </summary>
	public const int FVE_E_CONV_WRITE = unchecked((int)0x8031001C);
	/// <summary>
	/// One or more BitLocker key protectors are required. You cannot delete the last key on this drive.
	/// </summary>
	public const int FVE_E_KEY_REQUIRED = unchecked((int)0x8031001D);
	/// <summary>
	/// Cluster configurations are not supported by BitLocker Drive Encryption.
	/// </summary>
	public const int FVE_E_CLUSTERING_NOT_SUPPORTED = unchecked((int)0x8031001E);
	/// <summary>
	/// The drive specified is already configured to be automatically unlocked on the current computer.
	/// </summary>
	public const int FVE_E_VOLUME_BOUND_ALREADY = unchecked((int)0x8031001F);
	/// <summary>
	/// The operating system drive is not protected by BitLocker Drive Encryption.
	/// </summary>
	public const int FVE_E_OS_NOT_PROTECTED = unchecked((int)0x80310020);
	/// <summary>
	/// BitLocker Drive Encryption has been suspended on this drive. All BitLocker key protectors configured for this drive are effectively disabled, and the drive will be automatically unlocked using an unencrypted (clear) key.
	/// </summary>
	public const int FVE_E_PROTECTION_DISABLED = unchecked((int)0x80310021);
	/// <summary>
	/// The drive you are attempting to lock does not have any key protectors available for encryption because BitLocker protection is currently suspended. Re-enable BitLocker to lock this drive.
	/// </summary>
	public const int FVE_E_RECOVERY_KEY_REQUIRED = unchecked((int)0x80310022);
	/// <summary>
	/// BitLocker cannot use the Trusted Platform Module (TPM) to protect a data drive. TPM protection can only be used with the operating system drive.
	/// </summary>
	public const int FVE_E_FOREIGN_VOLUME = unchecked((int)0x80310023);
	/// <summary>
	/// The BitLocker metadata for the encrypted drive cannot be updated because it was locked for updating by another process. Please try this process again.
	/// </summary>
	public const int FVE_E_OVERLAPPED_UPDATE = unchecked((int)0x80310024);
	/// <summary>
	/// The authorization data for the storage root key (SRK) of the Trusted Platform Module (TPM) is not zero and is therefore incompatible with BitLocker. Please initialize the TPM before attempting to use it with BitLocker.
	/// </summary>
	public const int FVE_E_TPM_SRK_AUTH_NOT_ZERO = unchecked((int)0x80310025);
	/// <summary>
	/// The drive encryption algorithm cannot be used on this sector size.
	/// </summary>
	public const int FVE_E_FAILED_SECTOR_SIZE = unchecked((int)0x80310026);
	/// <summary>
	/// The drive cannot be unlocked with the key provided. Confirm that you have provided the correct key and try again.
	/// </summary>
	public const int FVE_E_FAILED_AUTHENTICATION = unchecked((int)0x80310027);
	/// <summary>
	/// The drive specified is not the operating system drive.
	/// </summary>
	public const int FVE_E_NOT_OS_VOLUME = unchecked((int)0x80310028);
	/// <summary>
	/// BitLocker Drive Encryption cannot be turned off on the operating system drive until the auto unlock feature has been disabled for the fixed data drives and removable data drives associated with this computer.
	/// </summary>
	public const int FVE_E_AUTOUNLOCK_ENABLED = unchecked((int)0x80310029);
	/// <summary>
	/// The system partition boot sector does not perform Trusted Platform Module (TPM) measurements. Use the Bootrec.exe tool in the Windows Recovery Environment to update or repair the boot sector.
	/// </summary>
	public const int FVE_E_WRONG_BOOTSECTOR = unchecked((int)0x8031002A);
	/// <summary>
	/// BitLocker Drive Encryption operating system drives must be formatted with the NTFS file system in order to be encrypted. Convert the drive to NTFS, and then turn on BitLocker.
	/// </summary>
	public const int FVE_E_WRONG_SYSTEM_FS = unchecked((int)0x8031002B);
	/// <summary>
	/// Group Policy settings require that a recovery password be specified before encrypting the drive.
	/// </summary>
	public const int FVE_E_POLICY_PASSWORD_REQUIRED = unchecked((int)0x8031002C);
	/// <summary>
	/// The drive encryption algorithm and key cannot be set on a previously encrypted drive. To encrypt this drive with BitLocker Drive Encryption, remove the previous encryption and then turn on BitLocker.
	/// </summary>
	public const int FVE_E_CANNOT_SET_FVEK_ENCRYPTED = unchecked((int)0x8031002D);
	/// <summary>
	/// BitLocker Drive Encryption cannot encrypt the specified drive because an encryption key is not available. Add a key protector to encrypt this drive.
	/// </summary>
	public const int FVE_E_CANNOT_ENCRYPT_NO_KEY = unchecked((int)0x8031002E);
	/// <summary>
	/// BitLocker Drive Encryption detected bootable media (CD or DVD) in the computer. Remove the media and restart the computer before configuring BitLocker.
	/// </summary>
	public const int FVE_E_BOOTABLE_CDDVD = unchecked((int)0x80310030);
	/// <summary>
	/// This key protector cannot be added. Only one key protector of this type is allowed for this drive.
	/// </summary>
	public const int FVE_E_PROTECTOR_EXISTS = unchecked((int)0x80310031);
	/// <summary>
	/// The recovery password file was not found because a relative path was specified. Recovery passwords must be saved to a fully qualified path. Environment variables configured on the computer can be used in the path.
	/// </summary>
	public const int FVE_E_RELATIVE_PATH = unchecked((int)0x80310032);
	/// <summary>
	/// The specified key protector was not found on the drive. Try another key protector.
	/// </summary>
	public const int FVE_E_PROTECTOR_NOT_FOUND = unchecked((int)0x80310033);
	/// <summary>
	/// The recovery key provided is corrupt and cannot be used to access the drive. An alternative recovery method, such as recovery password, a data recovery agent, or a backup version of the recovery key must be used to recover access to the drive.
	/// </summary>
	public const int FVE_E_INVALID_KEY_FORMAT = unchecked((int)0x80310034);
	/// <summary>
	/// The format of the recovery password provided is invalid. BitLocker recovery passwords are 48 digits. Verify that the recovery password is in the correct format and then try again.
	/// </summary>
	public const int FVE_E_INVALID_PASSWORD_FORMAT = unchecked((int)0x80310035);
	/// <summary>
	/// The random number generator check test failed.
	/// </summary>
	public const int FVE_E_FIPS_RNG_CHECK_FAILED = unchecked((int)0x80310036);
	/// <summary>
	/// The Group Policy setting requiring FIPS compliance prevents a local recovery password from being generated or used by BitLocker Drive Encryption. When operating in FIPS-compliant mode, BitLocker recovery options can be either a recovery key stored on a USB drive or recovery through a data recovery agent.
	/// </summary>
	public const int FVE_E_FIPS_PREVENTS_RECOVERY_PASSWORD = unchecked((int)0x80310037);
	/// <summary>
	/// The Group Policy setting requiring FIPS compliance prevents the recovery password from being saved to Active Directory. When operating in FIPS-compliant mode, BitLocker recovery options can be either a recovery key stored on a USB drive or recovery through a data recovery agent. Check your Group Policy settings configuration.
	/// </summary>
	public const int FVE_E_FIPS_PREVENTS_EXTERNAL_KEY_EXPORT = unchecked((int)0x80310038);
	/// <summary>
	/// The drive must be fully decrypted to complete this operation.
	/// </summary>
	public const int FVE_E_NOT_DECRYPTED = unchecked((int)0x80310039);
	/// <summary>
	/// The key protector specified cannot be used for this operation.
	/// </summary>
	public const int FVE_E_INVALID_PROTECTOR_TYPE = unchecked((int)0x8031003A);
	/// <summary>
	/// No key protectors exist on the drive to perform the hardware test.
	/// </summary>
	public const int FVE_E_NO_PROTECTORS_TO_TEST = unchecked((int)0x8031003B);
	/// <summary>
	/// The BitLocker startup key or recovery password cannot be found on the USB device. Verify that you have the correct USB device, that the USB device is plugged into the computer on an active USB port, restart the computer, and then try again. If the problem persists, contact the computer manufacturer for BIOS upgrade instructions.
	/// </summary>
	public const int FVE_E_KEYFILE_NOT_FOUND = unchecked((int)0x8031003C);
	/// <summary>
	/// The BitLocker startup key or recovery password file provided is corrupt or invalid. Verify that you have the correct startup key or recovery password file and try again.
	/// </summary>
	public const int FVE_E_KEYFILE_INVALID = unchecked((int)0x8031003D);
	/// <summary>
	/// The BitLocker encryption key cannot be obtained from the startup key or recovery password. Verify that you have the correct startup key or recovery password and try again.
	/// </summary>
	public const int FVE_E_KEYFILE_NO_VMK = unchecked((int)0x8031003E);
	/// <summary>
	/// The Trusted Platform Module (TPM) is disabled. The TPM must be enabled, initialized, and have valid ownership before it can be used with BitLocker Drive Encryption.
	/// </summary>
	public const int FVE_E_TPM_DISABLED = unchecked((int)0x8031003F);
	/// <summary>
	/// The BitLocker configuration of the specified drive cannot be managed because this computer is currently operating in Safe Mode. While in Safe Mode, BitLocker Drive Encryption can only be used for recovery purposes.
	/// </summary>
	public const int FVE_E_NOT_ALLOWED_IN_SAFE_MODE = unchecked((int)0x80310040);
	/// <summary>
	/// The Trusted Platform Module (TPM) was not able to unlock the drive because the system boot information has changed or a PIN was not provided correctly. Verify that the drive has not been tampered with and that changes to the system boot information were caused by a trusted source. After verifying that the drive is safe to access, use the BitLocker recovery console to unlock the drive and then suspend and resume BitLocker to update system boot information that BitLocker associates with this drive.
	/// </summary>
	public const int FVE_E_TPM_INVALID_PCR = unchecked((int)0x80310041);
	/// <summary>
	/// The BitLocker encryption key cannot be obtained from the Trusted Platform Module (TPM).
	/// </summary>
	public const int FVE_E_TPM_NO_VMK = unchecked((int)0x80310042);
	/// <summary>
	/// The BitLocker encryption key cannot be obtained from the Trusted Platform Module (TPM) and PIN.
	/// </summary>
	public const int FVE_E_PIN_INVALID = unchecked((int)0x80310043);
	/// <summary>
	/// A boot application has changed since BitLocker Drive Encryption was enabled.
	/// </summary>
	public const int FVE_E_AUTH_INVALID_APPLICATION = unchecked((int)0x80310044);
	/// <summary>
	/// The Boot Configuration Data (BCD) settings have changed since BitLocker Drive Encryption was enabled.
	/// </summary>
	public const int FVE_E_AUTH_INVALID_CONFIG = unchecked((int)0x80310045);
	/// <summary>
	/// The Group Policy setting requiring FIPS compliance prohibits the use of unencrypted keys, which prevents BitLocker from being suspended on this drive. Please contact your domain administrator for more information.
	/// </summary>
	public const int FVE_E_FIPS_DISABLE_PROTECTION_NOT_ALLOWED = unchecked((int)0x80310046);
	/// <summary>
	/// This drive cannot be encrypted by BitLocker Drive Encryption because the file system does not extend to the end of the drive. Repartition this drive and then try again.
	/// </summary>
	public const int FVE_E_FS_NOT_EXTENDED = unchecked((int)0x80310047);
	/// <summary>
	/// BitLocker Drive Encryption cannot be enabled on the operating system drive. Contact the computer manufacturer for BIOS upgrade instructions.
	/// </summary>
	public const int FVE_E_FIRMWARE_TYPE_NOT_SUPPORTED = unchecked((int)0x80310048);
	/// <summary>
	/// This version of Windows does not include BitLocker Drive Encryption. To use BitLocker Drive Encryption, please upgrade the operating system.
	/// </summary>
	public const int FVE_E_NO_LICENSE = unchecked((int)0x80310049);
	/// <summary>
	/// BitLocker Drive Encryption cannot be used because critical BitLocker system files are missing or corrupted. Use Windows Startup Repair to restore these files to your computer.
	/// </summary>
	public const int FVE_E_NOT_ON_STACK = unchecked((int)0x8031004A);
	/// <summary>
	/// The drive cannot be locked when the drive is in use.
	/// </summary>
	public const int FVE_E_FS_MOUNTED = unchecked((int)0x8031004B);
	/// <summary>
	/// The access token associated with the current thread is not an impersonated token.
	/// </summary>
	public const int FVE_E_TOKEN_NOT_IMPERSONATED = unchecked((int)0x8031004C);
	/// <summary>
	/// The BitLocker encryption key cannot be obtained. Verify that the Trusted Platform Module (TPM) is enabled and ownership has been taken. If this computer does not have a TPM, verify that the USB drive is inserted and available.
	/// </summary>
	public const int FVE_E_DRY_RUN_FAILED = unchecked((int)0x8031004D);
	/// <summary>
	/// You must restart your computer before continuing with BitLocker Drive Encryption.
	/// </summary>
	public const int FVE_E_REBOOT_REQUIRED = unchecked((int)0x8031004E);
	/// <summary>
	/// Drive encryption cannot occur while boot debugging is enabled. Use the bcdedit command-line tool to turn off boot debugging.
	/// </summary>
	public const int FVE_E_DEBUGGER_ENABLED = unchecked((int)0x8031004F);
	/// <summary>
	/// No action was taken as BitLocker Drive Encryption is in raw access mode.
	/// </summary>
	public const int FVE_E_RAW_ACCESS = unchecked((int)0x80310050);
	/// <summary>
	/// BitLocker Drive Encryption cannot enter raw access mode for this drive because the drive is currently in use.
	/// </summary>
	public const int FVE_E_RAW_BLOCKED = unchecked((int)0x80310051);
	/// <summary>
	/// The path specified in the Boot Configuration Data (BCD) for a BitLocker Drive Encryption integrity-protected application is incorrect. Please verify and correct your BCD settings and try again.
	/// </summary>
	public const int FVE_E_BCD_APPLICATIONS_PATH_INCORRECT = unchecked((int)0x80310052);
	/// <summary>
	/// BitLocker Drive Encryption can only be used for limited provisioning or recovery purposes when the computer is running in pre-installation or recovery environments.
	/// </summary>
	public const int FVE_E_NOT_ALLOWED_IN_VERSION = unchecked((int)0x80310053);
	/// <summary>
	/// The auto-unlock master key was not available from the operating system drive.
	/// </summary>
	public const int FVE_E_NO_AUTOUNLOCK_MASTER_KEY = unchecked((int)0x80310054);
	/// <summary>
	/// The system firmware failed to enable clearing of system memory when the computer was restarted.
	/// </summary>
	public const int FVE_E_MOR_FAILED = unchecked((int)0x80310055);
	/// <summary>
	/// The hidden drive cannot be encrypted.
	/// </summary>
	public const int FVE_E_HIDDEN_VOLUME = unchecked((int)0x80310056);
	/// <summary>
	/// BitLocker encryption keys were ignored because the drive was in a transient state.
	/// </summary>
	public const int FVE_E_TRANSIENT_STATE = unchecked((int)0x80310057);
	/// <summary>
	/// Public key based protectors are not allowed on this drive.
	/// </summary>
	public const int FVE_E_PUBKEY_NOT_ALLOWED = unchecked((int)0x80310058);
	/// <summary>
	/// BitLocker Drive Encryption is already performing an operation on this drive. Please complete all operations before continuing.
	/// </summary>
	public const int FVE_E_VOLUME_HANDLE_OPEN = unchecked((int)0x80310059);
	/// <summary>
	/// This version of Windows does not support this feature of BitLocker Drive Encryption. To use this feature, upgrade the operating system.
	/// </summary>
	public const int FVE_E_NO_FEATURE_LICENSE = unchecked((int)0x8031005A);
	/// <summary>
	/// The Group Policy settings for BitLocker startup options are in conflict and cannot be applied. Contact your system administrator for more information.
	/// </summary>
	public const int FVE_E_INVALID_STARTUP_OPTIONS = unchecked((int)0x8031005B);
	/// <summary>
	/// Group policy settings do not permit the creation of a recovery password.
	/// </summary>
	public const int FVE_E_POLICY_RECOVERY_PASSWORD_NOT_ALLOWED = unchecked((int)0x8031005C);
	/// <summary>
	/// Group policy settings require the creation of a recovery password.
	/// </summary>
	public const int FVE_E_POLICY_RECOVERY_PASSWORD_REQUIRED = unchecked((int)0x8031005D);
	/// <summary>
	/// Group policy settings do not permit the creation of a recovery key.
	/// </summary>
	public const int FVE_E_POLICY_RECOVERY_KEY_NOT_ALLOWED = unchecked((int)0x8031005E);
	/// <summary>
	/// Group policy settings require the creation of a recovery key.
	/// </summary>
	public const int FVE_E_POLICY_RECOVERY_KEY_REQUIRED = unchecked((int)0x8031005F);
	/// <summary>
	/// Group policy settings do not permit the use of a PIN at startup. Please choose a different BitLocker startup option.
	/// </summary>
	public const int FVE_E_POLICY_STARTUP_PIN_NOT_ALLOWED = unchecked((int)0x80310060);
	/// <summary>
	/// Group policy settings require the use of a PIN at startup. Please choose this BitLocker startup option.
	/// </summary>
	public const int FVE_E_POLICY_STARTUP_PIN_REQUIRED = unchecked((int)0x80310061);
	/// <summary>
	/// Group policy settings do not permit the use of a startup key. Please choose a different BitLocker startup option.
	/// </summary>
	public const int FVE_E_POLICY_STARTUP_KEY_NOT_ALLOWED = unchecked((int)0x80310062);
	/// <summary>
	/// Group policy settings require the use of a startup key. Please choose this BitLocker startup option.
	/// </summary>
	public const int FVE_E_POLICY_STARTUP_KEY_REQUIRED = unchecked((int)0x80310063);
	/// <summary>
	/// Group policy settings do not permit the use of a startup key and PIN. Please choose a different BitLocker startup option.
	/// </summary>
	public const int FVE_E_POLICY_STARTUP_PIN_KEY_NOT_ALLOWED = unchecked((int)0x80310064);
	/// <summary>
	/// Group policy settings require the use of a startup key and PIN. Please choose this BitLocker startup option.
	/// </summary>
	public const int FVE_E_POLICY_STARTUP_PIN_KEY_REQUIRED = unchecked((int)0x80310065);
	/// <summary>
	/// Group policy does not permit the use of TPM-only at startup. Please choose a different BitLocker startup option.
	/// </summary>
	public const int FVE_E_POLICY_STARTUP_TPM_NOT_ALLOWED = unchecked((int)0x80310066);
	/// <summary>
	/// Group policy settings require the use of TPM-only at startup. Please choose this BitLocker startup option.
	/// </summary>
	public const int FVE_E_POLICY_STARTUP_TPM_REQUIRED = unchecked((int)0x80310067);
	/// <summary>
	/// The PIN provided does not meet minimum or maximum length requirements.
	/// </summary>
	public const int FVE_E_POLICY_INVALID_PIN_LENGTH = unchecked((int)0x80310068);
	/// <summary>
	/// The key protector is not supported by the version of BitLocker Drive Encryption currently on the drive. Upgrade the drive to add the key protector.
	/// </summary>
	public const int FVE_E_KEY_PROTECTOR_NOT_SUPPORTED = unchecked((int)0x80310069);
	/// <summary>
	/// Group policy settings do not permit the creation of a password.
	/// </summary>
	public const int FVE_E_POLICY_PASSPHRASE_NOT_ALLOWED = unchecked((int)0x8031006A);
	/// <summary>
	/// Group policy settings require the creation of a password.
	/// </summary>
	public const int FVE_E_POLICY_PASSPHRASE_REQUIRED = unchecked((int)0x8031006B);
	/// <summary>
	/// The group policy setting requiring FIPS compliance prevented the password from being generated or used. Please contact your domain administrator for more information.
	/// </summary>
	public const int FVE_E_FIPS_PREVENTS_PASSPHRASE = unchecked((int)0x8031006C);
	/// <summary>
	/// A password cannot be added to the operating system drive.
	/// </summary>
	public const int FVE_E_OS_VOLUME_PASSPHRASE_NOT_ALLOWED = unchecked((int)0x8031006D);
	/// <summary>
	/// The BitLocker object identifier (OID) on the drive appears to be invalid or corrupt. Use manage-BDE to reset the OID on this drive.
	/// </summary>
	public const int FVE_E_INVALID_BITLOCKER_OID = unchecked((int)0x8031006E);
	/// <summary>
	/// The drive is too small to be protected using BitLocker Drive Encryption.
	/// </summary>
	public const int FVE_E_VOLUME_TOO_SMALL = unchecked((int)0x8031006F);
	/// <summary>
	/// The selected discovery drive type is incompatible with the file system on the drive. BitLocker To Go discovery drives must be created on FAT formatted drives.
	/// </summary>
	public const int FVE_E_DV_NOT_SUPPORTED_ON_FS = unchecked((int)0x80310070);
	/// <summary>
	/// The selected discovery drive type is not allowed by the computer's Group Policy settings. Verify that Group Policy settings allow the creation of discovery drives for use with BitLocker To Go.
	/// </summary>
	public const int FVE_E_DV_NOT_ALLOWED_BY_GP = unchecked((int)0x80310071);
	/// <summary>
	/// Group Policy settings do not permit user certificates such as smart cards to be used with BitLocker Drive Encryption.
	/// </summary>
	public const int FVE_E_POLICY_USER_CERTIFICATE_NOT_ALLOWED = unchecked((int)0x80310072);
	/// <summary>
	/// Group Policy settings require that you have a valid user certificate, such as a smart card, to be used with BitLocker Drive Encryption.
	/// </summary>
	public const int FVE_E_POLICY_USER_CERTIFICATE_REQUIRED = unchecked((int)0x80310073);
	/// <summary>
	/// Group Policy settings requires that you use a smart card-based key protector with BitLocker Drive Encryption.
	/// </summary>
	public const int FVE_E_POLICY_USER_CERT_MUST_BE_HW = unchecked((int)0x80310074);
	/// <summary>
	/// Group Policy settings do not permit BitLocker-protected fixed data drives to be automatically unlocked.
	/// </summary>
	public const int FVE_E_POLICY_USER_CONFIGURE_FDV_AUTOUNLOCK_NOT_ALLOWED = unchecked((int)0x80310075);
	/// <summary>
	/// Group Policy settings do not permit BitLocker-protected removable data drives to be automatically unlocked.
	/// </summary>
	public const int FVE_E_POLICY_USER_CONFIGURE_RDV_AUTOUNLOCK_NOT_ALLOWED = unchecked((int)0x80310076);
	/// <summary>
	/// Group Policy settings do not permit you to configure BitLocker Drive Encryption on removable data drives.
	/// </summary>
	public const int FVE_E_POLICY_USER_CONFIGURE_RDV_NOT_ALLOWED = unchecked((int)0x80310077);
	/// <summary>
	/// Group Policy settings do not permit you to turn on BitLocker Drive Encryption on removable data drives. Please contact your system administrator if you need to turn on BitLocker.
	/// </summary>
	public const int FVE_E_POLICY_USER_ENABLE_RDV_NOT_ALLOWED = unchecked((int)0x80310078);
	/// <summary>
	/// Group Policy settings do not permit turning off BitLocker Drive Encryption on removable data drives. Please contact your system administrator if you need to turn off BitLocker.
	/// </summary>
	public const int FVE_E_POLICY_USER_DISABLE_RDV_NOT_ALLOWED = unchecked((int)0x80310079);
	/// <summary>
	/// Your password does not meet minimum password length requirements. By default, passwords must be at least 8 characters in length. Check with your system administrator for the password length requirement in your organization.
	/// </summary>
	public const int FVE_E_POLICY_INVALID_PASSPHRASE_LENGTH = unchecked((int)0x80310080);
	/// <summary>
	/// Your password does not meet the complexity requirements set by your system administrator. Try adding upper and lowercase characters, numbers, and symbols.
	/// </summary>
	public const int FVE_E_POLICY_PASSPHRASE_TOO_SIMPLE = unchecked((int)0x80310081);
	/// <summary>
	/// This drive cannot be encrypted because it is reserved for Windows System Recovery Options.
	/// </summary>
	public const int FVE_E_RECOVERY_PARTITION = unchecked((int)0x80310082);
	/// <summary>
	/// BitLocker Drive Encryption cannot be applied to this drive because of conflicting Group Policy settings. BitLocker cannot be configured to automatically unlock fixed data drives when user recovery options are disabled. If you want BitLocker-protected fixed data drives to be automatically unlocked after key validation has occurred, please ask your system administrator to resolve the settings conflict before enabling BitLocker.
	/// </summary>
	public const int FVE_E_POLICY_CONFLICT_FDV_RK_OFF_AUK_ON = unchecked((int)0x80310083);
	/// <summary>
	/// BitLocker Drive Encryption cannot be applied to this drive because of conflicting Group Policy settings. BitLocker cannot be configured to automatically unlock removable data drives when user recovery option are disabled. If you want BitLocker-protected removable data drives to be automatically unlocked after key validation has occured, please ask your system administrator to resolve the settings conflict before enabling BitLocker.
	/// </summary>
	public const int FVE_E_POLICY_CONFLICT_RDV_RK_OFF_AUK_ON = unchecked((int)0x80310084);
	/// <summary>
	/// The Enhanced Key Usage (EKU) attribute of the specified certificate does not permit it to be used for BitLocker Drive Encryption. BitLocker does not require that a certificate have an EKU attribute, but if one is configured it must be set to an object identifier (OID) that matches the OID configured for BitLocker.
	/// </summary>
	public const int FVE_E_NON_BITLOCKER_OID = unchecked((int)0x80310085);
	/// <summary>
	/// BitLocker Drive Encryption cannot be applied to this drive as currently configured because of Group Policy settings. The certificate you provided for drive encryption is self-signed. Current Group Policy settings do not permit the use of self-signed certificates. Obtain a new certificate from your certification authority before attempting to enable BitLocker. 
	/// </summary>
	public const int FVE_E_POLICY_PROHIBITS_SELFSIGNED = unchecked((int)0x80310086);
	/// <summary>
	/// BitLocker Encryption cannot be applied to this drive because of conflicting Group Policy settings. When write access to drives not protected by BitLocker is denied, the use of a USB startup key cannot be required. Please have your system administrator resolve these policy conflicts before attempting to enable BitLocker. 
	/// </summary>
	public const int FVE_E_POLICY_CONFLICT_RO_AND_STARTUP_KEY_REQUIRED = unchecked((int)0x80310087);
	/// <summary>
	/// BitLocker Drive Encryption cannot be applied to this drive because there are conflicting Group Policy settings for recovery options on operating system drives. Storing recovery information to Active Directory Domain Services cannot be required when the generation of recovery passwords is not permitted. Please have your system administrator resolve these policy conflicts before attempting to enable BitLocker. 
	/// </summary>
	public const int FVE_E_CONV_RECOVERY_FAILED = unchecked((int)0x80310088);
	/// <summary>
	/// The requested virtualization size is too big.
	/// </summary>
	public const int FVE_E_VIRTUALIZED_SPACE_TOO_BIG = unchecked((int)0x80310089);
	/// <summary>
	/// BitLocker Drive Encryption cannot be applied to this drive because there are conflicting Group Policy settings for recovery options on operating system drives. Storing recovery information to Active Directory Domain Services cannot be required when the generation of recovery passwords is not permitted. Please have your system administrator resolve these policy conflicts before attempting to enable BitLocker.
	/// </summary>
	public const int FVE_E_POLICY_CONFLICT_OSV_RP_OFF_ADB_ON = unchecked((int)0x80310090);
	/// <summary>
	/// BitLocker Drive Encryption cannot be applied to this drive because there are conflicting Group Policy settings for recovery options on fixed data drives. Storing recovery information to Active Directory Domain Services cannot be required when the generation of recovery passwords is not permitted. Please have your system administrator resolve these policy conflicts before attempting to enable BitLocker.
	/// </summary>
	public const int FVE_E_POLICY_CONFLICT_FDV_RP_OFF_ADB_ON = unchecked((int)0x80310091);
	/// <summary>
	/// BitLocker Drive Encryption cannot be applied to this drive because there are conflicting Group Policy settings for recovery options on removable data drives. Storing recovery information to Active Directory Domain Services cannot be required when the generation of recovery passwords is not permitted. Please have your system administrator resolve these policy conflicts before attempting to enable BitLocker. 
	/// </summary>
	public const int FVE_E_POLICY_CONFLICT_RDV_RP_OFF_ADB_ON = unchecked((int)0x80310092);
	/// <summary>
	/// The Key Usage (KU) attribute of the specified certificate does not permit it to be used for BitLocker Drive Encryption. BitLocker does not require that a certificate have a KU attribute, but if one is configured it must be set to either Key Encipherment or Key Agreement.
	/// </summary>
	public const int FVE_E_NON_BITLOCKER_KU = unchecked((int)0x80310093);
	/// <summary>
	/// The private key associated with the specified certificate cannot be authorized. The private key authorization was either not provided or the provided authorization was invalid.
	/// </summary>
	public const int FVE_E_PRIVATEKEY_AUTH_FAILED = unchecked((int)0x80310094);
	/// <summary>
	/// Removal of the data recovery agent certificate must be done using the Certificates snap-in.
	/// </summary>
	public const int FVE_E_REMOVAL_OF_DRA_FAILED = unchecked((int)0x80310095);
	/// <summary>
	/// This drive was encrypted using the version of BitLocker Drive Encryption included with Windows Vista and Windows Server 2008 which does not support organizational identifiers. To specify organizational identifiers for this drive upgrade the drive encryption to the latest version using the "manage-bde -upgrade" command.
	/// </summary>
	public const int FVE_E_OPERATION_NOT_SUPPORTED_ON_VISTA_VOLUME = unchecked((int)0x80310096);
	/// <summary>
	/// The drive cannot be locked because it is automatically unlocked on this computer. Remove the automatic unlock protector to lock this drive.
	/// </summary>
	public const int FVE_E_CANT_LOCK_AUTOUNLOCK_ENABLED_VOLUME = unchecked((int)0x80310097);
	/// <summary>
	/// The default Bitlocker Key Derivation Function SP800-56A for ECC smart cards is not supported by your smart card. The Group Policy setting requiring FIPS-compliance prevents BitLocker from using any other key derivation function for encryption. You have to use a FIPS compliant smart card in FIPS restricted environments.
	/// </summary>
	public const int FVE_E_FIPS_HASH_KDF_NOT_ALLOWED = unchecked((int)0x80310098);
	/// <summary>
	/// The BitLocker encryption key could not be obtained from the Trusted Platform Module (TPM) and enhanced PIN. Try using a PIN containing only numerals.
	/// </summary>
	public const int FVE_E_ENH_PIN_INVALID = unchecked((int)0x80310099);
	/// <summary>
	/// The requested TPM PIN contains invalid characters.
	/// </summary>
	public const int FVE_E_INVALID_PIN_CHARS = unchecked((int)0x8031009A);
	/// <summary>
	/// The management information stored on the drive contained an unknown type. If you are using an old version of Windows, try accessing the drive from the latest version.
	/// </summary>
	public const int FVE_E_INVALID_DATUM_TYPE = unchecked((int)0x8031009B);
	/// <summary>
	/// The feature is only supported on EFI systems.
	/// </summary>
	public const int FVE_E_EFI_ONLY = unchecked((int)0x8031009C);
	/// <summary>
	/// More than one Network Key Protector certificate has been found on the system.
	/// </summary>
	public const int FVE_E_MULTIPLE_NKP_CERTS = unchecked((int)0x8031009D);
	/// <summary>
	/// Removal of the Network Key Protector certificate must be done using the Certificates snap-in.
	/// </summary>
	public const int FVE_E_REMOVAL_OF_NKP_FAILED = unchecked((int)0x8031009E);
	/// <summary>
	/// An invalid certificate has been found in the Network Key Protector certificate store.
	/// </summary>
	public const int FVE_E_INVALID_NKP_CERT = unchecked((int)0x8031009F);
	/// <summary>
	/// This drive isn't protected with a PIN.
	/// </summary>
	public const int FVE_E_NO_EXISTING_PIN = unchecked((int)0x803100A0);
	/// <summary>
	/// Please enter the correct current PIN.
	/// </summary>
	public const int FVE_E_PROTECTOR_CHANGE_PIN_MISMATCH = unchecked((int)0x803100A1);
	/// <summary>
	/// You must be logged on with an administrator account to change the PIN or password. Click the link to reset the PIN or password as an administrator.
	/// </summary>
	public const int FVE_E_PROTECTOR_CHANGE_BY_STD_USER_DISALLOWED = unchecked((int)0x803100A2);
	/// <summary>
	/// BitLocker has disabled PIN and password changes after too many failed requests. Click the link to reset the PIN or password as an administrator.
	/// </summary>
	public const int FVE_E_PROTECTOR_CHANGE_MAX_PIN_CHANGE_ATTEMPTS_REACHED = unchecked((int)0x803100A3);
	/// <summary>
	/// Your system administrator requires that passwords contain only printable ASCII characters. This includes unaccented letters (A-Z, a-z), numbers (0-9), space, arithmetic signs, common punctuation, separators, and the following symbols: # $ &amp; @ ^ _ ~ .
	/// </summary>
	public const int FVE_E_POLICY_PASSPHRASE_REQUIRES_ASCII = unchecked((int)0x803100A4);
	/// <summary>
	/// BitLocker Drive Encryption only supports used space only encryption on thin provisioned storage.
	/// </summary>
	public const int FVE_E_FULL_ENCRYPTION_NOT_ALLOWED_ON_TP_STORAGE = unchecked((int)0x803100A5);
	/// <summary>
	/// BitLocker Drive Encryption does not support wiping free space on thin provisioned storage.
	/// </summary>
	public const int FVE_E_WIPE_NOT_ALLOWED_ON_TP_STORAGE = unchecked((int)0x803100A6);
	/// <summary>
	/// The required authentication key length is not supported by the drive.
	/// </summary>
	public const int FVE_E_KEY_LENGTH_NOT_SUPPORTED_BY_EDRIVE = unchecked((int)0x803100A7);
	/// <summary>
	/// This drive isn't protected with a password.
	/// </summary>
	public const int FVE_E_NO_EXISTING_PASSPHRASE = unchecked((int)0x803100A8);
	/// <summary>
	/// Please enter the correct current password.
	/// </summary>
	public const int FVE_E_PROTECTOR_CHANGE_PASSPHRASE_MISMATCH = unchecked((int)0x803100A9);
	/// <summary>
	/// The password cannot exceed 256 characters.
	/// </summary>
	public const int FVE_E_PASSPHRASE_TOO_LONG = unchecked((int)0x803100AA);
	/// <summary>
	/// A password key protector cannot be added because a TPM protector exists on the drive.
	/// </summary>
	public const int FVE_E_NO_PASSPHRASE_WITH_TPM = unchecked((int)0x803100AB);
	/// <summary>
	/// A TPM key protector cannot be added because a password protector exists on the drive.
	/// </summary>
	public const int FVE_E_NO_TPM_WITH_PASSPHRASE = unchecked((int)0x803100AC);
	/// <summary>
	/// This command can only be performed from the coordinator node for the specified CSV volume.
	/// </summary>
	public const int FVE_E_NOT_ALLOWED_ON_CSV_STACK = unchecked((int)0x803100AD);
	/// <summary>
	/// This command cannot be performed on a volume when it is part of a cluster.
	/// </summary>
	public const int FVE_E_NOT_ALLOWED_ON_CLUSTER = unchecked((int)0x803100AE);
	/// <summary>
	/// BitLocker did not revert to using BitLocker software encryption due to group policy configuration.
	/// </summary>
	public const int FVE_E_EDRIVE_NO_FAILOVER_TO_SW = unchecked((int)0x803100AF);
	/// <summary>
	/// The drive cannot be managed by BitLocker because the drive's hardware encryption feature is already in use.
	/// </summary>
	public const int FVE_E_EDRIVE_BAND_IN_USE = unchecked((int)0x803100B0);
	/// <summary>
	/// Group Policy settings do not allow the use of hardware-based encryption.
	/// </summary>
	public const int FVE_E_EDRIVE_DISALLOWED_BY_GP = unchecked((int)0x803100B1);
	/// <summary>
	/// The drive specified does not support hardware-based encryption.
	/// </summary>
	public const int FVE_E_EDRIVE_INCOMPATIBLE_VOLUME = unchecked((int)0x803100B2);
	/// <summary>
	/// BitLocker cannot be upgraded during disk encryption or decryption.
	/// </summary>
	public const int FVE_E_NOT_ALLOWED_TO_UPGRADE_WHILE_CONVERTING = unchecked((int)0x803100B3);
	/// <summary>
	/// Discovery Volumes are not supported for volumes using hardware encryption.
	/// </summary>
	public const int FVE_E_EDRIVE_DV_NOT_SUPPORTED = unchecked((int)0x803100B4);
	/// <summary>
	/// No pre-boot keyboard detected. The user may not be able to provide required input to unlock the volume.
	/// </summary>
	public const int FVE_E_NO_PREBOOT_KEYBOARD_DETECTED = unchecked((int)0x803100B5);
	/// <summary>
	/// No pre-boot keyboard or Windows Recovery Environment detected. The user may not be able to provide required input to unlock the volume.
	/// </summary>
	public const int FVE_E_NO_PREBOOT_KEYBOARD_OR_WINRE_DETECTED = unchecked((int)0x803100B6);
	/// <summary>
	/// Group Policy settings require the creation of a startup PIN, but a pre-boot keyboard is not available on this device. The user may not be able to provide required input to unlock the volume.
	/// </summary>
	public const int FVE_E_POLICY_REQUIRES_STARTUP_PIN_ON_TOUCH_DEVICE = unchecked((int)0x803100B7);
	/// <summary>
	/// Group Policy settings require the creation of a recovery password, but neither a pre-boot keyboard nor Windows Recovery Environment is available on this device. The user may not be able to provide required input to unlock the volume.
	/// </summary>
	public const int FVE_E_POLICY_REQUIRES_RECOVERY_PASSWORD_ON_TOUCH_DEVICE = unchecked((int)0x803100B8);
	/// <summary>
	/// Wipe of free space is not currently taking place.
	/// </summary>
	public const int FVE_E_WIPE_CANCEL_NOT_APPLICABLE = unchecked((int)0x803100B9);
	/// <summary>
	/// BitLocker cannot use Secure Boot for platform integrity because Secure Boot has been disabled.
	/// </summary>
	public const int FVE_E_SECUREBOOT_DISABLED = unchecked((int)0x803100BA);
	/// <summary>
	/// BitLocker cannot use Secure Boot for platform integrity because the Secure Boot configuration does not meet the requirements for BitLocker.
	/// </summary>
	public const int FVE_E_SECUREBOOT_CONFIGURATION_INVALID = unchecked((int)0x803100BB);
	/// <summary>
	/// Your computer doesn't support BitLocker hardware-based encryption. Check with your computer manufacturer for firmware updates.
	/// </summary>
	public const int FVE_E_EDRIVE_DRY_RUN_FAILED = unchecked((int)0x803100BC);
	/// <summary>
	/// BitLocker cannot be enabled on the volume because it contains a Volume Shadow Copy. Remove all Volume Shadow Copies before encrypting the volume.
	/// </summary>
	public const int FVE_E_SHADOW_COPY_PRESENT = unchecked((int)0x803100BD);
	/// <summary>
	/// BitLocker Drive Encryption cannot be applied to this drive because the Group Policy setting for Enhanced Boot Configuration Data contains invalid data. Please have your system administrator resolve this invalid configuration before attempting to enable BitLocker.
	/// </summary>
	public const int FVE_E_POLICY_INVALID_ENHANCED_BCD_SETTINGS = unchecked((int)0x803100BE);
	/// <summary>
	/// This PC's firmware is not capable of supporting hardware encryption.
	/// </summary>
	public const int FVE_E_EDRIVE_INCOMPATIBLE_FIRMWARE = unchecked((int)0x803100BF);
	/// <summary>
	/// BitLocker has disabled password changes after too many failed requests. Click the link to reset the password as an administrator.
	/// </summary>
	public const int FVE_E_PROTECTOR_CHANGE_MAX_PASSPHRASE_CHANGE_ATTEMPTS_REACHED = unchecked((int)0x803100C0);
	/// <summary>
	/// You must be logged on with an administrator account to change the password. Click the link to reset the password as an administrator.
	/// </summary>
	public const int FVE_E_PASSPHRASE_PROTECTOR_CHANGE_BY_STD_USER_DISALLOWED = unchecked((int)0x803100C1);
	/// <summary>
	/// BitLocker cannot save the recovery password because the specified Microsoft account is Suspended.
	/// </summary>
	public const int FVE_E_LIVEID_ACCOUNT_SUSPENDED = unchecked((int)0x803100C2);
	/// <summary>
	/// BitLocker cannot save the recovery password because the specified MIcrosoft account is Blocked.
	/// </summary>
	public const int FVE_E_LIVEID_ACCOUNT_BLOCKED = unchecked((int)0x803100C3);
	/// <summary>
	/// This PC is not provisioned to support device encryption. Please enable BitLocker on all volumes to comply with device encryption policy.
	/// </summary>
	public const int FVE_E_NOT_PROVISIONED_ON_ALL_VOLUMES = unchecked((int)0x803100C4);
	/// <summary>
	/// This PC cannot support device encryption because unencrypted fixed data volumes are present.
	/// </summary>
	public const int FVE_E_DE_FIXED_DATA_NOT_SUPPORTED = unchecked((int)0x803100C5);
	/// <summary>
	/// This PC does not meet the hardware requirements to support device encryption.
	/// </summary>
	public const int FVE_E_DE_HARDWARE_NOT_COMPLIANT = unchecked((int)0x803100C6);
	/// <summary>
	/// This PC cannot support device encryption because WinRE is not properly configured.
	/// </summary>
	public const int FVE_E_DE_WINRE_NOT_CONFIGURED = unchecked((int)0x803100C7);
	/// <summary>
	/// Protection is enabled on the volume but has been suspended. This is likely to have happened due to an update being applied to your system. Please try again after a reboot.
	/// </summary>
	public const int FVE_E_DE_PROTECTION_SUSPENDED = unchecked((int)0x803100C8);
	/// <summary>
	/// This PC is not provisioned to support device encryption.
	/// </summary>
	public const int FVE_E_DE_OS_VOLUME_NOT_PROTECTED = unchecked((int)0x803100C9);
	/// <summary>
	/// Device Lock has been triggered due to too many incorrect password attempts.
	/// </summary>
	public const int FVE_E_DE_DEVICE_LOCKEDOUT = unchecked((int)0x803100CA);
	/// <summary>
	/// Protection has not been enabled on the volume. Enabling protection requires a connected account. If you already have a connected account and are seeing this error, please refer to the event log for more information.
	/// </summary>
	public const int FVE_E_DE_PROTECTION_NOT_YET_ENABLED = unchecked((int)0x803100CB);
	/// <summary>
	/// Your PIN can only contain numbers from 0 to 9.
	/// </summary>
	public const int FVE_E_INVALID_PIN_CHARS_DETAILED = unchecked((int)0x803100CC);
	/// <summary>
	/// BitLocker cannot use hardware replay protection because no counter is available on your PC.
	/// </summary>
	public const int FVE_E_DEVICE_LOCKOUT_COUNTER_UNAVAILABLE = unchecked((int)0x803100CD);
	/// <summary>
	/// Device Lockout state validation failed due to counter mismatch.
	/// </summary>
	public const int FVE_E_DEVICELOCKOUT_COUNTER_MISMATCH = unchecked((int)0x803100CE);
	/// <summary>
	/// The input buffer is too large.
	/// </summary>
	public const int FVE_E_BUFFER_TOO_LARGE = unchecked((int)0x803100CF);
	#endregion

	#region FWP, WS, NDIS, HyperV
	/// <summary>
	/// The callout does not exist.
	/// </summary>
	public const int FWP_E_CALLOUT_NOT_FOUND = unchecked((int)0x80320001);
	/// <summary>
	/// The filter condition does not exist.
	/// </summary>
	public const int FWP_E_CONDITION_NOT_FOUND = unchecked((int)0x80320002);
	/// <summary>
	/// The filter does not exist.
	/// </summary>
	public const int FWP_E_FILTER_NOT_FOUND = unchecked((int)0x80320003);
	/// <summary>
	/// The layer does not exist.
	/// </summary>
	public const int FWP_E_LAYER_NOT_FOUND = unchecked((int)0x80320004);
	/// <summary>
	/// The provider does not exist.
	/// </summary>
	public const int FWP_E_PROVIDER_NOT_FOUND = unchecked((int)0x80320005);
	/// <summary>
	/// The provider context does not exist.
	/// </summary>
	public const int FWP_E_PROVIDER_CONTEXT_NOT_FOUND = unchecked((int)0x80320006);
	/// <summary>
	/// The sublayer does not exist.
	/// </summary>
	public const int FWP_E_SUBLAYER_NOT_FOUND = unchecked((int)0x80320007);
	/// <summary>
	/// The object does not exist.
	/// </summary>
	public const int FWP_E_NOT_FOUND = unchecked((int)0x80320008);
	/// <summary>
	/// An object with that GUID or LUID already exists.
	/// </summary>
	public const int FWP_E_ALREADY_EXISTS = unchecked((int)0x80320009);
	/// <summary>
	/// The object is referenced by other objects so cannot be deleted.
	/// </summary>
	public const int FWP_E_IN_USE = unchecked((int)0x8032000A);
	/// <summary>
	/// The call is not allowed from within a dynamic session.
	/// </summary>
	public const int FWP_E_DYNAMIC_SESSION_IN_PROGRESS = unchecked((int)0x8032000B);
	/// <summary>
	/// The call was made from the wrong session so cannot be completed.
	/// </summary>
	public const int FWP_E_WRONG_SESSION = unchecked((int)0x8032000C);
	/// <summary>
	/// The call must be made from within an explicit transaction.
	/// </summary>
	public const int FWP_E_NO_TXN_IN_PROGRESS = unchecked((int)0x8032000D);
	/// <summary>
	/// The call is not allowed from within an explicit transaction.
	/// </summary>
	public const int FWP_E_TXN_IN_PROGRESS = unchecked((int)0x8032000E);
	/// <summary>
	/// The explicit transaction has been forcibly canceled.
	/// </summary>
	public const int FWP_E_TXN_ABORTED = unchecked((int)0x8032000F);
	/// <summary>
	/// The session has been canceled.
	/// </summary>
	public const int FWP_E_SESSION_ABORTED = unchecked((int)0x80320010);
	/// <summary>
	/// The call is not allowed from within a read-only transaction.
	/// </summary>
	public const int FWP_E_INCOMPATIBLE_TXN = unchecked((int)0x80320011);
	/// <summary>
	/// The call timed out while waiting to acquire the transaction lock.
	/// </summary>
	public const int FWP_E_TIMEOUT = unchecked((int)0x80320012);
	/// <summary>
	/// Collection of network diagnostic events is disabled.
	/// </summary>
	public const int FWP_E_NET_EVENTS_DISABLED = unchecked((int)0x80320013);
	/// <summary>
	/// The operation is not supported by the specified layer.
	/// </summary>
	public const int FWP_E_INCOMPATIBLE_LAYER = unchecked((int)0x80320014);
	/// <summary>
	/// The call is allowed for kernel-mode callers only.
	/// </summary>
	public const int FWP_E_KM_CLIENTS_ONLY = unchecked((int)0x80320015);
	/// <summary>
	/// The call tried to associate two objects with incompatible lifetimes.
	/// </summary>
	public const int FWP_E_LIFETIME_MISMATCH = unchecked((int)0x80320016);
	/// <summary>
	/// The object is built in so cannot be deleted.
	/// </summary>
	public const int FWP_E_BUILTIN_OBJECT = unchecked((int)0x80320017);
	/// <summary>
	/// The maximum number of callouts has been reached.
	/// </summary>
	public const int FWP_E_TOO_MANY_CALLOUTS = unchecked((int)0x80320018);
	/// <summary>
	/// A notification could not be delivered because a message queue is at its maximum capacity.
	/// </summary>
	public const int FWP_E_NOTIFICATION_DROPPED = unchecked((int)0x80320019);
	/// <summary>
	/// The traffic parameters do not match those for the security association context.
	/// </summary>
	public const int FWP_E_TRAFFIC_MISMATCH = unchecked((int)0x8032001A);
	/// <summary>
	/// The call is not allowed for the current security association state.
	/// </summary>
	public const int FWP_E_INCOMPATIBLE_SA_STATE = unchecked((int)0x8032001B);
	/// <summary>
	/// A required pointer is null.
	/// </summary>
	public const int FWP_E_NULL_POINTER = unchecked((int)0x8032001C);
	/// <summary>
	/// An enumerator is not valid.
	/// </summary>
	public const int FWP_E_INVALID_ENUMERATOR = unchecked((int)0x8032001D);
	/// <summary>
	/// The flags field contains an invalid value.
	/// </summary>
	public const int FWP_E_INVALID_FLAGS = unchecked((int)0x8032001E);
	/// <summary>
	/// A network mask is not valid.
	/// </summary>
	public const int FWP_E_INVALID_NET_MASK = unchecked((int)0x8032001F);
	/// <summary>
	/// An FWP_RANGE is not valid.
	/// </summary>
	public const int FWP_E_INVALID_RANGE = unchecked((int)0x80320020);
	/// <summary>
	/// The time interval is not valid.
	/// </summary>
	public const int FWP_E_INVALID_INTERVAL = unchecked((int)0x80320021);
	/// <summary>
	/// An array that must contain at least one element is zero length.
	/// </summary>
	public const int FWP_E_ZERO_LENGTH_ARRAY = unchecked((int)0x80320022);
	/// <summary>
	/// The displayData.name field cannot be null.
	/// </summary>
	public const int FWP_E_NULL_DISPLAY_NAME = unchecked((int)0x80320023);
	/// <summary>
	/// The action type is not one of the allowed action types for a filter.
	/// </summary>
	public const int FWP_E_INVALID_ACTION_TYPE = unchecked((int)0x80320024);
	/// <summary>
	/// The filter weight is not valid.
	/// </summary>
	public const int FWP_E_INVALID_WEIGHT = unchecked((int)0x80320025);
	/// <summary>
	/// A filter condition contains a match type that is not compatible with the operands.
	/// </summary>
	public const int FWP_E_MATCH_TYPE_MISMATCH = unchecked((int)0x80320026);
	/// <summary>
	/// An FWP_VALUE or FWPM_CONDITION_VALUE is of the wrong type.
	/// </summary>
	public const int FWP_E_TYPE_MISMATCH = unchecked((int)0x80320027);
	/// <summary>
	/// An integer value is outside the allowed range.
	/// </summary>
	public const int FWP_E_OUT_OF_BOUNDS = unchecked((int)0x80320028);
	/// <summary>
	/// A reserved field is nonzero.
	/// </summary>
	public const int FWP_E_RESERVED = unchecked((int)0x80320029);
	/// <summary>
	/// A filter cannot contain multiple conditions operating on a single field.
	/// </summary>
	public const int FWP_E_DUPLICATE_CONDITION = unchecked((int)0x8032002A);
	/// <summary>
	/// A policy cannot contain the same keying module more than once.
	/// </summary>
	public const int FWP_E_DUPLICATE_KEYMOD = unchecked((int)0x8032002B);
	/// <summary>
	/// The action type is not compatible with the layer.
	/// </summary>
	public const int FWP_E_ACTION_INCOMPATIBLE_WITH_LAYER = unchecked((int)0x8032002C);
	/// <summary>
	/// The action type is not compatible with the sublayer.
	/// </summary>
	public const int FWP_E_ACTION_INCOMPATIBLE_WITH_SUBLAYER = unchecked((int)0x8032002D);
	/// <summary>
	/// The raw context or the provider context is not compatible with the layer.
	/// </summary>
	public const int FWP_E_CONTEXT_INCOMPATIBLE_WITH_LAYER = unchecked((int)0x8032002E);
	/// <summary>
	/// The raw context or the provider context is not compatible with the callout.
	/// </summary>
	public const int FWP_E_CONTEXT_INCOMPATIBLE_WITH_CALLOUT = unchecked((int)0x8032002F);
	/// <summary>
	/// The authentication method is not compatible with the policy type.
	/// </summary>
	public const int FWP_E_INCOMPATIBLE_AUTH_METHOD = unchecked((int)0x80320030);
	/// <summary>
	/// The Diffie-Hellman group is not compatible with the policy type.
	/// </summary>
	public const int FWP_E_INCOMPATIBLE_DH_GROUP = unchecked((int)0x80320031);
	/// <summary>
	/// An IKE policy cannot contain an Extended Mode policy.
	/// </summary>
	public const int FWP_E_EM_NOT_SUPPORTED = unchecked((int)0x80320032);
	/// <summary>
	/// The enumeration template or subscription will never match any objects.
	/// </summary>
	public const int FWP_E_NEVER_MATCH = unchecked((int)0x80320033);
	/// <summary>
	/// The provider context is of the wrong type.
	/// </summary>
	public const int FWP_E_PROVIDER_CONTEXT_MISMATCH = unchecked((int)0x80320034);
	/// <summary>
	/// The parameter is incorrect.
	/// </summary>
	public const int FWP_E_INVALID_PARAMETER = unchecked((int)0x80320035);
	/// <summary>
	/// The maximum number of sublayers has been reached.
	/// </summary>
	public const int FWP_E_TOO_MANY_SUBLAYERS = unchecked((int)0x80320036);
	/// <summary>
	/// The notification function for a callout returned an error.
	/// </summary>
	public const int FWP_E_CALLOUT_NOTIFICATION_FAILED = unchecked((int)0x80320037);
	/// <summary>
	/// The IPsec authentication transform is not valid.
	/// </summary>
	public const int FWP_E_INVALID_AUTH_TRANSFORM = unchecked((int)0x80320038);
	/// <summary>
	/// The IPsec cipher transform is not valid.
	/// </summary>
	public const int FWP_E_INVALID_CIPHER_TRANSFORM = unchecked((int)0x80320039);
	/// <summary>
	/// The IPsec cipher transform is not compatible with the policy.
	/// </summary>
	public const int FWP_E_INCOMPATIBLE_CIPHER_TRANSFORM = unchecked((int)0x8032003A);
	/// <summary>
	/// The combination of IPsec transform types is not valid.
	/// </summary>
	public const int FWP_E_INVALID_TRANSFORM_COMBINATION = unchecked((int)0x8032003B);
	/// <summary>
	/// A policy cannot contain the same auth method more than once.
	/// </summary>
	public const int FWP_E_DUPLICATE_AUTH_METHOD = unchecked((int)0x8032003C);
	/// <summary>
	/// A tunnel endpoint configuration is invalid.
	/// </summary>
	public const int FWP_E_INVALID_TUNNEL_ENDPOINT = unchecked((int)0x8032003D);
	/// <summary>
	/// The WFP MAC Layers are not ready.
	/// </summary>
	public const int FWP_E_L2_DRIVER_NOT_READY = unchecked((int)0x8032003E);
	/// <summary>
	/// A key manager capable of key dictation is already registered
	/// </summary>
	public const int FWP_E_KEY_DICTATOR_ALREADY_REGISTERED = unchecked((int)0x8032003F);
	/// <summary>
	/// A key manager dictated invalid keys
	/// </summary>
	public const int FWP_E_KEY_DICTATION_INVALID_KEYING_MATERIAL = unchecked((int)0x80320040);
	/// <summary>
	/// The BFE IPsec Connection Tracking is disabled.
	/// </summary>
	public const int FWP_E_CONNECTIONS_DISABLED = unchecked((int)0x80320041);
	/// <summary>
	/// The DNS name is invalid.
	/// </summary>
	public const int FWP_E_INVALID_DNS_NAME = unchecked((int)0x80320042);
	/// <summary>
	/// The engine option is still enabled due to other configuration settings.
	/// </summary>
	public const int FWP_E_STILL_ON = unchecked((int)0x80320043);
	/// <summary>
	/// The IKEEXT service is not running. This service only runs when there is IPsec policy applied to the machine.
	/// </summary>
	public const int FWP_E_IKEEXT_NOT_RUNNING = unchecked((int)0x80320044);
	/// <summary>
	/// The packet should be dropped, no ICMP should be sent.
	/// </summary>
	public const int FWP_E_DROP_NOICMP = unchecked((int)0x80320104);
	/// <summary>
	/// The function call is completing asynchronously.
	/// </summary>
	public const int WS_S_ASYNC = 0x003D0000;
	/// <summary>
	/// There are no more messages available on the channel.
	/// </summary>
	public const int WS_S_END = 0x003D0001;
	/// <summary>
	/// The input data was not in the expected format or did not have the expected value.
	/// </summary>
	public const int WS_E_INVALID_FORMAT = unchecked((int)0x803D0000);
	/// <summary>
	/// The operation could not be completed because the object is in a faulted state due to a previous error.
	/// </summary>
	public const int WS_E_OBJECT_FAULTED = unchecked((int)0x803D0001);
	/// <summary>
	/// The operation could not be completed because it would lead to numeric overflow.
	/// </summary>
	public const int WS_E_NUMERIC_OVERFLOW = unchecked((int)0x803D0002);
	/// <summary>
	/// The operation is not allowed due to the current state of the object.
	/// </summary>
	public const int WS_E_INVALID_OPERATION = unchecked((int)0x803D0003);
	/// <summary>
	/// The operation was aborted.
	/// </summary>
	public const int WS_E_OPERATION_ABORTED = unchecked((int)0x803D0004);
	/// <summary>
	/// Access was denied by the remote endpoint.
	/// </summary>
	public const int WS_E_ENDPOINT_ACCESS_DENIED = unchecked((int)0x803D0005);
	/// <summary>
	/// The operation did not complete within the time allotted.
	/// </summary>
	public const int WS_E_OPERATION_TIMED_OUT = unchecked((int)0x803D0006);
	/// <summary>
	/// The operation was abandoned.
	/// </summary>
	public const int WS_E_OPERATION_ABANDONED = unchecked((int)0x803D0007);
	/// <summary>
	/// A quota was exceeded.
	/// </summary>
	public const int WS_E_QUOTA_EXCEEDED = unchecked((int)0x803D0008);
	/// <summary>
	/// The information was not available in the specified language.
	/// </summary>
	public const int WS_E_NO_TRANSLATION_AVAILABLE = unchecked((int)0x803D0009);
	/// <summary>
	/// Security verification was not successful for the received data.
	/// </summary>
	public const int WS_E_SECURITY_VERIFICATION_FAILURE = unchecked((int)0x803D000A);
	/// <summary>
	/// The address is already being used.
	/// </summary>
	public const int WS_E_ADDRESS_IN_USE = unchecked((int)0x803D000B);
	/// <summary>
	/// The address is not valid for this context.
	/// </summary>
	public const int WS_E_ADDRESS_NOT_AVAILABLE = unchecked((int)0x803D000C);
	/// <summary>
	/// The remote endpoint does not exist or could not be located.
	/// </summary>
	public const int WS_E_ENDPOINT_NOT_FOUND = unchecked((int)0x803D000D);
	/// <summary>
	/// The remote endpoint is not currently in service at this location.
	/// </summary>
	public const int WS_E_ENDPOINT_NOT_AVAILABLE = unchecked((int)0x803D000E);
	/// <summary>
	/// The remote endpoint could not process the request.
	/// </summary>
	public const int WS_E_ENDPOINT_FAILURE = unchecked((int)0x803D000F);
	/// <summary>
	/// The remote endpoint was not reachable.
	/// </summary>
	public const int WS_E_ENDPOINT_UNREACHABLE = unchecked((int)0x803D0010);
	/// <summary>
	/// The operation was not supported by the remote endpoint.
	/// </summary>
	public const int WS_E_ENDPOINT_ACTION_NOT_SUPPORTED = unchecked((int)0x803D0011);
	/// <summary>
	/// The remote endpoint is unable to process the request due to being overloaded.
	/// </summary>
	public const int WS_E_ENDPOINT_TOO_BUSY = unchecked((int)0x803D0012);
	/// <summary>
	/// A message containing a fault was received from the remote endpoint.
	/// </summary>
	public const int WS_E_ENDPOINT_FAULT_RECEIVED = unchecked((int)0x803D0013);
	/// <summary>
	/// The connection with the remote endpoint was terminated.
	/// </summary>
	public const int WS_E_ENDPOINT_DISCONNECTED = unchecked((int)0x803D0014);
	/// <summary>
	/// The HTTP proxy server could not process the request.
	/// </summary>
	public const int WS_E_PROXY_FAILURE = unchecked((int)0x803D0015);
	/// <summary>
	/// Access was denied by the HTTP proxy server.
	/// </summary>
	public const int WS_E_PROXY_ACCESS_DENIED = unchecked((int)0x803D0016);
	/// <summary>
	/// The requested feature is not available on this platform.
	/// </summary>
	public const int WS_E_NOT_SUPPORTED = unchecked((int)0x803D0017);
	/// <summary>
	/// The HTTP proxy server requires HTTP authentication scheme 'basic'.
	/// </summary>
	public const int WS_E_PROXY_REQUIRES_BASIC_AUTH = unchecked((int)0x803D0018);
	/// <summary>
	/// The HTTP proxy server requires HTTP authentication scheme 'digest'.
	/// </summary>
	public const int WS_E_PROXY_REQUIRES_DIGEST_AUTH = unchecked((int)0x803D0019);
	/// <summary>
	/// The HTTP proxy server requires HTTP authentication scheme 'NTLM'.
	/// </summary>
	public const int WS_E_PROXY_REQUIRES_NTLM_AUTH = unchecked((int)0x803D001A);
	/// <summary>
	/// The HTTP proxy server requires HTTP authentication scheme 'negotiate'.
	/// </summary>
	public const int WS_E_PROXY_REQUIRES_NEGOTIATE_AUTH = unchecked((int)0x803D001B);
	/// <summary>
	/// The remote endpoint requires HTTP authentication scheme 'basic'.
	/// </summary>
	public const int WS_E_SERVER_REQUIRES_BASIC_AUTH = unchecked((int)0x803D001C);
	/// <summary>
	/// The remote endpoint requires HTTP authentication scheme 'digest'.
	/// </summary>
	public const int WS_E_SERVER_REQUIRES_DIGEST_AUTH = unchecked((int)0x803D001D);
	/// <summary>
	/// The remote endpoint requires HTTP authentication scheme 'NTLM'.
	/// </summary>
	public const int WS_E_SERVER_REQUIRES_NTLM_AUTH = unchecked((int)0x803D001E);
	/// <summary>
	/// The remote endpoint requires HTTP authentication scheme 'negotiate'.
	/// </summary>
	public const int WS_E_SERVER_REQUIRES_NEGOTIATE_AUTH = unchecked((int)0x803D001F);
	/// <summary>
	/// The endpoint address URL is invalid.
	/// </summary>
	public const int WS_E_INVALID_ENDPOINT_URL = unchecked((int)0x803D0020);
	/// <summary>
	/// Unrecognized error occurred in the Windows Web Services framework.
	/// </summary>
	public const int WS_E_OTHER = unchecked((int)0x803D0021);
	/// <summary>
	/// A security token was rejected by the server because it has expired.
	/// </summary>
	public const int WS_E_SECURITY_TOKEN_EXPIRED = unchecked((int)0x803D0022);
	/// <summary>
	/// A security operation failed in the Windows Web Services framework.
	/// </summary>
	public const int WS_E_SECURITY_SYSTEM_FAILURE = unchecked((int)0x803D0023);
	/// <summary>
	/// The binding to the network interface is being closed.
	/// </summary>
	public const int ERROR_NDIS_INTERFACE_CLOSING = unchecked((int)0x80340002);
	/// <summary>
	/// An invalid version was specified.
	/// </summary>
	public const int ERROR_NDIS_BAD_VERSION = unchecked((int)0x80340004);
	/// <summary>
	/// An invalid characteristics table was used.
	/// </summary>
	public const int ERROR_NDIS_BAD_CHARACTERISTICS = unchecked((int)0x80340005);
	/// <summary>
	/// Failed to find the network interface or network interface is not ready.
	/// </summary>
	public const int ERROR_NDIS_ADAPTER_NOT_FOUND = unchecked((int)0x80340006);
	/// <summary>
	/// Failed to open the network interface.
	/// </summary>
	public const int ERROR_NDIS_OPEN_FAILED = unchecked((int)0x80340007);
	/// <summary>
	/// Network interface has encountered an internal unrecoverable failure.
	/// </summary>
	public const int ERROR_NDIS_DEVICE_FAILED = unchecked((int)0x80340008);
	/// <summary>
	/// The multicast list on the network interface is full.
	/// </summary>
	public const int ERROR_NDIS_MULTICAST_FULL = unchecked((int)0x80340009);
	/// <summary>
	/// An attempt was made to add a duplicate multicast address to the list.
	/// </summary>
	public const int ERROR_NDIS_MULTICAST_EXISTS = unchecked((int)0x8034000A);
	/// <summary>
	/// At attempt was made to remove a multicast address that was never added.
	/// </summary>
	public const int ERROR_NDIS_MULTICAST_NOT_FOUND = unchecked((int)0x8034000B);
	/// <summary>
	/// Netowork interface aborted the request.
	/// </summary>
	public const int ERROR_NDIS_REQUEST_ABORTED = unchecked((int)0x8034000C);
	/// <summary>
	/// Network interface cannot process the request because it is being reset.
	/// </summary>
	public const int ERROR_NDIS_RESET_IN_PROGRESS = unchecked((int)0x8034000D);
	/// <summary>
	/// Netword interface does not support this request.
	/// </summary>
	public const int ERROR_NDIS_NOT_SUPPORTED = unchecked((int)0x803400BB);
	/// <summary>
	/// An attempt was made to send an invalid packet on a network interface.
	/// </summary>
	public const int ERROR_NDIS_INVALID_PACKET = unchecked((int)0x8034000F);
	/// <summary>
	/// Network interface is not ready to complete this operation.
	/// </summary>
	public const int ERROR_NDIS_ADAPTER_NOT_READY = unchecked((int)0x80340011);
	/// <summary>
	/// The length of the buffer submitted for this operation is not valid.
	/// </summary>
	public const int ERROR_NDIS_INVALID_LENGTH = unchecked((int)0x80340014);
	/// <summary>
	/// The data used for this operation is not valid.
	/// </summary>
	public const int ERROR_NDIS_INVALID_DATA = unchecked((int)0x80340015);
	/// <summary>
	/// The length of buffer submitted for this operation is too small.
	/// </summary>
	public const int ERROR_NDIS_BUFFER_TOO_SHORT = unchecked((int)0x80340016);
	/// <summary>
	/// Network interface does not support this OID (Object Identifier)
	/// </summary>
	public const int ERROR_NDIS_INVALID_OID = unchecked((int)0x80340017);
	/// <summary>
	/// The network interface has been removed.
	/// </summary>
	public const int ERROR_NDIS_ADAPTER_REMOVED = unchecked((int)0x80340018);
	/// <summary>
	/// Network interface does not support this media type.
	/// </summary>
	public const int ERROR_NDIS_UNSUPPORTED_MEDIA = unchecked((int)0x80340019);
	/// <summary>
	/// An attempt was made to remove a token ring group address that is in use by other components.
	/// </summary>
	public const int ERROR_NDIS_GROUP_ADDRESS_IN_USE = unchecked((int)0x8034001A);
	/// <summary>
	/// An attempt was made to map a file that cannot be found.
	/// </summary>
	public const int ERROR_NDIS_FILE_NOT_FOUND = unchecked((int)0x8034001B);
	/// <summary>
	/// An error occurred while NDIS tried to map the file.
	/// </summary>
	public const int ERROR_NDIS_ERROR_READING_FILE = unchecked((int)0x8034001C);
	/// <summary>
	/// An attempt was made to map a file that is alreay mapped.
	/// </summary>
	public const int ERROR_NDIS_ALREADY_MAPPED = unchecked((int)0x8034001D);
	/// <summary>
	/// An attempt to allocate a hardware resource failed because the resource is used by another component.
	/// </summary>
	public const int ERROR_NDIS_RESOURCE_CONFLICT = unchecked((int)0x8034001E);
	/// <summary>
	/// The I/O operation failed because network media is disconnected or wireless access point is out of range.
	/// </summary>
	public const int ERROR_NDIS_MEDIA_DISCONNECTED = unchecked((int)0x8034001F);
	/// <summary>
	/// The network address used in the request is invalid.
	/// </summary>
	public const int ERROR_NDIS_INVALID_ADDRESS = unchecked((int)0x80340022);
	/// <summary>
	/// The specified request is not a valid operation for the target device.
	/// </summary>
	public const int ERROR_NDIS_INVALID_DEVICE_REQUEST = unchecked((int)0x80340010);
	/// <summary>
	/// The offload operation on the network interface has been paused.
	/// </summary>
	public const int ERROR_NDIS_PAUSED = unchecked((int)0x8034002A);
	/// <summary>
	/// Network interface was not found.
	/// </summary>
	public const int ERROR_NDIS_INTERFACE_NOT_FOUND = unchecked((int)0x8034002B);
	/// <summary>
	/// The revision number specified in the structure is not supported.
	/// </summary>
	public const int ERROR_NDIS_UNSUPPORTED_REVISION = unchecked((int)0x8034002C);
	/// <summary>
	/// The specified port does not exist on this network interface.
	/// </summary>
	public const int ERROR_NDIS_INVALID_PORT = unchecked((int)0x8034002D);
	/// <summary>
	/// The current state of the specified port on this network interface does not support the requested operation.
	/// </summary>
	public const int ERROR_NDIS_INVALID_PORT_STATE = unchecked((int)0x8034002E);
	/// <summary>
	/// The miniport adapter is in low power state.
	/// </summary>
	public const int ERROR_NDIS_LOW_POWER_STATE = unchecked((int)0x8034002F);
	/// <summary>
	/// This operation requires the miniport adapter to be reinitialized.
	/// </summary>
	public const int ERROR_NDIS_REINIT_REQUIRED = unchecked((int)0x80340030);
	/// <summary>
	/// The wireless local area network interface is in auto configuration mode and doesn't support the requested parameter change operation.
	/// </summary>
	public const int ERROR_NDIS_DOT11_AUTO_CONFIG_ENABLED = unchecked((int)0x80342000);
	/// <summary>
	/// The wireless local area network interface is busy and cannot perform the requested operation.
	/// </summary>
	public const int ERROR_NDIS_DOT11_MEDIA_IN_USE = unchecked((int)0x80342001);
	/// <summary>
	/// The wireless local area network interface is powered down and doesn't support the requested operation.
	/// </summary>
	public const int ERROR_NDIS_DOT11_POWER_STATE_INVALID = unchecked((int)0x80342002);
	/// <summary>
	/// The list of wake on LAN patterns is full.
	/// </summary>
	public const int ERROR_NDIS_PM_WOL_PATTERN_LIST_FULL = unchecked((int)0x80342003);
	/// <summary>
	/// The list of low power protocol offloads is full.
	/// </summary>
	public const int ERROR_NDIS_PM_PROTOCOL_OFFLOAD_LIST_FULL = unchecked((int)0x80342004);
	/// <summary>
	/// The request will be completed later by NDIS status indication.
	/// </summary>
	public const int ERROR_NDIS_INDICATION_REQUIRED = 0x00340001;
	/// <summary>
	/// The TCP connection is not offloadable because of a local policy setting.
	/// </summary>
	public const int ERROR_NDIS_OFFLOAD_POLICY = unchecked((int)0xC034100F);
	/// <summary>
	/// The TCP connection is not offloadable by the Chimney Offload target.
	/// </summary>
	public const int ERROR_NDIS_OFFLOAD_CONNECTION_REJECTED = unchecked((int)0xC0341012);
	/// <summary>
	/// The IP Path object is not in an offloadable state.
	/// </summary>
	public const int ERROR_NDIS_OFFLOAD_PATH_REJECTED = unchecked((int)0xC0341013);
	/// <summary>
	/// The hypervisor does not support the operation because the specified hypercall code is not supported.
	/// </summary>
	public const int ERROR_HV_INVALID_HYPERCALL_CODE = unchecked((int)0xC0350002);
	/// <summary>
	/// The hypervisor does not support the operation because the encoding for the hypercall input register is not supported.
	/// </summary>
	public const int ERROR_HV_INVALID_HYPERCALL_INPUT = unchecked((int)0xC0350003);
	/// <summary>
	/// The hypervisor could not perform the operation because a parameter has an invalid alignment.
	/// </summary>
	public const int ERROR_HV_INVALID_ALIGNMENT = unchecked((int)0xC0350004);
	/// <summary>
	/// The hypervisor could not perform the operation because an invalid parameter was specified.
	/// </summary>
	public const int ERROR_HV_INVALID_PARAMETER = unchecked((int)0xC0350005);
	/// <summary>
	/// Access to the specified object was denied.
	/// </summary>
	public const int ERROR_HV_ACCESS_DENIED = unchecked((int)0xC0350006);
	/// <summary>
	/// The hypervisor could not perform the operation because the partition is entering or in an invalid state.
	/// </summary>
	public const int ERROR_HV_INVALID_PARTITION_STATE = unchecked((int)0xC0350007);
	/// <summary>
	/// The operation is not allowed in the current state.
	/// </summary>
	public const int ERROR_HV_OPERATION_DENIED = unchecked((int)0xC0350008);
	/// <summary>
	/// The hypervisor does not recognize the specified partition property.
	/// </summary>
	public const int ERROR_HV_UNKNOWN_PROPERTY = unchecked((int)0xC0350009);
	/// <summary>
	/// The specified value of a partition property is out of range or violates an invariant.
	/// </summary>
	public const int ERROR_HV_PROPERTY_VALUE_OUT_OF_RANGE = unchecked((int)0xC035000A);
	/// <summary>
	/// There is not enough memory in the hypervisor pool to complete the operation.
	/// </summary>
	public const int ERROR_HV_INSUFFICIENT_MEMORY = unchecked((int)0xC035000B);
	/// <summary>
	/// The maximum partition depth has been exceeded for the partition hierarchy.
	/// </summary>
	public const int ERROR_HV_PARTITION_TOO_DEEP = unchecked((int)0xC035000C);
	/// <summary>
	/// A partition with the specified partition Id does not exist.
	/// </summary>
	public const int ERROR_HV_INVALID_PARTITION_ID = unchecked((int)0xC035000D);
	/// <summary>
	/// The hypervisor could not perform the operation because the specified VP index is invalid.
	/// </summary>
	public const int ERROR_HV_INVALID_VP_INDEX = unchecked((int)0xC035000E);
	/// <summary>
	/// The hypervisor could not perform the operation because the specified port identifier is invalid.
	/// </summary>
	public const int ERROR_HV_INVALID_PORT_ID = unchecked((int)0xC0350011);
	/// <summary>
	/// The hypervisor could not perform the operation because the specified connection identifier is invalid.
	/// </summary>
	public const int ERROR_HV_INVALID_CONNECTION_ID = unchecked((int)0xC0350012);
	/// <summary>
	/// Not enough buffers were supplied to send a message.
	/// </summary>
	public const int ERROR_HV_INSUFFICIENT_BUFFERS = unchecked((int)0xC0350013);
	/// <summary>
	/// The previous virtual interrupt has not been acknowledged.
	/// </summary>
	public const int ERROR_HV_NOT_ACKNOWLEDGED = unchecked((int)0xC0350014);
	/// <summary>
	/// The previous virtual interrupt has already been acknowledged.
	/// </summary>
	public const int ERROR_HV_ACKNOWLEDGED = unchecked((int)0xC0350016);
	/// <summary>
	/// The indicated partition is not in a valid state for saving or restoring.
	/// </summary>
	public const int ERROR_HV_INVALID_SAVE_RESTORE_STATE = unchecked((int)0xC0350017);
	/// <summary>
	/// The hypervisor could not complete the operation because a required feature of the synthetic interrupt controller (SynIC) was disabled.
	/// </summary>
	public const int ERROR_HV_INVALID_SYNIC_STATE = unchecked((int)0xC0350018);
	/// <summary>
	/// The hypervisor could not perform the operation because the object or value was either already in use or being used for a purpose that would not permit completing the operation.
	/// </summary>
	public const int ERROR_HV_OBJECT_IN_USE = unchecked((int)0xC0350019);
	/// <summary>
	/// The proximity domain information is invalid.
	/// </summary>
	public const int ERROR_HV_INVALID_PROXIMITY_DOMAIN_INFO = unchecked((int)0xC035001A);
	/// <summary>
	/// An attempt to retrieve debugging data failed because none was available.
	/// </summary>
	public const int ERROR_HV_NO_DATA = unchecked((int)0xC035001B);
	/// <summary>
	/// The physical connection being used for debugging has not recorded any receive activity since the last operation.
	/// </summary>
	public const int ERROR_HV_INACTIVE = unchecked((int)0xC035001C);
	/// <summary>
	/// There are not enough resources to complete the operation.
	/// </summary>
	public const int ERROR_HV_NO_RESOURCES = unchecked((int)0xC035001D);
	/// <summary>
	/// A hypervisor feature is not available to the user.
	/// </summary>
	public const int ERROR_HV_FEATURE_UNAVAILABLE = unchecked((int)0xC035001E);
	/// <summary>
	/// The maximum number of domains supported by the platform I/O remapping hardware is currently in use. No domains are available to assign this device to this partition.
	/// </summary>
	public const int ERROR_HV_INSUFFICIENT_DEVICE_DOMAINS = unchecked((int)0xC0350038);
	/// <summary>
	/// The hypervisor could not perform the operation because the specified LP index is invalid.
	/// </summary>
	public const int ERROR_HV_INVALID_LP_INDEX = unchecked((int)0xC0350041);
	/// <summary>
	/// No hypervisor is present on this system.
	/// </summary>
	public const int ERROR_HV_NOT_PRESENT = unchecked((int)0xC0351000);
	/// <summary>
	/// The handler for the virtualization infrastructure driver is already registered. Restarting the virtual machine may fix the problem. If the problem persists, try restarting the physical computer.
	/// </summary>
	public const int ERROR_VID_DUPLICATE_HANDLER = unchecked((int)0xC0370001);
	/// <summary>
	/// The number of registered handlers for the virtualization infrastructure driver exceeded the maximum. Restarting the virtual machine may fix the problem. If the problem persists, try restarting the physical computer.
	/// </summary>
	public const int ERROR_VID_TOO_MANY_HANDLERS = unchecked((int)0xC0370002);
	/// <summary>
	/// The message queue for the virtualization infrastructure driver is full and cannot accept new messages. Restarting the virtual machine may fix the problem. If the problem persists, try restarting the physical computer.
	/// </summary>
	public const int ERROR_VID_QUEUE_FULL = unchecked((int)0xC0370003);
	/// <summary>
	/// No handler exists to handle the message for the virtualization infrastructure driver. Restarting the virtual machine may fix the problem. If the problem persists, try restarting the physical computer.
	/// </summary>
	public const int ERROR_VID_HANDLER_NOT_PRESENT = unchecked((int)0xC0370004);
	/// <summary>
	/// The name of the partition or message queue for the virtualization infrastructure driver is invalid. Restarting the virtual machine may fix the problem. If the problem persists, try restarting the physical computer.
	/// </summary>
	public const int ERROR_VID_INVALID_OBJECT_NAME = unchecked((int)0xC0370005);
	/// <summary>
	/// The partition name of the virtualization infrastructure driver exceeds the maximum.
	/// </summary>
	public const int ERROR_VID_PARTITION_NAME_TOO_LONG = unchecked((int)0xC0370006);
	/// <summary>
	/// The message queue name of the virtualization infrastructure driver exceeds the maximum.
	/// </summary>
	public const int ERROR_VID_MESSAGE_QUEUE_NAME_TOO_LONG = unchecked((int)0xC0370007);
	/// <summary>
	/// Cannot create the partition for the virtualization infrastructure driver because another partition with the same name already exists.
	/// </summary>
	public const int ERROR_VID_PARTITION_ALREADY_EXISTS = unchecked((int)0xC0370008);
	/// <summary>
	/// The virtualization infrastructure driver has encountered an error. The requested partition does not exist. Restarting the virtual machine may fix the problem. If the problem persists, try restarting the physical computer.
	/// </summary>
	public const int ERROR_VID_PARTITION_DOES_NOT_EXIST = unchecked((int)0xC0370009);
	/// <summary>
	/// The virtualization infrastructure driver has encountered an error. Could not find the requested partition. Restarting the virtual machine may fix the problem. If the problem persists, try restarting the physical computer.
	/// </summary>
	public const int ERROR_VID_PARTITION_NAME_NOT_FOUND = unchecked((int)0xC037000A);
	/// <summary>
	/// A message queue with the same name already exists for the virtualization infrastructure driver.
	/// </summary>
	public const int ERROR_VID_MESSAGE_QUEUE_ALREADY_EXISTS = unchecked((int)0xC037000B);
	/// <summary>
	/// The memory block page for the virtualization infrastructure driver cannot be mapped because the page map limit has been reached. Restarting the virtual machine may fix the problem. If the problem persists, try restarting the physical computer.
	/// </summary>
	public const int ERROR_VID_EXCEEDED_MBP_ENTRY_MAP_LIMIT = unchecked((int)0xC037000C);
	/// <summary>
	/// The memory block for the virtualization infrastructure driver is still being used and cannot be destroyed.
	/// </summary>
	public const int ERROR_VID_MB_STILL_REFERENCED = unchecked((int)0xC037000D);
	/// <summary>
	/// Cannot unlock the page array for the guest operating system memory address because it does not match a previous lock request. Restarting the virtual machine may fix the problem. If the problem persists, try restarting the physical computer.
	/// </summary>
	public const int ERROR_VID_CHILD_GPA_PAGE_SET_CORRUPTED = unchecked((int)0xC037000E);
	/// <summary>
	/// The non-uniform memory access (NUMA) node settings do not match the system NUMA topology. In order to start the virtual machine, you will need to modify the NUMA configuration.
	/// </summary>
	public const int ERROR_VID_INVALID_NUMA_SETTINGS = unchecked((int)0xC037000F);
	/// <summary>
	/// The non-uniform memory access (NUMA) node index does not match a valid index in the system NUMA topology.
	/// </summary>
	public const int ERROR_VID_INVALID_NUMA_NODE_INDEX = unchecked((int)0xC0370010);
	/// <summary>
	/// The memory block for the virtualization infrastructure driver is already associated with a message queue.
	/// </summary>
	public const int ERROR_VID_NOTIFICATION_QUEUE_ALREADY_ASSOCIATED = unchecked((int)0xC0370011);
	/// <summary>
	/// The handle is not a valid memory block handle for the virtualization infrastructure driver.
	/// </summary>
	public const int ERROR_VID_INVALID_MEMORY_BLOCK_HANDLE = unchecked((int)0xC0370012);
	/// <summary>
	/// The request exceeded the memory block page limit for the virtualization infrastructure driver. Restarting the virtual machine may fix the problem. If the problem persists, try restarting the physical computer.
	/// </summary>
	public const int ERROR_VID_PAGE_RANGE_OVERFLOW = unchecked((int)0xC0370013);
	/// <summary>
	/// The handle is not a valid message queue handle for the virtualization infrastructure driver.
	/// </summary>
	public const int ERROR_VID_INVALID_MESSAGE_QUEUE_HANDLE = unchecked((int)0xC0370014);
	/// <summary>
	/// The handle is not a valid page range handle for the virtualization infrastructure driver.
	/// </summary>
	public const int ERROR_VID_INVALID_GPA_RANGE_HANDLE = unchecked((int)0xC0370015);
	/// <summary>
	/// Cannot install client notifications because no message queue for the virtualization infrastructure driver is associated with the memory block.
	/// </summary>
	public const int ERROR_VID_NO_MEMORY_BLOCK_NOTIFICATION_QUEUE = unchecked((int)0xC0370016);
	/// <summary>
	/// The request to lock or map a memory block page failed because the virtualization infrastructure driver memory block limit has been reached. Restarting the virtual machine may fix the problem. If the problem persists, try restarting the physical computer.
	/// </summary>
	public const int ERROR_VID_MEMORY_BLOCK_LOCK_COUNT_EXCEEDED = unchecked((int)0xC0370017);
	/// <summary>
	/// The handle is not a valid parent partition mapping handle for the virtualization infrastructure driver.
	/// </summary>
	public const int ERROR_VID_INVALID_PPM_HANDLE = unchecked((int)0xC0370018);
	/// <summary>
	/// Notifications cannot be created on the memory block because it is use.
	/// </summary>
	public const int ERROR_VID_MBPS_ARE_LOCKED = unchecked((int)0xC0370019);
	/// <summary>
	/// The message queue for the virtualization infrastructure driver has been closed. Restarting the virtual machine may fix the problem. If the problem persists, try restarting the physical computer.
	/// </summary>
	public const int ERROR_VID_MESSAGE_QUEUE_CLOSED = unchecked((int)0xC037001A);
	/// <summary>
	/// Cannot add a virtual processor to the partition because the maximum has been reached.
	/// </summary>
	public const int ERROR_VID_VIRTUAL_PROCESSOR_LIMIT_EXCEEDED = unchecked((int)0xC037001B);
	/// <summary>
	/// Cannot stop the virtual processor immediately because of a pending intercept.
	/// </summary>
	public const int ERROR_VID_STOP_PENDING = unchecked((int)0xC037001C);
	/// <summary>
	/// Invalid state for the virtual processor. Restarting the virtual machine may fix the problem. If the problem persists, try restarting the physical computer.
	/// </summary>
	public const int ERROR_VID_INVALID_PROCESSOR_STATE = unchecked((int)0xC037001D);
	/// <summary>
	/// The maximum number of kernel mode clients for the virtualization infrastructure driver has been reached. Restarting the virtual machine may fix the problem. If the problem persists, try restarting the physical computer.
	/// </summary>
	public const int ERROR_VID_EXCEEDED_KM_CONTEXT_COUNT_LIMIT = unchecked((int)0xC037001E);
	/// <summary>
	/// This kernel mode interface for the virtualization infrastructure driver has already been initialized. Restarting the virtual machine may fix the problem. If the problem persists, try restarting the physical computer.
	/// </summary>
	public const int ERROR_VID_KM_INTERFACE_ALREADY_INITIALIZED = unchecked((int)0xC037001F);
	/// <summary>
	/// Cannot set or reset the memory block property more than once for the virtualization infrastructure driver. Restarting the virtual machine may fix the problem. If the problem persists, try restarting the physical computer.
	/// </summary>
	public const int ERROR_VID_MB_PROPERTY_ALREADY_SET_RESET = unchecked((int)0xC0370020);
	/// <summary>
	/// The memory mapped I/O for this page range no longer exists. Restarting the virtual machine may fix the problem. If the problem persists, try restarting the physical computer.
	/// </summary>
	public const int ERROR_VID_MMIO_RANGE_DESTROYED = unchecked((int)0xC0370021);
	/// <summary>
	/// The lock or unlock request uses an invalid guest operating system memory address. Restarting the virtual machine may fix the problem. If the problem persists, try restarting the physical computer.
	/// </summary>
	public const int ERROR_VID_INVALID_CHILD_GPA_PAGE_SET = unchecked((int)0xC0370022);
	/// <summary>
	/// Cannot destroy or reuse the reserve page set for the virtualization infrastructure driver because it is in use. Restarting the virtual machine may fix the problem. If the problem persists, try restarting the physical computer.
	/// </summary>
	public const int ERROR_VID_RESERVE_PAGE_SET_IS_BEING_USED = unchecked((int)0xC0370023);
	/// <summary>
	/// The reserve page set for the virtualization infrastructure driver is too small to use in the lock request. Restarting the virtual machine may fix the problem. If the problem persists, try restarting the physical computer.
	/// </summary>
	public const int ERROR_VID_RESERVE_PAGE_SET_TOO_SMALL = unchecked((int)0xC0370024);
	/// <summary>
	/// Cannot lock or map the memory block page for the virtualization infrastructure driver because it has already been locked using a reserve page set page. Restarting the virtual machine may fix the problem. If the problem persists, try restarting the physical computer.
	/// </summary>
	public const int ERROR_VID_MBP_ALREADY_LOCKED_USING_RESERVED_PAGE = unchecked((int)0xC0370025);
	/// <summary>
	/// Cannot create the memory block for the virtualization infrastructure driver because the requested number of pages exceeded the limit. Restarting the virtual machine may fix the problem. If the problem persists, try restarting the physical computer.
	/// </summary>
	public const int ERROR_VID_MBP_COUNT_EXCEEDED_LIMIT = unchecked((int)0xC0370026);
	/// <summary>
	/// Cannot restore this virtual machine because the saved state data cannot be read. Delete the saved state data and then try to start the virtual machine.
	/// </summary>
	public const int ERROR_VID_SAVED_STATE_CORRUPT = unchecked((int)0xC0370027);
	/// <summary>
	/// Cannot restore this virtual machine because an item read from the saved state data is not recognized. Delete the saved state data and then try to start the virtual machine.
	/// </summary>
	public const int ERROR_VID_SAVED_STATE_UNRECOGNIZED_ITEM = unchecked((int)0xC0370028);
	/// <summary>
	/// Cannot restore this virtual machine to the saved state because of hypervisor incompatibility. Delete the saved state data and then try to start the virtual machine.
	/// </summary>
	public const int ERROR_VID_SAVED_STATE_INCOMPATIBLE = unchecked((int)0xC0370029);
	/// <summary>
	/// A virtual machine is running with its memory allocated across multiple NUMA nodes. This does not indicate a problem unless the performance of your virtual machine is unusually slow. If you are experiencing performance problems, you may need to modify the NUMA configuration.
	/// </summary>
	public const int ERROR_VID_REMOTE_NODE_PARENT_GPA_PAGES_USED = unchecked((int)0x80370001);
	#endregion

	#region VOLMGR, BCD, VHD, SDIAG
	/// <summary>
	/// The regeneration operation was not able to copy all data from the active plexes due to bad sectors.
	/// </summary>
	public const int ERROR_VOLMGR_INCOMPLETE_REGENERATION = unchecked((int)0x80380001);
	/// <summary>
	/// One or more disks were not fully migrated to the target pack. They may or may not require reimport after fixing the hardware problems.
	/// </summary>
	public const int ERROR_VOLMGR_INCOMPLETE_DISK_MIGRATION = unchecked((int)0x80380002);
	/// <summary>
	/// The configuration database is full.
	/// </summary>
	public const int ERROR_VOLMGR_DATABASE_FULL = unchecked((int)0xC0380001);
	/// <summary>
	/// The configuration data on the disk is corrupted.
	/// </summary>
	public const int ERROR_VOLMGR_DISK_CONFIGURATION_CORRUPTED = unchecked((int)0xC0380002);
	/// <summary>
	/// The configuration on the disk is not insync with the in-memory configuration.
	/// </summary>
	public const int ERROR_VOLMGR_DISK_CONFIGURATION_NOT_IN_SYNC = unchecked((int)0xC0380003);
	/// <summary>
	/// A majority of disks failed to be updated with the new configuration.
	/// </summary>
	public const int ERROR_VOLMGR_PACK_CONFIG_UPDATE_FAILED = unchecked((int)0xC0380004);
	/// <summary>
	/// The disk contains non-simple volumes.
	/// </summary>
	public const int ERROR_VOLMGR_DISK_CONTAINS_NON_SIMPLE_VOLUME = unchecked((int)0xC0380005);
	/// <summary>
	/// The same disk was specified more than once in the migration list.
	/// </summary>
	public const int ERROR_VOLMGR_DISK_DUPLICATE = unchecked((int)0xC0380006);
	/// <summary>
	/// The disk is already dynamic.
	/// </summary>
	public const int ERROR_VOLMGR_DISK_DYNAMIC = unchecked((int)0xC0380007);
	/// <summary>
	/// The specified disk id is invalid. There are no disks with the specified disk id.
	/// </summary>
	public const int ERROR_VOLMGR_DISK_ID_INVALID = unchecked((int)0xC0380008);
	/// <summary>
	/// The specified disk is an invalid disk. Operation cannot complete on an invalid disk.
	/// </summary>
	public const int ERROR_VOLMGR_DISK_INVALID = unchecked((int)0xC0380009);
	/// <summary>
	/// The specified disk(s) cannot be removed since it is the last remaining voter.
	/// </summary>
	public const int ERROR_VOLMGR_DISK_LAST_VOTER = unchecked((int)0xC038000A);
	/// <summary>
	/// The specified disk has an invalid disk layout.
	/// </summary>
	public const int ERROR_VOLMGR_DISK_LAYOUT_INVALID = unchecked((int)0xC038000B);
	/// <summary>
	/// The disk layout contains non-basic partitions which appear after basic paritions. This is an invalid disk layout.
	/// </summary>
	public const int ERROR_VOLMGR_DISK_LAYOUT_NON_BASIC_BETWEEN_BASIC_PARTITIONS = unchecked((int)0xC038000C);
	/// <summary>
	/// The disk layout contains partitions which are not cylinder aligned.
	/// </summary>
	public const int ERROR_VOLMGR_DISK_LAYOUT_NOT_CYLINDER_ALIGNED = unchecked((int)0xC038000D);
	/// <summary>
	/// The disk layout contains partitions which are samller than the minimum size.
	/// </summary>
	public const int ERROR_VOLMGR_DISK_LAYOUT_PARTITIONS_TOO_SMALL = unchecked((int)0xC038000E);
	/// <summary>
	/// The disk layout contains primary partitions in between logical drives. This is an invalid disk layout.
	/// </summary>
	public const int ERROR_VOLMGR_DISK_LAYOUT_PRIMARY_BETWEEN_LOGICAL_PARTITIONS = unchecked((int)0xC038000F);
	/// <summary>
	/// The disk layout contains more than the maximum number of supported partitions.
	/// </summary>
	public const int ERROR_VOLMGR_DISK_LAYOUT_TOO_MANY_PARTITIONS = unchecked((int)0xC0380010);
	/// <summary>
	/// The specified disk is missing. The operation cannot complete on a missing disk.
	/// </summary>
	public const int ERROR_VOLMGR_DISK_MISSING = unchecked((int)0xC0380011);
	/// <summary>
	/// The specified disk is not empty.
	/// </summary>
	public const int ERROR_VOLMGR_DISK_NOT_EMPTY = unchecked((int)0xC0380012);
	/// <summary>
	/// There is not enough usable space for this operation.
	/// </summary>
	public const int ERROR_VOLMGR_DISK_NOT_ENOUGH_SPACE = unchecked((int)0xC0380013);
	/// <summary>
	/// The force revectoring of bad sectors failed.
	/// </summary>
	public const int ERROR_VOLMGR_DISK_REVECTORING_FAILED = unchecked((int)0xC0380014);
	/// <summary>
	/// The specified disk has an invalid sector size.
	/// </summary>
	public const int ERROR_VOLMGR_DISK_SECTOR_SIZE_INVALID = unchecked((int)0xC0380015);
	/// <summary>
	/// The specified disk set contains volumes which exist on disks outside of the set.
	/// </summary>
	public const int ERROR_VOLMGR_DISK_SET_NOT_CONTAINED = unchecked((int)0xC0380016);
	/// <summary>
	/// A disk in the volume layout provides extents to more than one member of a plex.
	/// </summary>
	public const int ERROR_VOLMGR_DISK_USED_BY_MULTIPLE_MEMBERS = unchecked((int)0xC0380017);
	/// <summary>
	/// A disk in the volume layout provides extents to more than one plex.
	/// </summary>
	public const int ERROR_VOLMGR_DISK_USED_BY_MULTIPLE_PLEXES = unchecked((int)0xC0380018);
	/// <summary>
	/// Dynamic disks are not supported on this system.
	/// </summary>
	public const int ERROR_VOLMGR_DYNAMIC_DISK_NOT_SUPPORTED = unchecked((int)0xC0380019);
	/// <summary>
	/// The specified extent is already used by other volumes.
	/// </summary>
	public const int ERROR_VOLMGR_EXTENT_ALREADY_USED = unchecked((int)0xC038001A);
	/// <summary>
	/// The specified volume is retained and can only be extended into a contiguous extent. The specified extent to grow the volume is not contiguous with the specified volume.
	/// </summary>
	public const int ERROR_VOLMGR_EXTENT_NOT_CONTIGUOUS = unchecked((int)0xC038001B);
	/// <summary>
	/// The specified volume extent is not within the public region of the disk.
	/// </summary>
	public const int ERROR_VOLMGR_EXTENT_NOT_IN_PUBLIC_REGION = unchecked((int)0xC038001C);
	/// <summary>
	/// The specified volume extent is not sector aligned.
	/// </summary>
	public const int ERROR_VOLMGR_EXTENT_NOT_SECTOR_ALIGNED = unchecked((int)0xC038001D);
	/// <summary>
	/// The specified parition overlaps an EBR (the first track of an extended partition on a MBR disks).
	/// </summary>
	public const int ERROR_VOLMGR_EXTENT_OVERLAPS_EBR_PARTITION = unchecked((int)0xC038001E);
	/// <summary>
	/// The specified extent lengths cannot be used to construct a volume with specified length.
	/// </summary>
	public const int ERROR_VOLMGR_EXTENT_VOLUME_LENGTHS_DO_NOT_MATCH = unchecked((int)0xC038001F);
	/// <summary>
	/// The system does not support fault tolerant volumes.
	/// </summary>
	public const int ERROR_VOLMGR_FAULT_TOLERANT_NOT_SUPPORTED = unchecked((int)0xC0380020);
	/// <summary>
	/// The specified interleave length is invalid.
	/// </summary>
	public const int ERROR_VOLMGR_INTERLEAVE_LENGTH_INVALID = unchecked((int)0xC0380021);
	/// <summary>
	/// There is already a maximum number of registered users.
	/// </summary>
	public const int ERROR_VOLMGR_MAXIMUM_REGISTERED_USERS = unchecked((int)0xC0380022);
	/// <summary>
	/// The specified member is already in-sync with the other active members. It does not need to be regenerated.
	/// </summary>
	public const int ERROR_VOLMGR_MEMBER_IN_SYNC = unchecked((int)0xC0380023);
	/// <summary>
	/// The same member index was specified more than once.
	/// </summary>
	public const int ERROR_VOLMGR_MEMBER_INDEX_DUPLICATE = unchecked((int)0xC0380024);
	/// <summary>
	/// The specified member index is greater or equal than the number of members in the volume plex.
	/// </summary>
	public const int ERROR_VOLMGR_MEMBER_INDEX_INVALID = unchecked((int)0xC0380025);
	/// <summary>
	/// The specified member is missing. It cannot be regenerated.
	/// </summary>
	public const int ERROR_VOLMGR_MEMBER_MISSING = unchecked((int)0xC0380026);
	/// <summary>
	/// The specified member is not detached. Cannot replace a member which is not detached.
	/// </summary>
	public const int ERROR_VOLMGR_MEMBER_NOT_DETACHED = unchecked((int)0xC0380027);
	/// <summary>
	/// The specified member is already regenerating.
	/// </summary>
	public const int ERROR_VOLMGR_MEMBER_REGENERATING = unchecked((int)0xC0380028);
	/// <summary>
	/// All disks belonging to the pack failed.
	/// </summary>
	public const int ERROR_VOLMGR_ALL_DISKS_FAILED = unchecked((int)0xC0380029);
	/// <summary>
	/// There are currently no registered users for notifications. The task number is irrelevant unless there are registered users.
	/// </summary>
	public const int ERROR_VOLMGR_NO_REGISTERED_USERS = unchecked((int)0xC038002A);
	/// <summary>
	/// The specified notification user does not exist. Failed to unregister user for notifications.
	/// </summary>
	public const int ERROR_VOLMGR_NO_SUCH_USER = unchecked((int)0xC038002B);
	/// <summary>
	/// The notifications have been reset. Notifications for the current user are invalid. Unregister and re-register for notifications.
	/// </summary>
	public const int ERROR_VOLMGR_NOTIFICATION_RESET = unchecked((int)0xC038002C);
	/// <summary>
	/// The specified number of members is invalid.
	/// </summary>
	public const int ERROR_VOLMGR_NUMBER_OF_MEMBERS_INVALID = unchecked((int)0xC038002D);
	/// <summary>
	/// The specified number of plexes is invalid.
	/// </summary>
	public const int ERROR_VOLMGR_NUMBER_OF_PLEXES_INVALID = unchecked((int)0xC038002E);
	/// <summary>
	/// The specified source and target packs are identical.
	/// </summary>
	public const int ERROR_VOLMGR_PACK_DUPLICATE = unchecked((int)0xC038002F);
	/// <summary>
	/// The specified pack id is invalid. There are no packs with the specified pack id.
	/// </summary>
	public const int ERROR_VOLMGR_PACK_ID_INVALID = unchecked((int)0xC0380030);
	/// <summary>
	/// The specified pack is the invalid pack. The operation cannot complete with the invalid pack.
	/// </summary>
	public const int ERROR_VOLMGR_PACK_INVALID = unchecked((int)0xC0380031);
	/// <summary>
	/// The specified pack name is invalid.
	/// </summary>
	public const int ERROR_VOLMGR_PACK_NAME_INVALID = unchecked((int)0xC0380032);
	/// <summary>
	/// The specified pack is offline.
	/// </summary>
	public const int ERROR_VOLMGR_PACK_OFFLINE = unchecked((int)0xC0380033);
	/// <summary>
	/// The specified pack already has a quorum of healthy disks.
	/// </summary>
	public const int ERROR_VOLMGR_PACK_HAS_QUORUM = unchecked((int)0xC0380034);
	/// <summary>
	/// The pack does not have a quorum of healthy disks.
	/// </summary>
	public const int ERROR_VOLMGR_PACK_WITHOUT_QUORUM = unchecked((int)0xC0380035);
	/// <summary>
	/// The specified disk has an unsupported partition style. Only MBR and GPT partition styles are supported.
	/// </summary>
	public const int ERROR_VOLMGR_PARTITION_STYLE_INVALID = unchecked((int)0xC0380036);
	/// <summary>
	/// Failed to update the disk's partition layout.
	/// </summary>
	public const int ERROR_VOLMGR_PARTITION_UPDATE_FAILED = unchecked((int)0xC0380037);
	/// <summary>
	/// The specified plex is already in-sync with the other active plexes. It does not need to be regenerated.
	/// </summary>
	public const int ERROR_VOLMGR_PLEX_IN_SYNC = unchecked((int)0xC0380038);
	/// <summary>
	/// The same plex index was specified more than once.
	/// </summary>
	public const int ERROR_VOLMGR_PLEX_INDEX_DUPLICATE = unchecked((int)0xC0380039);
	/// <summary>
	/// The specified plex index is greater or equal than the number of plexes in the volume.
	/// </summary>
	public const int ERROR_VOLMGR_PLEX_INDEX_INVALID = unchecked((int)0xC038003A);
	/// <summary>
	/// The specified plex is the last active plex in the volume. The plex cannot be removed or else the volume will go offline.
	/// </summary>
	public const int ERROR_VOLMGR_PLEX_LAST_ACTIVE = unchecked((int)0xC038003B);
	/// <summary>
	/// The specified plex is missing.
	/// </summary>
	public const int ERROR_VOLMGR_PLEX_MISSING = unchecked((int)0xC038003C);
	/// <summary>
	/// The specified plex is currently regenerating.
	/// </summary>
	public const int ERROR_VOLMGR_PLEX_REGENERATING = unchecked((int)0xC038003D);
	/// <summary>
	/// The specified plex type is invalid.
	/// </summary>
	public const int ERROR_VOLMGR_PLEX_TYPE_INVALID = unchecked((int)0xC038003E);
	/// <summary>
	/// The operation is only supported on RAID-5 plexes.
	/// </summary>
	public const int ERROR_VOLMGR_PLEX_NOT_RAID5 = unchecked((int)0xC038003F);
	/// <summary>
	/// The operation is only supported on simple plexes.
	/// </summary>
	public const int ERROR_VOLMGR_PLEX_NOT_SIMPLE = unchecked((int)0xC0380040);
	/// <summary>
	/// The Size fields in the VM_VOLUME_LAYOUT input structure are incorrectly set.
	/// </summary>
	public const int ERROR_VOLMGR_STRUCTURE_SIZE_INVALID = unchecked((int)0xC0380041);
	/// <summary>
	/// There is already a pending request for notifications. Wait for the existing request to return before requesting for more notifications.
	/// </summary>
	public const int ERROR_VOLMGR_TOO_MANY_NOTIFICATION_REQUESTS = unchecked((int)0xC0380042);
	/// <summary>
	/// There is currently a transaction in process.
	/// </summary>
	public const int ERROR_VOLMGR_TRANSACTION_IN_PROGRESS = unchecked((int)0xC0380043);
	/// <summary>
	/// An unexpected layout change occurred outside of the volume manager.
	/// </summary>
	public const int ERROR_VOLMGR_UNEXPECTED_DISK_LAYOUT_CHANGE = unchecked((int)0xC0380044);
	/// <summary>
	/// The specified volume contains a missing disk.
	/// </summary>
	public const int ERROR_VOLMGR_VOLUME_CONTAINS_MISSING_DISK = unchecked((int)0xC0380045);
	/// <summary>
	/// The specified volume id is invalid. There are no volumes with the specified volume id.
	/// </summary>
	public const int ERROR_VOLMGR_VOLUME_ID_INVALID = unchecked((int)0xC0380046);
	/// <summary>
	/// The specified volume length is invalid.
	/// </summary>
	public const int ERROR_VOLMGR_VOLUME_LENGTH_INVALID = unchecked((int)0xC0380047);
	/// <summary>
	/// The specified size for the volume is not a multiple of the sector size.
	/// </summary>
	public const int ERROR_VOLMGR_VOLUME_LENGTH_NOT_SECTOR_SIZE_MULTIPLE = unchecked((int)0xC0380048);
	/// <summary>
	/// The operation is only supported on mirrored volumes.
	/// </summary>
	public const int ERROR_VOLMGR_VOLUME_NOT_MIRRORED = unchecked((int)0xC0380049);
	/// <summary>
	/// The specified volume does not have a retain partition.
	/// </summary>
	public const int ERROR_VOLMGR_VOLUME_NOT_RETAINED = unchecked((int)0xC038004A);
	/// <summary>
	/// The specified volume is offline.
	/// </summary>
	public const int ERROR_VOLMGR_VOLUME_OFFLINE = unchecked((int)0xC038004B);
	/// <summary>
	/// The specified volume already has a retain partition.
	/// </summary>
	public const int ERROR_VOLMGR_VOLUME_RETAINED = unchecked((int)0xC038004C);
	/// <summary>
	/// The specified number of extents is invalid.
	/// </summary>
	public const int ERROR_VOLMGR_NUMBER_OF_EXTENTS_INVALID = unchecked((int)0xC038004D);
	/// <summary>
	/// All disks participating to the volume must have the same sector size.
	/// </summary>
	public const int ERROR_VOLMGR_DIFFERENT_SECTOR_SIZE = unchecked((int)0xC038004E);
	/// <summary>
	/// The boot disk experienced failures.
	/// </summary>
	public const int ERROR_VOLMGR_BAD_BOOT_DISK = unchecked((int)0xC038004F);
	/// <summary>
	/// The configuration of the pack is offline.
	/// </summary>
	public const int ERROR_VOLMGR_PACK_CONFIG_OFFLINE = unchecked((int)0xC0380050);
	/// <summary>
	/// The configuration of the pack is online.
	/// </summary>
	public const int ERROR_VOLMGR_PACK_CONFIG_ONLINE = unchecked((int)0xC0380051);
	/// <summary>
	/// The specified pack is not the primary pack.
	/// </summary>
	public const int ERROR_VOLMGR_NOT_PRIMARY_PACK = unchecked((int)0xC0380052);
	/// <summary>
	/// All disks failed to be updated with the new content of the log.
	/// </summary>
	public const int ERROR_VOLMGR_PACK_LOG_UPDATE_FAILED = unchecked((int)0xC0380053);
	/// <summary>
	/// The specified number of disks in a plex is invalid.
	/// </summary>
	public const int ERROR_VOLMGR_NUMBER_OF_DISKS_IN_PLEX_INVALID = unchecked((int)0xC0380054);
	/// <summary>
	/// The specified number of disks in a plex member is invalid.
	/// </summary>
	public const int ERROR_VOLMGR_NUMBER_OF_DISKS_IN_MEMBER_INVALID = unchecked((int)0xC0380055);
	/// <summary>
	/// The operation is not supported on mirrored volumes.
	/// </summary>
	public const int ERROR_VOLMGR_VOLUME_MIRRORED = unchecked((int)0xC0380056);
	/// <summary>
	/// The operation is only supported on simple and spanned plexes.
	/// </summary>
	public const int ERROR_VOLMGR_PLEX_NOT_SIMPLE_SPANNED = unchecked((int)0xC0380057);
	/// <summary>
	/// The pack has no valid log copies.
	/// </summary>
	public const int ERROR_VOLMGR_NO_VALID_LOG_COPIES = unchecked((int)0xC0380058);
	/// <summary>
	/// A primary pack is already present.
	/// </summary>
	public const int ERROR_VOLMGR_PRIMARY_PACK_PRESENT = unchecked((int)0xC0380059);
	/// <summary>
	/// The specified number of disks is invalid.
	/// </summary>
	public const int ERROR_VOLMGR_NUMBER_OF_DISKS_INVALID = unchecked((int)0xC038005A);
	/// <summary>
	/// The system does not support mirrored volumes.
	/// </summary>
	public const int ERROR_VOLMGR_MIRROR_NOT_SUPPORTED = unchecked((int)0xC038005B);
	/// <summary>
	/// The system does not support RAID-5 volumes.
	/// </summary>
	public const int ERROR_VOLMGR_RAID5_NOT_SUPPORTED = unchecked((int)0xC038005C);
	/// <summary>
	/// Some BCD entries were not imported correctly from the BCD store.
	/// </summary>
	public const int ERROR_BCD_NOT_ALL_ENTRIES_IMPORTED = unchecked((int)0x80390001);
	/// <summary>
	/// Entries enumerated have exceeded the allowed threshold.
	/// </summary>
	public const int ERROR_BCD_TOO_MANY_ELEMENTS = unchecked((int)0xC0390002);
	/// <summary>
	/// Some BCD entries were not synchronized correctly with the firmware.
	/// </summary>
	public const int ERROR_BCD_NOT_ALL_ENTRIES_SYNCHRONIZED = unchecked((int)0x80390003);
	/// <summary>
	/// The virtual hard disk is corrupted. The virtual hard disk drive footer is missing.
	/// </summary>
	public const int ERROR_VHD_DRIVE_FOOTER_MISSING = unchecked((int)0xC03A0001);
	/// <summary>
	/// The virtual hard disk is corrupted. The virtual hard disk drive footer checksum does not match the on-disk checksum.
	/// </summary>
	public const int ERROR_VHD_DRIVE_FOOTER_CHECKSUM_MISMATCH = unchecked((int)0xC03A0002);
	/// <summary>
	/// The virtual hard disk is corrupted. The virtual hard disk drive footer in the virtual hard disk is corrupted.
	/// </summary>
	public const int ERROR_VHD_DRIVE_FOOTER_CORRUPT = unchecked((int)0xC03A0003);
	/// <summary>
	/// The system does not recognize the file format of this virtual hard disk.
	/// </summary>
	public const int ERROR_VHD_FORMAT_UNKNOWN = unchecked((int)0xC03A0004);
	/// <summary>
	/// The version does not support this version of the file format.
	/// </summary>
	public const int ERROR_VHD_FORMAT_UNSUPPORTED_VERSION = unchecked((int)0xC03A0005);
	/// <summary>
	/// The virtual hard disk is corrupted. The sparse header checksum does not match the on-disk checksum.
	/// </summary>
	public const int ERROR_VHD_SPARSE_HEADER_CHECKSUM_MISMATCH = unchecked((int)0xC03A0006);
	/// <summary>
	/// The system does not support this version of the virtual hard disk.This version of the sparse header is not supported.
	/// </summary>
	public const int ERROR_VHD_SPARSE_HEADER_UNSUPPORTED_VERSION = unchecked((int)0xC03A0007);
	/// <summary>
	/// The virtual hard disk is corrupted. The sparse header in the virtual hard disk is corrupt.
	/// </summary>
	public const int ERROR_VHD_SPARSE_HEADER_CORRUPT = unchecked((int)0xC03A0008);
	/// <summary>
	/// Failed to write to the virtual hard disk failed because the system failed to allocate a new block in the virtual hard disk.
	/// </summary>
	public const int ERROR_VHD_BLOCK_ALLOCATION_FAILURE = unchecked((int)0xC03A0009);
	/// <summary>
	/// The virtual hard disk is corrupted. The block allocation table in the virtual hard disk is corrupt.
	/// </summary>
	public const int ERROR_VHD_BLOCK_ALLOCATION_TABLE_CORRUPT = unchecked((int)0xC03A000A);
	/// <summary>
	/// The system does not support this version of the virtual hard disk. The block size is invalid.
	/// </summary>
	public const int ERROR_VHD_INVALID_BLOCK_SIZE = unchecked((int)0xC03A000B);
	/// <summary>
	/// The virtual hard disk is corrupted. The block bitmap does not match with the block data present in the virtual hard disk.
	/// </summary>
	public const int ERROR_VHD_BITMAP_MISMATCH = unchecked((int)0xC03A000C);
	/// <summary>
	/// The chain of virtual hard disks is broken. The system cannot locate the parent virtual hard disk for the differencing disk.
	/// </summary>
	public const int ERROR_VHD_PARENT_VHD_NOT_FOUND = unchecked((int)0xC03A000D);
	/// <summary>
	/// The chain of virtual hard disks is corrupted. There is a mismatch in the identifiers of the parent virtual hard disk and differencing disk.
	/// </summary>
	public const int ERROR_VHD_CHILD_PARENT_ID_MISMATCH = unchecked((int)0xC03A000E);
	/// <summary>
	/// The chain of virtual hard disks is corrupted. The time stamp of the parent virtual hard disk does not match the time stamp of the differencing disk.
	/// </summary>
	public const int ERROR_VHD_CHILD_PARENT_TIMESTAMP_MISMATCH = unchecked((int)0xC03A000F);
	/// <summary>
	/// Failed to read the metadata of the virtual hard disk.
	/// </summary>
	public const int ERROR_VHD_METADATA_READ_FAILURE = unchecked((int)0xC03A0010);
	/// <summary>
	/// Failed to write to the metadata of the virtual hard disk.
	/// </summary>
	public const int ERROR_VHD_METADATA_WRITE_FAILURE = unchecked((int)0xC03A0011);
	/// <summary>
	/// The size of the virtual hard disk is not valid.
	/// </summary>
	public const int ERROR_VHD_INVALID_SIZE = unchecked((int)0xC03A0012);
	/// <summary>
	/// The file size of this virtual hard disk is not valid.
	/// </summary>
	public const int ERROR_VHD_INVALID_FILE_SIZE = unchecked((int)0xC03A0013);
	/// <summary>
	/// A virtual disk support provider for the specified file was not found.
	/// </summary>
	public const int ERROR_VIRTDISK_PROVIDER_NOT_FOUND = unchecked((int)0xC03A0014);
	/// <summary>
	/// The specified disk is not a virtual disk.
	/// </summary>
	public const int ERROR_VIRTDISK_NOT_VIRTUAL_DISK = unchecked((int)0xC03A0015);
	/// <summary>
	/// The chain of virtual hard disks is inaccessible. The process has not been granted access rights to the parent virtual hard disk for the differencing disk.
	/// </summary>
	public const int ERROR_VHD_PARENT_VHD_ACCESS_DENIED = unchecked((int)0xC03A0016);
	/// <summary>
	/// The chain of virtual hard disks is corrupted. There is a mismatch in the virtual sizes of the parent virtual hard disk and differencing disk.
	/// </summary>
	public const int ERROR_VHD_CHILD_PARENT_SIZE_MISMATCH = unchecked((int)0xC03A0017);
	/// <summary>
	/// The chain of virtual hard disks is corrupted. A differencing disk is indicated in its own parent chain.
	/// </summary>
	public const int ERROR_VHD_DIFFERENCING_CHAIN_CYCLE_DETECTED = unchecked((int)0xC03A0018);
	/// <summary>
	/// The chain of virtual hard disks is inaccessible. There was an error opening a virtual hard disk further up the chain.
	/// </summary>
	public const int ERROR_VHD_DIFFERENCING_CHAIN_ERROR_IN_PARENT = unchecked((int)0xC03A0019);
	/// <summary>
	/// The requested operation could not be completed due to a virtual disk system limitation. On NTFS, virtual hard disk files must be uncompressed and unencrypted. On ReFS, virtual hard disk files must not have the integrity bit set.
	/// </summary>
	public const int ERROR_VIRTUAL_DISK_LIMITATION = unchecked((int)0xC03A001A);
	/// <summary>
	/// The requested operation cannot be performed on a virtual disk of this type.
	/// </summary>
	public const int ERROR_VHD_INVALID_TYPE = unchecked((int)0xC03A001B);
	/// <summary>
	/// The requested operation cannot be performed on the virtual disk in its current state.
	/// </summary>
	public const int ERROR_VHD_INVALID_STATE = unchecked((int)0xC03A001C);
	/// <summary>
	/// The sector size of the physical disk on which the virtual disk resides is not supported.
	/// </summary>
	public const int ERROR_VIRTDISK_UNSUPPORTED_DISK_SECTOR_SIZE = unchecked((int)0xC03A001D);
	/// <summary>
	/// The disk is already owned by a different owner.
	/// </summary>
	public const int ERROR_VIRTDISK_DISK_ALREADY_OWNED = unchecked((int)0xC03A001E);
	/// <summary>
	/// The disk must be offline or read-only.
	/// </summary>
	public const int ERROR_VIRTDISK_DISK_ONLINE_AND_WRITABLE = unchecked((int)0xC03A001F);
	/// <summary>
	/// Change Tracking is not initialized for this Virtual Disk.
	/// </summary>
	public const int ERROR_CTLOG_TRACKING_NOT_INITIALIZED = unchecked((int)0xC03A0020);
	/// <summary>
	/// Size of change tracking file exceeded the maximum size limit
	/// </summary>
	public const int ERROR_CTLOG_LOGFILE_SIZE_EXCEEDED_MAXSIZE = unchecked((int)0xC03A0021);
	/// <summary>
	/// VHD file is changed due to compaction, expansion or offline patching
	/// </summary>
	public const int ERROR_CTLOG_VHD_CHANGED_OFFLINE = unchecked((int)0xC03A0022);
	/// <summary>
	/// Change Tracking for the virtual disk is not in a valid state to perform this request. Change tracking could be discontinued or already in the requested state.
	/// </summary>
	public const int ERROR_CTLOG_INVALID_TRACKING_STATE = unchecked((int)0xC03A0023);
	/// <summary>
	/// Change Tracking file for the virtual disk is not in a valid state.
	/// </summary>
	public const int ERROR_CTLOG_INCONSISTANT_TRACKING_FILE = unchecked((int)0xC03A0024);
	/// <summary>
	/// The requested resize operation could not be completed because it might truncate user data residing on the virtual disk.
	/// </summary>
	public const int ERROR_VHD_RESIZE_WOULD_TRUNCATE_DATA = unchecked((int)0xC03A0025);
	/// <summary>
	/// The requested operation could not be completed because the virtual disk's minimum safe size could not be determined. This may be due to a missing or corrupt partition table.
	/// </summary>
	public const int ERROR_VHD_COULD_NOT_COMPUTE_MINIMUM_VIRTUAL_SIZE = unchecked((int)0xC03A0026);
	/// <summary>
	/// The requested operation could not be completed because the virtual disk's size cannot be safely reduced further.
	/// </summary>
	public const int ERROR_VHD_ALREADY_AT_OR_BELOW_MINIMUM_VIRTUAL_SIZE = unchecked((int)0xC03A0027);
	/// <summary>
	/// There is not enough space in the virtual disk file for the provided metadata item.
	/// </summary>
	public const int ERROR_VHD_METADATA_FULL = unchecked((int)0xC03A0028);
	/// <summary>
	/// The virtualization storage subsystem has generated an error.
	/// </summary>
	public const int ERROR_QUERY_STORAGE_ERROR = unchecked((int)0x803A0001);
	/// <summary>
	/// The operation was canceled.
	/// </summary>
	public const int SDIAG_E_CANCELLED = unchecked((int)0x803C0100);
	/// <summary>
	/// An error occurred when running a PowerShell script.
	/// </summary>
	public const int SDIAG_E_SCRIPT = unchecked((int)0x803C0101);
	/// <summary>
	/// An error occurred when interacting with PowerShell runtime.
	/// </summary>
	public const int SDIAG_E_POWERSHELL = unchecked((int)0x803C0102);
	/// <summary>
	/// An error occurred in the Scripted Diagnostic Managed Host.
	/// </summary>
	public const int SDIAG_E_MANAGEDHOST = unchecked((int)0x803C0103);
	/// <summary>
	/// The troubleshooting pack does not contain a required verifier to complete the verification.
	/// </summary>
	public const int SDIAG_E_NOVERIFIER = unchecked((int)0x803C0104);
	/// <summary>
	/// The troubleshooting pack cannot be executed on this system.
	/// </summary>
	public const int SDIAG_S_CANNOTRUN = 0x003C0105;
	/// <summary>
	/// Scripted diagnostics is disabled by group policy.
	/// </summary>
	public const int SDIAG_E_DISABLED = unchecked((int)0x803C0106);
	/// <summary>
	/// Trust validation of the diagnostic package failed.
	/// </summary>
	public const int SDIAG_E_TRUST = unchecked((int)0x803C0107);
	/// <summary>
	/// The troubleshooting pack cannot be executed on this system.
	/// </summary>
	public const int SDIAG_E_CANNOTRUN = unchecked((int)0x803C0108);
	/// <summary>
	/// This version of the troubleshooting pack is not supported.
	/// </summary>
	public const int SDIAG_E_VERSION = unchecked((int)0x803C0109);
	/// <summary>
	/// A required resource cannot be loaded.
	/// </summary>
	public const int SDIAG_E_RESOURCE = unchecked((int)0x803C010A);
	/// <summary>
	/// The troubleshooting pack reported information for a root cause without adding the root cause.
	/// </summary>
	public const int SDIAG_E_ROOTCAUSE = unchecked((int)0x803C010B);
	#endregion

	#region WPN, MBN, P2P, Bluetooth
	/// <summary>
	/// The notification channel has already been closed.
	/// </summary>
	public const int WPN_E_CHANNEL_CLOSED = unchecked((int)0x803E0100);
	/// <summary>
	/// The notification channel request did not complete successfully.
	/// </summary>
	public const int WPN_E_CHANNEL_REQUEST_NOT_COMPLETE = unchecked((int)0x803E0101);
	/// <summary>
	/// The application identifier provided is invalid.
	/// </summary>
	public const int WPN_E_INVALID_APP = unchecked((int)0x803E0102);
	/// <summary>
	/// A notification channel request for the provided application identifier is in progress.
	/// </summary>
	public const int WPN_E_OUTSTANDING_CHANNEL_REQUEST = unchecked((int)0x803E0103);
	/// <summary>
	/// The channel identifier is already tied to another application endpoint.
	/// </summary>
	public const int WPN_E_DUPLICATE_CHANNEL = unchecked((int)0x803E0104);
	/// <summary>
	/// The notification platform is unavailable.
	/// </summary>
	public const int WPN_E_PLATFORM_UNAVAILABLE = unchecked((int)0x803E0105);
	/// <summary>
	/// The notification has already been posted.
	/// </summary>
	public const int WPN_E_NOTIFICATION_POSTED = unchecked((int)0x803E0106);
	/// <summary>
	/// The notification has already been hidden.
	/// </summary>
	public const int WPN_E_NOTIFICATION_HIDDEN = unchecked((int)0x803E0107);
	/// <summary>
	/// The notification cannot be hidden until it has been shown.
	/// </summary>
	public const int WPN_E_NOTIFICATION_NOT_POSTED = unchecked((int)0x803E0108);
	/// <summary>
	/// Cloud notifications have been turned off.
	/// </summary>
	public const int WPN_E_CLOUD_DISABLED = unchecked((int)0x803E0109);
	/// <summary>
	/// The application does not have the cloud notification capability.
	/// </summary>
	public const int WPN_E_CLOUD_INCAPABLE = unchecked((int)0x803E0110);
	/// <summary>
	/// Settings prevent the notification from being delivered.
	/// </summary>
	public const int WPN_E_NOTIFICATION_DISABLED = unchecked((int)0x803E0111);
	/// <summary>
	/// Application capabilities prevent the notification from being delivered.
	/// </summary>
	public const int WPN_E_NOTIFICATION_INCAPABLE = unchecked((int)0x803E0112);
	/// <summary>
	/// The application does not have the internet access capability.
	/// </summary>
	public const int WPN_E_INTERNET_INCAPABLE = unchecked((int)0x803E0113);
	/// <summary>
	/// Settings prevent the notification type from being delivered.
	/// </summary>
	public const int WPN_E_NOTIFICATION_TYPE_DISABLED = unchecked((int)0x803E0114);
	/// <summary>
	/// The size of the notification content is too large.
	/// </summary>
	public const int WPN_E_NOTIFICATION_SIZE = unchecked((int)0x803E0115);
	/// <summary>
	/// The size of the notification tag is too large.
	/// </summary>
	public const int WPN_E_TAG_SIZE = unchecked((int)0x803E0116);
	/// <summary>
	/// The notification platform doesn't have appropriate privilege on resources.
	/// </summary>
	public const int WPN_E_ACCESS_DENIED = unchecked((int)0x803E0117);
	/// <summary>
	/// The notification platform found application is already registered.
	/// </summary>
	public const int WPN_E_DUPLICATE_REGISTRATION = unchecked((int)0x803E0118);
	/// <summary>
	/// The notification platform has run out of presentation layer sessions.
	/// </summary>
	public const int WPN_E_OUT_OF_SESSION = unchecked((int)0x803E0200);
	/// <summary>
	/// The notification platform rejects image download request due to system in power save mode.
	/// </summary>
	public const int WPN_E_POWER_SAVE = unchecked((int)0x803E0201);
	/// <summary>
	/// The notification platform doesn't have the requested image in its cache.
	/// </summary>
	public const int WPN_E_IMAGE_NOT_FOUND_IN_CACHE = unchecked((int)0x803E0202);
	/// <summary>
	/// The notification platform cannot complete all of requested image.
	/// </summary>
	public const int WPN_E_ALL_URL_NOT_COMPLETED = unchecked((int)0x803E0203);
	/// <summary>
	/// A cloud image downloaded from the notification platform is invalid.
	/// </summary>
	public const int WPN_E_INVALID_CLOUD_IMAGE = unchecked((int)0x803E0204);
	/// <summary>
	/// Notification Id provided as filter is matched with what the notification platform maintains.
	/// </summary>
	public const int WPN_E_NOTIFICATION_ID_MATCHED = unchecked((int)0x803E0205);
	/// <summary>
	/// Notification callback interface is already registered.
	/// </summary>
	public const int WPN_E_CALLBACK_ALREADY_REGISTERED = unchecked((int)0x803E0206);
	/// <summary>
	/// Toast Notification was dropped without being displayed to the user.
	/// </summary>
	public const int WPN_E_TOAST_NOTIFICATION_DROPPED = unchecked((int)0x803E0207);
	/// <summary>
	/// The notification platform does not have the proper privileges to complete the request.
	/// </summary>
	public const int WPN_E_STORAGE_LOCKED = unchecked((int)0x803E0208);
	/// <summary>
	/// Context is not activated.
	/// </summary>
	public const int E_MBN_CONTEXT_NOT_ACTIVATED = unchecked((int)0x80548201);
	/// <summary>
	/// Bad SIM is inserted.
	/// </summary>
	public const int E_MBN_BAD_SIM = unchecked((int)0x80548202);
	/// <summary>
	/// Requested data class is not available.
	/// </summary>
	public const int E_MBN_DATA_CLASS_NOT_AVAILABLE = unchecked((int)0x80548203);
	/// <summary>
	/// Access point name (APN) or Access string is incorrect.
	/// </summary>
	public const int E_MBN_INVALID_ACCESS_STRING = unchecked((int)0x80548204);
	/// <summary>
	/// Max activated contexts have reached.
	/// </summary>
	public const int E_MBN_MAX_ACTIVATED_CONTEXTS = unchecked((int)0x80548205);
	/// <summary>
	/// Device is in packet detach state.
	/// </summary>
	public const int E_MBN_PACKET_SVC_DETACHED = unchecked((int)0x80548206);
	/// <summary>
	/// Provider is not visible.
	/// </summary>
	public const int E_MBN_PROVIDER_NOT_VISIBLE = unchecked((int)0x80548207);
	/// <summary>
	/// Radio is powered off.
	/// </summary>
	public const int E_MBN_RADIO_POWER_OFF = unchecked((int)0x80548208);
	/// <summary>
	/// MBN subscription is not activated.
	/// </summary>
	public const int E_MBN_SERVICE_NOT_ACTIVATED = unchecked((int)0x80548209);
	/// <summary>
	/// SIM is not inserted.
	/// </summary>
	public const int E_MBN_SIM_NOT_INSERTED = unchecked((int)0x8054820A);
	/// <summary>
	/// Voice call in progress.
	/// </summary>
	public const int E_MBN_VOICE_CALL_IN_PROGRESS = unchecked((int)0x8054820B);
	/// <summary>
	/// Visible provider cache is invalid.
	/// </summary>
	public const int E_MBN_INVALID_CACHE = unchecked((int)0x8054820C);
	/// <summary>
	/// Device is not registered.
	/// </summary>
	public const int E_MBN_NOT_REGISTERED = unchecked((int)0x8054820D);
	/// <summary>
	/// Providers not found.
	/// </summary>
	public const int E_MBN_PROVIDERS_NOT_FOUND = unchecked((int)0x8054820E);
	/// <summary>
	/// Pin is not supported.
	/// </summary>
	public const int E_MBN_PIN_NOT_SUPPORTED = unchecked((int)0x8054820F);
	/// <summary>
	/// Pin is required.
	/// </summary>
	public const int E_MBN_PIN_REQUIRED = unchecked((int)0x80548210);
	/// <summary>
	/// PIN is disabled.
	/// </summary>
	public const int E_MBN_PIN_DISABLED = unchecked((int)0x80548211);
	/// <summary>
	/// Generic Failure.
	/// </summary>
	public const int E_MBN_FAILURE = unchecked((int)0x80548212);
	/// <summary>
	/// Profile is invalid.
	/// </summary>
	public const int E_MBN_INVALID_PROFILE = unchecked((int)0x80548218);
	/// <summary>
	/// Default profile exist.
	/// </summary>
	public const int E_MBN_DEFAULT_PROFILE_EXIST = unchecked((int)0x80548219);
	/// <summary>
	/// SMS encoding is not supported.
	/// </summary>
	public const int E_MBN_SMS_ENCODING_NOT_SUPPORTED = unchecked((int)0x80548220);
	/// <summary>
	/// SMS filter is not supported.
	/// </summary>
	public const int E_MBN_SMS_FILTER_NOT_SUPPORTED = unchecked((int)0x80548221);
	/// <summary>
	/// Invalid SMS memory index is used.
	/// </summary>
	public const int E_MBN_SMS_INVALID_MEMORY_INDEX = unchecked((int)0x80548222);
	/// <summary>
	/// SMS language is not supported.
	/// </summary>
	public const int E_MBN_SMS_LANG_NOT_SUPPORTED = unchecked((int)0x80548223);
	/// <summary>
	/// SMS memory failure occurred.
	/// </summary>
	public const int E_MBN_SMS_MEMORY_FAILURE = unchecked((int)0x80548224);
	/// <summary>
	/// SMS network timeout happened.
	/// </summary>
	public const int E_MBN_SMS_NETWORK_TIMEOUT = unchecked((int)0x80548225);
	/// <summary>
	/// Unknown SMSC address is used.
	/// </summary>
	public const int E_MBN_SMS_UNKNOWN_SMSC_ADDRESS = unchecked((int)0x80548226);
	/// <summary>
	/// SMS format is not supported.
	/// </summary>
	public const int E_MBN_SMS_FORMAT_NOT_SUPPORTED = unchecked((int)0x80548227);
	/// <summary>
	/// SMS operation is not allowed.
	/// </summary>
	public const int E_MBN_SMS_OPERATION_NOT_ALLOWED = unchecked((int)0x80548228);
	/// <summary>
	/// Device SMS memory is full.
	/// </summary>
	public const int E_MBN_SMS_MEMORY_FULL = unchecked((int)0x80548229);
	/// <summary>
	/// The IPv6 protocol is not installed.
	/// </summary>
	public const int PEER_E_IPV6_NOT_INSTALLED = unchecked((int)0x80630001);
	/// <summary>
	/// The component has not been initialized.
	/// </summary>
	public const int PEER_E_NOT_INITIALIZED = unchecked((int)0x80630002);
	/// <summary>
	/// The required service canot be started.
	/// </summary>
	public const int PEER_E_CANNOT_START_SERVICE = unchecked((int)0x80630003);
	/// <summary>
	/// The P2P protocol is not licensed to run on this OS.
	/// </summary>
	public const int PEER_E_NOT_LICENSED = unchecked((int)0x80630004);
	/// <summary>
	/// The graph handle is invalid.
	/// </summary>
	public const int PEER_E_INVALID_GRAPH = unchecked((int)0x80630010);
	/// <summary>
	/// The GRaphing database name has changed.
	/// </summary>
	public const int PEER_E_DBNAME_CHANGED = unchecked((int)0x80630011);
	/// <summary>
	/// A graph with the same ID already exists.
	/// </summary>
	public const int PEER_E_DUPLICATE_GRAPH = unchecked((int)0x80630012);
	/// <summary>
	/// The graph is not ready.
	/// </summary>
	public const int PEER_E_GRAPH_NOT_READY = unchecked((int)0x80630013);
	/// <summary>
	/// The graph is shutting down.
	/// </summary>
	public const int PEER_E_GRAPH_SHUTTING_DOWN = unchecked((int)0x80630014);
	/// <summary>
	/// The graph is still in use.
	/// </summary>
	public const int PEER_E_GRAPH_IN_USE = unchecked((int)0x80630015);
	/// <summary>
	/// The graph database is corrupt.
	/// </summary>
	public const int PEER_E_INVALID_DATABASE = unchecked((int)0x80630016);
	/// <summary>
	/// Too many attributes have been used.
	/// </summary>
	public const int PEER_E_TOO_MANY_ATTRIBUTES = unchecked((int)0x80630017);
	/// <summary>
	/// The connection can not be found.
	/// </summary>
	public const int PEER_E_CONNECTION_NOT_FOUND = unchecked((int)0x80630103);
	/// <summary>
	/// The peer attempted to connect to itself.
	/// </summary>
	public const int PEER_E_CONNECT_SELF = unchecked((int)0x80630106);
	/// <summary>
	/// The peer is already listening for connections.
	/// </summary>
	public const int PEER_E_ALREADY_LISTENING = unchecked((int)0x80630107);
	/// <summary>
	/// The node was not found.
	/// </summary>
	public const int PEER_E_NODE_NOT_FOUND = unchecked((int)0x80630108);
	/// <summary>
	/// The Connection attempt failed.
	/// </summary>
	public const int PEER_E_CONNECTION_FAILED = unchecked((int)0x80630109);
	/// <summary>
	/// The peer connection could not be authenticated.
	/// </summary>
	public const int PEER_E_CONNECTION_NOT_AUTHENTICATED = unchecked((int)0x8063010A);
	/// <summary>
	/// The connection was refused.
	/// </summary>
	public const int PEER_E_CONNECTION_REFUSED = unchecked((int)0x8063010B);
	/// <summary>
	/// The peer name classifier is too long.
	/// </summary>
	public const int PEER_E_CLASSIFIER_TOO_LONG = unchecked((int)0x80630201);
	/// <summary>
	/// The maximum number of identies have been created.
	/// </summary>
	public const int PEER_E_TOO_MANY_IDENTITIES = unchecked((int)0x80630202);
	/// <summary>
	/// Unable to access a key.
	/// </summary>
	public const int PEER_E_NO_KEY_ACCESS = unchecked((int)0x80630203);
	/// <summary>
	/// The group already exists.
	/// </summary>
	public const int PEER_E_GROUPS_EXIST = unchecked((int)0x80630204);
	/// <summary>
	/// The requested record could not be found.
	/// </summary>
	public const int PEER_E_RECORD_NOT_FOUND = unchecked((int)0x80630301);
	/// <summary>
	/// Access to the database was denied.
	/// </summary>
	public const int PEER_E_DATABASE_ACCESSDENIED = unchecked((int)0x80630302);
	/// <summary>
	/// The Database could not be initialized.
	/// </summary>
	public const int PEER_E_DBINITIALIZATION_FAILED = unchecked((int)0x80630303);
	/// <summary>
	/// The record is too big.
	/// </summary>
	public const int PEER_E_MAX_RECORD_SIZE_EXCEEDED = unchecked((int)0x80630304);
	/// <summary>
	/// The database already exists.
	/// </summary>
	public const int PEER_E_DATABASE_ALREADY_PRESENT = unchecked((int)0x80630305);
	/// <summary>
	/// The database could not be found.
	/// </summary>
	public const int PEER_E_DATABASE_NOT_PRESENT = unchecked((int)0x80630306);
	/// <summary>
	/// The identity could not be found.
	/// </summary>
	public const int PEER_E_IDENTITY_NOT_FOUND = unchecked((int)0x80630401);
	/// <summary>
	/// The event handle could not be found.
	/// </summary>
	public const int PEER_E_EVENT_HANDLE_NOT_FOUND = unchecked((int)0x80630501);
	/// <summary>
	/// Invalid search.
	/// </summary>
	public const int PEER_E_INVALID_SEARCH = unchecked((int)0x80630601);
	/// <summary>
	/// The search attributes are invalid.
	/// </summary>
	public const int PEER_E_INVALID_ATTRIBUTES = unchecked((int)0x80630602);
	/// <summary>
	/// The invitiation is not trusted.
	/// </summary>
	public const int PEER_E_INVITATION_NOT_TRUSTED = unchecked((int)0x80630701);
	/// <summary>
	/// The certchain is too long.
	/// </summary>
	public const int PEER_E_CHAIN_TOO_LONG = unchecked((int)0x80630703);
	/// <summary>
	/// The time period is invalid.
	/// </summary>
	public const int PEER_E_INVALID_TIME_PERIOD = unchecked((int)0x80630705);
	/// <summary>
	/// A circular cert chain was detected.
	/// </summary>
	public const int PEER_E_CIRCULAR_CHAIN_DETECTED = unchecked((int)0x80630706);
	/// <summary>
	/// The certstore is corrupted.
	/// </summary>
	public const int PEER_E_CERT_STORE_CORRUPTED = unchecked((int)0x80630801);
	/// <summary>
	/// The specified PNRP cloud deos not exist.
	/// </summary>
	public const int PEER_E_NO_CLOUD = unchecked((int)0x80631001);
	/// <summary>
	/// The cloud name is ambiguous.
	/// </summary>
	public const int PEER_E_CLOUD_NAME_AMBIGUOUS = unchecked((int)0x80631005);
	/// <summary>
	/// The record is invlaid.
	/// </summary>
	public const int PEER_E_INVALID_RECORD = unchecked((int)0x80632010);
	/// <summary>
	/// Not authorized.
	/// </summary>
	public const int PEER_E_NOT_AUTHORIZED = unchecked((int)0x80632020);
	/// <summary>
	/// The password does not meet policy requirements.
	/// </summary>
	public const int PEER_E_PASSWORD_DOES_NOT_MEET_POLICY = unchecked((int)0x80632021);
	/// <summary>
	/// The record validation has been defered.
	/// </summary>
	public const int PEER_E_DEFERRED_VALIDATION = unchecked((int)0x80632030);
	/// <summary>
	/// The group properies are invalid.
	/// </summary>
	public const int PEER_E_INVALID_GROUP_PROPERTIES = unchecked((int)0x80632040);
	/// <summary>
	/// The peername is invalid.
	/// </summary>
	public const int PEER_E_INVALID_PEER_NAME = unchecked((int)0x80632050);
	/// <summary>
	/// The classifier is invalid.
	/// </summary>
	public const int PEER_E_INVALID_CLASSIFIER = unchecked((int)0x80632060);
	/// <summary>
	/// The friendly name is invalid.
	/// </summary>
	public const int PEER_E_INVALID_FRIENDLY_NAME = unchecked((int)0x80632070);
	/// <summary>
	/// Invalid role property.
	/// </summary>
	public const int PEER_E_INVALID_ROLE_PROPERTY = unchecked((int)0x80632071);
	/// <summary>
	/// Invalid classifier protopery.
	/// </summary>
	public const int PEER_E_INVALID_CLASSIFIER_PROPERTY = unchecked((int)0x80632072);
	/// <summary>
	/// Invlaid record expiration.
	/// </summary>
	public const int PEER_E_INVALID_RECORD_EXPIRATION = unchecked((int)0x80632080);
	/// <summary>
	/// Invlaid credential info.
	/// </summary>
	public const int PEER_E_INVALID_CREDENTIAL_INFO = unchecked((int)0x80632081);
	/// <summary>
	/// Invalid credential.
	/// </summary>
	public const int PEER_E_INVALID_CREDENTIAL = unchecked((int)0x80632082);
	/// <summary>
	/// Invalid record size.
	/// </summary>
	public const int PEER_E_INVALID_RECORD_SIZE = unchecked((int)0x80632083);
	/// <summary>
	/// Unsupported version.
	/// </summary>
	public const int PEER_E_UNSUPPORTED_VERSION = unchecked((int)0x80632090);
	/// <summary>
	/// The group is not ready.
	/// </summary>
	public const int PEER_E_GROUP_NOT_READY = unchecked((int)0x80632091);
	/// <summary>
	/// The group is still in use.
	/// </summary>
	public const int PEER_E_GROUP_IN_USE = unchecked((int)0x80632092);
	/// <summary>
	/// The group is invalid.
	/// </summary>
	public const int PEER_E_INVALID_GROUP = unchecked((int)0x80632093);
	/// <summary>
	/// No members were found.
	/// </summary>
	public const int PEER_E_NO_MEMBERS_FOUND = unchecked((int)0x80632094);
	/// <summary>
	/// There are no member connections.
	/// </summary>
	public const int PEER_E_NO_MEMBER_CONNECTIONS = unchecked((int)0x80632095);
	/// <summary>
	/// Unable to listen.
	/// </summary>
	public const int PEER_E_UNABLE_TO_LISTEN = unchecked((int)0x80632096);
	/// <summary>
	/// The identity does not exist.
	/// </summary>
	public const int PEER_E_IDENTITY_DELETED = unchecked((int)0x806320A0);
	/// <summary>
	/// The service is not availible.
	/// </summary>
	public const int PEER_E_SERVICE_NOT_AVAILABLE = unchecked((int)0x806320A1);
	/// <summary>
	/// The contact could not be found.
	/// </summary>
	public const int PEER_E_CONTACT_NOT_FOUND = unchecked((int)0x80636001);
	/// <summary>
	/// The graph data was created.
	/// </summary>
	public const int PEER_S_GRAPH_DATA_CREATED = 0x00630001;
	/// <summary>
	/// There is not more event data.
	/// </summary>
	public const int PEER_S_NO_EVENT_DATA = 0x00630002;
	/// <summary>
	/// The graph is already connect.
	/// </summary>
	public const int PEER_S_ALREADY_CONNECTED = 0x00632000;
	/// <summary>
	/// The subscription already exists.
	/// </summary>
	public const int PEER_S_SUBSCRIPTION_EXISTS = 0x00636000;
	/// <summary>
	/// No connectivity.
	/// </summary>
	public const int PEER_S_NO_CONNECTIVITY = 0x00630005;
	/// <summary>
	/// Already a member.
	/// </summary>
	public const int PEER_S_ALREADY_A_MEMBER = 0x00630006;
	/// <summary>
	/// The peername could not be converted to a DNS pnrp name.
	/// </summary>
	public const int PEER_E_CANNOT_CONVERT_PEER_NAME = unchecked((int)0x80634001);
	/// <summary>
	/// Invalid peer host name.
	/// </summary>
	public const int PEER_E_INVALID_PEER_HOST_NAME = unchecked((int)0x80634002);
	/// <summary>
	/// No more data could be found.
	/// </summary>
	public const int PEER_E_NO_MORE = unchecked((int)0x80634003);
	/// <summary>
	/// The existing peer name is already registered.
	/// </summary>
	public const int PEER_E_PNRP_DUPLICATE_PEER_NAME = unchecked((int)0x80634005);
	/// <summary>
	/// The app invite request was canceld by the user.
	/// </summary>
	public const int PEER_E_INVITE_CANCELLED = unchecked((int)0x80637000);
	/// <summary>
	/// No respose ot the invite was received.
	/// </summary>
	public const int PEER_E_INVITE_RESPONSE_NOT_AVAILABLE = unchecked((int)0x80637001);
	/// <summary>
	/// User is not siged into serverless presence.
	/// </summary>
	public const int PEER_E_NOT_SIGNED_IN = unchecked((int)0x80637003);
	/// <summary>
	/// The user declinded the privacy policy prompt.
	/// </summary>
	public const int PEER_E_PRIVACY_DECLINED = unchecked((int)0x80637004);
	/// <summary>
	/// A timeout occured.
	/// </summary>
	public const int PEER_E_TIMEOUT = unchecked((int)0x80637005);
	/// <summary>
	/// The address is invalid.
	/// </summary>
	public const int PEER_E_INVALID_ADDRESS = unchecked((int)0x80637007);
	/// <summary>
	/// A required firewall exception is disabled.
	/// </summary>
	public const int PEER_E_FW_EXCEPTION_DISABLED = unchecked((int)0x80637008);
	/// <summary>
	/// The service is block by a firewall policy.
	/// </summary>
	public const int PEER_E_FW_BLOCKED_BY_POLICY = unchecked((int)0x80637009);
	/// <summary>
	/// Firewall exceptions are disabled.
	/// </summary>
	public const int PEER_E_FW_BLOCKED_BY_SHIELDS_UP = unchecked((int)0x8063700A);
	/// <summary>
	/// The user declinded to enable the firewall exceptions.
	/// </summary>
	public const int PEER_E_FW_DECLINED = unchecked((int)0x8063700B);
	/// <summary>
	/// The attribute handle given was not valid on this server.
	/// </summary>
	public const int E_BLUETOOTH_ATT_INVALID_HANDLE = unchecked((int)0x80650001);
	/// <summary>
	/// The attribute cannot be read.
	/// </summary>
	public const int E_BLUETOOTH_ATT_READ_NOT_PERMITTED = unchecked((int)0x80650002);
	/// <summary>
	/// The attribute cannot be written.
	/// </summary>
	public const int E_BLUETOOTH_ATT_WRITE_NOT_PERMITTED = unchecked((int)0x80650003);
	/// <summary>
	/// The attribute PDU was invalid.
	/// </summary>
	public const int E_BLUETOOTH_ATT_INVALID_PDU = unchecked((int)0x80650004);
	/// <summary>
	/// The attribute requires authentication before it can be read or written.
	/// </summary>
	public const int E_BLUETOOTH_ATT_INSUFFICIENT_AUTHENTICATION = unchecked((int)0x80650005);
	/// <summary>
	/// Attribute server does not support the request received from the client.
	/// </summary>
	public const int E_BLUETOOTH_ATT_REQUEST_NOT_SUPPORTED = unchecked((int)0x80650006);
	/// <summary>
	/// Offset specified was past the end of the attribute.
	/// </summary>
	public const int E_BLUETOOTH_ATT_INVALID_OFFSET = unchecked((int)0x80650007);
	/// <summary>
	/// The attribute requires authorization before it can be read or written.
	/// </summary>
	public const int E_BLUETOOTH_ATT_INSUFFICIENT_AUTHORIZATION = unchecked((int)0x80650008);
	/// <summary>
	/// Too many prepare writes have been queued.
	/// </summary>
	public const int E_BLUETOOTH_ATT_PREPARE_QUEUE_FULL = unchecked((int)0x80650009);
	/// <summary>
	/// No attribute found within the given attribute handle range.
	/// </summary>
	public const int E_BLUETOOTH_ATT_ATTRIBUTE_NOT_FOUND = unchecked((int)0x8065000A);
	/// <summary>
	/// The attribute cannot be read or written using the Read Blob Request.
	/// </summary>
	public const int E_BLUETOOTH_ATT_ATTRIBUTE_NOT_LONG = unchecked((int)0x8065000B);
	/// <summary>
	/// The Encryption Key Size used for encrypting this link is insufficient.
	/// </summary>
	public const int E_BLUETOOTH_ATT_INSUFFICIENT_ENCRYPTION_KEY_SIZE = unchecked((int)0x8065000C);
	/// <summary>
	/// The attribute value length is invalid for the operation.
	/// </summary>
	public const int E_BLUETOOTH_ATT_INVALID_ATTRIBUTE_VALUE_LENGTH = unchecked((int)0x8065000D);
	/// <summary>
	/// The attribute request that was requested has encountered an error that was unlikely, and therefore could not be completed as requested.
	/// </summary>
	public const int E_BLUETOOTH_ATT_UNLIKELY = unchecked((int)0x8065000E);
	/// <summary>
	/// The attribute requires encryption before it can be read or written.
	/// </summary>
	public const int E_BLUETOOTH_ATT_INSUFFICIENT_ENCRYPTION = unchecked((int)0x8065000F);
	/// <summary>
	/// The attribute type is not a supported grouping attribute as defined by a higher layer specification.
	/// </summary>
	public const int E_BLUETOOTH_ATT_UNSUPPORTED_GROUP_TYPE = unchecked((int)0x80650010);
	/// <summary>
	/// Insufficient Resources to complete the request.
	/// </summary>
	public const int E_BLUETOOTH_ATT_INSUFFICIENT_RESOURCES = unchecked((int)0x80650011);
	/// <summary>
	/// An error that lies in the reserved range has been received.
	/// </summary>
	public const int E_BLUETOOTH_ATT_UNKNOWN_ERROR = unchecked((int)0x80651000);
	#endregion

	#region UI, Audio, DirectX, Codec
	/// <summary>
	/// The object could not be created.
	/// </summary>
	public const int UI_E_CREATE_FAILED = unchecked((int)0x802A0001);
	/// <summary>
	/// Shutdown was already called on this object or the object that owns it.
	/// </summary>
	public const int UI_E_SHUTDOWN_CALLED = unchecked((int)0x802A0002);
	/// <summary>
	/// This method cannot be called during this type of callback.
	/// </summary>
	public const int UI_E_ILLEGAL_REENTRANCY = unchecked((int)0x802A0003);
	/// <summary>
	/// This object has been sealed, so this change is no longer allowed.
	/// </summary>
	public const int UI_E_OBJECT_SEALED = unchecked((int)0x802A0004);
	/// <summary>
	/// The requested value was never set.
	/// </summary>
	public const int UI_E_VALUE_NOT_SET = unchecked((int)0x802A0005);
	/// <summary>
	/// The requested value cannot be determined.
	/// </summary>
	public const int UI_E_VALUE_NOT_DETERMINED = unchecked((int)0x802A0006);
	/// <summary>
	/// A callback returned an invalid output parameter.
	/// </summary>
	public const int UI_E_INVALID_OUTPUT = unchecked((int)0x802A0007);
	/// <summary>
	/// A callback returned a success code other than S_OK or S_FALSE.
	/// </summary>
	public const int UI_E_BOOLEAN_EXPECTED = unchecked((int)0x802A0008);
	/// <summary>
	/// A parameter that should be owned by this object is owned by a different object.
	/// </summary>
	public const int UI_E_DIFFERENT_OWNER = unchecked((int)0x802A0009);
	/// <summary>
	/// More than one item matched the search criteria.
	/// </summary>
	public const int UI_E_AMBIGUOUS_MATCH = unchecked((int)0x802A000A);
	/// <summary>
	/// A floating-point overflow occurred.
	/// </summary>
	public const int UI_E_FP_OVERFLOW = unchecked((int)0x802A000B);
	/// <summary>
	/// This method can only be called from the thread that created the object.
	/// </summary>
	public const int UI_E_WRONG_THREAD = unchecked((int)0x802A000C);
	/// <summary>
	/// The storyboard is currently in the schedule.
	/// </summary>
	public const int UI_E_STORYBOARD_ACTIVE = unchecked((int)0x802A0101);
	/// <summary>
	/// The storyboard is not playing.
	/// </summary>
	public const int UI_E_STORYBOARD_NOT_PLAYING = unchecked((int)0x802A0102);
	/// <summary>
	/// The start keyframe might occur after the end keyframe.
	/// </summary>
	public const int UI_E_START_KEYFRAME_AFTER_END = unchecked((int)0x802A0103);
	/// <summary>
	/// It might not be possible to determine the end keyframe time when the start keyframe is reached.
	/// </summary>
	public const int UI_E_END_KEYFRAME_NOT_DETERMINED = unchecked((int)0x802A0104);
	/// <summary>
	/// Two repeated portions of a storyboard might overlap.
	/// </summary>
	public const int UI_E_LOOPS_OVERLAP = unchecked((int)0x802A0105);
	/// <summary>
	/// The transition has already been added to a storyboard.
	/// </summary>
	public const int UI_E_TRANSITION_ALREADY_USED = unchecked((int)0x802A0106);
	/// <summary>
	/// The transition has not been added to a storyboard.
	/// </summary>
	public const int UI_E_TRANSITION_NOT_IN_STORYBOARD = unchecked((int)0x802A0107);
	/// <summary>
	/// The transition might eclipse the beginning of another transition in the storyboard.
	/// </summary>
	public const int UI_E_TRANSITION_ECLIPSED = unchecked((int)0x802A0108);
	/// <summary>
	/// The given time is earlier than the time passed to the last update.
	/// </summary>
	public const int UI_E_TIME_BEFORE_LAST_UPDATE = unchecked((int)0x802A0109);
	/// <summary>
	/// This client is already connected to a timer.
	/// </summary>
	public const int UI_E_TIMER_CLIENT_ALREADY_CONNECTED = unchecked((int)0x802A010A);
	/// <summary>
	/// The passed dimension is invalid or does not match the object's dimension.
	/// </summary>
	public const int UI_E_INVALID_DIMENSION = unchecked((int)0x802A010B);
	/// <summary>
	/// The added primitive begins at or beyond the duration of the interpolator.
	/// </summary>
	public const int UI_E_PRIMITIVE_OUT_OF_BOUNDS = unchecked((int)0x802A010C);
	/// <summary>
	/// The operation cannot be completed because the window is being closed.
	/// </summary>
	public const int UI_E_WINDOW_CLOSED = unchecked((int)0x802A0201);
	/// <summary>
	/// PortCls could not find an audio engine node exposed by a miniport driver claiming support for IMiniportAudioEngineNode.
	/// </summary>
	public const int E_AUDIO_ENGINE_NODE_NOT_FOUND = unchecked((int)0x80660001);
	/// <summary>
	/// The Present operation was invisible to the user.
	/// </summary>
	public const int DXGI_STATUS_OCCLUDED = 0x087A0001;
	/// <summary>
	/// The Present operation was partially invisible to the user.
	/// </summary>
	public const int DXGI_STATUS_CLIPPED = 0x087A0002;
	/// <summary>
	/// The driver is requesting that the DXGI runtime not use shared resources to communicate with the Desktop Window Manager.
	/// </summary>
	public const int DXGI_STATUS_NO_REDIRECTION = 0x087A0004;
	/// <summary>
	/// The Present operation was not visible because the Windows session has switched to another desktop (for example, ctrl-alt-del).
	/// </summary>
	public const int DXGI_STATUS_NO_DESKTOP_ACCESS = 0x087A0005;
	/// <summary>
	/// The Present operation was not visible because the target monitor was being used for some other purpose.
	/// </summary>
	public const int DXGI_STATUS_GRAPHICS_VIDPN_SOURCE_IN_USE = 0x087A0006;
	/// <summary>
	/// The Present operation was not visible because the display mode changed. DXGI will have re-attempted the presentation.
	/// </summary>
	public const int DXGI_STATUS_MODE_CHANGED = 0x087A0007;
	/// <summary>
	/// The Present operation was not visible because another Direct3D device was attempting to take fullscreen mode at the time.
	/// </summary>
	public const int DXGI_STATUS_MODE_CHANGE_IN_PROGRESS = 0x087A0008;
	/// <summary>
	/// The application made a call that is invalid. Either the parameters of the call or the state of some object was incorrect. Enable the D3D debug layer in order to see details via debug messages.
	/// </summary>
	public const int DXGI_ERROR_INVALID_CALL = unchecked((int)0x887A0001);
	/// <summary>
	/// The object was not found. If calling IDXGIFactory::EnumAdaptes, there is no adapter with the specified ordinal.
	/// </summary>
	public const int DXGI_ERROR_NOT_FOUND = unchecked((int)0x887A0002);
	/// <summary>
	/// The caller did not supply a sufficiently large buffer.
	/// </summary>
	public const int DXGI_ERROR_MORE_DATA = unchecked((int)0x887A0003);
	/// <summary>
	/// The specified device interface or feature level is not supported on this system.
	/// </summary>
	public const int DXGI_ERROR_UNSUPPORTED = unchecked((int)0x887A0004);
	/// <summary>
	/// The GPU device instance has been suspended. Use GetDeviceRemovedReason to determine the appropriate action.
	/// </summary>
	public const int DXGI_ERROR_DEVICE_REMOVED = unchecked((int)0x887A0005);
	/// <summary>
	/// The GPU will not respond to more commands, most likely because of an invalid command passed by the calling application.
	/// </summary>
	public const int DXGI_ERROR_DEVICE_HUNG = unchecked((int)0x887A0006);
	/// <summary>
	/// The GPU will not respond to more commands, most likely because some other application submitted invalid commands. The calling application should re-create the device and continue.
	/// </summary>
	public const int DXGI_ERROR_DEVICE_RESET = unchecked((int)0x887A0007);
	/// <summary>
	/// The GPU was busy at the moment when the call was made, and the call was neither executed nor scheduled.
	/// </summary>
	public const int DXGI_ERROR_WAS_STILL_DRAWING = unchecked((int)0x887A000A);
	/// <summary>
	/// An event (such as power cycle) interrupted the gathering of presentation statistics. Any previous statistics should be considered invalid.
	/// </summary>
	public const int DXGI_ERROR_FRAME_STATISTICS_DISJOINT = unchecked((int)0x887A000B);
	/// <summary>
	/// Fullscreen mode could not be achieved because the specified output was already in use.
	/// </summary>
	public const int DXGI_ERROR_GRAPHICS_VIDPN_SOURCE_IN_USE = unchecked((int)0x887A000C);
	/// <summary>
	/// An internal issue prevented the driver from carrying out the specified operation. The driver's state is probably suspect, and the application should not continue.
	/// </summary>
	public const int DXGI_ERROR_DRIVER_INTERNAL_ERROR = unchecked((int)0x887A0020);
	/// <summary>
	/// A global counter resource was in use, and the specified counter cannot be used by this Direct3D device at this time.
	/// </summary>
	public const int DXGI_ERROR_NONEXCLUSIVE = unchecked((int)0x887A0021);
	/// <summary>
	/// A resource is not available at the time of the call, but may become available later.
	/// </summary>
	public const int DXGI_ERROR_NOT_CURRENTLY_AVAILABLE = unchecked((int)0x887A0022);
	/// <summary>
	/// The application's remote device has been removed due to session disconnect or network disconnect. The application should call IDXGIFactory1::IsCurrent to find out when the remote device becomes available again.
	/// </summary>
	public const int DXGI_ERROR_REMOTE_CLIENT_DISCONNECTED = unchecked((int)0x887A0023);
	/// <summary>
	/// The device has been removed during a remote session because the remote computer ran out of memory.
	/// </summary>
	public const int DXGI_ERROR_REMOTE_OUTOFMEMORY = unchecked((int)0x887A0024);
	/// <summary>
	/// The keyed mutex was abandoned.
	/// </summary>
	public const int DXGI_ERROR_ACCESS_LOST = unchecked((int)0x887A0026);
	/// <summary>
	/// The timeout value has elapsed and the resource is not yet available.
	/// </summary>
	public const int DXGI_ERROR_WAIT_TIMEOUT = unchecked((int)0x887A0027);
	/// <summary>
	/// The output duplication has been turned off because the Windows session ended or was disconnected. This happens when a remote user disconnects, or when "switch user" is used locally.
	/// </summary>
	public const int DXGI_ERROR_SESSION_DISCONNECTED = unchecked((int)0x887A0028);
	/// <summary>
	/// The DXGI outuput (monitor) to which the swapchain content was restricted, has been disconnected or changed.
	/// </summary>
	public const int DXGI_ERROR_RESTRICT_TO_OUTPUT_STALE = unchecked((int)0x887A0029);
	/// <summary>
	/// DXGI is unable to provide content protection on the swapchain. This is typically caused by an older driver, or by the application using a swapchain that is incompatible with content protection.
	/// </summary>
	public const int DXGI_ERROR_CANNOT_PROTECT_CONTENT = unchecked((int)0x887A002A);
	/// <summary>
	/// The application is trying to use a resource to which it does not have the required access privileges. This is most commonly caused by writing to a shared resource with read-only access.
	/// </summary>
	public const int DXGI_ERROR_ACCESS_DENIED = unchecked((int)0x887A002B);
	/// <summary>
	/// The swapchain has become unoccluded.
	/// </summary>
	public const int DXGI_STATUS_UNOCCLUDED = 0x087A0009;
	/// <summary>
	/// The adapter did not have access to the required resources to complete the Desktop Duplication Present() call, the Present() call needs to be made again.
	/// </summary>
	public const int DXGI_STATUS_DDA_WAS_STILL_DRAWING = 0x087A000A;
	/// <summary>
	/// An on-going mode change prevented completion of the call. The call may succeed if attempted later.
	/// </summary>
	public const int DXGI_ERROR_MODE_CHANGE_IN_PROGRESS = unchecked((int)0x887A0025);
	/// <summary>
	/// The GPU was busy when the operation was requested.
	/// </summary>
	public const int DXGI_DDI_ERR_WASSTILLDRAWING = unchecked((int)0x887B0001);
	/// <summary>
	/// The driver has rejected the creation of this resource.
	/// </summary>
	public const int DXGI_DDI_ERR_UNSUPPORTED = unchecked((int)0x887B0002);
	/// <summary>
	/// The GPU counter was in use by another process or d3d device when application requested access to it.
	/// </summary>
	public const int DXGI_DDI_ERR_NONEXCLUSIVE = unchecked((int)0x887B0003);
	/// <summary>
	/// The application has exceeded the maximum number of unique state objects per Direct3D device. The limit is 4096 for feature levels up to 11.1.
	/// </summary>
	public const int D3D10_ERROR_TOO_MANY_UNIQUE_STATE_OBJECTS = unchecked((int)0x88790001);
	/// <summary>
	/// The specified file was not found.
	/// </summary>
	public const int D3D10_ERROR_FILE_NOT_FOUND = unchecked((int)0x88790002);
	/// <summary>
	/// The application has exceeded the maximum number of unique state objects per Direct3D device. The limit is 4096 for feature levels up to 11.1.
	/// </summary>
	public const int D3D11_ERROR_TOO_MANY_UNIQUE_STATE_OBJECTS = unchecked((int)0x887C0001);
	/// <summary>
	/// The specified file was not found.
	/// </summary>
	public const int D3D11_ERROR_FILE_NOT_FOUND = unchecked((int)0x887C0002);
	/// <summary>
	/// The application has exceeded the maximum number of unique view objects per Direct3D device. The limit is 2^20 for feature levels up to 11.1.
	/// </summary>
	public const int D3D11_ERROR_TOO_MANY_UNIQUE_VIEW_OBJECTS = unchecked((int)0x887C0003);
	/// <summary>
	/// The application's first call per command list to Map on a deferred context did not use D3D11_MAP_WRITE_DISCARD.
	/// </summary>
	public const int D3D11_ERROR_DEFERRED_CONTEXT_MAP_WITHOUT_INITIAL_DISCARD = unchecked((int)0x887C0004);
	/// <summary>
	/// The object was not in the correct state to process the method.
	/// </summary>
	public const int D2DERR_WRONG_STATE = unchecked((int)0x88990001);
	/// <summary>
	/// The object has not yet been initialized.
	/// </summary>
	public const int D2DERR_NOT_INITIALIZED = unchecked((int)0x88990002);
	/// <summary>
	/// The requested operation is not supported.
	/// </summary>
	public const int D2DERR_UNSUPPORTED_OPERATION = unchecked((int)0x88990003);
	/// <summary>
	/// The geometry scanner failed to process the data.
	/// </summary>
	public const int D2DERR_SCANNER_FAILED = unchecked((int)0x88990004);
	/// <summary>
	/// Direct2D could not access the screen.
	/// </summary>
	public const int D2DERR_SCREEN_ACCESS_DENIED = unchecked((int)0x88990005);
	/// <summary>
	/// A valid display state could not be determined.
	/// </summary>
	public const int D2DERR_DISPLAY_STATE_INVALID = unchecked((int)0x88990006);
	/// <summary>
	/// The supplied vector is zero.
	/// </summary>
	public const int D2DERR_ZERO_VECTOR = unchecked((int)0x88990007);
	/// <summary>
	/// An internal error (Direct2D bug) occurred. On checked builds, we would assert. The application should close this instance of Direct2D and should consider restarting its process.
	/// </summary>
	public const int D2DERR_INTERNAL_ERROR = unchecked((int)0x88990008);
	/// <summary>
	/// The display format Direct2D needs to render is not supported by the hardware device.
	/// </summary>
	public const int D2DERR_DISPLAY_FORMAT_NOT_SUPPORTED = unchecked((int)0x88990009);
	/// <summary>
	/// A call to this method is invalid.
	/// </summary>
	public const int D2DERR_INVALID_CALL = unchecked((int)0x8899000A);
	/// <summary>
	/// No hardware rendering device is available for this operation.
	/// </summary>
	public const int D2DERR_NO_HARDWARE_DEVICE = unchecked((int)0x8899000B);
	/// <summary>
	/// There has been a presentation error that may be recoverable. The caller needs to recreate, rerender the entire frame, and reattempt present.
	/// </summary>
	public const int D2DERR_RECREATE_TARGET = unchecked((int)0x8899000C);
	/// <summary>
	/// Shader construction failed because it was too complex.
	/// </summary>
	public const int D2DERR_TOO_MANY_SHADER_ELEMENTS = unchecked((int)0x8899000D);
	/// <summary>
	/// Shader compilation failed.
	/// </summary>
	public const int D2DERR_SHADER_COMPILE_FAILED = unchecked((int)0x8899000E);
	/// <summary>
	/// Requested DirectX surface size exceeded maximum texture size.
	/// </summary>
	public const int D2DERR_MAX_TEXTURE_SIZE_EXCEEDED = unchecked((int)0x8899000F);
	/// <summary>
	/// The requested Direct2D version is not supported.
	/// </summary>
	public const int D2DERR_UNSUPPORTED_VERSION = unchecked((int)0x88990010);
	/// <summary>
	/// Invalid number.
	/// </summary>
	public const int D2DERR_BAD_NUMBER = unchecked((int)0x88990011);
	/// <summary>
	/// Objects used together must be created from the same factory instance.
	/// </summary>
	public const int D2DERR_WRONG_FACTORY = unchecked((int)0x88990012);
	/// <summary>
	/// A layer resource can only be in use once at any point in time.
	/// </summary>
	public const int D2DERR_LAYER_ALREADY_IN_USE = unchecked((int)0x88990013);
	/// <summary>
	/// The pop call did not match the corresponding push call.
	/// </summary>
	public const int D2DERR_POP_CALL_DID_NOT_MATCH_PUSH = unchecked((int)0x88990014);
	/// <summary>
	/// The resource was realized on the wrong render target.
	/// </summary>
	public const int D2DERR_WRONG_RESOURCE_DOMAIN = unchecked((int)0x88990015);
	/// <summary>
	/// The push and pop calls were unbalanced.
	/// </summary>
	public const int D2DERR_PUSH_POP_UNBALANCED = unchecked((int)0x88990016);
	/// <summary>
	/// Attempt to copy from a render target while a layer or clip rect is applied.
	/// </summary>
	public const int D2DERR_RENDER_TARGET_HAS_LAYER_OR_CLIPRECT = unchecked((int)0x88990017);
	/// <summary>
	/// The brush types are incompatible for the call.
	/// </summary>
	public const int D2DERR_INCOMPATIBLE_BRUSH_TYPES = unchecked((int)0x88990018);
	/// <summary>
	/// An unknown win32 failure occurred.
	/// </summary>
	public const int D2DERR_WIN32_ERROR = unchecked((int)0x88990019);
	/// <summary>
	/// The render target is not compatible with GDI.
	/// </summary>
	public const int D2DERR_TARGET_NOT_GDI_COMPATIBLE = unchecked((int)0x8899001A);
	/// <summary>
	/// A text client drawing effect object is of the wrong type.
	/// </summary>
	public const int D2DERR_TEXT_EFFECT_IS_WRONG_TYPE = unchecked((int)0x8899001B);
	/// <summary>
	/// The application is holding a reference to the IDWriteTextRenderer interface after the corresponding DrawText or DrawTextLayout call has returned. The IDWriteTextRenderer instance will be invalid.
	/// </summary>
	public const int D2DERR_TEXT_RENDERER_NOT_RELEASED = unchecked((int)0x8899001C);
	/// <summary>
	/// The requested size is larger than the guaranteed supported texture size at the Direct3D device's current feature level.
	/// </summary>
	public const int D2DERR_EXCEEDS_MAX_BITMAP_SIZE = unchecked((int)0x8899001D);
	/// <summary>
	/// There was a configuration error in the graph.
	/// </summary>
	public const int D2DERR_INVALID_GRAPH_CONFIGURATION = unchecked((int)0x8899001E);
	/// <summary>
	/// There was a internal configuration error in the graph.
	/// </summary>
	public const int D2DERR_INVALID_INTERNAL_GRAPH_CONFIGURATION = unchecked((int)0x8899001F);
	/// <summary>
	/// There was a cycle in the graph.
	/// </summary>
	public const int D2DERR_CYCLIC_GRAPH = unchecked((int)0x88990020);
	/// <summary>
	/// Cannot draw with a bitmap that has the D2D1_BITMAP_OPTIONS_CANNOT_DRAW option.
	/// </summary>
	public const int D2DERR_BITMAP_CANNOT_DRAW = unchecked((int)0x88990021);
	/// <summary>
	/// The operation cannot complete while there are outstanding references to the target bitmap.
	/// </summary>
	public const int D2DERR_OUTSTANDING_BITMAP_REFERENCES = unchecked((int)0x88990022);
	/// <summary>
	/// The operation failed because the original target is not currently bound as a target.
	/// </summary>
	public const int D2DERR_ORIGINAL_TARGET_NOT_BOUND = unchecked((int)0x88990023);
	/// <summary>
	/// Cannot set the image as a target because it is either an effect or is a bitmap that does not have the D2D1_BITMAP_OPTIONS_TARGET flag set.
	/// </summary>
	public const int D2DERR_INVALID_TARGET = unchecked((int)0x88990024);
	/// <summary>
	/// Cannot draw with a bitmap that is currently bound as the target bitmap.
	/// </summary>
	public const int D2DERR_BITMAP_BOUND_AS_TARGET = unchecked((int)0x88990025);
	/// <summary>
	/// D3D Device does not have sufficient capabilities to perform the requested action.
	/// </summary>
	public const int D2DERR_INSUFFICIENT_DEVICE_CAPABILITIES = unchecked((int)0x88990026);
	/// <summary>
	/// The graph could not be rendered with the context's current tiling settings.
	/// </summary>
	public const int D2DERR_INTERMEDIATE_TOO_LARGE = unchecked((int)0x88990027);
	/// <summary>
	/// The CLSID provided to Unregister did not correspond to a registered effect.
	/// </summary>
	public const int D2DERR_EFFECT_IS_NOT_REGISTERED = unchecked((int)0x88990028);
	/// <summary>
	/// The specified property does not exist.
	/// </summary>
	public const int D2DERR_INVALID_PROPERTY = unchecked((int)0x88990029);
	/// <summary>
	/// The specified sub-property does not exist.
	/// </summary>
	public const int D2DERR_NO_SUBPROPERTIES = unchecked((int)0x8899002A);
	/// <summary>
	/// AddPage or Close called after print job is already closed.
	/// </summary>
	public const int D2DERR_PRINT_JOB_CLOSED = unchecked((int)0x8899002B);
	/// <summary>
	/// Error during print control creation. Indicates that none of the package target types (representing printer formats) are supported by Direct2D print control.
	/// </summary>
	public const int D2DERR_PRINT_FORMAT_NOT_SUPPORTED = unchecked((int)0x8899002C);
	/// <summary>
	/// An effect attempted to use a transform with too many inputs.
	/// </summary>
	public const int D2DERR_TOO_MANY_TRANSFORM_INPUTS = unchecked((int)0x8899002D);
	/// <summary>
	/// Indicates an error in an input file such as a font file.
	/// </summary>
	public const int DWRITE_E_FILEFORMAT = unchecked((int)0x88985000);
	/// <summary>
	/// Indicates an error originating in DirectWrite code, which is not expected to occur but is safe to recover from.
	/// </summary>
	public const int DWRITE_E_UNEXPECTED = unchecked((int)0x88985001);
	/// <summary>
	/// Indicates the specified font does not exist.
	/// </summary>
	public const int DWRITE_E_NOFONT = unchecked((int)0x88985002);
	/// <summary>
	/// A font file could not be opened because the file, directory, network location, drive, or other storage location does not exist or is unavailable.
	/// </summary>
	public const int DWRITE_E_FILENOTFOUND = unchecked((int)0x88985003);
	/// <summary>
	/// A font file exists but could not be opened due to access denied, sharing violation, or similar error.
	/// </summary>
	public const int DWRITE_E_FILEACCESS = unchecked((int)0x88985004);
	/// <summary>
	/// A font collection is obsolete due to changes in the system.
	/// </summary>
	public const int DWRITE_E_FONTCOLLECTIONOBSOLETE = unchecked((int)0x88985005);
	/// <summary>
	/// The given interface is already registered.
	/// </summary>
	public const int DWRITE_E_ALREADYREGISTERED = unchecked((int)0x88985006);
	/// <summary>
	/// The font cache contains invalid data.
	/// </summary>
	public const int DWRITE_E_CACHEFORMAT = unchecked((int)0x88985007);
	/// <summary>
	/// A font cache file corresponds to a different version of DirectWrite.
	/// </summary>
	public const int DWRITE_E_CACHEVERSION = unchecked((int)0x88985008);
	/// <summary>
	/// The operation is not supported for this type of font.
	/// </summary>
	public const int DWRITE_E_UNSUPPORTEDOPERATION = unchecked((int)0x88985009);
	/// <summary>
	/// The codec is in the wrong state.
	/// </summary>
	public const int WINCODEC_ERR_WRONGSTATE = unchecked((int)0x88982F04);
	/// <summary>
	/// The value is out of range.
	/// </summary>
	public const int WINCODEC_ERR_VALUEOUTOFRANGE = unchecked((int)0x88982F05);
	/// <summary>
	/// The image format is unknown.
	/// </summary>
	public const int WINCODEC_ERR_UNKNOWNIMAGEFORMAT = unchecked((int)0x88982F07);
	/// <summary>
	/// The SDK version is unsupported.
	/// </summary>
	public const int WINCODEC_ERR_UNSUPPORTEDVERSION = unchecked((int)0x88982F0B);
	/// <summary>
	/// The component is not initialized.
	/// </summary>
	public const int WINCODEC_ERR_NOTINITIALIZED = unchecked((int)0x88982F0C);
	/// <summary>
	/// There is already an outstanding read or write lock.
	/// </summary>
	public const int WINCODEC_ERR_ALREADYLOCKED = unchecked((int)0x88982F0D);
	/// <summary>
	/// The specified bitmap property cannot be found.
	/// </summary>
	public const int WINCODEC_ERR_PROPERTYNOTFOUND = unchecked((int)0x88982F40);
	/// <summary>
	/// The bitmap codec does not support the bitmap property.
	/// </summary>
	public const int WINCODEC_ERR_PROPERTYNOTSUPPORTED = unchecked((int)0x88982F41);
	/// <summary>
	/// The bitmap property size is invalid.
	/// </summary>
	public const int WINCODEC_ERR_PROPERTYSIZE = unchecked((int)0x88982F42);
	/// <summary>
	/// An unknown error has occurred.
	/// </summary>
	public const int WINCODEC_ERR_CODECPRESENT = unchecked((int)0x88982F43);
	/// <summary>
	/// The bitmap codec does not support a thumbnail.
	/// </summary>
	public const int WINCODEC_ERR_CODECNOTHUMBNAIL = unchecked((int)0x88982F44);
	/// <summary>
	/// The bitmap palette is unavailable.
	/// </summary>
	public const int WINCODEC_ERR_PALETTEUNAVAILABLE = unchecked((int)0x88982F45);
	/// <summary>
	/// Too many scanlines were requested.
	/// </summary>
	public const int WINCODEC_ERR_CODECTOOMANYSCANLINES = unchecked((int)0x88982F46);
	/// <summary>
	/// An internal error occurred.
	/// </summary>
	public const int WINCODEC_ERR_INTERNALERROR = unchecked((int)0x88982F48);
	/// <summary>
	/// The bitmap bounds do not match the bitmap dimensions.
	/// </summary>
	public const int WINCODEC_ERR_SOURCERECTDOESNOTMATCHDIMENSIONS = unchecked((int)0x88982F49);
	/// <summary>
	/// The component cannot be found.
	/// </summary>
	public const int WINCODEC_ERR_COMPONENTNOTFOUND = unchecked((int)0x88982F50);
	/// <summary>
	/// The bitmap size is outside the valid range.
	/// </summary>
	public const int WINCODEC_ERR_IMAGESIZEOUTOFRANGE = unchecked((int)0x88982F51);
	/// <summary>
	/// There is too much metadata to be written to the bitmap.
	/// </summary>
	public const int WINCODEC_ERR_TOOMUCHMETADATA = unchecked((int)0x88982F52);
	/// <summary>
	/// The image is unrecognized.
	/// </summary>
	public const int WINCODEC_ERR_BADIMAGE = unchecked((int)0x88982F60);
	/// <summary>
	/// The image header is unrecognized.
	/// </summary>
	public const int WINCODEC_ERR_BADHEADER = unchecked((int)0x88982F61);
	/// <summary>
	/// The bitmap frame is missing.
	/// </summary>
	public const int WINCODEC_ERR_FRAMEMISSING = unchecked((int)0x88982F62);
	/// <summary>
	/// The image metadata header is unrecognized.
	/// </summary>
	public const int WINCODEC_ERR_BADMETADATAHEADER = unchecked((int)0x88982F63);
	/// <summary>
	/// The stream data is unrecognized.
	/// </summary>
	public const int WINCODEC_ERR_BADSTREAMDATA = unchecked((int)0x88982F70);
	/// <summary>
	/// Failed to write to the stream.
	/// </summary>
	public const int WINCODEC_ERR_STREAMWRITE = unchecked((int)0x88982F71);
	/// <summary>
	/// Failed to read from the stream.
	/// </summary>
	public const int WINCODEC_ERR_STREAMREAD = unchecked((int)0x88982F72);
	/// <summary>
	/// The stream is not available.
	/// </summary>
	public const int WINCODEC_ERR_STREAMNOTAVAILABLE = unchecked((int)0x88982F73);
	/// <summary>
	/// The bitmap pixel format is unsupported.
	/// </summary>
	public const int WINCODEC_ERR_UNSUPPORTEDPIXELFORMAT = unchecked((int)0x88982F80);
	/// <summary>
	/// The operation is unsupported.
	/// </summary>
	public const int WINCODEC_ERR_UNSUPPORTEDOPERATION = unchecked((int)0x88982F81);
	/// <summary>
	/// The component registration is invalid.
	/// </summary>
	public const int WINCODEC_ERR_INVALIDREGISTRATION = unchecked((int)0x88982F8A);
	/// <summary>
	/// The component initialization has failed.
	/// </summary>
	public const int WINCODEC_ERR_COMPONENTINITIALIZEFAILURE = unchecked((int)0x88982F8B);
	/// <summary>
	/// The buffer allocated is insufficient.
	/// </summary>
	public const int WINCODEC_ERR_INSUFFICIENTBUFFER = unchecked((int)0x88982F8C);
	/// <summary>
	/// Duplicate metadata is present.
	/// </summary>
	public const int WINCODEC_ERR_DUPLICATEMETADATAPRESENT = unchecked((int)0x88982F8D);
	/// <summary>
	/// The bitmap property type is unexpected.
	/// </summary>
	public const int WINCODEC_ERR_PROPERTYUNEXPECTEDTYPE = unchecked((int)0x88982F8E);
	/// <summary>
	/// The size is unexpected.
	/// </summary>
	public const int WINCODEC_ERR_UNEXPECTEDSIZE = unchecked((int)0x88982F8F);
	/// <summary>
	/// The property query is invalid.
	/// </summary>
	public const int WINCODEC_ERR_INVALIDQUERYREQUEST = unchecked((int)0x88982F90);
	/// <summary>
	/// The metadata type is unexpected.
	/// </summary>
	public const int WINCODEC_ERR_UNEXPECTEDMETADATATYPE = unchecked((int)0x88982F91);
	/// <summary>
	/// The specified bitmap property is only valid at root level.
	/// </summary>
	public const int WINCODEC_ERR_REQUESTONLYVALIDATMETADATAROOT = unchecked((int)0x88982F92);
	/// <summary>
	/// The query string contains an invalid character.
	/// </summary>
	public const int WINCODEC_ERR_INVALIDQUERYCHARACTER = unchecked((int)0x88982F93);
	/// <summary>
	/// Windows Codecs received an error from the Win32 system.
	/// </summary>
	public const int WINCODEC_ERR_WIN32ERROR = unchecked((int)0x88982F94);
	/// <summary>
	/// The requested level of detail is not present.
	/// </summary>
	public const int WINCODEC_ERR_INVALIDPROGRESSIVELEVEL = unchecked((int)0x88982F95);
	#endregion

	public static bool Succeeded(int result) { return result >= 0; }

	public static bool Failed(int result) { return result < 0; }
}

public static class ResultWin32
{
	/// <summary>
	///     The operation completed successfully.
	/// </summary>
	public const int ERROR_SUCCESS = 0;

	/// <summary>
	///     Incorrect function.
	/// </summary>
	public const int ERROR_INVALID_FUNCTION = 1;

	/// <summary>
	///     The system cannot find the file specified.
	/// </summary>
	public const int ERROR_FILE_NOT_FOUND = 2;

	/// <summary>
	///     The system cannot find the path specified.
	/// </summary>
	public const int ERROR_PATH_NOT_FOUND = 3;

	/// <summary>
	///     The system cannot open the file.
	/// </summary>
	public const int ERROR_TOO_MANY_OPEN_FILES = 4;

	/// <summary>
	///     Access is denied.
	/// </summary>
	public const int ERROR_ACCESS_DENIED = 5;

	/// <summary>
	///     The handle is invalid.
	/// </summary>
	public const int ERROR_INVALID_HANDLE = 6;

	/// <summary>
	///     The storage control blocks were destroyed.
	/// </summary>
	public const int ERROR_ARENA_TRASHED = 7;

	/// <summary>
	///     Not enough storage is available to process this command.
	/// </summary>
	public const int ERROR_NOT_ENOUGH_MEMORY = 8;

	/// <summary>
	///     The storage control block address is invalid.
	/// </summary>
	public const int ERROR_INVALID_BLOCK = 9;

	/// <summary>
	///     The environment is incorrect.
	/// </summary>
	public const int ERROR_BAD_ENVIRONMENT = 10;

	/// <summary>
	///     An attempt was made to load a program with an incorrect format.
	/// </summary>
	public const int ERROR_BAD_FORMAT = 11;

	/// <summary>
	///     The access code is invalid.
	/// </summary>
	public const int ERROR_INVALID_ACCESS = 12;

	/// <summary>
	///     The data is invalid.
	/// </summary>
	public const int ERROR_INVALID_DATA = 13;

	/// <summary>
	///     Not enough storage is available to complete this operation.
	/// </summary>
	public const int ERROR_OUTOFMEMORY = 14;

	/// <summary>
	///     The system cannot find the drive specified.
	/// </summary>
	public const int ERROR_INVALID_DRIVE = 15;

	/// <summary>
	///     The directory cannot be removed.
	/// </summary>
	public const int ERROR_CURRENT_DIRECTORY = 16;

	/// <summary>
	///     The system cannot move the file to a different disk drive.
	/// </summary>
	public const int ERROR_NOT_SAME_DEVICE = 17;

	/// <summary>
	///     There are no more files.
	/// </summary>
	public const int ERROR_NO_MORE_FILES = 18;

	/// <summary>
	///     The media is write protected.
	/// </summary>
	public const int ERROR_WRITE_PROTECT = 19;

	/// <summary>
	///     The system cannot find the device specified.
	/// </summary>
	public const int ERROR_BAD_UNIT = 20;

	/// <summary>
	///     The device is not ready.
	/// </summary>
	public const int ERROR_NOT_READY = 21;

	/// <summary>
	///     The device does not recognize the command.
	/// </summary>
	public const int ERROR_BAD_COMMAND = 22;

	/// <summary>
	///     Data error (cyclic redundancy check).
	/// </summary>
	public const int ERROR_CRC = 23;

	/// <summary>
	///     The program issued a command but the command length is incorrect.
	/// </summary>
	public const int ERROR_BAD_LENGTH = 24;

	/// <summary>
	///     The drive cannot locate a specific area or track on the disk.
	/// </summary>
	public const int ERROR_SEEK = 25;

	/// <summary>
	///     The specified disk or diskette cannot be accessed.
	/// </summary>
	public const int ERROR_NOT_DOS_DISK = 26;

	/// <summary>
	///     The drive cannot find the sector requested.
	/// </summary>
	public const int ERROR_SECTOR_NOT_FOUND = 27;

	/// <summary>
	///     The printer is out of paper.
	/// </summary>
	public const int ERROR_OUT_OF_PAPER = 28;

	/// <summary>
	///     The system cannot write to the specified device.
	/// </summary>
	public const int ERROR_WRITE_FAULT = 29;

	/// <summary>
	///     The system cannot read from the specified device.
	/// </summary>
	public const int ERROR_READ_FAULT = 30;

	/// <summary>
	///     A device attached to the system is not functioning.
	/// </summary>
	public const int ERROR_GEN_FAILURE = 31;

	/// <summary>
	///     The process cannot access the file because it is being used by another process.
	/// </summary>
	public const int ERROR_SHARING_VIOLATION = 32;

	/// <summary>
	///     The process cannot access the file because another process has locked a portion of the file.
	/// </summary>
	public const int ERROR_LOCK_VIOLATION = 33;

	/// <summary>
	///     The wrong diskette is in the drive.
	///     Insert %2 (Volume Serial Number: %3) into drive %1.
	/// </summary>
	public const int ERROR_WRONG_DISK = 34;

	/// <summary>
	///     Too many files opened for sharing.
	/// </summary>
	public const int ERROR_SHARING_BUFFER_EXCEEDED = 36;

	/// <summary>
	///     Reached the end of the file.
	/// </summary>
	public const int ERROR_HANDLE_EOF = 38;

	/// <summary>
	///     The disk is full.
	/// </summary>
	public const int ERROR_HANDLE_DISK_FULL = 39;

	/// <summary>
	///     The request is not supported.
	/// </summary>
	public const int ERROR_NOT_SUPPORTED = 50;

	/// <summary>
	///     Windows cannot find the network path. Verify that the network path is correct and the destination computer is not
	///     busy or turned off. If Windows still cannot find the network path, contact your network administrator.
	/// </summary>
	public const int ERROR_REM_NOT_LIST = 51;

	/// <summary>
	///     You were not connected because a duplicate name exists on the network. Go to System in Control Panel to change the
	///     computer name and try again.
	/// </summary>
	public const int ERROR_DUP_NAME = 52;

	/// <summary>
	///     The network path was not found.
	/// </summary>
	public const int ERROR_BAD_NETPATH = 53;

	/// <summary>
	///     The network is busy.
	/// </summary>
	public const int ERROR_NETWORK_BUSY = 54;

	/// <summary>
	///     The specified network resource or device is no longer available.
	/// </summary>
	public const int ERROR_DEV_NOT_EXIST = 55;

	/// <summary>
	///     The network BIOS command limit has been reached.
	/// </summary>
	public const int ERROR_TOO_MANY_CMDS = 56;

	/// <summary>
	///     A network adapter hardware error occurred.
	/// </summary>
	public const int ERROR_ADAP_HDW_ERR = 57;

	/// <summary>
	///     The specified server cannot perform the requested operation.
	/// </summary>
	public const int ERROR_BAD_NET_RESP = 58;

	/// <summary>
	///     An unexpected network error occurred.
	/// </summary>
	public const int ERROR_UNEXP_NET_ERR = 59;

	/// <summary>
	///     The remote adapter is not compatible.
	/// </summary>
	public const int ERROR_BAD_REM_ADAP = 60;

	/// <summary>
	///     The printer queue is full.
	/// </summary>
	public const int ERROR_PRINTQ_FULL = 61;

	/// <summary>
	///     Space to store the file waiting to be printed is not available on the server.
	/// </summary>
	public const int ERROR_NO_SPOOL_SPACE = 62;

	/// <summary>
	///     Your file waiting to be printed was deleted.
	/// </summary>
	public const int ERROR_PRINT_CANCELLED = 63;

	/// <summary>
	///     The specified network name is no longer available.
	/// </summary>
	public const int ERROR_NETNAME_DELETED = 64;

	/// <summary>
	///     Network access is denied.
	/// </summary>
	public const int ERROR_NETWORK_ACCESS_DENIED = 65;

	/// <summary>
	///     The network resource type is not correct.
	/// </summary>
	public const int ERROR_BAD_DEV_TYPE = 66;

	/// <summary>
	///     The network name cannot be found.
	/// </summary>
	public const int ERROR_BAD_NET_NAME = 67;

	/// <summary>
	///     The name limit for the local computer network adapter card was exceeded.
	/// </summary>
	public const int ERROR_TOO_MANY_NAMES = 68;

	/// <summary>
	///     The network BIOS session limit was exceeded.
	/// </summary>
	public const int ERROR_TOO_MANY_SESS = 69;

	/// <summary>
	///     The remote server has been paused or is in the process of being started.
	/// </summary>
	public const int ERROR_SHARING_PAUSED = 70;

	/// <summary>
	///     No more connections can be made to this remote computer at this time because there are already as many connections
	///     as the computer can accept.
	/// </summary>
	public const int ERROR_REQ_NOT_ACCEP = 71;

	/// <summary>
	///     The specified printer or disk device has been paused.
	/// </summary>
	public const int ERROR_REDIR_PAUSED = 72;

	/// <summary>
	///     The file exists.
	/// </summary>
	public const int ERROR_FILE_EXISTS = 80;

	/// <summary>
	///     The directory or file cannot be created.
	/// </summary>
	public const int ERROR_CANNOT_MAKE = 82;

	/// <summary>
	///     Fail on INT 24.
	/// </summary>
	public const int ERROR_FAIL_I24 = 83;

	/// <summary>
	///     Storage to process this request is not available.
	/// </summary>
	public const int ERROR_OUT_OF_STRUCTURES = 84;

	/// <summary>
	///     The local device name is already in use.
	/// </summary>
	public const int ERROR_ALREADY_ASSIGNED = 85;

	/// <summary>
	///     The specified network password is not correct.
	/// </summary>
	public const int ERROR_INVALID_PASSWORD = 86;

	/// <summary>
	///     The parameter is incorrect.
	/// </summary>
	public const int ERROR_INVALID_PARAMETER = 87;

	/// <summary>
	///     A write fault occurred on the network.
	/// </summary>
	public const int ERROR_NET_WRITE_FAULT = 88;

	/// <summary>
	///     The system cannot start another process at this time.
	/// </summary>
	public const int ERROR_NO_PROC_SLOTS = 89;

	/// <summary>
	///     Cannot create another system semaphore.
	/// </summary>
	public const int ERROR_TOO_MANY_SEMAPHORES = 100;

	/// <summary>
	///     The exclusive semaphore is owned by another process.
	/// </summary>
	public const int ERROR_EXCL_SEM_ALREADY_OWNED = 101;

	/// <summary>
	///     The semaphore is set and cannot be closed.
	/// </summary>
	public const int ERROR_SEM_IS_SET = 102;

	/// <summary>
	///     The semaphore cannot be set again.
	/// </summary>
	public const int ERROR_TOO_MANY_SEM_REQUESTS = 103;

	/// <summary>
	///     Cannot request exclusive semaphores at interrupt time.
	/// </summary>
	public const int ERROR_INVALID_AT_INTERRUPT_TIME = 104;

	/// <summary>
	///     The previous ownership of this semaphore has ended.
	/// </summary>
	public const int ERROR_SEM_OWNER_DIED = 105;

	/// <summary>
	///     Insert the diskette for drive %1.
	/// </summary>
	public const int ERROR_SEM_USER_LIMIT = 106;

	/// <summary>
	///     The program stopped because an alternate diskette was not inserted.
	/// </summary>
	public const int ERROR_DISK_CHANGE = 107;

	/// <summary>
	///     The disk is in use or locked by another process.
	/// </summary>
	public const int ERROR_DRIVE_LOCKED = 108;

	/// <summary>
	///     The pipe has been ended.
	/// </summary>
	public const int ERROR_BROKEN_PIPE = 109;

	/// <summary>
	///     The system cannot open the device or file specified.
	/// </summary>
	public const int ERROR_OPEN_FAILED = 110;

	/// <summary>
	///     The file name is too long.
	/// </summary>
	public const int ERROR_BUFFER_OVERFLOW = 111;

	/// <summary>
	///     There is not enough space on the disk.
	/// </summary>
	public const int ERROR_DISK_FULL = 112;

	/// <summary>
	///     No more internal file identifiers available.
	/// </summary>
	public const int ERROR_NO_MORE_SEARCH_HANDLES = 113;

	/// <summary>
	///     The target internal file identifier is incorrect.
	/// </summary>
	public const int ERROR_INVALID_TARGET_HANDLE = 114;

	/// <summary>
	///     The IOCTL call made by the application program is not correct.
	/// </summary>
	public const int ERROR_INVALID_CATEGORY = 117;

	/// <summary>
	///     The verify-on-write switch parameter value is not correct.
	/// </summary>
	public const int ERROR_INVALID_VERIFY_SWITCH = 118;

	/// <summary>
	///     The system does not support the command requested.
	/// </summary>
	public const int ERROR_BAD_DRIVER_LEVEL = 119;

	/// <summary>
	///     This function is not supported on this system.
	/// </summary>
	public const int ERROR_CALL_NOT_IMPLEMENTED = 120;

	/// <summary>
	///     The semaphore timeout period has expired.
	/// </summary>
	public const int ERROR_SEM_TIMEOUT = 121;

	/// <summary>
	///     The data area passed to a system call is too small.
	/// </summary>
	public const int ERROR_INSUFFICIENT_BUFFER = 122;

	/// <summary>
	///     The filename, directory name, or volume label syntax is incorrect.
	/// </summary>
	public const int ERROR_INVALID_NAME = 123;

	/// <summary>
	///     The system call level is not correct.
	/// </summary>
	public const int ERROR_INVALID_LEVEL = 124;

	/// <summary>
	///     The disk has no volume label.
	/// </summary>
	public const int ERROR_NO_VOLUME_LABEL = 125;

	/// <summary>
	///     The specified module could not be found.
	/// </summary>
	public const int ERROR_MOD_NOT_FOUND = 126;

	/// <summary>
	///     The specified procedure could not be found.
	/// </summary>
	public const int ERROR_PROC_NOT_FOUND = 127;

	/// <summary>
	///     There are no child processes to wait for.
	/// </summary>
	public const int ERROR_WAIT_NO_CHILDREN = 128;

	/// <summary>
	///     The %1 application cannot be run in Win32 mode.
	/// </summary>
	public const int ERROR_CHILD_NOT_COMPLETE = 129;

	/// <summary>
	///     Attempt to use a file handle to an open disk partition for an operation other than raw disk I/O.
	/// </summary>
	public const int ERROR_DIRECT_ACCESS_HANDLE = 130;

	/// <summary>
	///     An attempt was made to move the file pointer before the beginning of the file.
	/// </summary>
	public const int ERROR_NEGATIVE_SEEK = 131;

	/// <summary>
	///     The file pointer cannot be set on the specified device or file.
	/// </summary>
	public const int ERROR_SEEK_ON_DEVICE = 132;

	/// <summary>
	///     A JOIN or SUBST command cannot be used for a drive that contains previously joined drives.
	/// </summary>
	public const int ERROR_IS_JOIN_TARGET = 133;

	/// <summary>
	///     An attempt was made to use a JOIN or SUBST command on a drive that has already been joined.
	/// </summary>
	public const int ERROR_IS_JOINED = 134;

	/// <summary>
	///     An attempt was made to use a JOIN or SUBST command on a drive that has already been substituted.
	/// </summary>
	public const int ERROR_IS_SUBSTED = 135;

	/// <summary>
	///     The system tried to delete the JOIN of a drive that is not joined.
	/// </summary>
	public const int ERROR_NOT_JOINED = 136;

	/// <summary>
	///     The system tried to delete the substitution of a drive that is not substituted.
	/// </summary>
	public const int ERROR_NOT_SUBSTED = 137;

	/// <summary>
	///     The system tried to join a drive to a directory on a joined drive.
	/// </summary>
	public const int ERROR_JOIN_TO_JOIN = 138;

	/// <summary>
	///     The system tried to substitute a drive to a directory on a substituted drive.
	/// </summary>
	public const int ERROR_SUBST_TO_SUBST = 139;

	/// <summary>
	///     The system tried to join a drive to a directory on a substituted drive.
	/// </summary>
	public const int ERROR_JOIN_TO_SUBST = 140;

	/// <summary>
	///     The system tried to SUBST a drive to a directory on a joined drive.
	/// </summary>
	public const int ERROR_SUBST_TO_JOIN = 141;

	/// <summary>
	///     The system cannot perform a JOIN or SUBST at this time.
	/// </summary>
	public const int ERROR_BUSY_DRIVE = 142;

	/// <summary>
	///     The system cannot join or substitute a drive to or for a directory on the same drive.
	/// </summary>
	public const int ERROR_SAME_DRIVE = 143;

	/// <summary>
	///     The directory is not a subdirectory of the root directory.
	/// </summary>
	public const int ERROR_DIR_NOT_ROOT = 144;

	/// <summary>
	///     The directory is not empty.
	/// </summary>
	public const int ERROR_DIR_NOT_EMPTY = 145;

	/// <summary>
	///     The path specified is being used in a substitute.
	/// </summary>
	public const int ERROR_IS_SUBST_PATH = 146;

	/// <summary>
	///     Not enough resources are available to process this command.
	/// </summary>
	public const int ERROR_IS_JOIN_PATH = 147;

	/// <summary>
	///     The path specified cannot be used at this time.
	/// </summary>
	public const int ERROR_PATH_BUSY = 148;

	/// <summary>
	///     An attempt was made to join or substitute a drive for which a directory on the drive is the target of a previous
	///     substitute.
	/// </summary>
	public const int ERROR_IS_SUBST_TARGET = 149;

	/// <summary>
	///     System trace information was not specified in your CONFIG.SYS file, or tracing is disallowed.
	/// </summary>
	public const int ERROR_SYSTEM_TRACE = 150;

	/// <summary>
	///     The number of specified semaphore events for DosMuxSemWait is not correct.
	/// </summary>
	public const int ERROR_INVALID_EVENT_COUNT = 151;

	/// <summary>
	///     DosMuxSemWait did not execute; too many semaphores are already set.
	/// </summary>
	public const int ERROR_TOO_MANY_MUXWAITERS = 152;

	/// <summary>
	///     The DosMuxSemWait list is not correct.
	/// </summary>
	public const int ERROR_INVALID_LIST_FORMAT = 153;

	/// <summary>
	///     The volume label you entered exceeds the label character limit of the target file system.
	/// </summary>
	public const int ERROR_LABEL_TOO_Int32 = 154;

	/// <summary>
	///     Cannot create another thread.
	/// </summary>
	public const int ERROR_TOO_MANY_TCBS = 155;

	/// <summary>
	///     The recipient process has refused the signal.
	/// </summary>
	public const int ERROR_SIGNAL_REFUSED = 156;

	/// <summary>
	///     The segment is already discarded and cannot be locked.
	/// </summary>
	public const int ERROR_DISCARDED = 157;

	/// <summary>
	///     The segment is already unlocked.
	/// </summary>
	public const int ERROR_NOT_LOCKED = 158;

	/// <summary>
	///     The address for the thread ID is not correct.
	/// </summary>
	public const int ERROR_BAD_THREADID_ADDR = 159;

	/// <summary>
	///     One or more arguments are not correct.
	/// </summary>
	public const int ERROR_BAD_ARGUMENTS = 160;

	/// <summary>
	///     The specified path is invalid.
	/// </summary>
	public const int ERROR_BAD_PATHNAME = 161;

	/// <summary>
	///     A signal is already pending.
	/// </summary>
	public const int ERROR_SIGNAL_PENDING = 162;

	/// <summary>
	///     No more threads can be created in the system.
	/// </summary>
	public const int ERROR_MAX_THRDS_REACHED = 164;

	/// <summary>
	///     Unable to lock a region of a file.
	/// </summary>
	public const int ERROR_LOCK_FAILED = 167;

	/// <summary>
	///     The requested resource is in use.
	/// </summary>
	public const int ERROR_BUSY = 170;

	/// <summary>
	///     A lock request was not outstanding for the supplied cancel region.
	/// </summary>
	public const int ERROR_CANCEL_VIOLATION = 173;

	/// <summary>
	///     The file system does not support atomic changes to the lock type.
	/// </summary>
	public const int ERROR_ATOMIC_LOCKS_NOT_SUPPORTED = 174;

	/// <summary>
	///     The system detected a segment number that was not correct.
	/// </summary>
	public const int ERROR_INVALID_SEGMENT_NUMBER = 180;

	/// <summary>
	///     The operating system cannot run %1.
	/// </summary>
	public const int ERROR_INVALID_ORDINAL = 182;

	/// <summary>
	///     Cannot create a file when that file already exists.
	/// </summary>
	public const int ERROR_ALREADY_EXISTS = 183;

	/// <summary>
	///     The flag passed is not correct.
	/// </summary>
	public const int ERROR_INVALID_FLAG_NUMBER = 186;

	/// <summary>
	///     The specified system semaphore name was not found.
	/// </summary>
	public const int ERROR_SEM_NOT_FOUND = 187;

	/// <summary>
	///     The operating system cannot run %1.
	/// </summary>
	public const int ERROR_INVALID_STARTING_CODESEG = 188;

	/// <summary>
	///     The operating system cannot run %1.
	/// </summary>
	public const int ERROR_INVALID_STACKSEG = 189;

	/// <summary>
	///     The operating system cannot run %1.
	/// </summary>
	public const int ERROR_INVALID_MODULETYPE = 190;

	/// <summary>
	///     Cannot run %1 in Win32 mode.
	/// </summary>
	public const int ERROR_INVALID_EXE_SIGNATURE = 191;

	/// <summary>
	///     The operating system cannot run %1.
	/// </summary>
	public const int ERROR_EXE_MARKED_INVALID = 192;

	/// <summary>
	///     %1 is not a valid Win32 application.
	/// </summary>
	public const int ERROR_BAD_EXE_FORMAT = 193;

	/// <summary>
	///     The operating system cannot run %1.
	/// </summary>
	public const int ERROR_ITERATED_DATA_EXCEEDS_64k = 194;

	/// <summary>
	///     The operating system cannot run %1.
	/// </summary>
	public const int ERROR_INVALID_MINALLOCSIZE = 195;

	/// <summary>
	///     The operating system cannot run this application program.
	/// </summary>
	public const int ERROR_DYNLINK_FROM_INVALID_RING = 196;

	/// <summary>
	///     The operating system is not presently configured to run this application.
	/// </summary>
	public const int ERROR_IOPL_NOT_ENABLED = 197;

	/// <summary>
	///     The operating system cannot run %1.
	/// </summary>
	public const int ERROR_INVALID_SEGDPL = 198;

	/// <summary>
	///     The operating system cannot run this application program.
	/// </summary>
	public const int ERROR_AUTODATASEG_EXCEEDS_64k = 199;

	/// <summary>
	///     The code segment cannot be greater than or equal to 64K.
	/// </summary>
	public const int ERROR_RING2SEG_MUST_BE_MOVABLE = 200;

	/// <summary>
	///     The operating system cannot run %1.
	/// </summary>
	public const int ERROR_RELOC_CHAIN_XEEDS_SEGLIM = 201;

	/// <summary>
	///     The operating system cannot run %1.
	/// </summary>
	public const int ERROR_INFLOOP_IN_RELOC_CHAIN = 202;

	/// <summary>
	///     The system could not find the environment option that was entered.
	/// </summary>
	public const int ERROR_ENVVAR_NOT_FOUND = 203;

	/// <summary>
	///     No process in the command subtree has a signal handler.
	/// </summary>
	public const int ERROR_NO_SIGNAL_SENT = 205;

	/// <summary>
	///     The filename or extension is too long.
	/// </summary>
	public const int ERROR_FILENAME_EXCED_RANGE = 206;

	/// <summary>
	///     The ring 2 stack is in use.
	/// </summary>
	public const int ERROR_RING2_STACK_IN_USE = 207;

	/// <summary>
	///     The global filename characters, * or ?, are entered incorrectly or too many global filename characters are
	///     specified.
	/// </summary>
	public const int ERROR_META_EXPANSION_TOO_Int32 = 208;

	/// <summary>
	///     The signal being posted is not correct.
	/// </summary>
	public const int ERROR_INVALID_SIGNAL_NUMBER = 209;

	/// <summary>
	///     The signal handler cannot be set.
	/// </summary>
	public const int ERROR_THREAD_1_INACTIVE = 210;

	/// <summary>
	///     The segment is locked and cannot be reallocated.
	/// </summary>
	public const int ERROR_LOCKED = 212;

	/// <summary>
	///     Too many dynamic-link modules are attached to this program or dynamic-link module.
	/// </summary>
	public const int ERROR_TOO_MANY_MODULES = 214;

	/// <summary>
	///     Cannot nest calls to LoadModule.
	/// </summary>
	public const int ERROR_NESTING_NOT_ALLOWED = 215;

	/// <summary>
	///     The image file %1 is valid, but is for a machine type other than the current machine.
	/// </summary>
	public const int ERROR_EXE_MACHINE_TYPE_MISMATCH = 216;

	/// <summary>
	///     No information avialable.
	/// </summary>
	public const int ERROR_EXE_CANNOT_MODIFY_SIGNED_BINARY = 217;

	/// <summary>
	///     No information avialable.
	/// </summary>
	public const int ERROR_EXE_CANNOT_MODIFY_STRONG_SIGNED_BINARY = 218;

	/// <summary>
	///     The pipe state is invalid.
	/// </summary>
	public const int ERROR_BAD_PIPE = 230;

	/// <summary>
	///     All pipe instances are busy.
	/// </summary>
	public const int ERROR_PIPE_BUSY = 231;

	/// <summary>
	///     The pipe is being closed.
	/// </summary>
	public const int ERROR_NO_DATA = 232;

	/// <summary>
	///     No process is on the other end of the pipe.
	/// </summary>
	public const int ERROR_PIPE_NOT_CONNECTED = 233;

	/// <summary>
	///     More data is available.
	/// </summary>
	public const int ERROR_MORE_DATA = 234;

	/// <summary>
	///     The session was canceled.
	/// </summary>
	public const int ERROR_VC_DISCONNECTED = 240;

	/// <summary>
	///     The specified extended attribute name was invalid.
	/// </summary>
	public const int ERROR_INVALID_EA_NAME = 254;

	/// <summary>
	///     The extended attributes are inconsistent.
	/// </summary>
	public const int ERROR_EA_LIST_INCONSISTENT = 255;

	public const int WAIT_ABANDONED = 0x80;
	public const int WAIT_OBJECT_0 = 0x00;

	/// <summary>
	///     The wait operation timed out.
	/// </summary>
	public const int WAIT_TIMEOUT = 0x102;

	public const int WAIT_FAILED = -1;

	/// <summary>
	///     No more data is available.
	/// </summary>
	public const int ERROR_NO_MORE_ITEMS = 259;

	/// <summary>
	///     The copy functions cannot be used.
	/// </summary>
	public const int ERROR_CANNOT_COPY = 266;

	/// <summary>
	///     The directory name is invalid.
	/// </summary>
	public const int ERROR_DIRECTORY = 267;

	/// <summary>
	///     The extended attributes did not fit in the buffer.
	/// </summary>
	public const int ERROR_EAS_DIDNT_FIT = 275;

	/// <summary>
	///     The extended attribute file on the mounted file system is corrupt.
	/// </summary>
	public const int ERROR_EA_FILE_CORRUPT = 276;

	/// <summary>
	///     The extended attribute table file is full.
	/// </summary>
	public const int ERROR_EA_TABLE_FULL = 277;

	/// <summary>
	///     The specified extended attribute handle is invalid.
	/// </summary>
	public const int ERROR_INVALID_EA_HANDLE = 278;

	/// <summary>
	///     The mounted file system does not support extended attributes.
	/// </summary>
	public const int ERROR_EAS_NOT_SUPPORTED = 282;

	/// <summary>
	///     Attempt to release mutex not owned by caller.
	/// </summary>
	public const int ERROR_NOT_OWNER = 288;

	/// <summary>
	///     Too many posts were made to a semaphore.
	/// </summary>
	public const int ERROR_TOO_MANY_POSTS = 298;

	/// <summary>
	///     Only part of a ReadProcessMemory or WriteProcessMemory request was completed.
	/// </summary>
	public const int ERROR_PARTIAL_COPY = 299;

	/// <summary>
	///     The oplock request is denied.
	/// </summary>
	public const int ERROR_OPLOCK_NOT_GRANTED = 300;

	/// <summary>
	///     An invalid oplock acknowledgment was received by the system.
	/// </summary>
	public const int ERROR_INVALID_OPLOCK_PROTOCOL = 301;

	/// <summary>
	///     The volume is too fragmented to complete this operation.
	/// </summary>
	public const int ERROR_DISK_TOO_FRAGMENTED = 302;

	/// <summary>
	///     The file cannot be opened because it is in the process of being deleted.
	/// </summary>
	public const int ERROR_DELETE_PENDING = 303;

	/// <summary>
	///     The system cannot find message text for message number 0x%1 in the message file for %2.
	/// </summary>
	public const int ERROR_MR_MID_NOT_FOUND = 317;

	/// <summary>
	///     No information avialable.
	/// </summary>
	public const int ERROR_SCOPE_NOT_FOUND = 318;

	/// <summary>
	///     Attempt to access invalid address.
	/// </summary>
	public const int ERROR_INVALID_ADDRESS = 487;

	/// <summary>
	///     Arithmetic result exceeded 32 bits.
	/// </summary>
	public const int ERROR_ARITHMETIC_OVERFLOW = 534;

	/// <summary>
	///     There is a process on other end of the pipe.
	/// </summary>
	public const int ERROR_PIPE_CONNECTED = 535;

	/// <summary>
	///     Waiting for a process to open the other end of the pipe.
	/// </summary>
	public const int ERROR_PIPE_LISTENING = 536;

	/// <summary>
	///     Access to the extended attribute was denied.
	/// </summary>
	public const int ERROR_EA_ACCESS_DENIED = 994;

	/// <summary>
	///     The I/O operation has been aborted because of either a thread exit or an application request.
	/// </summary>
	public const int ERROR_OPERATION_ABORTED = 995;

	/// <summary>
	///     Overlapped I/O event is not in a signaled state.
	/// </summary>
	public const int ERROR_IO_INCOMPLETE = 996;

	/// <summary>
	///     Overlapped I/O operation is in progress.
	/// </summary>
	public const int ERROR_IO_PENDING = 997;

	/// <summary>
	///     Invalid access to memory location.
	/// </summary>
	public const int ERROR_NOACCESS = 998;

	/// <summary>
	///     Error performing inpage operation.
	/// </summary>
	public const int ERROR_SWAPERROR = 999;

	/// <summary>
	///     Recursion too deep; the stack overflowed.
	/// </summary>
	public const int ERROR_STACK_OVERFLOW = 1001;

	/// <summary>
	///     The window cannot act on the sent message.
	/// </summary>
	public const int ERROR_INVALID_MESSAGE = 1002;

	/// <summary>
	///     Cannot complete this function.
	/// </summary>
	public const int ERROR_CAN_NOT_COMPLETE = 1003;

	/// <summary>
	///     Invalid flags.
	/// </summary>
	public const int ERROR_INVALID_FLAGS = 1004;

	/// <summary>
	///     The volume does not contain a recognized file system.
	///     Please make sure that all required file system drivers are loaded and that the volume is not corrupted.
	/// </summary>
	public const int ERROR_UNRECOGNIZED_VOLUME = 1005;

	/// <summary>
	///     The volume for a file has been externally altered so that the opened file is no longer valid.
	/// </summary>
	public const int ERROR_FILE_INVALID = 1006;

	/// <summary>
	///     The requested operation cannot be performed in full-screen mode.
	/// </summary>
	public const int ERROR_FULLSCREEN_MODE = 1007;

	/// <summary>
	///     An attempt was made to reference a token that does not exist.
	/// </summary>
	public const int ERROR_NO_TOKEN = 1008;

	/// <summary>
	///     The configuration registry database is corrupt.
	/// </summary>
	public const int ERROR_BADDB = 1009;

	/// <summary>
	///     The configuration registry key is invalid.
	/// </summary>
	public const int ERROR_BADKEY = 1010;

	/// <summary>
	///     The configuration registry key could not be opened.
	/// </summary>
	public const int ERROR_CANTOPEN = 1011;

	/// <summary>
	///     The configuration registry key could not be read.
	/// </summary>
	public const int ERROR_CANTREAD = 1012;

	/// <summary>
	///     The configuration registry key could not be written.
	/// </summary>
	public const int ERROR_CANTWRITE = 1013;

	/// <summary>
	///     One of the files in the registry database had to be recovered by use of a log or alternate copy. The recovery was
	///     successful.
	/// </summary>
	public const int ERROR_REGISTRY_RECOVERED = 1014;

	/// <summary>
	///     The registry is corrupted. The structure of one of the files containing registry data is corrupted, or the system's
	///     memory image of the file is corrupted, or the file could not be recovered because the alternate copy or log was
	///     absent or corrupted.
	/// </summary>
	public const int ERROR_REGISTRY_CORRUPT = 1015;

	/// <summary>
	///     An I/O operation initiated by the registry failed unrecoverably. The registry could not read in, or write out, or
	///     flush, one of the files that contain the system's image of the registry.
	/// </summary>
	public const int ERROR_REGISTRY_IO_FAILED = 1016;

	/// <summary>
	///     The system has attempted to load or restore a file into the registry, but the specified file is not in a registry
	///     file format.
	/// </summary>
	public const int ERROR_NOT_REGISTRY_FILE = 1017;

	/// <summary>
	///     Illegal operation attempted on a registry key that has been marked for deletion.
	/// </summary>
	public const int ERROR_KEY_DELETED = 1018;

	/// <summary>
	///     System could not allocate the required space in a registry log.
	/// </summary>
	public const int ERROR_NO_LOG_SPACE = 1019;

	/// <summary>
	///     Cannot create a symbolic link in a registry key that already has subkeys or values.
	/// </summary>
	public const int ERROR_KEY_HAS_CHILDREN = 1020;

	/// <summary>
	///     Cannot create a stable subkey under a volatile parent key.
	/// </summary>
	public const int ERROR_CHILD_MUST_BE_VOLATILE = 1021;

	/// <summary>
	///     A notify change request is being completed and the information is not being returned in the caller's buffer. The
	///     caller now needs to enumerate the files to find the changes.
	/// </summary>
	public const int ERROR_NOTIFY_ENUM_DIR = 1022;

	/// <summary>
	///     A stop control has been sent to a service that other running services are dependent on.
	/// </summary>
	public const int ERROR_DEPENDENT_SERVICES_RUNNING = 1051;

	/// <summary>
	///     The requested control is not valid for this service.
	/// </summary>
	public const int ERROR_INVALID_SERVICE_CONTROL = 1052;

	/// <summary>
	///     The service did not respond to the start or control request in a timely fashion.
	/// </summary>
	public const int ERROR_SERVICE_REQUEST_TIMEOUT = 1053;

	/// <summary>
	///     A thread could not be created for the service.
	/// </summary>
	public const int ERROR_SERVICE_NO_THREAD = 1054;

	/// <summary>
	///     The service database is locked.
	/// </summary>
	public const int ERROR_SERVICE_DATABASE_LOCKED = 1055;

	/// <summary>
	///     An instance of the service is already running.
	/// </summary>
	public const int ERROR_SERVICE_ALREADY_RUNNING = 1056;

	/// <summary>
	///     The account name is invalid or does not exist, or the password is invalid for the account name specified.
	/// </summary>
	public const int ERROR_INVALID_SERVICE_ACCOUNT = 1057;

	/// <summary>
	///     The service cannot be started, either because it is disabled or because it has no enabled devices associated with
	///     it.
	/// </summary>
	public const int ERROR_SERVICE_DISABLED = 1058;

	/// <summary>
	///     Circular service dependency was specified.
	/// </summary>
	public const int ERROR_CIRCULAR_DEPENDENCY = 1059;

	/// <summary>
	///     The specified service does not exist as an installed service.
	/// </summary>
	public const int ERROR_SERVICE_DOES_NOT_EXIST = 1060;

	/// <summary>
	///     The service cannot accept control messages at this time.
	/// </summary>
	public const int ERROR_SERVICE_CANNOT_ACCEPT_CTRL = 1061;

	/// <summary>
	///     The service has not been started.
	/// </summary>
	public const int ERROR_SERVICE_NOT_ACTIVE = 1062;

	/// <summary>
	///     The service process could not connect to the service controller.
	/// </summary>
	public const int ERROR_FAILED_SERVICE_CONTROLLER_CONNECT = 1063;

	/// <summary>
	///     An exception occurred in the service when handling the control request.
	/// </summary>
	public const int ERROR_EXCEPTION_IN_SERVICE = 1064;

	/// <summary>
	///     The database specified does not exist.
	/// </summary>
	public const int ERROR_DATABASE_DOES_NOT_EXIST = 1065;

	/// <summary>
	///     The service has returned a service-specific error code.
	/// </summary>
	public const int ERROR_SERVICE_SPECIFIC_ERROR = 1066;

	/// <summary>
	///     The process terminated unexpectedly.
	/// </summary>
	public const int ERROR_PROCESS_ABORTED = 1067;

	/// <summary>
	///     The dependency service or group failed to start.
	/// </summary>
	public const int ERROR_SERVICE_DEPENDENCY_FAIL = 1068;

	/// <summary>
	///     The service did not start due to a logon failure.
	/// </summary>
	public const int ERROR_SERVICE_LOGON_FAILED = 1069;

	/// <summary>
	///     After starting, the service hung in a start-pending state.
	/// </summary>
	public const int ERROR_SERVICE_START_HANG = 1070;

	/// <summary>
	///     The specified service database lock is invalid.
	/// </summary>
	public const int ERROR_INVALID_SERVICE_LOCK = 1071;

	/// <summary>
	///     The specified service has been marked for deletion.
	/// </summary>
	public const int ERROR_SERVICE_MARKED_FOR_DELETE = 1072;

	/// <summary>
	///     The specified service already exists.
	/// </summary>
	public const int ERROR_SERVICE_EXISTS = 1073;

	/// <summary>
	///     The system is currently running with the last-known-good configuration.
	/// </summary>
	public const int ERROR_ALREADY_RUNNING_LKG = 1074;

	/// <summary>
	///     The dependency service does not exist or has been marked for deletion.
	/// </summary>
	public const int ERROR_SERVICE_DEPENDENCY_DELETED = 1075;

	/// <summary>
	///     The current boot has already been accepted for use as the last-known-good control set.
	/// </summary>
	public const int ERROR_BOOT_ALREADY_ACCEPTED = 1076;

	/// <summary>
	///     No attempts to start the service have been made since the last boot.
	/// </summary>
	public const int ERROR_SERVICE_NEVER_STARTED = 1077;

	/// <summary>
	///     The name is already in use as either a service name or a service display name.
	/// </summary>
	public const int ERROR_DUPLICATE_SERVICE_NAME = 1078;

	/// <summary>
	///     The account specified for this service is different from the account specified for other services running in the
	///     same process.
	/// </summary>
	public const int ERROR_DIFFERENT_SERVICE_ACCOUNT = 1079;

	/// <summary>
	///     Failure actions can only be set for Win32 services, not for drivers.
	/// </summary>
	public const int ERROR_CANNOT_DETECT_DRIVER_FAILURE = 1080;

	/// <summary>
	///     This service runs in the same process as the service control manager.
	///     Therefore, the service control manager cannot take action if this service's process terminates unexpectedly.
	/// </summary>
	public const int ERROR_CANNOT_DETECT_PROCESS_ABORT = 1081;

	/// <summary>
	///     No recovery program has been configured for this service.
	/// </summary>
	public const int ERROR_NO_RECOVERY_PROGRAM = 1082;

	/// <summary>
	///     The executable program that this service is configured to run in does not implement the service.
	/// </summary>
	public const int ERROR_SERVICE_NOT_IN_EXE = 1083;

	/// <summary>
	///     This service cannot be started in Safe Mode
	/// </summary>
	public const int ERROR_NOT_SAFEBOOT_SERVICE = 1084;

	/// <summary>
	///     The physical end of the tape has been reached.
	/// </summary>
	public const int ERROR_END_OF_MEDIA = 1100;

	/// <summary>
	///     A tape access reached a filemark.
	/// </summary>
	public const int ERROR_FILEMARK_DETECTED = 1101;

	/// <summary>
	///     The beginning of the tape or a partition was encountered.
	/// </summary>
	public const int ERROR_BEGINNING_OF_MEDIA = 1102;

	/// <summary>
	///     A tape access reached the end of a set of files.
	/// </summary>
	public const int ERROR_SETMARK_DETECTED = 1103;

	/// <summary>
	///     No more data is on the tape.
	/// </summary>
	public const int ERROR_NO_DATA_DETECTED = 1104;

	/// <summary>
	///     Tape could not be partitioned.
	/// </summary>
	public const int ERROR_PARTITION_FAILURE = 1105;

	/// <summary>
	///     When accessing a new tape of a multivolume partition, the current block size is incorrect.
	/// </summary>
	public const int ERROR_INVALID_BLOCK_LENGTH = 1106;

	/// <summary>
	///     Tape partition information could not be found when loading a tape.
	/// </summary>
	public const int ERROR_DEVICE_NOT_PARTITIONED = 1107;

	/// <summary>
	///     Unable to lock the media eject mechanism.
	/// </summary>
	public const int ERROR_UNABLE_TO_LOCK_MEDIA = 1108;

	/// <summary>
	///     Unable to unload the media.
	/// </summary>
	public const int ERROR_UNABLE_TO_UNLOAD_MEDIA = 1109;

	/// <summary>
	///     The media in the drive may have changed.
	/// </summary>
	public const int ERROR_MEDIA_CHANGED = 1110;

	/// <summary>
	///     The I/O bus was reset.
	/// </summary>
	public const int ERROR_BUS_RESET = 1111;

	/// <summary>
	///     No media in drive.
	/// </summary>
	public const int ERROR_NO_MEDIA_IN_DRIVE = 1112;

	/// <summary>
	///     No mapping for the Unicode character exists in the target multi-byte code page.
	/// </summary>
	public const int ERROR_NO_UNICODE_TRANSLATION = 1113;

	/// <summary>
	///     A dynamic link library (DLL) initialization routine failed.
	/// </summary>
	public const int ERROR_DLL_INIT_FAILED = 1114;

	/// <summary>
	///     A system shutdown is in progress.
	/// </summary>
	public const int ERROR_SHUTDOWN_IN_PROGRESS = 1115;

	/// <summary>
	///     Unable to abort the system shutdown because no shutdown was in progress.
	/// </summary>
	public const int ERROR_NO_SHUTDOWN_IN_PROGRESS = 1116;

	/// <summary>
	///     The request could not be performed because of an I/O device error.
	/// </summary>
	public const int ERROR_IO_DEVICE = 1117;

	/// <summary>
	///     No serial device was successfully initialized. The serial driver will unload.
	/// </summary>
	public const int ERROR_SERIAL_NO_DEVICE = 1118;

	/// <summary>
	///     Unable to open a device that was sharing an interrupt request (IRQ) with other devices. At least one other device
	///     that uses that IRQ was already opened.
	/// </summary>
	public const int ERROR_IRQ_BUSY = 1119;

	/// <summary>
	///     A serial I/O operation was completed by another write to the serial port.
	///     (The IOCTL_SERIAL_XOFF_COUNTER reached zero.)
	/// </summary>
	public const int ERROR_MORE_WRITES = 1120;

	/// <summary>
	///     A serial I/O operation completed because the timeout period expired.
	///     (The IOCTL_SERIAL_XOFF_COUNTER did not reach zero.)
	/// </summary>
	public const int ERROR_COUNTER_TIMEOUT = 1121;

	/// <summary>
	///     No ID address mark was found on the floppy disk.
	/// </summary>
	public const int ERROR_FLOPPY_ID_MARK_NOT_FOUND = 1122;

	/// <summary>
	///     Mismatch between the floppy disk sector ID field and the floppy disk controller track address.
	/// </summary>
	public const int ERROR_FLOPPY_WRONG_CYLINDER = 1123;

	/// <summary>
	///     The floppy disk controller reported an error that is not recognized by the floppy disk driver.
	/// </summary>
	public const int ERROR_FLOPPY_UNKNOWN_ERROR = 1124;

	/// <summary>
	///     The floppy disk controller returned inconsistent results in its registers.
	/// </summary>
	public const int ERROR_FLOPPY_BAD_REGISTERS = 1125;

	/// <summary>
	///     While accessing the hard disk, a recalibrate operation failed, even after retries.
	/// </summary>
	public const int ERROR_DISK_RECALIBRATE_FAILED = 1126;

	/// <summary>
	///     While accessing the hard disk, a disk operation failed even after retries.
	/// </summary>
	public const int ERROR_DISK_OPERATION_FAILED = 1127;

	/// <summary>
	///     While accessing the hard disk, a disk controller reset was needed, but even that failed.
	/// </summary>
	public const int ERROR_DISK_RESET_FAILED = 1128;

	/// <summary>
	///     Physical end of tape encountered.
	/// </summary>
	public const int ERROR_EOM_OVERFLOW = 1129;

	/// <summary>
	///     Not enough server storage is available to process this command.
	/// </summary>
	public const int ERROR_NOT_ENOUGH_SERVER_MEMORY = 1130;

	/// <summary>
	///     A potential deadlock condition has been detected.
	/// </summary>
	public const int ERROR_POSSIBLE_DEADLOCK = 1131;

	/// <summary>
	///     The base address or the file offset specified does not have the proper alignment.
	/// </summary>
	public const int ERROR_MAPPED_ALIGNMENT = 1132;

	/// <summary>
	///     An attempt to change the system power state was vetoed by another application or driver.
	/// </summary>
	public const int ERROR_SET_POWER_STATE_VETOED = 1140;

	/// <summary>
	///     The system BIOS failed an attempt to change the system power state.
	/// </summary>
	public const int ERROR_SET_POWER_STATE_FAILED = 1141;

	/// <summary>
	///     An attempt was made to create more links on a file than the file system supports.
	/// </summary>
	public const int ERROR_TOO_MANY_LINKS = 1142;

	/// <summary>
	///     The specified program requires a newer version of Windows.
	/// </summary>
	public const int ERROR_OLD_WIN_VERSION = 1150;

	/// <summary>
	///     The specified program is not a Windows or MS-DOS program.
	/// </summary>
	public const int ERROR_APP_WRONG_OS = 1151;

	/// <summary>
	///     Cannot start more than one instance of the specified program.
	/// </summary>
	public const int ERROR_SINGLE_INSTANCE_APP = 1152;

	/// <summary>
	///     The specified program was written for an earlier version of Windows.
	/// </summary>
	public const int ERROR_RMODE_APP = 1153;

	/// <summary>
	///     One of the library files needed to run this application is damaged.
	/// </summary>
	public const int ERROR_INVALID_DLL = 1154;

	/// <summary>
	///     No application is associated with the specified file for this operation.
	/// </summary>
	public const int ERROR_NO_ASSOCIATION = 1155;

	/// <summary>
	///     An error occurred in sending the command to the application.
	/// </summary>
	public const int ERROR_DDE_FAIL = 1156;

	/// <summary>
	///     One of the library files needed to run this application cannot be found.
	/// </summary>
	public const int ERROR_DLL_NOT_FOUND = 1157;

	/// <summary>
	///     The current process has used all of its system allowance of handles for Window Manager objects.
	/// </summary>
	public const int ERROR_NO_MORE_USER_HANDLES = 1158;

	/// <summary>
	///     The message can be used only with synchronous operations.
	/// </summary>
	public const int ERROR_MESSAGE_SYNC_ONLY = 1159;

	/// <summary>
	///     The indicated source element has no media.
	/// </summary>
	public const int ERROR_SOURCE_ELEMENT_EMPTY = 1160;

	/// <summary>
	///     The indicated destination element already contains media.
	/// </summary>
	public const int ERROR_DESTINATION_ELEMENT_FULL = 1161;

	/// <summary>
	///     The indicated element does not exist.
	/// </summary>
	public const int ERROR_ILLEGAL_ELEMENT_ADDRESS = 1162;

	/// <summary>
	///     The indicated element is part of a magazine that is not present.
	/// </summary>
	public const int ERROR_MAGAZINE_NOT_PRESENT = 1163;

	/// <summary>
	///     The indicated device requires reinitialization due to hardware errors.
	/// </summary>
	public const int ERROR_DEVICE_REINITIALIZATION_NEEDED = 1164;

	/// <summary>
	///     The device has indicated that cleaning is required before further operations are attempted.
	/// </summary>
	public const int ERROR_DEVICE_REQUIRES_CLEANING = 1165;

	/// <summary>
	///     The device has indicated that its door is open.
	/// </summary>
	public const int ERROR_DEVICE_DOOR_OPEN = 1166;

	/// <summary>
	///     The device is not connected.
	/// </summary>
	public const int ERROR_DEVICE_NOT_CONNECTED = 1167;

	/// <summary>
	///     Element not found.
	/// </summary>
	public const int ERROR_NOT_FOUND = 1168;

	/// <summary>
	///     There was no match for the specified key in the index.
	/// </summary>
	public const int ERROR_NO_MATCH = 1169;

	/// <summary>
	///     The property set specified does not exist on the object.
	/// </summary>
	public const int ERROR_SET_NOT_FOUND = 1170;

	/// <summary>
	///     The point passed to GetMouseMovePoints is not in the buffer.
	/// </summary>
	public const int ERROR_POINT_NOT_FOUND = 1171;

	/// <summary>
	///     The tracking (workstation) service is not running.
	/// </summary>
	public const int ERROR_NO_TRACKING_SERVICE = 1172;

	/// <summary>
	///     The Volume ID could not be found.
	/// </summary>
	public const int ERROR_NO_VOLUME_ID = 1173;

	/// <summary>
	///     Unable to remove the file to be replaced.
	/// </summary>
	public const int ERROR_UNABLE_TO_REMOVE_REPLACED = 1175;

	/// <summary>
	///     Unable to move the replacement file to the file to be replaced. The file to be replaced has retained its original
	///     name.
	/// </summary>
	public const int ERROR_UNABLE_TO_MOVE_REPLACEMENT = 1176;

	/// <summary>
	///     Unable to move the replacement file to the file to be replaced. The file to be replaced has been renamed using the
	///     backup name.
	/// </summary>
	public const int ERROR_UNABLE_TO_MOVE_REPLACEMENT_2 = 1177;

	/// <summary>
	///     The volume change journal is being deleted.
	/// </summary>
	public const int ERROR_JOURNAL_DELETE_IN_PROGRESS = 1178;

	/// <summary>
	///     The volume change journal is not active.
	/// </summary>
	public const int ERROR_JOURNAL_NOT_ACTIVE = 1179;

	/// <summary>
	///     A file was found, but it may not be the correct file.
	/// </summary>
	public const int ERROR_POTENTIAL_FILE_FOUND = 1180;

	/// <summary>
	///     The journal entry has been deleted from the journal.
	/// </summary>
	public const int ERROR_JOURNAL_ENTRY_DELETED = 1181;

	/// <summary>
	///     The specified device name is invalid.
	/// </summary>
	public const int ERROR_BAD_DEVICE = 1200;

	/// <summary>
	///     The device is not currently connected but it is a remembered connection.
	/// </summary>
	public const int ERROR_CONNECTION_UNAVAIL = 1201;

	/// <summary>
	///     The local device name has a remembered connection to another network resource.
	/// </summary>
	public const int ERROR_DEVICE_ALREADY_REMEMBERED = 1202;

	/// <summary>
	///     No network provider accepted the given network path.
	/// </summary>
	public const int ERROR_NO_NET_OR_BAD_PATH = 1203;

	/// <summary>
	///     The specified network provider name is invalid.
	/// </summary>
	public const int ERROR_BAD_PROVIDER = 1204;

	/// <summary>
	///     Unable to open the network connection profile.
	/// </summary>
	public const int ERROR_CANNOT_OPEN_PROFILE = 1205;

	/// <summary>
	///     The network connection profile is corrupted.
	/// </summary>
	public const int ERROR_BAD_PROFILE = 1206;

	/// <summary>
	///     Cannot enumerate a noncontainer.
	/// </summary>
	public const int ERROR_NOT_CONTAINER = 1207;

	/// <summary>
	///     An extended error has occurred.
	/// </summary>
	public const int ERROR_EXTENDED_ERROR = 1208;

	/// <summary>
	///     The format of the specified group name is invalid.
	/// </summary>
	public const int ERROR_INVALID_GROUPNAME = 1209;

	/// <summary>
	///     The format of the specified computer name is invalid.
	/// </summary>
	public const int ERROR_INVALID_COMPUTERNAME = 1210;

	/// <summary>
	///     The format of the specified event name is invalid.
	/// </summary>
	public const int ERROR_INVALID_EVENTNAME = 1211;

	/// <summary>
	///     The format of the specified domain name is invalid.
	/// </summary>
	public const int ERROR_INVALID_DOMAINNAME = 1212;

	/// <summary>
	///     The format of the specified service name is invalid.
	/// </summary>
	public const int ERROR_INVALID_SERVICENAME = 1213;

	/// <summary>
	///     The format of the specified network name is invalid.
	/// </summary>
	public const int ERROR_INVALID_NETNAME = 1214;

	/// <summary>
	///     The format of the specified share name is invalid.
	/// </summary>
	public const int ERROR_INVALID_SHARENAME = 1215;

	/// <summary>
	///     The format of the specified password is invalid.
	/// </summary>
	public const int ERROR_INVALID_PASSUInt16NAME = 1216;

	/// <summary>
	///     The format of the specified message name is invalid.
	/// </summary>
	public const int ERROR_INVALID_MESSAGENAME = 1217;

	/// <summary>
	///     The format of the specified message destination is invalid.
	/// </summary>
	public const int ERROR_INVALID_MESSAGEDEST = 1218;

	/// <summary>
	///     Multiple connections to a server or shared resource by the same user, using more than one user name, are not
	///     allowed. Disconnect all previous connections to the server or shared resource and try again..
	/// </summary>
	public const int ERROR_SESSION_CREDENTIAL_CONFLICT = 1219;

	/// <summary>
	///     An attempt was made to establish a session to a network server, but there are already too many sessions established
	///     to that server.
	/// </summary>
	public const int ERROR_REMOTE_SESSION_LIMIT_EXCEEDED = 1220;

	/// <summary>
	///     The workgroup or domain name is already in use by another computer on the network.
	/// </summary>
	public const int ERROR_DUP_DOMAINNAME = 1221;

	/// <summary>
	///     The network is not present or not started.
	/// </summary>
	public const int ERROR_NO_NETWORK = 1222;

	/// <summary>
	///     The operation was canceled by the user.
	/// </summary>
	public const int ERROR_CANCELLED = 1223;

	/// <summary>
	///     The requested operation cannot be performed on a file with a user-mapped section open.
	/// </summary>
	public const int ERROR_USER_MAPPED_FILE = 1224;

	/// <summary>
	///     The remote system refused the network connection.
	/// </summary>
	public const int ERROR_CONNECTION_REFUSED = 1225;

	/// <summary>
	///     The network connection was gracefully closed.
	/// </summary>
	public const int ERROR_GRACEFUL_DISCONNECT = 1226;

	/// <summary>
	///     The network transport endpoint already has an address associated with it.
	/// </summary>
	public const int ERROR_ADDRESS_ALREADY_ASSOCIATED = 1227;

	/// <summary>
	///     An address has not yet been associated with the network endpoint.
	/// </summary>
	public const int ERROR_ADDRESS_NOT_ASSOCIATED = 1228;

	/// <summary>
	///     An operation was attempted on a nonexistent network connection.
	/// </summary>
	public const int ERROR_CONNECTION_INVALID = 1229;

	/// <summary>
	///     An invalid operation was attempted on an active network connection.
	/// </summary>
	public const int ERROR_CONNECTION_ACTIVE = 1230;

	/// <summary>
	///     The network location cannot be reached. For information about network troubleshooting, see Windows Help.
	/// </summary>
	public const int ERROR_NETWORK_UNREACHABLE = 1231;

	/// <summary>
	///     The network location cannot be reached. For information about network troubleshooting, see Windows Help.
	/// </summary>
	public const int ERROR_HOST_UNREACHABLE = 1232;

	/// <summary>
	///     The network location cannot be reached. For information about network troubleshooting, see Windows Help.
	/// </summary>
	public const int ERROR_PROTOCOL_UNREACHABLE = 1233;

	/// <summary>
	///     No service is operating at the destination network endpoint on the remote system.
	/// </summary>
	public const int ERROR_PORT_UNREACHABLE = 1234;

	/// <summary>
	///     The request was aborted.
	/// </summary>
	public const int ERROR_REQUEST_ABORTED = 1235;

	/// <summary>
	///     The network connection was aborted by the local system.
	/// </summary>
	public const int ERROR_CONNECTION_ABORTED = 1236;

	/// <summary>
	///     The operation could not be completed. A retry should be performed.
	/// </summary>
	public const int ERROR_RETRY = 1237;

	/// <summary>
	///     A connection to the server could not be made because the limit on the number of concurrent connections for this
	///     account has been reached.
	/// </summary>
	public const int ERROR_CONNECTION_COUNT_LIMIT = 1238;

	/// <summary>
	///     Attempting to log in during an unauthorized time of day for this account.
	/// </summary>
	public const int ERROR_LOGIN_TIME_RESTRICTION = 1239;

	/// <summary>
	///     The account is not authorized to log in from this station.
	/// </summary>
	public const int ERROR_LOGIN_WKSTA_RESTRICTION = 1240;

	/// <summary>
	///     The network address could not be used for the operation requested.
	/// </summary>
	public const int ERROR_INCORRECT_ADDRESS = 1241;

	/// <summary>
	///     The service is already registered.
	/// </summary>
	public const int ERROR_ALREADY_REGISTERED = 1242;

	/// <summary>
	///     The specified service does not exist.
	/// </summary>
	public const int ERROR_SERVICE_NOT_FOUND = 1243;

	/// <summary>
	///     The operation being requested was not performed because the user has not been authenticated.
	/// </summary>
	public const int ERROR_NOT_AUTHENTICATED = 1244;

	/// <summary>
	///     The operation being requested was not performed because the user has not logged on to the network.
	///     The specified service does not exist.
	/// </summary>
	public const int ERROR_NOT_LOGGED_ON = 1245;

	/// <summary>
	///     Continue with work in progress.
	/// </summary>
	public const int ERROR_CONTINUE = 1246;

	/// <summary>
	///     An attempt was made to perform an initialization operation when initialization has already been completed.
	/// </summary>
	public const int ERROR_ALREADY_INITIALIZED = 1247;

	/// <summary>
	///     No more local devices.
	/// </summary>
	public const int ERROR_NO_MORE_DEVICES = 1248;

	/// <summary>
	///     The specified site does not exist.
	/// </summary>
	public const int ERROR_NO_SUCH_SITE = 1249;

	/// <summary>
	///     A domain controller with the specified name already exists.
	/// </summary>
	public const int ERROR_DOMAIN_CONTROLLER_EXISTS = 1250;

	/// <summary>
	///     This operation is supported only when you are connected to the server.
	/// </summary>
	public const int ERROR_ONLY_IF_CONNECTED = 1251;

	/// <summary>
	///     The group policy framework should call the extension even if there are no changes.
	/// </summary>
	public const int ERROR_OVERRIDE_NOCHANGES = 1252;

	/// <summary>
	///     The specified user does not have a valid profile.
	/// </summary>
	public const int ERROR_BAD_USER_PROFILE = 1253;

	/// <summary>
	///     This operation is not supported on a Microsoft Small Business Server
	/// </summary>
	public const int ERROR_NOT_SUPPORTED_ON_SBS = 1254;

	/// <summary>
	///     The server machine is shutting down.
	/// </summary>
	public const int ERROR_SERVER_SHUTDOWN_IN_PROGRESS = 1255;

	/// <summary>
	///     The remote system is not available. For information about network troubleshooting, see Windows Help.
	/// </summary>
	public const int ERROR_HOST_DOWN = 1256;

	/// <summary>
	///     The security identifier provided is not from an account domain.
	/// </summary>
	public const int ERROR_NON_ACCOUNT_SID = 1257;

	/// <summary>
	///     The security identifier provided does not have a domain component.
	/// </summary>
	public const int ERROR_NON_DOMAIN_SID = 1258;

	/// <summary>
	///     AppHelp dialog canceled thus preventing the application from starting.
	/// </summary>
	public const int ERROR_APPHELP_BLOCK = 1259;

	/// <summary>
	///     Windows cannot open this program because it has been prevented by a software restriction policy. For more
	///     information, open Event Viewer or contact your system administrator.
	/// </summary>
	public const int ERROR_ACCESS_DISABLED_BY_POLICY = 1260;

	/// <summary>
	///     A program attempt to use an invalid register value.  Normally caused by an uninitialized register. This error is
	///     Itanium specific.
	/// </summary>
	public const int ERROR_REG_NAT_CONSUMPTION = 1261;

	/// <summary>
	///     The share is currently offline or does not exist.
	/// </summary>
	public const int ERROR_CSCSHARE_OFFLINE = 1262;

	/// <summary>
	///     The kerberos protocol encountered an error while validating the
	///     KDC certificate during smartcard logon.
	/// </summary>
	public const int ERROR_PKINIT_FAILURE = 1263;

	/// <summary>
	///     The kerberos protocol encountered an error while attempting to utilize
	///     the smartcard subsystem.
	/// </summary>
	public const int ERROR_SMARTCARD_SUBSYSTEM_FAILURE = 1264;

	/// <summary>
	///     The system detected a possible attempt to compromise security. Please ensure that you can contact the server that
	///     authenticated you.
	/// </summary>
	public const int ERROR_DOWNGRADE_DETECTED = 1265;

	/// <summary>
	///     The machine is locked and can not be shut down without the force option.
	/// </summary>
	public const int ERROR_MACHINE_LOCKED = 1271;

	/// <summary>
	///     An application-defined callback gave invalid data when called.
	/// </summary>
	public const int ERROR_CALLBACK_SUPPLIED_INVALID_DATA = 1273;

	/// <summary>
	///     The group policy framework should call the extension in the synchronous foreground policy refresh.
	/// </summary>
	public const int ERROR_SYNC_FOREGROUND_REFRESH_REQUIRED = 1274;

	/// <summary>
	///     This driver has been blocked from loading
	/// </summary>
	public const int ERROR_DRIVER_BLOCKED = 1275;

	/// <summary>
	///     A dynamic link library (DLL) referenced a module that was neither a DLL nor the process's executable image.
	/// </summary>
	public const int ERROR_INVALID_IMPORT_OF_NON_DLL = 1276;

	/// <summary>
	///     No information avialable.
	/// </summary>
	public const int ERROR_ACCESS_DISABLED_WEBBLADE = 1277;

	/// <summary>
	///     No information avialable.
	/// </summary>
	public const int ERROR_ACCESS_DISABLED_WEBBLADE_TAMPER = 1278;

	/// <summary>
	///     No information avialable.
	/// </summary>
	public const int ERROR_RECOVERY_FAILURE = 1279;

	/// <summary>
	///     No information avialable.
	/// </summary>
	public const int ERROR_ALREADY_FIBER = 1280;

	/// <summary>
	///     No information avialable.
	/// </summary>
	public const int ERROR_ALREADY_THREAD = 1281;

	/// <summary>
	///     No information avialable.
	/// </summary>
	public const int ERROR_STACK_BUFFER_OVERRUN = 1282;

	/// <summary>
	///     No information avialable.
	/// </summary>
	public const int ERROR_PARAMETER_QUOTA_EXCEEDED = 1283;

	/// <summary>
	///     No information avialable.
	/// </summary>
	public const int ERROR_DEBUGGER_INACTIVE = 1284;

	/// <summary>
	///     No information avialable.
	/// </summary>
	public const int ERROR_DELAY_LOAD_FAILED = 1285;

	/// <summary>
	///     No information avialable.
	/// </summary>
	public const int ERROR_VDM_DISALLOWED = 1286;

	/// <summary>
	///     Not all privileges referenced are assigned to the caller.
	/// </summary>
	public const int ERROR_NOT_ALL_ASSIGNED = 1300;

	/// <summary>
	///     Some mapping between account names and security IDs was not done.
	/// </summary>
	public const int ERROR_SOME_NOT_MAPPED = 1301;

	/// <summary>
	///     No system quota limits are specifically set for this account.
	/// </summary>
	public const int ERROR_NO_QUOTAS_FOR_ACCOUNT = 1302;

	/// <summary>
	///     No encryption key is available. A well-known encryption key was returned.
	/// </summary>
	public const int ERROR_LOCAL_USER_SESSION_KEY = 1303;

	/// <summary>
	///     The password is too complex to be converted to a LAN Manager password. The LAN Manager password returned is a NULL
	///     string.
	/// </summary>
	public const int ERROR_NULL_LM_PASSUInt16 = 1304;

	/// <summary>
	///     The revision level is unknown.
	/// </summary>
	public const int ERROR_UNKNOWN_REVISION = 1305;

	/// <summary>
	///     Indicates two revision levels are incompatible.
	/// </summary>
	public const int ERROR_REVISION_MISMATCH = 1306;

	/// <summary>
	///     This security ID may not be assigned as the owner of this object.
	/// </summary>
	public const int ERROR_INVALID_OWNER = 1307;

	/// <summary>
	///     This security ID may not be assigned as the primary group of an object.
	/// </summary>
	public const int ERROR_INVALID_PRIMARY_GROUP = 1308;

	/// <summary>
	///     An attempt has been made to operate on an impersonation token by a thread that is not currently impersonating a
	///     client.
	/// </summary>
	public const int ERROR_NO_IMPERSONATION_TOKEN = 1309;

	/// <summary>
	///     The group may not be disabled.
	/// </summary>
	public const int ERROR_CANT_DISABLE_MANDATORY = 1310;

	/// <summary>
	///     There are currently no logon servers available to service the logon request.
	/// </summary>
	public const int ERROR_NO_LOGON_SERVERS = 1311;

	/// <summary>
	///     A specified logon session does not exist. It may already have been terminated.
	/// </summary>
	public const int ERROR_NO_SUCH_LOGON_SESSION = 1312;

	/// <summary>
	///     A specified privilege does not exist.
	/// </summary>
	public const int ERROR_NO_SUCH_PRIVILEGE = 1313;

	/// <summary>
	///     A required privilege is not held by the client.
	/// </summary>
	public const int ERROR_PRIVILEGE_NOT_HELD = 1314;

	/// <summary>
	///     The name provided is not a properly formed account name.
	/// </summary>
	public const int ERROR_INVALID_ACCOUNT_NAME = 1315;

	/// <summary>
	///     The specified user already exists.
	/// </summary>
	public const int ERROR_USER_EXISTS = 1316;

	/// <summary>
	///     The specified user does not exist.
	/// </summary>
	public const int ERROR_NO_SUCH_USER = 1317;

	/// <summary>
	///     The specified group already exists.
	/// </summary>
	public const int ERROR_GROUP_EXISTS = 1318;

	/// <summary>
	///     The specified group does not exist.
	/// </summary>
	public const int ERROR_NO_SUCH_GROUP = 1319;

	/// <summary>
	///     Either the specified user account is already a member of the specified group, or the specified group cannot be
	///     deleted because it contains a member.
	/// </summary>
	public const int ERROR_MEMBER_IN_GROUP = 1320;

	/// <summary>
	///     The specified user account is not a member of the specified group account.
	/// </summary>
	public const int ERROR_MEMBER_NOT_IN_GROUP = 1321;

	/// <summary>
	///     The last remaining administration account cannot be disabled or deleted.
	/// </summary>
	public const int ERROR_LAST_ADMIN = 1322;

	/// <summary>
	///     Unable to update the password. The value provided as the current password is incorrect.
	/// </summary>
	public const int ERROR_WRONG_PASSWORD = 1323;

	/// <summary>
	///     Unable to update the password. The value provided for the new password contains values that are not allowed in
	///     passwords.
	/// </summary>
	public const int ERROR_ILL_FORMED_PASSWORD = 1324;

	/// <summary>
	///     Unable to update the password. The value provided for the new password does not meet the length, complexity, or
	///     history requirement of the domain.
	/// </summary>
	public const int ERROR_PASSWORD_RESTRICTION = 1325;

	/// <summary>
	///     Logon failure: unknown user name or bad password.
	/// </summary>
	public const int ERROR_LOGON_FAILURE = 1326;

	/// <summary>
	///     Logon failure: user account restriction.  Possible reasons are blank passwords not allowed, logon hour
	///     restrictions, or a policy restriction has been enforced.
	/// </summary>
	public const int ERROR_ACCOUNT_RESTRICTION = 1327;

	/// <summary>
	///     Logon failure: account logon time restriction violation.
	/// </summary>
	public const int ERROR_INVALID_LOGON_HOURS = 1328;

	/// <summary>
	///     Logon failure: user not allowed to log on to this computer.
	/// </summary>
	public const int ERROR_INVALID_WORKSTATION = 1329;

	/// <summary>
	///     Logon failure: the specified account password has expired.
	/// </summary>
	public const int ERROR_PASSUInt16_EXPIRED = 1330;

	/// <summary>
	///     Logon failure: account currently disabled.
	/// </summary>
	public const int ERROR_ACCOUNT_DISABLED = 1331;

	/// <summary>
	///     No mapping between account names and security IDs was done.
	/// </summary>
	public const int ERROR_NONE_MAPPED = 1332;

	/// <summary>
	///     Too many local user identifiers (LUIDs) were requested at one time.
	/// </summary>
	public const int ERROR_TOO_MANY_LUIDS_REQUESTED = 1333;

	/// <summary>
	///     No more local user identifiers (LUIDs) are available.
	/// </summary>
	public const int ERROR_LUIDS_EXHAUSTED = 1334;

	/// <summary>
	///     The subauthority part of a security ID is invalid for this particular use.
	/// </summary>
	public const int ERROR_INVALID_SUB_AUTHORITY = 1335;

	/// <summary>
	///     The access control list (ACL) structure is invalid.
	/// </summary>
	public const int ERROR_INVALID_ACL = 1336;

	/// <summary>
	///     The security ID structure is invalid.
	/// </summary>
	public const int ERROR_INVALID_SID = 1337;

	/// <summary>
	///     The security descriptor structure is invalid.
	/// </summary>
	public const int ERROR_INVALID_SECURITY_DESCR = 1338;

	/// <summary>
	///     The inherited access control list (ACL) or access control entry (ACE) could not be built.
	/// </summary>
	public const int ERROR_BAD_INHERITANCE_ACL = 1340;

	/// <summary>
	///     The server is currently disabled.
	/// </summary>
	public const int ERROR_SERVER_DISABLED = 1341;

	/// <summary>
	///     The server is currently enabled.
	/// </summary>
	public const int ERROR_SERVER_NOT_DISABLED = 1342;

	/// <summary>
	///     The value provided was an invalid value for an identifier authority.
	/// </summary>
	public const int ERROR_INVALID_ID_AUTHORITY = 1343;

	/// <summary>
	///     No more memory is available for security information updates.
	/// </summary>
	public const int ERROR_ALLOTTED_SPACE_EXCEEDED = 1344;

	/// <summary>
	///     The specified attributes are invalid, or incompatible with the attributes for the group as a whole.
	/// </summary>
	public const int ERROR_INVALID_GROUP_ATTRIBUTES = 1345;

	/// <summary>
	///     Either a required impersonation level was not provided, or the provided impersonation level is invalid.
	/// </summary>
	public const int ERROR_BAD_IMPERSONATION_LEVEL = 1346;

	/// <summary>
	///     Cannot open an anonymous level security token.
	/// </summary>
	public const int ERROR_CANT_OPEN_ANONYMOUS = 1347;

	/// <summary>
	///     The validation information class requested was invalid.
	/// </summary>
	public const int ERROR_BAD_VALIDATION_CLASS = 1348;

	/// <summary>
	///     The type of the token is inappropriate for its attempted use.
	/// </summary>
	public const int ERROR_BAD_TOKEN_TYPE = 1349;

	/// <summary>
	///     Unable to perform a security operation on an object that has no associated security.
	/// </summary>
	public const int ERROR_NO_SECURITY_ON_OBJECT = 1350;

	/// <summary>
	///     Configuration information could not be read from the domain controller, either because the machine is unavailable,
	///     or access has been denied.
	/// </summary>
	public const int ERROR_CANT_ACCESS_DOMAIN_INFO = 1351;

	/// <summary>
	///     The security account manager (SAM) or local security authority (LSA) server was in the wrong state to perform the
	///     security operation.
	/// </summary>
	public const int ERROR_INVALID_SERVER_STATE = 1352;

	/// <summary>
	///     The domain was in the wrong state to perform the security operation.
	/// </summary>
	public const int ERROR_INVALID_DOMAIN_STATE = 1353;

	/// <summary>
	///     This operation is only allowed for the Primary Domain Controller of the domain.
	/// </summary>
	public const int ERROR_INVALID_DOMAIN_ROLE = 1354;

	/// <summary>
	///     The specified domain either does not exist or could not be contacted.
	/// </summary>
	public const int ERROR_NO_SUCH_DOMAIN = 1355;

	/// <summary>
	///     The specified domain already exists.
	/// </summary>
	public const int ERROR_DOMAIN_EXISTS = 1356;

	/// <summary>
	///     An attempt was made to exceed the limit on the number of domains per server.
	/// </summary>
	public const int ERROR_DOMAIN_LIMIT_EXCEEDED = 1357;

	/// <summary>
	///     Unable to complete the requested operation because of either a catastrophic media failure or a data structure
	///     corruption on the disk.
	/// </summary>
	public const int ERROR_INTERNAL_DB_CORRUPTION = 1358;

	/// <summary>
	///     An internal error occurred.
	/// </summary>
	public const int ERROR_INTERNAL_ERROR = 1359;

	/// <summary>
	///     Generic access types were contained in an access mask which should already be mapped to nongeneric types.
	/// </summary>
	public const int ERROR_GENERIC_NOT_MAPPED = 1360;

	/// <summary>
	///     A security descriptor is not in the right format (absolute or self-relative).
	/// </summary>
	public const int ERROR_BAD_DESCRIPTOR_FORMAT = 1361;

	/// <summary>
	///     The requested action is restricted for use by logon processes only. The calling process has not registered as a
	///     logon process.
	/// </summary>
	public const int ERROR_NOT_LOGON_PROCESS = 1362;

	/// <summary>
	///     Cannot start a new logon session with an ID that is already in use.
	/// </summary>
	public const int ERROR_LOGON_SESSION_EXISTS = 1363;

	/// <summary>
	///     A specified authentication package is unknown.
	/// </summary>
	public const int ERROR_NO_SUCH_PACKAGE = 1364;

	/// <summary>
	///     The logon session is not in a state that is consistent with the requested operation.
	/// </summary>
	public const int ERROR_BAD_LOGON_SESSION_STATE = 1365;

	/// <summary>
	///     The logon session ID is already in use.
	/// </summary>
	public const int ERROR_LOGON_SESSION_COLLISION = 1366;

	/// <summary>
	///     A logon request contained an invalid logon type value.
	/// </summary>
	public const int ERROR_INVALID_LOGON_TYPE = 1367;

	/// <summary>
	///     Unable to impersonate using a named pipe until data has been read from that pipe.
	/// </summary>
	public const int ERROR_CANNOT_IMPERSONATE = 1368;

	/// <summary>
	///     The transaction state of a registry subtree is incompatible with the requested operation.
	/// </summary>
	public const int ERROR_RXACT_INVALID_STATE = 1369;

	/// <summary>
	///     An internal security database corruption has been encountered.
	/// </summary>
	public const int ERROR_RXACT_COMMIT_FAILURE = 1370;

	/// <summary>
	///     Cannot perform this operation on built-in accounts.
	/// </summary>
	public const int ERROR_SPECIAL_ACCOUNT = 1371;

	/// <summary>
	///     Cannot perform this operation on this built-in special group.
	/// </summary>
	public const int ERROR_SPECIAL_GROUP = 1372;

	/// <summary>
	///     Cannot perform this operation on this built-in special user.
	/// </summary>
	public const int ERROR_SPECIAL_USER = 1373;

	/// <summary>
	///     The user cannot be removed from a group because the group is currently the user's primary group.
	/// </summary>
	public const int ERROR_MEMBERS_PRIMARY_GROUP = 1374;

	/// <summary>
	///     The token is already in use as a primary token.
	/// </summary>
	public const int ERROR_TOKEN_ALREADY_IN_USE = 1375;

	/// <summary>
	///     The specified local group does not exist.
	/// </summary>
	public const int ERROR_NO_SUCH_ALIAS = 1376;

	/// <summary>
	///     The specified account name is not a member of the local group.
	/// </summary>
	public const int ERROR_MEMBER_NOT_IN_ALIAS = 1377;

	/// <summary>
	///     The specified account name is already a member of the local group.
	/// </summary>
	public const int ERROR_MEMBER_IN_ALIAS = 1378;

	/// <summary>
	///     The specified local group already exists.
	/// </summary>
	public const int ERROR_ALIAS_EXISTS = 1379;

	/// <summary>
	///     Logon failure: the user has not been granted the requested logon type at this computer.
	/// </summary>
	public const int ERROR_LOGON_NOT_GRANTED = 1380;

	/// <summary>
	///     The maximum number of secrets that may be stored in a single system has been exceeded.
	/// </summary>
	public const int ERROR_TOO_MANY_SECRETS = 1381;

	/// <summary>
	///     The length of a secret exceeds the maximum length allowed.
	/// </summary>
	public const int ERROR_SECRET_TOO_Int32 = 1382;

	/// <summary>
	///     The local security authority database contains an internal inconsistency.
	/// </summary>
	public const int ERROR_INTERNAL_DB_ERROR = 1383;

	/// <summary>
	///     During a logon attempt, the user's security context accumulated too many security IDs.
	/// </summary>
	public const int ERROR_TOO_MANY_CONTEXT_IDS = 1384;

	/// <summary>
	///     Logon failure: the user has not been granted the requested logon type at this computer.
	/// </summary>
	public const int ERROR_LOGON_TYPE_NOT_GRANTED = 1385;

	/// <summary>
	///     A cross-encrypted password is necessary to change a user password.
	/// </summary>
	public const int ERROR_NT_CROSS_ENCRYPTION_REQUIRED = 1386;

	/// <summary>
	///     A member could not be added to or removed from the local group because the member does not exist.
	/// </summary>
	public const int ERROR_NO_SUCH_MEMBER = 1387;

	/// <summary>
	///     A new member could not be added to a local group because the member has the wrong account type.
	/// </summary>
	public const int ERROR_INVALID_MEMBER = 1388;

	/// <summary>
	///     Too many security IDs have been specified.
	/// </summary>
	public const int ERROR_TOO_MANY_SIDS = 1389;

	/// <summary>
	///     A cross-encrypted password is necessary to change this user password.
	/// </summary>
	public const int ERROR_LM_CROSS_ENCRYPTION_REQUIRED = 1390;

	/// <summary>
	///     Indicates an ACL contains no inheritable components.
	/// </summary>
	public const int ERROR_NO_INHERITANCE = 1391;

	/// <summary>
	///     The file or directory is corrupted and unreadable.
	/// </summary>
	public const int ERROR_FILE_CORRUPT = 1392;

	/// <summary>
	///     The disk structure is corrupted and unreadable.
	/// </summary>
	public const int ERROR_DISK_CORRUPT = 1393;

	/// <summary>
	///     There is no user session key for the specified logon session.
	/// </summary>
	public const int ERROR_NO_USER_SESSION_KEY = 1394;

	/// <summary>
	///     The service being accessed is licensed for a particular number of connections.
	///     No more connections can be made to the service at this time because there are already as many connections as the
	///     service can accept.
	/// </summary>
	public const int ERROR_LICENSE_QUOTA_EXCEEDED = 1395;

	/// <summary>
	///     Logon Failure: The target account name is incorrect.
	/// </summary>
	public const int ERROR_WRONG_TARGET_NAME = 1396;

	/// <summary>
	///     Mutual Authentication failed. The server's password is out of date at the domain controller.
	/// </summary>
	public const int ERROR_MUTUAL_AUTH_FAILED = 1397;

	/// <summary>
	///     There is a time and/or date difference between the client and server.
	/// </summary>
	public const int ERROR_TIME_SKEW = 1398;

	/// <summary>
	///     This operation can not be performed on the current domain.
	/// </summary>
	public const int ERROR_CURRENT_DOMAIN_NOT_ALLOWED = 1399;

	/// <summary>
	///     Invalid window handle.
	/// </summary>
	public const int ERROR_INVALID_WINDOW_HANDLE = 1400;

	/// <summary>
	///     Invalid menu handle.
	/// </summary>
	public const int ERROR_INVALID_MENU_HANDLE = 1401;

	/// <summary>
	///     Invalid cursor handle.
	/// </summary>
	public const int ERROR_INVALID_CURSOR_HANDLE = 1402;

	/// <summary>
	///     Invalid accelerator table handle.
	/// </summary>
	public const int ERROR_INVALID_ACCEL_HANDLE = 1403;

	/// <summary>
	///     Invalid hook handle.
	/// </summary>
	public const int ERROR_INVALID_HOOK_HANDLE = 1404;

	/// <summary>
	///     Invalid handle to a multiple-window position structure.
	/// </summary>
	public const int ERROR_INVALID_DWP_HANDLE = 1405;

	/// <summary>
	///     Cannot create a top-level child window.
	/// </summary>
	public const int ERROR_TLW_WITH_WSCHILD = 1406;

	/// <summary>
	///     Cannot find window class.
	/// </summary>
	public const int ERROR_CANNOT_FIND_WND_CLASS = 1407;

	/// <summary>
	///     Invalid window; it belongs to other thread.
	/// </summary>
	public const int ERROR_WINDOW_OF_OTHER_THREAD = 1408;

	/// <summary>
	///     Hot key is already registered.
	/// </summary>
	public const int ERROR_HOTKEY_ALREADY_REGISTERED = 1409;

	/// <summary>
	///     Class already exists.
	/// </summary>
	public const int ERROR_CLASS_ALREADY_EXISTS = 1410;

	/// <summary>
	///     Class does not exist.
	/// </summary>
	public const int ERROR_CLASS_DOES_NOT_EXIST = 1411;

	/// <summary>
	///     Class still has open windows.
	/// </summary>
	public const int ERROR_CLASS_HAS_WINDOWS = 1412;

	/// <summary>
	///     Invalid index.
	/// </summary>
	public const int ERROR_INVALID_INDEX = 1413;

	/// <summary>
	///     Invalid icon handle.
	/// </summary>
	public const int ERROR_INVALID_ICON_HANDLE = 1414;

	/// <summary>
	///     Using private DIALOG window words.
	/// </summary>
	public const int ERROR_PRIVATE_DIALOG_INDEX = 1415;

	/// <summary>
	///     The list box identifier was not found.
	/// </summary>
	public const int ERROR_LISTBOX_ID_NOT_FOUND = 1416;

	/// <summary>
	///     No wildcards were found.
	/// </summary>
	public const int ERROR_NO_WILDCARD_CHARACTERS = 1417;

	/// <summary>
	///     Thread does not have a clipboard open.
	/// </summary>
	public const int ERROR_CLIPBOARD_NOT_OPEN = 1418;

	/// <summary>
	///     Hot key is not registered.
	/// </summary>
	public const int ERROR_HOTKEY_NOT_REGISTERED = 1419;

	/// <summary>
	///     The window is not a valid dialog window.
	/// </summary>
	public const int ERROR_WINDOW_NOT_DIALOG = 1420;

	/// <summary>
	///     Control ID not found.
	/// </summary>
	public const int ERROR_CONTROL_ID_NOT_FOUND = 1421;

	/// <summary>
	///     Invalid message for a combo box because it does not have an edit control.
	/// </summary>
	public const int ERROR_INVALID_COMBOBOX_MESSAGE = 1422;

	/// <summary>
	///     The window is not a combo box.
	/// </summary>
	public const int ERROR_WINDOW_NOT_COMBOBOX = 1423;

	/// <summary>
	///     Height must be less than 256.
	/// </summary>
	public const int ERROR_INVALID_EDIT_HEIGHT = 1424;

	/// <summary>
	///     Invalid device context (DC) handle.
	/// </summary>
	public const int ERROR_DC_NOT_FOUND = 1425;

	/// <summary>
	///     Invalid hook procedure type.
	/// </summary>
	public const int ERROR_INVALID_HOOK_FILTER = 1426;

	/// <summary>
	///     Invalid hook procedure.
	/// </summary>
	public const int ERROR_INVALID_FILTER_PROC = 1427;

	/// <summary>
	///     Cannot set nonlocal hook without a module handle.
	/// </summary>
	public const int ERROR_HOOK_NEEDS_HMOD = 1428;

	/// <summary>
	///     This hook procedure can only be set globally.
	/// </summary>
	public const int ERROR_GLOBAL_ONLY_HOOK = 1429;

	/// <summary>
	///     The journal hook procedure is already installed.
	/// </summary>
	public const int ERROR_JOURNAL_HOOK_SET = 1430;

	/// <summary>
	///     The hook procedure is not installed.
	/// </summary>
	public const int ERROR_HOOK_NOT_INSTALLED = 1431;

	/// <summary>
	///     Invalid message for single-selection list box.
	/// </summary>
	public const int ERROR_INVALID_LB_MESSAGE = 1432;

	/// <summary>
	///     LB_SETCOUNT sent to non-lazy list box.
	/// </summary>
	public const int ERROR_SETCOUNT_ON_BAD_LB = 1433;

	/// <summary>
	///     This list box does not support tab stops.
	/// </summary>
	public const int ERROR_LB_WITHOUT_TABSTOPS = 1434;

	/// <summary>
	///     Cannot destroy object created by another thread.
	/// </summary>
	public const int ERROR_DESTROY_OBJECT_OF_OTHER_THREAD = 1435;

	/// <summary>
	///     Child windows cannot have menus.
	/// </summary>
	public const int ERROR_CHILD_WINDOW_MENU = 1436;

	/// <summary>
	///     The window does not have a system menu.
	/// </summary>
	public const int ERROR_NO_SYSTEM_MENU = 1437;

	/// <summary>
	///     Invalid message box style.
	/// </summary>
	public const int ERROR_INVALID_MSGBOX_STYLE = 1438;

	/// <summary>
	///     Invalid system-wide (SPI_*) parameter.
	/// </summary>
	public const int ERROR_INVALID_SPI_VALUE = 1439;

	/// <summary>
	///     Screen already locked.
	/// </summary>
	public const int ERROR_SCREEN_ALREADY_LOCKED = 1440;

	/// <summary>
	///     All handles to windows in a multiple-window position structure must have the same parent.
	/// </summary>
	public const int ERROR_HWNDS_HAVE_DIFF_PARENT = 1441;

	/// <summary>
	///     The window is not a child window.
	/// </summary>
	public const int ERROR_NOT_CHILD_WINDOW = 1442;

	/// <summary>
	///     Invalid GW_* command.
	/// </summary>
	public const int ERROR_INVALID_GW_COMMAND = 1443;

	/// <summary>
	///     Invalid thread identifier.
	/// </summary>
	public const int ERROR_INVALID_THREAD_ID = 1444;

	/// <summary>
	///     Cannot process a message from a window that is not a multiple document interface (MDI) window.
	/// </summary>
	public const int ERROR_NON_MDICHILD_WINDOW = 1445;

	/// <summary>
	///     Popup menu already active.
	/// </summary>
	public const int ERROR_POPUP_ALREADY_ACTIVE = 1446;

	/// <summary>
	///     The window does not have scroll bars.
	/// </summary>
	public const int ERROR_NO_SCROLLBARS = 1447;

	/// <summary>
	///     Scroll bar range cannot be greater than MAXLONG.
	/// </summary>
	public const int ERROR_INVALID_SCROLLBAR_RANGE = 1448;

	/// <summary>
	///     Cannot show or remove the window in the way specified.
	/// </summary>
	public const int ERROR_INVALID_SHOWWIN_COMMAND = 1449;

	/// <summary>
	///     Insufficient system resources exist to complete the requested service.
	/// </summary>
	public const int ERROR_NO_SYSTEM_RESOURCES = 1450;

	/// <summary>
	///     Insufficient system resources exist to complete the requested service.
	/// </summary>
	public const int ERROR_NONPAGED_SYSTEM_RESOURCES = 1451;

	/// <summary>
	///     Insufficient system resources exist to complete the requested service.
	/// </summary>
	public const int ERROR_PAGED_SYSTEM_RESOURCES = 1452;

	/// <summary>
	///     Insufficient quota to complete the requested service.
	/// </summary>
	public const int ERROR_WORKING_SET_QUOTA = 1453;

	/// <summary>
	///     Insufficient quota to complete the requested service.
	/// </summary>
	public const int ERROR_PAGEFILE_QUOTA = 1454;

	/// <summary>
	///     The paging file is too small for this operation to complete.
	/// </summary>
	public const int ERROR_COMMITMENT_LIMIT = 1455;

	/// <summary>
	///     A menu item was not found.
	/// </summary>
	public const int ERROR_MENU_ITEM_NOT_FOUND = 1456;

	/// <summary>
	///     Invalid keyboard layout handle.
	/// </summary>
	public const int ERROR_INVALID_KEYBOARD_HANDLE = 1457;

	/// <summary>
	///     Hook type not allowed.
	/// </summary>
	public const int ERROR_HOOK_TYPE_NOT_ALLOWED = 1458;

	/// <summary>
	///     This operation requires an interactive window station.
	/// </summary>
	public const int ERROR_REQUIRES_INTERACTIVE_WINDOWSTATION = 1459;

	/// <summary>
	///     This operation returned because the timeout period expired.
	/// </summary>
	public const int ERROR_TIMEOUT = 1460;

	/// <summary>
	///     Invalid monitor handle.
	/// </summary>
	public const int ERROR_INVALID_MONITOR_HANDLE = 1461;

	/// <summary>
	///     The event log file is corrupted.
	/// </summary>
	public const int ERROR_EVENTLOG_FILE_CORRUPT = 1500;

	/// <summary>
	///     No event log file could be opened, so the event logging service did not start.
	/// </summary>
	public const int ERROR_EVENTLOG_CANT_START = 1501;

	/// <summary>
	///     The event log file is full.
	/// </summary>
	public const int ERROR_LOG_FILE_FULL = 1502;

	/// <summary>
	///     The event log file has changed between read operations.
	/// </summary>
	public const int ERROR_EVENTLOG_FILE_CHANGED = 1503;

	/// <summary>
	///     The Windows Installer Service could not be accessed. This can occur if you are running Windows in safe mode, or if
	///     the Windows Installer is not correctly installed. Contact your support personnel for assistance.
	/// </summary>
	public const int ERROR_INSTALL_SERVICE_FAILURE = 1601;

	/// <summary>
	///     User cancelled installation.
	/// </summary>
	public const int ERROR_INSTALL_USEREXIT = 1602;

	/// <summary>
	///     Fatal error during installation.
	/// </summary>
	public const int ERROR_INSTALL_FAILURE = 1603;

	/// <summary>
	///     Installation suspended, incomplete.
	/// </summary>
	public const int ERROR_INSTALL_SUSPEND = 1604;

	/// <summary>
	///     This action is only valid for products that are currently installed.
	/// </summary>
	public const int ERROR_UNKNOWN_PRODUCT = 1605;

	/// <summary>
	///     Feature ID not registered.
	/// </summary>
	public const int ERROR_UNKNOWN_FEATURE = 1606;

	/// <summary>
	///     Component ID not registered.
	/// </summary>
	public const int ERROR_UNKNOWN_COMPONENT = 1607;

	/// <summary>
	///     Unknown property.
	/// </summary>
	public const int ERROR_UNKNOWN_PROPERTY = 1608;

	/// <summary>
	///     Handle is in an invalid state.
	/// </summary>
	public const int ERROR_INVALID_HANDLE_STATE = 1609;

	/// <summary>
	///     The configuration data for this product is corrupt.  Contact your support personnel.
	/// </summary>
	public const int ERROR_BAD_CONFIGURATION = 1610;

	/// <summary>
	///     Component qualifier not present.
	/// </summary>
	public const int ERROR_INDEX_ABSENT = 1611;

	/// <summary>
	///     The installation source for this product is not available.  Verify that the source exists and that you can access
	///     it.
	/// </summary>
	public const int ERROR_INSTALL_SOURCE_ABSENT = 1612;

	/// <summary>
	///     This installation package cannot be installed by the Windows Installer service.  You must install a Windows service
	///     pack that contains a newer version of the Windows Installer service.
	/// </summary>
	public const int ERROR_INSTALL_PACKAGE_VERSION = 1613;

	/// <summary>
	///     Product is uninstalled.
	/// </summary>
	public const int ERROR_PRODUCT_UNINSTALLED = 1614;

	/// <summary>
	///     SQL query syntax invalid or unsupported.
	/// </summary>
	public const int ERROR_BAD_QUERY_SYNTAX = 1615;

	/// <summary>
	///     Record field does not exist.
	/// </summary>
	public const int ERROR_INVALID_FIELD = 1616;

	/// <summary>
	///     The device has been removed.
	/// </summary>
	public const int ERROR_DEVICE_REMOVED = 1617;

	/// <summary>
	///     Another installation is already in progress.  Complete that installation before proceeding with this install.
	/// </summary>
	public const int ERROR_INSTALL_ALREADY_RUNNING = 1618;

	/// <summary>
	///     This installation package could not be opened.  Verify that the package exists and that you can access it, or
	///     contact the application vendor to verify that this is a valid Windows Installer package.
	/// </summary>
	public const int ERROR_INSTALL_PACKAGE_OPEN_FAILED = 1619;

	/// <summary>
	///     This installation package could not be opened.  Contact the application vendor to verify that this is a valid
	///     Windows Installer package.
	/// </summary>
	public const int ERROR_INSTALL_PACKAGE_INVALID = 1620;

	/// <summary>
	///     There was an error starting the Windows Installer service user interface.  Contact your support personnel.
	/// </summary>
	public const int ERROR_INSTALL_UI_FAILURE = 1621;

	/// <summary>
	///     Error opening installation log file. Verify that the specified log file location exists and that you can write to
	///     it.
	/// </summary>
	public const int ERROR_INSTALL_LOG_FAILURE = 1622;

	/// <summary>
	///     The language of this installation package is not supported by your system.
	/// </summary>
	public const int ERROR_INSTALL_LANGUAGE_UNSUPPORTED = 1623;

	/// <summary>
	///     Error applying transforms.  Verify that the specified transform paths are valid.
	/// </summary>
	public const int ERROR_INSTALL_TRANSFORM_FAILURE = 1624;

	/// <summary>
	///     This installation is forbidden by system policy.  Contact your system administrator.
	/// </summary>
	public const int ERROR_INSTALL_PACKAGE_REJECTED = 1625;

	/// <summary>
	///     Function could not be executed.
	/// </summary>
	public const int ERROR_FUNCTION_NOT_CALLED = 1626;

	/// <summary>
	///     Function failed during execution.
	/// </summary>
	public const int ERROR_FUNCTION_FAILED = 1627;

	/// <summary>
	///     Invalid or unknown table specified.
	/// </summary>
	public const int ERROR_INVALID_TABLE = 1628;

	/// <summary>
	///     Data supplied is of wrong type.
	/// </summary>
	public const int ERROR_DATATYPE_MISMATCH = 1629;

	/// <summary>
	///     Data of this type is not supported.
	/// </summary>
	public const int ERROR_UNSUPPORTED_TYPE = 1630;

	/// <summary>
	///     The Windows Installer service failed to start.  Contact your support personnel.
	/// </summary>
	public const int ERROR_CREATE_FAILED = 1631;

	/// <summary>
	///     The Temp folder is on a drive that is full or is inaccessible. Free up space on the drive or verify that you have
	///     write permission on the Temp folder.
	/// </summary>
	public const int ERROR_INSTALL_TEMP_UNWRITABLE = 1632;

	/// <summary>
	///     This installation package is not supported by this processor type. Contact your product vendor.
	/// </summary>
	public const int ERROR_INSTALL_PLATFORM_UNSUPPORTED = 1633;

	/// <summary>
	///     Component not used on this computer.
	/// </summary>
	public const int ERROR_INSTALL_NOTUSED = 1634;

	/// <summary>
	///     This patch package could not be opened.  Verify that the patch package exists and that you can access it, or
	///     contact the application vendor to verify that this is a valid Windows Installer patch package.
	/// </summary>
	public const int ERROR_PATCH_PACKAGE_OPEN_FAILED = 1635;

	/// <summary>
	///     This patch package could not be opened.  Contact the application vendor to verify that this is a valid Windows
	///     Installer patch package.
	/// </summary>
	public const int ERROR_PATCH_PACKAGE_INVALID = 1636;

	/// <summary>
	///     This patch package cannot be processed by the Windows Installer service.  You must install a Windows service pack
	///     that contains a newer version of the Windows Installer service.
	/// </summary>
	public const int ERROR_PATCH_PACKAGE_UNSUPPORTED = 1637;

	/// <summary>
	///     Another version of this product is already installed.  Installation of this version cannot continue.  To configure
	///     or remove the existing version of this product, use Add/Remove Programs on the Control Panel.
	/// </summary>
	public const int ERROR_PRODUCT_VERSION = 1638;

	/// <summary>
	///     Invalid command line argument.  Consult the Windows Installer SDK for detailed command line help.
	/// </summary>
	public const int ERROR_INVALID_COMMAND_LINE = 1639;

	/// <summary>
	///     Only administrators have permission to add, remove, or configure server software during a Terminal services remote
	///     session. If you want to install or configure software on the server, contact your network administrator.
	/// </summary>
	public const int ERROR_INSTALL_REMOTE_DISALLOWED = 1640;

	/// <summary>
	///     The requested operation completed successfully.  The system will be restarted so the changes can take effect.
	/// </summary>
	public const int ERROR_SUCCESS_REBOOT_INITIATED = 1641;

	/// <summary>
	///     The upgrade patch cannot be installed by the Windows Installer service because the program to be upgraded may be
	///     missing, or the upgrade patch may update a different version of the program. Verify that the program to be upgraded
	///     exists on your computer an
	///     d that you have the correct upgrade patch.
	/// </summary>
	public const int ERROR_PATCH_TARGET_NOT_FOUND = 1642;

	/// <summary>
	///     The patch package is not permitted by software restriction policy.
	/// </summary>
	public const int ERROR_PATCH_PACKAGE_REJECTED = 1643;

	/// <summary>
	///     One or more customizations are not permitted by software restriction policy.
	/// </summary>
	public const int ERROR_INSTALL_TRANSFORM_REJECTED = 1644;

	/// <summary>
	///     No information avialable.
	/// </summary>
	public const int ERROR_INSTALL_REMOTE_PROHIBITED = 1645;

	/// <summary>
	///     The string binding is invalid.
	/// </summary>
	public const int RPC_S_INVALID_STRING_BINDING = 1700;

	/// <summary>
	///     The binding handle is not the correct type.
	/// </summary>
	public const int RPC_S_WRONG_KIND_OF_BINDING = 1701;

	/// <summary>
	///     The binding handle is invalid.
	/// </summary>
	public const int RPC_S_INVALID_BINDING = 1702;

	/// <summary>
	///     The RPC protocol sequence is not supported.
	/// </summary>
	public const int RPC_S_PROTSEQ_NOT_SUPPORTED = 1703;

	/// <summary>
	///     The RPC protocol sequence is invalid.
	/// </summary>
	public const int RPC_S_INVALID_RPC_PROTSEQ = 1704;

	/// <summary>
	///     The string universal unique identifier (UUID) is invalid.
	/// </summary>
	public const int RPC_S_INVALID_STRING_UUID = 1705;

	/// <summary>
	///     The endpoint format is invalid.
	/// </summary>
	public const int RPC_S_INVALID_ENDPOINT_FORMAT = 1706;

	/// <summary>
	///     The network address is invalid.
	/// </summary>
	public const int RPC_S_INVALID_NET_ADDR = 1707;

	/// <summary>
	///     No endpoint was found.
	/// </summary>
	public const int RPC_S_NO_ENDPOINT_FOUND = 1708;

	/// <summary>
	///     The timeout value is invalid.
	/// </summary>
	public const int RPC_S_INVALID_TIMEOUT = 1709;

	/// <summary>
	///     The object universal unique identifier (UUID) was not found.
	/// </summary>
	public const int RPC_S_OBJECT_NOT_FOUND = 1710;

	/// <summary>
	///     The object universal unique identifier (UUID) has already been registered.
	/// </summary>
	public const int RPC_S_ALREADY_REGISTERED = 1711;

	/// <summary>
	///     The type universal unique identifier (UUID) has already been registered.
	/// </summary>
	public const int RPC_S_TYPE_ALREADY_REGISTERED = 1712;

	/// <summary>
	///     The RPC server is already listening.
	/// </summary>
	public const int RPC_S_ALREADY_LISTENING = 1713;

	/// <summary>
	///     No protocol sequences have been registered.
	/// </summary>
	public const int RPC_S_NO_PROTSEQS_REGISTERED = 1714;

	/// <summary>
	///     The RPC server is not listening.
	/// </summary>
	public const int RPC_S_NOT_LISTENING = 1715;

	/// <summary>
	///     The manager type is unknown.
	/// </summary>
	public const int RPC_S_UNKNOWN_MGR_TYPE = 1716;

	/// <summary>
	///     The interface is unknown.
	/// </summary>
	public const int RPC_S_UNKNOWN_IF = 1717;

	/// <summary>
	///     There are no bindings.
	/// </summary>
	public const int RPC_S_NO_BINDINGS = 1718;

	/// <summary>
	///     There are no protocol sequences.
	/// </summary>
	public const int RPC_S_NO_PROTSEQS = 1719;

	/// <summary>
	///     The endpoint cannot be created.
	/// </summary>
	public const int RPC_S_CANT_CREATE_ENDPOINT = 1720;

	/// <summary>
	///     Not enough resources are available to complete this operation.
	/// </summary>
	public const int RPC_S_OUT_OF_RESOURCES = 1721;

	/// <summary>
	///     The RPC server is unavailable.
	/// </summary>
	public const int RPC_S_SERVER_UNAVAILABLE = 1722;

	/// <summary>
	///     The RPC server is too busy to complete this operation.
	/// </summary>
	public const int RPC_S_SERVER_TOO_BUSY = 1723;

	/// <summary>
	///     The network options are invalid.
	/// </summary>
	public const int RPC_S_INVALID_NETWORK_OPTIONS = 1724;

	/// <summary>
	///     There are no remote procedure calls active on this thread.
	/// </summary>
	public const int RPC_S_NO_CALL_ACTIVE = 1725;

	/// <summary>
	///     The remote procedure call failed.
	/// </summary>
	public const int RPC_S_CALL_FAILED = 1726;

	/// <summary>
	///     The remote procedure call failed and did not execute.
	/// </summary>
	public const int RPC_S_CALL_FAILED_DNE = 1727;

	/// <summary>
	///     A remote procedure call (RPC) protocol error occurred.
	/// </summary>
	public const int RPC_S_PROTOCOL_ERROR = 1728;

	/// <summary>
	///     The transfer syntax is not supported by the RPC server.
	/// </summary>
	public const int RPC_S_UNSUPPORTED_TRANS_SYN = 1730;

	/// <summary>
	///     The universal unique identifier (UUID) type is not supported.
	/// </summary>
	public const int RPC_S_UNSUPPORTED_TYPE = 1732;

	/// <summary>
	///     The tag is invalid.
	/// </summary>
	public const int RPC_S_INVALID_TAG = 1733;

	/// <summary>
	///     The array bounds are invalid.
	/// </summary>
	public const int RPC_S_INVALID_BOUND = 1734;

	/// <summary>
	///     The binding does not contain an entry name.
	/// </summary>
	public const int RPC_S_NO_ENTRY_NAME = 1735;

	/// <summary>
	///     The name syntax is invalid.
	/// </summary>
	public const int RPC_S_INVALID_NAME_SYNTAX = 1736;

	/// <summary>
	///     The name syntax is not supported.
	/// </summary>
	public const int RPC_S_UNSUPPORTED_NAME_SYNTAX = 1737;

	/// <summary>
	///     No network address is available to use to construct a universal unique identifier (UUID).
	/// </summary>
	public const int RPC_S_UUID_NO_ADDRESS = 1739;

	/// <summary>
	///     The endpoint is a duplicate.
	/// </summary>
	public const int RPC_S_DUPLICATE_ENDPOINT = 1740;

	/// <summary>
	///     The authentication type is unknown.
	/// </summary>
	public const int RPC_S_UNKNOWN_AUTHN_TYPE = 1741;

	/// <summary>
	///     The maximum number of calls is too small.
	/// </summary>
	public const int RPC_S_MAX_CALLS_TOO_SMALL = 1742;

	/// <summary>
	///     The string is too long.
	/// </summary>
	public const int RPC_S_STRING_TOO_Int32 = 1743;

	/// <summary>
	///     The RPC protocol sequence was not found.
	/// </summary>
	public const int RPC_S_PROTSEQ_NOT_FOUND = 1744;

	/// <summary>
	///     The procedure number is out of range.
	/// </summary>
	public const int RPC_S_PROCNUM_OUT_OF_RANGE = 1745;

	/// <summary>
	///     The binding does not contain any authentication information.
	/// </summary>
	public const int RPC_S_BINDING_HAS_NO_AUTH = 1746;

	/// <summary>
	///     The authentication service is unknown.
	/// </summary>
	public const int RPC_S_UNKNOWN_AUTHN_SERVICE = 1747;

	/// <summary>
	///     The authentication level is unknown.
	/// </summary>
	public const int RPC_S_UNKNOWN_AUTHN_LEVEL = 1748;

	/// <summary>
	///     The security context is invalid.
	/// </summary>
	public const int RPC_S_INVALID_AUTH_IDENTITY = 1749;

	/// <summary>
	///     The authorization service is unknown.
	/// </summary>
	public const int RPC_S_UNKNOWN_AUTHZ_SERVICE = 1750;

	/// <summary>
	///     The entry is invalid.
	/// </summary>
	public const int EPT_S_INVALID_ENTRY = 1751;

	/// <summary>
	///     The server endpoint cannot perform the operation.
	/// </summary>
	public const int EPT_S_CANT_PERFORM_OP = 1752;

	/// <summary>
	///     There are no more endpoints available from the endpoint mapper.
	/// </summary>
	public const int EPT_S_NOT_REGISTERED = 1753;

	/// <summary>
	///     No interfaces have been exported.
	/// </summary>
	public const int RPC_S_NOTHING_TO_EXPORT = 1754;

	/// <summary>
	///     The entry name is incomplete.
	/// </summary>
	public const int RPC_S_INCOMPLETE_NAME = 1755;

	/// <summary>
	///     The version option is invalid.
	/// </summary>
	public const int RPC_S_INVALID_VERS_OPTION = 1756;

	/// <summary>
	///     There are no more members.
	/// </summary>
	public const int RPC_S_NO_MORE_MEMBERS = 1757;

	/// <summary>
	///     There is nothing to unexport.
	/// </summary>
	public const int RPC_S_NOT_ALL_OBJS_UNEXPORTED = 1758;

	/// <summary>
	///     The interface was not found.
	/// </summary>
	public const int RPC_S_INTERFACE_NOT_FOUND = 1759;

	/// <summary>
	///     The entry already exists.
	/// </summary>
	public const int RPC_S_ENTRY_ALREADY_EXISTS = 1760;

	/// <summary>
	///     The entry is not found.
	/// </summary>
	public const int RPC_S_ENTRY_NOT_FOUND = 1761;

	/// <summary>
	///     The name service is unavailable.
	/// </summary>
	public const int RPC_S_NAME_SERVICE_UNAVAILABLE = 1762;

	/// <summary>
	///     The network address family is invalid.
	/// </summary>
	public const int RPC_S_INVALID_NAF_ID = 1763;

	/// <summary>
	///     The requested operation is not supported.
	/// </summary>
	public const int RPC_S_CANNOT_SUPPORT = 1764;

	/// <summary>
	///     No security context is available to allow impersonation.
	/// </summary>
	public const int RPC_S_NO_CONTEXT_AVAILABLE = 1765;

	/// <summary>
	///     An internal error occurred in a remote procedure call (RPC).
	/// </summary>
	public const int RPC_S_INTERNAL_ERROR = 1766;

	/// <summary>
	///     The RPC server attempted an integer division by zero.
	/// </summary>
	public const int RPC_S_ZERO_DIVIDE = 1767;

	/// <summary>
	///     An addressing error occurred in the RPC server.
	/// </summary>
	public const int RPC_S_ADDRESS_ERROR = 1768;

	/// <summary>
	///     A floating-point operation at the RPC server caused a division by zero.
	/// </summary>
	public const int RPC_S_FP_DIV_ZERO = 1769;

	/// <summary>
	///     A floating-point underflow occurred at the RPC server.
	/// </summary>
	public const int RPC_S_FP_UNDERFLOW = 1770;

	/// <summary>
	///     A floating-point overflow occurred at the RPC server.
	/// </summary>
	public const int RPC_S_FP_OVERFLOW = 1771;

	/// <summary>
	///     The list of RPC servers available for the binding of auto handles has been exhausted.
	/// </summary>
	public const int RPC_X_NO_MORE_ENTRIES = 1772;

	/// <summary>
	///     Unable to open the character translation table file.
	/// </summary>
	public const int RPC_X_SS_CHAR_TRANS_OPEN_FAIL = 1773;

	/// <summary>
	///     The file containing the character translation table has fewer than 512 bytes.
	/// </summary>
	public const int RPC_X_SS_CHAR_TRANS_Int16_FILE = 1774;

	/// <summary>
	///     A null context handle was passed from the client to the host during a remote procedure call.
	/// </summary>
	public const int RPC_X_SS_IN_NULL_CONTEXT = 1775;

	/// <summary>
	///     The context handle changed during a remote procedure call.
	/// </summary>
	public const int RPC_X_SS_CONTEXT_DAMAGED = 1777;

	/// <summary>
	///     The binding handles passed to a remote procedure call do not match.
	/// </summary>
	public const int RPC_X_SS_HANDLES_MISMATCH = 1778;

	/// <summary>
	///     The stub is unable to get the remote procedure call handle.
	/// </summary>
	public const int RPC_X_SS_CANNOT_GET_CALL_HANDLE = 1779;

	/// <summary>
	///     A null reference pointer was passed to the stub.
	/// </summary>
	public const int RPC_X_NULL_REF_POINTER = 1780;

	/// <summary>
	///     The enumeration value is out of range.
	/// </summary>
	public const int RPC_X_ENUM_VALUE_OUT_OF_RANGE = 1781;

	/// <summary>
	///     The byte count is too small.
	/// </summary>
	public const int RPC_X_BYTE_COUNT_TOO_SMALL = 1782;

	/// <summary>
	///     The stub received bad data.
	/// </summary>
	public const int RPC_X_BAD_STUB_DATA = 1783;

	/// <summary>
	///     The supplied user buffer is not valid for the requested operation.
	/// </summary>
	public const int ERROR_INVALID_USER_BUFFER = 1784;

	/// <summary>
	///     The disk media is not recognized. It may not be formatted.
	/// </summary>
	public const int ERROR_UNRECOGNIZED_MEDIA = 1785;

	/// <summary>
	///     The workstation does not have a trust secret.
	/// </summary>
	public const int ERROR_NO_TRUST_LSA_SECRET = 1786;

	/// <summary>
	///     The security database on the server does not have a computer account for this workstation trust relationship.
	/// </summary>
	public const int ERROR_NO_TRUST_SAM_ACCOUNT = 1787;

	/// <summary>
	///     The trust relationship between the primary domain and the trusted domain failed.
	/// </summary>
	public const int ERROR_TRUSTED_DOMAIN_FAILURE = 1788;

	/// <summary>
	///     The trust relationship between this workstation and the primary domain failed.
	/// </summary>
	public const int ERROR_TRUSTED_RELATIONSHIP_FAILURE = 1789;

	/// <summary>
	///     The network logon failed.
	/// </summary>
	public const int ERROR_TRUST_FAILURE = 1790;

	/// <summary>
	///     A remote procedure call is already in progress for this thread.
	/// </summary>
	public const int RPC_S_CALL_IN_PROGRESS = 1791;

	/// <summary>
	///     An attempt was made to logon, but the network logon service was not started.
	/// </summary>
	public const int ERROR_NETLOGON_NOT_STARTED = 1792;

	/// <summary>
	///     The user's account has expired.
	/// </summary>
	public const int ERROR_ACCOUNT_EXPIRED = 1793;

	/// <summary>
	///     The redirector is in use and cannot be unloaded.
	/// </summary>
	public const int ERROR_REDIRECTOR_HAS_OPEN_HANDLES = 1794;

	/// <summary>
	///     The specified printer driver is already installed.
	/// </summary>
	public const int ERROR_PRINTER_DRIVER_ALREADY_INSTALLED = 1795;

	/// <summary>
	///     The specified port is unknown.
	/// </summary>
	public const int ERROR_UNKNOWN_PORT = 1796;

	/// <summary>
	///     The printer driver is unknown.
	/// </summary>
	public const int ERROR_UNKNOWN_PRINTER_DRIVER = 1797;

	/// <summary>
	///     The print processor is unknown.
	/// </summary>
	public const int ERROR_UNKNOWN_PRINTPROCESSOR = 1798;

	/// <summary>
	///     The specified separator file is invalid.
	/// </summary>
	public const int ERROR_INVALID_SEPARATOR_FILE = 1799;

	/// <summary>
	///     The specified priority is invalid.
	/// </summary>
	public const int ERROR_INVALID_PRIORITY = 1800;

	/// <summary>
	///     The printer name is invalid.
	/// </summary>
	public const int ERROR_INVALID_PRINTER_NAME = 1801;

	/// <summary>
	///     The printer already exists.
	/// </summary>
	public const int ERROR_PRINTER_ALREADY_EXISTS = 1802;

	/// <summary>
	///     The printer command is invalid.
	/// </summary>
	public const int ERROR_INVALID_PRINTER_COMMAND = 1803;

	/// <summary>
	///     The specified datatype is invalid.
	/// </summary>
	public const int ERROR_INVALID_DATATYPE = 1804;

	/// <summary>
	///     The environment specified is invalid.
	/// </summary>
	public const int ERROR_INVALID_ENVIRONMENT = 1805;

	/// <summary>
	///     There are no more bindings.
	/// </summary>
	public const int RPC_S_NO_MORE_BINDINGS = 1806;

	/// <summary>
	///     The account used is an interdomain trust account. Use your global user account or local user account to access this
	///     server.
	/// </summary>
	public const int ERROR_NOLOGON_INTERDOMAIN_TRUST_ACCOUNT = 1807;

	/// <summary>
	///     The account used is a computer account. Use your global user account or local user account to access this server.
	/// </summary>
	public const int ERROR_NOLOGON_WORKSTATION_TRUST_ACCOUNT = 1808;

	/// <summary>
	///     The account used is a server trust account. Use your global user account or local user account to access this
	///     server.
	/// </summary>
	public const int ERROR_NOLOGON_SERVER_TRUST_ACCOUNT = 1809;

	/// <summary>
	///     The name or security ID (SID) of the domain specified is inconsistent with the trust information for that domain.
	/// </summary>
	public const int ERROR_DOMAIN_TRUST_INCONSISTENT = 1810;

	/// <summary>
	///     The server is in use and cannot be unloaded.
	/// </summary>
	public const int ERROR_SERVER_HAS_OPEN_HANDLES = 1811;

	/// <summary>
	///     The specified image file did not contain a resource section.
	/// </summary>
	public const int ERROR_RESOURCE_DATA_NOT_FOUND = 1812;

	/// <summary>
	///     The specified resource type cannot be found in the image file.
	/// </summary>
	public const int ERROR_RESOURCE_TYPE_NOT_FOUND = 1813;

	/// <summary>
	///     The specified resource name cannot be found in the image file.
	/// </summary>
	public const int ERROR_RESOURCE_NAME_NOT_FOUND = 1814;

	/// <summary>
	///     The specified resource language ID cannot be found in the image file.
	/// </summary>
	public const int ERROR_RESOURCE_LANG_NOT_FOUND = 1815;

	/// <summary>
	///     Not enough quota is available to process this command.
	/// </summary>
	public const int ERROR_NOT_ENOUGH_QUOTA = 1816;

	/// <summary>
	///     No interfaces have been registered.
	/// </summary>
	public const int RPC_S_NO_INTERFACES = 1817;

	/// <summary>
	///     The remote procedure call was cancelled.
	/// </summary>
	public const int RPC_S_CALL_CANCELLED = 1818;

	/// <summary>
	///     The binding handle does not contain all required information.
	/// </summary>
	public const int RPC_S_BINDING_INCOMPLETE = 1819;

	/// <summary>
	///     A communications failure occurred during a remote procedure call.
	/// </summary>
	public const int RPC_S_COMM_FAILURE = 1820;

	/// <summary>
	///     The requested authentication level is not supported.
	/// </summary>
	public const int RPC_S_UNSUPPORTED_AUTHN_LEVEL = 1821;

	/// <summary>
	///     No principal name registered.
	/// </summary>
	public const int RPC_S_NO_PRINC_NAME = 1822;

	/// <summary>
	///     The error specified is not a valid Windows RPC error code.
	/// </summary>
	public const int RPC_S_NOT_RPC_ERROR = 1823;

	/// <summary>
	///     A UUID that is valid only on this computer has been allocated.
	/// </summary>
	public const int RPC_S_UUID_LOCAL_ONLY = 1824;

	/// <summary>
	///     A security package specific error occurred.
	/// </summary>
	public const int RPC_S_SEC_PKG_ERROR = 1825;

	/// <summary>
	///     Thread is not canceled.
	/// </summary>
	public const int RPC_S_NOT_CANCELLED = 1826;

	/// <summary>
	///     Invalid operation on the encoding/decoding handle.
	/// </summary>
	public const int RPC_X_INVALID_ES_ACTION = 1827;

	/// <summary>
	///     Incompatible version of the serializing package.
	/// </summary>
	public const int RPC_X_WRONG_ES_VERSION = 1828;

	/// <summary>
	///     Incompatible version of the RPC stub.
	/// </summary>
	public const int RPC_X_WRONG_STUB_VERSION = 1829;

	/// <summary>
	///     The RPC pipe object is invalid or corrupted.
	/// </summary>
	public const int RPC_X_INVALID_PIPE_OBJECT = 1830;

	/// <summary>
	///     An invalid operation was attempted on an RPC pipe object.
	/// </summary>
	public const int RPC_X_WRONG_PIPE_ORDER = 1831;

	/// <summary>
	///     Unsupported RPC pipe version.
	/// </summary>
	public const int RPC_X_WRONG_PIPE_VERSION = 1832;

	/// <summary>
	///     The group member was not found.
	/// </summary>
	public const int RPC_S_GROUP_MEMBER_NOT_FOUND = 1898;

	/// <summary>
	///     The endpoint mapper database entry could not be created.
	/// </summary>
	public const int EPT_S_CANT_CREATE = 1899;

	/// <summary>
	///     The object universal unique identifier (UUID) is the nil UUID.
	/// </summary>
	public const int RPC_S_INVALID_OBJECT = 1900;

	/// <summary>
	///     The specified time is invalid.
	/// </summary>
	public const int ERROR_INVALID_TIME = 1901;

	/// <summary>
	///     The specified form name is invalid.
	/// </summary>
	public const int ERROR_INVALID_FORM_NAME = 1902;

	/// <summary>
	///     The specified form size is invalid.
	/// </summary>
	public const int ERROR_INVALID_FORM_SIZE = 1903;

	/// <summary>
	///     The specified printer handle is already being waited on
	/// </summary>
	public const int ERROR_ALREADY_WAITING = 1904;

	/// <summary>
	///     The specified printer has been deleted.
	/// </summary>
	public const int ERROR_PRINTER_DELETED = 1905;

	/// <summary>
	///     The state of the printer is invalid.
	/// </summary>
	public const int ERROR_INVALID_PRINTER_STATE = 1906;

	/// <summary>
	///     The user's password must be changed before logging on the first time.
	/// </summary>
	public const int ERROR_PASSUInt16_MUST_CHANGE = 1907;

	/// <summary>
	///     Could not find the domain controller for this domain.
	/// </summary>
	public const int ERROR_DOMAIN_CONTROLLER_NOT_FOUND = 1908;

	/// <summary>
	///     The referenced account is currently locked out and may not be logged on to.
	/// </summary>
	public const int ERROR_ACCOUNT_LOCKED_OUT = 1909;

	/// <summary>
	///     The object exporter specified was not found.
	/// </summary>
	public const int OR_INVALID_OXID = 1910;

	/// <summary>
	///     The object specified was not found.
	/// </summary>
	public const int OR_INVALID_OID = 1911;

	/// <summary>
	///     The object resolver set specified was not found.
	/// </summary>
	public const int OR_INVALID_SET = 1912;

	/// <summary>
	///     Some data remains to be sent in the request buffer.
	/// </summary>
	public const int RPC_S_SEND_INCOMPLETE = 1913;

	/// <summary>
	///     Invalid asynchronous remote procedure call handle.
	/// </summary>
	public const int RPC_S_INVALID_ASYNC_HANDLE = 1914;

	/// <summary>
	///     Invalid asynchronous RPC call handle for this operation.
	/// </summary>
	public const int RPC_S_INVALID_ASYNC_CALL = 1915;

	/// <summary>
	///     The RPC pipe object has already been closed.
	/// </summary>
	public const int RPC_X_PIPE_CLOSED = 1916;

	/// <summary>
	///     The RPC call completed before all pipes were processed.
	/// </summary>
	public const int RPC_X_PIPE_DISCIPLINE_ERROR = 1917;

	/// <summary>
	///     No more data is available from the RPC pipe.
	/// </summary>
	public const int RPC_X_PIPE_EMPTY = 1918;

	/// <summary>
	///     No site name is available for this machine.
	/// </summary>
	public const int ERROR_NO_SITENAME = 1919;

	/// <summary>
	///     The file can not be accessed by the system.
	/// </summary>
	public const int ERROR_CANT_ACCESS_FILE = 1920;

	/// <summary>
	///     The name of the file cannot be resolved by the system.
	/// </summary>
	public const int ERROR_CANT_RESOLVE_FILENAME = 1921;

	/// <summary>
	///     The entry is not of the expected type.
	/// </summary>
	public const int RPC_S_ENTRY_TYPE_MISMATCH = 1922;

	/// <summary>
	///     Not all object UUIDs could be exported to the specified entry.
	/// </summary>
	public const int RPC_S_NOT_ALL_OBJS_EXPORTED = 1923;

	/// <summary>
	///     Interface could not be exported to the specified entry.
	/// </summary>
	public const int RPC_S_INTERFACE_NOT_EXPORTED = 1924;

	/// <summary>
	///     The specified profile entry could not be added.
	/// </summary>
	public const int RPC_S_PROFILE_NOT_ADDED = 1925;

	/// <summary>
	///     The specified profile element could not be added.
	/// </summary>
	public const int RPC_S_PRF_ELT_NOT_ADDED = 1926;

	/// <summary>
	///     The specified profile element could not be removed.
	/// </summary>
	public const int RPC_S_PRF_ELT_NOT_REMOVED = 1927;

	/// <summary>
	///     The group element could not be added.
	/// </summary>
	public const int RPC_S_GRP_ELT_NOT_ADDED = 1928;

	/// <summary>
	///     The group element could not be removed.
	/// </summary>
	public const int RPC_S_GRP_ELT_NOT_REMOVED = 1929;

	/// <summary>
	///     The printer driver is not compatible with a policy enabled on your computer that blocks NT 4.0 drivers.
	/// </summary>
	public const int ERROR_KM_DRIVER_BLOCKED = 1930;

	/// <summary>
	///     The context has expired and can no longer be used.
	/// </summary>
	public const int ERROR_CONTEXT_EXPIRED = 1931;

	/// <summary>
	///     No information avialable.
	/// </summary>
	public const int ERROR_PER_USER_TRUST_QUOTA_EXCEEDED = 1932;

	/// <summary>
	///     No information avialable.
	/// </summary>
	public const int ERROR_ALL_USER_TRUST_QUOTA_EXCEEDED = 1933;

	/// <summary>
	///     No information avialable.
	/// </summary>
	public const int ERROR_USER_DELETE_TRUST_QUOTA_EXCEEDED = 1934;

	/// <summary>
	///     No information avialable.
	/// </summary>
	public const int ERROR_AUTHENTICATION_FIREWALL_FAILED = 1935;

	/// <summary>
	///     No information avialable.
	/// </summary>
	public const int ERROR_REMOTE_PRINT_CONNECTIONS_BLOCKED = 1936;

	/// <summary>
	///     The pixel format is invalid.
	/// </summary>
	public const int ERROR_INVALID_PIXEL_FORMAT = 2000;

	/// <summary>
	///     The specified driver is invalid.
	/// </summary>
	public const int ERROR_BAD_DRIVER = 2001;

	/// <summary>
	///     The window style or class attribute is invalid for this operation.
	/// </summary>
	public const int ERROR_INVALID_WINDOW_STYLE = 2002;

	/// <summary>
	///     The requested metafile operation is not supported.
	/// </summary>
	public const int ERROR_METAFILE_NOT_SUPPORTED = 2003;

	/// <summary>
	///     The requested transformation operation is not supported.
	/// </summary>
	public const int ERROR_TRANSFORM_NOT_SUPPORTED = 2004;

	/// <summary>
	///     The requested clipping operation is not supported.
	/// </summary>
	public const int ERROR_CLIPPING_NOT_SUPPORTED = 2005;

	/// <summary>
	///     The specified color management module is invalid.
	/// </summary>
	public const int ERROR_INVALID_CMM = 2010;

	/// <summary>
	///     The specified color profile is invalid.
	/// </summary>
	public const int ERROR_INVALID_PROFILE = 2011;

	/// <summary>
	///     The specified tag was not found.
	/// </summary>
	public const int ERROR_TAG_NOT_FOUND = 2012;

	/// <summary>
	///     A required tag is not present.
	/// </summary>
	public const int ERROR_TAG_NOT_PRESENT = 2013;

	/// <summary>
	///     The specified tag is already present.
	/// </summary>
	public const int ERROR_DUPLICATE_TAG = 2014;

	/// <summary>
	///     The specified color profile is not associated with any device.
	/// </summary>
	public const int ERROR_PROFILE_NOT_ASSOCIATED_WITH_DEVICE = 2015;

	/// <summary>
	///     The specified color profile was not found.
	/// </summary>
	public const int ERROR_PROFILE_NOT_FOUND = 2016;

	/// <summary>
	///     The specified color space is invalid.
	/// </summary>
	public const int ERROR_INVALID_COLORSPACE = 2017;

	/// <summary>
	///     Image Color Management is not enabled.
	/// </summary>
	public const int ERROR_ICM_NOT_ENABLED = 2018;

	/// <summary>
	///     There was an error while deleting the color transform.
	/// </summary>
	public const int ERROR_DELETING_ICM_XFORM = 2019;

	/// <summary>
	///     The specified color transform is invalid.
	/// </summary>
	public const int ERROR_INVALID_TRANSFORM = 2020;

	/// <summary>
	///     The specified transform does not match the bitmap's color space.
	/// </summary>
	public const int ERROR_COLORSPACE_MISMATCH = 2021;

	/// <summary>
	///     The specified named color index is not present in the profile.
	/// </summary>
	public const int ERROR_INVALID_COLORINDEX = 2022;

	/// <summary>
	///     The network connection was made successfully, but the user had to be prompted for a password other than the one
	///     originally specified.
	/// </summary>
	public const int ERROR_CONNECTED_OTHER_PASSUInt16 = 2108;

	/// <summary>
	///     The network connection was made successfully using default credentials.
	/// </summary>
	public const int ERROR_CONNECTED_OTHER_PASSUInt16_DEFAULT = 2109;

	/// <summary>
	///     The specified username is invalid.
	/// </summary>
	public const int ERROR_BAD_USERNAME = 2202;

	/// <summary>
	///     This network connection does not exist.
	/// </summary>
	public const int ERROR_NOT_CONNECTED = 2250;

	/// <summary>
	///     This network connection has files open or requests pending.
	/// </summary>
	public const int ERROR_OPEN_FILES = 2401;

	/// <summary>
	///     Active connections still exist.
	/// </summary>
	public const int ERROR_ACTIVE_CONNECTIONS = 2402;

	/// <summary>
	///     The device is in use by an active process and cannot be disconnected.
	/// </summary>
	public const int ERROR_DEVICE_IN_USE = 2404;

	/// <summary>
	///     The specified print monitor is unknown.
	/// </summary>
	public const int ERROR_UNKNOWN_PRINT_MONITOR = 3000;

	/// <summary>
	///     The specified printer driver is currently in use.
	/// </summary>
	public const int ERROR_PRINTER_DRIVER_IN_USE = 3001;

	/// <summary>
	///     The spool file was not found.
	/// </summary>
	public const int ERROR_SPOOL_FILE_NOT_FOUND = 3002;

	/// <summary>
	///     A StartDocPrinter call was not issued.
	/// </summary>
	public const int ERROR_SPL_NO_STARTDOC = 3003;

	/// <summary>
	///     An AddJob call was not issued.
	/// </summary>
	public const int ERROR_SPL_NO_ADDJOB = 3004;

	/// <summary>
	///     The specified print processor has already been installed.
	/// </summary>
	public const int ERROR_PRINT_PROCESSOR_ALREADY_INSTALLED = 3005;

	/// <summary>
	///     The specified print monitor has already been installed.
	/// </summary>
	public const int ERROR_PRINT_MONITOR_ALREADY_INSTALLED = 3006;

	/// <summary>
	///     The specified print monitor does not have the required functions.
	/// </summary>
	public const int ERROR_INVALID_PRINT_MONITOR = 3007;

	/// <summary>
	///     The specified print monitor is currently in use.
	/// </summary>
	public const int ERROR_PRINT_MONITOR_IN_USE = 3008;

	/// <summary>
	///     The requested operation is not allowed when there are jobs queued to the printer.
	/// </summary>
	public const int ERROR_PRINTER_HAS_JOBS_QUEUED = 3009;

	/// <summary>
	///     The requested operation is successful. Changes will not be effective until the system is rebooted.
	/// </summary>
	public const int ERROR_SUCCESS_REBOOT_REQUIRED = 3010;

	/// <summary>
	///     The requested operation is successful. Changes will not be effective until the service is restarted.
	/// </summary>
	public const int ERROR_SUCCESS_RESTART_REQUIRED = 3011;

	/// <summary>
	///     No printers were found.
	/// </summary>
	public const int ERROR_PRINTER_NOT_FOUND = 3012;

	/// <summary>
	///     The printer driver is known to be unreliable.
	/// </summary>
	public const int ERROR_PRINTER_DRIVER_WARNED = 3013;

	/// <summary>
	///     The printer driver is known to harm the system.
	/// </summary>
	public const int ERROR_PRINTER_DRIVER_BLOCKED = 3014;

	/// <summary>
	///     WINS encountered an error while processing the command.
	/// </summary>
	public const int ERROR_WINS_INTERNAL = 4000;

	/// <summary>
	///     The local WINS can not be deleted.
	/// </summary>
	public const int ERROR_CAN_NOT_DEL_LOCAL_WINS = 4001;

	/// <summary>
	///     The importation from the file failed.
	/// </summary>
	public const int ERROR_STATIC_INIT = 4002;

	/// <summary>
	///     The backup failed. Was a full backup done before?
	/// </summary>
	public const int ERROR_INC_BACKUP = 4003;

	/// <summary>
	///     The backup failed. Check the directory to which you are backing the database.
	/// </summary>
	public const int ERROR_FULL_BACKUP = 4004;

	/// <summary>
	///     The name does not exist in the WINS database.
	/// </summary>
	public const int ERROR_REC_NON_EXISTENT = 4005;

	/// <summary>
	///     Replication with a nonconfigured partner is not allowed.
	/// </summary>
	public const int ERROR_RPL_NOT_ALLOWED = 4006;

	/// <summary>
	///     The DHCP client has obtained an IP address that is already in use on the network. The local interface will be
	///     disabled until the DHCP client can obtain a new address.
	/// </summary>
	public const int ERROR_DHCP_ADDRESS_CONFLICT = 4100;

	/// <summary>
	///     The GUID passed was not recognized as valid by a WMI data provider.
	/// </summary>
	public const int ERROR_WMI_GUID_NOT_FOUND = 4200;

	/// <summary>
	///     The instance name passed was not recognized as valid by a WMI data provider.
	/// </summary>
	public const int ERROR_WMI_INSTANCE_NOT_FOUND = 4201;

	/// <summary>
	///     The data item ID passed was not recognized as valid by a WMI data provider.
	/// </summary>
	public const int ERROR_WMI_ITEMID_NOT_FOUND = 4202;

	/// <summary>
	///     The WMI request could not be completed and should be retried.
	/// </summary>
	public const int ERROR_WMI_TRY_AGAIN = 4203;

	/// <summary>
	///     The WMI data provider could not be located.
	/// </summary>
	public const int ERROR_WMI_DP_NOT_FOUND = 4204;

	/// <summary>
	///     The WMI data provider references an instance set that has not been registered.
	/// </summary>
	public const int ERROR_WMI_UNRESOLVED_INSTANCE_REF = 4205;

	/// <summary>
	///     The WMI data block or event notification has already been enabled.
	/// </summary>
	public const int ERROR_WMI_ALREADY_ENABLED = 4206;

	/// <summary>
	///     The WMI data block is no longer available.
	/// </summary>
	public const int ERROR_WMI_GUID_DISCONNECTED = 4207;

	/// <summary>
	///     The WMI data service is not available.
	/// </summary>
	public const int ERROR_WMI_SERVER_UNAVAILABLE = 4208;

	/// <summary>
	///     The WMI data provider failed to carry out the request.
	/// </summary>
	public const int ERROR_WMI_DP_FAILED = 4209;

	/// <summary>
	///     The WMI MOF information is not valid.
	/// </summary>
	public const int ERROR_WMI_INVALID_MOF = 4210;

	/// <summary>
	///     The WMI registration information is not valid.
	/// </summary>
	public const int ERROR_WMI_INVALID_REGINFO = 4211;

	/// <summary>
	///     The WMI data block or event notification has already been disabled.
	/// </summary>
	public const int ERROR_WMI_ALREADY_DISABLED = 4212;

	/// <summary>
	///     The WMI data item or data block is read only.
	/// </summary>
	public const int ERROR_WMI_READ_ONLY = 4213;

	/// <summary>
	///     The WMI data item or data block could not be changed.
	/// </summary>
	public const int ERROR_WMI_SET_FAILURE = 4214;

	/// <summary>
	///     The media identifier does not represent a valid medium.
	/// </summary>
	public const int ERROR_INVALID_MEDIA = 4300;

	/// <summary>
	///     The library identifier does not represent a valid library.
	/// </summary>
	public const int ERROR_INVALID_LIBRARY = 4301;

	/// <summary>
	///     The media pool identifier does not represent a valid media pool.
	/// </summary>
	public const int ERROR_INVALID_MEDIA_POOL = 4302;

	/// <summary>
	///     The drive and medium are not compatible or exist in different libraries.
	/// </summary>
	public const int ERROR_DRIVE_MEDIA_MISMATCH = 4303;

	/// <summary>
	///     The medium currently exists in an offline library and must be online to perform this operation.
	/// </summary>
	public const int ERROR_MEDIA_OFFLINE = 4304;

	/// <summary>
	///     The operation cannot be performed on an offline library.
	/// </summary>
	public const int ERROR_LIBRARY_OFFLINE = 4305;

	/// <summary>
	///     The library, drive, or media pool is empty.
	/// </summary>
	public const int ERROR_EMPTY = 4306;

	/// <summary>
	///     The library, drive, or media pool must be empty to perform this operation.
	/// </summary>
	public const int ERROR_NOT_EMPTY = 4307;

	/// <summary>
	///     No media is currently available in this media pool or library.
	/// </summary>
	public const int ERROR_MEDIA_UNAVAILABLE = 4308;

	/// <summary>
	///     A resource required for this operation is disabled.
	/// </summary>
	public const int ERROR_RESOURCE_DISABLED = 4309;

	/// <summary>
	///     The media identifier does not represent a valid cleaner.
	/// </summary>
	public const int ERROR_INVALID_CLEANER = 4310;

	/// <summary>
	///     The drive cannot be cleaned or does not support cleaning.
	/// </summary>
	public const int ERROR_UNABLE_TO_CLEAN = 4311;

	/// <summary>
	///     The object identifier does not represent a valid object.
	/// </summary>
	public const int ERROR_OBJECT_NOT_FOUND = 4312;

	/// <summary>
	///     Unable to read from or write to the database.
	/// </summary>
	public const int ERROR_DATABASE_FAILURE = 4313;

	/// <summary>
	///     The database is full.
	/// </summary>
	public const int ERROR_DATABASE_FULL = 4314;

	/// <summary>
	///     The medium is not compatible with the device or media pool.
	/// </summary>
	public const int ERROR_MEDIA_INCOMPATIBLE = 4315;

	/// <summary>
	///     The resource required for this operation does not exist.
	/// </summary>
	public const int ERROR_RESOURCE_NOT_PRESENT = 4316;

	/// <summary>
	///     The operation identifier is not valid.
	/// </summary>
	public const int ERROR_INVALID_OPERATION = 4317;

	/// <summary>
	///     The media is not mounted or ready for use.
	/// </summary>
	public const int ERROR_MEDIA_NOT_AVAILABLE = 4318;

	/// <summary>
	///     The device is not ready for use.
	/// </summary>
	public const int ERROR_DEVICE_NOT_AVAILABLE = 4319;

	/// <summary>
	///     The operator or administrator has refused the request.
	/// </summary>
	public const int ERROR_REQUEST_REFUSED = 4320;

	/// <summary>
	///     The drive identifier does not represent a valid drive.
	/// </summary>
	public const int ERROR_INVALID_DRIVE_OBJECT = 4321;

	/// <summary>
	///     Library is full.  No slot is available for use.
	/// </summary>
	public const int ERROR_LIBRARY_FULL = 4322;

	/// <summary>
	///     The transport cannot access the medium.
	/// </summary>
	public const int ERROR_MEDIUM_NOT_ACCESSIBLE = 4323;

	/// <summary>
	///     Unable to load the medium into the drive.
	/// </summary>
	public const int ERROR_UNABLE_TO_LOAD_MEDIUM = 4324;

	/// <summary>
	///     Unable to retrieve the drive status.
	/// </summary>
	public const int ERROR_UNABLE_TO_INVENTORY_DRIVE = 4325;

	/// <summary>
	///     Unable to retrieve the slot status.
	/// </summary>
	public const int ERROR_UNABLE_TO_INVENTORY_SLOT = 4326;

	/// <summary>
	///     Unable to retrieve status about the transport.
	/// </summary>
	public const int ERROR_UNABLE_TO_INVENTORY_TRANSPORT = 4327;

	/// <summary>
	///     Cannot use the transport because it is already in use.
	/// </summary>
	public const int ERROR_TRANSPORT_FULL = 4328;

	/// <summary>
	///     Unable to open or close the inject/eject port.
	/// </summary>
	public const int ERROR_CONTROLLING_IEPORT = 4329;

	/// <summary>
	///     Unable to eject the medium because it is in a drive.
	/// </summary>
	public const int ERROR_UNABLE_TO_EJECT_MOUNTED_MEDIA = 4330;

	/// <summary>
	///     A cleaner slot is already reserved.
	/// </summary>
	public const int ERROR_CLEANER_SLOT_SET = 4331;

	/// <summary>
	///     A cleaner slot is not reserved.
	/// </summary>
	public const int ERROR_CLEANER_SLOT_NOT_SET = 4332;

	/// <summary>
	///     The cleaner cartridge has performed the maximum number of drive cleanings.
	/// </summary>
	public const int ERROR_CLEANER_CARTRIDGE_SPENT = 4333;

	/// <summary>
	///     Unexpected on-medium identifier.
	/// </summary>
	public const int ERROR_UNEXPECTED_OMID = 4334;

	/// <summary>
	///     The last remaining item in this group or resource cannot be deleted.
	/// </summary>
	public const int ERROR_CANT_DELETE_LAST_ITEM = 4335;

	/// <summary>
	///     The message provided exceeds the maximum size allowed for this parameter.
	/// </summary>
	public const int ERROR_MESSAGE_EXCEEDS_MAX_SIZE = 4336;

	/// <summary>
	///     The volume contains system or paging files.
	/// </summary>
	public const int ERROR_VOLUME_CONTAINS_SYS_FILES = 4337;

	/// <summary>
	///     The media type cannot be removed from this library since at least one drive in the library reports it can support
	///     this media type.
	/// </summary>
	public const int ERROR_INDIGENOUS_TYPE = 4338;

	/// <summary>
	///     This offline media cannot be mounted on this system since no enabled drives are present which can be used.
	/// </summary>
	public const int ERROR_NO_SUPPORTING_DRIVES = 4339;

	/// <summary>
	///     A cleaner cartridge is present in the tape library.
	/// </summary>
	public const int ERROR_CLEANER_CARTRIDGE_INSTALLED = 4340;

	/// <summary>
	///     The remote storage service was not able to recall the file.
	/// </summary>
	public const int ERROR_FILE_OFFLINE = 4350;

	/// <summary>
	///     The remote storage service is not operational at this time.
	/// </summary>
	public const int ERROR_REMOTE_STORAGE_NOT_ACTIVE = 4351;

	/// <summary>
	///     The remote storage service encountered a media error.
	/// </summary>
	public const int ERROR_REMOTE_STORAGE_MEDIA_ERROR = 4352;

	/// <summary>
	///     The file or directory is not a reparse point.
	/// </summary>
	public const int ERROR_NOT_A_REPARSE_POINT = 4390;

	/// <summary>
	///     The reparse point attribute cannot be set because it conflicts with an existing attribute.
	/// </summary>
	public const int ERROR_REPARSE_ATTRIBUTE_CONFLICT = 4391;

	/// <summary>
	///     The data present in the reparse point buffer is invalid.
	/// </summary>
	public const int ERROR_INVALID_REPARSE_DATA = 4392;

	/// <summary>
	///     The tag present in the reparse point buffer is invalid.
	/// </summary>
	public const int ERROR_REPARSE_TAG_INVALID = 4393;

	/// <summary>
	///     There is a mismatch between the tag specified in the request and the tag present in the reparse point.
	/// </summary>
	public const int ERROR_REPARSE_TAG_MISMATCH = 4394;

	/// <summary>
	///     Single Instance Storage is not available on this volume.
	/// </summary>
	public const int ERROR_VOLUME_NOT_SIS_ENABLED = 4500;

	/// <summary>
	///     The cluster resource cannot be moved to another group because other resources are dependent on it.
	/// </summary>
	public const int ERROR_DEPENDENT_RESOURCE_EXISTS = 5001;

	/// <summary>
	///     The cluster resource dependency cannot be found.
	/// </summary>
	public const int ERROR_DEPENDENCY_NOT_FOUND = 5002;

	/// <summary>
	///     The cluster resource cannot be made dependent on the specified resource because it is already dependent.
	/// </summary>
	public const int ERROR_DEPENDENCY_ALREADY_EXISTS = 5003;

	/// <summary>
	///     The cluster resource is not online.
	/// </summary>
	public const int ERROR_RESOURCE_NOT_ONLINE = 5004;

	/// <summary>
	///     A cluster node is not available for this operation.
	/// </summary>
	public const int ERROR_HOST_NODE_NOT_AVAILABLE = 5005;

	/// <summary>
	///     The cluster resource is not available.
	/// </summary>
	public const int ERROR_RESOURCE_NOT_AVAILABLE = 5006;

	/// <summary>
	///     The cluster resource could not be found.
	/// </summary>
	public const int ERROR_RESOURCE_NOT_FOUND = 5007;

	/// <summary>
	///     The cluster is being shut down.
	/// </summary>
	public const int ERROR_SHUTDOWN_CLUSTER = 5008;

	/// <summary>
	///     A cluster node cannot be evicted from the cluster unless the node is down or it is the last node.
	/// </summary>
	public const int ERROR_CANT_EVICT_ACTIVE_NODE = 5009;

	/// <summary>
	///     The object already exists.
	/// </summary>
	public const int ERROR_OBJECT_ALREADY_EXISTS = 5010;

	/// <summary>
	///     The object is already in the list.
	/// </summary>
	public const int ERROR_OBJECT_IN_LIST = 5011;

	/// <summary>
	///     The cluster group is not available for any new requests.
	/// </summary>
	public const int ERROR_GROUP_NOT_AVAILABLE = 5012;

	/// <summary>
	///     The cluster group could not be found.
	/// </summary>
	public const int ERROR_GROUP_NOT_FOUND = 5013;

	/// <summary>
	///     The operation could not be completed because the cluster group is not online.
	/// </summary>
	public const int ERROR_GROUP_NOT_ONLINE = 5014;

	/// <summary>
	///     The cluster node is not the owner of the resource.
	/// </summary>
	public const int ERROR_HOST_NODE_NOT_RESOURCE_OWNER = 5015;

	/// <summary>
	///     The cluster node is not the owner of the group.
	/// </summary>
	public const int ERROR_HOST_NODE_NOT_GROUP_OWNER = 5016;

	/// <summary>
	///     The cluster resource could not be created in the specified resource monitor.
	/// </summary>
	public const int ERROR_RESMON_CREATE_FAILED = 5017;

	/// <summary>
	///     The cluster resource could not be brought online by the resource monitor.
	/// </summary>
	public const int ERROR_RESMON_ONLINE_FAILED = 5018;

	/// <summary>
	///     The operation could not be completed because the cluster resource is online.
	/// </summary>
	public const int ERROR_RESOURCE_ONLINE = 5019;

	/// <summary>
	///     The cluster resource could not be deleted or brought offline because it is the quorum resource.
	/// </summary>
	public const int ERROR_QUORUM_RESOURCE = 5020;

	/// <summary>
	///     The cluster could not make the specified resource a quorum resource because it is not capable of being a quorum
	///     resource.
	/// </summary>
	public const int ERROR_NOT_QUORUM_CAPABLE = 5021;

	/// <summary>
	///     The cluster software is shutting down.
	/// </summary>
	public const int ERROR_CLUSTER_SHUTTING_DOWN = 5022;

	/// <summary>
	///     The group or resource is not in the correct state to perform the requested operation.
	/// </summary>
	public const int ERROR_INVALID_STATE = 5023;

	/// <summary>
	///     The properties were stored but not all changes will take effect until the next time the resource is brought online.
	/// </summary>
	public const int ERROR_RESOURCE_PROPERTIES_STORED = 5024;

	/// <summary>
	///     The cluster could not make the specified resource a quorum resource because it does not belong to a shared storage
	///     class.
	/// </summary>
	public const int ERROR_NOT_QUORUM_CLASS = 5025;

	/// <summary>
	///     The cluster resource could not be deleted since it is a core resource.
	/// </summary>
	public const int ERROR_CORE_RESOURCE = 5026;

	/// <summary>
	///     The quorum resource failed to come online.
	/// </summary>
	public const int ERROR_QUORUM_RESOURCE_ONLINE_FAILED = 5027;

	/// <summary>
	///     The quorum log could not be created or mounted successfully.
	/// </summary>
	public const int ERROR_QUORUMLOG_OPEN_FAILED = 5028;

	/// <summary>
	///     The cluster log is corrupt.
	/// </summary>
	public const int ERROR_CLUSTERLOG_CORRUPT = 5029;

	/// <summary>
	///     The record could not be written to the cluster log since it exceeds the maximum size.
	/// </summary>
	public const int ERROR_CLUSTERLOG_RECORD_EXCEEDS_MAXSIZE = 5030;

	/// <summary>
	///     The cluster log exceeds its maximum size.
	/// </summary>
	public const int ERROR_CLUSTERLOG_EXCEEDS_MAXSIZE = 5031;

	/// <summary>
	///     No checkpoint record was found in the cluster log.
	/// </summary>
	public const int ERROR_CLUSTERLOG_CHKPOINT_NOT_FOUND = 5032;

	/// <summary>
	///     The minimum required disk space needed for logging is not available.
	/// </summary>
	public const int ERROR_CLUSTERLOG_NOT_ENOUGH_SPACE = 5033;

	/// <summary>
	///     The cluster node failed to take control of the quorum resource because the resource is owned by another active
	///     node.
	/// </summary>
	public const int ERROR_QUORUM_OWNER_ALIVE = 5034;

	/// <summary>
	///     A cluster network is not available for this operation.
	/// </summary>
	public const int ERROR_NETWORK_NOT_AVAILABLE = 5035;

	/// <summary>
	///     A cluster node is not available for this operation.
	/// </summary>
	public const int ERROR_NODE_NOT_AVAILABLE = 5036;

	/// <summary>
	///     All cluster nodes must be running to perform this operation.
	/// </summary>
	public const int ERROR_ALL_NODES_NOT_AVAILABLE = 5037;

	/// <summary>
	///     A cluster resource failed.
	/// </summary>
	public const int ERROR_RESOURCE_FAILED = 5038;

	/// <summary>
	///     The cluster node is not valid.
	/// </summary>
	public const int ERROR_CLUSTER_INVALID_NODE = 5039;

	/// <summary>
	///     The cluster node already exists.
	/// </summary>
	public const int ERROR_CLUSTER_NODE_EXISTS = 5040;

	/// <summary>
	///     A node is in the process of joining the cluster.
	/// </summary>
	public const int ERROR_CLUSTER_JOIN_IN_PROGRESS = 5041;

	/// <summary>
	///     The cluster node was not found.
	/// </summary>
	public const int ERROR_CLUSTER_NODE_NOT_FOUND = 5042;

	/// <summary>
	///     The cluster local node information was not found.
	/// </summary>
	public const int ERROR_CLUSTER_LOCAL_NODE_NOT_FOUND = 5043;

	/// <summary>
	///     The cluster network already exists.
	/// </summary>
	public const int ERROR_CLUSTER_NETWORK_EXISTS = 5044;

	/// <summary>
	///     The cluster network was not found.
	/// </summary>
	public const int ERROR_CLUSTER_NETWORK_NOT_FOUND = 5045;

	/// <summary>
	///     The cluster network interface already exists.
	/// </summary>
	public const int ERROR_CLUSTER_NETINTERFACE_EXISTS = 5046;

	/// <summary>
	///     The cluster network interface was not found.
	/// </summary>
	public const int ERROR_CLUSTER_NETINTERFACE_NOT_FOUND = 5047;

	/// <summary>
	///     The cluster request is not valid for this object.
	/// </summary>
	public const int ERROR_CLUSTER_INVALID_REQUEST = 5048;

	/// <summary>
	///     The cluster network provider is not valid.
	/// </summary>
	public const int ERROR_CLUSTER_INVALID_NETWORK_PROVIDER = 5049;

	/// <summary>
	///     The cluster node is down.
	/// </summary>
	public const int ERROR_CLUSTER_NODE_DOWN = 5050;

	/// <summary>
	///     The cluster node is not reachable.
	/// </summary>
	public const int ERROR_CLUSTER_NODE_UNREACHABLE = 5051;

	/// <summary>
	///     The cluster node is not a member of the cluster.
	/// </summary>
	public const int ERROR_CLUSTER_NODE_NOT_MEMBER = 5052;

	/// <summary>
	///     A cluster join operation is not in progress.
	/// </summary>
	public const int ERROR_CLUSTER_JOIN_NOT_IN_PROGRESS = 5053;

	/// <summary>
	///     The cluster network is not valid.
	/// </summary>
	public const int ERROR_CLUSTER_INVALID_NETWORK = 5054;

	/// <summary>
	///     The cluster node is up.
	/// </summary>
	public const int ERROR_CLUSTER_NODE_UP = 5056;

	/// <summary>
	///     The cluster IP address is already in use.
	/// </summary>
	public const int ERROR_CLUSTER_IPADDR_IN_USE = 5057;

	/// <summary>
	///     The cluster node is not paused.
	/// </summary>
	public const int ERROR_CLUSTER_NODE_NOT_PAUSED = 5058;

	/// <summary>
	///     No cluster security context is available.
	/// </summary>
	public const int ERROR_CLUSTER_NO_SECURITY_CONTEXT = 5059;

	/// <summary>
	///     The cluster network is not configured for internal cluster communication.
	/// </summary>
	public const int ERROR_CLUSTER_NETWORK_NOT_INTERNAL = 5060;

	/// <summary>
	///     The cluster node is already up.
	/// </summary>
	public const int ERROR_CLUSTER_NODE_ALREADY_UP = 5061;

	/// <summary>
	///     The cluster node is already down.
	/// </summary>
	public const int ERROR_CLUSTER_NODE_ALREADY_DOWN = 5062;

	/// <summary>
	///     The cluster network is already online.
	/// </summary>
	public const int ERROR_CLUSTER_NETWORK_ALREADY_ONLINE = 5063;

	/// <summary>
	///     The cluster network is already offline.
	/// </summary>
	public const int ERROR_CLUSTER_NETWORK_ALREADY_OFFLINE = 5064;

	/// <summary>
	///     The cluster node is already a member of the cluster.
	/// </summary>
	public const int ERROR_CLUSTER_NODE_ALREADY_MEMBER = 5065;

	/// <summary>
	///     The cluster network is the only one configured for internal cluster communication between two or more active
	///     cluster nodes. The internal communication capability cannot be removed from the network.
	/// </summary>
	public const int ERROR_CLUSTER_LAST_INTERNAL_NETWORK = 5066;

	/// <summary>
	///     One or more cluster resources depend on the network to provide service to clients. The client access capability
	///     cannot be removed from the network.
	/// </summary>
	public const int ERROR_CLUSTER_NETWORK_HAS_DEPENDENTS = 5067;

	/// <summary>
	///     This operation cannot be performed on the cluster resource as it the quorum resource. You may not bring the quorum
	///     resource offline or modify its possible owners list.
	/// </summary>
	public const int ERROR_INVALID_OPERATION_ON_QUORUM = 5068;

	/// <summary>
	///     The cluster quorum resource is not allowed to have any dependencies.
	/// </summary>
	public const int ERROR_DEPENDENCY_NOT_ALLOWED = 5069;

	/// <summary>
	///     The cluster node is paused.
	/// </summary>
	public const int ERROR_CLUSTER_NODE_PAUSED = 5070;

	/// <summary>
	///     The cluster resource cannot be brought online. The owner node cannot run this resource.
	/// </summary>
	public const int ERROR_NODE_CANT_HOST_RESOURCE = 5071;

	/// <summary>
	///     The cluster node is not ready to perform the requested operation.
	/// </summary>
	public const int ERROR_CLUSTER_NODE_NOT_READY = 5072;

	/// <summary>
	///     The cluster node is shutting down.
	/// </summary>
	public const int ERROR_CLUSTER_NODE_SHUTTING_DOWN = 5073;

	/// <summary>
	///     The cluster join operation was aborted.
	/// </summary>
	public const int ERROR_CLUSTER_JOIN_ABORTED = 5074;

	/// <summary>
	///     The cluster join operation failed due to incompatible software versions between the joining node and its sponsor.
	/// </summary>
	public const int ERROR_CLUSTER_INCOMPATIBLE_VERSIONS = 5075;

	/// <summary>
	///     This resource cannot be created because the cluster has reached the limit on the number of resources it can
	///     monitor.
	/// </summary>
	public const int ERROR_CLUSTER_MAXNUM_OF_RESOURCES_EXCEEDED = 5076;

	/// <summary>
	///     The system configuration changed during the cluster join or form operation. The join or form operation was aborted.
	/// </summary>
	public const int ERROR_CLUSTER_SYSTEM_CONFIG_CHANGED = 5077;

	/// <summary>
	///     The specified resource type was not found.
	/// </summary>
	public const int ERROR_CLUSTER_RESOURCE_TYPE_NOT_FOUND = 5078;

	/// <summary>
	///     The specified node does not support a resource of this type.  This may be due to version inconsistencies or due to
	///     the absence of the resource DLL on this node.
	/// </summary>
	public const int ERROR_CLUSTER_RESTYPE_NOT_SUPPORTED = 5079;

	/// <summary>
	///     The specified resource name is not supported by this resource DLL. This may be due to a bad (or changed) name
	///     supplied to the resource DLL.
	/// </summary>
	public const int ERROR_CLUSTER_RESNAME_NOT_FOUND = 5080;

	/// <summary>
	///     No authentication package could be registered with the RPC server.
	/// </summary>
	public const int ERROR_CLUSTER_NO_RPC_PACKAGES_REGISTERED = 5081;

	/// <summary>
	///     You cannot bring the group online because the owner of the group is not in the preferred list for the group. To
	///     change the owner node for the group, move the group.
	/// </summary>
	public const int ERROR_CLUSTER_OWNER_NOT_IN_PREFLIST = 5082;

	/// <summary>
	///     The join operation failed because the cluster database sequence number has changed or is incompatible with the
	///     locker node. This may happen during a join operation if the cluster database was changing during the join.
	/// </summary>
	public const int ERROR_CLUSTER_DATABASE_SEQMISMATCH = 5083;

	/// <summary>
	///     The resource monitor will not allow the fail operation to be performed while the resource is in its current state.
	///     This may happen if the resource is in a pending state.
	/// </summary>
	public const int ERROR_RESMON_INVALID_STATE = 5084;

	/// <summary>
	///     A non locker code got a request to reserve the lock for making global updates.
	/// </summary>
	public const int ERROR_CLUSTER_GUM_NOT_LOCKER = 5085;

	/// <summary>
	///     The quorum disk could not be located by the cluster service.
	/// </summary>
	public const int ERROR_QUORUM_DISK_NOT_FOUND = 5086;

	/// <summary>
	///     The backed up cluster database is possibly corrupt.
	/// </summary>
	public const int ERROR_DATABASE_BACKUP_CORRUPT = 5087;

	/// <summary>
	///     A DFS root already exists in this cluster node.
	/// </summary>
	public const int ERROR_CLUSTER_NODE_ALREADY_HAS_DFS_ROOT = 5088;

	/// <summary>
	///     An attempt to modify a resource property failed because it conflicts with another existing property.
	/// </summary>
	public const int ERROR_RESOURCE_PROPERTY_UNCHANGEABLE = 5089;

	/// <summary>
	///     An operation was attempted that is incompatible with the current membership state of the node.
	/// </summary>
	public const int ERROR_CLUSTER_MEMBERSHIP_INVALID_STATE = 5890;

	/// <summary>
	///     The quorum resource does not contain the quorum log.
	/// </summary>
	public const int ERROR_CLUSTER_QUORUMLOG_NOT_FOUND = 5891;

	/// <summary>
	///     The membership engine requested shutdown of the cluster service on this node.
	/// </summary>
	public const int ERROR_CLUSTER_MEMBERSHIP_HALT = 5892;

	/// <summary>
	///     The join operation failed because the cluster instance ID of the joining node does not match the cluster instance
	///     ID of the sponsor node.
	/// </summary>
	public const int ERROR_CLUSTER_INSTANCE_ID_MISMATCH = 5893;

	/// <summary>
	///     A matching network for the specified IP address could not be found. Please also specify a subnet mask and a cluster
	///     network.
	/// </summary>
	public const int ERROR_CLUSTER_NETWORK_NOT_FOUND_FOR_IP = 5894;

	/// <summary>
	///     The actual data type of the property did not match the expected data type of the property.
	/// </summary>
	public const int ERROR_CLUSTER_PROPERTY_DATA_TYPE_MISMATCH = 5895;

	/// <summary>
	///     The cluster node was evicted from the cluster successfully, but the node was not cleaned up.  Extended status
	///     information explaining why the node was not cleaned up is available.
	/// </summary>
	public const int ERROR_CLUSTER_EVICT_WITHOUT_CLEANUP = 5896;

	/// <summary>
	///     Two or more parameter values specified for a resource's properties are in conflict.
	/// </summary>
	public const int ERROR_CLUSTER_PARAMETER_MISMATCH = 5897;

	/// <summary>
	///     This computer cannot be made a member of a cluster.
	/// </summary>
	public const int ERROR_NODE_CANNOT_BE_CLUSTERED = 5898;

	/// <summary>
	///     This computer cannot be made a member of a cluster because it does not have the correct version of Windows
	///     installed.
	/// </summary>
	public const int ERROR_CLUSTER_WRONG_OS_VERSION = 5899;

	/// <summary>
	///     A cluster cannot be created with the specified cluster name because that cluster name is already in use. Specify a
	///     different name for the cluster.
	/// </summary>
	public const int ERROR_CLUSTER_CANT_CREATE_DUP_CLUSTER_NAME = 5900;

	/// <summary>
	///     No information avialable.
	/// </summary>
	public const int ERROR_CLUSCFG_ALREADY_COMMITTED = 5901;

	/// <summary>
	///     No information avialable.
	/// </summary>
	public const int ERROR_CLUSCFG_ROLLBACK_FAILED = 5902;

	/// <summary>
	///     No information avialable.
	/// </summary>
	public const int ERROR_CLUSCFG_SYSTEM_DISK_DRIVE_LETTER_CONFLICT = 5903;

	/// <summary>
	///     No information avialable.
	/// </summary>
	public const int ERROR_CLUSTER_OLD_VERSION = 5904;

	/// <summary>
	///     No information avialable.
	/// </summary>
	public const int ERROR_CLUSTER_MISMATCHED_COMPUTER_ACCT_NAME = 5905;

	/// <summary>
	///     The specified file could not be encrypted.
	/// </summary>
	public const int ERROR_ENCRYPTION_FAILED = 6000;

	/// <summary>
	///     The specified file could not be decrypted.
	/// </summary>
	public const int ERROR_DECRYPTION_FAILED = 6001;

	/// <summary>
	///     The specified file is encrypted and the user does not have the ability to decrypt it.
	/// </summary>
	public const int ERROR_FILE_ENCRYPTED = 6002;

	/// <summary>
	///     There is no valid encryption recovery policy configured for this system.
	/// </summary>
	public const int ERROR_NO_RECOVERY_POLICY = 6003;

	/// <summary>
	///     The required encryption driver is not loaded for this system.
	/// </summary>
	public const int ERROR_NO_EFS = 6004;

	/// <summary>
	///     The file was encrypted with a different encryption driver than is currently loaded.
	/// </summary>
	public const int ERROR_WRONG_EFS = 6005;

	/// <summary>
	///     There are no EFS keys defined for the user.
	/// </summary>
	public const int ERROR_NO_USER_KEYS = 6006;

	/// <summary>
	///     The specified file is not encrypted.
	/// </summary>
	public const int ERROR_FILE_NOT_ENCRYPTED = 6007;

	/// <summary>
	///     The specified file is not in the defined EFS export format.
	/// </summary>
	public const int ERROR_NOT_EXPORT_FORMAT = 6008;

	/// <summary>
	///     The specified file is read only.
	/// </summary>
	public const int ERROR_FILE_READ_ONLY = 6009;

	/// <summary>
	///     The directory has been disabled for encryption.
	/// </summary>
	public const int ERROR_DIR_EFS_DISALLOWED = 6010;

	/// <summary>
	///     The server is not trusted for remote encryption operation.
	/// </summary>
	public const int ERROR_EFS_SERVER_NOT_TRUSTED = 6011;

	/// <summary>
	///     Recovery policy configured for this system contains invalid recovery certificate.
	/// </summary>
	public const int ERROR_BAD_RECOVERY_POLICY = 6012;

	/// <summary>
	///     The encryption algorithm used on the source file needs a bigger key buffer than the one on the destination file.
	/// </summary>
	public const int ERROR_EFS_ALG_BLOB_TOO_BIG = 6013;

	/// <summary>
	///     The disk partition does not support file encryption.
	/// </summary>
	public const int ERROR_VOLUME_NOT_SUPPORT_EFS = 6014;

	/// <summary>
	///     This machine is disabled for file encryption.
	/// </summary>
	public const int ERROR_EFS_DISABLED = 6015;

	/// <summary>
	///     A newer system is required to decrypt this encrypted file.
	/// </summary>
	public const int ERROR_EFS_VERSION_NOT_SUPPORT = 6016;

	/// <summary>
	///     The list of servers for this workgroup is not currently available
	/// </summary>
	public const int ERROR_NO_BROWSER_SERVERS_FOUND = 6118;

	/// <summary>
	///     The Task Scheduler service must be configured to run in the System account to function properly.  Individual tasks
	///     may be configured to run in other accounts.
	/// </summary>
	public const int SCHED_E_SERVICE_NOT_LOCALSYSTEM = 6200;

	/// <summary>
	///     The specified session name is invalid.
	/// </summary>
	public const int ERROR_CTX_WINSTATION_NAME_INVALID = 7001;

	/// <summary>
	///     The specified protocol driver is invalid.
	/// </summary>
	public const int ERROR_CTX_INVALID_PD = 7002;

	/// <summary>
	///     The specified protocol driver was not found in the system path.
	/// </summary>
	public const int ERROR_CTX_PD_NOT_FOUND = 7003;

	/// <summary>
	///     The specified terminal connection driver was not found in the system path.
	/// </summary>
	public const int ERROR_CTX_WD_NOT_FOUND = 7004;

	/// <summary>
	///     A registry key for event logging could not be created for this session.
	/// </summary>
	public const int ERROR_CTX_CANNOT_MAKE_EVENTLOG_ENTRY = 7005;

	/// <summary>
	///     A service with the same name already exists on the system.
	/// </summary>
	public const int ERROR_CTX_SERVICE_NAME_COLLISION = 7006;

	/// <summary>
	///     A close operation is pending on the session.
	/// </summary>
	public const int ERROR_CTX_CLOSE_PENDING = 7007;

	/// <summary>
	///     There are no free output buffers available.
	/// </summary>
	public const int ERROR_CTX_NO_OUTBUF = 7008;

	/// <summary>
	///     The MODEM.INF file was not found.
	/// </summary>
	public const int ERROR_CTX_MODEM_INF_NOT_FOUND = 7009;

	/// <summary>
	///     The modem name was not found in MODEM.INF.
	/// </summary>
	public const int ERROR_CTX_INVALID_MODEMNAME = 7010;

	/// <summary>
	///     The modem did not accept the command sent to it. Verify that the configured modem name matches the attached modem.
	/// </summary>
	public const int ERROR_CTX_MODEM_RESPONSE_ERROR = 7011;

	/// <summary>
	///     The modem did not respond to the command sent to it. Verify that the modem is properly cabled and powered on.
	/// </summary>
	public const int ERROR_CTX_MODEM_RESPONSE_TIMEOUT = 7012;

	/// <summary>
	///     Carrier detect has failed or carrier has been dropped due to disconnect.
	/// </summary>
	public const int ERROR_CTX_MODEM_RESPONSE_NO_CARRIER = 7013;

	/// <summary>
	///     Dial tone not detected within the required time. Verify that the phone cable is properly attached and functional.
	/// </summary>
	public const int ERROR_CTX_MODEM_RESPONSE_NO_DIALTONE = 7014;

	/// <summary>
	///     Busy signal detected at remote site on callback.
	/// </summary>
	public const int ERROR_CTX_MODEM_RESPONSE_BUSY = 7015;

	/// <summary>
	///     Voice detected at remote site on callback.
	/// </summary>
	public const int ERROR_CTX_MODEM_RESPONSE_VOICE = 7016;

	/// <summary>
	///     Transport driver error
	/// </summary>
	public const int ERROR_CTX_TD_ERROR = 7017;

	/// <summary>
	///     The specified session cannot be found.
	/// </summary>
	public const int ERROR_CTX_WINSTATION_NOT_FOUND = 7022;

	/// <summary>
	///     The specified session name is already in use.
	/// </summary>
	public const int ERROR_CTX_WINSTATION_ALREADY_EXISTS = 7023;

	/// <summary>
	///     The requested operation cannot be completed because the terminal connection is currently busy processing a connect,
	///     disconnect, reset, or delete operation.
	/// </summary>
	public const int ERROR_CTX_WINSTATION_BUSY = 7024;

	/// <summary>
	///     An attempt has been made to connect to a session whose video mode is not supported by the current client.
	/// </summary>
	public const int ERROR_CTX_BAD_VIDEO_MODE = 7025;

	/// <summary>
	///     The application attempted to enable DOS graphics mode.
	///     DOS graphics mode is not supported.
	/// </summary>
	public const int ERROR_CTX_GRAPHICS_INVALID = 7035;

	/// <summary>
	///     Your interactive logon privilege has been disabled.
	///     Please contact your administrator.
	/// </summary>
	public const int ERROR_CTX_LOGON_DISABLED = 7037;

	/// <summary>
	///     The requested operation can be performed only on the system console.
	///     This is most often the result of a driver or system DLL requiring direct console access.
	/// </summary>
	public const int ERROR_CTX_NOT_CONSOLE = 7038;

	/// <summary>
	///     The client failed to respond to the server connect message.
	/// </summary>
	public const int ERROR_CTX_CLIENT_QUERY_TIMEOUT = 7040;

	/// <summary>
	///     Disconnecting the console session is not supported.
	/// </summary>
	public const int ERROR_CTX_CONSOLE_DISCONNECT = 7041;

	/// <summary>
	///     Reconnecting a disconnected session to the console is not supported.
	/// </summary>
	public const int ERROR_CTX_CONSOLE_CONNECT = 7042;

	/// <summary>
	///     The request to control another session remotely was denied.
	/// </summary>
	public const int ERROR_CTX_SHADOW_DENIED = 7044;

	/// <summary>
	///     The requested session access is denied.
	/// </summary>
	public const int ERROR_CTX_WINSTATION_ACCESS_DENIED = 7045;

	/// <summary>
	///     The specified terminal connection driver is invalid.
	/// </summary>
	public const int ERROR_CTX_INVALID_WD = 7049;

	/// <summary>
	///     The requested session cannot be controlled remotely.
	///     This may be because the session is disconnected or does not currently have a user logged on.
	/// </summary>
	public const int ERROR_CTX_SHADOW_INVALID = 7050;

	/// <summary>
	///     The requested session is not configured to allow remote control.
	/// </summary>
	public const int ERROR_CTX_SHADOW_DISABLED = 7051;

	/// <summary>
	///     Your request to connect to this Terminal Server has been rejected. Your Terminal Server client license number is
	///     currently being used by another user.
	///     Please call your system administrator to obtain a unique license number.
	/// </summary>
	public const int ERROR_CTX_CLIENT_LICENSE_IN_USE = 7052;

	/// <summary>
	///     Your request to connect to this Terminal Server has been rejected. Your Terminal Server client license number has
	///     not been entered for this copy of the Terminal Server client.
	///     Please contact your system administrator.
	/// </summary>
	public const int ERROR_CTX_CLIENT_LICENSE_NOT_SET = 7053;

	/// <summary>
	///     The system has reached its licensed logon limit.
	///     Please try again later.
	/// </summary>
	public const int ERROR_CTX_LICENSE_NOT_AVAILABLE = 7054;

	/// <summary>
	///     The client you are using is not licensed to use this system.  Your logon request is denied.
	/// </summary>
	public const int ERROR_CTX_LICENSE_CLIENT_INVALID = 7055;

	/// <summary>
	///     The system license has expired.  Your logon request is denied.
	/// </summary>
	public const int ERROR_CTX_LICENSE_EXPIRED = 7056;

	/// <summary>
	///     Remote control could not be terminated because the specified session is not currently being remotely controlled.
	/// </summary>
	public const int ERROR_CTX_SHADOW_NOT_RUNNING = 7057;

	/// <summary>
	///     The remote control of the console was terminated because the display mode was changed. Changing the display mode in
	///     a remote control session is not supported.
	/// </summary>
	public const int ERROR_CTX_SHADOW_ENDED_BY_MODE_CHANGE = 7058;

	/// <summary>
	///     No information avialable.
	/// </summary>
	public const int ERROR_ACTIVATION_COUNT_EXCEEDED = 7059;

	/// <summary>
	///     The file replication service API was called incorrectly.
	/// </summary>
	public const int FRS_ERR_INVALID_API_SEQUENCE = 8001;

	/// <summary>
	///     The file replication service cannot be started.
	/// </summary>
	public const int FRS_ERR_STARTING_SERVICE = 8002;

	/// <summary>
	///     The file replication service cannot be stopped.
	/// </summary>
	public const int FRS_ERR_STOPPING_SERVICE = 8003;

	/// <summary>
	///     The file replication service API terminated the request.
	///     The event log may have more information.
	/// </summary>
	public const int FRS_ERR_INTERNAL_API = 8004;

	/// <summary>
	///     The file replication service terminated the request.
	///     The event log may have more information.
	/// </summary>
	public const int FRS_ERR_INTERNAL = 8005;

	/// <summary>
	///     The file replication service cannot be contacted.
	///     The event log may have more information.
	/// </summary>
	public const int FRS_ERR_SERVICE_COMM = 8006;

	/// <summary>
	///     The file replication service cannot satisfy the request because the user has insufficient privileges.
	///     The event log may have more information.
	/// </summary>
	public const int FRS_ERR_INSUFFICIENT_PRIV = 8007;

	/// <summary>
	///     The file replication service cannot satisfy the request because authenticated RPC is not available.
	///     The event log may have more information.
	/// </summary>
	public const int FRS_ERR_AUTHENTICATION = 8008;

	/// <summary>
	///     The file replication service cannot satisfy the request because the user has insufficient privileges on the domain
	///     controller.
	///     The event log may have more information.
	/// </summary>
	public const int FRS_ERR_PARENT_INSUFFICIENT_PRIV = 8009;

	/// <summary>
	///     The file replication service cannot satisfy the request because authenticated RPC is not available on the domain
	///     controller.
	///     The event log may have more information.
	/// </summary>
	public const int FRS_ERR_PARENT_AUTHENTICATION = 8010;

	/// <summary>
	///     The file replication service cannot communicate with the file replication service on the domain controller.
	///     The event log may have more information.
	/// </summary>
	public const int FRS_ERR_CHILD_TO_PARENT_COMM = 8011;

	/// <summary>
	///     The file replication service on the domain controller cannot communicate with the file replication service on this
	///     computer.
	///     The event log may have more information.
	/// </summary>
	public const int FRS_ERR_PARENT_TO_CHILD_COMM = 8012;

	/// <summary>
	///     The file replication service cannot populate the system volume because of an internal error.
	///     The event log may have more information.
	/// </summary>
	public const int FRS_ERR_SYSVOL_POPULATE = 8013;

	/// <summary>
	///     The file replication service cannot populate the system volume because of an internal timeout.
	///     The event log may have more information.
	/// </summary>
	public const int FRS_ERR_SYSVOL_POPULATE_TIMEOUT = 8014;

	/// <summary>
	///     The file replication service cannot process the request. The system volume is busy with a previous request.
	/// </summary>
	public const int FRS_ERR_SYSVOL_IS_BUSY = 8015;

	/// <summary>
	///     The file replication service cannot stop replicating the system volume because of an internal error.
	///     The event log may have more information.
	/// </summary>
	public const int FRS_ERR_SYSVOL_DEMOTE = 8016;

	/// <summary>
	///     The file replication service detected an invalid parameter.
	/// </summary>
	public const int FRS_ERR_INVALID_SERVICE_PARAMETER = 8017;

	/// <summary>
	///     An error occurred while installing the directory service. For more information, see the event log.
	/// </summary>
	public const int ERROR_DS_NOT_INSTALLED = 8200;

	/// <summary>
	///     The directory service evaluated group memberships locally.
	/// </summary>
	public const int ERROR_DS_MEMBERSHIP_EVALUATED_LOCALLY = 8201;

	/// <summary>
	///     The specified directory service attribute or value does not exist.
	/// </summary>
	public const int ERROR_DS_NO_ATTRIBUTE_OR_VALUE = 8202;

	/// <summary>
	///     The attribute syntax specified to the directory service is invalid.
	/// </summary>
	public const int ERROR_DS_INVALID_ATTRIBUTE_SYNTAX = 8203;

	/// <summary>
	///     The attribute type specified to the directory service is not defined.
	/// </summary>
	public const int ERROR_DS_ATTRIBUTE_TYPE_UNDEFINED = 8204;

	/// <summary>
	///     The specified directory service attribute or value already exists.
	/// </summary>
	public const int ERROR_DS_ATTRIBUTE_OR_VALUE_EXISTS = 8205;

	/// <summary>
	///     The directory service is busy.
	/// </summary>
	public const int ERROR_DS_BUSY = 8206;

	/// <summary>
	///     The directory service is unavailable.
	/// </summary>
	public const int ERROR_DS_UNAVAILABLE = 8207;

	/// <summary>
	///     The directory service was unable to allocate a relative identifier.
	/// </summary>
	public const int ERROR_DS_NO_RIDS_ALLOCATED = 8208;

	/// <summary>
	///     The directory service has exhausted the pool of relative identifiers.
	/// </summary>
	public const int ERROR_DS_NO_MORE_RIDS = 8209;

	/// <summary>
	///     The requested operation could not be performed because the directory service is not the master for that type of
	///     operation.
	/// </summary>
	public const int ERROR_DS_INCORRECT_ROLE_OWNER = 8210;

	/// <summary>
	///     The directory service was unable to initialize the subsystem that allocates relative identifiers.
	/// </summary>
	public const int ERROR_DS_RIDMGR_INIT_ERROR = 8211;

	/// <summary>
	///     The requested operation did not satisfy one or more constraints associated with the class of the object.
	/// </summary>
	public const int ERROR_DS_OBJ_CLASS_VIOLATION = 8212;

	/// <summary>
	///     The directory service can perform the requested operation only on a leaf object.
	/// </summary>
	public const int ERROR_DS_CANT_ON_NON_LEAF = 8213;

	/// <summary>
	///     The directory service cannot perform the requested operation on the RDN attribute of an object.
	/// </summary>
	public const int ERROR_DS_CANT_ON_RDN = 8214;

	/// <summary>
	///     The directory service detected an attempt to modify the object class of an object.
	/// </summary>
	public const int ERROR_DS_CANT_MOD_OBJ_CLASS = 8215;

	/// <summary>
	///     The requested cross-domain move operation could not be performed.
	/// </summary>
	public const int ERROR_DS_CROSS_DOM_MOVE_ERROR = 8216;

	/// <summary>
	///     Unable to contact the global catalog server.
	/// </summary>
	public const int ERROR_DS_GC_NOT_AVAILABLE = 8217;

	/// <summary>
	///     The policy object is shared and can only be modified at the root.
	/// </summary>
	public const int ERROR_SHARED_POLICY = 8218;

	/// <summary>
	///     The policy object does not exist.
	/// </summary>
	public const int ERROR_POLICY_OBJECT_NOT_FOUND = 8219;

	/// <summary>
	///     The requested policy information is only in the directory service.
	/// </summary>
	public const int ERROR_POLICY_ONLY_IN_DS = 8220;

	/// <summary>
	///     A domain controller promotion is currently active.
	/// </summary>
	public const int ERROR_PROMOTION_ACTIVE = 8221;

	/// <summary>
	///     A domain controller promotion is not currently active
	/// </summary>
	public const int ERROR_NO_PROMOTION_ACTIVE = 8222;

	/// <summary>
	///     An operations error occurred.
	/// </summary>
	public const int ERROR_DS_OPERATIONS_ERROR = 8224;

	/// <summary>
	///     A protocol error occurred.
	/// </summary>
	public const int ERROR_DS_PROTOCOL_ERROR = 8225;

	/// <summary>
	///     The time limit for this request was exceeded.
	/// </summary>
	public const int ERROR_DS_TIMELIMIT_EXCEEDED = 8226;

	/// <summary>
	///     The size limit for this request was exceeded.
	/// </summary>
	public const int ERROR_DS_SIZELIMIT_EXCEEDED = 8227;

	/// <summary>
	///     The administrative limit for this request was exceeded.
	/// </summary>
	public const int ERROR_DS_ADMIN_LIMIT_EXCEEDED = 8228;

	/// <summary>
	///     The compare response was false.
	/// </summary>
	public const int ERROR_DS_COMPARE_FALSE = 8229;

	/// <summary>
	///     The compare response was true.
	/// </summary>
	public const int ERROR_DS_COMPARE_TRUE = 8230;

	/// <summary>
	///     The requested authentication method is not supported by the server.
	/// </summary>
	public const int ERROR_DS_AUTH_METHOD_NOT_SUPPORTED = 8231;

	/// <summary>
	///     A more secure authentication method is required for this server.
	/// </summary>
	public const int ERROR_DS_STRONG_AUTH_REQUIRED = 8232;

	/// <summary>
	///     Inappropriate authentication.
	/// </summary>
	public const int ERROR_DS_INAPPROPRIATE_AUTH = 8233;

	/// <summary>
	///     The authentication mechanism is unknown.
	/// </summary>
	public const int ERROR_DS_AUTH_UNKNOWN = 8234;

	/// <summary>
	///     A referral was returned from the server.
	/// </summary>
	public const int ERROR_DS_REFERRAL = 8235;

	/// <summary>
	///     The server does not support the requested critical extension.
	/// </summary>
	public const int ERROR_DS_UNAVAILABLE_CRIT_EXTENSION = 8236;

	/// <summary>
	///     This request requires a secure connection.
	/// </summary>
	public const int ERROR_DS_CONFIDENTIALITY_REQUIRED = 8237;

	/// <summary>
	///     Inappropriate matching.
	/// </summary>
	public const int ERROR_DS_INAPPROPRIATE_MATCHING = 8238;

	/// <summary>
	///     A constraint violation occurred.
	/// </summary>
	public const int ERROR_DS_CONSTRAINT_VIOLATION = 8239;

	/// <summary>
	///     There is no such object on the server.
	/// </summary>
	public const int ERROR_DS_NO_SUCH_OBJECT = 8240;

	/// <summary>
	///     There is an alias problem.
	/// </summary>
	public const int ERROR_DS_ALIAS_PROBLEM = 8241;

	/// <summary>
	///     An invalid dn syntax has been specified.
	/// </summary>
	public const int ERROR_DS_INVALID_DN_SYNTAX = 8242;

	/// <summary>
	///     The object is a leaf object.
	/// </summary>
	public const int ERROR_DS_IS_LEAF = 8243;

	/// <summary>
	///     There is an alias dereferencing problem.
	/// </summary>
	public const int ERROR_DS_ALIAS_DEREF_PROBLEM = 8244;

	/// <summary>
	///     The server is unwilling to process the request.
	/// </summary>
	public const int ERROR_DS_UNWILLING_TO_PERFORM = 8245;

	/// <summary>
	///     A loop has been detected.
	/// </summary>
	public const int ERROR_DS_LOOP_DETECT = 8246;

	/// <summary>
	///     There is a naming violation.
	/// </summary>
	public const int ERROR_DS_NAMING_VIOLATION = 8247;

	/// <summary>
	///     The result set is too large.
	/// </summary>
	public const int ERROR_DS_OBJECT_RESULTS_TOO_LARGE = 8248;

	/// <summary>
	///     The operation affects multiple DSAs
	/// </summary>
	public const int ERROR_DS_AFFECTS_MULTIPLE_DSAS = 8249;

	/// <summary>
	///     The server is not operational.
	/// </summary>
	public const int ERROR_DS_SERVER_DOWN = 8250;

	/// <summary>
	///     A local error has occurred.
	/// </summary>
	public const int ERROR_DS_LOCAL_ERROR = 8251;

	/// <summary>
	///     An encoding error has occurred.
	/// </summary>
	public const int ERROR_DS_ENCODING_ERROR = 8252;

	/// <summary>
	///     A decoding error has occurred.
	/// </summary>
	public const int ERROR_DS_DECODING_ERROR = 8253;

	/// <summary>
	///     The search filter cannot be recognized.
	/// </summary>
	public const int ERROR_DS_FILTER_UNKNOWN = 8254;

	/// <summary>
	///     One or more parameters are illegal.
	/// </summary>
	public const int ERROR_DS_PARAM_ERROR = 8255;

	/// <summary>
	///     The specified method is not supported.
	/// </summary>
	public const int ERROR_DS_NOT_SUPPORTED = 8256;

	/// <summary>
	///     No results were returned.
	/// </summary>
	public const int ERROR_DS_NO_RESULTS_RETURNED = 8257;

	/// <summary>
	///     The specified control is not supported by the server.
	/// </summary>
	public const int ERROR_DS_CONTROL_NOT_FOUND = 8258;

	/// <summary>
	///     A referral loop was detected by the client.
	/// </summary>
	public const int ERROR_DS_CLIENT_LOOP = 8259;

	/// <summary>
	///     The preset referral limit was exceeded.
	/// </summary>
	public const int ERROR_DS_REFERRAL_LIMIT_EXCEEDED = 8260;

	/// <summary>
	///     The search requires a SORT control.
	/// </summary>
	public const int ERROR_DS_SORT_CONTROL_MISSING = 8261;

	/// <summary>
	///     The search results exceed the offset range specified.
	/// </summary>
	public const int ERROR_DS_OFFSET_RANGE_ERROR = 8262;

	/// <summary>
	///     The root object must be the head of a naming context. The root object cannot have an instantiated parent.
	/// </summary>
	public const int ERROR_DS_ROOT_MUST_BE_NC = 8301;

	/// <summary>
	///     The add replica operation cannot be performed. The naming context must be writable in order to create the replica.
	/// </summary>
	public const int ERROR_DS_ADD_REPLICA_INHIBITED = 8302;

	/// <summary>
	///     A reference to an attribute that is not defined in the schema occurred.
	/// </summary>
	public const int ERROR_DS_ATT_NOT_DEF_IN_SCHEMA = 8303;

	/// <summary>
	///     The maximum size of an object has been exceeded.
	/// </summary>
	public const int ERROR_DS_MAX_OBJ_SIZE_EXCEEDED = 8304;

	/// <summary>
	///     An attempt was made to add an object to the directory with a name that is already in use.
	/// </summary>
	public const int ERROR_DS_OBJ_STRING_NAME_EXISTS = 8305;

	/// <summary>
	///     An attempt was made to add an object of a class that does not have an RDN defined in the schema.
	/// </summary>
	public const int ERROR_DS_NO_RDN_DEFINED_IN_SCHEMA = 8306;

	/// <summary>
	///     An attempt was made to add an object using an RDN that is not the RDN defined in the schema.
	/// </summary>
	public const int ERROR_DS_RDN_DOESNT_MATCH_SCHEMA = 8307;

	/// <summary>
	///     None of the requested attributes were found on the objects.
	/// </summary>
	public const int ERROR_DS_NO_REQUESTED_ATTS_FOUND = 8308;

	/// <summary>
	///     The user buffer is too small.
	/// </summary>
	public const int ERROR_DS_USER_BUFFER_TO_SMALL = 8309;

	/// <summary>
	///     The attribute specified in the operation is not present on the object.
	/// </summary>
	public const int ERROR_DS_ATT_IS_NOT_ON_OBJ = 8310;

	/// <summary>
	///     Illegal modify operation. Some aspect of the modification is not permitted.
	/// </summary>
	public const int ERROR_DS_ILLEGAL_MOD_OPERATION = 8311;

	/// <summary>
	///     The specified object is too large.
	/// </summary>
	public const int ERROR_DS_OBJ_TOO_LARGE = 8312;

	/// <summary>
	///     The specified instance type is not valid.
	/// </summary>
	public const int ERROR_DS_BAD_INSTANCE_TYPE = 8313;

	/// <summary>
	///     The operation must be performed at a master DSA.
	/// </summary>
	public const int ERROR_DS_MASTERDSA_REQUIRED = 8314;

	/// <summary>
	///     The object class attribute must be specified.
	/// </summary>
	public const int ERROR_DS_OBJECT_CLASS_REQUIRED = 8315;

	/// <summary>
	///     A required attribute is missing.
	/// </summary>
	public const int ERROR_DS_MISSING_REQUIRED_ATT = 8316;

	/// <summary>
	///     An attempt was made to modify an object to include an attribute that is not legal for its class.
	/// </summary>
	public const int ERROR_DS_ATT_NOT_DEF_FOR_CLASS = 8317;

	/// <summary>
	///     The specified attribute is already present on the object.
	/// </summary>
	public const int ERROR_DS_ATT_ALREADY_EXISTS = 8318;

	/// <summary>
	///     The specified attribute is not present, or has no values.
	/// </summary>
	public const int ERROR_DS_CANT_ADD_ATT_VALUES = 8320;

	/// <summary>
	///     Mutliple values were specified for an attribute that can have only one value.
	/// </summary>
	public const int ERROR_DS_SINGLE_VALUE_CONSTRAINT = 8321;

	/// <summary>
	///     A value for the attribute was not in the acceptable range of values.
	/// </summary>
	public const int ERROR_DS_RANGE_CONSTRAINT = 8322;

	/// <summary>
	///     The specified value already exists.
	/// </summary>
	public const int ERROR_DS_ATT_VAL_ALREADY_EXISTS = 8323;

	/// <summary>
	///     The attribute cannot be removed because it is not present on the object.
	/// </summary>
	public const int ERROR_DS_CANT_REM_MISSING_ATT = 8324;

	/// <summary>
	///     The attribute value cannot be removed because it is not present on the object.
	/// </summary>
	public const int ERROR_DS_CANT_REM_MISSING_ATT_VAL = 8325;

	/// <summary>
	///     The specified root object cannot be a subref.
	/// </summary>
	public const int ERROR_DS_ROOT_CANT_BE_SUBREF = 8326;

	/// <summary>
	///     Chaining is not permitted.
	/// </summary>
	public const int ERROR_DS_NO_CHAINING = 8327;

	/// <summary>
	///     Chained evaluation is not permitted.
	/// </summary>
	public const int ERROR_DS_NO_CHAINED_EVAL = 8328;

	/// <summary>
	///     The operation could not be performed because the object's parent is either uninstantiated or deleted.
	/// </summary>
	public const int ERROR_DS_NO_PARENT_OBJECT = 8329;

	/// <summary>
	///     Having a parent that is an alias is not permitted. Aliases are leaf objects.
	/// </summary>
	public const int ERROR_DS_PARENT_IS_AN_ALIAS = 8330;

	/// <summary>
	///     The object and parent must be of the same type, either both masters or both replicas.
	/// </summary>
	public const int ERROR_DS_CANT_MIX_MASTER_AND_REPS = 8331;

	/// <summary>
	///     The operation cannot be performed because child objects exist. This operation can only be performed on a leaf
	///     object.
	/// </summary>
	public const int ERROR_DS_CHILDREN_EXIST = 8332;

	/// <summary>
	///     Directory object not found.
	/// </summary>
	public const int ERROR_DS_OBJ_NOT_FOUND = 8333;

	/// <summary>
	///     The aliased object is missing.
	/// </summary>
	public const int ERROR_DS_ALIASED_OBJ_MISSING = 8334;

	/// <summary>
	///     The object name has bad syntax.
	/// </summary>
	public const int ERROR_DS_BAD_NAME_SYNTAX = 8335;

	/// <summary>
	///     It is not permitted for an alias to refer to another alias.
	/// </summary>
	public const int ERROR_DS_ALIAS_POINTS_TO_ALIAS = 8336;

	/// <summary>
	///     The alias cannot be dereferenced.
	/// </summary>
	public const int ERROR_DS_CANT_DEREF_ALIAS = 8337;

	/// <summary>
	///     The operation is out of scope.
	/// </summary>
	public const int ERROR_DS_OUT_OF_SCOPE = 8338;

	/// <summary>
	///     The operation cannot continue because the object is in the process of being removed.
	/// </summary>
	public const int ERROR_DS_OBJECT_BEING_REMOVED = 8339;

	/// <summary>
	///     The DSA object cannot be deleted.
	/// </summary>
	public const int ERROR_DS_CANT_DELETE_DSA_OBJ = 8340;

	/// <summary>
	///     A directory service error has occurred.
	/// </summary>
	public const int ERROR_DS_GENERIC_ERROR = 8341;

	/// <summary>
	///     The operation can only be performed on an internal master DSA object.
	/// </summary>
	public const int ERROR_DS_DSA_MUST_BE_INT_MASTER = 8342;

	/// <summary>
	///     The object must be of class DSA.
	/// </summary>
	public const int ERROR_DS_CLASS_NOT_DSA = 8343;

	/// <summary>
	///     Insufficient access rights to perform the operation.
	/// </summary>
	public const int ERROR_DS_INSUFF_ACCESS_RIGHTS = 8344;

	/// <summary>
	///     The object cannot be added because the parent is not on the list of possible superiors.
	/// </summary>
	public const int ERROR_DS_ILLEGAL_SUPERIOR = 8345;

	/// <summary>
	///     Access to the attribute is not permitted because the attribute is owned by the Security Accounts Manager (SAM).
	/// </summary>
	public const int ERROR_DS_ATTRIBUTE_OWNED_BY_SAM = 8346;

	/// <summary>
	///     The name has too many parts.
	/// </summary>
	public const int ERROR_DS_NAME_TOO_MANY_PARTS = 8347;

	/// <summary>
	///     The name is too long.
	/// </summary>
	public const int ERROR_DS_NAME_TOO_Int32 = 8348;

	/// <summary>
	///     The name value is too long.
	/// </summary>
	public const int ERROR_DS_NAME_VALUE_TOO_Int32 = 8349;

	/// <summary>
	///     The directory service encountered an error parsing a name.
	/// </summary>
	public const int ERROR_DS_NAME_UNPARSEABLE = 8350;

	/// <summary>
	///     The directory service cannot get the attribute type for a name.
	/// </summary>
	public const int ERROR_DS_NAME_TYPE_UNKNOWN = 8351;

	/// <summary>
	///     The name does not identify an object; the name identifies a phantom.
	/// </summary>
	public const int ERROR_DS_NOT_AN_OBJECT = 8352;

	/// <summary>
	///     The security descriptor is too short.
	/// </summary>
	public const int ERROR_DS_SEC_DESC_TOO_Int16 = 8353;

	/// <summary>
	///     The security descriptor is invalid.
	/// </summary>
	public const int ERROR_DS_SEC_DESC_INVALID = 8354;

	/// <summary>
	///     Failed to create name for deleted object.
	/// </summary>
	public const int ERROR_DS_NO_DELETED_NAME = 8355;

	/// <summary>
	///     The parent of a new subref must exist.
	/// </summary>
	public const int ERROR_DS_SUBREF_MUST_HAVE_PARENT = 8356;

	/// <summary>
	///     The object must be a naming context.
	/// </summary>
	public const int ERROR_DS_NCNAME_MUST_BE_NC = 8357;

	/// <summary>
	///     It is not permitted to add an attribute which is owned by the system.
	/// </summary>
	public const int ERROR_DS_CANT_ADD_SYSTEM_ONLY = 8358;

	/// <summary>
	///     The class of the object must be structural; you cannot instantiate an abstract class.
	/// </summary>
	public const int ERROR_DS_CLASS_MUST_BE_CONCRETE = 8359;

	/// <summary>
	///     The schema object could not be found.
	/// </summary>
	public const int ERROR_DS_INVALID_DMD = 8360;

	/// <summary>
	///     A local object with this GUID (dead or alive) already exists.
	/// </summary>
	public const int ERROR_DS_OBJ_GUID_EXISTS = 8361;

	/// <summary>
	///     The operation cannot be performed on a back link.
	/// </summary>
	public const int ERROR_DS_NOT_ON_BACKLINK = 8362;

	/// <summary>
	///     The cross reference for the specified naming context could not be found.
	/// </summary>
	public const int ERROR_DS_NO_CROSSREF_FOR_NC = 8363;

	/// <summary>
	///     The operation could not be performed because the directory service is shutting down.
	/// </summary>
	public const int ERROR_DS_SHUTTING_DOWN = 8364;

	/// <summary>
	///     The directory service request is invalid.
	/// </summary>
	public const int ERROR_DS_UNKNOWN_OPERATION = 8365;

	/// <summary>
	///     The role owner attribute could not be read.
	/// </summary>
	public const int ERROR_DS_INVALID_ROLE_OWNER = 8366;

	/// <summary>
	///     The requested FSMO operation failed. The current FSMO holder could not be contacted.
	/// </summary>
	public const int ERROR_DS_COULDNT_CONTACT_FSMO = 8367;

	/// <summary>
	///     Modification of a DN across a naming context is not permitted.
	/// </summary>
	public const int ERROR_DS_CROSS_NC_DN_RENAME = 8368;

	/// <summary>
	///     The attribute cannot be modified because it is owned by the system.
	/// </summary>
	public const int ERROR_DS_CANT_MOD_SYSTEM_ONLY = 8369;

	/// <summary>
	///     Only the replicator can perform this function.
	/// </summary>
	public const int ERROR_DS_REPLICATOR_ONLY = 8370;

	/// <summary>
	///     The specified class is not defined.
	/// </summary>
	public const int ERROR_DS_OBJ_CLASS_NOT_DEFINED = 8371;

	/// <summary>
	///     The specified class is not a subclass.
	/// </summary>
	public const int ERROR_DS_OBJ_CLASS_NOT_SUBCLASS = 8372;

	/// <summary>
	///     The name reference is invalid.
	/// </summary>
	public const int ERROR_DS_NAME_REFERENCE_INVALID = 8373;

	/// <summary>
	///     A cross reference already exists.
	/// </summary>
	public const int ERROR_DS_CROSS_REF_EXISTS = 8374;

	/// <summary>
	///     It is not permitted to delete a master cross reference.
	/// </summary>
	public const int ERROR_DS_CANT_DEL_MASTER_CROSSREF = 8375;

	/// <summary>
	///     Subtree notifications are only supported on NC heads.
	/// </summary>
	public const int ERROR_DS_SUBTREE_NOTIFY_NOT_NC_HEAD = 8376;

	/// <summary>
	///     Notification filter is too complex.
	/// </summary>
	public const int ERROR_DS_NOTIFY_FILTER_TOO_COMPLEX = 8377;

	/// <summary>
	///     Schema update failed: duplicate RDN.
	/// </summary>
	public const int ERROR_DS_DUP_RDN = 8378;

	/// <summary>
	///     Schema update failed: duplicate OID.
	/// </summary>
	public const int ERROR_DS_DUP_OID = 8379;

	/// <summary>
	///     Schema update failed: duplicate MAPI identifier.
	/// </summary>
	public const int ERROR_DS_DUP_MAPI_ID = 8380;

	/// <summary>
	///     Schema update failed: duplicate schema-id GUID.
	/// </summary>
	public const int ERROR_DS_DUP_SCHEMA_ID_GUID = 8381;

	/// <summary>
	///     Schema update failed: duplicate LDAP display name.
	/// </summary>
	public const int ERROR_DS_DUP_LDAP_DISPLAY_NAME = 8382;

	/// <summary>
	///     Schema update failed: range-lower less than range upper.
	/// </summary>
	public const int ERROR_DS_SEMANTIC_ATT_TEST = 8383;

	/// <summary>
	///     Schema update failed: syntax mismatch.
	/// </summary>
	public const int ERROR_DS_SYNTAX_MISMATCH = 8384;

	/// <summary>
	///     Schema deletion failed: attribute is used in must-contain.
	/// </summary>
	public const int ERROR_DS_EXISTS_IN_MUST_HAVE = 8385;

	/// <summary>
	///     Schema deletion failed: attribute is used in may-contain.
	/// </summary>
	public const int ERROR_DS_EXISTS_IN_MAY_HAVE = 8386;

	/// <summary>
	///     Schema update failed: attribute in may-contain does not exist.
	/// </summary>
	public const int ERROR_DS_NONEXISTENT_MAY_HAVE = 8387;

	/// <summary>
	///     Schema update failed: attribute in must-contain does not exist.
	/// </summary>
	public const int ERROR_DS_NONEXISTENT_MUST_HAVE = 8388;

	/// <summary>
	///     Schema update failed: class in aux-class list does not exist or is not an auxiliary class.
	/// </summary>
	public const int ERROR_DS_AUX_CLS_TEST_FAIL = 8389;

	/// <summary>
	///     Schema update failed: class in poss-superiors does not exist.
	/// </summary>
	public const int ERROR_DS_NONEXISTENT_POSS_SUP = 8390;

	/// <summary>
	///     Schema update failed: class in subclassof list does not exist or does not satisfy hierarchy rules.
	/// </summary>
	public const int ERROR_DS_SUB_CLS_TEST_FAIL = 8391;

	/// <summary>
	///     Schema update failed: Rdn-Att-Id has wrong syntax.
	/// </summary>
	public const int ERROR_DS_BAD_RDN_ATT_ID_SYNTAX = 8392;

	/// <summary>
	///     Schema deletion failed: class is used as auxiliary class.
	/// </summary>
	public const int ERROR_DS_EXISTS_IN_AUX_CLS = 8393;

	/// <summary>
	///     Schema deletion failed: class is used as sub class.
	/// </summary>
	public const int ERROR_DS_EXISTS_IN_SUB_CLS = 8394;

	/// <summary>
	///     Schema deletion failed: class is used as poss superior.
	/// </summary>
	public const int ERROR_DS_EXISTS_IN_POSS_SUP = 8395;

	/// <summary>
	///     Schema update failed in recalculating validation cache.
	/// </summary>
	public const int ERROR_DS_RECALCSCHEMA_FAILED = 8396;

	/// <summary>
	///     The tree deletion is not finished.  The request must be made again to continue deleting the tree.
	/// </summary>
	public const int ERROR_DS_TREE_DELETE_NOT_FINISHED = 8397;

	/// <summary>
	///     The requested delete operation could not be performed.
	/// </summary>
	public const int ERROR_DS_CANT_DELETE = 8398;

	/// <summary>
	///     Cannot read the governs class identifier for the schema record.
	/// </summary>
	public const int ERROR_DS_ATT_SCHEMA_REQ_ID = 8399;

	/// <summary>
	///     The attribute schema has bad syntax.
	/// </summary>
	public const int ERROR_DS_BAD_ATT_SCHEMA_SYNTAX = 8400;

	/// <summary>
	///     The attribute could not be cached.
	/// </summary>
	public const int ERROR_DS_CANT_CACHE_ATT = 8401;

	/// <summary>
	///     The class could not be cached.
	/// </summary>
	public const int ERROR_DS_CANT_CACHE_CLASS = 8402;

	/// <summary>
	///     The attribute could not be removed from the cache.
	/// </summary>
	public const int ERROR_DS_CANT_REMOVE_ATT_CACHE = 8403;

	/// <summary>
	///     The class could not be removed from the cache.
	/// </summary>
	public const int ERROR_DS_CANT_REMOVE_CLASS_CACHE = 8404;

	/// <summary>
	///     The distinguished name attribute could not be read.
	/// </summary>
	public const int ERROR_DS_CANT_RETRIEVE_DN = 8405;

	/// <summary>
	///     A required subref is missing.
	/// </summary>
	public const int ERROR_DS_MISSING_SUPREF = 8406;

	/// <summary>
	///     The instance type attribute could not be retrieved.
	/// </summary>
	public const int ERROR_DS_CANT_RETRIEVE_INSTANCE = 8407;

	/// <summary>
	///     An internal error has occurred.
	/// </summary>
	public const int ERROR_DS_CODE_INCONSISTENCY = 8408;

	/// <summary>
	///     A database error has occurred.
	/// </summary>
	public const int ERROR_DS_DATABASE_ERROR = 8409;

	/// <summary>
	///     The attribute GOVERNSID is missing.
	/// </summary>
	public const int ERROR_DS_GOVERNSID_MISSING = 8410;

	/// <summary>
	///     An expected attribute is missing.
	/// </summary>
	public const int ERROR_DS_MISSING_EXPECTED_ATT = 8411;

	/// <summary>
	///     The specified naming context is missing a cross reference.
	/// </summary>
	public const int ERROR_DS_NCNAME_MISSING_CR_REF = 8412;

	/// <summary>
	///     A security checking error has occurred.
	/// </summary>
	public const int ERROR_DS_SECURITY_CHECKING_ERROR = 8413;

	/// <summary>
	///     The schema is not loaded.
	/// </summary>
	public const int ERROR_DS_SCHEMA_NOT_LOADED = 8414;

	/// <summary>
	///     Schema allocation failed. Please check if the machine is running low on memory.
	/// </summary>
	public const int ERROR_DS_SCHEMA_ALLOC_FAILED = 8415;

	/// <summary>
	///     Failed to obtain the required syntax for the attribute schema.
	/// </summary>
	public const int ERROR_DS_ATT_SCHEMA_REQ_SYNTAX = 8416;

	/// <summary>
	///     The global catalog verification failed. The global catalog is not available or does not support the operation. Some
	///     part of the directory is currently not available.
	/// </summary>
	public const int ERROR_DS_GCVERIFY_ERROR = 8417;

	/// <summary>
	///     The replication operation failed because of a schema mismatch between the servers involved.
	/// </summary>
	public const int ERROR_DS_DRA_SCHEMA_MISMATCH = 8418;

	/// <summary>
	///     The DSA object could not be found.
	/// </summary>
	public const int ERROR_DS_CANT_FIND_DSA_OBJ = 8419;

	/// <summary>
	///     The naming context could not be found.
	/// </summary>
	public const int ERROR_DS_CANT_FIND_EXPECTED_NC = 8420;

	/// <summary>
	///     The naming context could not be found in the cache.
	/// </summary>
	public const int ERROR_DS_CANT_FIND_NC_IN_CACHE = 8421;

	/// <summary>
	///     The child object could not be retrieved.
	/// </summary>
	public const int ERROR_DS_CANT_RETRIEVE_CHILD = 8422;

	/// <summary>
	///     The modification was not permitted for security reasons.
	/// </summary>
	public const int ERROR_DS_SECURITY_ILLEGAL_MODIFY = 8423;

	/// <summary>
	///     The operation cannot replace the hidden record.
	/// </summary>
	public const int ERROR_DS_CANT_REPLACE_HIDDEN_REC = 8424;

	/// <summary>
	///     The hierarchy file is invalid.
	/// </summary>
	public const int ERROR_DS_BAD_HIERARCHY_FILE = 8425;

	/// <summary>
	///     The attempt to build the hierarchy table failed.
	/// </summary>
	public const int ERROR_DS_BUILD_HIERARCHY_TABLE_FAILED = 8426;

	/// <summary>
	///     The directory configuration parameter is missing from the registry.
	/// </summary>
	public const int ERROR_DS_CONFIG_PARAM_MISSING = 8427;

	/// <summary>
	///     The attempt to count the address book indices failed.
	/// </summary>
	public const int ERROR_DS_COUNTING_AB_INDICES_FAILED = 8428;

	/// <summary>
	///     The allocation of the hierarchy table failed.
	/// </summary>
	public const int ERROR_DS_HIERARCHY_TABLE_MALLOC_FAILED = 8429;

	/// <summary>
	///     The directory service encountered an internal failure.
	/// </summary>
	public const int ERROR_DS_INTERNAL_FAILURE = 8430;

	/// <summary>
	///     The directory service encountered an unknown failure.
	/// </summary>
	public const int ERROR_DS_UNKNOWN_ERROR = 8431;

	/// <summary>
	///     A root object requires a class of 'top'.
	/// </summary>
	public const int ERROR_DS_ROOT_REQUIRES_CLASS_TOP = 8432;

	/// <summary>
	///     This directory server is shutting down, and cannot take ownership of new floating single-master operation roles.
	/// </summary>
	public const int ERROR_DS_REFUSING_FSMO_ROLES = 8433;

	/// <summary>
	///     The directory service is missing mandatory configuration information, and is unable to determine the ownership of
	///     floating single-master operation roles.
	/// </summary>
	public const int ERROR_DS_MISSING_FSMO_SETTINGS = 8434;

	/// <summary>
	///     The directory service was unable to transfer ownership of one or more floating single-master operation roles to
	///     other servers.
	/// </summary>
	public const int ERROR_DS_UNABLE_TO_SURRENDER_ROLES = 8435;

	/// <summary>
	///     The replication operation failed.
	/// </summary>
	public const int ERROR_DS_DRA_GENERIC = 8436;

	/// <summary>
	///     An invalid parameter was specified for this replication operation.
	/// </summary>
	public const int ERROR_DS_DRA_INVALID_PARAMETER = 8437;

	/// <summary>
	///     The directory service is too busy to complete the replication operation at this time.
	/// </summary>
	public const int ERROR_DS_DRA_BUSY = 8438;

	/// <summary>
	///     The distinguished name specified for this replication operation is invalid.
	/// </summary>
	public const int ERROR_DS_DRA_BAD_DN = 8439;

	/// <summary>
	///     The naming context specified for this replication operation is invalid.
	/// </summary>
	public const int ERROR_DS_DRA_BAD_NC = 8440;

	/// <summary>
	///     The distinguished name specified for this replication operation already exists.
	/// </summary>
	public const int ERROR_DS_DRA_DN_EXISTS = 8441;

	/// <summary>
	///     The replication system encountered an internal error.
	/// </summary>
	public const int ERROR_DS_DRA_INTERNAL_ERROR = 8442;

	/// <summary>
	///     The replication operation encountered a database inconsistency.
	/// </summary>
	public const int ERROR_DS_DRA_INCONSISTENT_DIT = 8443;

	/// <summary>
	///     The server specified for this replication operation could not be contacted.
	/// </summary>
	public const int ERROR_DS_DRA_CONNECTION_FAILED = 8444;

	/// <summary>
	///     The replication operation encountered an object with an invalid instance type.
	/// </summary>
	public const int ERROR_DS_DRA_BAD_INSTANCE_TYPE = 8445;

	/// <summary>
	///     The replication operation failed to allocate memory.
	/// </summary>
	public const int ERROR_DS_DRA_OUT_OF_MEM = 8446;

	/// <summary>
	///     The replication operation encountered an error with the mail system.
	/// </summary>
	public const int ERROR_DS_DRA_MAIL_PROBLEM = 8447;

	/// <summary>
	///     The replication reference information for the target server already exists.
	/// </summary>
	public const int ERROR_DS_DRA_REF_ALREADY_EXISTS = 8448;

	/// <summary>
	///     The replication reference information for the target server does not exist.
	/// </summary>
	public const int ERROR_DS_DRA_REF_NOT_FOUND = 8449;

	/// <summary>
	///     The naming context cannot be removed because it is replicated to another server.
	/// </summary>
	public const int ERROR_DS_DRA_OBJ_IS_REP_SOURCE = 8450;

	/// <summary>
	///     The replication operation encountered a database error.
	/// </summary>
	public const int ERROR_DS_DRA_DB_ERROR = 8451;

	/// <summary>
	///     The naming context is in the process of being removed or is not replicated from the specified server.
	/// </summary>
	public const int ERROR_DS_DRA_NO_REPLICA = 8452;

	/// <summary>
	///     Replication access was denied.
	/// </summary>
	public const int ERROR_DS_DRA_ACCESS_DENIED = 8453;

	/// <summary>
	///     The requested operation is not supported by this version of the directory service.
	/// </summary>
	public const int ERROR_DS_DRA_NOT_SUPPORTED = 8454;

	/// <summary>
	///     The replication remote procedure call was cancelled.
	/// </summary>
	public const int ERROR_DS_DRA_RPC_CANCELLED = 8455;

	/// <summary>
	///     The source server is currently rejecting replication requests.
	/// </summary>
	public const int ERROR_DS_DRA_SOURCE_DISABLED = 8456;

	/// <summary>
	///     The destination server is currently rejecting replication requests.
	/// </summary>
	public const int ERROR_DS_DRA_SINK_DISABLED = 8457;

	/// <summary>
	///     The replication operation failed due to a collision of object names.
	/// </summary>
	public const int ERROR_DS_DRA_NAME_COLLISION = 8458;

	/// <summary>
	///     The replication source has been reinstalled.
	/// </summary>
	public const int ERROR_DS_DRA_SOURCE_REINSTALLED = 8459;

	/// <summary>
	///     The replication operation failed because a required parent object is missing.
	/// </summary>
	public const int ERROR_DS_DRA_MISSING_PARENT = 8460;

	/// <summary>
	///     The replication operation was preempted.
	/// </summary>
	public const int ERROR_DS_DRA_PREEMPTED = 8461;

	/// <summary>
	///     The replication synchronization attempt was abandoned because of a lack of updates.
	/// </summary>
	public const int ERROR_DS_DRA_ABANDON_SYNC = 8462;

	/// <summary>
	///     The replication operation was terminated because the system is shutting down.
	/// </summary>
	public const int ERROR_DS_DRA_SHUTDOWN = 8463;

	/// <summary>
	///     The replication synchronization attempt failed as the destination partial attribute set is not a subset of source
	///     partial attribute set.
	/// </summary>
	public const int ERROR_DS_DRA_INCOMPATIBLE_PARTIAL_SET = 8464;

	/// <summary>
	///     The replication synchronization attempt failed because a master replica attempted to sync from a partial replica.
	/// </summary>
	public const int ERROR_DS_DRA_SOURCE_IS_PARTIAL_REPLICA = 8465;

	/// <summary>
	///     The server specified for this replication operation was contacted, but that server was unable to contact an
	///     additional server needed to complete the operation.
	/// </summary>
	public const int ERROR_DS_DRA_EXTN_CONNECTION_FAILED = 8466;

	/// <summary>
	///     The version of the Active Directory schema of the source forest is not compatible with the version of Active
	///     Directory on this computer.  You must upgrade the operating system on a domain controller in the source forest
	///     before this computer can be added as a domain controller to that forest.
	/// </summary>
	public const int ERROR_DS_INSTALL_SCHEMA_MISMATCH = 8467;

	/// <summary>
	///     Schema update failed: An attribute with the same link identifier already exists.
	/// </summary>
	public const int ERROR_DS_DUP_LINK_ID = 8468;

	/// <summary>
	///     Name translation: Generic processing error.
	/// </summary>
	public const int ERROR_DS_NAME_ERROR_RESOLVING = 8469;

	/// <summary>
	///     Name translation: Could not find the name or insufficient right to see name.
	/// </summary>
	public const int ERROR_DS_NAME_ERROR_NOT_FOUND = 8470;

	/// <summary>
	///     Name translation: Input name mapped to more than one output name.
	/// </summary>
	public const int ERROR_DS_NAME_ERROR_NOT_UNIQUE = 8471;

	/// <summary>
	///     Name translation: Input name found, but not the associated output format.
	/// </summary>
	public const int ERROR_DS_NAME_ERROR_NO_MAPPING = 8472;

	/// <summary>
	///     Name translation: Unable to resolve completely, only the domain was found.
	/// </summary>
	public const int ERROR_DS_NAME_ERROR_DOMAIN_ONLY = 8473;

	/// <summary>
	///     Name translation: Unable to perform purely syntactical mapping at the client without going out to the wire.
	/// </summary>
	public const int ERROR_DS_NAME_ERROR_NO_SYNTACTICAL_MAPPING = 8474;

	/// <summary>
	///     Modification of a constructed att is not allowed.
	/// </summary>
	public const int ERROR_DS_CONSTRUCTED_ATT_MOD = 8475;

	/// <summary>
	///     The OM-Object-Class specified is incorrect for an attribute with the specified syntax.
	/// </summary>
	public const int ERROR_DS_WRONG_OM_OBJ_CLASS = 8476;

	/// <summary>
	///     The replication request has been posted; waiting for reply.
	/// </summary>
	public const int ERROR_DS_DRA_REPL_PENDING = 8477;

	/// <summary>
	///     The requested operation requires a directory service, and none was available.
	/// </summary>
	public const int ERROR_DS_DS_REQUIRED = 8478;

	/// <summary>
	///     The LDAP display name of the class or attribute contains non-ASCII characters.
	/// </summary>
	public const int ERROR_DS_INVALID_LDAP_DISPLAY_NAME = 8479;

	/// <summary>
	///     The requested search operation is only supported for base searches.
	/// </summary>
	public const int ERROR_DS_NON_BASE_SEARCH = 8480;

	/// <summary>
	///     The search failed to retrieve attributes from the database.
	/// </summary>
	public const int ERROR_DS_CANT_RETRIEVE_ATTS = 8481;

	/// <summary>
	///     The schema update operation tried to add a backward link attribute that has no corresponding forward link.
	/// </summary>
	public const int ERROR_DS_BACKLINK_WITHOUT_LINK = 8482;

	/// <summary>
	///     Source and destination of a cross-domain move do not agree on the object's epoch number.  Either source or
	///     destination does not have the latest version of the object.
	/// </summary>
	public const int ERROR_DS_EPOCH_MISMATCH = 8483;

	/// <summary>
	///     Source and destination of a cross-domain move do not agree on the object's current name.  Either source or
	///     destination does not have the latest version of the object.
	/// </summary>
	public const int ERROR_DS_SRC_NAME_MISMATCH = 8484;

	/// <summary>
	///     Source and destination for the cross-domain move operation are identical.  Caller should use local move operation
	///     instead of cross-domain move operation.
	/// </summary>
	public const int ERROR_DS_SRC_AND_DST_NC_IDENTICAL = 8485;

	/// <summary>
	///     Source and destination for a cross-domain move are not in agreement on the naming contexts in the forest.  Either
	///     source or destination does not have the latest version of the Partitions container.
	/// </summary>
	public const int ERROR_DS_DST_NC_MISMATCH = 8486;

	/// <summary>
	///     Destination of a cross-domain move is not authoritative for the destination naming context.
	/// </summary>
	public const int ERROR_DS_NOT_AUTHORITIVE_FOR_DST_NC = 8487;

	/// <summary>
	///     Source and destination of a cross-domain move do not agree on the identity of the source object.  Either source or
	///     destination does not have the latest version of the source object.
	/// </summary>
	public const int ERROR_DS_SRC_GUID_MISMATCH = 8488;

	/// <summary>
	///     Object being moved across-domains is already known to be deleted by the destination server.  The source server does
	///     not have the latest version of the source object.
	/// </summary>
	public const int ERROR_DS_CANT_MOVE_DELETED_OBJECT = 8489;

	/// <summary>
	///     Another operation which requires exclusive access to the PDC FSMO is already in progress.
	/// </summary>
	public const int ERROR_DS_PDC_OPERATION_IN_PROGRESS = 8490;

	/// <summary>
	///     A cross-domain move operation failed such that two versions of the moved object exist - one each in the source and
	///     destination domains.  The destination object needs to be removed to restore the system to a consistent state.
	/// </summary>
	public const int ERROR_DS_CROSS_DOMAIN_CLEANUP_REQD = 8491;

	/// <summary>
	///     This object may not be moved across domain boundaries either because cross-domain moves for this class are
	///     disallowed, or the object has some special characteristics, eg: trust account or restricted RID, which prevent its
	///     move.
	/// </summary>
	public const int ERROR_DS_ILLEGAL_XDOM_MOVE_OPERATION = 8492;

	/// <summary>
	///     Can't move objects with memberships across domain boundaries as once moved, this would violate the membership
	///     conditions of the account group.  Remove the object from any account group memberships and retry.
	/// </summary>
	public const int ERROR_DS_CANT_WITH_ACCT_GROUP_MEMBERSHPS = 8493;

	/// <summary>
	///     A naming context head must be the immediate child of another naming context head, not of an interior node.
	/// </summary>
	public const int ERROR_DS_NC_MUST_HAVE_NC_PARENT = 8494;

	/// <summary>
	///     The directory cannot validate the proposed naming context name because it does not hold a replica of the naming
	///     context above the proposed naming context.  Please ensure that the domain naming master role is held by a server
	///     that is configured as a global catalog server, and that the server is up to date with its replication partners.
	///     (Applies only to Windows 2000 Domain Naming masters)
	/// </summary>
	public const int ERROR_DS_CR_IMPOSSIBLE_TO_VALIDATE = 8495;

	/// <summary>
	///     Destination domain must be in native mode.
	/// </summary>
	public const int ERROR_DS_DST_DOMAIN_NOT_NATIVE = 8496;

	/// <summary>
	///     The operation can not be performed because the server does not have an infrastructure container in the domain of
	///     interest.
	/// </summary>
	public const int ERROR_DS_MISSING_INFRASTRUCTURE_CONTAINER = 8497;

	/// <summary>
	///     Cross-domain move of non-empty account groups is not allowed.
	/// </summary>
	public const int ERROR_DS_CANT_MOVE_ACCOUNT_GROUP = 8498;

	/// <summary>
	///     Cross-domain move of non-empty resource groups is not allowed.
	/// </summary>
	public const int ERROR_DS_CANT_MOVE_RESOURCE_GROUP = 8499;

	/// <summary>
	///     The search flags for the attribute are invalid. The ANR bit is valid only on attributes of Unicode or Teletex
	///     strings.
	/// </summary>
	public const int ERROR_DS_INVALID_SEARCH_FLAG = 8500;

	/// <summary>
	///     Tree deletions starting at an object which has an NC head as a descendant are not allowed.
	/// </summary>
	public const int ERROR_DS_NO_TREE_DELETE_ABOVE_NC = 8501;

	/// <summary>
	///     The directory service failed to lock a tree in preparation for a tree deletion because the tree was in use.
	/// </summary>
	public const int ERROR_DS_COULDNT_LOCK_TREE_FOR_DELETE = 8502;

	/// <summary>
	///     The directory service failed to identify the list of objects to delete while attempting a tree deletion.
	/// </summary>
	public const int ERROR_DS_COULDNT_IDENTIFY_OBJECTS_FOR_TREE_DELETE = 8503;

	/// <summary>
	///     Security Accounts Manager initialization failed because of the following error: %1.
	///     Error Status: 0x%2. Click OK to shut down the system and reboot into Directory Services Restore Mode. Check the
	///     event log for detailed information.
	/// </summary>
	public const int ERROR_DS_SAM_INIT_FAILURE = 8504;

	/// <summary>
	///     Only an administrator can modify the membership list of an administrative group.
	/// </summary>
	public const int ERROR_DS_SENSITIVE_GROUP_VIOLATION = 8505;

	/// <summary>
	///     Cannot change the primary group ID of a domain controller account.
	/// </summary>
	public const int ERROR_DS_CANT_MOD_PRIMARYGROUPID = 8506;

	/// <summary>
	///     An attempt is made to modify the base schema.
	/// </summary>
	public const int ERROR_DS_ILLEGAL_BASE_SCHEMA_MOD = 8507;

	/// <summary>
	///     Adding a new mandatory attribute to an existing class, deleting a mandatory attribute from an existing class, or
	///     adding an optional attribute to the special class Top that is not a backlink attribute (directly or through
	///     inheritance, for example, by adding or deleting an auxiliary class) is not allowed.
	/// </summary>
	public const int ERROR_DS_NONSAFE_SCHEMA_CHANGE = 8508;

	/// <summary>
	///     Schema update is not allowed on this DC because the DC is not the schema FSMO Role Owner.
	/// </summary>
	public const int ERROR_DS_SCHEMA_UPDATE_DISALLOWED = 8509;

	/// <summary>
	///     An object of this class cannot be created under the schema container. You can only create attribute-schema and
	///     class-schema objects under the schema container.
	/// </summary>
	public const int ERROR_DS_CANT_CREATE_UNDER_SCHEMA = 8510;

	/// <summary>
	///     The replica/child install failed to get the objectVersion attribute on the schema container on the source DC.
	///     Either the attribute is missing on the schema container or the credentials supplied do not have permission to read
	///     it.
	/// </summary>
	public const int ERROR_DS_INSTALL_NO_SRC_SCH_VERSION = 8511;

	/// <summary>
	///     The replica/child install failed to read the objectVersion attribute in the SCHEMA section of the file schema.ini
	///     in the system32 directory.
	/// </summary>
	public const int ERROR_DS_INSTALL_NO_SCH_VERSION_IN_INIFILE = 8512;

	/// <summary>
	///     The specified group type is invalid.
	/// </summary>
	public const int ERROR_DS_INVALID_GROUP_TYPE = 8513;

	/// <summary>
	///     You cannot nest global groups in a mixed domain if the group is security-enabled.
	/// </summary>
	public const int ERROR_DS_NO_NEST_GLOBALGROUP_IN_MIXEDDOMAIN = 8514;

	/// <summary>
	///     You cannot nest local groups in a mixed domain if the group is security-enabled.
	/// </summary>
	public const int ERROR_DS_NO_NEST_LOCALGROUP_IN_MIXEDDOMAIN = 8515;

	/// <summary>
	///     A global group cannot have a local group as a member.
	/// </summary>
	public const int ERROR_DS_GLOBAL_CANT_HAVE_LOCAL_MEMBER = 8516;

	/// <summary>
	///     A global group cannot have a universal group as a member.
	/// </summary>
	public const int ERROR_DS_GLOBAL_CANT_HAVE_UNIVERSAL_MEMBER = 8517;

	/// <summary>
	///     A universal group cannot have a local group as a member.
	/// </summary>
	public const int ERROR_DS_UNIVERSAL_CANT_HAVE_LOCAL_MEMBER = 8518;

	/// <summary>
	///     A global group cannot have a cross-domain member.
	/// </summary>
	public const int ERROR_DS_GLOBAL_CANT_HAVE_CROSSDOMAIN_MEMBER = 8519;

	/// <summary>
	///     A local group cannot have another cross domain local group as a member.
	/// </summary>
	public const int ERROR_DS_LOCAL_CANT_HAVE_CROSSDOMAIN_LOCAL_MEMBER = 8520;

	/// <summary>
	///     A group with primary members cannot change to a security-disabled group.
	/// </summary>
	public const int ERROR_DS_HAVE_PRIMARY_MEMBERS = 8521;

	/// <summary>
	///     The schema cache load failed to convert the string default SD on a class-schema object.
	/// </summary>
	public const int ERROR_DS_STRING_SD_CONVERSION_FAILED = 8522;

	/// <summary>
	///     Only DSAs configured to be Global Catalog servers should be allowed to hold the Domain Naming Master FSMO role.
	///     (Applies only to Windows 2000 servers)
	/// </summary>
	public const int ERROR_DS_NAMING_MASTER_GC = 8523;

	/// <summary>
	///     The DSA operation is unable to proceed because of a DNS lookup failure.
	/// </summary>
	public const int ERROR_DS_DNS_LOOKUP_FAILURE = 8524;

	/// <summary>
	///     While processing a change to the DNS Host Name for an object, the Service Principal Name values could not be kept
	///     in sync.
	/// </summary>
	public const int ERROR_DS_COULDNT_UPDATE_SPNS = 8525;

	/// <summary>
	///     The Security Descriptor attribute could not be read.
	/// </summary>
	public const int ERROR_DS_CANT_RETRIEVE_SD = 8526;

	/// <summary>
	///     The object requested was not found, but an object with that key was found.
	/// </summary>
	public const int ERROR_DS_KEY_NOT_UNIQUE = 8527;

	/// <summary>
	///     The syntax of the linked attribute being added is incorrect. Forward links can only have syntax 2.5.5.1, 2.5.5.7,
	///     and 2.5.5.14, and backlinks can only have syntax 2.5.5.1
	/// </summary>
	public const int ERROR_DS_WRONG_LINKED_ATT_SYNTAX = 8528;

	/// <summary>
	///     Security Account Manager needs to get the boot password.
	/// </summary>
	public const int ERROR_DS_SAM_NEED_BOOTKEY_PASSUInt16 = 8529;

	/// <summary>
	///     Security Account Manager needs to get the boot key from floppy disk.
	/// </summary>
	public const int ERROR_DS_SAM_NEED_BOOTKEY_FLOPPY = 8530;

	/// <summary>
	///     Directory Service cannot start.
	/// </summary>
	public const int ERROR_DS_CANT_START = 8531;

	/// <summary>
	///     Directory Services could not start.
	/// </summary>
	public const int ERROR_DS_INIT_FAILURE = 8532;

	/// <summary>
	///     The connection between client and server requires packet privacy or better.
	/// </summary>
	public const int ERROR_DS_NO_PKT_PRIVACY_ON_CONNECTION = 8533;

	/// <summary>
	///     The source domain may not be in the same forest as destination.
	/// </summary>
	public const int ERROR_DS_SOURCE_DOMAIN_IN_FOREST = 8534;

	/// <summary>
	///     The destination domain must be in the forest.
	/// </summary>
	public const int ERROR_DS_DESTINATION_DOMAIN_NOT_IN_FOREST = 8535;

	/// <summary>
	///     The operation requires that destination domain auditing be enabled.
	/// </summary>
	public const int ERROR_DS_DESTINATION_AUDITING_NOT_ENABLED = 8536;

	/// <summary>
	///     The operation couldn't locate a DC for the source domain.
	/// </summary>
	public const int ERROR_DS_CANT_FIND_DC_FOR_SRC_DOMAIN = 8537;

	/// <summary>
	///     The source object must be a group or user.
	/// </summary>
	public const int ERROR_DS_SRC_OBJ_NOT_GROUP_OR_USER = 8538;

	/// <summary>
	///     The source object's SID already exists in destination forest.
	/// </summary>
	public const int ERROR_DS_SRC_SID_EXISTS_IN_FOREST = 8539;

	/// <summary>
	///     The source and destination object must be of the same type.
	/// </summary>
	public const int ERROR_DS_SRC_AND_DST_OBJECT_CLASS_MISMATCH = 8540;

	/// <summary>
	///     Security Accounts Manager initialization failed because of the following error: %1.
	///     Error Status: 0x%2. Click OK to shut down the system and reboot into Safe Mode. Check the event log for detailed
	///     information.
	/// </summary>
	public const int ERROR_SAM_INIT_FAILURE = 8541;

	/// <summary>
	///     Schema information could not be included in the replication request.
	/// </summary>
	public const int ERROR_DS_DRA_SCHEMA_INFO_SHIP = 8542;

	/// <summary>
	///     The replication operation could not be completed due to a schema incompatibility.
	/// </summary>
	public const int ERROR_DS_DRA_SCHEMA_CONFLICT = 8543;

	/// <summary>
	///     The replication operation could not be completed due to a previous schema incompatibility.
	/// </summary>
	public const int ERROR_DS_DRA_EARLIER_SCHEMA_CONFLICT = 8544;

	/// <summary>
	///     The replication update could not be applied because either the source or the destination has not yet received
	///     information regarding a recent cross-domain move operation.
	/// </summary>
	public const int ERROR_DS_DRA_OBJ_NC_MISMATCH = 8545;

	/// <summary>
	///     The requested domain could not be deleted because there exist domain controllers that still host this domain.
	/// </summary>
	public const int ERROR_DS_NC_STILL_HAS_DSAS = 8546;

	/// <summary>
	///     The requested operation can be performed only on a global catalog server.
	/// </summary>
	public const int ERROR_DS_GC_REQUIRED = 8547;

	/// <summary>
	///     A local group can only be a member of other local groups in the same domain.
	/// </summary>
	public const int ERROR_DS_LOCAL_MEMBER_OF_LOCAL_ONLY = 8548;

	/// <summary>
	///     Foreign security principals cannot be members of universal groups.
	/// </summary>
	public const int ERROR_DS_NO_FPO_IN_UNIVERSAL_GROUPS = 8549;

	/// <summary>
	///     The attribute is not allowed to be replicated to the GC because of security reasons.
	/// </summary>
	public const int ERROR_DS_CANT_ADD_TO_GC = 8550;

	/// <summary>
	///     The checkpoint with the PDC could not be taken because there too many modifications being processed currently.
	/// </summary>
	public const int ERROR_DS_NO_CHECKPOINT_WITH_PDC = 8551;

	/// <summary>
	///     The operation requires that source domain auditing be enabled.
	/// </summary>
	public const int ERROR_DS_SOURCE_AUDITING_NOT_ENABLED = 8552;

	/// <summary>
	///     Security principal objects can only be created inside domain naming contexts.
	/// </summary>
	public const int ERROR_DS_CANT_CREATE_IN_NONDOMAIN_NC = 8553;

	/// <summary>
	///     A Service Principal Name (SPN) could not be constructed because the provided hostname is not in the necessary
	///     format.
	/// </summary>
	public const int ERROR_DS_INVALID_NAME_FOR_SPN = 8554;

	/// <summary>
	///     A Filter was passed that uses constructed attributes.
	/// </summary>
	public const int ERROR_DS_FILTER_USES_CONTRUCTED_ATTRS = 8555;

	/// <summary>
	///     The unicodePwd attribute value must be enclosed in double quotes.
	/// </summary>
	public const int ERROR_DS_UNICODEPWD_NOT_IN_QUOTES = 8556;

	/// <summary>
	///     Your computer could not be joined to the domain. You have exceeded the maximum number of computer accounts you are
	///     allowed to create in this domain. Contact your system administrator to have this limit reset or increased.
	/// </summary>
	public const int ERROR_DS_MACHINE_ACCOUNT_QUOTA_EXCEEDED = 8557;

	/// <summary>
	///     For security reasons, the operation must be run on the destination DC.
	/// </summary>
	public const int ERROR_DS_MUST_BE_RUN_ON_DST_DC = 8558;

	/// <summary>
	///     For security reasons, the source DC must be NT4SP4 or greater.
	/// </summary>
	public const int ERROR_DS_SRC_DC_MUST_BE_SP4_OR_GREATER = 8559;

	/// <summary>
	///     Critical Directory Service System objects cannot be deleted during tree delete operations.  The tree delete may
	///     have been partially performed.
	/// </summary>
	public const int ERROR_DS_CANT_TREE_DELETE_CRITICAL_OBJ = 8560;

	/// <summary>
	///     Directory Services could not start because of the following error: %1.
	///     Error Status: 0x%2. Please click OK to shutdown the system. You can use the recovery console to diagnose the system
	///     further.
	/// </summary>
	public const int ERROR_DS_INIT_FAILURE_CONSOLE = 8561;

	/// <summary>
	///     Security Accounts Manager initialization failed because of the following error: %1.
	///     Error Status: 0x%2. Please click OK to shutdown the system. You can use the recovery console to diagnose the system
	///     further.
	/// </summary>
	public const int ERROR_DS_SAM_INIT_FAILURE_CONSOLE = 8562;

	/// <summary>
	///     This version of Windows is too old to support the current directory forest behavior.  You must upgrade the
	///     operating system on this server before it can become a domain controller in this forest.
	/// </summary>
	public const int ERROR_DS_FOREST_VERSION_TOO_HIGH = 8563;

	/// <summary>
	///     This version of Windows is too old to support the current domain behavior.  You must upgrade the operating system
	///     on this server before it can become a domain controller in this domain.
	/// </summary>
	public const int ERROR_DS_DOMAIN_VERSION_TOO_HIGH = 8564;

	/// <summary>
	///     This version of Windows no longer supports the behavior version in use in this directory forest.  You must advance
	///     the forest behavior version before this server can become a domain controller in the forest.
	/// </summary>
	public const int ERROR_DS_FOREST_VERSION_TOO_LOW = 8565;

	/// <summary>
	///     This version of Windows no longer supports the behavior version in use in this domain.  You must advance the domain
	///     behavior version before this server can become a domain controller in the domain.
	/// </summary>
	public const int ERROR_DS_DOMAIN_VERSION_TOO_LOW = 8566;

	/// <summary>
	///     The version of Windows is incompatible with the behavior version of the domain or forest.
	/// </summary>
	public const int ERROR_DS_INCOMPATIBLE_VERSION = 8567;

	/// <summary>
	///     The behavior version cannot be increased to the requested value because Domain Controllers still exist with
	///     versions lower than the requested value.
	/// </summary>
	public const int ERROR_DS_LOW_DSA_VERSION = 8568;

	/// <summary>
	///     The behavior version value cannot be increased while the domain is still in mixed domain mode.  You must first
	///     change the domain to native mode before increasing the behavior version.
	/// </summary>
	public const int ERROR_DS_NO_BEHAVIOR_VERSION_IN_MIXEDDOMAIN = 8569;

	/// <summary>
	///     The sort order requested is not supported.
	/// </summary>
	public const int ERROR_DS_NOT_SUPPORTED_SORT_ORDER = 8570;

	/// <summary>
	///     Found an object with a non unique name.
	/// </summary>
	public const int ERROR_DS_NAME_NOT_UNIQUE = 8571;

	/// <summary>
	///     The machine account was created pre-NT4.  The account needs to be recreated.
	/// </summary>
	public const int ERROR_DS_MACHINE_ACCOUNT_CREATED_PRENT4 = 8572;

	/// <summary>
	///     The database is out of version store.
	/// </summary>
	public const int ERROR_DS_OUT_OF_VERSION_STORE = 8573;

	/// <summary>
	///     Unable to continue operation because multiple conflicting controls were used.
	/// </summary>
	public const int ERROR_DS_INCOMPATIBLE_CONTROLS_USED = 8574;

	/// <summary>
	///     Unable to find a valid security descriptor reference domain for this partition.
	/// </summary>
	public const int ERROR_DS_NO_REF_DOMAIN = 8575;

	/// <summary>
	///     Schema update failed: The link identifier is reserved.
	/// </summary>
	public const int ERROR_DS_RESERVED_LINK_ID = 8576;

	/// <summary>
	///     Schema update failed: There are no link identifiers available.
	/// </summary>
	public const int ERROR_DS_LINK_ID_NOT_AVAILABLE = 8577;

	/// <summary>
	///     A account group can not have a universal group as a member.
	/// </summary>
	public const int ERROR_DS_AG_CANT_HAVE_UNIVERSAL_MEMBER = 8578;

	/// <summary>
	///     Rename or move operations on naming context heads or read-only objects are not allowed.
	/// </summary>
	public const int ERROR_DS_MODIFYDN_DISALLOWED_BY_INSTANCE_TYPE = 8579;

	/// <summary>
	///     Move operations on objects in the schema naming context are not allowed.
	/// </summary>
	public const int ERROR_DS_NO_OBJECT_MOVE_IN_SCHEMA_NC = 8580;

	/// <summary>
	///     A system flag has been set on the object and does not allow the object to be moved or renamed.
	/// </summary>
	public const int ERROR_DS_MODIFYDN_DISALLOWED_BY_FLAG = 8581;

	/// <summary>
	///     This object is not allowed to change its grandparent container. Moves are not forbidden on this object, but are
	///     restricted to sibling containers.
	/// </summary>
	public const int ERROR_DS_MODIFYDN_WRONG_GRANDPARENT = 8582;

	/// <summary>
	///     Unable to resolve completely, a referral to another forest is generated.
	/// </summary>
	public const int ERROR_DS_NAME_ERROR_TRUST_REFERRAL = 8583;

	/// <summary>
	///     The requested action is not supported on standard server.
	/// </summary>
	public const int ERROR_NOT_SUPPORTED_ON_STANDARD_SERVER = 8584;

	/// <summary>
	///     Could not access a partition of the Active Directory located on a remote server.  Make sure at least one server is
	///     running for the partition in question.
	/// </summary>
	public const int ERROR_DS_CANT_ACCESS_REMOTE_PART_OF_AD = 8585;

	/// <summary>
	///     The directory cannot validate the proposed naming context (or partition) name because it does not hold a replica
	///     nor can it contact a replica of the naming context above the proposed naming context.  Please ensure that the
	///     parent naming context is properly registered in DNS, and at least one replica of this naming context is reachable
	///     by the Domain Naming master.
	/// </summary>
	public const int ERROR_DS_CR_IMPOSSIBLE_TO_VALIDATE_V2 = 8586;

	/// <summary>
	///     The thread limit for this request was exceeded.
	/// </summary>
	public const int ERROR_DS_THREAD_LIMIT_EXCEEDED = 8587;

	/// <summary>
	///     The Global catalog server is not in the closest site.
	/// </summary>
	public const int ERROR_DS_NOT_CLOSEST = 8588;

	/// <summary>
	///     The DS cannot derive a service principal name (SPN) with which to mutually authenticate the target server because
	///     the corresponding server object in the local DS database has no serverReference attribute.
	/// </summary>
	public const int ERROR_DS_CANT_DERIVE_SPN_WITHOUT_SERVER_REF = 8589;

	/// <summary>
	///     The Directory Service failed to enter single user mode.
	/// </summary>
	public const int ERROR_DS_SINGLE_USER_MODE_FAILED = 8590;

	/// <summary>
	///     The Directory Service cannot parse the script because of a syntax error.
	/// </summary>
	public const int ERROR_DS_NTDSCRIPT_SYNTAX_ERROR = 8591;

	/// <summary>
	///     The Directory Service cannot process the script because of an error.
	/// </summary>
	public const int ERROR_DS_NTDSCRIPT_PROCESS_ERROR = 8592;

	/// <summary>
	///     The directory service cannot perform the requested operation because the servers
	///     involved are of different replication epochs (which is usually related to a
	///     domain rename that is in progress).
	/// </summary>
	public const int ERROR_DS_DIFFERENT_REPL_EPOCHS = 8593;

	/// <summary>
	///     The directory service binding must be renegotiated due to a change in the server
	///     extensions information.
	/// </summary>
	public const int ERROR_DS_DRS_EXTENSIONS_CHANGED = 8594;

	/// <summary>
	///     Operation not allowed on a disabled cross ref.
	/// </summary>
	public const int ERROR_DS_REPLICA_SET_CHANGE_NOT_ALLOWED_ON_DISABLED_CR = 8595;

	/// <summary>
	///     Schema update failed: No values for msDS-IntId are available.
	/// </summary>
	public const int ERROR_DS_NO_MSDS_INTID = 8596;

	/// <summary>
	///     Schema update failed: Duplicate msDS-INtId. Retry the operation.
	/// </summary>
	public const int ERROR_DS_DUP_MSDS_INTID = 8597;

	/// <summary>
	///     Schema deletion failed: attribute is used in rDNAttID.
	/// </summary>
	public const int ERROR_DS_EXISTS_IN_RDNATTID = 8598;

	/// <summary>
	///     The directory service failed to authorize the request.
	/// </summary>
	public const int ERROR_DS_AUTHORIZATION_FAILED = 8599;

	/// <summary>
	///     The Directory Service cannot process the script because it is invalid.
	/// </summary>
	public const int ERROR_DS_INVALID_SCRIPT = 8600;

	/// <summary>
	///     The remote create cross reference operation failed on the Domain Naming Master FSMO.  The operation's error is in
	///     the extended data.
	/// </summary>
	public const int ERROR_DS_REMOTE_CROSSREF_OP_FAILED = 8601;

	/// <summary>
	///     No information avialable.
	/// </summary>
	public const int ERROR_DS_CROSS_REF_BUSY = 8602;

	/// <summary>
	///     No information avialable.
	/// </summary>
	public const int ERROR_DS_CANT_DERIVE_SPN_FOR_DELETED_DOMAIN = 8603;

	/// <summary>
	///     No information avialable.
	/// </summary>
	public const int ERROR_DS_CANT_DEMOTE_WITH_WRITEABLE_NC = 8604;

	/// <summary>
	///     No information avialable.
	/// </summary>
	public const int ERROR_DS_DUPLICATE_ID_FOUND = 8605;

	/// <summary>
	///     No information avialable.
	/// </summary>
	public const int ERROR_DS_INSUFFICIENT_ATTR_TO_CREATE_OBJECT = 8606;

	/// <summary>
	///     No information avialable.
	/// </summary>
	public const int ERROR_DS_GROUP_CONVERSION_ERROR = 8607;

	/// <summary>
	///     No information avialable.
	/// </summary>
	public const int ERROR_DS_CANT_MOVE_APP_BASIC_GROUP = 8608;

	/// <summary>
	///     No information avialable.
	/// </summary>
	public const int ERROR_DS_CANT_MOVE_APP_QUERY_GROUP = 8609;

	/// <summary>
	///     No information avialable.
	/// </summary>
	public const int ERROR_DS_ROLE_NOT_VERIFIED = 8610;

	/// <summary>
	///     No information avialable.
	/// </summary>
	public const int ERROR_DS_WKO_CONTAINER_CANNOT_BE_SPECIAL = 8611;

	/// <summary>
	///     No information avialable.
	/// </summary>
	public const int ERROR_DS_DOMAIN_RENAME_IN_PROGRESS = 8612;

	/// <summary>
	///     No information avialable.
	/// </summary>
	public const int ERROR_DS_EXISTING_AD_CHILD_NC = 8613;

	/// <summary>
	///     No information avialable.
	/// </summary>
	public const int ERROR_DS_REPL_LIFETIME_EXCEEDED = 8614;

	/// <summary>
	///     No information avialable.
	/// </summary>
	public const int ERROR_DS_DISALLOWED_IN_SYSTEM_CONTAINER = 8615;

	/// <summary>
	///     No information avialable.
	/// </summary>
	public const int ERROR_DS_LDAP_SEND_QUEUE_FULL = 8616;

	/// <summary>
	///     No information avialable.
	/// </summary>
	public const int DNS_ERROR_RESPONSE_CODES_BASE = 9000;

	/// <summary>
	///     DNS server unable to interpret format.
	/// </summary>
	public const int DNS_ERROR_RCODE_FORMAT_ERROR = 9001;

	/// <summary>
	///     DNS server failure.
	/// </summary>
	public const int DNS_ERROR_RCODE_SERVER_FAILURE = 9002;

	/// <summary>
	///     DNS name does not exist.
	/// </summary>
	public const int DNS_ERROR_RCODE_NAME_ERROR = 9003;

	/// <summary>
	///     DNS request not supported by name server.
	/// </summary>
	public const int DNS_ERROR_RCODE_NOT_IMPLEMENTED = 9004;

	/// <summary>
	///     DNS operation refused.
	/// </summary>
	public const int DNS_ERROR_RCODE_REFUSED = 9005;

	/// <summary>
	///     DNS name that ought not exist, does exist.
	/// </summary>
	public const int DNS_ERROR_RCODE_YXDOMAIN = 9006;

	/// <summary>
	///     DNS RR set that ought not exist, does exist.
	/// </summary>
	public const int DNS_ERROR_RCODE_YXRRSET = 9007;

	/// <summary>
	///     DNS RR set that ought to exist, does not exist.
	/// </summary>
	public const int DNS_ERROR_RCODE_NXRRSET = 9008;

	/// <summary>
	///     DNS server not authoritative for zone.
	/// </summary>
	public const int DNS_ERROR_RCODE_NOTAUTH = 9009;

	/// <summary>
	///     DNS name in update or prereq is not in zone.
	/// </summary>
	public const int DNS_ERROR_RCODE_NOTZONE = 9010;

	/// <summary>
	///     DNS signature failed to verify.
	/// </summary>
	public const int DNS_ERROR_RCODE_BADSIG = 9016;

	/// <summary>
	///     DNS bad key.
	/// </summary>
	public const int DNS_ERROR_RCODE_BADKEY = 9017;

	/// <summary>
	///     DNS signature validity expired.
	/// </summary>
	public const int DNS_ERROR_RCODE_BADTIME = 9018;

	/// <summary>
	///     No information avialable.
	/// </summary>
	public const int DNS_ERROR_PACKET_FMT_BASE = 9500;

	/// <summary>
	///     No records found for given DNS query.
	/// </summary>
	public const int DNS_INFO_NO_RECORDS = 9501;

	/// <summary>
	///     Bad DNS packet.
	/// </summary>
	public const int DNS_ERROR_BAD_PACKET = 9502;

	/// <summary>
	///     No DNS packet.
	/// </summary>
	public const int DNS_ERROR_NO_PACKET = 9503;

	/// <summary>
	///     DNS error, check rcode.
	/// </summary>
	public const int DNS_ERROR_RCODE = 9504;

	/// <summary>
	///     Unsecured DNS packet.
	/// </summary>
	public const int DNS_ERROR_UNSECURE_PACKET = 9505;

	/// <summary>
	///     No information avialable.
	/// </summary>
	public const int DNS_ERROR_NO_MEMORY = ERROR_OUTOFMEMORY;

	/// <summary>
	///     No information avialable.
	/// </summary>
	public const int DNS_ERROR_INVALID_NAME = ERROR_INVALID_NAME;

	/// <summary>
	///     No information avialable.
	/// </summary>
	public const int DNS_ERROR_INVALID_DATA = ERROR_INVALID_DATA;

	/// <summary>
	///     No information avialable.
	/// </summary>
	public const int DNS_ERROR_GENERAL_API_BASE = 9550;

	/// <summary>
	///     Invalid DNS type.
	/// </summary>
	public const int DNS_ERROR_INVALID_TYPE = 9551;

	/// <summary>
	///     Invalid IP address.
	/// </summary>
	public const int DNS_ERROR_INVALID_IP_ADDRESS = 9552;

	/// <summary>
	///     Invalid property.
	/// </summary>
	public const int DNS_ERROR_INVALID_PROPERTY = 9553;

	/// <summary>
	///     Try DNS operation again later.
	/// </summary>
	public const int DNS_ERROR_TRY_AGAIN_LATER = 9554;

	/// <summary>
	///     Record for given name and type is not unique.
	/// </summary>
	public const int DNS_ERROR_NOT_UNIQUE = 9555;

	/// <summary>
	///     DNS name does not comply with RFC specifications.
	/// </summary>
	public const int DNS_ERROR_NON_RFC_NAME = 9556;

	/// <summary>
	///     DNS name is a fully-qualified DNS name.
	/// </summary>
	public const int DNS_STATUS_FQDN = 9557;

	/// <summary>
	///     DNS name is dotted (multi-label).
	/// </summary>
	public const int DNS_STATUS_DOTTED_NAME = 9558;

	/// <summary>
	///     DNS name is a single-part name.
	/// </summary>
	public const int DNS_STATUS_SINGLE_PART_NAME = 9559;

	/// <summary>
	///     DNS name contains an invalid character.
	/// </summary>
	public const int DNS_ERROR_INVALID_NAME_CHAR = 9560;

	/// <summary>
	///     DNS name is entirely numeric.
	/// </summary>
	public const int DNS_ERROR_NUMERIC_NAME = 9561;

	/// <summary>
	///     The operation requested is not permitted on a DNS root server.
	/// </summary>
	public const int DNS_ERROR_NOT_ALLOWED_ON_ROOT_SERVER = 9562;

	/// <summary>
	///     No information avialable.
	/// </summary>
	public const int DNS_ERROR_NOT_ALLOWED_UNDER_DELEGATION = 9563;

	/// <summary>
	///     No information avialable.
	/// </summary>
	public const int DNS_ERROR_CANNOT_FIND_ROOT_HINTS = 9564;

	/// <summary>
	///     No information avialable.
	/// </summary>
	public const int DNS_ERROR_INCONSISTENT_ROOT_HINTS = 9565;

	/// <summary>
	///     No information avialable.
	/// </summary>
	public const int DNS_ERROR_ZONE_BASE = 9600;

	/// <summary>
	///     DNS zone does not exist.
	/// </summary>
	public const int DNS_ERROR_ZONE_DOES_NOT_EXIST = 9601;

	/// <summary>
	///     DNS zone information not available.
	/// </summary>
	public const int DNS_ERROR_NO_ZONE_INFO = 9602;

	/// <summary>
	///     Invalid operation for DNS zone.
	/// </summary>
	public const int DNS_ERROR_INVALID_ZONE_OPERATION = 9603;

	/// <summary>
	///     Invalid DNS zone configuration.
	/// </summary>
	public const int DNS_ERROR_ZONE_CONFIGURATION_ERROR = 9604;

	/// <summary>
	///     DNS zone has no start of authority (SOA) record.
	/// </summary>
	public const int DNS_ERROR_ZONE_HAS_NO_SOA_RECORD = 9605;

	/// <summary>
	///     DNS zone has no Name Server (NS) record.
	/// </summary>
	public const int DNS_ERROR_ZONE_HAS_NO_NS_RECORDS = 9606;

	/// <summary>
	///     DNS zone is locked.
	/// </summary>
	public const int DNS_ERROR_ZONE_LOCKED = 9607;

	/// <summary>
	///     DNS zone creation failed.
	/// </summary>
	public const int DNS_ERROR_ZONE_CREATION_FAILED = 9608;

	/// <summary>
	///     DNS zone already exists.
	/// </summary>
	public const int DNS_ERROR_ZONE_ALREADY_EXISTS = 9609;

	/// <summary>
	///     DNS automatic zone already exists.
	/// </summary>
	public const int DNS_ERROR_AUTOZONE_ALREADY_EXISTS = 9610;

	/// <summary>
	///     Invalid DNS zone type.
	/// </summary>
	public const int DNS_ERROR_INVALID_ZONE_TYPE = 9611;

	/// <summary>
	///     Secondary DNS zone requires master IP address.
	/// </summary>
	public const int DNS_ERROR_SECONDARY_REQUIRES_MASTER_IP = 9612;

	/// <summary>
	///     DNS zone not secondary.
	/// </summary>
	public const int DNS_ERROR_ZONE_NOT_SECONDARY = 9613;

	/// <summary>
	///     Need secondary IP address.
	/// </summary>
	public const int DNS_ERROR_NEED_SECONDARY_ADDRESSES = 9614;

	/// <summary>
	///     WINS initialization failed.
	/// </summary>
	public const int DNS_ERROR_WINS_INIT_FAILED = 9615;

	/// <summary>
	///     Need WINS servers.
	/// </summary>
	public const int DNS_ERROR_NEED_WINS_SERVERS = 9616;

	/// <summary>
	///     NBTSTAT initialization call failed.
	/// </summary>
	public const int DNS_ERROR_NBSTAT_INIT_FAILED = 9617;

	/// <summary>
	///     Invalid delete of start of authority (SOA)
	/// </summary>
	public const int DNS_ERROR_SOA_DELETE_INVALID = 9618;

	/// <summary>
	///     A conditional forwarding zone already exists for that name.
	/// </summary>
	public const int DNS_ERROR_FORWARDER_ALREADY_EXISTS = 9619;

	/// <summary>
	///     This zone must be configured with one or more master DNS server IP addresses.
	/// </summary>
	public const int DNS_ERROR_ZONE_REQUIRES_MASTER_IP = 9620;

	/// <summary>
	///     The operation cannot be performed because this zone is shutdown.
	/// </summary>
	public const int DNS_ERROR_ZONE_IS_SHUTDOWN = 9621;

	/// <summary>
	///     No information avialable.
	/// </summary>
	public const int DNS_ERROR_DATAFILE_BASE = 9650;

	/// <summary>
	///     Primary DNS zone requires datafile.
	/// </summary>
	public const int DNS_ERROR_PRIMARY_REQUIRES_DATAFILE = 9651;

	/// <summary>
	///     Invalid datafile name for DNS zone.
	/// </summary>
	public const int DNS_ERROR_INVALID_DATAFILE_NAME = 9652;

	/// <summary>
	///     Failed to open datafile for DNS zone.
	/// </summary>
	public const int DNS_ERROR_DATAFILE_OPEN_FAILURE = 9653;

	/// <summary>
	///     Failed to write datafile for DNS zone.
	/// </summary>
	public const int DNS_ERROR_FILE_WRITEBACK_FAILED = 9654;

	/// <summary>
	///     Failure while reading datafile for DNS zone.
	/// </summary>
	public const int DNS_ERROR_DATAFILE_PARSING = 9655;

	/// <summary>
	///     No information avialable.
	/// </summary>
	public const int DNS_ERROR_DATABASE_BASE = 9700;

	/// <summary>
	///     DNS record does not exist.
	/// </summary>
	public const int DNS_ERROR_RECORD_DOES_NOT_EXIST = 9701;

	/// <summary>
	///     DNS record format error.
	/// </summary>
	public const int DNS_ERROR_RECORD_FORMAT = 9702;

	/// <summary>
	///     Node creation failure in DNS.
	/// </summary>
	public const int DNS_ERROR_NODE_CREATION_FAILED = 9703;

	/// <summary>
	///     Unknown DNS record type.
	/// </summary>
	public const int DNS_ERROR_UNKNOWN_RECORD_TYPE = 9704;

	/// <summary>
	///     DNS record timed out.
	/// </summary>
	public const int DNS_ERROR_RECORD_TIMED_OUT = 9705;

	/// <summary>
	///     Name not in DNS zone.
	/// </summary>
	public const int DNS_ERROR_NAME_NOT_IN_ZONE = 9706;

	/// <summary>
	///     CNAME loop detected.
	/// </summary>
	public const int DNS_ERROR_CNAME_LOOP = 9707;

	/// <summary>
	///     Node is a CNAME DNS record.
	/// </summary>
	public const int DNS_ERROR_NODE_IS_CNAME = 9708;

	/// <summary>
	///     A CNAME record already exists for given name.
	/// </summary>
	public const int DNS_ERROR_CNAME_COLLISION = 9709;

	/// <summary>
	///     Record only at DNS zone root.
	/// </summary>
	public const int DNS_ERROR_RECORD_ONLY_AT_ZONE_ROOT = 9710;

	/// <summary>
	///     DNS record already exists.
	/// </summary>
	public const int DNS_ERROR_RECORD_ALREADY_EXISTS = 9711;

	/// <summary>
	///     Secondary DNS zone data error.
	/// </summary>
	public const int DNS_ERROR_SECONDARY_DATA = 9712;

	/// <summary>
	///     Could not create DNS cache data.
	/// </summary>
	public const int DNS_ERROR_NO_CREATE_CACHE_DATA = 9713;

	/// <summary>
	///     DNS name does not exist.
	/// </summary>
	public const int DNS_ERROR_NAME_DOES_NOT_EXIST = 9714;

	/// <summary>
	///     Could not create pointer (PTR) record.
	/// </summary>
	public const int DNS_WARNING_PTR_CREATE_FAILED = 9715;

	/// <summary>
	///     DNS domain was undeleted.
	/// </summary>
	public const int DNS_WARNING_DOMAIN_UNDELETED = 9716;

	/// <summary>
	///     The directory service is unavailable.
	/// </summary>
	public const int DNS_ERROR_DS_UNAVAILABLE = 9717;

	/// <summary>
	///     DNS zone already exists in the directory service.
	/// </summary>
	public const int DNS_ERROR_DS_ZONE_ALREADY_EXISTS = 9718;

	/// <summary>
	///     DNS server not creating or reading the boot file for the directory service integrated DNS zone.
	/// </summary>
	public const int DNS_ERROR_NO_BOOTFILE_IF_DS_ZONE = 9719;

	/// <summary>
	///     No information avialable.
	/// </summary>
	public const int DNS_ERROR_OPERATION_BASE = 9750;

	/// <summary>
	///     DNS AXFR (zone transfer) complete.
	/// </summary>
	public const int DNS_INFO_AXFR_COMPLETE = 9751;

	/// <summary>
	///     DNS zone transfer failed.
	/// </summary>
	public const int DNS_ERROR_AXFR = 9752;

	/// <summary>
	///     Added local WINS server.
	/// </summary>
	public const int DNS_INFO_ADDED_LOCAL_WINS = 9753;

	/// <summary>
	///     No information avialable.
	/// </summary>
	public const int DNS_ERROR_SECURE_BASE = 9800;

	/// <summary>
	///     Secure update call needs to continue update request.
	/// </summary>
	public const int DNS_STATUS_CONTINUE_NEEDED = 9801;

	/// <summary>
	///     No information avialable.
	/// </summary>
	public const int DNS_ERROR_SETUP_BASE = 9850;

	/// <summary>
	///     TCP/IP network protocol not installed.
	/// </summary>
	public const int DNS_ERROR_NO_TCPIP = 9851;

	/// <summary>
	///     No DNS servers configured for local system.
	/// </summary>
	public const int DNS_ERROR_NO_DNS_SERVERS = 9852;

	/// <summary>
	///     No information avialable.
	/// </summary>
	public const int DNS_ERROR_DP_BASE = 9900;

	/// <summary>
	///     The specified directory partition does not exist.
	/// </summary>
	public const int DNS_ERROR_DP_DOES_NOT_EXIST = 9901;

	/// <summary>
	///     The specified directory partition already exists.
	/// </summary>
	public const int DNS_ERROR_DP_ALREADY_EXISTS = 9902;

	/// <summary>
	///     The DS is not enlisted in the specified directory partition.
	/// </summary>
	public const int DNS_ERROR_DP_NOT_ENLISTED = 9903;

	/// <summary>
	///     The DS is already enlisted in the specified directory partition.
	/// </summary>
	public const int DNS_ERROR_DP_ALREADY_ENLISTED = 9904;

	/// <summary>
	///     No information avialable.
	/// </summary>
	public const int DNS_ERROR_DP_NOT_AVAILABLE = 9905;

	/// <summary>
	///     No information avialable.
	/// </summary>
	public const int WSABASEERR = 10000;

	/// <summary>
	///     A blocking operation was interrupted by a call to WSACancelBlockingCall.
	/// </summary>
	public const int WSAEINTR = 10004;

	/// <summary>
	///     The file handle supplied is not valid.
	/// </summary>
	public const int WSAEBADF = 10009;

	/// <summary>
	///     An attempt was made to access a socket in a way forbidden by its access permissions.
	/// </summary>
	public const int WSAEACCES = 10013;

	/// <summary>
	///     The system detected an invalid pointer address in attempting to use a pointer argument in a call.
	/// </summary>
	public const int WSAEFAULT = 10014;

	/// <summary>
	///     An invalid argument was supplied.
	/// </summary>
	public const int WSAEINVAL = 10022;

	/// <summary>
	///     Too many open sockets.
	/// </summary>
	public const int WSAEMFILE = 10024;

	/// <summary>
	///     A non-blocking socket operation could not be completed immediately.
	/// </summary>
	public const int WSAEWOULDBLOCK = 10035;

	/// <summary>
	///     A blocking operation is currently executing.
	/// </summary>
	public const int WSAEINPROGRESS = 10036;

	/// <summary>
	///     An operation was attempted on a non-blocking socket that already had an operation in progress.
	/// </summary>
	public const int WSAEALREADY = 10037;

	/// <summary>
	///     An operation was attempted on something that is not a socket.
	/// </summary>
	public const int WSAENOTSOCK = 10038;

	/// <summary>
	///     A required address was omitted from an operation on a socket.
	/// </summary>
	public const int WSAEDESTADDRREQ = 10039;

	/// <summary>
	///     A message sent on a datagram socket was larger than the internal message buffer or some other network limit, or the
	///     buffer used to receive a datagram into was smaller than the datagram itself.
	/// </summary>
	public const int WSAEMSGSIZE = 10040;

	/// <summary>
	///     A protocol was specified in the socket function call that does not support the semantics of the socket type
	///     requested.
	/// </summary>
	public const int WSAEPROTOTYPE = 10041;

	/// <summary>
	///     An unknown, invalid, or unsupported option or level was specified in a getsockopt or setsockopt call.
	/// </summary>
	public const int WSAENOPROTOOPT = 10042;

	/// <summary>
	///     The requested protocol has not been configured into the system, or no implementation for it exists.
	/// </summary>
	public const int WSAEPROTONOSUPPORT = 10043;

	/// <summary>
	///     The support for the specified socket type does not exist in this address family.
	/// </summary>
	public const int WSAESOCKTNOSUPPORT = 10044;

	/// <summary>
	///     The attempted operation is not supported for the type of object referenced.
	/// </summary>
	public const int WSAEOPNOTSUPP = 10045;

	/// <summary>
	///     The protocol family has not been configured into the system or no implementation for it exists.
	/// </summary>
	public const int WSAEPFNOSUPPORT = 10046;

	/// <summary>
	///     An address incompatible with the requested protocol was used.
	/// </summary>
	public const int WSAEAFNOSUPPORT = 10047;

	/// <summary>
	///     Only one usage of each socket address (protocol/network address/port) is normally permitted.
	/// </summary>
	public const int WSAEADDRINUSE = 10048;

	/// <summary>
	///     The requested address is not valid in its context.
	/// </summary>
	public const int WSAEADDRNOTAVAIL = 10049;

	/// <summary>
	///     A socket operation encountered a dead network.
	/// </summary>
	public const int WSAENETDOWN = 10050;

	/// <summary>
	///     A socket operation was attempted to an unreachable network.
	/// </summary>
	public const int WSAENETUNREACH = 10051;

	/// <summary>
	///     The connection has been broken due to keep-alive activity detecting a failure while the operation was in progress.
	/// </summary>
	public const int WSAENETRESET = 10052;

	/// <summary>
	///     An established connection was aborted by the software in your host machine.
	/// </summary>
	public const int WSAECONNABORTED = 10053;

	/// <summary>
	///     An existing connection was forcibly closed by the remote host.
	/// </summary>
	public const int WSAECONNRESET = 10054;

	/// <summary>
	///     An operation on a socket could not be performed because the system lacked sufficient buffer space or because a
	///     queue was full.
	/// </summary>
	public const int WSAENOBUFS = 10055;

	/// <summary>
	///     A connect request was made on an already connected socket.
	/// </summary>
	public const int WSAEISCONN = 10056;

	/// <summary>
	///     A request to send or receive data was disallowed because the socket is not connected and (when sending on a
	///     datagram socket using a sendto call) no address was supplied.
	/// </summary>
	public const int WSAENOTCONN = 10057;

	/// <summary>
	///     A request to send or receive data was disallowed because the socket had already been shut down in that direction
	///     with a previous shutdown call.
	/// </summary>
	public const int WSAESHUTDOWN = 10058;

	/// <summary>
	///     Too many references to some kernel object.
	/// </summary>
	public const int WSAETOOMANYREFS = 10059;

	/// <summary>
	///     A connection attempt failed because the connected party did not properly respond after a period of time, or
	///     established connection failed because connected host has failed to respond.
	/// </summary>
	public const int WSAETIMEDOUT = 10060;

	/// <summary>
	///     No connection could be made because the target machine actively refused it.
	/// </summary>
	public const int WSAECONNREFUSED = 10061;

	/// <summary>
	///     Cannot translate name.
	/// </summary>
	public const int WSAELOOP = 10062;

	/// <summary>
	///     Name component or name was too long.
	/// </summary>
	public const int WSAENAMETOOInt32 = 10063;

	/// <summary>
	///     A socket operation failed because the destination host was down.
	/// </summary>
	public const int WSAEHOSTDOWN = 10064;

	/// <summary>
	///     A socket operation was attempted to an unreachable host.
	/// </summary>
	public const int WSAEHOSTUNREACH = 10065;

	/// <summary>
	///     Cannot remove a directory that is not empty.
	/// </summary>
	public const int WSAENOTEMPTY = 10066;

	/// <summary>
	///     A Windows Sockets implementation may have a limit on the number of applications that may use it simultaneously.
	/// </summary>
	public const int WSAEPROCLIM = 10067;

	/// <summary>
	///     Ran out of quota.
	/// </summary>
	public const int WSAEUSERS = 10068;

	/// <summary>
	///     Ran out of disk quota.
	/// </summary>
	public const int WSAEDQUOT = 10069;

	/// <summary>
	///     File handle reference is no longer available.
	/// </summary>
	public const int WSAESTALE = 10070;

	/// <summary>
	///     Item is not available locally.
	/// </summary>
	public const int WSAEREMOTE = 10071;

	/// <summary>
	///     WSAStartup cannot function at this time because the underlying system it uses to provide network services is
	///     currently unavailable.
	/// </summary>
	public const int WSASYSNOTREADY = 10091;

	/// <summary>
	///     The Windows Sockets version requested is not supported.
	/// </summary>
	public const int WSAVERNOTSUPPORTED = 10092;

	/// <summary>
	///     Either the application has not called WSAStartup, or WSAStartup failed.
	/// </summary>
	public const int WSANOTINITIALISED = 10093;

	/// <summary>
	///     Returned by WSARecv or WSARecvFrom to indicate the remote party has initiated a graceful shutdown sequence.
	/// </summary>
	public const int WSAEDISCON = 10101;

	/// <summary>
	///     No more results can be returned by WSALookupServiceNext.
	/// </summary>
	public const int WSAENOMORE = 10102;

	/// <summary>
	///     A call to WSALookupServiceEnd was made while this call was still processing. The call has been canceled.
	/// </summary>
	public const int WSAECANCELLED = 10103;

	/// <summary>
	///     The procedure call table is invalid.
	/// </summary>
	public const int WSAEINVALIDPROCTABLE = 10104;

	/// <summary>
	///     The requested service provider is invalid.
	/// </summary>
	public const int WSAEINVALIDPROVIDER = 10105;

	/// <summary>
	///     The requested service provider could not be loaded or initialized.
	/// </summary>
	public const int WSAEPROVIDERFAILEDINIT = 10106;

	/// <summary>
	///     A system call that should never fail has failed.
	/// </summary>
	public const int WSASYSCALLFAILURE = 10107;

	/// <summary>
	///     No such service is known. The service cannot be found in the specified name space.
	/// </summary>
	public const int WSASERVICE_NOT_FOUND = 10108;

	/// <summary>
	///     The specified class was not found.
	/// </summary>
	public const int WSATYPE_NOT_FOUND = 10109;

	/// <summary>
	///     No more results can be returned by WSALookupServiceNext.
	/// </summary>
	public const int WSA_E_NO_MORE = 10110;

	/// <summary>
	///     A call to WSALookupServiceEnd was made while this call was still processing. The call has been canceled.
	/// </summary>
	public const int WSA_E_CANCELLED = 10111;

	/// <summary>
	///     A database query failed because it was actively refused.
	/// </summary>
	public const int WSAEREFUSED = 10112;

	/// <summary>
	///     No such host is known.
	/// </summary>
	public const int WSAHOST_NOT_FOUND = 11001;

	/// <summary>
	///     This is usually a temporary error during hostname resolution and means that the local server did not receive a
	///     response from an authoritative server.
	/// </summary>
	public const int WSATRY_AGAIN = 11002;

	/// <summary>
	///     A non-recoverable error occurred during a database lookup.
	/// </summary>
	public const int WSANO_RECOVERY = 11003;

	/// <summary>
	///     The requested name is valid and was found in the database, but it does not have the correct associated data being
	///     resolved for.
	/// </summary>
	public const int WSANO_DATA = 11004;

	/// <summary>
	///     At least one reserve has arrived.
	/// </summary>
	public const int WSA_QOS_RECEIVERS = 11005;

	/// <summary>
	///     At least one path has arrived.
	/// </summary>
	public const int WSA_QOS_SENDERS = 11006;

	/// <summary>
	///     There are no senders.
	/// </summary>
	public const int WSA_QOS_NO_SENDERS = 11007;

	/// <summary>
	///     There are no receivers.
	/// </summary>
	public const int WSA_QOS_NO_RECEIVERS = 11008;

	/// <summary>
	///     Reserve has been confirmed.
	/// </summary>
	public const int WSA_QOS_REQUEST_CONFIRMED = 11009;

	/// <summary>
	///     Error due to lack of resources.
	/// </summary>
	public const int WSA_QOS_ADMISSION_FAILURE = 11010;

	/// <summary>
	///     Rejected for administrative reasons - bad credentials.
	/// </summary>
	public const int WSA_QOS_POLICY_FAILURE = 11011;

	/// <summary>
	///     Unknown or conflicting style.
	/// </summary>
	public const int WSA_QOS_BAD_STYLE = 11012;

	/// <summary>
	///     Problem with some part of the filterspec or providerspecific buffer in general.
	/// </summary>
	public const int WSA_QOS_BAD_OBJECT = 11013;

	/// <summary>
	///     Problem with some part of the flowspec.
	/// </summary>
	public const int WSA_QOS_TRAFFIC_CTRL_ERROR = 11014;

	/// <summary>
	///     General QOS error.
	/// </summary>
	public const int WSA_QOS_GENERIC_ERROR = 11015;

	/// <summary>
	///     An invalid or unrecognized service type was found in the flowspec.
	/// </summary>
	public const int WSA_QOS_ESERVICETYPE = 11016;

	/// <summary>
	///     An invalid or inconsistent flowspec was found in the QOS structure.
	/// </summary>
	public const int WSA_QOS_EFLOWSPEC = 11017;

	/// <summary>
	///     Invalid QOS provider-specific buffer.
	/// </summary>
	public const int WSA_QOS_EPROVSPECBUF = 11018;

	/// <summary>
	///     An invalid QOS filter style was used.
	/// </summary>
	public const int WSA_QOS_EFILTERSTYLE = 11019;

	/// <summary>
	///     An invalid QOS filter type was used.
	/// </summary>
	public const int WSA_QOS_EFILTERTYPE = 11020;

	/// <summary>
	///     An incorrect number of QOS FILTERSPECs were specified in the FLOWDESCRIPTOR.
	/// </summary>
	public const int WSA_QOS_EFILTERCOUNT = 11021;

	/// <summary>
	///     An object with an invalid ObjectLength field was specified in the QOS provider-specific buffer.
	/// </summary>
	public const int WSA_QOS_EOBJLENGTH = 11022;

	/// <summary>
	///     An incorrect number of flow descriptors was specified in the QOS structure.
	/// </summary>
	public const int WSA_QOS_EFLOWCOUNT = 11023;

	/// <summary>
	///     An unrecognized object was found in the QOS provider-specific buffer.
	/// </summary>
	public const int WSA_QOS_EUNKOWNPSOBJ = 11024;

	/// <summary>
	///     An invalid policy object was found in the QOS provider-specific buffer.
	/// </summary>
	public const int WSA_QOS_EPOLICYOBJ = 11025;

	/// <summary>
	///     An invalid QOS flow descriptor was found in the flow descriptor list.
	/// </summary>
	public const int WSA_QOS_EFLOWDESC = 11026;

	/// <summary>
	///     An invalid or inconsistent flowspec was found in the QOS provider specific buffer.
	/// </summary>
	public const int WSA_QOS_EPSFLOWSPEC = 11027;

	/// <summary>
	///     An invalid FILTERSPEC was found in the QOS provider-specific buffer.
	/// </summary>
	public const int WSA_QOS_EPSFILTERSPEC = 11028;

	/// <summary>
	///     An invalid shape discard mode object was found in the QOS provider specific buffer.
	/// </summary>
	public const int WSA_QOS_ESDMODEOBJ = 11029;

	/// <summary>
	///     An invalid shaping rate object was found in the QOS provider-specific buffer.
	/// </summary>
	public const int WSA_QOS_ESHAPERATEOBJ = 11030;

	/// <summary>
	///     A reserved policy element was found in the QOS provider-specific buffer.
	/// </summary>
	public const int WSA_QOS_RESERVED_PETYPE = 11031;

	/// <summary>
	///     The requested section was not present in the activation context.
	/// </summary>
	public const int ERROR_SXS_SECTION_NOT_FOUND = 14000;

	/// <summary>
	///     This application has failed to start because the application configuration is incorrect. Reinstalling the
	///     application may fix this problem.
	/// </summary>
	public const int ERROR_SXS_CANT_GEN_ACTCTX = 14001;

	/// <summary>
	///     The application binding data format is invalid.
	/// </summary>
	public const int ERROR_SXS_INVALID_ACTCTXDATA_FORMAT = 14002;

	/// <summary>
	///     The referenced assembly is not installed on your system.
	/// </summary>
	public const int ERROR_SXS_ASSEMBLY_NOT_FOUND = 14003;

	/// <summary>
	///     The manifest file does not begin with the required tag and format information.
	/// </summary>
	public const int ERROR_SXS_MANIFEST_FORMAT_ERROR = 14004;

	/// <summary>
	///     The manifest file contains one or more syntax errors.
	/// </summary>
	public const int ERROR_SXS_MANIFEST_PARSE_ERROR = 14005;

	/// <summary>
	///     The application attempted to activate a disabled activation context.
	/// </summary>
	public const int ERROR_SXS_ACTIVATION_CONTEXT_DISABLED = 14006;

	/// <summary>
	///     The requested lookup key was not found in any active activation context.
	/// </summary>
	public const int ERROR_SXS_KEY_NOT_FOUND = 14007;

	/// <summary>
	///     A component version required by the application conflicts with another component version already active.
	/// </summary>
	public const int ERROR_SXS_VERSION_CONFLICT = 14008;

	/// <summary>
	///     The type requested activation context section does not match the query API used.
	/// </summary>
	public const int ERROR_SXS_WRONG_SECTION_TYPE = 14009;

	/// <summary>
	///     Lack of system resources has required isolated activation to be disabled for the current thread of execution.
	/// </summary>
	public const int ERROR_SXS_THREAD_QUERIES_DISABLED = 14010;

	/// <summary>
	///     An attempt to set the process default activation context failed because the process default activation context was
	///     already set.
	/// </summary>
	public const int ERROR_SXS_PROCESS_DEFAULT_ALREADY_SET = 14011;

	/// <summary>
	///     The encoding group identifier specified is not recognized.
	/// </summary>
	public const int ERROR_SXS_UNKNOWN_ENCODING_GROUP = 14012;

	/// <summary>
	///     The encoding requested is not recognized.
	/// </summary>
	public const int ERROR_SXS_UNKNOWN_ENCODING = 14013;

	/// <summary>
	///     The manifest contains a reference to an invalid URI.
	/// </summary>
	public const int ERROR_SXS_INVALID_XML_NAMESPACE_URI = 14014;

	/// <summary>
	///     The application manifest contains a reference to a dependent assembly which is not installed
	/// </summary>
	public const int ERROR_SXS_ROOT_MANIFEST_DEPENDENCY_NOT_INSTALLED = 14015;

	/// <summary>
	///     The manifest for an assembly used by the application has a reference to a dependent assembly which is not installed
	/// </summary>
	public const int ERROR_SXS_LEAF_MANIFEST_DEPENDENCY_NOT_INSTALLED = 14016;

	/// <summary>
	///     The manifest contains an attribute for the assembly identity which is not valid.
	/// </summary>
	public const int ERROR_SXS_INVALID_ASSEMBLY_IDENTITY_ATTRIBUTE = 14017;

	/// <summary>
	///     The manifest is missing the required default namespace specification on the assembly element.
	/// </summary>
	public const int ERROR_SXS_MANIFEST_MISSING_REQUIRED_DEFAULT_NAMESPACE = 14018;

	/// <summary>
	///     The manifest has a default namespace specified on the assembly element but its value is not
	///     "urn:schemas-microsoft-com:essentialMix.v1".
	/// </summary>
	public const int ERROR_SXS_MANIFEST_INVALID_REQUIRED_DEFAULT_NAMESPACE = 14019;

	/// <summary>
	///     The private manifest probed has crossed reparse-point-associated path
	/// </summary>
	public const int ERROR_SXS_PRIVATE_MANIFEST_CROSS_PATH_WITH_REPARSE_POINT = 14020;

	/// <summary>
	///     Two or more components referenced directly or indirectly by the application manifest have files by the same name.
	/// </summary>
	public const int ERROR_SXS_DUPLICATE_DLL_NAME = 14021;

	/// <summary>
	///     Two or more components referenced directly or indirectly by the application manifest have window classes with the
	///     same name.
	/// </summary>
	public const int ERROR_SXS_DUPLICATE_WINDOWCLASS_NAME = 14022;

	/// <summary>
	///     Two or more components referenced directly or indirectly by the application manifest have the same COM server
	///     CLSIDs.
	/// </summary>
	public const int ERROR_SXS_DUPLICATE_CLSID = 14023;

	/// <summary>
	///     Two or more components referenced directly or indirectly by the application manifest have proxies for the same COM
	///     interface IIDs.
	/// </summary>
	public const int ERROR_SXS_DUPLICATE_IID = 14024;

	/// <summary>
	///     Two or more components referenced directly or indirectly by the application manifest have the same COM type library
	///     TLBIDs.
	/// </summary>
	public const int ERROR_SXS_DUPLICATE_TLBID = 14025;

	/// <summary>
	///     Two or more components referenced directly or indirectly by the application manifest have the same COM ProgIDs.
	/// </summary>
	public const int ERROR_SXS_DUPLICATE_PROGID = 14026;

	/// <summary>
	///     Two or more components referenced directly or indirectly by the application manifest are different versions of the
	///     same component which is not permitted.
	/// </summary>
	public const int ERROR_SXS_DUPLICATE_ASSEMBLY_NAME = 14027;

	/// <summary>
	///     A component's file does not match the verification information present in the
	///     component manifest.
	/// </summary>
	public const int ERROR_SXS_FILE_HASH_MISMATCH = 14028;

	/// <summary>
	///     The policy manifest contains one or more syntax errors.
	/// </summary>
	public const int ERROR_SXS_POLICY_PARSE_ERROR = 14029;

	/// <summary>
	///     Manifest Parse Error : A string literal was expected, but no opening quote character was found.
	/// </summary>
	public const int ERROR_SXS_XML_E_MISSINGQUOTE = 14030;

	/// <summary>
	///     Manifest Parse Error : Incorrect syntax was used in a comment.
	/// </summary>
	public const int ERROR_SXS_XML_E_COMMENTSYNTAX = 14031;

	/// <summary>
	///     Manifest Parse Error : A name was started with an invalid character.
	/// </summary>
	public const int ERROR_SXS_XML_E_BADSTARTNAMECHAR = 14032;

	/// <summary>
	///     Manifest Parse Error : A name contained an invalid character.
	/// </summary>
	public const int ERROR_SXS_XML_E_BADNAMECHAR = 14033;

	/// <summary>
	///     Manifest Parse Error : A string literal contained an invalid character.
	/// </summary>
	public const int ERROR_SXS_XML_E_BADCHARINSTRING = 14034;

	/// <summary>
	///     Manifest Parse Error : Invalid syntax for an xml declaration.
	/// </summary>
	public const int ERROR_SXS_XML_E_XMLDECLSYNTAX = 14035;

	/// <summary>
	///     Manifest Parse Error : An Invalid character was found in text content.
	/// </summary>
	public const int ERROR_SXS_XML_E_BADCHARDATA = 14036;

	/// <summary>
	///     Manifest Parse Error : Required white space was missing.
	/// </summary>
	public const int ERROR_SXS_XML_E_MISSINGWHITESPACE = 14037;

	/// <summary>
	///     Manifest Parse Error : The character '>' was expected.
	/// </summary>
	public const int ERROR_SXS_XML_E_EXPECTINGTAGEND = 14038;

	/// <summary>
	///     Manifest Parse Error : A semi colon character was expected.
	/// </summary>
	public const int ERROR_SXS_XML_E_MISSINGSEMICOLON = 14039;

	/// <summary>
	///     Manifest Parse Error : Unbalanced parentheses.
	/// </summary>
	public const int ERROR_SXS_XML_E_UNBALANCEDPAREN = 14040;

	/// <summary>
	///     Manifest Parse Error : Internal error.
	/// </summary>
	public const int ERROR_SXS_XML_E_INTERNALERROR = 14041;

	/// <summary>
	///     Manifest Parse Error : Whitespace is not allowed at this location.
	/// </summary>
	public const int ERROR_SXS_XML_E_UNEXPECTED_WHITESPACE = 14042;

	/// <summary>
	///     Manifest Parse Error : End of file reached in invalid state for current encoding.
	/// </summary>
	public const int ERROR_SXS_XML_E_INCOMPLETE_ENCODING = 14043;

	/// <summary>
	///     Manifest Parse Error : Missing parenthesis.
	/// </summary>
	public const int ERROR_SXS_XML_E_MISSING_PAREN = 14044;

	/// <summary>
	///     Manifest Parse Error : A single or double closing quote character (\' or \") is missing.
	/// </summary>
	public const int ERROR_SXS_XML_E_EXPECTINGCLOSEQUOTE = 14045;

	/// <summary>
	///     Manifest Parse Error : Multiple colons are not allowed in a name.
	/// </summary>
	public const int ERROR_SXS_XML_E_MULTIPLE_COLONS = 14046;

	/// <summary>
	///     Manifest Parse Error : Invalid character for decimal digit.
	/// </summary>
	public const int ERROR_SXS_XML_E_INVALID_DECIMAL = 14047;

	/// <summary>
	///     Manifest Parse Error : Invalid character for hexidecimal digit.
	/// </summary>
	public const int ERROR_SXS_XML_E_INVALID_HEXIDECIMAL = 14048;

	/// <summary>
	///     Manifest Parse Error : Invalid unicode character value for this platform.
	/// </summary>
	public const int ERROR_SXS_XML_E_INVALID_UNICODE = 14049;

	/// <summary>
	///     Manifest Parse Error : Expecting whitespace or '?'.
	/// </summary>
	public const int ERROR_SXS_XML_E_WHITESPACEORQUESTIONMARK = 14050;

	/// <summary>
	///     Manifest Parse Error : End tag was not expected at this location.
	/// </summary>
	public const int ERROR_SXS_XML_E_UNEXPECTEDENDTAG = 14051;

	/// <summary>
	///     Manifest Parse Error : The following tags were not closed: %1.
	/// </summary>
	public const int ERROR_SXS_XML_E_UNCLOSEDTAG = 14052;

	/// <summary>
	///     Manifest Parse Error : Duplicate attribute.
	/// </summary>
	public const int ERROR_SXS_XML_E_DUPLICATEATTRIBUTE = 14053;

	/// <summary>
	///     Manifest Parse Error : Only one top level element is allowed in an XML document.
	/// </summary>
	public const int ERROR_SXS_XML_E_MULTIPLEROOTS = 14054;

	/// <summary>
	///     Manifest Parse Error : Invalid at the top level of the document.
	/// </summary>
	public const int ERROR_SXS_XML_E_INVALIDATROOTLEVEL = 14055;

	/// <summary>
	///     Manifest Parse Error : Invalid xml declaration.
	/// </summary>
	public const int ERROR_SXS_XML_E_BADXMLDECL = 14056;

	/// <summary>
	///     Manifest Parse Error : XML document must have a top level element.
	/// </summary>
	public const int ERROR_SXS_XML_E_MISSINGROOT = 14057;

	/// <summary>
	///     Manifest Parse Error : Unexpected end of file.
	/// </summary>
	public const int ERROR_SXS_XML_E_UNEXPECTEDEOF = 14058;

	/// <summary>
	///     Manifest Parse Error : Parameter entities cannot be used inside markup declarations in an internal subset.
	/// </summary>
	public const int ERROR_SXS_XML_E_BADPEREFINSUBSET = 14059;

	/// <summary>
	///     Manifest Parse Error : Element was not closed.
	/// </summary>
	public const int ERROR_SXS_XML_E_UNCLOSEDSTARTTAG = 14060;

	/// <summary>
	///     Manifest Parse Error : End element was missing the character '>'.
	/// </summary>
	public const int ERROR_SXS_XML_E_UNCLOSEDENDTAG = 14061;

	/// <summary>
	///     Manifest Parse Error : A string literal was not closed.
	/// </summary>
	public const int ERROR_SXS_XML_E_UNCLOSEDSTRING = 14062;

	/// <summary>
	///     Manifest Parse Error : A comment was not closed.
	/// </summary>
	public const int ERROR_SXS_XML_E_UNCLOSEDCOMMENT = 14063;

	/// <summary>
	///     Manifest Parse Error : A declaration was not closed.
	/// </summary>
	public const int ERROR_SXS_XML_E_UNCLOSEDDECL = 14064;

	/// <summary>
	///     Manifest Parse Error : A CDATA section was not closed.
	/// </summary>
	public const int ERROR_SXS_XML_E_UNCLOSEDCDATA = 14065;

	/// <summary>
	///     Manifest Parse Error : The namespace prefix is not allowed to start with the reserved string "xml".
	/// </summary>
	public const int ERROR_SXS_XML_E_RESERVEDNAMESPACE = 14066;

	/// <summary>
	///     Manifest Parse Error : System does not support the specified encoding.
	/// </summary>
	public const int ERROR_SXS_XML_E_INVALIDENCODING = 14067;

	/// <summary>
	///     Manifest Parse Error : Switch from current encoding to specified encoding not supported.
	/// </summary>
	public const int ERROR_SXS_XML_E_INVALIDSWITCH = 14068;

	/// <summary>
	///     Manifest Parse Error : The name 'xml' is reserved and must be lower case.
	/// </summary>
	public const int ERROR_SXS_XML_E_BADXMLCASE = 14069;

	/// <summary>
	///     Manifest Parse Error : The standalone attribute must have the value 'yes' or 'no'.
	/// </summary>
	public const int ERROR_SXS_XML_E_INVALID_STANDALONE = 14070;

	/// <summary>
	///     Manifest Parse Error : The standalone attribute cannot be used in external entities.
	/// </summary>
	public const int ERROR_SXS_XML_E_UNEXPECTED_STANDALONE = 14071;

	/// <summary>
	///     Manifest Parse Error : Invalid version number.
	/// </summary>
	public const int ERROR_SXS_XML_E_INVALID_VERSION = 14072;

	/// <summary>
	///     Manifest Parse Error : Missing equals sign between attribute and attribute value.
	/// </summary>
	public const int ERROR_SXS_XML_E_MISSINGEQUALS = 14073;

	/// <summary>
	///     Assembly Protection Error : Unable to recover the specified assembly.
	/// </summary>
	public const int ERROR_SXS_PROTECTION_RECOVERY_FAILED = 14074;

	/// <summary>
	///     Assembly Protection Error : The public key for an assembly was too short to be allowed.
	/// </summary>
	public const int ERROR_SXS_PROTECTION_PUBLIC_KEY_TOO_Int16 = 14075;

	/// <summary>
	///     Assembly Protection Error : The catalog for an assembly is not valid, or does not match the assembly's manifest.
	/// </summary>
	public const int ERROR_SXS_PROTECTION_CATALOG_NOT_VALID = 14076;

	/// <summary>
	///     An HRESULT could not be translated to a corresponding Win32 error code.
	/// </summary>
	public const int ERROR_SXS_UNTRANSLATABLE_HRESULT = 14077;

	/// <summary>
	///     Assembly Protection Error : The catalog for an assembly is missing.
	/// </summary>
	public const int ERROR_SXS_PROTECTION_CATALOG_FILE_MISSING = 14078;

	/// <summary>
	///     The supplied assembly identity is missing one or more attributes which must be present in this context.
	/// </summary>
	public const int ERROR_SXS_MISSING_ASSEMBLY_IDENTITY_ATTRIBUTE = 14079;

	/// <summary>
	///     The supplied assembly identity has one or more attribute names that contain characters not permitted in XML names.
	/// </summary>
	public const int ERROR_SXS_INVALID_ASSEMBLY_IDENTITY_ATTRIBUTE_NAME = 14080;

	/// <summary>
	///     The specified quick mode policy already exists.
	/// </summary>
	public const int ERROR_IPSEC_QM_POLICY_EXISTS = 13000;

	/// <summary>
	///     The specified quick mode policy was not found.
	/// </summary>
	public const int ERROR_IPSEC_QM_POLICY_NOT_FOUND = 13001;

	/// <summary>
	///     The specified quick mode policy is being used.
	/// </summary>
	public const int ERROR_IPSEC_QM_POLICY_IN_USE = 13002;

	/// <summary>
	///     The specified main mode policy already exists.
	/// </summary>
	public const int ERROR_IPSEC_MM_POLICY_EXISTS = 13003;

	/// <summary>
	///     The specified main mode policy was not found
	/// </summary>
	public const int ERROR_IPSEC_MM_POLICY_NOT_FOUND = 13004;

	/// <summary>
	///     The specified main mode policy is being used.
	/// </summary>
	public const int ERROR_IPSEC_MM_POLICY_IN_USE = 13005;

	/// <summary>
	///     The specified main mode filter already exists.
	/// </summary>
	public const int ERROR_IPSEC_MM_FILTER_EXISTS = 13006;

	/// <summary>
	///     The specified main mode filter was not found.
	/// </summary>
	public const int ERROR_IPSEC_MM_FILTER_NOT_FOUND = 13007;

	/// <summary>
	///     The specified transport mode filter already exists.
	/// </summary>
	public const int ERROR_IPSEC_TRANSPORT_FILTER_EXISTS = 13008;

	/// <summary>
	///     The specified transport mode filter does not exist.
	/// </summary>
	public const int ERROR_IPSEC_TRANSPORT_FILTER_NOT_FOUND = 13009;

	/// <summary>
	///     The specified main mode authentication list exists.
	/// </summary>
	public const int ERROR_IPSEC_MM_AUTH_EXISTS = 13010;

	/// <summary>
	///     The specified main mode authentication list was not found.
	/// </summary>
	public const int ERROR_IPSEC_MM_AUTH_NOT_FOUND = 13011;

	/// <summary>
	///     The specified quick mode policy is being used.
	/// </summary>
	public const int ERROR_IPSEC_MM_AUTH_IN_USE = 13012;

	/// <summary>
	///     The specified main mode policy was not found.
	/// </summary>
	public const int ERROR_IPSEC_DEFAULT_MM_POLICY_NOT_FOUND = 13013;

	/// <summary>
	///     The specified quick mode policy was not found
	/// </summary>
	public const int ERROR_IPSEC_DEFAULT_MM_AUTH_NOT_FOUND = 13014;

	/// <summary>
	///     The manifest file contains one or more syntax errors.
	/// </summary>
	public const int ERROR_IPSEC_DEFAULT_QM_POLICY_NOT_FOUND = 13015;

	/// <summary>
	///     The application attempted to activate a disabled activation context.
	/// </summary>
	public const int ERROR_IPSEC_TUNNEL_FILTER_EXISTS = 13016;

	/// <summary>
	///     The requested lookup key was not found in any active activation context.
	/// </summary>
	public const int ERROR_IPSEC_TUNNEL_FILTER_NOT_FOUND = 13017;

	/// <summary>
	///     The Main Mode filter is pending deletion.
	/// </summary>
	public const int ERROR_IPSEC_MM_FILTER_PENDING_DELETION = 13018;

	/// <summary>
	///     The transport filter is pending deletion.
	/// </summary>
	public const int ERROR_IPSEC_TRANSPORT_FILTER_PENDING_DELETION = 13019;

	/// <summary>
	///     The tunnel filter is pending deletion.
	/// </summary>
	public const int ERROR_IPSEC_TUNNEL_FILTER_PENDING_DELETION = 13020;

	/// <summary>
	///     The Main Mode policy is pending deletion.
	/// </summary>
	public const int ERROR_IPSEC_MM_POLICY_PENDING_DELETION = 13021;

	/// <summary>
	///     The Main Mode authentication bundle is pending deletion.
	/// </summary>
	public const int ERROR_IPSEC_MM_AUTH_PENDING_DELETION = 13022;

	/// <summary>
	///     The Quick Mode policy is pending deletion.
	/// </summary>
	public const int ERROR_IPSEC_QM_POLICY_PENDING_DELETION = 13023;

	/// <summary>
	///     No information avialable.
	/// </summary>
	public const int WARNING_IPSEC_MM_POLICY_PRUNED = 13024;

	/// <summary>
	///     No information avialable.
	/// </summary>
	public const int WARNING_IPSEC_QM_POLICY_PRUNED = 13025;

	/// <summary>
	///     ERROR_IPSEC_IKE_NEG_STATUS_BEGIN
	/// </summary>
	public const int ERROR_IPSEC_IKE_NEG_STATUS_BEGIN = 13800;

	/// <summary>
	///     IKE authentication credentials are unacceptable
	/// </summary>
	public const int ERROR_IPSEC_IKE_AUTH_FAIL = 13801;

	/// <summary>
	///     IKE security attributes are unacceptable
	/// </summary>
	public const int ERROR_IPSEC_IKE_ATTRIB_FAIL = 13802;

	/// <summary>
	///     IKE Negotiation in progress
	/// </summary>
	public const int ERROR_IPSEC_IKE_NEGOTIATION_PENDING = 13803;

	/// <summary>
	///     General processing error
	/// </summary>
	public const int ERROR_IPSEC_IKE_GENERAL_PROCESSING_ERROR = 13804;

	/// <summary>
	///     Negotiation timed out
	/// </summary>
	public const int ERROR_IPSEC_IKE_TIMED_OUT = 13805;

	/// <summary>
	///     IKE failed to find valid machine certificate
	/// </summary>
	public const int ERROR_IPSEC_IKE_NO_CERT = 13806;

	/// <summary>
	///     IKE SA deleted by peer before establishment completed
	/// </summary>
	public const int ERROR_IPSEC_IKE_SA_DELETED = 13807;

	/// <summary>
	///     IKE SA deleted before establishment completed
	/// </summary>
	public const int ERROR_IPSEC_IKE_SA_REAPED = 13808;

	/// <summary>
	///     Negotiation request sat in Queue too long
	/// </summary>
	public const int ERROR_IPSEC_IKE_MM_ACQUIRE_DROP = 13809;

	/// <summary>
	///     Negotiation request sat in Queue too long
	/// </summary>
	public const int ERROR_IPSEC_IKE_QM_ACQUIRE_DROP = 13810;

	/// <summary>
	///     Negotiation request sat in Queue too long
	/// </summary>
	public const int ERROR_IPSEC_IKE_QUEUE_DROP_MM = 13811;

	/// <summary>
	///     Negotiation request sat in Queue too long
	/// </summary>
	public const int ERROR_IPSEC_IKE_QUEUE_DROP_NO_MM = 13812;

	/// <summary>
	///     No response from peer
	/// </summary>
	public const int ERROR_IPSEC_IKE_DROP_NO_RESPONSE = 13813;

	/// <summary>
	///     Negotiation took too long
	/// </summary>
	public const int ERROR_IPSEC_IKE_MM_DELAY_DROP = 13814;

	/// <summary>
	///     Negotiation took too long
	/// </summary>
	public const int ERROR_IPSEC_IKE_QM_DELAY_DROP = 13815;

	/// <summary>
	///     Unknown error occurred
	/// </summary>
	public const int ERROR_IPSEC_IKE_ERROR = 13816;

	/// <summary>
	///     Certificate Revocation Check failed
	/// </summary>
	public const int ERROR_IPSEC_IKE_CRL_FAILED = 13817;

	/// <summary>
	///     Invalid certificate key usage
	/// </summary>
	public const int ERROR_IPSEC_IKE_INVALID_KEY_USAGE = 13818;

	/// <summary>
	///     Invalid certificate type
	/// </summary>
	public const int ERROR_IPSEC_IKE_INVALID_CERT_TYPE = 13819;

	/// <summary>
	///     No private key associated with machine certificate
	/// </summary>
	public const int ERROR_IPSEC_IKE_NO_PRIVATE_KEY = 13820;

	/// <summary>
	///     Failure in Diffie-Helman computation
	/// </summary>
	public const int ERROR_IPSEC_IKE_DH_FAIL = 13822;

	/// <summary>
	///     Invalid header
	/// </summary>
	public const int ERROR_IPSEC_IKE_INVALID_HEADER = 13824;

	/// <summary>
	///     No policy configured
	/// </summary>
	public const int ERROR_IPSEC_IKE_NO_POLICY = 13825;

	/// <summary>
	///     Failed to verify signature
	/// </summary>
	public const int ERROR_IPSEC_IKE_INVALID_SIGNATURE = 13826;

	/// <summary>
	///     Failed to authenticate using kerberos
	/// </summary>
	public const int ERROR_IPSEC_IKE_KERBEROS_ERROR = 13827;

	/// <summary>
	///     Peer's certificate did not have a public key
	/// </summary>
	public const int ERROR_IPSEC_IKE_NO_PUBLIC_KEY = 13828;

	/// <summary>
	///     Error processing error payload
	/// </summary>
	public const int ERROR_IPSEC_IKE_PROCESS_ERR = 13829;

	/// <summary>
	///     Error processing SA payload
	/// </summary>
	public const int ERROR_IPSEC_IKE_PROCESS_ERR_SA = 13830;

	/// <summary>
	///     Error processing Proposal payload
	/// </summary>
	public const int ERROR_IPSEC_IKE_PROCESS_ERR_PROP = 13831;

	/// <summary>
	///     Error processing Transform payload
	/// </summary>
	public const int ERROR_IPSEC_IKE_PROCESS_ERR_TRANS = 13832;

	/// <summary>
	///     Error processing KE payload
	/// </summary>
	public const int ERROR_IPSEC_IKE_PROCESS_ERR_KE = 13833;

	/// <summary>
	///     Error processing ID payload
	/// </summary>
	public const int ERROR_IPSEC_IKE_PROCESS_ERR_ID = 13834;

	/// <summary>
	///     Error processing Cert payload
	/// </summary>
	public const int ERROR_IPSEC_IKE_PROCESS_ERR_CERT = 13835;

	/// <summary>
	///     Error processing Certificate Request payload
	/// </summary>
	public const int ERROR_IPSEC_IKE_PROCESS_ERR_CERT_REQ = 13836;

	/// <summary>
	///     Error processing Hash payload
	/// </summary>
	public const int ERROR_IPSEC_IKE_PROCESS_ERR_HASH = 13837;

	/// <summary>
	///     Error processing Signature payload
	/// </summary>
	public const int ERROR_IPSEC_IKE_PROCESS_ERR_SIG = 13838;

	/// <summary>
	///     Error processing Nonce payload
	/// </summary>
	public const int ERROR_IPSEC_IKE_PROCESS_ERR_NONCE = 13839;

	/// <summary>
	///     Error processing Notify payload
	/// </summary>
	public const int ERROR_IPSEC_IKE_PROCESS_ERR_NOTIFY = 13840;

	/// <summary>
	///     Error processing Delete Payload
	/// </summary>
	public const int ERROR_IPSEC_IKE_PROCESS_ERR_DELETE = 13841;

	/// <summary>
	///     Error processing VendorId payload
	/// </summary>
	public const int ERROR_IPSEC_IKE_PROCESS_ERR_VENDOR = 13842;

	/// <summary>
	///     Invalid payload received
	/// </summary>
	public const int ERROR_IPSEC_IKE_INVALID_PAYLOAD = 13843;

	/// <summary>
	///     Soft SA loaded
	/// </summary>
	public const int ERROR_IPSEC_IKE_LOAD_SOFT_SA = 13844;

	/// <summary>
	///     Soft SA torn down
	/// </summary>
	public const int ERROR_IPSEC_IKE_SOFT_SA_TORN_DOWN = 13845;

	/// <summary>
	///     Invalid cookie received.
	/// </summary>
	public const int ERROR_IPSEC_IKE_INVALID_COOKIE = 13846;

	/// <summary>
	///     Peer failed to send valid machine certificate
	/// </summary>
	public const int ERROR_IPSEC_IKE_NO_PEER_CERT = 13847;

	/// <summary>
	///     Certification Revocation check of peer's certificate failed
	/// </summary>
	public const int ERROR_IPSEC_IKE_PEER_CRL_FAILED = 13848;

	/// <summary>
	///     New policy invalidated SAs formed with old policy
	/// </summary>
	public const int ERROR_IPSEC_IKE_POLICY_CHANGE = 13849;

	/// <summary>
	///     There is no available Main Mode IKE policy.
	/// </summary>
	public const int ERROR_IPSEC_IKE_NO_MM_POLICY = 13850;

	/// <summary>
	///     Failed to enabled TCB privilege.
	/// </summary>
	public const int ERROR_IPSEC_IKE_NOTCBPRIV = 13851;

	/// <summary>
	///     Failed to load SECURITY.DLL.
	/// </summary>
	public const int ERROR_IPSEC_IKE_SECLOADFAIL = 13852;

	/// <summary>
	///     Failed to obtain security function table dispatch address from SSPI.
	/// </summary>
	public const int ERROR_IPSEC_IKE_FAILSSPINIT = 13853;

	/// <summary>
	///     Failed to query Kerberos package to obtain max token size.
	/// </summary>
	public const int ERROR_IPSEC_IKE_FAILQUERYSSP = 13854;

	/// <summary>
	///     Failed to obtain Kerberos server credentials for ISAKMP/ERROR_IPSEC_IKE service.  Kerberos authentication will not
	///     function.  The most likely reason for this is lack of domain membership.  This is normal if your computer is a
	///     member of a workgroup.
	/// </summary>
	public const int ERROR_IPSEC_IKE_SRVACQFAIL = 13855;

	/// <summary>
	///     Failed to determine SSPI principal name for ISAKMP/ERROR_IPSEC_IKE service (QueryCredentialsAttributes).
	/// </summary>
	public const int ERROR_IPSEC_IKE_SRVQUERYCRED = 13856;

	/// <summary>
	///     Failed to obtain new SPI for the inbound SA from Ipsec driver.  The most common cause for this is that the driver
	///     does not have the correct filter.  Check your policy to verify the filters.
	/// </summary>
	public const int ERROR_IPSEC_IKE_GETSPIFAIL = 13857;

	/// <summary>
	///     Given filter is invalid
	/// </summary>
	public const int ERROR_IPSEC_IKE_INVALID_FILTER = 13858;

	/// <summary>
	///     Memory allocation failed.
	/// </summary>
	public const int ERROR_IPSEC_IKE_OUT_OF_MEMORY = 13859;

	/// <summary>
	///     Failed to add Security Association to IPSec Driver.  The most common cause for this is if the IKE negotiation took
	///     too long to complete.  If the problem persists, reduce the load on the faulting machine.
	/// </summary>
	public const int ERROR_IPSEC_IKE_ADD_UPDATE_KEY_FAILED = 13860;

	/// <summary>
	///     Invalid policy
	/// </summary>
	public const int ERROR_IPSEC_IKE_INVALID_POLICY = 13861;

	/// <summary>
	///     Invalid DOI
	/// </summary>
	public const int ERROR_IPSEC_IKE_UNKNOWN_DOI = 13862;

	/// <summary>
	///     Invalid situation
	/// </summary>
	public const int ERROR_IPSEC_IKE_INVALID_SITUATION = 13863;

	/// <summary>
	///     Diffie-Hellman failure
	/// </summary>
	public const int ERROR_IPSEC_IKE_DH_FAILURE = 13864;

	/// <summary>
	///     Invalid Diffie-Hellman group
	/// </summary>
	public const int ERROR_IPSEC_IKE_INVALID_GROUP = 13865;

	/// <summary>
	///     Error encrypting payload
	/// </summary>
	public const int ERROR_IPSEC_IKE_ENCRYPT = 13866;

	/// <summary>
	///     Error decrypting payload
	/// </summary>
	public const int ERROR_IPSEC_IKE_DECRYPT = 13867;

	/// <summary>
	///     Policy match error
	/// </summary>
	public const int ERROR_IPSEC_IKE_POLICY_MATCH = 13868;

	/// <summary>
	///     Unsupported ID
	/// </summary>
	public const int ERROR_IPSEC_IKE_UNSUPPORTED_ID = 13869;

	/// <summary>
	///     Hash verification failed
	/// </summary>
	public const int ERROR_IPSEC_IKE_INVALID_HASH = 13870;

	/// <summary>
	///     Invalid hash algorithm
	/// </summary>
	public const int ERROR_IPSEC_IKE_INVALID_HASH_ALG = 13871;

	/// <summary>
	///     Invalid hash size
	/// </summary>
	public const int ERROR_IPSEC_IKE_INVALID_HASH_SIZE = 13872;

	/// <summary>
	///     Invalid encryption algorithm
	/// </summary>
	public const int ERROR_IPSEC_IKE_INVALID_ENCRYPT_ALG = 13873;

	/// <summary>
	///     Invalid authentication algorithm
	/// </summary>
	public const int ERROR_IPSEC_IKE_INVALID_AUTH_ALG = 13874;

	/// <summary>
	///     Invalid certificate signature
	/// </summary>
	public const int ERROR_IPSEC_IKE_INVALID_SIG = 13875;

	/// <summary>
	///     Load failed
	/// </summary>
	public const int ERROR_IPSEC_IKE_LOAD_FAILED = 13876;

	/// <summary>
	///     Deleted via RPC call
	/// </summary>
	public const int ERROR_IPSEC_IKE_RPC_DELETE = 13877;

	/// <summary>
	///     Temporary state created to perform reinit. This is not a real failure.
	/// </summary>
	public const int ERROR_IPSEC_IKE_BENIGN_REINIT = 13878;

	/// <summary>
	///     The lifetime value received in the Responder Lifetime Notify is below the Windows 2000 configured minimum value.
	///     Please fix the policy on the peer machine.
	/// </summary>
	public const int ERROR_IPSEC_IKE_INVALID_RESPONDER_LIFETIME_NOTIFY = 13879;

	/// <summary>
	///     Key length in certificate is too small for configured security requirements.
	/// </summary>
	public const int ERROR_IPSEC_IKE_INVALID_CERT_KEYLEN = 13881;

	/// <summary>
	///     Max number of established MM SAs to peer exceeded.
	/// </summary>
	public const int ERROR_IPSEC_IKE_MM_LIMIT = 13882;

	/// <summary>
	///     IKE received a policy that disables negotiation.
	/// </summary>
	public const int ERROR_IPSEC_IKE_NEGOTIATION_DISABLED = 13883;

	/// <summary>
	///     ERROR_IPSEC_IKE_NEG_STATUS_END
	/// </summary>
	public const int ERROR_IPSEC_IKE_NEG_STATUS_END = 13884;
}

public static class ShellExecuteVerbs
{
	public const string Open = "open"; //Opens a file or a application
	public const string OpenAs = "openas"; //Opens dialog when no program is associated to the extension
	public const string OpenNew = "opennew"; //see MSDN 
	public const string RunAs = "runas"; //In Windows 7 and Vista, opens the UAC dialog and in others, open the Run as... Dialog
	public const string Null = "null"; //Specifies that the operation is the default for the selected file type.
	public const string Edit = "edit"; //Opens the default text editor for the file.    
	public const string Explore = "explore"; //Opens the Windows Explorer in the folder specified in lpDirectory.
	public const string Properties = "properties"; //Opens the properties window of the file.
	public const string Copy = "copy"; //see MSDN
	public const string Cut = "cut"; //see MSDN
	public const string Paste = "paste"; //see MSDN
	public const string PasteLink = "pastelink"; //pastes a shortcut
	public const string Delete = "delete"; //see MSDN
	public const string Print = "print"; //Start printing the file with the default application.
	public const string PrintTo = "printto"; //see MSDN
	public const string Find = "find"; //Start a search
}

public static class DeviceGuids
{
	public const string D61883 = "7EBEFBC0-3200-11d2-B4C2-00A0C9697D07";
	public const string AVC = "095780C3-48A1-4570-BD95-46707F78C2DC";
	public const string BLUETOOTH = "0850302A-B344-4fda-9BE9-90576B8D46F0";
	public const string APPLICATION_LAUNCH_BUTTON = "629758EE-986E-4D9E-8E47-DE27F8AB054D";
	public const string BATTERY = "72631E54-78A4-11D0-BCF7-00AA00B7B32A";
	public const string LID = "4AFA3D52-74A7-11d0-be5e-00A0C9062857";
	public const string MEMORY = "3FD0F03D-92E0-45FB-B75C-5ED8FFB01021";
	public const string MESSAGE_INDICATOR = "CD48A365-FA94-4CE2-A232-A1B764E5D8B4";
	public const string PROCESSOR = "97FADB10-4E33-40AE-359C-8BEF029DBDD0";
	public const string SYS_BUTTON = "4AFA3D53-74A7-11d0-be5e-00A0C9062857";
	public const string THERMAL_ZONE = "4AFA3D51-74A7-11d0-be5e-00A0C9062857";
	public const string BRIGHTNESS = "FDE5BBA4-B3F9-46FB-BDAA-0728CE3100B4";
	public const string CD_CHANGER = "53F56312-B6BF-11D0-94F2-00A0C91EFB8B";
	public const string CDROM = "53F56308-B6BF-11D0-94F2-00A0C91EFB8B";
	public const string COM_PORT = "86E0D1E0-8089-11D0-9CE4-08003E301F73";
	public const string DISK = "53F56307-B6BF-11D0-94F2-00A0C91EFB8B";
	public const string DISPLAY_ADAPTER = "5B45201D-F2F2-4F3B-85BB-30FF1F953599";
	public const string FLOPPY = "53F56311-B6BF-11D0-94F2-00A0C91EFB8B";
	public const string HID = "4D1E55B2-F16F-11CF-88CB-001111000030";
	public const string I2C = "2564AA4F-DDDB-4495-B497-6AD4A84163D7";
	// WIA devices and Still Image (STI) devices
	public const string IMAGE = "6BDD1FC6-810F-11D0-BEC7-08002BE2092F";
	public const string KEYBOARD = "884b96c3-56ef-11d1-bc8c-00a0c91405dd";
	public const string MEDIUM_CHANGER = "53F56310-B6BF-11D0-94F2-00A0C91EFB8B";
	public const string MODEM = "2C7089AA-2E0E-11D1-B114-00C04FC2AAE4";
	public const string MONITOR = "E6F07B5F-EE97-4a90-B076-33F57BF4EAA7";
	public const string MOUSE = "378DE44C-56EF-11D1-BC8C-00A0C91405DD";
	public const string NETWORK = "CAC88484-7515-4C03-82E6-71A87ABAC361";
	public const string OPM = "BF4672DE-6B4E-4BE4-A325-68A91EA49C09";
	public const string PARALLEL_PORTS = "97F76EF0-F883-11D0-AF1F-0000F800845C";
	// PARCLASS
	public const string PARALLEL_DEVICES = "811FC6A5-F728-11D0-A537-0000F8753ED1";
	public const string PARTITION = "53F5630A-B6BF-11D0-94F2-00A0C91EFB8B";
	// 4D36E978-E325-11CE-BFC1-08002BE10318
	public const string SERENUM_BUS_ENUMERATOR = "53F5630A-B6BF-11D0-94F2-00A0C91EFB8B";
	public const string SIDE_SHOW = "152E5811-FEB9-4B00-90F4-D32947AE1681";
	public const string STORAGE_PORT = "2ACCFE60-C130-11D2-B082-00A0C91EFB8B";
	public const string TAPE = "53F5630B-B6BF-11D0-94F2-00A0C91EFB8B";
	public const string USB = "A5DCBF10-6530-11D2-901F-00C04FB951ED";
	public const string USB_HOST_CONTROLLER = "3ABF6F2D-71C4-462A-8A92-1E6861E6AF27";
	public const string USB_HUB = "F18A0E88-C30C-11D0-8815-00A0C906BED8";
	public const string VIDEO_OUTPUT_ARRIVAL = "1AD9E4F0-F88D-4360-BAB9-4C2D55E564CD";
	public const string VOLUME = "53F5630D-B6BF-11D0-94F2-00A0C91EFB8B";
	public const string WINDOWS_PORTABLE_DEVICES = "6AC27878-A6FA-4155-BA85-F98F491D4F33";
	public const string WINDOWS_PORTABLE_DEVICES_PRIVATE = "BA0C718F-4DED-49B7-BDD3-FABE28661211";
	public const string WRITE_ONCE_DISK = "53F5630C-B6BF-11D0-94F2-00A0C91EFB8B";
	public const string DISPLAY_DEVICE_ARRIVAL = "1CA05180-A699-450A-9A0C-DE4FBE3DDD89";
	public const string IO_VOLUME_DEVICE = "53F5630D-B6BF-11D0-94F2-00A0C91EFB8B";
}
#endregion

/// <summary>
/// Win32 API and constants
/// </summary>
public static class Win32
{
	public const int OS_ANYSERVER = 29;

	public const int CLASS_NAME_MAX_LENGTH = 256;

	public const string WND_CLASS_MSGBOX = "#32770";
	public const string WND_CLASS_VBMAINFORM = "ThunderRT6Main";
	public const string WND_CLASS_VBFORM = "ThunderRT6FormDC";

	public const int STD_INPUT_HANDLE = -10;
	public const int STD_OUTPUT_HANDLE = -11;
	public const int STD_ERROR_HANDLE = -12;

	public const uint STILL_ACTIVE = 259;

	public const int INFINITE = -1;

	public const uint GENERIC_READ = 0x80000000;
	public const uint GENERIC_WRITE = 0x40000000;

	public const uint FILE_FLAG_OVERLAPPED = 0x40000000;
	public const uint FILE_FLAG_NO_BUFFERING = 0x20000000;

	public const uint PIPE_ACCESS_OUTBOUND = 0x00000002;
	public const uint PIPE_ACCESS_DUPLEX = 0x00000003;
	public const uint PIPE_ACCESS_INBOUND = 0x00000001;
	public const uint PIPE_WAIT = 0x00000000;
	public const uint PIPE_NOWAIT = 0x00000001;
	public const uint PIPE_READMODE_BYTE = 0x00000000;
	public const uint PIPE_READMODE_MESSAGE = 0x00000002;
	public const uint PIPE_TYPE_BYTE = 0x00000000;
	public const uint PIPE_TYPE_MESSAGE = 0x00000004;
	public const uint PIPE_CLIENT_END = 0x00000000;
	public const uint PIPE_SERVER_END = 0x00000001;
	public const uint PIPE_UNLIMITED_INSTANCES = 255;

	public const uint NMPWAIT_WAIT_FOREVER = 0xffffffff;
	public const uint NMPWAIT_NOWAIT = 0x00000001;
	public const uint NMPWAIT_USE_DEFAULT_WAIT = 0x00000000;

	public const uint EXTENDED_STARTUPINFO_PRESENT = 0x00080000;
	public const int PROC_THREAD_ATTRIBUTE_PARENT_PROCESS = 0x00020000;

	public const int MAX_PATH = 260;
	public const int INTERNET_MAX_URL_LENGTH = 2083;

	public const int MAX_WINDOW_NAME_LENGTH = 100;

	public const int WM_CHANGEUISTATE = 0x0127;
	public const int UIS_INITIALIZE = 3;

	public const int UOI_NAME = 2;

	public const int LF_FACESIZE = 32;

	public const int WM_DEVICECHANGE = 0x0219;

	public const int DEVICE_NOTIFY_WINDOW_HANDLE = 0x00000000;
	public const int DEVICE_NOTIFY_SERVICE_HANDLE = 0x00000001;
	public const int DEVICE_NOTIFY_ALL_INTERFACE_CLASSES = 0x00000004;

	public const string GUID_DEVINTERFACE_USB_DEVICE = "A5DCBF10-6530-11D2-901F-00C04FB951ED";
	public const string GUID_DEVINTERFACE_USB_HOST_CONTROLLER = "3ABF6F2D-71C4-462A-8A92-1E6861E6AF27";

	public static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

	public static readonly HandleRef NullHandleRef = new HandleRef(null, IntPtr.Zero);

	[SecurityCritical]
	[DllImport("kernel32.dll")]
	public static extern FileType GetFileType(IntPtr hFile);

	[SecurityCritical]
	[DllImport("kernel32.dll")]
	public static extern FileType GetFileType(SafeHandle handle);

	[SecurityCritical]
	[DllImport("shlwapi.dll", SetLastError = true, EntryPoint = "#437")]
	public static extern bool IsOS(int value);

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern void GetSystemTime(ref SystemTime sysTime);

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool SetSystemTime(ref SystemTime sysTime);

	[SecurityCritical]
	[DllImport("kernel32.dll")]
	public static extern ErrorModesEnum SetErrorMode(ErrorModesEnum uMode);

	[SecurityCritical]
	[DllImport("kernel32.dll")]
	public static extern ErrorModesEnum GetErrorMode();

	[SecurityCritical]
	[DllImport("kernel32.dll")]
	public static extern bool SetThreadErrorMode(ErrorModesEnum dwNewMode, out ErrorModesEnum lpOldMode);

	[SecurityCritical]
	[DllImport("kernel32.dll")]
	public static extern ErrorModesEnum GetThreadErrorMode();

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool GetThreadTimes(IntPtr hThread, out long lpCreationTime, out long lpExitTime, out long lpKernelTime, out long lpUserTime);

	[SecurityCritical]
	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool EnumWindows(EnumWindowsProcessor lpEnumFunc, IntPtr lParam);

	[SecurityCritical]
	[DllImport("user32.dll", CharSet = CharSet.Unicode, PreserveSig = true, SetLastError = false, ExactSpelling = true)]
	public static extern IntPtr SetFocus(IntPtr hWnd);

	[SecurityCritical]
	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool EnumChildWindows(IntPtr hWndParent, EnumWindowsProcessor lpEnumFunc, IntPtr lParam);

	[SecurityCritical]
	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool EnumChildWindows(IntPtr hWndParent, IntPtr lpEnumFunc, IntPtr lParam);

	[SecurityCritical]
	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool EnumThreadWindows(int dwThreadId, EnumWindowsProcessor lpfn, IntPtr lParam);

	[SecurityCritical]
	[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
	public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

	[SecurityCritical]
	[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
	public static extern int GetWindowTextLength(IntPtr hWnd);

	[SecurityCritical]
	[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
	public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

	[SecurityCritical]
	[DllImport("user32.dll")]
	public static extern IntPtr GetDlgItem(IntPtr hDlg, int nIDDlgItem);

	[SecurityCritical]
	[DllImport("user32.dll")]
	public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

	[SecurityCritical]
	[DllImport("user32.dll")]
	public static extern bool ShowWindowAsync(IntPtr hWnd, ShowWindowEnum nCmdShow);

	[SecurityCritical]
	[DllImport("user32.dll")]
	public static extern bool ShowWindow(IntPtr hWnd, ShowWindowEnum nCmdShow);

	[SecurityCritical]
	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool IsWindow(IntPtr hWnd);

	[SecurityCritical]
	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	public static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

	[SecurityCritical]
	[DllImport("user32.dll")]
	public static extern bool MoveWindow(IntPtr hWnd, int x, int y, int cx, int cy, bool repaint);

	[SecurityCritical]
	[DllImport("user32.dll")]
	public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int y, int cx, int cy, WindowPositionFlagsEnum uFlags);

	[SecurityCritical]
	[DllImport("user32.dll")]
	public static extern uint GetWindowLong(IntPtr hWnd, WindowLongSettingIndexEnum nIndex);

	[SecurityCritical]
	[DllImport("user32.dll")]
	public static extern uint SetWindowLong(IntPtr hWnd, WindowLongSettingIndexEnum nIndex, uint dwNewLong);

	[SecurityCritical]
	[DllImport("user32.dll", SetLastError = true)]
	public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

	[SecurityCritical]
	[DllImport("user32.dll", SetLastError = true)]
	public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

	[SecurityCritical]
	[DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
	public static extern IntPtr GetParent(IntPtr hWnd);

	[SecurityCritical]
	[DllImport("user32.dll")]
	public static extern void DisableProcessWindowsGhosting();

	[SecurityCritical]
	[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
	public static extern IntPtr CreateJobObject(IntPtr lpJobAttributes, string name);

	[SecurityCritical]
	[DllImport("kernel32.dll")]
	public static extern bool SetInformationJobObject(IntPtr job, JobObjectInfoTypeEnum infoType, IntPtr lpJobObjectInfo, uint cbJobObjectInfoLength);

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool AssignProcessToJobObject(IntPtr job, IntPtr process);

	[SecurityCritical]
	[DllImport("user32.dll")]
	public static extern int MapVirtualKey(uint uCode, uint uMapType);

	public static bool IsKeyPressed(int keyCode) { return (GetAsyncKeyState(keyCode) & 0x0800) == 0; }

	[SecurityCritical]
	[DllImport("user32.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
	public static extern short GetAsyncKeyState(int vKey);

	[SecurityCritical]
	[DllImport("user32.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr GetDesktopWindow();

	[SecurityCritical]
	[DllImport("user32.dll", SetLastError = true)]
	public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

	[SecurityCritical]
	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool GetWindowRect(HandleRef hWnd, out RECT lpRect);

	[SecurityCritical]
	[DllImport("user32.dll", SetLastError = true)]
	public static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

	[SecurityCritical]
	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool GetClientRect(HandleRef hWnd, out RECT lpRect);

	[SecurityCritical]
	[DllImport("user32.dll", SetLastError = true)]
	public static extern IntPtr SetActiveWindow(IntPtr hWnd);

	[SecurityCritical]
	[DllImport("user32.dll")]
	public static extern IntPtr SetCapture(IntPtr hWnd);

	[SecurityCritical]
	[DllImport("user32.dll")]
	public static extern IntPtr GetCapture();

	[SecurityCritical]
	[DllImport("user32.dll")]
	public static extern bool ReleaseCapture();

	[SecurityCritical]
	[DllImport("user32", ExactSpelling = true, SetLastError = true)]
	public static extern int MapWindowPoints(IntPtr hWndFrom, IntPtr hWndTo, [In][Out] ref RECT rect, [MarshalAs(UnmanagedType.U4)]
		int cPoints);

	[SecurityCritical]
	[DllImport("user32", ExactSpelling = true, SetLastError = true)]
	public static extern int MapWindowPoints(IntPtr hWndFrom, IntPtr hWndTo, [In][Out] ref POINT pt, [MarshalAs(UnmanagedType.U4)]
		int cPoints);

	[SecurityCritical]
	[DllImport("kernel32.dll")]
	public static extern IntPtr GetConsoleWindow();

	[SecurityCritical]
	[DllImport("kernel32.dll")]
	public static extern bool GetConsoleMode(IntPtr hConsoleHandle, out ConsoleModesEnum lpMode);

	[SecurityCritical]
	[DllImport("kernel32.dll")]
	public static extern bool SetConsoleMode(IntPtr hConsoleHandle, ConsoleModesEnum dwMode);

	[SecurityCritical]
	[DllImport("kernel32.dll")]
	public static extern bool ReadConsoleInput(IntPtr hConsoleInput, [Out] INPUT_RECORD[] lpBuffer, uint nLength, out uint lpNumberOfEventsRead);

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool ReadConsoleOutput(IntPtr hConsoleOutput, [Out] CHAR_INFO[] lpBuffer, COORD dwBufferSize, COORD dwBufferCoord,
		ref SMALL_RECT lpReadRegion);

	[SecurityCritical]
	[DllImport("kernel32.dll")]
	public static extern IntPtr GetStdHandle(int handle);

	[SecurityCritical]
	[DllImport("user32.dll")]
	public static extern IntPtr GetForegroundWindow();

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool CreatePipe(out IntPtr hReadPipe, out IntPtr hWritePipe, ref SECURITY_ATTRIBUTES lpPipeAttributes, uint nSize);

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern IntPtr CreateNamedPipe(string lpName, uint dwOpenMode, uint dwPipeMode, uint nMaxInstances, uint nOutBufferSize, uint nInBufferSize,
		uint nDefaultTimeOut, ref SECURITY_ATTRIBUTES lpPipeAttributes);

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool ConnectNamedPipe(IntPtr hHandle, IntPtr lpOverlapped);

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool DisconnectNamedPipe(IntPtr hHandle);

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool PeekNamedPipe(IntPtr hHandle, byte[] buffer, uint nBufferSize, ref uint bytesRead, ref uint bytesAvail,
		ref uint BytesLeftThisMessage);

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool SetHandleInformation(IntPtr hObject, HandleFlagsEnum dwMask, HandleFlagsEnum dwFlags);

	[SecurityCritical]
	[DllImport("Kernel32.dll", EntryPoint = "RtlZeroMemory", SetLastError = false)]
	public static extern void ZeroMemory(IntPtr dest, IntPtr size);

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
	public static extern bool CreateProcess(string lpApplicationName, string lpCommandLine, ref SECURITY_ATTRIBUTES lpProcessAttributes,
		ref SECURITY_ATTRIBUTES lpThreadAttributes, bool bInheritHandles, CreateProcessFlagsEnum dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory,
		[In] ref STARTUPINFO lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation);

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
	public static extern bool CreateProcess(string lpApplicationName, string lpCommandLine, ref SECURITY_ATTRIBUTES lpProcessAttributes,
		ref SECURITY_ATTRIBUTES lpThreadAttributes, bool bInheritHandles, CreateProcessFlagsEnum dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory,
		[In] ref STARTUPINFOEX lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation);

	[SecurityCritical]
	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, PreserveSig = true, SetLastError = true, ExactSpelling = true)]
	public static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, int bInheritHandle, int dwProcessId);

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool UpdateProcThreadAttribute(IntPtr lpAttributeList, uint dwFlags, IntPtr Attribute, IntPtr lpValue, IntPtr cbSize,
		IntPtr lpPreviousValue, IntPtr lpReturnSize);

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool InitializeProcThreadAttributeList(IntPtr lpAttributeList, int dwAttributeCount, int dwFlags, ref IntPtr lpSize);

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool DeleteProcThreadAttributeList(IntPtr lpAttributeList);

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool TerminateProcess(IntPtr hProcess, uint uExitCode);

	[SecurityCritical]
	[DllImport("kernel32.dll", EntryPoint = "CreateFile", SetLastError = true)]
	public static extern IntPtr CreateFile(string lpFileName, FileAccessEnum dwDesiredAccess, FileShareEnum dwShareMode,
		ref SECURITY_ATTRIBUTES lpSecurityAttributes, CreationDispositionEnum dwCreationDisposition, FileAttributesEnum dwFlagsAndAttributes, IntPtr hTemplateFile);

	[SecurityCritical]
	[DllImport("kernel32.dll", EntryPoint = "CreateFile", SetLastError = true)]
	public static extern IntPtr CreateFile(string lpFileName, FileAccessEnum dwDesiredAccess, FileShareEnum dwShareMode, IntPtr lpSecurityAttributes,
		CreationDispositionEnum dwCreationDisposition, FileAttributesEnum dwFlagsAndAttributes, IntPtr hTemplateFile);

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool ReadFile(IntPtr hFile, [Out] byte[] lpBuffer, uint nNumberOfBytesToRead, out uint lpNumberOfBytesRead, IntPtr lpOverlapped);

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool ReadFile(IntPtr hFile, [Out] StringBuilder lpBuffer, uint nNumberOfBytesToRead, out uint lpNumberOfBytesRead, IntPtr lpOverlapped);

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool WriteFile(IntPtr hFile, byte[] lpBuffer, uint nNumberOfBytesToWrite, out uint written, IntPtr lpOverlapped);

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool WriteFile(IntPtr hFile, StringBuilder lpBuffer, uint nNumberOfBytesToWrite, out uint written, IntPtr lpOverlapped);

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool FlushFileBuffers(IntPtr handle);

	[SecurityCritical]
	[DllImport("kernel32.dll")]
	public static extern uint GetTempFileName(string lpPathName, string lpPrefixString, uint uUnique, [Out] StringBuilder lpTempFileName);

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern int WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

	[SecurityCritical]
	[DllImport("kernel32.dll")]
	public static extern int WaitForMultipleObjects(uint nCount, IntPtr[] lpHandles, bool bWaitAll, uint dwMilliseconds);

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	[SuppressUnmanagedCodeSecurity]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool CloseHandle(IntPtr hObject);

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool GetExitCodeProcess(IntPtr hProcess, out uint lpExitCode);

	[SecurityCritical]
	[DllImport("shell32.dll", SetLastError = true, CharSet = CharSet.Auto)]
	public static extern bool ShellExecuteEx(SHELLEXECUTEINFO lpExecInfo);

	[SecurityCritical]
	[DllImport("netapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	public static extern int NetGetJoinInformation(string computerName, ref IntPtr buffer, ref NetworkJoinStatus status);

	[SecurityCritical]
	[DllImport("netapi32.dll", SetLastError = true)]
	public static extern int NetApiBufferFree(IntPtr buffer);

	[SecurityCritical]
	[DllImport("shlwapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
	public static extern IntPtr PathAddBackslash([MarshalAs(UnmanagedType.LPTStr)]
		StringBuilder lpszPath);

	[SecurityCritical]
	[DllImport("shlwapi.dll", CharSet = CharSet.Auto)]
	public static extern bool PathCompactPathEx([Out] StringBuilder pszOut, string szPath, uint cchMax, int dwFlags);

	[SecurityCritical]
	[DllImport("shlwapi.dll", CharSet = CharSet.Auto)]
	public static extern int PathCommonPrefix([In] string pszFile1, [In] string pszFile2, [Out] StringBuilder pszPath);

	[SecurityCritical]
	[DllImport("shlwapi.dll", SetLastError = true)]
	public static extern int PathCreateFromUrl([In] string url, [Out] StringBuilder path, [In][Out] ref uint pathLength, [In] uint reserved);

	[SecurityCritical]
	[DllImport("shlwapi.dll", CharSet = CharSet.Auto)]
	public static extern string PathFindNextComponent([In] string pszPath);

	[SecurityCritical]
	[DllImport("shlwapi.dll", CharSet = CharSet.Auto)]
	public static extern bool PathIsSameRoot([In] string pszPath1, [In] string pszPath2);

	[SecurityCritical]
	[DllImport("shlwapi.dll", CharSet = CharSet.Auto)]
	public static extern int UrlCreateFromPath([In] string path, [Out] StringBuilder url, [In][Out] ref uint urlLength, [In] uint reserved);

	[SecurityCritical]
	[DllImport("shlwapi.dll", CharSet = CharSet.Auto)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool PathIsUNC([MarshalAs(UnmanagedType.LPWStr)][In]
		string pszPath);

	[SecurityCritical]
	[DllImport("shlwapi.dll", CharSet = CharSet.Auto)]
	public static extern bool PathRelativePathTo([Out] StringBuilder pszPath, [In] string pszFrom, [In] FileAttributes dwAttrFrom, [In] string pszTo,
		[In] FileAttributes dwAttrTo);

	[SecurityCritical]
	[DllImport("kernel32.dll")]
	public static extern int GetThreadId(IntPtr thread);

	[SecurityCritical]
	[DllImport("kernel32.dll")]
	public static extern int GetProcessId(IntPtr process);

	[SecurityCritical]
	[DllImport("user32.dll")]
	public static extern IntPtr CreateDesktop(string lpszDesktop, IntPtr lpszDevice, IntPtr pDevmode, int dwFlags, DesktopAccessRightsEnum dwDesiredAccess,
		IntPtr lpsa);

	[SecurityCritical]
	[DllImport("user32.dll")]
	public static extern bool CloseDesktop(IntPtr hDesktop);

	[SecurityCritical]
	[DllImport("user32.dll")]
	public static extern IntPtr OpenDesktop(string lpszDesktop, int dwFlags, bool fInherit, DesktopAccessRightsEnum dwDesiredAccess);

	[SecurityCritical]
	[DllImport("user32.dll")]
	public static extern IntPtr OpenInputDesktop(int dwFlags, bool fInherit, DesktopAccessRightsEnum dwDesiredAccess);

	[SecurityCritical]
	[DllImport("user32.dll")]
	public static extern bool SwitchDesktop(IntPtr hDesktop);

	[SecurityCritical]
	[DllImport("user32.dll")]
	public static extern bool EnumDesktops(IntPtr hwinsta, EnumDesktopProc lpEnumFunc, IntPtr lParam);

	[SecurityCritical]
	[DllImport("user32.dll")]
	public static extern IntPtr GetProcessWindowStation();

	[SecurityCritical]
	[DllImport("user32.dll")]
	public static extern bool EnumDesktopWindows(IntPtr hDesktop, EnumDesktopWindowsProc lpfn, IntPtr lParam);

	[SecurityCritical]
	[DllImport("user32.dll")]
	public static extern bool SetThreadDesktop(IntPtr hDesktop);

	[SecurityCritical]
	[DllImport("user32.dll")]
	public static extern IntPtr GetThreadDesktop(int dwThreadId);

	[SecurityCritical]
	[DllImport("user32.dll")]
	public static extern bool GetUserObjectInformation(IntPtr hObj, int nIndex, IntPtr pvInfo, int nLength, ref int lpnLengthNeeded);

	[SecurityCritical]
	[DllImport("wininet.dll", SetLastError = true)]
	public static extern bool InternetGetConnectedState(out int lpdwFlags, int dwReserved);

	[SecurityCritical]
	[DllImport("kernel32.dll")]
	public static extern COORD GetConsoleFontSize(IntPtr hConsoleOutput, int nFont);

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern int SetConsoleFont(IntPtr hOut, uint dwFontNum);

	[SecurityCritical]
	[DllImport("kernel32")]
	public static extern bool GetConsoleFontInfo(IntPtr hOutput, bool bMaximize, uint count, [MarshalAs(UnmanagedType.LPArray)][Out]
		ConsoleFont[] fonts);

	[SecurityCritical]
	[DllImport("kernel32")]
	public static extern uint GetNumberOfConsoleFonts();

	[SecurityCritical]
	[DllImport("kernel32.dll")]
	public static extern bool GetCurrentConsoleFont(IntPtr hConsoleOutput, bool bMaximumWindow, out CONSOLE_FONT_INFO lpConsoleCurrentFont);

	[SecurityCritical]
	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	public static extern bool GetCurrentConsoleFontEx(IntPtr hConsoleOutput, bool bMaximumWindow, [In][Out] CONSOLE_FONT_INFO_EX lpConsoleCurrentFont);

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool SetCurrentConsoleFontEx(IntPtr hConsoleOutput, bool bMaximumWindow, ref CONSOLE_FONT_INFO_EX lpConsoleCurrentFontEx);

	[SecurityCritical]
	[DllImport("kernel32", SetLastError = true)]
	public static extern bool AddConsoleAlias(string source, string target, string exeName);

	[SecurityCritical]
	[DllImport("kernel32", SetLastError = true)]
	public static extern bool AllocConsole();

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool AttachConsole(uint dwProcessId);

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern IntPtr CreateConsoleScreenBuffer(uint dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, uint dwFlags,
		IntPtr lpScreenBufferData);

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool FillConsoleOutputAttribute(IntPtr hConsoleOutput, ushort wAttribute, uint nLength, COORD dwWriteCoord,
		out uint lpNumberOfAttrsWritten);

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool FillConsoleOutputCharacter(IntPtr hConsoleOutput, char cCharacter, uint nLength, COORD dwWriteCoord,
		out uint lpNumberOfCharsWritten);

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool FlushConsoleInputBuffer(IntPtr hConsoleInput);

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
	public static extern bool FreeConsole();

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool GenerateConsoleCtrlEvent(CTRL_EVENT dwCtrlEvent, uint dwProcessGroupId);

	[SecurityCritical]
	[DllImport("kernel32", SetLastError = true)]
	public static extern bool GetConsoleAlias(string Source, out StringBuilder targetBuffer, uint targetBufferLength, string exeName);

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern uint GetConsoleAliases(StringBuilder[] lpTargetBuffer, uint targetBufferLength, string lpExeName);

	[SecurityCritical]
	[DllImport("kernel32", SetLastError = true)]
	public static extern uint GetConsoleAliasesLength(string exeName);

	[SecurityCritical]
	[DllImport("kernel32", SetLastError = true)]
	public static extern uint GetConsoleAliasExes(out StringBuilder ExeNameBuffer, uint exeNameBufferLength);

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern uint GetConsoleAliasExesLength();

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern uint GetConsoleCP();

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool GetConsoleCursorInfo(IntPtr hConsoleOutput, out CONSOLE_CURSOR_INFO lpConsoleCursorInfo);

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool GetConsoleDisplayMode(out uint ModeFlags);

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool GetConsoleHistoryInfo(out CONSOLE_HISTORY_INFO ConsoleHistoryInfo);

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern uint GetConsoleOriginalTitle(out StringBuilder ConsoleTitle, uint size);

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern uint GetConsoleOutputCP();

	// http://pinvoke.net/default.aspx/kernel32/GetConsoleProcessList.html
	// TODO: Test - what's an out uint[] during interop? This probably isn't quite right, but provides a starting point:
	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern uint GetConsoleProcessList(out uint[] processList, uint processCount);

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool GetConsoleScreenBufferInfo(IntPtr hConsoleOutput, out CONSOLE_SCREEN_BUFFER_INFO lpConsoleScreenBufferInfo);

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool GetConsoleScreenBufferInfoEx(IntPtr hConsoleOutput, ref CONSOLE_SCREEN_BUFFER_INFO_EX ConsoleScreenBufferInfo);

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool GetConsoleSelectionInfo(CONSOLE_SELECTION_INFO consoleSelectionInfo);

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern uint GetConsoleTitle([Out] StringBuilder lpConsoleTitle, uint nSize);

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern COORD GetLargestConsoleWindowSize(IntPtr hConsoleOutput);

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool GetNumberOfConsoleInputEvents(IntPtr hConsoleInput, out uint lpcNumberOfEvents);

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool GetNumberOfConsoleMouseButtons(ref uint lpNumberOfMouseButtons);

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool PeekConsoleInput(IntPtr hConsoleInput, [Out] INPUT_RECORD[] lpBuffer, uint nLength, out uint lpNumberOfEventsRead);

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool ReadConsole(IntPtr hConsoleInput, [Out] StringBuilder lpBuffer, uint nNumberOfCharsToRead, out uint lpNumberOfCharsRead,
		IntPtr lpReserved);

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool WriteConsole(IntPtr hConsoleOutput, StringBuilder lpBuffer, uint nNumberOfCharsToWrite, out uint lpNumberOfCharsWritten,
		IntPtr lpReserved);

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool WriteConsole(IntPtr hConsoleOutput, string lpBuffer, uint nNumberOfCharsToWrite, out uint lpNumberOfCharsWritten,
		IntPtr lpReserved);

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool ReadConsoleOutputAttribute(IntPtr hConsoleOutput, [Out] ushort[] lpAttribute, uint nLength, COORD dwReadCoord,
		out uint lpNumberOfAttrsRead);

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool ReadConsoleOutputCharacter(IntPtr hConsoleOutput, [Out] StringBuilder lpCharacter, uint nLength, COORD dwReadCoord,
		out uint lpNumberOfCharsRead);

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool ScrollConsoleScreenBuffer(IntPtr hConsoleOutput, [In] ref SMALL_RECT lpScrollRectangle, IntPtr lpClipRectangle,
		COORD dwDestinationOrigin, [In] ref CHAR_INFO lpFill);

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool SetConsoleActiveScreenBuffer(IntPtr hConsoleOutput);

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool SetConsoleCP(uint wCodePageID);

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool SetConsoleCtrlHandler(ConsoleCtrlDelegate handlerRoutine, bool add);

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool SetConsoleCursorInfo(IntPtr hConsoleOutput, [In] ref CONSOLE_CURSOR_INFO lpConsoleCursorInfo);

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool SetConsoleCursorPosition(IntPtr hConsoleOutput, COORD dwCursorPosition);

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool SetConsoleDisplayMode(IntPtr ConsoleOutput, uint flags, out COORD newScreenBufferDimensions);

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool SetConsoleHistoryInfo(CONSOLE_HISTORY_INFO ConsoleHistoryInfo);

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool SetConsoleOutputCP(uint wCodePageID);

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool SetConsoleScreenBufferInfoEx(IntPtr ConsoleOutput, CONSOLE_SCREEN_BUFFER_INFO_EX consoleScreenBufferInfoEx);

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool SetConsoleScreenBufferSize(IntPtr hConsoleOutput, COORD dwSize);

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool SetConsoleTextAttribute(IntPtr hConsoleOutput, ushort wAttributes);

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool SetConsoleTitle(string lpConsoleTitle);

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool SetConsoleWindowInfo(IntPtr hConsoleOutput, bool bAbsolute, [In] ref SMALL_RECT lpConsoleWindow);

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool SetStdHandle(IntPtr hConsole, IntPtr hHandle);

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool WriteConsoleInput(IntPtr hConsoleInput, INPUT_RECORD[] lpBuffer, uint nLength, out uint lpNumberOfEventsWritten);

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool WriteConsoleOutput(IntPtr hConsoleOutput, CHAR_INFO[] lpBuffer, COORD dwBufferSize, COORD dwBufferCoord,
		ref SMALL_RECT lpWriteRegion);

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool WriteConsoleOutputAttribute(IntPtr hConsoleOutput, ushort[] lpAttribute, uint nLength, COORD dwWriteCoord,
		out uint lpNumberOfAttrsWritten);

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool WriteConsoleOutputCharacter(IntPtr hConsoleOutput, string lpCharacter, uint nLength, COORD dwWriteCoord,
		out uint lpNumberOfCharsWritten);

	[SecurityCritical]
	[DllImport("kernel32")]
	public static extern bool SetConsoleIcon(IntPtr hIcon);

	[SecurityCritical]
	[DllImport("user32.dll", SetLastError = true)]
	public static extern bool LockWindowUpdate(IntPtr hWnd);

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern IntPtr LocalFree(IntPtr hMem);

	[SecurityCritical]
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern int FormatMessage(FormatMessageFlags dwFlags, IntPtr lpSource, uint dwMessageId, uint dwLanguageId, ref IntPtr lpBuffer, uint nSize,
		IntPtr pArguments);

	[SecurityCritical]
	[DllImport("kernel32.dll", CharSet = CharSet.Auto, BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError = true)]
	public static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)]
		string fileName);

	[SecurityCritical]
	[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool FreeLibrary(IntPtr hModule);

	[SecurityCritical]
	[DllImport("kernel32.dll", BestFitMapping = false, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
	public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

	[SecurityCritical]
	[DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
	public static extern void CopyMemory(IntPtr dest, IntPtr src, int length);

	[SecurityCritical]
	[DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
	public static extern unsafe byte* CopyMemory(byte* dst, byte* src, int length);

	[SecurityCritical]
	[DllImport("ntdll.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern unsafe byte* memset(byte* dst, byte c, int count);

	[SecurityCritical]
	[DllImport("advapi32.dll", SetLastError = true)]
	public static extern bool LogonUser(string lpszUsername, string lpszDomain, string lpszPassword, LogonType dwLogonType, int dwLogonProvider, out IntPtr phToken);

	[SecurityCritical]
	[DllImport("advapi32.dll", SetLastError = true)]
	public static extern bool LogonUser(string lpszUsername, string lpszDomain, IntPtr phPassword, LogonType dwLogonType, int dwLogonProvider, out IntPtr phToken);

	[SecurityCritical]
	[DllImport("advapi32.dll", SetLastError = true)]
	public static extern bool RevertToSelf();

	[SecurityCritical]
	[DllImport("advapi32.dll", SetLastError = true)]
	public static extern bool DuplicateToken(IntPtr existingTokenHandle, SecurityImpersonationLevel securityImpersonationLevel, out IntPtr duplicateTokenHandle);

	[SecurityCritical]
	[DllImport("Winmm.dll", CharSet = CharSet.Auto)]
	public static extern uint timeKillEvent(uint uTimerID);

	[SecurityCritical]
	[DllImport("Winmm.dll", CharSet = CharSet.Auto)]
	public static extern uint timeGetTime();

	[SecurityCritical]
	[DllImport("Winmm.dll", CharSet = CharSet.Auto)]
	public static extern uint timeBeginPeriod(uint uPeriod);

	[SecurityCritical]
	[DllImport("Winmm.dll", CharSet = CharSet.Auto)]
	public static extern uint timeEndPeriod(uint uPeriod);

	[SecurityCritical]
	[DllImport("Winmm.dll", CharSet = CharSet.Auto)]
	public static extern uint timeSetEvent(uint uDelay, uint uResolution, TimerCallback lpTimeProc, UIntPtr dwUser, fuEvent fuEvent);

	[SecurityCritical]
	[DllImport("rpcrt4.dll", SetLastError = true)]
	public static extern int UuidCreateSequential(out Guid guid);

	[SecurityCritical]
	[DllImport("advapi32.dll", SetLastError = true)]
	public static extern bool GetTokenInformation(IntPtr tokenHandle, TokenInformationClass tokenInformationClass, IntPtr tokenInformation, int tokenInformationLength, out int returnLength);

	[SecurityCritical]
	[DllImport("advapi32.dll")]
	public static extern uint GetEffectiveRightsFromAcl(byte[] pacl, ref TRUSTEE pTrustee, ref uint pAccessRights);

	[SecurityCritical]
	[DllImport("advapi32.dll", SetLastError = true)]
	public static extern void BuildTrusteeWithSid(ref TRUSTEE pTrustee, byte[] sid);

	[SecurityCritical]
	[DllImport("advapi32.dll", SetLastError = true)]
	public static extern bool QueryServiceObjectSecurity(SafeHandle serviceHandle, SecurityInfos secInfo, byte[] lpSecDesrBuf, uint bufSize, out uint bufSizeNeeded);

	[SecurityCritical]
	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	public static extern IntPtr RegisterDeviceNotification(IntPtr recipient, IntPtr notificationFilter, int flags);

	[SecurityCritical]
	[DllImport("user32.dll")]
	public static extern bool UnregisterDeviceNotification(IntPtr handle);

	public static string GetLastErrorMessage() { return GetSystemMessage(Marshal.GetLastWin32Error()); }

	public static string GetSystemMessage(int errorCode)
	{
		const FormatMessageFlags FMTMSG_FLAGS = FormatMessageFlags.FORMAT_MESSAGE_ALLOCATE_BUFFER |
												FormatMessageFlags.FORMAT_MESSAGE_FROM_SYSTEM |
												FormatMessageFlags.FORMAT_MESSAGE_IGNORE_INSERTS;
		try
		{
			IntPtr lpMsgBuf = IntPtr.Zero;
			int dwChars = FormatMessage(FMTMSG_FLAGS, IntPtr.Zero, (uint)errorCode, 0 /* Default language */, ref lpMsgBuf, 0, IntPtr.Zero);

			if (dwChars == 0)
			{
				int le = Marshal.GetLastWin32Error();
				return "Unable to get error code string from System - Error " + le;
			}

			string sRet = Marshal.PtrToStringAnsi(lpMsgBuf);
			LocalFree(lpMsgBuf);
			return sRet;
		}
		catch (Exception e)
		{
			return "Unable to get error code string from System -> " + e;
		}
	}

	[DllImport("user32.dll")]
	public static extern IntPtr MonitorFromWindow(IntPtr handle, uint flags);

	[DllImport("user32.dll")]
	public static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

	[DllImport("Dwmapi.dll")]
	public static extern int DwmIsCompositionEnabled([MarshalAs(UnmanagedType.Bool)] out bool pfEnabled);

	[DllImport("Dwmapi.dll", EntryPoint = "#127")] // Undocumented API
	public static extern int DwmGetColorizationParameters(out DWMCOLORIZATIONPARAMS parameters);

	[DllImport("Shell32.dll", SetLastError = false)]
	public static extern int SHGetStockIconInfo(SHSTOCKICONID siid, SHGSI uFlags, ref SHSTOCKICONINFO psii);

	[DllImport("user32.dll", SetLastError = true)]
	public static extern bool DestroyIcon(IntPtr hIcon);
}
using System;

namespace asm.DevExpress
{
	[Flags]
	public enum UploadVisibleElements
	{
		None = 0x0,
		UploadButton = 0x1,
		ClearButton = 0x2,
		AddRemoveButton = 0x4,
		TextBox = 0x8,
		Progress = 0x10
	}
}
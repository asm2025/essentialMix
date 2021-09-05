using System;
using System.Runtime.InteropServices;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation
{
 	[StructLayout(LayoutKind.Sequential)]
	[UnmanagedName("ASF_INDEX_IDENTIFIER")]
	public class ASFIndexIdentifier
    {
        public Guid guidIndexType;
        public short wStreamNumber;
    }
}

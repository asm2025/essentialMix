using System.ComponentModel.DataAnnotations;

namespace essentialMix.Media
{
	public enum VideoAspectRatio
	{
		Default,
		[Display(Name = "1:1")] R1T1,
		[Display(Name = "2.4:1")] R2P4T1,
		[Display(Name = "3:2")] R3T2,
		[Display(Name = "4:3")] R4T3,
		[Display(Name = "5:3")] R5T3,
		[Display(Name = "5:4")] R5T4,
		[Display(Name = "16:9")] R16T9,
		[Display(Name = "16:10")] R16T10,
		[Display(Name = "17:9")] R17T9
	}
}
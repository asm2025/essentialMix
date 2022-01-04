using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable once CheckNamespace
namespace Other.Microsoft.MultiLanguage;

[ComImport]
[ClassInterface(ClassInterfaceType.None)]
// todo: Where is TypeLibType (NetStandard 2.0)?
//[TypeLibType(TypeLibTypeFlags.FCanCreate)]
[Guid("D66D6F99-CDAA-11D0-B822-00C04FC9B31F")]
internal class CMLangConvertCharsetClass : IMLangConvertCharset, CMLangConvertCharset
{
	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	public virtual extern void DoConversion([In] ref byte pSrcStr,
		[In] [Out] ref uint pcSrcSize,
		[In] ref byte pDstStr,
		[In] [Out] ref uint pcDstSize);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	public virtual extern void DoConversionFromUnicode([In] ref ushort pSrcStr,
		[In] [Out] ref uint pcSrcSize,
		[In] ref sbyte pDstStr,
		[In] [Out] ref uint pcDstSize);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	public virtual extern void DoConversionToUnicode([In] ref sbyte pSrcStr,
		[In] [Out] ref uint pcSrcSize,
		[In] ref ushort pDstStr,
		[In] [Out] ref uint pcDstSize);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	public virtual extern void GetDestinationCodePage(out uint puiDstCodePage);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	public virtual extern void GetProperty(out uint pdwProperty);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	public virtual extern void GetSourceCodePage(out uint puiSrcCodePage);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	public virtual extern void Initialize([In] uint uiSrcCodePage, [In] uint uiDstCodePage, [In] uint dwProperty);
}
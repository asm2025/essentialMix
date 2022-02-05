using System.Runtime.InteropServices;

// ReSharper disable once CheckNamespace
namespace Other.Microsoft.MultiLanguage;

[ComImport]
[CoClass(typeof(CMLangStringClass))]
[Guid("C04D65CE-B70D-11D0-B188-00AA0038C969")]
internal interface CMLangString : IMLangString
{
}
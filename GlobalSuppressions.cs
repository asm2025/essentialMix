
// This file is used by Code Analysis to maintain SuppressMessage 
// attributes that are applied to this project.
// Project-level suppression either have no target or are given 
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Style", "CC0001:You should use 'var' whenever possible.", Justification = "Absolutely bullshit. Explicit type definition is more readable which makes the code self explanatory.", Scope = "member")]
[assembly: SuppressMessage("Design", "CC0120:Your Switch maybe include default clause", Justification = "Or not!", Scope = "member")]
[assembly: SuppressMessage("Style", "IDE0058:Expression value is never used", Justification = "So what?! Must every value returned from a method call be used?", Scope = "member")]
[assembly: SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase", Justification = "System.dll is still using pre-v4 security model and needs this demand")]

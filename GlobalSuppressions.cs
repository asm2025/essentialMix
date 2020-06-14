
// This file is used by Code Analysis to maintain SuppressMessage 
// attributes that are applied to this project.
// Project-level suppression either have no target or are given 
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Style", "CC0001:You should use 'var' whenever possible.", Justification = "Explicit type definition is more readable which makes the code self explanatory. This rule shouldn't exist!", Scope = "member")]
[assembly: SuppressMessage("Design", "CC0120:Your Switch maybe include default clause", Justification = "Or not! because it depends. This rule shouldn't exist!", Scope = "member")]
[assembly: SuppressMessage("Style", "IDE0058:Expression value is never used", Justification = "Must every value returned from any method call be used? This rule shouldn't exist!", Scope = "member")]
[assembly: SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase", Justification = "System.dll is still using pre-v4 security model and needs this demand.")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Justification = "not an expected scenario.")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Internal methods might be used.")]
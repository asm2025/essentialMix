
// This file is used by Code Analysis to maintain SuppressMessage 
// attributes that are applied to this project.
// Project-level suppression either have no target or are given 
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Style", "CC0001:You should use 'var' whenever possible.", Justification = "Explicit type definition is more readable which makes the code self explanatory.")]
[assembly: SuppressMessage("Design", "CC0120:Your Switch maybe include default clause", Justification = "Or not! because it depends.")]
[assembly: SuppressMessage("Style", "IDE0058:Expression value is never used", Justification = "Must every value returned from any method call be used?")]
[assembly: SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase", Justification = "System.dll is still using pre-v4 security model and needs this demand.")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Justification = "Not an expected scenario.")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Internal methods might be used.")]
[assembly: SuppressMessage("Microsoft.Performance", "HAA0302:Display class allocation to capture closure", Justification = "Is capturing closures bad for task methods? What's the alternative?")]
[assembly: SuppressMessage("Microsoft.Performance", "HAA0301:Closure Allocation Source", Justification = "Wasting my time!")]
[assembly: SuppressMessage("Microsoft.Performance", "HAA0601:Value type to reference type conversion causing boxing allocation", Justification = "String interpolation can't accept value types such as int or long? What's the alternative?")]
[assembly: SuppressMessage("Microsoft.Performance", "HAA0102:Non-overridden virtual method call on value type", Justification = "String interpolation error/warning. Wasting my time!")]
[assembly: SuppressMessage("Microsoft.Performance", "HAA0401:Possible allocation of reference type enumerator", Justification = "Wasting my time!")]
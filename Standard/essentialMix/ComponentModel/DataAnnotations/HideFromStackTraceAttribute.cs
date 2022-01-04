using System;

namespace essentialMix.ComponentModel.DataAnnotations;

/*
* If all you want is to hide a method in the Call Stack window during a debugging session, 
* simply apply the [DebuggerHidden] attribute to the method
*/
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class HideFromStackTraceAttribute : Attribute
{
}
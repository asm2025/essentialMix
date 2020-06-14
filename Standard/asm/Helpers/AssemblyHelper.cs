using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using asm.Extensions;
using JetBrains.Annotations;
using asm.ComponentModel.DataAnnotations;

namespace asm.Helpers
{
	public static class AssemblyHelper
	{
		public static Assembly GetEntryAssembly()
		{
			Assembly result = Assembly.GetEntryAssembly();
			if (result != null) return result;

			MethodBase methodCurrent;
			// number of frames to skip
			int skipFrames = 1;

			do
			{
				// get the stack frame, skipping the given number of frames
				StackFrame stackFrame = new StackFrame(skipFrames);
				// get the method
				methodCurrent = stackFrame.GetMethod();
				
				// if found
				if (methodCurrent != null && !methodCurrent.IsDefined<HideFromStackTraceAttribute>())
				{
					// get its type
					Type typeCurrent = methodCurrent.DeclaringType;
					
					// if valid
					if (typeCurrent != null && typeCurrent != typeof(RuntimeMethodHandle))
					{
						Assembly assembly = typeCurrent.Assembly;

						// if valid
						if (!assembly.GlobalAssemblyCache
							&& !assembly.IsDynamic
							&& !assembly.HasAttribute<GeneratedCodeAttribute>())
						{
							result = assembly;
						}
					}
				}

				skipFrames++;
			} while (methodCurrent != null);

			return result;
		}

		public static bool TryLoad([NotNull] string name, out Assembly loadedAssembly)
		{
			try
			{
				loadedAssembly = Assembly.Load(name);
			}
			catch
			{
				loadedAssembly = null;
			}

			return loadedAssembly != null;
		}

		public static bool TryLoad([NotNull] AssemblyName name, out Assembly loadedAssembly)
		{
			try
			{
				loadedAssembly = Assembly.Load(name);
			}
			catch
			{
				loadedAssembly = null;
			}

			return loadedAssembly != null;
		}

		public static IEnumerable<Assembly> GetAssemblies()
		{
			ISet<string> assemblyNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
	
			foreach (Assembly domainAssembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				foreach (Assembly referencedAssembly in GetReferencedAssemblies(domainAssembly, assemblyNames))
					yield return referencedAssembly;
			}

			static IEnumerable<Assembly> GetReferencedAssemblies(Assembly assembly, ISet<string> set)
			{
				if (!set.Add(assembly.FullName)) yield break;
				yield return assembly;

				foreach (AssemblyName name in assembly.GetReferencedAssemblies())
				{
					if (!set.Add(name.FullName) || !TryLoad(name, out Assembly loadedAssembly)) continue;
					yield return loadedAssembly;
				}
			}
		}
	}
}
using System;
using System.Reflection;
using System.Web.Compilation;
using System.Web.UI;
using asm.Extensions;
using JetBrains.Annotations;

namespace asm.Web.Extensions
{
	public static class TemplateControlExtension
	{
		public static Type GetTypeFromUrl([NotNull] this TemplateControl thisValue, string virtualPath)
		{
			object result = LoadObjectFromUrl(thisValue, virtualPath);
			return result?.GetType();
		}

		public static T LoadControl<T>([NotNull] this TemplateControl thisValue, string virtualPath, params object[] parameters)
			where T : class, new()
		{
			return LoadControl<T>(thisValue, virtualPath, null, parameters);
		}

		public static T LoadControl<T>([NotNull] this TemplateControl thisValue, string virtualPath, string invokeMethod, params object[] parameters) 
			where T : class
		{
			T ctlToLoad;

			// If the invokeMethod is empty, I will go for the constructor
			if (string.IsNullOrEmpty(invokeMethod))
			{
				Type type = GetTypeFromUrl(thisValue, virtualPath);
				ctlToLoad = thisValue.LoadControl(type, parameters) as T;
			}
			else
			{
				ctlToLoad = thisValue.LoadControl(thisValue.ResolveUrl(virtualPath)) as T;
				MethodInfo method = ctlToLoad?.AsType().FindMethod(invokeMethod, types: parameters.Types());
				method?.Invoke(ctlToLoad, parameters);
			}

			return ctlToLoad;
		}

		public static object LoadObjectFromUrl([NotNull] this TemplateControl thisValue, string virtualPath) { return LoadObjectFromUrl<object>(thisValue, virtualPath, null); }

		public static T LoadObjectFromUrl<T>([NotNull] this TemplateControl thisValue, string virtualPath) where T : class
		{
			return LoadObjectFromUrl<T>(thisValue, virtualPath, null);
		}

		public static T LoadObjectFromUrl<T>([NotNull] this TemplateControl thisValue, string virtualPath, Type baseType)
			where T : class
		{
			if (string.IsNullOrEmpty(virtualPath)) return null;

			T result;
			Type btype = baseType ?? typeof(object);

			try
			{
				result = BuildManager.CreateInstanceFromVirtualPath(thisValue.ResolveUrl(virtualPath), btype) as T;
			}
			catch
			{
				result = null;
			}

			return result;
		}

		public static bool UrlHasType([NotNull] this TemplateControl thisValue, string virtualPath, Type typeToCheck)
		{
			object result = LoadObjectFromUrl(thisValue, virtualPath);
			Type type = result.GetType();
			bool bFound = false;

			while(type != null)
			{
				if (type == typeToCheck)
				{
					bFound = true;
					break;
				}

				type = type.BaseType;
			}

			return bFound;
		}
	}
}

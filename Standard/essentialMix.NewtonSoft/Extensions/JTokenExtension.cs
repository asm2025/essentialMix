using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions
{
	public static class JTokenExtension
	{
		[NotNull]
		public static ICollection<string> GetFieldNames([NotNull] this JToken thisValue)
		{
			ISet<string> fieldNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			AddPropertyNames(thisValue, fieldNames);
			return fieldNames;
		}

		private static void AddPropertyNames(JToken root, ISet<string> fieldNames)
		{
			switch (root)
			{
				case null:
				case JValue:
					return;
				case JProperty property when property.Name != "$id":
					fieldNames.Add(property.Path);
					break;
			}

			foreach (JToken child in root.Where(e => e.HasValues))
				AddPropertyNames(child, fieldNames);
		}
	}
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Linq;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace asm.Extensions
{
	public static class DataObjectExtension
	{
		/// <summary>
		///     Can be used for deep cloning.
		///     The thing has to be marked with DataContract attribute obviously.
		///     For more information, you can check the links:
		///     https://msdn.microsoft.com/en-us/library/global::System.runtime.serialization.datacontractserializer(v=vs.110).aspx
		///     And https://msdn.microsoft.com/en-us/library/ms731923(v=vs.110).aspx
		///     Also note that there are some limitations for using this in a partial
		///     trust mode, see the last link for more information.
		/// </summary>
		/// <param name="thisValue"></param>
		/// <returns>object</returns>
		public static object CloneDataContract([NotNull] this object thisValue)
		{
			Type type = thisValue.GetType();
			if (type.IsDelegate()) return null;
			if (type.IsPrimitive()) return thisValue;

			using (MemoryStream stream = new MemoryStream())
			{
				DataContractSerializer dcs = new DataContractSerializer(type);
				dcs.WriteObject(stream, thisValue);
				stream.Position = 0;
				return dcs.ReadObject(stream);
			}
		}

		/// <summary>
		/// Returns the properties of the given object as XElements.
		/// Properties with null values are still returned, but as empty
		/// elements. Underscores in property names are replaces with hyphens.
		/// </summary>
		[NotNull]
		public static IEnumerable<XElement> AsXElements([NotNull] this object thisValue)
		{
			return thisValue.GetType()
				.GetProperties()
				.Select(prop => new {prop, value = prop.GetValue(thisValue, null)})
				.Select(t => new XElement(t.prop.Name.Replace("_", "-"), t.value));
		}

		/// <summary>
		/// Returns the properties of the given object as XElements.
		/// Properties with null values are returned as empty attributes.
		/// Underscores in property names are replaces with hyphens.
		/// </summary>
		[NotNull]
		public static IEnumerable<XAttribute> AsXAttributes([NotNull] this object thisValue)
		{
			return thisValue.GetType()
				.GetProperties()
				.Select(prop => new {prop, value = prop.GetValue(thisValue, null)})
				.Select(t => new XAttribute(t.prop.Name.Replace("_", "-"), t.value ?? string.Empty));
		}
	}
}
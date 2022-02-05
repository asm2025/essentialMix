using System;
using System.Xml.Serialization;

namespace essentialMix.Data.Xml;

public class XmlSerializerSettings
{
	public XmlAttributeOverrides Overrides { get; set; }
	public XmlRootAttribute Root { get; set; }
	public string DefaultNamespace { get; set; }
	public string Location { get; set; }
	public Type[] ExtraTypes { get; set; }
}
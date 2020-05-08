using System;

namespace asm.Newtonsoft.Serialization
{
	[Flags]
	public enum JsonSerializerSettingsConverters
	{
		Default = 0,
		Binary = 1,
		DataSet = 1 << 1,
		DataTable = 1 << 2,
		EntityKeyMember = 1 << 3,
		ExpandoObject = 1 << 4,
		IsoDateTime = 1 << 5,
		JavaScriptDateTime = 1 << 6,
		UnixDateTime = 1 << 7,
		KeyValuePair = 1 << 8,
		Regex = 1 << 9,
		StringEnum = 1 << 10,
		StringEnumTranslation = 1 << 11,
		Version = 1 << 12,
		XmlNode = 1 << 13
	}
}
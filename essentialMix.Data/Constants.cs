using System.Xml.Schema;

namespace essentialMix.Data;

public static class Constants
{
	public const ushort DB_BATCH_SIZE_MIN = 10;
	public const ushort DB_BATCH_SIZE_MAX = ushort.MaxValue;
	public const ushort DB_BATCH_SIZE_DEF = 100;

	public const XmlSchemaValidationFlags XML_SIMPLE_VALIDATION_FLAGS = XmlSchemaValidationFlags.ProcessIdentityConstraints | XmlSchemaValidationFlags.AllowXmlAttributes | XmlSchemaValidationFlags.ProcessInlineSchema;
	public const XmlSchemaValidationFlags XML_VALIDATE_EVERYTHING_FLAGS = XML_SIMPLE_VALIDATION_FLAGS | XmlSchemaValidationFlags.ProcessSchemaLocation;
}
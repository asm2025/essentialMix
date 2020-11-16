using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Text;
using asm.Extensions;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using NewtonsoftJsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace asm.Newtonsoft.Converters
{
	public class StringEnumTranslationConverter : StringEnumConverter
	{
		private static readonly ConcurrentDictionary<Type, bool> __types = new ConcurrentDictionary<Type, bool>();
        private static ResourceManager __resources;

        /// <inheritdoc />
        public StringEnumTranslationConverter() 
		{
		}

		/// <inheritdoc />
		public StringEnumTranslationConverter([NotNull] NamingStrategy namingStrategy, bool allowIntegerValues = true)
			: base(namingStrategy, allowIntegerValues)
		{
		}

		/// <inheritdoc />
		public StringEnumTranslationConverter([NotNull] Type namingStrategyType)
			: base(namingStrategyType)
		{
		}

		/// <inheritdoc />
		public StringEnumTranslationConverter([NotNull] Type namingStrategyType, [NotNull] object[] namingStrategyParameters)
			: base(namingStrategyType, namingStrategyParameters)
		{
		}

		/// <inheritdoc />
		public StringEnumTranslationConverter([NotNull] Type namingStrategyType, [NotNull] object[] namingStrategyParameters, bool allowIntegerValues)
			: base(namingStrategyType, namingStrategyParameters, allowIntegerValues)
		{
		}

        [NotNull]
        public static ResourceManager Resources
        {
            get => __resources;
            set
            {
                __resources = value;
                __resources.IgnoreCase = true;
            }
        }

		[NotNull]
		public static ICollection<Type> RegisteredTypes => __types.Keys;

        [NotNull]
		public CultureInfo Culture { get; set; } = CultureInfo.CurrentCulture;

		/// <inheritdoc />
		public override bool CanConvert(Type objectType) { return objectType != null && IsTypeIncluded(objectType); }

		/// <inheritdoc />
		public override void WriteJson([NotNull] JsonWriter writer, object value, NewtonsoftJsonSerializer jsonSerializer)
		{
			if (value == null)
			{
				writer.WriteNull();
				return;
			}
			
			Type type = value.AsType();

			if (!IsTypeIncluded(type))
			{
				base.WriteJson(writer, value, jsonSerializer);
				return;
			}

			StringBuilder sb = new StringBuilder();
			
			using (StringWriter stringWriter = new StringWriter(sb))
			{
				using (JsonWriter jsonWriter = new JsonTextWriter(stringWriter))
				{
					base.WriteJson(jsonWriter, value, jsonSerializer);
				}
			}

			string enumString = sb.ToString().UnQuote();
			string translation = Translate(enumString, Culture);
			writer.WriteValue(translation);
		}

		public static void Register([NotNull] Type type)
		{
			if (!type.IsEnum) throw new InvalidEnumArgumentException();
			__types.AddOrUpdate(type, t => true, (t, b) => true);
		}

		public static void Unregister([NotNull] Type type)
		{
			if (!type.IsEnum) throw new InvalidEnumArgumentException();
			__types.TryRemove(type, out _);
		}

		public static void Clear()
		{
			__types.Clear();
		}

		public static bool IsTypeRegistered([NotNull] Type type) { return type.IsEnum && __types.ContainsKey(type); }
		public static bool IsTypeIncluded([NotNull] Type type) { return type.IsEnum && __types.TryGetValue(type, out bool enabled) && enabled; }

		public static string Translate(string enumString, CultureInfo culture = null)
		{
			try
			{
				string translation = Resources.GetString(enumString, culture ?? CultureInfo.CurrentCulture);
				return string.IsNullOrEmpty(translation)
							? enumString
							: translation;
			}
			catch
			{
				return enumString;
			}
		}
	}
}

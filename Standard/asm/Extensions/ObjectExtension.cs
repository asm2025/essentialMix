using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using JetBrains.Annotations;
using asm.Exceptions;
using asm.Reflection;

namespace asm.Extensions
{
	public static class ObjectExtension
	{
		public static void ClearEventInvocations([NotNull] this object thisValue, [NotNull] string eventName) { thisValue.AsType().FindEventField(eventName)?.SetValue(thisValue, null); }

		public static T As<T>([NotNull] this object thisValue, T defaultValue)
		{
			T value;

			try
			{
				value = (T)thisValue;
			}
			catch
			{
				value = defaultValue;
			}

			return value;
		}

		public static bool Is<TType>(this object thisValue) { return thisValue is TType; }

		public static bool Is(this object thisValue, Type type) { return thisValue.AsType().Is(type); }

		[NotNull]
		public static string GetObjectText([NotNull] this object thisValue, bool includeSubObjects = false)
		{
			StringBuilder sb = new StringBuilder();

			try
			{
				foreach (PropertyDescriptor propertyDescriptor in TypeDescriptor.GetProperties(thisValue))
				{
					if (!propertyDescriptor.IsBrowsable || propertyDescriptor.SerializationVisibility == DesignerSerializationVisibility.Hidden) continue;

					if (propertyDescriptor.SerializationVisibility == DesignerSerializationVisibility.Content)
					{
						if (!includeSubObjects) continue;

						string value = Convert.ToString(propertyDescriptor.GetValue(thisValue));
						if (string.IsNullOrEmpty(value)) continue;
						sb.Separator(", ");
						sb.AppendFormat("{0} = {{ {1} }}", propertyDescriptor.Name, value);
					}
					else if (!propertyDescriptor.IsReadOnly && propertyDescriptor.ShouldSerializeValue(thisValue))
					{
						sb.Separator(", ");
						sb.Append(propertyDescriptor.Name);
						sb.AppendFormat(!(propertyDescriptor.PropertyType == typeof(string)) ? " = {0}" : " = '{0}'", propertyDescriptor.GetValue(thisValue));
					}
				}
			}
			catch
			{
			}

			return sb.ToString();
		}

		public static int PropertiesCount([NotNull] this object thisValue, BindingFlags bindingAttributes = BindingFlags.Default, Type returnType = null)
		{
			return thisValue.AsType()
							.PropertiesCount(bindingAttributes, returnType);
		}

		[NotNull]
		public static MethodBase GetMethod([NotNull] this object thisValue, [NotNull] LambdaExpression expression) { return thisValue.AsType().GetMethod(expression); }

		[NotNull]
		public static EventInfo GetEvent([NotNull] this object thisValue, [NotNull] LambdaExpression expression) { return thisValue.AsType().GetEvent(expression); }

		[NotNull] public static MethodInfo[] GetGetters([NotNull] this object thisValue) { return thisValue.AsType().GetGetters(); }
		[NotNull] public static MethodInfo[] GetGetters([NotNull] this object thisValue, BindingFlags bindingFlags) { return thisValue.AsType().GetGetters(bindingFlags); }

		[NotNull] public static MethodInfo[] GetSetters([NotNull] this object thisValue) { return thisValue.AsType().GetGetters(); }
		[NotNull] public static MethodInfo[] GetSetters([NotNull] this object thisValue, BindingFlags bindingFlags) { return thisValue.AsType().GetSetters(bindingFlags); }

		public static IEnumerable<PropertyInfo> GetProperties([NotNull] this object thisValue, PropertyInfoType type) { return thisValue.AsType().GetProperties(type); }
		public static IEnumerable<PropertyInfo> GetProperties([NotNull] this object thisValue, BindingFlags bindingFlags, PropertyInfoType type)
		{
			return thisValue.AsType().GetProperties(bindingFlags, type);
		}

		public static IEnumerable<PropertyInfo> GetProperties([NotNull] this object thisValue, PropertyInfoType type, Type returnType)
		{
			return thisValue.AsType().GetProperties(type, returnType);
		}

		public static IEnumerable<PropertyInfo> GetProperties([NotNull] this object thisValue, BindingFlags bindingFlags, PropertyInfoType type, Type returnType)
		{
			return thisValue.AsType().GetProperties(bindingFlags, type, returnType);
		}

		[NotNull]
		public static IEnumerable<PropertyInfo> GetProperties([NotNull] this object thisValue, [NotNull] params Expression<Func<object, PropertyInfo>>[] expressions)
		{
			return thisValue.AsType().GetProperties(expressions);
		}

		[NotNull]
		public static PropertyInfo GetProperty([NotNull] this object thisValue, [NotNull] LambdaExpression expression) { return thisValue.AsType().GetProperty(expression); }

		[NotNull]
		public static FieldInfo GetField([NotNull] this object thisValue, [NotNull] LambdaExpression expression) { return thisValue.AsType().GetField(expression); }

		[NotNull]
		public static MemberInfo GetMember([NotNull] this object thisValue, [NotNull] LambdaExpression expression)
		{
			return thisValue.AsType().GetMember(expression);
		}

		public static TAttribute GetPropertyAttribute<TAttribute>([NotNull] this object thisValue, [NotNull] LambdaExpression expression, bool inherit = false)
			where TAttribute : Attribute
		{
			PropertyInfo property = GetProperty(thisValue, expression);
			return property.GetAttribute<TAttribute>(inherit);
		}

		[NotNull]
		public static string GetPropertyDisplayName([NotNull] this object thisValue, [NotNull] LambdaExpression expression)
		{
			PropertyInfo property = GetProperty(thisValue, expression);
			return property.GetDisplayName(property.Name);
		}

		[NotNull]
		public static DisplayAttribute GetPropertyDisplay([NotNull] this object thisValue, [NotNull] LambdaExpression expression)
		{
			PropertyInfo property = GetProperty(thisValue, expression);
			return property.GetDisplay();
		}

		public static int FieldCount([NotNull] this object thisValue, BindingFlags bindingAttributes = BindingFlags.Default, Type baseType = null)
		{
			return thisValue.AsType()
							.FieldsCount(bindingAttributes, baseType);
		}

		public static TAttribute GetFieldAttribute<TAttribute>([NotNull] this object thisValue, [NotNull] LambdaExpression expression)
			where TAttribute : Attribute
		{
			FieldInfo field = GetField(thisValue, expression);
			return field.GetAttribute<TAttribute>();
		}

		[NotNull]
		public static string GetFieldDisplayName([NotNull] this object thisValue, [NotNull] LambdaExpression expression)
		{
			FieldInfo field = GetField(thisValue, expression);
			return field.GetDisplayName(field.Name);
		}

		[NotNull]
		public static DisplayAttribute GetFieldDisplay([NotNull] this object thisValue, [NotNull] LambdaExpression expression)
		{
			FieldInfo field = GetField(thisValue, expression);
			return field.GetDisplay();
		}

		[NotNull]
		public static string GetMemberDisplayName([NotNull] this object thisValue, [NotNull] LambdaExpression expression)
		{
			MemberInfo member = GetMember(thisValue, expression);
			return member.GetDisplayName(member.Name);
		}

		public static FieldInfo GetField([NotNull] this object thisValue, [NotNull] string name, BindingFlags bindingFlags = Constants.BF_PUBLIC_NON_PUBLIC_INSTANCE_STATIC,  Type type = null)
		{
			FieldInfo info = thisValue.AsType().FindField(name, bindingFlags, type);
			return info;
		}

		public static bool SetFieldValue<T>([NotNull] this object thisValue, [NotNull] LambdaExpression expression, T value)
		{
			FieldInfo info = GetField(thisValue, expression);
			info.SetValue(thisValue, value);
			return true;
		}

		public static bool SetFieldValue<T>([NotNull] this object thisValue, [NotNull] string name, T value, BindingFlags bindingFlags = Constants.BF_PUBLIC_NON_PUBLIC_INSTANCE_STATIC, Type type = null)
		{
			FieldInfo info = GetField(thisValue, name, bindingFlags, type ?? typeof(T));
			if (info == null) return false;
			info.SetValue(thisValue, value);
			return true;
		}

		public static T GetFieldValueOr<T>([NotNull] this object thisValue, [NotNull] LambdaExpression expression, T defaultValue)
		{
			T value;

			try
			{
				FieldInfo info = GetField(thisValue, expression);
				value = info.GetValue(thisValue).To(defaultValue);
			}
			catch
			{
				value = defaultValue;
			}

			return value;
		}

		public static T GetFieldValue<T>([NotNull] this object thisValue, [NotNull] LambdaExpression expression, T defaultValue)
		{
			FieldInfo info = GetField(thisValue, expression);
			T value = info.GetValue(thisValue).To(defaultValue);
			return value;
		}

		public static T GetFieldValueOr<T>([NotNull] this object thisValue, [NotNull] string name, T defaultValue, BindingFlags bindingFlags = Constants.BF_PUBLIC_NON_PUBLIC_INSTANCE_STATIC, Type type = null)
		{
			T value;

			try
			{
				if (!GetFieldValue(thisValue, name, out value, defaultValue, bindingFlags, type))
					value = defaultValue;
			}
			catch
			{
				value = defaultValue;
			}

			return value;
		}

		public static bool GetFieldValue<T>([NotNull] this object thisValue, [NotNull] string name, out T value, T defaultValue, BindingFlags bindingFlags = Constants.BF_PUBLIC_NON_PUBLIC_INSTANCE_STATIC, Type type = null)
		{
			value = defaultValue;
			FieldInfo info = GetField(thisValue, name, bindingFlags, type ?? typeof(T));
			if (info == null) return false;
			value = info.GetValue(thisValue).To(defaultValue);
			return true;
		}

		public static PropertyInfo GetProperty([NotNull] this object thisValue, [NotNull] string name, BindingFlags bindingFlags = Constants.BF_PUBLIC_NON_PUBLIC_INSTANCE_STATIC,  Type returnType = null)
		{
			PropertyInfo info = thisValue.AsType().FindProperty(name, bindingFlags, null, returnType);
			return info;
		}

		public static bool SetPropertyValue<T>([NotNull] this object thisValue, [NotNull] LambdaExpression expression, T value)
		{
			PropertyInfo info = GetProperty(thisValue, expression);
			info.SetValue(thisValue, value);
			return true;
		}

		public static bool SetPropertyValue<T>([NotNull] this object thisValue, [NotNull] string name, T value, params object[] indexes)
		{
			return SetPropertyValue(thisValue, name, value, Constants.BF_PUBLIC_NON_PUBLIC_INSTANCE_STATIC, typeof(T), indexes);
		}

		public static bool SetPropertyValue<T>([NotNull] this object thisValue, [NotNull] string name, T value, [NotNull] Type returnType, params object[] indexes)
		{
			return SetPropertyValue(thisValue, name, value, Constants.BF_PUBLIC_NON_PUBLIC_INSTANCE_STATIC, returnType, indexes);
		}

		public static bool SetPropertyValue<T>([NotNull] this object thisValue, [NotNull] string name, T value, BindingFlags bindingFlags, params object[] indexes)
		{
			return SetPropertyValue(thisValue, name, value, bindingFlags, typeof(T), indexes);
		}

		public static bool SetPropertyValue<T>([NotNull] this object thisValue, [NotNull] string name, T value, BindingFlags bindingFlags, Type returnType, params object[] indexes)
		{
			Type type = thisValue.AsType();
			PropertyInfo info = type.FindProperty(name, bindingFlags, null, returnType);
			if (info == null) return false;

			ParameterInfo[] parameters = info.GetIndexParameters();

			if (!indexes.IsNullOrEmpty() && parameters.Length == 0)
			{
				thisValue = info.GetValue(thisValue, null);
				type = thisValue?.AsType() ?? throw new NotFoundException($"No indexed property was found with the name '{name}'.");

				PropertyInfo[] props = type.GetProperties(p =>
				{
					ParameterInfo[] pi = p.GetIndexParameters();
					return ValidateParametersAndValues(pi, indexes);
				}, Constants.BF_PUBLIC_NON_PUBLIC_INSTANCE_STATIC, typeof(T)).ToArray();

				if (props.Length == 0) throw new NotFoundException("No matching indexed property was found.");
				if (props.Length > 1) throw new AmbiguousMatchException("More than one indexed property found with the same");
				info = props[0];
				parameters = info.GetIndexParameters();
			}

			if (!info.CanWrite) throw new AccessViolationException($"Cannot write value to property {info.Name}.");

			if (parameters.Length == 0)
			{
				info.SetValue(thisValue, value, null);
				return true;
			}

			if (!ValidateParametersAndValues(parameters, indexes)) throw new ArrayTypeMismatchException($"Either indexes arguments length does not match the required parameters by the property {info.Name} or one of the provided indexes type mismatches.");
			info.SetValue(thisValue, value, indexes);
			return true;
		}

		public static T GetPropertyValueOr<T>([NotNull] this object thisValue, [NotNull] LambdaExpression expression, T defaultValue)
		{
			T value;

			try
			{
				PropertyInfo info = GetProperty(thisValue, expression);
				value = info.GetValue(thisValue).To(defaultValue);
			}
			catch
			{
				value = defaultValue;
			}

			return value;
		}

		public static T GetPropertyValue<T>([NotNull] this object thisValue, [NotNull] LambdaExpression expression, T defaultValue)
		{
			PropertyInfo info = GetProperty(thisValue, expression);
			T value = info.GetValue(thisValue).To(defaultValue);
			return value;
		}

		public static T GetPropertyValueOr<T>([NotNull] this object thisValue, [NotNull] string name, T defaultValue = default(T), params object[] indexes)
		{
			return GetPropertyValueOr(thisValue, name, Constants.BF_PUBLIC_NON_PUBLIC_INSTANCE_STATIC, typeof(T), defaultValue, indexes);
		}

		public static T GetPropertyValueOr<T>([NotNull] this object thisValue, [NotNull] string name, [NotNull] Type returnType, T defaultValue = default(T), params object[] indexes)
		{
			return GetPropertyValueOr(thisValue, name, Constants.BF_PUBLIC_NON_PUBLIC_INSTANCE_STATIC, returnType, defaultValue, indexes);
		}

		public static T GetPropertyValueOr<T>([NotNull] this object thisValue, [NotNull] string name, BindingFlags bindingFlags, T defaultValue = default(T), params object[] indexes)
		{
			return GetPropertyValueOr(thisValue, name, bindingFlags, typeof(T), defaultValue, indexes);
		}

		public static T GetPropertyValueOr<T>([NotNull] this object thisValue, [NotNull] string name, BindingFlags bindingFlags,  Type returnType,
			T defaultValue = default(T), params object[] indexes)
		{
			T value;

			try
			{
				if (!GetPropertyValue(thisValue, name, out value, bindingFlags, typeof(T), defaultValue, indexes))
					value = defaultValue;
			}
			catch
			{
				value = defaultValue;
			}

			return value;
		}

		public static bool GetPropertyValue<T>([NotNull] this object thisValue, [NotNull] string name, out T value, T defaultValue = default(T), params object[] indexes)
		{
			return GetPropertyValue(thisValue, name, out value, Constants.BF_PUBLIC_NON_PUBLIC_INSTANCE_STATIC, typeof(T), defaultValue, indexes);
		}

		public static bool GetPropertyValue<T>([NotNull] this object thisValue, [NotNull] string name, out T value, [NotNull] Type returnType, T defaultValue = default(T), params object[] indexes)
		{
			return GetPropertyValue(thisValue, name, out value, Constants.BF_PUBLIC_NON_PUBLIC_INSTANCE_STATIC, returnType, defaultValue, indexes);
		}

		public static bool GetPropertyValue<T>([NotNull] this object thisValue, [NotNull] string name, out T value, BindingFlags bindingFlags, T defaultValue = default(T), params object[] indexes)
		{
			return GetPropertyValue(thisValue, name, out value, bindingFlags, typeof(T), defaultValue, indexes);
		}

		public static bool GetPropertyValue<T>([NotNull] this object thisValue, [NotNull] string name, out T value, BindingFlags bindingFlags, Type returnType, T defaultValue = default(T), params object[] indexes)
		{
			value = defaultValue;

			Type type = thisValue.AsType();
			PropertyInfo info = type.FindProperty(name, bindingFlags, null, returnType);
			if (info == null) return false;

			ParameterInfo[] parameters = info.GetIndexParameters();

			if (!indexes.IsNullOrEmpty() && parameters.Length == 0)
			{
				thisValue = info.GetValue(thisValue, null);
				type = thisValue?.AsType() ?? throw new NotFoundException($"No indexed property was found with the name '{name}'.");

				PropertyInfo[] props = type.GetProperties(p =>
				{
					ParameterInfo[] pi = p.GetIndexParameters();
					return ValidateParametersAndValues(pi, indexes);
				}, Constants.BF_PUBLIC_NON_PUBLIC_INSTANCE_STATIC, typeof(T)).ToArray();

				if (props.Length == 0) throw new NotFoundException("No matching indexed property was found.");
				if (props.Length > 1) throw new AmbiguousMatchException("More than one indexed property found with the same");
				info = props[0];
				parameters = info.GetIndexParameters();
			}

			if (!info.CanRead) throw new AccessViolationException($"Property {info.Name} cannot be read.");

			if (parameters.Length == 0)
			{
				value = info.GetValue(thisValue, null).To(defaultValue);
				return true;
			}

			if (!ValidateParametersAndValues(parameters, indexes)) throw new ArrayTypeMismatchException($"Either indexes arguments length does not match the required parameters by the property {info.Name} or one of the provided indexes type mismatches.");
			value = info.GetValue(thisValue, indexes).To(defaultValue);
			return true;
		}

		public static bool SetMemberValue<T>([NotNull] this object thisValue, [NotNull] LambdaExpression expression, T value)
		{
			MemberInfo info = GetMember(thisValue, expression);
			FieldInfo field = info as FieldInfo;

			if (field != null)
				field.SetValue(thisValue, value);
			else
				((PropertyInfo)info).SetValue(thisValue, value);

			return true;
		}

		public static bool SetMemberValue<T>([NotNull] this object thisValue, [NotNull] string name, T value)
		{
			return SetMemberValue(thisValue, name, value, Constants.BF_PUBLIC_NON_PUBLIC_INSTANCE_STATIC, typeof(T));
		}

		public static bool SetMemberValue<T>([NotNull] this object thisValue, [NotNull] string name, T value, [NotNull] Type returnType)
		{
			return SetMemberValue(thisValue, name, value, Constants.BF_PUBLIC_NON_PUBLIC_INSTANCE_STATIC, returnType);
		}

		public static bool SetMemberValue<T>([NotNull] this object thisValue, [NotNull] string name, T value, BindingFlags bindingFlags)
		{
			return SetMemberValue(thisValue, name, value, bindingFlags, typeof(T));
		}

		public static bool SetMemberValue<T>([NotNull] this object thisValue, [NotNull] string name, T value, BindingFlags bindingFlags, Type returnType)
		{
			Type type = thisValue.AsType();
			FieldInfo field = type.FindField(name, bindingFlags, returnType ?? typeof(T));

			if (field != null)
			{
				field.SetValue(thisValue, value);
				return true;
			}

			PropertyInfo info = type.FindProperty(name, bindingFlags, null, returnType);
			if (info == null) return false;
			if (!info.CanWrite) throw new AccessViolationException($"Cannot write value to property {info.Name}.");
			info.SetValue(thisValue, value, null);
			return true;
		}

		public static T GetMemberValueOr<T>([NotNull] this object thisValue, [NotNull] LambdaExpression expression, T defaultValue)
		{
			T value;

			try
			{
				MemberInfo info = GetMember(thisValue, expression);
				FieldInfo field = info as FieldInfo;
				value = field != null
					? field.GetValue(thisValue).To(defaultValue)
					: ((PropertyInfo)info).GetValue(thisValue).To(defaultValue);
			}
			catch
			{
				value = defaultValue;
			}

			return value;
		}

		public static T GetMemberValue<T>([NotNull] this object thisValue, [NotNull] LambdaExpression expression, T defaultValue)
		{
			MemberInfo info = GetMember(thisValue, expression);
			FieldInfo field = info as FieldInfo;
			T value = field != null 
				? field.GetValue(thisValue).To(defaultValue) 
				: ((PropertyInfo)info).GetValue(thisValue).To(defaultValue);

			return value;
		}

		public static T GetMemberValueOr<T>([NotNull] this object thisValue, [NotNull] string name, T defaultValue = default(T))
		{
			return GetMemberValueOr(thisValue, name, Constants.BF_PUBLIC_NON_PUBLIC_INSTANCE_STATIC, typeof(T), defaultValue);
		}

		public static T GetMemberValueOr<T>([NotNull] this object thisValue, [NotNull] string name, [NotNull] Type returnType, T defaultValue = default(T))
		{
			return GetMemberValueOr(thisValue, name, Constants.BF_PUBLIC_NON_PUBLIC_INSTANCE_STATIC, returnType, defaultValue);
		}

		public static T GetMemberValueOr<T>([NotNull] this object thisValue, [NotNull] string name, BindingFlags bindingFlags, T defaultValue = default(T))
		{
			return GetMemberValueOr(thisValue, name, bindingFlags, typeof(T), defaultValue);
		}

		public static T GetMemberValueOr<T>([NotNull] this object thisValue, [NotNull] string name, BindingFlags bindingFlags,  Type returnType, T defaultValue = default(T))
		{
			T value;

			try
			{
				if (!GetMemberValue(thisValue, name, out value, bindingFlags, returnType ?? typeof(T), defaultValue))
					value = defaultValue;
			}
			catch
			{
				value = defaultValue;
			}

			return value;
		}

		public static bool GetMemberValue<T>([NotNull] this object thisValue, [NotNull] string name, out T value, T defaultValue = default(T))
		{
			return GetMemberValue(thisValue, name, out value, Constants.BF_PUBLIC_NON_PUBLIC_INSTANCE_STATIC, typeof(T), defaultValue);
		}

		public static bool GetMemberValue<T>([NotNull] this object thisValue, [NotNull] string name, out T value, [NotNull] Type returnType, T defaultValue = default(T))
		{
			return GetMemberValue(thisValue, name, out value, Constants.BF_PUBLIC_NON_PUBLIC_INSTANCE_STATIC, returnType, defaultValue);
		}

		public static bool GetMemberValue<T>([NotNull] this object thisValue, [NotNull] string name, out T value, BindingFlags bindingFlags, T defaultValue = default(T))
		{
			return GetPropertyValue(thisValue, name, out value, bindingFlags, typeof(T), defaultValue);
		}

		public static bool GetMemberValue<T>([NotNull] this object thisValue, [NotNull] string name, out T value, BindingFlags bindingFlags, Type returnType, T defaultValue = default(T))
		{
			value = defaultValue;

			Type type = thisValue.AsType();
			FieldInfo field = type.FindField(name, bindingFlags, returnType ?? typeof(T));

			if (field != null)
			{
				value = field.GetValue(thisValue).To(defaultValue);
				return true;
			}

			PropertyInfo info = type.FindProperty(name, bindingFlags, null, returnType);
			if (info == null) return false;
			if (!info.CanRead) throw new AccessViolationException($"Property {info.Name} cannot be read.");
			value = info.GetValue(thisValue, null).To(defaultValue);
			return true;
		}

		[NotNull]
		public static object ToEnum([NotNull] this object thisValue, [NotNull] Type type)
		{
			return thisValue is string value
						? Enum.Parse(type, value, true)
						: Enum.ToObject(type, thisValue);
		}

		public static string ObjectName(this object thisValue) { return thisValue.AsType()?.FullName; }

		private static bool ValidateParametersAndValues(ParameterInfo[] parameters, object[] values)
		{
			if (parameters.IsNullOrEmpty()) return values.IsNullOrEmpty();

			int n = parameters.Count(p => !p.IsOptional);
			if (values == null || values.Length < n || values.Length > parameters.Length) return false;

			return values.All((x, i) =>
			{
				if (x == null) return !parameters[i].ParameterType.IsValueType;
				return x.GetType().IsAssignableFrom(parameters[i].ParameterType);
			});
		}
	}
}
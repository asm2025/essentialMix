using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using asm.Collections;
using JetBrains.Annotations;
using Microsoft.CSharp.RuntimeBinder;
using asm.Helpers;
using asm.Reflection;
using Binder = System.Reflection.Binder;
using CSharpBinder = Microsoft.CSharp.RuntimeBinder.Binder;

namespace asm.Extensions
{
	public static class TypeExtension
	{
		private const BindingFlags BF_FIND_EVENT_FIELD = Constants.BF_PUBLIC_NON_PUBLIC_INSTANCE_STATIC | BindingFlags.DeclaredOnly;

		private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<string, MethodInfo>> __explicitInterfaceCache = new ConcurrentDictionary<Type, ConcurrentDictionary<string, MethodInfo>>();
		private static readonly ConcurrentDictionary<KeyValuePair<Type, Type>, bool> __castCache = new ConcurrentDictionary<KeyValuePair<Type, Type>, bool>();
		private static readonly ConcurrentDictionary<KeyValuePair<Type, Type>, bool> __implicitCastCache = new ConcurrentDictionary<KeyValuePair<Type, Type>, bool>();

		private static readonly IReadOnlySet<Type> __findInterfaceExceptionBreak = new ReadOnlySet<Type>(new HashSet<Type>
		{
			typeof(NotSupportedException),
			typeof(ArgumentException),
			typeof(NullReferenceException),
		});

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static TypeCode AsTypeCode([NotNull] this Type thisValue, bool resolveGenerics = false)
		{
			return Type.GetTypeCode(resolveGenerics
										? ResolveType(thisValue)
										: thisValue);
		}

		public static Type[] GetGenericArguments([NotNull] this Type thisValue, [NotNull] Type genericTypeDefinition)
		{
			if (!genericTypeDefinition.IsGenericTypeDefinition) throw new ArgumentException("Argument is not a generic type definition.", nameof(genericTypeDefinition));

			if (genericTypeDefinition.IsInterface)
			{
				Type type = thisValue.GetInterfaces()
					.FirstOrDefault(e => e.IsGenericType && e.GetGenericTypeDefinition() == genericTypeDefinition);
				return type.NullSafe(e => e.GetGenericArguments(), Type.EmptyTypes);
			}

			for (Type type = thisValue; type != null; type = type.BaseType)
			{
				if (!type.IsGenericType || type.GetGenericTypeDefinition() != genericTypeDefinition) continue;
				return thisValue.GetGenericArguments();
			}

			return Type.EmptyTypes;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static IList CreateList([NotNull] this Type thisValue, params object[] arguments)
		{
			return typeof(List<>).MakeGenericType(thisValue).CreateInstance<IList>(arguments);
		}

		[NotNull]
		public static MethodBase GetMethod([NotNull] this Type thisValue, [NotNull] LambdaExpression expression)
		{
			MethodBase result = expression.GetMethod();
			if (result == null || result.ReflectedType == null || thisValue != result.ReflectedType && !thisValue.IsSubclassOf(result.ReflectedType))
				throw new ArgumentException($"Expression '{expression}' refers to a method that is not from type {thisValue}.");

			return result;
		}

		[NotNull]
		public static EventInfo GetEvent([NotNull] this Type thisValue, [NotNull] LambdaExpression expression)
		{
			EventInfo result = expression.GetEvent();
			if (result.ReflectedType == null || thisValue != result.ReflectedType && !thisValue.IsSubclassOf(result.ReflectedType))
				throw new ArgumentException($"Expression '{expression}' refers to an event that is not from type {thisValue}.");

			return result;
		}

		[NotNull]
		public static PropertyInfo GetProperty([NotNull] this Type thisValue, [NotNull] LambdaExpression expression)
		{
			PropertyInfo result = expression.GetProperty();
			if (result.ReflectedType == null || thisValue != result.ReflectedType && !thisValue.IsSubclassOf(result.ReflectedType))
				throw new ArgumentException($"Expression '{expression}' refers to a property that is not from type {thisValue}.");

			return result;
		}

		public static PropertyInfo GetProperty([NotNull] this Type thisValue, [NotNull] string name, params Type[] types)
		{
			return GetProperty(thisValue, name, BindingFlags.Default, null, null, types, null);
		}

		public static PropertyInfo GetProperty([NotNull] this Type thisValue, [NotNull] string name, BindingFlags bindingAttributes, params Type[] types)
		{
			return GetProperty(thisValue, name, bindingAttributes, null, null, types, null);
		}

		public static PropertyInfo GetProperty([NotNull] this Type thisValue, [NotNull] string name, BindingFlags bindingAttributes, Type returnType, params Type[] types)
		{
			return GetProperty(thisValue, name, bindingAttributes, null, returnType, types, null);
		}

		public static PropertyInfo GetProperty([NotNull] this Type thisValue, [NotNull] string name, BindingFlags bindingAttributes, Binder binder, Type returnType, params Type[] types)
		{
			return GetProperty(thisValue, name, bindingAttributes, binder, returnType, types, null);
		}

		public static PropertyInfo GetProperty([NotNull] this Type thisValue, [NotNull] string name, BindingFlags bindingAttributes, Binder binder, Type returnType, Type[] types, params ParameterModifier[] modifiers)
		{
			return thisValue.GetProperty(name, bindingAttributes, binder, returnType, types ?? Type.EmptyTypes, modifiers);
		}

		[NotNull] 
		public static MethodInfo[] GetGetters([NotNull] this Type thisValue) { return GetGetters(thisValue, Constants.BF_PUBLIC_NON_PUBLIC_INSTANCE_STATIC); }

		[NotNull]
		public static MethodInfo[] GetGetters([NotNull] this Type thisValue, BindingFlags bindingFlags)
		{
			return thisValue.GetMethods(m => m.Name.StartsWith("get_") && m.GetParameters().Length == 0, bindingFlags).ToArray();
		}

		[NotNull] 
		public static MethodInfo[] GetSetters([NotNull] this Type thisValue) { return GetGetters(thisValue, Constants.BF_PUBLIC_NON_PUBLIC_INSTANCE_STATIC); }

		[NotNull]
		public static MethodInfo[] GetSetters([NotNull] this Type thisValue, BindingFlags bindingFlags)
		{
			return thisValue.GetMethods(m => m.Name.StartsWith("set_") && m.GetParameters().Length == 1, bindingFlags).ToArray();
		}

		[ItemNotNull]
		public static IEnumerable<PropertyInfo> GetProperties([NotNull] this Type thisValue, [NotNull] Predicate<PropertyInfo> selector, BindingFlags bindingAttributes = BindingFlags.Default, Type returnType = null)
		{
			Type type = thisValue;
			if (returnType == null && !bindingAttributes.HasFlag(BindingFlags.IgnoreReturn)) bindingAttributes |= BindingFlags.IgnoreReturn;
			bool declaredOnly = bindingAttributes.HasFlag(BindingFlags.DeclaredOnly);

			do
			{
				foreach (PropertyInfo property in type.GetProperties(bindingAttributes))
				{
					if (!selector(property)) continue;
					yield return property;
				}
			}
			while (!declaredOnly && (type = type.BaseType) != null);
		}

		public static IEnumerable<PropertyInfo> GetProperties([NotNull] this Type thisValue, PropertyInfoType type)
		{
			return GetProperties(thisValue, Constants.BF_PUBLIC_NON_PUBLIC_INSTANCE_STATIC, type, null);
		}

		public static IEnumerable<PropertyInfo> GetProperties([NotNull] this Type thisValue, BindingFlags bindingFlags, PropertyInfoType type)
		{
			return GetProperties(thisValue, bindingFlags, type, null);
		}

		public static IEnumerable<PropertyInfo> GetProperties([NotNull] this Type thisValue, PropertyInfoType type, Type returnType)
		{
			return GetProperties(thisValue, Constants.BF_PUBLIC_NON_PUBLIC_INSTANCE_STATIC, type, returnType);
		}

		public static IEnumerable<PropertyInfo> GetProperties([NotNull] this Type thisValue, BindingFlags bindingFlags, PropertyInfoType type, Type returnType)
		{
			if (type == PropertyInfoType.Default) type = PropertyInfoType.Get;
			return GetProperties(thisValue, p => type.HasFlag(PropertyInfoType.Get) && p.GetGetMethod() != null || type.HasFlag(PropertyInfoType.Set) && p.GetSetMethod() != null, bindingFlags, returnType);
		}

		[NotNull]
		[ItemNotNull]
		public static IEnumerable<PropertyInfo> GetProperties([NotNull] this Type thisValue, [NotNull] params Expression<Func<object, PropertyInfo>>[] expressions)
		{
			foreach (Expression<Func<object, PropertyInfo>> expression in expressions)
			{
				PropertyInfo property = expression.GetProperty();
				if (property.ReflectedType == null || thisValue != property.ReflectedType && !thisValue.IsSubclassOf(property.ReflectedType))
					throw new ArgumentException($"Expression '{expression}' refers to a property that is not from type {thisValue}.");
				yield return property;
			}
		}

		[NotNull]
		public static string GetPropertyDisplayName([NotNull] this Type thisValue, [NotNull] LambdaExpression expression)
		{
			PropertyInfo property = GetProperty(thisValue, expression);
			return property.GetDisplayName(property.Name);
		}

		[NotNull]
		public static DisplayAttribute GetPropertyDisplay([NotNull] this Type thisValue, [NotNull] LambdaExpression expression)
		{
			PropertyInfo property = GetProperty(thisValue, expression);
			return property.GetDisplay();
		}

		[NotNull]
		public static FieldInfo GetField([NotNull] this Type thisValue, [NotNull] LambdaExpression expression)
		{
			FieldInfo result = expression.GetField();
			if (result.ReflectedType == null || thisValue != result.ReflectedType && !thisValue.IsSubclassOf(result.ReflectedType))
				throw new ArgumentException($"Expression '{expression}' refers to a field that is not from type {thisValue}.");

			return result;
		}

		[NotNull]
		public static string GetFieldDisplayName([NotNull] this Type thisValue, [NotNull] LambdaExpression expression)
		{
			FieldInfo field = GetField(thisValue, expression);
			return field.GetDisplayName(field.Name);
		}

		[NotNull]
		public static DisplayAttribute GetFieldDisplay<TField>([NotNull] this Type thisValue, [NotNull] Expression<Func<TField>> expression)
		{
			FieldInfo field = GetField(thisValue, expression);
			return field.GetDisplay();
		}

		[NotNull]
		public static MemberInfo GetMember([NotNull] this Type thisValue, [NotNull] LambdaExpression expression)
		{
			MemberInfo memberInfo = expression.GetMember();
			if (memberInfo.ReflectedType == null || thisValue != memberInfo.ReflectedType && !thisValue.IsSubclassOf(memberInfo.ReflectedType))
				throw new ArgumentException($"Expression '{expression}' refers to a property or field that is not from type {thisValue}.");

			return memberInfo;
		}

		public static MemberInfo GetPropertyOrField([NotNull] this Type thisValue, string name, BindingFlags bindingAttributes = BindingFlags.Default)
		{
			return FindMember(thisValue, name, MemberTypes.Property | MemberTypes.Field, bindingAttributes);
		}

		[NotNull]
		public static string GetMemberDisplayName([NotNull] this Type thisValue, [NotNull] LambdaExpression expression)
		{
			MemberInfo member = GetMember(thisValue, expression);
			return member.GetDisplayName(member.Name);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static object Default([NotNull] this Type thisValue)
		{
			return thisValue.IsClass
						? null
						: Activator.CreateInstance(thisValue);
		}

		public static IEnumerable<Type> SelfAndMembersTypes([NotNull] this Type thisValue, BreadthDepthTraverse method)
		{
			return method switch
			{
				BreadthDepthTraverse.BreadthFirst => BFSelfAndMembersTypesLocal(thisValue),
				BreadthDepthTraverse.DepthFirst => DFSelfAndMembersTypesLocal(thisValue),
				_ => throw new ArgumentOutOfRangeException(nameof(method), method, null)
			};

			static IEnumerable<Type> BFSelfAndMembersTypesLocal(Type value)
			{
				Queue<Type> queue = new Queue<Type>();
				queue.Enqueue(value);

				while (queue.Count > 0)
				{
					Type type = queue.Dequeue();
					yield return type;

					foreach (MemberInfo neighbor in type.GetMembers(Constants.BF_PUBLIC_INSTANCE).Where(e => e.MemberType == MemberTypes.Property || e.MemberType == MemberTypes.Field))
					{
						switch (neighbor.MemberType)
						{
							case MemberTypes.Property:
								queue.Enqueue(((PropertyInfo)neighbor).PropertyType);
								break;
							case MemberTypes.Field:
								queue.Enqueue(((FieldInfo)neighbor).FieldType);
								break;
						}
					}
				}
			}

			static IEnumerable<Type> DFSelfAndMembersTypesLocal(Type value)
			{
				Stack<Type> stack = new Stack<Type>();
				stack.Push(value);

				while (stack.Count > 0)
				{
					Type type = stack.Pop();
					yield return type;

					foreach (MemberInfo neighbor in type.GetMembers(Constants.BF_PUBLIC_INSTANCE).Where(e => e.MemberType == MemberTypes.Property || e.MemberType == MemberTypes.Field))
					{
						switch (neighbor.MemberType)
						{
							case MemberTypes.Property:
								stack.Push(((PropertyInfo)neighbor).PropertyType);
								break;
							case MemberTypes.Field:
								stack.Push(((FieldInfo)neighbor).FieldType);
								break;
						}
					}
				}
			}
		}

		public static Type FindInterface([NotNull] this Type thisValue, [NotNull] string name, bool declaredOnly = false, bool ignoreCase = true)
		{
			if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

			Type type = thisValue;
			Type interfaceType = null;

			do
			{
				try
				{
					interfaceType = type.GetInterface(name, ignoreCase);
				}
				catch (Exception e)
				{
					if (__findInterfaceExceptionBreak.Contains(e.GetType())) break;
					// ignored
				}
			}
			while (interfaceType == null && !declaredOnly && (type = type.BaseType) != null);

			return interfaceType;
		}

		[ItemNotNull]
		public static IEnumerable<Type> GetInterfaces([NotNull] this Type thisValue, bool declaredOnly)
		{
			Type type = thisValue;

			do
			{
				foreach (Type interfaceType in type.GetInterfaces())
					yield return interfaceType;
			}
			while (!declaredOnly && (type = type.BaseType) != null);
		}
		[ItemNotNull]
		public static IEnumerable<Type> GetInterfaces([NotNull] this Type thisValue, [NotNull] Predicate<Type> selector, bool declaredOnly = false)
		{
			Type type = thisValue;

			do
			{
				foreach (Type interfaceType in type.GetInterfaces())
				{
					if (!selector(interfaceType)) continue;
					yield return interfaceType;
				}
			}
			while (!declaredOnly && (type = type.BaseType) != null);
		}

		public static Type FindInterface<T>([NotNull] this Type thisValue, bool declaredOnly = false)
			where T : class
		{
			return FindInterface(thisValue, typeof(T), declaredOnly);
		}

		public static Type FindInterface([NotNull] this Type thisValue, [NotNull] Type type, bool declaredOnly = false)
		{
			if (!type.IsInterface) throw new ArgumentException("Type is not an interface.", nameof(type));
			return type.IsGenericType
				? thisValue.GetInterfaces(x => x.IsGenericType && x.GetGenericTypeDefinition().IsAssignableTo(type), declaredOnly).FirstOrDefault()
				: thisValue.GetInterfaces(x => x.IsAssignableTo(type), declaredOnly).FirstOrDefault();
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool Implements<T>([NotNull] this Type thisValue) { return Implements(thisValue, typeof(T)); }
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool Implements([NotNull] this Type thisValue, [NotNull] Type type)
		{
			if (!type.IsInterface) throw new ArgumentException("Type is not an interface.", nameof(type));
			return type.IsAssignableFrom(thisValue);
		}

		public static bool Implements<T>([NotNull] this Type thisValue, bool declaredOnly) { return Implements(thisValue, typeof(T), declaredOnly); }
		public static bool Implements([NotNull] this Type thisValue, [NotNull] Type type, bool declaredOnly)
		{
			if (!type.IsInterface) throw new ArgumentException("Type is not an interface.", nameof(type));

			Type interfaceType = null;

			while (thisValue != null && interfaceType == null)
			{
				interfaceType = thisValue.GetInterface(type.Name);
				if (!interfaceType.IsAssignableTo(type)) interfaceType = null;
				thisValue = declaredOnly
								? null
								: thisValue.BaseType;
			}

			return interfaceType != null;
		}

		public static bool Implements([NotNull] this Type thisValue, [NotNull] IEnumerable<Type> types, bool declaredOnly = false)
		{
			if (types == null) throw new ArgumentNullException(nameof(types));

			switch (types)
			{
				case ISet<Type> set:
					return set.Count > 0 && GetInterfaces(thisValue, set.Contains, declaredOnly).Count() == set.Count;
				case ICollection<Type> collection:
					return collection.Count > 0 && GetInterfaces(thisValue, collection.Contains, declaredOnly).Count() == collection.Count;
				case IReadOnlyCollection<Type> readOnlyCollection:
					return readOnlyCollection.Count > 0 && GetInterfaces(thisValue, readOnlyCollection.Contains, declaredOnly).Count() == readOnlyCollection.Count;
				default:
					ISet<Type> hashTypes = types.ToHashSet();
					return hashTypes.Count > 0 && GetInterfaces(thisValue, hashTypes.Contains, declaredOnly).Count() == hashTypes.Count;
			}
		}

		public static bool Implements([NotNull] this Type thisValue, [NotNull] params Type[] types) { return Implements(thisValue, false, types); }

		public static bool Implements([NotNull] this Type thisValue, bool declaredOnly, [NotNull] params Type[] types)
		{
			if (types == null) throw new ArgumentNullException(nameof(types));
			return types.Length > 0 && GetInterfaces(thisValue, types.Contains, declaredOnly).Count() == types.Length;
		}

		public static bool ImplementsAny([NotNull] this Type thisValue, [NotNull] IEnumerable<Type> types, bool declaredOnly = false)
		{
			if (types == null) throw new ArgumentNullException(nameof(types));

			switch (types)
			{
				case ISet<Type> set:
					return set.Count > 0 && GetInterfaces(thisValue, set.Contains, declaredOnly).Any();
				case ICollection<Type> collection:
					return collection.Count > 0 && GetInterfaces(thisValue, collection.Contains, declaredOnly).Any();
				case IReadOnlyCollection<Type> readOnlyCollection:
					return readOnlyCollection.Count > 0 && GetInterfaces(thisValue, readOnlyCollection.Contains, declaredOnly).Any();
				default:
					ISet<Type> hashTypes = types.ToHashSet();
					return hashTypes.Count > 0 && GetInterfaces(thisValue, hashTypes.Contains, declaredOnly).Any();
			}
		}

		public static bool ImplementsAny([NotNull] this Type thisValue, [NotNull] params Type[] types) { return ImplementsAny(thisValue, false, types); }

		public static bool ImplementsAny([NotNull] this Type thisValue, bool declaredOnly, [NotNull] params Type[] types)
		{
			if (types == null) throw new ArgumentNullException(nameof(types));
			return types.Length > 0 && GetInterfaces(thisValue, types.Contains, declaredOnly).Any();
		}

		public static Type FindNestedType([NotNull] this Type thisValue, [NotNull] string name, BindingFlags bindingAttributes = BindingFlags.Default, Type baseType = null)
		{
			if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

			Type type = thisValue;
			Type nested = null;
			bool declaredOnly = bindingAttributes.HasFlag(BindingFlags.DeclaredOnly);

			do
			{
				try
				{
					nested = type.GetNestedType(name, bindingAttributes);
					if (!nested.Is(baseType)) nested = null;
				}
				catch (Exception e)
				{
					if (__findInterfaceExceptionBreak.Contains(e.GetType())) break;
					// ignored
				}
			}
			while (nested == null && !declaredOnly && (type = type.BaseType) != null);

			return nested;
		}

		[ItemNotNull]
		public static IEnumerable<Type> GetNestedTypes([NotNull] this Type thisValue, [NotNull] Predicate<Type> selector, BindingFlags bindingAttributes = BindingFlags.Default, Type baseType = null)
		{
			Type type = thisValue;
			bool declaredOnly = bindingAttributes.HasFlag(BindingFlags.DeclaredOnly);

			do
			{
				foreach (Type nestedType in type.GetNestedTypes(bindingAttributes))
				{
					if (baseType != null && !baseType.IsAssignableFrom(nestedType) || !selector(nestedType)) continue;
					yield return nestedType;
				}
			}
			while (!declaredOnly && (type = type.BaseType) != null);
		}

		public static int NestedTypesCount([NotNull] this Type thisValue, BindingFlags bindingAttributes = BindingFlags.Default, Type baseType = null)
		{
			return GetNestedTypes(thisValue, p => true, bindingAttributes, baseType)?.Count() ?? 0;
		}

		public static FieldInfo FindField([NotNull] this Type thisValue, [NotNull] string name, BindingFlags bindingAttributes = BindingFlags.Default, Type baseType = null)
		{
			if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

			Type type = thisValue;
			FieldInfo info = null;
			bool declaredOnly = bindingAttributes.HasFlag(BindingFlags.DeclaredOnly);

			do
			{
				try
				{
					info = type.GetField(name, bindingAttributes);
					if (info != null && !info.FieldType.Is(baseType)) info = null;
				}
				catch (Exception e)
				{
					if (__findInterfaceExceptionBreak.Contains(e.GetType())) break;
					// ignored
				}
			}
			while (info == null && !declaredOnly && (type = type.BaseType) != null);

			return info;
		}

		[ItemNotNull]
		public static IEnumerable<FieldInfo> GetFields([NotNull] this Type thisValue, [NotNull] Predicate<FieldInfo> selector,
			BindingFlags bindingAttributes = BindingFlags.Default, Type baseType = null)
		{
			Type type = thisValue;
			bool declaredOnly = bindingAttributes.HasFlag(BindingFlags.DeclaredOnly);

			do
			{
				foreach (FieldInfo field in type.GetFields(bindingAttributes))
				{
					if (baseType != null && !baseType.IsAssignableFrom(field.FieldType) || !selector(field)) continue;
					yield return field;
				}
			}
			while (!declaredOnly && (type = type.BaseType) != null);
		}

		public static int FieldsCount([NotNull] this Type thisValue, BindingFlags bindingAttributes = BindingFlags.Default, Type baseType = null)
		{
			return GetFields(thisValue, p => true, bindingAttributes, baseType)?.Count() ?? 0;
		}

		public static EventInfo FindEvent([NotNull] this Type thisValue, [NotNull] string name, BindingFlags bindingAttributes = BindingFlags.Default)
		{
			if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

			Type type = thisValue;
			EventInfo info = null;
			bool declaredOnly = bindingAttributes.HasFlag(BindingFlags.DeclaredOnly);

			do
			{
				try
				{
					info = type.GetEvent(name, bindingAttributes);
				}
				catch (Exception e)
				{
					if (__findInterfaceExceptionBreak.Contains(e.GetType())) break;
					// ignored
				}
			}
			while (info == null && !declaredOnly && (type = type.BaseType) != null);

			return info;
		}

		[ItemNotNull]
		public static IEnumerable<EventInfo> GetEvents([NotNull] this Type thisValue, [NotNull] Predicate<EventInfo> selector,
			BindingFlags bindingAttributes = BindingFlags.Default)
		{
			Type type = thisValue;
			bool declaredOnly = bindingAttributes.HasFlag(BindingFlags.DeclaredOnly);

			do
			{
				foreach (EventInfo eventInfo in type.GetEvents(bindingAttributes))
				{
					if (!selector(eventInfo)) continue;
					yield return eventInfo;
				}
			}
			while (!declaredOnly && (type = type.BaseType) != null);
		}

		public static int EventsCount([NotNull] this Type thisValue, BindingFlags bindingAttributes = BindingFlags.Default)
		{
			return GetEvents(thisValue, p => true, bindingAttributes)?.Count() ?? 0;
		}

		public static FieldInfo FindEventField([NotNull] this Type thisValue, [NotNull] string name, bool declaredOnly = false)
		{
			if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

			string prop = "EVENT_" + name.ToUpper();
			Type type = thisValue;
			Type multiCastType = typeof(MulticastDelegate);
			FieldInfo field = null;

			do
			{
				try
				{
					/* Find events defined as field */
					field = type.GetField(name, BF_FIND_EVENT_FIELD);
					if (field?.FieldType.Is(multiCastType) ?? false) break;

					/* Find events defined as property { add; remove; } */
					field = type.GetField(prop, BF_FIND_EVENT_FIELD);
					if (field != null) break;
				}
				catch (Exception e)
				{
					if (__findInterfaceExceptionBreak.Contains(e.GetType())) break;
					// ignored
				}
			}
			while (field == null && !declaredOnly && (type = type.BaseType) != null);

			return field;
		}

		public static PropertyInfo FindProperty([NotNull] this Type thisValue, [NotNull] string name, BindingFlags bindingAttributes = BindingFlags.Default, Binder binder = null,
			Type returnType = null,
			ParameterModifier[] modifiers = null, params Type[] types)
		{
			if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

			Type type = thisValue;
			Type[] paramTypes = types ?? Type.EmptyTypes;
			PropertyInfo info = null;
			if (returnType == null && !bindingAttributes.HasFlag(BindingFlags.IgnoreReturn)) bindingAttributes |= BindingFlags.IgnoreReturn;

			bool declaredOnly = bindingAttributes.HasFlag(BindingFlags.DeclaredOnly);

			do
			{
				try
				{
					info = type.GetProperty(name, bindingAttributes, binder, returnType, paramTypes, modifiers);
					if (info != null && returnType != null && !info.PropertyType.Is(returnType)) info = null;
				}
				catch (Exception e)
				{
					if (__findInterfaceExceptionBreak.Contains(e.GetType())) break;
					// ignored
				}
			}
			while (info == null && !declaredOnly && (type = type.BaseType) != null);

			return info;
		}

		public static int PropertiesCount([NotNull] this Type thisValue, BindingFlags bindingAttributes = BindingFlags.Default, Type returnType = null)
		{
			return GetProperties(thisValue, p => true, bindingAttributes, returnType)?.Count() ?? 0;
		}

		public static ConstructorInfo FindConstructor([NotNull] this Type thisValue, BindingFlags bindingAttributes = BindingFlags.Default, Binder binder = null,
			CallingConventions callConvention = CallingConventions.Any, ParameterModifier[] modifiers = null, params Type[] types)
		{
			Type type = thisValue;
			ConstructorInfo info = null;
			bool declaredOnly = bindingAttributes.HasFlag(BindingFlags.DeclaredOnly);

			do
			{
				try
				{
					info = type.GetConstructor(bindingAttributes, binder, callConvention, types ?? Type.EmptyTypes, modifiers);
				}
				catch (Exception e)
				{
					if (__findInterfaceExceptionBreak.Contains(e.GetType())) break;
					// ignored
				}
			}
			while (info == null && !declaredOnly && (type = type.BaseType) != null);

			return info;
		}

		public static ConstructorInfo GetConstructor([NotNull] this Type thisValue, params Type[] types)
		{
			return GetConstructor(thisValue, BindingFlags.Default, null, CallingConventions.Any, types, null);
		}

		public static ConstructorInfo GetConstructor([NotNull] this Type thisValue, BindingFlags bindingAttributes, params Type[] types)
		{
			return GetConstructor(thisValue, bindingAttributes, null, CallingConventions.Any, types, null);
		}

		public static ConstructorInfo GetConstructor([NotNull] this Type thisValue, BindingFlags bindingAttributes, CallingConventions callConvention, params Type[] types)
		{
			return GetConstructor(thisValue, bindingAttributes, null, callConvention, types, null);
		}

		public static ConstructorInfo GetConstructor([NotNull] this Type thisValue, BindingFlags bindingAttributes, Binder binder, CallingConventions callConvention, params Type[] types)
		{
			return GetConstructor(thisValue, bindingAttributes, binder, callConvention, types, null);
		}

		public static ConstructorInfo GetConstructor([NotNull] this Type thisValue, BindingFlags bindingAttributes, Binder binder, CallingConventions callConvention, Type[] types, params ParameterModifier[] modifiers)
		{
			return thisValue.GetConstructor(bindingAttributes, binder, callConvention, types ?? Type.EmptyTypes, modifiers);
		}

		[ItemNotNull]
		public static IEnumerable<ConstructorInfo> GetConstructors([NotNull] this Type thisValue, [NotNull] Predicate<ConstructorInfo> selector,
			BindingFlags bindingAttributes = BindingFlags.Default)
		{
			Type type = thisValue;
			bool declaredOnly = bindingAttributes.HasFlag(BindingFlags.DeclaredOnly);

			do
			{
				foreach (ConstructorInfo constructor in type.GetConstructors(bindingAttributes))
				{
					if (!selector(constructor)) continue;
					yield return constructor;
				}
			}
			while (!declaredOnly && (type = type.BaseType) != null);
		}

		public static int ConstructorsCount([NotNull] this Type thisValue, BindingFlags bindingAttributes = BindingFlags.Default)
		{
			return GetConstructors(thisValue, p => true, bindingAttributes)?.Count() ?? 0;
		}

		/// <summary>
		/// Obtains a delegate to invoke a constructor which takes a parameter
		/// </summary>
		/// <returns>A delegate to the constructor if found, else null</returns>
		[NotNull]
		public static Delegate Ctor([NotNull] this Type thisValue, params ParameterExpression[] parameters)
		{
			if (parameters == null || parameters.Length == 0)
			{
				ConstructorInfo ci = GetConstructor(thisValue);
				return Expression.Lambda(Expression.New(ci)).Compile();
			}

			Type[] types = parameters.Select(e => e.Type).ToArray();
			ConstructorInfo cip = GetConstructor(thisValue, types);
			return Expression.Lambda(Expression.New(cip, parameters.Cast<Expression>()), parameters).Compile();
		}

		public static MethodInfo FindMethod([NotNull] this Type thisValue, [NotNull] string name, BindingFlags bindingAttributes = BindingFlags.Default, Binder binder = null,
			Type returnType = null, CallingConventions callConvention = CallingConventions.Any, ParameterModifier[] modifiers = null, params Type[] types)
		{
			if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

			Type type = thisValue;
			Type[] paramTypes = types ?? Type.EmptyTypes;
			MethodInfo info = null;
			if (returnType == null && !bindingAttributes.HasFlag(BindingFlags.IgnoreReturn)) bindingAttributes |= BindingFlags.IgnoreReturn;

			bool declaredOnly = bindingAttributes.HasFlag(BindingFlags.DeclaredOnly);

			do
			{
				try
				{
					info = type.GetMethod(name, bindingAttributes, binder, callConvention, paramTypes, modifiers);
					if (info != null && returnType != null && !info.ReturnType.Is(returnType)) info = null;
				}
				catch (Exception e)
				{
					if (__findInterfaceExceptionBreak.Contains(e.GetType())) break;
					// ignored
				}
			}
			while (info == null && !declaredOnly && (type = type.BaseType) != null);

			return info;
		}

		public static MethodInfo GetMethod([NotNull] this Type thisValue, [NotNull] string name, params Type[] types)
		{
			return GetMethod(thisValue, name, BindingFlags.Default, null, CallingConventions.Any, types, null);
		}

		public static MethodInfo GetMethod([NotNull] this Type thisValue, [NotNull] string name, BindingFlags bindingAttributes, params Type[] types)
		{
			return GetMethod(thisValue, name, bindingAttributes, null, CallingConventions.Any, types, null);
		}

		public static MethodInfo GetMethod([NotNull] this Type thisValue, [NotNull] string name, BindingFlags bindingAttributes, CallingConventions callConvention, params Type[] types)
		{
			return GetMethod(thisValue, name, bindingAttributes, null, callConvention, types, null);
		}

		public static MethodInfo GetMethod([NotNull] this Type thisValue, [NotNull] string name, BindingFlags bindingAttributes, Binder binder, CallingConventions callConvention, params Type[] types)
		{
			return GetMethod(thisValue, name, bindingAttributes, binder, callConvention, types, null);
		}

		public static MethodInfo GetMethod([NotNull] this Type thisValue, [NotNull] string name, BindingFlags bindingAttributes, Binder binder, CallingConventions callConvention, Type[] types, params ParameterModifier[] modifiers)
		{
			return thisValue.GetMethod(name, bindingAttributes, binder, callConvention, types ?? Type.EmptyTypes, modifiers);
		}

		[ItemNotNull]
		public static IEnumerable<MethodInfo> GetMethods([NotNull] this Type thisValue, [NotNull] Predicate<MethodInfo> selector,
			BindingFlags bindingAttributes = BindingFlags.Default)
		{
			Type type = thisValue;
			bool declaredOnly = bindingAttributes.HasFlag(BindingFlags.DeclaredOnly);

			do
			{
				foreach (MethodInfo method in type.GetMethods(bindingAttributes))
				{
					if (!selector(method)) continue;
					yield return method;
				}
			}
			while (!declaredOnly && (type = type.BaseType) != null);
		}

		public static MethodInfo GetExplicitInterfaceMethod([NotNull] this Type thisValue, [NotNull] string name)
		{
			//explicit interface implementation
			const BindingFlags BINDING_FLAGS = Constants.BF_NON_PUBLIC_INSTANCE | BindingFlags.DeclaredOnly;

			if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
			if (!thisValue.IsInterface) throw new ArgumentException("Type is not an interface.", nameof(thisValue));

			ConcurrentDictionary<string, MethodInfo> dictionary = __explicitInterfaceCache.GetOrAdd(thisValue, type => new ConcurrentDictionary<string, MethodInfo>(StringComparer.OrdinalIgnoreCase));
			string key = $"{thisValue.FullName}_{name}";
			MethodInfo method = dictionary.GetOrAdd(key, s =>
			{
				string dotName = "." + name;
				return thisValue.GetMethods(BINDING_FLAGS)
								.FirstOrDefault(m => m.IsFinal && (m.Name.IsSame(name) || m.Name.EndsWithOrdinal(dotName)));
			});
			return method;
		}

		public static int MethodsCount([NotNull] this Type thisValue, BindingFlags bindingAttributes = BindingFlags.Default)
		{
			return GetMethods(thisValue, p => true, bindingAttributes)?.Count() ?? 0;
		}

		public static MemberInfo FindMember([NotNull] this Type thisValue, string name, MemberTypes searchType, BindingFlags bindingAttributes = BindingFlags.Default,
			Binder binder = null, Type returnType = null,
			CallingConventions callConvention = CallingConventions.Any, ParameterModifier[] modifiers = null, params Type[] types)
		{
			MemberInfo info = null;

			switch (searchType)
			{
				case MemberTypes.NestedType:
					info = FindNestedType(thisValue, name, bindingAttributes);
					break;
				case MemberTypes.Field:
					info = FindField(thisValue, name, bindingAttributes);
					break;
				case MemberTypes.Event:
					info = FindEvent(thisValue, name, bindingAttributes);
					break;
				case MemberTypes.Property:
					info = FindProperty(thisValue, name, bindingAttributes, binder, returnType, modifiers, types);
					break;
				case MemberTypes.Constructor:
					info = FindConstructor(thisValue, bindingAttributes, binder, callConvention, modifiers, types);
					break;
				case MemberTypes.Method:
					info = FindMethod(thisValue, name, bindingAttributes, binder, returnType, callConvention, modifiers, types);
					break;
				default:
					Type type = thisValue.AsType();
					bool declaredOnly = bindingAttributes.HasFlag(BindingFlags.DeclaredOnly);

					do
					{
						try
						{
							MemberInfo[] members = type.GetMember(name, searchType, bindingAttributes);
							if (members.Length == 0) continue;
							if (members.Length > 1) throw new AmbiguousMatchException();
							info = members[0];
						}
						catch (Exception e)
						{
							if (__findInterfaceExceptionBreak.Contains(e.GetType())) break;
							// ignored
						}
					}
					while (info == null && !declaredOnly && (type = type.BaseType) != null);

					break;
			}

			return info;
		}

		[ItemNotNull]
		public static IEnumerable<MemberInfo> GetMembers([NotNull] this Type thisValue, [NotNull] Predicate<MemberInfo> selector,
			BindingFlags bindingAttributes = BindingFlags.Default, Type returnType = null)
		{
			Type type = thisValue;
			bool useReturnType = returnType != null;
			bool declaredOnly = bindingAttributes.HasFlag(BindingFlags.DeclaredOnly);

			do
			{
				foreach (MemberInfo member in type.GetMembers(bindingAttributes))
				{
					if (useReturnType && member.HasUnderlyingType() && !member.GetUnderlyingType().Is(returnType)) continue;
					if (!selector(member)) continue;
					yield return member;
				}
			}
			while (!declaredOnly && (type = type.BaseType) != null);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsPrimitive([NotNull] this Type thisValue)
		{
			thisValue = ResolveType(thisValue);
			return thisValue != null && (thisValue.IsValueType && (thisValue.IsPrimitive || thisValue.IsEnum) || thisValue.IsAssignableTo(typeof(string)) || IsPointer(thisValue));
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsPrimitiveOrStruct([NotNull] this Type thisValue)
		{
			thisValue = ResolveType(thisValue);
			return thisValue != null && (thisValue.IsValueType || IsPrimitive(thisValue));
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsStruct([NotNull] this Type thisValue) { return thisValue.IsValueType && !thisValue.IsEnum; }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsPointer([NotNull] this Type thisValue) { return thisValue.IsPointer || Is(thisValue, typeof(Pointer)); }

		public static bool HasConversionOperator([NotNull] this Type thisValue, [NotNull] Type to)
		{
			//https://stackoverflow.com/questions/292437/determine-if-a-reflected-type-can-be-cast-to-another-reflected-type
			//Performs very poorly
			UnaryExpression BodyFunction(Expression body) { return Expression.Convert(body, to); }

			ParameterExpression inp = Expression.Parameter(thisValue, "inp");

			try
			{
				// If this succeeds then we can cast 'from' type to 'to' type using implicit coercion
				Expression.Lambda(BodyFunction(inp), inp).Compile();
				return true;
			}
			catch (InvalidOperationException)
			{
				return false;
			}
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsDerivedFrom([NotNull] this Type thisValue, Type type) { return type != null && (thisValue == type || thisValue.IsSubclassOf(type)); }

		public static bool CanCastTo([NotNull] this Type thisValue, Type type)
		{
			// http://www.codeducky.org/dynamically-determining-implicit-explicit-type-conversions-c/

			// explicit conversion always works if to : from OR if 
			// there's an implicit conversion
			// for nullable types, we can simply strip off the nullability and evaluate the underlying types
			thisValue = thisValue.ResolveType();
			type = type.ResolveType();
			if (CanImplicitlyCastTo(thisValue, type)) return true;

			KeyValuePair<Type, Type> key = new KeyValuePair<Type, Type>(thisValue, type);
			return __castCache.GetOrAdd(key, e =>
			{
				if (thisValue.IsValueType)
				{
					try
					{
						Func<bool> expr = AttemptExplicitCast<object, object>;
						return (bool)expr.Method
											.GetGenericMethodDefinition()
											.MakeGenericMethod(thisValue, type)
											.Invoke(null, Array.Empty<object>());
					}
					catch
					{
						// if the code runs in an environment where this message is localized, we could attempt a known failure first and base the regex on it's message
						return false;
					}
				}

				/*
				 * if the from type == null, the dynamic logic above won't be of any help because
				 * either both types are nullable and thus a runtime cast of null => null will
				 * succeed OR we get a runtime failure related to the inability to cast null to
				 * the desired type, which may or may not indicate an actual issue. thus, we do
				 * the work manually
				 */ 
				return CanReferenceTypeExplicitlyCastTo(thisValue, type);
			});
		}

		public static bool CanImplicitlyCastTo([NotNull] this Type thisValue, [NotNull] Type value)
		{
			// not strictly necessary, but speeds things up and avoids polluting the cache
			if (value.IsAssignableFrom(thisValue)) return true;

			KeyValuePair<Type, Type> key = new KeyValuePair<Type, Type>(thisValue, value);
			return __implicitCastCache.GetOrAdd(key, e =>
			{
				try
				{
					// overload of GetMethod() from http://www.codeducky.org/10-utilities-c-developers-should-know-part-two/ 
					// that takes Expression<Action>
					Func<bool> expr = AttemptImplicitCast<object, object>;
					return (bool)expr.Method
									.GetGenericMethodDefinition()
									.MakeGenericMethod(thisValue, value)
									.Invoke(null, new object[0]);
				}
				catch
				{
					return false;
				}
			});
		}

		public static bool CanReferenceTypeExplicitlyCastTo([NotNull] this Type thisValue, [NotNull] Type value)
		{
			if (value.IsInterface && !thisValue.IsSealed || thisValue.IsInterface && !value.IsSealed)
			{
				// any non-sealed type can be cast to any interface since the runtime type MIGHT implement
				// that interface. The reverse is also true; we can cast to any non-sealed type from any interface
				// since the runtime type that implements the interface might be a derived type of to.
				return true;
			}

			// arrays are complex because of array covariance 
			// (see http://msmvps.com/blogs/jon_skeet/archive/2013/06/22/array-covariance-not-just-ugly-but-slow-too.aspx).
			// Thus, we have to allow for things like var x = (IEnumerable<string>)new object[0];
			// and var x = (object[])default(IEnumerable<string>);

			Type arrayType = thisValue.IsArray && !(thisValue.GetElementType()?.IsValueType ?? false)
				? thisValue
				: value.IsArray && !(value.GetElementType()?.IsValueType ?? false)
									? value
									: null;
			if (arrayType != null)
			{
				Type genericInterfaceType = thisValue.IsInterface && thisValue.IsGenericType
					? thisValue
					: value.IsInterface && value.IsGenericType
													? value
													: null;
				if (genericInterfaceType != null)
				{
					return arrayType.GetInterfaces()
						.Any(i => i.IsGenericType
							&& i.GetGenericTypeDefinition() == genericInterfaceType.GetGenericTypeDefinition()
							&& i.GetGenericArguments().Zip(value.GetGenericArguments(), (ia, ta) => ta.IsAssignableFrom(ia) || ia.IsAssignableFrom(ta)).All(b => b));
				}
			}

			// look for conversion operators. Even though we already checked for implicit conversions, we have to look
			// for operators of both types because, for example, if a class defines an implicit conversion to int then it can be explicitly
			// cast to uint
			IEnumerable<MethodInfo> conversionMethods = thisValue.GetMethods(Constants.BF_PUBLIC_STATIC | BindingFlags.FlattenHierarchy)
				.Concat(value.GetMethods(Constants.BF_PUBLIC_STATIC | BindingFlags.FlattenHierarchy))
				.Where(OpFilter);

			if (value.IsPrimitive && typeof(IConvertible).IsAssignableFrom(value))
			{
				// as mentioned in the OpFilter, primitive convertible types (i. e. not IntPtr) get special 
				// treatment in the sense that if you can convert from Foo => int, you can convert
				// from Foo => double as well
				return conversionMethods.Any(m => m.ReturnType.CanCastTo(value));
			}

			return conversionMethods.Any(m => m.ReturnType == value);

			bool OpFilter(MethodInfo m)
			{
				if (m.Name != "op_Explicit" && m.Name != "op_Implicit" || !m.Attributes.HasFlag(MethodAttributes.SpecialName)) return false;

				ParameterInfo[] parameters = m.GetParameters();
				if (parameters.Length != 1) return false;

				// the from argument of the conversion function can be an indirect match to from in
				// either direction. For example, if we have A : B and Foo defines a conversion from B => Foo,
				// then C# allows A to be cast to Foo
				return parameters[0].ParameterType.IsAssignableFrom(thisValue) || thisValue.IsAssignableFrom(parameters[0].ParameterType);
			}
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool Is<T>([NotNull] this Type thisValue) { return Is(thisValue, typeof(T)); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool Is(this Type thisValue, [NotNull] Type type) { return thisValue != null && type.IsAssignableFrom(thisValue); }

		public static bool IsAssignableFrom<T>([NotNull] this Type thisValue, bool declaredOnly = false) { return IsAssignableFrom(thisValue, typeof(T), declaredOnly); }
		public static bool IsAssignableFrom([NotNull] this Type thisValue, [NotNull] Type value, bool declaredOnly = false)
		{
			return IsAssignableTo(value, thisValue, declaredOnly);
		}

		public static bool IsAssignableTo<T>([NotNull] this Type thisValue, bool declaredOnly = false) { return IsAssignableTo(thisValue, typeof(T), declaredOnly); }
		public static bool IsAssignableTo([NotNull] this Type thisValue, [NotNull] Type value, bool declaredOnly = false)
		{
			return IsAssignableToLocal(thisValue, value, declaredOnly);

			static bool IsAssignableToLocal(Type source, Type target, bool decOnly)
			{
				if (target.IsAssignableFrom(source)) return true;
				if (source.IsConstructedGenericType && target == source.GetGenericTypeDefinition()) return true;

				if (source.GetTypeInfo().ImplementedInterfaces.Any(e => e.IsConstructedGenericType && target == e.GetGenericTypeDefinition()))
					return true;

				if (!decOnly) source = source.BaseType;
				return source != null && IsAssignableToLocal(source, target, decOnly);
			}
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsAbstractOf<T>([NotNull] this Type thisValue) { return IsAbstractOf(thisValue, typeof(T)); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsAbstractOf([NotNull] this Type thisValue, [NotNull] Type type) { return thisValue.IsAbstract && thisValue.IsSubclassOf(type); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsConcreteOf<T>([NotNull] this Type thisValue) { return IsConcreteOf(thisValue, typeof(T)); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsConcreteOf([NotNull] this Type thisValue, [NotNull] Type type) { return !thisValue.IsAbstract && thisValue.IsSubclassOf(type); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsDelegate([NotNull] this Type thisValue) { return typeof(Delegate).IsAssignableFrom(thisValue); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool CanClone([NotNull] this Type thisValue) { return thisValue.IsAssignableFrom(typeof(ICloneable)); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsArray([NotNull] this Type thisValue) { return thisValue.IsArray; }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsEnumerable([NotNull] this Type thisValue) { return thisValue.IsAssignableFrom(typeof(IEnumerable)); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsEnumerable<T>([NotNull] this Type thisValue) { return thisValue.IsAssignableFrom(typeof(IEnumerable<T>)); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsCollection([NotNull] this Type thisValue) { return thisValue.IsAssignableFrom(typeof(ICollection)); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsCollection<T>([NotNull] this Type thisValue) { return thisValue.IsAssignableFrom(typeof(ICollection<T>)); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsList([NotNull] this Type thisValue) { return thisValue.IsAssignableFrom(typeof(IList)); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsList<T>([NotNull] this Type thisValue) { return thisValue.IsAssignableFrom(typeof(IList<T>)); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsDictionary([NotNull] this Type thisValue) { return thisValue.IsAssignableFrom(typeof(IDictionary)); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsDictionary<TKey, TValue>([NotNull] this Type thisValue) { return thisValue.IsAssignableFrom(typeof(IDictionary<TKey, TValue>)); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsSet<T>([NotNull] this Type thisValue) { return thisValue.IsAssignableFrom(typeof(ISet<T>)); }

		public static Type GetEnumerableType([NotNull] this Type thisValue, bool inherit = true, int argumentsCount = -1)
		{
			if (argumentsCount < -1) throw new ArgumentOutOfRangeException(nameof(argumentsCount));

			Type type = thisValue;
			Type targetType = typeof(IEnumerable);

			while (type != null && !targetType.IsAssignableFrom(type) && inherit)
			{
				type = type.BaseType;
				if (type == typeof(object)) type = null;
			}

			if (type == null || !targetType.IsAssignableFrom(type)) return null;
			return argumentsCount == -1
						? type
						: type.GetGenericArguments().Length == argumentsCount
							? type
							: null;
		}

		public static Type GetCollectionType([NotNull] this Type thisValue, bool inherit = true)
		{
			Type type = thisValue;
			Type targetType = typeof(ICollection);

			while (type != null && !targetType.IsAssignableFrom(type) && inherit)
			{
				type = type.BaseType;
				if (type == typeof(object)) type = null;
			}

			return type == null || !targetType.IsAssignableFrom(type) ? null : type;
		}

		public static Type GetCollectionType<T>([NotNull] this Type thisValue, bool inherit = true)
		{
			Type type = thisValue;
			Type targetType = typeof(ICollection<T>);

			while (type != null && !targetType.IsAssignableFrom(type) && inherit)
			{
				type = type.BaseType;
				if (type == typeof(object)) type = null;
			}

			return type == null || !targetType.IsAssignableFrom(type) ? null : type;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsIntegral([NotNull] this Type thisValue) { return TypeHelper.IntegralTypes.Contains(thisValue); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsFloater([NotNull] this Type thisValue) { return TypeHelper.FloatingTypes.Contains(thisValue); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsNumeric([NotNull] this Type thisValue) { return TypeHelper.NumericTypes.Contains(thisValue); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsNullable([NotNull] this Type thisValue)
		{
			if (!thisValue.IsGenericType) return false;
			Type typeDefinition = thisValue.GetGenericTypeDefinition();
			return typeDefinition == typeof(Nullable) || typeDefinition == typeof(Nullable<>);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsNullableValueType([NotNull] this Type thisValue)
		{
			if (!thisValue.IsGenericType) return false;
			Type typeDefinition = thisValue.GetGenericTypeDefinition();
			return (typeDefinition == typeof(Nullable) || typeDefinition == typeof(Nullable<>)) && (Nullable.GetUnderlyingType(typeDefinition)?.IsValueType ?? false);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static Type ResolveType(this Type thisValue)
		{
			return thisValue == null || !IsNullable(thisValue)
						? thisValue
						: Nullable.GetUnderlyingType(thisValue);
		}

		public static Type ResolveGenericType([NotNull] this Type thisValue, bool declaredOnly = false) { return ResolveGenericTypeInternal(thisValue, null, declaredOnly); }
		public static Type ResolveGenericType([NotNull] this Type thisValue, [NotNull] Type genericType, bool declaredOnly = false)
		{
			if (!genericType.IsGenericType) throw new ArgumentException("Argument is not a generic type.", nameof(genericType));
			return ResolveGenericTypeInternal(thisValue, genericType, declaredOnly);
		}

		[NotNull]
		public static Array GetValues([NotNull] this Type thisValue)
		{
			if (!thisValue.IsEnum) throw new InvalidEnumArgumentException();
			Array values = Enum.GetValues(thisValue);
			Array.Sort(values);
			return values;
		}

		public static IEnumerable<string> GetNames([NotNull] this Type thisValue)
		{
			Array values = GetValues(thisValue);

			foreach (Enum value in values.OfType<Enum>())
				yield return GetName(thisValue, value);
		}

		public static string GetName([NotNull] this Type thisValue, [NotNull] Enum value)
		{
			if (!thisValue.IsEnum) throw new InvalidEnumArgumentException();
			return Enum.GetName(thisValue, value);
		}

		public static IEnumerable<string> GetDisplayNames([NotNull] this Type thisValue)
		{
			Array values = GetValues(thisValue);

			foreach (object value in values)
				yield return GetDisplayName(thisValue, value);
		}

		public static string GetDisplayName([NotNull] this Type thisValue, [NotNull] object value)
		{
			if (!thisValue.IsEnum) throw new InvalidEnumArgumentException();
			string name = Enum.GetName(thisValue, value);
			return thisValue.GetField(name).GetDisplayName(name);
		}

		[NotNull]
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static Type GetUnderlyingType([NotNull] this Type thisValue)
		{
			if (!thisValue.IsEnum) throw new InvalidEnumArgumentException();
			return Enum.GetUnderlyingType(thisValue);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static TypeCode GetUnderlyingTypeCode([NotNull] this Type thisValue)
		{
			if (!thisValue.IsEnum) throw new InvalidEnumArgumentException();
			return Type.GetTypeCode(Enum.GetUnderlyingType(thisValue));
		}

		public static bool IsDefined([NotNull] this Type thisValue, object value)
		{
			if (!thisValue.IsEnum) throw new InvalidEnumArgumentException();
			return !value.IsNull() &&
					(value is string s
						? EnumHelper.TryParse(thisValue, s, out _)
						: Enum.IsDefined(thisValue, value));
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsDefined([NotNull] this Type thisValue, [NotNull] Enum value)
		{
			if (!thisValue.IsEnum) throw new InvalidEnumArgumentException();
			return Enum.IsDefined(thisValue, value);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsDefined<T>([NotNull] this Type thisValue, T value)
			where T : struct, IComparable
		{
			if (!thisValue.IsEnum) throw new InvalidEnumArgumentException();
			return Enum.IsDefined(thisValue, value);
		}

		/// <summary>
		/// Returns the entry point for a method, or null if no entry points can be used.
		/// An entry point taking string[] is preferred to one with no parameters.
		/// </summary>
		[ItemNotNull]
		public static IEnumerable<MethodBase> GetEntryPoint([NotNull] this Type thisValue, Predicate<MethodBase> filter = null)
		{
			if (thisValue.IsGenericTypeDefinition || thisValue.IsGenericType) yield break;

			// Can't use GetMethod directly as then we can't ignore generic methods :(
			MethodInfo[] methods = thisValue.GetMethods(Constants.BF_PUBLIC_NON_PUBLIC_STATIC | BindingFlags.DeclaredOnly);

			foreach (MethodInfo method in methods
				.Where(m => m.Name == "Main" && !m.IsGenericMethod && !m.IsGenericMethodDefinition))
			{
				if (filter != null && !filter(method)) continue;
				yield return method;
			}
		}

		public static T CreateInstance<T>([NotNull] this Type thisValue, params object[] arguments) { return (T)CreateInstance(thisValue, arguments); }
		public static T CreateInstance<T>([NotNull] this Type thisValue, BindingFlags bindingFlags, params object[] arguments) { return (T)CreateInstance(thisValue, bindingFlags, arguments); }
		public static object CreateInstance([NotNull] this Type thisValue, params object[] arguments) { return CreateInstance(thisValue, Constants.BF_PUBLIC_NON_PUBLIC_INSTANCE, arguments); }
		public static object CreateInstance([NotNull] this Type thisValue, BindingFlags bindingFlags, params object[] arguments)
		{
			string typeName = thisValue.FullName ?? throw new InvalidOperationException("Type name is null.");
			Assembly assembly = thisValue.Assembly;
			if (arguments != null && arguments.Length == 0) arguments = null;

			object value;

			try
			{
				value = assembly.CreateInstance(typeName, false, bindingFlags, null, arguments, null, null);
			}
			catch
			{
				try
				{
					KeyValuePair<ConstructorInfo, ParameterInfo[]> pair = thisValue.GetConstructors()
																			.ToDictionary(k => k, v => v.GetParameters())
																			.Where(e => !e.Value.Any(p => p.IsOut))
																			.OrderBy(e => e.Value.Length)
																			.FirstOrDefault();
					if (pair.Key != null)
					{
						object[] parameters = new object[pair.Value.Length];

						int i = 0;

						if (arguments != null && arguments.Length > 0)
						{
							for (; i < arguments.Length && i < parameters.Length; i++)
							{
								parameters[i] = arguments[i];
							}
						}

						for (; i < parameters.Length; i++)
						{
							ParameterInfo parameter = pair.Value[i];
							parameters[i] = parameter.HasDefaultValue
												? parameter.DefaultValue
												: parameter.ParameterType.Default();
						}

						value = assembly.CreateInstance(typeName, false, bindingFlags, null, parameters, null, null);
					}
					else
					{
						value = null;
					}
				}
				catch
				{
					value = null;
				}
			}

			return value;
		}

		public static string AssemblyPath([NotNull] this Type thisValue) { return thisValue.Assembly.GetDirectoryPath(); }

		/// <summary>
		/// https://codereview.stackexchange.com/questions/1070/generic-advanced-delegate-createdelegate-using-expression-trees
		/// </summary>
		/// <returns></returns>
		[NotNull]
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static MethodInfo GetMethodInfoFromDelegate([NotNull] this Type thisValue)
		{
			if (!thisValue.IsSubclassOf(typeof(MulticastDelegate))) throw new InvalidOperationException("Type is not a subclass of MulticastDelegate.");
			MethodInfo invoke = thisValue.GetMethod("Invoke") ?? throw new InvalidOperationException("Type is not a delegate.");
			return invoke;
		}

		[NotNull]
		public static MethodInfo GetMethodInfoFromEventDelegate([NotNull] this Type thisValue)
		{
			MethodInfo invoke = GetMethodInfoFromDelegate(thisValue);
			if (invoke.ReturnType != typeof(void)) throw new InvalidOperationException("Invalid signature.");

			ParameterInfo[] parameters = invoke.GetParameters();
			if (parameters.Length != 2 || parameters[0].ParameterType != typeof(object) || parameters[1].ParameterType.IsSubclassOf(typeof(EventArgs))) throw new InvalidOperationException("Invalid signature.");
			return invoke;
		}

		public static string Name([NotNull] this Type thisValue, string removeSuffix = null)
		{
			string name = thisValue.Name;
			if (!string.IsNullOrEmpty(removeSuffix)) name = name.RemoveSuffix(removeSuffix);
			return name;
		}

		private static bool AttemptExplicitCast<TFrom, TTo>()
		{
			try
			{
				// based on the IL generated from
				// var x = (TTo)(dynamic)default(TFrom);
				CallSiteBinder binder = CSharpBinder.Convert(CSharpBinderFlags.ConvertExplicit, typeof(TTo), typeof(TypeExtension));
				CallSite<Func<CallSite, TFrom, TTo>> callSite = CallSite<Func<CallSite, TFrom, TTo>>.Create(binder);
				callSite.Target(callSite, default);
				return true;
			}
			catch
			{
				return false;
			}
		}

		private static bool AttemptImplicitCast<TFrom, TTo>()
		{
			try
			{
				// based on the IL produced by:
				// dynamic list = new List<TTo>();
				// list.Add(default(TFrom));
				// We can't use the above code because it will mimic a cast in a generic method
				// which doesn't have the same semantics as a cast in a non-generic method
				List<TTo> list = new List<TTo>(1);
				CallSiteBinder binder = CSharpBinder.InvokeMember(CSharpBinderFlags.ResultDiscarded, "Add", null, typeof(TypeExtension), new[]
				{
				CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
				CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null)
			});
				CallSite<Action<CallSite, object, TFrom>> callSite = CallSite<Action<CallSite, object, TFrom>>.Create(binder);
				callSite.Target.Invoke(callSite, list, default);
				return true;
			}
			catch
			{
				return false;
			}
		}

		private static Type ResolveGenericTypeInternal(Type thisValue, Type genericType, bool declaredOnly)
		{
			Type type = thisValue;
			IList<Type[]> types = null;

			while (type != null)
			{
				if (type.IsGenericType && (genericType == null || genericType.IsAssignableFrom(type)))
				{
					Type[] genericArgs = type.GetGenericArguments();

					switch (genericArgs.Length)
					{
						case 0:
							Type elementType = type.GetElementType();
							if (elementType != null) return elementType;
							continue;
						case 1:
							return genericArgs[0];
						default:
							types ??= new List<Type[]>();
							types.Add(genericArgs);
							break;
					}
				}

				type = declaredOnly ? null : type.BaseType;
			}

			return types != null && types.Count > 0
						? types[0][0]
						: thisValue;
		}
	}
}
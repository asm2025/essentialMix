using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using asm.Collections;
using asm.Json.Abstraction;
using JetBrains.Annotations;

namespace asm.Core.Web.Http.ModelBinding
{
	public static class HyperModelBinderProvider
	{
		public static IReadOnlySet<Type> ExcludedTypes { get; } = new ReadOnlySet<Type>(new HashSet<Type>
								{
									typeof(object)
								});

		public static ConcurrentDictionary<Type, HyperModelBinder> Types { get; } = new ConcurrentDictionary<Type, HyperModelBinder>();
	}

	public class HyperModelBinderProvider<T> : IModelBinderProvider
	{
		/// <inheritdoc />
		public HyperModelBinderProvider([NotNull] IJsonSerializer serializer)
		{
			Type type = typeof(T);

			while (type != null && !HyperModelBinderProvider.ExcludedTypes.Contains(type))
			{
				HyperModelBinderProvider.Types.TryAdd(type, new HyperModelBinder(type, serializer));
				type = type.BaseType;
			}
		}

		public IModelBinder GetBinder([NotNull] ModelBinderProviderContext context)
		{
			HyperModelBinderProvider.Types.TryGetValue(context.Metadata.ModelType, out HyperModelBinder binder);
			return binder;
		}
	}
}
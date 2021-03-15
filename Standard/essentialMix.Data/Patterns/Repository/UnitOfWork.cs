using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using essentialMix.Patterns.Object;
using JetBrains.Annotations;

namespace essentialMix.Data.Patterns.Repository
{
	public class UnitOfWork : Disposable, IUnitOfWork
	{
		private static readonly Type __targetType = typeof(IRepositoryBase);
		
		private readonly ConcurrentDictionary<Type, Func<IRepositoryBase>> _templates = new ConcurrentDictionary<Type, Func<IRepositoryBase>>();
		private readonly ConcurrentDictionary<Type, IRepositoryBase> _instances = new ConcurrentDictionary<Type, IRepositoryBase>();

		/// <inheritdoc />
		public UnitOfWork()
		{
		}

		/// <inheritdoc />
		public void Register<T>(Expression<Func<T>> template)
			where T : IRepositoryBase
		{
			Expression castExpr = Expression.Convert(template.Body, __targetType);
			Register(typeof(T), Expression.Lambda<Func<IRepositoryBase>>(castExpr));
		}

		/// <inheritdoc />
		public void Register(Type type, Expression<Func<IRepositoryBase>> template)
		{
			if (!__targetType.IsAssignableFrom(type)) throw new ArgumentException($"{type} does not implement {nameof(IRepositoryBase)} interface.", nameof(type));
			_templates.TryAdd(type, template.Compile());
		}

		/// <inheritdoc />
		public void Unregister<T>()
			where T : IRepositoryBase
		{
			Unregister(typeof(T));
		}

		/// <inheritdoc />
		public void Unregister(Type type)
		{
			_templates.TryRemove(type, out _);
			_instances.TryRemove(type, out _);
		}

		/// <inheritdoc />
		public void ClearRegistration()
		{
			_templates.Clear();
		}
	
		/// <inheritdoc />
		[NotNull]
		public TRepository GetOrCreate<TRepository>()
			where TRepository : IRepositoryBase
		{
			return (TRepository)GetOrCreate(typeof(TRepository));
		}

		/// <inheritdoc />
		[NotNull]
		public IRepositoryBase GetOrCreate(Type type)
		{
			return (_instances.TryGetValue(type, out IRepositoryBase instance)
						? instance
						: !_templates.TryGetValue(type, out Func<IRepositoryBase> template)
							? null
							: _instances.GetOrAdd(type, t => template())) ?? throw new InvalidOperationException("Type is not registered or created.");
		}

		/// <inheritdoc />
		public void Clear()
		{
			_instances.Clear();
		}
	}
}
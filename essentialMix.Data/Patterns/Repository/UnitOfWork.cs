using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using essentialMix.Patterns.Object;
using JetBrains.Annotations;

namespace essentialMix.Data.Patterns.Repository;

public class UnitOfWork : Disposable, IUnitOfWork
{
	private readonly IDictionary<Type, Func<IRepositoryBase>> _templates = new Dictionary<Type, Func<IRepositoryBase>>();
	private readonly IDictionary<Type, IRepositoryBase> _instances = new Dictionary<Type, IRepositoryBase>();

	/// <inheritdoc />
	public UnitOfWork()
	{
	}

	/// <inheritdoc />
	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			lock (_templates)
			{
				_templates.Clear();
				Clear();
			}
		}

		base.Dispose(disposing);
	}

	/// <inheritdoc />
	public void Register<T>(Expression<Func<T>> template)
		where T : IRepositoryBase
	{
		Expression castExpr = Expression.Convert(template.Body, typeof(IRepositoryBase));
		Register(typeof(T), Expression.Lambda<Func<IRepositoryBase>>(castExpr));
	}

	/// <inheritdoc />
	public void Register(Type type, Expression<Func<IRepositoryBase>> template)
	{
		if (!typeof(IRepositoryBase).IsAssignableFrom(type)) throw new ArgumentException($"{type} does not implement {nameof(IRepositoryBase)} interface.", nameof(type));

		lock (_templates)
		{
			if (_templates.ContainsKey(type)) return;
			_templates.Add(type, template.Compile());
		}
	}

	/// <inheritdoc />
	public void Deregister<T>()
		where T : IRepositoryBase
	{
		Deregister(typeof(T));
	}

	/// <inheritdoc />
	public void Deregister(Type type)
	{
		lock (_templates)
		{
			if (!_templates.ContainsKey(type)) return;
			_templates.Remove(type);
		}
	}

	/// <inheritdoc />
	public void ClearRegistration()
	{
		lock (_templates)
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
		lock (_instances)
		{
			if (_instances.TryGetValue(type, out IRepositoryBase repository)) return repository;

			lock (_templates)
			{
				if (!_templates.TryGetValue(type, out Func<IRepositoryBase> template)) throw new InvalidOperationException("Type is not registered or created.");
				repository = template();
				_instances.Add(type, repository);
				return repository;
			}
		}
	}

	/// <inheritdoc />
	public void Clear()
	{
		lock (_instances)
		{
			foreach (IRepositoryBase repository in _instances.Values)
				repository?.Dispose();

			_instances.Clear();
		}
	}
}
using System;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace essentialMix.Data.Patterns.Repository
{
	public interface IUnitOfWork : IRepositoryBase
	{
		void Register<T>([NotNull] Expression<Func<T>> template) where T : IRepositoryBase;
		void Register([NotNull] Type type, [NotNull] Expression<Func<IRepositoryBase>> template);
		void Unregister<T>() where T : IRepositoryBase;
		void Unregister([NotNull] Type type);
		void ClearRegistration();
		TRepository GetOrCreate<TRepository>() where TRepository : IRepositoryBase;
		IRepositoryBase GetOrCreate([NotNull] Type type);
		void Clear();
	}
}
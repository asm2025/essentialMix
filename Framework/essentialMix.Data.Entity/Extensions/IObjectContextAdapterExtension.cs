using System;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Linq;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions
{
	public static class IObjectContextAdapterExtension
	{
		public static string GetTableName<T>([NotNull] this IObjectContextAdapter thisValue)
			where T : class
		{
			return GetTableName(thisValue, typeof(T));
		}

		public static string GetTableName([NotNull] this IObjectContextAdapter thisValue, Type type)
		{
			// based on https://romiller.com/2014/04/08/ef6-1-mapping-between-types-tables/
			if (type == null) return null;

			MetadataWorkspace metadata = thisValue.ObjectContext.MetadataWorkspace;
			ObjectItemCollection objectItemCollection = (ObjectItemCollection)metadata.GetItemCollection(DataSpace.OSpace);

			// Get the entity type from the model that maps to the CLR type
			EntityType entityType = metadata
									.GetItems<EntityType>(DataSpace.OSpace)
									.Single(e => objectItemCollection.GetClrType(e) == type);

			// Get the entity set that uses this entity type
			EntitySet entitySet = metadata
								.GetItems<EntityContainer>(DataSpace.CSpace)
								.Single()
								.EntitySets
								.Single(s => s.ElementType.Name == entityType.Name);

			// Find the mapping between conceptual and storage model for this entity set
			EntitySetMapping mapping = metadata.GetItems<EntityContainerMapping>(DataSpace.CSSpace)
												.Single()
												.EntitySetMappings
												.Single(s => s.EntitySet == entitySet);

			// Find the storage entity set (table) that the entity is mapped
			EntitySet table = mapping
							.EntityTypeMappings.Single()
							.Fragments.Single()
							.StoreEntitySet;

			// Return the table name from the storage entity set
			return (string)table.MetadataProperties["Table"].Value ?? table.Name;
		}
	}
}
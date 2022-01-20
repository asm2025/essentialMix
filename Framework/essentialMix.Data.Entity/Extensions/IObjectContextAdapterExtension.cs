using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Linq;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions;

public static class IObjectContextAdapterExtension
{
	[NotNull]
	public static EntityType GetEntityType([NotNull] this IObjectContextAdapter thisValue, [NotNull] Type type)
	{
		// based on https://romiller.com/2014/04/08/ef6-1-mapping-between-types-tables/
		MetadataWorkspace metadata = thisValue.ObjectContext.MetadataWorkspace;
		ObjectItemCollection objectItemCollection = (ObjectItemCollection)metadata.GetItemCollection(DataSpace.OSpace);
		// Get the entity type from the model that maps to the CLR type
		return metadata
								.GetItems<EntityType>(DataSpace.OSpace)
								.Single(e => objectItemCollection.GetClrType(e) == type);
	}

	public static string GetTableName([NotNull] this IObjectContextAdapter thisValue, [NotNull] Type type)
	{
		MetadataWorkspace metadata = thisValue.ObjectContext.MetadataWorkspace;
		EntityType entityType = GetEntityType(thisValue, type);
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
		EntitySet table = mapping
						.EntityTypeMappings.First()
						.Fragments.First()
						.StoreEntitySet;
		return (string)table.MetadataProperties["Table"].Value ?? table.Name;
	}

	[NotNull]
	public static IEnumerable<string> GetKeyNames([NotNull] this IObjectContextAdapter thisValue, [NotNull] Type type)
	{
		EntityType entityType = GetEntityType(thisValue, type);
		return entityType.KeyProperties.Select(e => e.Name);
	}
}
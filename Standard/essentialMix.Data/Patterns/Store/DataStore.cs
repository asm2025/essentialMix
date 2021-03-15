using System;
using System.Data;
using System.Runtime.Serialization;
using essentialMix.Extensions;
using JetBrains.Annotations;
using essentialMix.Data.Patterns.Provider;

namespace essentialMix.Data.Patterns.Store
{
	[Serializable]
	public class DataStore<TProvider, TDbType> : DataSet
		where TDbType : struct, IComparable
		where TProvider : IDataProvider<TDbType>
	{
		public DataStore([NotNull] TProvider provider)
		{
			Provider = provider;
		}

		public DataStore([NotNull] TProvider provider, string dataSetName) 
			: base(dataSetName)
		{
			Provider = provider;
		}

		protected DataStore([NotNull] SerializationInfo info, StreamingContext context) 
			: base(info, context)
		{
			TProvider p = (TProvider)info.GetValue("Provider", typeof(TProvider));
			Provider = p;
		}

		protected DataStore([NotNull] SerializationInfo info, StreamingContext context, bool constructSchema) 
			: base(info, context, constructSchema)
		{
			TProvider p = (TProvider)info.GetValue("Provider", typeof(TProvider));
			Provider = p;
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("Provider", Provider, typeof(TProvider));
		}

		[NotNull]
		public TProvider Provider { get; set; }

		public void CopyTo([NotNull] DataSet dataSet, bool overwrite = false)
		{
			((DataSet)this).CopyTo(dataSet, overwrite);
		}

		public void CopyFrom([NotNull] DataSet dataSet, bool overwrite = false)
		{
			((DataSet)this).CopyFrom(dataSet, overwrite);
		}

		public static DataStore<TProvider, TDbType> FromDataSet(DataSet dataSet, [NotNull] TProvider provider)
		{
			if (dataSet == null) return null;
			DataStore<TProvider, TDbType> dataStore = (DataStore<TProvider, TDbType>)Activator.CreateInstance(typeof(DataStore<TProvider, TDbType>), provider);
			dataStore.CopyFrom(dataStore);
			return dataStore;
		}
	}
}
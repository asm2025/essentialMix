using System;
using System.Collections.Generic;
using System.Linq;
using asm.Patterns.Future;
using JetBrains.Annotations;

namespace asm.Patterns.Producer
{
	public static class IEnumerableExtension
	{
		/// <summary>
		/// Groups and executes a pipeline for a single result per group
		/// </summary>
		[NotNull]
		public static IEnumerable<(TKey Key, TResult Result)> GroupWithPipeline<TElement, TKey, TResult>([NotNull] this IEnumerable<TElement> thisValue, [NotNull] Func<TElement, TKey> keySelector, [NotNull] Func<IDataProducer<TElement>, IFuture<TResult>> pipeline)
		{
			return GroupWithPipeline(thisValue, keySelector, EqualityComparer<TKey>.Default, pipeline);
		}

		/// <summary>
		/// Groups and executes a pipeline for a single result per group
		/// </summary>
		[NotNull]
		public static IEnumerable<(TKey Key, TResult Result)> GroupWithPipeline<TElement, TKey, TResult>([NotNull] this IEnumerable<TElement> thisValue, [NotNull] Func<TElement, TKey> keySelector, IEqualityComparer<TKey> comparer, [NotNull] Func<IDataProducer<TElement>, IFuture<TResult>> pipeline)
		{
			IDictionary<TKey, DataProducer<TElement>> keyMap = new Dictionary<TKey, DataProducer<TElement>>(comparer ?? EqualityComparer<TKey>.Default);
			IList<(TKey Key, IFuture<TResult> Result)> results = new List<(TKey, IFuture<TResult>)>();

			foreach (TElement element in thisValue)
			{
				TKey key = keySelector(element);

				if (!keyMap.TryGetValue(key, out DataProducer<TElement> producer))
				{
					producer = new DataProducer<TElement>();
					keyMap[key] = producer;
					results.Add((key, pipeline(producer)));
				}
				producer.Produce(element);
			}

			foreach (DataProducer<TElement> producer in keyMap.Values)
				producer.Complete();

			return results.Select(e => (e.Key, e.Result.Value));
		}

		/// <summary>
		/// Groups and executes a pipeline for two results per group
		/// </summary>
		[NotNull]
		public static IEnumerable<(TKey Key, TResult1 Result1, TResult2 Result2)> GroupWithPipeline<TElement, TKey, TResult1, TResult2>([NotNull] this IEnumerable<TElement> thisValue, [NotNull] Func<TElement, TKey> keySelector,
			[NotNull] Func<IDataProducer<TElement>, IFuture<TResult1>> pipeline1, [NotNull] Func<IDataProducer<TElement>, IFuture<TResult2>> pipeline2)
		{
			return GroupWithPipeline(thisValue, keySelector, EqualityComparer<TKey>.Default, pipeline1, pipeline2);
		}

		/// <summary>
		/// Groups and executes a pipeline for two results per group
		/// </summary>
		[NotNull]
		public static IEnumerable<(TKey Key, TResult1 Result1, TResult2 Result2)> GroupWithPipeline<TElement, TKey, TResult1, TResult2>([NotNull] this IEnumerable<TElement> thisValue, [NotNull] Func<TElement, TKey> keySelector,
			IEqualityComparer<TKey> comparer, [NotNull] Func<IDataProducer<TElement>, IFuture<TResult1>> pipeline1, [NotNull] Func<IDataProducer<TElement>, IFuture<TResult2>> pipeline2)
		{
			IDictionary<TKey, DataProducer<TElement>> keyMap = new Dictionary<TKey, DataProducer<TElement>>(comparer ?? EqualityComparer<TKey>.Default);
			IList<(TKey Key, IFuture<TResult1> Result1, IFuture<TResult2> Result2)> results = new List<(TKey, IFuture<TResult1>, IFuture<TResult2>)>();

			foreach (TElement element in thisValue)
			{
				TKey key = keySelector(element);

				if (!keyMap.TryGetValue(key, out DataProducer<TElement> producer))
				{
					producer = new DataProducer<TElement>();
					keyMap[key] = producer;
					results.Add((key, pipeline1(producer), pipeline2(producer)));
				}
				producer.Produce(element);
			}

			foreach (DataProducer<TElement> producer in keyMap.Values)
				producer.Complete();

			return results.Select(e => (e.Key, e.Result1.Value, e.Result2.Value));
		}

		/// <summary>
		/// Groups and executes a pipeline for three results per group
		/// </summary>
		[NotNull]
		public static IEnumerable<(TKey Key, TResult1 Result1, TResult2 Result2, TResult3 Result3)> GroupWithPipeline<TElement, TKey, TResult1, TResult2, TResult3>([NotNull] this IEnumerable<TElement> thisValue, [NotNull] Func<TElement, TKey> keySelector,
			[NotNull] Func<IDataProducer<TElement>, IFuture<TResult1>> pipeline1, [NotNull] Func<IDataProducer<TElement>, IFuture<TResult2>> pipeline2, [NotNull] Func<IDataProducer<TElement>, IFuture<TResult3>> pipeline3)
		{
			return GroupWithPipeline(thisValue, keySelector, EqualityComparer<TKey>.Default, pipeline1, pipeline2, pipeline3);
		}

		/// <summary>
		/// Groups and executes a pipeline for three results per group
		/// </summary>
		[NotNull]
		public static IEnumerable<(TKey Key, TResult1 Result1, TResult2 Result2, TResult3 Result3)> GroupWithPipeline<TElement, TKey, TResult1, TResult2, TResult3>([NotNull] this IEnumerable<TElement> thisValue, [NotNull] Func<TElement, TKey> keySelector,
			IEqualityComparer<TKey> comparer, [NotNull] Func<IDataProducer<TElement>, IFuture<TResult1>> pipeline1, [NotNull] Func<IDataProducer<TElement>, IFuture<TResult2>> pipeline2, [NotNull] Func<IDataProducer<TElement>, IFuture<TResult3>> pipeline3)
		{
			IDictionary<TKey, DataProducer<TElement>> keyMap = new Dictionary<TKey, DataProducer<TElement>>(comparer ?? EqualityComparer<TKey>.Default);
			IList<(TKey Key, IFuture<TResult1> Result1, IFuture<TResult2> Result2, IFuture<TResult3> Result3)> results = new List<(TKey, IFuture<TResult1>, IFuture<TResult2>, IFuture<TResult3>)>();

			foreach (TElement element in thisValue)
			{
				TKey key = keySelector(element);
				if (!keyMap.TryGetValue(key, out DataProducer<TElement> producer))
				{
					producer = new DataProducer<TElement>();
					keyMap[key] = producer;
					results.Add((key, pipeline1(producer), pipeline2(producer), pipeline3(producer)));
				}
				producer.Produce(element);
			}

			foreach (DataProducer<TElement> producer in keyMap.Values)
				producer.Complete();

			return results.Select(e => (e.Key, e.Result1.Value, e.Result2.Value, e.Result3.Value));
		}

		/// <summary>
		/// Groups and executes a pipeline for four results per group
		/// </summary>
		[NotNull]
		public static IEnumerable<(TKey Key, TResult1 Result1, TResult2 Result2, TResult3 Result3, TResult4 Result4)> GroupWithPipeline<TElement, TKey, TResult1, TResult2, TResult3, TResult4>([NotNull] this IEnumerable<TElement> thisValue, [NotNull] Func<TElement, TKey> keySelector,
			[NotNull] Func<IDataProducer<TElement>, IFuture<TResult1>> pipeline1, [NotNull] Func<IDataProducer<TElement>, IFuture<TResult2>> pipeline2, [NotNull] Func<IDataProducer<TElement>, IFuture<TResult3>> pipeline3, [NotNull] Func<IDataProducer<TElement>, IFuture<TResult4>> pipeline4)
		{
			return GroupWithPipeline(thisValue, keySelector, EqualityComparer<TKey>.Default, pipeline1, pipeline2, pipeline3, pipeline4);
		}

		/// <summary>
		/// Groups and executes a pipeline for four results per group
		/// </summary>
		[NotNull]
		public static IEnumerable<(TKey Key, TResult1 Result1, TResult2 Result2, TResult3 Result3, TResult4 Result4)> GroupWithPipeline<TElement, TKey, TResult1, TResult2, TResult3, TResult4>([NotNull] this IEnumerable<TElement> thisValue, [NotNull] Func<TElement, TKey> keySelector,
			IEqualityComparer<TKey> comparer, [NotNull] Func<IDataProducer<TElement>, IFuture<TResult1>> pipeline1, [NotNull] Func<IDataProducer<TElement>, IFuture<TResult2>> pipeline2, [NotNull] Func<IDataProducer<TElement>, IFuture<TResult3>> pipeline3, [NotNull] Func<IDataProducer<TElement>, IFuture<TResult4>> pipeline4)
		{
			IDictionary<TKey, DataProducer<TElement>> keyMap = new Dictionary<TKey, DataProducer<TElement>>(comparer ?? EqualityComparer<TKey>.Default);
			IList<(TKey Key, IFuture<TResult1> Result1, IFuture<TResult2> Result2, IFuture<TResult3> Result3, IFuture<TResult4> Result4)> results = new List<(TKey, IFuture<TResult1>, IFuture<TResult2>, IFuture<TResult3>, IFuture<TResult4>)>();

			foreach (TElement element in thisValue)
			{
				TKey key = keySelector(element);

				if (!keyMap.TryGetValue(key, out DataProducer<TElement> producer))
				{
					producer = new DataProducer<TElement>();
					keyMap[key] = producer;
					results.Add((key, pipeline1(producer), pipeline2(producer), pipeline3(producer), pipeline4(producer)));
				}
				producer.Produce(element);
			}

			foreach (DataProducer<TElement> producer in keyMap.Values)
				producer.Complete();

			return results.Select(e => (e.Key, e.Result1.Value, e.Result2.Value, e.Result3.Value, e.Result4.Value));
		}

		/// <summary>
		/// Groups and executes a pipeline for five results per group
		/// </summary>
		[NotNull]
		public static IEnumerable<(TKey Key, TResult1 Result1, TResult2 Result2, TResult3 Result3, TResult4 Result4, TResult5 Result5)> GroupWithPipeline<TElement, TKey, TResult1, TResult2, TResult3, TResult4, TResult5>([NotNull] this IEnumerable<TElement> thisValue, [NotNull] Func<TElement, TKey> keySelector,
			[NotNull] Func<IDataProducer<TElement>, IFuture<TResult1>> pipeline1, [NotNull] Func<IDataProducer<TElement>, IFuture<TResult2>> pipeline2, [NotNull] Func<IDataProducer<TElement>, IFuture<TResult3>> pipeline3, [NotNull] Func<IDataProducer<TElement>, IFuture<TResult4>> pipeline4, [NotNull] Func<IDataProducer<TElement>, IFuture<TResult5>> pipeline5)
		{
			return GroupWithPipeline(thisValue, keySelector, EqualityComparer<TKey>.Default, pipeline1, pipeline2, pipeline3, pipeline4, pipeline5);
		}

		/// <summary>
		/// Groups and executes a pipeline for five results per group
		/// </summary>
		[NotNull]
		public static IEnumerable<(TKey Key, TResult1 Result1, TResult2 Result2, TResult3 Result3, TResult4 Result4, TResult5 Result5)> GroupWithPipeline<TElement, TKey, TResult1, TResult2, TResult3, TResult4, TResult5>([NotNull] this IEnumerable<TElement> thisValue, [NotNull] Func<TElement, TKey> keySelector,
			IEqualityComparer<TKey> comparer, [NotNull] Func<IDataProducer<TElement>, IFuture<TResult1>> pipeline1, [NotNull] Func<IDataProducer<TElement>, IFuture<TResult2>> pipeline2, [NotNull] Func<IDataProducer<TElement>, IFuture<TResult3>> pipeline3, [NotNull] Func<IDataProducer<TElement>, IFuture<TResult4>> pipeline4, [NotNull] Func<IDataProducer<TElement>, IFuture<TResult5>> pipeline5)
		{
			IDictionary<TKey, DataProducer<TElement>> keyMap = new Dictionary<TKey, DataProducer<TElement>>(comparer ?? EqualityComparer<TKey>.Default);
			IList<(TKey Key, IFuture<TResult1> Result1, IFuture<TResult2> Result2, IFuture<TResult3> Result3, IFuture<TResult4> Result4, IFuture<TResult5> Result5)> results = new List<(TKey, IFuture<TResult1>, IFuture<TResult2>, IFuture<TResult3>, IFuture<TResult4>, IFuture<TResult5>)>();

			foreach (TElement element in thisValue)
			{
				TKey key = keySelector(element);

				if (!keyMap.TryGetValue(key, out DataProducer<TElement> producer))
				{
					producer = new DataProducer<TElement>();
					keyMap[key] = producer;
					results.Add((key, pipeline1(producer), pipeline2(producer), pipeline3(producer), pipeline4(producer), pipeline5(producer)));
				}
				producer.Produce(element);
			}

			foreach (DataProducer<TElement> producer in keyMap.Values)
				producer.Complete();

			return results.Select(e => (e.Key, e.Result1.Value, e.Result2.Value, e.Result3.Value, e.Result4.Value, e.Result5.Value));
		}
	}
}
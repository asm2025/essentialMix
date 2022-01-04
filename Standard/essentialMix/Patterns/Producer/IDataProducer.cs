using System;
using System.Collections.Generic;
using System.Linq;
using essentialMix.Extensions;
using essentialMix.Helpers;
using Other.JonSkeet.MiscUtil.Collections;
using Other.MarcGravell;
using essentialMix.Patterns.Future;
using JetBrains.Annotations;

namespace essentialMix.Patterns.Producer;

/// <summary>
/// Interface to be implemented by sequences of data which have a "push"
/// nature rather than "pull" - instead of the IEnumerable model of
/// the client pulling data from the sequence, here the client registers
/// an interest in the data being produced, and in the sequence reaching
/// an end. The data producer than produces data whenever it wishes, and the
/// clients can react. This allows other actions to occur between items being
/// pulled, as well as multiple clients for the same sequence of data.
/// </summary>
public interface IDataProducer<T>
{
	/// <summary>
	/// Event which is raised when an item of data is produced.
	/// This will not be raised after Completed has been raised.
	/// The parameter for the event is the 
	/// </summary>
	event EventHandler<T> Data;
	/// <summary>
	/// Event which is raised when the sequence has finished being
	/// produced. This will be raised exactly once, and after all
	/// Data events (if any) have been raised.
	/// </summary>
	event EventHandler Completed;
}
	
public static class IDataProducerExtension
{
	/// <summary>
	/// Converts an IDataProducer into an IFuture[IEnumerable]. The results
	/// are buffered in memory (as a list), so be warned that this loses the "streaming"
	/// nature of most of the IDataProducer extension methods. The "future" nature of
	/// the result ensures that all results are produced before the enumeration can take
	/// place.
	/// </summary>
	/// <remarks>This will force all values to be buffered</remarks>
	[NotNull]
	public static IFuture<IEnumerable<TSource>> AsFutureEnumerable<TSource>([NotNull] this IDataProducer<TSource> thisValue)
	{
		Future<IEnumerable<TSource>> ret = new Future<IEnumerable<TSource>>();
		List<TSource> list = new List<TSource>();
		thisValue.Data += (_, value) => list.Add(value);
		thisValue.Completed += (_, _) => ret.Value = list;
		return ret;
	}

	/// <summary>
	/// Converts an IDataProducer into an IEnumerable. The results
	/// are buffered in memory (as a list), so be warned that this loses the "streaming"
	/// nature of most of the IDataProducer extension methods. The list is returned
	/// immediately, but further data productions add to it. You must therefore be careful
	/// when the list is used - it is a good idea to only use it after all data has been
	/// produced.
	/// </summary>
	/// <remarks>This will force all values to be buffered</remarks>
	[NotNull]
	public static IEnumerable<TSource> AsEnumerable<TSource>([NotNull] this IDataProducer<TSource> thisValue) { return thisValue.ToList(); }

	/// <summary>
	/// Converts an IDataProducer into a list. An empty list is returned immediately,
	/// and any results produced are added to it.
	/// </summary>
	/// <remarks>This will force all values to be buffered</remarks>
	[NotNull]
	public static List<TSource> ToList<TSource>([NotNull] this IDataProducer<TSource> thisValue)
	{
		List<TSource> list = new List<TSource>();
		thisValue.Data += (_, value) => list.Add(value);
		return list;
	}

	/// <summary>
	/// Converts an IDataProducer into a future array.
	/// </summary>
	/// <remarks>This will force all values to be buffered</remarks>
	[NotNull]
	public static IFuture<TSource[]> ToFutureArray<TSource>([NotNull] this IDataProducer<TSource> thisValue)
	{
		Future<TSource[]> ret = new Future<TSource[]>();
		List<TSource> list = thisValue.ToList();
		thisValue.Completed += (_, _) => ret.Value = list.ToArray();
		return ret;
	}

	[NotNull]
	public static ILookup<TKey, TSource> ToLookup<TSource, TKey>([NotNull] this IDataProducer<TSource> thisValue, [NotNull] Func<TSource, TKey> keySelector)
	{ return ToLookup(thisValue, keySelector, t => t, EqualityComparer<TKey>.Default); }
	[NotNull]
	public static ILookup<TKey, TSource> ToLookup<TSource, TKey>([NotNull] this IDataProducer<TSource> thisValue, [NotNull] Func<TSource, TKey> keySelector, IEqualityComparer<TKey> keyComparer)
	{ return ToLookup(thisValue, keySelector, t => t, keyComparer); }
	[NotNull]
	public static ILookup<TKey, TElement> ToLookup<TSource, TKey, TElement>([NotNull] this IDataProducer<TSource> thisValue, [NotNull] Func<TSource, TKey> keySelector, [NotNull] Func<TSource, TElement> elementSelector)
	{
		return ToLookup(thisValue, keySelector, elementSelector, EqualityComparer<TKey>.Default);
	}

	/// <summary>
	/// Converts an IDataProducer into a lookup.
	/// </summary>
	/// <param name="elementSelector">A transform function to produce a result element value from each element.</param>
	/// <param name="keySelector">A function to extract a key from each element.</param>
	/// <param name="keyComparer">Used to compare keys.</param>
	/// <param name="thisValue">The data source.</param>
	/// <remarks>This will force all values to be buffered</remarks>
	[NotNull]
	public static ILookup<TKey, TElement> ToLookup<TSource, TKey, TElement>([NotNull] this IDataProducer<TSource> thisValue, [NotNull] Func<TSource, TKey> keySelector, [NotNull] Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> keyComparer)
	{
		keyComparer ??= EqualityComparer<TKey>.Default;
		Collections.Lookup<TKey, TElement> lookup = new Collections.Lookup<TKey, TElement>(keyComparer);
		thisValue.Data += (_, value) => lookup.Add(keySelector(value), elementSelector(value));
		thisValue.Completed += (_, _) => lookup.TrimExcess();
		return lookup;
	}

	[NotNull]
	public static IDictionary<TKey, TSource> ToDictionary<TSource, TKey>([NotNull] this IDataProducer<TSource> thisValue, [NotNull] Func<TSource, TKey> keySelector)
	{
		return ToDictionary(thisValue, keySelector, t => t, EqualityComparer<TKey>.Default);
	}

	[NotNull]
	public static IDictionary<TKey, TSource> ToDictionary<TSource, TKey>([NotNull] this IDataProducer<TSource> thisValue, [NotNull] Func<TSource, TKey> keySelector,
		[NotNull] IEqualityComparer<TKey> keyComparer)
	{
		return ToDictionary(thisValue, keySelector, t => t, keyComparer);
	}
	[NotNull]
	public static IDictionary<TKey, TElement> ToDictionary<TSource, TKey, TElement>([NotNull] this IDataProducer<TSource> thisValue, [NotNull] Func<TSource, TKey> keySelector, [NotNull] Func<TSource, TElement> elementSelector)
	{
		return ToDictionary(thisValue, keySelector, elementSelector, EqualityComparer<TKey>.Default);
	}

	/// <summary>
	/// Converts an IDataProducer into a dictionary.
	/// </summary>
	/// <param name="elementSelector">A transform function to produce a result element value from each element.</param>
	/// <param name="keySelector">A function to extract a key from each element.</param>
	/// <param name="keyComparer">Used to compare keys.</param>
	/// <param name="thisValue">The data source.</param>
	/// <remarks>This will force all values to be buffered</remarks>
	[NotNull]
	public static IDictionary<TKey, TElement> ToDictionary<TSource, TKey, TElement>([NotNull] this IDataProducer<TSource> thisValue, [NotNull] Func<TSource, TKey> keySelector, [NotNull] Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> keyComparer)
	{
		keyComparer ??= EqualityComparer<TKey>.Default;

		Dictionary<TKey, TElement> dict = new Dictionary<TKey, TElement>(keyComparer);
		thisValue.Data += (_, value) => dict.Add(keySelector(value), elementSelector(value));
		return dict;
	}

	/// <summary>
	/// Groups the elements of a sequence according to a specified key selector function
	/// and creates a result value from each group and its key.
	/// </summary>
	/// <typeparam name="TKey">The return-type of the transform used to group the sequence</typeparam>
	/// <typeparam name="TSource">The values to be yielded by the original data-source</typeparam>
	/// <param name="keySelector">A function to extract the key for each element in hte original sequence.</param>
	/// <param name="thisValue">The data-source to be grouped</param>
	/// <remarks>This will force each unique grouping key to
	/// be buffered, but not the data itself</remarks>
	[NotNull]
	public static IDataProducer<IProducerGrouping<TKey, TSource>> GroupBy<TSource, TKey>([NotNull] this IDataProducer<TSource> thisValue, [NotNull] Func<TSource, TKey> keySelector)
	{
		return thisValue.GroupBy(keySelector, elt => elt, (key, elements) => (IProducerGrouping<TKey, TSource>)new ProducerGrouping<TKey, TSource>(key, elements), EqualityComparer<TKey>.Default);
	}
	/// <summary>
	/// Groups the elements of a sequence according to a specified key selector function
	/// and creates a result value from each group and its key.
	/// </summary>
	/// <typeparam name="TKey">The return-type of the transform used to group the sequence</typeparam>
	/// <typeparam name="TSource">The values to be yielded by the original data-source</typeparam>
	/// <param name="comparer">Used to compare grouping keys</param>
	/// <param name="keySelector">A function to extract the key for each element in hte original sequence.</param>
	/// <param name="thisValue">The data-source to be grouped</param>
	/// <remarks>This will force each unique grouping key to
	/// be buffered, but not the data itself</remarks>
	[NotNull]
	public static IDataProducer<IProducerGrouping<TKey, TSource>> GroupBy<TSource, TKey>([NotNull] this IDataProducer<TSource> thisValue, [NotNull] Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
	{
		return thisValue.GroupBy(keySelector, elt => elt, (key, elements) => (IProducerGrouping<TKey, TSource>)new ProducerGrouping<TKey, TSource>(key, elements), comparer);
	}

	/// <summary>
	/// Groups the elements of a sequence according to a specified key selector function
	/// and creates a result value from each group and its key. The elements of each
	/// group are projected by using a specified function.
	/// </summary>
	/// <typeparam name="TElement">The return-type of the transform used to process the
	/// values within each grouping.</typeparam>
	/// <typeparam name="TKey">The return-type of the transform used to group the sequence</typeparam>
	/// <typeparam name="TSource">The values to be yielded by the original data-source</typeparam>
	/// <param name="elementSelector">A function to map each source element to an element in the appropriate group</param>
	/// <param name="keySelector">A function to extract the key for each element in hte original sequence.</param>
	/// <param name="thisValue">The data-source to be grouped</param>
	/// <remarks>This will force each unique grouping key to
	/// be buffered, but not the data itself</remarks>
	[NotNull]
	public static IDataProducer<IProducerGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>([NotNull] this IDataProducer<TSource> thisValue, [NotNull] Func<TSource, TKey> keySelector, [NotNull] Func<TSource, TElement> elementSelector)
	{
		return thisValue.GroupBy(keySelector, elementSelector, (key, elements) => (IProducerGrouping<TKey, TElement>)new ProducerGrouping<TKey, TElement>(key, elements), EqualityComparer<TKey>.Default);
	}

	/// <summary>
	/// Groups the elements of a sequence according to a specified key selector function
	/// and creates a result value from each group and its key.
	/// </summary>
	/// <typeparam name="TKey">The return-type of the transform used to group the sequence</typeparam>
	/// <typeparam name="TResult">The final values to be yielded after processing</typeparam>
	/// <typeparam name="TSource">The values to be yielded by the original data-source</typeparam>
	/// <param name="keySelector">A function to extract the key for each element in hte original sequence.</param>
	/// <param name="resultSelector">A function to create a result value from each group.</param>
	/// <param name="thisValue">The data-source to be grouped</param>
	/// <remarks>This will force each unique grouping key to
	/// be buffered, but not the data itself</remarks>
	[NotNull]
	public static IDataProducer<TResult> GroupBy<TSource, TKey, TResult>([NotNull] this IDataProducer<TSource> thisValue, [NotNull] Func<TSource, TKey> keySelector, [NotNull] Func<TKey, IDataProducer<TSource>, TResult> resultSelector)
	{
		return thisValue.GroupBy(keySelector, elt => elt, resultSelector, EqualityComparer<TKey>.Default);
	}
	/// <summary>
	/// Groups the elements of a sequence according to a specified key selector function
	/// and creates a result value from each group and its key. The elements of each
	/// group are projected by using a specified function.
	/// </summary>
	/// <typeparam name="TElement">The return-type of the transform used to process the
	/// values within each grouping.</typeparam>
	/// <typeparam name="TKey">The return-type of the transform used to group the sequence</typeparam>
	/// <typeparam name="TSource">The values to be yielded by the original data-source</typeparam>
	/// <param name="comparer">Used to compare grouping keys</param>
	/// <param name="elementSelector">A function to map each source element to an element in the appropriate group</param>
	/// <param name="keySelector">A function to extract the key for each element in hte original sequence.</param>
	/// <param name="thisValue">The data-source to be grouped</param>
	/// <remarks>This will force each unique grouping key to
	/// be buffered, but not the data itself</remarks>
	[NotNull]
	public static IDataProducer<IProducerGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>([NotNull] this IDataProducer<TSource> thisValue, [NotNull] Func<TSource, TKey> keySelector, [NotNull] Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer)
	{
		return thisValue.GroupBy(keySelector, elementSelector, (key, elements) => (IProducerGrouping<TKey, TElement>)new ProducerGrouping<TKey, TElement>(key, elements), comparer);
	}
	/// <summary>
	/// Groups the elements of a sequence according to a specified key selector function
	/// and creates a result value from each group and its key. The elements of each
	/// group are projected by using a specified function.
	/// </summary>
	/// <typeparam name="TElement">The return-type of the transform used to process the
	/// values within each grouping.</typeparam>
	/// <typeparam name="TKey">The return-type of the transform used to group the sequence</typeparam>
	/// <typeparam name="TResult">The final values to be yielded after processing</typeparam>
	/// <typeparam name="TSource">The values to be yielded by the original data-source</typeparam>
	/// <param name="elementSelector">A function to map each source element to an element in the appropriate group</param>
	/// <param name="keySelector">A function to extract the key for each element in hte original sequence.</param>
	/// <param name="resultSelector">A function to create a result value from each group.</param>
	/// <param name="thisValue">The data-source to be grouped</param>
	/// <remarks>This will force each unique grouping key to
	/// be buffered, but not the data itself</remarks>
	[NotNull]
	public static IDataProducer<TResult> GroupBy<TSource, TKey, TElement, TResult>([NotNull] this IDataProducer<TSource> thisValue, [NotNull] Func<TSource, TKey> keySelector, [NotNull] Func<TSource, TElement> elementSelector, [NotNull] Func<TKey, IDataProducer<TElement>, TResult> resultSelector)
	{
		return thisValue.GroupBy(keySelector, elementSelector, resultSelector, EqualityComparer<TKey>.Default);
	}

	/// <summary>
	/// Groups the elements of a sequence according to a specified key selector function
	/// and creates a result value from each group and its key.
	/// </summary>
	/// <typeparam name="TKey">The return-type of the transform used to group the sequence</typeparam>
	/// <typeparam name="TResult">The final values to be yielded after processing</typeparam>
	/// <typeparam name="TSource">The values to be yielded by the original data-source</typeparam>
	/// <param name="comparer">Used to compare grouping keys</param>
	/// <param name="keySelector">A function to extract the key for each element in hte original sequence.</param>
	/// <param name="resultSelector">A function to create a result value from each group.</param>
	/// <param name="thisValue">The data-source to be grouped</param>
	/// <remarks>This will force each unique grouping key to
	/// be buffered, but not the data itself</remarks>
	[NotNull]
	public static IDataProducer<TResult> GroupBy<TSource, TKey, TResult>([NotNull] this IDataProducer<TSource> thisValue, [NotNull] Func<TSource, TKey> keySelector, [NotNull] Func<TKey, IDataProducer<TSource>, TResult> resultSelector, IEqualityComparer<TKey> comparer)
	{
		return thisValue.GroupBy(keySelector, elt => elt, resultSelector, comparer);
	}
	/// <summary>
	/// Groups the elements of a sequence according to a specified key selector function
	/// and creates a result value from each group and its key. The elements of each
	/// group are projected by using a specified function.
	/// </summary>
	/// <typeparam name="TElement">The return-type of the transform used to process the
	/// values within each grouping.</typeparam>
	/// <typeparam name="TKey">The return-type of the transform used to group the sequence</typeparam>
	/// <typeparam name="TResult">The final values to be yielded after processing</typeparam>
	/// <typeparam name="TSource">The values to be yielded by the original data-source</typeparam>
	/// <param name="comparer">Used to compare grouping keys</param>
	/// <param name="elementSelector">A function to map each source element to an element in the appropriate group</param>
	/// <param name="keySelector">A function to extract the key for each element in hte original sequence.</param>
	/// <param name="resultSelector">A function to create a result value from each group.</param>
	/// <param name="thisValue">The data-source to be grouped</param>
	/// <remarks>This will force each unique grouping key to
	/// be buffered, but not the data itself</remarks>
	[NotNull]
	public static IDataProducer<TResult> GroupBy<TSource, TKey, TElement, TResult>([NotNull] this IDataProducer<TSource> thisValue, [NotNull] Func<TSource, TKey> keySelector, [NotNull] Func<TSource, TElement> elementSelector, [NotNull] Func<TKey, IDataProducer<TElement>, TResult> resultSelector, IEqualityComparer<TKey> comparer)
	{
		comparer ??= EqualityComparer<TKey>.Default;

		DataProducer<TResult> ret = new DataProducer<TResult>();
		Dictionary<TKey, DataProducer<TElement>> dictionary = new Dictionary<TKey, DataProducer<TElement>>(comparer);

		thisValue.Data += (_, value) =>
		{
			TKey key = keySelector(value);

			if (!dictionary.TryGetValue(key, out DataProducer<TElement> subProducer))
			{
				subProducer = new DataProducer<TElement>();
				dictionary[key] = subProducer;
				ret.Produce(resultSelector(key, subProducer));
			}

			subProducer.Produce(elementSelector(value));
		};

		thisValue.Completed += (_, _) =>
		{
			foreach (DataProducer<TElement> value in dictionary.Values)
				value.Complete();

			ret.Complete();
		};

		return ret;
	}

	/// <summary>
	/// Returns a future to the sum of a sequence of values that are
	/// obtained by taking a transform of the input sequence
	/// </summary>
	/// <remarks>Null values are removed from the sum</remarks>
	[NotNull]
	public static IFuture<TResult> Sum<TSource, TResult>([NotNull] this IDataProducer<TSource> thisValue, [NotNull] Func<TSource, TResult> selector)
	{
		Future<TResult> ret = new Future<TResult>();
		TResult sum = default(TResult);
		thisValue.Data += OnDataProduced;
		thisValue.Completed += OnEndOfData;
		return ret;

		void OnDataProduced(object sender, TSource item)
		{
			GenericsHelper.AddIfNotNull(ref sum, selector(item));
		}

		void OnEndOfData(object sender, EventArgs args)
		{
			ret.Value = sum;
		}
	}

	/// <summary>
	/// Returns a future to the sum of a sequence of values
	/// </summary>
	/// <remarks>Null values are removed from the sum</remarks>
	[NotNull]
	public static IFuture<TSource> Sum<TSource>([NotNull] this IDataProducer<TSource> thisValue)
	{
		return Sum(thisValue, x => x);
	}
	/// <summary>
	/// Returns a future to the average of a sequence of values that are
	/// obtained by taking a transform of the input sequence
	/// </summary>
	/// <remarks>Null values are removed from the average</remarks>
	[NotNull]
	public static IFuture<TResult> Average<TSource, TResult>([NotNull] this IDataProducer<TSource> thisValue, [NotNull] Func<TSource, TResult> selector)
		where TResult : struct, IComparable, IComparable<TResult>, IEquatable<TResult>, IConvertible, IFormattable
	{
		Future<TResult> ret = new Future<TResult>();
		TResult sum = Operator<TResult>.Zero;
		int count = 0; // should this be long? Would demand an Operator<TResult>.DivideInt64
		thisValue.Data += OnDataProduced;
		thisValue.Completed += OnEndOfData;
		return ret;

		void OnDataProduced(object sender, TSource item)
		{
			if (GenericsHelper.AddIfNotNull(ref sum, selector(item))) count++;
		}

		void OnEndOfData(object sender, EventArgs args)
		{
			if (count == 0)
			{
				sum = default(TResult);
				ret.Value = sum;
			}
			else
			{
				ret.Value = sum.DivideInt32(count);
			}
		}
	}

	/// <summary>
	/// Returns a future to the average of a sequence of values
	/// </summary>
	/// <remarks>Null values are removed from the average</remarks>
	[NotNull]
	public static IFuture<TSource> Average<TSource>([NotNull] this IDataProducer<TSource> thisValue)
		where TSource : struct, IComparable, IComparable<TSource>, IEquatable<TSource>, IConvertible, IFormattable
	{
		return Average(thisValue, x => x);
	}

	/// <summary>
	/// Returns a future to the average of a sequence of values
	/// </summary>
	[NotNull]
	public static IFuture<double> Average([NotNull] this IDataProducer<int> thisValue)
	{
		return Average<int, double>(thisValue, x => x); // silent cast to double
	}
	/// <summary>
	/// Returns a future to the average of a sequence of values
	/// </summary>
	/// <remarks>Null values are removed from the average</remarks>
	[NotNull]
	public static IFuture<double> Average([NotNull] this IDataProducer<int?> thisValue)
	{
		return Average<int?, double>(thisValue, x => x ?? 0); // silent cast to double?
	}
	/// <summary>
	/// Returns a future to the average of a sequence of values
	/// </summary>
	[NotNull]
	public static IFuture<double> Average([NotNull] this IDataProducer<long> thisValue)
	{
		return Average<long, double>(thisValue, x => x); // silent cast to double
	}
	/// <summary>
	/// Returns a future to the average of a sequence of values
	/// </summary>
	/// <remarks>Null values are removed from the average</remarks>
	[NotNull]
	public static IFuture<double> Average([NotNull] this IDataProducer<long?> thisValue)
	{
		return Average<long?, double>(thisValue, x => x ?? 0L); // silent cast to double?
	}
	/// <summary>
	/// Returns a future to the average of a sequence of values that are
	/// obtained by taking a transform of the input sequence
	/// </summary>
	[NotNull]
	public static IFuture<double> Average<TSource>([NotNull] this IDataProducer<TSource> thisValue, [NotNull] Func<TSource, int> selector)
	{
		return Average<TSource, double>(thisValue, x => selector(x)); // silent cast to double
	}
	/// <summary>
	/// Returns a future to the average of a sequence of values that are
	/// obtained by taking a transform of the input sequence
	/// </summary>
	/// <remarks>Null values are removed from the average</remarks>
	[NotNull]
	public static IFuture<double> Average<TSource>([NotNull] this IDataProducer<TSource> thisValue, [NotNull] Func<TSource, int?> selector)
	{
		return Average(thisValue, x => selector(x) ?? 0); // silent cast to double?
	}
	/// <summary>
	/// Returns a future to the average of a sequence of values that are
	/// obtained by taking a transform of the input sequence
	/// </summary>
	[NotNull]
	public static IFuture<double> Average<TSource>([NotNull] this IDataProducer<TSource> thisValue, [NotNull] Func<TSource, long> selector)
	{
		return Average<TSource, double>(thisValue, x => selector(x)); // silent cast to double
	}
	/// <summary>
	/// Returns a future to the average of a sequence of values that are
	/// obtained by taking a transform of the input sequence
	/// </summary>
	/// <remarks>Null values are removed from the average</remarks>
	[NotNull]
	public static IFuture<double> Average<TSource>([NotNull] this IDataProducer<TSource> thisValue, [NotNull] Func<TSource, long?> selector)
	{
		return Average<TSource, double>(thisValue, x => selector(x) ?? 0L); // silent cast to double?
	}

	/// <summary>
	/// Returns a future to the maximum of a sequence of values that are
	/// obtained by taking a transform of the input sequence, using the default comparer, using the default comparer
	/// </summary>
	/// <remarks>Null values are removed from the maximum</remarks>
	[NotNull]
	public static IFuture<TResult> Max<TSource, TResult>([NotNull] this IDataProducer<TSource> thisValue, [NotNull] Func<TSource, TResult> selector)
	{
		return thisValue.Select(selector).Max();
	}

	/// <summary>
	/// Returns a future to the maximum of a sequence of values, using the default comparer
	/// </summary>
	/// <remarks>Null values are removed from the maximum</remarks>
	[NotNull]
	public static IFuture<TSource> Max<TSource>([NotNull] this IDataProducer<TSource> thisValue)
	{
		Future<TSource> ret = new Future<TSource>();
		IComparer<TSource> comparer = Comparer<TSource>.Default;

		TSource current = default(TSource);
		bool empty = true, canBeNull = typeof(TSource).IsClass;

		thisValue.Data += (_, value) =>
		{
			if (canBeNull && !value.HasValue())
			{
				// NOP
			}
			else if (empty)
			{
				current = value;
				empty = false;
			}
			else if (comparer.Compare(value, current) > 0)
			{
				current = value;
			}
		};

		thisValue.Completed += (_, _) =>
		{
			// Only value types should throw an exception
			if (empty && !ReferenceEquals(current, null)) throw new InvalidOperationException("Empty sequence");
			ret.Value = current;
		};

		return ret;
	}

	/// <summary>
	/// Returns a future to the minimum of a sequence of values that are
	/// obtained by taking a transform of the input sequence, using the default comparer
	/// </summary>
	/// <remarks>Null values are removed from the minimum</remarks>
	[NotNull]
	public static IFuture<TResult> Min<TSource, TResult>([NotNull] this IDataProducer<TSource> thisValue, [NotNull] Func<TSource, TResult> selector)
	{
		return thisValue.Select(selector).Min();
	}

	/// <summary>
	/// Returns a future to the minimum of a sequence of values, using the default comparer
	/// </summary>
	/// <remarks>Null values are removed from the minimum</remarks>
	[NotNull]
	public static IFuture<TSource> Min<TSource>([NotNull] this IDataProducer<TSource> thisValue)
	{
		Future<TSource> ret = new Future<TSource>();
		IComparer<TSource> comparer = Comparer<TSource>.Default;

		TSource current = default(TSource);
		bool empty = true, canBeNull = typeof(TSource).IsClass;

		thisValue.Data += (_, value) =>
		{
			if (canBeNull && !value.HasValue())
			{
				// NOP
			}
			else if (empty)
			{
				current = value;
				empty = false;
			}
			else if (comparer.Compare(value, current) < 0)
			{
				current = value;
			}
		};
		thisValue.Completed += (_, _) =>
		{
			// Only value types should throw an exception
			if (empty && !ReferenceEquals(current, null)) throw new InvalidOperationException("Empty sequence");
			ret.Value = current;
		};

		return ret;
	}

	/// <summary>
	/// Filters a data-producer based on a predicate on each value
	/// </summary>
	/// <param name="thisValue">The data-producer to be filtered</param>
	/// <param name="predicate">The condition to be satisfied</param>
	/// <returns>A filtered data-producer; only matching values will raise the Data event</returns>
	[NotNull]
	public static IDataProducer<TSource> Where<TSource>([NotNull] this IDataProducer<TSource> thisValue, [NotNull] Func<TSource, bool> predicate)
	{
		return thisValue.Where((x, _) => predicate(x));
	}
	/// <summary>
	/// Filters a data-producer based on a predicate on each value; the index
	/// in the sequence is used in the predicate
	/// </summary>
	/// <param name="thisValue">The data-producer to be filtered</param>
	/// <param name="predicate">The condition to be satisfied</param>
	/// <returns>A filtered data-producer; only matching values will raise the Data event</returns>
	[NotNull]
	public static IDataProducer<TSource> Where<TSource>([NotNull] this IDataProducer<TSource> thisValue, [NotNull] Func<TSource, int, bool> predicate)
	{
		DataProducer<TSource> ret = new DataProducer<TSource>();
		int index = 0;

		thisValue.Data += (_, value) =>
		{
			if (predicate(value, index++))
			{
				ret.Produce(value);
			}
		};

		thisValue.Completed += (_, _) => ret.Complete();
		return ret;
	}

	/// <summary>
	/// Returns a data-producer that yields the values from the sequence, or which yields the given
	/// singleton value if no data is produced.
	/// </summary>
	/// <param name="defaultValue">The default value to be yielded if no data is produced.</param>
	/// <param name="thisValue">The thisValue data-producer.</param>
	[NotNull]
	public static IDataProducer<TSource> DefaultIfEmpty<TSource>([NotNull] this IDataProducer<TSource> thisValue, TSource defaultValue)
	{
		DataProducer<TSource> ret = new DataProducer<TSource>();
		bool empty = true;
		thisValue.Data += (_, value) =>
		{
			empty = false;
			ret.Produce(value);
		};
		thisValue.Completed += (_, _) =>
		{
			if (empty) ret.Produce(defaultValue);
			ret.Complete();
		};
		return ret;
	}
	/// <summary>
	/// Returns a data-producer that yields the values from the sequence, or which yields the default
	/// value for the Type if no data is produced.
	/// </summary>
	/// <param name="thisValue">The thisValue data-producer.</param>
	[NotNull]
	public static IDataProducer<TSource> DefaultIfEmpty<TSource>([NotNull] this IDataProducer<TSource> thisValue) { return thisValue.DefaultIfEmpty(default(TSource)); }

	/// <summary>
	/// Returns a projection on the data-producer, using a transformation to
	/// map each element into a new form.
	/// </summary>
	/// <typeparam name="TSource">The thisValue type</typeparam>
	/// <typeparam name="TResult">The projected type</typeparam>
	/// <param name="thisValue">The thisValue data-producer</param>
	/// <param name="projection">The transformation to apply to each element.</param>
	[NotNull]
	public static IDataProducer<TResult> Select<TSource, TResult>([NotNull] this IDataProducer<TSource> thisValue, [NotNull] Func<TSource, TResult> projection)
	{
		return thisValue.Select((t, _) => projection(t));
	}
	/// <summary>
	/// Returns a projection on the data-producer, using a transformation
	/// (involving the elements' index in the sequence) to
	/// map each element into a new form.
	/// </summary>
	/// <typeparam name="TSource">The thisValue type</typeparam>
	/// <typeparam name="TResult">The projected type</typeparam>
	/// <param name="thisValue">The thisValue data-producer</param>
	/// <param name="projection">The transformation to apply to each element.</param>
	[NotNull]
	public static IDataProducer<TResult> Select<TSource, TResult>([NotNull] this IDataProducer<TSource> thisValue, [NotNull] Func<TSource, int, TResult> projection)
	{
		DataProducer<TResult> ret = new DataProducer<TResult>();
		int index = 0;
		thisValue.Data += (_, value) => ret.Produce(projection(value, index++));
		thisValue.Completed += (_, _) => ret.Complete();
		return ret;
	}

	/// <summary>
	/// Returns a data-producer that will yield a specified number of
	/// contiguous elements from the start of a sequence - i.e.
	/// "the first &lt;x&gt; elements".
	/// </summary>
	/// <param name="thisValue">The thisValue data-producer</param>
	/// <param name="count">The maximum number of elements to return</param>
	[NotNull]
	public static IDataProducer<TSource> Take<TSource>([NotNull] this IDataProducer<TSource> thisValue, int count)
	{
		DataProducer<TSource> ret = new DataProducer<TSource>();
		thisValue.Data += Production;
		thisValue.Completed += Completion;
		return ret;

		void Production(object sender, TSource value)
		{
			if (count > 0)
			{
				ret.Produce(value);
				count--;
			}

			if (count <= 0)
			{
				thisValue.Completed -= Completion;
				thisValue.Data -= Production;
				ret.Complete();
			}
		}

		void Completion(object sender, EventArgs args) { ret.Complete(); }
	}

	/// <summary>
	/// Returns a data-producer that will ignore a specified number of
	/// contiguous elements from the start of a sequence, and yield
	/// all elements after this point - i.e. 
	/// "elements from index &lt;x&gt; onwards".
	/// </summary>
	/// <param name="thisValue">The thisValue data-producer</param>
	/// <param name="count">The number of elements to ignore</param>
	[NotNull]
	public static IDataProducer<TSource> Skip<TSource>([NotNull] this IDataProducer<TSource> thisValue, int count)
	{
		DataProducer<TSource> ret = new DataProducer<TSource>();
		thisValue.Data += (_, value) =>
		{
			if (count > 0) count--;
			else ret.Produce(value);
		};
		thisValue.Completed += (_, _) => ret.Complete();
		return ret;
	}
	/// <summary>
	/// Returns a data-producer that will yield
	/// elements a sequence as long as a condition
	/// is satisfied; when the condition fails for an element,
	/// that element and all subsequent elements are ignored.
	/// </summary>
	/// <param name="thisValue">The thisValue data-producer</param>
	/// <param name="predicate">The condition to yield elements</param>
	[NotNull]
	public static IDataProducer<TSource> TakeWhile<TSource>([NotNull] this IDataProducer<TSource> thisValue, [NotNull] Func<TSource, bool> predicate)
	{
		return thisValue.TakeWhile((x, _) => predicate(x));
	}
	/// <summary>
	/// Returns a data-producer that will yield
	/// elements a sequence as long as a condition
	/// (involving the element's index in the sequence)
	/// is satisfied; when the condition fails for an element,
	/// that element and all subsequent elements are ignored.
	/// </summary>
	/// <param name="thisValue">The thisValue data-producer</param>
	/// <param name="predicate">The condition to yield elements</param>        
	[NotNull]
	public static IDataProducer<TSource> TakeWhile<TSource>([NotNull] this IDataProducer<TSource> thisValue, [NotNull] Func<TSource, int, bool> predicate)
	{
		DataProducer<TSource> ret = new DataProducer<TSource>();
		int index = 0;
		thisValue.Data += Production;
		thisValue.Completed += Completion;
		return ret;

		void Production(object sender, TSource value)
		{
			if (!predicate(value, index++))
			{
				ret.Complete();
				thisValue.Data -= Production;
				thisValue.Completed -= Completion;
			}
			else
			{
				ret.Produce(value);
			}
		}

		void Completion(object sender, EventArgs args) { ret.Complete(); }
	}

	/// <summary>
	/// Returns a data-producer that will ignore the
	/// elements from the start of a sequence while a condition
	/// is satisfied; when the condition fails for an element,
	/// that element and all subsequent elements are yielded.
	/// </summary>
	/// <param name="thisValue">The thisValue data-producer</param>
	/// <param name="predicate">The condition to skip elements</param>
	[NotNull]
	public static IDataProducer<TSource> SkipWhile<TSource>([NotNull] this IDataProducer<TSource> thisValue, [NotNull] Func<TSource, bool> predicate)
	{
		return thisValue.SkipWhile((t, _) => predicate(t));
	}
	/// <summary>
	/// Returns a data-producer that will ignore the
	/// elements from the start of a sequence while a condition
	/// (involving the elements' index in the sequence)
	/// is satisfied; when the condition fails for an element,
	/// that element and all subsequent elements are yielded.
	/// </summary>
	/// <param name="thisValue">The thisValue data-producer</param>
	/// <param name="predicate">The condition to skip elements</param>
	[NotNull]
	public static IDataProducer<TSource> SkipWhile<TSource>([NotNull] this IDataProducer<TSource> thisValue, [NotNull] Func<TSource, int, bool> predicate)
	{
		DataProducer<TSource> ret = new DataProducer<TSource>();
		bool skipping = true;
		int index = 0;
		thisValue.Data += (_, value) =>
		{
			if (skipping) skipping = predicate(value, index++);
			// Note - not an else clause!
			if (!skipping) ret.Produce(value);
		};
		thisValue.Completed += (_, _) => ret.Complete();
		return ret;
	}

	/// <summary>
	/// Returns a data-producer that yields the first instance of each unique
	/// value in the sequence; subsequent identical values are ignored.
	/// </summary>
	/// <param name="thisValue">The data-producer</param>
	/// <remarks>This will force the first instance of each unique value to be buffered</remarks>
	[NotNull]
	public static IDataProducer<TSource> Distinct<TSource>([NotNull] this IDataProducer<TSource> thisValue) { return thisValue.Distinct(EqualityComparer<TSource>.Default); }

	/// <summary>
	/// Returns a data-producer that yields the first instance of each unique
	/// value in the sequence; subsequent identical values are ignored.
	/// </summary>
	/// <param name="thisValue">The data-producer</param>
	/// <param name="comparer">Used to determine equality between values</param>
	/// <remarks>This will force the first instance of each unique value to be buffered</remarks>
	[NotNull]
	public static IDataProducer<TSource> Distinct<TSource>([NotNull] this IDataProducer<TSource> thisValue, IEqualityComparer<TSource> comparer)
	{
		comparer ??= EqualityComparer<TSource>.Default;

		DataProducer<TSource> ret = new DataProducer<TSource>();
		HashSet<TSource> set = new HashSet<TSource>(comparer);

		thisValue.Data += (_, value) =>
		{
			if (set.Add(value))
			{
				ret.Produce(value);
			}
		};
		thisValue.Completed += (_, _) => ret.Complete();
		return ret;
	}

	/// <summary>
	/// Reverses the order of a sequence
	/// </summary>
	/// <param name="thisValue">The data-producer</param>
	/// <returns>A data-producer that yields the sequence
	/// in the reverse order</returns>
	/// <remarks>This will force all data to be buffered</remarks>
	[NotNull]
	public static IDataProducer<TSource> Reverse<TSource>([NotNull] this IDataProducer<TSource> thisValue)
	{
		DataProducer<TSource> ret = new DataProducer<TSource>();

		// use List (rather than ToList) so we have a List<T> with
		// Reverse immediately available (more efficient, and 2.0 compatible)
		List<TSource> results = new List<TSource>();
		thisValue.Data += (_, item) => results.Add(item);
		thisValue.Completed += (_, _) => 
		{
			List<TSource> items = new List<TSource>(results);
			items.Reverse();
			ret.ProduceAndComplete(items);
		};

		return ret;
	}

	/// <summary>
	/// Further orders the values from an ordered data-thisValue by a transform on each term, ascending
	/// (the sort operation is only applied once for the combined ordering)
	/// </summary>
	/// <param name="thisValue">The original data-producer and ordering</param>
	/// <param name="selector">Returns the value (for each term) by which to order the sequence</param>
	/// <returns>A data-producer that yields the sequence ordered
	/// by the selected value</returns>
	/// <remarks>This will force all data to be buffered</remarks>
	[NotNull]
	public static IOrderedDataProducer<TSource> ThenBy<TSource, TKey>([NotNull] this IOrderedDataProducer<TSource> thisValue, [NotNull] Func<TSource, TKey> selector)
	{
		return ThenBy(thisValue, selector, Comparer<TKey>.Default, false);
	}

	/// <summary>
	/// Further orders the values from an ordered data-thisValue by a transform on each term, ascending
	/// (the sort operation is only applied once for the combined ordering)
	/// </summary>
	/// <param name="thisValue">The original data-producer and ordering</param>
	/// <param name="selector">Returns the value (for each term) by which to order the sequence</param>
	/// <param name="comparer">Comparer to compare the selected values</param>
	/// <returns>A data-producer that yields the sequence ordered
	/// by the selected value</returns>
	/// <remarks>This will force all data to be buffered</remarks>
	[NotNull]
	public static IOrderedDataProducer<TSource> ThenBy<TSource, TKey>([NotNull] this IOrderedDataProducer<TSource> thisValue, [NotNull] Func<TSource, TKey> selector, IComparer<TKey> comparer)
	{
		return ThenBy(thisValue, selector, comparer, false);
	}

	/// <summary>
	/// Further orders the values from an ordered data-thisValue by a transform on each term, descending
	/// (the sort operation is only applied once for the combined ordering)
	/// </summary>
	/// <param name="thisValue">The original data-producer and ordering</param>
	/// <param name="selector">Returns the value (for each term) by which to order the sequence</param>
	/// <returns>A data-producer that yields the sequence ordered
	/// by the selected value</returns>
	/// <remarks>This will force all data to be buffered</remarks>
	[NotNull]
	public static IOrderedDataProducer<TSource> ThenByDescending<TSource, TKey>([NotNull] this IOrderedDataProducer<TSource> thisValue, [NotNull] Func<TSource, TKey> selector)
	{
		return ThenBy(thisValue, selector, Comparer<TKey>.Default, true);
	}

	/// <summary>
	/// Further orders the values from an ordered data-thisValue by a transform on each term, descending
	/// (the sort operation is only applied once for the combined ordering)
	/// </summary>
	/// <param name="thisValue">The original data-producer and ordering</param>
	/// <param name="selector">Returns the value (for each term) by which to order the sequence</param>
	/// <param name="comparer">Comparer to compare the selected values</param>
	/// <returns>A data-producer that yields the sequence ordered
	/// by the selected value</returns>
	/// <remarks>This will force all data to be buffered</remarks>        
	[NotNull]
	public static IOrderedDataProducer<TSource> ThenByDescending<TSource, TKey>([NotNull] this IOrderedDataProducer<TSource> thisValue, [NotNull] Func<TSource, TKey> selector, IComparer<TKey> comparer)
	{
		return ThenBy(thisValue, selector, comparer, true);
	}

	/// <summary>
	/// Orders the values from a data-thisValue by a transform on each term, ascending
	/// </summary>
	/// <param name="thisValue">The original data-producer</param>
	/// <param name="selector">Returns the value (for each term) by which to order the sequence</param>
	/// <returns>A data-producer that yields the sequence ordered
	/// by the selected value</returns>
	/// <remarks>This will force all data to be buffered</remarks>
	[NotNull]
	public static IOrderedDataProducer<TSource> OrderBy<TSource, TKey>([NotNull] this IDataProducer<TSource> thisValue, [NotNull] Func<TSource, TKey> selector)
	{
		return OrderBy(thisValue, selector, Comparer<TKey>.Default, false);
	}

	/// <summary>
	/// Orders the values from a data-thisValue by a transform on each term, ascending
	/// </summary>
	/// <param name="thisValue">The original data-producer</param>
	/// <param name="selector">Returns the value (for each term) by which to order the sequence</param>
	/// <param name="comparer">Comparer to compare the selected values</param>
	/// <returns>A data-producer that yields the sequence ordered
	/// by the selected value</returns>
	/// <remarks>This will force all data to be buffered</remarks>
	[NotNull]
	public static IOrderedDataProducer<TSource> OrderBy<TSource, TKey>([NotNull] this IDataProducer<TSource> thisValue, [NotNull] Func<TSource, TKey> selector, IComparer<TKey> comparer)
	{
		return OrderBy(thisValue, selector, comparer, false);
	}

	/// <summary>
	/// Orders the values from a data-thisValue by a transform on each term, descending
	/// </summary>
	/// <param name="thisValue">The original data-producer</param>
	/// <param name="selector">Returns the value (for each term) by which to order the sequence</param>
	/// <returns>A data-producer that yields the sequence ordered
	/// by the selected value</returns>
	/// <remarks>This will force all data to be buffered</remarks>
	[NotNull]
	public static IOrderedDataProducer<TSource> OrderByDescending<TSource, TKey>([NotNull] this IDataProducer<TSource> thisValue, [NotNull] Func<TSource, TKey> selector)
	{
		return OrderBy(thisValue, selector, Comparer<TKey>.Default, true);
	}

	/// <summary>
	/// Orders the values from a data-thisValue by a transform on each term, descending
	/// </summary>
	/// <param name="thisValue">The original data-producer</param>
	/// <param name="selector">Returns the value (for each term) by which to order the sequence</param>
	/// <param name="comparer">Comparer to compare the selected values</param>
	/// <returns>A data-producer that yields the sequence ordered
	/// by the selected value</returns>
	/// <remarks>This will force all data to be buffered</remarks>
	[NotNull]
	public static IOrderedDataProducer<TSource> OrderByDescending<TSource, TKey>([NotNull] this IDataProducer<TSource> thisValue, [NotNull] Func<TSource, TKey> selector, IComparer<TKey> comparer)
	{
		return OrderBy(thisValue, selector, comparer, true);
	}

	[NotNull]
	private static IOrderedDataProducer<TSource> OrderBy<TSource, TKey>(IDataProducer<TSource> thisValue, [NotNull] Func<TSource, TKey> selector, IComparer<TKey> comparer, bool descending)
	{
		IComparer<TSource> itemComparer = new ProjectionComparer<TSource, TKey>(selector, comparer ?? Comparer<TKey>.Default);
		if (descending) itemComparer = itemComparer.Reverse();

		// first, discard any existing "order by"s by going back to the producer
		IOrderedDataProducer<TSource> orderedProducer;
		bool first = true;

		while ((orderedProducer = thisValue as IOrderedDataProducer<TSource>) != null)
		{
			if (first)
			{
				// keep the top-most comparer to enforce a balanced sort
				itemComparer = new LinkedComparer<TSource>(itemComparer, orderedProducer.Comparer);
				first = false;
			}
			thisValue = orderedProducer.BaseProducer;
		}
		return new OrderedDataProducer<TSource>(thisValue, itemComparer);
	}

	[NotNull]
	private static IOrderedDataProducer<TSource> ThenBy<TSource, TKey>([NotNull] IOrderedDataProducer<TSource> thisValue, [NotNull] Func<TSource, TKey> selector, IComparer<TKey> comparer, bool descending)
	{
		IComparer<TSource> itemComparer = new ProjectionComparer<TSource, TKey>(selector, comparer ?? Comparer<TKey>.Default);
		if (descending) itemComparer = itemComparer.Reverse();
		itemComparer = new LinkedComparer<TSource>(thisValue.Comparer, itemComparer);
		return new OrderedDataProducer<TSource>(thisValue, itemComparer);
	}




























	/// <summary>
	/// Returns the number of elements in a sequence, as a future value.
	/// </summary>
	/// <typeparam name="TSource">The type of the elements of thisValue.</typeparam>
	/// <param name="thisValue">A sequence that contains elements to be counted.</param>
	/// <returns>The number of elements in the input sequence, as a future value.
	/// The actual count can only be retrieved after the thisValue has indicated the end
	/// of its data.
	/// </returns>
	[NotNull]
	public static IFuture<int> Count<TSource>([NotNull] this IDataProducer<TSource> thisValue)
	{
		Future<int> ret = new Future<int>();
		int count = 0;
		thisValue.Data += (_, _) => count++;
		thisValue.Completed += (_, _) => ret.Value = count;

		return ret;
	}

	/// <summary>
	/// Returns the number of elements in the specified sequence satisfy a condition,
	/// as a future value.
	/// </summary>
	/// <typeparam name="TSource">The type of the elements of thisValue.</typeparam>
	/// <param name="thisValue">A sequence that contains elements to be tested and counted.</param>
	/// <param name="predicate">A function to test each element for a condition.</param>
	/// <returns>A number that represents how many elements in the sequence satisfy 
	/// the condition in the predicate function, as a future value.
	/// The actual count can only be retrieved after the thisValue has indicated the end
	/// of its data.
	/// </returns>
	[NotNull]
	public static IFuture<int> Count<TSource>([NotNull] this IDataProducer<TSource> thisValue, [NotNull] Func<TSource, bool> predicate)
	{
		Future<int> ret = new Future<int>();
		int count = 0;

		thisValue.Data += (_, t) =>
		{
			if (predicate(t)) count++;
		};
		thisValue.Completed += (_, _) => ret.Value = count;

		return ret;
	}

	/// <summary>
	/// Returns the number of elements in a sequence, as a future value.
	/// </summary>
	/// <typeparam name="TSource">The type of the elements of thisValue.</typeparam>
	/// <param name="thisValue">A sequence that contains elements to be counted.</param>
	/// <returns>The number of elements in the input sequence, as a future value.
	/// The actual count can only be retrieved after the thisValue has indicated the end
	/// of its data.
	/// </returns>
	[NotNull]
	public static IFuture<long> LongCount<TSource>([NotNull] this IDataProducer<TSource> thisValue)
	{
		Future<long> ret = new Future<long>();
		int count = 0;
		thisValue.Data += (_, _) => count++;
		thisValue.Completed += (_, _) => ret.Value = count;

		return ret;
	}

	/// <summary>
	/// Returns the number of elements in the specified sequence satisfy a condition,
	/// as a future value.
	/// </summary>
	/// <typeparam name="TSource">The type of the elements of thisValue.</typeparam>
	/// <param name="thisValue">A sequence that contains elements to be tested and counted.</param>
	/// <param name="predicate">A function to test each element for a condition.</param>
	/// <returns>A number that represents how many elements in the sequence satisfy 
	/// the condition in the predicate function, as a future value.
	/// The actual count can only be retrieved after the thisValue has indicated the end
	/// of its data.
	/// </returns>
	[NotNull]
	public static IFuture<long> LongCount<TSource>([NotNull] this IDataProducer<TSource> thisValue, [NotNull] Func<TSource, bool> predicate)
	{
		Future<long> ret = new Future<long>();
		int count = 0;

		thisValue.Data += (_, t) =>
		{
			if (predicate(t)) count++;
		};
		thisValue.Completed += (_, _) => ret.Value = count;

		return ret;
	}

	/// <summary>
	/// Returns the first element of a sequence, as a future value.
	/// </summary>
	/// <typeparam name="TSource">The type of the elements of thisValue.</typeparam>
	/// <param name="thisValue">The sequence to return the first element of.</param>
	/// <returns>The first element in the specified sequence, as a future value.
	/// The actual value can only be retrieved after the thisValue has indicated the end
	/// of its data.
	/// </returns>
	[NotNull]
	public static IFuture<TSource> First<TSource>([NotNull] this IDataProducer<TSource> thisValue)
	{
		return thisValue.First(_ => true);
	}

	/// <summary>
	/// Returns the first element in a sequence that satisfies a specified condition, as a future value.
	/// </summary>
	/// <typeparam name="TSource">The type of the elements of thisValue.</typeparam>
	/// <param name="thisValue">The sequence to an element from.</param>
	/// <param name="predicate">A function to test each element for a condition.</param>
	/// <returns>The first element in the specified sequence that passes the test in 
	/// the specified predicate function, as a future value.
	/// The actual value can only be retrieved after the thisValue has indicated the end
	/// of its data.
	/// </returns>
	[NotNull]
	public static IFuture<TSource> First<TSource>([NotNull] this IDataProducer<TSource> thisValue, [NotNull] Func<TSource, bool> predicate)
	{
		Future<TSource> ret = new Future<TSource>();
		thisValue.Data += Production;
		thisValue.Completed += Completion;

		return ret;

		void Production(object sender, TSource t)
		{
			if (predicate(t))
			{
				ret.Value = t;
				thisValue.Completed -= Completion;
				thisValue.Data -= Production;
			}
		}

		static void Completion(object sender, EventArgs args) { throw new InvalidOperationException("Sequence is empty"); }
	}

	/// <summary>
	/// Returns the last element of a sequence, as a future value.
	/// </summary>
	/// <typeparam name="TSource">The type of the elements of thisValue.</typeparam>
	/// <param name="thisValue">The sequence to return the last element of.</param>
	/// <returns>The last element in the specified sequence, as a future value.
	/// The actual value can only be retrieved after the thisValue has indicated the end
	/// of its data.
	/// </returns>
	[NotNull]
	public static IFuture<TSource> Last<TSource>([NotNull] this IDataProducer<TSource> thisValue)
	{
		return thisValue.Last(_ => true);
	}

	/// <summary>
	/// Returns the last element in a sequence that satisfies a specified condition, as a future value.
	/// </summary>
	/// <typeparam name="TSource">The type of the elements of thisValue.</typeparam>
	/// <param name="thisValue">The sequence to an element from.</param>
	/// <param name="predicate">A function to test each element for a condition.</param>
	/// <returns>The last element in the specified sequence that passes the test in 
	/// the specified predicate function, as a future value.
	/// The actual value can only be retrieved after the thisValue has indicated the end
	/// of its data.
	/// </returns>
	[NotNull]
	public static IFuture<TSource> Last<TSource>([NotNull] this IDataProducer<TSource> thisValue, [NotNull] Func<TSource, bool> predicate)
	{
		Future<TSource> ret = new Future<TSource>();
		bool gotData = false;
		TSource prev = default(TSource);

		thisValue.Data += (_, value) =>
		{
			if (predicate(value))
			{
				prev = value;
				gotData = true;
			}
		};
		thisValue.Completed += (_, _) =>
		{
			if (!gotData) throw new InvalidOperationException("Sequence is empty");
			ret.Value = prev;
		};

		return ret;
	}
	/// <summary>
	/// Returns a future to the first value from a sequence, or the default for that type
	/// if no value is produced.
	/// </summary>
	/// <param name="thisValue">The thisValue data-producer.</param>
	[NotNull]
	public static IFuture<TSource> FirstOrDefault<TSource>([NotNull] this IDataProducer<TSource> thisValue)
	{
		return thisValue.FirstOrDefault(_ => true);
	}
	/// <summary>
	/// Returns a future to the first value from a sequence that matches the given condition, or the default
	/// for that type if no matching value is produced.
	/// </summary>
	/// <param name="thisValue">The thisValue data-producer.</param>
	/// <param name="predicate">The condition to be satisfied.</param>
	[NotNull]
	public static IFuture<TSource> FirstOrDefault<TSource>([NotNull] this IDataProducer<TSource> thisValue, [NotNull] Func<TSource, bool> predicate)
	{
		Future<TSource> ret = new Future<TSource>();
		thisValue.Data += Production;
		thisValue.Completed += Completion;

		return ret;

		void Production(object sender, TSource t)
		{
			if (predicate(t))
			{
				ret.Value = t;
				thisValue.Completed -= Completion;
				thisValue.Data -= Production;
			}
		}

		void Completion(object sender, EventArgs args) { ret.Value = default(TSource); }
	}
	/// <summary>
	/// Returns a future to the last value from a sequence, or the default for that type
	/// if no value is produced.
	/// </summary>
	/// <param name="thisValue">The thisValue data-producer.</param>
	[NotNull]
	public static IFuture<TSource> LastOrDefault<TSource>([NotNull] this IDataProducer<TSource> thisValue)
	{
		return thisValue.LastOrDefault(_ => true);
	}
	/// <summary>
	/// Returns the last value from a sequence that matches the given condition, or the default
	/// for that type if no matching value is produced.
	/// </summary>
	/// <param name="thisValue">The thisValue data-producer.</param>
	/// <param name="predicate">The condition to be satisfied.</param>
	[NotNull]
	public static IFuture<TSource> LastOrDefault<TSource>([NotNull] this IDataProducer<TSource> thisValue, [NotNull] Func<TSource, bool> predicate)
	{
		Future<TSource> ret = new Future<TSource>();
		TSource prev = default(TSource);

		thisValue.Data += (_, value) =>
		{
			if (predicate(value)) prev = value;
		};
		thisValue.Completed += (_, _) => ret.Value = prev;

		return ret;
	}
	/// <summary>
	/// Returns a future to a single value from a data-thisValue; an exception
	/// is thrown if no values, or multiple values, are encountered.
	/// </summary>
	/// <param name="thisValue">The thisValue data-producer.</param>
	/// <exception cref="InvalidOperationException">Zero or multiple terms are encountered.</exception>
	[NotNull]
	public static IFuture<TSource> Single<TSource>([NotNull] this IDataProducer<TSource> thisValue)
	{
		return thisValue.Single(_ => true);
	}

	/// <summary>
	/// Returns a future to a single value from a data-thisValue that matches the
	/// specified condition; an exception
	/// is thrown if no matching values, or multiple matching values, are encountered.
	/// </summary>
	/// <param name="thisValue">The thisValue data-producer.</param>
	/// <param name="predicate">The condition to be satisfied.</param>
	/// <exception cref="InvalidOperationException">Zero or multiple matching terms are encountered.</exception>
	[NotNull]
	public static IFuture<TSource> Single<TSource>([NotNull] this IDataProducer<TSource> thisValue, [NotNull] Func<TSource, bool> predicate)
	{
		Future<TSource> ret = new Future<TSource>();
		TSource output = default(TSource);
		bool gotValue = false;

		thisValue.Data += (_, value) =>
		{
			if (predicate(value))
			{
				if (gotValue) throw new InvalidOperationException("More than one element in thisValue data");
				output = value;
				gotValue = true;
			}
		};

		thisValue.Completed += (_, _) =>
		{
			if (!gotValue) throw new InvalidOperationException("No elements in thisValue data");
			ret.Value = output;
		};

		return ret;
	}
	/// <summary>
	/// Returns a future to a single value from a data-thisValue or the default value if no values
	/// are encountered. An exception
	/// is thrown if multiple values are encountered.
	/// </summary>
	/// <param name="thisValue">The thisValue data-producer.</param>
	/// <exception cref="InvalidOperationException">Multiple terms are encountered.</exception>
	[NotNull]
	public static IFuture<TSource> SingleOrDefault<TSource>([NotNull] this IDataProducer<TSource> thisValue)
	{
		return thisValue.SingleOrDefault(_ => true);
	}
	/// <summary>
	/// Returns a future to a single value from a data-thisValue that matches the
	/// specified condition, or the default value if no matching values
	/// are encountered. An exception
	/// is thrown if multiple matching values are encountered.
	/// </summary>
	/// <param name="thisValue">The thisValue data-producer.</param>
	/// <param name="predicate">The condition to be satisfied.</param>
	/// <exception cref="InvalidOperationException">Multiple matching terms are encountered.</exception>
	[NotNull]
	public static IFuture<TSource> SingleOrDefault<TSource>([NotNull] this IDataProducer<TSource> thisValue, [NotNull] Func<TSource, bool> predicate)
	{
		Future<TSource> ret = new Future<TSource>();
		TSource output = default(TSource);
		bool gotValue = false;

		thisValue.Data += (_, value) =>
		{
			if (predicate(value))
			{
				if (gotValue) throw new InvalidOperationException("More than one element in thisValue data");
				output = value;
				gotValue = true;
			}
		};

		thisValue.Completed += (_, _) =>
		{
			ret.Value = output;
		};

		return ret;
	}
	/// <summary>
	/// Returns a future to the element at the given position in the sequence
	/// </summary>
	/// <param name="thisValue">The data-producer</param>
	/// <param name="index">The index of the desired term in the sequence</param>
	/// <exception cref="ArgumentOutOfRangeException">If the specified index is negative
	/// or is never reached</exception>
	[NotNull]
	public static IFuture<TSource> ElementAt<TSource>([NotNull] this IDataProducer<TSource> thisValue, int index)
	{
		if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));

		Future<TSource> ret = new Future<TSource>();
		thisValue.Data += Production;
		thisValue.Completed += Completion;

		return ret;

		void Production(object sender, TSource value)
		{
			if (index == 0)
			{
				ret.Value = value;
				thisValue.Data -= Production;
				thisValue.Completed -= Completion;
			}
			else
			{
				index--;
			}
		}

		static void Completion(object sender, EventArgs args) { throw new ArgumentOutOfRangeException(nameof(index), "Specified index never reached"); }
	}

	/// <summary>
	/// Returns a future to the element at the given position in the sequence,
	/// or the default-value if the specified index is never reached
	/// </summary>
	/// <param name="thisValue">The data-producer</param>
	/// <param name="index">The index of the desired term in the sequence</param>
	/// <exception cref="ArgumentOutOfRangeException">If the specified index is negative</exception>
	[NotNull]
	public static IFuture<TSource> ElementAtOrDefault<TSource>([NotNull] this IDataProducer<TSource> thisValue, int index)
	{
		if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));

		Future<TSource> ret = new Future<TSource>();
		thisValue.Data += Production;
		thisValue.Completed += Completion;

		return ret;

		void Production(object sender, TSource value)
		{
			if (index == 0)
			{
				ret.Value = value;
				thisValue.Data -= Production;
				thisValue.Completed -= Completion;
			}
			else
			{
				index--;
			}
		}

		void Completion(object sender, EventArgs args) { ret.Value = default(TSource); }
	}

	/// <summary>
	/// Returns a future that indicates whether all values
	/// yielded by the data-producer satisfy a given condition.
	/// The future will return true for an empty sequence or
	/// where all values satisfy the condition, else false
	/// (if any value value fails to satisfy the condition).
	/// </summary>
	/// <param name="thisValue">The data-producer to be monitored.</param>
	/// <param name="predicate">The condition that must be satisfied by all terms.</param>
	[NotNull]
	public static IFuture<bool> All<TSource>([NotNull] this IDataProducer<TSource> thisValue, [NotNull] Func<TSource, bool> predicate)
		where TSource : IFuture<bool>
	{
		return FutureProxy<bool>.FromFuture(thisValue.Any(value => !predicate(value)), value => !value);
	}

	/// <summary>
	/// Returns a future that indicates whether any values are
	/// yielded by the data-producer. The future will return false
	/// for an empty sequence, or true for a sequence with values.
	/// </summary>
	/// <param name="thisValue">The data-producer to be monitored.</param>
	[NotNull]
	public static IFuture<bool> Any<TSource>([NotNull] this IDataProducer<TSource> thisValue)
	{
		Future<bool> ret = new Future<bool>();
		thisValue.Data += Production;
		thisValue.Completed += Completion;

		return ret;

		void Production(object sender, TSource value)
		{
			ret.Value = true;
			thisValue.Data -= Production;
			thisValue.Completed -= Completion;
		}

		void Completion(object sender, EventArgs args) { ret.Value = false; }
	}

	/// <summary>
	/// Returns a future that indicates whether any suitable values are
	/// yielded by the data-producer. The future will return false
	/// for an empty sequence or one with no matching values, or true for a sequence with matching values.
	/// </summary>
	/// <param name="thisValue">The data-producer to be monitored.</param>
	/// <param name="predicate">The condition that must be satisfied.</param>
	[NotNull]
	public static IFuture<bool> Any<TSource>([NotNull] this IDataProducer<TSource> thisValue, [NotNull] Func<TSource, bool> predicate)
	{
		Future<bool> ret = new Future<bool>();
		thisValue.Data += Production;
		thisValue.Completed += Completion;

		return ret;

		void Production(object sender, TSource value)
		{
			if (!predicate(value)) return;
			ret.Value = true;
			thisValue.Data -= Production;
			thisValue.Completed -= Completion;
		}

		void Completion(object sender, EventArgs args) { ret.Value = false; }
	}

	/// <summary>
	/// Returns a future to indicate whether the specified value
	/// is yielded by the data-thisValue.
	/// </summary>
	/// <typeparam name="TSource">The type of data to be yielded</typeparam>
	/// <param name="thisValue">The data-thisValue</param>
	/// <param name="value">The value to detect from the data-thisValue, checked with the default comparer</param>
	[NotNull]
	public static IFuture<bool> Contains<TSource>([NotNull] this IDataProducer<TSource> thisValue, TSource value)
	{
		return thisValue.Contains(value, EqualityComparer<TSource>.Default);
	}

	/// <summary>
	/// Returns a future to indicate whether the specified value
	/// is yielded by the data-thisValue.
	/// </summary>
	/// <typeparam name="TSource">The type of data to be yielded</typeparam>
	/// <param name="thisValue">The data-thisValue</param>
	/// <param name="value">The value to detect from the data-thisValue</param>
	/// <param name="comparer">The comparer to use to determine equality</param>
	[NotNull]
	public static IFuture<bool> Contains<TSource>([NotNull] this IDataProducer<TSource> thisValue, TSource value, IEqualityComparer<TSource> comparer)
	{
		comparer ??= EqualityComparer<TSource>.Default;
		return thisValue.Any(element => comparer.Equals(value, element));
	}

	/// <summary>
	/// Applies an accumulator function over the values yielded from
	/// a data-producer. The first value in the sequence
	/// is used as the initial accumulator value, and the specified function is used
	/// to select the result value. If the sequence is empty then
	/// the default value for TSource is returned.
	/// </summary>
	/// <typeparam name="TSource">The type of data yielded by the data-thisValue</typeparam>
	/// <param name="func">Accumulator function to be applied to each term in the sequence</param>
	/// <param name="thisValue">The data-thisValue for the values</param>
	[NotNull]
	public static IFuture<TSource> Aggregate<TSource>([NotNull] this IDataProducer<TSource> thisValue, [NotNull] Func<TSource, TSource, TSource> func)
	{
		Future<TSource> ret = new Future<TSource>();
		bool first = true;
		TSource current = default(TSource);

		thisValue.Data += (_, value) =>
		{
			if (first)
			{
				first = false;
				current = value;
			}
			else
			{
				current = func(current, value);
			}
		};
		thisValue.Completed += (_, _) => ret.Value = current;

		return ret;
	}

	/// <summary>
	/// Applies an accumulator function over the values yielded from
	/// a data-producer. The specified seed value
	/// is used as the initial accumulator value, and the specified function is used
	/// to select the result value
	/// </summary>
	/// <typeparam name="TSource">The type of data yielded by the data-thisValue</typeparam>
	/// <typeparam name="TAccumulate">The type to be used for the accumulator</typeparam>
	/// <param name="func">Accumulator function to be applied to each term in the sequence</param>
	/// <param name="seed">The initial value for the accumulator</param>
	/// <param name="thisValue">The data-thisValue for the values</param>
	[NotNull]
	public static IFuture<TAccumulate> Aggregate<TSource, TAccumulate>([NotNull] this IDataProducer<TSource> thisValue, TAccumulate seed, [NotNull] Func<TAccumulate, TSource, TAccumulate> func)
	{
		return thisValue.Aggregate(seed, func, x => x);
	}

	/// <summary>
	/// Applies an accumulator function over the values yielded from
	/// a data-producer, performing a transformation on the final
	/// accumulated value. The specified seed value
	/// is used as the initial accumulator value, and the specified function is used
	/// to select the result value
	/// </summary>
	/// <typeparam name="TSource">The type of data yielded by the data-thisValue</typeparam>
	/// <typeparam name="TResult">The final result type (after the accumulator has been transformed)</typeparam>
	/// <typeparam name="TAccumulate">The type to be used for the accumulator</typeparam>
	/// <param name="func">Accumulator function to be applied to each term in the sequence</param>
	/// <param name="resultSelector">Transformation to apply to the final
	/// accumulated value to produce the result</param>
	/// <param name="seed">The initial value for the accumulator</param>
	/// <param name="thisValue">The data-thisValue for the values</param>
	[NotNull]
	public static IFuture<TResult> Aggregate<TSource, TAccumulate, TResult>([NotNull] this IDataProducer<TSource> thisValue, TAccumulate seed, [NotNull] Func<TAccumulate, TSource, TAccumulate> func, [NotNull] Func<TAccumulate, TResult> resultSelector)
	{
		Future<TResult> result = new Future<TResult>();
		TAccumulate current = seed;

		thisValue.Data += (_, value) => current = func(current, value);
		thisValue.Completed += (_, _) => result.Value = resultSelector(current);

		return result;
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions
{
	public static class WebMvcIEnumerableExtension
	{
		[NotNull]
		public static IReadOnlyCollection<SelectListItem> ToSelectListItems<TSource, TText, TValue>([NotNull] this IEnumerable<TSource> thisValue, [NotNull] Func<TSource, TText> text,
			[NotNull] Func<TSource, TValue> value, int startIndex = 0, int count = -1)
		{
			if (startIndex < 0) throw new ArgumentOutOfRangeException(nameof(startIndex));

			ICollection<SelectListItem> newCollection;
			int i = startIndex - 1;

			switch (thisValue)
			{
				case ISet<TSource> set:
					set.Count.ValidateRange(startIndex, ref count);
					if (set.Count == 0 || count == 0) return Array.Empty<SelectListItem>();
					count += startIndex;
					newCollection = new List<SelectListItem>(count);
				
					foreach (TSource x in set.Skip(startIndex))
					{
						i++;
						if (i > count) break;

						SelectListItem item = new SelectListItem
						{
							Text = Convert.ToString(text(x)),
							Value = Convert.ToString(value(x))
						};

						newCollection.Add(item);
					}

					return newCollection.ToArray();
				case IList<TSource> list:
					list.Count.ValidateRange(startIndex, ref count);
					if (list.Count == 0 || count == 0) return Array.Empty<SelectListItem>();
					count += startIndex;
					newCollection = new List<SelectListItem>(count);

					for (i = startIndex; i < count; i++)
					{
						TSource x = list[i];
						SelectListItem item = new SelectListItem
						{
							Text = Convert.ToString(text(x)),
							Value = Convert.ToString(value(x))
						};

						newCollection.Add(item);
					}

					return newCollection.ToArray();
				case ICollection<TSource> collection:
					collection.Count.ValidateRange(startIndex, ref count);
					if (collection.Count == 0 || count == 0) return Array.Empty<SelectListItem>();
					count += startIndex;
					newCollection = new List<SelectListItem>(count);

					foreach (TSource x in collection.Skip(startIndex))
					{
						i++;
						if (i > count) break;

						SelectListItem item = new SelectListItem
						{
							Text = Convert.ToString(text(x)),
							Value = Convert.ToString(value(x))
						};

						newCollection.Add(item);
					}

					return newCollection.ToArray();
				case IReadOnlyCollection<TSource> readOnlyCollection:
					readOnlyCollection.Count.ValidateRange(startIndex, ref count);
					if (readOnlyCollection.Count == 0 || count == 0) return Array.Empty<SelectListItem>();
					count += startIndex;
					newCollection = new List<SelectListItem>(count);

					foreach (TSource x in readOnlyCollection.Skip(startIndex))
					{
						i++;
						if (i > count) break;

						SelectListItem item = new SelectListItem
						{
							Text = Convert.ToString(text(x)),
							Value = Convert.ToString(value(x))
						};

						newCollection.Add(item);
					}

					return newCollection.ToArray();
			}

			newCollection = new List<SelectListItem>(count);

			if (count > 0)
			{
				count += startIndex;

				foreach (TSource x in thisValue.Skip(startIndex))
				{
					i++;
					if (i > count) break;

					SelectListItem item = new SelectListItem
					{
						Text = Convert.ToString(text(x)),
						Value = Convert.ToString(value(x))
					};

					newCollection.Add(item);
				}
			}
			else
			{
				foreach (TSource x in thisValue.Skip(startIndex))
				{
					i++;

					SelectListItem item = new SelectListItem
					{
						Text = Convert.ToString(text(x)),
						Value = Convert.ToString(value(x))
					};

					newCollection.Add(item);
				}
			}

			return newCollection.ToArray();
		}

		[NotNull]
		public static SelectList ToSelectList<TSource, TText, TValue>([NotNull] this IEnumerable<TSource> thisValue, [NotNull] Func<TSource, TText> text, [NotNull] Func<TSource, TValue> value, int startIndex = 0, int count = -1)
		{
			IReadOnlyCollection<SelectListItem> items = ToSelectListItems(thisValue, text, value, startIndex, count);
			return new SelectList(items, nameof(SelectListItem.Value), nameof(SelectListItem.Text));
		}

		[NotNull]
		public static SelectList ToSelectList<TSource, TText, TValue>([NotNull] this IEnumerable<TSource> thisValue, [NotNull] Func<TSource, TText> text,
			[NotNull] Func<TSource, TValue> value, TValue selectedValue, int startIndex = 0, int count = -1)
		{
			IReadOnlyCollection<SelectListItem> items = ToSelectListItems(thisValue, text, value, startIndex, count);
			return new SelectList(items, nameof(SelectListItem.Value), nameof(SelectListItem.Text), selectedValue);
		}
	}
}
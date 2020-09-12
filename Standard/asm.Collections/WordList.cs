using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using asm.Extensions;
using JetBrains.Annotations;

namespace asm.Collections
{
	[Serializable]
	public class WordList : List<ushort>
	{
		public WordList()
		{
		}

		public WordList(int capacity) 
			: base(capacity)
		{
		}

		public WordList([NotNull] IEnumerable<ushort> collection) 
			: base(collection)
		{
		}

		public WordList([NotNull] IEnumerable<byte> enumerable)
		{
			AddRange(enumerable);
		}

		public WordList([NotNull] IEnumerable<char> enumerable)
		{
			AddRange(enumerable);
		}

		[NotNull]
		public static explicit operator WordList([NotNull] byte[] value) { return new WordList(value); }

		[NotNull]
		public static explicit operator byte[] ([NotNull] WordList value) { return value.ToByteArray(); }

		[NotNull]
		public byte[] ToByteArray()
		{
			if (Count == 0) return Array.Empty<byte>();

			byte[] bytes = new byte[Count * Constants.USHORT_SIZE];
			CopyTo(bytes);
			return bytes;
		}

		public void AddRange([NotNull] IEnumerable<byte> enumerable)
		{
			int c = 0;
			byte[] bytes = new byte[Constants.USHORT_SIZE];

			foreach (byte b in enumerable)
			{
				bytes[c % Constants.USHORT_SIZE] = b;
				c++;
				if (c % Constants.USHORT_SIZE == 0) Add(BitConverter.ToUInt16(bytes, 0));
			}
			
			if (c == 0 || c % Constants.USHORT_SIZE == 0) return;

			for (int i = c % Constants.USHORT_SIZE; i < Constants.USHORT_SIZE; i++)
				bytes[i] = 0;

			Add(BitConverter.ToUInt16(bytes, 0));
		}

		public void AddRange([NotNull] IEnumerable<char> enumerable)
		{
			foreach (char c in enumerable)
				Add(Convert.ToUInt16(c));
		}

		public void CopyTo([NotNull] byte[] array) { CopyTo(0, array, 0, Count); }
		public void CopyTo([NotNull] byte[] array, int arrayIndex) { CopyTo(0, array, arrayIndex, Count); }
		public void CopyTo(int index, [NotNull] byte[] array, int arrayIndex, int count)
		{
			if (array == null) throw new ArgumentNullException(nameof(array));

			if (count < 0) count = Count;
			int total = count * Constants.USHORT_SIZE;
			total.ValidateRange(index, ref count);
			total.ValidateRange(arrayIndex, ref count);
			if (count == 0 || total == 0) return;

			int lastIndex = index + count - 1;

			for (int i = index, j = 0; i <= lastIndex; i++, j += Constants.USHORT_SIZE)
				Array.Copy(BitConverter.GetBytes(this[i]), 0, array, j, Constants.USHORT_SIZE);
		}

		[NotNull]
		public static WordList From<T>([NotNull] IEnumerable<T> enumerable)
			where T : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible
		{
			WordList list = new WordList();
			list.AddRange(enumerable.Select(value => Convert.ToUInt16(value)));
			return list;
		}
	}

	public static class WordListExtension
	{
		[NotNull]
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static byte[] AsBytes([NotNull] this WordList thisValue) { return (byte[])thisValue; }

		[NotNull]
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static WordList AsWordList([NotNull] this byte[] thisValue) { return (WordList)thisValue; }
	}
}
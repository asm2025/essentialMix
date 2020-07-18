using System;
using System.Collections.Generic;
using System.Linq;
using asm.Extensions;
using JetBrains.Annotations;

namespace asm.Collections
{
	[Serializable]
	public class DwordList : List<uint>
	{
		public DwordList()
		{
		}

		public DwordList(int capacity) 
			: base(capacity)
		{
		}

		public DwordList([NotNull] IEnumerable<uint> collection) 
			: base(collection)
		{
		}

		public DwordList([NotNull] IEnumerable<byte> enumerable)
		{
			AddRange(enumerable);
		}

		public void AddRange([NotNull] IEnumerable<char> enumerable)
		{
			foreach (char c in enumerable)
				Add(Convert.ToUInt16(c));
		}

		[NotNull]
		public static explicit operator DwordList([NotNull] byte[] value) { return new DwordList(value); }

		[NotNull]
		public static explicit operator byte[] ([NotNull] DwordList value) { return value.ToByteArray(); }

		[NotNull]
		public byte[] ToByteArray()
		{
			if (Count == 0) return Array.Empty<byte>();
			
			byte[] bytes = new byte[Count * Constants.UINT_SIZE];
			CopyTo(bytes);
			return bytes;
		}

		public void AddRange([NotNull] IEnumerable<byte> enumerable)
		{
			int c = 0;
			byte[] bytes = new byte[Constants.UINT_SIZE];

			foreach (byte b in enumerable)
			{
				bytes[c % Constants.UINT_SIZE] = b;
				c++;
				if (c % Constants.UINT_SIZE == 0) Add(BitConverter.ToUInt32(bytes, 0));
			}
			
			if (c == 0 || c % Constants.UINT_SIZE == 0) return;

			for (int i = c % Constants.UINT_SIZE; i < Constants.UINT_SIZE; i++)
				bytes[i] = 0;

			Add(BitConverter.ToUInt32(bytes, 0));
		}

		public void CopyTo([NotNull] byte[] array) { CopyTo(0, array, 0, Count); }
		public void CopyTo([NotNull] byte[] array, int arrayIndex) { CopyTo(0, array, arrayIndex, Count); }
		public void CopyTo(int index, [NotNull] byte[] array, int arrayIndex, int count)
		{
			if (Count == 0) return;
			array.Length.ValidateRange(arrayIndex, ref count);
			if (count == 0) return;
			Count.ValidateRange(index, ref count);

			int lastIndex = index + count - 1;

			for (int i = index, j = 0; i <= lastIndex; i++, j += Constants.UINT_SIZE)
				Array.Copy(BitConverter.GetBytes(this[i]), 0, array, j, Constants.UINT_SIZE);
		}

		[NotNull]
		public static DwordList From<T>([NotNull] IEnumerable<T> enumerable)
			where T : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible
		{
			DwordList list = new DwordList();
			list.AddRange(enumerable.Select(value => Convert.ToUInt32(value)));
			return list;
		}
	}
}
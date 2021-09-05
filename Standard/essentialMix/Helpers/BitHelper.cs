// ****************************************************************************
//
// FLV Extract
// Copyright (C) 2006-2012  J.D. Purcell (moitah@yahoo.com)
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//
// ****************************************************************************

// some parts are from https://referencesource.microsoft.com/#system/compmod/system/collections/generic/bithelper.cs
// I have no idea why it's internal!!! Just have to duplicate the code.

using System;
using System.Security;
using essentialMix.Extensions;
using JetBrains.Annotations;

namespace essentialMix.Helpers
{
	/// <summary>
	/// ABOUT:
	/// Helps with operations that rely on bit marking to indicate whether an item in the 
	/// collection should be added, removed, visited already, etc. 
	/// 
	/// BitHelper doesn't allocate the array; you must pass in an array or ints allocated on the 
	/// stack or heap. ToIntArrayLength() tells you the int array size you must allocate. 
	/// 
	/// USAGE:
	/// Suppose you need to represent a bit array of length (i.e. logical bit array length)
	/// BIT_ARRAY_LENGTH. Then this is the suggested way to instantiate BitHelper:
	/// ***************************************************************************
	/// int intArrayLength = BitHelper.ToIntArrayLength(BIT_ARRAY_LENGTH);
	/// BitHelper bitHelper;
	/// if (intArrayLength less than stack alloc threshold)
	///     int* _arrayPtr = stackalloc int[intArrayLength];
	///     bitHelper = new BitHelper(_arrayPtr, intArrayLength);
	/// else
	///     int[] _arrayPtr = new int[intArrayLength];
	///     bitHelper = new BitHelper(_arrayPtr, intArrayLength);
	/// ***************************************************************************
	/// 
	/// IMPORTANT:
	/// The second ctor args, length, should be specified as the length of the int array, not
	/// the logical bit array. Because length is used for bounds checking into the int array,
	/// it's especially important to get this correct for the stackalloc version. See the code 
	/// samples above; this is the value gotten from ToIntArrayLength(). 
	/// 
	/// The length ctor argument is the only exception; for other methods -- MarkBit and 
	/// IsMarked -- pass in values as indices into the logical bit array, and it will be mapped
	/// to the position within the array of ints.
	/// </summary>
	// should not be serialized
	public unsafe class BitHelper
	{
		private const byte MARKED_BIT_FLAG = 1;
 
		// _length of underlying int array (not logical bit array)
		private readonly int _length;
        
		// ptr to stack alloc'd array of ints
		[SecurityCritical]
		private readonly int* _arrayPtr;
 
		// array of ints
		private readonly int[] _array;
 
		// whether to operate on stack alloc'd or heap alloc'd array 
		private readonly bool _useStackAlloc;
		
		/// <summary>
		/// Instantiates a BitHelper with a heap alloc'd array of ints
		/// </summary>
		/// <param name="bitArrayPtr">int array to hold bits</param>
		/// <param name="length">length of int array</param>
		// <SecurityKernel Critical="True" Ring="0">
		// <UsesUnsafeCode Name="Field: _arrayPtr" />
		// <UsesUnsafeCode Name="Parameter bitArrayPtr of type: Int32*" />
		// </SecurityKernel>
		[SecurityCritical]
		public BitHelper(int* bitArrayPtr, int length) 
		{
			_arrayPtr = bitArrayPtr;
			_length = length;
			_useStackAlloc = true;
		}
 
		/// <summary>
		/// Instantiates a BitHelper with a heap alloc'd array of ints
		/// </summary>
		/// <param name="bitArray">int array to hold bits</param>
		/// <param name="length">length of int array</param>
		public BitHelper(int[] bitArray, int length) 
		{
			_array = bitArray;
			_length = length;
		}
		
		/// <summary>
		/// Mark bit at specified position
		/// </summary>
		/// <param name="bitPosition"></param>
		// <SecurityKernel Critical="True" Ring="0">
		// <UsesUnsafeCode Name="Field: _arrayPtr" />
		// </SecurityKernel>
		[SecurityCritical]
		public void MarkBit(int bitPosition) 
		{
			if (_useStackAlloc) 
			{
				int bitArrayIndex = bitPosition / Constants.INT_BIT_SIZE;
				if (bitArrayIndex < _length && bitArrayIndex >= 0) _arrayPtr[bitArrayIndex] |= MARKED_BIT_FLAG << (bitPosition % Constants.INT_BIT_SIZE);
			}
			else 
			{
				int bitArrayIndex = bitPosition / Constants.INT_BIT_SIZE;
				if (bitArrayIndex < _length && bitArrayIndex >= 0) _array[bitArrayIndex] |= MARKED_BIT_FLAG << (bitPosition % Constants.INT_BIT_SIZE);
			}
		}

		/// <summary>
		/// Is bit at specified position marked?
		/// </summary>
		/// <param name="bitPosition"></param>
		/// <returns></returns>
		// <SecurityKernel Critical="True" Ring="0">
		// <UsesUnsafeCode Name="Field: m_arrayPtr" />
		// </SecurityKernel>
		[SecurityCritical]
		public bool IsMarked(int bitPosition)
		{
			if (_useStackAlloc)
			{
				int bitArrayIndex = bitPosition / Constants.INT_BIT_SIZE;
				return bitArrayIndex < _length && bitArrayIndex >= 0 
												&& (_arrayPtr[bitArrayIndex] & (MARKED_BIT_FLAG << (bitPosition % Constants.INT_BIT_SIZE))) != 0;
			}
			else
			{
				int bitArrayIndex = bitPosition / Constants.INT_BIT_SIZE;
				return bitArrayIndex < _length && bitArrayIndex >= 0 
												&& (_array[bitArrayIndex] & (MARKED_BIT_FLAG << (bitPosition % Constants.INT_BIT_SIZE))) != 0;
			}
		}

		[NotNull]
		public static byte[] CopyBlock([NotNull] byte[] bytes, int offset, int length)
		{
			if (!offset.InRangeRx(0, bytes.Length)) throw new ArgumentOutOfRangeException(nameof(offset));
			if (!length.InRange(0, bytes.Length)) throw new ArgumentOutOfRangeException(nameof(length));
			if (!(offset + length).InRange(0, bytes.Length)) throw new ArgumentOutOfRangeException(nameof(length));

			int startByte = offset / 8;
			int endByte = (offset + length - 1) / 8;
			int shiftA = offset % 8;
			int shiftB = 8 - shiftA;
			byte[] dst = new byte[(length + 7) / 8];

			if (shiftA == 0)
			{
				Buffer.BlockCopy(bytes, startByte, dst, 0, dst.Length);
			}
			else
			{
				int i;

				for (i = 0; i < endByte - startByte; i++)
				{
					dst[i] = (byte)((bytes[startByte + i] << shiftA) | (bytes[startByte + i + 1] >> shiftB));
				}

				if (i < dst.Length) dst[i] = (byte)(bytes[startByte + i] << shiftA);
			}

			dst[dst.Length - 1] &= (byte)(0xFF << (dst.Length * 8 - length));
			return dst;
		}

		public static void CopyBytes([NotNull] byte[] dst, int dstOffset, [NotNull] byte[] src) { Buffer.BlockCopy(src, 0, dst, dstOffset, src.Length); }

		public static int Read(ref ulong x, int length)
		{
			int r = (int)(x >> (64 - length));
			x <<= length;
			return r;
		}

		public static int Read([NotNull] byte[] bytes, ref int offset, int length)
		{
			if (!offset.InRangeRx(0, bytes.Length)) throw new ArgumentOutOfRangeException(nameof(offset));
			if (!length.InRange(0, bytes.Length)) throw new ArgumentOutOfRangeException(nameof(length));
			if (!(offset + length).InRange(0, bytes.Length)) throw new ArgumentOutOfRangeException(nameof(length));

			int startByte = offset / 8;
			int endByte = (offset + length - 1) / 8;
			int skipBits = offset % 8;
			ulong bits = 0;

			for (int i = 0; i <= Math.Min(endByte - startByte, 7); i++)
			{
				bits |= (ulong)bytes[startByte + i] << (56 - i * 8);
			}

			if (skipBits != 0) Read(ref bits, skipBits);
			offset += length;
			return Read(ref bits, length);
		}

		public static void Write(ref ulong x, int length, int value)
		{
			ulong mask = 0xFFFFFFFFFFFFFFFF >> (64 - length);
			x = (x << length) | ((ulong)value & mask);
		}

		public static short GetBitSize(short value)
		{
			return value < 1
						? (short)0
						: (short)(value * 8);
		}

		public static ushort GetBitSize(ushort value)
		{
			return value < 1
						? (ushort)0
						: (ushort)(value * 8);
		}

		public static int GetBitSize(int value)
		{
			return value < 1
						? 0
						: value * 8;
		}

		public static uint GetBitSize(uint value)
		{
			return value < 1
						? 0U
						: value * 8U;
		}

		public static long GetBitSize(long value)
		{
			return value < 1L
						? 0L
						: value * 8L;
		}

		public static ulong GetBitSize(ulong value)
		{
			return value < 1
						? 0UL
						: value * 8UL;
		}

		public static int ToIntArrayLength(int n)
		{
			return n > 0 ? (n - 1) / Constants.INT_BIT_SIZE + 1 : 0;
		}
	}
}
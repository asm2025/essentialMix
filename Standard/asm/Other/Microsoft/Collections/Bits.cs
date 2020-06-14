namespace asm.Other.Microsoft.Collections
{
	public unsafe class Bits // should not be serialized
	{
		private const byte MARKED_BIT_FLAG = 1;

		// m_length of underlying int array (not logical bit array)
		private readonly int _length;

		// ptr to stack alloc'd array of ints
		[System.Security.SecurityCritical]
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
		// <UsesUnsafeCode Name="Field: m_arrayPtr" />
		// <UsesUnsafeCode Name="Parameter bitArrayPtr of type: Int32*" />
		// </SecurityKernel>
		[System.Security.SecurityCritical]
		public Bits(int* bitArrayPtr, int length)
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
		public Bits(int[] bitArray, int length)
		{
			_array = bitArray;
			_length = length;
		}

		/// <summary>
		/// Mark bit at specified position
		/// </summary>
		/// <param name="bitPosition"></param>
		// <SecurityKernel Critical="True" Ring="0">
		// <UsesUnsafeCode Name="Field: m_arrayPtr" />
		// </SecurityKernel>
		[System.Security.SecurityCritical]
		public void MarkBit(int bitPosition)
		{
			int bitArrayIndex = bitPosition / Constants.INT_SIZE;
			if (bitArrayIndex >= _length || bitArrayIndex < 0) return;

			if (_useStackAlloc)
			{
				_arrayPtr[bitArrayIndex] |= MARKED_BIT_FLAG << (bitPosition % Constants.INT_SIZE);
			}
			else
			{
				_array[bitArrayIndex] |= MARKED_BIT_FLAG << (bitPosition % Constants.INT_SIZE);
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
		[System.Security.SecurityCritical]
		public bool IsMarked(int bitPosition)
		{
			if (_useStackAlloc)
			{
				int bitArrayIndex = bitPosition / Constants.INT_SIZE;
				return bitArrayIndex < _length && bitArrayIndex >= 0 && (_arrayPtr[bitArrayIndex] & (MARKED_BIT_FLAG << (bitPosition % Constants.INT_SIZE))) != 0;
			}
			else
			{
				int bitArrayIndex = bitPosition / Constants.INT_SIZE;
				return bitArrayIndex < _length && bitArrayIndex >= 0 && (_array[bitArrayIndex] & (MARKED_BIT_FLAG << (bitPosition % Constants.INT_SIZE))) != 0;
			}
		}
	}
}
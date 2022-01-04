using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using essentialMix.Patterns.Object;

namespace essentialMix.Internal;

internal unsafe class FastStringReplacer : Disposable
{
	public int FoundIndexes;
	private readonly char* _oldValue;
	private readonly int _oldValueLength;

	private readonly bool* _fastTable;
	private readonly char* _input;

	internal FastStringReplacer([NotNull] string input, [NotNull] string oldValue)
	{
		int inputLength = input.Length;
		_oldValueLength = oldValue.Length;

		_oldValue = (char*)Marshal.AllocHGlobal((_oldValueLength + 1) * sizeof(char));
		_input = (char*)Marshal.AllocHGlobal((inputLength + 1) * sizeof(char));
		_fastTable = (bool*)Marshal.AllocHGlobal(inputLength * sizeof(bool));

		fixed(char* inputSrc = input, oldValueSrc = oldValue)
		{
			Copy(inputSrc, _input);
			Copy(oldValueSrc, _oldValue);
		}

		BuildMatchedIndexes();
	}

	public void Replace([NotNull] char* outputPtr, int outputLength, [NotNull] string newValue)
	{
		int newValueLength = newValue.Length;

		char* inputPtr = _input;
		bool* fastTablePtr = _fastTable;

		fixed(char* newValuePtr = newValue)
		{
			while (*inputPtr != 0)
			{
				if (*fastTablePtr)
				{
					Copy(newValuePtr, outputPtr);
					outputLength -= newValueLength;
					outputPtr += newValueLength;

					inputPtr += _oldValueLength;
					fastTablePtr += _oldValueLength;
					continue;
				}

				*outputPtr++ = *inputPtr++;
				fastTablePtr++;
			}
		}

		*outputPtr = '\0';
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (_fastTable != null) Marshal.FreeHGlobal(new IntPtr(_fastTable));
			if (_input != null) Marshal.FreeHGlobal(new IntPtr(_input));
			if (_oldValue != null) Marshal.FreeHGlobal(new IntPtr(_oldValue));
		}
		base.Dispose(disposing);
	}

	private static void Copy(char* sourcePtr, char* targetPtr)
	{
		while ((*targetPtr++ = *sourcePtr++) != 0) { }
	}

	/// <summary>
	/// KMP?!
	/// </summary>
	private void BuildMatchedIndexes()
	{
		char* sourcePtr = _input;
		bool* fastTablePtr = _fastTable;

		int i = 0;

		while (sourcePtr[i] != 0)
		{
			fastTablePtr[i] = false;

			if (sourcePtr[i] == _oldValue[0])
			{
				char* tempSourcePtr = &sourcePtr[i];
				char* tempOldValuePtr = _oldValue;
				bool isMatch = true;

				while (isMatch &&
						*tempSourcePtr != 0 &&
						*tempOldValuePtr != 0)
				{
					if (*tempSourcePtr != *tempOldValuePtr) isMatch = false;

					tempSourcePtr++;
					tempOldValuePtr++;
				}

				if (isMatch)
				{
					fastTablePtr[i] = true;
					i += _oldValueLength;

					FoundIndexes++;
					continue;
				}
			}

			i++;
		}
	}
}
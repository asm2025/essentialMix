//using System;
//using asm.Extensions;
//using asm.Threading;

//namespace asm.IO.Parsers
//{
//	public class CsvSettings : LockableBase
//	{
//		private FieldType _fieldType;
//		private int[] _columnWidths;
//		private bool _firstRowSetsExpectedColumnCount;
//		private int _bufferSize = Constants.BUFFER_1KB * 4;
//		private int _expectedColumnCount;
//		private char? _columnDelimiter;
//		private char? _textQualifier;
//		private char? _escapeCharacter;
//		private char? _commentCharacter;
//		private bool _hasHeaders;
//		private bool _trimResults;
//		private bool _stripControlChars;
//		private bool _skipEmptyRows = true;

//		public FieldType FieldType
//		{
//			get => _fieldType;
//			set
//			{
//				ThrowIfLocked();
//				_fieldType = value;

//				if (_fieldType == FieldType.FixedWidth)
//				{
//					_columnDelimiter = null;
//					_firstRowSetsExpectedColumnCount = false;
//				}
//				else
//				{
//					_columnWidths = null;
//				}
//			}
//		}

//		public int[] ColumnWidths
//		{
//			get => _columnWidths;
//			set
//			{
//				ThrowIfLocked();
//				if (value != null && value.Length < 1) throw new ArgumentOutOfRangeException(nameof(value), nameof(ColumnWidths) + " can either be null or an array with elements.");

//				if (value == null)
//				{
//					_columnWidths = null;
//					_fieldType = FieldType.Delimited;
//					_expectedColumnCount = 0;
//				}
//				else
//				{
//					// Make sure all of the ColumnWidths are valid.
//					if (value.Any(e => e < 1)) throw new ArgumentOutOfRangeException(nameof(value), nameof(ColumnWidths) + " cannot contain a number less than one.");
//					_columnWidths = value;
//					_fieldType = FieldType.FixedWidth;
//					_expectedColumnCount = _columnWidths.Length;
//				}
//			}
//		}

//		public bool FirstRowSetsExpectedColumnCount
//		{
//			get => _firstRowSetsExpectedColumnCount;
//			set
//			{
//				ThrowIfLocked();
//				_firstRowSetsExpectedColumnCount = value;
//				if (_firstRowSetsExpectedColumnCount) _fieldType = FieldType.Delimited;
//			}
//		}

//		public int BufferSize
//		{
//			get => _bufferSize;
//			set
//			{
//				ThrowIfLocked();
//				_bufferSize = value.NotBelow(Constants.BUFFER_1KB);
//			}
//		}

//		public int ExpectedColumnCount
//		{
//			get => _expectedColumnCount;
//			set
//			{
//				ThrowIfLocked();
//				_expectedColumnCount = value;
//			}
//		}

//		public char? ColumnDelimiter
//		{
//			get => _columnDelimiter;
//			set
//			{
//				ThrowIfLocked();
//				_columnDelimiter = value;
//				_fieldType = _columnDelimiter == null ? FieldType.FixedWidth : FieldType.Delimited;
//			}
//		}

//		public char? TextQualifier
//		{
//			get => _textQualifier;
//			set
//			{
//				ThrowIfLocked();
//				_textQualifier = value;
//			}
//		}

//		public char? EscapeCharacter
//		{
//			get => _escapeCharacter;
//			set
//			{
//				ThrowIfLocked();
//				_escapeCharacter = value;
//			}
//		}

//		public char? CommentCharacter
//		{
//			get => _commentCharacter;
//			set
//			{
//				ThrowIfLocked();
//				_commentCharacter = value;
//			}
//		}

//		public bool HasHeaders
//		{
//			get => _hasHeaders;
//			set
//			{
//				ThrowIfLocked();
//				_hasHeaders = value;
//			}
//		}

//		public bool TrimResults
//		{
//			get => _trimResults;
//			set
//			{
//				ThrowIfLocked();
//				_trimResults = value;
//			}
//		}

//		public bool StripControlChars
//		{
//			get => _stripControlChars;
//			set
//			{
//				ThrowIfLocked();
//				_stripControlChars = value;
//			}
//		}

//		public bool SkipEmptyRows
//		{
//			get => _skipEmptyRows;
//			set
//			{
//				ThrowIfLocked();
//				_skipEmptyRows = value;
//			}
//		}

//		internal void Lock() { IsLocked = true; }
//		internal void UnLock() { IsLocked = false; }
//	}
//}
using System;
using System.Globalization;
using System.Reflection;

namespace essentialMix
{
	public static class Constants
	{
		public const int N_DATA_BATCH_MAX = 100;

		public const int BUFFER_KB = 1024;
		public const int BUFFER_2_KB = BUFFER_KB * 2;
		public const int BUFFER_4_KB = BUFFER_KB * 4;
		public const int BUFFER_8_KB = BUFFER_KB * 8;
		public const int BUFFER_16_KB = BUFFER_KB * 16;
		public const int BUFFER_32_KB = BUFFER_KB * 32;
		public const int BUFFER_64_KB = BUFFER_KB * 64;
		public const int BUFFER_128_KB = BUFFER_KB * 128;
		public const int BUFFER_256_KB = BUFFER_KB * 256;
		public const int BUFFER_MB = BUFFER_KB * BUFFER_KB;
		public const int BUFFER_2_MB = BUFFER_MB * 2;
		public const int BUFFER_4_MB = BUFFER_MB * 4;
		public const int BUFFER_8_MB = BUFFER_MB * 8;
		public const int BUFFER_16_MB = BUFFER_MB * 16;
		public const int BUFFER_32_MB = BUFFER_MB * 32;
		public const int BUFFER_64_MB = BUFFER_MB * 64;
		public const int BUFFER_128_MB = BUFFER_MB * 128;
		public const int BUFFER_256_MB = BUFFER_MB * 256;

		public const int DEFAULT_CAPACITY = 4;

		public const char CR = '\r';
		public const char LF = '\n';
		public const char CHAR_NULL = (char)0;

		public const BindingFlags BINDING_FLAGS = BindingFlags.IgnoreCase;
		public const BindingFlags BF_PUBLIC = BINDING_FLAGS | BindingFlags.Public;
		public const BindingFlags BF_NON_PUBLIC = BINDING_FLAGS | BindingFlags.NonPublic;
		public const BindingFlags BF_PUBLIC_NON_PUBLIC = BINDING_FLAGS | BindingFlags.Public | BindingFlags.NonPublic;
		public const BindingFlags BF_PUBLIC_INSTANCE = BF_PUBLIC | BindingFlags.Instance;
		public const BindingFlags BF_NON_PUBLIC_INSTANCE = BF_NON_PUBLIC | BindingFlags.Instance;
		public const BindingFlags BF_PUBLIC_NON_PUBLIC_INSTANCE = BF_PUBLIC_NON_PUBLIC | BindingFlags.Instance;
		public const BindingFlags BF_PUBLIC_STATIC = BF_PUBLIC | BindingFlags.Static;
		public const BindingFlags BF_NON_PUBLIC_STATIC = BF_NON_PUBLIC | BindingFlags.Static;
		public const BindingFlags BF_PUBLIC_NON_PUBLIC_STATIC = BF_PUBLIC_NON_PUBLIC | BindingFlags.Static;
		public const BindingFlags BF_PUBLIC_INSTANCE_STATIC = BF_PUBLIC | BindingFlags.Instance | BindingFlags.Static;
		public const BindingFlags BF_NON_PUBLIC_INSTANCE_STATIC = BF_NON_PUBLIC | BindingFlags.Instance | BindingFlags.Static;
		public const BindingFlags BF_PUBLIC_NON_PUBLIC_INSTANCE_STATIC = BF_PUBLIC_NON_PUBLIC | BindingFlags.Instance | BindingFlags.Static;

		public const StringSplitOptions SPLIT_OPTIONS = StringSplitOptions.RemoveEmptyEntries;

		public const DateTimeStyles DATE_TIME_FORMATS_STYLES = DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeLocal | DateTimeStyles.RoundtripKind | DateTimeStyles.NoCurrentDateDefault;

		public const int TIMEOUT_MINIMUM = 100;

		public const int REMOTE_CONNECTION_CLOSED = -2147023667;

		public const int CHAR_SIZE = sizeof(char);
		public const int CHAR_BIT_SIZE = CHAR_SIZE * 8;
		public const int BOOL_SIZE = sizeof(bool);
		public const int BOOL_BIT_SIZE = BOOL_SIZE * 8;
		public const int SHORT_SIZE = sizeof(short);
		public const int SHORT_BIT_SIZE = SHORT_SIZE * 8;
		public const int USHORT_SIZE = sizeof(ushort);
		public const int USHORT_BIT_SIZE = USHORT_SIZE * 8;
		public const int INT_24_SIZE = INT_SIZE - 1;
		public const int INT_24_BIT_SIZE = INT_24_SIZE * 8;
		public const int INT_SIZE = sizeof(int);
		public const int INT_BIT_SIZE = INT_SIZE * 8;
		public const int UINT_SIZE = sizeof(uint);
		public const int UINT_BIT_SIZE = UINT_SIZE * 8;
		public const int LONG_SIZE = sizeof(long);
		public const int LONG_BIT_SIZE = LONG_SIZE * 8;
		public const int ULONG_SIZE = sizeof(ulong);
		public const int ULONG_BIT_SIZE = ULONG_SIZE * 8;
		public const int FLOAT_SIZE = sizeof(float);
		public const int FLOAT_BIT_SIZE = FLOAT_SIZE * 8;
		public const int DOUBLE_SIZE = sizeof(double);
		public const int DOUBLE_BIT_SIZE = DOUBLE_SIZE * 8;
		public const int DECIMAL_SIZE = sizeof(decimal);
		public const int DECIMAL_BIT_SIZE = DECIMAL_SIZE * 8;

		public const string NULL = "<null>";
		public const int INDENT = 2;

		public static int GetBufferKB(ushort value) { return BUFFER_KB * value; }

		public static int GetBufferMB(ushort value) { return BUFFER_MB * value; }
	}
}

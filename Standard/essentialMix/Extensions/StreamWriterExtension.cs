using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;

namespace essentialMix.Extensions
{
	public static class StreamWriterExtension
	{
		[NotNull]
		public static StreamWriter Write([NotNull] this StreamWriter thisValue, [NotNull] params char[] values)
		{
			if (values.Length == 0) return thisValue;

			foreach (char c in values)
				thisValue.Write(c);

			return thisValue;
		}

		[NotNull]
		public static StreamWriter Write([NotNull] this StreamWriter thisValue, [NotNull] params string[] values)
		{
			foreach (string value in values.Where(e => !string.IsNullOrEmpty(e)))
				thisValue.Write(value);

			return thisValue;
		}

		[NotNull]
		public static StreamWriter Write([NotNull] this StreamWriter thisValue, [NotNull] params object[] values)
		{
			foreach (object value in values.Where(e => !e.IsNull()))
				thisValue.Write(value);

			return thisValue;
		}

		[NotNull]
		public static StreamWriter Write<TKey, TValue>([NotNull] this StreamWriter thisValue, KeyValuePair<TKey, TValue> pair)
		{
			thisValue.Write(pair.ToString('='));
			return thisValue;
		}

		[NotNull]
		public static StreamWriter Write([NotNull] this StreamWriter thisValue, DictionaryEntry pair)
		{
			thisValue.Write(pair.ToString('='));
			return thisValue;
		}

		[NotNull]
		public static StreamWriter Write<TKey, TValue>([NotNull] this StreamWriter thisValue, KeyValuePair<TKey, TValue> pair, [NotNull] string format)
		{
			thisValue.Write(pair.Format(format));
			return thisValue;
		}

		[NotNull]
		public static StreamWriter Write([NotNull] this StreamWriter thisValue, DictionaryEntry pair, [NotNull] string format)
		{
			thisValue.Write(pair.Format(format));
			return thisValue;
		}

		[NotNull]
		public static StreamWriter WriteWithLine([NotNull] this StreamWriter thisValue, string value)
		{
			if (string.IsNullOrEmpty(value)) return thisValue;
			if (thisValue.Length() > 0) thisValue.WriteLine();
			thisValue.Write(value);
			return thisValue;
		}

		/// <summary>
		/// Will cause the StreamWriter to flush.
		/// </summary>
		/// <param name="thisValue">The writer.</param>
		/// <returns></returns>
		public static long Position([NotNull] this StreamWriter thisValue)
		{
			thisValue.Flush();
			return thisValue.BaseStream.Position;
		}

		/// <summary>
		/// Will cause the StreamWriter to flush.
		/// </summary>
		/// <param name="thisValue">The writer.</param>
		/// <returns></returns>
		public static long Length([NotNull] this StreamWriter thisValue)
		{
			thisValue.Flush();
			return thisValue.BaseStream.Length;
		}

		public static int WriteLines<T>([NotNull] this StreamWriter thisValue, [NotNull] IReadOnlyCollection<T> lines, ReaderWriterLockSlim writerLock = null)
		{
			if (lines == null) throw new ArgumentNullException(nameof(lines));
			if (lines.Count == 0) return 0;

			int result = 0;
			writerLock?.EnterWriteLock();

			try
			{
				if (lines.Count == 1)
				{
					thisValue.Write(lines.First());
					result++;
				}
				else
				{
					foreach (T line in lines.Take(lines.Count - 1))
					{
						thisValue.WriteLine(line);
						result++;
					}

					thisValue.Write(lines.Last());
					result++;
				}
			}
			catch
			{
				result = -1;
			}
			finally
			{
				writerLock?.ExitWriteLock();
			}

			return result;
		}

		public static int WriteLines<T>([NotNull] this StreamWriter thisValue, [NotNull] ICollection<T> lines, ReaderWriterLockSlim writerLock = null)
		{
			if (lines == null) throw new ArgumentNullException(nameof(lines));
			if (lines.Count == 0) return 0;

			int result = 0;
			writerLock?.EnterWriteLock();

			try
			{
				if (lines.Count == 1)
				{
					thisValue.Write(lines.First());
					result++;
				}
				else
				{
					foreach (T line in lines.Take(lines.Count - 1))
					{
						thisValue.WriteLine(line);
						result++;
					}

					thisValue.Write(lines.Last());
					result++;
				}
			}
			catch
			{
				result = -1;
			}
			finally
			{
				writerLock?.ExitWriteLock();
			}

			return result;
		}

		public static bool WriteWithLock<T>([NotNull] this StreamWriter thisValue, T value, ReaderWriterLockSlim writerLock = null)
		{
			if (value == null) return false;
			writerLock?.EnterWriteLock();

			try
			{
				thisValue.Write(value);
				return true;
			}
			catch
			{
				return false;
			}
			finally
			{
				writerLock?.ExitWriteLock();
			}
		}

		public static bool WriteLineWithLock<T>([NotNull] this StreamWriter thisValue, T value, ReaderWriterLockSlim writerLock = null)
		{
			if (value == null) return false;
			writerLock?.EnterWriteLock();

			try
			{
				thisValue.WriteLine(value);
				return true;
			}
			catch
			{
				return false;
			}
			finally
			{
				writerLock?.ExitWriteLock();
			}
		}
	}
}
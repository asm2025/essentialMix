using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using asm.Extensions;
using asm.Other.Microsoft.MultiLanguage;
using JetBrains.Annotations;

namespace asm.Helpers
{
	public static class EncodingHelper
	{
		static EncodingHelper()
		{
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
			EncodingInfo[] encodings = Encoding.GetEncodings();
			SystemEncodingCount = encodings.Length;

			IList<EncodingInfo> streamEncodings = new List<EncodingInfo>();
			IList<EncodingInfo> mimeEncodings = new List<EncodingInfo>();
			IList<EncodingInfo> allEncodings = new List<EncodingInfo>();

			// ASCII - most simple so put it in first place...
			EncodingInfo encodingInfo = Encoding.ASCII.EncodingInfo();
			streamEncodings.Add(encodingInfo);
			mimeEncodings.Add(encodingInfo);
			allEncodings.Add(encodingInfo);

			// add default 2nd for all encodings
			encodingInfo = Default.EncodingInfo();
			allEncodings.Add(encodingInfo);

			// default is single byte?
			if (Default.IsSingleByte)
			{
				// put it in second place
				streamEncodings.Add(encodingInfo);
				mimeEncodings.Add(encodingInfo);
			}

			// prefer JIS over JIS-SHIFT (JIS is detected better than JIS-SHIFT)
			// this one does include Cyrillic (strange but true)
			encodingInfo = Encoding.GetEncoding(50220).EncodingInfo();
			allEncodings.Add(encodingInfo);
			mimeEncodings.Add(encodingInfo);

			// always allow Unicode flavors for streams (they all have a preamble)
			streamEncodings.Add(Encoding.Unicode.EncodingInfo());

			foreach (EncodingInfo enc in encodings)
			{
				if (streamEncodings.Contains(enc)) continue;
				Encoding encoding = Encoding.GetEncoding(enc.CodePage);
				if (encoding.GetPreamble().Length == 0) continue;
				streamEncodings.Add(enc);
			}

			PreferredStreamEncodings = streamEncodings.ToArray();
			PreferredStreamEncodingCodePages = streamEncodings.Select(e => e.CodePage).ToArray();

			// all single byte encodings
			foreach (EncodingInfo enc in encodings)
			{
				if (!enc.GetEncoding().IsSingleByte || allEncodings.Contains(enc)) continue;
				allEncodings.Add(enc);

				// only add ISO and IBM encodings to mime encodings 
				if (enc.CodePage > 1258) continue;
				mimeEncodings.Add(enc);
			}

			// add the rest (multi-byte)
			foreach (EncodingInfo enc in encodings)
			{
				if (enc.GetEncoding().IsSingleByte || allEncodings.Contains(enc)) continue;
				allEncodings.Add(enc);

				// only add ISO and IBM encodings to mime encodings 
				if (enc.CodePage > 1258) continue;
				mimeEncodings.Add(enc);
			}

			// add Unicode
			mimeEncodings.Add(Encoding.Unicode.EncodingInfo());

			// this contains all code pages, sorted by preference and byte usage 
			PreferredMimeEncodings = mimeEncodings.ToArray();
			PreferredMimeEncodingCodePages = mimeEncodings.Select(e => e.CodePage).ToArray();

			// this contains all code pages, sorted by preference and byte usage 
			Encodings = allEncodings.ToArray();
			EncodingCodePages = allEncodings.Select(e => e.CodePage).ToArray();
		}

		[NotNull]
		public static Encoding Default => Encoding.Default;
			
		public static EncodingInfo[] Encodings { get; }
		public static int[] EncodingCodePages { get; }

		public static EncodingInfo[] PreferredMimeEncodings { get; }
		public static int[] PreferredMimeEncodingCodePages { get; }

		public static EncodingInfo[] PreferredEncodings => PreferredMimeEncodings;
		public static int[] PreferredEncodingCodePages => PreferredMimeEncodingCodePages;

		public static EncodingInfo[] PreferredStreamEncodings { get; }
		public static int[] PreferredStreamEncodingCodePages { get; }

		public static int SystemEncodingCount { get; }

		[NotNull]
		public static Encoding GetEncoding(string input) { return FindEncoding(input, EncodingCodePages); }

		[NotNull]
		public static Encoding GetEncoding(string input, bool preserveOrder) { return FindEncoding(input, EncodingCodePages, preserveOrder); }

		[NotNull]
		public static Encoding GetEncoding(char[] input)
		{
			return input.IsNullOrEmpty()
						? Default
						: FindEncoding(new string(input), EncodingCodePages);
		}

		public static Encoding GetEncoding(byte[] input)
		{
			try
			{
				Encoding[] detected = GetEncodings(input, 1);
				return detected.Length > 0 ? detected[0] : Default;
			}
			catch (COMException)
			{
				// return default code page on error
				return Default;
			}
		}

		[NotNull]
		public static Encoding[] GetEncodings(string input) { return FindEncodings(input, EncodingCodePages, true); }

		[NotNull]
		public static Encoding[] GetEncodings(string input, bool preserveOrder) { return FindEncodings(input, EncodingCodePages, preserveOrder); }

		[NotNull]
		public static Encoding[] GetEncodings(byte[] input, int maxEncodings)
		{
			if (input.IsNullOrEmpty())
			{
				return new[]
						{
							Default
						};
			}

			if (maxEncodings < 1) maxEncodings = 1;

			// expand the string to be at least 256 bytes
			if (input.Length < 256)
			{
				byte[] newInput = new byte[256];
				int steps = 256 / input.Length;

				for (int i = 0; i < steps; i++)
					Array.Copy(input, 0, newInput, input.Length * i, input.Length);

				int rest = 256 % input.Length;
				if (rest > 0) Array.Copy(input, 0, newInput, steps * input.Length, rest);
				input = newInput;
			}

			List<Encoding> result = new List<Encoding>();

			// get the IMultiLanguage" interface
			IMultiLanguage2 multiLang2 = new CMultiLanguageClass();
			if (multiLang2 == null) throw new COMException("Failed to get " + nameof(IMultiLanguage2));

			try
			{
				DetectEncodingInfo[] detectedEncodings = new DetectEncodingInfo[maxEncodings];
				int scores = detectedEncodings.Length;
				int srcLen = input.Length;

				// finally... call to DetectInputCodepage
				multiLang2.DetectInputCodepage(MLDETECTCP.MLDETECTCP_NONE, 0, ref input[0], ref srcLen, ref detectedEncodings[0], ref scores);

				// get result
				if (scores > 0)
				{
					for (int i = 0; i < scores; i++)
						result.Add(Encoding.GetEncoding((int)detectedEncodings[i].nCodePage));
				}
			}
			finally
			{
				Marshal.FinalReleaseComObject(multiLang2);
			}

			return result.ToArray();
		}

		[NotNull]
		public static Encoding GetPreferredEncoding(string input) { return FindEncoding(input, PreferredEncodingCodePages); }

		[NotNull]
		public static Encoding GetPreferredEncoding(string input, bool preserveOrder) { return FindEncoding(input, PreferredEncodingCodePages, preserveOrder); }

		[NotNull]
		public static Encoding[] GetPreferredEncodings(string input) { return FindEncodings(input, PreferredEncodingCodePages, true); }

		[NotNull]
		public static Encoding[] GetPreferredEncodings(string input, bool preserveOrder) { return FindEncodings(input, PreferredEncodingCodePages, preserveOrder); }

		[NotNull]
		public static Encoding GetPreferredStreamEncoding(string input) { return FindEncoding(input, PreferredStreamEncodingCodePages); }

		[NotNull]
		public static Encoding GetPreferredStreamEncoding(string input, bool preserveOrder) { return FindEncoding(input, PreferredStreamEncodingCodePages, preserveOrder); }

		[NotNull]
		public static Encoding[] GetPreferredStreamEncodings(string input) { return FindEncodings(input, PreferredStreamEncodingCodePages, true); }

		[NotNull]
		public static Encoding[] GetPreferredStreamEncodings(string input, bool preserveOrder) { return FindEncodings(input, PreferredStreamEncodingCodePages, preserveOrder); }

		[NotNull]
		private static Encoding FindEncoding(string input, int[] preferredEncodings)
		{
			Encoding enc = FindEncoding(input, preferredEncodings, true);
			if (enc.CodePage != Encoding.Unicode.CodePage) return enc;

			// Unicode.. hmmm... check for smallest encoding
			int byteCount = Encoding.UTF7.GetByteCount(input);
			int bestByteCount = byteCount;

			enc = Encoding.UTF7;

			// utf8 smaller?
			byteCount = Encoding.UTF8.GetByteCount(input);

			if (byteCount < bestByteCount)
			{
				enc = Encoding.UTF8;
				bestByteCount = byteCount;
			}

			// Unicode smaller?
			byteCount = Encoding.Unicode.GetByteCount(input);

			if (byteCount < bestByteCount) enc = Encoding.Unicode;
			return enc;
		}

		[NotNull]
		private static Encoding FindEncoding(string input, int[] preferredEncodings, bool preserveOrder)
		{
			// empty strings can always be encoded as ASCII
			if (string.IsNullOrEmpty(input)) return Default;

			bool bPrefEnc = !preferredEncodings.IsNullOrEmpty();
			Encoding result = Default;

			// get the IMultiLanguage3 interface
			IMultiLanguage3 multiLang3 = new CMultiLanguageClass();
			if (multiLang3 == null) throw new COMException("Failed to get " + nameof(IMultiLanguage3));

			try
			{
				int count = bPrefEnc ? preferredEncodings.Length : SystemEncodingCount;
				int[] resultCodePages = new int[count];
				uint detectedCodePages = (uint)resultCodePages.Length;
				ushort specialChar = '?';

				// get unmanaged arrays
				IntPtr preferred = bPrefEnc ? Marshal.AllocCoTaskMem(sizeof(uint) * preferredEncodings.Length) : IntPtr.Zero;
				IntPtr detected = Marshal.AllocCoTaskMem(sizeof(uint) * resultCodePages.Length);

				try
				{
					if (bPrefEnc) Marshal.Copy(preferredEncodings, 0, preferred, preferredEncodings.Length);

					Marshal.Copy(resultCodePages, 0, detected, resultCodePages.Length);
					MLCPF options = MLCPF.MLDETECTF_VALID_NLS;

					if (preserveOrder) options |= MLCPF.MLDETECTF_PRESERVE_ORDER;

					if (bPrefEnc) options |= MLCPF.MLDETECTF_PREFERRED_ONLY;

					multiLang3.DetectOutboundCodePage(options,
						input,
						(uint)input.Length,
						preferred,
						(uint)(bPrefEnc ? preferredEncodings.Length : 0),
						detected,
						ref detectedCodePages,
						ref specialChar);

					// get result
					if (detectedCodePages > 0)
					{
						int[] theResult = new int[detectedCodePages];
						Marshal.Copy(detected, theResult, 0, theResult.Length);
						result = Encoding.GetEncoding(theResult[0]);
					}
				}
				finally
				{
					if (!preferred.IsZero()) Marshal.FreeCoTaskMem(preferred);
					Marshal.FreeCoTaskMem(detected);
				}
			}
			finally
			{
				Marshal.FinalReleaseComObject(multiLang3);
			}

			return result;
		}

		[NotNull]
		private static Encoding[] FindEncodings(string input, int[] preferredEncodings, bool preserveOrder)
		{
			// empty strings can always be encoded as ASCII
			if (string.IsNullOrEmpty(input))
			{
				return new[]
						{
							Default
						};
			}

			bool bPrefEnc = !preferredEncodings.IsNullOrEmpty();
			List<Encoding> result = new List<Encoding>();

			// get the IMultiLanguage3 interface
			IMultiLanguage3 multiLang3 = new CMultiLanguageClass();
			if (multiLang3 == null) throw new COMException("Failed to get " + nameof(IMultiLanguage3));

			try
			{
				int count = bPrefEnc ? preferredEncodings.Length : SystemEncodingCount;
				int[] resultCodePages = new int[count];
				uint detectedCodePages = (uint)resultCodePages.Length;
				ushort specialChar = '?';

				// get unmanaged arrays
				IntPtr preferred = bPrefEnc ? Marshal.AllocCoTaskMem(sizeof(uint) * preferredEncodings.Length) : IntPtr.Zero;
				IntPtr detected = Marshal.AllocCoTaskMem(sizeof(uint) * resultCodePages.Length);

				try
				{
					if (bPrefEnc) Marshal.Copy(preferredEncodings, 0, preferred, preferredEncodings.Length);

					Marshal.Copy(resultCodePages, 0, detected, resultCodePages.Length);
					MLCPF options = MLCPF.MLDETECTF_VALID_NLS;
					if (preserveOrder) options |= MLCPF.MLDETECTF_PRESERVE_ORDER;
					if (bPrefEnc) options |= MLCPF.MLDETECTF_PREFERRED_ONLY;

					// finally... call to DetectOutboundCodePage
					multiLang3.DetectOutboundCodePage(options,
						input,
						(uint)input.Length,
						preferred,
						(uint)(bPrefEnc ? preferredEncodings.Length : 0),
						detected,
						ref detectedCodePages,
						ref specialChar);

					// get result
					if (detectedCodePages > 0)
					{
						int[] theResult = new int[detectedCodePages];
						Marshal.Copy(detected, theResult, 0, theResult.Length);

						// get the encodings for the code pages
						for (int i = 0; i < detectedCodePages; i++) result.Add(Encoding.GetEncoding(theResult[i]));
					}
				}
				finally
				{
					if (!preferred.IsZero()) Marshal.FreeCoTaskMem(preferred);
					Marshal.FreeCoTaskMem(detected);
				}
			}
			finally
			{
				Marshal.FinalReleaseComObject(multiLang3);
			}

			return result.ToArray();
		}
	}
}
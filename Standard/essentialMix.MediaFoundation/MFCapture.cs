using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using essentialMix.Extensions;
using essentialMix.Helpers;
using essentialMix.MediaFoundation.Alt;
using essentialMix.MediaFoundation.IO;
using essentialMix.MediaFoundation.Transform;
using JetBrains.Annotations;

namespace essentialMix.MediaFoundation
{
	public class MFCapture : COMBase, IMFSourceReaderCallback
	{
		private const int TARGET_BIT_RATE = 240 * 1000;

		private static readonly ISet<Guid> __acceptedSubTypes = new HashSet<Guid>
		{
			MFMediaType.NV12,
			MFMediaType.YUY2,
			MFMediaType.UYVY,
			MFMediaType.RGB32,
			MFMediaType.RGB24,
			MFMediaType.IYUV
		};

		private object _syncRoot;
		private CancellationTokenSource _ctx;
		private IMFSourceReaderAsync _reader;
		private IMFByteStream _byteStream;
		private IMFSinkWriter _writer;
		private bool _firstSample;
		private long _baseTime;

		private volatile int _isCapturing;

		/// <inheritdoc />
		public MFCapture()
		{
		}

		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			if (disposing) Stop();
			base.Dispose(disposing);
		}

		public object SyncRoot
		{
			get
			{
				if (_syncRoot == null) Interlocked.CompareExchange(ref _syncRoot, new object(), null);
				return _syncRoot;
			}
		}

		public bool IsCapturing
		{
			get
			{
				Thread.MemoryBarrier();
				return _isCapturing != 0;
			}
			private set => Interlocked.CompareExchange(ref _isCapturing, value ? 1 : 0, _isCapturing);
		}

		public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(3);

		private CancellationToken Token { get; set; }

		public int Start([NotNull] IMFActivate activator) 
		{
			return Start(activator, new EncodingParameters
			{
				subtype = MFMediaType.H264,
				bitrate = TARGET_BIT_RATE
			});
		}

		public int Start([NotNull] IMFActivate activator, EncodingParameters parameters)
		{
			ThrowIfDisposed();

			if (Token.CanBeCanceled)
			{
				bool lockTaken = false;

				if (!SpinWait.SpinUntil(() =>
				{
					lockTaken = Monitor.TryEnter(SyncRoot, TimeSpanHelper.MINIMUM);
					return lockTaken || Token.IsCancellationRequested;
				}, Timeout))
				{
					return ResultCom.WS_E_ENDPOINT_TOO_BUSY;
				}

				if (Token.IsCancellationRequested)
				{
					if (lockTaken) Monitor.Exit(SyncRoot);
					return ResultCom.E_ABORT;
				}
			}
			else
			{
				if (!Monitor.TryEnter(SyncRoot, Timeout)) 
					return ResultCom.WS_E_ENDPOINT_TOO_BUSY;
			}

			if (IsCapturing) return ResultCom.WS_E_ENDPOINT_TOO_BUSY;
			IsCapturing = true;

			int hr = ResultCom.S_OK;

			try
			{
				InitializeCancellationTokenSource();

				// Create the media source for the device.
				hr = activator.ActivateObject(typeof(IMFMediaSource).GUID, out object sourceObj);
				if (ResultCom.Failed(hr)) return hr;

				IMFMediaSource source = (IMFMediaSource)sourceObj;
				hr = OpenMediaSource(source);
				if (ResultCom.Failed(hr)) return hr;

				// Create the sink writer
				hr = MFAPI.MFCreateAttributes(out IMFAttributes attributes, 1);
				if (ResultCom.Failed(hr)) return hr;
				hr = attributes.SetGUID(MFAttributesClsid.MF_TRANSCODE_CONTAINERTYPE, MFTranscodeContainerType.MPEG4);
				if (ResultCom.Failed(hr)) return hr;

				// todo remove this and send the bytes
				hr = MFAPI.MFCreateTempFile(MFFileAccessMode.ReadWrite, MFFileOpenMode.DeleteIfExist, MFFileFlags.None, out _byteStream);
				if (ResultCom.Failed(hr)) return hr;

				/*
				 * tracing the temp file name/path.
				 * Casting from a COM object to another COM interface will cause .Net to marshal the interface
				 * and call C++ QueryInterface. The returned type MUST be checked for null in case it does not support it.
				 */
				// ReSharper disable once SuspiciousTypeConversion.Global
				IMFAttributes bsAttributes = (IMFAttributes)_byteStream;
				
				if (bsAttributes != null && ResultCom.Succeeded(MFAPI.MFGetAttributeString(bsAttributes, MFAttributesClsid.MF_BYTESTREAM_ORIGIN_NAME, out string tempPath)))
					Trace($"IMFByteStreamPath: '{tempPath}'");

				hr = MFAPI.MFCreateSinkWriterFromURL(null, _byteStream, attributes, out _writer);
				if (ResultCom.Failed(hr)) return hr;
				// remove this and send the bytes
				
				
				hr = ConfigureCapture(_reader, _writer, parameters);
				if (ResultCom.Failed(hr)) return hr;
				_firstSample = true;
				_baseTime = 0;
				// Request the first video frame.
				hr = _reader.ReadSample(MFSourceReader.FIRST_VIDEO_STREAM, MF_SOURCE_READER_CONTROL_FLAG.None, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
			}
			finally
			{
				if (ResultCom.Failed(hr)) Stop();
				Monitor.Exit(SyncRoot);
			}

			return hr;
		}

		public void Stop()
		{
			ThrowIfDisposed();
			Cancel();
			IsCapturing = false;
			// Sleep this thread so the cancellation can take effect
			Thread.Sleep(0);

			if (_writer != null)
			{
				_writer.Finalize_();
				ObjectHelper.MarshalDispose(ref _writer);
			}

			if (_byteStream != null) ObjectHelper.MarshalDispose(ref _byteStream);
			if (_reader != null) ObjectHelper.MarshalDispose(ref _reader);
			_firstSample = false;
			_baseTime = -1;
			ReleaseCancellationTokenSource();
		}

		/// <inheritdoc />
		int IMFSourceReaderCallback.OnReadSample(int hrStatus, int dwStreamIndex, MF_SOURCE_READER_FLAG dwStreamFlags, long llTimestamp, IMFSample pSample)
		{
			if (IsDisposed || Token.IsCancellationRequested) return ResultCom.E_ABORT;

			int hr;

			try
			{
				if (ResultCom.Failed(hrStatus)) return hrStatus;
				if (!IsCapturing) return ResultCom.S_OK;

				if (pSample != null)
				{
					if (_firstSample)
					{
						_baseTime = llTimestamp;
						_firstSample = false;
					}

					//llTimestamp -= _baseTime;
					//hr = pSample.SetSampleTime(llTimestamp);
					//if (ResultCom.Failed(hr)) return hr;
					hr = _writer.WriteSample(0, pSample);
					if (ResultCom.Failed(hr)) return hr;
				}

				if (IsDisposed || Token.IsCancellationRequested) return ResultCom.E_ABORT;
				// Read another sample.
				hr = _reader.ReadSample(MFSourceReader.FIRST_VIDEO_STREAM, 0, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
			}
			catch (Exception ex)
			{
				hr = Marshal.GetHRForException(ex);
			}
			finally
			{
				ObjectHelper.MarshalDispose(ref pSample);
			}

			return hr;
		}

		/// <inheritdoc />
		int IMFSourceReaderCallback.OnFlush(int dwStreamIndex) { return ResultCom.S_OK; }

		/// <inheritdoc />
		int IMFSourceReaderCallback.OnEvent(int dwStreamIndex, IMFMediaEvent pEvent) { return ResultCom.S_OK; }

		[NotNull]
		private CancellationTokenSource InitializeCancellationTokenSource()
		{
			if (_ctx != null) return _ctx;
			Interlocked.CompareExchange(ref _ctx, new CancellationTokenSource(), null);
			Token = _ctx.Token;
			return _ctx;
		}

		private void ReleaseCancellationTokenSource()
		{
			if (_ctx == null) return;
			Interlocked.Exchange(ref _ctx, null);
			Token = CancellationToken.None;
		}

		private void Cancel()
		{
			_ctx.CancelIfNotDisposed();
		}

		private int OpenMediaSource(IMFMediaSource source)
		{
			int hr;
			IMFAttributes attributes = null;

			try
			{
				hr = MFAPI.MFCreateAttributes(out attributes, 2);
				if (ResultCom.Failed(hr)) return hr;
				hr = attributes.SetUnknown(MFAttributesClsid.MF_SOURCE_READER_ASYNC_CALLBACK, this);
				if (ResultCom.Failed(hr)) return hr;
				hr = MFAPI.MFCreateSourceReaderFromMediaSource(source, attributes, out IMFSourceReader reader);
				if (ResultCom.Failed(hr)) return hr;
				// ReSharper disable once SuspiciousTypeConversion.Global
				_reader = (IMFSourceReaderAsync)reader;
			}
			finally
			{
				ObjectHelper.MarshalDispose(ref attributes);
			}

			return hr;
		}

		private static int ConfigureCapture([NotNull] IMFSourceReaderAsync reader, [NotNull] IMFSinkWriter writer, EncodingParameters parameters)
		{
			int hr;
			IMFMediaType type = null;

			try
			{
				hr = ConfigureSourceReader(reader);
				if (ResultCom.Failed(hr)) return hr;
				hr = reader.GetCurrentMediaType(MFSourceReader.FIRST_VIDEO_STREAM, out type);
				if (ResultCom.Failed(hr)) return hr;
				hr = ConfigureEncoder(parameters, type, writer, out int sinkStream);
				if (ResultCom.Failed(hr)) return hr;
				/*
				 * Register the color converter DSP for this process, in the video
				 * processor category. This will enable the sink writer to enumerate
				 * the color converter when the sink writer attempts to match the
				 * media types.
				 */
				hr = MFAPI.MFTRegisterLocalByCLSID(typeof(CColorConvertDMO).GUID, MFTransformCategory.MFT_CATEGORY_VIDEO_PROCESSOR, 
													string.Empty, MFT_EnumFlag.SyncMFT, 0, null, 0, null);
				if (ResultCom.Failed(hr)) return hr;
				hr = writer.SetInputMediaType(sinkStream, type, null);
				if (ResultCom.Failed(hr)) return hr;
				hr = writer.BeginWriting();
			}
			finally
			{
				ObjectHelper.MarshalDispose(ref type);
			}

			return hr;
		}

		private static int ConfigureSourceReader([NotNull] IMFSourceReaderAsync reader)
		{
			int hr;
			bool useNativeType = false;
			IMFMediaType type = null;

			/*
			 * If the source's native format matches any of the formats in
			 * the list, prefer the native format.
			 *
			 * Note: The camera might support multiple output formats,
			 * including a range of frame dimensions. The application could
			 * provide a list to the user and have the user select the
			 * camera's output format. That is outside the scope of this
			 * sample, however.
			 */
			try
			{
				hr = reader.GetNativeMediaType(MFSourceReader.FIRST_VIDEO_STREAM, 0, out type);
				if (ResultCom.Failed(hr)) return hr;
				hr = type.GetGUID(MFAttributesClsid.MF_MT_SUBTYPE, out Guid subType);
				if (ResultCom.Failed(hr)) return hr;

				if (__acceptedSubTypes.Contains(subType))
				{
					hr = reader.SetCurrentMediaType(MFSourceReader.FIRST_VIDEO_STREAM, null, type);
					if (ResultCom.Failed(hr)) return hr;
					useNativeType = true;
				}

				if (!useNativeType)
				{
					/*
					 * None of the native types worked. The camera might offer
					 * output of a compressed type such as MJPEG or DV.
					 * Try adding a decoder.
					 */
					foreach (Guid acceptedSubType in __acceptedSubTypes)
					{
						hr = type.SetGUID(MFAttributesClsid.MF_MT_SUBTYPE, acceptedSubType);
						if (ResultCom.Failed(hr)) break;
						hr = reader.SetCurrentMediaType(MFSourceReader.FIRST_VIDEO_STREAM, null, type);
						if (ResultCom.Failed(hr)) break;
					}
				}
			}
			finally
			{
				ObjectHelper.MarshalDispose(type);
			}

			return hr;
		}

		private static int ConfigureEncoder(EncodingParameters parameters, [NotNull] IMFMediaType type, [NotNull] IMFSinkWriter writer, out int pdwStreamIndex)
		{
			int hr;
			IMFMediaType type2 = null;
			pdwStreamIndex = -1;

			try
			{
				hr = MFAPI.MFCreateMediaType(out type2);
				if (ResultCom.Failed(hr)) return hr;
				type2.SetGUID(MFAttributesClsid.MF_MT_MAJOR_TYPE, MFMediaType.Video);
				type2.SetGUID(MFAttributesClsid.MF_MT_SUBTYPE, parameters.subtype);
				if (parameters.bitrate > 0) type2.SetUINT32(MFAttributesClsid.MF_MT_AVG_BITRATE, parameters.bitrate);
				type2.SetUINT32(MFAttributesClsid.CODECAPI_AVEncCommonQuality, 70);
				type2.SetUINT32(MFAttributesClsid.CODECAPI_AVEncCommonRateControlMode, (int)eAVEncCommonRateControlMode.eAVEncCommonRateControlMode_Quality);
				type2.SetUINT32(MFAttributesClsid.CODECAPI_AVEncCommonQualityVsSpeed, 50);
				//type2.SetUINT32(MFAttributesClsid.CODECAPI_AVEncCommonMeanBitRate, TARGET_AVERAGE_BIT_RATE);
				//type2.SetUINT32(MFAttributesClsid.CODECAPI_AVEncCommonMaxBitRate, TARGET_AVERAGE_BIT_RATE);
				//type2.SetUINT32(MFAttributesClsid.CODECAPI_AVEncCommonBufferSize, AVEncCommonBufferSize);
				type2.SetUINT32(MFAttributesClsid.CODECAPI_AVEncMPVGOPSize, 1);
				type2.SetUINT32(MFAttributesClsid.CODECAPI_AVEncVideoEncodeQP, 10);
				type2.SetUINT32(MFAttributesClsid.CODECAPI_AVEncVideoForceKeyFrame, 1);
				type2.SetUINT32(MFAttributesClsid.MF_MT_MPEG2_PROFILE, (int)eAVEncH264VProfile.eAVEncH264VProfile_Base);
				type2.SetUINT32(MFAttributesClsid.MF_MT_ALL_SAMPLES_INDEPENDENT, 1);
				CopyAttribute(type, type2, MFAttributesClsid.MF_MT_FRAME_SIZE);
				CopyAttribute(type, type2, MFAttributesClsid.MF_MT_FRAME_RATE);
				CopyAttribute(type, type2, MFAttributesClsid.MF_MT_PIXEL_ASPECT_RATIO);
				CopyAttribute(type, type2, MFAttributesClsid.MF_MT_INTERLACE_MODE);
				hr = writer.AddStream(type2, out pdwStreamIndex);
			}
			finally
			{
				ObjectHelper.MarshalDispose(ref type2);
			}

			return hr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.ForwardRef)]
		private static int CopyAttribute([NotNull] IMFAttributes source, [NotNull] IMFAttributes target, Guid key)
		{
			Variant variant = new Variant();
			int hr = source.GetItem(key, variant);
			if (ResultCom.Succeeded(hr)) hr = target.SetItem(key, variant);
			return hr;
		}
	}
}
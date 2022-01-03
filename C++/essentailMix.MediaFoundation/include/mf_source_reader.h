#pragma once

#include <stdio.h>
#include <tchar.h>
#include <iostream>
#include <locale>
#include <string>
#include <unordered_set>

#include <rpc.h>
#include <evr.h>
#include <mfapi.h>
#include <mfplay.h>
#include <mfreadwrite.h>
#include <mferror.h>
#include <wmcodecdsp.h>

#pragma comment(lib, "Strmiids.lib")
#pragma comment(lib, "Rpcrt4.lib")

#pragma comment(lib, "mf.lib")
#pragma comment(lib, "mfplat.lib")
#pragma comment(lib, "mfplay.lib")
#pragma comment(lib, "mfreadwrite.lib")
#pragma comment(lib, "mfuuid.lib")
#pragma comment(lib, "wmcodecdspuuid.lib")

namespace essentialMix { namespace MediaFoundation
{
	struct GUID_hasher
	{
		size_t operator() (const GUID& value) const
		{
			RPC_STATUS status = RPC_S_OK;
			return ::UuidHash(&const_cast<GUID&>(value), &status);
		}
	};

	struct GUID_comparator
	{
		bool operator() (const GUID& a, const GUID& b) const
		{
			RPC_STATUS status = RPC_S_OK;
			return ::UuidCompare(&const_cast<GUID&>(a), &const_cast<GUID&>(b), &status) == 0;
		}
	};

	public class MFSourceReader : public IMFSourceReaderCallback
	{
	private:
		IMFMediaSource* _source = nullptr;
		IMFSourceReader* _reader = nullptr;
		LPTSTR _name = nullptr;
		UINT _nameLength {0};
		IMFMediaType* _mediaType = nullptr;
		IUnknown* _transformUnknown = nullptr;
		IMFTransform* _transform = nullptr; // H264 Encoder MFT
		IMFMediaType* _mftInputMediaType = nullptr, * _mftOutputMediaType = nullptr;
		IMFSinkWriter* _writer = nullptr;
		IMFMediaType* _videoOutType = nullptr;
		DWORD _writerVideoStreamIndex {0};
		DWORD _totalSampleBufferSize {0};
		DWORD _mftStatus {0};

		static bool __initialized;
		static std::unordered_set<GUID, GUID_hasher, GUID_comparator> __acceptedTypes;

	public:
		MFSourceReader();
		virtual ~MFSourceReader();

		HRESULT __stdcall OnReadSample(HRESULT hrStatus, DWORD dwStreamIndex, DWORD dwStreamFlags, LONGLONG llTimestamp, IMFSample* pSample) override;
		HRESULT __stdcall OnFlush(DWORD dwStreamIndex) override;
		HRESULT __stdcall OnEvent(DWORD dwStreamIndex, IMFMediaEvent* pEvent) override;

		static HRESULT Initialize();
		static HRESULT Shutdown();
	};
}}

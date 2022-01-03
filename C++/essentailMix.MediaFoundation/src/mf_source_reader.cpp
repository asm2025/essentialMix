#include "pch.h"
#include "utilities/com.h"

#include "mf_source_reader.h"

using namespace essentialMix::utilities;
using namespace essentialMix::MediaFoundation;

bool MFSourceReader::__initialized { false };
std::unordered_set<GUID, GUID_hasher, GUID_comparator> MFSourceReader::__acceptedTypes {
	MFVideoFormat_NV12,
	MFVideoFormat_YUY2,
	MFVideoFormat_UYVY,
	MFVideoFormat_RGB32,
	MFVideoFormat_RGB24,
	MFVideoFormat_IYUV,
	MFVideoFormat_MP4S,
	MFVideoFormat_MP4V
};

MFSourceReader::MFSourceReader()
{
}

MFSourceReader::~MFSourceReader()
{
}

HRESULT MFSourceReader::OnReadSample(HRESULT hrStatus, DWORD dwStreamIndex, DWORD dwStreamFlags, LONGLONG llTimestamp, IMFSample* pSample)
{
	return S_OK;
}

HRESULT MFSourceReader::OnFlush(DWORD dwStreamIndex)
{
	return S_OK;
}

HRESULT MFSourceReader::OnEvent(DWORD dwStreamIndex, IMFMediaEvent* pEvent)
{
	return S_OK;
}

HRESULT MFSourceReader::Initialize()
{
	if (__initialized) return S_OK;
	throw_for_HR(CoInitializeEx(nullptr, COINIT_MULTITHREADED));
	throw_for_HR(MFStartup(MF_VERSION));
	__initialized = true;
	return S_OK;
}

HRESULT MFSourceReader::Shutdown()
{
	if (!__initialized) return S_OK;
	MFShutdown();
	__initialized = false;
	return S_OK;
}

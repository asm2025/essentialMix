#pragma once

#include <codecvt>
#include <fstream>
#include <iostream>
#include <memory>
#include <locale>
#include <mfapi.h>
#include <mferror.h>
#include <mfplay.h>
#include <mfreadwrite.h>
#include <mmdeviceapi.h>
#include <stdio.h>
#include <string>
#include <tchar.h>
#include <wmcodecdsp.h>
#include <wmsdkidl.h>

#include "macros.h"

#pragma comment(lib, "Strmiids.lib")
#pragma comment(lib, "Rpcrt4.lib")

#pragma comment(lib, "mf.lib")
#pragma comment(lib, "mfplat.lib")
#pragma comment(lib, "mfplay.lib")
#pragma comment(lib, "mfreadwrite.lib")
#pragma comment(lib, "mfuuid.lib")
#pragma comment(lib, "wmcodecdspuuid.lib")

inline std::string GetGUIDName(const GUID& guid)
{
	std::string name{};

	NAME_ASSIGN(guid, MF_MT_MAJOR_TYPE, name);
	NAME_ASSIGN(guid, MF_MT_MAJOR_TYPE, name);
	NAME_ASSIGN(guid, MF_MT_SUBTYPE, name);
	NAME_ASSIGN(guid, MF_MT_ALL_SAMPLES_INDEPENDENT, name);
	NAME_ASSIGN(guid, MF_MT_FIXED_SIZE_SAMPLES, name);
	NAME_ASSIGN(guid, MF_MT_COMPRESSED, name);
	NAME_ASSIGN(guid, MF_MT_SAMPLE_SIZE, name);
	NAME_ASSIGN(guid, MF_MT_WRAPPED_TYPE, name);
	NAME_ASSIGN(guid, MF_MT_AUDIO_NUM_CHANNELS, name);
	NAME_ASSIGN(guid, MF_MT_AUDIO_SAMPLES_PER_SECOND, name);
	NAME_ASSIGN(guid, MF_MT_AUDIO_FLOAT_SAMPLES_PER_SECOND, name);
	NAME_ASSIGN(guid, MF_MT_AUDIO_AVG_BYTES_PER_SECOND, name);
	NAME_ASSIGN(guid, MF_MT_AUDIO_BLOCK_ALIGNMENT, name);
	NAME_ASSIGN(guid, MF_MT_AUDIO_BITS_PER_SAMPLE, name);
	NAME_ASSIGN(guid, MF_MT_AUDIO_VALID_BITS_PER_SAMPLE, name);
	NAME_ASSIGN(guid, MF_MT_AUDIO_SAMPLES_PER_BLOCK, name);
	NAME_ASSIGN(guid, MF_MT_AUDIO_CHANNEL_MASK, name);
	NAME_ASSIGN(guid, MF_MT_AUDIO_FOLDDOWN_MATRIX, name);
	NAME_ASSIGN(guid, MF_MT_AUDIO_WMADRC_PEAKREF, name);
	NAME_ASSIGN(guid, MF_MT_AUDIO_WMADRC_PEAKTARGET, name);
	NAME_ASSIGN(guid, MF_MT_AUDIO_WMADRC_AVGREF, name);
	NAME_ASSIGN(guid, MF_MT_AUDIO_WMADRC_AVGTARGET, name);
	NAME_ASSIGN(guid, MF_MT_AUDIO_PREFER_WAVEFORMATEX, name);
	NAME_ASSIGN(guid, MF_MT_AAC_PAYLOAD_TYPE, name);
	NAME_ASSIGN(guid, MF_MT_AAC_AUDIO_PROFILE_LEVEL_INDICATION, name);
	NAME_ASSIGN(guid, MF_MT_FRAME_SIZE, name);
	NAME_ASSIGN(guid, MF_MT_FRAME_RATE, name);
	NAME_ASSIGN(guid, MF_MT_FRAME_RATE_RANGE_MAX, name);
	NAME_ASSIGN(guid, MF_MT_FRAME_RATE_RANGE_MIN, name);
	NAME_ASSIGN(guid, MF_MT_PIXEL_ASPECT_RATIO, name);
	NAME_ASSIGN(guid, MF_MT_DRM_FLAGS, name);
	NAME_ASSIGN(guid, MF_MT_PAD_CONTROL_FLAGS, name);
	NAME_ASSIGN(guid, MF_MT_SOURCE_CONTENT_HINT, name);
	NAME_ASSIGN(guid, MF_MT_VIDEO_CHROMA_SITING, name);
	NAME_ASSIGN(guid, MF_MT_INTERLACE_MODE, name);
	NAME_ASSIGN(guid, MF_MT_TRANSFER_FUNCTION, name);
	NAME_ASSIGN(guid, MF_MT_VIDEO_PRIMARIES, name);
	NAME_ASSIGN(guid, MF_MT_CUSTOM_VIDEO_PRIMARIES, name);
	NAME_ASSIGN(guid, MF_MT_YUV_MATRIX, name);
	NAME_ASSIGN(guid, MF_MT_VIDEO_LIGHTING, name);
	NAME_ASSIGN(guid, MF_MT_VIDEO_NOMINAL_RANGE, name);
	NAME_ASSIGN(guid, MF_MT_GEOMETRIC_APERTURE, name);
	NAME_ASSIGN(guid, MF_MT_MINIMUM_DISPLAY_APERTURE, name);
	NAME_ASSIGN(guid, MF_MT_PAN_SCAN_APERTURE, name);
	NAME_ASSIGN(guid, MF_MT_PAN_SCAN_ENABLED, name);
	NAME_ASSIGN(guid, MF_MT_AVG_BITRATE, name);
	NAME_ASSIGN(guid, MF_MT_AVG_BIT_ERROR_RATE, name);
	NAME_ASSIGN(guid, MF_MT_MAX_KEYFRAME_SPACING, name);
	NAME_ASSIGN(guid, MF_MT_DEFAULT_STRIDE, name);
	NAME_ASSIGN(guid, MF_MT_PALETTE, name);
	NAME_ASSIGN(guid, MF_MT_USER_DATA, name);
	NAME_ASSIGN(guid, MF_MT_AM_FORMAT_TYPE, name);
	NAME_ASSIGN(guid, MF_MT_MPEG_START_TIME_CODE, name);
	NAME_ASSIGN(guid, MF_MT_MPEG2_PROFILE, name);
	NAME_ASSIGN(guid, MF_MT_MPEG2_LEVEL, name);
	NAME_ASSIGN(guid, MF_MT_MPEG2_FLAGS, name);
	NAME_ASSIGN(guid, MF_MT_MPEG_SEQUENCE_HEADER, name);
	NAME_ASSIGN(guid, MF_MT_DV_AAUX_SRC_PACK_0, name);
	NAME_ASSIGN(guid, MF_MT_DV_AAUX_CTRL_PACK_0, name);
	NAME_ASSIGN(guid, MF_MT_DV_AAUX_SRC_PACK_1, name);
	NAME_ASSIGN(guid, MF_MT_DV_AAUX_CTRL_PACK_1, name);
	NAME_ASSIGN(guid, MF_MT_DV_VAUX_SRC_PACK, name);
	NAME_ASSIGN(guid, MF_MT_DV_VAUX_CTRL_PACK, name);
	NAME_ASSIGN(guid, MF_MT_ARBITRARY_HEADER, name);
	NAME_ASSIGN(guid, MF_MT_ARBITRARY_FORMAT, name);
	NAME_ASSIGN(guid, MF_MT_IMAGE_LOSS_TOLERANT, name);
	NAME_ASSIGN(guid, MF_MT_MPEG4_SAMPLE_DESCRIPTION, name);
	NAME_ASSIGN(guid, MF_MT_MPEG4_CURRENT_SAMPLE_ENTRY, name);
	NAME_ASSIGN(guid, MF_MT_ORIGINAL_4CC, name);
	NAME_ASSIGN(guid, MF_MT_ORIGINAL_WAVE_FORMAT_TAG, name);
	NAME_ASSIGN(guid, MFMediaType_Audio, name);
	NAME_ASSIGN(guid, MFMediaType_Video, name);
	NAME_ASSIGN(guid, MFMediaType_Protected, name);
	NAME_ASSIGN(guid, MFMediaType_SAMI, name);
	NAME_ASSIGN(guid, MFMediaType_Script, name);
	NAME_ASSIGN(guid, MFMediaType_Image, name);
	NAME_ASSIGN(guid, MFMediaType_HTML, name);
	NAME_ASSIGN(guid, MFMediaType_Binary, name);
	NAME_ASSIGN(guid, MFMediaType_FileTransfer, name);

	NAME_ASSIGN(guid, MFVideoFormat_AI44, name);
	NAME_ASSIGN(guid, MFVideoFormat_ARGB32, name);
	NAME_ASSIGN(guid, MFVideoFormat_AYUV, name);
	NAME_ASSIGN(guid, MFVideoFormat_DV25, name);
	NAME_ASSIGN(guid, MFVideoFormat_DV50, name);
	NAME_ASSIGN(guid, MFVideoFormat_DVH1, name);
	NAME_ASSIGN(guid, MFVideoFormat_DVSD, name);
	NAME_ASSIGN(guid, MFVideoFormat_DVSL, name);
	NAME_ASSIGN(guid, MFVideoFormat_H264, name);
	NAME_ASSIGN(guid, MFVideoFormat_I420, name);
	NAME_ASSIGN(guid, MFVideoFormat_IYUV, name);
	NAME_ASSIGN(guid, MFVideoFormat_M4S2, name);
	NAME_ASSIGN(guid, MFVideoFormat_MJPG, name);
	NAME_ASSIGN(guid, MFVideoFormat_MP43, name);
	NAME_ASSIGN(guid, MFVideoFormat_MP4S, name);
	NAME_ASSIGN(guid, MFVideoFormat_MP4V, name);
	NAME_ASSIGN(guid, MFVideoFormat_MPG1, name);
	NAME_ASSIGN(guid, MFVideoFormat_MSS1, name);
	NAME_ASSIGN(guid, MFVideoFormat_MSS2, name);
	NAME_ASSIGN(guid, MFVideoFormat_NV11, name);
	NAME_ASSIGN(guid, MFVideoFormat_NV12, name);
	NAME_ASSIGN(guid, MFVideoFormat_P010, name);
	NAME_ASSIGN(guid, MFVideoFormat_P016, name);
	NAME_ASSIGN(guid, MFVideoFormat_P210, name);
	NAME_ASSIGN(guid, MFVideoFormat_P216, name);
	NAME_ASSIGN(guid, MFVideoFormat_RGB24, name);
	NAME_ASSIGN(guid, MFVideoFormat_RGB32, name);
	NAME_ASSIGN(guid, MFVideoFormat_RGB555, name);
	NAME_ASSIGN(guid, MFVideoFormat_RGB565, name);
	NAME_ASSIGN(guid, MFVideoFormat_RGB8, name);
	NAME_ASSIGN(guid, MFVideoFormat_UYVY, name);
	NAME_ASSIGN(guid, MFVideoFormat_v210, name);
	NAME_ASSIGN(guid, MFVideoFormat_v410, name);
	NAME_ASSIGN(guid, MFVideoFormat_WMV1, name);
	NAME_ASSIGN(guid, MFVideoFormat_WMV2, name);
	NAME_ASSIGN(guid, MFVideoFormat_WMV3, name);
	NAME_ASSIGN(guid, MFVideoFormat_WVC1, name);
	NAME_ASSIGN(guid, MFVideoFormat_Y210, name);
	NAME_ASSIGN(guid, MFVideoFormat_Y216, name);
	NAME_ASSIGN(guid, MFVideoFormat_Y410, name);
	NAME_ASSIGN(guid, MFVideoFormat_Y416, name);
	NAME_ASSIGN(guid, MFVideoFormat_Y41P, name);
	NAME_ASSIGN(guid, MFVideoFormat_Y41T, name);
	NAME_ASSIGN(guid, MFVideoFormat_YUY2, name);
	NAME_ASSIGN(guid, MFVideoFormat_YV12, name);
	NAME_ASSIGN(guid, MFVideoFormat_YVYU, name);

	NAME_ASSIGN(guid, MFAudioFormat_PCM, name);
	NAME_ASSIGN(guid, MFAudioFormat_Float, name);
	NAME_ASSIGN(guid, MFAudioFormat_DTS, name);
	NAME_ASSIGN(guid, MFAudioFormat_Dolby_AC3_SPDIF, name);
	NAME_ASSIGN(guid, MFAudioFormat_DRM, name);
	NAME_ASSIGN(guid, MFAudioFormat_WMAudioV8, name);
	NAME_ASSIGN(guid, MFAudioFormat_WMAudioV9, name);
	NAME_ASSIGN(guid, MFAudioFormat_WMAudio_Lossless, name);
	NAME_ASSIGN(guid, MFAudioFormat_WMASPDIF, name);
	NAME_ASSIGN(guid, MFAudioFormat_MSP1, name);
	NAME_ASSIGN(guid, MFAudioFormat_MP3, name);
	NAME_ASSIGN(guid, MFAudioFormat_MPEG, name);
	NAME_ASSIGN(guid, MFAudioFormat_AAC, name);
	NAME_ASSIGN(guid, MFAudioFormat_ADTS, name);

	return name;
}

std::string GetMediaTypeDescription(const IMFMediaType* mediaType)
{
	HRESULT hr = S_OK;
	GUID majprType;
}
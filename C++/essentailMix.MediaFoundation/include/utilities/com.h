#pragma once

#include<comdef.h>
#include <string>
#include <sstream>

namespace essentialMix::utilities
{
	template <class T>
	void safe_release(T** ppT)
	{
		if (*ppT)
		{
			(*ppT)->Release();
			*ppT = nullptr;
		}
	}

	template <class T>
	void safe_release(T*& pT)
	{
		if (*pT != nullptr)
		{
			pT->Release();
			pT = nullptr;
		}
	}

	inline System::String^ com_error_message(const HRESULT hr)
	{
		const _com_error error(hr);
		std::stringstream ss;
		ss << "Error 0x" << std::hex << hr << " " << error.ErrorMessage();
		return gcnew System::String(ss.str().c_str());
	}

	inline void throw_for_HR(const HRESULT hr, System::String^ msg = nullptr)
	{
		if (FAILED(hr))
		{
			if (msg) throw gcnew System::Runtime::InteropServices::COMException(msg);
			throw gcnew System::Runtime::InteropServices::COMException(com_error_message(hr));
		}
	}
}

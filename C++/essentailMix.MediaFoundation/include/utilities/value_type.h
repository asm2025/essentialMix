#pragma once

#include <stdexcept>

namespace essentialMix::utilities
{
	inline uint8_t get_uint8_t(short value, unsigned char position)
	{
		if (position > sizeof(short)) throw std::out_of_range("Position parameter is out of range.");
		position *= 8;
		return static_cast<uint8_t>((value & 0xFF << position) >> position);
	}

	inline uint8_t get_uint8_t(unsigned short value, unsigned char position)
	{
		if (position > sizeof(short)) throw std::out_of_range("Position parameter is out of range.");
		position *= 8;
		return static_cast<uint8_t>((value & 0xFF << position) >> position);
	}

	inline uint8_t get_uint8_t(int value, unsigned char position)
	{
		if (position > sizeof(int)) throw std::out_of_range("Position parameter is out of range.");
		position *= 8;
		return static_cast<uint8_t>((value & 0xFF << position) >> position);
	}

	inline uint8_t get_uint8_t(unsigned int value, unsigned char position)
	{
		if (position > sizeof(int)) throw std::out_of_range("Position parameter is out of range.");
		position *= 8;
		return static_cast<uint8_t>((value & 0xFF << position) >> position);
	}

	inline uint8_t get_uint8_t(long value, unsigned char position)
	{
		if (position > sizeof(long)) throw std::out_of_range("Position parameter is out of range.");
		position *= 8;
		return static_cast<uint8_t>((value & static_cast<long>(0xFF) << position) >> position);
	}

	inline uint8_t get_uint8_t(unsigned long value, unsigned char position)
	{
		if (position > sizeof(long)) throw std::out_of_range("Position parameter is out of range.");
		position *= 8;
		return static_cast<uint8_t>((value & static_cast<unsigned long>(0xFF) << position) >> position);
	}
}

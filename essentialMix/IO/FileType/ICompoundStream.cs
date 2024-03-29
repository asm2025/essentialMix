﻿using System;
using System.IO;
using JetBrains.Annotations;

namespace essentialMix.IO.FileType;

// based on https://github.com/neilharvey/FileSignatures/
public interface ICompoundStream
{
	bool HasGuid { get; }
	bool HasStorage { get; }
	IDisposable GetStream([NotNull] Stream stream);
	bool IsMatch(IDisposable stream);
}
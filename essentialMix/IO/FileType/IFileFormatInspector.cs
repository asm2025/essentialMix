using System.IO;
using JetBrains.Annotations;

namespace essentialMix.IO.FileType;

// based on https://github.com/neilharvey/FileSignatures/
public interface IFileFormatInspector
{
	FileFormat Detect([NotNull] Stream stream);
}
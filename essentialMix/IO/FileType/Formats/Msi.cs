using System;

namespace essentialMix.IO.FileType.Formats;

public record Msi() : CompoundFileBinaryBase("msi", "Windows Installer", Guid.Parse("{000c1084-0000-0000-c000-000000000046}"));
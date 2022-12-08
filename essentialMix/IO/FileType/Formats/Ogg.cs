﻿using essentialMix.Web;

namespace essentialMix.IO.FileType.Formats;

public record Ogg() : FileFormatBase("ogg", MediaTypeNames.Audio.Ogg, new byte[] { 0x4F, 0x67, 0x67, 0x53, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
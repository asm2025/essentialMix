﻿using System;

namespace asm.Patterns.Images
{
	public interface IImageBuilder
	{
		string BaseUri { get; set; }

		string ImageName { get; set; }

		ImageSize ImageSize { get; set; }

		Uri Build();
		Uri Build(string imageName);
		Uri Build(ImageSize imageSize);
		Uri Build(string imageName, ImageSize imageSize);
	}
}
using System;
using asm.Helpers;

namespace asm.Patterns.Images
{
	public class ImageBuilder : IImageBuilder
	{
		/// <inheritdoc />
		public ImageBuilder() 
			: this("/images/", null)
		{
		}

		/// <inheritdoc />
		public ImageBuilder(string baseUri) 
			: this(baseUri, null)
		{
		}

		public ImageBuilder(string baseUri, string imageName)
		{
			BaseUri = baseUri;
			ImageName = imageName;
		}

		/// <inheritdoc />
		public string BaseUri { get; set; }

		/// <inheritdoc />
		public string ImageName { get; set; }

		/// <inheritdoc />
		public ImageSize ImageSize { get; set; }

		/// <inheritdoc />
		public Uri Build() { return Build(ImageName, ImageSize); }

		/// <inheritdoc />
		public Uri Build(string imageName) { return Build(imageName, ImageSize); }

		/// <inheritdoc />
		public Uri Build(ImageSize imageSize) { return Build(ImageName, imageSize); }

		/// <inheritdoc />
		public virtual Uri Build(string imageName, ImageSize imageSize)
		{
			return UriHelper.Combine(BaseUri, imageName);
		}
	}
}
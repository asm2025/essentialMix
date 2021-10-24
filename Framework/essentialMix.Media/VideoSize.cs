using System;
using System.Diagnostics;
using essentialMix.Numeric;

namespace essentialMix.Media
{
	[DebuggerDisplay("{DisplayName}")]
	public sealed class VideoSize
	{
		private readonly object _lock = new object();

		private VideoSizeEnum _value;
		private int _width;
		private int _height;

		public VideoSize()
			: this(VideoSizeEnum.Empty)
		{
		}

		public VideoSize(VideoSizeEnum videoSize)
		{
			Enabled = true;
			Value = videoSize;
		}

		public VideoSize(int width, int height)
		{
			Enabled = true;
			SetWidthAndHeight(width, height);
		}

		public override string ToString() { return ffmpeg ?? DimensionsString; }

		public VideoSizeEnum Value
		{
			get => _value;
			set
			{
				if (_value == value) return;
				_value = value;
				UpdateByValue();
			}
		}

		public bool Enabled { get; set; }
		public string Name { get; private set; }
		public string DisplayName { get; private set; }
		public string ffmpeg { get; private set; }
		public string DimensionsString { get; private set; }

		public int Width
		{
			get => _width;
			set
			{
				if (_width == value) return;
				_width = value;
				UpdateByWidthAndHeight();
			}
		}

		public int Height
		{
			get => _height;
			set
			{
				if (_height == value) return;
				_height = value;
				UpdateByWidthAndHeight();
			}
		}

		public int BitRate { get; private set; }
		public double AspectRatio { get; private set; }
		public string RationalApproximation { get; private set; }

		public bool IsEmpty => Value == VideoSizeEnum.Empty || Width <= 0 && Height <= 0;

		public void SetWidthAndHeight(int width, int height)
		{
			_width = width;
			_height = height;
			UpdateByWidthAndHeight();
		}

		private void UpdateByValue()
		{
			lock(_lock)
			{
				switch (_value)
				{
					case VideoSizeEnum.Empty:
						_width = _height = -1;
						break;
					case VideoSizeEnum.Automatic:
						_width = _height = 0;
						break;
				}

				DisplayName = _value is VideoSizeEnum.Automatic or VideoSizeEnum.Empty
								? _value.ToString()
								: $"{_width}x{_height}";
				ffmpeg = null;
				Name = _value.ToString();

				if (_width > 0 && _height > 0)
				{
					BitRate = _width * _height;
					AspectRatio = (double)_width / _height;
					RationalApproximation = Math2.RationalApproximation(AspectRatio).ToString();
					DimensionsString = $"{Width}x{Height}";
				}
				else
				{
					BitRate = 0;
					AspectRatio = 0;
					RationalApproximation = null;
					DimensionsString = null;
				}
			}
		}

		private void UpdateByWidthAndHeight()
		{
			lock(_lock)
			{
				if (_width < 0 && _height < 0)
					_value = VideoSizeEnum.Empty;
				else if (_width == 0 && _height == 0)
					_value = VideoSizeEnum.Automatic;
				else
				{
					string searchString = $"{_width}x{_height}";
					_value = Enum.TryParse(searchString, out VideoSizeEnum v)
								? v
								: VideoSizeEnum.Custom;
				}
			}

			UpdateByValue();
		}
	}
}
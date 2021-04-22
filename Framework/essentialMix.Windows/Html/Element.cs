using System;
using System.Drawing;
using JetBrains.Annotations;

namespace essentialMix.Windows.Html
{
	/// <summary>
	/// Represent an Element which can be HTML element or Style element
	/// </summary>
	/// <remarks>
	/// _size represent the size of HTML element (Style element has no size)
	/// _spaceSize represent the size of space � � character and is calculated
	/// as the average size of a character
	/// _dispRect represent the displayed Rectangle of HTML element
	/// </remarks>
	public class Element
	{
		private SizeF _size;

		public Element([NotNull] Status status)
		{
			Type = ElementType.Status;
			Status = new Status(status);
			_size = new SizeF(0, 0);
			SpaceSize = 0;
			Html = null;
		}

		public Element([NotNull] SimplePart html)
		{
			Type = ElementType.Html;
			Status = null;
			SpaceSize = 0;
			Html = html;

			if (html is Text)
			{
				_size = new SizeF(0, 0);
				return;
			}

			if (html is not Label label) throw new TypeAccessException();
			_size = new SizeF(label.Width, label.Height);
		}

		public override string ToString()
		{
			switch (Type)
			{
				case ElementType.Status:
					return $"STAT Element: stat={Status};sz={_size};ss={SpaceSize}";
				case ElementType.Html:
					return $"HTML Element: type={Html.Type.ToString()};sz={_size};ss={SpaceSize}";
			}

			return "NULL Element";
		}

		public Rectangle DisplayedRect { get; set; }

		public SimplePart Html { get; set; }

		public ElementType Type { get; set; }

		public Status Status { get; set; }

		public SizeF Size
		{
			get => _size;
			set
			{
				_size = value;
				SpaceSize = 0;
				if (Html != null)
				{
					SpaceSize = (float)(Html.Value.Length > 0 ? value.Width / Html.Value.Length : 0.0);
				}
			}
		}

		public float SpaceSize { get; private set; }
	}
}
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using essentialMix.Helpers;
using essentialMix.Patterns.Layout;
using essentialMix.Windows.Html;
using JetBrains.Annotations;
using Label = essentialMix.Windows.Html.Label;

namespace essentialMix.Windows.Controls
{
	/// <inheritdoc />
	/// <summary>
	///     Represent HTML - a label which can show very small subset of HTML
	/// </summary>
	public class HtmlLabel : Control
	{
		public delegate void RecalculatedEvent(object sender, EventArgs e);

		private Bitmap _bmpOffScr;
		private readonly Elements _htmlElements;
		private readonly Dictionary<string, Element> _labelsPositions;

		private bool _textChanged = true;
		private bool _labelTextChanged;
		private bool _recreateImage;
		private bool _showBorders;
		private VerticalAlignment _vertAlign;
		private float _spaceSize;
		private readonly List<TextLine> _textLines = new List<TextLine>();
		private float _totalHeight;
		private Color _bckgrndColor;

		public HtmlLabel()
		{
			_spaceSize = -1;
			_vertAlign = VerticalAlignment.Top;
			_spaceSize = -1;
			_htmlElements = new Elements();
			_labelsPositions = new Dictionary<string, Element>();
			Size = new Size(50, 20);
			TabStop = false;
			base.Text = Name;
		}

		public override string Text
		{
			get => base.Text;
			set
			{
				if (base.Text == value) return;
				base.Text = value;
				_textChanged = true;
				Invalidate();
			}
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			if (_bmpOffScr == null || Size != _bmpOffScr.Size || _bckgrndColor != BackColor ||
				_textChanged || _labelTextChanged || _recreateImage) DrawOffscreenPicture();

			if (_bmpOffScr != null) e.Graphics.DrawImage(_bmpOffScr, 0, 0);
			base.OnPaint(e);
		}

		protected override void OnPaintBackground(PaintEventArgs e)
		{
			//Do nothing
		}

		public event RecalculatedEvent Recalculated;

		public VerticalAlignment VerticalAlignment
		{
			get => _vertAlign;
			set
			{
				if (_vertAlign == value) return;
				_vertAlign = value;
				RecreateImage();
			}
		}

		public float SpaceSize
		{
			get => _spaceSize;
			set
			{
				if (Math.Abs(_spaceSize - value) < double.Epsilon) return;
				_spaceSize = value;
				RecreateImage();
			}
		}

		public bool ShowBorders
		{
			get => _showBorders;
			set
			{
				if (_showBorders == value) return;
				_showBorders = value;
				RecreateImage();
			}
		}

		public bool GetLabelPos([NotNull] string labelID, ref Rectangle rect)
		{
			if (!_labelsPositions.ContainsKey(labelID)) return false;
			rect = _labelsPositions[labelID].DisplayedRect;
			return true;
		}

		public bool ChangeLabelText([NotNull] string labelID, string value)
		{
			if (!_labelsPositions.ContainsKey(labelID)) return false;
			_labelsPositions[labelID].Html.Value = value;
			_labelTextChanged = true;
			Invalidate();
			return true;
		}

		public void RepositionControlOnLabel([NotNull] string labelID, [NotNull] Control label)
		{
			Rectangle rect = label.Bounds;
			if (GetLabelPos(labelID, ref rect))
			{
				rect.Offset(Left, Top);
				label.Bounds = rect;
			}
		}

		private bool ParseText()
		{
			if (!_textChanged) return false;

			Status status = new Status
			{
				Font = new StrFont(Font),
				Brush = new StrBrush(ForeColor)
			};

			_htmlElements.Parse(base.Text, status);
			_textChanged = false;
			return true;
		}

		[NotNull]
		private Graphics CreateBackground()
		{
			if (_bmpOffScr == null || Size != _bmpOffScr.Size) _bmpOffScr = new Bitmap(ClientSize.Width, ClientSize.Height);

			Graphics grOffScr = Graphics.FromImage(_bmpOffScr);
			grOffScr.Clear(BackColor);
			_bckgrndColor = BackColor;
			return grOffScr;
		}

		// raèunanje velikosti elementov in doloèanje posameznih vrstic
		private void CalculateLines(Graphics grOff)
		{
			_textLines.Clear();
			int offset = 2;
			float currWdth = 0, currHgth = 0;

			IList<Element> values = _htmlElements.Value;

			Status status = values[0].Status;
			Font font = status.Font.GetRealFont();
			double lastSpaceSize = 0;
			_totalHeight = 0;

			for (int i = 1; i < values.Count; ++i)
			{
				switch (values[i].Type)
				{
					case ElementType.Status:
						bool newLine = status.Alignment != values[i].Status.Alignment ||
										status.WordWrap != values[i].Status.WordWrap;
						status = values[i].Status;
						font = status.Font.GetRealFont();
						newLine = newLine | status.NewLine;

						if (newLine)
						{
							_textLines.Add(new TextLine(currWdth, currHgth, i - 1));
							_totalHeight += currHgth;
							currWdth = 0;
							currHgth = 0;
						}

						break;
					case ElementType.Html:
						if (values[i].Html.Type == PartType.Text) values[i].Size = grOff.MeasureString(values[i].Html.Value, font);
						else
						{
							if (values[i].Size.Width < 0 || values[i].Size.Height < 0)
							{
								string txt = values[i].Html.Value;
								if (txt == string.Empty) txt = " ";
								SizeF size = grOff.MeasureString(txt, font);
								if (values[i].Size.Width > 0) size.Width = values[i].Size.Width;
								if (values[i].Size.Height > 0) size.Height = values[i].Size.Height;
								values[i].Size = size;
							}
						}

						if (i == 0)
						{
							currWdth = values[i].Size.Width;
							currHgth = values[i].Size.Height;
						}
						else
						{
							float spaceSize = _spaceSize;
							if (spaceSize < 0) spaceSize = (float)(values[i].SpaceSize + lastSpaceSize) / 2;

							if (status.WordWrap && currWdth + values[i].Size.Width + spaceSize + 2 * offset >= Width)
							{
								_textLines.Add(new TextLine(currWdth, currHgth, i - 1));
								_totalHeight += currHgth;
								currWdth = values[i].Size.Width;
								currHgth = values[i].Size.Height;
							}
							else
							{
								if (currWdth > 0) currWdth += spaceSize;
								currWdth += values[i].Size.Width;
								currHgth = Math.Max(currHgth, values[i].Size.Height);
							}
						}
						lastSpaceSize = values[i].SpaceSize;
						break;
				}
			}
			_textLines.Add(new TextLine(currWdth, currHgth, values.Count - 1)); // stražar
			_totalHeight += currHgth;

			/*
			global::System.IO.StreamWriter sw = new global::System.IO.StreamWriter(@"\temp\parse.txt", true);
			sw.WriteLine("-------------------");
			sw.WriteLine("   DRAW   ");
			sw.WriteLine("-------------------");
			sw.WriteLine(this.Text);
			sw.WriteLine("-------------------");
			foreach (TextLine tl in _textLines)
			sw.WriteLine(tl.ToString());
			sw.Close();
			*/
		}

		private void DrawLines(Graphics grOff)
		{
			IList<Element> values = _htmlElements.Value;
			Status status = values[0].Status;
			Brush brush = status.Brush.GetRealBrush();
			Font font = status.Font.GetRealFont();

			int offset = 2;
			int currElement = 1;
			float top = offset;

			_labelsPositions.Clear();
			switch (_vertAlign)
			{
				case VerticalAlignment.Center:
					top = (Height - _totalHeight) / 2;
					break;
				case VerticalAlignment.Bottom:
					top = Height - _totalHeight - offset;
					break;
			}

			foreach (TextLine line in _textLines)
			{
				if (values[currElement].Type == ElementType.Status)
				{
					status = values[currElement].Status;
					brush = status.Brush.GetRealBrush();
					font = status.Font.GetRealFont();
				}

				float left;
				switch (status.Alignment)
				{
					case ContentAlignment.TopCenter:
						left = (Width - line.Width) / 2;
						break;
					case ContentAlignment.TopRight:
						left = Width - line.Width - offset;
						break;
					default:
						left = offset;
						break;
				}

				float lastSpaceSize = 0;
				for (; currElement <= line.LastElement; ++currElement)
				{
					if (values[currElement].Type == ElementType.Status)
					{
						status = values[currElement].Status;
						brush = status.Brush.GetRealBrush();
						font = status.Font.GetRealFont();
					}

					if (values[currElement].Type == ElementType.Html)
					{
						if (values[currElement].Html.Type == PartType.Text)
						{
							if (_showBorders)
							{
								Rectangle irect = new Rectangle((int)left, (int)(top + line.Height - values[currElement].Size.Height),
									(int)values[currElement].Size.Width, (int)values[currElement].Size.Height);
								grOff.FillRectangle(new SolidBrush(Color.LightCyan), irect);
							}

							grOff.DrawString(values[currElement].Html.Value, font, brush, left, top + line.Height - values[currElement].Size.Height);
						}
						else
						{
							if (values[currElement].Html is not Label label) continue;
							RectangleF rect = new RectangleF(left, top + line.Height - values[currElement].Size.Height,
								values[currElement].Size.Width, values[currElement].Size.Height);
							Rectangle irect = new Rectangle((int)left, (int)(top + line.Height - values[currElement].Size.Height),
								(int)values[currElement].Size.Width, (int)values[currElement].Size.Height);
							if (_showBorders) grOff.FillRectangle(new SolidBrush(Color.LightBlue), irect);

							grOff.DrawString(label.Value, font, brush, rect);

							values[currElement].DisplayedRect = irect;
							_labelsPositions.Add(label.ID, values[currElement]);
						}

						if (Math.Abs(lastSpaceSize) < double.Epsilon) lastSpaceSize = values[currElement].SpaceSize;

						float spaceSize = _spaceSize;
						if (spaceSize < 0) spaceSize = (values[currElement].SpaceSize + lastSpaceSize) / 2;
						left += values[currElement].Size.Width + spaceSize;
						lastSpaceSize = values[currElement].SpaceSize;
					}
				}
				top += line.Height;
			}
		}

		private void DrawOffscreenPicture()
		{
			bool parsed = ParseText();
			Graphics grOff = CreateBackground();

			if (_htmlElements.Value.Count > 1)
			{
				if (parsed || _recreateImage) CalculateLines(grOff);
				DrawLines(grOff);
				Recalculated?.Invoke(this, null);
				_labelTextChanged = false;
				_recreateImage = false;
			}

			ObjectHelper.Dispose(ref grOff);
		}

		private void RecreateImage()
		{
			_recreateImage = true;
			Invalidate();
		}
	}
}
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using JetBrains.Annotations;

namespace essentialMix.Core.WPF.Controls
{
	public enum TransitionQuality
	{
		None,
		Low = 72,
		Medium = 96,
		High = 120
	}

	public enum TransitionDirection
	{
		Default,
		Left,
		Right
	}

	public interface ITransitionSlide
	{
		TransitionDirection Direction { get; set; }
		bool TransitionEnabled { get; set; }
		int Order { get; set; }
		bool WideScreen { get; }
		bool FullScreen { get; }
	}

	public class SlideTransition : Freezable
	{
		private static readonly DependencyPropertyKey __enabledPropertyKey = DependencyProperty.RegisterReadOnly(nameof(Enabled), typeof(bool), typeof(SlideTransition), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.None));

		public static readonly DependencyProperty DurationProperty = DependencyProperty.Register(nameof(Duration), typeof(Duration), typeof(SlideTransition), new FrameworkPropertyMetadata(new Duration(TimeSpan.FromMilliseconds(300)), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnEnabledDependenciesChanged), OnValidateDuration);
		public static readonly DependencyProperty QualityProperty = DependencyProperty.Register(nameof(Quality), typeof(TransitionQuality), typeof(SlideTransition), new FrameworkPropertyMetadata(TransitionQuality.Medium, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnEnabledDependenciesChanged), OnValidateQuality);
		public static readonly DependencyProperty EnabledProperty = __enabledPropertyKey.DependencyProperty;

		/// <inheritdoc />
		public SlideTransition()
		{
		}

		public Duration Duration
		{
			get => (Duration)GetValue(DurationProperty);
			set => SetValue(DurationProperty, value);
		}

		public TransitionQuality Quality
		{
			get => (TransitionQuality)GetValue(QualityProperty);
			set => SetValue(QualityProperty, value);
		}

		public bool Enabled => (bool)GetValue(EnabledProperty);

		/// <inheritdoc />
		protected override Freezable CreateInstanceCore() { return new SlideTransition(); }

		private static bool OnValidateDuration(object value)
		{
			return value is Duration d && (d.HasTimeSpan || d == Duration.Automatic);
		}

		private static bool OnValidateQuality(object value)
		{
			return value is TransitionQuality tq && Enum.IsDefined(typeof(TransitionQuality), tq);
		}

		private static void OnEnabledDependenciesChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
		{
			SlideTransition transition = (SlideTransition)sender;
			bool enabled = transition.Quality > TransitionQuality.None &&
							(transition.Duration == Duration.Automatic || transition.Duration.HasTimeSpan && transition.Duration.TimeSpan > TimeSpan.Zero);
			transition.SetValue(__enabledPropertyKey, enabled);
		}
	}

	/// <inheritdoc />
	[TemplatePart(Name = "PART_PaintArea", Type = typeof(Shape))]
	[TemplatePart(Name = "PART_Content", Type = typeof(ContentPresenter))]
	public class SlidingContent : ContentControl
	{
		public static readonly DependencyProperty TransitionProperty = DependencyProperty.Register(nameof(Transition), typeof(SlideTransition), typeof(SlidingContent), new FrameworkPropertyMetadata(new SlideTransition(), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

		private Storyboard _slidingInAnimation;
		private Storyboard _slidingOutAnimation;
		// represents the old content
		private Shape _paintArea;
		private ContentPresenter _content;

		static SlidingContent()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(SlidingContent), new FrameworkPropertyMetadata(typeof(SlidingContent)));
		}

		/// <inheritdoc />
		public SlidingContent()
		{
		}

		[NotNull]
		public SlideTransition Transition
		{
			get => (SlideTransition)GetValue(TransitionProperty);
			set => SetValue(TransitionProperty, value);
		}

		/// <inheritdoc />
		public override void OnApplyTemplate()
		{
			_paintArea = Template.FindName("PART_PaintArea", this) as Shape;
			_content = Template.FindName("PART_Content", this) as ContentPresenter;
			base.OnApplyTemplate();
		}

		/// <inheritdoc />
		protected override void OnContentChanged(object oldContent, object newContent)
		{
			if (_paintArea == null || _content == null || !Transition.Enabled)
			{
				base.OnContentChanged(oldContent, newContent);
				return;
			}

			ITransitionSlide oldSlide = oldContent as ITransitionSlide;
			ITransitionSlide newSlide = newContent as ITransitionSlide;

			if (oldSlide == null && newSlide == null
				|| oldSlide is { WideScreen: false }
				|| newSlide is { WideScreen: false }
				|| oldSlide is { TransitionEnabled: false } && newSlide is { TransitionEnabled: false })
			{
				base.OnContentChanged(oldContent, newContent);
				return;
			}

			TransitionDirection direction;

			// if we are here, then at least one of the contents has transition enabled
			if (newSlide is { TransitionEnabled: true })
			{
				if (newSlide.Direction == TransitionDirection.Default)
				{
					if (oldSlide != null)
					{
						// check the new slide order relative to the old slide
						int diff = oldSlide.Order - newSlide.Order;
						direction = diff <= 0
										? TransitionDirection.Left
										: TransitionDirection.Right;
					}
					else
					{
						direction = TransitionDirection.Left;
					}
				}
				else
				{
					direction = newSlide.Direction;
				}
			}
			else if (oldSlide is { TransitionEnabled: true })
			{
				// old slide transition direction will be reversed
				if (oldSlide.Direction == TransitionDirection.Default)
				{
					if (newSlide != null)
					{
						// check the new slide order relative to the old slide
						int diff = oldSlide.Order - newSlide.Order;
						direction = diff > 0
										? TransitionDirection.Right
										: TransitionDirection.Left;
					}
					else
					{
						direction = TransitionDirection.Right;
					}
				}
				else
				{
					direction = oldSlide.Direction == TransitionDirection.Left
									? TransitionDirection.Right
									: TransitionDirection.Left;
				}
			}
			else
			{
				base.OnContentChanged(oldContent, newContent);
				return;
			}

			_paintArea.Fill = CreateBrushFromVisual(_content);
			BeginAnimateContentReplacement(direction);
			base.OnContentChanged(oldContent, newContent);
		}

		/// <summary>
		/// Creates a snapshot image from current content
		/// </summary>
		[NotNull]
		private Brush CreateBrushFromVisual([NotNull] Visual element)
		{
			int quality = (int)Transition.Quality;
			Debug.Assert(quality > 0);
			RenderTargetBitmap target = new RenderTargetBitmap((int)ActualWidth, (int)ActualHeight, quality, quality, PixelFormats.Pbgra32);
			target.Render(element);
			Brush brush = new ImageBrush(target);
			brush.Freeze();
			return brush;
		}

		private void BeginAnimateContentReplacement(TransitionDirection direction)
		{
			if (_paintArea == null || _content == null) return;

			Duration duration = Transition.Duration;
			double offset = ActualWidth + Math.Max(Padding.Left, Padding.Right);
			if (direction == TransitionDirection.Right) offset *= -1;
			_paintArea.RenderTransform = new TranslateTransform(-offset, 0);
			_content.RenderTransform = new TranslateTransform(offset, 0);
			_paintArea.Visibility = Visibility.Visible;
			_slidingOutAnimation ??= CreateOutAnimation(() => _paintArea.Visibility = Visibility.Hidden);
			_slidingInAnimation ??= CreateInAnimation();

			foreach (DoubleAnimation animation in _slidingOutAnimation.Children.Cast<DoubleAnimation>())
			{
				animation.Duration = duration;
				Storyboard.SetTarget(animation, _paintArea);
			}

			foreach (DoubleAnimation animation in _slidingInAnimation.Children.Cast<DoubleAnimation>())
			{
				animation.Duration = duration;
				Storyboard.SetTarget(animation, _content);
			}

			_slidingOutAnimation.Begin();
			_slidingInAnimation.Begin();

			static Storyboard CreateOutAnimation(Action onCompleted)
			{
				Storyboard storyboard = new Storyboard
				{
					Children =
					{
						new DoubleAnimation
						{
							From = 0d,
							EasingFunction = new CubicEase
							{
								EasingMode = EasingMode.EaseOut
							}
						},
						new DoubleAnimation
						{
							From = 1d,
							To = 0d,
							EasingFunction = new CubicEase
							{
								EasingMode = EasingMode.EaseOut
							}
						}
					}
				};
				Storyboard.SetTargetProperty(storyboard.Children[0], new PropertyPath("RenderTransform.(TranslateTransform.X)"));
				Storyboard.SetTargetProperty(storyboard.Children[1], new PropertyPath(OpacityProperty));
				storyboard.Completed += (_, _) => onCompleted();
				return storyboard;
			}

			static Storyboard CreateInAnimation()
			{
				Storyboard storyboard = new Storyboard
				{
					Children =
					{
						new DoubleAnimation
						{
							To = 0d,
							EasingFunction = new CubicEase
							{
								EasingMode = EasingMode.EaseOut
							}
						},
						new DoubleAnimation
						{
							From = 0d,
							To = 1d,
							EasingFunction = new CubicEase
							{
								EasingMode = EasingMode.EaseOut
							}
						}
					}
				};
				Storyboard.SetTargetProperty(storyboard.Children[0], new PropertyPath("RenderTransform.(TranslateTransform.X)"));
				Storyboard.SetTargetProperty(storyboard.Children[1], new PropertyPath(OpacityProperty));
				return storyboard;
			}
		}
	}
}

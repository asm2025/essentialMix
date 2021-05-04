using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using essentialMix.Collections;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;

namespace essentialMix.Media.ffmpeg.Commands
{
	public sealed class MetadataCommand : FastMetadataCommand
	{
		//https://trac.ffmpeg.org/wiki/FFprobeTips
		//https://stackoverflow.com/questions/2017843/fetch-frame-count-with-ffmpeg
		//https://stackoverflow.com/questions/2246694/how-to-convert-json-object-to-custom-c-sharp-object
		//ffmpeg -i FILENAME.mov
		private class StreamMetaDataCommand : InputCommand
		{
			//ffprobe.exe -i video_name -print_format json -loglevel fatal -show_format -show_streams -count_frames
			private readonly Metadata _metadata;

			public StreamMetaDataCommand([NotNull] Metadata metadata)
				: base(Properties.Settings.Default.FFPROBE_NAME)
			{
				Arguments.Insert(0, new Property("default", "-hide_banner -print_format json -show_format -show_streams -count_frames", true, true));
				_metadata = metadata;
			}

			protected override void OnCreate(Process process)
			{
				base.OnCreate(process);
				if (_metadata.Video != null) _metadata.Video.StreamMetadata = null;
				if (_metadata.Audio != null) _metadata.Audio.StreamMetadata = null;
				_metadata.FormatLongName = null;
				_metadata.ProbeScore = 0;
				_metadata.Tags = null;
			}

			protected override void OnCompleted()
			{
				base.OnCompleted();

				string json = RunOutput?.OutputBuilder.ToString();
				if (string.IsNullOrEmpty(json)) return;

				JObject jObject = JObject.Parse(json);

				if (jObject["format"] is JObject jFormat)
				{
					_metadata.FormatLongName = (string)jFormat["format_long_name"];
					_metadata.ProbeScore = (int)jFormat["probe_score"];

					if (jFormat["tags"] is JObject { Count: > 0 } jTags)
					{
						IDictionary<string, string> tags = new Dictionary<string, string>(jTags.Count);

						foreach (JProperty jTag in jTags.Children<JProperty>())
						{
							tags.Add(jTag.Name, jTag.Value.ToString());
						}

						_metadata.Tags = tags;
					}
				}

				if (jObject["streams"] is not JArray streamsObjects) return;

				foreach (JObject jStream in streamsObjects.OfType<JObject>())
				{
					IDictionary<string, JToken> dictionary = jStream;

					if (dictionary.ContainsKey("width") || dictionary.ContainsKey("display_aspect_ratio"))
					{
						// this is a video stream.
						_metadata.Video ??= new Metadata.VideoMetadata();
						_metadata.Video.StreamMetadata = jStream;
					}
					else
					{
						_metadata.Audio ??= new Metadata.AudioMetadata();
						_metadata.Audio.StreamMetadata ??= new List<dynamic>();
						_metadata.Audio.StreamMetadata.Add(jStream);
					}
				}
			}
		}

		public MetadataCommand()
		{
			Dependencies.Add(Properties.Settings.Default.FFPROBE_NAME);
		}

		protected override void OnCompleted()
		{
			base.OnCompleted();

			if (Metadata.Video != null)
			{
				if (Metadata.Video.BitRate > 0) Metadata.Video.BitRate *= 1000;
				else Metadata.Video.BitRate = Metadata.Video.Size.BitRate;
			}

			if (Metadata.Audio is { BitRate: > 0 }) Metadata.Audio.BitRate *= 1000;

			using StreamMetaDataCommand streamMetaData = new StreamMetaDataCommand(Metadata)
			{
				Input = Input
			};
			streamMetaData.Run();
		}
	}
}
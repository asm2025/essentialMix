using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using asm.Data.Extensions;
using asm.Extensions;
using asm.Globalization;
using asm.Helpers;
using asm.Media.Youtube.Exceptions;
using asm.Media.Youtube.Internal;
using asm.Media.Youtube.Internal.CipherOperations;
using asm.Web;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;

namespace asm.Media.Youtube
{
	/// <summary>
	///     Original source from YoutubeExplode + YoutubeExtractor on GitHub
	/// </summary>
	public sealed class YoutubeClient
	{
		private const string UNAVAILABLE_CONTAINER = @"<div id=""watch-player-unavailable"">";
		private const string RATE_BYPASS_FLAG = "ratebypass";
		private const string SIGNATURE_QUERY = "signature";

		private static readonly Regex YTPLAYER_CONFIG = new Regex(@"ytplayer\.config\s*=\s*(\{.+?\});", RegexHelper.OPTIONS_I | RegexOptions.Multiline);

		private static YoutubeClient _instance;

		private readonly IDictionary<string, PlayerSource> _playerSourceCache = new Dictionary<string, PlayerSource>();
		private HttpService _service;

		public YoutubeClient() { }

		public YoutubeClient([NotNull] HttpService httpService) { _service = httpService; }

		private HttpService Service => _service ??= HttpService.Instance;

		public bool VideoExists([NotNull] string videoId)
		{
			if (videoId == null) throw new ArgumentNullException(nameof(videoId));
			if (!ValidateVideoId(videoId)) throw new ArgumentException("Invalid Youtube video ID", nameof(videoId));
			return TaskHelper.Run(() => VideoExistsAsync(videoId));
		}

		/// <summary>
		///     Checks whether a video with the given ID exists
		/// </summary>
		public async Task<bool> VideoExistsAsync([NotNull] string videoId)
		{
			if (videoId == null) throw new ArgumentNullException(nameof(videoId));
			if (!ValidateVideoId(videoId)) throw new ArgumentException("Invalid Youtube video ID", nameof(videoId));

			string request = $"https://www.youtube.com/get_video_info?video_id={videoId}&el=info&ps=default&hl=en";
			string response = await Service.GetStringAsync(request).ConfigureAwait();
			IDictionary<string, string> videoInfoDic = UriHelper.GetDictionaryFromUrlQuery(response);

			if (!videoInfoDic.ContainsKey("errorcode")) return true;

			int errorCode = videoInfoDic.Get("errorcode").To(0);
			return errorCode != 100 && errorCode != 150;
		}

		public VideoInfo GetVideoInfo([NotNull] string videoId)
		{
			if (videoId == null) throw new ArgumentNullException(nameof(videoId));
			if (!ValidateVideoId(videoId)) throw new ArgumentException("Invalid Youtube video ID", nameof(videoId));
			return TaskHelper.Run(() => GetVideoInfoAsync(videoId));
		}

		/// <summary>
		///     Gets video info by ID
		/// </summary>
		public async Task<VideoInfo> GetVideoInfoAsync(string videoId)
		{
			if (videoId == null) throw new ArgumentNullException(nameof(videoId));
			if (!ValidateVideoId(videoId)) throw new ArgumentException("Invalid Youtube video ID", nameof(videoId));

			PlayerContext playerContext = await GetPlayerContextAsync(videoId).ConfigureAwait();
			string request = $"https://www.youtube.com/get_video_info?video_id={videoId}&sts={playerContext.Sts}&el=info&ps=default&hl=en";
			string response = await Service.GetStringAsync(request).ConfigureAwait();
			IDictionary<string, string> videoInfoDic = UriHelper.GetDictionaryFromUrlQuery(response);

			// Check for error
			if (videoInfoDic.ContainsKey("errorcode"))
			{
				int errorCode = videoInfoDic.Get("errorcode").To(0);
				string errorReason = videoInfoDic.GetOrDefault("reason");
				throw new VideoNotAvailableException(errorCode, errorReason);
			}

			if (videoInfoDic.GetOrDefault("requires_purchase") == "1") throw new VideoRequiresPurchaseException();

			request = $"http://youtube.com/watch?v={videoId}&hl=en";
			response = await Service.GetStringAsync(request).ConfigureAwait();
			if (response == null || response.Contains(UNAVAILABLE_CONTAINER)) throw new VideoNotAvailableException();

			Match ytPlayerConfig = YTPLAYER_CONFIG.Match(response);
			if (!ytPlayerConfig.Success) throw new VideoNotAvailableException();

			PlayerSource playerSource = await GetPlayerSourceAsync(playerContext.Version).ConfigureAwait();
			JObject ytData = JObject.Parse(ytPlayerConfig.Result("$1"));
			JToken streamMap = ytData["args"]["url_encoded_fmt_stream_map"];
			string streamMapString = streamMap?.ToString();
			if (streamMapString == null || streamMapString.Contains("been+removed")) throw new VideoNotAvailableException("Video is removed or has an age restriction.");

			string[] splitByUrls = streamMapString.Split(',');
			streamMap = ytData["args"]["adaptive_fmts"] ?? ytData["args"]["url_encoded_fmt_stream_map"];

			string[] adaptiveFmtSplitByUrls = streamMap?.ToString().Split(',');
			if (adaptiveFmtSplitByUrls != null && adaptiveFmtSplitByUrls.Length > 0) splitByUrls = splitByUrls.Concat(adaptiveFmtSplitByUrls).ToArray();

			IDictionary<int, MixedStreamInfo> mixedStreams = new Dictionary<int, MixedStreamInfo>();
			IDictionary<int, AudioStreamInfo> audioStreams = new Dictionary<int, AudioStreamInfo>();
			IDictionary<int, VideoStreamInfo> videoStreams = new Dictionary<int, VideoStreamInfo>();

			foreach (string s in splitByUrls)
			{
				IDictionary<string, string> streamDic = UriHelper.GetDictionaryFromUrlQuery(s);
				string url = streamDic.Get("url");
				string sig = streamDic.GetOrDefault("s");

				// Decipher signature if needed
				if (!string.IsNullOrEmpty(sig))
				{
					sig = playerSource.Decipher(sig);
					url = UriHelper.SetQueryParameter(url, SIGNATURE_QUERY, sig);
					if (streamDic.ContainsKey("fallback_host")) url += "&fallback_host=" + streamDic.Get("fallback_host");
				}

				url = UriHelper.SetQueryParameter(url, RATE_BYPASS_FLAG, "yes");

				IDictionary<string, string> parameters = UriHelper.GetDictionaryFromUrlQuery(url);
				int itag = parameters.Get("itag").To(0);
				TagTypeEnum tagType = MediaStreamInfo.TAG_MAP.GetOrDefault(itag)?.TagType ?? TagTypeEnum.Unknown;
				long contentLength;

				switch (tagType)
				{
					case TagTypeEnum.Mixed:
						if (mixedStreams.ContainsKey(itag)) continue;

						// Get content length
						using (HttpRequestMessage reqMsg = new HttpRequestMessage(HttpMethod.Head, url))
						{
							using HttpResponseMessage resMsg = await Service.PerformRequestAsync(reqMsg).ConfigureAwait();
							// Check status code (https://github.com/Tyrrrz/YoutubeExplode/issues/36)
							if (resMsg.StatusCode == HttpStatusCode.NotFound || resMsg.StatusCode == HttpStatusCode.Gone) continue;
							resMsg.EnsureSuccessStatusCode();
							contentLength = resMsg.Content.Headers.ContentLength ?? -1;
							if (contentLength <= 0) continue;
						}

						MixedStreamInfo mixedStreamInfo = new MixedStreamInfo
						{
							iTag = itag,
							Url = url,
							ContentLength = contentLength
						};
						mixedStreams.Add(itag, mixedStreamInfo);
						break;
					case TagTypeEnum.VideoOnly:
					case TagTypeEnum.AudioOnly:
						contentLength = streamDic.Get("clen").To(0L);
						if (contentLength <= 0) continue;

						long bitrate = streamDic.Get("bitrate").To(0L);
						bool isAudio = tagType == TagTypeEnum.AudioOnly;

						if (isAudio)
						{
							if (audioStreams.ContainsKey(itag)) continue;

							AudioStreamInfo audioStreamInfo = new AudioStreamInfo
							{
								iTag = itag,
								Url = url,
								ContentLength = contentLength,
								Bitrate = bitrate
							};
							audioStreams.Add(itag, audioStreamInfo);
						}
						else
						{
							if (videoStreams.ContainsKey(itag)) continue;
							// Parse additional data
							string size = streamDic.Get("size");
							int width = size.SubstringUntil("x").To(0);
							int height = size.SubstringAfter("x").To(0);
							double frameRate = streamDic.Get("fps").To(0.0d);

							VideoStreamInfo stream = new VideoStreamInfo
							{
								iTag = itag,
								Url = url,
								ContentLength = contentLength,
								Bitrate = bitrate,
								VideoSize = new VideoSize(width, height),
								VideoFrameRate = frameRate
							};
							videoStreams.Add(itag, stream);
						}
						break;
					default:
						continue;
				}
			}

			string title = videoInfoDic.Get("title");
			TimeSpan duration = TimeSpan.FromSeconds(videoInfoDic.Get("length_seconds").To(0.0d));
			long viewCount = videoInfoDic.Get("view_count").To(0L);
			string[] keywords = videoInfoDic.Get("keywords").Split(",");
			string[] watermarks = videoInfoDic.Get("watermark").Split(",");
			bool isListed = videoInfoDic.GetOrDefault("is_listed") == "1"; // https://github.com/Tyrrrz/YoutubeExplode/issues/45
			bool isRatingAllowed = videoInfoDic.Get("allow_ratings") == "1";
			bool isMuted = videoInfoDic.Get("muted") == "1";
			bool isEmbeddingAllowed = videoInfoDic.Get("allow_embed") == "1";

			// Parse mixed streams
			string mixedStreamsEncoded = videoInfoDic.GetOrDefault("url_encoded_fmt_stream_map");

			if (!string.IsNullOrEmpty(mixedStreamsEncoded))
			{
				foreach (string streamEncoded in mixedStreamsEncoded.Split(','))
				{
					IDictionary<string, string> streamDic = UriHelper.GetDictionaryFromUrlQuery(streamEncoded);
					int itag = streamDic.Get("itag").To(0);
					if (mixedStreams.ContainsKey(itag)) continue;
                    if (!MediaStreamInfo.IsKnown(itag)) continue;

					string url = streamDic.Get("url");
					string sig = streamDic.GetOrDefault("s");

					// Decipher signature if needed
					if (!string.IsNullOrEmpty(sig))
					{
						sig = playerSource.Decipher(sig);
						url = UriHelper.SetQueryParameter(url, SIGNATURE_QUERY, sig);
						if (streamDic.ContainsKey("fallback_host")) url += "&fallback_host=" + streamDic.Get("fallback_host");
					}

					url = UriHelper.SetQueryParameter(url, RATE_BYPASS_FLAG, "yes");
					// Get content length
					long contentLength;

					using (HttpRequestMessage reqMsg = new HttpRequestMessage(HttpMethod.Head, url))
					{
						using HttpResponseMessage resMsg = await Service.PerformRequestAsync(reqMsg).ConfigureAwait();
						// Check status code (https://github.com/Tyrrrz/YoutubeExplode/issues/36)
						if (resMsg.StatusCode == HttpStatusCode.NotFound || resMsg.StatusCode == HttpStatusCode.Gone) continue;
						resMsg.EnsureSuccessStatusCode();
						contentLength = resMsg.Content.Headers.ContentLength ?? -1;
						if (contentLength <= 0) continue;
					}

					MixedStreamInfo stream = new MixedStreamInfo
					{
						iTag = itag,
						Url = url,
						ContentLength = contentLength
					};
					mixedStreams.Add(itag, stream);
				}
			}

			string adaptiveStreamsEncoded = videoInfoDic.GetOrDefault("adaptive_fmts");

			if (!string.IsNullOrEmpty(adaptiveStreamsEncoded))
			{
				foreach (string streamEncoded in adaptiveStreamsEncoded.Split(","))
				{
					IDictionary<string, string> streamDic = UriHelper.GetDictionaryFromUrlQuery(streamEncoded);

					long contentLength = streamDic.Get("clen").To(0L);
					if (contentLength <= 0) continue;

					int itag = streamDic.Get("itag").To(0);
					bool isAudio = streamDic.Get("type")?.Contains("audio/") ?? false;

					if (isAudio)
					{
						if (audioStreams.ContainsKey(itag)) continue;
					}
					else
					{
						if (videoStreams.ContainsKey(itag)) continue;
					}

                    if (!MediaStreamInfo.IsKnown(itag)) continue;

					string url = streamDic.Get("url");
					string sig = streamDic.GetOrDefault("s");
					long bitrate = streamDic.Get("bitrate").To(0L);

					// Decipher signature if needed
					if (!string.IsNullOrEmpty(sig))
					{
						sig = playerSource.Decipher(sig);
						url = UriHelper.SetQueryParameter(url, SIGNATURE_QUERY, sig);
						if (streamDic.ContainsKey("fallback_host")) url += "&fallback_host=" + streamDic.Get("fallback_host");
					}

					// Set rate bypass
					url = UriHelper.SetQueryParameter(url, RATE_BYPASS_FLAG, "yes");

					// If audio stream
					if (isAudio)
					{
						AudioStreamInfo stream = new AudioStreamInfo
						{
							iTag = itag,
							Url = url,
							ContentLength = contentLength,
							Bitrate = bitrate
						};
						audioStreams.Add(itag, stream);
					}
					// If video stream
					else
					{
						// Parse additional data
						string size = streamDic.Get("size");
						int width = size.SubstringUntil("x").To(0);
						int height = size.SubstringAfter("x").To(0);
						double frameRate = streamDic.Get("fps").To(0.0d);

						VideoStreamInfo stream = new VideoStreamInfo
						{
							iTag = itag,
							Url = url,
							ContentLength = contentLength,
							Bitrate = bitrate,
							VideoSize = new VideoSize(width, height),
							VideoFrameRate = frameRate
						};
						videoStreams.Add(itag, stream);
					}
				}
			}

			// Parse adaptive streams from dash
			string dashManifestUrl = videoInfoDic.GetOrDefault("dashmpd");

			if (!string.IsNullOrEmpty(dashManifestUrl))
			{
				// Parse signature
				string sig = Regex.Match(dashManifestUrl, "/s/(.*?)(?:/|$)").Groups[1].Value;

				// Decipher signature if needed
				if (!string.IsNullOrEmpty(sig))
				{
					sig = playerSource.Decipher(sig);
					dashManifestUrl = UriHelper.SetPathParameter(dashManifestUrl, SIGNATURE_QUERY, sig);
				}

				// Get the manifest
				response = await Service.GetStringAsync(dashManifestUrl).ConfigureAwait();
				XElement dashManifestXml = XElement.Parse(response).StripNamespaces();
				IEnumerable<XElement> streamsXml = dashManifestXml.Descendants("Representation");

				// Filter out partial streams
				streamsXml = streamsXml
					.Where(x => !(x.Descendants("Initialization")
									.FirstOrDefault()
									?.Attribute("sourceURL")
									?.Value.Contains("sq/") ?? false));

				// Parse streams
				foreach (XElement streamXml in streamsXml)
				{
					int itag = (int)streamXml.AttributeStrict("id");
					bool isAudio = streamXml.Element("AudioChannelConfiguration") != null;

					if (isAudio)
					{
						if (audioStreams.ContainsKey(itag)) continue;
					}
					else
					{
						if (videoStreams.ContainsKey(itag)) continue;
					}

                    if (!MediaStreamInfo.IsKnown(itag)) continue;

					string url = streamXml.ElementStrict("BaseURL").Value;
					long bitrate = (long)streamXml.AttributeStrict("bandwidth");

					// Parse content length
					long contentLength = Regex.Match(url, @"clen[/=](\d+)").Groups[1].Value.To(0L);

					// Set rate bypass
					url = url.Contains("&")
						? UriHelper.SetQueryParameter(url, RATE_BYPASS_FLAG, "yes")
						: UriHelper.SetPathParameter(url, RATE_BYPASS_FLAG, "yes");

					// If audio stream
					if (isAudio)
					{
						AudioStreamInfo stream = new AudioStreamInfo
						{
							iTag = itag,
							Url = url,
							ContentLength = contentLength,
							Bitrate = bitrate
						};
						audioStreams.Add(itag, stream);
					}
					// If video stream
					else
					{
						// Parse additional data
						int width = (int)streamXml.AttributeStrict("width");
						int height = (int)streamXml.AttributeStrict("height");
						double frameRate = (double)streamXml.AttributeStrict("frameRate");

						VideoStreamInfo stream = new VideoStreamInfo
						{
							iTag = itag,
							Url = url,
							ContentLength = contentLength,
							Bitrate = bitrate,
							VideoSize = new VideoSize(width, height),
							VideoFrameRate = frameRate
						};
						videoStreams.Add(itag, stream);
					}
				}
			}

			// Parse closed caption tracks
			List<ClosedCaptionTrackInfo> captions = new List<ClosedCaptionTrackInfo>();
			string captionsEncoded = videoInfoDic.GetOrDefault("caption_tracks");

			if (!string.IsNullOrEmpty(captionsEncoded))
			{
				foreach (string captionEncoded in captionsEncoded.Split(","))
				{
					IDictionary<string, string> captionDic = UriHelper.GetDictionaryFromUrlQuery(captionEncoded);

					string url = captionDic.Get("u");
					bool isAuto = captionDic.Get("v").Contains("a.");
					string code = captionDic.Get("lc");
					string name = captionDic.Get("n");

					ClosedCaptionTrackInfo caption = new ClosedCaptionTrackInfo
					{
						Url = url,
						Language = new Language(code, name),
						IsAutoGenerated = isAuto
					};
					captions.Add(caption);
				}
			}

			// Get metadata extension
			request = $"https://www.youtube.com/get_video_metadata?video_id={videoId}";
			response = await Service.GetStringAsync(request).ConfigureAwait();
			XElement videoInfoExtXml = XElement.Parse(response).StripNamespaces().ElementStrict("html_content");

			// Parse
			string description = videoInfoExtXml.ElementStrict("video_info").ElementStrict("description").Value;
			long likeCount = (long)videoInfoExtXml.ElementStrict("video_info").ElementStrict("likes_count_unformatted");
			long dislikeCount = (long)videoInfoExtXml.ElementStrict("video_info").ElementStrict("dislikes_count_unformatted");

			// Parse author info
			string authorId = videoInfoExtXml.ElementStrict("user_info").ElementStrict("channel_external_id").Value;
			string authorName = videoInfoExtXml.ElementStrict("user_info").ElementStrict("username").Value;
			string authorTitle = videoInfoExtXml.ElementStrict("user_info").ElementStrict("channel_title").Value;
			bool authorIsPaid = videoInfoExtXml.ElementStrict("user_info").ElementStrict("channel_paid").Value == "1";
			string authorLogoUrl = videoInfoExtXml.ElementStrict("user_info").ElementStrict("channel_logo_url").Value;
			string authorBannerUrl = videoInfoExtXml.ElementStrict("user_info").ElementStrict("channel_banner_url").Value;
			ChannelInfo author = new ChannelInfo
			{
				Id = authorId,
				Name = authorName,
				Title = authorTitle,
				IsPaid = authorIsPaid,
				LogoUrl = authorLogoUrl,
				BannerUrl = authorBannerUrl
			};

			return new VideoInfo
			{
				Id = videoId,
				Title = title,
				Author = author,
				Duration = duration,
				Description = description,
				Keywords = keywords,
				Watermarks = watermarks,
				ViewCount = viewCount,
				LikeCount = likeCount,
				DislikeCount = dislikeCount,
				IsListed = isListed,
				IsRatingAllowed = isRatingAllowed,
				IsMuted = isMuted,
				IsEmbeddingAllowed = isEmbeddingAllowed,
				MixedStreams = mixedStreams.Values.OrderByDescending(s => s.VideoQuality).ToArray(),
				AudioStreams = audioStreams.Values.OrderByDescending(s => s.Bitrate).ToArray(),
				VideoStreams = videoStreams.Values.OrderByDescending(s => s.VideoSize.Value).ToArray(),
				ClosedCaptionTracks = captions
			};
		}

		/// <summary>
		///     Gets play list info by ID, truncating resulting video list at given number of pages (1 page ≤ 200 videos)
		/// </summary>
		[ItemNotNull]
		public async Task<PlaylistInfo> GetPlaylistInfoAsync([NotNull] string playlistId, int maxPages)
		{
			if (playlistId == null) throw new ArgumentNullException(nameof(playlistId));
			if (!ValidatePlaylistId(playlistId)) throw new ArgumentException("Invalid Youtube play list ID", nameof(playlistId));
			if (maxPages <= 0) throw new ArgumentOutOfRangeException(nameof(maxPages));

			// Get all videos across pages
			int pagesDone = 0;
			int offset = 0;
			XElement playlistInfoXml;
			List<VideoInfoSnippet> videos = new List<VideoInfoSnippet>();
			HashSet<string> videoIds = new HashSet<string>();

			do
			{
				// Get
				string request = $"https://www.youtube.com/list_ajax?style=xml&action_get_list=1&list={playlistId}&index={offset}";
				string response = await Service.GetStringAsync(request).ConfigureAwait();
				playlistInfoXml = XElement.Parse(response).StripNamespaces();

				// Parse videos
				int total = 0;
				int delta = 0;

				foreach (XElement videoInfoSnippetXml in playlistInfoXml.Elements("video"))
				{
					// Basic info
					string videoId = videoInfoSnippetXml.ElementStrict("encrypted_id").Value;
					string videoTitle = videoInfoSnippetXml.ElementStrict("title").Value;
					TimeSpan videoDuration = TimeSpan.FromSeconds(videoInfoSnippetXml.ElementStrict("length_seconds").Value.To(0.0d));
					string videoDescription = videoInfoSnippetXml.ElementStrict("description").Value;
					long videoViewCount = Regex.Replace(videoInfoSnippetXml.ElementStrict("views").Value, @"\D", string.Empty).To(0L);
					long videoLikeCount = Regex.Replace(videoInfoSnippetXml.ElementStrict("likes").Value, @"\D", string.Empty).To(0L);
					long videoDislikeCount = Regex.Replace(videoInfoSnippetXml.ElementStrict("dislikes").Value, @"\D", string.Empty).To(0L);

					// Keywords
					string videoKeywordsJoined = videoInfoSnippetXml.ElementStrict("keywords").Value;
					string[] videoKeywords = Regex
						.Matches(videoKeywordsJoined, @"(?<=(^|\s)(?<quote>""?))([^""]|(""""))*?(?=\<quote>(?=\s|$))")
						.Cast<Match>()
						.Select(m => m.Value)
						.Where(s => !string.IsNullOrEmpty(s))
						.ToArray();

					VideoInfoSnippet snippet = new VideoInfoSnippet
					{
						Id = videoId,
						Title = videoTitle,
						Duration = videoDuration,
						Description = videoDescription,
						Keywords = videoKeywords,
						ViewCount = videoViewCount,
						LikeCount = videoLikeCount,
						DislikeCount = videoDislikeCount
					};

					// Add to list if not already there
					if (videoIds.Add(snippet.Id))
					{
						videos.Add(snippet);
						delta++;
					}
					total++;
				}

				// Break if the videos started repeating
				if (delta <= 0) break;

				// Prepare for next page
				pagesDone++;
				offset += total;
			}
			while (pagesDone < maxPages);

			// Parse metadata
			string title = playlistInfoXml.ElementStrict("title").Value;
			string author = playlistInfoXml.Element("author")?.Value ?? string.Empty;
			string description = playlistInfoXml.ElementStrict("description").Value;
			long viewCount = (long)playlistInfoXml.ElementStrict("views");

			return new PlaylistInfo
			{
				Id = playlistId,
				Title = title,
				Author = author,
				Description = description,
				ViewCount = viewCount,
				Videos = videos
			};
		}

		/// <summary>
		///     Gets play list info by ID
		/// </summary>
		public async Task<PlaylistInfo> GetPlaylistInfoAsync([NotNull] string playlistId) { return await GetPlaylistInfoAsync(playlistId, int.MaxValue).ConfigureAwait(); }

		/// <summary>
		///     Gets videos uploaded to a channel with given ID, truncating resulting video list at given number of pages (1 page ≤
		///     200 videos)
		/// </summary>
		public async Task<IEnumerable<VideoInfoSnippet>> GetChannelUploadsAsync([NotNull] string channelId, int maxPages)
		{
			if (channelId == null) throw new ArgumentNullException(nameof(channelId));
			if (!ValidateChannelId(channelId)) throw new ArgumentException("Invalid Youtube channel ID", nameof(channelId));
			if (maxPages <= 0) throw new ArgumentOutOfRangeException(nameof(maxPages));

			// Compose a play list ID
			string playlistId = "UU" + channelId.SubstringAfter("UC");

			// Get play list info
			PlaylistInfo playlistInfo = await GetPlaylistInfoAsync(playlistId, maxPages).ConfigureAwait();
			return playlistInfo.Videos;
		}

		/// <summary>
		///     Gets videos uploaded to a channel with given ID
		/// </summary>
		public async Task<IEnumerable<VideoInfoSnippet>> GetChannelUploadsAsync([NotNull] string channelId)
		{
			return await GetChannelUploadsAsync(channelId, int.MaxValue)
						.ConfigureAwait();
		}

		/// <summary>
		///     Gets channel info by ID
		/// </summary>
		public async Task<ChannelInfo> GetChannelInfoAsync([NotNull] string channelId)
		{
			if (channelId == null) throw new ArgumentNullException(nameof(channelId));
			if (!ValidateChannelId(channelId)) throw new ArgumentException("Invalid Youtube channel ID", nameof(channelId));

			// Get channel uploads
			IEnumerable<VideoInfoSnippet> uploads = await GetChannelUploadsAsync(channelId, 1).ConfigureAwait();
			VideoInfoSnippet videoInfoSnippet = uploads.FirstOrDefault();
			if (videoInfoSnippet == null) throw new ParseException("Cannot get channel info because it doesn't have any uploaded videos.");

			// Get video info of the first video
			VideoInfo videoInfo = await GetVideoInfoAsync(videoInfoSnippet.Id).ConfigureAwait();
			return videoInfo.Author;
		}

		/// <summary>
		///     Gets the actual media stream represented by given metadata
		/// </summary>
		[ItemNotNull]
		public async Task<MediaStream> GetMediaStreamAsync([NotNull] MediaStreamInfo mediaStreamInfo)
		{
			if (mediaStreamInfo == null) throw new ArgumentNullException(nameof(mediaStreamInfo));

			Stream stream = await Service.GetStreamAsync(mediaStreamInfo.Url).ConfigureAwait();
			return new MediaStream(mediaStreamInfo, stream);
		}

		/// <summary>
		///     Gets the actual closed caption track represented by given metadata
		/// </summary>
		[ItemNotNull]
		public async Task<ClosedCaptionTrack> GetClosedCaptionTrackAsync([NotNull] ClosedCaptionTrackInfo closedCaptionTrackInfo)
		{
			if (closedCaptionTrackInfo == null) throw new ArgumentNullException(nameof(closedCaptionTrackInfo));

			string response = await Service.GetStringAsync(closedCaptionTrackInfo.Url).ConfigureAwait();
			XElement captionTrackXml = XElement.Parse(response).StripNamespaces();

			List<ClosedCaption> captions = new List<ClosedCaption>();

			foreach (XElement captionXml in captionTrackXml.Descendants("text"))
			{
				string text = captionXml.Value;
				TimeSpan offset = TimeSpan.FromSeconds((double)captionXml.AttributeStrict("start"));
				TimeSpan duration = TimeSpan.FromSeconds((double)captionXml.AttributeStrict("dur"));

				ClosedCaption caption = new ClosedCaption
				{
					Text = text,
					Offset = offset,
					Duration = duration
				};
				captions.Add(caption);
			}

			return new ClosedCaptionTrack
			{
				Info = closedCaptionTrackInfo,
				Captions = captions
			};
		}

		public async Task DownloadMediaStreamAsync([NotNull] MediaStreamInfo mediaStreamInfo, [NotNull] string filePath, IProgress<double> progress, CancellationToken cancellationToken, int bufferSize)
		{
			if (bufferSize <= 0) throw new ArgumentOutOfRangeException(nameof(bufferSize));

			// Save to file
			using MediaStream input = await GetMediaStreamAsync(mediaStreamInfo).ConfigureAwait();
			using FileStream output = File.Create(filePath, bufferSize);
			byte[] buffer = new byte[bufferSize];
			int bytesRead;
			long totalBytesRead = 0;

			do
			{
				totalBytesRead += bytesRead = await input.ReadAsync(buffer, cancellationToken).ConfigureAwait();
				await output.WriteAsync(buffer, 0, bytesRead, cancellationToken).ConfigureAwait();
				progress?.Report(1.0 * totalBytesRead / input.Length);
			}
			while (bytesRead > 0);
		}

		/// <summary>
		///     Downloads a media stream to file
		/// </summary>
		public async Task DownloadMediaStreamAsync([NotNull] MediaStreamInfo mediaStreamInfo, [NotNull] string filePath, IProgress<double> progress, CancellationToken cancellationToken)
		{
			await DownloadMediaStreamAsync(mediaStreamInfo, filePath, progress, cancellationToken, 4096).ConfigureAwait();
		}

		/// <summary>
		///     Downloads a media stream to file
		/// </summary>
		public async Task DownloadMediaStreamAsync([NotNull] MediaStreamInfo mediaStreamInfo, [NotNull] string filePath, IProgress<double> progress)
		{
			await DownloadMediaStreamAsync(mediaStreamInfo, filePath, progress, CancellationToken.None).ConfigureAwait();
		}

		/// <summary>
		///     Downloads a media stream to file
		/// </summary>
		public async Task DownloadMediaStreamAsync([NotNull] MediaStreamInfo mediaStreamInfo, [NotNull] string filePath)
		{
			await DownloadMediaStreamAsync(mediaStreamInfo, filePath, null).ConfigureAwait();
		}

		/// <summary>
		///     Downloads a closed caption track to file
		/// </summary>
		public async Task DownloadClosedCaptionTrackAsync([NotNull] ClosedCaptionTrackInfo closedCaptionTrackInfo,
			[NotNull] string filePath, IProgress<double> progress, CancellationToken cancellationToken, int bufferSize)
		{
			if (bufferSize <= 0) throw new ArgumentOutOfRangeException(nameof(bufferSize));

			// Get and create streams
			ClosedCaptionTrack closedCaptionTrack = await GetClosedCaptionTrackAsync(closedCaptionTrackInfo).ConfigureAwait();

			// Save to file as SRT
			using FileStream output = File.Create(filePath, bufferSize);
			using StreamWriter sw = new StreamWriter(output, Encoding.Unicode, bufferSize);

			for (int i = 0; i < closedCaptionTrack.Captions.Count; i++)
			{
				// Make sure cancellation was not requested
				cancellationToken.ThrowIfCancellationRequested();

				ClosedCaption closedCaption = closedCaptionTrack.Captions[i];
				StringBuilder buffer = new StringBuilder();

				// Line number
				buffer.AppendLine((i + 1).ToString());

				// Time start --> time end
				buffer.Append(closedCaption.Offset.ToString(@"hh\:mm\:ss\,fff"));
				buffer.Append(" --> ");
				buffer.Append((closedCaption.Offset + closedCaption.Duration).ToString(@"hh\:mm\:ss\,fff"));
				buffer.AppendLine();

				// Actual text
				buffer.AppendLine(closedCaption.Text);

				// Write to stream
				await sw.WriteLineAsync(buffer.ToString()).ConfigureAwait();

				// Report progress
				progress?.Report((i + 1.0) / closedCaptionTrack.Captions.Count);
			}
		}

		/// <summary>
		///     Downloads a closed caption track to file
		/// </summary>
		public async Task DownloadClosedCaptionTrackAsync([NotNull] ClosedCaptionTrackInfo closedCaptionTrackInfo, [NotNull] string filePath, IProgress<double> progress,
			CancellationToken cancellationToken)
		{
			await DownloadClosedCaptionTrackAsync(closedCaptionTrackInfo, filePath, progress, cancellationToken, 4096).ConfigureAwait();
		}

		/// <summary>
		///     Downloads a closed caption track to file
		/// </summary>
		public async Task DownloadClosedCaptionTrackAsync([NotNull] ClosedCaptionTrackInfo closedCaptionTrackInfo, [NotNull] string filePath, IProgress<double> progress)
		{
			await DownloadClosedCaptionTrackAsync(closedCaptionTrackInfo, filePath, progress, CancellationToken.None).ConfigureAwait();
		}

		/// <summary>
		///     Downloads a closed caption track to file
		/// </summary>
		public async Task DownloadClosedCaptionTrackAsync([NotNull] ClosedCaptionTrackInfo closedCaptionTrackInfo, [NotNull] string filePath)
		{
			await DownloadClosedCaptionTrackAsync(closedCaptionTrackInfo, filePath, null).ConfigureAwait();
		}

		[ItemNotNull]
		private async Task<PlayerContext> GetPlayerContextAsync(string videoId)
		{
			string version = null;
			string sts = null;
			int tries = 0;
			const int MAX_TRIES = 10;

			// Request with retry (https://github.com/Tyrrrz/YoutubeExplode/issues/38)
			while (tries++ <= MAX_TRIES && (string.IsNullOrEmpty(version) || string.IsNullOrEmpty(sts)))
			{
				string request = $"https://www.youtube.com/embed/{videoId}";
				string response = await Service.GetStringAsync(request).ConfigureAwait();

				version = Regex.Match(response, @"<script.*?\ssrc=""/yts/jsbin/player-(.*?)/base.js").Groups[1].Value;
				sts = Regex.Match(response, @"""sts""\s*:\s*(\d+)").Groups[1].Value;
			}

			// Check if successful
			if (string.IsNullOrEmpty(version) || string.IsNullOrEmpty(sts)) throw new ParseException("Could not parse player context");
			return new PlayerContext(version, sts);
		}

		[ItemNotNull]
		private async Task<PlayerSource> GetPlayerSourceAsync([NotNull] string version)
		{
			// Original code credit: Decipherer class of https://github.com/flagbug/YoutubeExtractor

			// Try to resolve from cache first
			PlayerSource playerSource = _playerSourceCache.GetOrDefault(version);
			if (playerSource != null) return playerSource;

			// Get player source code
			string request = $"https://www.youtube.com/yts/jsbin/player-{version}/base.js";
			string response = await Service.GetStringAsync(request).ConfigureAwait();

			// Find the name of the function that handles deciphering
			string funcName = Regex.Match(response, @"\""signature"",\s?([a-zA-Z0-9\$]+)\(").Groups[1].Value;
			if (string.IsNullOrEmpty(funcName)) throw new ParseException("Could not find the entry function for signature deciphering");

			// Find the body of the function
			string funcPattern = @"(?!h\.)" + Regex.Escape(funcName) + @"=function\(\w+\)\{(.*?)\}";
			string funcBody = Regex.Match(response, funcPattern, RegexHelper.OPTIONS_I).Groups[1].Value;
			if (string.IsNullOrEmpty(funcBody)) throw new ParseException("Could not find the signature decipher function body");
			string[] funcLines = funcBody.Split(StringSplitOptions.RemoveEmptyEntries, ';');

			// Identify cipher functions
			string reverseFuncName = null;
			string sliceFuncName = null;
			string charSwapFuncName = null;
			List<ICipherOperation> operations = new List<ICipherOperation>();

			// Analyze the function body to determine the names of cipher functions
			foreach (string line in funcLines)
			{
				// Break when all functions are found
				if (!string.IsNullOrEmpty(reverseFuncName) && !string.IsNullOrEmpty(sliceFuncName) && !string.IsNullOrEmpty(charSwapFuncName)) break;

				// Get the function called on this line
				string calledFunctionName = Regex.Match(line, @"\w+\.(\w+)\(").Groups[1].Value;
				if (string.IsNullOrEmpty(calledFunctionName)) continue;

				// Find cipher function names
				if (Regex.IsMatch(response, $@"{Regex.Escape(calledFunctionName)}:\bfunction\b\(\w+\)"))
				{
					reverseFuncName = calledFunctionName;
				}
				else if (Regex.IsMatch(response, $@"{Regex.Escape(calledFunctionName)}:\bfunction\b\([a],b\).(\breturn\b)?.?\w+\."))
				{
					sliceFuncName = calledFunctionName;
				}
				else if (Regex.IsMatch(response, $@"{Regex.Escape(calledFunctionName)}:\bfunction\b\(\w+\,\w\).\bvar\b.\bc=a\b"))
				{
					charSwapFuncName = calledFunctionName;
				}
			}

			// Analyze the function body again to determine the operation set and order
			foreach (string line in funcLines)
			{
				// Get the function called on this line
				string calledFunctionName = Regex.Match(line, @"\w+\.(\w+)\(").Groups[1].Value;
				if (string.IsNullOrEmpty(calledFunctionName)) continue;

				// Swap operation
				if (calledFunctionName == charSwapFuncName)
				{
					int index = Regex.Match(line, @"\(\w+,(\d+)\)").Groups[1].Value.To(0);
					operations.Add(new SwapCipherOperation(index));
				}
				// Slice operation
				else if (calledFunctionName == sliceFuncName)
				{
					int index = Regex.Match(line, @"\(\w+,(\d+)\)").Groups[1].Value.To(0);
					operations.Add(new SliceCipherOperation(index));
				}
				// Reverse operation
				else if (calledFunctionName == reverseFuncName)
				{
					operations.Add(new ReverseCipherOperation());
				}
			}

			return _playerSourceCache[version] = new PlayerSource(version, operations);
		}

		[NotNull]
		public static YoutubeClient Instance => _instance ??= new YoutubeClient();

		/// <summary>
		///     Verifies that the given string is syntactically a valid Youtube video ID
		/// </summary>
		public static bool ValidateVideoId(string videoId)
		{
			if (string.IsNullOrEmpty(videoId)) return false;
			if (videoId.Length != 11) return false;
			return !Regex.IsMatch(videoId, @"[^0-9a-zA-Z_\-]");
		}

		/// <summary>
		///     Tries to parse video ID from a Youtube video URL
		/// </summary>
		public static bool TryParseVideoId(string videoUrl, out string videoId)
		{
			videoId = default(string);

			if (string.IsNullOrEmpty(videoUrl)) return false;

			// https://www.youtube.com/watch?v=yIVRs6YSbOM
			string regularMatch = Regex.Match(videoUrl, @"youtube\..+?/watch.*?v=(.*?)(?:&|/|$)").Groups[1].Value;

			if (!string.IsNullOrEmpty(regularMatch) && ValidateVideoId(regularMatch))
			{
				videoId = regularMatch;
				return true;
			}

			// https://youtu.be/yIVRs6YSbOM
			string shortMatch = Regex.Match(videoUrl, @"youtu\.be/(.*?)(?:\?|&|/|$)").Groups[1].Value;

			if (!string.IsNullOrEmpty(shortMatch) && ValidateVideoId(shortMatch))
			{
				videoId = shortMatch;
				return true;
			}

			// https://www.youtube.com/embed/yIVRs6YSbOM
			string embedMatch = Regex.Match(videoUrl, @"youtube\..+?/embed/(.*?)(?:\?|&|/|$)").Groups[1].Value;

			if (!string.IsNullOrEmpty(embedMatch) && ValidateVideoId(embedMatch))
			{
				videoId = embedMatch;
				return true;
			}

			return false;
		}

		/// <summary>
		///     Parses video ID from a Youtube video URL
		/// </summary>
		public static string ParseVideoId([NotNull] string videoUrl)
		{
			if (videoUrl == null) throw new ArgumentNullException(nameof(videoUrl));

			bool success = TryParseVideoId(videoUrl, out string result);
			if (success) return result;

			throw new FormatException($"Could not parse video ID from given string [{videoUrl}]");
		}

		/// <summary>
		///     Verifies that the given string is syntactically a valid Youtube play list ID
		/// </summary>
		public static bool ValidatePlaylistId(string playlistId)
		{
			if (string.IsNullOrEmpty(playlistId)) return false;

			if (playlistId.Length != 2 &&
				playlistId.Length != 13 &&
				playlistId.Length != 18 &&
				playlistId.Length != 24 &&
				playlistId.Length != 26 &&
				playlistId.Length != 34) return false;

			return !Regex.IsMatch(playlistId, @"[^0-9a-zA-Z_\-]");
		}

		/// <summary>
		///     Tries to parse play list ID from a Youtube play list URL
		/// </summary>
		public static bool TryParsePlaylistId(string playlistUrl, out string playlistId)
		{
			playlistId = default(string);
			if (string.IsNullOrEmpty(playlistUrl)) return false;

			// https://www.youtube.com/playlist?list=PLOU2XLYxmsIJGErt5rrCqaSGTMyyqNt2H
			string regularMatch = Regex.Match(playlistUrl, @"youtube\..+?/playlist.*?list=(.*?)(?:&|/|$)").Groups[1].Value;

			if (!string.IsNullOrEmpty(regularMatch) && ValidatePlaylistId(regularMatch))
			{
				playlistId = regularMatch;
				return true;
			}

			// https://www.youtube.com/watch?v=b8m9zhNAgKs&list=PL9tY0BWXOZFuFEG_GtOBZ8-8wbkH-NVAr
			string compositeMatch = Regex.Match(playlistUrl, @"youtube\..+?/watch.*?list=(.*?)(?:&|/|$)").Groups[1].Value;

			if (!string.IsNullOrEmpty(compositeMatch) && ValidatePlaylistId(compositeMatch))
			{
				playlistId = compositeMatch;
				return true;
			}

			// https://youtu.be/b8m9zhNAgKs/?list=PL9tY0BWXOZFuFEG_GtOBZ8-8wbkH-NVAr
			string shortCompositeMatch = Regex.Match(playlistUrl, @"youtu\.be/.*?/.*?list=(.*?)(?:&|/|$)").Groups[1].Value;

			if (!string.IsNullOrEmpty(shortCompositeMatch) && ValidatePlaylistId(shortCompositeMatch))
			{
				playlistId = shortCompositeMatch;
				return true;
			}

			// https://www.youtube.com/embed/b8m9zhNAgKs/?list=PL9tY0BWXOZFuFEG_GtOBZ8-8wbkH-NVAr
			string embedCompositeMatch = Regex.Match(playlistUrl, @"youtube\..+?/embed/.*?/.*?list=(.*?)(?:&|/|$)").Groups[1].Value;

			if (!string.IsNullOrEmpty(embedCompositeMatch) && ValidatePlaylistId(embedCompositeMatch))
			{
				playlistId = embedCompositeMatch;
				return true;
			}

			return false;
		}

		/// <summary>
		///     Parses playlist ID from a Youtube playlist URL
		/// </summary>
		public static string ParsePlaylistId([NotNull] string playlistUrl)
		{
			if (playlistUrl == null) throw new ArgumentNullException(nameof(playlistUrl));

			bool success = TryParsePlaylistId(playlistUrl, out string result);
			if (success) return result;

			throw new FormatException($"Could not parse playlist ID from given string [{playlistUrl}]");
		}

		/// <summary>
		///     Verifies that the given string is syntactically a valid Youtube channel ID
		/// </summary>
		public static bool ValidateChannelId(string channelId)
		{
			if (string.IsNullOrEmpty(channelId)) return false;

			if (channelId.Length != 24) return false;

			if (!channelId.StartsWith("UC", StringComparison.OrdinalIgnoreCase)) return false;

			return !Regex.IsMatch(channelId, @"[^0-9a-zA-Z_\-]");
		}

		/// <summary>
		///     Tries to parse channel ID from a Youtube channel URL
		/// </summary>
		public static bool TryParseChannelId(string channelUrl, out string channelId)
		{
			channelId = default(string);

			if (string.IsNullOrEmpty(channelUrl)) return false;

			// https://www.youtube.com/channel/UC3xnGqlcL3y-GXz5N3wiTJQ
			string regularMatch =
				Regex.Match(channelUrl, @"youtube\..+?/channel/(.*?)(?:&|/|$)").Groups[1].Value;
			if (!string.IsNullOrEmpty(regularMatch) && ValidateChannelId(regularMatch))
			{
				channelId = regularMatch;
				return true;
			}

			return false;
		}

		/// <summary>
		///     Parses channel ID from a Youtube channel URL
		/// </summary>
		public static string ParseChannelId([NotNull] string channelUrl)
		{
			if (channelUrl == null) throw new ArgumentNullException(nameof(channelUrl));

			bool success = TryParseChannelId(channelUrl, out string result);
			if (success) return result;

			throw new FormatException($"Could not parse channel ID from given string [{channelUrl}]");
		}
	}
}
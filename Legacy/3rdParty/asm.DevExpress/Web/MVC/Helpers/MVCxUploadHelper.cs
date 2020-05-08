using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Hosting;
using System.Web.UI.WebControls;
using asm.Extensions;
using asm.Helpers;
using DevExpress.Utils;
using DevExpress.Web;
using DevExpress.Web.Mvc;
using JetBrains.Annotations;

namespace asm.DevExpress.Web.MVC.Helpers
{
	public class MVCxUploadHelper
	{
		public static UploadControlSettings Settings([NotNull] object routeValues, string name = null,
			int width = -1, string externalDropZoneID = null, string tmpUrl = null, UploadVisibleElements elements = UploadVisibleElements.TextBox, 
			Action<UploadControlValidationSettings> onValidationSettings = null, Action<UploadControlSettings> onSettingsCreated = null, Action<UploadControlClientSideEvents> onClientSideEvents = null)
		{
			name = name?.Trim();
			if (string.IsNullOrEmpty(name)) name = "Uploader";
			if (width <= 0) width = 480;
			externalDropZoneID = externalDropZoneID?.Trim();

			string tmpPath = null;
			string rootPath = HostingEnvironment.MapPath(HttpRuntime.AppDomainAppVirtualPath);

			if (!string.IsNullOrEmpty(rootPath))
			{
				StringBuilder path = new StringBuilder(rootPath).AppendIfDoesNotEndWith(Path.DirectorySeparatorChar);

				if (!string.IsNullOrEmpty(tmpUrl))
				{
					path.Append(tmpUrl.Trim('~', '/')
									.Replace('/', '\\'));
				}
				else
					path.Append("tmp");

				tmpPath = path.ToString();
				DirectoryHelper.Ensure(tmpPath);
			}

			bool rtl = Thread.CurrentThread.CurrentUICulture.IsRightToLeft();

			UploadControlSettings settings = new UploadControlSettings
			{
				Name = name,
				CallbackRouteValues = routeValues,
				UploadMode = UploadControlUploadMode.Auto,
				Width = width < 1 ? Unit.Percentage(100) : Unit.Pixel(width),
				EncodeHtml = true,
				RightToLeft = rtl ? DefaultBoolean.True : DefaultBoolean.False,
				ShowUploadButton = elements.HasFlag(UploadVisibleElements.UploadButton),
				ShowClearFileSelectionButton = elements.HasFlag(UploadVisibleElements.ClearButton),
				ShowAddRemoveButtons = elements.HasFlag(UploadVisibleElements.AddRemoveButton),
				ShowTextBox = elements.HasFlag(UploadVisibleElements.TextBox),
				ShowProgressPanel = elements.HasFlag(UploadVisibleElements.Progress),
				UploadButton =
				{
					Text = @"<span class=""fa fa-upload""></span>"
				},
				CancelButton =
				{
					Text = @"<span class=""fa fa-times""></span>"
				},
				BrowseButton =
				{
					Text = @"<span class=""fa fa-ellipsis-h""></span>"
				},
				AddButton =
				{
					Text = @"<span class=""fa fa-plus""></span>"
				},
				RemoveButton =
				{
					Text = @"<span class=""fa fa-times""></span>"
				},
				ProgressBarSettings =
				{
					DisplayMode = ProgressBarDisplayMode.Percentage
				},
				AdvancedModeSettings =
				{
					EnableFileList = elements.HasFlag(UploadVisibleElements.AddRemoveButton),
					EnableMultiSelect = elements.HasFlag(UploadVisibleElements.AddRemoveButton),
					DropZoneText = string.Empty,
					TemporaryFolder = tmpPath,
					UploadingExpirationTime = TimeSpan.FromHours(1)
				},
				ValidationSettings =
				{
					ErrorStyle =
					{
						CssClass = "alert alert-danger"
					}
				},
				Styles =
				{
					Native = true,
					Button = { CssClass = "btn btn-default btn-xs" },
					ErrorMessage = { CssClass = "alert alert-danger" },
					DropZone = { CssClass = "uploadControlDropZone" },
					ProgressBar = { CssClass = "uploadControlProgressBar" }
				}
			};

			onValidationSettings?.Invoke(settings.ValidationSettings);

			if (!string.IsNullOrEmpty(externalDropZoneID))
			{
				settings.AutoStartUpload = true;
				settings.DialogTriggerID = externalDropZoneID;
				settings.AdvancedModeSettings.ExternalDropZoneID = externalDropZoneID;
				settings.AdvancedModeSettings.EnableDragAndDrop = true;
			}
			else if (!elements.HasFlag(UploadVisibleElements.UploadButton))
			{
				settings.AutoStartUpload = true;
			}

			onSettingsCreated?.Invoke(settings);
			onClientSideEvents?.Invoke(settings.ClientSideEvents);
			return settings;
		}
	}
}
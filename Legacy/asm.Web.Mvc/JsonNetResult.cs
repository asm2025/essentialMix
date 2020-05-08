using System;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using asm.Extensions;
using JetBrains.Annotations;
using asm.Newtonsoft.Helpers;
using Newtonsoft.Json;

namespace asm.Web.Mvc
{
	/// <summary>
	/// Must register and use asm.MVC.Annotations.JsonHandlerAttribute
	/// </summary>
	public class JsonNetResult : JsonResult
	{
		public JsonNetResult()
		{
		}

		[NotNull]
		public JsonSerializerSettings Settings { get; } = JsonHelper.CreateSettings();

		public override void ExecuteResult([NotNull] ControllerContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (JsonRequestBehavior == JsonRequestBehavior.DenyGet && context.HttpContext.Request.HttpMethod.IsSame(HttpMethod.Get.Method)) throw new InvalidOperationException("JSON GET is not allowed");
			base.ExecuteResult(context);

			HttpResponseBase response = context.HttpContext.Response;
			response.ContentType = string.IsNullOrWhiteSpace(ContentType) ? "application/json" : ContentType;
			if (ContentEncoding != null) response.ContentEncoding = ContentEncoding;
			JsonSerializer scriptSerializer = JsonSerializer.Create(Settings);
			scriptSerializer.Serialize(response.Output, Data);
		}

		[NotNull]
		public static JsonNetResult FromJsonResult([NotNull] JsonResult jsonResult)
		{
			return new JsonNetResult
			{
				ContentEncoding = jsonResult.ContentEncoding,
				ContentType = !string.IsNullOrWhiteSpace(jsonResult.ContentType)
								? jsonResult.ContentType
								: "application/json",
				Data = jsonResult.Data,
				JsonRequestBehavior = jsonResult.JsonRequestBehavior
			};
		}
	}
}
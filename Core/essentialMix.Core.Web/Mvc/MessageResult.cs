using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace essentialMix.Core.Web.Mvc
{
	public class MessageResult : ResponseResult
	{
		/// <inheritdoc />
		public MessageResult(HttpStatusCode statusCode, string message)
		{
			StatusCode = statusCode;
			Message = message;
		}

		public HttpStatusCode StatusCode { get; set; }
		public string Message { get; set; }

		/// <inheritdoc />
		public override Task ExecuteResultAsync(ActionContext context)
		{
			ObjectResult result = new ObjectResult(Message)
			{
				StatusCode = (int)StatusCode,
				DeclaredType = typeof(string)
			};

			return result.ExecuteResultAsync(context);
		}
	}
}
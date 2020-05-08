using System.Threading.Tasks;
using asm.Logging.Helpers;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace asm.Core.Web.Middleware
{
	public abstract class MiddlewareBase
	{
		/// <inheritdoc />
		protected MiddlewareBase(RequestDelegate next, ILogger logger)
		{
			Next = next;
			Logger = logger ?? LogHelper.Empty;
		}

		protected RequestDelegate Next { get; }

		[NotNull]
		protected ILogger Logger { get; }

		public abstract Task Invoke(HttpContext context);
	}

	public abstract class MiddlewareBase<TOptions> : MiddlewareBase
		where TOptions : class, new()
	{
		/// <inheritdoc />
		protected MiddlewareBase(RequestDelegate next, IOptions<TOptions> options, ILogger logger)
			: base(next, logger)
		{
			Options = options;
		}

		protected IOptions<TOptions> Options { get; }
	}
}
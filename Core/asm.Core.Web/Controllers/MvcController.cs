﻿using System;
using System.Globalization;
using System.Threading;
using asm.Extensions;
using asm.Helpers;
using asm.Logging.Helpers;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace asm.Core.Web.Controllers
{
	public abstract class MvcController : Controller
	{
		private IConfiguration _configuration;
		private IWebHostEnvironment _environment;
		private ILogger _logger;

		/// <inheritdoc />
		protected MvcController(IConfiguration configuration)
			: this(configuration, null, null)
		{
		}

		/// <inheritdoc />
		protected MvcController(IWebHostEnvironment environment)
			: this(null, environment, null)
		{
		}

		/// <inheritdoc />
		protected MvcController(ILogger logger)
			: this(null, null, logger)
		{
		}

		/// <inheritdoc />
		protected MvcController(IConfiguration configuration, IWebHostEnvironment environment)
			: this(configuration, environment, null)
		{
		}

		/// <inheritdoc />
		protected MvcController(IConfiguration configuration, ILogger logger)
			: this(configuration, null, logger)
		{
		}

		/// <inheritdoc />
		protected MvcController(IWebHostEnvironment environment, ILogger logger)
			: this(null, environment, logger)
		{
		}

		/// <inheritdoc />
		protected MvcController(IConfiguration configuration, IWebHostEnvironment environment, ILogger logger)
		{
			_configuration = configuration;
			_environment = environment;
			_logger = logger;
		}

		[NotNull]
		public FormOptions FormOptions { get; } = new FormOptions();

		[NotNull]
		public CultureInfo UICulture => Thread.CurrentThread.CurrentUICulture;

		[NotNull]
		public CultureInfo Culture => Thread.CurrentThread.CurrentCulture;

		public bool IsRightToLeft => UICulture.IsRightToLeft();

		[NotNull]
		protected IConfiguration Configuration => _configuration ??= ControllerContext.HttpContext.RequestServices.GetRequiredService<IConfiguration>();

		[NotNull]
		protected IWebHostEnvironment Environment => _environment ??= ControllerContext.HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();

		[NotNull]
		protected ILogger Logger => _logger ??= ControllerContext.HttpContext.RequestServices.GetService<ILogger<ApiController>>() ?? LogHelper.Empty;

		[NotNull]
		protected IMemoryCache Cache { get; } = new MemoryCache(new MemoryCacheOptions());

		[AllowAnonymous]
		[HttpGet("[action]")]
		public virtual IActionResult ChangeCulture(string name, string returnUrl)
		{
			name = name?.Trim();
			if (!CultureInfoHelper.IsCultureName(name)) return LocalRedirect(returnUrl);
			Response.Cookies.Append(CookieRequestCultureProvider.DefaultCookieName, CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(name)), new CookieOptions
			{
				Secure = Request.IsHttps,
				IsEssential = true,
				Expires = DateTimeOffset.UtcNow.AddDays(7)
			});
			return LocalRedirect(returnUrl);
		}
	}
}
using System;
using asm.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using asm.Helpers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using JetBrains.Annotations;

namespace asm.Core.Web.Helpers
{
	public static class IConfigurationBuilderHelper
	{
		[NotNull]
		public static IConfigurationBuilder CreateConfiguration()
		{
			IHostEnvironment env = new HostingEnvironment
			{
				EnvironmentName = EnvironmentHelper.GetEnvironmentName(),
				ApplicationName = AppDomain.CurrentDomain.FriendlyName,
				ContentRootPath = AppDomain.CurrentDomain.BaseDirectory,
				ContentRootFileProvider = new PhysicalFileProvider(AppDomain.CurrentDomain.BaseDirectory)
			};

			return new ConfigurationBuilder()
				.Setup(env);
		}
	}
}
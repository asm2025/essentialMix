﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace asm.Core.Web.Mvc
{
	public abstract class ResponseResult : IActionResult
	{
		protected ResponseResult()
		{
		}

		/// <inheritdoc />
		public abstract Task ExecuteResultAsync(ActionContext context);
	}
}
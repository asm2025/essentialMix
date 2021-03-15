using System;
using System.Linq.Expressions;
using essentialMix.Extensions;
using JetBrains.Annotations;

namespace essentialMix.Web.Mvc.Helpers
{
	public static class ExpressionHelper
	{
		public static string GetExpressionText<TModel>([NotNull] Expression<Func<TModel, object>> value)
		{
			return value.GetPath() ?? System.Web.Mvc.ExpressionHelper.GetExpressionText(value);
		}
	}
}
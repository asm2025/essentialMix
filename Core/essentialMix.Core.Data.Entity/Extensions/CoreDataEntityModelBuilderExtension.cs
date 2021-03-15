using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions
{
	public static class CoreDataEntityModelBuilderExtension
	{
		[NotNull]
		public static ModelBuilder RegisterStringComparisonFunctions([NotNull] this ModelBuilder thisValue)
		{
			// https://stackoverflow.com/questions/59090817/enitityframework-is-very-slow-to-compare-strings-because-create-a-nvarchar-sqlpa#59095579
			RegisterBinaryExpression(thisValue, StringExtension.IsEqual, ExpressionType.Equal);
			RegisterBinaryExpression(thisValue, StringExtension.IsLessThan, ExpressionType.LessThan);
			RegisterBinaryExpression(thisValue, StringExtension.IsLessThanOrEqual, ExpressionType.LessThanOrEqual);
			RegisterBinaryExpression(thisValue, StringExtension.IsGreaterThan, ExpressionType.GreaterThan);
			RegisterBinaryExpression(thisValue, StringExtension.IsGreaterThanOrEqual, ExpressionType.GreaterThanOrEqual);
			return thisValue;

			static void RegisterBinaryExpression(ModelBuilder builder, Func<string, string, bool> method, ExpressionType type)
			{
				builder.HasDbFunction(method.Method)
						.HasTranslation(parameters =>
						{
							int i = 0;
							SqlExpression first = null, second = null;

							foreach (SqlExpression parameter in parameters)
							{
								if (i == 0) first = parameter;
								else second = parameter;
								if (++i > 1) break;
							}

							if (first == null || second == null) throw new InvalidOperationException("Incorrect number of arguments.");

							//// EF Core 2.x
							// return Expression.MakeBinary(type, first, second, false, method);
							
							// EF Core 3.0
							if (second is SqlParameterExpression secondExpression) second = secondExpression.ApplyTypeMapping(first.TypeMapping);
							else if (first is SqlParameterExpression firstExpression) first = firstExpression.ApplyTypeMapping(second.TypeMapping);

							return new SqlBinaryExpression(type, first, second, typeof(bool), null);
						});
			}
		}
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using essentialMix.Extensions;
using JetBrains.Annotations;

namespace essentialMix.Helpers
{
	/// <summary>
	/// General purpose Expression utilities
	/// This is largely based on Joseph and Ben Albahari and Eric Johannsen
	/// <see href="http://www.albahari.com/nutshell/predicatebuilder.aspx">predicate builder</see>
	/// </summary>
	public static class ExpressionHelper
	{
		[NotNull]
		public static Expression<Func<T, bool>> True<T>() { return f => true; }
		[NotNull]
		public static Expression<Func<T, bool>> False<T>() { return f => false; }

		[NotNull]
		public static Func<TResult> Function<TResult>([NotNull] Func<UnaryExpression> value)
		{
			return Expression.Lambda<Func<TResult>>(value()).Compile();
		}

		[NotNull]
		public static Func<T, TResult> Function<T, TResult>([NotNull] Func<Expression, UnaryExpression> value)
		{
			ParameterExpression t = Expression.Parameter(typeof(T), "t");
			return Expression.Lambda<Func<T, TResult>>(value(t), t).Compile();
		}

		[NotNull]
		public static Func<T1, T2, TResult> Function<T1, T2, TResult>([NotNull] Func<Expression, Expression, BinaryExpression> value, bool castArgsToResultOnFailure = false)
		{
			ParameterExpression t1 = Expression.Parameter(typeof(T1), "t1");
			ParameterExpression t2 = Expression.Parameter(typeof(T2), "t2");

			try
			{
				try
				{
					return Expression.Lambda<Func<T1, T2, TResult>>(value(t1, t2), 
																	t1, t2).Compile();
				}
				catch (InvalidOperationException) when(castArgsToResultOnFailure)
				{
					IList<Expression> p = CastToResultType<TResult>(t1, t2);
					if (p == null) throw;
					return Expression.Lambda<Func<T1, T2, TResult>>(value(p[0], p[1]), 
																	t1, t2).Compile();
				}
			}
			catch (Exception ex)
			{
				string msg = ex.CollectMessages(); // avoid capture of ex itself
				return delegate { throw new InvalidOperationException(msg); };
			}
		}

		[NotNull]
		public static Func<T1, T2, T3, TResult> Function<T1, T2, T3, TResult>([NotNull] Func<Expression, Expression, Expression, Expression> value, bool castArgsToResultOnFailure = false)
		{
			ParameterExpression t1 = Expression.Parameter(typeof(T1), "t1");
			ParameterExpression t2 = Expression.Parameter(typeof(T2), "t2");
			ParameterExpression t3 = Expression.Parameter(typeof(T3), "t3");

			try
			{
				try
				{
					return Expression.Lambda<Func<T1, T2, T3, TResult>>(value(t1, t2, t3), 
																		t1, t2, t3).Compile();
				}
				catch (InvalidOperationException) when(castArgsToResultOnFailure)
				{
					IList<Expression> p = CastToResultType<TResult>(t1, t2, t3);
					if (p == null) throw;
					return Expression.Lambda<Func<T1, T2, T3, TResult>>(value(p[0], p[1], p[2]), 
																		t1, t2, t3).Compile();
				}
			}
			catch (Exception ex)
			{
				string msg = ex.CollectMessages(); // avoid capture of ex itself
				return delegate { throw new InvalidOperationException(msg); };
			}
		}

		[NotNull]
		public static Func<T1, T2, T3, T4, TResult> Function<T1, T2, T3, T4, TResult>([NotNull] Func<Expression, Expression, Expression, Expression, Expression> value, bool castArgsToResultOnFailure = false)
		{
			ParameterExpression t1 = Expression.Parameter(typeof(T1), "t1");
			ParameterExpression t2 = Expression.Parameter(typeof(T2), "t2");
			ParameterExpression t3 = Expression.Parameter(typeof(T3), "t3");
			ParameterExpression t4 = Expression.Parameter(typeof(T4), "t4");

			try
			{
				try
				{
					return Expression.Lambda<Func<T1, T2, T3, T4, TResult>>(value(t1, t2, t3, t4), 
																			t1, t2, t3, t4).Compile();
				}
				catch (InvalidOperationException) when(castArgsToResultOnFailure)
				{
					IList<Expression> p = CastToResultType<TResult>(t1, t2, t3, t4);
					if (p == null) throw;
					return Expression.Lambda<Func<T1, T2, T3, T4, TResult>>(value(p[0], p[1], p[2], p[3]), 
																			t1, t2, t3, t4).Compile();
				}
			}
			catch (Exception ex)
			{
				string msg = ex.CollectMessages(); // avoid capture of ex itself
				return delegate { throw new InvalidOperationException(msg); };
			}
		}

		[NotNull]
		public static Func<T1, T2, T3, T4, T5, TResult> Function<T1, T2, T3, T4, T5, TResult>([NotNull] Func<Expression, Expression, Expression, Expression, Expression, Expression> value, bool castArgsToResultOnFailure = false)
		{
			ParameterExpression t1 = Expression.Parameter(typeof(T1), "t1");
			ParameterExpression t2 = Expression.Parameter(typeof(T2), "t2");
			ParameterExpression t3 = Expression.Parameter(typeof(T3), "t3");
			ParameterExpression t4 = Expression.Parameter(typeof(T4), "t4");
			ParameterExpression t5 = Expression.Parameter(typeof(T5), "t5");

			try
			{
				try
				{
					return Expression.Lambda<Func<T1, T2, T3, T4, T5, TResult>>(value(t1, t2, t3, t4, t5), 
																				t1, t2, t3, t4, t5).Compile();
				}
				catch (InvalidOperationException) when(castArgsToResultOnFailure)
				{
					IList<Expression> p = CastToResultType<TResult>(t1, t2, t3, t4, t5);
					if (p == null) throw;
					return Expression.Lambda<Func<T1, T2, T3, T4, T5, TResult>>(value(p[0], p[1], p[2], p[3], p[4]), 
																			t1, t2, t3, t4, t5).Compile();
				}
			}
			catch (Exception ex)
			{
				string msg = ex.CollectMessages(); // avoid capture of ex itself
				return delegate { throw new InvalidOperationException(msg); };
			}
		}

		[NotNull]
		public static Func<T1, T2, T3, T4, T5, T6, TResult> Function<T1, T2, T3, T4, T5, T6, TResult>([NotNull] Func<Expression, Expression, Expression, Expression, Expression, Expression, Expression> value, bool castArgsToResultOnFailure = false)
		{
			ParameterExpression t1 = Expression.Parameter(typeof(T1), "t1");
			ParameterExpression t2 = Expression.Parameter(typeof(T2), "t2");
			ParameterExpression t3 = Expression.Parameter(typeof(T3), "t3");
			ParameterExpression t4 = Expression.Parameter(typeof(T4), "t4");
			ParameterExpression t5 = Expression.Parameter(typeof(T5), "t5");
			ParameterExpression t6 = Expression.Parameter(typeof(T6), "t6");

			try
			{
				try
				{
					return Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, TResult>>(value(t1, t2, t3, t4, t5, t6), 
																				t1, t2, t3, t4, t5, t6).Compile();
				}
				catch (InvalidOperationException) when(castArgsToResultOnFailure)
				{
					IList<Expression> p = CastToResultType<TResult>(t1, t2, t3, t4, t5, t6);
					if (p == null) throw;
					return Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, TResult>>(value(p[0], p[1], p[2], p[3], p[4], p[5]), 
																				t1, t2, t3, t4, t5, t6).Compile();
				}
			}
			catch (Exception ex)
			{
				string msg = ex.CollectMessages(); // avoid capture of ex itself
				return delegate { throw new InvalidOperationException(msg); };
			}
		}

		[NotNull]
		public static Func<T1, T2, T3, T4, T5, T6, T7, TResult> Function<T1, T2, T3, T4, T5, T6, T7, TResult>([NotNull] Func<Expression, Expression, Expression, Expression, Expression, Expression, Expression, Expression> value, bool castArgsToResultOnFailure = false)
		{
			ParameterExpression t1 = Expression.Parameter(typeof(T1), "t1");
			ParameterExpression t2 = Expression.Parameter(typeof(T2), "t2");
			ParameterExpression t3 = Expression.Parameter(typeof(T3), "t3");
			ParameterExpression t4 = Expression.Parameter(typeof(T4), "t4");
			ParameterExpression t5 = Expression.Parameter(typeof(T5), "t5");
			ParameterExpression t6 = Expression.Parameter(typeof(T6), "t6");
			ParameterExpression t7 = Expression.Parameter(typeof(T7), "t7");

			try
			{
				try
				{
					return Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, TResult>>(value(t1, t2, t3, t4, t5, t6, t7), 
																					t1, t2, t3, t4, t5, t6, t7).Compile();
				}
				catch (InvalidOperationException) when(castArgsToResultOnFailure)
				{
					IList<Expression> p = CastToResultType<TResult>(t1, t2, t3, t4, t5, t6, t7);
					if (p == null) throw;
					return Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, TResult>>(value(p[0], p[1], p[2], p[3], p[4], p[5], p[6]), 
																					t1, t2, t3, t4, t5, t6, t7).Compile();
				}
			}
			catch (Exception ex)
			{
				string msg = ex.CollectMessages(); // avoid capture of ex itself
				return delegate { throw new InvalidOperationException(msg); };
			}
		}

		[NotNull]
		public static Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> Function<T1, T2, T3, T4, T5, T6, T7, T8, TResult>([NotNull] Func<Expression, Expression, Expression, Expression, Expression, Expression, Expression, Expression, Expression> value, bool castArgsToResultOnFailure = false)
		{
			ParameterExpression t1 = Expression.Parameter(typeof(T1), "t1");
			ParameterExpression t2 = Expression.Parameter(typeof(T2), "t2");
			ParameterExpression t3 = Expression.Parameter(typeof(T3), "t3");
			ParameterExpression t4 = Expression.Parameter(typeof(T4), "t4");
			ParameterExpression t5 = Expression.Parameter(typeof(T5), "t5");
			ParameterExpression t6 = Expression.Parameter(typeof(T6), "t6");
			ParameterExpression t7 = Expression.Parameter(typeof(T7), "t7");
			ParameterExpression t8 = Expression.Parameter(typeof(T8), "t8");

			try
			{
				try
				{
					return Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult>>(value(t1, t2, t3, t4, t5, t6, t7, t8), 
																						t1, t2, t3, t4, t5, t6, t7, t8).Compile();
				}
				catch (InvalidOperationException) when(castArgsToResultOnFailure)
				{
					IList<Expression> p = CastToResultType<TResult>(t1, t2, t3, t4, t5, t6, t7, t8);
					if (p == null) throw;
					return Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult>>(value(p[0], p[1], p[2], p[3], p[4], p[5], p[6], p[7]), 
																						t1, t2, t3, t4, t5, t6, t7, t8).Compile();
				}
			}
			catch (Exception ex)
			{
				string msg = ex.CollectMessages(); // avoid capture of ex itself
				return delegate { throw new InvalidOperationException(msg); };
			}
		}

		[NotNull]
		public static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> Function<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>([NotNull] Func<Expression, Expression, Expression, Expression, Expression, Expression, Expression, Expression, Expression, Expression> value, bool castArgsToResultOnFailure = false)
		{
			ParameterExpression t1 = Expression.Parameter(typeof(T1), "t1");
			ParameterExpression t2 = Expression.Parameter(typeof(T2), "t2");
			ParameterExpression t3 = Expression.Parameter(typeof(T3), "t3");
			ParameterExpression t4 = Expression.Parameter(typeof(T4), "t4");
			ParameterExpression t5 = Expression.Parameter(typeof(T5), "t5");
			ParameterExpression t6 = Expression.Parameter(typeof(T6), "t6");
			ParameterExpression t7 = Expression.Parameter(typeof(T7), "t7");
			ParameterExpression t8 = Expression.Parameter(typeof(T8), "t8");
			ParameterExpression t9 = Expression.Parameter(typeof(T9), "t9");

			try
			{
				try
				{
					return Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>>(value(t1, t2, t3, t4, t5, t6, t7, t8, t9), 
																							t1, t2, t3, t4, t5, t6, t7, t8, t9).Compile();
				}
				catch (InvalidOperationException) when(castArgsToResultOnFailure)
				{
					IList<Expression> p = CastToResultType<TResult>(t1, t2, t3, t4, t5, t6, t7, t8, t9);
					if (p == null) throw;
					return Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>>(value(p[0], p[1], p[2], p[3], p[4], p[5], p[6], p[7], p[8]), 
																							t1, t2, t3, t4, t5, t6, t7, t8, t9).Compile();
				}
			}
			catch (Exception ex)
			{
				string msg = ex.CollectMessages(); // avoid capture of ex itself
				return delegate { throw new InvalidOperationException(msg); };
			}
		}

		[NotNull]
		public static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> Function<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>([NotNull] Func<Expression, Expression, Expression, Expression, Expression, Expression, Expression, Expression, Expression, Expression, Expression> value, bool castArgsToResultOnFailure = false)
		{
			ParameterExpression t1 = Expression.Parameter(typeof(T1), "t1");
			ParameterExpression t2 = Expression.Parameter(typeof(T2), "t2");
			ParameterExpression t3 = Expression.Parameter(typeof(T3), "t3");
			ParameterExpression t4 = Expression.Parameter(typeof(T4), "t4");
			ParameterExpression t5 = Expression.Parameter(typeof(T5), "t5");
			ParameterExpression t6 = Expression.Parameter(typeof(T6), "t6");
			ParameterExpression t7 = Expression.Parameter(typeof(T7), "t7");
			ParameterExpression t8 = Expression.Parameter(typeof(T8), "t8");
			ParameterExpression t9 = Expression.Parameter(typeof(T9), "t9");
			ParameterExpression t10 = Expression.Parameter(typeof(T10), "t10");

			try
			{
				try
				{
					return Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>>(value(t1, t2, t3, t4, t5, t6, t7, t8, t9, t10), 
																								t1, t2, t3, t4, t5, t6, t7, t8, t9, t10).Compile();
				}
				catch (InvalidOperationException) when(castArgsToResultOnFailure)
				{
					IList<Expression> p = CastToResultType<TResult>(t1, t2, t3, t4, t5, t6, t7, t8, t9, t10);
					if (p == null) throw;
					return Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>>(value(p[0], p[1], p[2], p[3], p[4], p[5], p[6], p[7], p[8], p[9]), 
																								t1, t2, t3, t4, t5, t6, t7, t8, t9, t10).Compile();
				}
			}
			catch (Exception ex)
			{
				string msg = ex.CollectMessages(); // avoid capture of ex itself
				return delegate { throw new InvalidOperationException(msg); };
			}
		}

		[NotNull]
		public static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> Function<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>([NotNull] Func<Expression, Expression, Expression, Expression, Expression, Expression, Expression, Expression, Expression, Expression, Expression, Expression> value, bool castArgsToResultOnFailure = false)
		{
			ParameterExpression t1 = Expression.Parameter(typeof(T1), "t1");
			ParameterExpression t2 = Expression.Parameter(typeof(T2), "t2");
			ParameterExpression t3 = Expression.Parameter(typeof(T3), "t3");
			ParameterExpression t4 = Expression.Parameter(typeof(T4), "t4");
			ParameterExpression t5 = Expression.Parameter(typeof(T5), "t5");
			ParameterExpression t6 = Expression.Parameter(typeof(T6), "t6");
			ParameterExpression t7 = Expression.Parameter(typeof(T7), "t7");
			ParameterExpression t8 = Expression.Parameter(typeof(T8), "t8");
			ParameterExpression t9 = Expression.Parameter(typeof(T9), "t9");
			ParameterExpression t10 = Expression.Parameter(typeof(T10), "t10");
			ParameterExpression t11 = Expression.Parameter(typeof(T11), "t11");

			try
			{
				try
				{
					return Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>>(value(t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11), 
																									t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11).Compile();
				}
				catch (InvalidOperationException) when(castArgsToResultOnFailure)
				{
					IList<Expression> p = CastToResultType<TResult>(t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11);
					if (p == null) throw;
					return Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>>(value(p[0], p[1], p[2], p[3], p[4], p[5], p[6], p[7], p[8], p[9], p[10]), 
																									t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11).Compile();
				}
			}
			catch (Exception ex)
			{
				string msg = ex.CollectMessages(); // avoid capture of ex itself
				return delegate { throw new InvalidOperationException(msg); };
			}
		}

		[NotNull]
		public static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> Function<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>([NotNull] Func<Expression, Expression, Expression, Expression, Expression, Expression, Expression, Expression, Expression, Expression, Expression, Expression, Expression> value, bool castArgsToResultOnFailure = false)
		{
			ParameterExpression t1 = Expression.Parameter(typeof(T1), "t1");
			ParameterExpression t2 = Expression.Parameter(typeof(T2), "t2");
			ParameterExpression t3 = Expression.Parameter(typeof(T3), "t3");
			ParameterExpression t4 = Expression.Parameter(typeof(T4), "t4");
			ParameterExpression t5 = Expression.Parameter(typeof(T5), "t5");
			ParameterExpression t6 = Expression.Parameter(typeof(T6), "t6");
			ParameterExpression t7 = Expression.Parameter(typeof(T7), "t7");
			ParameterExpression t8 = Expression.Parameter(typeof(T8), "t8");
			ParameterExpression t9 = Expression.Parameter(typeof(T9), "t9");
			ParameterExpression t10 = Expression.Parameter(typeof(T10), "t10");
			ParameterExpression t11 = Expression.Parameter(typeof(T11), "t11");
			ParameterExpression t12 = Expression.Parameter(typeof(T12), "t12");

			try
			{
				try
				{
					return Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>>(value(t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12), 
																										t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12).Compile();
				}
				catch (InvalidOperationException) when(castArgsToResultOnFailure)
				{
					IList<Expression> p = CastToResultType<TResult>(t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12);
					if (p == null) throw;
					return Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>>(value(p[0], p[1], p[2], p[3], p[4], p[5], p[6], p[7], p[8], p[9], p[10], p[11]), 
																										t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12).Compile();
				}
			}
			catch (Exception ex)
			{
				string msg = ex.CollectMessages(); // avoid capture of ex itself
				return delegate { throw new InvalidOperationException(msg); };
			}
		}

		[NotNull]
		public static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> Function<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>([NotNull] Func<Expression, Expression, Expression, Expression, Expression, Expression, Expression, Expression, Expression, Expression, Expression, Expression, Expression, Expression> value, bool castArgsToResultOnFailure = false)
		{
			ParameterExpression t1 = Expression.Parameter(typeof(T1), "t1");
			ParameterExpression t2 = Expression.Parameter(typeof(T2), "t2");
			ParameterExpression t3 = Expression.Parameter(typeof(T3), "t3");
			ParameterExpression t4 = Expression.Parameter(typeof(T4), "t4");
			ParameterExpression t5 = Expression.Parameter(typeof(T5), "t5");
			ParameterExpression t6 = Expression.Parameter(typeof(T6), "t6");
			ParameterExpression t7 = Expression.Parameter(typeof(T7), "t7");
			ParameterExpression t8 = Expression.Parameter(typeof(T8), "t8");
			ParameterExpression t9 = Expression.Parameter(typeof(T9), "t9");
			ParameterExpression t10 = Expression.Parameter(typeof(T10), "t10");
			ParameterExpression t11 = Expression.Parameter(typeof(T11), "t11");
			ParameterExpression t12 = Expression.Parameter(typeof(T12), "t12");
			ParameterExpression t13 = Expression.Parameter(typeof(T13), "t13");

			try
			{
				try
				{
					return Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>>(value(t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13), 
																												t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13).Compile();
				}
				catch (InvalidOperationException) when(castArgsToResultOnFailure)
				{
					IList<Expression> p = CastToResultType<TResult>(t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13);
					if (p == null) throw;
					return Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>>(value(p[0], p[1], p[2], p[3], p[4], p[5], p[6], p[7], p[8], p[9], p[10], p[11], p[12]), 
																												t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13).Compile();
				}
			}
			catch (Exception ex)
			{
				string msg = ex.CollectMessages(); // avoid capture of ex itself
				return delegate { throw new InvalidOperationException(msg); };
			}
		}

		[NotNull]
		public static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> Function<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>([NotNull] Func<Expression, Expression, Expression, Expression, Expression, Expression, Expression, Expression, Expression, Expression, Expression, Expression, Expression, Expression, Expression> value, bool castArgsToResultOnFailure = false)
		{
			ParameterExpression t1 = Expression.Parameter(typeof(T1), "t1");
			ParameterExpression t2 = Expression.Parameter(typeof(T2), "t2");
			ParameterExpression t3 = Expression.Parameter(typeof(T3), "t3");
			ParameterExpression t4 = Expression.Parameter(typeof(T4), "t4");
			ParameterExpression t5 = Expression.Parameter(typeof(T5), "t5");
			ParameterExpression t6 = Expression.Parameter(typeof(T6), "t6");
			ParameterExpression t7 = Expression.Parameter(typeof(T7), "t7");
			ParameterExpression t8 = Expression.Parameter(typeof(T8), "t8");
			ParameterExpression t9 = Expression.Parameter(typeof(T9), "t9");
			ParameterExpression t10 = Expression.Parameter(typeof(T10), "t10");
			ParameterExpression t11 = Expression.Parameter(typeof(T11), "t11");
			ParameterExpression t12 = Expression.Parameter(typeof(T12), "t12");
			ParameterExpression t13 = Expression.Parameter(typeof(T13), "t13");
			ParameterExpression t14 = Expression.Parameter(typeof(T14), "t14");

			try
			{
				try
				{
					return Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>>(value(t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13, t14), 
																													t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13, t14).Compile();
				}
				catch (InvalidOperationException) when(castArgsToResultOnFailure)
				{
					IList<Expression> p = CastToResultType<TResult>(t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13, t14);
					if (p == null) throw;
					return Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>>(value(p[0], p[1], p[2], p[3], p[4], p[5], p[6], p[7], p[8], p[9], p[10], p[11], p[12], p[13]), 
																													t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13, t14).Compile();
				}
			}
			catch (Exception ex)
			{
				string msg = ex.CollectMessages(); // avoid capture of ex itself
				return delegate { throw new InvalidOperationException(msg); };
			}
		}

		[NotNull]
		public static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> Function<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>([NotNull] Func<Expression, Expression, Expression, Expression, Expression, Expression, Expression, Expression, Expression, Expression, Expression, Expression, Expression, Expression, Expression, Expression> value, bool castArgsToResultOnFailure = false)
		{
			ParameterExpression t1 = Expression.Parameter(typeof(T1), "t1");
			ParameterExpression t2 = Expression.Parameter(typeof(T2), "t2");
			ParameterExpression t3 = Expression.Parameter(typeof(T3), "t3");
			ParameterExpression t4 = Expression.Parameter(typeof(T4), "t4");
			ParameterExpression t5 = Expression.Parameter(typeof(T5), "t5");
			ParameterExpression t6 = Expression.Parameter(typeof(T6), "t6");
			ParameterExpression t7 = Expression.Parameter(typeof(T7), "t7");
			ParameterExpression t8 = Expression.Parameter(typeof(T8), "t8");
			ParameterExpression t9 = Expression.Parameter(typeof(T9), "t9");
			ParameterExpression t10 = Expression.Parameter(typeof(T10), "t10");
			ParameterExpression t11 = Expression.Parameter(typeof(T11), "t11");
			ParameterExpression t12 = Expression.Parameter(typeof(T12), "t12");
			ParameterExpression t13 = Expression.Parameter(typeof(T13), "t13");
			ParameterExpression t14 = Expression.Parameter(typeof(T14), "t14");
			ParameterExpression t15 = Expression.Parameter(typeof(T15), "t15");

			try
			{
				try
				{
					return Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>>(value(t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13, t14, t15), 
																														t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13, t14, t15).Compile();
				}
				catch (InvalidOperationException) when(castArgsToResultOnFailure)
				{
					IList<Expression> p = CastToResultType<TResult>(t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13, t14, t15);
					if (p == null) throw;
					return Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>>(value(p[0], p[1], p[2], p[3], p[4], p[5], p[6], p[7], p[8], p[9], p[10], p[11], p[12], p[13], p[14]), 
																														t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13, t14, t15).Compile();
				}
			}
			catch (Exception ex)
			{
				string msg = ex.CollectMessages(); // avoid capture of ex itself
				return delegate { throw new InvalidOperationException(msg); };
			}
		}

		[NotNull]
		public static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> Function<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>([NotNull] Func<Expression, Expression, Expression, Expression, Expression, Expression, Expression, Expression, Expression, Expression, Expression, Expression, Expression, Expression, Expression, Expression, Expression> value, bool castArgsToResultOnFailure = false)
		{
			ParameterExpression t1 = Expression.Parameter(typeof(T1), "t1");
			ParameterExpression t2 = Expression.Parameter(typeof(T2), "t2");
			ParameterExpression t3 = Expression.Parameter(typeof(T3), "t3");
			ParameterExpression t4 = Expression.Parameter(typeof(T4), "t4");
			ParameterExpression t5 = Expression.Parameter(typeof(T5), "t5");
			ParameterExpression t6 = Expression.Parameter(typeof(T6), "t6");
			ParameterExpression t7 = Expression.Parameter(typeof(T7), "t7");
			ParameterExpression t8 = Expression.Parameter(typeof(T8), "t8");
			ParameterExpression t9 = Expression.Parameter(typeof(T9), "t9");
			ParameterExpression t10 = Expression.Parameter(typeof(T10), "t10");
			ParameterExpression t11 = Expression.Parameter(typeof(T11), "t11");
			ParameterExpression t12 = Expression.Parameter(typeof(T12), "t12");
			ParameterExpression t13 = Expression.Parameter(typeof(T13), "t13");
			ParameterExpression t14 = Expression.Parameter(typeof(T14), "t14");
			ParameterExpression t15 = Expression.Parameter(typeof(T15), "t15");
			ParameterExpression t16 = Expression.Parameter(typeof(T16), "t16");

			try
			{
				try
				{
					return Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>>(value(t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13, t14, t15, t16), 
																															t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13, t14, t15, t16).Compile();
				}
				catch (InvalidOperationException) when(castArgsToResultOnFailure)
				{
					IList<Expression> p = CastToResultType<TResult>(t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13, t14, t15, t16);
					if (p == null) throw;
					return Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>>(value(p[0], p[1], p[2], p[3], p[4], p[5], p[6], p[7], p[8], p[9], p[10], p[11], p[12], p[13], p[14], p[15]), 
																															t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13, t14, t15, t16).Compile();
				}
			}
			catch (Exception ex)
			{
				string msg = ex.CollectMessages(); // avoid capture of ex itself
				return delegate { throw new InvalidOperationException(msg); };
			}
		}

		[NotNull]
		public static LambdaExpression ToLambdaExpression<T>(string name, [NotNull] out Type memberType)
		{
			name = name?.Trim('.', ' ');
			if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

			Type type = typeof(T);
			string[] names = name.Split(StringSplitOptions.RemoveEmptyEntries, '.');
			MemberInfo member = type.GetMember(names[0]).SingleOrDefault() ?? throw new MemberAccessException($"Member '{names[0]}' is not a member of the type '{type}'.");
			ParameterExpression parameter = Expression.Parameter(type, $"{type.Name}Expr");
			MemberExpression memberAccess = Expression.MakeMemberAccess(parameter, member);
			memberType = type;

			if (names.Length > 1)
			{
				for (int i = 1; i < names.Length; i++)
				{
					switch (member.MemberType)
					{
						case MemberTypes.Property:
							memberType = ((PropertyInfo)member).PropertyType;
							break;
						case MemberTypes.Field:
							memberType = ((FieldInfo)member).FieldType;
							break;
						default:
							throw new MemberAccessException($"Member '{names[i]}' is not valid from type '{memberType}'.");
					}

					member = memberType.GetPropertyOrField(names[i]) ?? throw new MemberAccessException($"Member '{names[i]}' is not a member of the type '{memberType}'.");
					memberAccess = Expression.MakeMemberAccess(memberAccess, member);
				}
			}

			return Expression.Lambda(memberAccess, parameter);
		}

		private static IList<Expression> CastToResultType<TResult>([NotNull] params ParameterExpression[] parameters)
		{
			if (parameters.Length == 0) return null;

			Type type = typeof(TResult);
			IList<Expression> result = new List<Expression>(parameters.Length);
			
			foreach (ParameterExpression expression in parameters)
			{
				result.Add(type == expression.Type 
								? expression 
								: Expression.Convert(expression, type));
			}

			return result.Count == 0
						? null
						: result;
		}
	}
}

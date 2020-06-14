using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web.Routing;
using asm.Extensions;
using asm.Helpers;
using asm.Newtonsoft.Helpers;
using asm.Web.Helpers;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace asm.Web.Routing
{
	public abstract class RouteBase : IComparable<RouteBase>, IComparable, IEquatable<RouteBase>
	{
		private const string ROUTE_MAP = @"(?s)(?<spaces>\s*)(?<item>\w+\.MapRoute\s*\(\s*(?:(?:name:\s*)?(?<name>[^,]+?))(?:\s*,\s*(?:(?:url:\s*)?(?<url>""{0}"")))(?:\s*,\s*(?:(?:defaults:\s*)?(?:(?:new\s*(?<defaults>\{{.*?\}}))|null))(?:\s*,\s*(?:(?:constraints:\s*)?(?:(?:new\s*(?<constraints>\{{.*?\}}))|null))(?:\s*,\s*(?:(?:namespaces:\s*)?(?:(?:new\s*\[\]\s*\{{(?<namespaces>\s*"".+?""(?:\s*,\s*"".+?"")*\s*)\}})|null)))?)?)?\s*\)[^;]*?;)";

		private static readonly Regex ROUTE_IGNORE = new Regex(@"(?s)(?<spaces>\s*)(?<item>\w+\.IgnoreRoute\s*\(\s*""(?<url>[^/]+?(?:/(?:[^/]+?))*)""(?:\s*,\s*(?:(?:constraints:\s*)?(?:(?:new\s*(?<constraints>\{.*?\}))|null)))?\s*\)[^;]*?;)", RegexHelper.OPTIONS_I | RegexOptions.Multiline);
		private static readonly Regex ROUTE_ANY = new Regex(string.Format(ROUTE_MAP, "[^/]+?(?:/(?:[^/]+?))*"), RegexHelper.OPTIONS_I | RegexOptions.Multiline);
		private static readonly Regex ROUTE_DEFAULT = new Regex(string.Format(ROUTE_MAP, @"{[\w0-9]+?}(?:/(?:\{[\w0-9]+?}))*"), RegexHelper.OPTIONS_I | RegexOptions.Multiline);
		private static readonly Regex IS_ROUTE_VAR = new Regex(@"\A\{[\w0-9]+?\}\z", RegexHelper.OPTIONS_I);
		private static readonly Regex ROUTE_VAR_ASSIGNMENT = new Regex(@"\s*=\s*", RegexHelper.OPTIONS_I | RegexOptions.Multiline);
		private static readonly Regex ROUTE_VAR_STRING = new Regex(@"(?<var>\w+\s*:\s*)(?<value>[^\d""]+?)(?<ter>[,\s])", RegexHelper.OPTIONS_I | RegexOptions.Multiline);

		private static readonly string[] SPECIAL_CASES =
		{
			"{controller}/{id}",
			"{action}/{id}",
			"{id}"
		};

		private static readonly Type IGNORE_ROUTE_INTERNAL;

		private string _url;
		private object _constraints;
		private object _defaultsInternal;
		private string[] _namespacesInternal;
		private RouteTypeEnum _type = RouteTypeEnum.Route;
		private IRouteHandler _routeHandlerInternal;
		private bool _hasFileHandler;

		static RouteBase() { IGNORE_ROUTE_INTERNAL = System.Type.GetType("global::System.Web.Routing.RouteCollection+IgnoreRouteInternal,global::System.Web"); }

		protected RouteBase([NotNull] string url) { Url = url; }

		public static explicit operator RouteBase(System.Web.Routing.Route route)
		{
			if (route == null) return null;

			string url = route.Url;
			RouteTypeEnum type = IsMvcIgnoreRoute(route) ? RouteTypeEnum.Ignore : RouteTypeEnum.Route;
			RouteBase routeBase = FromUrl(url, type);
			if (routeBase == null) return null;
			routeBase.Constraints = route.Constraints;
			routeBase.DefaultsInternal = route.Defaults;
			routeBase.RouteHandlerInternal = route.RouteHandler;
			routeBase.DataTokens = route.DataTokens;
			return routeBase;
		}

		public static explicit operator System.Web.Routing.Route(RouteBase routeBase)
		{
			if (routeBase == null) return null;

			RouteValueDictionary defaults = RouteValueDictionaryHelper.FromObject(routeBase.DefaultsInternal);
			RouteValueDictionary constraints = RouteValueDictionaryHelper.FromObject(routeBase.Constraints);
			RouteValueDictionary dataTokens = RouteValueDictionaryHelper.FromObject(routeBase.DataTokens);
			return new System.Web.Routing.Route(routeBase.Url, defaults, constraints, dataTokens, routeBase.RouteHandlerInternal);
		}

		public override string ToString() { return Url; }

		[NotNull]
		public string Url
		{
			get => _url;
			set
			{
				value = value.Trim('"', ' ');
				if (string.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(value));
				_url = value;
			}
		}

		public object Constraints
		{
			get => _constraints;
			set
			{
				_constraints = value;
				ConstraintsCount = _constraints is ICollection collection 
					? collection.Count 
					: _constraints.PropertiesCount(BindingFlags.DeclaredOnly);
			}
		}

		public int ConstraintsCount { get; private set; }

		public object DataTokens { get; private set; }

		public int Segments { get; private set; }
		public int StaticStart { get; private set; }
		public int Static { get; private set; }
		public int Dynamic { get; private set; }

		public RouteTypeEnum Type
		{
			get => _hasFileHandler ? RouteTypeEnum.File : _type;
			private set => _type = value;
		}

		protected virtual string NameInternal { get; set; }

		protected virtual object DefaultsInternal
		{
			get => _defaultsInternal;
			set
			{
				_defaultsInternal = value;
				DefaultsInternalCount = _defaultsInternal is ICollection collection 
					? collection.Count 
					: _defaultsInternal.PropertiesCount(BindingFlags.DeclaredOnly);
			}
		}

		protected virtual string[] NamespacesInternal
		{
			get => _namespacesInternal;
			set
			{
				_namespacesInternal = value;
				NamespacesInternalCount = _namespacesInternal?.Length ?? 0;
			}
		}

		protected virtual IRouteHandler RouteHandlerInternal
		{
			get => _routeHandlerInternal;
			set
			{
				_routeHandlerInternal = value;
				_hasFileHandler = IsMvcPageRoute(_routeHandlerInternal);
			}
		}

		protected int DefaultsInternalCount { get; private set; }

		protected int NamespacesInternalCount { get; private set; }

		public int CompareTo(object obj) { return CompareTo(obj as RouteBase); }

		public int CompareTo(RouteBase other)
		{
			if (other == null) return -1;

			/*
			 * Order logic:
			 * 1. Type precedence: (namespaces give higher precedence)
			 *		Ignore
			 *		Static
			 *		StaticWithDynamic
			 *		Dynamic
			 *		Route
			 *	if the type is equal, See #3.
			 * 2. Handle special cases
			 * 3. StaticWithDynamic, Dynamic and Route are special cases and handled as follows:
			 *		namespaces
			 *		Static relative to Segments: more comes FIRST (+defaults: more comes LAST +constraints: more comes FIRST).
			 *		Dynamic relative to Segments: more comes LAST (+defaults: more comes LAST +constraints: more comes FIRST).
			 *		Segments: more comes FIRST (+defaults: more comes LAST +constraints: more comes FIRST).
			 *		StaticStart: more comes FIRST (+defaults: more comes LAST +constraints: more comes FIRST).
			 *		Constraints: more comes FIRST.
			 * SEE GetRouteCompareValue(RouteBase route) for details
			*/
			if (Type == RouteTypeEnum.Ignore && other.Type != RouteTypeEnum.Ignore) return -1;
			if (other.Type == RouteTypeEnum.Ignore && Type != RouteTypeEnum.Ignore) return 1;

			int xprec = 0, yprec = 0, awd = 1;
			string xurl = Url, yurl = other.Url;

			for (int i = 0; i < SPECIAL_CASES.Length; i++, awd = (int)Math.Pow(2, i + 1))
			{
				string specialCase = SPECIAL_CASES[i];
				if (xurl.IsSame(specialCase)) xprec += awd;
				if (yurl.IsSame(specialCase)) yprec += awd;
			}

			if (xprec > 0 || yprec > 0)
			{
				int n = SPECIAL_CASES.Length;
				awd = (int)Math.Pow(2, n + 1);
				if (xprec > 0 && ConstraintsCount > 0) xprec += awd;
				if (yprec > 0 && other.ConstraintsCount > 0) yprec += awd;
				n++;
				awd = (int)Math.Pow(2, n + 1);
				if (xprec > 0 && NamespacesInternalCount > 0) xprec += awd;
				if (yprec > 0 && other.NamespacesInternalCount > 0) yprec += awd;
			}

			int diff = yprec - xprec;
			if (diff != 0) return diff;

			switch (Type)
			{
				case RouteTypeEnum.Ignore:
					switch (other.Type)
					{
						case RouteTypeEnum.Ignore:
							return other.Segments - Segments;
						default:
							return -1;
					}
				case RouteTypeEnum.File:
					switch (other.Type)
					{
						case RouteTypeEnum.Ignore:
							return 1;
						case RouteTypeEnum.File:
							return other.Segments - Segments;
						default:
							return -1;
					}
				case RouteTypeEnum.Static:
					switch (other.Type)
					{
						case RouteTypeEnum.Ignore:
						case RouteTypeEnum.File:
							return 1;
						case RouteTypeEnum.Static:
							return GetRouteCompareValue();
						default:
							return -1;
					}
				case RouteTypeEnum.StaticWithDynamic:
					switch (other.Type)
					{
						case RouteTypeEnum.Ignore:
						case RouteTypeEnum.File:
						case RouteTypeEnum.Static:
							return 1;
						case RouteTypeEnum.StaticWithDynamic:
							return GetRouteCompareValue();
						default:
							return -1;
					}
				case RouteTypeEnum.Dynamic:
					switch (other.Type)
					{
						case RouteTypeEnum.Ignore:
						case RouteTypeEnum.File:
						case RouteTypeEnum.Static:
						case RouteTypeEnum.StaticWithDynamic:
							return 1;
						case RouteTypeEnum.Dynamic:
							return GetRouteCompareValue();
						default:
							return -1;
					}
				default:
					switch (other.Type)
					{
						case RouteTypeEnum.Ignore:
						case RouteTypeEnum.File:
						case RouteTypeEnum.Static:
						case RouteTypeEnum.StaticWithDynamic:
						case RouteTypeEnum.Dynamic:
							return 1;
						default:
							return GetRouteCompareValue();
					}
			}

			int GetRouteCompareValue()
			{
				if (Segments == 0) return other.Segments;
				if (other.Segments == 0) return -Segments;

				int[] xvalues = GetRouteValues(this);
				int[] yvalues = GetRouteValues(other);
				xprec = 0;
				yprec = 0;
				awd = 0;

				for (int i = 0; i < xvalues.Length; i++, awd = (int)Math.Pow(2, i))
				{
					int x = xvalues[i];
					int y = yvalues[i];
					int cmp = x < 0 || y < 0 
						? x.CompareTo(y)
						: y.CompareTo(x);

					if (cmp < 0) xprec |= awd;
					else if (cmp > 0) yprec |= awd;
				}

				return yprec - xprec;
			}

			static int[] GetRouteValues(RouteBase route)
			{
				return new[]
				{
					(int)(route.DefaultsInternalCount / (double)route.Dynamic.NotBelow(1) * 100.0) * -1,
					(int)(route.StaticStart / (double)route.Segments * 100.0),
					(int)(route.Dynamic / (double)route.Segments * 100.0) * -1,
					(int)(route.Static / (double)route.Segments * 100.0),
					(int)(route.ConstraintsCount / (double)route.Dynamic.NotBelow(1) * 100.0),
					route.NamespacesInternalCount > 0
						? 1
						: 0,
					route.Segments
				};
			}
		}

		public bool Equals(RouteBase other) { return ReferenceEquals(this, other) || other != null && GetType() == other.GetType() && CompareTo(other) == 0; }

		public static RouteBase FromUrl(string url, RouteTypeEnum type) { return FromUrlInternal(url, type, null); }

		[NotNull]
		public static IEnumerable<RouteBase> ParseRoutes(string value) { return ParseRoutesInternal(value).Select(e => e.Item1); }

		public static bool IsMvcIgnoreRoute(System.Web.Routing.RouteBase routeBase) { return routeBase != null && routeBase.GetType() == IGNORE_ROUTE_INTERNAL; }

		public static bool IsMvcPageRoute(System.Web.Routing.RouteBase routeBase) { return IsMvcPageRoute((routeBase as System.Web.Routing.Route)?.RouteHandler); }

		public static bool IsMvcPageRoute(IRouteHandler handler) { return handler is PageRouteHandler; }

		internal static IEnumerable<(RouteBase Route, string Value)> ParseRoutesInternal(string value)
		{
			if (string.IsNullOrEmpty(value)) yield break;

			HashSet<string> routes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			Match match = ROUTE_IGNORE.Match(value);

			while (match.Success)
			{
				if (ParseRoute(match.Value, out (RouteBase Route, string Value) route, RouteTypeEnum.Ignore) && routes.Add(route.Route.Url)) yield return route;
				match = match.NextMatch();
			}

			match = ROUTE_DEFAULT.Match(value);

			while (match.Success)
			{
				if (ParseRoute(match.Value, out (RouteBase Route, string Value) route, RouteTypeEnum.Dynamic) && routes.Add(route.Route.Url)) yield return route;
				match = match.NextMatch();
			}

			match = ROUTE_ANY.Match(value);

			while (match.Success)
			{
				if (ParseRoute(match.Value, out (RouteBase Route, string Value) route, RouteTypeEnum.Route) && routes.Add(route.Route.Url)) yield return route;
				match = match.NextMatch();
			}
		}

		private static RouteBase FromUrlInternal(string url, RouteTypeEnum type, Match match)
		{
			url = url?.Replace("//", "/").Trim('/', '"', ' ');
			if (string.IsNullOrEmpty(url)) return null;

			string[] segments = url.Split('/');

			if (type == RouteTypeEnum.Ignore)
			{
				return new IgnoreRoute(url)
						{
							Type = type,
							Segments = segments.Length
						};
			}

			Route route = new Route(url)
			{
				Type = type,
				Segments = segments.Length
			};

			bool dynamicFound = false;

			foreach (string segment in segments)
			{
				if (string.IsNullOrEmpty(segment)) return null;
				if (IS_ROUTE_VAR.IsMatch(segment))
				{
					route.Dynamic++;
					dynamicFound = true;
					continue;
				}
				if (segment.ContainsAny('{', '}')) return null;
				if (!dynamicFound) route.StaticStart++;
				route.Static++;
			}

			if (route.Dynamic == route.Segments) route.Type = RouteTypeEnum.Dynamic;
			else if (route.Static == route.Segments) route.Type = RouteTypeEnum.Static;
			else if (route.Static > 0 && route.Dynamic > 0) route.Type = RouteTypeEnum.StaticWithDynamic;

			if (match != null && match.Groups.Count > 0)
			{
				route.Name = match.Groups["name"]?.Value;

				string tmp = match.Groups["defaults"]?.Value.Trim();
				JsonLoadSettings settings = null;
				
				if (!string.IsNullOrEmpty(tmp))
				{
					tmp = ROUTE_VAR_STRING.Replace(ROUTE_VAR_ASSIGNMENT.Replace(tmp, ": "), @"${var}""${value}""${ter}");

					if (!string.IsNullOrEmpty(tmp))
					{
						settings = JsonHelper.CreateLoadSettings();
						route.DefaultsInternal = JObject.Parse(tmp, settings);
					}
				}

				tmp = match.Groups["constraints"]?.Value.Trim();

				if (!string.IsNullOrEmpty(tmp))
				{
					tmp = ROUTE_VAR_STRING.Replace(ROUTE_VAR_ASSIGNMENT.Replace(tmp, ": "), @"${var}""${value}""${ter}");

					if (!string.IsNullOrEmpty(tmp))
					{
						settings ??= JsonHelper.CreateLoadSettings();
						route.Constraints = JObject.Parse(tmp, settings);
					}
				}

				tmp = match.Groups["namespaces"]?.Value.Trim();
				if (!string.IsNullOrEmpty(tmp)) route.Namespaces = JsonConvert.DeserializeObject<string[]>($"[{tmp}]", JsonHelper.CreateSettings());
			}

			return route;
		}

		private static bool ParseRoute(string value, out (RouteBase Route, string Value) routeTuple, RouteTypeEnum type)
		{
			routeTuple = default((RouteBase, string));
			if (string.IsNullOrEmpty(value)) return false;

			Match match;

			switch (type)
			{
				case RouteTypeEnum.Ignore:
					match = ROUTE_IGNORE.Match(value);
					break;
				case RouteTypeEnum.Dynamic:
					match = ROUTE_DEFAULT.Match(value);
					break;
				default:
					type = RouteTypeEnum.Route;
					match = ROUTE_ANY.Match(value);
					break;
			}

			if (!match.Success) return false;

			RouteBase route = FromUrlInternal(match.Groups["url"]?.Value, type, match);
			if (route == null) return false;
			routeTuple = (route, match.Value.Trim());
			return true;
		}
	}
}
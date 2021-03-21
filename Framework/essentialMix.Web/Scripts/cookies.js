(function (global, undefined) {
	"use strict";

	var factory = function (window) {
		if (typeof window.document !== "object")
			throw new Error("Unable to get a reference to a document object.");

		var cookies = function (key, value, options) {
			return arguments.length === 1
				? cookies.get(key)
				: cookies.set(key, value, options);
		};

		cookies._document = window.document;

		// Used to ensure cookie keys do not collide with built-in `Object` properties
		cookies._cacheKeyPrefix = "essentialMix";
		cookies._maxExpireDate = new Date("Fri, 31 Dec 9999 23:59:59 UTC");

		cookies.defaults = {
			path: "/",
			secure: false
		};

		cookies.get = function (key) {
			if (cookies._cachedDocumentCookie !== cookies._document.cookie)
				cookies._renewCache();

			var value = cookies._cache[cookies._cacheKeyPrefix + key];
			return value === undefined ? undefined : decodeURIComponent(value);
		};

		cookies.set = function (key, value, options) {
			options = cookies._getExtendedOptions(options);
			options.expires = cookies._getExpiresDate(value === undefined ? -1 : options.expires);
			cookies._document.cookie = cookies._generateCookieString(key, value, options);
			return cookies;
		};

		cookies.remove = function (key, options) {
			return cookies.set(key, undefined, options);
		};

		cookies.keys = function () {
			if (cookies._cachedDocumentCookie !== cookies._document.cookie)
				cookies._renewCache();

			var allKeys = cookies._cachedDocumentCookie.replace(/((?:^|\s*;)[^\=]+)(?=;|$)|^\s*|\s*(?:\=[^;]*)?(?:\1|$)/g, "")
				.split(/\s*(?:\=[^;]*)?;\s*/)
				.map(k => decodeURIComponent(k));
			return allKeys;
		}

		cookies._getExtendedOptions = function (options) {
			return {
				path: options && options.path || cookies.defaults.path,
				domain: options && options.domain || cookies.defaults.domain,
				expires: options && options.expires || cookies.defaults.expires,
				secure: options && options.secure !== undefined ? options.secure : cookies.defaults.secure
			};
		};

		cookies._isValidDate = function (date) {
			return Object.prototype.toString.call(date) === "[object Date]" && !isNaN(date.getTime());
		};

		cookies._getExpiresDate = function (expires, now) {
			now = now || new Date();

			switch (expires.constructor) {
				case Number:
					expires = expires === Infinity
						? cookies._maxExpireDate
						: new Date(now.getTime() + expires * 1000);
					break;
				case String:
					expires = new Date(expires);
					break;
			}

			if (expires && !cookies._isValidDate(expires))
				throw new Error("`expires` parameter cannot be converted to a valid Date instance");

			return expires;
		};

		cookies._generateCookieString = function (key, value, options) {
			key = key.replace(/[^#$&+\^`|]/g, encodeURIComponent)
				.replace(/\(/g, "%28")
				.replace(/\)/g, "%29");
			value = (value + "").replace(/[^!#$&-+\--:<-\[\]-~]/g, encodeURIComponent);
			options = options || {};

			var cookieString = key + "=" + value;
			if (options.path) cookieString +=";path=" + options.path;
			if (options.domain) cookieString += ";domain=" + options.domain;
			if (options.expires) cookieString += ";expires=" + options.expires.toUTCString();
			if (options.secure) cookieString += ";secure";
			return cookieString;
		};

		cookies._getCacheFromString = function (documentCookie) {
			var cookieCache = {};
			var cookiesArray = documentCookie ? documentCookie.split("; ") : [];

			for (var i = 0; i < cookiesArray.length; i++) {
				var cookieKvp = cookies._getKeyValuePairFromCookieString(cookiesArray[i]);

				if (cookieCache[cookies._cacheKeyPrefix + cookieKvp.key] === undefined)
					cookieCache[cookies._cacheKeyPrefix + cookieKvp.key] = cookieKvp.value;
			}

			return cookieCache;
		};

		cookies._getKeyValuePairFromCookieString = function (cookieString) {
			// "=" is a valid character in a cookie value according to RFC6265, so cannot `split('=')`
			var separatorIndex = cookieString.indexOf("=");

			// IE omits the "=" when the cookie value is an empty string
			separatorIndex = separatorIndex < 0 ? cookieString.length : separatorIndex;

			var key = cookieString.substr(0, separatorIndex);
			var decodedKey = null;

			try {
				decodedKey = decodeURIComponent(key);
			} catch (e) {
				if (console && typeof console.error === "function") {
					console.error('Could not decode cookie with key "' + key + '"', e);
				}
			}

			return {
				key: decodedKey,
				value: cookieString.substr(separatorIndex + 1) // Defer decoding value until accessed
			};
		};

		cookies._renewCache = function () {
			cookies._cache = cookies._getCacheFromString(cookies._document.cookie);
			cookies._cachedDocumentCookie = cookies._document.cookie;
		};

		cookies._areEnabled = function () {
			var testKey = "essentialMix";
			var areEnabled = cookies.set(testKey, 1).get(testKey) === "1";
			cookies.expire(testKey);
			return areEnabled;
		};

		cookies.enabled = cookies._areEnabled();
		return cookies;
	};

	var cookiesExport = (global && typeof global.document === "object") ? factory(global) : factory;

	// AMD support
	if (typeof define === "function" && define.amd) {
		define(function () { return cookiesExport; });
		// CommonJS/Node.js support
	} else if (typeof exports === "object") {
		// Support Node.js specific `module.exports` (which can be a function)
		if (typeof module === "object" && typeof module.exports === "object") {
			exports = module.exports = cookiesExport;
		}
		// But always support CommonJS module 1.1.1 spec (`exports` cannot be a function)
		exports.Cookies = cookiesExport;
	} else {
		global.Cookies = cookiesExport;
	}
})(window);

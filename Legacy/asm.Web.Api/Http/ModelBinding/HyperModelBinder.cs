using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using System.Web.Http.Routing;
using System.Xml.Linq;
using JetBrains.Annotations;
using asm.Data.Extensions;
using asm.Data.Helpers;
using asm.Extensions;
using asm.Newtonsoft.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace asm.Web.Api.Http.ModelBinding
{
	public class HyperModelBinder : IModelBinder
	{
		//Set default maximum recursion limit
		private int _recursionCount;

		/// <inheritdoc /> 
		public HyperModelBinder([NotNull] Type type)
			: this(type, 100)
		{
		}

		public HyperModelBinder([NotNull] Type type, int maxRecursionLimit)
		{
			TargetType = type;
			MaxRecursionLimit = maxRecursionLimit;
		}

		[NotNull]
		protected Type TargetType { get; }
		protected int MaxRecursionLimit { get; }

		/// <inheritdoc />
		public virtual bool BindModel([NotNull] HttpActionContext actionContext, [NotNull] ModelBindingContext bindingContext)
		{
			if (!TargetType.IsAssignableFrom(bindingContext.ModelType)) throw new TypeInitializationException(bindingContext.ModelType.FullName, new InvalidCastException($"Type {TargetType.FullName} is not assignable from type {bindingContext.ModelType.FullName}."));

			JsonLoadSettings jsonLoadSettings = JsonHelper.CreateLoadSettings();
			IDictionary<string, string> values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

			try
			{
				HttpRequestMessage request = actionContext.Request;
				IHttpRouteData routeData = actionContext.RequestContext.RouteData;

				/*
				Route data
				FormData/Body
				Query string
				*/
				if (routeData != null)
				{
					foreach (KeyValuePair<string, object> pair in routeData.Values)
					{
						Type type = pair.Value?.GetType();
						if (type != null && !type.IsPrimitive()) continue;
						values[pair.Key] = Convert.ToString(pair.Value);
					}
				}

				string contentType = request.Content.Headers.ContentType.MediaType.ToLowerInvariant();

				switch (contentType)
				{
					case "application/x-www-form-urlencoded":
						HandleFormData(request.Content.ReadAsStringAsync().GetAwaiter().GetResult().TrimStart('?'));
						break;
					case "application/json":
					case "text/json":
						HandleJson(request.Content.ReadAsStringAsync().GetAwaiter().GetResult().Trim());
						break;
					case "application/xml":
					case "text/xml":
						HandleXml(request.Content.ReadAsStringAsync().GetAwaiter().GetResult().Trim());
						break;
				}

				// Query string
				HandleQuery(request);

				//Initiate primary object
				object obj = Activator.CreateInstance(bindingContext.ModelType);

				//First call for processing primary object
				SetPropertyValues(values, obj);

				//Assign completed object tree to Model
				bindingContext.Model = obj;
				return true;
			}
			catch (Exception ex)
			{
				bindingContext.ModelState.AddModelError(bindingContext.ModelName, ex.CollectMessages());
				return false;
			}

			void HandleQuery(HttpRequestMessage request)
			{
				if (string.IsNullOrEmpty(request.RequestUri.Query)) return;

				foreach (KeyValuePair<string, string> pair in request.GetQueryNameValuePairs())
				{
					string data = pair.Value == null ? null : WebUtility.UrlDecode(pair.Value.Trim());

					if (!string.IsNullOrEmpty(data))
					{
						if (data.StartsWith('{') || data.StartsWith('['))
						{
							JObject bodyJson = JObject.Parse(data, jsonLoadSettings);
							AddJsonProperties(values, bodyJson);
						}
						else if (data.StartsWith("<?xml", StringComparison.OrdinalIgnoreCase))
						{
							XDocument xml = XmlHelper.XParse(data);
							AddXProperties(values, xml?.Root);
						}
						else
							values[pair.Key] = data;
					}
					else if (!values.ContainsKey(pair.Key))
					{
						values.Add(pair.Key, null);
					}
				}
			}
			
			void HandleFormData(string formData)
			{
				if (string.IsNullOrEmpty(formData)) return;

				string[] elements = formData.Split('=', '&');

				for (int i = 0; i < elements.Length; i += 2)
				{
					string key = elements[i];
					string value = i < elements.Length - 1
										? elements[i + 1]
										: null;

					if (!string.IsNullOrEmpty(value)) values[key] = value;
					else if (!values.ContainsKey(key)) values.Add(key, null);
				}
			}

			void HandleJson(string jsonData)
			{
				if (string.IsNullOrEmpty(jsonData)) return;
				jsonData = WebUtility.UrlDecode(jsonData);
				JObject bodyJson = JObject.Parse(jsonData, jsonLoadSettings);
				AddJsonProperties(values, bodyJson);
			}

			void HandleXml(string xmlData)
			{
				if (string.IsNullOrEmpty(xmlData)) return;
				xmlData = WebUtility.UrlDecode(xmlData);
				XDocument xml = XmlHelper.XParse(xmlData, true);
				AddXProperties(values, xml?.Root);
			}
		}

		public virtual void SetPropertyValues(IDictionary<string, string> values, [NotNull] object source, object parent = null, PropertyInfo parentProperty = null)
		{
			//Recursively set PropertyInfo array for object hierarchy
			PropertyInfo[] properties = source.GetType().GetProperties();
			JsonSerializerSettings jsonSettings = JsonHelper.CreateSettings();

			//Set KV Work List for real iteration process so that kvps is not in iteration and
			//its items from kvps can be removed after each iteration
			foreach (PropertyInfo property in properties)
			{
				if (!values.TryGetValue(property.Name, out string value)) continue;
				if (TryAddSingleProperty(values, source, property, value)) continue;

				Type targetType = property.PropertyType.ResolveType();
				object target = JsonHelper.Deserialize(value, targetType, null, jsonSettings);

				if (target != null)
				{
					property.SetValue(source, target);
					continue;
				}

				//Check and process property encompassing complex object recursively
				WalkThePathOfSorrow(values, source, property);
			}

			//Add property of this object to parent object 
			if (parent != null && parentProperty != null) parentProperty.SetValue(parent, source);
		}

		protected virtual void AddJsonProperties(IDictionary<string, string> values, JToken root)
		{
			switch (root)
			{
				case null:
				case JValue _:
					return;
				case JProperty property when property.Name != "$id":
					values[property.Path] = Convert.ToString(property.Value);
					break;
			}

			foreach (JToken child in root.Where(e => e.HasValues))
				AddJsonProperties(values, child);
		}

		protected virtual void AddXProperties(IDictionary<string, string> values, XElement root)
		{
			if (root == null) return;

			string path = root.GetXPath() ?? string.Empty;
			
			foreach (XAttribute attribute in root.Attributes())
			{
				values[string.Join(".", path, attribute)] = attribute.Value;
			}

			foreach (XElement child in root.Elements())
				AddXProperties(values, child);
		}

		protected virtual void WalkThePathOfSorrow(IDictionary<string, string> values, object source, PropertyInfo property, string parentName = null, string parentIndex = null)
		{
			if (source == null || property == null) return;

			//Check recursion limit
			if (_recursionCount > MaxRecursionLimit) throw new Exception($"Exceed maximum recursion limit {MaxRecursionLimit}.");
			_recursionCount++;

			Type type = property.PropertyType.GetEnumerableType();
			//Validate collection types
			Type[] genericArgTypes = null;
			if (type == null || !type.IsArray && (genericArgTypes = type.GetGenericArguments()).Length > 1) return;

			//Dynamically create instances for nested collection items
			object childObj;

			if (property.PropertyType.IsGenericType || property.PropertyType != type)
			{
				// This type ie either a generic type or inherits from IEnumerable
				Type childType = type.IsArray
									? type.GetElementType()
									: genericArgTypes?[0];
				if (childType == null) return;
				// Call to process collection
				childObj = Activator.CreateInstance(childType);
				// grrrrr
				SetPropertyValuesForMany(values, childObj, source, property, parentName, parentIndex);
			}
			else
			{
				//Dynamically create instances for nested object and call to process it
				childObj = Activator.CreateInstance(property.PropertyType);
				SetPropertyValues(values, childObj, source, property);
			}
		}

		protected virtual void SetPropertyValuesForMany(IDictionary<string, string> values, object source, object parent, PropertyInfo parentProperty, string parentName = null, string parentIndex = null)
		{
			// Not now. absolutely not :(
			/*
			if (source == null || parent == null || parentProperty == null || !parentProperty.PropertyType.IsList()) return;

			IList parentList = (IList)parentProperty.GetValue(parent);
			if (parentList == null) return;

			//Get props for type of object item in collection
			PropertyInfo[] properties = source.GetType().GetProperties();
			//KV Work For each object item in collection
			IList<KeyValueWork> kvwsGroup = new List<KeyValueWork>();
			//KV Work for collection
			List<IList<KeyValueWork>> kvwsGroups = new List<IList<KeyValueWork>>();

			string lastIndex = string.Empty;

			foreach (KeyValuePair<string, string> item in values)
			{
				//Passed parent and parentName are for List, whereas obj is instance of type for List
				if (!item.Key.Contains(parentProperty.Name)) continue;

				//Get data only from parent-parent for linked child KV Work
				Regex regex;
				Match match;

				if (!string.IsNullOrEmpty(parentName) & !string.IsNullOrEmpty(parentIndex))
				{
					regex = new Regex(parentName + RGX_SEARCH_BRACKET);
					match = regex.Match(item.Key);
					if (!match.Success || match.Groups[1].Value != parentIndex) break;
				}

				//Get parts from current KV Work
				regex = new Regex(parentProperty.Name + RGX_SEARCH_BRACKET);
				match = regex.Match(item.Key);
				if (!match.Success) continue;

				string brackets = match.Value.Replace(parentProperty.Name, string.Empty);
				string objIdx = match.Groups[1].Value;

				//Point to start next idx and save last kvwsGroup data to kvwsGroups
				if (!string.IsNullOrEmpty(lastIndex) && objIdx != lastIndex)
				{
					kvwsGroups.Add(kvwsGroup);
					kvwsGroup = new List<KeyValueWork>();
				}

				//Get parts array from Key
				string[] keyParts = item.Key.Split(StringSplitOptions.RemoveEmptyEntries, brackets);
				//Populate KV Work
				KeyValueWork kvw = new KeyValueWork
				{
					ObjIndex = objIdx,
					ParentName = parentProperty.Name,
					//Get last part from prefixed name
					Key = keyParts[keyParts.Length - 1],
					Value = item.Value,
					SourceKvp = item
				};
				//add KV Work to kvwsGroup list
				kvwsGroup.Add(kvw);
				lastIndex = objIdx;
				//isGroupAdded = false;
			}

			//Handle the last kvwsgroup item if not added to final kvwsGroups List.
			if (kvwsGroup.Count > 0) kvwsGroups.Add(kvwsGroup);

			//Initiate IList
			Type collectionType = parentProperty.PropertyType.GetCollectionType();
			if (collectionType == null) return;

			foreach (IList<KeyValueWork> group in kvwsGroups)
			{
				//Initiate object with type of collection item
				object tempObj = Activator.CreateInstance(source.GetType());

				//Iterate through properties of object model.
				foreach (PropertyInfo property in properties)
				{
					if (property.PropertyType.IsPrimitive())
					{
						KeyValueWork item = group.FirstOrDefault(e => e.Key == property.Name);
						if (item != null) TryAddSingleProperty(tempObj, property, item);
						continue;
					}

					//Check if List<string> or string[] type and assign string value directly to list or array item.    
					if (property.PropertyType.IsCollection<string>())
					{
						//Match passed current processing object.
						PropertyInfo[] tempProps = tempObj.GetType().GetProperties();

						//Iterate through current processing object properties.
						foreach (PropertyInfo tempProp in tempProps.Where(e => e.Name == property.Name && e.PropertyType.IsList<string>()))
						{
							IList<string> list = (IList<string>)tempProp.GetValue(tempObj);
							if (list == null || list.IsReadOnly) continue;

							//Iterate through passed data items.
							foreach (KeyValueWork item in group)
							{
								//Remove any brackets and enclosure from Key.
								string itemKey = Regex.Replace(item.Key, RGX_BRACKETS, string.Empty);
								if (itemKey != tempProp.Name) continue;
								list.Add(item.Value);
								values.Remove(item.SourceKvp);
							}
						}
					}
					//Check and process nested objects in collection recursively
					//Pass ObjIndex for child KV Work items only for this parent object                        
					else WalkThePathOfSorrow(tempObj, property, group[0].ParentName, group[0].ObjIndex);
				}

				//Add populated object to List or Array                    
				parentList.Add(tempObj);
			}
			*/
		}

		protected virtual bool TryAddSingleProperty(IDictionary<string, string> values, object source, [NotNull] PropertyInfo property, string value)
		{
			// Try to find a converter
			Type targetType = property.PropertyType.ResolveType();
			TypeConverter converter = TypeDescriptor.GetConverter(targetType);
			if (!converter.CanConvertFrom(typeof(string))) return false;

			try
			{
				object newValue = converter.ConvertFrom(value);
				property.SetValue(source, newValue);
				values.Remove(property.Name);
				return true;
			}
			catch
			{
				return false;
			}
		}
	}

	public class HyperModelBinder<T> : HyperModelBinder
	{
		/// <inheritdoc />
		public HyperModelBinder()
			: this(100)
		{
		}

		/// <inheritdoc />
		public HyperModelBinder(int maxRecursionLimit)
			: base(typeof(T), maxRecursionLimit)
		{
		}
	}
}
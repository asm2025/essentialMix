using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using asm.Comparers;
using asm.Extensions;
using JetBrains.Annotations;

namespace asm.Reflection
{
	[DebuggerDisplay("{Path}")]
	public class PropertyPath : IEnumerable<PropertyInfo>, IEnumerable
	{
		private readonly IReadOnlyList<PropertyInfo> _components;

		private string _path;

		public PropertyPath([NotNull] PropertyInfo component)
		{
			_components = new []{ component };
		}

		public PropertyPath([NotNull] IEnumerable<PropertyInfo> components)
		{
			_components = components.ToArray();
		}

		private PropertyPath()
		{
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _components.GetEnumerator();
		}

		public IEnumerator<PropertyInfo> GetEnumerator()
		{
			return _components.GetEnumerator();
		}

		public int Count => _components.Count;

		public PropertyInfo this[int index] => _components[index];

		[NotNull]
		public string Path => _path ??= _components.Aggregate(new StringBuilder(), (builder, property) => builder.Separator('.').Append(property.Name)).ToString();

		[NotNull]
		public override string ToString()
		{
			return Path;
		}

		public override bool Equals(object obj)
		{
			return obj != null && (ReferenceEquals(this, obj) || obj.GetType() == typeof(PropertyPath) && Equals((PropertyPath)obj));
		}

		public override int GetHashCode()
		{
			return _components.GetHashCode();
		}

		public bool Equals(PropertyPath other)
		{
			return other != null &&
					(ReferenceEquals(this, other) || _components.SequenceEqual(other._components, PropertyInfoComparer.Default));
		}

		public static bool operator ==(PropertyPath left, PropertyPath right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(PropertyPath left, PropertyPath right)
		{
			return !Equals(left, right);
		}

		public static PropertyPath Empty { get; } = new PropertyPath();
	}
}
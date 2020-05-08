using asm.Extensions;
using JetBrains.Annotations;

namespace asm.Windows.Html
{
	/// <summary>
	/// Attribute (name and value) in HTML tag
	/// </summary>
	public class Attribute
	{
		private string _name;
		private string _value;

		public Attribute(string name, string value) { Set(name, value); }

		public Attribute([NotNull] Attribute oldAttr) { Set(oldAttr.Name, oldAttr.Value); }

		public override string ToString() { return $"{_name}=\"{_value}\""; }

		public string Name { get; set; }
		public string Value { get; set; }

		public T Get<T>(T defaultValue) { return _value.To(defaultValue); }

		private void Set(string name, string value)
		{
			_name = name;
			_value = value;
		}
	}
}
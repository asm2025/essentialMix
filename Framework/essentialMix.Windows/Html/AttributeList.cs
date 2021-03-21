using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using essentialMix.Extensions;
using JetBrains.Annotations;

namespace essentialMix.Windows.Html
{
	/// <summary>
	/// List of attributes
	/// </summary>
	public class AttributeList : List<Attribute>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="T:global::System.Collections.Generic.List`1"/> class that is empty and has the default initial capacity.
		/// </summary>
		public AttributeList()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:global::System.Collections.Generic.List`1"/> class that is empty and has the specified initial capacity.
		/// </summary>
		/// <param name="capacity">The number of elements that the new list can initially store.</param><exception cref="T:global::System.ArgumentOutOfRangeException"><paramref name="capacity"/> is less than 0. </exception>
		public AttributeList(int capacity) 
			: base(capacity)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:global::System.Collections.Generic.List`1"/> class that contains elements copied from the specified collection and has sufficient capacity to accommodate the number of elements copied.
		/// </summary>
		/// <param name="collection">The collection whose elements are copied to the new list.</param><exception cref="T:global::System.ArgumentNullException"><paramref name="collection"/> is null.</exception>
		public AttributeList([NotNull] IEnumerable<Attribute> collection) 
			: base(collection)
		{
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder(1024);
			ForEach(a => sb.AppendLine(Convert.ToString(a)));
			return sb.ToString();
		}

		public Attribute Find(string name) { return string.IsNullOrEmpty(name) ? null : this.FirstOrDefault(a => a.Name.IsSame(name)); }
	}
}
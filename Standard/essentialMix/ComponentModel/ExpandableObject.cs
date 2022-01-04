using System.ComponentModel;
using essentialMix.Patterns.Object;
using JetBrains.Annotations;

namespace essentialMix.ComponentModel;

///  <summary>
///  NOTE THAT: The basic architecture and the cache-mechanism used in ExpandableObjects
///  are lifted from Stephen Taub's article in NET Matters April/May issues, 
///  NET Matters ICustomTypeDescriptor, Part 1 -- MSDN Magazine, April 2005
/// 		http://msdn.microsoft.com/msdnmag/issues/05/04/NETMatters/default.aspx
/// 	NET Matters ICustomTypeDescriptor, Part 2 -- MSDN Magazine, May 2005
/// 		http://msdn.microsoft.com/msdnmag/issues/05/05/NETMatters/default.aspx
///  My thanks to the author for the information/techniques covered therein and permission to
///  reuse code for non-profit purposes.
///  https://www.codeproject.com/Articles/12615/Automatic-Expandable-Properties-in-a-PropertyGrid
///  </summary>
///  <seealso cref="T:essentialMix.Disposable" />
///  <seealso cref="T:System.IDisposable" />
[TypeConverter(typeof(ExpandableObjectConverter<ExpandableObject>))]
public abstract class ExpandableObject : Disposable
{
	private ExpandablePropertiesTypeDescriptionProvider _provider;

	/// <inheritdoc />
	protected ExpandableObject()
	{
		_provider = new ExpandablePropertiesTypeDescriptionProvider(GetType());
		TypeDescriptor.AddProvider(_provider, this);
	}

	/// <inheritdoc />
	[NotNull]
	public override string ToString() { return string.Empty; }

	/// <inheritdoc />
	protected override void Dispose(bool disposing)
	{
		if (disposing && _provider != null)
		{
			TypeDescriptor.RemoveProvider(_provider, this);
			_provider = null;
		}
		base.Dispose(disposing);
	}
}
using System;
using asm.Collections.Specialized;

namespace asm.Windows.UITypeEditors
{
	public class IPAddressEntryCollectionEditor : CollectionEditor<IPAddressEntry>
	{
		public IPAddressEntryCollectionEditor(Type type) 
			: base(type)
		{
		}
	}
}
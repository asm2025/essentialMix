using System;
using asm.Network;

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
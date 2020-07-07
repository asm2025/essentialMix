using System.Diagnostics;
using System.Runtime.InteropServices;
using asm.Collections;

namespace asm.Other.Microsoft.Collections
{
	// based on https://referencesource.microsoft.com/#system/compmod/system/collections/generic/linkedlist.cs
	// Note following class is not serializable since we customized the serialization of SinglyLinkedList. 
	[DebuggerDisplay("{Value}")]
	[ComVisible(false)]
	public sealed class SinglyLinkedListNode<T> : INode<T>
	{
		internal SinglyLinkedList<T> _list;
		internal SinglyLinkedListNode<T> _next;
		internal T _item;

		public SinglyLinkedListNode(T value)
		{
			_item = value;
		}

		internal SinglyLinkedListNode(SinglyLinkedList<T> list, T value)
		{
			_list = list;
			_item = value;
		}

		public SinglyLinkedList<T> List => _list;

		public SinglyLinkedListNode<T> Next => _next == null || _next == _list._head ? null : _next;

		public T Value
		{
			get => _item;
			set => _item = value;
		}

		internal void Invalidate()
		{
			_list = null;
			_next = null;
		}
	}
}
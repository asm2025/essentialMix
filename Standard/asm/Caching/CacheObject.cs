﻿using System;
using System.Runtime.Serialization;

namespace asm.Caching
{
	[Serializable, DataContract]
	public class CacheObject : ICacheObject
    {
		/// <inheritdoc />
		public CacheObject()
		{
		}

		[DataMember]
		public string Validator { get; set; }

		[DataMember]
		public byte[] Item { get; set; }
    }
}
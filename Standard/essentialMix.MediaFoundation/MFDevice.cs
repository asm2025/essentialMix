using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using essentialMix.MediaFoundation.Transform;
using essentialMix.Patterns.Object;
using JetBrains.Annotations;

namespace essentialMix.MediaFoundation
{
	[DebuggerDisplay("{Name}")]
	public class MFDevice : Disposable
	{
		private object _syncRoot;
		private string _name;
		private string _symbolicName;

		/// <inheritdoc />
		public MFDevice([NotNull] IMFActivate activator)
		{
			Activator = activator;
		}

		/// <inheritdoc />
		public override string ToString() { return Name; }

		public object SyncRoot
		{
			get
			{
				if (_syncRoot == null) Interlocked.CompareExchange(ref _syncRoot, new object(), null);
				return _syncRoot;
			}
		}

		public IMFActivate Activator { get; protected set; }

		public string Name
		{
			get
			{
				if (_name == null)
				{
					lock(SyncRoot)
					{
						if (_name == null) Activator.GetAllocatedString(MFAttributesClsid.MF_DEVSOURCE_ATTRIBUTE_FRIENDLY_NAME, out _name, out _);
					}
				}

				return _name;
			}
		}

		public string SymbolicName
		{
			get
			{
				if (_symbolicName == null)
				{
					lock(SyncRoot)
					{
						if (_symbolicName == null)
							Activator.GetAllocatedString(MFAttributesClsid.MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK, out _symbolicName, out _);
					}
				}

				return _symbolicName;
			}
		}

		[NotNull]
		public static IMFActivate[] GetSources(Guid filterCategory)
		{
			IMFAttributes attributes = null;

			try
			{
				if (ResultCom.Failed(MFAPI.MFCreateAttributes(out attributes, 1)) ||
					ResultCom.Failed(attributes.SetGUID(MFAttributesClsid.MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE, filterCategory)) ||
					ResultCom.Failed(MFAPI.MFEnumDeviceSources(attributes, out IMFActivate[] mfActivators, out int count))) return Array.Empty<IMFActivate>();
				if (count < mfActivators.Length) Array.Resize(ref mfActivators, count);
				return mfActivators;
			}
			finally
			{
				if (attributes != null) Marshal.ReleaseComObject(attributes);
			}
		}

		[NotNull]
		public static IMFActivate[] GetTransforms(Guid filterCategory)
		{
			if (ResultCom.Failed(MFAPI.MFTEnumEx(filterCategory, MFT_EnumFlag.All, null, null, out IMFActivate[] mfActivators, out int count))) return Array.Empty<IMFActivate>();
			if (count < mfActivators.Length) Array.Resize(ref mfActivators, count);
			return mfActivators;
		}
	}
}

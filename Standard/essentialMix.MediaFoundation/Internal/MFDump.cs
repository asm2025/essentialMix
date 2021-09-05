using System;
using System.Reflection;
using System.Text;
using JetBrains.Annotations;

namespace essentialMix.MediaFoundation.Internal
{
	internal static class MFDump
	{
		[NotNull]
		public static string LookupName([NotNull] Type t, Guid gSeeking)
		{
			FieldInfo[] fia = t.GetFields(BindingFlags.Static | BindingFlags.Public);

			foreach (FieldInfo fi in fia)
			{
				if (gSeeking.CompareTo(fi.GetValue(t)) == 0)
					return fi.Name;
			}

			return gSeeking.ToString();
		}

		[NotNull]
		public static string DumpAttribs(IMFAttributes ia)
		{
			if (ia == null) return "<null>\n";

			Variant pv = new Variant();
			StringBuilder sb = new StringBuilder(1024);

			int hr = ia.GetCount(out int iCount);
			MFError.ThrowExceptionForHR(hr);

			for (int x = 0; x < iCount; x++)
			{
				hr = ia.GetItemByIndex(x, out Guid g, pv);
				MFError.ThrowExceptionForHR(hr);
				sb.AppendFormat("{0} {1} {2}\n", LookupName(typeof(MFAttributesClsid), g), pv.GetMFAttributeType(), AttribValueToString(g, pv));
			}

			return sb.ToString();

		}

		[NotNull]
		public static string UnpackRatio(ulong l)
		{
			int w, h;
			MFAPI.Unpack2UINT32AsUINT64((long)l, out w, out h);
			return string.Format("{0}x{1}", w, h);
		}

		public static string AttribValueToString(Guid gAttrib, [NotNull] ConstPropVariant pv)
		{
			if (gAttrib == MFAttributesClsid.MF_MT_MAJOR_TYPE || gAttrib == MFAttributesClsid.MF_MT_SUBTYPE)
				return LookupName(typeof(MFMediaType), pv.GetGuid());

			if (gAttrib == MFAttributesClsid.MF_TRANSFORM_CATEGORY_Attribute)
				return LookupName(typeof(MFTransformCategory), pv.GetGuid());

			if (gAttrib == MFAttributesClsid.MF_TRANSCODE_CONTAINERTYPE)
				return LookupName(typeof(MFTranscodeContainerType), pv.GetGuid());

			if (gAttrib == MFAttributesClsid.MF_EVENT_TOPOLOGY_STATUS)
				return ((MFTopoStatus)pv.GetUInt()).ToString();

			if (gAttrib == MFAttributesClsid.MF_MT_INTERLACE_MODE)
				return ((MFVideoInterlaceMode)pv.GetUInt()).ToString();

			if (gAttrib == MFAttributesClsid.MF_TOPOLOGY_DXVA_MODE)
				return ((MFTOPOLOGY_DXVA_MODE)pv.GetUInt()).ToString();

			return gAttrib == MFAttributesClsid.MF_MT_FRAME_SIZE ||
					gAttrib == MFAttributesClsid.MF_MT_FRAME_RATE ||
					gAttrib == MFAttributesClsid.MF_MT_PIXEL_ASPECT_RATIO
						? UnpackRatio(pv.GetULong())
						: pv.ToString();
		}
	}
}
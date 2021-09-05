﻿using System.Runtime.InteropServices;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.EVR
{
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	[UnmanagedName("MFVideoNormalizedRect")]
	public class MFVideoNormalizedRect
	{
		public float left;
		public float top;
		public float right;
		public float bottom;

		public MFVideoNormalizedRect() { }

		public MFVideoNormalizedRect(float l, float t, float r, float b)
		{
			left = l;
			top = t;
			right = r;
			bottom = b;
		}

		public override string ToString() { return string.Format("left = {0}, top = {1}, right = {2}, bottom = {3}", left, top, right, bottom); }

		public override int GetHashCode()
		{
			return left.GetHashCode() |
					top.GetHashCode() |
					right.GetHashCode() |
					bottom.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (obj is MFVideoNormalizedRect)
			{
				MFVideoNormalizedRect cmp = (MFVideoNormalizedRect)obj;

				return right == cmp.right && bottom == cmp.bottom && left == cmp.left && top == cmp.top;
			}

			return false;
		}

		public bool IsEmpty() { return right <= left || bottom <= top; }

		public void CopyFrom(MFVideoNormalizedRect from)
		{
			left = from.left;
			top = from.top;
			right = from.right;
			bottom = from.bottom;
		}
	}
}
using System.Globalization;
using asm.DevExpress.XtraEditors;
using asm.Patterns.DateTime;
using DevExpress.Data.Mask;

namespace asm.DevExpress.Data
{
	public class CustomDateTimeMaskFormatInfo : DateTimeMaskFormatInfo
	{
		public CustomDateTimeMaskFormatInfo(string mask, DateTimeFormatInfo dateTimeFormatInfo, TimeSpanEdit timeSpanEdit)
			: base(mask, dateTimeFormatInfo)
		{
			for (int i = 0; i < Count; i++)
			{
				if (innerList[i] is DateTimeMaskFormatElement_d)
				{
					innerList[i] = new DateTimeMaskFormatElementDHMS(timeSpanEdit, TimeUnit.Day, dateTimeFormatInfo);
				}
				if (innerList[i] is DateTimeMaskFormatElement_H24 || innerList[i] is DateTimeMaskFormatElement_h12)
				{
					innerList[i] = new DateTimeMaskFormatElementDHMS(timeSpanEdit, TimeUnit.Hour, dateTimeFormatInfo);
				}
				if (innerList[i] is DateTimeMaskFormatElement_Min)
				{
					innerList[i] = new DateTimeMaskFormatElementDHMS(timeSpanEdit, TimeUnit.Minute, dateTimeFormatInfo);
				}
				if (innerList[i] is DateTimeMaskFormatElement_s)
				{
					innerList[i] = new DateTimeMaskFormatElementDHMS(timeSpanEdit, TimeUnit.Second, dateTimeFormatInfo);
				}
			}
		}
	}
}
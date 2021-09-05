using System.Diagnostics;
using essentialMix.Patterns.Object;

namespace essentialMix.MediaFoundation
{
	public abstract class COMBase : Disposable
	{
		/// <inheritdoc />
		protected COMBase()
		{
		}

		[Conditional("DEBUG")]
		public static void Trace(string s)
		{
			Debug.WriteLine(s);
		}
	}
}
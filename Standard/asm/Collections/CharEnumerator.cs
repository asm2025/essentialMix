namespace asm.Collections
{
	public class CharEnumerator : StringEnumerator<char>
	{
		public CharEnumerator(string targetString, char delimiter)
			: base(targetString, delimiter)
		{
		}

		protected override bool FindNext()
		{
			if (!IsIterable || Done) return false;

			if (Index < 0)
			{
				Index = TargetString.IndexOf(Delimiter, 0);

				if (Index < 0)
				{
					Index = 0;
					SearchLength = TargetString.Length;
					Done = true;
					return true;
				}
			}
			else
			{
				Index = NextIndex;
			}

			Index++;

			if (Index >= TargetString.Length)
			{
				Reset();
				Done = true;
				return false;
			}

			NextIndex = TargetString.IndexOf(Delimiter, Index);

			while (NextIndex > -1 && Index + 1 == NextIndex)
			{
				Index = NextIndex + 1;

				if (Index >= TargetString.Length)
				{
					Reset();
					Done = true;
					return false;
				}

				NextIndex = TargetString.IndexOf(Delimiter, Index);
			}

			SearchLength = NextIndex > -1 ? NextIndex - Index : TargetString.Length - Index;
			Done = NextIndex < 0;
			return true;
		}
	}
}
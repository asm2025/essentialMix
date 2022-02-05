namespace essentialMix.Collections;

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
			Index = 0;

			while (Index < TargetString.Length && TargetString[Index] == Delimiter) 
				Index++;

			if (Index == TargetString.Length - 1)
			{
				Reset();
				Done = true;
				return false;
			}

			NextIndex = TargetString.IndexOf(Delimiter, Index + 1);
			SearchLength = (NextIndex > -1
								? NextIndex
								: TargetString.Length) - Index;
			return true;
		}
			
		if (NextIndex < 0 || NextIndex >= TargetString.Length - 1)
		{
			Reset();
			Done = true;
			return false;
		}

		Index = NextIndex + 1;

		while (Index < TargetString.Length && TargetString[Index] == Delimiter) 
			Index++;

		if (Index == TargetString.Length - 1)
		{
			Reset();
			Done = true;
			return false;
		}

		NextIndex = TargetString.IndexOf(Delimiter, Index + 1);
		SearchLength = (NextIndex > -1
							? NextIndex
							: TargetString.Length) - Index;
		Done = NextIndex < 0;
		return true;
	}
}
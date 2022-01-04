namespace essentialMix.Collections;

public class CharLister : StringLister<char>
{
	public CharLister(string targetString, char delimiter) 
		: base(targetString, delimiter)
	{
	}

	protected override bool FindNext()
	{
		if (!IsIterable || Done) return false;

		if (SearchIndex < 0)
		{
			SearchIndex = TargetString.IndexOf(Delimiter, 0);

			if (SearchIndex < 0)
			{
				SearchIndex = 0;
				SearchLength = TargetString.Length;
				Done = true;
				return true;
			}
		}
		else
		{
			SearchIndex = NextIndex;
		}

		SearchIndex++;

		if (SearchIndex >= TargetString.Length)
		{
			Reset();
			Done = true;
			return false;
		}

		NextIndex = TargetString.IndexOf(Delimiter, SearchIndex);

		while (NextIndex > -1 && SearchIndex + 1 == NextIndex)
		{
			SearchIndex = NextIndex + 1;

			if (SearchIndex >= TargetString.Length)
			{
				Reset();
				Done = true;
				return false;
			}

			NextIndex = TargetString.IndexOf(Delimiter, SearchIndex);
		}

		SearchLength = NextIndex > -1 ? NextIndex - SearchIndex : TargetString.Length - SearchIndex;
		Done = NextIndex < 0;
		return true;
	}
}
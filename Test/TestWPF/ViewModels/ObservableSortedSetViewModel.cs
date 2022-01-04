using System.Collections.Generic;
using System.IO;
using essentialMix.Collections;
using essentialMix.Helpers;

namespace TestWPF.ViewModels;

public class ObservableSortedSetViewModel : ViewModelBase
{
	private ObservableSortedSet<string> _items;

	/// <inheritdoc />
	public ObservableSortedSetViewModel() 
	{
	}

	public ObservableSortedSet<string> Items
	{
		get => _items;
		set
		{
			_items = value;
			OnPropertyChanged();
		}
	}

	/// <inheritdoc />
	public override void Generate()
	{
		ObservableSortedSet<string> items = Items ?? new ObservableSortedSet<string>();
		items.Clear();

		string path = Directory.GetCurrentDirectory();
		IEnumerable<string> fileNames = DirectoryHelper.EnumerateFiles(path, SearchOption.AllDirectories);

		foreach (string fileName in fileNames)
		{
			string filePath = Path.GetRelativePath(path, fileName);
			items.Add(filePath);
		}

		Items = items;
	}

	/// <inheritdoc />
	public override void Clear()
	{
		Items?.Clear();
	}
}
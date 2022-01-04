using System.Collections.Generic;
using System.IO;
using essentialMix.Collections;
using essentialMix.Helpers;

namespace TestWPF.ViewModels;

public class ObservableHashSetViewModel : ViewModelBase
{
	private ObservableHashSet<string> _items;

	public ObservableHashSetViewModel() 
	{
	}

	public ObservableHashSet<string> Items
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
		ObservableHashSet<string> items = Items ?? new ObservableHashSet<string>();
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
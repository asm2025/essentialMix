using System.Collections.Generic;
using System.IO;
using essentialMix.Collections;
using essentialMix.Helpers;

namespace TestWPF.ViewModels
{
	public class ObservableDictionaryViewModel : ViewModelBase
	{
		private ObservableDictionary<string, long> _items;

		public ObservableDictionaryViewModel() 
		{
		}

		public ObservableDictionary<string, long> Items
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
			ObservableDictionary<string, long> items = Items ?? new ObservableDictionary<string, long>();
			items.Clear();

			string path = Directory.GetCurrentDirectory();
			IEnumerable<string> fileNames = DirectoryHelper.EnumerateFiles(path, SearchOption.AllDirectories);

			foreach (string fileName in fileNames)
			{
				string filePath = Path.GetRelativePath(path, fileName);
				long size = FileHelper.GetLength(fileName);
				items.Add(filePath, size);
			}

			Items = items;
		}

		/// <inheritdoc />
		public override void Clear()
		{
			Items?.Clear();
		}
	}
}

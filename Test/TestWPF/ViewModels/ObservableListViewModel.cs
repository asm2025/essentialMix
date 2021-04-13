using System.Collections.Generic;
using System.IO;
using essentialMix.Collections;
using essentialMix.Helpers;

namespace TestWPF.ViewModels
{
	public class ObservableListViewModel : ViewModelBase
	{
		private ObservableList<string> _items;

		/// <inheritdoc />
		public ObservableListViewModel() 
		{
		}

		public ObservableList<string> Items
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
			ObservableList<string> items = Items ?? new ObservableList<string>();
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
}

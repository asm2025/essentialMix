using System.Collections.Generic;
using System.IO;
using essentialMix.Collections;
using essentialMix.Helpers;

namespace TestWPF.ViewModels
{
	public class ObservableKeyedDictionaryViewModel : ViewModelBase
	{
		public class FileInfo
		{
			public string FileName { get; set; }
			public long Size { get; set; }
		}

		private ObservableKeyedDictionary<string, FileInfo> _items;

		/// <inheritdoc />
		public ObservableKeyedDictionaryViewModel() 
		{
		}

		public ObservableKeyedDictionary<string, FileInfo> Items
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
			ObservableKeyedDictionary<string, FileInfo> items = Items ?? new ObservableKeyedDictionary<string, FileInfo>(e => e.FileName);
			items.Clear();

			string path = Directory.GetCurrentDirectory();
			IEnumerable<string> fileNames = DirectoryHelper.EnumerateFiles(path, SearchOption.AllDirectories);

			foreach (string fileName in fileNames)
			{
				FileInfo fi = new FileInfo
				{
					FileName = Path.GetRelativePath(path, fileName),
					Size = FileHelper.GetLength(fileName)
				};

				items.Add(fi);
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

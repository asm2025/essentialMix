using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using asm.Extensions;
using JetBrains.Annotations;
using MissingWXSFiles.Properties;

namespace MissingWXSFiles
{
	public partial class MainForm : Form
	{
		private const string FMT_HEAD = "<Component Id=";

		public MainForm()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			txtBin.Text = Settings.Default.LastFolder;
			txtFile.Text = Settings.Default.LastFile;
		}

		private void btnBin_Click(object sender, EventArgs e)
		{
			fbd.Reset();
			if (fbd.ShowDialog(this) != DialogResult.OK) return;
			txtBin.Text = fbd.SelectedPath;
			Settings.Default.LastFolder = fbd.SelectedPath;
			Settings.Default.Save();
		}

		private void btnFile_Click(object sender, EventArgs e)
		{
			ofd.Reset();
			if (ofd.ShowDialog(this) != DialogResult.OK) return;
			txtFile.Text = ofd.FileName;
			Settings.Default.LastFile = ofd.FileName;
			Settings.Default.Save();
		}

		private void txtBin_TextChanged(object sender, EventArgs e) { btnProcess.Enabled = txtBin.Text.Length > 0 && txtFile.Text.Length > 0; }

		private void txtFile_TextChanged(object sender, EventArgs e) { btnProcess.Enabled = txtBin.Text.Length > 0 && txtFile.Text.Length > 0; }

		private void btnProcess_Click(object sender, EventArgs e)
		{
			txtOut.Text = string.Empty;

			string file = txtFile.Text;
			if (!File.Exists(file)) return;

			string path = txtBin.Text;
			if (!Directory.Exists(path)) return;

			HashSet<string> visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			int rootLength = path.Length + 1;

			try
			{
				Cursor.Current = Cursors.WaitCursor;

				string alreadyAdded = File.ReadAllText(file);
				StringBuilder sb = new StringBuilder();

				foreach (string directory in Directory.EnumerateDirectories(path))
				{
					List(sb, directory, rootLength, alreadyAdded, visited);
				}

				List(sb, path, rootLength, alreadyAdded, visited);
				txtOut.Text = sb.ToString();
			}
			finally
			{
				Cursor.Current = Cursors.Default;
			}
		}

		private void List([NotNull] StringBuilder sb, [NotNull] string path, int rootLength, string alreadyAdded, ISet<string> visited)
		{
			const string FMT_COMP = @"<Component Id=""{1}"" DiskId=""1"" Win64=""yes"" Guid=""{0}"">
	<File Id=""{1}"" Name=""{2}"" Source=""$(var.SourcePath){2}"" KeyPath=""yes"" Vital=""yes"" />
</Component>";

			foreach (string directory in Directory.EnumerateDirectories(path))
			{
				List(sb, directory, rootLength, alreadyAdded, visited);
			}

			foreach (string file in Directory.EnumerateFiles(path))
			{
				if (file.Contains("JetBrains.Annotations", StringComparison.OrdinalIgnoreCase) || file.EndsWith(".pdb", StringComparison.OrdinalIgnoreCase)) continue;
				string pth = file.Substring(rootLength);
				string fun = pth.ToUpperInvariant();
				string search = GetIDSearchString(fun);
				if (!visited.Add(search) || alreadyAdded.Contains(search, StringComparison.OrdinalIgnoreCase)) continue;
				sb.AppendWithLine(string.Format(FMT_COMP, Guid.NewGuid().ToString().Trim('{', '}').ToUpperInvariant(), fun, pth));
			}

			if (sb.Length > 0) sb.AppendLine();
		}

		private static string GetIDSearchString(string pathId)
		{
			return FMT_HEAD + pathId.Replace('\\', '_').Quote();
		}
	}
}

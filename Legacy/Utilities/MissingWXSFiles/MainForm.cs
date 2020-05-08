using System;
using System.IO;
using System.Windows.Forms;
using JetBrains.Annotations;
using MissingWXSFiles.Properties;

namespace MissingWXSFiles
{
	public partial class MainForm : Form
	{
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

			string content = File.ReadAllText(file);

			foreach (string directory in Directory.EnumerateDirectories(path))
			{
				List(directory, path.Length + 1, content);
			}

			List(path, path.Length + 1, content);
		}

		private void List([NotNull] string path, int rootLength, string content)
		{
			const string FMT_HEAD = "<Component Id=\"{0}\"";
			const string FMT_COMP = @"
			<Component Id=""{1}"" DiskId=""1"" Win64=""yes"" Guid=""{0}"">
				<File Id=""{1}"" Name=""{2}"" Source=""$(var.SourcePath){2}"" KeyPath=""yes"" Vital=""yes"" />
			</Component>
";

			foreach (string directory in Directory.EnumerateDirectories(path))
			{
				List(directory, rootLength, content);
			}

			foreach (string file in Directory.EnumerateFiles(path))
			{
				string pth = file.Substring(rootLength);
				string fun = pth.ToUpper();
				string search = string.Format(FMT_HEAD, fun);
				if (content.Contains(search) || txtOut.Text.Contains(search)) continue;
				txtOut.Text += string.Format(FMT_COMP, Guid.NewGuid().ToString().Trim('{', '}').ToUpper(), fun, pth);
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using asm.Extensions;
using GenerateWIXComponents.Properties;

namespace GenerateWIXComponents
{
	public partial class MainForm : Form
	{
		private class ComponentInfo
		{
			public ComponentInfo()
			{
			}

			public string Prefix { get; set; }
			public string KeyFileName { get; set; }
			public IList<string> FileNames { get; set; }
		}

		private const string DEF_DIR = "INSTALLDIR";

		//{0} = Uppercase file name, {1} = file name, {2} = GUID, {3} = add line
		private const string GROUP_BEGIN = "<ComponentGroup Id=\"{0}Components\" Directory=\"{1}\">";
		private const string GROUP_END = "</ComponentGroup>";
		private const string COMPONENT_BEGIN = "<Component Id=\"{0}{1}\" DiskId=\"1\" Win64=\"$(var.Win64)\" Guid=\"{2}\">";
		private const string COMPONENT_END = "</Component>";
		private const string ADD_KEY_FILE = "<File Id=\"{0}{1}\" Name=\"{2}\" Source=\"$(var.SourcePath){3}\" KeyPath=\"yes\" />";
		private const string ADD_FILE = "<File Id=\"{0}{1}\" Name=\"{2}\" Source=\"$(var.SourcePath){2}\" />";

		private readonly string _startText;
		private readonly string _endText = "&Stop";

		private bool _loading;

		public MainForm()
		{
			_loading = true;
			InitializeComponent();
			_startText = btnGenerate.Text;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			chkGroupFiles.Checked = Settings.Default.GroupFiles;
			EnableControls();
			_loading = false;
		}

		private void btnBin_Click(object sender, EventArgs e)
		{
			fbd.Reset();
			if (fbd.ShowDialog(this) != DialogResult.OK) return;
			txtBin.Text = fbd.SelectedPath;
		}

		private void txtBin_TextChanged(object sender, EventArgs e)
		{
			txtInput.Text = string.Empty;

			string path = txtBin.Text;
			if (string.IsNullOrEmpty(path) || !Directory.Exists(path)) return;

			int rootLength = path.Length + 1;
			StringBuilder sb = new StringBuilder();

			foreach (string directory in Directory.EnumerateDirectories(path))
			{
				AddFiles(sb, directory, rootLength);
			}

			AddFiles(sb, path, rootLength);
			txtInput.Text = sb.ToString();

			static void AddFiles(StringBuilder sb, string path, int rootLength)
			{
				foreach (string directory in Directory.EnumerateDirectories(path))
				{
					AddFiles(sb, directory, rootLength);
				}

				foreach (string file in Directory.EnumerateFiles(path))
				{
					if (file.Contains("JetBrains.Annotations", StringComparison.OrdinalIgnoreCase) || file.EndsWith(".pdb", StringComparison.OrdinalIgnoreCase)) continue;
					string pth = file.Substring(rootLength);
					sb.AppendWithLine(pth);
				}
			}
		}

		private void worker_DoWork(object sender, DoWorkEventArgs e)
		{
			bool groupFiles = false;
			Queue<string> lines = null;

			this.InvokeIf(() =>
			{
				tvwOutput.Nodes.Clear();
				groupFiles = chkGroupFiles.Checked;
				lines = new Queue<string>(txtInput.Lines.SkipNullOrEmptyTrim());
			});

			if (lines == null || lines.Count == 0) return;
			EnableControls();

			IDictionary<string, IDictionary<string, ComponentInfo>> dirs = new Dictionary<string, IDictionary<string, ComponentInfo>>(StringComparer.OrdinalIgnoreCase);
			bool orphansCleared = false;
			HashSet<string> orphans = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			try
			{
				while (lines.Count > 0)
				{
					string line = lines.Dequeue();
					if (line.Contains("JetBrains.Annotations", StringComparison.OrdinalIgnoreCase)) continue;

					string ext = Path.GetExtension(line).ToLowerInvariant();
					if (ext == ".pdb") continue;

					int sep = line.IndexOf('\\');
					string dir;
					string pfx;

					if (sep < 0)
					{
						dir = DEF_DIR;
						pfx = string.Empty;
					}
					else
					{
						dir = line.Substring(0, sep);
						pfx = dir.ToUpperInvariant() + "_";
						line = line.Substring(sep + 1, line.Length - sep - 1);
					}

					if (!dirs.TryGetValue(dir, out IDictionary<string, ComponentInfo> filtered))
					{
						filtered = new Dictionary<string, ComponentInfo>(StringComparer.OrdinalIgnoreCase);
						dirs[dir] = filtered;
					}

					string fileName = Path.GetFileNameWithoutExtension(line);
					string key = pfx + fileName;

					if (ext == ".dll" || ext == ".exe")
					{
						if (!filtered.ContainsKey(key)) filtered.Add(key, new ComponentInfo {Prefix = pfx, KeyFileName = line});
					}
					else
					{
						if (!filtered.TryGetValue(key, out ComponentInfo component))
						{
							if (!orphansCleared) orphans.Add(dir + '\\' + line);
						}
						else
						{
							IList<string> list = component.FileNames;

							if (list == null)
							{
								list = new List<string> {line};
								filtered[key].FileNames = list;
							}
							else
							{
								list.Add(line);
							}
						}
					}

					if (lines.Count == 0 || orphans.Count == 0) continue;

					foreach (string orphan in orphans) 
						lines.Enqueue(orphan);

					orphans.Clear();
					orphansCleared = true;
				}
			}
			catch (Exception exception)
			{
				MessageBox.Show(this, exception.CollectMessages(), "Error...", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			if (dirs.Count == 0) return;

			tvwOutput.InvokeIf(() =>
			{
				tvwOutput.BeginUpdate();

				foreach (KeyValuePair<string, IDictionary<string, ComponentInfo>> dirPair in dirs)
				{
					TreeNode node = new TreeNode(dirPair.Key);
					AddComponentNodes(node, dirPair.Value, groupFiles);
					tvwOutput.Nodes.Add(node);
					node.Expand();
				}

				tvwOutput.EndUpdate();
			});

			static void AddComponentNodes(TreeNode root, IDictionary<string, ComponentInfo> dictionary, bool groupFiles)
			{
				foreach (KeyValuePair<string, ComponentInfo> pair in dictionary)
				{
					ComponentInfo component = pair.Value;
					TreeNode componentNode = new TreeNode(component.KeyFileName) { Tag = component };
					root.Nodes.Add(componentNode);
					if (component.FileNames == null) continue;
					
					if (groupFiles)
					{
						foreach (string file in component.FileNames)
							componentNode.Nodes.Add(file);
					}
					else
					{
						foreach (string file in component.FileNames)
						{
							ComponentInfo comp = new ComponentInfo {Prefix = component.Prefix, KeyFileName = file};
							TreeNode cn = new TreeNode(comp.KeyFileName) { Tag = comp };
							root.Nodes.Add(cn);
						}

						component.FileNames = null;
					}
				}
			}
		}

		private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			EnableControls();
		}

		private void txtInput_TextChanged(object sender, EventArgs e)
		{
			EnableControls();
		}

		private void chkGroupFiles_CheckedChanged(object sender, EventArgs e)
		{
			if (_loading) return;
			Settings.Default.GroupFiles = chkGroupFiles.Checked;
			Settings.Default.Save();
		}

		private void btnGenerate_Click(object sender, EventArgs e)
		{
			if (worker.IsBusy)
			{
				worker.CancelAsync();
				SpinWait.SpinUntil(() => !worker.IsBusy);
			}

			worker.RunWorkerAsync();
		}

		private void btnCopy_Click(object sender, EventArgs e)
		{
			const int RETRIES = 3;

			Clipboard.Clear();
			if (!HasOutput()) return;

			StringBuilder sbd = new StringBuilder();
			string tabs = string.Empty;

			foreach (TreeNode rootNode in tvwOutput.Nodes)
			{
				string dir;

				if (rootNode.Text == DEF_DIR)
				{
					dir = string.Empty;
					tabs = string.Empty;
				}
				else
				{
					sbd.AppendWithLine(string.Format(GROUP_BEGIN, rootNode.Text.ToUpperInvariant(), rootNode.Text));
					dir = rootNode.Text + Path.DirectorySeparatorChar;
					tabs = "\t";
				}

				foreach (TreeNode componentNode in rootNode.Nodes)
				{
					ComponentInfo component = (ComponentInfo)componentNode.Tag;

					if (component.FileNames == null)
					{
						sbd.AppendWithLine(tabs + string.Format(COMPONENT_BEGIN, component.Prefix, component.KeyFileName.ToUpperInvariant(), Guid.NewGuid()));
						sbd.AppendWithLine(tabs + "\t" + string.Format(ADD_KEY_FILE, component.Prefix, component.KeyFileName.ToUpperInvariant(), component.KeyFileName, dir + component.KeyFileName));
						sbd.AppendWithLine(tabs + COMPONENT_END);
					}
					else
					{
						sbd.AppendWithLine(tabs + string.Format(COMPONENT_BEGIN, component.Prefix, component.KeyFileName.ToUpperInvariant(), Guid.NewGuid()));
						sbd.AppendWithLine(tabs + "\t" + string.Format(ADD_KEY_FILE, component.Prefix, component.KeyFileName.ToUpperInvariant(), component.KeyFileName, dir + component.KeyFileName));
						
						foreach (string file in component.FileNames)
							sbd.AppendWithLine(tabs + "\t" + string.Format(ADD_FILE, component.Prefix, file.ToUpperInvariant(), file));

						sbd.AppendWithLine(tabs + COMPONENT_END);
					}
				}

				if (tabs.Length > 0) sbd.AppendWithLine(GROUP_END);
			}

			if (sbd.Length > 0) sbd.AppendLine();

			string data = sbd.ToString();
			int clipboardErr = 0;

			while (clipboardErr < RETRIES)
			{
				try
				{
					Clipboard.Clear();
					Clipboard.SetText(data, TextDataFormat.Text);
					break;
				}
				catch (Exception ex)
				{
					clipboardErr++;
					if (clipboardErr < RETRIES) continue;
					MessageBox.Show(ex.Unwrap(), "Error...", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}

		private void btnExit_Click(object sender, EventArgs e)
		{
			worker.CancelAsync();
			Application.Exit();
		}

		private bool HasText()
		{
			bool hasText = false;
			txtInput.InvokeIf(() => hasText = txtInput.Text.Length > 0);
			return hasText;
		}

		private bool HasOutput()
		{
			bool hasItems = false;
			tvwOutput.InvokeIf(() => hasItems = tvwOutput.Nodes.Count > 0);
			return hasItems;
		}

		private void EnableControls(bool enable = true)
		{
			if (worker.IsBusy) enable = false;

			if (!enable)
			{
				this.EnableControls(false, Array.Empty<Control>());
				return;
			}

			this.EnableControls(true, btnGenerate, btnCopy, btnExit);
			this.InvokeIf(() =>
			{
				if (worker.IsBusy)
				{
					btnGenerate.Enabled = true;
					if (btnGenerate.Text != _endText) btnGenerate.Text = _endText;
				}
				else
				{
					btnGenerate.Enabled = HasText();
					if (btnGenerate.Text != _startText) btnGenerate.Text = _startText;
				}

				btnCopy.Enabled = HasOutput();
				btnExit.Enabled = !worker.IsBusy;
			});
		}
	}
}

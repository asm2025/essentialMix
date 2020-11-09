using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
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
		private const string ADD_KEY_LINE = "\t\t<File Id=\"{0}{1}\" Name=\"{2}\" Source=\"$(var.SourcePath){3}\" KeyPath=\"yes\" />";
		private const string ADD_LINE = "\t\t<File Id=\"{0}{1}\" Name=\"{2}\" Source=\"$(var.SourcePath){2}\" />";
		private const string COMPONENT_LINE = @"<Component Id=""{0}{1}"" DiskId=""1"" Win64=""$(var.Win64)"" Guid=""{2}"">{3}</Component>";

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

		private void worker_DoWork(object sender, DoWorkEventArgs e)
		{
			tvwOutput.InvokeIf(() => tvwOutput.Nodes.Clear());

			bool groupFiles = false;
			chkGroupFiles.InvokeIf(() => groupFiles = chkGroupFiles.Checked);

			Queue<string> lines = null;
			txtInput.InvokeIf(() => lines = new Queue<string>(txtInput.Lines));
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
					string ext = Path.GetExtension(line)?.ToLower();
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
						pfx = dir.ToUpper() + "_";
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

					if (lines.Count == 0 && orphans.Count > 0)
					{
						foreach (string orphan in orphans) 
							lines.Enqueue(orphan);

						orphans.Clear();
						orphansCleared = true;
					}
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
				return;
			}

			worker.RunWorkerAsync();
		}

		private void btnCopy_Click(object sender, EventArgs e)
		{
			Clipboard.Clear();
			if (!HasOutput()) return;

			StringBuilder sbdir = new StringBuilder(tvwOutput.GetNodeCount(true) * 256);

			foreach (TreeNode rootNode in tvwOutput.Nodes)
			{
				string dir;

				if (rootNode.Text == DEF_DIR)
				{
					sbdir.AppendWithLine($"<ComponentGroup Id=\"XXXComponents\" Directory=\"{rootNode.Text.ToUpper()}\">");
					dir = string.Empty;
				}
				else
				{
					sbdir.AppendWithLine($"<ComponentGroup Id=\"{rootNode.Text.ToUpper()}Components\" Directory=\"{rootNode.Text.ToUpper()}\">");
					dir = rootNode.Text + Path.DirectorySeparatorChar;
				}

				foreach (TreeNode componentNode in rootNode.Nodes)
				{
					ComponentInfo component = (ComponentInfo)componentNode.Tag;

					if (component.FileNames == null)
						sbdir.AppendWithLine(string.Format(COMPONENT_LINE, component.Prefix, component.KeyFileName.ToUpper(), Guid.NewGuid(), string.Format(ADD_KEY_LINE, component.Prefix, component.KeyFileName.ToUpper(), component.KeyFileName, dir + component.KeyFileName)));
					else
					{
						StringBuilder sb = new StringBuilder(1024);
						sb.Append(string.Format(ADD_KEY_LINE, component.Prefix, component.KeyFileName.ToUpper(), component.KeyFileName, dir + component.KeyFileName));

						foreach (string file in component.FileNames)
							sb.AppendWithLine(string.Format(ADD_LINE, component.Prefix, file.ToUpper(), file));

						sbdir.AppendWithLine(string.Format(COMPONENT_LINE, component.Prefix, component.KeyFileName.ToUpper(), Guid.NewGuid(), sb));
					}
				}

				sbdir.AppendWithLine("</ComponentGroup>");
			}

			sbdir.AppendLine();
			Clipboard.SetText(sbdir.ToString(), TextDataFormat.Text);
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
			btnGenerate.InvokeIf(() =>
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
			});
			btnCopy.InvokeIf(() => btnCopy.Enabled = HasOutput());
			btnExit.InvokeIf(() => btnExit.Enabled = !worker.IsBusy);
		}
	}
}

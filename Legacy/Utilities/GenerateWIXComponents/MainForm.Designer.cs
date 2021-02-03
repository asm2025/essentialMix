using System.ComponentModel;
using System.Windows.Forms;
using asm.Helpers;

namespace GenerateWIXComponents
{
	partial class MainForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) ObjectHelper.Dispose(ref components);
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.tlp = new System.Windows.Forms.TableLayoutPanel();
			this.flpButtons = new System.Windows.Forms.FlowLayoutPanel();
			this.btnExit = new System.Windows.Forms.Button();
			this.btnCopy = new System.Windows.Forms.Button();
			this.btnGenerate = new System.Windows.Forms.Button();
			this.chkGroupFiles = new System.Windows.Forms.CheckBox();
			this.txtBin = new System.Windows.Forms.TextBox();
			this.btnBin = new System.Windows.Forms.Button();
			this.splt = new System.Windows.Forms.SplitContainer();
			this.txtInput = new System.Windows.Forms.TextBox();
			this.tvwOutput = new System.Windows.Forms.TreeView();
			this.worker = new System.ComponentModel.BackgroundWorker();
			this.tip = new System.Windows.Forms.ToolTip(this.components);
			this.fbd = new System.Windows.Forms.FolderBrowserDialog();
			this.tlp.SuspendLayout();
			this.flpButtons.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splt)).BeginInit();
			this.splt.Panel1.SuspendLayout();
			this.splt.Panel2.SuspendLayout();
			this.splt.SuspendLayout();
			this.SuspendLayout();
			// 
			// tlp
			// 
			this.tlp.ColumnCount = 4;
			this.tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 32F));
			this.tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 32F));
			this.tlp.Controls.Add(this.flpButtons, 0, 2);
			this.tlp.Controls.Add(this.txtBin, 0, 0);
			this.tlp.Controls.Add(this.btnBin, 3, 0);
			this.tlp.Controls.Add(this.splt, 0, 1);
			this.tlp.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tlp.Location = new System.Drawing.Point(0, 0);
			this.tlp.Name = "tlp";
			this.tlp.RowCount = 2;
			this.tlp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
			this.tlp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tlp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
			this.tlp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tlp.Size = new System.Drawing.Size(414, 391);
			this.tlp.TabIndex = 0;
			// 
			// flpButtons
			// 
			this.tlp.SetColumnSpan(this.flpButtons, 4);
			this.flpButtons.Controls.Add(this.btnExit);
			this.flpButtons.Controls.Add(this.btnCopy);
			this.flpButtons.Controls.Add(this.btnGenerate);
			this.flpButtons.Controls.Add(this.chkGroupFiles);
			this.flpButtons.Dock = System.Windows.Forms.DockStyle.Fill;
			this.flpButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
			this.flpButtons.Location = new System.Drawing.Point(0, 359);
			this.flpButtons.Margin = new System.Windows.Forms.Padding(0);
			this.flpButtons.Name = "flpButtons";
			this.flpButtons.Size = new System.Drawing.Size(414, 32);
			this.flpButtons.TabIndex = 2;
			this.flpButtons.WrapContents = false;
			// 
			// btnExit
			// 
			this.btnExit.Location = new System.Drawing.Point(332, 2);
			this.btnExit.Margin = new System.Windows.Forms.Padding(2);
			this.btnExit.Name = "btnExit";
			this.btnExit.Size = new System.Drawing.Size(80, 28);
			this.btnExit.TabIndex = 3;
			this.btnExit.Text = "E&xit";
			this.tip.SetToolTip(this.btnExit, "Exits the application");
			this.btnExit.UseVisualStyleBackColor = true;
			this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
			// 
			// btnCopy
			// 
			this.btnCopy.Location = new System.Drawing.Point(248, 2);
			this.btnCopy.Margin = new System.Windows.Forms.Padding(2);
			this.btnCopy.Name = "btnCopy";
			this.btnCopy.Size = new System.Drawing.Size(80, 28);
			this.btnCopy.TabIndex = 2;
			this.btnCopy.Text = "&Copy";
			this.tip.SetToolTip(this.btnCopy, "Copies the results to clipboard");
			this.btnCopy.UseVisualStyleBackColor = true;
			this.btnCopy.Click += new System.EventHandler(this.btnCopy_Click);
			// 
			// btnGenerate
			// 
			this.btnGenerate.Location = new System.Drawing.Point(164, 2);
			this.btnGenerate.Margin = new System.Windows.Forms.Padding(2);
			this.btnGenerate.Name = "btnGenerate";
			this.btnGenerate.Size = new System.Drawing.Size(80, 28);
			this.btnGenerate.TabIndex = 1;
			this.btnGenerate.Text = "&Generate";
			this.tip.SetToolTip(this.btnGenerate, "Generate the new components");
			this.btnGenerate.UseVisualStyleBackColor = true;
			this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
			// 
			// chkGroupFiles
			// 
			this.chkGroupFiles.BackColor = System.Drawing.Color.Transparent;
			this.chkGroupFiles.Location = new System.Drawing.Point(79, 3);
			this.chkGroupFiles.Name = "chkGroupFiles";
			this.chkGroupFiles.Size = new System.Drawing.Size(80, 28);
			this.chkGroupFiles.TabIndex = 0;
			this.chkGroupFiles.Text = "&Group Files";
			this.tip.SetToolTip(this.chkGroupFiles, "Groups files in their containing directory.");
			this.chkGroupFiles.UseVisualStyleBackColor = false;
			this.chkGroupFiles.CheckedChanged += new System.EventHandler(this.chkGroupFiles_CheckedChanged);
			// 
			// txtBin
			// 
			this.txtBin.BackColor = System.Drawing.SystemColors.Window;
			this.tlp.SetColumnSpan(this.txtBin, 3);
			this.txtBin.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtBin.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtBin.Location = new System.Drawing.Point(0, 4);
			this.txtBin.Margin = new System.Windows.Forms.Padding(0, 4, 0, 4);
			this.txtBin.Name = "txtBin";
			this.txtBin.Size = new System.Drawing.Size(382, 24);
			this.txtBin.TabIndex = 0;
			this.tip.SetToolTip(this.txtBin, "Binary folder path which contains the target files");
			this.txtBin.WordWrap = false;
			this.txtBin.TextChanged += new System.EventHandler(this.txtBin_TextChanged);
			// 
			// btnBin
			// 
			this.btnBin.Dock = System.Windows.Forms.DockStyle.Fill;
			this.btnBin.Location = new System.Drawing.Point(382, 3);
			this.btnBin.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
			this.btnBin.Name = "btnBin";
			this.btnBin.Size = new System.Drawing.Size(32, 26);
			this.btnBin.TabIndex = 1;
			this.btnBin.Text = "...";
			this.tip.SetToolTip(this.btnBin, "Select the binary folder path which contains the target files to automatically ad" +
        "d it to the input text");
			this.btnBin.UseVisualStyleBackColor = true;
			this.btnBin.Click += new System.EventHandler(this.btnBin_Click);
			// 
			// splt
			// 
			this.splt.BackColor = System.Drawing.Color.Transparent;
			this.tlp.SetColumnSpan(this.splt, 4);
			this.splt.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splt.Location = new System.Drawing.Point(0, 32);
			this.splt.Margin = new System.Windows.Forms.Padding(0);
			this.splt.Name = "splt";
			this.splt.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splt.Panel1
			// 
			this.splt.Panel1.Controls.Add(this.txtInput);
			// 
			// splt.Panel2
			// 
			this.splt.Panel2.Controls.Add(this.tvwOutput);
			this.splt.Size = new System.Drawing.Size(414, 327);
			this.splt.SplitterDistance = 163;
			this.splt.SplitterWidth = 2;
			this.splt.TabIndex = 0;
			// 
			// txtInput
			// 
			this.txtInput.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtInput.Location = new System.Drawing.Point(0, 0);
			this.txtInput.MaxLength = 65535;
			this.txtInput.Multiline = true;
			this.txtInput.Name = "txtInput";
			this.txtInput.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtInput.Size = new System.Drawing.Size(414, 163);
			this.txtInput.TabIndex = 0;
			this.tip.SetToolTip(this.txtInput, "Add file paths here");
			this.txtInput.WordWrap = false;
			this.txtInput.TextChanged += new System.EventHandler(this.txtInput_TextChanged);
			// 
			// tvwOutput
			// 
			this.tvwOutput.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tvwOutput.FullRowSelect = true;
			this.tvwOutput.Indent = 16;
			this.tvwOutput.Location = new System.Drawing.Point(0, 0);
			this.tvwOutput.Name = "tvwOutput";
			this.tvwOutput.Size = new System.Drawing.Size(414, 162);
			this.tvwOutput.TabIndex = 0;
			this.tip.SetToolTip(this.tvwOutput, "This will contain the newly generated components.");
			// 
			// worker
			// 
			this.worker.WorkerSupportsCancellation = true;
			this.worker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.worker_DoWork);
			this.worker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.worker_RunWorkerCompleted);
			// 
			// fbd
			// 
			this.fbd.Description = "Select the path to the binary folder...";
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(414, 391);
			this.Controls.Add(this.tlp);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MinimumSize = new System.Drawing.Size(350, 350);
			this.Name = "MainForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.WindowsDefaultBounds;
			this.Text = "Generate WIX toolset Components";
			this.tlp.ResumeLayout(false);
			this.tlp.PerformLayout();
			this.flpButtons.ResumeLayout(false);
			this.splt.Panel1.ResumeLayout(false);
			this.splt.Panel1.PerformLayout();
			this.splt.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splt)).EndInit();
			this.splt.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private TableLayoutPanel tlp;
		private SplitContainer splt;
		private TextBox txtInput;
		private Button btnExit;
		private Button btnCopy;
		private Button btnGenerate;
		private BackgroundWorker worker;
		private TreeView tvwOutput;
		private CheckBox chkGroupFiles;
		private ToolTip tip;
		private Button btnBin;
		private TextBox txtBin;
		private FlowLayoutPanel flpButtons;
		private FolderBrowserDialog fbd;
	}
}


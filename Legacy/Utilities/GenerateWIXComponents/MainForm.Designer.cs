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
			global::System.ComponentModel.ComponentResourceManager resources = new global::System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.tlp = new global::System.Windows.Forms.TableLayoutPanel();
			this.splt = new global::System.Windows.Forms.SplitContainer();
			this.txtInput = new global::System.Windows.Forms.TextBox();
			this.tvwOutput = new global::System.Windows.Forms.TreeView();
			this.flp = new global::System.Windows.Forms.FlowLayoutPanel();
			this.btnExit = new global::System.Windows.Forms.Button();
			this.btnCopy = new global::System.Windows.Forms.Button();
			this.btnGenerate = new global::System.Windows.Forms.Button();
			this.chkGroupFiles = new global::System.Windows.Forms.CheckBox();
			this.worker = new global::System.ComponentModel.BackgroundWorker();
			this.tlp.SuspendLayout();
			((global::System.ComponentModel.ISupportInitialize)(this.splt)).BeginInit();
			this.splt.Panel1.SuspendLayout();
			this.splt.Panel2.SuspendLayout();
			this.splt.SuspendLayout();
			this.flp.SuspendLayout();
			this.SuspendLayout();
			// 
			// tlp
			// 
			this.tlp.ColumnCount = 1;
			this.tlp.ColumnStyles.Add(new global::System.Windows.Forms.ColumnStyle(global::System.Windows.Forms.SizeType.Percent, 100F));
			this.tlp.ColumnStyles.Add(new global::System.Windows.Forms.ColumnStyle(global::System.Windows.Forms.SizeType.Absolute, 20F));
			this.tlp.Controls.Add(this.splt, 0, 0);
			this.tlp.Controls.Add(this.flp, 0, 1);
			this.tlp.Dock = global::System.Windows.Forms.DockStyle.Fill;
			this.tlp.Location = new global::System.Drawing.Point(0, 0);
			this.tlp.Name = "tlp";
			this.tlp.RowCount = 2;
			this.tlp.RowStyles.Add(new global::System.Windows.Forms.RowStyle(global::System.Windows.Forms.SizeType.Percent, 100F));
			this.tlp.RowStyles.Add(new global::System.Windows.Forms.RowStyle(global::System.Windows.Forms.SizeType.Absolute, 32F));
			this.tlp.Size = new global::System.Drawing.Size(414, 391);
			this.tlp.TabIndex = 0;
			// 
			// splt
			// 
			this.splt.BackColor = global::System.Drawing.Color.Transparent;
			this.splt.Dock = global::System.Windows.Forms.DockStyle.Fill;
			this.splt.Location = new global::System.Drawing.Point(0, 0);
			this.splt.Margin = new global::System.Windows.Forms.Padding(0);
			this.splt.Name = "splt";
			this.splt.Orientation = global::System.Windows.Forms.Orientation.Horizontal;
			// 
			// splt.Panel1
			// 
			this.splt.Panel1.Controls.Add(this.txtInput);
			// 
			// splt.Panel2
			// 
			this.splt.Panel2.Controls.Add(this.tvwOutput);
			this.splt.Size = new global::System.Drawing.Size(414, 359);
			this.splt.SplitterDistance = 179;
			this.splt.SplitterWidth = 2;
			this.splt.TabIndex = 0;
			// 
			// txtInput
			// 
			this.txtInput.Dock = global::System.Windows.Forms.DockStyle.Fill;
			this.txtInput.Location = new global::System.Drawing.Point(0, 0);
			this.txtInput.MaxLength = 65535;
			this.txtInput.Multiline = true;
			this.txtInput.Name = "txtInput";
			this.txtInput.ScrollBars = global::System.Windows.Forms.ScrollBars.Both;
			this.txtInput.Size = new global::System.Drawing.Size(414, 179);
			this.txtInput.TabIndex = 0;
			this.txtInput.WordWrap = false;
			this.txtInput.TextChanged += new global::System.EventHandler(this.txtInput_TextChanged);
			// 
			// tvwOutput
			// 
			this.tvwOutput.Dock = global::System.Windows.Forms.DockStyle.Fill;
			this.tvwOutput.FullRowSelect = true;
			this.tvwOutput.Indent = 16;
			this.tvwOutput.Location = new global::System.Drawing.Point(0, 0);
			this.tvwOutput.Name = "tvwOutput";
			this.tvwOutput.Size = new global::System.Drawing.Size(414, 178);
			this.tvwOutput.TabIndex = 0;
			// 
			// flp
			// 
			this.flp.Controls.Add(this.btnExit);
			this.flp.Controls.Add(this.btnCopy);
			this.flp.Controls.Add(this.btnGenerate);
			this.flp.Controls.Add(this.chkGroupFiles);
			this.flp.Dock = global::System.Windows.Forms.DockStyle.Fill;
			this.flp.FlowDirection = global::System.Windows.Forms.FlowDirection.RightToLeft;
			this.flp.Location = new global::System.Drawing.Point(0, 359);
			this.flp.Margin = new global::System.Windows.Forms.Padding(0);
			this.flp.Name = "flp";
			this.flp.Size = new global::System.Drawing.Size(414, 32);
			this.flp.TabIndex = 0;
			this.flp.WrapContents = false;
			// 
			// btnExit
			// 
			this.btnExit.Location = new global::System.Drawing.Point(332, 2);
			this.btnExit.Margin = new global::System.Windows.Forms.Padding(2);
			this.btnExit.Name = "btnExit";
			this.btnExit.Size = new global::System.Drawing.Size(80, 28);
			this.btnExit.TabIndex = 3;
			this.btnExit.Text = "E&xit";
			this.btnExit.UseVisualStyleBackColor = true;
			this.btnExit.Click += new global::System.EventHandler(this.btnExit_Click);
			// 
			// btnCopy
			// 
			this.btnCopy.Location = new global::System.Drawing.Point(248, 2);
			this.btnCopy.Margin = new global::System.Windows.Forms.Padding(2);
			this.btnCopy.Name = "btnCopy";
			this.btnCopy.Size = new global::System.Drawing.Size(80, 28);
			this.btnCopy.TabIndex = 2;
			this.btnCopy.Text = "&Copy";
			this.btnCopy.UseVisualStyleBackColor = true;
			this.btnCopy.Click += new global::System.EventHandler(this.btnCopy_Click);
			// 
			// btnGenerate
			// 
			this.btnGenerate.Location = new global::System.Drawing.Point(164, 2);
			this.btnGenerate.Margin = new global::System.Windows.Forms.Padding(2);
			this.btnGenerate.Name = "btnGenerate";
			this.btnGenerate.Size = new global::System.Drawing.Size(80, 28);
			this.btnGenerate.TabIndex = 1;
			this.btnGenerate.Text = "&Generate";
			this.btnGenerate.UseVisualStyleBackColor = true;
			this.btnGenerate.Click += new global::System.EventHandler(this.btnGenerate_Click);
			// 
			// chkGroupFiles
			// 
			this.chkGroupFiles.BackColor = global::System.Drawing.Color.Transparent;
			this.chkGroupFiles.Location = new global::System.Drawing.Point(79, 3);
			this.chkGroupFiles.Name = "chkGroupFiles";
			this.chkGroupFiles.Size = new global::System.Drawing.Size(80, 28);
			this.chkGroupFiles.TabIndex = 0;
			this.chkGroupFiles.Text = "&Group Files";
			this.chkGroupFiles.UseVisualStyleBackColor = false;
			this.chkGroupFiles.CheckedChanged += new global::System.EventHandler(this.chkGroupFiles_CheckedChanged);
			// 
			// worker
			// 
			this.worker.WorkerSupportsCancellation = true;
			this.worker.DoWork += new global::System.ComponentModel.DoWorkEventHandler(this.worker_DoWork);
			this.worker.RunWorkerCompleted += new global::System.ComponentModel.RunWorkerCompletedEventHandler(this.worker_RunWorkerCompleted);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new global::System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new global::System.Drawing.Size(414, 391);
			this.Controls.Add(this.tlp);
			this.Icon = ((global::System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MinimumSize = new global::System.Drawing.Size(350, 350);
			this.Name = "MainForm";
			this.Text = "Generate WIX toolset Components";
			this.tlp.ResumeLayout(false);
			this.splt.Panel1.ResumeLayout(false);
			this.splt.Panel1.PerformLayout();
			this.splt.Panel2.ResumeLayout(false);
			((global::System.ComponentModel.ISupportInitialize)(this.splt)).EndInit();
			this.splt.ResumeLayout(false);
			this.flp.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private TableLayoutPanel tlp;
		private SplitContainer splt;
		private TextBox txtInput;
		private FlowLayoutPanel flp;
		private Button btnExit;
		private Button btnCopy;
		private Button btnGenerate;
		private BackgroundWorker worker;
		private TreeView tvwOutput;
		private CheckBox chkGroupFiles;
	}
}


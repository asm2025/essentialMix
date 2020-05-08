using System.ComponentModel;
using System.Windows.Forms;

namespace MissingWXSFiles
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
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.tlp = new System.Windows.Forms.TableLayoutPanel();
			this.txtFile = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.txtBin = new System.Windows.Forms.TextBox();
			this.txtOut = new System.Windows.Forms.TextBox();
			this.btnBin = new System.Windows.Forms.Button();
			this.btnFile = new System.Windows.Forms.Button();
			this.fbd = new System.Windows.Forms.FolderBrowserDialog();
			this.ofd = new System.Windows.Forms.OpenFileDialog();
			this.flpButtons = new System.Windows.Forms.FlowLayoutPanel();
			this.btnProcess = new System.Windows.Forms.Button();
			this.tlp.SuspendLayout();
			this.flpButtons.SuspendLayout();
			this.SuspendLayout();
			// 
			// tlp
			// 
			this.tlp.ColumnCount = 4;
			this.tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 32F));
			this.tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 32F));
			this.tlp.Controls.Add(this.btnFile, 3, 2);
			this.tlp.Controls.Add(this.txtFile, 0, 2);
			this.tlp.Controls.Add(this.label1, 0, 0);
			this.tlp.Controls.Add(this.txtBin, 0, 1);
			this.tlp.Controls.Add(this.txtOut, 0, 3);
			this.tlp.Controls.Add(this.btnBin, 3, 1);
			this.tlp.Controls.Add(this.flpButtons, 0, 4);
			this.tlp.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tlp.Location = new System.Drawing.Point(0, 0);
			this.tlp.Margin = new System.Windows.Forms.Padding(0);
			this.tlp.Name = "tlp";
			this.tlp.RowCount = 5;
			this.tlp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
			this.tlp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
			this.tlp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
			this.tlp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tlp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
			this.tlp.Size = new System.Drawing.Size(556, 313);
			this.tlp.TabIndex = 0;
			// 
			// txtFile
			// 
			this.tlp.SetColumnSpan(this.txtFile, 3);
			this.txtFile.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtFile.Location = new System.Drawing.Point(0, 55);
			this.txtFile.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
			this.txtFile.Name = "txtFile";
			this.txtFile.Size = new System.Drawing.Size(524, 20);
			this.txtFile.TabIndex = 3;
			this.txtFile.WordWrap = false;
			this.txtFile.TextChanged += new System.EventHandler(this.txtFile_TextChanged);
			// 
			// label1
			// 
			this.tlp.SetColumnSpan(this.label1, 4);
			this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label1.Location = new System.Drawing.Point(3, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(550, 26);
			this.label1.TabIndex = 0;
			this.label1.Text = "Find missing WSX entries of files in bin directory not in WiX products file...";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// txtBin
			// 
			this.tlp.SetColumnSpan(this.txtBin, 3);
			this.txtBin.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtBin.Location = new System.Drawing.Point(0, 29);
			this.txtBin.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
			this.txtBin.Name = "txtBin";
			this.txtBin.Size = new System.Drawing.Size(524, 20);
			this.txtBin.TabIndex = 1;
			this.txtBin.WordWrap = false;
			this.txtBin.TextChanged += new System.EventHandler(this.txtBin_TextChanged);
			// 
			// txtOut
			// 
			this.tlp.SetColumnSpan(this.txtOut, 4);
			this.txtOut.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtOut.Location = new System.Drawing.Point(3, 81);
			this.txtOut.MaxLength = int.MaxValue;
			this.txtOut.Multiline = true;
			this.txtOut.Name = "txtOut";
			this.txtOut.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtOut.Size = new System.Drawing.Size(550, 197);
			this.txtOut.TabIndex = 5;
			this.txtOut.WordWrap = false;
			// 
			// btnBin
			// 
			this.btnBin.Dock = System.Windows.Forms.DockStyle.Fill;
			this.btnBin.Location = new System.Drawing.Point(524, 26);
			this.btnBin.Margin = new System.Windows.Forms.Padding(0);
			this.btnBin.Name = "btnBin";
			this.btnBin.Size = new System.Drawing.Size(32, 26);
			this.btnBin.TabIndex = 2;
			this.btnBin.Text = "...";
			this.btnBin.UseVisualStyleBackColor = true;
			this.btnBin.Click += new System.EventHandler(this.btnBin_Click);
			// 
			// btnFile
			// 
			this.btnFile.Dock = System.Windows.Forms.DockStyle.Fill;
			this.btnFile.Location = new System.Drawing.Point(524, 52);
			this.btnFile.Margin = new System.Windows.Forms.Padding(0);
			this.btnFile.Name = "btnFile";
			this.btnFile.Size = new System.Drawing.Size(32, 26);
			this.btnFile.TabIndex = 4;
			this.btnFile.Text = "...";
			this.btnFile.UseVisualStyleBackColor = true;
			this.btnFile.Click += new System.EventHandler(this.btnFile_Click);
			// 
			// ofd
			// 
			this.ofd.Filter = "WiX File (*.wxs)|*.wxs|All Files (*.*)|*.*";
			// 
			// flpButtons
			// 
			this.tlp.SetColumnSpan(this.flpButtons, 4);
			this.flpButtons.Controls.Add(this.btnProcess);
			this.flpButtons.Dock = System.Windows.Forms.DockStyle.Fill;
			this.flpButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
			this.flpButtons.Location = new System.Drawing.Point(0, 281);
			this.flpButtons.Margin = new System.Windows.Forms.Padding(0);
			this.flpButtons.Name = "flpButtons";
			this.flpButtons.Size = new System.Drawing.Size(556, 32);
			this.flpButtons.TabIndex = 6;
			this.flpButtons.WrapContents = false;
			// 
			// btnProcess
			// 
			this.btnProcess.Enabled = false;
			this.btnProcess.Location = new System.Drawing.Point(478, 3);
			this.btnProcess.Name = "btnProcess";
			this.btnProcess.Size = new System.Drawing.Size(75, 26);
			this.btnProcess.TabIndex = 0;
			this.btnProcess.Text = "Start";
			this.btnProcess.UseVisualStyleBackColor = true;
			this.btnProcess.Click += new System.EventHandler(this.btnProcess_Click);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(556, 313);
			this.Controls.Add(this.tlp);
			this.Name = "MainForm";
			this.Text = "Find Missing WSX Entries...";
			this.tlp.ResumeLayout(false);
			this.tlp.PerformLayout();
			this.flpButtons.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private TableLayoutPanel tlp;
		private Label label1;
		private TextBox txtBin;
		private TextBox txtFile;
		private TextBox txtOut;
		private Button btnBin;
		private Button btnFile;
		private FolderBrowserDialog fbd;
		private OpenFileDialog ofd;
		private FlowLayoutPanel flpButtons;
		private Button btnProcess;
	}
}


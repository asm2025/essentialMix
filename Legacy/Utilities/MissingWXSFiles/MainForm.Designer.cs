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
			if (disposing && (components != null)) components.Dispose();
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
			this.tlp = new System.Windows.Forms.TableLayoutPanel();
			this.btnFile = new System.Windows.Forms.Button();
			this.txtFile = new System.Windows.Forms.TextBox();
			this.lblTitle = new System.Windows.Forms.Label();
			this.txtBin = new System.Windows.Forms.TextBox();
			this.txtOut = new System.Windows.Forms.TextBox();
			this.btnBin = new System.Windows.Forms.Button();
			this.flpButtons = new System.Windows.Forms.FlowLayoutPanel();
			this.btnProcess = new System.Windows.Forms.Button();
			this.fbd = new System.Windows.Forms.FolderBrowserDialog();
			this.ofd = new System.Windows.Forms.OpenFileDialog();
			this.tip = new System.Windows.Forms.ToolTip(this.components);
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
			this.tlp.Controls.Add(this.lblTitle, 0, 0);
			this.tlp.Controls.Add(this.txtBin, 0, 1);
			this.tlp.Controls.Add(this.txtOut, 0, 3);
			this.tlp.Controls.Add(this.btnBin, 3, 1);
			this.tlp.Controls.Add(this.flpButtons, 0, 4);
			this.tlp.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tlp.Location = new System.Drawing.Point(0, 0);
			this.tlp.Margin = new System.Windows.Forms.Padding(0);
			this.tlp.Name = "tlp";
			this.tlp.RowCount = 5;
			this.tlp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
			this.tlp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
			this.tlp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
			this.tlp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tlp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
			this.tlp.Size = new System.Drawing.Size(556, 313);
			this.tlp.TabIndex = 0;
			// 
			// btnFile
			// 
			this.btnFile.Dock = System.Windows.Forms.DockStyle.Fill;
			this.btnFile.Location = new System.Drawing.Point(524, 67);
			this.btnFile.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
			this.btnFile.Name = "btnFile";
			this.btnFile.Size = new System.Drawing.Size(32, 26);
			this.btnFile.TabIndex = 4;
			this.btnFile.Text = "...";
			this.tip.SetToolTip(this.btnFile, "Select the path of the file that contains the WiX components. Most likely will be" +
        " \'Products.wxs\'");
			this.btnFile.UseVisualStyleBackColor = true;
			this.btnFile.Click += new System.EventHandler(this.btnFile_Click);
			// 
			// txtFile
			// 
			this.txtFile.BackColor = System.Drawing.SystemColors.Window;
			this.tlp.SetColumnSpan(this.txtFile, 3);
			this.txtFile.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtFile.Location = new System.Drawing.Point(0, 68);
			this.txtFile.Margin = new System.Windows.Forms.Padding(0, 4, 0, 4);
			this.txtFile.Name = "txtFile";
			this.txtFile.Size = new System.Drawing.Size(524, 24);
			this.txtFile.TabIndex = 3;
			this.tip.SetToolTip(this.txtFile, "Path of the file that contains the WiX components. Most likely will be \'Products." +
        "wxs\'");
			this.txtFile.WordWrap = false;
			this.txtFile.TextChanged += new System.EventHandler(this.txtFile_TextChanged);
			// 
			// lblTitle
			// 
			this.tlp.SetColumnSpan(this.lblTitle, 4);
			this.lblTitle.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblTitle.Location = new System.Drawing.Point(3, 0);
			this.lblTitle.Name = "lblTitle";
			this.lblTitle.Size = new System.Drawing.Size(550, 32);
			this.lblTitle.TabIndex = 0;
			this.lblTitle.Text = "Find missing WSX entries of files in bin directory not in WiX products file...";
			this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// txtBin
			// 
			this.txtBin.BackColor = System.Drawing.SystemColors.Window;
			this.tlp.SetColumnSpan(this.txtBin, 3);
			this.txtBin.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtBin.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtBin.Location = new System.Drawing.Point(0, 36);
			this.txtBin.Margin = new System.Windows.Forms.Padding(0, 4, 0, 4);
			this.txtBin.Name = "txtBin";
			this.txtBin.Size = new System.Drawing.Size(524, 24);
			this.txtBin.TabIndex = 1;
			this.tip.SetToolTip(this.txtBin, "Binary folder path which contains the target files");
			this.txtBin.WordWrap = false;
			this.txtBin.TextChanged += new System.EventHandler(this.txtBin_TextChanged);
			// 
			// txtOut
			// 
			this.tlp.SetColumnSpan(this.txtOut, 4);
			this.txtOut.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtOut.Location = new System.Drawing.Point(0, 98);
			this.txtOut.Margin = new System.Windows.Forms.Padding(0, 2, 0, 2);
			this.txtOut.MaxLength = 2147483647;
			this.txtOut.Multiline = true;
			this.txtOut.Name = "txtOut";
			this.txtOut.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtOut.Size = new System.Drawing.Size(556, 181);
			this.txtOut.TabIndex = 5;
			this.tip.SetToolTip(this.txtOut, "Results that will contain newly generated components that was missing from the WX" +
        "S file");
			this.txtOut.WordWrap = false;
			// 
			// btnBin
			// 
			this.btnBin.Dock = System.Windows.Forms.DockStyle.Fill;
			this.btnBin.Location = new System.Drawing.Point(524, 35);
			this.btnBin.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
			this.btnBin.Name = "btnBin";
			this.btnBin.Size = new System.Drawing.Size(32, 26);
			this.btnBin.TabIndex = 2;
			this.btnBin.Text = "...";
			this.tip.SetToolTip(this.btnBin, "Select the binary folder path which contains the target files");
			this.btnBin.UseVisualStyleBackColor = true;
			this.btnBin.Click += new System.EventHandler(this.btnBin_Click);
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
			this.tip.SetToolTip(this.btnProcess, "Starts generating the new components. This will be enabled when you have selected" +
        " both binary folder and WXS file paths");
			this.btnProcess.UseVisualStyleBackColor = true;
			this.btnProcess.Click += new System.EventHandler(this.btnProcess_Click);
			// 
			// fbd
			// 
			this.fbd.Description = "Select the path to the binary folder...";
			// 
			// ofd
			// 
			this.ofd.Filter = "WiX File (*.wxs)|*.wxs|Test File (*.txt)|*.txt|All Files (*.*)|*.*";
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(556, 313);
			this.Controls.Add(this.tlp);
			this.Name = "MainForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.WindowsDefaultBounds;
			this.Text = "Find Missing WSX Entries...";
			this.tlp.ResumeLayout(false);
			this.tlp.PerformLayout();
			this.flpButtons.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private TableLayoutPanel tlp;
		private Label lblTitle;
		private TextBox txtBin;
		private TextBox txtFile;
		private TextBox txtOut;
		private Button btnBin;
		private Button btnFile;
		private FolderBrowserDialog fbd;
		private OpenFileDialog ofd;
		private FlowLayoutPanel flpButtons;
		private Button btnProcess;
		private ToolTip tip;
	}
}


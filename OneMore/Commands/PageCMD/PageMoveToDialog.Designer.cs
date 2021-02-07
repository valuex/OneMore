namespace River.OneMoreAddIn.Commands
{
	partial class PageMoveToDialog
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose (bool disposing)
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
		private void InitializeComponent ()
		{
            this.tBoxKW = new System.Windows.Forms.TextBox();
            this.treeView = new System.Windows.Forms.TreeView();
            this.SuspendLayout();
            // 
            // tBoxKW
            // 
            this.tBoxKW.HideSelection = false;
            this.tBoxKW.Location = new System.Drawing.Point(23, 16);
            this.tBoxKW.Margin = new System.Windows.Forms.Padding(4);
            this.tBoxKW.Name = "tBoxKW";
            this.tBoxKW.Size = new System.Drawing.Size(904, 35);
            this.tBoxKW.TabIndex = 12;
            this.tBoxKW.TextChanged += new System.EventHandler(this.tBoxKW_TextChanged);
            this.tBoxKW.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tBoxKW_KeyDown);
            // 
            // treeView
            // 
            this.treeView.Location = new System.Drawing.Point(23, 92);
            this.treeView.Margin = new System.Windows.Forms.Padding(4);
            this.treeView.Name = "treeView";
            this.treeView.Size = new System.Drawing.Size(904, 861);
            this.treeView.TabIndex = 13;
            this.treeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView_AfterSelect);
            this.treeView.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeView_NodeDbClick);
            // 
            // PageMoveToDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(945, 971);
            this.Controls.Add(this.treeView);
            this.Controls.Add(this.tBoxKW);
            this.Font = new System.Drawing.Font("SimSun", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PageMoveToDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Select";
            this.Deactivate += new System.EventHandler(this.PageMoveToDialog_Deactivate);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.PageMoveToDialog_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
        private System.Windows.Forms.TextBox tBoxKW;
        private System.Windows.Forms.TreeView treeView;
    }
}
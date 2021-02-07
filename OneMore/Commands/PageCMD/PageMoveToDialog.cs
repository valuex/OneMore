//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;
	using System.Collections.Generic;
	using System.Linq;
	using System.Xml.Linq;
	using System.Text;
	using System.Xml;
	using NPinyin;
	using System.Drawing;



	internal partial class PageMoveToDialog : UI.LocalizableForm
	{
		//private static extern bool SetForegroundWindow(IntPtr hWnd);
		private readonly OneNote one;

		public string FilterednotebookXml;

		public string PageAction;
		public string SelectedObjID { get; set; }
		public PageMoveToDialog(string PageManu)
		{
			one = new OneNote();
			PageAction = PageManu;
			InitializeComponent();
			XmlDocument dom = new XmlDocument();
			XElement hierarchy = one.GetNotebooks(OneNote.Scope.Pages);
			string notebookXml = hierarchy.ToString();
			dom.LoadXml(notebookXml);
			treeView.Nodes.Clear();
			treeView.Nodes.Add(new TreeNode(dom.DocumentElement.LocalName + "(" + dom.DocumentElement.ChildNodes.Count + ")"));
			AddNode(dom.DocumentElement, treeView.Nodes[0]);
		}

		private void PageMoveToDialog_Shown(object sender, EventArgs e)
		{
			this.WindowState = FormWindowState.Minimized;
			this.WindowState = FormWindowState.Normal;
			this.tBoxKW.Focus();
			SwitchIMETo("en-US");

		}
		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (keyData == Keys.Escape)
			{
				this.Close();
				return true;
			}
			return base.ProcessCmdKey(ref msg, keyData);
		}

		private void tBoxKW_KeyDown(object sender, KeyEventArgs e)
		{
			// click the last node when the textbox receiving ENTER
			if (e.KeyCode == Keys.Enter)
			{
				treeView_NodeDbClick(sender,null);
			}
		}





		private void tBoxKW_TextChanged(object sender, EventArgs e)
        {

			XElement hierarchy = one.GetNotebooks(OneNote.Scope.Pages);

			string notebookXml = hierarchy.ToString();
			try
			{
				string strKeyWords = tBoxKW.Text.ToLower();
                string v = FilterXml(notebookXml, strKeyWords);
                FilterednotebookXml = v;

				XmlDocument dom = new XmlDocument();
				dom.LoadXml(FilterednotebookXml);
				treeView.Nodes.Clear();
				treeView.Nodes.Add(new TreeNode(dom.DocumentElement.LocalName + "(" + dom.DocumentElement.ChildNodes.Count + ")"));
				AddNode(dom.DocumentElement, treeView.Nodes[0]);
				treeView.TopNode = treeView.Nodes[0];
			}
			catch
			{
				MessageBox.Show("Failed to filter!");
			}			
		}

		public string FilterXml(string notebookXml, string strKeyWords)
		{
			string strSecKw, strPageKw;
			XElement root = XElement.Parse(notebookXml);
			if (strKeyWords.Contains(" "))
			{
				strSecKw = strKeyWords.Substring(0, strKeyWords.IndexOf(" "));  //sub-string before SPACE
				strPageKw = strKeyWords.Substring(strKeyWords.IndexOf(" ") + 1); //sub-string after SPACE
			}
			else
			{
				strSecKw = strKeyWords;
				strPageKw = null;
			}

			string RecycleBinSec = "onenote recyclebin";

			foreach (var NBSec in root.Elements().Elements())
			{
				string NBSecName = PinYinFirstLetter(NBSec.Attribute("name").Value).ToLower();
				if (!NBSecName.Contains(strSecKw) || NBSecName.Contains(RecycleBinSec))
					NBSec.RemoveAll();

			}
			if (strPageKw != null)
			{
				foreach (var NBPage in root.Elements().Elements().Elements())
				{
					string NBPageName = PinYinFirstLetter(NBPage.Attribute("name").Value).ToLower();
					if (!NBPageName.Contains(strPageKw))
						NBPage.RemoveAll();
				}
			}

			// delete empty page
			foreach (XElement child in root.Descendants().Reverse())
			{
				if (!child.HasElements && string.IsNullOrEmpty(child.Value) && !child.HasAttributes) child.Remove();

			}

			// delete empty section
			foreach (XElement child in root.Elements().Elements().Reverse())
			{
				if (!child.HasElements) child.Remove();

			}

			//delete empty notebook
			foreach (var child1 in root.Elements().Reverse())
			{
				if (!child1.HasElements) child1.Remove();
			}

			return root.ToString();

		}

		private string PinYinFirstLetter(string strInput)
		{
			//System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
			Encoding gb2312 = Encoding.GetEncoding("GB2312");
			string s = Pinyin.ConvertEncoding(strInput, Encoding.UTF8, gb2312);
			return Pinyin.GetInitials(s, gb2312);
		}
		private void AddNode(XmlNode inXmlNode, TreeNode inTreeNode)
		{
			// add child xml nodes of inXmlNode to inTreeNode
			//ToolTipText
			if (inXmlNode.HasChildNodes)
			{
				XmlNodeList nodeList = inXmlNode.ChildNodes;
				foreach (XmlElement xNode in nodeList)
				{
					string str = xNode.GetAttribute("name");
					string strID = xNode.GetAttribute("ID");
					if (string.IsNullOrEmpty(str))
						str = xNode.LocalName;
					if (str.Contains("OneNote_RecycleBin"))
						continue;
					if (xNode.ChildNodes.Count != 0)
						str = str + "(" + xNode.ChildNodes.Count + ")";
					inTreeNode.Nodes.Add(new TreeNode(str));
					inTreeNode.Name = strID;
					inTreeNode.ToolTipText = strID;
					AddNode(xNode, inTreeNode.LastNode);
					if (nodeList.Count <= 5 || inTreeNode.Level <= 1 || xNode.ParentNode.ParentNode.ChildNodes.Count < 3)
						inTreeNode.Expand();
					switch (inTreeNode.Level)
					{
						case 1:
							inTreeNode.ForeColor = Color.Orange;
							break;
						case 2:
							inTreeNode.ForeColor = Color.Blue;
							break;
					}

				}
			}
		}

		private void SwitchIMETo(string cultureType)
		{
			var installedInputLanguages = InputLanguage.InstalledInputLanguages;
			if (installedInputLanguages.Cast<InputLanguage>().Any(i => i.Culture.Name == cultureType))
			{
				InputLanguage.CurrentInputLanguage = InputLanguage.FromCulture(System.Globalization.CultureInfo.GetCultureInfo(cultureType));
			}
		}


		private void treeView_NodeDbClick(object sender, TreeNodeMouseClickEventArgs e)
		{
			TreeNode ThisSelectedTreeNode;
			if (e == null)
			 ThisSelectedTreeNode = getLastNode(treeView.Nodes[treeView.Nodes.Count - 1]);
			else
			 ThisSelectedTreeNode = e.Node;			

			string CurPgID = one.CurrentPageId;
			string[] SelectedID = GetSeletedNodeID(ThisSelectedTreeNode);
			string DestID = SelectedID.Last();
			SelectedObjID = DestID;
			this.Close();			

		}
		private TreeNode getLastNode(TreeNode subroot)
		{
			if (subroot.Nodes.Count == 0)
				return subroot;

			return getLastNode(subroot.Nodes[subroot.Nodes.Count - 1]);
		}

		private async void JumpTo(string DestID)
		{
			try
			{
				using (var one = new OneNote())
				{
					await one.NavigateTo(DestID);
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error navigating to {DestID}", exc);
			}
		}


		public void MovePage(string CurPgID, string DestID)
		{
			var sections = new Dictionary<string, XElement>();
			//string sectionId = GetSeletedNodeID(ThisSelectedTreeNode).Last();

			var Destination = one.GetObj(DestID);
			if (Destination == null) return;
			var ns = one.GetNamespace(Destination);
			string SelectedNodeType = Destination.Name.LocalName;

			string parentId = one.GetParent(CurPgID);
			XElement CurSection = one.GetSection(parentId);
			XElement DestSection;
			//*-- remove from current section--*//
			// get the Page reference within the current section
			var element = CurSection.Elements(ns + "Page")
				.FirstOrDefault(e => e.Attribute("ID").Value == CurPgID);

			element.Remove(); // remove page from current section

			// remove misc attributes; OneNote will recreate them
			element.Attributes()
				.Where(a => a.Name != "ID" && a.Name != "name")
				.Remove();

			//*-- add to destination section--*//
			// append page to the selected section
			if (SelectedNodeType == "Section")
            {
				DestSection = one.GetSection(DestID);
				DestSection.Add(element);  // append to the end of destination section
				
			}
			// insert page behind the selected page
			else
			{
				// find the section that currently owns the page
				parentId = one.GetParent(DestID);
				DestSection = one.GetSection(parentId);
				//  insert after the selected page
				var NewPageNode = DestSection.Descendants(ns + "Page").Where(n => n.Attribute("ID").Value == DestID).FirstOrDefault();
				NewPageNode.AddAfterSelf(element);
			}

			one.UpdateHierarchy(CurSection);
			one.UpdateHierarchy(DestSection);
			
		}

		private string[] GetSeletedNodeID(TreeNode SelectedNode)
		{
			//string FilterednotebookXml=null;
			int NodeIndex = SelectedNode.Index;
			int NodeLevel = SelectedNode.Level;

			XElement hierarchy = one.GetNotebooks(OneNote.Scope.Pages);
			string notebookXml = hierarchy.ToString();
			int[] IndexArr;
			string[] IndexNodeIDArr;
			IndexArr = new int[NodeLevel + 1];
			IndexNodeIDArr = new string[NodeLevel + 1];
			for (int i = NodeLevel - 1; i >= 0; i--)
			{
				IndexArr[i] = SelectedNode.Parent.Index;
				SelectedNode = SelectedNode.Parent;
			}
			IndexArr[NodeLevel] = NodeIndex;


			if (FilterednotebookXml == null) FilterednotebookXml = notebookXml;
			XElement root = XElement.Parse(FilterednotebookXml);
			XElement ThisNode;

			IndexNodeIDArr[0] = null;
			for (int i = 1; i <= NodeLevel; i++)
			{
				NodeIndex = IndexArr[i];
				ThisNode = root.Elements().Skip(NodeIndex).FirstOrDefault();
				IndexNodeIDArr[i] = ThisNode.Attribute("ID").Value;
				root = ThisNode;
			}
			return IndexNodeIDArr;			
		}

        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {

        }

        private void PageMoveToDialog_FormClosing(object sender, EventArgs e)
        {
			this.Close();
		}

        private void PageMoveToDialog_Deactivate(object sender, EventArgs e)
        {
			this.Close();

		}

        private void PageMoveToDialog_KeyDown(object sender, KeyEventArgs e)
        {
			if (e.KeyCode == Keys.Escape)
			{
				this.Close();
			}
		}
    }
}

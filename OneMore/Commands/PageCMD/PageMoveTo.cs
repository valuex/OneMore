//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

using System.Threading.Tasks;
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
using System.Globalization;

namespace River.OneMoreAddIn.Commands
{

	internal class PageExecuteCommand : Command
	{
		private string PageAction;
		private  OneNote one;
		private const int OutlineMargin = 30;

		public PageExecuteCommand()
		{
		}

		//public string  PageAction { get; set; }
		public override async Task Execute(params object[] args)
		{
			PageAction = (string)args[0];
			one = new OneNote();
			string DestID = "";
			string CurPgID = one.CurrentPageId;
			if (PageAction == "del")
			{
				one.DeleteHierarchy(CurPgID);
			}
			else if (PageAction == "LevelLoop")
            {
				PageLevelLoop();
			}
			else if (PageAction == "NewPageAtBottom")
			{
				one.CreatePage(one.CurrentSectionId, out var pageId);	
			}
			else if (PageAction == "NewPageBelow")
			{
				AddNewPageBelow(CurPgID);
			}
			else if (PageAction == "MovePageUp")
			{
				MoveCurPageUp(CurPgID);
			}
			else if (PageAction == "MovePageDown")
			{
				MoveCurPageDown(CurPgID);
			}
			else
			{
				using (var dialog = new PageMoveToDialog(PageAction))
				{
					dialog.ShowDialog(owner);
					DestID = dialog.SelectedObjID;


					switch (PageAction)
					{
						case "move":
							dialog.MovePage(CurPgID, DestID);
							break;
						case "goto":
							one.NavigateToID(DestID);
							break;
						case "merge":
							MergePages(CurPgID, DestID);
							break;
					}
				}
			}
			await Task.Yield();
		}

		public  async void MergePages(string CurPgID, string DestPgID)
		{
			using (var one = new OneNote())
			{
				var CurSection = one.GetSection();
				var CurPage = one.GetPage();
				var DestPage = one.GetPage(DestPgID);

				var ns = CurPage.Namespace;
				//ns = one.GetNamespace(section);

				// find first selected - active page
				XElement CurPageElement = CurSection.Elements(ns + "Page")
					.FirstOrDefault(e => e.Attribute("ID").Value == CurPgID);


				var quickmap = DestPage.GetQuickStyleMap();

				var offset = GetPageBottomOffset(ns, DestPage);

				// track running bottom as we add new outlines
				var maxOffset = offset;

				// find maximum z-offset
				var z = DestPage.Root.Elements(ns + "Outline").Elements(ns + "Position")
					.Attributes("z").Max(a => int.Parse(a.Value)) + 1;

				// merge current pages into the destnation page


				var map = DestPage.MergeQuickStyles(CurPage);

				var childOutlines = CurPage.Root.Elements(ns + "Outline");
				if (childOutlines == null || !childOutlines.Any())
				{
					return;
				}

				var topOffset = childOutlines.Elements(ns + "Position")
					.Min(p => double.Parse(p.Attribute("y").Value, CultureInfo.InvariantCulture));

				foreach (var childOutline in childOutlines)
				{
					// adjust position relative to new parent page outlines
					var position = childOutline.Elements(ns + "Position").FirstOrDefault();
					var y = double.Parse(position.Attribute("y").Value, CultureInfo.InvariantCulture)
						- topOffset + offset + OutlineMargin;

					position.Attribute("y").Value = y.ToString("#0.0", CultureInfo.InvariantCulture);

					// keep track of lowest bottom
					var size = childOutline.Elements(ns + "Size").FirstOrDefault();
					var bottom = y + double.Parse(size.Attribute("height").Value, CultureInfo.InvariantCulture);
					if (bottom > maxOffset)
					{
						maxOffset = bottom;
					}

					position.Attribute("z").Value = z.ToString();
					z++;

					// remove its IDs so the page can apply its own
					childOutline.Attributes("objectID").Remove();
					childOutline.Descendants().Attributes("objectID").Remove();

					DestPage.ApplyStyleMapping(map, childOutline);

					DestPage.Root.Add(childOutline);
				}

				if (maxOffset > offset)
				{
					offset = maxOffset;
				}

				// update page and section hierarchy
				one.DeleteHierarchy(CurPgID);
				await one.Update(DestPage);
			}
		}

		private double GetPageBottomOffset(XNamespace ns, Models.Page page)
		{
			// find bottom of current page; bottom of lowest Outline
			double offset = 0.0;
			foreach (var outline in page.Root.Elements(ns + "Outline"))
			{
				var position = outline.Elements(ns + "Position").FirstOrDefault();
				if (position != null)
				{
					var size = outline.Elements(ns + "Size").FirstOrDefault();
					if (size != null)
					{
						var bottom = double.Parse(position.Attribute("y").Value, CultureInfo.InvariantCulture)
							+ double.Parse(size.Attribute("height").Value, CultureInfo.InvariantCulture);

						if (bottom > offset)
						{
							offset = bottom;
						}
					}
				}
			}

			return offset;
		}
		private  void SmartMergePages(string CurPgID, string DestPgID)
        {
			var CurPage = one.GetPage();
			var ns = CurPage.Namespace;
			string CurPgXml = one.GetPageXml(CurPgID);
			string DestPgXml = one.GetPageXml(DestPgID);
			XDocument CurPgEles = XDocument.Parse(CurPgXml);
			XDocument DestPgEles = XDocument.Parse(DestPgXml);

			int OutlineNumInCurPg = CurPgEles.Descendants(ns + "Outline").Count();
			int OutlineNumInDestPg = DestPgEles.Descendants(ns + "Outline").Count();
			if (OutlineNumInCurPg == 1 && OutlineNumInDestPg == 1)
				MergePagesWithOneOutline(CurPgID, DestPgID);
			else
				MergePages(CurPgID, DestPgID);
		}

		private async void MergePagesWithOneOutline(string CurPgID, string DestPgID)
		{
			// current page, with only one outline
			// destnation page, with only one outline
			string CurPgXml = one.GetPageXml(CurPgID);
			XDocument CurPgEles = XDocument.Parse(CurPgXml);
			var ns = CurPgEles.Root.Name.Namespace;
			if (DestPgID == null) return;  // when not seltec any page
			string DestPgXml = one.GetPageXml(DestPgID);
			var DestPgXmlRoot = XElement.Parse(DestPgXml);

			IEnumerable<XElement> lstCurPgElements = CurPgEles.Descendants(ns + "OE");

			foreach (var xmlElement in lstCurPgElements)
			{
				DestPgXmlRoot	
					.Descendants(ns + "OE")
					.LastOrDefault()
					.AddAfterSelf(xmlElement);
			}
			var DestPage = one.GetPage(DestPgID);
			await one.Update(DestPgXmlRoot);

			//one.UpdatePageContent(DestPgXmlRoot.ToString(), DateTime.MinValue);
			one.DeleteHierarchy(CurPgID);
		}

		private async void PageLevelLoop()
		{
			// make page level+1, if at bottom level, reverse to level1
			var CurPage = one.GetPage();
			var ns = CurPage.Namespace;

			int CurPgLevel = int.Parse(CurPage.Root.Attribute("pageLevel").Value);
			if (CurPgLevel < 3)
				CurPage.Root.Attribute("pageLevel").Value = (CurPgLevel + 1).ToString();
			else
				CurPage.Root.Attribute("pageLevel").Value = "1";
		
			await one.Update(CurPage);

		}

		public void AddNewPageBelow(string CurPgID)
		{
			one.CreatePage(one.CurrentSectionId, out var NewPageId);

			var CurPage = one.GetPage();
			var ns = CurPage.Namespace;
			XElement CurSection = one.GetSection();
			//*-- remove from current section--*//
			// get the Page reference within the current section
			var element = CurSection.Elements(ns + "Page")
				.FirstOrDefault(e => e.Attribute("ID").Value == NewPageId);
			int position = element.ElementsAfterSelf().Count();
			if (position == 0) return;          // if current page is the bottom page, no need to move down
			element.Remove(); // remove page from current section

			// remove misc attributes; OneNote will recreate them
			element.Attributes()
				.Where(a => a.Name != "ID" && a.Name != "name" && a.Name != "pageLevel")
				.Remove();
			//  insert after the selected page
			XElement CurPageNode = CurSection.Descendants(ns + "Page").Where(n => n.Attribute("ID").Value == CurPgID).FirstOrDefault();
			string CurPgLevel = CurPageNode.Attribute("pageLevel").Value;
			element.Attribute("pageLevel").Value= CurPgLevel;
			CurPageNode.AddAfterSelf(element);

			one.UpdateHierarchy(CurSection);
		}

		public void MoveCurPageUp(string CurPgID)
		{
		
			var CurPage = one.GetPage();
			var ns = CurPage.Namespace;
			XElement CurSection = one.GetSection();

			//*-- remove from current section--*//
			// get the Page reference within the current section
			var element = CurSection.Elements(ns + "Page")
				.FirstOrDefault(e => e.Attribute("ID").Value == CurPgID);
			int position = element.ElementsBeforeSelf().Count();
			if (position == 0)  return;			// if current page is the top page, no need to move up

			XElement PreElement = element.ElementsBeforeSelf().Last();
			element.Remove();
			string CurPgLevel = PreElement.Attribute("pageLevel").Value;
			element.Attribute("pageLevel").Value = CurPgLevel;
			PreElement.AddBeforeSelf(element);
			one.UpdateHierarchy(CurSection);
		}

		public void MoveCurPageDown(string CurPgID)
		{

			var CurPage = one.GetPage();
			var ns = CurPage.Namespace;
			XElement CurSection = one.GetSection();

			//*-- remove from current section--*//
			// get the Page reference within the current section
			var element = CurSection.Elements(ns + "Page")
				.FirstOrDefault(e => e.Attribute("ID").Value == CurPgID);
			int position = element.ElementsAfterSelf().Count();
			if (position == 0) return;          // if current page is the bottom page, no need to move down

			XElement NextElement = element.ElementsAfterSelf().FirstOrDefault();
			element.Remove();
			string CurPgLevel = NextElement.Attribute("pageLevel").Value;
			element.Attribute("pageLevel").Value = CurPgLevel;
			NextElement.AddAfterSelf(element);
			one.UpdateHierarchy(CurSection);
		}

		public void MoveCurPageToTop(string CurPgID)
		{

			var CurPage = one.GetPage();
			var ns = CurPage.Namespace;
			XElement CurSection = one.GetSection();

			//*-- remove from current section--*//
			// get the Page reference within the current section
			var element = CurSection.Elements(ns + "Page")
				.FirstOrDefault(e => e.Attribute("ID").Value == CurPgID);
			int position = element.ElementsBeforeSelf().Count();
			if (position == 0) return;          // if current page is the top page, no need to move up

			XElement PreElement = element.ElementsBeforeSelf().FirstOrDefault();
			element.Remove();
			string CurPgLevel = PreElement.Attribute("pageLevel").Value;
			element.Attribute("pageLevel").Value = CurPgLevel;
			PreElement.AddBeforeSelf(element);
			one.UpdateHierarchy(CurSection);
		}
		public void MoveCurPageToBottom(string CurPgID)
		{

			var CurPage = one.GetPage();
			var ns = CurPage.Namespace;
			XElement CurSection = one.GetSection();

			//*-- remove from current section--*//
			// get the Page reference within the current section
			var element = CurSection.Elements(ns + "Page")
				.FirstOrDefault(e => e.Attribute("ID").Value == CurPgID);
			int position = element.ElementsAfterSelf().Count();
			if (position == 0) return;          // if current page is the bottom page, no need to move down

			XElement NextElement = element.ElementsAfterSelf().Last();
			element.Remove();
			string CurPgLevel = NextElement.Attribute("pageLevel").Value;
			element.Attribute("pageLevel").Value = CurPgLevel;
			NextElement.AddAfterSelf(element);
			one.UpdateHierarchy(CurSection);
		}

	}
}

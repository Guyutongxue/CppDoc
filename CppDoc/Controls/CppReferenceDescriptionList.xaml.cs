// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using AngleSharp.Dom;
using CppDoc.Pages;
using CppDoc.Parser;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace CppDoc.Controls
{
    public sealed partial class CppReferenceDescriptionList : UserControl
    {
        Frame frame;
        bool loaded = false;

        public CppReferenceDescriptionList(Frame frame, IElement table)
        {
            this.InitializeComponent();
            this.frame = frame;
            this.ContactsCVS.Source = GetItems(table);
            new Task(() =>
            {
                System.Threading.Thread.Sleep(500);
                loaded = true;
            }).Start();
        }

        static void SetMaxWidth(GroupInfoList list)
        {
            var width = list.Select(i => i.Names.MaxBy(s => s.Length)?.Length ?? 0).Max() * 16 + 80;
            var gl = new GridLength(width);
            list.ForEach(i => i.Width = gl);
        }

        public static List<GroupInfoList> GetItems(IElement table)
        {
            var result = new List<GroupInfoList>();
            var list = new GroupInfoList();
            foreach (var tr in table.QuerySelectorAll("tr"))
            {
                var tds = tr.QuerySelectorAll("td");
                if (tds.Length == 1)
                {
                    var header = tr.QuerySelector(".mw-headline");
                    if (header is not null)
                    {
                        if (list.Key is null)
                        {
                            list.Key = header.TextContent.Trim();
                        } else
                        {
                            SetMaxWidth(list);
                            result.Add(list);
                            list = new GroupInfoList() { Key = header.TextContent.Trim() };
                        }
                        System.Diagnostics.Debug.WriteLine(header.TextContent.Trim());
                    }
                }
                if (tds.Length == 2)
                {
                    var item = new DescriptionItem();
                    var leftTd = tds[0];
                    var rightTd = tds[1];
                    var tlines = leftTd.QuerySelectorAll(".t-lines");
                    if (tlines.Length == 0) continue;
                    item.Names = tlines[0].Children.Select(e => e.TextContent.Trim()).ToList();
                    if (tlines.Length > 1)
                    {
                        item.Marks = tlines[1].Children.Select(e => e.TextContent.Trim()).ToList();
                    }
                    if (leftTd.QuerySelector("a") is IElement a && a.Attributes["href"] is IAttr href)
                    {
                        item.Link = href.Value;
                    }
                    var mark = rightTd.QuerySelector(".t-mark");
                    if (mark is not null)
                    {
                        item.Type = mark.TextContent.Trim();
                        mark.Remove();
                    }
                    item.Description = rightTd.TextContent.Trim();
                    list.Add(item);
                }
            }
            if (list.Count > 0)
            {
                SetMaxWidth(list);
                result.Add(list);
            }
            return result;
        }

        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var list = (ListView)sender;
            if (!loaded)
            {

                list.SelectedItem = null;
                return;
            }
            if (list.SelectedItem is DescriptionItem item && item.Link is string link)
            {
                string pageLink = CppRefDocumentBase.HrefToNavigateLink(link);
                if (CppReferencePage.CurrentLink != pageLink)
                {
                    frame.Navigate(typeof(CppReferencePage), new CppReferenceNavigateParameter(pageLink));
                }
            }
        }
    }

    public class GroupInfoList : List<DescriptionItem>
    {
        public GroupInfoList()
        {
        }
        public string? Key { get; set; }
    }

    public class DescriptionItem
    {
        public List<string> Names { get; set; } = new();

        public List<string> Marks { get; set; } = new();
        public string? Link { get; set; }
        public string Description { get; set; } = "";
        public string Type { get; set; } = "";

        public GridLength Width { get; set; } = GridLength.Auto;

    }
}

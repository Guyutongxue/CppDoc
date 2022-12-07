// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using AngleSharp.Dom;
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
        public CppReferenceDescriptionList(IElement table)
        {
            this.InitializeComponent();
            this.ContactsCVS.Source = GetItems(table);
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
                    var header = tr.QuerySelector("h5");
                    if (header is not null)
                    {
                        if (list.Key is null)
                        {
                            list.Key = header.TextContent.Trim();
                        } else
                        {
                            result.Add(list);
                            list = new GroupInfoList() { Key = header.TextContent.Trim() };
                        }
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
                result.Add(list);
            }
            return result;
        }

        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ((ListView)sender).SelectedItem = null;
        }
    }

    public class GroupInfoList : List<object>
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
        public string Link { get; set; } = "";
        public string Description { get; set; } = "";
        public string Type { get; set; } = "";

    }
}

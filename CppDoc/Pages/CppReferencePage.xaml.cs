// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Documents;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace CppDoc
{
    public class CppReferenceNavigateParameter
    {
        public string PageLink { get; set; }
        public CppReferenceNavigateParameter(string pageLink)
        {
            PageLink = pageLink;
        }
    }


    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CppReferencePage : Page
    {
        public CppReferencePage()
        {
            this.InitializeComponent();
            var text = new Italic();
            text.Inlines.Add(new Run() { Text = "italic text..." });
            var link = new Hyperlink();
            link.Inlines.Add(new Run() { Text = "click me" });
            link.Click += (_, _) => { Frame.Navigate(typeof(CppReferencePage), new CppReferenceNavigateParameter("another")); };
            textBlock.Inlines.Add(text);
            textBlock.Inlines.Add(link);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is CppReferenceNavigateParameter p) {
                textBlock.Inlines.Add(new Run() { Text = p.PageLink });
            }
            base.OnNavigatedTo(e);
        }
    }
}

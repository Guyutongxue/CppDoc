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
using CppDoc.Controls;
using CppDoc.Parser;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace CppDoc.Pages
{
    public class CppReferenceNavigateParameter
    {
        public string PageLink { get; set; }
        public CppReferenceNavigateParameter(string pageLink)
        {
            PageLink = pageLink;
        }

        public override string ToString()
        {
            return PageLink;
        }
    }


    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CppReferencePage : Page
    {
        static public string? CurrentLink { get; private set; }

        public CppReferencePage()
        {
            this.InitializeComponent();
            searchBox.IndexChosen += NavigateByLink;
            //var text = new Italic();
            //text.Inlines.Add(new Run() { Text = "italic text..." });
            //var link = new Hyperlink();
            //link.Inlines.Add(new Run() { Text = "click me" });
            //link.Click += (_, _) => { Frame.Navigate(typeof(CppReferencePage), new CppReferenceNavigateParameter("another")); };
            //textBlock.Blocks.Add(text);
            //textBlock.Blocks.Add(link);
        }

        void NavigateByLink(object sender, IndexChosenEventArgs e)
        {
            Frame.Navigate(typeof(CppReferencePage), new CppReferenceNavigateParameter(e.Link));
        }

        void SetHeader(CppReferenceHeader header)
        {
            var nv = MainWindow.GetNavigationView(this);
            if (nv is not null)
            {
                header.IndexChosen += NavigateByLink;
                nv.Header = header;
            }
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var newLink = (e.Parameter as CppReferenceNavigateParameter)?.PageLink;
            CurrentLink = newLink;
            if (newLink is null)
            {
                searchBox.Visibility = Visibility.Visible;
                SetHeader(CppReferenceHeader.CreateHome());
            }
            else
            {
                var doc = await CppRefDocumentFactory.Create(Frame, newLink);
                SetHeader(CppReferenceHeader.Create(doc));
                foreach (var ele in doc.Parse())
                {
                    panel.Children.Add(ele);
                }
                panel.Children.Add(new HyperlinkButton
                {
                    Content = "原网页",
                    NavigateUri = new Uri($"https://{SettingsPage.GetLanguage()}.cppreference.com/w/{newLink}")
                });
            }
        }
    }
}

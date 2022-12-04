// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace CppDoc
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
        }

        readonly Dictionary<string, Type> pageMap = new Dictionary<string, Type> {
            { "CppReference", typeof(CppReferencePage) },
            { "CompilerExplorer", typeof(CompilerExplorerPage) },
        };

        private void contentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            navigation.IsBackEnabled = contentFrame.CanGoBack;

            if (contentFrame.SourcePageType == typeof(SettingsPage))
            {
                // SettingsItem is not part of NavView.MenuItems, and doesn't have a Tag.
                navigation.SelectedItem = navigation.SettingsItem;
            }
            else if (contentFrame.SourcePageType != null)
            {
                navigation.SelectedItem = navigation.MenuItems
                    .OfType<NavigationViewItem>()
                    .First(n => n.Tag.Equals(pageMap.First(kv => kv.Value == contentFrame.SourcePageType).Key));
            }

            navigation.Header = ((NavigationViewItem)navigation.SelectedItem)?.Content?.ToString();
        }

        private void navigation_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            if (contentFrame.CanGoBack) contentFrame.GoBack();
        }

        private void navigation_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected == true)
            {
                contentFrame.Navigate(typeof(SettingsPage));
            }
            else
            {
                var selectedItem = (NavigationViewItem)args.SelectedItem;
                string selectedItemTag = (string)selectedItem.Tag;
                Type pageType = pageMap[selectedItemTag];
                contentFrame.Navigate(pageType, null, args.RecommendedNavigationTransitionInfo);
            }
        }
    }
}

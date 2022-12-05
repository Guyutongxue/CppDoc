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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace CppDoc.Controls
{
    public enum CppReferenceHeaderType
    {
        Home,
        LibraryName,
        Core
    }

    public sealed partial class CppReferenceHeader : UserControl
    {
        public CppReferenceHeader(CppReferenceHeaderType type, string title, string prefix = "")
        {
            Type = type;
            Prefix = prefix;
            Title = title;
            this.InitializeComponent();
            if (Type == CppReferenceHeaderType.Home)
            {
                searchBox.Visibility = Visibility.Collapsed;
            } else
            {
                searchBox.IndexChosen += (o, e) => IndexChosen?.Invoke(o, e);
            }
            if (Type == CppReferenceHeaderType.LibraryName)
            {
                textBlockTitle.FontFamily = new FontFamily("Consolas");
            }
        }

        public event CpprefIndexChosenDelegate? IndexChosen;

        public string Prefix { get; set; }
        public string Title { get; set; }
        public CppReferenceHeaderType Type { get; set; }

        public static CppReferenceHeader HomePage()
        {
            return new CppReferenceHeader(CppReferenceHeaderType.Home, "CppReference.com");
        }
        public static CppReferenceHeader CorePage(string coreTitle)
        {
            return new CppReferenceHeader(CppReferenceHeaderType.Core, coreTitle);
        }
        public static CppReferenceHeader LibraryNamePage(string librayName, string? prefix)
        {
            return new CppReferenceHeader(CppReferenceHeaderType.LibraryName, librayName, prefix ?? "");
        }
    }
}

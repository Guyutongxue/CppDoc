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
using CppDoc.Parser;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace CppDoc.Controls
{

    public sealed partial class CppReferenceHeader : UserControl
    {
        public CppReferenceHeader(CppRefDocumentType type, string title, string prefix = "")
        {
            Type = type;
            Prefix = prefix;
            Title = title;
            this.InitializeComponent();
            if (Type == CppRefDocumentType.Home)
            {
                searchBox.Visibility = Visibility.Collapsed;
            } else
            {
                searchBox.IndexChosen += (o, e) => IndexChosen?.Invoke(o, e);
            }
            if (Type == CppRefDocumentType.LibraryName)
            {
                textBlockTitle.FontFamily = new FontFamily("Consolas");
            }
        }

        public event CpprefIndexChosenDelegate? IndexChosen;

        public string Prefix { get; set; }
        public string Title { get; set; }
        public CppRefDocumentType Type { get; set; }

        public static CppReferenceHeader CreateHome()
        {
            return new CppReferenceHeader(CppRefDocumentType.Home, "CppReference.com");
        }
        public static CppReferenceHeader Create(ICppRefDocument doc)
        {
            return doc.Type switch
            {
                CppRefDocumentType.Home => CreateHome(),
                CppRefDocumentType.LibraryName => new CppReferenceHeader(CppRefDocumentType.LibraryName, doc.GetTitle(), doc.GetPrefix() ?? ""),
                _ => new CppReferenceHeader(CppRefDocumentType.Other, doc.GetTitle())
            };
        }
    }
}

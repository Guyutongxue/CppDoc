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
using System.Net.Http;
using System.Net.Http.Json;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace CppDoc.Controls
{
    public class IndexItem
    {
        public string Name { get; set; }
        public string Link { get; set; }
    }
    public record class IndexItemRaw(
        string Type,
        string Name,
        string Description,
        string Link,
        string? SymbolType,
        string? TokenType);

    public class IndexChosenEventArgs : EventArgs
    {
        public string Link { get; private set; }
        public string? Name { get; private set; }
        public string? Type { get; private set; }
        public IndexChosenEventArgs(string link, string? name, string? type)
        {
            Link = link;
            Name = name;
            Type = type;
        }
    }
    public delegate void CpprefIndexChosenDelegate(object sender, IndexChosenEventArgs e);

    public sealed partial class CppReferenceIndex : UserControl
    {
        private readonly List<IndexItem> items = new();

        public CppReferenceIndex()
        {
            this.InitializeComponent();
            LoadCpprefIndex();
        }

        public event CpprefIndexChosenDelegate? IndexChosen;

        async void LoadCpprefIndex()
        {
            var client = new HttpClient();
            var result = await client.GetFromJsonAsync<List<IndexItemRaw>>("https://cdn.jsdelivr.net/npm/@gytx/cppreference-index/dist/generated.json");
            foreach (var item in result)
            {
                items.Add(new IndexItem
                {
                    Name = item.Name,
                    Link = item.Link,
                });
            }
        }

        void suggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs e)
        {
            var word = sender.Text;
            if (e.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                sender.ItemsSource = items
                    .Where((i) => i.Name.Contains(word))
                    .OrderBy((i) => {
                        var index = i.Name.IndexOf(':');
                        if (index == -1) return (1, i.Name.Length, i.Name);
                        var substr = i.Name[(index + 1)..];
                        return (0, substr.Length, substr);
                    });
            }
        }

        void suggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs e)
        {
            if (e.SelectedItem is IndexItem item)
            {
                sender.Text = item.Name;
            }
        }

        void suggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs e)
        {
            if (e.ChosenSuggestion is IndexItem item)
            {
                IndexChosen?.Invoke(this, new IndexChosenEventArgs(item.Link, item.Name, null));
            }
        }
    }
}

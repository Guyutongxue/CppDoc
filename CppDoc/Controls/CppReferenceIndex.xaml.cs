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
using System.Text.Json;
using System.Threading.Tasks;
using AngleSharp.Dom;
using System.Globalization;
using System.Reflection.Metadata;
using System.Security.Claims;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace CppDoc.Controls
{
    public abstract class IndexItem
    {
        public string Name { get; set; } = "";
        public string Link { get; set; } = "";
        public string Description { get; set; } = "";
        public Marks? Marks { get; set; }

        public virtual string DetailedName { get => Name; }
    }

    public enum SymbolType
    {
        Concept, Class, ClassTemplate, ClassTemplateSpecialization, TypeAlias,
        TypeAliasTemplate, Function, FunctionTemplate, Enumeration, Enumerator,
        Macro, FunctionLikeMacro, Constant, Niebloid, Object,
        VariableTemplate, Namespace, Other
    }
    public enum TokenType
    {
        DirectiveName, Operator, Replacement, OperatorOutsideDirective
    }
    public class SymbolIndexItem : IndexItem
    {
        public SymbolType SymbolType { get; set; } = SymbolType.Other;
        public string? Note { get; set; }

        public override string DetailedName { get => Name + (Note is null ? "" : $" ({Note})"); }
    }
    public class HeaderIndexItem : IndexItem { }
    public class KeywordIndexItem : IndexItem
    {
        public bool CanBeUsedAsIdentifier { get; set; } = false;
    }
    public class AttributeIndexItem : IndexItem { }
    public class PreprocessorTokenIndexItem : IndexItem
    {
        public TokenType TokenType { get; set; } = TokenType.DirectiveName;
    }

    public record class Marks(
        string? since,
        string? deprecated,
        string? removed);

    record class IndexItemRaw(
        string type,
        string name,
        string description,
        string link,
        Marks? marks,
        string? symbolType,
        string? note,
        string? tokenType,
        bool? canBeUsedAsIdentifier);

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

    class CpprefIndexLoader
    {
        private static readonly CpprefIndexLoader instance = new();
        public static List<IndexItem> Items
        {
            get
            {
                return instance.items;
            }
        }

        static CpprefIndexLoader() { }

        public List<IndexItem> items = new();

        private CpprefIndexLoader()
        {
            var _ = Load();
        }

        const string LOCAL_FILENAME = "cppreference-index.json";
        async Task Load()
        {
            var local = await LoadLocal();
            if (local is not null)
            {
                items = local;
            }
            else
            {
                var remote = await LoadRemote();
                if (remote is not null)
                {
                    items = remote;
                }
            }
        }

        static List<IndexItem> LoadFromRaw(List<IndexItemRaw> raw)
        {
            var items = new List<IndexItem>();
            foreach (var item in raw)
            {
                IndexItem result;
                switch (item.type)
                {
                    case "symbol":
                        result = new SymbolIndexItem()
                        {
                            Name = item.name,
                            Link = item.link,
                            Description = item.description,
                            Marks = item.marks,
                            Note = item.note,
                            SymbolType = Enum.Parse<SymbolType>(item.symbolType!, true)
                        };
                        break;
                    case "header":
                        result = new HeaderIndexItem()
                        {
                            Name = item.name,
                            Link = item.link,
                            Description = item.description,
                            Marks = item.marks,
                        };
                        break;
                    case "keyword":
                        result = new KeywordIndexItem()
                        {
                            Name = item.name,
                            Link = item.link,
                            Description = item.description,
                            Marks = item.marks,
                            CanBeUsedAsIdentifier = item.canBeUsedAsIdentifier!.Value
                        };
                        break;
                    case "attribute":
                        result = new AttributeIndexItem()
                        {
                            Name = item.name,
                            Link = item.link,
                            Description = item.description,
                            Marks = item.marks,
                        };
                        break;
                    case "preprocessorToken":
                        result = new PreprocessorTokenIndexItem()
                        {
                            Name = item.name,
                            Link = item.link,
                            Description = item.description,
                            Marks = item.marks,
                            TokenType = Enum.Parse<TokenType>(item.tokenType!, true)
                        };
                        break;
                    default: continue;
                }
                items.Add(result);
            }
            return items;
        }

        static async Task<List<IndexItem>?> LoadLocal()
        {
            try
            {
                var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                using var stream = await localFolder.OpenStreamForReadAsync(LOCAL_FILENAME);
                var result = JsonSerializer.Deserialize<List<IndexItemRaw>>(stream);
                if (result is null) return null;
                return LoadFromRaw(result);
            }
            catch
            {
                return null;
            }
        }

        static async Task<List<IndexItem>?> LoadRemote()
        {
            var client = new HttpClient();
            using var remote = await client.GetStreamAsync("https://cdn.jsdelivr.net/npm/@gytx/cppreference-index/dist/generated.json");
            if (remote is null) return null;
            var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            {
                using var local = await localFolder.OpenStreamForWriteAsync(LOCAL_FILENAME, Windows.Storage.CreationCollisionOption.ReplaceExisting);
                await remote.CopyToAsync(local);
            }
            return await LoadLocal();
        }
    }


    public sealed partial class CppReferenceIndex : UserControl
    {

        public CppReferenceIndex()
        {
            this.InitializeComponent();
        }

        public event CpprefIndexChosenDelegate? IndexChosen;

        void suggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs e)
        {
            var word = sender.Text;
            if (e.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                sender.ItemsSource = CpprefIndexLoader.Items
                    .Where((i) => i.Name.Contains(word))
                    .OrderBy((i) =>
                    {
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

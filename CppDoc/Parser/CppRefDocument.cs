
using AngleSharp;
using AngleSharp.Dom;
using CppDoc.Controls;
using CppDoc.Pages;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Text;

namespace CppDoc.Parser
{
    public enum CppRefDocumentType
    {
        Core,
        Header,
        Keyword,
        LibraryName,
        Home,
        Other
    }

    public interface ICppRefDocument
    {
        public CppRefDocumentType Type { get; }
        public string? GetPrefix() { return null; }
        public string GetTitle();
        public List<UIElement> Parse();
    }

    public class CppRefDocumentBase
    {
        Frame frame;
        protected CppRefDocumentBase(Frame frame)
        {
            this.frame = frame;
        }

        static public string HrefToNavigateLink(string href)
        {
            var pageLink = href[3..];
            var hashIndex = pageLink.IndexOf('#');
            if (hashIndex != -1)
            {
                pageLink = pageLink[..hashIndex];
            }
            return pageLink;
        }

        protected RichTextBlock ParseParagraphs(IElement article)
        {
            var rtb = new RichTextBlock() { Padding = new Thickness(0) };
            Paragraph? inlines = null;
            var inlineTags = new string[] {
                "A", "SPAN", "CODE", "B", "I"
            };
            foreach (var n in article.ChildNodes)
            {
                if (n is IElement ee && inlineTags.Contains(ee.TagName) 
                    || n is IText && !string.IsNullOrWhiteSpace(n.TextContent))
                {
                    if (inlines is null)
                    {
                        inlines = new Paragraph();
                    }
                    inlines.Inlines.Add(ParseInline(n));
                }
                else if (n is Element e)
                {
                    if (inlines is not null)
                    {
                        rtb.Blocks.Add(inlines);
                        inlines = null;
                    }

                    if (e.TagName == "P")
                    {
                        var paragraph = new Paragraph();
                        foreach (var i in ParseInlines(e))
                        {
                            paragraph.Inlines.Add(i);
                        }
                        rtb.Blocks.Add(paragraph);
                    }
                    else if (e.TagName == "UL")
                    {
                        rtb.Blocks.Add(ParseList(e));
                    }
                    else if (e.TagName == "DL")
                    {
                        rtb.Blocks.Add(ParseDescriptionList(e));
                    }
                    else if (e.TagName == "H3")
                    {
                        var para = new Paragraph();
                        foreach (var i in ParseInlines(e.QuerySelector(".mw-headline")))
                        {
                            para.Inlines.Add(i);
                        }
                        para.FontWeight = new FontWeight(600);
                        para.FontSize = 20;
                        para.Margin = new Thickness(0, 20, 0, 10);
                        rtb.Blocks.Add(para);
                    } 
                    else if (e.TagName == "TABLE" && e.ClassList.Contains("t-dsc-begin"))
                    {
                        rtb.Blocks.Add(ParseDescTable(e));
                    }
                    else if (e.ClassList.Contains("t-example"))
                    {
                        var code = e.QuerySelector(".cpp")?.TextContent.Trim().Replace('\xa0', ' ') ?? "";
                        rtb.Blocks.Add(UIElementToParagraph(new EditorWebView(code)));
                    }
                }
            }
            if (inlines is not null)
            {
                rtb.Blocks.Add(inlines);
            }
            return rtb;
        }
        private Paragraph UIElementToParagraph(UIElement e)
        {
            var para = new Paragraph();
            var container = new InlineUIContainer();
            container.Child = e;
            para.Inlines.Add(container);
            return para;
        }

        private Paragraph ParseDescriptionList(IElement dl)
        {
            var grid = new Grid();
            foreach (var (item, index) in dl.Children.Select((v, i) => (v, i)))
            {
                if (item.TagName != "DD") continue;
                var uiElement = ParseParagraphs(item);
                grid.Children.Add(uiElement);
                Grid.SetColumn(uiElement, 1);
                Grid.SetRow(uiElement, index);
                grid.RowDefinitions.Add(new RowDefinition());
            }
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(10) });
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            return UIElementToParagraph(grid);
        }

        private Paragraph ParseList(IElement list)
        {
            var grid = new Grid();
            foreach (var (item, index) in list.Children.Select((v, i) => (v, i)))
            {
                if (item.TagName != "LI") continue;
                var rtb = ParseParagraphs(item);
                var marker = new TextBlock() { Text = "•" };
                grid.Children.Add(marker);
                grid.Children.Add(rtb);
                Grid.SetColumn(marker, 0);
                Grid.SetColumn(rtb, 1);
                Grid.SetRow(marker, index);
                Grid.SetRow(rtb, index);
                grid.RowDefinitions.Add(new RowDefinition());
            }
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            return UIElementToParagraph(grid);
        }

        private Inline ParseInline(INode node)
        {
            if (node is IElement e)
            {
                if (e.TagName == "CODE"
                    || e.TagName == "TT"
                    || e.TagName == "SPAN" && (
                        e.ClassList.Contains("t-lc")
                        || e.ClassList.Contains("t-c")
                    ))
                {
                    var codeSpan = new Span()
                    {
                        FontFamily = new Microsoft.UI.Xaml.Media.FontFamily("Consolas")
                    };
                    foreach (var i in ParseInlines(e))
                    {
                        codeSpan.Inlines.Add(i);
                    }
                    return codeSpan;
                }
                else if (e.TagName == "A")
                {
                    var a = new Hyperlink();
                    foreach (var i in ParseInlines(e))
                    {
                        a.Inlines.Add(i);
                    }
                    if (e.Attributes["href"] is IAttr href)
                    {
                        if (href.Value.StartsWith("/w/cpp"))
                        {
                            a.Click += (sender, _) =>
                            {
                                string pageLink = HrefToNavigateLink(href.Value);
                                if (CppReferencePage.CurrentLink != pageLink)
                                {
                                    frame.Navigate(typeof(CppReferencePage), new CppReferenceNavigateParameter(pageLink));
                                }
                            };
                        }
                        else
                        {
                            var baseUri = new Uri($"https://{SettingsPage.GetLanguage()}.cppreference.com");
                            a.NavigateUri = new Uri(baseUri, href.Value);
                        }
                    }
                    return a;
                }
                else if (e.TagName == "BR")
                {
                    return new LineBreak();
                }
                else
                {
                    return new Run() { Text = node.TextContent };
                }
            }
            else
            {
                return new Run() { Text = node.TextContent.Trim() };
            }
        }

        protected List<Inline> ParseInlines(IElement? p)
        {
            var inlineList = new List<Inline>();
            if (p is null) return inlineList;
            foreach (var n in p.ChildNodes)
            {
                inlineList.Add(ParseInline(n));
            }
            return inlineList;
        }

        private Paragraph ParseDescTable(IElement p)
        {
            return UIElementToParagraph(new CppReferenceDescriptionList(frame, p));
        }
    }

    static public class CppRefDocumentFactory
    {
        static public async Task<ICppRefDocument> Create(Frame frame, string link)
        {
            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            var doc = await context.OpenAsync($"https://{SettingsPage.GetLanguage()}.cppreference.com/w/" + link);
            if (doc is null)
            {
                throw new Exception("Open link failed");
            }
            var main = doc.QuerySelector("#content");
            if (main is null)
            {
                throw new ArgumentNullException();
            }
            foreach (var e in main.QuerySelectorAll(".editsection"))
            {
                e.Remove();
            }
            if (link.StartsWith("cpp/header/"))
            {
                return new CppRefHeaderDocument(frame, main);
            }
            else
            {
                return new CppRefLibraryDocument(frame, main);
            }
        }
    }

}


using AngleSharp;
using AngleSharp.Dom;
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

        protected RichTextBlock ParseParagraphs(IElement article)
        {
            var rtb = new RichTextBlock();
            foreach (var e in article.Children)
            {
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
                var rtb = ParseParagraphs(item);
                grid.Children.Add(rtb);
                Grid.SetColumn(rtb, 1);
                Grid.SetRow(rtb, index);
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
                var tb = new TextBlock();
                foreach (var inline in ParseInlines(item))
                {
                    tb.Inlines.Add(inline);
                }
                var marker = new TextBlock() { Text = "·" };
                grid.Children.Add(marker);
                grid.Children.Add(tb);
                Grid.SetColumn(marker, 0);
                Grid.SetColumn(tb, 1);
                Grid.SetRow(marker, index);
                Grid.SetRow(tb, index);
                grid.RowDefinitions.Add(new RowDefinition());
            }
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            return UIElementToParagraph(grid);
        }

        private List<Inline> ParseInlines(IElement? p)
        {
            var inlineList = new List<Inline>();
            if (p is null) return inlineList;
            foreach (var n in p.ChildNodes)
            {
                if (n is IElement e)
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
                        inlineList.Add(codeSpan);
                    }
                    else if (e.TagName == "A")
                    {
                        var a = new Hyperlink();
                        foreach (var i in ParseInlines(e))
                        {
                            a.Inlines.Add(i);
                        }
                        if (e.Attributes["href"] is IAttr href) {
                            if (href.Value.StartsWith("/w/cpp"))
                            {
                                var pageLink = href.Value[3..];
                                var hashIndex = pageLink.IndexOf('#');
                                if (hashIndex != -1)
                                {
                                    pageLink = pageLink[..hashIndex];
                                }
                                a.Click += (sender, _) =>
                                {
                                    if (CppReferencePage.CurrentLink != pageLink)
                                    {
                                        frame.Navigate(typeof(CppReferencePage), new CppReferenceNavigateParameter(pageLink));
                                    }
                                };
                            } else
                            {
                                var baseUri = new Uri("https://zh.cppreference.com");
                                a.NavigateUri = new Uri(baseUri, href.Value);
                            }
                        }
                        inlineList.Add(a);
                    }
                    else if (e.TagName == "BR")
                    {
                        inlineList.Add(new LineBreak());
                    }
                    else
                    {
                        inlineList.Add(new Run() { Text = n.TextContent });
                    }
                }
                else
                {
                    inlineList.Add(new Run() { Text = n.TextContent });
                }
            }
            return inlineList;
        }
    }

    static public class CppRefDocumentFactory
    {
        static public async Task<ICppRefDocument> Create(Frame frame, string link)
        {
            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            var doc = await context.OpenAsync("https://zh.cppreference.com/w/" + link);
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

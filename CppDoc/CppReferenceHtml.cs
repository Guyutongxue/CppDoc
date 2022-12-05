
using AngleSharp;
using AngleSharp.Dom;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CppDoc
{
    public interface ICppRefDocument
    {
        public List<UIElement> Parse();
    }

    public class CppRefLibraryDocument : ICppRefDocument
    {
        IElement content;
        public CppRefLibraryDocument(IElement content)
        {
            this.content = content;
        }

        public string? GetPrefix()
        {
            return content.QuerySelector("#firstHeading span")?.Text();
        }

        public string GetTitle()
        {
            var n = content.QuerySelector("#firstHeading")?.Clone();
            System.Diagnostics.Debug.WriteLine(n.Text());
            if (n is IElement e)
            {
                foreach (var s in e.QuerySelectorAll("span"))
                {
                    s.Remove();
                }
                return e.Text() ?? string.Empty;
            }
            else
            {
                return string.Empty;
            }
        }

        public List<UIElement> Parse()
        {
            var elements = new List<UIElement>();
            var rtb = new RichTextBlock();
            var article = content.QuerySelector("#bodyContent");
            if (article is null)
            {
                throw new ArgumentNullException();
            }
            var beginTable = article.QuerySelector(".t-dcl-begin");
            if (beginTable is not null)
            {
                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition());
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

                foreach (var (tr, i) in beginTable.QuerySelectorAll("tr").Select((e, i) => (e, i)))
                {
                    grid.RowDefinitions.Add(new RowDefinition());
                    var tds = tr.QuerySelectorAll("td");
                    if (tds is null) continue;
                    if (tds.Length < 3) continue;
                    for (int j = 0; j < 3; j++)
                    {
                        var tb = new TextBlock() { Text = tds[j].Text() };
                        if (j == 0)
                        {
                            tb.FontFamily = new Microsoft.UI.Xaml.Media.FontFamily("Consolas");
                        }
                        grid.Children.Add(tb);
                        Grid.SetRow(tb, i);
                        Grid.SetColumn(tb, j);
                    }
                }
                elements.Add(grid);
                beginTable.Remove();
            }
            foreach (var e in article.QuerySelectorAll("p"))
            {
                var paragraph = new Paragraph();
                paragraph.Inlines.Add(new Run() { Text = e.Text() });
                rtb.Blocks.Add(paragraph);
            }
            elements.Add(rtb);
            return elements;
        }
    }

    static public class CppRefDocumentFactory
    {
        static public async Task<ICppRefDocument> Create(string link)
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
            var page = new CppRefLibraryDocument(main);
            return page;
        }
    }

}

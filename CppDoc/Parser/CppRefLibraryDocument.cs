using AngleSharp.Dom;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppDoc.Parser
{
    public class CppRefLibraryDocument : CppRefDocumentBase, ICppRefDocument
    {
        public CppRefDocumentType Type { get; } = CppRefDocumentType.LibraryName;

        IElement content;
        public CppRefLibraryDocument(Frame frame, IElement content) : base(frame)
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
            var article = content.QuerySelector("#mw-content-text");
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
                        var tb = new TextBlock();
                        if (j == 0 && tr.ClassList.Contains("t-dsc-header"))
                        {
                            foreach (var inline in ParseInlines(tds[j].QuerySelector("div")!))
                            {
                                tb.Inlines.Add(inline);
                            }
                        } else
                        {
                            tb.Text = tds[j].TextContent;
                            if (j == 0)
                            {
                                tb.FontFamily = new Microsoft.UI.Xaml.Media.FontFamily("Consolas");
                            }
                            if (j == 2)
                            {
                                tb.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.DarkGreen);
                            }
                        }
                        grid.Children.Add(tb);
                        Grid.SetRow(tb, i);
                        Grid.SetColumn(tb, j);
                    }
                }
                elements.Add(grid);
                beginTable.Remove();
            }
            elements.Add(ParseParagraphs(article));
            return elements;
        }
    }

}

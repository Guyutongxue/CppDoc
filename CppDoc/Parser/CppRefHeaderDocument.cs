using AngleSharp.Dom;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppDoc.Parser
{
    internal class CppRefHeaderDocument : CppRefDocumentBase, ICppRefDocument
    {
        public CppRefDocumentType Type { get; } = CppRefDocumentType.Header;

        IElement content;
        public CppRefHeaderDocument(Frame frame, IElement content) : base(frame)
        {
            this.content = content;
        }
        public string GetTitle()
        {
            var n = content.QuerySelector("#firstHeading")?.Clone();
            if (n is IElement e)
            {
                e.QuerySelector("span")?.Remove();
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

            elements.Add(ParseParagraphs(content));
            return elements;
        }
    }
}

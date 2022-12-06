using CppDoc.Controls;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace CppDoc.Converters
{
    public class CpprefItemToTypeColorConverter : IValueConverter
    {
        (byte, byte, byte) ToColor(IndexItem item)
        {
            if (item is HeaderIndexItem)
            {
                return (0xa3, 0x15, 0x15);
            }
            else if (item is KeywordIndexItem)
            {
                return (0, 0, 255);
            }
            else if (item is SymbolIndexItem symbol)
            {
                return symbol.SymbolType switch
                {
                    SymbolType.Concept => (0x04, 0x51, 0xa5),
                    SymbolType.Class => (0, 0x70, 0xc1),
                    SymbolType.ClassTemplate => (0, 0x70, 0xc1),
                    SymbolType.ClassTemplateSpecialization => (0, 0x70, 0xc1),
                    SymbolType.TypeAlias => (0x26, 0x7f, 0x99),
                    SymbolType.TypeAliasTemplate => (0x26, 0x7f, 0x99),
                    SymbolType.Function => (0x79, 0x5e, 0x26),
                    SymbolType.FunctionTemplate => (0x79, 0x5e, 0x26),
                    SymbolType.Enumeration => (0x26, 0x7f, 0x99),
                    SymbolType.Enumerator => (0x09, 0x86, 0x58),
                    SymbolType.Macro => (0, 128, 0),
                    SymbolType.FunctionLikeMacro => (0, 128, 0),
                    SymbolType.Constant => (0x09, 0x86, 0x58),
                    SymbolType.Niebloid => (0x79, 0x5e, 0x26),
                    SymbolType.Object => (0, 0x10, 0x80),
                    SymbolType.VariableTemplate => (0x09, 0x86, 0x58),
                    SymbolType.Namespace => (0x81, 0x1f, 0x3f),
                    _ => (128, 128, 128)
                };
            }
            else if (item is PreprocessorTokenIndexItem pp)
            {
                return (0xaf, 0, 0xdb);
                //pp.TokenType switch
                //{
                //    TokenType.Replacement => ,
                //    TokenType.DirectiveName => (0xaf, 0, 0xdb),
                //    TokenType.Operator => (0xaf, 0, 0xdb),
                //    TokenType.OperatorOutsideDirective => (0xaf, 0, 0xdb),
                //    _ => (128, 128, 128)
                //};
            }
            else if (item is AttributeIndexItem)
            {
                return (128, 128, 128);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var item = (IndexItem)value;
            var (r, g, b) = ToColor(item);
            return new SolidColorBrush(ColorHelper.FromArgb(255, r, g, b));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

}

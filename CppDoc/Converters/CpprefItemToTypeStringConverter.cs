using CppDoc.Controls;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppDoc.Converters
{
    public class CpprefItemToTypeStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var item = (IndexItem)value;
            if (item is HeaderIndexItem)
            {
                return "标头";
            }
            else if (item is KeywordIndexItem)
            {
                return "关键字";
            }
            else if (item is SymbolIndexItem symbol)
            {
                return symbol.SymbolType switch
                {
                    SymbolType.Concept => "概念",
                    SymbolType.Class => "类",
                    SymbolType.ClassTemplate => "类模板",
                    SymbolType.ClassTemplateSpecialization => "类模板特化",
                    SymbolType.TypeAlias => "类型别名",
                    SymbolType.TypeAliasTemplate => "类型别名模板",
                    SymbolType.Function => "函数",
                    SymbolType.FunctionTemplate => "函数模板",
                    SymbolType.Enumeration => "枚举",
                    SymbolType.Enumerator => "枚举项",
                    SymbolType.Macro => "宏",
                    SymbolType.FunctionLikeMacro => "仿函数宏",
                    SymbolType.Constant => "常量",
                    SymbolType.Niebloid => "尼氏体",
                    SymbolType.Object => "对象",
                    SymbolType.VariableTemplate => "变量模板",
                    SymbolType.Namespace => "命名空间",
                    _ => "其它"
                };
            }
            else if (item is PreprocessorTokenIndexItem pp)
            {
                return pp.TokenType switch
                {
                    TokenType.Replacement => "预处理替换记号",
                    TokenType.DirectiveName => "预处理指令",
                    TokenType.Operator => "预处理运算符",
                    TokenType.OperatorOutsideDirective => "预处理运算符",
                    _ => "预处理记号"
                };
            }
            else if (item is AttributeIndexItem)
            {
                return "特性";
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

}

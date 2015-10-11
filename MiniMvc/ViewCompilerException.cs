using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Razor.Parser.SyntaxTree;

namespace MiniMvc
{
    public class ViewCompilerException : Exception
    {
        private IList<RazorError> _parseErrors;
        private CompilerErrorCollection _compileErrors;
        private string _sourceFileName;

        public ViewCompilerException(IList<RazorError> errors, string sourceFileName)
        {
            _parseErrors = errors;
            _sourceFileName = sourceFileName;
        }

        public ViewCompilerException(CompilerErrorCollection errors, string sourceFileName)
        {
            _compileErrors = errors;
            _sourceFileName = sourceFileName;
        }

        public IList<RazorError> Errors { get { return _parseErrors; } }

        public override string ToString()
        {
            if (_parseErrors != null)
                return string.Join(Environment.NewLine, _parseErrors.Select(o => string.Format("{0} ({1},{2}): {3}", _sourceFileName, o.Location.LineIndex, o.Location.CharacterIndex, o.Message)));
            if (_compileErrors != null)
                return string.Join(Environment.NewLine, Enumerate(_compileErrors).Cast<CompilerError>().Select(o => string.Format("{0} ({1},{2}): {3} {4}", _sourceFileName, o.Line, o.Column, o.IsWarning ? "WARNING" : "ERROR", o.ErrorText)));
            return base.ToString();
        }

        private IEnumerable<object> Enumerate(System.Collections.IEnumerable src)
        {
            foreach (var c in src) yield return c;
        }

    }
}

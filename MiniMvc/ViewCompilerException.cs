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

        public ViewCompilerException(IList<RazorError> errors, string sourceFileName):
            base($"Error in {sourceFileName}:{Environment.NewLine}{string.Join(Environment.NewLine, errors.Select(o => string.Format("{0} ({1},{2}): {3}", sourceFileName, o.Location.LineIndex, o.Location.CharacterIndex, o.Message)))}")
        {
            _parseErrors = errors;
            _sourceFileName = sourceFileName;
        }

        public ViewCompilerException(CompilerErrorCollection errors, string sourceFileName) :
            base($"Error in {sourceFileName}:{Environment.NewLine}{string.Join(Environment.NewLine, Enumerate(errors).Cast<CompilerError>().Select(o => string.Format("{0} ({1},{2}): {3} {4}", sourceFileName, o.Line, o.Column, o.IsWarning ? "WARNING" : "ERROR", o.ErrorText)))}")
        {
            _compileErrors = errors;
            _sourceFileName = sourceFileName;
        }

        public IList<RazorError> Errors { get { return _parseErrors; } }

        private static IEnumerable<object> Enumerate(System.Collections.IEnumerable src)
        {
            foreach (var c in src) yield return c;
        }

    }
}

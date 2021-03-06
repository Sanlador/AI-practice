//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.5.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from TakPGN.g4 by ANTLR 4.5.1

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591

using System;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using DFA = Antlr4.Runtime.Dfa.DFA;
namespace TakEngine.Notation.Generated
{
    [System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.5.1")]
    public partial class TakPGNLexer : Lexer
    {
        public const int
            P1_WINS_ROAD = 1, P1_WINS_FLATS = 2, P2_WINS_ROAD = 3, P2_WINS_FLATS = 4, DRAWN_GAME = 5,
            REST_OF_LINE_COMMENT = 6, BRACE_COMMENT = 7, ESCAPE = 8, SPACES = 9, STRING = 10,
            INTEGER = 11, PERIOD = 12, ASTERISK = 13, LEFT_BRACKET = 14, RIGHT_BRACKET = 15,
            LEFT_PARENTHESIS = 16, RIGHT_PARENTHESIS = 17, NUMERIC_ANNOTATION_GLYPH = 18,
            SYMBOL = 19, UNEXPECTED_CHAR = 20;
        public static string[] modeNames = {
		"DEFAULT_MODE"
	};

        public static readonly string[] ruleNames = {
		"P1_WINS_ROAD", "P1_WINS_FLATS", "P2_WINS_ROAD", "P2_WINS_FLATS", "DRAWN_GAME", 
		"REST_OF_LINE_COMMENT", "BRACE_COMMENT", "ESCAPE", "SPACES", "STRING", 
		"INTEGER", "PERIOD", "ASTERISK", "LEFT_BRACKET", "RIGHT_BRACKET", "LEFT_PARENTHESIS", 
		"RIGHT_PARENTHESIS", "NUMERIC_ANNOTATION_GLYPH", "SYMBOL", "UNEXPECTED_CHAR"
	};


        public TakPGNLexer(ICharStream input)
            : base(input)
        {
            Interpreter = new LexerATNSimulator(this, _ATN);
        }

        private static readonly string[] _LiteralNames = {
		null, "'R-0'", "'F-0'", "'0-R'", "'0-F'", "'1/2-1/2'", null, null, null, 
		null, null, null, "'.'", "'*'", "'['", "']'", "'('", "')'"
	};
        private static readonly string[] _SymbolicNames = {
		null, "P1_WINS_ROAD", "P1_WINS_FLATS", "P2_WINS_ROAD", "P2_WINS_FLATS", 
		"DRAWN_GAME", "REST_OF_LINE_COMMENT", "BRACE_COMMENT", "ESCAPE", "SPACES", 
		"STRING", "INTEGER", "PERIOD", "ASTERISK", "LEFT_BRACKET", "RIGHT_BRACKET", 
		"LEFT_PARENTHESIS", "RIGHT_PARENTHESIS", "NUMERIC_ANNOTATION_GLYPH", "SYMBOL", 
		"UNEXPECTED_CHAR"
	};
        public static readonly IVocabulary DefaultVocabulary = new Vocabulary(_LiteralNames, _SymbolicNames);

        [NotNull]
        public override IVocabulary Vocabulary
        {
            get
            {
                return DefaultVocabulary;
            }
        }

        public override string GrammarFileName { get { return "TakPGN.g4"; } }

        public override string[] RuleNames { get { return ruleNames; } }

        public override string[] ModeNames { get { return modeNames; } }

        public override string SerializedAtn { get { return _serializedATN; } }

        public override bool Sempred(RuleContext _localctx, int ruleIndex, int predIndex)
        {
            switch (ruleIndex)
            {
                case 7: return ESCAPE_sempred(_localctx, predIndex);
            }
            return true;
        }
        private bool ESCAPE_sempred(RuleContext _localctx, int predIndex)
        {
            switch (predIndex)
            {
                case 0: return ((ITokenSource)this).Column == 0;
            }
            return true;
        }

        public static readonly string _serializedATN =
            "\x3\x430\xD6D1\x8206\xAD2D\x4417\xAEF1\x8D80\xAADD\x2\x16\x94\b\x1\x4" +
            "\x2\t\x2\x4\x3\t\x3\x4\x4\t\x4\x4\x5\t\x5\x4\x6\t\x6\x4\a\t\a\x4\b\t\b" +
            "\x4\t\t\t\x4\n\t\n\x4\v\t\v\x4\f\t\f\x4\r\t\r\x4\xE\t\xE\x4\xF\t\xF\x4" +
            "\x10\t\x10\x4\x11\t\x11\x4\x12\t\x12\x4\x13\t\x13\x4\x14\t\x14\x4\x15" +
            "\t\x15\x3\x2\x3\x2\x3\x2\x3\x2\x3\x3\x3\x3\x3\x3\x3\x3\x3\x4\x3\x4\x3" +
            "\x4\x3\x4\x3\x5\x3\x5\x3\x5\x3\x5\x3\x6\x3\x6\x3\x6\x3\x6\x3\x6\x3\x6" +
            "\x3\x6\x3\x6\x3\a\x3\a\a\a\x46\n\a\f\a\xE\aI\v\a\x3\a\x3\a\x3\b\x3\b\a" +
            "\bO\n\b\f\b\xE\bR\v\b\x3\b\x3\b\x3\b\x3\b\x3\t\x3\t\x3\t\a\t[\n\t\f\t" +
            "\xE\t^\v\t\x3\t\x3\t\x3\n\x6\n\x63\n\n\r\n\xE\n\x64\x3\n\x3\n\x3\v\x3" +
            "\v\x3\v\x3\v\x3\v\x3\v\a\vo\n\v\f\v\xE\vr\v\v\x3\v\x3\v\x3\f\x6\fw\n\f" +
            "\r\f\xE\fx\x3\r\x3\r\x3\xE\x3\xE\x3\xF\x3\xF\x3\x10\x3\x10\x3\x11\x3\x11" +
            "\x3\x12\x3\x12\x3\x13\x3\x13\x6\x13\x89\n\x13\r\x13\xE\x13\x8A\x3\x14" +
            "\x3\x14\x6\x14\x8F\n\x14\r\x14\xE\x14\x90\x3\x15\x3\x15\x2\x2\x16\x3\x3" +
            "\x5\x4\a\x5\t\x6\v\a\r\b\xF\t\x11\n\x13\v\x15\f\x17\r\x19\xE\x1B\xF\x1D" +
            "\x10\x1F\x11!\x12#\x13%\x14\'\x15)\x16\x3\x2\t\x4\x2\f\f\xF\xF\x3\x2\x7F" +
            "\x7F\x5\x2\v\f\xF\xF\"\"\x4\x2$$^^\x3\x2\x32;\x5\x2\x32;\x43\\\x63|\t" +
            "\x2--//\x32;>>@@\x43\\\x63|\x9D\x2\x3\x3\x2\x2\x2\x2\x5\x3\x2\x2\x2\x2" +
            "\a\x3\x2\x2\x2\x2\t\x3\x2\x2\x2\x2\v\x3\x2\x2\x2\x2\r\x3\x2\x2\x2\x2\xF" +
            "\x3\x2\x2\x2\x2\x11\x3\x2\x2\x2\x2\x13\x3\x2\x2\x2\x2\x15\x3\x2\x2\x2" +
            "\x2\x17\x3\x2\x2\x2\x2\x19\x3\x2\x2\x2\x2\x1B\x3\x2\x2\x2\x2\x1D\x3\x2" +
            "\x2\x2\x2\x1F\x3\x2\x2\x2\x2!\x3\x2\x2\x2\x2#\x3\x2\x2\x2\x2%\x3\x2\x2" +
            "\x2\x2\'\x3\x2\x2\x2\x2)\x3\x2\x2\x2\x3+\x3\x2\x2\x2\x5/\x3\x2\x2\x2\a" +
            "\x33\x3\x2\x2\x2\t\x37\x3\x2\x2\x2\v;\x3\x2\x2\x2\r\x43\x3\x2\x2\x2\xF" +
            "L\x3\x2\x2\x2\x11W\x3\x2\x2\x2\x13\x62\x3\x2\x2\x2\x15h\x3\x2\x2\x2\x17" +
            "v\x3\x2\x2\x2\x19z\x3\x2\x2\x2\x1B|\x3\x2\x2\x2\x1D~\x3\x2\x2\x2\x1F\x80" +
            "\x3\x2\x2\x2!\x82\x3\x2\x2\x2#\x84\x3\x2\x2\x2%\x86\x3\x2\x2\x2\'\x8C" +
            "\x3\x2\x2\x2)\x92\x3\x2\x2\x2+,\aT\x2\x2,-\a/\x2\x2-.\a\x32\x2\x2.\x4" +
            "\x3\x2\x2\x2/\x30\aH\x2\x2\x30\x31\a/\x2\x2\x31\x32\a\x32\x2\x2\x32\x6" +
            "\x3\x2\x2\x2\x33\x34\a\x32\x2\x2\x34\x35\a/\x2\x2\x35\x36\aT\x2\x2\x36" +
            "\b\x3\x2\x2\x2\x37\x38\a\x32\x2\x2\x38\x39\a/\x2\x2\x39:\aH\x2\x2:\n\x3" +
            "\x2\x2\x2;<\a\x33\x2\x2<=\a\x31\x2\x2=>\a\x34\x2\x2>?\a/\x2\x2?@\a\x33" +
            "\x2\x2@\x41\a\x31\x2\x2\x41\x42\a\x34\x2\x2\x42\f\x3\x2\x2\x2\x43G\a=" +
            "\x2\x2\x44\x46\n\x2\x2\x2\x45\x44\x3\x2\x2\x2\x46I\x3\x2\x2\x2G\x45\x3" +
            "\x2\x2\x2GH\x3\x2\x2\x2HJ\x3\x2\x2\x2IG\x3\x2\x2\x2JK\b\a\x2\x2K\xE\x3" +
            "\x2\x2\x2LP\a}\x2\x2MO\n\x3\x2\x2NM\x3\x2\x2\x2OR\x3\x2\x2\x2PN\x3\x2" +
            "\x2\x2PQ\x3\x2\x2\x2QS\x3\x2\x2\x2RP\x3\x2\x2\x2ST\a\x7F\x2\x2TU\x3\x2" +
            "\x2\x2UV\b\b\x2\x2V\x10\x3\x2\x2\x2WX\x6\t\x2\x2X\\\a\'\x2\x2Y[\n\x2\x2" +
            "\x2ZY\x3\x2\x2\x2[^\x3\x2\x2\x2\\Z\x3\x2\x2\x2\\]\x3\x2\x2\x2]_\x3\x2" +
            "\x2\x2^\\\x3\x2\x2\x2_`\b\t\x2\x2`\x12\x3\x2\x2\x2\x61\x63\t\x4\x2\x2" +
            "\x62\x61\x3\x2\x2\x2\x63\x64\x3\x2\x2\x2\x64\x62\x3\x2\x2\x2\x64\x65\x3" +
            "\x2\x2\x2\x65\x66\x3\x2\x2\x2\x66g\b\n\x2\x2g\x14\x3\x2\x2\x2hp\a$\x2" +
            "\x2ij\a^\x2\x2jo\a^\x2\x2kl\a^\x2\x2lo\a$\x2\x2mo\n\x5\x2\x2ni\x3\x2\x2" +
            "\x2nk\x3\x2\x2\x2nm\x3\x2\x2\x2or\x3\x2\x2\x2pn\x3\x2\x2\x2pq\x3\x2\x2" +
            "\x2qs\x3\x2\x2\x2rp\x3\x2\x2\x2st\a$\x2\x2t\x16\x3\x2\x2\x2uw\t\x6\x2" +
            "\x2vu\x3\x2\x2\x2wx\x3\x2\x2\x2xv\x3\x2\x2\x2xy\x3\x2\x2\x2y\x18\x3\x2" +
            "\x2\x2z{\a\x30\x2\x2{\x1A\x3\x2\x2\x2|}\a,\x2\x2}\x1C\x3\x2\x2\x2~\x7F" +
            "\a]\x2\x2\x7F\x1E\x3\x2\x2\x2\x80\x81\a_\x2\x2\x81 \x3\x2\x2\x2\x82\x83" +
            "\a*\x2\x2\x83\"\x3\x2\x2\x2\x84\x85\a+\x2\x2\x85$\x3\x2\x2\x2\x86\x88" +
            "\a&\x2\x2\x87\x89\t\x6\x2\x2\x88\x87\x3\x2\x2\x2\x89\x8A\x3\x2\x2\x2\x8A" +
            "\x88\x3\x2\x2\x2\x8A\x8B\x3\x2\x2\x2\x8B&\x3\x2\x2\x2\x8C\x8E\t\a\x2\x2" +
            "\x8D\x8F\t\b\x2\x2\x8E\x8D\x3\x2\x2\x2\x8F\x90\x3\x2\x2\x2\x90\x8E\x3" +
            "\x2\x2\x2\x90\x91\x3\x2\x2\x2\x91(\x3\x2\x2\x2\x92\x93\v\x2\x2\x2\x93" +
            "*\x3\x2\x2\x2\f\x2GP\\\x64npx\x8A\x90\x3\b\x2\x2";
        public static readonly ATN _ATN =
            new ATNDeserializer().Deserialize(_serializedATN.ToCharArray());
    }

}
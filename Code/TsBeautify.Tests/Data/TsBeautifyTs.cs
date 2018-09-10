namespace TsBeautify.Data
{
    public class TsBeautifyTs
    {
        public const string Code = @"import { __TsBeautify_Types_typeLoaded, __TsBeautify_Types_defer } from ""./TsBeautify.Types.Defer"";
import { PropertyInfo, StringBuilder, TypeInfo, Coalesce, Type, Interface, Enumerable, StringUtil, Serialization } from ""@brandless/tsutility"";
import { TsBeautifyOptions } from ""./TsBeautifyOptions"";
import { StringExtensions } from ""./Extensions/StringExtensions"";
export class TsBeautifierInstance {





    public static FunctionsDeclared(): Array<string> {

        return new Array<string>(`PrintNewLineOrSpace`, `get_InGenerics`, `TrimOutput`, `PrintNewLine`, `PrintSpace`, `PrintToken`, `Indent`, `Unindent`, `RemoveIndent`, `SetMode`, `RestoreMode`, `IsTernaryOperation`, `GetNextToken`, `At`, `Beautify`);

    }

    public static PropertiesDeclared(): Array<PropertyInfo> {

        return new Array<PropertyInfo>(new PropertyInfo(`_digits`, TsBeautifierInstance, null, false),

            new PropertyInfo(`_genericsBrackets`, TsBeautifierInstance, null, false),

            new PropertyInfo(`_indentString`, TsBeautifierInstance, null, false),

            new PropertyInfo(`_input`, TsBeautifierInstance, null, false),

            new PropertyInfo(`_lastText`, TsBeautifierInstance, null, false),

            new PropertyInfo(`_lastType`, TsBeautifierInstance, null, false),

            new PropertyInfo(`_modes`, TsBeautifierInstance, null, false),

            new PropertyInfo(`_optPreserveNewlines`, TsBeautifierInstance, null, false),

            new PropertyInfo(`_output`, TsBeautifierInstance, null, false),

            new PropertyInfo(`_parserPos`, TsBeautifierInstance, null, false),

            new PropertyInfo(`_punct`, TsBeautifierInstance, null, false),

            new PropertyInfo(`_tokenText`, TsBeautifierInstance, null, false),

            new PropertyInfo(`_whitespace`, TsBeautifierInstance, null, false),

            new PropertyInfo(`_wordchar`, TsBeautifierInstance, null, false),

            new PropertyInfo(`_currentMode`, TsBeautifierInstance, null, false),

            new PropertyInfo(`_doBlockJustClosed`, TsBeautifierInstance, null, false),

            new PropertyInfo(`_generics`, TsBeautifierInstance, null, false),

            new PropertyInfo(`_genericsDepth`, TsBeautifierInstance, null, false),

            new PropertyInfo(`_ifLineFlag`, TsBeautifierInstance, null, false),

            new PropertyInfo(`_indentLevel`, TsBeautifierInstance, null, false),

            new PropertyInfo(`_options`, TsBeautifierInstance, null, false),

            new PropertyInfo(`_isImportBlock`, TsBeautifierInstance, null, false),

            new PropertyInfo(`InGenerics`, TsBeautifierInstance, null, false));

    }

    public static ClassName: string = `TsBeautifierInstance`;



    private _digits: string;



    private _genericsBrackets: string;



    private _indentString: string;



    private _input: string;



    private _lastText: string;



    private _lastType: string;





    private _modes: Array<string>;



    private _optPreserveNewlines: boolean = false;



    private _output: StringBuilder;



    private _parserPos: number = 0;



    private _punct: string[];



    private _tokenText: string;





    private _whitespace: string;



    private _wordchar: string;



    private _currentMode: string;



    private _doBlockJustClosed: boolean = false;



    private _generics: string;



    private _genericsDepth: number = 0;



    private _ifLineFlag: boolean = false;



    private _indentLevel: number = 0;



    private _options: TsBeautifyOptions;



    private _isImportBlock: boolean = false;





    constructor(jsSourceText: string | null, options: TsBeautifyOptions | null = null, interpolation: boolean = false) {





        options = Coalesce<TsBeautifyOptions>(options, () => new TsBeautifyOptions());



        this._options = options;



        let optIndentSize = Coalesce<number>(options.IndentSize, () => 4);



        let optIndentChar = Coalesce<number>(options.IndentChar, () => ' '.charCodeAt(0));



        let optIndentLevel = Coalesce<number>(options.IndentLevel, () => 0);



        this._optPreserveNewlines = Coalesce<boolean>(options.PreserveNewlines, () => true);



        this._output = new StringBuilder();



        this._modes = new Array<string>();





        this._indentString = ``;





        while (optIndentSize > 0) {



            this._indentString += String.fromCharCode(optIndentChar);



            optIndentSize -= 1;



        }





        this._indentLevel = optIndentLevel;







        this._input = jsSourceText;





        let lastWord = ``;



        this._lastType = `TK_START_EXPR`;// last token type



        this._lastText = ``;// last token text





        this._doBlockJustClosed = false;



        let varLine = false;



        let varLineTainted = false;





        this._whitespace = `\n\r\t `;



        this._wordchar = `abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_$`;



        this._digits = `0123456789`;



        this._genericsBrackets = `<>`;



        this._generics = this._whitespace + this._wordchar + this._digits + `,` + this._genericsBrackets;



        // <!-- is a special case (ok, it's a minor hack actually)

        this._punct = `=> + - * / % & ++ -- = += -= *= /= %= == === != !== > < >= <= >> << >>> >>>= >>= <<= && &= | || ?? ! !! , : ? ^ ^= |= ::`

            .split(' ');





        // words which should always start on new line.

        let lineStarters = `@test,import,let,continue,try,throw,return,var,if,switch,case,default,for,while,break,function`.split(',');





        // states showing if we are currently in expression (i.e. ""if"" case) - 'EXPRESSION', or in usual block (like, procedure), 'BLOCK'.

        // some formatting depends on that.

        this._currentMode = `BLOCK`;



        this._modes.push(<string>(this._currentMode));





        this._parserPos = 0;



        let inCase = false;



        let genericsDepth = 0;





        while (true) {



            let t = this.GetNextToken((value?: number): number => {

                if (value !== undefined) {

                    this._parserPos = value;

                }

                return this._parserPos;

            });



            this._tokenText = t.Value;



            let tokenType = t.TokenType;



            if (tokenType == `TK_END_BLOCK` && interpolation && this._indentLevel == 1) {



                return;



            }





            if (tokenType == `TK_EOF`) {



                break;



            }





            switch (tokenType) {

                case `TK_START_EXPR`:



                    varLine = false;



                    this.SetMode(<string>(`EXPRESSION`));



                    if (this._lastText == `;` || this._lastType == `TK_START_BLOCK`) {



                        this.PrintNewLine();



                    }
                    else if (this._lastType == `TK_END_EXPR` || this._lastType == `TK_START_EXPR`) {



                        // do nothing on (( and )( and ][ and ]( ..



                    }
                    else if (this._lastType != `TK_WORD` && this._lastType != `TK_OPERATOR` && this._lastType != `TK_GENERICS`) {



                        this.PrintSpace();



                    }
                    else if ((<boolean>(Enumerable.Contains(lineStarters, <string>(lastWord))))) {



                        this.PrintSpace();



                    }





                    this.PrintToken();



                    break;





                case `TK_END_EXPR`:



                    this.PrintToken();



                    this.RestoreMode();



                    break;





                case `TK_START_IMPORT`:



                case `TK_END_IMPORT`:



                    this.PrintSpace();



                    this._output.Append(<string>(t.Value));



                    this.PrintSpace();



                    break;



                case `TK_START_BLOCK`:





                    if (lastWord == `do`) {



                        this.SetMode(<string>(`DO_BLOCK`));



                    }
                    else {



                        this.SetMode(<string>(`BLOCK`));



                    }





                    if (this._lastType != `TK_OPERATOR` && this._lastType != `TK_START_EXPR`) {



                        if (this._lastType == `TK_START_BLOCK`) {



                            this.PrintNewLine();



                        }
                        else {



                            if (options.OpenBlockOnNewLine) {



                                this.PrintNewLine();



                            }
                            else {



                                this.PrintSpace();



                            }



                        }



                    }





                    this.PrintToken();



                    this.Indent();



                    break;





                case `TK_END_BLOCK`:



                    if (this._lastType == `TK_START_BLOCK`) {



                        // nothing

                        this.TrimOutput();



                        this.Unindent();



                    }
                    else {



                        this.Unindent();



                        this.PrintNewLine();



                    }





                    this.PrintToken();



                    this.RestoreMode();



                    break;





                case `TK_WORD`:





                    if (this._doBlockJustClosed) {



                        // do {} ## while ()

                        this.PrintSpace();



                        this.PrintToken();



                        this.PrintSpace();



                        this._doBlockJustClosed = false;



                        break;



                    }





                    if ((this._tokenText == `extends` || this._tokenText == `:`) && this._lastText == `>`) {



                        this.PrintSpace();



                    }





                    if (this._tokenText == `case` || this._tokenText == `default`) {



                        if (this._lastText == `:`) {



                            // switch cases following one another

                            this.RemoveIndent();



                        }
                        else {



                            // case statement starts in the same line where switch

                            this.Unindent();



                            this.PrintNewLine();



                            this.Indent();



                        }





                        this.PrintToken();



                        inCase = true;



                        break;



                    }





                    let prefix = `NONE`;





                    if (this._lastType == `TK_END_BLOCK`) {



                        if (!(<boolean>(Enumerable.Contains((<Array<string>>[`else`, `catch`, `finally`]), (<string>(this._tokenText.toLowerCase())))))) {



                            prefix = `NEWLINE`;



                        }
                        else {



                            prefix = `SPACE`;



                            this.PrintSpace();



                        }



                    }
                    else if (this._lastType == `TK_SEMICOLON` && (this._currentMode == `BLOCK` || this._currentMode == `DO_BLOCK`)) {



                        prefix = `NEWLINE`;



                    }
                    else if (this._lastType == `TK_SEMICOLON` && this._currentMode == `EXPRESSION`) {



                        prefix = `SPACE`;



                    }
                    else if (this._lastType == `TK_STRING`) {



                        prefix = `NEWLINE`;



                    }
                    else if (this._lastType == `TK_WORD`) {



                        prefix = `SPACE`;



                    }
                    else if (this._lastType == `TK_START_BLOCK`) {



                        prefix = `NEWLINE`;



                    }
                    else if (this._lastType == `TK_END_EXPR`) {



                        this.PrintSpace();



                        prefix = `NEWLINE`;



                    }





                    if (this._lastType != `TK_END_BLOCK` && (<boolean>(Enumerable.Contains((<Array<string>>[`else`, `catch`, `finally`]), (<string>(this._tokenText.toLowerCase())))))) {



                        this.PrintNewLine();



                    }
                    else if ((<boolean>(Enumerable.Contains(lineStarters, <string>(this._tokenText)))) || prefix == `NEWLINE`) {



                        if (this._lastText == `else`) {



                            // no need to force newline on else break

                            this.PrintSpace();



                        }
                        else if ((this._lastType == `TK_START_EXPR` || this._lastText == `=` || this._lastText == `,`) && this._tokenText == `function`) {



                            // no need to force newline on ""function"": (function

                            // DONOTHING



                        }
                        else if (this._lastType == `TK_WORD` && (this._lastText == `return` || this._lastText == `throw`)) {



                            // no newline between ""return nnn""

                            this.PrintSpace();



                        }
                        else if (this._lastType != `TK_END_EXPR`) {



                            if ((this._lastType != `TK_START_EXPR` || this._tokenText != `var`) && this._lastText != `:`) {



                                if (this._tokenText == `if` && this._lastType == `TK_WORD` && lastWord == `else`) {



                                    this.PrintSpace();



                                }
                                else {



                                    this.PrintNewLine();



                                }



                            }



                        }
                        else {



                            if ((<boolean>(Enumerable.Contains(lineStarters, <string>(this._tokenText)))) && this._lastText != `)`) {



                                this.PrintNewLine();



                            }



                        }



                    }
                    else if (prefix == `SPACE`) {



                        this.PrintSpace();



                    }





                    this.PrintToken();



                    lastWord = this._tokenText;





                    if (this._tokenText == `var`) {



                        varLine = true;



                        varLineTainted = false;



                    }





                    if (this._tokenText == `if` || this._tokenText == `else`) {



                        this._ifLineFlag = true;



                    }





                    break;





                case `TK_SEMICOLON`:





                    this.PrintToken();



                    varLine = false;



                    break;





                case `TK_STRING`:





                    if (this._lastType == `TK_START_BLOCK` || this._lastType == `TK_END_BLOCK` || this._lastType == `TK_SEMICOLON`) {



                        this.PrintNewLine();



                    }
                    else if (this._lastType == `TK_WORD` && this._lastText != `$`) {



                        this.PrintSpace();



                    }





                    this.PrintToken();



                    break;



                case `TK_GENERICS`:



                    if (t.Value == `<`) {



                        if (genericsDepth == 0) {



                            if (this._lastType == `TK_WORD` && (this._lastText == `return`)) {



                                this.PrintSpace();



                            }



                        }



                        this._output.Append(<string>(t.Value));



                        genericsDepth++;



                    }
                    else {



                        this._output.Append(<string>(t.Value));



                        genericsDepth--;



                    }



                    break;



                case `TK_OPERATOR`:





                    let startDelim = true;



                    let endDelim = true;



                    if (varLine && this._tokenText != `,`) {



                        varLineTainted = true;



                        if (this._tokenText == `:`) {



                            varLine = false;



                        }



                    }





                    if (varLine && this._tokenText == `,` && this._currentMode == `EXPRESSION`) {



                        varLineTainted = false;



                    }





                    if (this._tokenText == `:` && inCase) {



                        this.PrintToken();// colon really asks for separate treatment



                        this.PrintNewLine();



                        inCase = false;



                        break;



                    }





                    if (this._tokenText == `::`) {



                        // no spaces around exotic namespacing syntax operator

                        this.PrintToken();



                        break;



                    }





                    if (this._tokenText == `,`) {



                        if (varLine) {



                            if (varLineTainted) {



                                this.PrintToken();



                                this.PrintNewLine();



                                varLineTainted = false;



                            }
                            else {



                                this.PrintToken();



                                this.PrintSpace();



                            }



                        }
                        else if (this._lastType == `TK_END_BLOCK`) {



                            this.PrintToken();



                            this.PrintNewLine();



                        }
                        else {



                            if (this._currentMode == `BLOCK` && !this._isImportBlock) {



                                this.PrintToken();



                                if (genericsDepth > 0) {



                                    this.PrintSpace();



                                }
                                else {



                                    this.PrintNewLine();



                                }



                            }
                            else {



                                // EXPR od DO_BLOCK

                                this.PrintToken();



                                this.PrintSpace();



                            }



                        }





                        break;



                    }
                    else if (this._tokenText == `--` || this._tokenText == `++`) {



                        // unary operators special case

                        if (this._lastText == `;`) {



                            if (this._currentMode == `BLOCK`) {



                                // { foo; --i }

                                this.PrintNewLine();



                                startDelim = true;



                                endDelim = false;



                            }
                            else {



                                // space for (;; ++i)

                                startDelim = true;



                                endDelim = false;



                            }



                        }
                        else {



                            if (this._lastText == `{`) {



                                this.PrintNewLine();



                            }





                            startDelim = false;



                            endDelim = false;



                        }



                    }
                    else if ((this._tokenText == `!` || this._tokenText == `+` || this._tokenText == `-`) && (this._lastText == `return` || this._lastText == `case`)) {



                        startDelim = true;



                        endDelim = false;



                    }
                    else if ((this._tokenText == `!` || this._tokenText == `+` || this._tokenText == `-`) && this._lastType == `TK_START_EXPR`) {



                        // special case handling: if (!a)

                        startDelim = false;



                        endDelim = false;



                    }
                    else if (this._lastType == `TK_OPERATOR`) {



                        startDelim = false;



                        endDelim = false;



                    }
                    else if (this._lastType == `TK_END_EXPR`) {



                        startDelim = true;



                        endDelim = true;



                    }
                    else if (this._tokenText == `.`) {



                        // decimal digits or object.property

                        startDelim = false;



                        endDelim = false;



                    }
                    else if (this._tokenText == `:`) {



                        if ((<boolean>(this.IsTernaryOperation()))) {



                            startDelim = true;



                        }
                        else {



                            startDelim = false;



                        }



                    }





                    if (startDelim) {



                        this.PrintSpace();



                    }





                    this.PrintToken();



                    if (endDelim) {



                        this.PrintSpace();



                    }





                    break;





                case `TK_BLOCK_COMMENT`:



                    this.PrintNewLineOrSpace(t);



                    this.PrintToken();



                    this.PrintNewLineOrSpace(t);



                    break;





                case `TK_COMMENT`:





                    // print_newline();

                    if (this._lastType == `TK_START_BLOCK`) {



                        this.PrintNewLine();



                    }
                    else {



                        this.PrintNewLineOrSpace(t);



                    }



                    this.PrintToken();



                    this.PrintNewLine();



                    break;





                case `TK_UNKNOWN`:



                    this.PrintToken();



                    break;



            }





            this._lastType = tokenType;



            this._lastText = this._tokenText;



        }



    }



    private PrintNewLineOrSpace(t: Token | null): boolean {



        if (t.NewLineCount > 0) {



            this.PrintNewLine();



            return (<boolean>(true));



        }
        else {



            this.PrintSpace();



        }



        return (<boolean>(false));



    }







    private get InGenerics(): boolean {

        return this.TsBeautifierInstance_InGenericsGetter();

    }

    protected TsBeautifierInstance_InGenericsGetter(): boolean {

        return this._genericsDepth > 0;

    }









    private TrimOutput(): void {



        while (this._output.Length > 0 && (this._output.charCodeAt(<number>(this._output.Length - 1)) == ' '.charCodeAt(0) || (<string>(String.fromCharCode(this._output.charCodeAt(<number>(this._output.Length - 1))))) == this._indentString)) {



            this._output.Remove(<number>(this._output.Length - 1), <number>(1));



        }



    }





    private PrintNewLine(ignoreRepeated: boolean | null = null): void {



        ignoreRepeated = Coalesce<boolean>(ignoreRepeated, () => true);





        this._ifLineFlag = false;



        this.TrimOutput();





        if (this._output.Length == 0) {



            return;



        }





        if (this._output.charCodeAt(<number>(this._output.Length - 1)) != '\n'.charCodeAt(0) || !ignoreRepeated) {



            this._output.Append(<string>(`
`));



        }





        for (let i = 0; i < this._indentLevel; i++) {



            this._output.Append(<string>(this._indentString));



        }



    }





    private PrintSpace(): void {



        let lastOutput = ` `;



        if (this._output.Length > 0) {



            lastOutput = (<string>(String.fromCharCode(this._output.charCodeAt(<number>(this._output.Length - 1)))));



        }





        if (lastOutput != ` ` && lastOutput != `\n` && lastOutput != this._indentString) {



            this._output.Append(' '.charCodeAt(0));



        }



    }







    private PrintToken(): void {



        this._output.Append(<string>(this._tokenText));



    }





    private Indent(): void {



        this._indentLevel++;



    }





    private Unindent(): void {



        if (this._indentLevel > 0) {



            this._indentLevel--;



        }



    }





    private RemoveIndent(): void {



        if (this._output.Length > 0 && (<string>(String.fromCharCode(this._output.charCodeAt(<number>(this._output.Length - 1))))) == this._indentString) {



            this._output.Remove(<number>(this._output.Length - 1), <number>(1));



        }



    }





    private SetMode(mode: string | null): void {



        this._modes.push(<string>(this._currentMode));



        this._currentMode = mode;



    }





    private RestoreMode(): void {



        this._doBlockJustClosed = this._currentMode == `DO_BLOCK`;



        this._currentMode = (<string>(this._modes.pop()));



    }





    private IsTernaryOperation(): boolean {



        let level = 0;



        let colonCount = 0;



        for (let i = this._output.Length - 1; i >= 0; i--) {



            switch (this._output.charCodeAt(<number>(i))) {

                case ':'.charCodeAt(0):



                    if (level == 0) {



                        colonCount++;



                    }





                    break;



                case '?'.charCodeAt(0):



                    if (level == 0) {



                        if (colonCount == 0) {



                            return (<boolean>(true));



                        }





                        colonCount--;



                    }





                    break;



                case '{'.charCodeAt(0):



                    if (level == 0) {



                        return (<boolean>(false));



                    }





                    level--;



                    break;



                case '('.charCodeAt(0):



                case '['.charCodeAt(0):



                    level--;



                    break;



                case ')'.charCodeAt(0):



                case ']'.charCodeAt(0):



                case '}'.charCodeAt(0):



                    level++;



                    break;



            }



        }





        return (<boolean>(false));



    }





    private GetNextToken(parserPos: (value?: number) => number): Token | null {



        let newLineCount = 0;





        if (parserPos() >= this._input.length) {



            return new Token(<string>(``), <string>(`TK_EOF`), <number>(newLineCount));



        }





        let c = (<string>(String.fromCharCode(this._input.charCodeAt(<number>(parserPos())))));



        parserPos(parserPos() + 1);





        while ((<boolean>((this._whitespace.indexOf(<string>(c)) != -1)))) {



            if (parserPos() >= this._input.length) {



                return new Token(<string>(``), <string>(`TK_EOF`), <number>(newLineCount));



            }





            if (c == `\n`) {



                newLineCount++;



            }





            c = (<string>(String.fromCharCode(this._input.charCodeAt(<number>(parserPos())))));



            parserPos(parserPos() + 1);



        }





        let wantedNewline = false;





        if (this._optPreserveNewlines) {



            if (newLineCount > 1) {



                for (let i = 0; i < 2; i++) {



                    this.PrintNewLine(i == 0);



                }



            }





            wantedNewline = newLineCount == 1;



        }





        let isGenerics = false;



        if (c == `<`) {



            if (this.InGenerics) {



                this._genericsDepth++;



                isGenerics = true;



            }
            else {



                // Check for the start of generics

                for (let i = this._parserPos + 1; i < this._input.length; i++) {



                    let cSub = this._input.charCodeAt(<number>(i));



                    if (cSub == '>'.charCodeAt(0)) {



                        isGenerics = true;



                        break;



                    }





                    if (!(<boolean>((this._generics.indexOf(StringUtil.EnsureStringFromStringOrCharCode(cSub)) != -1)))) {



                        break;



                    }



                }





                if (isGenerics) {



                    this._genericsDepth++;



                }



            }



        }
        else if (c == `>` && this._genericsDepth > 0) {



            isGenerics = true;



            this._genericsDepth--;



        }





        if (isGenerics && (<boolean>((this._genericsBrackets.indexOf(<string>(c)) != -1)))) {



            return new Token(<string>(c), <string>(`TK_GENERICS`), <number>(newLineCount));



        }







        if ((<boolean>((this._wordchar.indexOf(<string>(c)) != -1)))) {



            if (parserPos() < this._input.length) {



                while ((<boolean>((this._wordchar.indexOf(StringUtil.EnsureStringFromStringOrCharCode(this._input.charCodeAt(<number>(parserPos())))) != -1)))) {



                    c += String.fromCharCode(this._input.charCodeAt(<number>(parserPos())));



                    parserPos(parserPos() + 1);



                    if (parserPos() == this._input.length) {



                        break;



                    }



                }



            }







            if (parserPos() != this._input.length && (<boolean>(new RegExp(<string>(`^[0-9]+[Ee]$`)).test(<string>(c)))) && (this._input.charCodeAt(<number>(parserPos())) == '-'.charCodeAt(0) || this._input.charCodeAt(<number>(parserPos())) == '+'.charCodeAt(0))) {



                let sign = this._input.charCodeAt(<number>(parserPos()));



                parserPos(parserPos() + 1);





                let t = this.GetNextToken((value?: number): number => {

                    if (value !== undefined) {

                        parserPos(value);

                    }

                    return parserPos();

                });



                c += String.fromCharCode(sign) + t.Value;



                return new Token(<string>(c), <string>(`TK_WORD`), <number>(newLineCount));



            }





            if (c == `in`) {



                return new Token(<string>(c), <string>(`TK_OPERATOR`), <number>(newLineCount));



            }





            if (wantedNewline && this._lastType != `TK_OPERATOR` && !this._ifLineFlag) {



                this.PrintNewLine();



            }





            return new Token(<string>(c), <string>(`TK_WORD`), <number>(newLineCount));



        }





        if (c == `(` || c == `[`) {



            return new Token(<string>(c), <string>(`TK_START_EXPR`), <number>(newLineCount));



        }





        if (c == `)` || c == `]`) {



            return new Token(<string>(c), <string>(`TK_END_EXPR`), <number>(newLineCount));



        }





        if (c == `{`) {



            if (this._lastText == `import`) {



                this._isImportBlock = true;



                return new Token(<string>(c), <string>(`TK_START_IMPORT`), <number>(newLineCount));



            }



            return new Token(<string>(c), <string>(`TK_START_BLOCK`), <number>(newLineCount));



        }





        if (c == `}`) {



            if (this._isImportBlock) {



                this._isImportBlock = false;



                return new Token(<string>(c), <string>(`TK_END_IMPORT`), <number>(newLineCount));



            }



            return new Token(<string>(c), <string>(`TK_END_BLOCK`), <number>(newLineCount));



        }





        if (c == `;`) {



            return new Token(<string>(c), <string>(`TK_SEMICOLON`), <number>(newLineCount));



        }





        if (c == `/`) {



            let comment = ``;



            if (this._input.charCodeAt(<number>(parserPos())) == '*'.charCodeAt(0)) {



                parserPos(parserPos() + 1);



                if (parserPos() < this._input.length) {



                    while (!(this._input.charCodeAt(<number>(parserPos())) == '*'.charCodeAt(0) && this._input.charCodeAt(<number>(parserPos() + 1)) > '\0'.charCodeAt(0) && this._input.charCodeAt(<number>(parserPos() + 1)) == '/'.charCodeAt(0) && parserPos() < this._input.length)) {



                        comment += String.fromCharCode(this._input.charCodeAt(<number>(parserPos())));



                        parserPos(parserPos() + 1);



                        if (parserPos() >= this._input.length) {



                            break;



                        }



                    }



                }





                parserPos(parserPos() + 2);



                return new Token(<string>(`/*` + comment + `*/`), <string>(`TK_BLOCK_COMMENT`), <number>(newLineCount));



            }





            if (this._input.charCodeAt(<number>(parserPos())) == '/'.charCodeAt(0)) {



                comment = c;



                while (this._input.charCodeAt(<number>(parserPos())) != '\x0d'.charCodeAt(0) && this._input.charCodeAt(<number>(parserPos())) != '\x0a'.charCodeAt(0)) {



                    comment += String.fromCharCode(this._input.charCodeAt(<number>(parserPos())));



                    parserPos(parserPos() + 1);



                    if (parserPos() >= this._input.length) {



                        break;



                    }



                }





                parserPos(parserPos() + 1);



                if (wantedNewline) {



                    this.PrintNewLine();



                }





                return new Token(<string>(comment), <string>(`TK_COMMENT`), <number>(newLineCount));



            }



        }





        let interpolationAllowed: StringInterpolationKind = StringInterpolationKind.None;



        // Allow C# interpolated strings

        if (c == `$` || this._lastText == `$`) {



            let lookahead = c == `$` ? 1 : 0;



            let next = (<string>(this.At(<number>(parserPos() + lookahead))));



            if (next == `@` || next == `\""`) {



                interpolationAllowed = StringInterpolationKind.CSharp;



                if (lookahead == 1) {



                    this._output.Append(<string>(c));



                    c = (<string>(String.fromCharCode(this._input.charCodeAt(<number>(parserPos())))));



                    parserPos(parserPos() + 1);



                }



            }



        }



        if (c == `@` && (<string>(String.fromCharCode(this._input.charCodeAt(<number>(parserPos()))))) == `\""`) {



            this._output.Append(<string>(c));



            c = (<string>(String.fromCharCode(this._input.charCodeAt(<number>(parserPos())))));



            parserPos(parserPos() + 1);



        }



        if ((c == `'` || c == `\""` || c == `/` || c == `\``)

            && ((this._lastType == `TK_WORD` && (this._lastText == `$` || this._lastText == `return` || this._lastText == `from` || this._lastText == `case`)) || this._lastType == `TK_START_EXPR` || this._lastType == `TK_START_BLOCK` || this._lastType == `TK_END_BLOCK` || this._lastType == `TK_OPERATOR` || this._lastType == `TK_EOF` || this._lastType == `TK_SEMICOLON`)

        ) {



            let sep = c;



            let esc = false;



            let resultingString = c;





            if (parserPos() < this._input.length) {



                if (sep == `/`) {



                    let inCharClass = false;



                    while (esc || inCharClass || (<string>(String.fromCharCode(this._input.charCodeAt(<number>(parserPos()))))) != sep) {



                        resultingString += String.fromCharCode(this._input.charCodeAt(<number>(parserPos())));



                        if (!esc) {



                            esc = this._input.charCodeAt(<number>(parserPos())) == '\\'.charCodeAt(0);



                            if (this._input.charCodeAt(<number>(parserPos())) == '['.charCodeAt(0)) {



                                inCharClass = true;



                            }
                            else if (this._input.charCodeAt(<number>(parserPos())) == ']'.charCodeAt(0)) {



                                inCharClass = false;



                            }



                        }
                        else {



                            esc = false;



                        }





                        parserPos(parserPos() + 1);



                        if (parserPos() >= this._input.length) {



                            return new Token(<string>(resultingString), <string>(`TK_STRING`), <number>(newLineCount));



                        }



                    }



                }
                else {



                    if (c == `\``) {



                        interpolationAllowed = StringInterpolationKind.TypeScript;



                    }



                    while (esc || (<string>(String.fromCharCode(this._input.charCodeAt(<number>(parserPos()))))) != sep) {



                        let interpolationActioned = false;



                        switch (interpolationAllowed) {

                            case StringInterpolationKind.CSharp:



                                if ((<boolean>(StringExtensions.StartsWithAt(this._input, <string>(`{`), <number>(parserPos())))) && !(<boolean>(StringExtensions.StartsWithAt(this._input, <string>(`{{`), <number>(parserPos()))))) {



                                    interpolationActioned = true;



                                }



                                break;



                            case StringInterpolationKind.TypeScript:



                                if ((<boolean>(StringExtensions.StartsWithAt(this._input, <string>(`\${`), <number>(parserPos()))))) {



                                    let escapeCount = 0;



                                    for (let i = parserPos() - 1; i > 0; i--) {



                                        if (this._input.charCodeAt(<number>(i)) == '\\'.charCodeAt(0)) {



                                            escapeCount++;



                                        }
                                        else {



                                            break;



                                        }



                                    }





                                    if (escapeCount % 2 == 0) {



                                        // Escaped

                                        interpolationActioned = true;



                                    }



                                }



                                break;



                        }





                        if (interpolationActioned) {



                            switch (interpolationAllowed) {

                                case StringInterpolationKind.CSharp:



                                    {



                                        let jsSourceText = (<string>(this._input.substr(<number>(parserPos()))));



                                        let sub = new TsBeautifierInstance(<string>(jsSourceText), this._options, true);



                                        let interpolated = (<string>((<string>(StringUtil.TrimStart((<string>(sub.Beautify())), '{'.charCodeAt(0)))).trim()));



                                        resultingString += `{${interpolated}}`;



                                        parserPos(parserPos() + sub._parserPos);



                                    }

                                    break;



                                case StringInterpolationKind.TypeScript:



                                    {



                                        let jsSourceText = (<string>(this._input.substr(<number>(parserPos() + 1))));



                                        let sub = new TsBeautifierInstance(<string>(jsSourceText), this._options, true);



                                        let interpolated = (<string>((<string>(StringUtil.TrimStart((<string>(sub.Beautify())), '{'.charCodeAt(0)))).trim()));



                                        resultingString += `\${${interpolated}}`;



                                        parserPos(parserPos() + sub._parserPos + 1);



                                    }

                                    break;



                            }



                        }
                        else {



                            resultingString += String.fromCharCode(this._input.charCodeAt(<number>(parserPos())));



                            if (!esc) {



                                esc = this._input.charCodeAt(<number>(parserPos())) == '\\'.charCodeAt(0);



                            }
                            else {



                                esc = false;



                            }





                            parserPos(parserPos() + 1);



                        }





                        if (parserPos() >= this._input.length) {



                            return new Token(<string>(resultingString), <string>(`TK_STRING`), <number>(newLineCount));



                        }



                    }



                }



            }





            parserPos(parserPos() + 1);





            resultingString += sep;





            if (sep == `/`) {



                while (parserPos() < this._input.length && (<boolean>((this._wordchar.indexOf(StringUtil.EnsureStringFromStringOrCharCode(this._input.charCodeAt(<number>(parserPos())))) != -1)))) {



                    resultingString += String.fromCharCode(this._input.charCodeAt(<number>(parserPos())));



                    parserPos(parserPos() + 1);



                }



            }





            return new Token(<string>(resultingString), <string>(`TK_STRING`), <number>(newLineCount));



        }





        if (c == `#`) {



            let sharp = `#`;



            if (parserPos() < this._input.length && (<boolean>((this._digits.indexOf(StringUtil.EnsureStringFromStringOrCharCode(this._input.charCodeAt(<number>(parserPos())))) != -1)))) {



                do {



                    c = (<string>(String.fromCharCode(this._input.charCodeAt(<number>(parserPos())))));



                    sharp += c;



                    parserPos(parserPos() + 1);



                }

                while (parserPos() < this._input.length && c != `#` && c != `=`);





                if (c == `#`) {



                    return new Token(<string>(sharp), <string>(`TK_WORD`), <number>(newLineCount));



                }





                return new Token(<string>(sharp), <string>(`TK_OPERATOR`), <number>(newLineCount));



            }



        }







        if (c == `<` && (<string>(this._input.substr(<number>(parserPos() - 1), <number>(3)))) == `<!--`) {



            parserPos(parserPos() + 3);



            return new Token(<string>(`<!--`), <string>(`TK_COMMENT`), <number>(newLineCount));



        }





        if (c == `-` && (<string>(this._input.substr(<number>(parserPos() - 1), <number>(2)))) == `-->`) {



            parserPos(parserPos() + 2);



            if (wantedNewline) {



                this.PrintNewLine();



            }





            return new Token(<string>(`-->`), <string>(`TK_COMMENT`), <number>(newLineCount));



        }





        if ((<boolean>(Enumerable.Contains(this._punct, <string>(c))))) {



            while (parserPos() < this._input.length && (<boolean>(Enumerable.Contains(this._punct, <string>(c + String.fromCharCode(this._input.charCodeAt(<number>(parserPos())))))))) {



                c += String.fromCharCode(this._input.charCodeAt(<number>(parserPos())));



                parserPos(parserPos() + 1);



                if (parserPos() >= this._input.length) {



                    break;



                }



            }





            return new Token(<string>(c), <string>(`TK_OPERATOR`), <number>(newLineCount));



        }





        return new Token(<string>(c), <string>(`TK_UNKNOWN`), <number>(newLineCount));



    }





    private At(position: number): string | null {



        if (position > this._input.length - 1) {



            return (<string>(null));



        }





        return (<string>((<string>(String.fromCharCode(this._input.charCodeAt(<number>(position)))))));



    }





    public Beautify(): string | null {



        return (<string>((<string>(this._output.toString()))));



    }



    toJSON(): any {

        return Serialization.PrepareForJson(this, TsBeautifierInstance);

    }

}









export class Token {





    public static FunctionsDeclared(): Array<string> {

        return new Array<string>(`get_Value`, `set_Value`, `get_TokenType`, `set_TokenType`, `get_NewLineCount`, `set_NewLineCount`);

    }

    public static PropertiesDeclared(): Array<PropertyInfo> {

        return new Array<PropertyInfo>(new PropertyInfo(`<Value>k__BackingField`, Token, null, false),

            new PropertyInfo(`Value`, Token, null, true),

            new PropertyInfo(`<TokenType>k__BackingField`, Token, null, false),

            new PropertyInfo(`TokenType`, Token, null, true),

            new PropertyInfo(`<NewLineCount>k__BackingField`, Token, null, false),

            new PropertyInfo(`NewLineCount`, Token, null, true));

    }

    public static ClassName: string = `Token`;



    private _Token_Value: string;

    public get Value(): string {

        return this.Token_ValueGetter();

    }

    protected Token_ValueGetter(): string {

        return this._Token_Value;

    }

    public set Value(value: string) {

        this.Token_ValueSetter(value);

    }

    protected Token_ValueSetter(value: string) {

        this._Token_Value = value;

    }



    private _Token_TokenType: string;

    public get TokenType(): string {

        return this.Token_TokenTypeGetter();

    }

    protected Token_TokenTypeGetter(): string {

        return this._Token_TokenType;

    }

    public set TokenType(value: string) {

        this.Token_TokenTypeSetter(value);

    }

    protected Token_TokenTypeSetter(value: string) {

        this._Token_TokenType = value;

    }



    private _Token_NewLineCount: number = 0;

    public get NewLineCount(): number {

        return this.Token_NewLineCountGetter();

    }

    protected Token_NewLineCountGetter(): number {

        return this._Token_NewLineCount;

    }

    public set NewLineCount(value: number) {

        this.Token_NewLineCountSetter(value);

    }

    protected Token_NewLineCountSetter(value: number) {

        this._Token_NewLineCount = value;

    }





    constructor(token: string | null, tokenType: string | null, newLineCount: number = 0) {





        this.Value = token;



        this.TokenType = tokenType;



        this.NewLineCount = newLineCount;



    }

    toJSON(): any {

        return Serialization.PrepareForJson(this, Token);

    }

}







export enum StringInterpolationKind {

    None = 1,



    CSharp = 2,



    TypeScript = 3

}

__TsBeautify_Types_typeLoaded(`./TsBeautify`);";
    }
}
//namespace TsBeautify.Data
//{
//    public class LargeFile
//    {
//        public static string Inputxx = @"import { PropertyInfo, StringBuilder, TypeInfo, Coalesce, Type, Interface, Enumerable, StringUtil } from ""@brandless/tsutility"";
//import { TsBeautifyOptions } from ""./TsBeautifyOptions"";
//import { StringExtensions } from ""./Extensions/StringExtensions"";
//export class TsBeautifierInstance 

//                {
//constructor() {
//let interpolationAllowed = c == `$\``;

//while (esc || (<string>(this._input.charCodeAt(<number>(parserPos())).toString())) != sep)
// {

//if(interpolationAllowed && (<boolean>(StringExtensions.StartsWithAt(this._input, <string>(`\${`), <number>(parserPos())))))
// {

//let jsSourceText = (<string>(this._input.substr(<number>(parserPos() + 1))));

//let sub = new TsBeautifierInstance (<string>(jsSourceText), this._options, true);

//let interpolated = (<string>((<string>(StringUtil.TrimStart((<string>(sub.Beautify())), '{'.charCodeAt(0)))).trim()));

//resultingString += `\${${interpolated}}`;

//parserPos(parserPos() + sub._parserPos + 1);

//}
// else {

//resultingString += String.fromCharCode(this._input.charCodeAt(<number>(parserPos())));

//if(!esc)
// {

//esc = this._input.charCodeAt(<number>(parserPos())) == '\\'.charCodeAt(0);

//}
// else {

//esc = false;

//}

//parserPos(parserPos() + 1);

//}

//if(parserPos() >= this._input.length)
// {

//return <Array<string>>[resultingString,`TK_STRING`];

//}

//}

//}
//                }";

//        public static string Beautified = @"import { PropertyInfo, StringBuilder, TypeInfo, Coalesce, Type, Interface, Enumerable, StringUtil } from ""@brandless/tsutility"";
//import { TsBeautifyOptions } from ""./TsBeautifyOptions"";
//import { StringExtensions } from ""./Extensions/StringExtensions"";
//export class TsBeautifierInstance {
//    constructor() {
//        let interpolationAllowed = c == `$\``;
//        while (esc || (<string>(this._input.charCodeAt(<number>(parserPos())).toString())) != sep) {
//            if (interpolationAllowed && (<boolean>(StringExtensions.StartsWithAt(this._input, <string>(`\${`), <number>(parserPos()))))) {
//                let jsSourceText = (<string>(this._input.substr(<number>(parserPos() + 1))));
//                let sub = new TsBeautifierInstance(<string>(jsSourceText), this._options, true);
//                let interpolated = (<string>((<string>(StringUtil.TrimStart((<string>(sub.Beautify())), '{'.charCodeAt(0)))).trim()));
//                resultingString += `\${${interpolated}}`;
//                parserPos(parserPos() + sub._parserPos + 1);
//            } else {
//                resultingString += String.fromCharCode(this._input.charCodeAt(<number>(parserPos())));
//                if (!esc) {
//                    esc = this._input.charCodeAt(<number>(parserPos())) == '\\'.charCodeAt(0);
//                } else {
//                    esc = false;
//                }
//                parserPos(parserPos() + 1);
//            }
//            if (parserPos() >= this._input.length) {
//                return<Array<string>>[resultingString, `TK_STRING`];
//            }
//        }
//    }
//}";
//    }
//}
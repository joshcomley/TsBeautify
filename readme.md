# TsBeautify

TsBeautify is an open source TypeScript beautifier written in C# and transpiled to TypeScript. It is highly opinionated and has few options!

## Installation

For usage within .NET:

    nuget install TsBeautify

For usage within a web application:

    npm i @brandless/tsbeautify --save
    // OR
    yarn add @brandless/tsbeautify

## Usage

### C#
    var typescript = @"import   { PropertyInfo ,   Type, Interface, TypeInfo} from ""@brandless/tsutility"";";
    var beautifier = new TsBeautifier();
    var result = beautifier.Beautify(typescript);

### TypeScript

    let  typescript  =  `import { PropertyInfo , Type, Interface, TypeInfo} from "@brandless/tsutility";`;
	let  beautifier  =  new  TsBeautifier();
	let  result  =  beautifier.Beautify(typescript);

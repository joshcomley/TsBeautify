namespace TsBeautify.Data
{
    public class Bug1
    {
        public const string TypeScript = @"//import { Type } from ""./TypeDefinition"";

import { Interface } from ""./Interface"";

import { TypeInfo } from './TypeInfo';

import { Collection } from ""./Collection"";



export class EnumValue<T> {

    constructor(public Name: string, public Value: number, public EnumType: Enum<T>) { }

}

export class EnumValueCollection<T> extends Collection<EnumValue<T>> {

    constructor(public EnumType: Enum<T>) {

        super();

        TypeInfo.SetPrototypeOf(this, EnumValueCollection.prototype);

    }

    public toString(): string {

        return Enum.ConvertToString(this.EnumType, this);

    }

}

type EnumType1 = number | EnumValue<any>;

type EnumType2 = EnumType1 | Array<EnumType1>;

export class Enum<T> extends Function {

    public Entries: Array<EnumValue<T>>;

    constructor(public Values: any) {

        super();

        TypeInfo.SetPrototypeOf(this, Enum.prototype);

        this.Entries = this.ResolveEntries();

    } // IsAssignableFrom<TFrom>(type: Type<TFrom> | Interface<TFrom> | Enum<TFrom> | Function): boolean {

    //     return false;

    // }

    public ResolveEntries(): Array<EnumValue<T>> {

        return Enum.ResolveEnumEntries(this.Values);

    }

    public static ResolveEnumEntries(enumType: any): EnumValueCollection<any> {

        if (enumType instanceof Enum) {

            enumType = enumType.Values;

        }

        let arr = new EnumValueCollection<any>(enumType);

        for (

            let name in enumType) {

            if (Object.prototype.hasOwnProperty.call(enumType, name)) {

                if (/^\d+$/.test(name)) {

                    arr.push(new EnumValue<any>(enumType[name], +name, enumType));

                }

            }

        }

        return arr;

    }

    public get IsEnum(): boolean {

        return true;

    }

    public ConvertToString(value: T): string {

        if (this.HasFlags) {

            let values = this.GetEntries(value);

        }

        return this.Values[value];

    }

    public static ConvertToString<T extends number | EnumValue<any> | Array<EnumValue<any> | number>>(enumType: any, value: T) {

        let values = Enum.Flatten(enumType, value);

        let result = """";

        for (

            let i = 0; i < values.length; i++) {

            result += values[i].Name;

            if (i < values.length - 1) {

                result += "", "";

            }

        }

        return result;

    }

    public ConvertFromString(value: string): T {

        if (!Object.prototype.hasOwnProperty.call(this.Values, value)) {

            throw new Error(""No such enum value"");

        }

        return <T>this.Values[value];

    }

    public TryConvertFromString(value: string): T {

        return <T>this.Values[value];

    }

    public static GetName<T>(type: any, value: T): string {

        if (typeof type === ""function"" && typeof type[""Values""] === ""object"") {

            type = type[""Values""];

        }

        return type[value] as string;

    }

    public static Parse(type: any, value: string): any {

        if (type instanceof Enum) {

            return Enum.Parse(type.Values, value);

        }

        if (Enum.HasFlags(type)) {

            let values = value.split("","");

            let result = 0;

            for (

                let i = 0; i < values.length; i++) {

                let name = (values[i] || """").trim();

                if (name) {

                    result |= type[name];

                }

            }

            return result;

        }

        var result = type[value] as number;

        return result === undefined || result === null ? null : result;

    }

    public static SetHasFlags(enumType: any) {

        Object.defineProperty(enumType, ""hasFlags"", {

            value: true,

            writable: false,

            enumerable: false

        });

    }

    public get HasFlags(): boolean {

        return this.Values[""hasFlags""] === true;

    }

    public static HasFlags(enumType: any) {

        if (enumType instanceof Enum) {

            return enumType.HasFlags;

        }

        return (<any>enumType).hasFlags === true;

    }

    public HasFlag(values: number, flag: number): boolean {

        return Enum.HasFlag(values, flag);

    }

    public static HasFlag(values: number, flag: number): boolean {

        return (values & flag) === flag;

    }

    public static SetFlag(values: number, flag: number) {

        if (!Enum.HasFlag(values, flag)) {

            return values | flag;

        }

        return values;

    }

    public static RemoveFlag(values: number, flag: number) {

        if (Enum.HasFlag(values, flag)) {

            return values & ~flag;

        }

        return values;

    }

    // public GetEntries2<TValue extends T | number | EnumValue<any> | Array<TValue>>(

    //     value: TValue

    // ) {

    //     if (!Array.isArray(value) && !(typeof value === ""number"") && !(value instanceof EnumValue)) {

    //         return Enum.Flatten(this, <number><any>value);

    //     }

    //     let flattened = Enum.Flatten(this, value);

    // }

    public GetEntries(value: T): EnumValueCollection<any> {

        return Enum.GetEntries(this.Values, <number><any>value);

    }

    public static ToObject(enumType: any, value: any) {

        return Enum.GetEntries(enumType, +value);

    } //public static ToString(enumType: any, value: )

    public static Flatten<T extends number | EnumValue<any> | Array<EnumValue<any> | number> | EnumValueCollection<any>>(enumType: any, value: T): EnumValueCollection<any> {

        let all = Enum.FlattenInternal(enumType, value, new EnumValueCollection<any>(enumType));

        let final = new EnumValueCollection<any>(enumType);

        let names = new Array<string>();

        for (

            let i = 0; i < all.length; i++) {

            let entry = all[i];

            if (names.indexOf(entry.Name) === -1) {

                names.push(entry.Name);

                final.push(entry);

            }

        }

        return final;

    }

    private static FlattenInternal<T extends number | EnumValue<any> | Array<EnumValue<any> | number> | EnumValueCollection<any>>(enumType: Enum<any>, value: T, result: EnumValueCollection<any>): EnumValueCollection<any> {

        if (Array.isArray(value) || value instanceof Array) {

            for (

                let i = 0; i < value.length; i++) {

                Enum.FlattenInternal(enumType, value[i], result);

            }

        } else if (typeof value === ""number"") {

            let thisResult = Enum.GetEntries(enumType, value);

            for (

                let i = 0; i < thisResult.length; i++) {

                result.push(thisResult[i]);

            }

        } else if (value[""constructor""] === EnumValue) {

            Enum.FlattenInternal(enumType, (<EnumValue<any>>value).Value, result);

        }

        return result;

    }

    public static GetEntries(enumType: any, value: number): EnumValueCollection<any> {

        let entries = Enum.ResolveEnumEntries(enumType);

        let hasFlags = Enum.HasFlags(enumType);

        let matches = new EnumValueCollection<any>(enumType);

        for (

            let i = 0; i < entries.length; i++) {

            if (hasFlags) {

                if (Enum.HasFlag(value, entries[i].Value)) {

                    matches.push(entries[i]);

                }

            } else if (value === entries[i].Value) {

                matches.push(entries[i]);

            }

        }

        return matches;

    }

}";
    }
}
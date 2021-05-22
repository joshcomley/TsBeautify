namespace TsBeautify.Data
{
    public class LargeFile
    {
        public static string Input1 = @"import { PropertyInfo, StringBuilder, TypeInfo, Coalesce, Type, Interface, Enumerable, StringUtil } from ""@brandless/tsutility"";
import { TsBeautifyOptions } from ""./TsBeautifyOptions"";
import { StringExtensions } from ""./Extensions/StringExtensions"";
export class TsBeautifierInstance 

                {
constructor() {
let interpolationAllowed = c == `$\``;

while (esc || (<string>(this._input.charCodeAt(<number>(parserPos())).toString())) != sep)
 {

if(interpolationAllowed && (<boolean>(StringExtensions.StartsWithAt(this._input, <string>(`\${`), <number>(parserPos())))))
 {

let jsSourceText = (<string>(this._input.substr(<number>(parserPos() + 1))));

let sub = new TsBeautifierInstance (<string>(jsSourceText), this._options, true);

let interpolated = (<string>((<string>(StringUtil.TrimStart((<string>(sub.Beautify())), '{'.charCodeAt(0)))).trim()));

resultingString += `\${${interpolated}}`;

parserPos(parserPos() + sub._parserPos + 1);

}
 else {

resultingString += String.fromCharCode(this._input.charCodeAt(<number>(parserPos())));

if(!esc)
 {

esc = this._input.charCodeAt(<number>(parserPos())) == '\\'.charCodeAt(0);

}
 else {

esc = false;

}

parserPos(parserPos() + 1);

}

if(parserPos() >= this._input.length)
 {

return <Array<string>>[resultingString,`TK_STRING`];

}

}

}
                }";

        public static string Input2 = @"import ""module-alias/register"";
import { suite, test, slow, timeout } from ""mocha-typescript"";
import { assert } from ""chai"";import { __Iql_Tests_Types_typeLoaded, __Iql_Tests_Types_defer } from ""../../Iql.Tests.Types.Defer"";
import { TestsBase } from ""../TestsBase"";

import { Site, Person, Client, PersonCategory, ApplicationUser, AppDbContext } from ""@brandless/iql.tests.data"";
import { DataContextExtensions } from ""@brandless/iql.data"";
import { IqlPointExpression, IqlType } from ""@brandless/iql"";
import { TestCurrentUserResolver } from ""../Services/TestCurrentUserResolver"";
import { Type, Interface, DateTime, Serialization } from ""@brandless/tsutility"";
import { TestCurrentLocationResolver } from ""../Services/TestCurrentLocationResolver"";
import { IqlCurrentUserService, IqlCurrentLocationService } from ""@brandless/iql.entities"";
/*[TestClass]*/

@suite export class PropertyTests extends TestsBase

 

                {

                

                

                public static FunctionsDeclared(): Array<string> {

                    return new Array<string>(`InferredWithConversionToStringTest`, `PopulateNewExistingInferredWithValueTest`, `PopulateNewEntityInferredWithValueTest`, `TestPropertyResolveFriendlyName`, `TestPropertyNullability`, `TestPropertyIqlTypeString`, `TestPropertyIqlTypeDate`, `TestPropertyIqlTypeInteger`, `TestPropertyIqlTypeDecimalFromDecimal`, `TestPropertyIqlTypeDecimalFromFloat`, `TestPropertyIqlTypeDecimalFromDouble`, `TestPropertyIqlTypeBoolean`);

                }

                

                public static ClassName: string = `PropertyTests`;

                

                @test public async ""Inferred With Conversion To String Test"" ()

{



PropertyTests.Db.ServiceProvider.Clear();



let site = new Site();



site.ClientId = 7;



site.Address = `A\nB`;



site.PostCode = `DEF`;



assert.isFalse(await DataContextExtensions.TrySetInferredValuesAsync(PropertyTests.Db, site));



assert.equal(site.Key, `7`);



assert.equal(site.FullAddress, `A\nB\nDEF`);



}





@test public async ""Populate New Existing Inferred With Value Test"" ()

{



AppDbContext.InMemoryDb.People.push((() => { let obj = new Person();



obj.Id = 177;

obj.SiteId = 87;

obj.Title = `My person`;

; return obj; })());



AppDbContext.InMemoryDb.Sites.push((() => { let obj = new Site();



obj.Id = 87;

obj.Name = `My site`;

obj.ClientId = 107;

; return obj; })());



AppDbContext.InMemoryDb.Clients.push((() => { let obj = new Client();



obj.Id = 107;

obj.Name = `My client`;

; return obj; })());



let person = await PropertyTests.Db.People.GetWithKeyAsync(177);



assert.isFalse(await DataContextExtensions.TrySetInferredValuesAsync(PropertyTests.Db, person));



assert.equal(person.CreatedByUserId, null);



assert.equal(person.Location, null);



assert.equal(person.ClientId, 107);



assert.exists(person.Client);





assert.isFalse(await DataContextExtensions.TrySetInferredValuesAsync(PropertyTests.Db, person));



assert.equal(person.CreatedByUserId, null);



assert.equal(person.Location, null);



assert.equal(person.ClientId, 107);



assert.exists(person.Client);





PropertyTests.Db.ServiceProvider.RegisterInstance<TestCurrentUserResolver>(new TestCurrentUserResolver(), TestCurrentUserResolver);



PropertyTests.Db.ServiceProvider.RegisterInstance<TestCurrentLocationResolver>(new TestCurrentLocationResolver(), TestCurrentLocationResolver);



assert.isTrue(await DataContextExtensions.TrySetInferredValuesAsync(PropertyTests.Db, person));



assert.equal(person.Location, null);



assert.equal(person.CreatedByUserId, `testuserid`);



assert.equal(person.ClientId, 107);



assert.exists(person.Client);





let currentLatitude = 51.5054597;



TestCurrentLocationResolver.CurrentLatitude = currentLatitude;



let currentLongitude = -0.0775452;



TestCurrentLocationResolver.CurrentLongitude = currentLongitude;



PropertyTests.Db.ServiceProvider.Unregister<TestCurrentUserResolver>(TestCurrentUserResolver);





assert.isFalse(await DataContextExtensions.TrySetInferredValuesAsync(PropertyTests.Db, person));



assert.equal(person.CreatedByUserId, `testuserid`);



assert.exists(person.Location);



assert.equal(person.Location.X, TestCurrentLocationResolver.CurrentLongitude);



assert.equal(person.Location.Y, TestCurrentLocationResolver.CurrentLatitude);



assert.equal(person.ClientId, 107);



assert.exists(person.Client);



let location = person.Location;





TestCurrentLocationResolver.CurrentLatitude = 41.5054597;



TestCurrentLocationResolver.CurrentLongitude = -1.0775452;





PropertyTests.Db.ServiceProvider.RegisterInstance<TestCurrentUserResolver>(new TestCurrentUserResolver(), TestCurrentUserResolver);



assert.isTrue(await DataContextExtensions.TrySetInferredValuesAsync(PropertyTests.Db, person));



assert.equal(person.CreatedByUserId, `testuserid`);



assert.equal(person.Location, location);



assert.equal(person.Location.X, currentLongitude);



assert.equal(person.Location.Y, currentLatitude);



assert.equal(person.ClientId, 107);



assert.exists(person.Client);





PropertyTests.Db.ServiceProvider.Unregister<IqlCurrentUserService>(IqlCurrentUserService);



assert.isFalse(await DataContextExtensions.TrySetInferredValuesAsync(PropertyTests.Db, person));



assert.equal(person.CreatedByUserId, `testuserid`);



assert.equal(person.Location, location);



assert.equal(person.Location.X, currentLongitude);



assert.equal(person.Location.Y, currentLatitude);



assert.equal(person.ClientId, 107);



assert.exists(person.Client);





PropertyTests.Db.ServiceProvider.RegisterInstance<IqlCurrentUserService>(new TestCurrentUserResolver(), IqlCurrentUserService);



person.Location = null;



assert.isTrue(await DataContextExtensions.TrySetInferredValuesAsync(PropertyTests.Db, person));



assert.equal(person.CreatedByUserId, `testuserid`);



assert.exists(person.Location);



assert.equal(person.Location.X, TestCurrentLocationResolver.CurrentLongitude);



assert.equal(person.Location.Y, TestCurrentLocationResolver.CurrentLatitude);



assert.equal(person.ClientId, 107);



assert.exists(person.Client);





PropertyTests.Db.ServiceProvider.Unregister<IqlCurrentUserService>(IqlCurrentUserService);



assert.isFalse(await DataContextExtensions.TrySetInferredValuesAsync(PropertyTests.Db, person));



assert.equal(person.CreatedByUserId, `testuserid`);



assert.exists(person.Location);



assert.equal(person.Location.X, TestCurrentLocationResolver.CurrentLongitude);



assert.equal(person.Location.Y, TestCurrentLocationResolver.CurrentLatitude);



assert.equal(person.ClientId, 107);



assert.exists(person.Client);



PropertyTests.Db.ServiceProvider.Unregister<IqlCurrentUserService>(IqlCurrentUserService);



PropertyTests.Db.ServiceProvider.Unregister<IqlCurrentLocationService>(IqlCurrentLocationService);



}





@test public async ""Populate New Entity Inferred With Value Test"" ()

{



let person = new Person();



person.SiteId = 87;



AppDbContext.InMemoryDb.Sites.push((() => { let obj = new Site();



obj.Id = 87;

obj.Name = `My site`;

obj.ClientId = 107;

; return obj; })());



AppDbContext.InMemoryDb.Clients.push((() => { let obj = new Client();



obj.Id = 107;

obj.Name = `My client`;

; return obj; })());





assert.isFalse(await DataContextExtensions.TrySetInferredValuesAsync(PropertyTests.Db, person));



assert.isTrue(person.CreatedDate > (<Date>(DateTime.AddSeconds(new Date(), -10))));



assert.equal(person.CreatedByUserId, null);



assert.equal(person.Description, null);



assert.equal(person.ClientId, 107);



assert.exists(person.Client);





person.Category = PersonCategory.AutoDescription;



assert.isFalse(await DataContextExtensions.TrySetInferredValuesAsync(PropertyTests.Db, person));



assert.equal(person.Description, `I'm \\ \""auto\""`);



assert.equal(person.CreatedByUserId, null);



assert.equal(person.ClientId, 107);



assert.exists(person.Client);





PropertyTests.Db.ServiceProvider.RegisterInstance<TestCurrentUserResolver>(new TestCurrentUserResolver(), TestCurrentUserResolver);



assert.isFalse(await DataContextExtensions.TrySetInferredValuesAsync(PropertyTests.Db, person));



assert.equal(person.CreatedByUserId, `testuserid`);



assert.equal(person.ClientId, 107);



assert.exists(person.Client);





PropertyTests.Db.ServiceProvider.Unregister<TestCurrentUserResolver>(TestCurrentUserResolver);



assert.isFalse(await DataContextExtensions.TrySetInferredValuesAsync(PropertyTests.Db, person));



assert.equal(person.CreatedByUserId, `testuserid`);



assert.equal(person.ClientId, 107);



assert.exists(person.Client);





PropertyTests.Db.ServiceProvider.RegisterInstance<TestCurrentUserResolver>(new TestCurrentUserResolver(), TestCurrentUserResolver);



assert.isFalse(await DataContextExtensions.TrySetInferredValuesAsync(PropertyTests.Db, person));



assert.equal(person.CreatedByUserId, `testuserid`);



assert.equal(person.ClientId, 107);



assert.exists(person.Client);





PropertyTests.Db.ServiceProvider.Unregister<IqlCurrentUserService>(IqlCurrentUserService);



assert.isFalse(await DataContextExtensions.TrySetInferredValuesAsync(PropertyTests.Db, person));



assert.equal(person.CreatedByUserId, `testuserid`);



assert.equal(person.ClientId, 107);



assert.exists(person.Client);





PropertyTests.Db.ServiceProvider.RegisterInstance<IqlCurrentUserService>(new TestCurrentUserResolver(), IqlCurrentUserService);



assert.isFalse(await DataContextExtensions.TrySetInferredValuesAsync(PropertyTests.Db, person));



assert.equal(person.CreatedByUserId, `testuserid`);



assert.equal(person.ClientId, 107);



assert.exists(person.Client);





PropertyTests.Db.ServiceProvider.Unregister<IqlCurrentUserService>(IqlCurrentUserService);



assert.isFalse(await DataContextExtensions.TrySetInferredValuesAsync(PropertyTests.Db, person));



assert.equal(person.CreatedByUserId, `testuserid`);



assert.equal(person.ClientId, 107);



assert.exists(person.Client);



PropertyTests.Db.ServiceProvider.Unregister<IqlCurrentUserService>(IqlCurrentUserService);



PropertyTests.Db.ServiceProvider.Unregister<IqlCurrentLocationService>(IqlCurrentLocationService);



}





@test public ""Test Property Resolve Friendly Name"" ()

{



let config = PropertyTests.Db.EntityConfigurationContext.EntityType<Client>(Client);



let property = config.FindPropertyByExpression<Date>(c => c.CreatedDate, Date);



assert.equal(property.Name, `CreatedDate`);



assert.equal(property.FriendlyName, `Created Date`);



}





@test public ""Test Property Nullability"" ()

{



assert.equal(PropertyTests.Db.EntityConfigurationContext.EntityType<Client>(Client).FindPropertyByExpression<Date>(p => p.CreatedDate, Date)

.TypeDefinition.Nullable, false);



}





@test public ""Test Property Iql Type String"" ()

{



assert.equal(PropertyTests.Db.EntityConfigurationContext.EntityType<Client>(Client).FindPropertyByExpression<string>(p => p.Name, String)

.TypeDefinition.Kind, IqlType.String);



}





@test public ""Test Property Iql Type Date"" ()

{



assert.equal(PropertyTests.Db.EntityConfigurationContext.EntityType<Client>(Client).FindPropertyByExpression<Date>(p => p.CreatedDate, Date)

.TypeDefinition.Kind, IqlType.Date);



}





@test public ""Test Property Iql Type Integer"" ()

{



assert.equal(PropertyTests.Db.EntityConfigurationContext.EntityType<Client>(Client).FindPropertyByExpression<number>(p => p.Id, Number)

.TypeDefinition.Kind, IqlType.Integer);



}





@test public ""Test Property Iql Type Decimal From Decimal"" ()

{



assert.equal(PropertyTests.Db.EntityConfigurationContext.EntityType<Client>(Client).FindPropertyByExpression<number>(p => p.Discount, Number)

.TypeDefinition.Kind, IqlType.Decimal);



}





@test public ""Test Property Iql Type Decimal From Float"" ()

{



assert.equal(PropertyTests.Db.EntityConfigurationContext.EntityType<Client>(Client).FindPropertyByExpression<number>(p => p.AverageSales, Number)

.TypeDefinition.Kind, IqlType.Decimal);



}





@test public ""Test Property Iql Type Decimal From Double"" ()

{



assert.equal(PropertyTests.Db.EntityConfigurationContext.EntityType<Client>(Client).FindPropertyByExpression<number>(p => p.AverageIncome, Number)

.TypeDefinition.Kind, IqlType.Decimal);



}





@test public ""Test Property Iql Type Boolean"" ()

{



assert.equal(PropertyTests.Db.EntityConfigurationContext.EntityType<ApplicationUser>(ApplicationUser).FindPropertyByExpression<boolean>(p => p.EmailConfirmed, Boolean)

.TypeDefinition.Kind, IqlType.Boolean);



}



                toJSON(): any {

        return Serialization.PrepareForJson(this, PropertyTests);

    }

                }







__Iql_Tests_Types_typeLoaded(`./Tests/Properties/PropertyTests`);";

        public static string Input1Beautified = @"import { PropertyInfo, StringBuilder, TypeInfo, Coalesce, Type, Interface, Enumerable, StringUtil } from ""@brandless/tsutility"";
import { TsBeautifyOptions } from ""./TsBeautifyOptions"";
import { StringExtensions } from ""./Extensions/StringExtensions"";
export class TsBeautifierInstance {
    constructor() {
        let interpolationAllowed = c == `$\``;
        while (esc || (<string>(this._input.charCodeAt(<number>(parserPos())).toString())) != sep) {
            if (interpolationAllowed && (<boolean>(StringExtensions.StartsWithAt(this._input, <string>(`\${`), <number>(parserPos()))))) {
                let jsSourceText = (<string>(this._input.substr(<number>(parserPos() + 1))));
                let sub = new TsBeautifierInstance(<string>(jsSourceText), this._options, true);
                let interpolated = (<string>((<string>(StringUtil.TrimStart((<string>(sub.Beautify())), '{'.charCodeAt(0)))).trim()));
                resultingString += `\${${interpolated}}`;
                parserPos(parserPos() + sub._parserPos + 1);
            } else {
                resultingString += String.fromCharCode(this._input.charCodeAt(<number>(parserPos())));
                if (!esc) {
                    esc = this._input.charCodeAt(<number>(parserPos())) == '\\'.charCodeAt(0);
                } else {
                    esc = false;
                }
                parserPos(parserPos() + 1);
            }
            if (parserPos() >= this._input.length) {
                return <Array<string>>[resultingString, `TK_STRING`];
            }
        }
    }
}";

        public static string Input2Beautified = @"import ""module-alias/register"";
import { suite, test, slow, timeout } from ""mocha-typescript"";
import { assert } from ""chai"";
import { __Iql_Tests_Types_typeLoaded, __Iql_Tests_Types_defer } from ""../../Iql.Tests.Types.Defer"";
import { TestsBase } from ""../TestsBase"";
import { Site, Person, Client, PersonCategory, ApplicationUser, AppDbContext } from ""@brandless/iql.tests.data"";
import { DataContextExtensions } from ""@brandless/iql.data"";
import { IqlPointExpression, IqlType } from ""@brandless/iql"";
import { TestCurrentUserResolver } from ""../Services/TestCurrentUserResolver"";
import { Type, Interface, DateTime, Serialization } from ""@brandless/tsutility"";
import { TestCurrentLocationResolver } from ""../Services/TestCurrentLocationResolver"";
import { IqlCurrentUserService, IqlCurrentLocationService } from ""@brandless/iql.entities"";
/*[TestClass]*/
@suite export class PropertyTests extends TestsBase {
    public static FunctionsDeclared(): Array<string> {
        return new Array<string>(`InferredWithConversionToStringTest`, `PopulateNewExistingInferredWithValueTest`, `PopulateNewEntityInferredWithValueTest`, `TestPropertyResolveFriendlyName`, `TestPropertyNullability`, `TestPropertyIqlTypeString`, `TestPropertyIqlTypeDate`, `TestPropertyIqlTypeInteger`, `TestPropertyIqlTypeDecimalFromDecimal`, `TestPropertyIqlTypeDecimalFromFloat`, `TestPropertyIqlTypeDecimalFromDouble`, `TestPropertyIqlTypeBoolean`);
    }
    public static ClassName: string = `PropertyTests`;
    @test public async ""Inferred With Conversion To String Test"" () {
        PropertyTests.Db.ServiceProvider.Clear();
        let site = new Site();
        site.ClientId = 7;
        site.Address = `A\nB`;
        site.PostCode = `DEF`;
        assert.isFalse(await DataContextExtensions.TrySetInferredValuesAsync(PropertyTests.Db, site));
        assert.equal(site.Key, `7`);
        assert.equal(site.FullAddress, `A\nB\nDEF`);
    }
    @test public async ""Populate New Existing Inferred With Value Test"" () {
        AppDbContext.InMemoryDb.People.push((() => {
            let obj = new Person();
            obj.Id = 177;
            obj.SiteId = 87;
            obj.Title = `My person`;;
            return obj;
        })());
        AppDbContext.InMemoryDb.Sites.push((() => {
            let obj = new Site();
            obj.Id = 87;
            obj.Name = `My site`;
            obj.ClientId = 107;;
            return obj;
        })());
        AppDbContext.InMemoryDb.Clients.push((() => {
            let obj = new Client();
            obj.Id = 107;
            obj.Name = `My client`;;
            return obj;
        })());
        let person = await PropertyTests.Db.People.GetWithKeyAsync(177);
        assert.isFalse(await DataContextExtensions.TrySetInferredValuesAsync(PropertyTests.Db, person));
        assert.equal(person.CreatedByUserId, null);
        assert.equal(person.Location, null);
        assert.equal(person.ClientId, 107);
        assert.exists(person.Client);
        assert.isFalse(await DataContextExtensions.TrySetInferredValuesAsync(PropertyTests.Db, person));
        assert.equal(person.CreatedByUserId, null);
        assert.equal(person.Location, null);
        assert.equal(person.ClientId, 107);
        assert.exists(person.Client);
        PropertyTests.Db.ServiceProvider.RegisterInstance<TestCurrentUserResolver>(new TestCurrentUserResolver(), TestCurrentUserResolver);
        PropertyTests.Db.ServiceProvider.RegisterInstance<TestCurrentLocationResolver>(new TestCurrentLocationResolver(), TestCurrentLocationResolver);
        assert.isTrue(await DataContextExtensions.TrySetInferredValuesAsync(PropertyTests.Db, person));
        assert.equal(person.Location, null);
        assert.equal(person.CreatedByUserId, `testuserid`);
        assert.equal(person.ClientId, 107);
        assert.exists(person.Client);
        let currentLatitude = 51.5054597;
        TestCurrentLocationResolver.CurrentLatitude = currentLatitude;
        let currentLongitude = -0.0775452;
        TestCurrentLocationResolver.CurrentLongitude = currentLongitude;
        PropertyTests.Db.ServiceProvider.Unregister<TestCurrentUserResolver>(TestCurrentUserResolver);
        assert.isFalse(await DataContextExtensions.TrySetInferredValuesAsync(PropertyTests.Db, person));
        assert.equal(person.CreatedByUserId, `testuserid`);
        assert.exists(person.Location);
        assert.equal(person.Location.X, TestCurrentLocationResolver.CurrentLongitude);
        assert.equal(person.Location.Y, TestCurrentLocationResolver.CurrentLatitude);
        assert.equal(person.ClientId, 107);
        assert.exists(person.Client);
        let location = person.Location;
        TestCurrentLocationResolver.CurrentLatitude = 41.5054597;
        TestCurrentLocationResolver.CurrentLongitude = -1.0775452;
        PropertyTests.Db.ServiceProvider.RegisterInstance<TestCurrentUserResolver>(new TestCurrentUserResolver(), TestCurrentUserResolver);
        assert.isTrue(await DataContextExtensions.TrySetInferredValuesAsync(PropertyTests.Db, person));
        assert.equal(person.CreatedByUserId, `testuserid`);
        assert.equal(person.Location, location);
        assert.equal(person.Location.X, currentLongitude);
        assert.equal(person.Location.Y, currentLatitude);
        assert.equal(person.ClientId, 107);
        assert.exists(person.Client);
        PropertyTests.Db.ServiceProvider.Unregister<IqlCurrentUserService>(IqlCurrentUserService);
        assert.isFalse(await DataContextExtensions.TrySetInferredValuesAsync(PropertyTests.Db, person));
        assert.equal(person.CreatedByUserId, `testuserid`);
        assert.equal(person.Location, location);
        assert.equal(person.Location.X, currentLongitude);
        assert.equal(person.Location.Y, currentLatitude);
        assert.equal(person.ClientId, 107);
        assert.exists(person.Client);
        PropertyTests.Db.ServiceProvider.RegisterInstance<IqlCurrentUserService>(new TestCurrentUserResolver(), IqlCurrentUserService);
        person.Location = null;
        assert.isTrue(await DataContextExtensions.TrySetInferredValuesAsync(PropertyTests.Db, person));
        assert.equal(person.CreatedByUserId, `testuserid`);
        assert.exists(person.Location);
        assert.equal(person.Location.X, TestCurrentLocationResolver.CurrentLongitude);
        assert.equal(person.Location.Y, TestCurrentLocationResolver.CurrentLatitude);
        assert.equal(person.ClientId, 107);
        assert.exists(person.Client);
        PropertyTests.Db.ServiceProvider.Unregister<IqlCurrentUserService>(IqlCurrentUserService);
        assert.isFalse(await DataContextExtensions.TrySetInferredValuesAsync(PropertyTests.Db, person));
        assert.equal(person.CreatedByUserId, `testuserid`);
        assert.exists(person.Location);
        assert.equal(person.Location.X, TestCurrentLocationResolver.CurrentLongitude);
        assert.equal(person.Location.Y, TestCurrentLocationResolver.CurrentLatitude);
        assert.equal(person.ClientId, 107);
        assert.exists(person.Client);
        PropertyTests.Db.ServiceProvider.Unregister<IqlCurrentUserService>(IqlCurrentUserService);
        PropertyTests.Db.ServiceProvider.Unregister<IqlCurrentLocationService>(IqlCurrentLocationService);
    }
    @test public async ""Populate New Entity Inferred With Value Test"" () {
        let person = new Person();
        person.SiteId = 87;
        AppDbContext.InMemoryDb.Sites.push((() => {
            let obj = new Site();
            obj.Id = 87;
            obj.Name = `My site`;
            obj.ClientId = 107;;
            return obj;
        })());
        AppDbContext.InMemoryDb.Clients.push((() => {
            let obj = new Client();
            obj.Id = 107;
            obj.Name = `My client`;;
            return obj;
        })());
        assert.isFalse(await DataContextExtensions.TrySetInferredValuesAsync(PropertyTests.Db, person));
        assert.isTrue(person.CreatedDate > (<Date>(DateTime.AddSeconds(new Date(), -10))));
        assert.equal(person.CreatedByUserId, null);
        assert.equal(person.Description, null);
        assert.equal(person.ClientId, 107);
        assert.exists(person.Client);
        person.Category = PersonCategory.AutoDescription;
        assert.isFalse(await DataContextExtensions.TrySetInferredValuesAsync(PropertyTests.Db, person));
        assert.equal(person.Description, `I'm \\ \""auto\""`);
        assert.equal(person.CreatedByUserId, null);
        assert.equal(person.ClientId, 107);
        assert.exists(person.Client);
        PropertyTests.Db.ServiceProvider.RegisterInstance<TestCurrentUserResolver>(new TestCurrentUserResolver(), TestCurrentUserResolver);
        assert.isFalse(await DataContextExtensions.TrySetInferredValuesAsync(PropertyTests.Db, person));
        assert.equal(person.CreatedByUserId, `testuserid`);
        assert.equal(person.ClientId, 107);
        assert.exists(person.Client);
        PropertyTests.Db.ServiceProvider.Unregister<TestCurrentUserResolver>(TestCurrentUserResolver);
        assert.isFalse(await DataContextExtensions.TrySetInferredValuesAsync(PropertyTests.Db, person));
        assert.equal(person.CreatedByUserId, `testuserid`);
        assert.equal(person.ClientId, 107);
        assert.exists(person.Client);
        PropertyTests.Db.ServiceProvider.RegisterInstance<TestCurrentUserResolver>(new TestCurrentUserResolver(), TestCurrentUserResolver);
        assert.isFalse(await DataContextExtensions.TrySetInferredValuesAsync(PropertyTests.Db, person));
        assert.equal(person.CreatedByUserId, `testuserid`);
        assert.equal(person.ClientId, 107);
        assert.exists(person.Client);
        PropertyTests.Db.ServiceProvider.Unregister<IqlCurrentUserService>(IqlCurrentUserService);
        assert.isFalse(await DataContextExtensions.TrySetInferredValuesAsync(PropertyTests.Db, person));
        assert.equal(person.CreatedByUserId, `testuserid`);
        assert.equal(person.ClientId, 107);
        assert.exists(person.Client);
        PropertyTests.Db.ServiceProvider.RegisterInstance<IqlCurrentUserService>(new TestCurrentUserResolver(), IqlCurrentUserService);
        assert.isFalse(await DataContextExtensions.TrySetInferredValuesAsync(PropertyTests.Db, person));
        assert.equal(person.CreatedByUserId, `testuserid`);
        assert.equal(person.ClientId, 107);
        assert.exists(person.Client);
        PropertyTests.Db.ServiceProvider.Unregister<IqlCurrentUserService>(IqlCurrentUserService);
        assert.isFalse(await DataContextExtensions.TrySetInferredValuesAsync(PropertyTests.Db, person));
        assert.equal(person.CreatedByUserId, `testuserid`);
        assert.equal(person.ClientId, 107);
        assert.exists(person.Client);
        PropertyTests.Db.ServiceProvider.Unregister<IqlCurrentUserService>(IqlCurrentUserService);
        PropertyTests.Db.ServiceProvider.Unregister<IqlCurrentLocationService>(IqlCurrentLocationService);
    }
    @test public ""Test Property Resolve Friendly Name"" () {
        let config = PropertyTests.Db.EntityConfigurationContext.EntityType<Client>(Client);
        let property = config.FindPropertyByExpression<Date>(c => c.CreatedDate, Date);
        assert.equal(property.Name, `CreatedDate`);
        assert.equal(property.FriendlyName, `Created Date`);
    }
    @test public ""Test Property Nullability"" () {
        assert.equal(PropertyTests.Db.EntityConfigurationContext.EntityType<Client>(Client).FindPropertyByExpression<Date>(p => p.CreatedDate, Date).TypeDefinition.Nullable, false);
    }
    @test public ""Test Property Iql Type String"" () {
        assert.equal(PropertyTests.Db.EntityConfigurationContext.EntityType<Client>(Client).FindPropertyByExpression<string>(p => p.Name, String).TypeDefinition.Kind, IqlType.String);
    }
    @test public ""Test Property Iql Type Date"" () {
        assert.equal(PropertyTests.Db.EntityConfigurationContext.EntityType<Client>(Client).FindPropertyByExpression<Date>(p => p.CreatedDate, Date).TypeDefinition.Kind, IqlType.Date);
    }
    @test public ""Test Property Iql Type Integer"" () {
        assert.equal(PropertyTests.Db.EntityConfigurationContext.EntityType<Client>(Client).FindPropertyByExpression<number>(p => p.Id, Number).TypeDefinition.Kind, IqlType.Integer);
    }
    @test public ""Test Property Iql Type Decimal From Decimal"" () {
        assert.equal(PropertyTests.Db.EntityConfigurationContext.EntityType<Client>(Client).FindPropertyByExpression<number>(p => p.Discount, Number).TypeDefinition.Kind, IqlType.Decimal);
    }
    @test public ""Test Property Iql Type Decimal From Float"" () {
        assert.equal(PropertyTests.Db.EntityConfigurationContext.EntityType<Client>(Client).FindPropertyByExpression<number>(p => p.AverageSales, Number).TypeDefinition.Kind, IqlType.Decimal);
    }
    @test public ""Test Property Iql Type Decimal From Double"" () {
        assert.equal(PropertyTests.Db.EntityConfigurationContext.EntityType<Client>(Client).FindPropertyByExpression<number>(p => p.AverageIncome, Number).TypeDefinition.Kind, IqlType.Decimal);
    }
    @test public ""Test Property Iql Type Boolean"" () {
        assert.equal(PropertyTests.Db.EntityConfigurationContext.EntityType<ApplicationUser>(ApplicationUser).FindPropertyByExpression<boolean>(p => p.EmailConfirmed, Boolean).TypeDefinition.Kind, IqlType.Boolean);
    }
    toJSON(): any {
        return Serialization.PrepareForJson(this, PropertyTests);
    }
}
__Iql_Tests_Types_typeLoaded(`./Tests/Properties/PropertyTests`);";
    }
}
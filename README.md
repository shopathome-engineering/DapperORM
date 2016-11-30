# Summary

This library exposes CRUD operations and SQL Select statements built from C# expressions. It attempts to provide some more full-featured ORM operations while still maintaining Dapper's minimalism and efficiency.

Much of this library is built off of the excellent Dapper.Contrib extension library found here: https://github.com/StackExchange/dapper-dot-net/tree/master/Dapper.Contrib

# API Documentation
This library uses Dapper as its data provider. For information about Dapper and how it works under the hood, reference the official documentation for that project. This will be important for understanding how the default object mapper behaves - if you are getting exceptions or strange behavior in object creation, check the official documentation first and make sure that your objects and SELECT statements are in-sync. We will detail class definition that is specific to this library in a later section.

# Getting Started
The entry point to the library is the IDBProvider interface. The implementation included in the library is the DapperDBProvider.
Once you have an IDBProvider, you must create a connection to a database. This is done by providing the name of a connection string that can be found in your app or web.config to the Connect() method of IDBProvider. Connect() returns an IDbConnection.


    <configuration>
        <connectionStrings>
            <add name="MyFavoriteConnectionString" [...] />
        </connectionStrings> 
    </configuration>


And then, in your code,

    var connectionStringName = "MyFavoriteConnectionString";
    var dbProvider = new DapperDBProvider();
    using (var connection = dbProvider.Connect(connectionStringName))
    {
    	// do things with the connection
    }

# Reading Data
There are many method and overloads of methods for reading data. This will cover most of the major operations and common patterns, but not every method.

## Stored Procedures
Executing stored procedures and reading the resulting SELECT into a collection of objects is simple. The Select() method is used for reading data from stored procedures.

    var storedProcedureName = "[Example_Select_AllRows]";
    IEnumerable<ExampleClass> resultData = new List<ExampleClass>();
    using (var connection = dbProvider.Connect(connectionStringName))
    {
	    resultData = dbProvider.Select<ExampleClass>(connection, storedProcedureName);
    }

If you need to pass arguments to a stored procedure, there is an overload that takes an object of arguments. This is consistent with the default Dapper API.

    var storedProcedureName = "[Example_Select_RowById]";
    var rowId = 24;
    IEnumerable<ExampleClass> resultData = new List<ExampleClass>();
    using (var connection = dbProvider.Connect(connectionStringName))
    {
    	resultData = dbProvider.Select<ExampleClass>(connection, storedProcedureName, new { rowId });
    }

## SQL Literals
Executing literal SQL queries and reading the resulting SELECT into a collection of objects is also supported. The Query() method is used for reading data from literal SQL.

    IEnumerable<ExampleClass> resultData = new List<ExampleClass>();
    using (var connection = dbProvider.Connect(connectionStringName))
    {
    	resultData = dbProvider.Query<ExampleClass>(connection, "SELECT * FROM Database.dbo.ExampleData");
    }

This example shows you how to pass parameters into a SQL Literal query.

    var rowId = 24;
    IEnumerable<ExampleClass> resultData = new List<ExampleClass>();
    using (var connection = dbProvider.Connect(connectionStringName))
    {
    	resultData = dbProvider.Query<ExampleClass>(connection, "SELECT * FROM Database.dbo.ExampleData WHERE RowId = @rowId", new { rowId });
    }

## C# Expressions
This library contains code which transforms C# predicates (Func<T, bool>) into SQL query clauses. This generates dynamic queries based on objects, allowing data reads without magic-strings (inline SQL or stored procedures). An overload of Select() is used for this purpose.

    using (var connection = dbProvider.Connect(connectionStringName))
    {
    	resultData = dbProvider.Select<ExampleClass>(connection, exmpl => exmpl.Foo == 24); 
    }

This will generate the following SQL: "SELECT * FROM Database.dbo.ExampleData WHERE Foo = 24".
NB: Multiple logical statements can be chained using C# operators (&& and ||). However, this method cannot perform joins or generate IN (...) statements, and should generally be used when simple SELECTs are called for. We have plans to expand this functionality, but it is not of high priority.
NB: Using this method requires properly-constructed classes - see the section on object decoration for more information.

## Reading single objects
Reading a row of data by its primary key is supported via the Get() methods.

    var key = 24;
    ExampleClass resultData;
    using (var connection = dbProvider.Connect(connectionStringName))
    {
    	resultData = dbProvider.Get<ExampleClass>(connection, key);
    }

NB: Using this method requires properly-constructed classes - see the section on object decoration for more information.
Writing Data
We use "write" to mean any operation that mutates data in the SQL store - thus inserts, updates, and deletes.
NB: Using all of these methods required properly-constructed classes - see the section on object decoration for more information.

# Inserts
There are two overloads, one for a single T, and one for a List<T>.

    var data = new ExampleClass { Foo = 24 };
    using (var connection = dbProvider.Connect(connectionStringName))
    {
    	dbProvider.Insert(connection, data);
    }

# Updates

    var data = new ExampleClass { Foo = 24 };
    using (var connection = dbProvider.Connect(connectionStringName))
    {
	    dbProvider.Update(connection, data);
    }

# Deletes

    var data = new ExampleClass { Foo = 24 };
    using (var connection = dbProvider.Connect(connectionStringName))
    {
    	dbProvider.Delete(connection, data);
    }

# SQL Literals
Executing SQL literals that modify data but do not select anything is supported via the Execute() void method.

    using (var connection = dbProvider.Connect(connectionStringName))
    {
    	dbProvider.Execute(connection, "UPDATE Database.dbo.ExampleData SET Foo = 24");
    }

# Object Decoration
In order for some of the features of the library to work, your classes must be decorated in the right way. These are the same as the Dapper.Contrib library.

## [Table]
The System.ComponentModel.DataAnnotations.Schema.TableAttribute is used to tell DapperORMCore to which table your object maps. By default, Dapper assumes that your class lives in a table that is the plural of the class name. You can (and should) override this default behavior with the TableAttribute class, and make the persistence store of your objects explicit.

    using System.ComponentModel.DataAnnotations.Schema;
 
    [Table("ExampleData")]
    public class ExampleClass
    {
    } 


## [Key]

The System.ComponentModel.DataAnnotations.KeyAttribute is used to tell DapperORMCore which property/column is the primary key of your object. This is required for write and Get() operations. Only a single property may be decorated in this way.

    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("ExampleData")]
    public class ExampleClass
    {
    	[Key]
	    public int ExampleClassId { get; set; }
    } 

## [NotMapped]
The System.ComponentModel.DataAnnotations.Schema.NotMappedAttribute is used to tell DapperORMCore that a property does not have a corresponding column in the SQL server schema. Properties that have been decorated in this way will not be written to the database or hydrated on reads.

    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("ExampleData")]
    public class ExampleClass
    {
    	[Key]
	    public int ExampleClassId { get; set; }
 
	    [NotMapped]
	    public string Bar { get; set; }
    }
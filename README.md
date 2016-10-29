# NPocoMigrations
The `NPocoMigrations` library provides a quite simple way to execute scripted database 
migrations (not only) for MS SQL Server, using NPoco. This project was inspired by the 
blog post [Handle Database Versioning with PetaPoco](https://kevin.giszewski.com/blogs/2016/05/handle-database-versioning-with-petapoco/) 
written by [Kevin Giszewski](https://kevin.giszewski.com).

## NuGet

 **Current Version: 1.0.3**
 
 Get it from [nuget.org/packages/NPocoMigrations](https://www.nuget.org/packages/NPocoMigrations) 
 or via  Package Manager Console.
 
  > *PM> Install-Package NPocoMigrations*

# Start migration scripts

The migrations will be processed if the following lines are put into the starting 
sequence of your application:

```csharp
public void YourStartupSequence(){

    // ... some code

    // -- NPoco Migrations
    var migrator = new NPocoMigrator();
    if (migrator.LoadConfig() && migrator.LoadMigrations()){
        migrator.ExecuteMigrations();
    }

    // ... some code

}
```
By using the NPocoMigrator (empty) standard constructor, the execution path of your 
application is taken as base directory for your migrations. If it is not intended that 
the migrations are stored in this path, it is possible to declare explicitly name it
in a constructor that takes the path to your migrations base directory as parameter.

If you have a web application and you want to store your migrations in the `App_Data` 
folder you have to instanciate the NPocoMigrator like so:  

```csharp
protected void Application_Start()
{

    // ... some code

    // -- NPoco Migrations
    var migrator = new NPocoMigrator(HostingEnvironment.MapPath(@"/App_Data"));
    if (migrator.LoadConfig() && migrator.LoadMigrations())
    {
        migrator.ExecuteMigrations();
    }

    // ... some code

}
```

# How it works

## Main config file (_migrationsconfig.json_)

This simple migrations system is based on a main configuration file which holds the
current database version (as "type" of System.Version), the connection string name
to the database and the path to the migration tasks (scripted in SQL). The `migrationsconfig.json` 
is always to be placed in the migration's base directory, as defined when instantiating
the NPocoMigrator (see above). 

```javascript
{
  "DbVersion": "0.0.0",
  "DbConnection": "DevDb",
  "MigrationsDir":".\\migrations"
}
```

**Take care of your DbVersion setting in production! Never Overwrite `migrationsconfig.json` 
when re-deploying. Just copy it once when deploying your binaries, initially!**

In a desktop application the `Copy to Output Directory` property for all the .json files 
needed for migrations should be set to `Copy if newer` and they should never be change in 
the project, itself after they were initially deployed.

If your migrations reside in the `App_Data` folder of a web application then you have to
set nothing at all. Just include your files in the web project and leave the `Copy to Output Directory`
set to `Do not copy`.  

## Migration scriptfiles

The migration scriptfiles are JSON files that can be named as you like, but they have 
to have the .json extension. The name schema used in the test project is like this:

```
migration_<dbVersion>.json
```

Although the `dbVersion` is included in the scriptfile, it can be useful as a filename 
suffix to quickly identify a scriptfile in the containing folder. All migrations scriptfiles 
have to reside in the migrations folder which is specified in the `MigrationsDir` setting 
of the migrations config file. From here all scriptfiles with a higher version than the
database are loaded by the migrator.

A scriptfile is set up like this:

```javascript
{
  // Database version after the migrations are successfully executed
  "Version": "1.0.0",

  // Short description
  "Description": "Initial database setup",

  // The list of script tasks to be executed against the database
  "Tasks": [
    {
      // Executes the given SQL/DLM script/line
      "Execute": "CREATE TABLE [dbo].[GeoData]( ... )",
      
      // An SQL Query which tests the execution of a script task. It should return
      // 1 if the test was successful.
      "Test": "SELECT CASE WHEN ((SELECT count(*) from ... )) THEN 1 ELSE 0 END"
    },
    {
      // Tests can also be omitted if they are not necessary.
      "Execute": "ALTER TABLE [dbo].[GeoData] ALTER COLUMN [PostalCode] [nvarchar](8)"
    }
  ]
}
``` 

## Transactional behavior

All tasks of a scriptfile are executed inside a transaction. **Each** task has to be 
either executed and tested or just executed successfully. After a successful run of a 
migration's scriptfile, the database version will be updated inside the `migrationsconfig.json` 
file. Any error/exception cancels the and rolls back the execution of the current set 
of tasks and finally also cancels the complete migration's run.

## Logging

All main migrator actions and errors are logged in the base directory written into the 
`migrator.log`. The script runs are logged inside the `migraions.log` which will be 
saved inside the `MigrationsDir`.

## Tests

The test project contained in the solution is just a small integration test that covers 
the main functionality of this library. Please have a look into it to maybe get some more 
informations on how to use `NPocoMigrations`. 

## Releases

2016-10-29 - 1.0.3 Constructor of NPocoMigrator can now set the base directory of the migrations.

2016-10-29 - 1.0.2 Target Framework changed to 4.5 

2016-10-29 - 1.0.1 Initial Release (Framework 4.5.2)

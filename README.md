# Postulate Merge console app

I created this so I could offer a free schema diff/merge app that uses my [SchemaSync](https://github.com/adamosoftware/SchemaSync) project. Use this to merge changes from a model class assembly using [Postulate.Lite ORM](https://github.com/adamosoftware/Postulate.Lite) to a physical SQL Server database. This is intended to be used as a Visual Studio "external tool." For more features, check out my commercial product [SQL Model Merge](https://aosoftware.net/Project/SqlModelMerge).

The free tool described here will merge model class changes to a SQL Server database. The paid app can merge from assembly to database as well as database to database.

## Setup

- Check the [Releases](https://github.com/adamosoftware/Postulate.Merge/releases) page for the latest Postulate.Merge installer. Download and install.

- In Visual Studio, create an External Tool **Postulate Merge** pointing to the full path of **Postulate.Merge.SqlServer.exe** that you just installed. Add the argument **$(SolutionDir)**.

- Run the **Postulate Merge** external tool to create a blank settings file in the current solution directory. The settings file is called *Postulate.Merge.json*. Edit this json file to customize it for the current solution -- specifically setting the [SourceAssembly](https://github.com/adamosoftware/Postulate.Merge/blob/master/Postulate.Merge.Models/Settings.cs#L26) and [TargetConnection](https://github.com/adamosoftware/Postulate.Merge/blob/master/Postulate.Merge.Models/Settings.cs#L34) properties. For info about what you can put in a Settings file, please see the [Settings class](https://github.com/adamosoftware/Postulate.Merge/blob/master/Postulate.Merge.Models/Settings.cs) which has plenty of comments. The blank settings file automatically adds exclusions for ASP.NET AspNetUsers table columns, which you would normally exclude from merges.

- There are default settings for SQL Server Management studio in the [CommandExe](https://github.com/adamosoftware/Postulate.Merge/blob/master/Postulate.Merge.Models/Settings.cs#L40) and [CommandArguments](https://github.com/adamosoftware/Postulate.Merge/blob/master/Postulate.Merge.Models/Settings.cs#L47) properties. Customize as needed to let you edit .sql files generated by Postulate Merge.

## Typical Use

- In a solution that has model classes that you must sync with a database, edit your model classes and build your project continuously as usual. After a build, use the **Postulate Merge** external tool to look for model changes to merge to your database. The console app will show the generated SQL script. You can press Enter to execute, T to save a test case, or E to edit in the designated editor (Ssms by default).

- Saving a test case will include schema information from your assembly and SQL Server database as well as the generated diff commands. If you find a bug, please submit an issue at the [SchemaSync](https://github.com/adamosoftware/SchemaSync) repository, and attach your test case. The test case content does not include any database data, but it does include your database schema information.

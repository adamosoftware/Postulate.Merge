# Postulate.Merge Console App

I created this so I could offer a free schema diff/merge app that uses my [SchemaSync](https://github.com/adamosoftware/SchemaSync) project. Use this to merge changes from a model class assembly using [Postulate.Lite ORM](https://github.com/adamosoftware/Postulate.Lite) to a physical SQL Server database. This is intended to be used as a Visual Studio "external tool." For more features, check out my commercial product [SQL Model Merge](https://aosoftware.net/Project/SqlModelMerge).

The free tool described here will merge model class changes to a SQL Server database. The paid app can merge from assembly to database as well as database to database.

## How to use

- Check the [Releases](https://github.com/adamosoftware/Postulate.Merge/releases) page for the latest Postulate.Merge installer. Download and install.

- In Visual Studio, create an External Tool **Postulate Merge** pointing to the full path of **Postulate.Merge.SqlServer.exe** that you just installed. Add the argument **$(SolutionDir)**.

- Run the **Postulate Merge** external tool to create a blank settings file in the current solution directory. The settings file is called *Postulate.Merge.json*. Edit this json file to customize it for the current solution. For info about what you can put in a Settings file, please see the [Settings class](https://github.com/adamosoftware/Postulate.Merge/blob/master/Postulate.Merge.Models/Settings.cs) which has plenty of comments.

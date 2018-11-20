using JsonSettings;
using Postulate.Merge.Models;
using SchemaSync.Library;
using SchemaSync.Library.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Postulate.Lite;
using Postulate.Lite.SqlServer;

namespace Postulate.Merge.SqlServer
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			try
			{
				string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

				if (args.Length == 0)
				{
					CreateEmptySettingsFile(path);					
					return;
				}

				string fileName = Path.Combine(path, (args.Length > 1) ? args[1] : "Postulate.Merge.json");
				var settings = JsonFile.Load<Settings>(fileName);

				var sourceDb = GetAssemblyDb(settings);
				var targetDb = GetConnectionDb(settings);
				var diff = SchemaComparison.Execute(sourceDb, targetDb, settings.ExcludeObjects);
				
				//diff.SaveScript(new SqlServerSyntax)
			}
			catch (Exception exc)
			{
				Console.WriteLine(exc.Message);
				Console.ReadLine();
			}
		}

		private static Database GetConnectionDb(Settings settings)
		{
			throw new NotImplementedException();
		}

		private static Database GetAssemblyDb(Settings settings)
		{
			throw new NotImplementedException();
		}

		private static void CreateEmptySettingsFile(string path)
		{
			var settings = new Settings()
			{
				SourceAssembly = "your model class assembly dll",
				TargetConnection = "target connection string",
				ExcludeObjects = new HashSet<ExcludeObject>()
				{
					new ExcludeObject() { ActionType = ActionType.Create, Name = "Sample", Type = "Table" }
				}
			};
			string fileName = Path.Combine(path, "Postulate.Merge.json");
			if (File.Exists(fileName))
			{
				Console.WriteLine($"Settings file already exists: {fileName}");
			}
			else
			{
				JsonFile.Save(fileName, settings);
				Console.WriteLine($"Empty settings file created: {fileName}");
			}
			Console.ReadLine();
		}
	}
}
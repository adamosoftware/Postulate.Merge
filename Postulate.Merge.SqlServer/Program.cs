using JsonSettings;
using Postulate.Merge.Models;
using SchemaSync.Library;
using SchemaSync.Library.Models;
using SchemaSync.SqlServer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Postulate.Merge.SqlServer
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			try
			{
				if ((args?.Length ?? 0) == 0)
				{
					Console.WriteLine("A folder name (usually a solution folder) is required to run this command.");
					return;
				}

				string path = args[0];
				string settingsFile = Path.Combine(path, "Postulate.MergeSettings.json");

				if (!File.Exists(settingsFile))
				{
					CreateEmptySettingsFile(path);
					return;
				}
								
				var settings = JsonFile.Load<Settings>(settingsFile);

				var sourceDb = GetAssemblyDb(settings);

				string connectionString = ResolveConnectionString(settings, path);
				var targetDb = new SqlServerDbProvider().GetDatabaseAsync(connectionString).Result;
				var diff = SchemaComparison.Execute(sourceDb, targetDb, settings.ExcludeObjects);

				string scriptFile = Path.Combine(path, "Postulate.Merge.sql");
				diff.SaveScript(new SqlServerSyntax(), scriptFile);
			}
			catch (Exception exc)
			{
				Console.WriteLine(exc.Message);
				Console.ReadLine();
			}
		}

		private static string ResolveConnectionString(Settings settings, string path)
		{
			switch (settings.TargetConnectionType)
			{
				case TargetConnectionType.ConnectionString:
					return settings.TargetConnection;

				case TargetConnectionType.ConfigFile:
					break;
			}

			throw new NotImplementedException();
		}

		private async static Task<Database> GetConnectionDbAsync(string connectionString)
		{
			return await new SqlServerDbProvider().GetDatabaseAsync(connectionString);
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
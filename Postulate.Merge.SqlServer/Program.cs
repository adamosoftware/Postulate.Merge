using JsonSettings;
using Postulate.Merge.Models;
using Postulate.Merge.Models.Models;
using SchemaSync.Library;
using SchemaSync.Library.Models;
using SchemaSync.SqlServer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
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
				var targetProvider = new SqlServerDbProvider();
				var targetDb = targetProvider.GetDatabaseAsync(connectionString).Result;
				var diff = SchemaComparison.Execute(sourceDb, targetDb, settings.ExcludeObjects);

				string scriptFile = Path.Combine(path, "Postulate.Merge.sql");
				if (File.Exists(scriptFile)) File.Delete(scriptFile);
				diff.SaveScript(targetProvider.GetDefaultSyntax(), scriptFile);
				
				RunScript(scriptFile, settings, connectionString);
			}
			catch (Exception exc)
			{
				Console.WriteLine(exc.Message);
				Console.ReadLine();
			}
		}

		private static void RunScript(string scriptFile, Settings settings, string connectionString)
		{
			if (string.IsNullOrEmpty(settings.CommandExe))
			{
				ProcessStartInfo psi = new ProcessStartInfo(scriptFile);
				psi.UseShellExecute = true;
				Process.Start(psi);
				return;
			}

			ProcessStartInfo exe = new ProcessStartInfo(settings.CommandExe);
			if (!string.IsNullOrEmpty(settings.CommandArguments))
			{
				exe.Arguments = ResolveArguments(settings.CommandArguments, connectionString);
			}
			Process.Start(exe);
		}

		/// <summary>
		/// Inserts connection string components into command line arguments
		/// </summary>
		private static string ResolveArguments(string arguments, string connectionString)
		{
			string result = arguments;

			var connectionInfo = ConnectionStrings.Parse(connectionString);
			var matches = Regex.Matches(arguments, "(?<!{)({[^{\r\n]*})(?!{)");
			foreach (Match match in matches)
			{
				string key = match.Value.Substring(1, match.Value.Length - 2);
				result = result.Replace(match.Value, connectionInfo[key]);
			}

			return result;
		}

		private static string ResolveConnectionString(Settings settings, string path)
		{
			switch (settings.TargetConnectionType)
			{
				case TargetConnectionType.ConnectionString:
					return settings.TargetConnection;

				case TargetConnectionType.ConfigFile:
					var info = settings.GetTargetInfo();					
					string configFile = FindFile(path, info.Filename);
					Dictionary<string, string> connections = FindConnections(configFile);
					return connections[info.ConnectionName];
			}

			throw new InvalidOperationException($"Unrecognized or not implemented target connection type {settings.TargetConnectionType}");
		}

		private static Dictionary<string, string> FindConnections(string configFile)
		{
			throw new NotImplementedException();
		}

		private static string FindFile(string path, string fileName)
		{
			string result = Path.Combine(path, fileName);
			if (File.Exists(result)) return result;

			try
			{
				string[] results = Directory.GetFiles(path, fileName, SearchOption.AllDirectories);
				return results.First();
			}
			catch (Exception exc)
			{
				throw new FileNotFoundException($"Couldn't find file {fileName} in {path}. ({exc.Message})");
			}
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
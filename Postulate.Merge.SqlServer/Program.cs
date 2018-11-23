using JsonSettings;
using Postulate.Lite.SqlServer;
using Postulate.Merge.Models;
using Postulate.Merge.Models.Models;
using SchemaSync.Library;
using SchemaSync.Library.Models;
using SchemaSync.Postulate;
using SchemaSync.SqlServer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;

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

				var assembly = Assembly.GetExecutingAssembly();
				Console.WriteLine($"Postulate.Merge.SqlServer version {assembly.GetName().Version}");

				const string settingsFilename = "Postulate.Merge.json";
				string path = args[0];
				string settingsFile = Path.Combine(path, settingsFilename);

				if (!File.Exists(settingsFile))
				{
					CreateEmptySettingsFile(path, settingsFilename);
					return;
				}

				var settings = JsonFile.Load<Settings>(settingsFile);

				Console.WriteLine("Analyzing model classes...");
				var sourceDb = GetAssemblyDb(settings);

				string connectionString = ResolveConnectionString(settings, path);
				var targetProvider = new SqlServerDbProvider();

				Console.WriteLine("Analyzing target database...");
				var targetDb = targetProvider.GetDatabaseAsync(connectionString).Result;

				Console.WriteLine("Generating merge script...");
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
				exe.Arguments = ResolveArguments(settings.CommandArguments, scriptFile, connectionString);
			}
			Process.Start(exe);
		}

		/// <summary>
		/// Inserts connection string components into command line arguments
		/// </summary>
		private static string ResolveArguments(string arguments, string scriptFile, string connectionString)
		{
			string result = arguments;

			result = result.Replace("%script_file%", scriptFile);

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
					var connections = FindConnections(configFile).ToDictionary(item => item.Name, item => item.ConnectionString);
					return connections[info.ConnectionName];
			}

			throw new InvalidOperationException($"Unrecognized or not implemented target connection type {settings.TargetConnectionType}");
		}

		private static IEnumerable<NamedConnection> FindConnections(string fileName)
		{
			var doc = XDocument.Load(fileName);
			var results = doc.XPathSelectElements("/configuration/connectionStrings/add");
			return results.Select(e => new NamedConnection()
			{
				Name = e.Attribute("name").Value,
				ConnectionString = e.Attribute("connectionString").Value
			});
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

		private static Database GetAssemblyDb(Settings settings)
		{
			return new PostulateDbProvider<SqlServerIntegrator>().GetDatabase(settings.SourceAssembly);
		}

		private static void CreateEmptySettingsFile(string path, string baseFile)
		{
			var settings = new Settings()
			{
				SourceAssembly = "your model class assembly dll",
				TargetConnection = "target connection string",
				ExcludeObjects = GetAspNetUsersObjects()
			};
			string fileName = Path.Combine(path, baseFile);
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

		private static HashSet<ExcludeObject> GetAspNetUsersObjects()
		{
			var items = new string[]
			{
				"dbo.AspNetUsers.Id",
				"dbo.AspNetUsers.NormalizedUserName",
				"dbo.AspNetUsers.NormalizedEmail",
				"dbo.AspNetUsers.EmailConfirmed",
				"dbo.AspNetUsers.PasswordHash",
				"dbo.AspNetUsers.SecurityStamp",
				"dbo.AspNetUsers.ConcurrencyStamp",
				"dbo.AspNetUsers.PhoneNumber",
				"dbo.AspNetUsers.PhoneNumberConfirmed",
				"dbo.AspNetUsers.TwoFactorEnabled",
				"dbo.AspNetUsers.LockoutEnd",
				"dbo.AspNetUsers.LockoutEnabled",
				"dbo.AspNetUsers.AccessFailedCount",
			}.Select(s => new ExcludeObject() { Name = s, ActionType = ActionType.Drop, Type = "Column" });

			return new HashSet<ExcludeObject>(items);
		}
	}
}
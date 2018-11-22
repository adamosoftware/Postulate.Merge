using Postulate.Merge.Models.Models;
using SchemaSync.Library.Models;
using System;
using System.Collections.Generic;

namespace Postulate.Merge.Models
{
	public enum TargetConnectionType
	{
		/// <summary>
		/// Literal connection string
		/// </summary>
		ConnectionString,
		/// <summary>
		/// Reference to a named connection in a config file, example
		/// DefaultConnection@web.config
		/// </summary>
		ConfigFile		
	}

	public class Settings
	{
		/// <summary>
		/// DLL of model classes to merge to the target connection
		/// </summary>
		public string SourceAssembly { get; set; }

		public TargetConnectionType TargetConnectionType { get; set; } = TargetConnectionType.ConnectionString;

		/// <summary>
		/// Connection string of merge target or config name @ config file,
		/// example: DefaultConnection@web.config
		/// </summary>
		public string TargetConnection { get; set; }

		/// <summary>
		/// Program you use to view/execute .sql files
		/// Leave blank to use shell execute behavior on .sql files
		/// </summary>
		public string CommandExe { get; set; } = "C:\\Program Files (x86)\\Microsoft SQL Server\\140\\Tools\\Binn\\ManagementStudio\\Ssms.exe";

		/// <summary>
		/// Arguments passed to CommandExe
		/// For SSMS arguments, see https://docs.microsoft.com/en-us/sql/ssms/ssms-utility?view=sql-server-2017
		/// Use connection string tokens (i.e. Data Source, Database, User Id, etc) enclosed in curly braces for variable insertion
		/// </summary>
		public string CommandArguments { get; set; } = "\"%script_file%\" -S {Server} -d {Database} -E";

		/// <summary>
		/// Source objects to exclude
		/// </summary>
		public HashSet<ExcludeObject> ExcludeObjects { get; set; }		

		public TargetConnectionInfo GetTargetInfo()
		{
			if (TargetConnectionType == TargetConnectionType.ConfigFile)
			{
				var parts = TargetConnection.Split('@');
				if (parts.Length == 2) return new TargetConnectionInfo() { ConnectionName = parts[0], Filename = parts[1] };
				throw new InvalidOperationException($@"The TargetConnection property is not well-formed for retrieving a filename: {TargetConnection}");
			}
			throw new InvalidOperationException($"TargetConnectionType must be set to {nameof(TargetConnectionType.ConfigFile)}");
		}

		public class TargetConnectionInfo
		{
			public string ConnectionName { get; set; }
			public string Filename { get; set; }
		}
	}
}
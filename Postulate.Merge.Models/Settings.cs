using SchemaSync.Library.Models;
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
		/// Source objects to exclude
		/// </summary>
		public HashSet<ExcludeObject> ExcludeObjects { get; set; }
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Postulate.Merge.Models.Models
{
	/// <summary>
	/// Represents a connection string within the connectionStrings section in a web.config file
	/// </summary>
	public class NamedConnection
	{
		public string Name { get; set; }
		public string ConnectionString { get; set; }
	}
}

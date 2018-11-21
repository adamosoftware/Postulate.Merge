using System;
using System.Collections.Generic;
using System.Linq;

namespace Postulate.Merge.SqlServer
{
	/// <summary>
	/// Utility class for working with connection strings
	/// </summary>
	internal static class ConnectionStrings
	{
		public static Dictionary<string, string> Parse(string connectionString)
		{
			var items = connectionString.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
			var keyPairs = items.Where(s => HasTwoParts(s)).Select(s =>
			{
				var parts = s.Split('=');
				return new KeyValuePair<string, string>(parts[0].Trim(), parts[1].Trim());
			});
			return keyPairs.ToDictionary(item => item.Key, item => item.Value);
		}

		private static bool HasTwoParts(string input)
		{
			var parts = input.Split('=');
			return (parts.Length == 2);
		}

		public static bool HasPassword(string connectionString)
		{
			var dictionary = Parse(connectionString);
			return dictionary.ContainsKey("Password");
		}
	}
}
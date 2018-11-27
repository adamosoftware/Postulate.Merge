using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Postulate.Merge.SqlServer
{
	internal static class DictionaryExtensions
	{
		public static TValue TryGetValues<TValue>(this Dictionary<string, TValue> dictionary, params string[] keys)
		{
			string firstKey = keys.First(s => dictionary.ContainsKey(s));
			return dictionary[firstKey];
		}
	}
}

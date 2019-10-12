using System.Collections.Generic;

namespace Smab.DebugInfo.Models
{
	public partial class MVCStructure
	{
		public class MVCAction
		{
			public string Name { get; set; } = "";
			public SortedDictionary<string, string> Parameters = new SortedDictionary<string, string>();
		}
	}

}

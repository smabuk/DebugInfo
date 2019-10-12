using System.Collections.Generic;

namespace Smab.DebugInfo.Models
{
	public partial class MVCStructure
	{
		public class MVCController
		{
			public string Name { get; set; } = "";
			public List<string> Fields = new List<string>();
			public List<MVCAction> Actions = new List<MVCAction>();
		}
	}

}

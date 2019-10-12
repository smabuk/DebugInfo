using System.Collections.Generic;
using System.Linq;

namespace Smab.DebugInfo.Models
{
	public class AssembliesInformation
	{
		public List<AssemblyInfo> AllAssemblies = new List<AssemblyInfo>();

		public List<AssemblyInfo> MicrosoftAssemblies => (
			from a in AllAssemblies
			where (a.IsMicrosoft)
			where (a.IsMicrosoft)
			select a
			).ToList();
		public List<AssemblyInfo> OtherAssemblies => (
			from a in AllAssemblies
			where !(a.IsMicrosoft)
			select a
			).ToList();
	}
}

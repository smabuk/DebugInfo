using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace Smab.DebugInfo.Models
{
	public class AssemblyInfo
	{
		public Assembly? Assembly { get; set; }

		public string Name { get; set; } = "";
		public string AssemblyVersion => Assembly?
			.GetName()
			.Version
			.ToString() ?? "";
		public string FileVersion => Assembly?
			.GetCustomAttribute<AssemblyFileVersionAttribute>()?
			.Version ?? "";
		public string ProductVersion => Assembly?
			.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
			.InformationalVersion ?? "";
		public string Location => IsDynamic ? "" : Assembly?.Location ?? "";
		public string CodeBase => IsDynamic ? "" : Assembly?.CodeBase ?? "";

		public SortedDictionary<string, string> CustomAttributes
		{
			get
			{
				int i = 0;
				SortedDictionary<string, string> ca1 = new SortedDictionary<string, string>();
				if (Assembly != null)
				{
					foreach (var ca in Assembly.CustomAttributes.Where(ca => ca.AttributeType.ToString().StartsWith("System.Reflection.Assembly")))
					{
						if (ca1.ContainsKey(ca.AttributeType.ToString().Replace("System.Reflection.Assembly", "").Replace("Attribute", "")))
						{
							i++;
							ca1.Add(ca.AttributeType.ToString().Replace("System.Reflection.Assembly", "").Replace("Attribute", "") + "_" + i.ToString(), ca.ConstructorArguments[0].Value.ToString());
						}
						else
						{
							ca1.Add(ca.AttributeType.ToString().Replace("System.Reflection.Assembly", "").Replace("Attribute", ""), ca.ConstructorArguments[0].Value.ToString());
						}
					}
				}
				return ca1;
			}
		}

		public bool IsDynamic => Assembly?.IsDynamic ?? false;
		public bool IsMicrosoft => (
							Name.StartsWith("Microsoft.")
						 || Name.StartsWith("Newtonsoft.Json")
						 || Name.StartsWith("ProductionBreakpoints")
						 || Name.StartsWith("Remotion")
						 || Name.StartsWith("SQLitePCLRaw")
						 || Name.StartsWith("System.")
						 || Name.StartsWith("FSharp.")
						 || Name.StartsWith("dotnet")
						 || Name.StartsWith("mscor")
						 || Name.StartsWith("netstandard"));
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Smab.DebugInfo.Pages
{
    public class DebugInfoModel : PageModel
    {
        public class MVCStructure
        {
            public class MVCAction
            {
                public string Name { get; set; } = "";
				public SortedDictionary<string, string> Parameters = new SortedDictionary<string, string>();
            }
            public class MVCController
            {
                public string Name { get; set; } = "";
				public List<string> Fields = new List<string>();
                public List<MVCAction> Actions = new List<MVCAction>();
            }
            public List<MVCController> Controllers { get; set; } = new List<MVCController>();
        }

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
        public class AssembliesInformation
        {
            public List<AssemblyInfo> AllAssemblies = new List<AssemblyInfo>();

            public List<AssemblyInfo> MicrosoftAssemblies => (from a in AllAssemblies
                                                              where (a.IsMicrosoft)
                                                              select a
                         ).ToList();
            public List<AssemblyInfo> OtherAssemblies => (from a in AllAssemblies
                                                          where !(a.IsMicrosoft)
                                                          select a
                         ).ToList();
        }

        public SortedDictionary<string, string> EnvironmentVariablesInfo = new SortedDictionary<string, string>();
        public SortedDictionary<string, string> RequestHeadersInfo = new SortedDictionary<string, string>();
        public SortedDictionary<string, string> QueryStringsInfo = new SortedDictionary<string, string>();
        public SortedDictionary<string, string> CookiesInfo = new SortedDictionary<string, string>();
        public string EnvironmentFromJson = "";
        public string ConfigFromJson = "";
        public string ProviderInfo = "";
        public MVCStructure MvcInfo = new MVCStructure();
        public AssembliesInformation AssembliesInfo = new AssembliesInformation();


        public string PreformattedMessage { get; set; } = "";


		private System.Text.StringBuilder strText = new System.Text.StringBuilder("");

        private readonly IHostingEnvironment _env;
        private readonly IConfiguration _config;
        private readonly ILogger _logger;

        public DebugInfoModel(
            ILogger<DebugInfoModel> logger,
            IHostingEnvironment env,
            IConfiguration config
            )
        {
            _logger = logger;
            _env = env;
            _config = config;
        }

        public void OnGet()
        {
            Debug.WriteLine($"Starting OnGet in {nameof(DebugInfoModel.OnGet)}");

            EnvironmentVariablesInfo.Clear();
            foreach (DictionaryEntry de in Environment.GetEnvironmentVariables())
            {
                string key = de.Key.ToString().ToLowerInvariant();
                if (key.Contains("secret") || key.Contains("key") || key.Contains("password") || key.Contains("pwd") || key.StartsWith("rules") || key.EndsWith("address"))
                {
                    EnvironmentVariablesInfo.TryAdd(de.Key.ToString(), "****************");
                }
                else
                {
                    EnvironmentVariablesInfo.TryAdd(de.Key.ToString(), de.Value.ToString());
                }
            }
            foreach (var hd in Request.Headers)
            {
                RequestHeadersInfo.TryAdd(hd.Key.ToString(), hd.Value.ToString());
            }
            foreach (var q in Request.Query)
            {
                QueryStringsInfo.TryAdd(q.Key.ToString(), q.Value.ToString());
            }
            foreach (var c in Request.Cookies)
            {
                CookiesInfo.TryAdd(c.Key.ToString(), c.Value.ToString());
            }

            EnvironmentFromJson = $"EnvironmentName: {_env.EnvironmentName}";
            EnvironmentFromJson += Environment.NewLine + $"ApplicationName: {_env.ApplicationName}";
            EnvironmentFromJson += Environment.NewLine + $"WebRootPath: {_env.WebRootPath}";
            EnvironmentFromJson += Environment.NewLine + $"ContentRootPath: {_env.ContentRootPath}";

            // ToDo Broken in 2.2 with Newtonsoft 12.xxx
            try
            {
                EnvironmentFromJson = JsonConvert.SerializeObject(_env, Formatting.Indented);
            }
            catch (Exception ex)
            {
				_logger.LogError("Serialize IHostingEnvironment Error: {Error}", ex.Message);

                EnvironmentFromJson = $"EnvironmentName: {_env.EnvironmentName}";
                EnvironmentFromJson += Environment.NewLine + $"ApplicationName: {_env.ApplicationName}";
                EnvironmentFromJson += Environment.NewLine + $"WebRootPath: {_env.WebRootPath}";
                EnvironmentFromJson += Environment.NewLine + $"ContentRootPath: {_env.ContentRootPath}";
            }
            try
            {
                ConfigFromJson = JsonConvert.SerializeObject(_config, Formatting.Indented);
            }
            catch (Exception ex)
            {
                ConfigFromJson = "Broken in ASP.NET Core 2.2 with Newtonsoft 12.0.0.0";
                ConfigFromJson += Environment.NewLine + Environment.NewLine;
                ConfigFromJson += "JsonConvert.SerializeObject(_config, Formatting.Indented)";
                ConfigFromJson += Environment.NewLine + $"{ex.Message}";
            }

            var mvcH = new DebugMvcHelper();
            foreach (var controller in mvcH.GetControllers<Controller>().OrderBy(i => i.Name))
            {
                var c = new MVCStructure.MVCController
                {
                    Name = controller.Name
                };

                mvcH.GetDeclaredFields(controller.Name)
                    .ForEach(f => c.Fields.Add(f.Name));
                mvcH.GetDeclaredMethods(controller.Name)
                    .Where(m => m.IsPublic)
                    .ToList()
                    .ForEach(f =>
                    {
                        var action = new MVCStructure.MVCAction
                        {
                            Name = f.Name
                        };
                        f.GetParameters().ToList().ForEach(p => action.Parameters.Add(p.Name, p.ParameterType.ToString()));
                        c.Actions.Add(action);
                    });
                MvcInfo.Controllers.Add(c);
            }

            AssembliesInfo.AllAssemblies = (from a in AppDomain.CurrentDomain.GetAssemblies()
                                            orderby a.FullName
                                            select new AssemblyInfo
                                            {
                                                Name = a.GetName().Name,
                                                Assembly = a
                                            }
                          ).ToList();

            strText.AppendLine($"Runtime OS:           {System.Runtime.InteropServices.RuntimeInformation.OSDescription}");
            strText.AppendLine($"Runtime Framework:    {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}");
            strText.AppendLine($"Process Architecture: {System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture}");
            strText.AppendLine($"OS Architecture:      {System.Runtime.InteropServices.RuntimeInformation.OSArchitecture}");
            strText.AppendLine($"Machine Name:         {Environment.MachineName.ToString()}");
            strText.AppendLine($"No of Processors:     {Environment.ProcessorCount.ToString()}");
            strText.AppendLine($"System Folder:        {Environment.GetFolderPath(Environment.SpecialFolder.System).ToString()}");
            strText.AppendLine();

            strText.AppendLine($"Runtime Version: {System.Runtime.InteropServices.RuntimeEnvironment.GetSystemVersion()}");
            strText.AppendLine($"Runtime Directory: {System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory()}");
            // strText.AppendLine($"Runtime Configuration: {System.Runtime.InteropServices.RuntimeEnvironment.SystemConfigurationFile}");
            strText.AppendLine();

            strText.AppendLine($"User Domain Name:     {Environment.UserDomainName.ToString()}");
            strText.AppendLine($"User Name:            {Environment.UserName.ToString()}");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                strText.AppendLine($"ASP.Net User Account: {System.Security.Principal.WindowsIdentity.GetCurrent().Name}");
            }
            strText.AppendLine();

            DateTime dt = DateTime.Now;

            strText.AppendLine($"CurrentThread.CurrentCulture:         {System.Threading.Thread.CurrentThread.CurrentCulture.ToString()}");
            strText.AppendLine($"CurrentThread.CurrentUICulture:       {System.Threading.Thread.CurrentThread.CurrentUICulture.ToString()}");
            strText.AppendLine($"Current Server Date using UI Culture: {dt.ToString("D", System.Threading.Thread.CurrentThread.CurrentUICulture)}");
            strText.AppendLine($"Current Server Time using UI Culture: {dt.ToString("T", System.Threading.Thread.CurrentThread.CurrentUICulture)}");
            strText.AppendLine($"Universal DateTime (UTC):             {dt.ToUniversalTime().ToString("u")}");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                strText.AppendLine($"UK Time:                              {TimeZoneInfo.ConvertTimeFromUtc(dt.ToUniversalTime(), TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time")).ToString("r")}");
            }
            else
            {
                strText.AppendLine($"UK Time:                              {TimeZoneInfo.ConvertTimeFromUtc(dt.ToUniversalTime(), TimeZoneInfo.FindSystemTimeZoneById("GMT")).ToString("r")}");
            };
            // strText.AppendLine($"Current TimeZone:                     {TimeZone.CurrentTimeZone.StandardName}"); // Obsolete
            // strText.AppendLine($"Utc Offset:                           {TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).ToString()}"); // Obsolete
            strText.AppendLine($"Current TimeZone:                     {TimeZoneInfo.Local.StandardName}");
            strText.AppendLine($"Utc Offset:                           {TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).ToString()}");
            strText.AppendLine();

            strText.AppendLine($"Is Request HTTPS?:                 {Request.IsHttps.ToString()}");
            strText.AppendLine($"Request.Host.Value:                {Request.Host.Value}");
            strText.AppendLine($"Request.Path.Value:                {Request.Path.Value}");
            strText.AppendLine($"Request.PathBase.Value:            {Request.PathBase.Value}");
            strText.AppendLine($"Request.HttpContext Ipaddress:     {Request.HttpContext.Connection.LocalIpAddress}");
            strText.AppendLine($"Request.HttpContext Identity.Name: {Request.HttpContext.User.Identity.Name}");
            strText.AppendLine();

            PreformattedMessage = strText.ToString();

            strText.Clear();

            IConfigurationRoot configurationRoot = (IConfigurationRoot)_config;
            foreach (IConfigurationProvider provider in configurationRoot.Providers)
            {
                strText.AppendLine($"Provider: {provider.ToString().Replace("Microsoft.Extensions.Configuration.", "")}");
                if (provider is FileConfigurationProvider f)
                {
                    strText.AppendLine($"  Source: {f.Source.Path}");
                }

                List<string> keys = new List<string>();
                string value = "";

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
				foreach (var key in GetProviderKeys(provider, null))
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
				{
                    provider.TryGet(key, out value);
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        if (key.ToLowerInvariant().Contains("secret") || key.ToLowerInvariant().Contains("key") || key.ToLowerInvariant().Contains("password") || key.ToLowerInvariant().Contains("pwd") || key.ToLowerInvariant().StartsWith("rules") || key.ToLowerInvariant().EndsWith("address"))
                        {
                            value = "********";
                        }
                        strText.AppendLine($"  {key,40}   ==>   {value}");
                    }
                }
            }
            strText.AppendLine();
            ProviderInfo = strText.ToString();

        }


        private static IEnumerable<string> GetProviderKeys(IConfigurationProvider provider,
            string parentPath)
        {
            var prefix = parentPath == null
                    ? string.Empty
                    : parentPath + ConfigurationPath.KeyDelimiter;

            List<string> keys = new List<string>();
            var childKeys = provider.GetChildKeys(Enumerable.Empty<string>(), parentPath)
                .Distinct()
                .Select(k => prefix + k).ToList();
            keys.AddRange(childKeys);
            foreach (var key in childKeys)
            {
                keys.AddRange(GetProviderKeys(provider, key));
            }

            return keys;
        }

    }



    class DebugMvcHelper
    {
        private static List<Type> GetSubClasses<T>()
        {
            return AppDomain
                .CurrentDomain
                .GetAssemblies()
                .SelectMany(
                    a => a.GetTypes().Where(type => type.IsSubclassOf(typeof(T)))
                )
                .Distinct()
                .ToList();
        }

        public List<Type> GetControllers<T>()
        {
            List<Type> controllers = new List<Type>();
            GetSubClasses<Controller>().ForEach(
                type => controllers.Add(type.GetTypeInfo()));
            return controllers;
        }

        public List<string> GetControllerNames()
        {
            List<string> controllerNames = new List<string>();
            GetSubClasses<Controller>().ForEach(
                type => controllerNames.Add(type.Name));
            return controllerNames;
        }

        public List<FieldInfo> GetDeclaredFields(string ControllerName)
        {
            List<FieldInfo> fields = new List<FieldInfo>();

            foreach (var controller in GetSubClasses<Controller>())
            {
                if (controller.Name == ControllerName)
                {
                    controller.GetTypeInfo().DeclaredFields.ToList().ForEach(f => fields.Add(f));
                }
            }
            return fields;
        }
        public List<MethodInfo> GetDeclaredMethods(string ControllerName)
        {
            List<MethodInfo> actions = new List<MethodInfo>();

            foreach (var controller in GetSubClasses<Controller>())
            {
                if (controller.Name == ControllerName)
                {
                    var methods = controller.GetTypeInfo().DeclaredMethods;
                    foreach (var m in methods)
                    {
                        actions.Add(m);
                    }
                }
            }
            return actions;
        }

        public List<string> GetActionNames(string ControllerName)
        {
            List<string> actionNames = new List<string>();

            foreach (var controller in GetSubClasses<Controller>())
            {
                if (controller.Name == ControllerName)
                {
                    var methods = controller.GetTypeInfo().DeclaredMethods;
                    foreach (var info in methods)
                    {
                        actionNames.Add(info.Name);
                    }
                }
            }
            return actionNames;
        }
    }
}

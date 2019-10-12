using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace Smab.DebugInfo.Helpers
{
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

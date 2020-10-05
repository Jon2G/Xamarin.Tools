using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Plugin.Xamarin.Tools.Shared.Reflection
{
    public class ReflectionCaller : IDisposable
    {
        private Assembly Dll;
        private Type Type;
        private PropertyInfo Property;
        public ReflectionCaller()
        {

        }
        public ReflectionCaller(string AssemblyName)
        {
            string Library = Plugin.Xamarin.Tools.Shared.Tools.Instance.LibraryPath;
            this.Dll = SearchAssembly(AssemblyName);
            if (this.Dll is null)
            {
                this.Dll = Assembly.LoadFile($"{Library}\\{AssemblyName}");
            }
        }
        private Assembly SearchAssembly(string AssemblyName)
        {
            Assembly[] LoadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in LoadedAssemblies)
            {
                if (assembly.ManifestModule.Name == AssemblyName)
                {
                    return assembly;
                }
            }
            return null;
        }
        public ReflectionCaller GetAssembly(Type type)
        {
            this.Type = type;
            this.Dll = type.Assembly;
            return this;
        }
        public ReflectionCaller SetType(string TypeName)
        {
            this.Type = this.Dll.GetType(TypeName);
            return this;
        }
        public ReflectionCaller SetType(PropertyInfo propertyInfo)
        {
            this.Type = propertyInfo.GetType();
            return this;
        }
        public ReflectionCaller SetProperty(string PropertyName)
        {
            this.Property = this.Type.GetProperty(PropertyName);
            this.Type = this.Property.PropertyType;
            return this;
        }
        public object CallMethod(string MethodName, params object[] Parameters)
        {
            object result = null;
            MethodInfo methodInfo = this.Type.GetMethod(MethodName);
            if (methodInfo != null)
            {
                ParameterInfo[] parameters = methodInfo.GetParameters();
                if (parameters.Length != (Parameters?.Length ?? 0))
                {
                    throw new TargetParameterCountException("El número de parametros enviados y requeridos difiere");
                }
                object instance = this.Property.GetValue(this.Type);
                result = methodInfo.Invoke(instance, Parameters);
            }
            return result;
        }
        public object CallMethodOf(object Instance, string MethodName, params object[] Parameters)
        {
            object result = null;
            MethodInfo methodInfo = this.Type.GetMethod(MethodName);
            if (methodInfo != null)
            {
                ParameterInfo[] parameters = methodInfo.GetParameters();
                if (parameters.Length != (Parameters?.Length ?? 0))
                {
                    throw new TargetParameterCountException("El número de parametros enviados y requeridos difiere");
                }
                result = methodInfo.Invoke(Instance, Parameters);
            }
            return result;
        }
        public Stream GetResource(string ResourceName)
        {
            string fullname = this.Dll.GetManifestResourceNames().First(x => x.EndsWith(ResourceName));
            if (string.IsNullOrEmpty(ResourceName))
            {
                return null;
            }
            return this.Dll.GetManifestResourceStream(fullname);
        }
        public object Instance()
        {
            return Activator.CreateInstance(this.Type);
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public void Dispose()
        {
            this.Type = null;
            this.Property = null;
            this.Dll = null;
        }


    }
}

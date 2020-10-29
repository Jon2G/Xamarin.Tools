using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SQLHelper.Reflection
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
            string Library = SQLHelper.Instance.LibraryPath;
            Dll = SearchAssembly(AssemblyName);
            if (Dll is null)
            {
                Dll = Assembly.LoadFile($"{Library}\\{AssemblyName}");
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
            Type = type;
            Dll = type.Assembly;
            return this;
        }
        public ReflectionCaller SetType(string TypeName)
        {
            Type = Dll.GetType(TypeName);
            return this;
        }
        public ReflectionCaller SetType(PropertyInfo propertyInfo)
        {
            Type = propertyInfo.GetType();
            return this;
        }
        public ReflectionCaller SetProperty(string PropertyName)
        {
            Property = Type.GetProperty(PropertyName);
            Type = Property.PropertyType;
            return this;
        }
        public object CallMethod(string MethodName, params object[] Parameters)
        {
            object result = null;
            MethodInfo methodInfo = Type.GetMethod(MethodName);
            if (methodInfo != null)
            {
                ParameterInfo[] parameters = methodInfo.GetParameters();
                if (parameters.Length != (Parameters?.Length ?? 0))
                {
                    throw new TargetParameterCountException("El número de parametros enviados y requeridos difiere");
                }
                object instance = Property.GetValue(Type);
                result = methodInfo.Invoke(instance, Parameters);
            }
            return result;
        }
        public object CallMethodOf(object Instance, string MethodName, params object[] Parameters)
        {
            object result = null;
            MethodInfo methodInfo = Type.GetMethod(MethodName);
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
            string fullname = Dll.GetManifestResourceNames().First(x => x.EndsWith(ResourceName));
            if (string.IsNullOrEmpty(ResourceName))
            {
                return null;
            }
            return Dll.GetManifestResourceStream(fullname);
        }
        public object Instance()
        {
            return Activator.CreateInstance(Type);
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public void Dispose()
        {
            Type = null;
            Property = null;
            Dll = null;
        }


    }
}

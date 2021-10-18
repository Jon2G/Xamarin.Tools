using Ninject;
using Ninject.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kit.Ninject
{
    public class IocKernel
    {
        private static StandardKernel _kernel;

        public static T Get<T>()
        {
            if (_kernel is null)
            {
                return default(T);
            }
            return _kernel.Get<T>() ?? default(T);
        }

        public static void Initialize(params INinjectModule[] modules)
        {
            if (_kernel == null)
            {
                _kernel = new StandardKernel(new NinjectSettings() { LoadExtensions = false }, modules);
            }
        }
    }
}
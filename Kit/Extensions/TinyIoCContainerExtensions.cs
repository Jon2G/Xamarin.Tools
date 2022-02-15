using System;
using System.Collections.Generic;
using System.Text;
using TinyIoC;

namespace Kit
{
    public static class TinyIoCContainerExtensions
    {
        public static T Get<T>(this TinyIoCContainer container) where T : class
        {
            if(container.TryResolve<T>(out T resolved))
            {
                return resolved;
            }
            return null;
        }
    }
}

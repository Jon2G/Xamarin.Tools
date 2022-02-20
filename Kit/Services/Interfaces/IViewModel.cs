using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Kit.Model;

namespace Kit.Services.Interfaces
{
    public interface IViewModel<T> : ICrossWindow where T : ModelBase
    {
        public T Model
        {
            get;
            set;
        }
    }
}

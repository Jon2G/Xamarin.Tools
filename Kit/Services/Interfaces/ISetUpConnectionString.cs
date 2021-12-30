using Kit.SetUpConnectionString;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kit.Services.Interfaces
{
    public interface ISetUpConnectionString : ICrossWindow
    {
        public SetUpConnectionStringViewModelBase Model { get; set; }
    }
}

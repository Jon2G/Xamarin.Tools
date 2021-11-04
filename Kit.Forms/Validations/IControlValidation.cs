using System;
using System.Collections.Generic;
using System.Text;

namespace Kit.Forms.Validations
{
    public interface IControlValidation
    {
        bool HasError { get; }
        string ErrorMessage { get; }
        bool ShowErrorMessage { get; set; }
    }
}

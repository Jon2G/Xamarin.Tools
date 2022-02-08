using System;
using System.Collections.Generic;
using System.Text;
using Kit.Sql.Attributes;

namespace Kit.Forms.Validations
{
    [Preserve(AllMembers = true)]
    public interface IControlValidation
    {
        bool HasError { get; }
        string ErrorMessage { get; }
        bool ShowErrorMessage { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Kit.Services.Interfaces
{
    public interface IBrightnessService
    {
        void SetBrightness(float factor);
        float GetBrightness();
    }
}

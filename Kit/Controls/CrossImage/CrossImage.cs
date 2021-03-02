using System;
using System.Collections.Generic;
using System.Text;

namespace Kit.Controls.CrossImage
{
    public abstract class CrossImage : ModelBase
    {
        private object _Native;
        public object Native
        {
            get => _Native;
            set
            {
                _Native = value;
                Raise(() => Native);
            }
        }

        public abstract byte[] ToArray();
    }
}

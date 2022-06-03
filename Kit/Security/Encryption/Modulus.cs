using System;

namespace Kit.Security.Encryption
{
    public static class Modulus
    {
        public static int Mod(int a, int b)
        {
            return (Math.Abs(a * b) + a) % b;
        }
    }
}

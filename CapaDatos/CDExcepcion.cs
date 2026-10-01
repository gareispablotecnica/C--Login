using System;

namespace CapaDatos
{
    public class CDExcepcion : Exception
    {
        public CDExcepcion(string mensaje)
            : base(mensaje)
        {
        }

        public CDExcepcion(string mensaje, Exception excepcionInterna)
            : base(mensaje, excepcionInterna)
        {
        }
    }
}
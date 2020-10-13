using System;
using System.Collections.Generic;
using System.Text;

namespace Tools.CadenaConexion
{
    public enum ConnectionState
    {
        /// <summary>
        /// The connection string is not set 
        /// </summary>
        NotSet = 0,
        ///// <summary>
        ///// The connection string is set up but is not valid
        ///// </summary>
        //InvalidFormat=1,
        /// <summary>
        /// The connection string is set up an valid but fails reaching the server
        /// </summary>
        Unavaible = 2,
        /// <summary>
        /// The connection string is set up,valid and the connection with the server is ok
        /// </summary>
        OK = 3,
        /// <summary>
        /// Unknown state [NO INFORMATION]
        /// </summary>
        Unknown = -1
    }
}

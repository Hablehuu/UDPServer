using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace UDPServer
{


    internal class VirtualSocket : Socket
    {
        private static double p_drop = 0.5; //probability to drop a package

        public VirtualSocket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType) : base(addressFamily, socketType, protocolType)
        {
        }
    }
}

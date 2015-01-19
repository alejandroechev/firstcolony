using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkModule
{
    public interface INetworkObject
    {
        byte[] Encode();

        void Decode(byte[] data);
    }
}

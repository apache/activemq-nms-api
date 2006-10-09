using System;
using System.Collections.Generic;
using System.Text;
using ActiveMQ.OpenWire;

namespace ActiveMQ.OpenWire
{
    interface IMarshallerFactory
    {
        void configure(OpenWireFormat format);
    }
}

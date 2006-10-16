using System;
using System.Text;
using ActiveMQ.OpenWire;

namespace ActiveMQ.OpenWire
{
    interface IMarshallerFactory
    {
        void configure(OpenWireFormat format);
    }
}

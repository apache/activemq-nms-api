/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Specialized;
using Apache.ActiveMQ.OpenWire;
using Apache.ActiveMQ.Transport.Stomp;
using Apache.ActiveMQ.Util;
using Apache.NMS;

namespace Apache.ActiveMQ.Transport.Tcp
{
	public class TcpTransportFactory : ITransportFactory
	{
		private bool useLogging = false;

		public TcpTransportFactory()
		{
		}

		public bool UseLogging
		{
			get { return useLogging; }
			set { useLogging = value; }
		}

		#region ITransportFactory Members

		public ITransport CreateTransport(Uri location)
		{
			// Extract query parameters from broker Uri
			StringDictionary map = URISupport.ParseQuery(location.Query);

			// Set transport. properties on this (the factory)
			URISupport.SetProperties(this, map, "transport.");

			Tracer.Debug("Opening socket to: " + location.Host + " on port: " + location.Port);
			Socket socket = Connect(location.Host, location.Port);
			IWireFormat wireformat = CreateWireFormat(location, map);
			ITransport transport = new TcpTransport(socket, wireformat);

			wireformat.Transport = transport;

			if(UseLogging)
			{
				transport = new LoggingTransport(transport);
			}

			if(wireformat is OpenWireFormat)
			{
				transport = new WireFormatNegotiator(transport, (OpenWireFormat) wireformat);
			}

			transport = new MutexTransport(transport);
			transport = new ResponseCorrelator(transport);

			return transport;
		}

		#endregion

		protected Socket Connect(string host, int port)
		{
			// Looping through the AddressList allows different type of connections to be tried
			// (IPv4, IPv6 and whatever else may be available).
			IPHostEntry hostEntry = Dns.GetHostEntry(host);
			foreach(IPAddress address in hostEntry.AddressList)
			{
				Socket socket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
				socket.Connect(new IPEndPoint(address, port));
				if(socket.Connected)
				{
					return socket;
				}
			}
			throw new SocketException();
		}

		protected IWireFormat CreateWireFormat(Uri location, StringDictionary map)
		{
			object properties = null;
			IWireFormat wireFormat = null;

			// Detect STOMP etc
			if(String.Compare(location.Scheme, "stomp", true) == 0)
			{
				wireFormat = new StompWireFormat();
				properties = wireFormat;
			}
			else
			{
				OpenWireFormat openwireFormat = new OpenWireFormat();

				wireFormat = openwireFormat;
				properties = openwireFormat.PreferedWireFormatInfo;
			}

			if(null != properties)
			{
				// Set wireformat. properties on the wireformat owned by the tcpTransport
				URISupport.SetProperties(properties, map, "wireFormat.");
			}

			return wireFormat;
		}
	}
}

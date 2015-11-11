// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net;
using System.Net.Sockets;
using Microsoft.AspNet.Testing.xunit;

namespace Microsoft.AspNet.Server.Kestrel.FunctionalTests
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class IPv6SupportedConditionAttribute : Attribute, ITestCondition
    {
        private static Lazy<bool> _ipv6Supported = new Lazy<bool>(CanBindToIPv6Address);

        public bool IsMet
        {
            get
            {
                return _ipv6Supported.Value;
            }
        }

        public string SkipReason
        {
            get
            {
                return "IPv6 not supported on the host.";
            }
        }

        private static bool CanBindToIPv6Address()
        {
            try
            {
                using (var socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp))
                {
                    socket.Bind(new IPEndPoint(IPAddress.IPv6Loopback, 8787));
                    return true;
                }
            }
            catch (SocketException)
            {
                return false;
            }
        }
    }
}

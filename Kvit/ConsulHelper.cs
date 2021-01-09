using System;
using Consul;

namespace Kvit
{
    public static class ConsulHelper
    {
        public static ConsulClient CreateConsulClient(Uri address, string token)
        {
            var addressFromEnvironment = Environment.GetEnvironmentVariable(EnvironmentVariables.KVIT_ADDRESS);
            var addressUriFromEnvironment = addressFromEnvironment == null ? null : new Uri(addressFromEnvironment);
            var tokenFromEnvironment = Environment.GetEnvironmentVariable(EnvironmentVariables.KVIT_TOKEN);

            return new ConsulClient(c =>
            {
                c.Address = address ?? addressUriFromEnvironment ?? Common.DefaultConsulUri;
                c.Token = token ?? tokenFromEnvironment;
            });
        }
    }
}
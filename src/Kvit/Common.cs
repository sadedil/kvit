using System;

namespace Kvit
{
    public static class Common
    {
        public const int ConsulTransactionMaximumOperationCount = 64;

        public static readonly Uri DefaultConsulUri = new Uri("http://localhost:8500");

        public static readonly string BaseDirectory = 
            Environment.GetEnvironmentVariable(EnvironmentVariables.KVIT_BASE_DIR) ?? "./";
    }
}
using System;
namespace aidemo
{
        public class Error
        {
            public string code { get; set; }
            public string message { get; set; }
        }

        public class ErrorObj
        {
            public Error error { get; set; }
        }
}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPC.CommonLibrary
{
    public enum HttpMethod
    {
        DELETE,
        GET,
        POST,
        PUT
    }

    public enum MessageFormat
    {
        SOAP,
        JSON,
        XML
    }
}

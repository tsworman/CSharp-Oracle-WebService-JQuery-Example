using System;
using System.Collections.Generic;


namespace WebServiceJQueryExample
{
    public class Response
    {
        public bool Success = true;
        public IEnumerable<Dictionary<string, object>> ResultSet;
        public String Message;
        public String Parameters;
        public String Caller;
        public String ErrorType;
    }
}
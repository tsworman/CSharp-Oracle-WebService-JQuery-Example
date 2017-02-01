using System;
using System.Text;
using System.Web.Services;
using System.Web.Script.Serialization;
using System.Web.Script.Services;
using Oracle.DataAccess.Client;
using System.Data;
using Newtonsoft.Json;

namespace WebServiceJQueryExample
{

    /// <summary>
    /// Sample methods to handle login, logoff, query, function and procedure call to Oracle database.
    /// Results are returned as JSON. Code for JSONP is supported below for cross server requests.
    /// To Enable JSONP:
    /// - wrap the context.response.write result in the method JSONPIFY(data,callback).
    /// - Pass a callback name to the function.
    /// - In each function ScriptMethod add: UseHttpGet = true
    /// - Turn on HTTPGet by adding the protocol to the web.config <add name="HttpGet"/>
    /// Why not to enable JSONP:
    /// If you use JSONP all requests are made by HTTPGet. Since our login is handled in application, 
    /// using HTTPGet to that will result in the password showing in the log files and being passed
    /// plain text in the get by the browser. If you are removing session support and moving to some 
    /// fixed service account JSONP support can be a valid use case.
    /// 
    /// Why would you handle sessions and not use a service account to connect to the database:
    /// The application this example was written for is used to manage user data and their account directly
    /// It logs in as them. Another option here would be to use the proxy packages like oracle APEX uses.
    /// It's entirely possible to work around this restriction if you want to go that route or expose
    /// functions to the service accounts and the proper grants to them to manage user objects.
    /// 
    /// </summary>
    [WebService(Namespace = "https://novaslp.net/JQuery")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    [ScriptService]
    public class HSDWWebApplication : System.Web.Services.WebService
    {

        //Test a standard select statement again'st an oracle database when logged in.
        //This request is done via Post and return JSON. 
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public void TestSQL(String callback)
        {

            ConnectionHandler LoginObj = GetConnection();
            OracleCommand cmd = LoginObj.GetCommand();

            cmd.CommandText = "select 'test' h from dual union select 'test2' h from dual";
            cmd.CommandType = CommandType.Text;
            OracleDataReader dr = cmd.ExecuteReader();
            OracleDataHandler dh = new OracleDataHandler();

            Response NewResponse = new Response();
            NewResponse.Success = true;
            NewResponse.ResultSet = dh.Serialize(dr);
            NewResponse.Message = "Test";

            //Return results as JSON.
            Context.Response.Clear();
            Context.Response.ContentType = "application/json";
            Context.Response.Write(JsonConvert.SerializeObject(NewResponse, Formatting.Indented));
            Context.Response.End();
        }

        //Test call to a procedure in a stored procedure package.
        //In this example the procedure takes an integer and returns
        //a cursor to the web service as one of the parameters.
        //
        //This request is done via Post and return JSON. 
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public void TestSQLProc(String callback)
        {
            //Create objects

            ConnectionHandler LoginObj = GetConnection();
            OracleCommand cmd = LoginObj.GetCommand();
            OracleDataAdapter da = new OracleDataAdapter();
            OracleDataHandler dh = new OracleDataHandler();

            cmd.CommandText = "somepackage.get_function";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("V_KEY", OracleDbType.Int32).Value = 123456;
            cmd.Parameters.Add("T_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
            cmd.ExecuteNonQuery();

            da.SelectCommand = cmd;
            DataTable dt = new DataTable();
            da.Fill(dt);

            Response NewResponse = new Response();
            NewResponse.Success = true;
            NewResponse.ResultSet = dh.Serialize(dt);
            NewResponse.Message = "Test Function";

            //Return results as a Json.
            Context.Response.Clear();
            Context.Response.ContentType = "application/json";
            Context.Response.Write(JsonConvert.SerializeObject(NewResponse, Formatting.Indented));
            Context.Response.End();
        }


        //Test call to a function.
        //In this example the function takes an integer and returns
        //a cursor to the web service.
        //It's important to note that when specifying the return value from a function
        //it must be specified first before any input parameters.
        //
        //This request is done via Post and return JSON. 
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public void TestSQLFunc(String callback)
        {
            ConnectionHandler LoginObj = GetConnection();
            OracleCommand cmd = LoginObj.GetCommand();
            Response NewResponse = new Response();

            cmd.CommandText = "testfunc";
            cmd.CommandType = CommandType.StoredProcedure;

            //Return param must be the first in collection
            cmd.Parameters.Add("O_VARCHAR", OracleDbType.Varchar2, 130, null, ParameterDirection.ReturnValue);
            cmd.Parameters.Add("PARAM1", OracleDbType.Varchar2).Value = "TestText";

            try
            {
                cmd.ExecuteNonQuery();
                NewResponse.Success = true;
                NewResponse.Message = "Test:" + cmd.Parameters["O_VARCHAR"].Value;

            }
            catch (OracleException ex)
            {
                ErrorHandler.CreateError(NewResponse, ex, "QUERY", "TestSQLFunc");
            }

            Context.Response.Clear();
            Context.Response.ContentType = "application/json";
            Context.Response.Write(JsonConvert.SerializeObject(NewResponse, Formatting.Indented));
            Context.Response.End();

        }

        //The Login function
        //Attempt a login to Oracle, store information and the connection during the session.
        //This session will live as long as the timeout on the APP pool doesn't die.
        //In this example our server cycles the app pool if it sees no activity for 20 minutes.
        //This won't work in an environment with multiple web servers as the login session will only be valid
        //on the first while the request could go to another server.
        //
        //This request is done via Post and return JSON. 
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public void Login(String server, String user_id, String password)
        {
            ConnectionHandler LoginObj;
            Response NewResponse = new Response();

            LoginObj = new ConnectionHandler();
            LoginObj.SetLogin(user_id, password, server);

            try
            {
                OracleCommand cmd = LoginObj.Connect(); //This forces disconnect
                Session["SESSION_LOGIN"] = LoginObj; //Store connected session for later use.
                NewResponse.Success = true;
                NewResponse.Message = "Successful Login";
            }
            catch (Exception ex)
            {
                ErrorHandler.CreateError(NewResponse, ex, "ORACLE", "Login");
            }

            Context.Response.Clear();
            Context.Response.ContentType = "application/json";
            Context.Response.Write(JsonConvert.SerializeObject(NewResponse, Formatting.Indented));
            Context.Response.End();
        }


        //The Logoff function
        //This closes a session and logs off the oracle instance. 
        //
        //This request is done via Post and return JSON. 
        [WebMethod(EnableSession = true)]
        [ScriptMethod(UseHttpGet = true, ResponseFormat = ResponseFormat.Json)]
        public void Logoff(String callback)
        {
            ConnectionHandler LoginObj = GetConnection();
            Response NewResponse = new Response();

            try
            {
                LoginObj.Disconnect();
                Session.Remove("SESSION_LOGIN");
                NewResponse.Success = true;
                NewResponse.Message = "Successful Logoff";
            }
            catch (Exception ex)
            {
                ErrorHandler.CreateError(NewResponse, ex, "ORACLE", "Logoff");
            }

            Context.Response.Clear();
            Context.Response.ContentType = "application/json";
            Context.Response.Write(JsonConvert.SerializeObject(NewResponse, Formatting.Indented));
            Context.Response.End();

        }


        //Test run random select function
        //This allows a user to run any SQL statement they wish as the logged in user.
        //This shouldn't be exposed on something using a service account unless it's
        //been properly secured.
        //
        //This request is setup to work as a HTTPGet and return a JSONP.
        //This function will not work without changes to the web.config
        //This is deliberate so it's not accidentally exposed.
        [WebMethod(EnableSession = true)]
        [ScriptMethod(UseHttpGet = true, ResponseFormat = ResponseFormat.Json)]
        private void RunSelectString(String sql, String callback)
        {

            ConnectionHandler LoginObj = GetConnection();
            OracleCommand cmd = LoginObj.GetCommand();

            cmd.CommandText = sql;
            cmd.CommandType = CommandType.Text;
            OracleDataReader dr = cmd.ExecuteReader();
            dr.Read();

            //Return results as a Json.
            JavaScriptSerializer js = new JavaScriptSerializer();

            Context.Response.Clear();
            Context.Response.ContentType = "application/json";
            Context.Response.Write(js.Serialize(dr));
            Context.Response.End();
        }



        //This function add the callback to a JSON string to convert to 
        //JSONP object for return to the browser.
        private String JSONPIfy(String input, String callback)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(callback + "(");
            sb.Append(input);
            sb.Append(");");

            return sb.ToString();
        }

        //This function returns a current session if one is available.
        //If no current session is available it throws an argument exception error.
        private ConnectionHandler GetConnection()
        {
            ConnectionHandler LoginObj;
            if (Session["SESSION_LOGIN"] == null) throw new System.ArgumentException("No Valid Connection.");
            else LoginObj = (ConnectionHandler)Session["SESSION_LOGIN"];

            return LoginObj;
        }


    }


}

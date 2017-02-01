using System;
using System.Linq;

using Oracle.DataAccess.Client;
using System.Text.RegularExpressions;


namespace WebServiceJQueryExample

    {
        public class ConnectionHandler
        {
            private String username;
            private String password;
            private String server;
            private Boolean connected = false;
            private OracleConnection conn;

            public bool IsConnected() { return connected; }
            public String GetUsername() { return username; }
            public String GetServer() { return server; }

            private bool checkValidServer(string v_server)
            {
                if (v_server == "SOMEDBTNSNAME")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            private bool checkValidPassword(string v_password)
            {
                if (v_password.Length > 30) throw new System.ArgumentException("Invalid Password Length.");

                Regex PasswordRegex = new Regex(@"[A-Za-z0-9_$#]");
                if (!PasswordRegex.IsMatch(v_password)) throw new System.ArgumentException("Invalid Password Characters");

                return true;
            }

            public bool checkContainsReservedWord(string v_string)
            {
                //select listAgg(keyword,'","') within group (order by keyword) from V$RESERVED_WORDS where reserved = 'Y'
                string[] stringArray = { "!", "&", "(", ")", "*", "+", ",", "-", ".", "/", ":", "<", "=", ">", "@", "ALL", "ALTER", "AND", "ANY", "AS", "ASC", "BETWEEN", "BY", "CHAR", "CHECK", "CLUSTER", "COMPRESS", "CONNECT", "CREATE", "DATE", "DECIMAL", "DEFAULT", "DELETE", "DESC", "DISTINCT", "DROP", "ELSE", "EXCLUSIVE", "EXISTS", "FLOAT", "FOR", "FROM", "GRANT", "GROUP", "HAVING", "IDENTIFIED", "IN", "INDEX", "INSERT", "INTEGER", "INTERSECT", "INTO", "IS", "LIKE", "LOCK", "LONG", "MINUS", "MODE", "NOCOMPRESS", "NOT", "NOWAIT", "NULL", "NUMBER", "OF", "ON", "OPTION", "OR", "ORDER", "PCTFREE", "PRIOR", "PUBLIC", "RAW", "RENAME", "RESOURCE", "REVOKE", "SELECT", "SET", "SHARE", "SIZE", "SMALLINT", "START", "SYNONYM", "TABLE", "THEN", "TO", "TRIGGER", "UNION", "UNIQUE", "UPDATE", "VALUES", "VARCHAR", "VARCHAR2", "VIEW", "WHERE", "WITH", "[", "]", "^", "|" };
                if (stringArray.Any(v_string.Contains)) return true;

                return false;

            }


            public void SetLogin(String v_username, String v_password, String v_server)
            {
                if (checkValidServer(v_server.ToUpper()) && checkValidPassword(v_password))
                {
                    username = v_username;
                    password = v_password;
                    server = v_server.ToUpper();
                }

            }

            public OracleCommand Connect()
            {

                String oradb = "Data Source=" + server + ".world;User Id=" + username + ";Password=" + password + ";";

                if (connected) { Disconnect(); };

                conn = new OracleConnection(oradb);

                try
                {
                    conn.Open();

                    //Set class connection
                    connected = true;

                    OracleCommand cmd = new OracleCommand();
                    cmd.Connection = conn;
                    return cmd;
                }
                catch (Exception e)
                {
                    throw e;
                }

            }

            public OracleCommand GetCommand()
            {

                OracleCommand cmd = new OracleCommand();
                cmd.Connection = conn;

                return cmd;
            }

            public void Disconnect()
            {
                try
                {
                    conn.Dispose();
                    connected = false;
                }
                catch (Exception Ex)
                {
                    throw Ex;
                }
            }


        }
    }
using Oracle.DataAccess.Client;
using System.Collections.Generic;
using System.Data;

namespace WebServiceJQueryExample
{

    //These functions allow us to convert our data coming from Oracle to JSON easily.
    public class OracleDataHandler
    {
        public IEnumerable<Dictionary<string, object>> Serialize(OracleDataReader reader)
        {
            var results = new List<Dictionary<string, object>>();
            var cols = new List<string>();
            for (var i = 0; i < reader.FieldCount; i++)
                cols.Add(reader.GetName(i));

            while (reader.Read())
                results.Add(SerializeRow(cols, reader));

            return results;
        }

        public IEnumerable<Dictionary<string, object>> Serialize(DataTable reader)
        {
            var results = new List<Dictionary<string, object>>();
            var cols = new List<string>();
            foreach (DataColumn col in reader.Columns)
                cols.Add(col.ColumnName);

            foreach (DataRow row in reader.Rows)
                results.Add(SerializeRow(cols, row));

            return results;
        }

        private Dictionary<string, object> SerializeRow(IEnumerable<string> cols,
                                                DataRow reader)
        {
            var result = new Dictionary<string, object>();
            foreach (var col in cols)
                result.Add(col, reader[col]);
            return result;
        }

        private Dictionary<string, object> SerializeRow(IEnumerable<string> cols,
                                                        OracleDataReader reader)
        {
            var result = new Dictionary<string, object>();
            foreach (var col in cols)
                result.Add(col, reader[col]);
            return result;
        }


    }
}
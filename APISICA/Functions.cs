using Newtonsoft.Json;
using System.Data;

namespace APISICA
{
    public class Functions
    {
        public string DataTableToJsonWithJsonNet(DataTable table)
        {
            string jsonString = string.Empty;
            jsonString = JsonConvert.SerializeObject(table);
            return jsonString;
        }
    }
}

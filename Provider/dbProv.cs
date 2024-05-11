using System.Data;
using MySql.Data.MySqlClient;

namespace AnalisisIClinicaMedicaBack.Provider
{
    public class DatabaseProvider
    {
        private readonly string _connectionString;

        public DatabaseProvider(string connectionString)
        {
            _connectionString = connectionString;
        }

        public DataTable ExecuteQuery(string query)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();
                    var dataTable = new DataTable();
                    var command = new MySqlCommand(query, connection);
                    var dataAdapter = new MySqlDataAdapter(command);
                    dataAdapter.Fill(dataTable);
                    return dataTable;
                }
                catch (Exception ex)
                {

                    throw;
                }

            }
        }
    }
}

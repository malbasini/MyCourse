using System;
using System.Data;
using Microsoft.Data.Sqlite;
namespace MyCourse.Models.Services.Infrastructure;

public class SqlliteDatabaseAccessor :IDatabaseAccessor
{
    public DataSet Query(string query)
    {
        using (SqliteConnection conn = new SqliteConnection("Data Source = Data/MyCourse.db"))
        {
            //Chiediamo al Connection Pool di fornirci una connessione attiva (metodo Open())
            conn.Open();
            using (SqliteCommand cmd = new SqliteCommand(query, conn))
            {
                using (SqliteDataReader reader = cmd.ExecuteReader())
                {
                    DataSet dataSet = new DataSet();
                    DataTable dataTable = new DataTable();
                    dataSet.Tables.Add(dataTable);
                    dataTable.Load(reader);
                    return dataSet;
                }
            }
        }
    }
}
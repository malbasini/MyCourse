using System.Data;

namespace MyCourse.Models.Services.Infrastructure;

public class SqlliteDatabaseAccessor :IDatabaseAccessor
{
    public DataSet Query(string query)
    {
        throw new System.NotImplementedException();
    }
}
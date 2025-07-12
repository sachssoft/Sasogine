using MySqlConnector;
using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace sachssoft.Sasogine.Net;

public class GameDatabase : IDisposable
{
    private IDataConnectionStringProvider _connection_string;
    private MySqlConnection _connection;

    public void Connect(IDataConnectionStringProvider connection)
    {
        _connection = new MySqlConnection(connection.ToString());
        _connection.Open();
        _connection_string = connection;
    }

    public bool TryConnect(IDataConnectionStringProvider connection) => TryConnect(connection, out _);

    public bool TryConnect(IDataConnectionStringProvider connection, out Exception? exception)
    {
        exception = null;

        try
        {
            Connect(connection);
            return true;
        }
        catch (Exception ex)
        {
            exception = ex;
            return false;
        }
    }

    public void Close()
    {
        _connection?.Close();
    }

    public void Dispose()
    {
        _connection?.Dispose();
    }

    public IDbCommand CreateCommand(string sql)
    {
        return new MySqlCommand(sql, _connection);
    }

    public object? ExecuteScalar(IDbCommand command)
    {
        return ((MySqlCommand)command).ExecuteScalar();
    }

    public async Task<object?> ExecuteScalarAsync(IDbCommand command)
    {
        return await ((MySqlCommand)command).ExecuteScalarAsync();
    }

    public DbDataReader ExecuteReader(IDbCommand command)
    {
        return ((MySqlCommand)command).ExecuteReader();
    }

    public async Task<DbDataReader> ExecuteReaderAsync(IDbCommand command)
    {
        return await ((MySqlCommand)command).ExecuteReaderAsync();
    }

    //public void UpdateTable(Type type)
    //{
    //    var table_attr = type.GetCustomAttribute<DataTableAttribute>();

    //    // Ist der Typ keine Tabelle
    //    if (table_attr == null)
    //        return;

    //    // Prüft, ob die Tabelle existiert
    //    var sql =
    //       $"SELECT EXISTS ( " +
    //       $"   SELECT * FROM information_schema.tables " +
    //       $"   WHERE table_schema = '{_connection_string.DatabaseName}' AND table_name = '{table_attr.Name}' " +
    //       $");";

    //    var columns = new List<DataColumnInfo>();
    //    foreach (var member in type.GetMembers(BindingFlags.Instance | BindingFlags.Public))
    //    {
    //        if (member is PropertyInfo property)
    //        {
    //            var attr = property.GetCustomAttribute<DataColumnAttribute>();

    //            if (attr == null)
    //                continue;

    //            columns.Add(new DataColumnInfo()
    //            {
    //                Name = attr.Name
    //            });
    //        }
    //    }

    //    using (var cmd = (MySqlCommand)CreateCommand(sql))
    //    {
    //        var count = ((int?)cmd.ExecuteScalar()).GetValueOrDefault();

    //        if (count > 0)
    //        {
    //            // Vorhanden
    //        }
    //        else
    //        {
    //            // Nicht vorhanden
    //            CreateTable(table_attr.Name, columns.ToArray());
    //        }

    //    }
    //}

    //private void CreateTable(string name, DataColumnInfo[] columns)
    //{

    //}

    //private class DataColumnInfo
    //{
    //    public string? Name;

    //    public override string? ToString() => Name;
    //}
}

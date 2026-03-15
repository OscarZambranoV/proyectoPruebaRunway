using System.Data;
using Npgsql;

namespace DAL.Infrastructure;

public sealed class DbSession : IDbSession
{
    private IDbConnection? _connection;
    private IDbTransaction? _transaction;
    private readonly string _connectionString;
    private bool _disposed;

    public DbSession(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IDbConnection Connection
    {
        get
        {
            if (_connection is null)
            {
                _connection = new NpgsqlConnection(_connectionString);
            }
            return _connection;
        }
    }

    public IDbTransaction? Transaction => _transaction;

    public void BeginTransaction()
    {
        if (_connection is null || _connection.State == ConnectionState.Closed)
        {
            _connection = new NpgsqlConnection(_connectionString);
            _connection.Open();
        }
        else if (_connection.State != ConnectionState.Open)
        {
            _connection.Open();
        }

        _transaction = _connection.BeginTransaction();
    }

    public void Commit()
    {
        _transaction?.Commit();
        _transaction?.Dispose();
        _transaction = null;
    }

    public void Rollback()
    {
        _transaction?.Rollback();
        _transaction?.Dispose();
        _transaction = null;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _transaction?.Dispose();
        _transaction = null;

        _connection?.Dispose();
        _connection = null;
    }
}

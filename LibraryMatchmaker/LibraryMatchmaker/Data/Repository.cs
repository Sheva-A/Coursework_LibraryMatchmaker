using System.Data;
using Dapper;

namespace LibraryMatchmaker.Data;

// Спільні CRUD-операції для всіх таблиць через Dapper
public abstract class Repository<T>(IDbConnection db, string tableName, string idColumn, string orderBy)
{
    protected readonly IDbConnection Db = db;

    // SQL для INSERT і UPDATE, свій для кожної таблиці.
    // Параметри (@LastName) Dapper бере з однойменних властивостей об'єкта
    protected abstract string InsertSql { get; }
    protected abstract string UpdateSql { get; }

    // Create: повертає id нового запису
    public int Add(T item) =>
        Db.QuerySingle<int>(InsertSql + "; SELECT CAST(SCOPE_IDENTITY() AS INT);", item);

    // Read
    public List<T> GetAll() =>
        Db.Query<T>($"SELECT * FROM {tableName} ORDER BY {orderBy}").ToList();

    public virtual T? GetById(int id) =>
        Db.QuerySingleOrDefault<T>($"SELECT * FROM {tableName} WHERE {idColumn} = @id", new { id });

    // Update
    public void Update(T item) => Db.Execute(UpdateSql, item);

    // Delete
    public void Delete(int id) =>
        Db.Execute($"DELETE FROM {tableName} WHERE {idColumn} = @id", new { id });
}

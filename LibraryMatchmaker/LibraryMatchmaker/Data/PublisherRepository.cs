using System.Data;

namespace LibraryMatchmaker.Data;

public class PublisherRepository(IDbConnection db)
    : Repository<Publisher>(db, "Publishers", "publisher_id", "name")
{
    protected override string InsertSql =>
        "INSERT INTO Publishers (name, city, country) VALUES (@Name, @City, @Country)";

    protected override string UpdateSql =>
        "UPDATE Publishers SET name = @Name, city = @City, country = @Country " +
        "WHERE publisher_id = @PublisherId";
}

using System.Data;

namespace LibraryMatchmaker.Data;

public class GenreRepository(IDbConnection db) : Repository<Genre>(db, "Genres", "genre_id", "name")
{
    protected override string InsertSql => "INSERT INTO Genres (name) VALUES (@Name)";

    protected override string UpdateSql => "UPDATE Genres SET name = @Name WHERE genre_id = @GenreId";
}

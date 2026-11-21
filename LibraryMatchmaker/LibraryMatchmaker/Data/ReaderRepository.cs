using System.Data;
using Dapper;

namespace LibraryMatchmaker.Data;

public class ReaderRepository(IDbConnection db)
    : Repository<Reader>(db, "Readers", "reader_id", "last_name, first_name")
{
    protected override string InsertSql =>
        "INSERT INTO Readers (last_name, first_name, phone, email, registration_date) " +
        "VALUES (@LastName, @FirstName, @Phone, @Email, @RegistrationDate)";

    protected override string UpdateSql =>
        "UPDATE Readers SET last_name = @LastName, first_name = @FirstName, phone = @Phone, " +
        "email = @Email, registration_date = @RegistrationDate WHERE reader_id = @ReaderId";

    // Читач разом зі списком улюблених жанрів
    public override Reader? GetById(int id)
    {
        var reader = base.GetById(id);
        if (reader != null)
            reader.GenreIds = Db.Query<int>(
                "SELECT genre_id FROM Reader_Genres WHERE reader_id = @id", new { id }).ToList();
        return reader;
    }

    // Перезаписує улюблені жанри читача
    public void SaveGenres(int readerId, List<int> genreIds)
    {
        Db.Execute("DELETE FROM Reader_Genres WHERE reader_id = @readerId", new { readerId });
        Db.Execute("INSERT INTO Reader_Genres (reader_id, genre_id) VALUES (@readerId, @genreId)",
            genreIds.Select(genreId => new { readerId, genreId }));
    }

    // Запит 7. Підбір книг для читача: процедура sp_RecommendBooks (матчмейкер)
    public List<Recommendation> Recommend(int readerId) => Db.Query<Recommendation>(
        "sp_RecommendBooks", new { reader_id = readerId }, commandType: CommandType.StoredProcedure).ToList();
}

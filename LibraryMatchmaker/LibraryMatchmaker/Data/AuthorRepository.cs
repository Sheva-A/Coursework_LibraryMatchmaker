using System.Data;

namespace LibraryMatchmaker.Data;

public class AuthorRepository(IDbConnection db)
    : Repository<Author>(db, "Authors", "author_id", "last_name, first_name")
{
    protected override string InsertSql =>
        "INSERT INTO Authors (last_name, first_name, birth_year, country) " +
        "VALUES (@LastName, @FirstName, @BirthYear, @Country)";

    protected override string UpdateSql =>
        "UPDATE Authors SET last_name = @LastName, first_name = @FirstName, " +
        "birth_year = @BirthYear, country = @Country WHERE author_id = @AuthorId";
}

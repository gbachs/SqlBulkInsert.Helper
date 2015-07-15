# SqlBulkInsert.Helper

```csharp
public class BulkInsertExample
{
	public void BulkInsertUsers()
	{
		var users = GetUsers().ToList();
		var sqlWriter = new SqlWriter<User>();

		using (var connection = new SqlConnection("<ConnectionString>"))
		{
			connection.Open();
			using (var transaction = connection.BeginTransaction())
			{
				sqlWriter.Write(transaction, users);
			}
		}
	}

	private static IEnumerable<User> GetUsers()
	{
		for (var i = 0; i < 100000; i++)
		{
			yield return new User
			{
				FirstName = Guid.NewGuid().ToString(),
				LastName = Guid.NewGuid().ToString(),
				Id = Guid.NewGuid(),
				UserName = Guid.NewGuid().ToString()
			};
		}
	}

	[Table("Users")]
	private class User
	{
		[Column("UserName")]
		public string UserName { get; set; }

		[Column("FirstName")]
		public string FirstName { get; set; }

		[Column("LastName")]
		public string LastName { get; set; }

		[ContainerIdColumn("Id")]
		public Guid Id { get; set; }
	}
}
```
# SqlBulkInsert.Helper



[![Build Status](http://gbach.net:8080/buildStatus/icon?job=SqlBulkInsert)](http://gbach.net:8080/job/SqlBulkInsert)

Example of inserting a list of objects 
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

		[Column("Id")]
		public Guid Id { get; set; }
	}
}
```

Example of inserting a list of objects when one of the properties of the object is auto generated from an identity column in the sql table. This is denoted by using the GeneratedColumn attribute on the property that is associated to the sql identity column. When the data is inserted the SqlWriter will then update the objects with the new values from the identity column.
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
			sqlWriter.Write(transaction, users);
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

		[GeneratedColumn("Id")]
		public Guid Id { get; set; }
	}
}
```

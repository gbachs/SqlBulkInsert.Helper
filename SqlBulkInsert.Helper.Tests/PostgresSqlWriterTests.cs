using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Npgsql;
using SqlBulkInsert.Helper.Extensions;
using SqlBulkInsert.Helper.SqlWriters;

namespace SqlBulkInsert.Helper.Tests
{
    [TestClass]
    public class PostgresSqlWriterTests
    {
        private const string ConnectionString = @"User ID=postgres;Password=password;Host=localhost;Port=5432;Database=TestDatabase;";

        [TestInitialize]
        public void TestInitialize()
        {
            const string dropTableSql = @"DROP TABLE IF EXISTS ""TestWithoutIdentity""";
            const string createTableSql = @"CREATE TABLE IF NOT EXISTS ""TestWithoutIdentity""
                                            (
                                              ""Id"" uuid,
                                              ""StringColumn"" character(500),
                                              ""BoolProperty"" boolean
                                            )
                                            WITH (
                                              OIDS=FALSE
                                            );
                                            ALTER TABLE ""TestWithoutIdentity""
                                              OWNER TO postgres;
                                            ";

            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                connection.ExecuteNonQuery(dropTableSql);
                connection.ExecuteNonQuery(createTableSql);
            }
        }

        [TestMethod]
        public void Should_insert_data_without_identity()
        {
            var items = CreateTestData(() => new TestObjectWithoutIdentity());

			var sqlWriter = new PostgresSqlWriter<TestObjectWithoutIdentity>();

			using (var connection = new NpgsqlConnection(ConnectionString))
			{
				connection.Open();
				using (var transaction = connection.BeginTransaction())
				{
					sqlWriter.Write(transaction, items.ToList());
					transaction.Commit();
				}
			}
        }

        private static IEnumerable<T> CreateTestData<T>(Func<T> create)
        {
            for (var i = 0; i < 1000000; i++)
            {
                yield return create();
            }
        }

        [Table("TestWithoutIdentity")]
        public class TestObjectWithoutIdentity
        {
            public TestObjectWithoutIdentity()
            {
                StringProperty = Guid.NewGuid().ToString();
                BoolProperty = DateTime.Now.Ticks % 2 == 0;
                Id = Guid.NewGuid();
            }

            [Column("StringColumn")]
            public string StringProperty { get; set; }

            [Column("BoolProperty")]
            public bool BoolProperty { get; set; }

            [Column("Id")]
            public Guid Id { get; set; }
        }
    }
}

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
    public class PostgreSqlWriterTests
    {
        private const string ConnectionString = @"User ID=postgres;Password=Tdci8760;Host=localhost;Port=5432;Database=TestDatabase;";

        [TestInitialize]
        public void TestInitialize()
        {
            const string dropTestWithoutIdentityColumnTableSql = @"DROP TABLE IF EXISTS ""TestWithoutIdentity""";
            const string createTestWithoutIdentityTableSql = @"CREATE TABLE IF NOT EXISTS ""TestWithoutIdentity""
                                                            (
                                                              ""Id"" uuid,
                                                              ""StringColumn"" character(500),
                                                              ""BoolProperty"" boolean
                                                            );";

            const string dropTestWithIdentityColumnTableSql = @"DROP TABLE IF EXISTS ""TestWithIdentity""";
            const string createTestWithIdentityTableSql = @"CREATE TABLE ""TestWithIdentity"" (
                                                            ""Id"" SERIAL PRIMARY KEY,
                                                            ""StringColumn"" character(500),
                                                            ""BoolProperty"" boolean);";

            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                connection.ExecuteNonQuery(dropTestWithoutIdentityColumnTableSql);
                connection.ExecuteNonQuery(createTestWithoutIdentityTableSql);
                connection.ExecuteNonQuery(dropTestWithIdentityColumnTableSql);
                connection.ExecuteNonQuery(createTestWithIdentityTableSql);
            }
        }

        [TestMethod]
        public void Should_insert_data_without_identity()
        {
            var items = CreateTestData(() => new TestObjectWithoutIdentity());

			var sqlWriter = new PostgreSqlWriter<TestObjectWithoutIdentity>();

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

        [TestMethod]
        public void Should_insert_using_object_with_identity_column()
        {
            var items = CreateTestData(() => new TestObjectWithIdentity()).ToList();

            var sqlWriter = new PostgreSqlWriter<TestObjectWithIdentity>();

            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    sqlWriter.Write(transaction, items);
                    transaction.Commit();
                }
            }

            foreach (var item in items)
            {
                Assert.AreNotEqual(0, item.Id);
            }
        }

        private static IEnumerable<T> CreateTestData<T>(Func<T> create)
        {
            for (var i = 0; i < 10000; i++)
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


        [Table("TestWithIdentity")]
        public class TestObjectWithIdentity
        {
            public TestObjectWithIdentity()
            {
                StringProperty = Guid.NewGuid().ToString();
                BoolProperty = DateTime.Now.Ticks % 2 == 0;
            }

            [Column("StringColumn")]
            public string StringProperty { get; set; }

            [Column("BoolProperty")]
            public bool BoolProperty { get; set; }

            [GeneratedColumn("Id")]
            public int Id { get; set; }
        }
    }
}
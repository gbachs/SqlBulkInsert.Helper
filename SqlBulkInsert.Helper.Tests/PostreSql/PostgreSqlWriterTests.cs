using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Npgsql;
using SqlBulkInsert.Helper.Extensions;
using SqlBulkInsert.Helper.SqlWriters;
using SqlBulkInsert.Helper.SqlWriters.PostreSql;

namespace SqlBulkInsert.Helper.Tests.PostreSql
{
    [TestClass]
    public class PostgreSqlWriterTests
    {
        private const string ConnectionString = @"User ID=postgres;Password=password;Host=localhost;Port=5432;Database=TestDatabase;";

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

        [TestMethod, TestCategory("IntegrationTest")]
        public void Should_insert_data_with_internal_transaction()
        {
            var items = CreateTestData(100, () => new TestObjectWithoutIdentity());
            var sqlWriter = new PostgreSqlWriter<TestObjectWithoutIdentity>();

            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                sqlWriter.Write(connection, items.ToList());
            }
        }

        [TestMethod, TestCategory("IntegrationTest")]
        public void Should_insert_data_with_external_transaction()
        {
            var items = CreateTestData(100, () => new TestObjectWithoutIdentity());
            var sqlWriter = new PostgreSqlWriter<TestObjectWithoutIdentity>();

            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    sqlWriter.Write(transaction, items.ToList());
                }
            }
        }

        [TestMethod, TestCategory("IntegrationTest")]
        public void Should_insert_data_without_identity()
        {
            var items = CreateTestData(100, () => new TestObjectWithoutIdentity());
            var sqlWriter = new PostgreSqlWriter<TestObjectWithoutIdentity>();

            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    sqlWriter.Write(transaction, items.ToList());
                }
            }
        }

        [TestMethod, TestCategory("IntegrationTest")]
        public void Should_insert_using_object_with_identity_column()
        {
            var items = CreateTestData(100, () => new TestObjectWithIdentity()).ToList();

            var sqlWriter = new PostgreSqlWriter<TestObjectWithIdentity>();

            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    sqlWriter.Write(transaction, items);
                }
            }

            foreach (var item in items)
            {
                Assert.AreNotEqual(0, item.Id);
            }
        }

        private static IEnumerable<T> CreateTestData<T>(int listCount, Func<T> create)
        {
            for (var i = 0; i < listCount; i++)
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
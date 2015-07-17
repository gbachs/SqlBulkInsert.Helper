using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBulkInsert.Helper.Extensions;
using SqlBulkInsert.Helper.SqlWriters;

namespace SqlBulkInsert.Helper.Tests
{
    [TestClass]
    public class MsSqlWriterTests
    {
        private const string ConnectionString = @"Server=.\;Database=TestDatabase;User Id=sa;Password=password;";

        [TestInitialize]
        public void TestInitialize()
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                const string dropTableIfExistsSql =
                    @"IF (EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TestWithIdentity'))
					BEGIN
					   DROP TABLE [dbo].[TestWithIdentity]
					END
					IF (EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TestWithoutIdentity'))
					BEGIN
						DROP TABLE [dbo].[TestWithoutIdentity]
					END";

                const string createTableSql = @"CREATE TABLE [dbo].[TestWithIdentity](
						[Id] [int] IDENTITY(1,1) NOT NULL,
						[StringColumn] [nvarchar](50) NULL,
						[BoolProperty] [bit] NULL,
					 CONSTRAINT [PK_TestWithIdentity] PRIMARY KEY CLUSTERED
					(
						[Id] ASC
					)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
					) ON [PRIMARY]

					CREATE TABLE [dbo].[TestWithoutIdentity](
						[Id] uniqueidentifier NOT NULL,
						[StringColumn] [nvarchar](50) NULL,
						[BoolProperty] [bit] NULL,
					 CONSTRAINT [PK_TestWithoutIdentity] PRIMARY KEY CLUSTERED
					(
						[Id] ASC
					)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
					) ON [PRIMARY]";

                connection.ExecuteNonQuery(dropTableIfExistsSql);
                connection.ExecuteNonQuery(createTableSql);
            }
        }

        [TestMethod, TestCategory("IntegrationTest")]
        public void Should_insert_data_with_internal_transaction()
        {
            var items = CreateTestData(100, () => new TestObjectWithoutIdentity()).ToList();
            var sqlWriter = new MsSqlWriter<TestObjectWithoutIdentity>();

            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                sqlWriter.Write(connection, items);
            }
        }

        [TestMethod, TestCategory("IntegrationTest")]
        public void Should_insert_data_with_external_transaction()
        {
            var items = CreateTestData(100, () => new TestObjectWithoutIdentity()).ToList();
            var sqlWriter = new MsSqlWriter<TestObjectWithoutIdentity>();

            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    sqlWriter.Write(transaction, items);
                }
            }
        }

        [TestMethod, TestCategory("IntegrationTest")]
        public void Should_insert_using_object_with_identity_column()
        {
            var items = CreateTestData(100, () => new TestObjectWithIdentity()).ToList();

            var sqlWriter = new MsSqlWriter<TestObjectWithIdentity>();

            using (var connection = new SqlConnection(ConnectionString))
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

        [TestMethod, TestCategory("IntegrationTest")]
        public void Should_insert_using_object_without_identity_column()
        {
            var items = CreateTestData(100, () => new TestObjectWithoutIdentity());

            var sqlWriter = new MsSqlWriter<TestObjectWithoutIdentity>();

            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    sqlWriter.Write(transaction, items.ToList());
                    transaction.Commit();
                }
            }
        }

        private static IEnumerable<T> CreateTestData<T>(int listCount, Func<T> create)
        {
            for (var i = 0; i < listCount; i++)
            {
                yield return create();
            }
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
            public long Id { get; set; }
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
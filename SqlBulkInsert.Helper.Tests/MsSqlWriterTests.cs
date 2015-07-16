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
                    END";

                const string createTableSql = @"CREATE TABLE [dbo].[TestWithIdentity](
	                    [Id] [int] IDENTITY(1,1) NOT NULL,
	                    [StringColumn] [nvarchar](50) NULL,
	                    [BoolProperty] [bit] NULL,
                     CONSTRAINT [PK_TestWithIdentity] PRIMARY KEY CLUSTERED
                    (
	                    [Id] ASC
                    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                    ) ON [PRIMARY]";

                connection.ExecuteNonQuery(dropTableIfExistsSql);
                connection.ExecuteNonQuery(createTableSql);
            }
        }

        [TestMethod]
        public void Should_Create_SqlMetadata_From_Object()
        {
            var items = CreateTestData();

            var sqlWriter = new MsSqlWriter<TestObject>();

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

        private static IEnumerable<TestObject> CreateTestData()
        {
            for (var i = 0; i < 100000; i++)
            {
                yield return new TestObject
                {
                    BoolProperty = i%2 == 0,
                    StringProperty = Guid.NewGuid().ToString()
                };
            }
        }

        [Table("TestWithIdentity")]
        public class TestObject
        {
            [Column("StringColumn")]
            public string StringProperty { get; set; }

            [Column("BoolProperty")]
            public bool BoolProperty { get; set; }

            [GeneratedColumn("Id")]
            public long Id { get; set; }
        }
    }
}
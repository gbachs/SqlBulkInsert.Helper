using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBulkInsert.Helper.SqlWriters;

namespace SqlBulkInsert.Helper.Tests
{
    [TestClass]
    public class SqlMetadataTests
    {
        [TestMethod]
        public void Should_Create_SqlMetadata_From_Object()
        {
            var sqlMetadata = new SqlMetadata<TestObject>();

            Assert.AreEqual("TestObjectTableName", sqlMetadata.TableName);
            Assert.AreEqual(4, sqlMetadata.AllProperties.Count);
            Assert.AreEqual("Id", sqlMetadata.GeneratedIdProperty.ColumnName);
        }

        [Table("TestObjectTableName")]
        public class TestObject
        {
            [Column("StringPropertyColumnName")]
            public string StringProperty { get; set; }

            [Column("DecimalProperty")]
            public decimal DecimalProperty { get; set; }

            [Column("BoolProperty")]
            public bool BoolProperty { get; set; }

            [GeneratedColumn("Id")]
            public Guid Id { get; set; }
        }
    }
}

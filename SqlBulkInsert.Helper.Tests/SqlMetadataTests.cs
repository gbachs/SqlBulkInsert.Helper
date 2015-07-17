using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBulkInsert.Helper.SqlWriters;

namespace SqlBulkInsert.Helper.Tests
{
    [TestClass]
    public class SqlMetadataTests
    {

        [TestMethod]
        public void Should_set_GeneratedIdProperty()
        {
            var sqlMetadata = new SqlMetadata<TestObject>();

            Assert.AreEqual("Id", sqlMetadata.GeneratedIdProperty.ColumnName);
            Assert.AreEqual("Id", sqlMetadata.GeneratedIdProperty.PropertyName);
        }

        [TestMethod]
        public void Should_set_AllProperites()
        {
            var sqlMetadata = new SqlMetadata<TestObject>();

            Assert.AreEqual(4, sqlMetadata.AllProperties.Count);
        }

        [TestMethod]
        public void Should_set_TableName()
        {
            var sqlMetadata = new SqlMetadata<TestObject>();
            Assert.AreEqual("TestObjectTableName", sqlMetadata.TableName);
        }

        [TestMethod]
        public void Should_not_set_GeneratedIdProperty()
        {
            var sqlMetadata = new SqlMetadata<TestObjectWithoutGeneratedColumn>();

            Assert.IsNull(sqlMetadata.GeneratedIdProperty);
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

        [Table("TestObjectTableName")]
        public class TestObjectWithoutGeneratedColumn
        {
            [Column("StringPropertyColumnName")]
            public string StringProperty { get; set; }

            [Column("DecimalProperty")]
            public decimal DecimalProperty { get; set; }

            [Column("BoolProperty")]
            public bool BoolProperty { get; set; }

            [Column("Id")]
            public Guid Id { get; set; }
        }
    }
}

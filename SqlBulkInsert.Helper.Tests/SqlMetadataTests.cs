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

        [TestMethod]
        public void Should_not_set_containerIds_property()
        {
            var sqlMetadata = new SqlMetadata<TestObjectWithoutGeneratedColumn>();

            Assert.AreEqual(0, sqlMetadata.ContainerIdProperties.Count);
        }

        [TestMethod]
        public void Should_set_containerIds_property()
        {
            var sqlMetadata = new SqlMetadata<TestObjectWithContainerId>();

            Assert.IsNotNull(sqlMetadata.ContainerIdProperties);
            Assert.AreEqual(1, sqlMetadata.ContainerIdProperties.Count);
            Assert.AreEqual("ParentId", sqlMetadata.ContainerIdProperties[0].ColumnName);
            Assert.AreEqual("ParentId", sqlMetadata.ContainerIdProperties[0].PropertyName);
        }

        [TestMethod]
        public void Should_set_TreeParentId_property()
        {
            var sqlMetadata = new SqlMetadata<TestObjectWithTreeId>();

            Assert.IsNotNull(sqlMetadata.TreeParentIdProperty);
            Assert.AreEqual("ParentId", sqlMetadata.TreeParentIdProperty.ColumnName);
            Assert.AreEqual("ParentId", sqlMetadata.TreeParentIdProperty.PropertyName);
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

        [Table("TestObjectTableName")]
        public class TestObjectWithContainerId
        {
            [Column("StringPropertyColumnName")]
            public string StringProperty { get; set; }

            [Column("DecimalProperty")]
            public decimal DecimalProperty { get; set; }

            [Column("BoolProperty")]
            public bool BoolProperty { get; set; }

            [GeneratedColumn("Id")]
            public Guid Id { get; set; }

            [ContainerIdColumn("ParentId")]
            public Guid ParentId { get; set; }
        }

        [Table("TestObjectTableName")]
        public class TestObjectWithTreeId
        {
            [Column("StringPropertyColumnName")]
            public string StringProperty { get; set; }

            [Column("DecimalProperty")]
            public decimal DecimalProperty { get; set; }

            [Column("BoolProperty")]
            public bool BoolProperty { get; set; }

            [GeneratedColumn("Id")]
            public Guid Id { get; set; }

            [TreeParentIdColumn("ParentId")]
            public Guid ParentId { get; set; }
        }
    }
}

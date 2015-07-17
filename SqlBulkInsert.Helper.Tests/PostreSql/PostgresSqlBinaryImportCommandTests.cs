using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBulkInsert.Helper.SqlWriters;
using SqlBulkInsert.Helper.SqlWriters.PostreSql;

namespace SqlBulkInsert.Helper.Tests.PostreSql
{
    [TestClass]
    public class PostgresSqlBinaryImportCommandTests
    {
        [TestMethod]
        public void Should_create_binary_import_command()
        {
            var factory = new BinaryImportCommandFactory();
            var metadata = new SqlMetadata<TestObjectWithoutIdentity>();

            var commandString = factory.Create(metadata);
            const string expectedCommandString = @"COPY ""TestWithoutIdentity"" (""StringColumn"",""BoolProperty"",""Id"") FROM STDIN BINARY";

            Assert.AreEqual(expectedCommandString, commandString);
        }

        [TestMethod]
        public void Should_create_binary_import_command_excluding_generated_column()
        {
            var factory = new BinaryImportCommandFactory();
            var metadata = new SqlMetadata<TestObjectWithIdentity>();

            var commandString = factory.Create(metadata);
            const string expectedCommandString = @"COPY ""TestWithIdentity"" (""StringColumn"",""BoolProperty"") FROM STDIN BINARY";

            Assert.AreEqual(expectedCommandString, commandString);
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

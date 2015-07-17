using System;

namespace SqlBulkInsert.Helper.SqlWriters
{
    public interface IColumnAttribute
    {
        string ColumnName { get; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnAttribute : Attribute, IColumnAttribute
    {
        public string ColumnName { get; private set; }


        public ColumnAttribute(string columnName)
        {
            ColumnName = columnName;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class GeneratedColumnAttribute : Attribute, IColumnAttribute
    {
        public string ColumnName { get; private set; }

        public GeneratedColumnAttribute(string columnName)
        {
            ColumnName = columnName;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class ContainerIdColumnAttribute : Attribute, IColumnAttribute
    {
        public string ColumnName { get; private set; }

        public ContainerIdColumnAttribute(string columnName)
        {
            ColumnName = columnName;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class TreeParentIdColumn : Attribute, IColumnAttribute
    {
        public string ColumnName { get; private set; }


        public TreeParentIdColumn(string columnName)
        {
            ColumnName = columnName;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class TableAttribute : Attribute
    {
        public string TableName { get; private set; }
        public TableAttribute(string tableName) { TableName = tableName; }
    }
}

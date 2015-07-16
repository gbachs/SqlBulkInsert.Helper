using System;

namespace SqlBulkInsert.Helper.SqlWriters
{
    public interface IColumnAttribute
    {
        string ColumnName { get; }
        bool IsSingleValue { get; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnAttribute : Attribute, IColumnAttribute
    {
        public string ColumnName { get; private set; }

        public bool IsSingleValue { get; private set; }

        public ColumnAttribute(string columnName, bool isSingleValue = false)
        {
            ColumnName = columnName;
            IsSingleValue = isSingleValue;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class GeneratedColumnAttribute : Attribute, IColumnAttribute
    {
        public bool IsSingleValue { get; private set; }
        public string ColumnName { get; private set; }

        public GeneratedColumnAttribute(string columnName)
        {
            ColumnName = columnName;
            IsSingleValue = false;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class ContainerIdColumnAttribute : Attribute, IColumnAttribute
    {
        public bool IsSingleValue { get; private set; }
        public string ColumnName { get; private set; }

        public ContainerIdColumnAttribute(string columnName, bool isSingleValue = false)
        {
            ColumnName = columnName;
            IsSingleValue = isSingleValue;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class TreeParentIdColumn : Attribute, IColumnAttribute
    {
        public string ColumnName { get; private set; }

        public bool IsSingleValue { get; private set; }

        public TreeParentIdColumn(string columnName)
        {
            ColumnName = columnName;
            IsSingleValue = false;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class TableAttribute : Attribute
    {
        public string TableName { get; private set; }
        public TableAttribute(string tableName) { TableName = tableName; }
    }
}

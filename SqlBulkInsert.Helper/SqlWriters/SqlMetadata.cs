using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SqlBulkInsert.Helper.SqlWriters
{
    public class SqlMetadata<T>
    {
        public SqlMetadata()
        {
            TableName = GetTableName();
            GeneratedIdProperty = GetColumn<GeneratedColumnAttribute>();
            ContainerIdProperties = GetColumns<ContainerIdColumnAttribute>();
            TreeParentIdProperty = GetColumn<TreeParentIdColumn>();
            Properties = GetColumns<ColumnAttribute>().ToList();

            AllProperties = new List<ColumnProperty>();

            if (GeneratedIdProperty != null)
                AllProperties.Add(GeneratedIdProperty);

            if (ContainerIdProperties != null && ContainerIdProperties.Any())
                AllProperties.AddRange(ContainerIdProperties.Where(x => x != null));

            if (TreeParentIdProperty != null)
                AllProperties.Add(TreeParentIdProperty);

            AllProperties.AddRange(Properties);

            if (GeneratedIdProperty != null && ContainerIdProperties == null)
                throw new InvalidOperationException(
                    "A dto type that has a generatedId field must have a containerId field to be bulk inserted.");
            if (TreeParentIdProperty != null && GeneratedIdProperty == null)
                throw new InvalidOperationException(
                    "A dto type that has a treeParentId field must have a generatedId field to be bulk inserted.");
        }

        public string TableName { get; set; }
        public ColumnProperty GeneratedIdProperty { get; private set; }
        public ColumnProperty TreeParentIdProperty { get; private set; }
        public List<ColumnProperty> ContainerIdProperties { get; private set; }
        public List<ColumnProperty> Properties { get; private set; }
        public List<ColumnProperty> AllProperties { get; private set; }

        private static string GetTableName()
        {
            var tableAttributes = typeof (T).GetCustomAttributes(typeof (TableAttribute), true);
            if (!tableAttributes.Any())
                throw new ApplicationException("Type does not contain Table attribute");

            return ((TableAttribute) tableAttributes[0]).TableName;
        }

        private static List<ColumnProperty> GetColumns<TColumnType>()
            where TColumnType : Attribute, IColumnAttribute
        {
            return Enumerable.Where(typeof (T)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty)
                .Select(CreateColumnProperty<TColumnType>), x => x != null)
                .ToList();
        }

        private static ColumnProperty CreateColumnProperty<TColumnType>(PropertyInfo property)
            where TColumnType : Attribute, IColumnAttribute
        {
            var columnAttr =
                (IColumnAttribute) property.GetCustomAttributes(typeof (TColumnType), true).FirstOrDefault();
            return columnAttr == null ? null : new ColumnProperty(property, columnAttr);
        }

        private static ColumnProperty GetColumn<TColumnType>()
            where TColumnType : Attribute, IColumnAttribute
        {
            return GetColumns<TColumnType>().FirstOrDefault();
        }

        public class ColumnProperty
        {
            public ColumnProperty(PropertyInfo property, IColumnAttribute columnAttribute)
            {
                Property = property;
                PropertyName = property.Name;
                ColumnName = columnAttribute.ColumnName;
                IsSingleValue = columnAttribute.IsSingleValue;
            }

            private PropertyInfo Property { get; set; }
            public string PropertyName { get; private set; }
            public string ColumnName { get; private set; }
            public bool IsSingleValue { get; private set; }

            public TType GetValue<TType>(T obj)
            {
                return (TType) Property.GetValue(obj, null);
            }

            public object GetValue(T obj)
            {
                return Property.GetValue(obj, null);
            }

            public void SetValue(T obj, object value)
            {
                if (Convert.IsDBNull(value))
                    value = null;
                if (Property.PropertyType.IsEnum)
                {
                    var stringValue = value as string;
                    if (stringValue != null)
                        value = Enum.Parse(Property.PropertyType, stringValue);
                }
                Property.SetValue(obj, value, null);
            }
        }
    }
}
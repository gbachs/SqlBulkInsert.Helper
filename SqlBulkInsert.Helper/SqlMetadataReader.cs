using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SqlBulkInsert.Helper
{
    public class SqlMetadataReader<T> : IDataReader
    {
        private readonly Dictionary<string, int> _ordinalLookup;
        private readonly List<SqlMetadata<T>.ColumnProperty> _properties;
        private IEnumerator<T> _enumerator;

        public SqlMetadataReader(IEnumerable<SqlMetadata<T>.ColumnProperty> columns, IEnumerable<T> items)
        {
            _properties = columns.ToList();
            var idx = 0;
            _ordinalLookup = _properties.ToDictionary(x => x.PropertyName, x => idx++);
            _enumerator = items.GetEnumerator();
        }

        public void Close()
        {
            Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public bool IsClosed
        {
            get { return _enumerator == null; }
        }

        public bool Read()
        {
            if (_enumerator == null)
                throw new ObjectDisposedException("ObjectDataReader");
            return _enumerator.MoveNext();
        }

        public int FieldCount
        {
            get { return _properties.Count; }
        }

        public int GetOrdinal(string name)
        {
            int ordinal;
            if (!_ordinalLookup.TryGetValue(name, out ordinal))
                throw new InvalidOperationException("Unknown parameter name " + name);
            return ordinal;
        }

        public object GetValue(int i)
        {
            if (_enumerator == null)
                throw new ObjectDisposedException("ObjectDataReader");
            return _properties[i].GetValue(_enumerator.Current);
        }

        public int Depth
        {
            get { return 1; }
        }

        public DataTable GetSchemaTable()
        {
            return null;
        }

        public int RecordsAffected
        {
            get { return -1; }
        }

        public bool NextResult()
        {
            return false;
        }

        public bool GetBoolean(int i)
        {
            throw new NotImplementedException();
        }

        public byte GetByte(int i)
        {
            throw new NotImplementedException();
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public char GetChar(int i)
        {
            throw new NotImplementedException();
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public IDataReader GetData(int i)
        {
            throw new NotImplementedException();
        }

        public string GetDataTypeName(int i)
        {
            throw new NotImplementedException();
        }

        public DateTime GetDateTime(int i)
        {
            throw new NotImplementedException();
        }

        public decimal GetDecimal(int i)
        {
            throw new NotImplementedException();
        }

        public double GetDouble(int i)
        {
            throw new NotImplementedException();
        }

        public Type GetFieldType(int i)
        {
            throw new NotImplementedException();
        }

        public float GetFloat(int i)
        {
            throw new NotImplementedException();
        }

        public Guid GetGuid(int i)
        {
            throw new NotImplementedException();
        }

        public short GetInt16(int i)
        {
            throw new NotImplementedException();
        }

        public int GetInt32(int i)
        {
            throw new NotImplementedException();
        }

        public long GetInt64(int i)
        {
            throw new NotImplementedException();
        }

        public string GetName(int i)
        {
            throw new NotImplementedException();
        }

        public string GetString(int i)
        {
            throw new NotImplementedException();
        }

        public int GetValues(object[] values)
        {
            throw new NotImplementedException();
        }

        public bool IsDBNull(int i)
        {
            throw new NotImplementedException();
        }

        public object this[string name]
        {
            get { throw new NotImplementedException(); }
        }

        public object this[int i]
        {
            get { throw new NotImplementedException(); }
        }

        protected void Dispose(bool disposing)
        {
            if (!disposing) return;

            if (_enumerator == null)
                return;

            _enumerator.Dispose();
            _enumerator = null;
        }
    }
}
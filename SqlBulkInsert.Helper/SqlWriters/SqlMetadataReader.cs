using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SqlBulkInsert.Helper.SqlWriters
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
            return (bool)GetValue(i);
        }

        public byte GetByte(int i)
        {
            return (byte)GetValue(i);
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public char GetChar(int i)
        {
            return (char)GetValue(i);

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
            return (DateTime)GetValue(i);
        }

        public decimal GetDecimal(int i)
        {
            return (decimal)GetValue(i);
        }

        public double GetDouble(int i)
        {
            return (double)GetValue(i);
        }

        public Type GetFieldType(int i)
        {
            return GetValue(i).GetType();
        }

        public float GetFloat(int i)
        {
            return (float)GetValue(i);
        }

        public Guid GetGuid(int i)
        {
            return (Guid)GetValue(i);
        }

        public short GetInt16(int i)
        {
            return (short)GetValue(i);
        }

        public int GetInt32(int i)
        {
            return (int)GetValue(i);
        }

        public long GetInt64(int i)
        {
            return (long) GetValue(i);
        }

        public string GetName(int i)
        {
            throw new NotImplementedException();
        }

        public string GetString(int i)
        {
            return (string)GetValue(i);
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
            get { return GetValue(i); }
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
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace DataRepositories.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="System.Data.DataTable"/>
    /// Source: https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/implement-copytodatatable-where-type-not-a-datarow
    /// </summary>
    public static class IEnumerableGenericExtensions
    {
        public static DataTable CopyToDataTable<T>(this IEnumerable<T> source)
        {
            return new ObjectShredder<T>().Shred(source, null/* TODO Change to default(_) if this is not a reference type */, null/* TODO Change to default(_) if this is not a reference type */);
        }

        public static DataTable CopyToDataTable<T>(this IEnumerable<T> source, DataTable table, LoadOption? options)
        {
            return new ObjectShredder<T>().Shred(source, table, options);
        }

        private class ObjectShredder<T>
        {
            private FieldInfo[] Fields { get; set; }
            private Dictionary<string, int> OrdinalMap { get; set; }
            private PropertyInfo[] Properties { get; set; }
            private Type Type { get; set; }

            public ObjectShredder()
            {
                this.Type = typeof(T);
                this.Fields = this.Type.GetFields();
                this.Properties = this.Type.GetProperties();
                this.OrdinalMap = new Dictionary<string, int>();
            }

            public object[] ShredObject(DataTable table, T instance)
            {
                FieldInfo[] fields = this.Fields;
                PropertyInfo[] properties = this.Properties;

                if (!(instance is T))
                {
                    // If the instance is derived from T, extend the table schema
                    // and get the properties and fields.
                    this.ExtendTable(table, instance.GetType());
                    fields = instance.GetType().GetFields();
                    properties = instance.GetType().GetProperties();
                }

                // Add the property and field values of the instance to an array.
                var values = new object[table.Columns.Count - 1 + 1];
                foreach (FieldInfo field in fields)
                    values[this.OrdinalMap[field.Name]] = field.GetValue(instance);
                foreach (PropertyInfo property in properties)
                    values[this.OrdinalMap[property.Name]] = property.GetValue(instance, null);

                // Return the property and field values of the instance.
                return values;
            }

            /// <summary>Loads a DataTable from a sequence of objects.</summary>
            ///         ''' <param name="source">The sequence of objects to load into the DataTable.</param>
            ///         ''' <param name="table">The input table. The schema of the table must match that the type T. If the table is null, a new table is created with a schema created from the public properties and fields of the type T.</param>
            ///         ''' <param name="options">Specifies how values from the source sequence will be applied to existing rows in the table.</param>
            ///         ''' <returns>A DataTable created from the source sequence.</returns>
            public DataTable Shred(IEnumerable<T> source, DataTable table, LoadOption? options)
            {

                // Load the table from the scalar sequence if T is a primitive type.
                if (typeof(T).IsPrimitive)
                    return this.ShredPrimitive(source, table, options);

                // Create a new table if the input table is null.
                if (table == null)
                    table = new DataTable(typeof(T).Name);

                // Initialize the ordinal map and extend the table schema based on type T.
                table = this.ExtendTable(table, typeof(T));

                // Enumerate the source sequence and load the object values into rows.
                table.BeginLoadData();
                using (IEnumerator<T> e = source.GetEnumerator())
                {
                    while (e.MoveNext())
                    {
                        if (options.HasValue)
                            table.LoadDataRow(this.ShredObject(table, e.Current), options.Value);
                        else
                            table.LoadDataRow(this.ShredObject(table, e.Current), true);
                    }
                }
                table.EndLoadData();

                // Return the table.
                return table;
            }

            public DataTable ShredPrimitive(IEnumerable<T> source, DataTable table, LoadOption? options)
            {
                // Create a new table if the input table is null.
                if (table == null)
                    table = new DataTable(typeof(T).Name);
                if (!table.Columns.Contains("Value"))
                    table.Columns.Add("Value", typeof(T));

                // Enumerate the source sequence and load the scalar values into rows.
                table.BeginLoadData();

                using (IEnumerator<T> e = source.GetEnumerator())
                {
                    var values = new object[table.Columns.Count - 1 + 1];
                    while (e.MoveNext())
                    {
                        values[table.Columns["Value"].Ordinal] = e.Current;
                        if (options.HasValue)
                            table.LoadDataRow(values, options.Value);
                        else
                            table.LoadDataRow(values, true);
                    }
                }

                table.EndLoadData();

                // Return the table.
                return table;
            }

            public DataTable ExtendTable(DataTable table, Type type)
            {
                // Extend the table schema if the input table was null or if the value 
                // in the sequence is derived from type T.
                foreach (FieldInfo f in type.GetFields())
                {
                    if (!this.OrdinalMap.ContainsKey(f.Name))
                    {
                        DataColumn dc;

                        // Add the field as a column in the table if it doesn't exist already.
                        dc = table.Columns.Contains(f.Name) ? table.Columns[f.Name] : table.Columns.Add(f.Name, f.FieldType);

                        // Add the field to the ordinal map.
                        this.OrdinalMap.Add(f.Name, dc.Ordinal);
                    }
                }

                foreach (PropertyInfo p in type.GetProperties())
                {
                    if (!this.OrdinalMap.ContainsKey(p.Name))
                    {
                        // Add the property as a column in the table if it doesn't exist already.
                        DataColumn dc;
                        dc = table.Columns.Contains(p.Name) ? table.Columns[p.Name] : table.Columns.Add(p.Name, p.PropertyType);

                        // Add the property to the ordinal map.
                        this.OrdinalMap.Add(p.Name, dc.Ordinal);
                    }
                }

                return table;
            }
        }
    }
}
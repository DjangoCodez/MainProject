
using SoftOne.Soe.Common.Attributes;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Common.DTO
{
    [TSInclude]
    public class GenericType
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsVisible { get; set; }
        public bool IsSelected { get; set; }
        public bool IsVisibleOrSelected
        {
            get
            {
                return IsVisible || IsSelected;
            }
        }
        public bool IsAll { get; set; }
        public bool IsNone { get; set; }
        public bool IsAllOrNone
        {
            get
            {
                return IsAll || IsNone;
            }
        }
    }
    
    public class GenericType<T, J>
    {
        public T Field1 { get; set; }
        public J Field2 { get; set; }
    }

    public class GenericType<T, J, K> : GenericType<T, J>
    {
        public K Field3 { get; set; }
    }

    public class GenericType<T, J, K, L> : GenericType<T, J, K>
    {
        public L Field4 { get; set; }
    }

    [TSInclude]
    public class SmallGenericType
    {
        // Do not add more properties to this type!
        // It it supposed to be as light weight as possible.
        public int Id { get; set; }
        public string Name { get; set; }

        public SmallGenericType() { }

        public SmallGenericType(int id, string name)
        {
            this.Id = id;
            this.Name = name;
        }
    }

    [TSInclude]
    public class IntKeyValue
    {
        public int Key { get; set; }
        public int Value { get; set; }

        public IntKeyValue(int key, int value)
        {
            this.Key = key;
            this.Value = value;
        }
    }

    [TSInclude]
    public class DecimalKeyValue
    {
        public int Key { get; set; }
        public decimal Value { get; set; }

        public DecimalKeyValue(int key, decimal value)
        {
            this.Key = key;
            this.Value = value;
        }
    }

    public class StringKeyValue
    {
        public string Key { get; set; }
        public string Value { get; set; }

        public StringKeyValue(string key, string value)
        {
            this.Key = key;
            this.Value = value;
        }
    }

    public class StringKeyValueList
    {
        public int Id { get; set; }
        public List<StringKeyValue> Values { get; set; }
    }

    public class IntDateType
    {
        public int Number { get; set; }
        public DateTime Date { get; set; }

        public IntDateType(int number, DateTime date)
        {
            Number = number;
            Date = date;
        }
    }
}

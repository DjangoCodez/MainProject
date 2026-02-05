using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Common.DTO
{
    [TSInclude]
    public class BatchUpdateDTO
    {
        public int Field { get; set; }
        public string Label { get; set; }
        public BatchUpdateFieldType DataType { get; set; }
        public bool DoShowFilter { get; set; }
        public bool DoShowFromDate { get; set; }
        public bool DoShowToDate { get; set; }
        public string StringValue { get; set; }
        public bool BoolValue { get; set; }
        public int IntValue { get; set; }
        public decimal DecimalValue { get; set; }
        public DateTime? DateValue { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public List<NameAndIdDTO> Options { get; set; }
        public List<BatchUpdateDTO> Children { get; set; }

        public BatchUpdateDTO() 
        { 
        }
        public BatchUpdateDTO(int field, BatchUpdateFieldType dataType, string label, bool doShowFilter = false, bool doShowFromDate = false, bool doShowToDate = false, List<NameAndIdDTO> options = null)
        {
            this.Field = field;
            this.DataType = dataType;
            this.Label = label;
            this.Options = options;
            this.DoShowFilter = doShowFilter;
            this.DoShowFromDate = doShowFromDate;
            this.DoShowToDate = doShowToDate;
        }
        public void AddChild(BatchFieldDefinition definition)
        {
            if (definition == null)
                return;

            if (this.Children == null)
                this.Children = new List<BatchUpdateDTO>();
            this.Children.Add(new BatchUpdateDTO(definition.Field, definition.DataType, $"{this.Label} - {definition.Label}", definition.DoShowFilter));
        }
        public BatchUpdateDTO GetFirstChild(BatchUpdateFieldType dataType)
        {
            return this.Children?.FirstOrDefault(b => b.DataType == dataType);
        }
    }
    public class BatchFieldDefinition
    {
        public int Field { get; set; }
        public BatchUpdateFieldType DataType { get; set; }
        public string Label { get; set; }
        public bool DoShowFilter { get; set; }
        public bool DoShowFromDate { get; set; }
        public bool DoShowToDate { get; set; }

        public BatchFieldDefinition() 
        { 
        }
        public BatchFieldDefinition(int field, BatchUpdateFieldType dataType, string label, bool doShowFilter = false, bool doShowFromDate = false, bool doShowToDate = false)
        {
            this.Field = field;
            this.DataType = dataType;
            this.Label = label;
            this.DoShowFilter = doShowFilter;
            this.DoShowFromDate = doShowFromDate;
            this.DoShowToDate = doShowToDate;
        }
    }

    [TSInclude]
    public class NameAndIdDTO
    {
        public string Name { get; set; }
        public int Id { get; set; }
    }
    public static class BatchUpdateExtensions
    {
        public static BatchUpdateDTO CreateField(this List<BatchFieldDefinition> definitions, bool doShowFromDate = false, bool doShowToDate = false)
        {
            BatchFieldDefinition definition = definitions?.FirstOrDefault();
            if (definition == null)
                return null;

            BatchUpdateDTO batchUpdate = new BatchUpdateDTO(definition.Field, definition.DataType, definition.Label, definition.DoShowFilter, doShowFromDate, doShowToDate);
            if (definitions.Count > 1)
            {
                foreach (BatchFieldDefinition childDefinition in definitions.Skip(1))
                {
                    batchUpdate.AddChild(childDefinition);
                }
            }
            return batchUpdate;
        }
    }
}

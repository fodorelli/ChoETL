﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ChoETL
{
    [DataContract]
    public class ChoFixedLengthRecordFieldConfiguration : ChoFileRecordFieldConfiguration
    {
        [DataMember]
        public int StartIndex
        {
            get;
            set;
        }
        [DataMember]
        public string FieldName
        {
            get;
            set;
        }

        public ChoFixedLengthRecordFieldConfiguration(string name, int startIndex, int size) : this(name, null)
        {
            StartIndex = startIndex;
            Size = size;
        }

        internal ChoFixedLengthRecordFieldConfiguration(string name, ChoFixedLengthRecordFieldAttribute attr = null) : base(name, attr)
        {
            FieldName = name;
            if (attr != null)
            {
                StartIndex = attr.StartIndex;
                Size = attr.Size;
                FieldName = attr.FieldName.IsNullOrWhiteSpace() ? Name.NTrim() : attr.FieldName.NTrim();
            }
        }

        internal void Validate(ChoFixedLengthRecordConfiguration config)
        {
            try
            {
                if (FieldName.IsNullOrWhiteSpace())
                    FieldName = Name;
                if (StartIndex < 0)
                    throw new ChoRecordConfigurationException("StartIndex must be > 0.");
                if (Size == null || Size.Value < 0)
                    throw new ChoRecordConfigurationException("Size must be > 0.");
                if (FillChar != null)
                {
                    if (FillChar.Value == ChoCharEx.NUL)
                        throw new ChoRecordConfigurationException("Invalid '{0}' FillChar specified.".FormatString(FillChar));
                    if (config.EOLDelimiter.Contains(FillChar.Value))
                        throw new ChoRecordConfigurationException("FillChar [{0}] can't be one EOLDelimiter characters [{1}]".FormatString(FillChar, config.EOLDelimiter));
                }
                if (config.Comments != null)
                {
                    if ((from comm in config.Comments
                         where comm.Contains(FillChar.ToNString(' '))
                         select comm).Any())
                        throw new ChoRecordConfigurationException("One of the Comments contains FillChar. Not allowed.");
                    if ((from comm in config.Comments
                         where comm.Contains(config.EOLDelimiter)
                         select comm).Any())
                        throw new ChoRecordConfigurationException("One of the Comments contains EOLDelimiter. Not allowed.");
                }
                if (Size != null && Size.Value <= 0)
                    throw new ChoRecordConfigurationException("Size must be > 0.");
                if (ErrorMode == null)
                    ErrorMode = ChoErrorMode.ReportAndContinue; // config.ErrorMode;
                if (IgnoreFieldValueMode == null)
                    IgnoreFieldValueMode = config.IgnoreFieldValueMode;
                if (QuoteField == null)
                    QuoteField = config.QuoteAllFields;
            }
            catch (Exception ex)
            {
                throw new ChoRecordConfigurationException("Invalid configuration found at '{0}' field.".FormatString(Name), ex);
            }
        }

        internal bool IgnoreFieldValue(object fieldValue)
        {
            if ((IgnoreFieldValueMode & ChoIgnoreFieldValueMode.Null) == ChoIgnoreFieldValueMode.Null && fieldValue == null)
                return true;
            else if ((IgnoreFieldValueMode & ChoIgnoreFieldValueMode.DBNull) == ChoIgnoreFieldValueMode.DBNull && fieldValue == DBNull.Value)
                return true;
            else if ((IgnoreFieldValueMode & ChoIgnoreFieldValueMode.Empty) == ChoIgnoreFieldValueMode.Empty && fieldValue is string && ((string)fieldValue).IsEmpty())
                return true;
            else if ((IgnoreFieldValueMode & ChoIgnoreFieldValueMode.WhiteSpace) == ChoIgnoreFieldValueMode.WhiteSpace && fieldValue is string && ((string)fieldValue).IsNullOrWhiteSpace())
                return true;

            return false;
        }

    }
}

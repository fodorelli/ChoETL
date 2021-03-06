﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;

namespace ChoETL
{
    [DataContract]
    public class ChoXmlRecordFieldConfiguration : ChoFileRecordFieldConfiguration
    {
        [DataMember]
        public string XPath
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
        [DataMember]
        public bool IsXmlAttribute
        {
            get;
            set;
        }

        internal bool IsCollection
        {
            get;
            set;
        }
        internal bool UseCache
        {
            get;
            set;
        }

        public ChoXmlRecordFieldConfiguration(string name, string xPath = null) : this(name, (ChoXmlNodeRecordFieldAttribute)null)
        {
            XPath = xPath;
            UseCache = true;
        }

        internal ChoXmlRecordFieldConfiguration(string name, ChoXmlNodeRecordFieldAttribute attr = null) : base(name, attr)
        {
            FieldName = name;
            UseCache = true;
            if (attr != null)
            {
                XPath = attr.XPath;
                FieldName = attr.FieldName.IsNullOrWhiteSpace() ? Name.NTrim() : attr.FieldName.NTrim();
                IsXmlAttribute = attr is ChoXmlAttributeRecordFieldAttribute;
            }
        }

        bool init = false;
        XPathExpression query = null;
        internal XPathExpression GetXPathExpr(XPathNavigator navigator)
        {
            if (init)
                return query;

            init = true;
            if (!XPath.IsNullOrWhiteSpace())
            {
                query = navigator.Compile(XPath);
            }
            return query;
        }

        internal void Validate(ChoXmlRecordConfiguration config)
        {
            try
            {
                if (FieldName.IsNullOrWhiteSpace())
                    FieldName = Name;

                if (XPath.IsNullOrWhiteSpace())
                    throw new ChoRecordConfigurationException("Missing XPath.");
                if (FillChar != null)
                {
                    if (FillChar.Value == ChoCharEx.NUL)
                        throw new ChoRecordConfigurationException("Invalid '{0}' FillChar specified.".FormatString(FillChar));
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

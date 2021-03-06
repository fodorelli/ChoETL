﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ChoETL
{
    [DataContract]
    public abstract class ChoRecordConfiguration
    {
        private Type _recordType;
        public Type RecordType
        {
            get { return _recordType; }
            set
            {
                _recordType = value;

                IsDynamicObject = RecordType == null ? true : RecordType == typeof(ExpandoObject);
                if (!IsDynamicObject)
                {
                    PIDict = ChoType.GetProperties(RecordType).ToDictionary(p => p.Name);
                    PDDict = new Dictionary<string, PropertyDescriptor>();
                    foreach (var fn in PIDict.Keys)
                        PDDict.Add(fn, ChoTypeDescriptor.GetProperty(RecordType, fn));
                }
            }
        }

        [DataMember]
        public ChoErrorMode ErrorMode
        {
            get;
            set;
        }
        [DataMember]
        public ChoIgnoreFieldValueMode IgnoreFieldValueMode
        {
            get;
            set;
        }
        [DataMember]
        public bool AutoDiscoverColumns
        {
            get;
            set;
        }
        [DataMember]
        public bool ThrowAndStopOnMissingField
        {
            get;
            set;
        }
        [DataMember]
        public ChoObjectValidationMode ObjectValidationMode
        {
            get;
            set;
        }
        [DataMember]
        public long NotifyAfter { get; set; }

        public Dictionary<string, PropertyInfo> PIDict = null;
        internal Dictionary<string, PropertyDescriptor> PDDict = null;
        public bool IsDynamicObject = true;
        internal bool HasConfigValidators = false;
        internal Dictionary<string, ValidationAttribute[]> ValDict = null;
        internal string[] PropertyNames;

        internal ChoRecordConfiguration(Type recordType = null)
        {
            RecordType = recordType;
            ErrorMode = ChoErrorMode.ThrowAndStop;
            AutoDiscoverColumns = true;
            ThrowAndStopOnMissingField = true;
            ObjectValidationMode = ChoObjectValidationMode.Off;
            IsDynamicObject = RecordType == null ? true : RecordType == typeof(ExpandoObject);
        }

        protected virtual void Init(Type recordType)
        {
            ChoRecordObjectAttribute recObjAttr = ChoType.GetAttribute<ChoRecordObjectAttribute>(recordType);
            if (recObjAttr != null)
            {
                ErrorMode = recObjAttr.ErrorMode;
                IgnoreFieldValueMode = recObjAttr.IgnoreFieldValueMode;
                ThrowAndStopOnMissingField = recObjAttr.ThrowAndStopOnMissingField;
                ObjectValidationMode = recObjAttr.ObjectValidationMode;
            }
        }

        public abstract void MapRecordFields<T>();
        public abstract void MapRecordFields(Type recordType);

        protected void LoadNCacheMembers(IEnumerable<ChoRecordFieldConfiguration> fcs)
        {
            if (!IsDynamicObject)
            {
                object defaultValue = null;
                object fallbackValue = null;
                foreach (var fc in fcs)
                {
                    if (!PDDict.ContainsKey(fc.Name))
                        continue;

                    fc.PD = PDDict[fc.Name];
                    fc.PI = PIDict[fc.Name];

                    //Load default value
                    defaultValue = ChoType.GetRawDefaultValue(PDDict[fc.Name]);
                    if (defaultValue != null)
                    {
                        fc.DefaultValue = defaultValue;
                        fc.IsDefaultValueSpecified = true;
                    }
                    //Load fallback value
                    fallbackValue = ChoType.GetRawFallbackValue(PDDict[fc.Name]);
                    if (fallbackValue != null)
                    {
                        fc.FallbackValue = fallbackValue;
                        fc.IsFallbackValueSpecified = true;
                    }

                    //Load Converters
                    fc.PropConverters = ChoTypeDescriptor.GetTypeConverters(fc.PI);
                    fc.PropConverterParams = ChoTypeDescriptor.GetTypeConverterParams(fc.PI);

                }

                PropertyNames = PDDict.Keys.ToArray();
            }

            //Validators
            HasConfigValidators = (from fc in fcs
                                        where fc.Validators != null
                                        select fc).FirstOrDefault() != null;

            if (!HasConfigValidators)
            {
                if (!IsDynamicObject)
                {
                    foreach (var fc in fcs)
                    {
                        if (!PDDict.ContainsKey(fc.Name))
                            continue;
                        fc.Validators = ChoTypeDescriptor.GetPropetyAttributes<ValidationAttribute>(fc.PD).ToArray();
                    }
                }
            }

            ValDict = (from fc in fcs select new KeyValuePair<string, ValidationAttribute[]>(fc.Name, fc.Validators)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoETL
{
    public interface IChoReader
    {
        event EventHandler<ChoBeginLoadEventArgs> BeginLoad;
        event EventHandler<ChoEndLoadEventArgs> EndLoad;

        event EventHandler<ChoBeforeRecordLoadEventArgs> BeforeRecordLoad;
        event EventHandler<ChoAfterRecordLoadEventArgs> AfterRecordLoad;
        event EventHandler<ChoRecordLoadErrorEventArgs> RecordLoadError;

        event EventHandler<ChoBeforeRecordFieldLoadEventArgs> BeforeRecordFieldLoad;
        event EventHandler<ChoAfterRecordFieldLoadEventArgs> AfterRecordFieldLoad;
        event EventHandler<ChoRecordFieldLoadErrorEventArgs> RecordFieldLoadError;
    }

    public abstract class ChoReader : IChoReader
    {
        public event EventHandler<ChoBeginLoadEventArgs> BeginLoad;
        public event EventHandler<ChoEndLoadEventArgs> EndLoad;

        public event EventHandler<ChoAfterRecordLoadEventArgs> AfterRecordLoad;
        public event EventHandler<ChoBeforeRecordLoadEventArgs> BeforeRecordLoad;
        public event EventHandler<ChoRecordLoadErrorEventArgs> RecordLoadError;

        public event EventHandler<ChoBeforeRecordFieldLoadEventArgs> BeforeRecordFieldLoad;
        public event EventHandler<ChoAfterRecordFieldLoadEventArgs> AfterRecordFieldLoad;
        public event EventHandler<ChoRecordFieldLoadErrorEventArgs> RecordFieldLoadError;

        public bool RaiseBeginLoad(object source)
        {
            EventHandler<ChoBeginLoadEventArgs> eh = BeginLoad;
            if (eh == null)
                return true;

            ChoBeginLoadEventArgs e = new ChoBeginLoadEventArgs() { Source = source };
            eh(this, e);
            return !e.Stop;
        }

        public void RaiseEndLoad(object source)
        {
            EventHandler<ChoEndLoadEventArgs> eh = EndLoad;
            if (eh == null)
                return;

            ChoEndLoadEventArgs e = new ChoEndLoadEventArgs() { Source = source };
            eh(this, e);
        }

        public bool RaiseBeforeRecordLoad(object record, long index, ref object source)
        {
            EventHandler<ChoBeforeRecordLoadEventArgs> eh = BeforeRecordLoad;
            if (eh == null)
                return true;

            ChoBeforeRecordLoadEventArgs e = new ChoBeforeRecordLoadEventArgs() { Record = record, Index = index, Source = source };
            eh(this, e);
            source = e.Source;
            return !e.Skip;
        }

        public bool RaiseAfterRecordLoad(object record, long index, object source)
        {
            EventHandler<ChoAfterRecordLoadEventArgs> eh = AfterRecordLoad;
            if (eh == null)
                return true;

            ChoAfterRecordLoadEventArgs e = new ChoAfterRecordLoadEventArgs() { Record = record, Index = index, Source = source };
            eh(this, e);
            return !e.Stop;
        }

        public bool RaiseRecordLoadError(object record, long index, object source, Exception ex)
        {
            EventHandler<ChoRecordLoadErrorEventArgs> eh = RecordLoadError;
            if (eh == null)
                return true;

            ChoRecordLoadErrorEventArgs e = new ChoRecordLoadErrorEventArgs() { Record = record, Index = index, Source = source, Exception = ex };
            eh(this, e);
            source = e.Source;
            return e.Handled;
        }

        public bool RaiseBeforeRecordFieldLoad(object record, long index, string propName, ref object source)
        {
            EventHandler<ChoBeforeRecordFieldLoadEventArgs> eh = BeforeRecordFieldLoad;
            if (eh == null)
                return true;

            ChoBeforeRecordFieldLoadEventArgs e = new ChoBeforeRecordFieldLoadEventArgs() { Record = record, Index = index, PropertyName = propName, Source = source };
            eh(this, e);
            source = e.Source;
            return !e.Skip;
        }

        public bool RaiseAfterRecordFieldLoad(object record, long index, string propName, object source)
        {
            EventHandler<ChoAfterRecordFieldLoadEventArgs> eh = AfterRecordFieldLoad;
            if (eh == null)
                return true;

            ChoAfterRecordFieldLoadEventArgs e = new ChoAfterRecordFieldLoadEventArgs() { Record = record, Index = index, PropertyName = propName, Source = source };
            eh(this, e);
            return !e.Stop;
        }

        public bool RaiseRecordFieldLoadError(object record, long index, string propName, object source, Exception ex)
        {
            EventHandler<ChoRecordFieldLoadErrorEventArgs> eh = RecordFieldLoadError;
            if (eh == null)
                return true;

            ChoRecordFieldLoadErrorEventArgs e = new ChoRecordFieldLoadErrorEventArgs() { Record = record, Index = index, PropertyName = propName, Source = source, Exception = ex };
            eh(this, e);
            source = e.Source;
            return e.Handled;
        }
    }

    public class ChoBeginLoadEventArgs : EventArgs
    {
        public object Source
        {
            get;
            internal set;
        }

        public bool Stop
        {
            get;
            set;
        }
    }

    public class ChoEndLoadEventArgs : EventArgs
    {
        public object Source
        {
            get;
            internal set;
        }
    }

    public class ChoBeforeRecordLoadEventArgs : EventArgs
    {
        public object Record
        {
            get;
            internal set;
        }
        public long Index
        {
            get;
            internal set;
        }
        public object Source
        {
            get;
            internal set;
        }
        public bool Skip
        {
            get;
            set;
        }
    }

    public class ChoBeforeRecordFieldLoadEventArgs : ChoBeforeRecordLoadEventArgs
    {
        public string PropertyName
        {
            get;
            internal set;
        }
    }

    public class ChoAfterRecordLoadEventArgs : EventArgs
    {
        public object Record
        {
            get;
            internal set;
        }
        public long Index
        {
            get;
            internal set;
        }
        public object Source
        {
            get;
            internal set;
        }
        public bool Stop
        {
            get;
            set;
        }
    }

    public class ChoAfterRecordFieldLoadEventArgs : ChoAfterRecordLoadEventArgs
    {
        public string PropertyName
        {
            get;
            internal set;
        }
    }

    public class ChoRecordLoadErrorEventArgs : EventArgs
    {
        public object Record
        {
            get;
            internal set;
        }
        public long Index
        {
            get;
            internal set;
        }
        public object Source
        {
            get;
            internal set;
        }
        public Exception Exception
        {
            get;
            internal set;
        }
        public bool Handled
        {
            get;
            set;
        }
    }

    public class ChoRecordFieldLoadErrorEventArgs : ChoRecordLoadErrorEventArgs
    {
        public string PropertyName
        {
            get;
            internal set;
        }
    }
}

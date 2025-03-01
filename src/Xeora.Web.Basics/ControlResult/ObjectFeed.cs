﻿using System;

namespace Xeora.Web.Basics.ControlResult
{
    [Serializable]
    public class ObjectFeed : IDataSource
    {
        private readonly object[] _Objects;
        private long _Total;

        public ObjectFeed() : this(null)
        { }

        public ObjectFeed(object[] objects) : this(objects, Guid.Empty)
        { }
        
        public ObjectFeed(object[] objects, Guid resultId)
        {
            this.Type = DataSourceTypes.ObjectFeed;
            this.Message = null;

            this.ResultId = resultId;
            this._Objects = objects ?? new object[] {};
        }

        public DataSourceTypes Type { get; }
        public Message Message { get; set; }
        public long Count => this._Objects.Length;

        public long Total
        {
            get
            {
                if (this._Total == 0)
                    this._Total = this._Objects.Length;

                return this._Total;
            }
            set => this._Total = value;
        }

        public Guid ResultId { get; set; }
        public object GetResult() =>
            this._Objects;
    }
}

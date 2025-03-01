﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Xeora.Web.Basics;

namespace Xeora.Web.Application
{
    public class MetaRecord : IMetaRecordCollection
    {
        private readonly ConcurrentDictionary<Basics.MetaRecord.Tags, string> _Records;
        private readonly ConcurrentDictionary<string, string> _CustomRecords;

        public MetaRecord()
        {
            this._Records = new ConcurrentDictionary<Basics.MetaRecord.Tags, string>();
            this._CustomRecords = new ConcurrentDictionary<string, string>();
        }

        public void Add(Basics.MetaRecord.TagSpaces tagSpace, string name, string value)
        {
            if (string.IsNullOrEmpty(name))
                throw new NullReferenceException("Name can not be null!");

            if (string.IsNullOrEmpty(value))
                value = string.Empty;

            name = tagSpace switch
            {
                Basics.MetaRecord.TagSpaces.name => $"name::{name}",
                Basics.MetaRecord.TagSpaces.httpequiv => $"httpequiv::{name}",
                Basics.MetaRecord.TagSpaces.property => $"property::{name}",
                _ => name
            };

            this._CustomRecords.AddOrUpdate(name, value, (cName, cValue) => value);
        }

        public void Add(Basics.MetaRecord.Tags tag, string value)
        {
            if (string.IsNullOrEmpty(value))
                value = string.Empty;

            this._Records.AddOrUpdate(tag, value, (cName, cValue) => value);
        }

        public void Remove(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new NullReferenceException("Name can not be null!");

            this._CustomRecords.TryRemove(name, out _);
        }

        public void Remove(Basics.MetaRecord.Tags tag) =>
            this._Records.TryRemove(tag, out _);

        public IEnumerable<KeyValuePair<Basics.MetaRecord.Tags, string>> CommonTags
        {
            get
            {
                KeyValuePair<Basics.MetaRecord.Tags, string>[] metaTags =
                    new KeyValuePair<Basics.MetaRecord.Tags, string>[this._Records.Keys.Count];

                int keyCount = 0;
                foreach (Basics.MetaRecord.Tags key in this._Records.Keys)
                {
                    this._Records.TryGetValue(key, out string value);

                    metaTags[keyCount] = new KeyValuePair<Basics.MetaRecord.Tags, string>(key, value);
                    keyCount++;
                }

                return metaTags;
            }
        }

        public IEnumerable<KeyValuePair<string, string>> CustomTags
        {
            get
            {
                KeyValuePair<string, string>[] metaTags =
                    new KeyValuePair<string, string>[this._CustomRecords.Keys.Count];

                int keyCount = 0;
                foreach (string key in this._CustomRecords.Keys)
                {
                    this._CustomRecords.TryGetValue(key, out string value);

                    metaTags[keyCount] = new KeyValuePair<string, string>(key, value);
                    keyCount++;
                }

                return metaTags;
            }
        }
    }
}

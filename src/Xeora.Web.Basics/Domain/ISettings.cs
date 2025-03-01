﻿using System;

namespace Xeora.Web.Basics.Domain
{
    public interface ISettings : IDisposable
    {
        IConfigurations Configurations { get; }
        IServices Services { get; }
        IUrl Mappings { get; }
    }
}

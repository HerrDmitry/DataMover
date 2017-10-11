﻿using System.Collections.Generic;

namespace Interfaces.Configuration
{
    public interface IConfiguration
    {
        IList<IFile> Sources { get;}
        IList<IFile> Targets { get;}
    }
}
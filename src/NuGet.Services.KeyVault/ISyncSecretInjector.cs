// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGet.Services.KeyVault
{
    /// <summary>
    /// A sync variant of <see cref="ISecretInjector"/>. 
    /// The implementation would likely use a <see cref="ISyncSecretReader"/>.
    /// </summary>
    public interface ISyncSecretInjector
    {
        string Inject(string input);
    }
}

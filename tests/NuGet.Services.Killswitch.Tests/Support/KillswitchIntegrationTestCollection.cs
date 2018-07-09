// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;

namespace NuGet.Services.Killswitch.Tests
{
    [CollectionDefinition(Name)]
    public class KillswitchIntegrationTestCollection : ICollectionFixture<KillswitchIntegrationTestFixture>
    {
        public const string Name = "Killswitch integration test collection";
    }
}

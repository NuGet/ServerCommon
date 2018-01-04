// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace NuGet.Services.Validation.Issues
{
    public class PackageIsSigned : ValidationIssue
    {
        public override ValidationIssueCode IssueCode => ValidationIssueCode.PackageIsSigned;

        public override string GetMessage() => "Package publishing failed: This package could not be published since it is signed. " +
                                               "We do not accept signed packages at this moment. To be notified about package signing and more, " +
                                               "watch our <a href=\"https://github.com/nuget/announcements/issues\">Announcements</a> page or follow us " +
                                               "on <a href=\"https://twitter.com/nuget\">Twitter</a>.";
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace NuGet.Services.Storage
{
    /// <summary>
    /// This class represents the typical results of trying to acquire a lease from <see cref="ILeaseService"/>.
    /// </summary>
    public class BlobLeaseResult
    {
        private BlobLeaseResult(bool isSuccess, string leaseId)
        {
            IsSuccess = isSuccess;
            LeaseId = leaseId;
        }

        /// <summary>
        /// True if the lease was acquired. False if another thread has the lease.
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// The lease ID used to identify an acquired lease. This property will be non-null only if
        /// <see cref="IsSuccess"/> is true.
        /// </summary>
        public string LeaseId { get; }

        public static BlobLeaseResult Success(string leaseId)
        {
            if (leaseId == null)
            {
                throw new ArgumentNullException(nameof(leaseId));
            }

            return new BlobLeaseResult(isSuccess: true, leaseId: leaseId);
        }

        public static BlobLeaseResult Failure()
        {
            return new BlobLeaseResult(isSuccess: false, leaseId: null);
        }
    }
}
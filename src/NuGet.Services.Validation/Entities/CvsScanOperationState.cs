// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGet.Services.Validation.Entities
{
    /// <summary>
    /// Status of the cvs scan operations
    /// </summary>
    public class CvsScanOperationState
    {
        /// <summary>
        /// The database-mastered identifier for this operation.
        /// </summary>
        public long Key { get; set; }

        /// <summary>
        /// Validation Step ID for which this operation is performed.
        /// </summary>
        public Guid ValidationStepId { get; set; }

        /// <summary>
        /// Status of the operation performed.
        /// </summary>
        public CvsScanStatus Status { get; set; }

        /// <summary>
        /// CVS's ID for the content scan job.
        /// </summary>
        public long JobId { get; set; }

        /// <summary>
        /// Time when the validator detected operation request.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Time when the operation updated.
        /// </summary>
        public DateTime? LastUpdatedAt { get; set; }

        /// <summary>
        /// The violation detalis found in cvs scan.
        /// </summary>
        public string Violation { get; set; }

        /// <summary>
        /// The path pointing to external content
        /// </summary>
        public string ContentPath { get; set; }

        /// <summary>
        /// The blob url pointing to an operation output blob.
        /// </summary>
        public string BlobUrl { get; set; }

        /// <summary>
        /// File ID
        /// </summary>
        public string FileId { get; set; }

        /// <summary>
        /// Used for optimistic concurrency when updating statuses.
        /// </summary>
        public byte[] RowVersion { get; set; }
    }
}

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
        /// Validation ID for which this operation is performed.
        /// </summary>
        public Guid ValidationSetId { get; set; }

        /// <summary>
        /// Status of the operation performed.
        /// </summary>
        public int Status { get; set; }

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
        /// The size of the blob sent for scan/sign operation
        /// </summary>
        public string? Violation { get; set; }

        /// <summary>
        /// The path pointing to external content
        /// </summary>
        public string ContentPath { get; set; }

        /// <summary>
        /// The url pointing to an operation output blob.
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

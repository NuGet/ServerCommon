// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace NuGet.Services.Status.Table
{
    public class ManualStatusChangeEntity : TableEntity
    {
        public const string DefaultPartitionKey = "manual";

        public ManualStatusChangeEntity()
        {
        }

        private ManualStatusChangeEntity(
            DateTime changeTimestamp,
            ManualStatusChangeType type,
            string eventAffectedComponentPath = null,
            ComponentStatus eventAffectedComponentStatus = default(ComponentStatus),
            DateTime? eventStartTime = null,
            DateTime? eventEndTime = null,
            DateTime? messageTimestamp = null,
            string messageContents = null,
            bool messageIsDelete = false)
            : base(DefaultPartitionKey, GetRowKey(changeTimestamp))
        {
            ChangeTimestamp = changeTimestamp;
            Type = (int)type;
            EventAffectedComponentPath = eventAffectedComponentPath;
            EventAffectedComponentStatus = (int)eventAffectedComponentStatus;
            EventStartTime = eventStartTime;
            EventEndTime = eventEndTime;
            MessageTimestamp = messageTimestamp;
            MessageContents = messageContents;
            MessageIsDelete = messageIsDelete;
        }

        private ManualStatusChangeEntity(
            ManualStatusChangeType type,
            string eventAffectedComponentPath = null,
            ComponentStatus eventAffectedComponentStatus = default(ComponentStatus),
            DateTime? eventStartTime = null,
            DateTime? eventEndTime = null,
            DateTime? messageTimestamp = null,
            string messageContents = null,
            bool messageIsDelete = false)
            : this(
                  DateTime.UtcNow, 
                  type, 
                  eventAffectedComponentPath, 
                  eventAffectedComponentStatus, 
                  eventStartTime, 
                  eventEndTime, 
                  messageTimestamp, 
                  messageContents, 
                  messageIsDelete)
        {
        }

        public static ManualStatusChangeEntity AddStatusEvent(
            string eventAffectedComponentPath, 
            ComponentStatus eventAffectedComponentStatus, 
            DateTime eventStartTime,
            string messageContents, 
            bool isEventActive)
        {
            if (isEventActive)
            {
                return new ManualStatusChangeEntity(
                    ManualStatusChangeType.AddStatusEvent,
                    eventAffectedComponentPath: eventAffectedComponentPath,
                    eventAffectedComponentStatus: eventAffectedComponentStatus,
                    eventStartTime: eventStartTime,
                    messageContents: messageContents);
            }
            else
            {
                return new ManualStatusChangeEntity(
                    ManualStatusChangeType.AddStatusEvent,
                    eventAffectedComponentPath: eventAffectedComponentPath,
                    eventAffectedComponentStatus: eventAffectedComponentStatus,
                    eventStartTime: eventStartTime,
                    eventEndTime: DateTime.UtcNow,
                    messageContents: messageContents);
            }
        }

        public static ManualStatusChangeEntity EditStatusEvent(
            string eventAffectedComponentPath,
            ComponentStatus eventAffectedComponentStatus,
            DateTime eventStartTime,
            bool isEventActive)
        {
            if (isEventActive)
            {
                return new ManualStatusChangeEntity(
                    ManualStatusChangeType.EditStatusEvent,
                    eventAffectedComponentPath: eventAffectedComponentPath,
                    eventAffectedComponentStatus: eventAffectedComponentStatus,
                    eventStartTime: eventStartTime);
            }
            else
            {
                return new ManualStatusChangeEntity(
                    ManualStatusChangeType.EditStatusEvent,
                    eventAffectedComponentPath: eventAffectedComponentPath,
                    eventAffectedComponentStatus: eventAffectedComponentStatus,
                    eventStartTime: eventStartTime,
                    eventEndTime: DateTime.UtcNow);
            }
        }

        public static ManualStatusChangeEntity DeleteStatusEvent(
            string eventAffectedComponentPath,
            DateTime eventStartTime)
        {
            return new ManualStatusChangeEntity(
                ManualStatusChangeType.DeleteStatusEvent,
                eventAffectedComponentPath: eventAffectedComponentPath,
                eventStartTime: eventStartTime);
        }

        public static ManualStatusChangeEntity AddStatusMessage(
            string eventAffectedComponentPath,
            DateTime eventStartTime,
            string messageContents,
            bool isEventActive)
        {
            if (isEventActive)
            {
                return new ManualStatusChangeEntity(
                    ManualStatusChangeType.AddStatusMessage,
                    eventAffectedComponentPath: eventAffectedComponentPath,
                    eventStartTime: eventStartTime,
                    messageContents: messageContents);
            }
            else
            {
                return new ManualStatusChangeEntity(
                    ManualStatusChangeType.AddStatusMessage,
                    eventAffectedComponentPath: eventAffectedComponentPath,
                    eventStartTime: eventStartTime,
                    eventEndTime: DateTime.UtcNow,
                    messageContents: messageContents);
            }
        }

        public static ManualStatusChangeEntity EditStatusMessage(
            string eventAffectedComponentPath,
            DateTime eventStartTime,
            DateTime messageTimestamp,
            string messageContents)
        {
            return new ManualStatusChangeEntity(
                ManualStatusChangeType.EditStatusMessage,
                eventAffectedComponentPath: eventAffectedComponentPath,
                eventStartTime: eventStartTime,
                messageTimestamp: messageTimestamp,
                messageContents: messageContents);
        }

        public static ManualStatusChangeEntity DeleteStatusMessage(
            string eventAffectedComponentPath,
            DateTime eventStartTime,
            DateTime messageTimestamp)
        {
            return new ManualStatusChangeEntity(
                ManualStatusChangeType.DeleteStatusMessage,
                eventAffectedComponentPath: eventAffectedComponentPath,
                eventStartTime: eventStartTime,
                messageTimestamp: messageTimestamp);
        }

        public DateTime ChangeTimestamp { get; set; }

        /// <remarks>
        /// This should be a <see cref="ManualStatusChangeType"/> converted to an enum.
        /// See https://github.com/Azure/azure-storage-net/issues/383
        /// </remarks>
        public int Type { get; set; }

        public string EventAffectedComponentPath { get; set; }

        /// <remarks>
        /// This should be a <see cref="ComponentStatus"/> converted to an enum.
        /// See https://github.com/Azure/azure-storage-net/issues/383
        /// </remarks>
        public int EventAffectedComponentStatus { get; set; }

        public DateTime? EventStartTime { get; set; }

        public DateTime? EventEndTime { get; set; }

        public DateTime? MessageTimestamp { get; set; }

        public string MessageContents { get; set; }

        public bool MessageIsDelete { get; set; }

        public static string GetRowKey(DateTime changeTimestamp)
        {
            return $"{changeTimestamp.ToString("o")}";
        }
    }
}

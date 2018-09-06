﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace NuGet.Services.Status
{
    /// <summary>
    /// Represents an event affecting a component.
    /// </summary>
    public class Event
    {
        /// <summary>
        /// The <see cref="IComponentDescription.Path"/> of the component affected.
        /// </summary>
        public string AffectedComponentPath { get; }

        /// <summary>
        /// When the event began.
        /// </summary>
        public DateTime StartTime { get; }

        /// <summary>
        /// When the event ended, or <c>null</c> if the event is still active.
        /// </summary>
        public DateTime? EndTime { get; }

        /// <summary>
        /// A set of <see cref="Message"/>s related to this event.
        /// </summary>
        public IEnumerable<Message> Messages { get; }

        [JsonConstructor]
        public Event(
            string affectedComponentPath,
            DateTime startTime,
            DateTime? endTime,
            IEnumerable<Message> messages)
        {
            AffectedComponentPath = affectedComponentPath;
            StartTime = startTime;
            EndTime = endTime;
            Messages = messages;
        }
    }
}

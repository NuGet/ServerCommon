﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGet.Services.Status
{
    public interface IComponentDescription
    {
        /// <summary>
        /// The name of this part of the service.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// A description of what this part of the service does.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// The status of this part of the service.
        /// </summary>
        ComponentStatus Status { get; }

        /// <summary>
        /// A string path used to identify this part of the service when accessed by a root component.
        /// For example, if "A" is a component with a subcomponent "B", the path of "B" is "A/B".
        /// </summary>
        string Path { get; }
    }
}

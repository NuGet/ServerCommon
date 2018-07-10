// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;

namespace NuGet.Services.Status
{
    public static class ComponentExtensions
    {
        public static IReadOnlyComponent GetByPath(this IReadOnlyComponent root, string path)
        {
            var componentPathParts = path.Split(Constants.ComponentPathDivider);
            return root.GetByPath(componentPathParts);
        }

        public static IReadOnlyComponent GetByPath(this IReadOnlyComponent root, params string[] pathParts)
        {
            if (pathParts.First() != root.Name)
            {
                return null;
            }

            IReadOnlyComponent component = root;
            foreach (var componentPathPart in pathParts.Skip(1))
            {
                component = component.SubComponents.FirstOrDefault(c => c.Name == componentPathPart);

                if (component == null)
                {
                    break;
                }
            }

            return component;
        }
    }
}

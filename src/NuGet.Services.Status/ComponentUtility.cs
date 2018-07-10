// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;

namespace NuGet.Services.Status
{
    public static class ComponentUtility
    {
        public static string GetPath(params string[] componentNames)
        {
            return string.Join(Constants.ComponentPathDivider.ToString(), componentNames);
        }

        public static IReadOnlyComponent GetByPath(this IReadOnlyComponent root, string path)
        {
            var componentNames = path.Split(Constants.ComponentPathDivider);
            return root.GetByPath(componentNames);
        }

        public static IReadOnlyComponent GetByPath(this IReadOnlyComponent root, params string[] componentNames)
        {
            if (componentNames.First() != root.Name)
            {
                return null;
            }

            IReadOnlyComponent component = root;
            foreach (var componentName in componentNames.Skip(1))
            {
                component = component.SubComponents.FirstOrDefault(c => c.Name == componentName);

                if (component == null)
                {
                    break;
                }
            }

            return component;
        }
    }
}

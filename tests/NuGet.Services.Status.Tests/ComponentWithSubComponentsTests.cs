﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace NuGet.Services.Status.Tests
{
    public abstract class ComponentWithSubComponentsTests : ComponentTests
    {
        protected abstract IComponent CreateComponent(string name, string description, IEnumerable<IComponent> subComponents);
        protected abstract ComponentStatus GetExpectedStatusWithTwoSubComponents(ComponentStatus subStatus1, ComponentStatus subStatus2);

        [Fact]
        public void ThrowsIfNameNullWithSubComponents()
        {
            Assert.Throws<ArgumentNullException>(() => CreateComponent(null, "description", Enumerable.Empty<IComponent>()));
        }

        [Fact]
        public void PropagatesPathCorrectly()
        {
            var innermostSubComponent = CreateComponent("innermostSubComponent", "innermostSubDescription");
            var middleSubComponent = CreateComponent("middleSubComponent", "middleSubDescription", new[] { innermostSubComponent });
            var rootComponent = CreateComponent("rootComponent", "rootDescription", new[] { middleSubComponent });

            Assert.Equal(rootComponent.Name, rootComponent.Path);
            Assert.Equal(string.Join(Component.ComponentPathDivider, rootComponent.Name, middleSubComponent.Name), rootComponent.SubComponents.First().Path);
            Assert.Equal(string.Join(Component.ComponentPathDivider, rootComponent.Name, middleSubComponent.Name, innermostSubComponent.Name), rootComponent.SubComponents.First().SubComponents.First().Path);
        }

        [Fact]
        public void ThrowsIfSubComponentsNull()
        {
            Assert.Throws<ArgumentNullException>(() => CreateComponent("name", "description", null));
        }

        [Theory]
        [ClassData(typeof(ComponentTestData.ComponentStatusPairs))]
        public void ReturnsStatusIfSetWithSubComponent(ComponentStatus status, ComponentStatus subStatus)
        {
            var subComponent = CreateComponent("subComponent", "subdescription");
            subComponent.Status = subStatus;

            var component = CreateComponent("component", "description", new[] { subComponent });
            component.Status = status;

            Assert.Equal(status, component.Status);
        }

        [Theory]
        [ClassData(typeof(ComponentTestData.ComponentStatusTriplets))]
        public void ReturnsStatusIfSetWithMultipleSubComponents(
            ComponentStatus status, 
            ComponentStatus subStatus1, 
            ComponentStatus subStatus2)
        {
            var subComponent1 = CreateComponent("subComponent1", "subdescription1");
            subComponent1.Status = subStatus1;

            var subComponent2 = CreateComponent("subComponent2", "subdescription2");
            subComponent2.Status = subStatus2;

            var component = CreateComponent("component", "description", new[] { subComponent1, subComponent2 });
            component.Status = status;

            Assert.Equal(status, component.Status);
        }

        [Theory]
        [ClassData(typeof(ComponentTestData.ComponentStatuses))]
        public void ReturnsSubStatusIfStatusNotSet(ComponentStatus subStatus)
        {
            var subComponent = CreateComponent("subComponent", "subdescription");
            subComponent.Status = subStatus;

            var component = CreateComponent("component", "description", new[] { subComponent });

            Assert.Equal(subStatus, component.Status);
        }

        [Theory]
        [ClassData(typeof(ComponentTestData.ComponentStatusPairs))]
        public void ReturnsSubStatusIfStatusNotSetWithTwoSubComponents(
            ComponentStatus subStatus1, 
            ComponentStatus subStatus2)
        {
            var subComponent1 = CreateComponent("subComponent1", "subdescription1");
            subComponent1.Status = subStatus1;

            var subComponent2 = CreateComponent("subComponent2", "subdescription2");
            subComponent2.Status = subStatus2;

            var component = CreateComponent("component", "description", new[] { subComponent1, subComponent2 });

            Assert.Equal(GetExpectedStatusWithTwoSubComponents(subStatus1, subStatus2), component.Status);
        }
    }
}
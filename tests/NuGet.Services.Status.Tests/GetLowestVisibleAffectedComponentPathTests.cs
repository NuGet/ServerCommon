using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace NuGet.Services.Status.Tests
{
    public class GetLowestVisibleAffectedComponentPathTests
    {
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ReturnsSubComponentIfPathHasNoParts(bool displaySubComponents)
        {
            var component = new TestComponent("", displaySubComponents);
            AssertLeastCommonVisibleAncestor(component, component, component);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ReturnsRootComponent(bool displaySubComponents)
        {
            var component = new TestComponent("path", displaySubComponents);
            AssertLeastCommonVisibleAncestor(component, component, component);
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void ReturnsNullIfNotASubComponent(bool displaySubComponentsRoot, bool displaySubComponentsSubComponent)
        {
            var root = new TestComponent("root", displaySubComponentsRoot);
            var subComponent = new TestComponent("notInTree", displaySubComponentsSubComponent);
            AssertLeastCommonVisibleAncestor(root, subComponent, null);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ReturnsSubComponent(bool displaySubComponents)
        {
            var subComponent = new TestComponent("child", displaySubComponents);
            var root = new TestComponent("root", new[] { subComponent }, displaySubComponents: true);
            AssertLeastCommonVisibleAncestor(
                root,
                root.SubComponents.Single(c => c.Name == subComponent.Name),
                root.SubComponents.Single(c => c.Name == subComponent.Name));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ReturnsIntermediateSubComponent(bool displaySubComponents)
        {
            var subComponent = new TestComponent("child", displaySubComponents);
            var intermediateComponent = new TestComponent("middle", new[] { subComponent }, displaySubComponents: false);
            var root = new TestComponent("root", new[] { intermediateComponent }, displaySubComponents: true);
            AssertLeastCommonVisibleAncestor(
                root,
                root.SubComponents.Single(c => c.Name == intermediateComponent.Name).SubComponents.Single(c => c.Name == subComponent.Name),
                root.SubComponents.Single(c => c.Name == intermediateComponent.Name));
        }

        private void AssertLeastCommonVisibleAncestor(IComponent root, IComponent subComponent, IComponent expected)
        {
            Assert.Equal(expected, root.GetLeastCommonVisibleAncestorOfSubComponent(subComponent));
        }

        private class TestComponent : Component
        {
            public TestComponent(
                string name,
                bool displaySubComponents)
                : this(name, new IComponent[0], displaySubComponents)
            {
            }

            public TestComponent(
                string name,
                IEnumerable<IComponent> subComponents,
                bool displaySubComponents)
                : base(name, "", subComponents, displaySubComponents)
            {
            }

            public override ComponentStatus Status { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        }
    }
}

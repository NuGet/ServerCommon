using System.Collections;
using System.Collections.Generic;

namespace NuGet.Services.Status.Tests
{
    public abstract class TestDataClass : IEnumerable<object[]>
    {
        public abstract IEnumerable<object[]> Data { get; }

        public IEnumerator<object[]> GetEnumerator() => Data.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

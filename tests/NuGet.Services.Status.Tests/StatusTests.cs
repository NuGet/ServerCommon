using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace NuGet.Services.Status.Tests
{
    public class StatusTests
    {
        [Fact]
        public void SerializesAndDeserializesCorrectly()
        {
            var expected = CreateStatus();

            var statusString = JsonConvert.SerializeObject(expected);
            var actual = JsonConvert.DeserializeObject<Status>(statusString);

            AssertStatus(expected, actual);
        }

        private static Status CreateStatus()
        {
            return new Status(GetDate(), CreateRootComponent(), new[] { CreateEvent(), CreateEvent(), CreateEvent() });
        }

        private static IComponent CreateRootComponent()
        {
            return CreateComponent(new TreeComponent(GetString(), GetString(), new[] { CreateTreeComponent(), CreatePrimarySecondaryComponent(), CreateSubComponent(), CreateTreeComponent(), CreatePrimarySecondaryComponent(), CreateSubComponent() }));
        }

        private static IComponent CreateTreeComponent()
        {
            return CreateComponent(new TreeComponent(GetString(), GetString(), new[] { CreateSubComponent(), CreateSubComponent() }));
        }

        private static IComponent CreatePrimarySecondaryComponent()
        {
            return CreateComponent(new PrimarySecondaryComponent(GetString(), GetString(), new[] { CreateSubComponent(), CreateSubComponent() }));
        }

        private static IComponent CreateSubComponent()
        {
            return CreateComponent(new LeafComponent(GetString(), GetString()));
        }

        private static IEvent CreateEvent()
        {
            return new Event(GetString(), GetComponentStatus(), GetDate(), GetDate(), new[] { CreateMessage(), CreateMessage() });
        }

        private static IMessage CreateMessage()
        {
            return new Message(GetDate(), GetString());
        }

        private static IComponent CreateComponent(IComponent component)
        {
            var status = GetComponentStatus();
            if (status != ComponentStatus.Up)
            {
                component.Status = status;
            }

            return component;
        }

        private static void AssertStatus(Status expected, Status actual)
        {
            AssertFieldEqual(expected, actual, i => i.LastUpdated);
            AssertFieldEqual(expected, actual, i => i.RootComponent, AssertComponent);
            AssertFieldEqual(expected, actual, i => i.Events, AssertEvent);
        }

        private static void AssertComponent(IReadOnlyComponent expected, IReadOnlyComponent actual)
        {
            AssertFieldEqual(expected, actual, i => i.Description);
            AssertFieldEqual(expected, actual, i => i.Name);
            AssertFieldEqual(expected, actual, i => i.Status);
            AssertFieldEqual(expected, actual, i => i.SubComponents, AssertComponent);
            AssertFieldEqual(expected, actual, i => i.Path);
        }

        private static void AssertEvent(IEvent expected, IEvent actual)
        {
            AssertFieldEqual(expected, actual, i => i.AffectedComponentPath);
            AssertFieldEqual(expected, actual, i => i.AffectedComponentStatus);
            AssertFieldEqual(expected, actual, i => i.EndTime);
            AssertFieldEqual(expected, actual, i => i.StartTime);
            AssertFieldEqual(expected, actual, i => i.Messages, AssertMessage);
        }

        private static void AssertMessage(IMessage expected, IMessage actual)
        {
            AssertFieldEqual(expected, actual, i => i.Contents);
            AssertFieldEqual(expected, actual, i => i.Time);
        }

        private static void AssertFieldEqual<TParent, TField>(
            TParent expected,
            TParent actual,
            Func<TParent, TField> accessor)
        {
            Assert.Equal(accessor(expected), accessor(actual));
        }

        private static void AssertFieldEqual<TParent, TField>(
            TParent expected,
            TParent actual,
            Func<TParent, TField> accessor,
            Action<TField, TField> assert)
        {
            assert(accessor(expected), accessor(actual));
        }

        private static void AssertFieldEqual<TParent, TField>(
            TParent expected,
            TParent actual,
            Func<TParent, IEnumerable<TField>> accessor,
            Action<TField, TField> assert)
        {
            AssertAll(accessor(expected), accessor(actual), assert);
        }

        private static void AssertAll<T>(IEnumerable<T> expecteds, IEnumerable<T> actuals, Action<T, T> assert)
        {
            if (expecteds == null)
            {
                Assert.Null(actuals);

                return;
            }

            Assert.Equal(expecteds.Count(), actuals.Count());
            var expectedsArray = expecteds.ToArray();
            var actualsArray = actuals.ToArray();
            for (int i = 0; i < expecteds.Count(); i++)
            {
                assert(expectedsArray[i], actualsArray[i]);
            }
        }

        private static int _currentStatusIndex = 0;
        private static IEnumerable<ComponentStatus> _statuses = Enum.GetValues(typeof(ComponentStatus)).Cast<ComponentStatus>();

        private static ComponentStatus GetComponentStatus()
        {
            _currentStatusIndex = (_currentStatusIndex + 1) % _statuses.Count();
            return _statuses.ElementAt(_currentStatusIndex);
        }

        private static string _currentString = "a";
        
        private static string GetString()
        {
            var last = _currentString[_currentString.Length - 1];
            if (last == 'z')
            {
                _currentString += 'a';
            }
            else
            {
                _currentString = _currentString.Substring(0, _currentString.Length - 1) + (last + 1);
            }

            return _currentString;
        }

        private static DateTime _currentDate = new DateTime(2018, 7, 3);
        private static TimeSpan _diffDate = new TimeSpan(1, 0, 0);

        private static DateTime GetDate()
        {
            _currentDate += _diffDate;
            return _currentDate;
        }
    }
}

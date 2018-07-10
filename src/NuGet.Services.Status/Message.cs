using Newtonsoft.Json;
using System;

namespace NuGet.Services.Status
{
    public class Message : IMessage
    {
        public DateTime Time { get; }
        public string Contents { get; }

        [JsonConstructor]
        public Message(DateTime time, string contents)
        {
            Time = time;
            Contents = contents;
        }
    }
}

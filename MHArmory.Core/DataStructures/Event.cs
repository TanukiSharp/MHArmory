using System;
using System.Collections.Generic;
using System.Text;

namespace MHArmory.Core.DataStructures
{
    public interface IEvent
    {
        int Id { get; }
        Dictionary<string, string> Name { get; }
    }

    public class Event : IEvent
    {
        public int Id { get; }
        public Dictionary<string, string> Name { get; }

        public Event(int id, Dictionary<string, string> name)
        {
            Id = id;
            Name = name;
        }
    }
}

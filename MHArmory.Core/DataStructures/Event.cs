using System;
using System.Collections.Generic;
using System.Text;

namespace MHArmory.Core.DataStructures
{
    public interface IEvent
    {
        int Id { get; }
        string Name { get; }
    }

    public class Event : IEvent
    {
        public int Id { get; }
        public string Name { get; }

        public Event(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}

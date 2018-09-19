using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MHArmory.Configurations;
using MHArmory.Core.DataStructures;

namespace MHArmory.ViewModels
{
    public class EventViewModel : ViewModelBase
    {
        private readonly EventsViewModel parent;

        public string Name { get; }

        public bool HasChanged { get; private set; }

        private bool isEnabled;
        public bool IsEnabled
        {
            get { return isEnabled; }
            set
            {
                if (SetValue(ref isEnabled, value))
                    parent.HasChanged = true;
            }
        }

        public EventViewModel(EventsViewModel parent, string name, bool initialValue)
        {
            this.parent = parent;
            Name = name;
            isEnabled = initialValue;
        }
    }

    public class EventsViewModel : ViewModelBase
    {
        private readonly RootViewModel parent;

        private IList<EventViewModel> events;
        public IList<EventViewModel> Events
        {
            get { return events; }
            private set { SetValue(ref events, value); }
        }

        public bool HasChanged { get; set; }

        public EventsViewModel(RootViewModel parent)
        {
            this.parent = parent;
        }

        public void NotifyDataLoaded()
        {
            IEnumerable<IEquipment> heads = GlobalData.Instance.Heads.Cast<IEquipment>();
            IEnumerable<IEquipment> chests = GlobalData.Instance.Chests.Cast<IEquipment>();
            IEnumerable<IEquipment> armss = GlobalData.Instance.Gloves.Cast<IEquipment>();
            IEnumerable<IEquipment> waists = GlobalData.Instance.Waists.Cast<IEquipment>();
            IEnumerable<IEquipment> legs = GlobalData.Instance.Legs.Cast<IEquipment>();
            IEnumerable<IEquipment> charms = GlobalData.Instance.Charms.Cast<IEquipment>();

            IList<IEvent> allEvents = heads
                .Concat(chests)
                .Concat(armss)
                .Concat(waists)
                .Concat(legs)
                .Concat(charms)
                .Select(x => x.Event)
                .Where(x => x != null)
                .Distinct((a, b) => a.Id == b.Id, x => x.Id)
                .ToList();

            IDictionary<string, bool> events = GlobalData.Instance.Configuration.Events;

            Events = allEvents
                .Select(x => CreateEventViewModel(x.Name, events))
                .ToList();
        }

        private EventViewModel CreateEventViewModel(string name, IDictionary<string, bool> events)
        {
            events.TryGetValue(name, out bool isEnabled);

            return new EventViewModel(this, name, isEnabled);
        }

        public void UpdateAndSaveConfiguration()
        {
            if (HasChanged == false)
                return;

            IDictionary<string, bool> events = GlobalData.Instance.Configuration.Events;

            events.Clear();

            foreach (EventViewModel vm in Events)
                events.Add(vm.Name, vm.IsEnabled);

            ConfigurationManager.Save(GlobalData.Instance.Configuration);

            parent.CreateSolverData();
        }
    }
}

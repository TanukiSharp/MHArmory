using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MHArmory.Core.ServiceContracts;

namespace MHArmory.Services
{
    public static class ServicesContainer
    {
        private static Dictionary<Type, object> registeredServices = new Dictionary<Type, object>();

        static ServicesContainer()
        {
            RegisterService<IMessageBoxService>(new MessageBoxService());
            RegisterService<ISaveDataService>(new SaveDataService());
            RegisterService<ISaveDataAdvancedService>(new SaveDataAdvancedService());
        }

        public static void RegisterService<T>(T instance)
        {
            registeredServices.Add(typeof(T), instance);
        }

        public static T GetService<T>()
        {
            if (registeredServices.TryGetValue(typeof(T), out object instance))
                return (T)instance;

            return default(T);
        }
    }
}

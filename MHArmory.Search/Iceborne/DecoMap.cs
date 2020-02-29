using MHArmory.Core.DataStructures;
using MHArmory.Search.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHArmory.Search.Iceborne
{
    class DecoMap
    {
        Dictionary<IJewel, int> decos = new Dictionary<IJewel, int>();

        public void AddDecos(IJewel deco, int count = 1)
        {
            if (decos.ContainsKey(deco))
                decos[deco] += count;
            else
                decos[deco] = count;
        }

        public int this[IJewel deco]
        {
            get
            {
                if (decos.TryGetValue(deco, out int value))
                    return value;
                else
                    return 0;
            }
        }

        public void Clear()
        {
            decos.Clear();
        }

        public List<ArmorSetJewelResult> ToArmorSetJewelResult()
        {
            return decos.Select(p => new ArmorSetJewelResult { Jewel = p.Key, Count = p.Value }).ToList();
        }
    }
}

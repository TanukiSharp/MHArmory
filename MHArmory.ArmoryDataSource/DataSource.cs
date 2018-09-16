using System;
using System.Threading.Tasks;
using MHArmory.Core;
using MHArmory.Core.DataStructures;

namespace MHArmory.ArmoryDataSource
{
    public class DataSource : IDataSource
    {
        public string Description { get; } = "Armory data source";

        public Task<IArmorPiece[]> GetArmorPieces()
        {
            throw new NotImplementedException();
        }

        public Task<ICharm[]> GetCharms()
        {
            throw new NotImplementedException();
        }

        public Task<IJewel[]> GetJewels()
        {
            throw new NotImplementedException();
        }

        public Task<ISkill[]> GetSkills()
        {
            throw new NotImplementedException();
        }
    }
}

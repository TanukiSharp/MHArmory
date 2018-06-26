using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MHArmory.Core.DataStructures;

namespace MHArmory.Core
{
    public interface IArmorDataSource
    {
        Task<IList<IArmorPiece>> GetArmorPieces();
    }
}

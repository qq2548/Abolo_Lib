using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AboloLib
{
    public interface IPersistable
    {
        void Save(GameDataWriter writer);

        void Load(GameDataReader reader);
    }
}

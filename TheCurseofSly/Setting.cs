using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Modding;
namespace TheCurseofSly
{
     public class Setting
    {
        public enum Mode
        {
            Easy,
            Hard,
            None
        }
        public enum PunishmentType
        {
            Die,
            Damage,
            None
        }
        public Mode mode = Mode.Easy;
        public PunishmentType punishment = PunishmentType.Die;
    }
}

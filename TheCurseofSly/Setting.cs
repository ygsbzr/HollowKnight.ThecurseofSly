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
        public enum SpawnGeo
        {
            Yes,
            No,
        }
        public Mode mode = Mode.Easy;
        public PunishmentType punishment = PunishmentType.Die;
        public SpawnGeo spawn=SpawnGeo.Yes;
    }
}

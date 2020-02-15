using Verse;

namespace WallStuff
{
    public class Building_CoolerGlower : Building
    {
        private Building_MediumCooler Cooler;

        public void Reinit( Building_MediumCooler h )
        {
            Cooler = h;
        }

        public override void Tick()
        {
            if (!this.IsHashIntervalTick( 60 ))
            {
                return;
            }

            if (Cooler == null || Cooler.glower != this)
            {
                Destroy();
            }
        }
    }
}
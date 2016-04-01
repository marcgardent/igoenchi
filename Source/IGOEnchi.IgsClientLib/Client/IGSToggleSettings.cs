namespace IGoEnchi
{
    public class IGSToggleSettings
    {
        public IGSToggleSettings(bool open, bool looking, bool kibitz)
        {
            Open = open;
            Looking = looking;
            Kibitz = kibitz;
        }

        public bool Open { get; private set; }
        public bool Looking { get; private set; }
        public bool Kibitz { get; private set; }

        public static IGSToggleSettings Default
        {
            get { return new IGSToggleSettings(true, false, true); }
        }
    }
}
namespace RedAlliance.LightBoardLib
{
    using rpi_ws281x;
    using Unosquare.RaspberryIO;
    using Unosquare.WiringPi;

    public class LightBoardManager
    {
        public LightBoardManager()
        {
            
        }
        
        public int ScreenWidth { get; private set } = 64;
        public int ScreenHeight { get; private set } = 8;
        public int LedCount => ScreenWidth * ScreenHeight;
        
        private void Initialize()
        {
            Pi.Init<BootstrapWiringPi>();
        }
    }
}

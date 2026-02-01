using Exiled.API.Features;
using IcomMediaDisplay.Logic;

namespace IcomMediaDisplay
{
    public class IcomMediaDisplay : Plugin<Config>
    {
        public static IcomMediaDisplay Instance { get; private set; }
        public static string PluginDirectory => Path.Combine(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EXILED"), "Plugins"), "IcomMediaDisplay");
        private static TransportHandler _playbackHandler;

        // Plugin information
        public override string Name => "IcomMediaDisplay_experimental";
        public override string Prefix { get; } = "IcomMediaDisplay_experimental";
        public override string Author { get; } = "mono";
        public override Version Version { get; } = new Version(2, 0, 1);
        public override Version RequiredExiledVersion { get; } = new Version(8, 11, 0);

        public override void OnEnabled()
        {
            Instance = this;
            _playbackHandler ??= new TransportHandler();
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Instance = null;
            _playbackHandler = null;
            base.OnDisabled();
        }
    }
}

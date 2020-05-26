using System.Runtime.CompilerServices;
using IPA.Config.Stores;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace JustTakeMeToTheSoloMode.Configuration
{
    internal class PluginConfig
    {
        public static PluginConfig Instance { get; set; }
        public virtual float wait { get; set; } = 1.0f;
        public virtual int selectTab { get; set; } = -1;
    }
}

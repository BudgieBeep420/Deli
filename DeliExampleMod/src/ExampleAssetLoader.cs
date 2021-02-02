using ADepIn;
using Deli;

namespace DeliExampleMod
{
    // Asset loaders are the classes that when bound, will be run to load an asset that specifies the same loader name
    // In this case, we'll use the name 'example_b64_asset' and load the asset from json as our struct
    [QuickNamedBind("example_asset")]
    public class ExampleAssetLoader : IAssetLoader
    {
        public void LoadAsset(IServiceKernel kernel, Mod mod, string path)
        {
            // We the asset as an ExampleAsset using the asset reader defined in another file
            var asset = mod.Resources.Get<ExampleJsonAsset>(path).Unwrap();

            // Switch on the severity and log the message from the logger of the mod that contains the asset
            mod.Logger.LogInfo(asset.Message);
        }
    }
}
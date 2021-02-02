using Deli;

namespace DeliExampleMod
{
    // Deli Mods are just mono behaviours that get added to a global game object when the game first starts.
    // Generally you want to put your setup code in Awake()
    public class DeliExampleMod : DeliBehaviour
    {
        private void Awake()
        {
            // Hello, world!
            Logger.LogInfo("Hello, world!");

            // Fetch and log the example resource
            var textResource = Resources.Get<string>("Resources/ExampleTextResource.txt").Unwrap();
            Logger.LogInfo("Text resource contents: " + textResource);
            
            // TODO: Put the rest of your setup code here
        }
    }
}
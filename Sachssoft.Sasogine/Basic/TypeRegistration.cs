namespace Sachssoft.Sasogine
{
    internal static class TypeRegistration
    {
        private static bool _isRegistered = false;

        public static void RegisterTypes()
        {
            if (_isRegistered) return;

            //AssetRegistration.RegisterToFactory<Texture2DAsset>("texture_2d");
            //AssetRegistration.RegisterToFactory<SoundAsset>("sound");
            //AssetRegistration.RegisterToFactory<MusicAsset>("music");

            _isRegistered = true;
        }

    }
}

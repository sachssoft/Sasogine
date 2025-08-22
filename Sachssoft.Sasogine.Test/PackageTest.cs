using Sachssoft.Documents.Json;
using Sachssoft.Sasogine.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sachssoft.Sasogine.Test
{
    public class PackageTest
    {
        private static string testPackagePath = "D:\\Projekte\\Sasogine\\sachssoft.Sasogine.Test\\TestPackage.pak";
        private static string testPackagePath1 = "D:\\Projekte\\MinerMania\\Sachssoft.MinerMania.Core\\Content\\collections\\testpackage-level.mmsc";


        [Fact]
        public void OpenPackage()
        {
            using (var fs = new FileStream(testPackagePath1, FileMode.Open))
            {
                var manifest = new TestManifest();
                using (var package = ProjectedPackage.Open(fs, manifest))
                {
                    Assert.True(true);
                }
            }
        }

        [Fact]
        public void OpenPackage2()
        {
            var fs = new FileStream(testPackagePath1, FileMode.Open);
            var manifest = new TestManifest();
            var package = ProjectedPackage.Open(fs, manifest);
            
            package.Close();
            fs.Close();

            Assert.True(true);
        }

        [Fact]
        public void LoadManifest()
        {
            using (var fs = new FileStream(testPackagePath, FileMode.Open))
            {
                var manifest = new TestManifest();
                using (var package = ProjectedPackage.Open(fs, manifest))
                {
                    manifest.Load();
                }
            }
        }

        [Fact]
        public void UpdateManifest()
        {
            using (var fs = new FileStream(testPackagePath, FileMode.Open))
            {
                var manifest = new TestManifest();
                using (var package = ProjectedPackage.Open(fs, manifest))
                {
                    manifest.Save();
                }
            }
        }

        [Theory]
        [InlineData("background1.jpg", "D:\\Projekte\\Sasogine\\sachssoft.Sasogine.Test\\Assets\\background-example1.jpg")]
        [InlineData("background2.jpg", "D:\\Projekte\\Sasogine\\sachssoft.Sasogine.Test\\Assets\\background-example2.jpg")]
        public void SetAsset(string newFilePath, string sourceFilePath)
        {
            using (var fs = new FileStream(testPackagePath, FileMode.Open))
            {
                var manifest = new TestManifest();
                using (var package = ProjectedPackage.Open(fs, manifest))
                {
                    package.Assets.AddOrReplace(sourceFilePath, newFilePath, AssetCategory.Texture);
                }
            }
        }

        [Fact]
        public void CreateAsset()
        {
            using (var fs = new FileStream(testPackagePath, FileMode.Open))
            {
                var manifest = new TestManifest();
                using (var package = ProjectedPackage.Open(fs, manifest))
                {
                    package.Assets.Add("HelloWorld!", "asset.txt", AssetCategory.Other);
                }
            }
        }
    }
}

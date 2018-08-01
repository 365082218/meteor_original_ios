using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Outfit7.Util.Io;
using UnityEngine;

namespace Outfit7.Common {

    [TestFixture]
    public static class ProjectAnalyzer {

        [Test]
        public static void FindScriptsWithoutNamespace() {

            string[] files = Directory.GetFiles(Path.Combine(Application.dataPath, "Code"), "*", SearchOption.AllDirectories);

            string problematicFiles = "";
            foreach (string file in files) {
                if (!file.EndsWith(".cs")) continue;
                string content = O7File.ReadAllText(file);
                if (content == null) continue;
                if (content.Contains("namespace ")) continue;
                problematicFiles += file + "\n";
            }
            Assert.IsEmpty(problematicFiles, "No namespace found in files:");
        }

        [Test]
        public static void FindMisplacedScriptsAndMaterials() {
            const string regexS = ".*\\\\Resources\\\\.*(\\.cs|\\.mat)$";
            Regex regex = new Regex(regexS);

            string[] files = Directory.GetFiles(Application.dataPath, "*", SearchOption.AllDirectories);

            string problematicFiles = "";
            foreach (string file in files) {
                Match m = regex.Match(file);
                if (!m.Success) continue;
                problematicFiles += file + "\n";
            }
            Assert.IsEmpty(problematicFiles, "Misplaced files found:");
        }
    }
}

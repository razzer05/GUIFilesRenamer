using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUIFilesRenamer
{
    class RenamedFilesTextFile
    {
        private StreamWriter sw;
        private StreamReader sr;
        private string path;

        public RenamedFilesTextFile() { }

        public RenamedFilesTextFile(string outputFiles)
        {
            path = "RenamedFiles "+DateTime.Now.ToString()+".txt";
            path = path.Replace("/", "-").Replace(":","-");
            if (outputFiles != null && outputFiles != "")
            {
                using (sw = File.AppendText(path))
                {
                    sw.WriteLine(outputFiles);
                }
            }
        }

        public string RevertFilesFrom(string filePath)
        {
            string files = "";
            using (sr = File.OpenText(filePath))
            {
                string oldName = "";
                while ((oldName = sr.ReadLine()) != null && oldName.Length > 1)
                {
                    string newName = sr.ReadLine();
                    oldName = oldName.Replace("OldName:", "");
                    newName = newName.Replace("NewName:", "");
                    if (File.Exists(newName))
                        File.Move(newName, oldName);

                    files += "Renamed: \n" + newName + "\n back to: \n" + oldName + "\n\n";
                }
                return files;
            }
        }

        public string RevertLastSet()
        {
            string files = "";
            using (sr = File.OpenText(path))
            {
                string oldName = "";
                while ((oldName = sr.ReadLine()) != null && oldName.Length>1)
                {
                    string newName = sr.ReadLine();
                    oldName = oldName.Replace("OldName:", "");
                    newName = newName.Replace("NewName:", "");
                    if(File.Exists(newName))
                        File.Move(newName, oldName);

                    files += "Renamed: \n" + newName + "\n back to: \n" + oldName + "\n\n";
                }
                
            }
            return files;
        }
    }
}

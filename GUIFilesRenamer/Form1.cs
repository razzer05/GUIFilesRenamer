using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GUIFilesRenamer
{
    public partial class Form1 : Form
    {
        private string[] mediaExtensions = {
                    ".WAV", ".MID", ".MIDI", ".WMA", ".MP3", ".OGG", ".RMA",
                    ".PNG", ".JPG", ".JPEG", ".BMP", ".GIF",
                    ".AVI", ".MP4", ".DIVX", ".WMV", ".mkv"
                };
        private bool shownError = false;
        const string title = "Error";
        private string path = "";

        public Form1()
        {
            InitializeComponent();
        }

        private void folderBrowserDialog1_HelpRequest(object sender, EventArgs e)
        {
            
        }

        //Browse Button click action
        private void button3_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                populateFilesToRename(folderBrowserDialog1.SelectedPath);
        }

        private void populateFilesToRename(string path)
        {
            this.path = textBox1.Text = path;
            richTextBox1.Text = richTextBox2.Text = "";

            var fileEntries = checkBox2.Checked ? Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories) : Directory.EnumerateFiles(path);

            foreach (var file in fileEntries)
            {
                FileInfo fileInfo = new FileInfo(file);
                var subFolders = GetSubFolders(fileInfo);
                richTextBox1.Text += subFolders + fileInfo.Name + "\n";
            }
            renameButton.Enabled = true;
        }

        //Rename button click action
        private void button2_Click(object sender, EventArgs e)
        {
            if (path != "" && textBox2.Text != "" && textBox3.Text != "" || path != "" && !checkBox1.Checked)
            {
                var fileEntries = checkBox2.Checked ? Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories) : Directory.EnumerateFiles(path);

                shownError = false;

                foreach (var item in fileEntries)
                {
                    FileInfo file = new FileInfo(item);
                    if (mediaExtensions.Contains(file.Extension, StringComparer.OrdinalIgnoreCase))
                    {
                        //tv shows
                        if (checkBox1.Checked)
                            TvShowRename(path, file);
                        //movies
                        else
                            MoviesRename(path, file);
                    }
                }
            }
            else
            {
                const string errorMsg = "Fill all the boxes";
                MessageBox.Show(errorMsg, title, MessageBoxButtons.OK);
            }

            //After completion clear the tv show rename options
            textBox2.Text = textBox3.Text = "";
            //Re-disable rename button
            renameButton.Enabled = false;
        }


        //display tv show options
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            label1.Visible = label2.Visible = textBox2.Visible = textBox3.Visible = checkBox1.Checked ? true : false;
            checkBox2.Checked = false;
        }

        //only works for tv shows with 'E' for the episode numbering
        private void TvShowRename(string path, FileInfo file)
        {
            string newName = "";
            bool foundEpisodeNum = false;
            var i1 = 0;
            var fileNameToSearch = file.Name.Substring(i1);

            if (textBox3.Text.Length <= 1)
                textBox3.Text = "0" + textBox3.Text;

            while (!foundEpisodeNum)
            {
                i1 = fileNameToSearch.IndexOf("E", StringComparison.InvariantCultureIgnoreCase);
                if (i1 == -1)
                {
                    string errorMsg = "Could not find episode number in file name " + file.Name;
                    MessageBox.Show(errorMsg);
                    return;
                }
                newName = fileNameToSearch.Substring(i1 + 1, 2);
                foundEpisodeNum = char.IsNumber(Convert.ToChar("".Substring(0, 1)));
                fileNameToSearch = fileNameToSearch.Substring(i1 + 1);
            }
            if (!char.IsNumber(Convert.ToChar("".Substring(1, 2))))
                newName = "0" + "".Substring(0, 1);

            textBox2.Text = textBox2.Text.Substring(0, 1).ToUpper() + textBox2.Text.Substring(1);
            var fileName = path + "\\" + textBox2.Text + " S" + textBox3.Text + "E" + "" + file.Extension.ToLower();
            RenameMethod(file, fileName, newName);
        }

        //only works for movies with the resolution in the name
        private void MoviesRename(string path, FileInfo file)
        {
            int indexOfResolution;
            bool foundResolution = false;
            string newName = "";
            string restOfName = file.Name;
            while (!foundResolution)
            {
                indexOfResolution = restOfName.IndexOf("p", StringComparison.InvariantCultureIgnoreCase);
                if (indexOfResolution == -1)
                    break; //error message?
                if (indexOfResolution - 4 > 0)
                {
                    int characterToCheck = char.IsNumber(Convert.ToChar(restOfName.Substring(indexOfResolution - 4, 1)))
                        ? indexOfResolution - 4 : indexOfResolution - 3;
                    foundResolution = char.IsNumber(Convert.ToChar(restOfName.Substring(characterToCheck,1))) ? true : false;
                    if (foundResolution)
                    {
                        newName = file.Name.Substring(0, indexOfResolution+1);
                        break;
                    }
                }
                restOfName = restOfName.Substring(indexOfResolution + 1);
                if (restOfName.Length == -1) break;
            }
            if (!string.IsNullOrEmpty(newName)) newName = newName.Replace(".", " ");
            else return; //Error Message
            var subFolders = GetSubFolders(file);
            var fileName = path + "\\" + subFolders + newName + file.Extension.ToLower();
            newName = subFolders + newName;
            RenameMethod(file, fileName, newName);
        }

        private string GetSubFolders(FileInfo file)
        {
            var subFolders = file.FullName.Replace(path + "\\", "");
            subFolders = subFolders.Replace(file.Name, "");
            return subFolders;
        }

        private void RenameMethod(FileInfo file, string fileName, string newName)
        {
            try
            {
                file.MoveTo(fileName);
                richTextBox2.Text += newName + file.Extension + "\n";
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                const string errorMsg =
                    "An error occured and didn't rename all files, make sure there isn't duplicate episodes with the same episode number.";
                if (!shownError)
                    MessageBox.Show(errorMsg, title, MessageBoxButtons.OK);
                shownError = true;
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (path != "") populateFilesToRename(path);
        }
    }
}

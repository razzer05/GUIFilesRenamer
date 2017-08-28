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
        private void BrowseButton_Click(object sender, EventArgs e)
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
                richTextBox1.Text += GetSubFolders(fileInfo) + fileInfo.Name + "\n";
            }
            renameButton.Enabled = true;
        }

        private void RenameButton_Click(object sender, EventArgs e)
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
                        if (checkBox1.Checked) //tv shows
                            TvShowRename(path, file);
                        else //movies
                            NewMoviesRename(path, file);
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
        private void DisplayTvShowOptions_CheckedChanged(object sender, EventArgs e)
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
            if (newName + file.Extension != file.Name)
                RenameMethod(file, fileName, newName);
        }

        //only works for movies with the resolution in the name (1080p 720p 480p etc)
        //Would probably be easier to look for 1080p within the string of the name..
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
                    foundResolution = char.IsNumber(Convert.ToChar(restOfName.Substring(characterToCheck,characterToCheck-indexOfResolution))) ? true : false;
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
            var fileName = path + "\\" + GetSubFolders(file) + newName + file.Extension.ToLower();
            if (newName + file.Extension != file.Name)
            {
                newName = GetSubFolders(file) + newName;
                RenameMethod(file, fileName, newName);
            }
        }

        private string[] resolutions = { "1080p", "720p" };

        private void NewMoviesRename(string path, FileInfo file)
        {
            string newName = "";
            string restOfName = file.Name;
            try
            {
                string something = resolutions.First(x => file.Name.Contains(x)).ToString();
                if (something != null)
                {
                    int startOfResolution = file.Name.IndexOf(something);
                    newName = file.Name.Substring(0, startOfResolution) + something;
                    richTextBox2.Text += newName + "\n";
                }
            }
            catch (Exception e)
            {
                if (e.Message == "Sequence contains no matching element")
                {
                    //richTextBox2.Text += "Could not find resolution in name of:"+ file.Name + "\n";
                }
                return;
            }

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

        private void IncludeSubFolders_CheckedChanged(object sender, EventArgs e)
        {
            if (path != "") populateFilesToRename(path);
        }

        private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string msg = "Created by Razzer\n"
                +"Contact: Razzer_05@hotmail.com\n"
                +"With subject: FilesRenamer APP";
            var title1 = "About";
            MessageBox.Show(msg, title1, MessageBoxButtons.OK);
        }

        private void QuitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Dispose(true);
        }
    }
}

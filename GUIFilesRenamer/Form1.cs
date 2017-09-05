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
        private RenamedFiles renamedFiles;
        private string fileNames;


        public Form1()
        {
            InitializeComponent();
        }

        private void FolderBrowserDialog1_HelpRequest(object sender, EventArgs e)
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
            //fileNames = DateTime.Now + "\r\n";
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
                        {
                            if (textBox3.Text.Length == 1)//move somewhere else...
                                textBox3.Text = "0" + textBox3.Text;
                            TvShowRename(path, file);
                        }
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
            //Output renamed files to a file for backup
            renamedFiles = new RenamedFiles(fileNames);
            Revert.Enabled = true;
        }

        //display tv show options
        private void DisplayTvShowOptions_CheckedChanged(object sender, EventArgs e)
        {
            label1.Visible = label2.Visible = textBox2.Visible = textBox3.Visible = checkBox1.Checked ? true : false;
            checkBox2.Checked = false;
            checkBox2.Enabled = checkBox1.Checked ? false : true;
        }

        //only works for tv shows with 'E' for the episode numbering
        private void TvShowRename(string path, FileInfo file)
        {
            string newName = "";
            bool foundEpisodeNum = false;
            var indexOfE = 0;
            var fileNameToSearch = file.Name;
            var episodeNum = "";

            while (!foundEpisodeNum)
            {
                indexOfE = fileNameToSearch.IndexOf("E", StringComparison.InvariantCultureIgnoreCase);
                if (indexOfE == -1)
                {
                    string errorMsg = "Could not find episode number in file name " + file.Name;
                    MessageBox.Show(errorMsg);
                    return;
                }
                episodeNum = fileNameToSearch.Substring(indexOfE + 1, 2);
                foundEpisodeNum = char.IsNumber(Convert.ToChar(episodeNum.Substring(0, 1)));
                fileNameToSearch = fileNameToSearch.Substring(indexOfE + 1);
            }
            if (!char.IsNumber(Convert.ToChar(episodeNum.Substring(1, 1))))
                episodeNum = "0" + episodeNum.Substring(0, 1);

            //find the last word inbetween last two / / and then find the number of it..

            var season = path.Substring(path.Length - 2, 2);
            //richTextBox2.Text = season;
            //find the word before the last two // for the name of the tv show


            textBox2.Text = textBox2.Text.Substring(0, 1).ToUpper() + textBox2.Text.Substring(1);

            //var fileName = path + "\\" + textBox2.Text + " S" + textBox3.Text + "E" + episodeNum + file.Extension.ToLower();
            var fileName = path + "\\" + textBox2.Text + " S" + season + "E" + episodeNum + file.Extension.ToLower();
            newName = textBox2.Text + " S" + season + "E" + episodeNum;
            if (newName + file.Extension != file.Name)
                RenameMethod(file, fileName, newName);
        }


        private void TvShowRenameBackup(string path, FileInfo file)
        {
            string newName = "";
            bool foundEpisodeNum = false;
            var indexOfE = 0;
            var fileNameToSearch = file.Name;
            var episodeNum = "";

            while (!foundEpisodeNum)
            {
                indexOfE = fileNameToSearch.IndexOf("E", StringComparison.InvariantCultureIgnoreCase);
                if (indexOfE == -1)
                {
                    string errorMsg = "Could not find episode number in file name " + file.Name;
                    MessageBox.Show(errorMsg);
                    return;
                }
                episodeNum = fileNameToSearch.Substring(indexOfE + 1, 2);
                foundEpisodeNum = char.IsNumber(Convert.ToChar(episodeNum.Substring(0, 1)));
                fileNameToSearch = fileNameToSearch.Substring(indexOfE + 1);
            }
            if (!char.IsNumber(Convert.ToChar(episodeNum.Substring(1, 1))))
                episodeNum = "0" + episodeNum.Substring(0, 1);

            textBox2.Text = textBox2.Text.Substring(0, 1).ToUpper() + textBox2.Text.Substring(1);

            var fileName = path + "\\" + textBox2.Text + " S" + textBox3.Text + "E" + episodeNum + file.Extension.ToLower();
            newName = textBox2.Text;
            if (newName + file.Extension != file.Name)
                RenameMethod(file, fileName, newName);
        }


        private string[] resolutions = { "1080p", "720p" };

        private void NewMoviesRename(string path, FileInfo file)
        {
            string newName = "";
            string restOfName = file.Name;
            try
            {
                string resolutionFound = resolutions.First(x => file.Name.Contains(x)).ToString();
                if (resolutionFound != null)
                {
                    newName = file.Name.Substring(0, file.Name.IndexOf(resolutionFound)) + resolutionFound;
                    if (newName + file.Extension != file.Name)
                    {
                        var fileName = path + "\\" + GetSubFolders(file) + newName + file.Extension.ToLower();
                        newName = GetSubFolders(file) + newName;
                        RenameMethod(file, fileName, newName);
                    }
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
            var subFolders = "";
            if (!checkBox1.Checked)
            {
                subFolders = file.FullName.Replace(path + "\\", "");
                subFolders = subFolders.Replace(file.Name, "");
            }
            else
            {
                checkBox2.Checked = false;
            }
            return subFolders;
        }

        private void RenameMethod(FileInfo file, string newFileName, string newName)
        {
            try
            {
                FileInfo exists = new FileInfo(newFileName);
                if (!exists.Exists)
                {
                    fileNames += "OldName:" + file.FullName + "\r\nNewName:" + newFileName + "\r\n";
                    file.MoveTo(newFileName);
                    richTextBox2.Text += newName + file.Extension + "\n";
                }
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
            checkBox1.Enabled = checkBox2.Checked ? false : true;
        }

        private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string msg = "Created by Razzer\n"
                + "Contact: Razzer_05@hotmail.com\n"
                + "With subject: FilesRenamer APP";
            var title1 = "About";
            MessageBox.Show(msg, title1, MessageBoxButtons.OK);
        }

        private void QuitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
            Dispose(true);
        }

        private void RevertButton_Click(object sender, EventArgs e)
        {
            richTextBox2.Text = "Reverted files back to:\n";
            richTextBox2.Text += renamedFiles.RevertLastSet();
            //renamedFiles.RevertLastSet();
            Revert.Enabled = false;
        }

        private void RevertFileButton_Click(object sender, EventArgs e)
        {
            
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                renamedFiles = new RenamedFiles();
                richTextBox2.Text += renamedFiles.RevertFilesFrom(openFileDialog1.FileName);
            }
        }
    }
}
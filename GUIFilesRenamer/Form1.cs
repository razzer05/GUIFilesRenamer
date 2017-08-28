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
                    ".PNG", ".JPG", ".JPEG", ".BMP", ".GIF",
                    ".WAV", ".MID", ".MIDI", ".WMA", ".MP3", ".OGG", ".RMA",
                    ".AVI", ".MP4", ".DIVX", ".WMV", ".mkv"
                };
        private bool shownError = false;
        const string title = "Error";

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
            {
                textBox1.Text = folderBrowserDialog1.SelectedPath;
                richTextBox1.Text = richTextBox2.Text = "";

                var fileEntries = Directory.EnumerateFiles(textBox1.Text);

                foreach (var file in fileEntries)
                {
                    FileInfo fileInfo = new FileInfo(file);
                    richTextBox1.Text += fileInfo.Name + "\n";
                }
                renameButton.Enabled = true;
            }
        }

        //Rename button click action
        private void button2_Click(object sender, EventArgs e)
        {
            var path = this.textBox1.Text;

            if (path != "" && textBox2.Text != "" && textBox3.Text != "" || path != "" && !checkBox1.Checked)
            {
                var fileEntries = Directory.EnumerateFiles(path);
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
        }

        //only works for tv shows with 'E' for the episode numbering
        private void TvShowRename(string path, FileInfo file)
        {
            string text = "";
            bool foundEpisodeNum = false;
            var i1 = 0;
            var fileNameToSearch = file.Name.Substring(i1);
            while (!foundEpisodeNum)
            {
                i1 = fileNameToSearch.IndexOf("E", StringComparison.InvariantCultureIgnoreCase);
                text = fileNameToSearch.Substring(i1 + 1, 2);
                foundEpisodeNum = Char.IsNumber(Convert.ToChar(text.Substring(0, 1)));
                if (i1 == -1)
                {
                    string errorMsg = "Could not find episode number in file name " + file.Name;
                    MessageBox.Show(errorMsg);
                    return;
                }
                fileNameToSearch = fileNameToSearch.Substring(i1 + 2);
            }
            if (!Char.IsNumber(Convert.ToChar(text.Substring(1, 1))))
                text = text.Substring(0, 1);
            if (text.Length <= 1)
                text = "0" + text;
            if (textBox3.Text.Length <= 1)
                textBox3.Text = "0" + textBox3.Text;

            textBox2.Text = textBox2.Text.Substring(0, 1).ToUpper() + textBox2.Text.Substring(1);
            var fileName = path + "\\" + textBox2.Text + " S" + textBox3.Text + "E" + text + file.Extension.ToLower();
            RenameMethod(file, fileName);
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
                var fileName = path + "\\" + newName + file.Extension.ToLower();
                RenameMethod(file, fileName);
        }

        private void RenameMethod(FileInfo file, string fileName)
        {
            try
            {
                file.MoveTo(fileName);
                richTextBox2.Text += fileName + "\n";
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
            richTextBox1.Text = "";
            textBox1.Text = @"C:\Users\Razzer\Desktop\test";
            string[] filenames = Directory.GetFiles(textBox1.Text, "*", SearchOption.AllDirectories);


            //remove dir info
            //rename filename
            //add dir info and extension back
            //move file
            foreach (var item in filenames)
            {
                
                int lastBackSlash = item.LastIndexOf("\\");
                string onlyFileName = item.Substring(lastBackSlash+1);
                string directoryInfo = item.Substring(0, lastBackSlash + 1);
                string extension = item.Substring(item.LastIndexOf("."));
                onlyFileName = onlyFileName.Replace(extension,"");
                richTextBox1.Text += "directory only : " + directoryInfo + "\n";
                richTextBox1.Text += "filename only : " + onlyFileName + "\n";
                richTextBox1.Text += "extension only : " + extension + "\n";

                //rename onlyFileName here



                //fileName = directoryInfo + reNamedFileName + extension;
                //RenameMethod(item,fileName);
            }
        }
    }
}

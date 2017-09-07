using System;
using System.IO;
using System.Linq;
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
        private RenamedFilesTextFile renamedFiles;
        private string fileNames;
        private bool warnedAboutTvShows = false;
        private string[] resolutions = { "1080p", "720p" };
        private string[] checkKeyWords = { "tv"};

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
            warnedAboutTvShows = false;
        }

        private void populateFilesToRename(string path)
        {
            fileNames = "";
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
            if (path != "" && textBox2.Text != "" || path != "" && !checkBox1.Checked)
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
                            TvShowRename(path, file);
                        }
                        else //movies
                        {
                            if (!warnedAboutTvShows)
                            {
                                int i = 0;
                                foreach (var keyWords in checkKeyWords)
                                {
                                    bool keyword = path.ToLower().Contains(checkKeyWords[i]);
                                    i++;
                                    if (keyword)
                                    {
                                        var response = MessageBox.Show("Are you sure these folders only contain movies?", "Found tvshows label in path", MessageBoxButtons.OKCancel);
                                        if (response == DialogResult.Cancel)
                                        {
                                            warnedAboutTvShows = false;
                                            return;
                                        }
                                        warnedAboutTvShows = true;
                                    }
                                }
                            }

                            MoviesRename(path, file);
                        }
                    }
                }
                //Re-disable rename button
                renameButton.Enabled = false;
                //Output renamed files to a file for backup
                renamedFiles = new RenamedFilesTextFile(fileNames);
                Revert.Enabled = true;
            }
            else
            {
                const string errorMsg = "Fill all the boxes";
                MessageBox.Show(errorMsg, title, MessageBoxButtons.OK);
            }
        }

        //display tv show options
        private void DisplayTvShowOptions_CheckedChanged(object sender, EventArgs e)
        {
            if(textBox2.Text=="")
                textBox2.Text = "E";
            label1.Visible = textBox2.Visible = checkBox1.Checked ? true : false;
        }
        
        private void TvShowRename(string path, FileInfo file)
        {
            string newName = "";
            bool foundEpisodeNum = false;
            var indexOfE = 0;
            var fileNameToSearch = file.Name;
            var episodeNum = "";
            var epSearchText = textBox2.Text;
            while (!foundEpisodeNum)
            {
                indexOfE = fileNameToSearch.IndexOf(epSearchText,StringComparison.OrdinalIgnoreCase);
                if (indexOfE == -1)
                    return;
                
                foundEpisodeNum = char.IsNumber(fileNameToSearch[indexOfE+epSearchText.Length]);
                episodeNum = fileNameToSearch.Substring(indexOfE + epSearchText.Length, 2);
                fileNameToSearch = fileNameToSearch.Substring(indexOfE+epSearchText.Length);
            }
            if (!char.IsNumber(Convert.ToChar(episodeNum.Substring(1, 1))))
                episodeNum = "0" + episodeNum.Substring(0, 1);

            var season = file.FullName.Replace(file.Name,"");
            season = file.FullName.Substring(season.Length - 3, 2);

            //get last two letters of the season - remove file name and then take last two chars
            //use to check that we are getting the a season number... we are presuming that the last two characters of season xx are numbers
            bool seasonFound = char.IsNumber(Convert.ToChar(season.Substring(1, 1)));
            if (!seasonFound) return;
            if (!char.IsNumber(Convert.ToChar(season.Substring(0, 1))))
                season = "0" + season.Substring(1);
            
            var pathWithoutSeason = file.FullName.Remove(file.FullName.LastIndexOf(file.Name)-1);
            pathWithoutSeason = pathWithoutSeason.Remove(pathWithoutSeason.LastIndexOf("\\"));
            var tvShowName = pathWithoutSeason.Substring(pathWithoutSeason.LastIndexOf("\\") + 1);


            newName = GetSubFolders(file) + tvShowName + " S" + season + "E" + episodeNum + file.Extension.ToLower();
            var fileName = path + "\\" + newName;
            
            if (newName != file.Name)
                RenameMethod(file, fileName, newName);
        }

        private void MoviesRename(string path, FileInfo file)
        {
            string newName = "";
            string restOfName = file.Name;
            try
            {
                string resolutionFound = resolutions.First(x => file.Name.Contains(x)).ToString();
                if (resolutionFound != null)
                {
                    newName = file.Name.Substring(0, file.Name.IndexOf(resolutionFound)) + resolutionFound;
                    if (String.Compare(newName + file.Extension, file.Name, StringComparison.InvariantCultureIgnoreCase)==0)
                    {
                        newName = newName.Replace(".", " ");
                        newName = GetSubFolders(file) + newName + file.Extension.ToLower();
                        var fileName = path + "\\" + newName;
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
            var subFolders = file.FullName.Replace(path + "\\", "");
            subFolders = subFolders.Replace(file.Name, "");
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
                    richTextBox2.Text += newName + "\n";
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
            //checkBox1.Enabled = checkBox2.Checked ? false : true;
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
                renamedFiles = new RenamedFilesTextFile();
                richTextBox2.Text += renamedFiles.RevertFilesFrom(openFileDialog1.FileName);
            }
        }
    }
}
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
        public Form1()
        {
            InitializeComponent();
        }

        private void folderBrowserDialog1_HelpRequest(object sender, EventArgs e)
        {
            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            

            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = folderBrowserDialog1.SelectedPath;
                richTextBox1.Text = "";
                richTextBox2.Text = "";


                var fileEntries = Directory.EnumerateFiles(textBox1.Text);

                foreach (var file in fileEntries)
                {
                    FileInfo fileInfo = new FileInfo(file);
                    if (richTextBox1.Text == "")
                        richTextBox1.Text = fileInfo.Name;
                    else
                    {
                        richTextBox1.Text = richTextBox1.Text + "\n" + fileInfo.Name;
                    }
                    
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {

            const string title = "Error";
            var path = this.textBox1.Text;

            if (path != "" && textBox2.Text != "" && textBox3.Text != "")
            {
                
                var fileEntries = Directory.EnumerateFiles(path);

                var i = 1;

                bool shownError = false;


                string[] mediaExtensions = {
                    ".PNG", ".JPG", ".JPEG", ".BMP", ".GIF",
                    ".WAV", ".MID", ".MIDI", ".WMA", ".MP3", ".OGG", ".RMA",
                    ".AVI", ".MP4", ".DIVX", ".WMV", ".mkv"
                };


                foreach (var item in fileEntries)
                {

                    FileInfo file = new FileInfo(item);
                    if (mediaExtensions.Contains(file.Extension, StringComparer.OrdinalIgnoreCase))
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
                            fileNameToSearch = fileNameToSearch.Substring(i1+2);
                        }
                        if (!Char.IsNumber(Convert.ToChar(text.Substring(1, 1))))
                        {
                            text = text.Substring(0, 1);
                        }
                        if (text.Length <= 1)
                        {
                            text = "0" + text;
                        }

                        //MessageBox.Show(Convert.ToString(i1), "error", MessageBoxButtons.OK);
                        if (textBox3.Text.Length <= 1)
                            textBox3.Text = "0" + textBox3.Text;

                        textBox2.Text = textBox2.Text.Substring(0, 1).ToUpper() + textBox2.Text.Substring(1);

                        var fileName = path + "\\" + textBox2.Text + " S" + textBox3.Text + "E" + text + file.Extension.ToLower();

                        try
                        {
                            file.MoveTo(fileName);
                            if (richTextBox2.Text == "")
                                richTextBox2.Text = textBox2.Text + " S" + textBox3.Text + "E" + text + file.Extension.ToLower();
                            else
                            {
                                richTextBox2.Text = richTextBox2.Text + "\n" + textBox2.Text + " S" + textBox3.Text + "E" + text + file.Extension.ToLower();
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

                        i++;
                    }
                    else
                    {
                        string errorMsg = "Could not rename file " + file.Name +
                                                " as the extension is not a media type";
                        MessageBox.Show(errorMsg);
                    }
                }

            }
            else
            {
                const string errorMsg = "Fill all the boxes";
                MessageBox.Show(errorMsg, title, MessageBoxButtons.OK);
            }

            textBox2.Text = "";
            textBox3.Text = "";
        }

    }
}

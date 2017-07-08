using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AuthTool
{
    public partial class AuthForm : Form
    {
        public AuthForm()
        {
            InitializeComponent();
        }

        private void btnLoadLocation_Click(object sender, EventArgs e)
        {
            DialogResult dialog_result = fbdLocation.ShowDialog();
            if (dialog_result == DialogResult.OK)
            {
                string projectLocation = fbdLocation.SelectedPath;
                txtProjectLocation.Text = projectLocation;
            }
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            if (txtPackage.Text.Length * txtProjectLocation.Text.Length != 0)
            {
                string path = ConvertPackageToPath(txtProjectLocation.Text, txtPackage.Text);
                Directory.CreateDirectory(path + "\\auth");
                Directory.CreateDirectory(path + "\\custom");
                Directory.CreateDirectory(txtProjectLocation.Text + "\\app\\src\\main\\assets");
                Directory.CreateDirectory(txtProjectLocation.Text + "\\app\\src\\main\\assets\\fonts");

                string excutePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);    // Get current directory

                ConfigManifest(excutePath + "\\Resources", txtProjectLocation.Text, txtPackage.Text);
                CopyAllFilesToPath(excutePath + "\\Resources\\java\\auth", path + "\\auth", true);
                CopyAllFilesToPath(excutePath + "\\Resources\\java\\custom", path + "\\custom", true);    // Copy custom controls
                CopyAllFilesToPath(excutePath + "\\Resources\\assets\\fonts", txtProjectLocation.Text + "\\app\\src\\main\\assets\\fonts", false);
                CopyAllFilesToPath(excutePath + "\\Resources\\res\\values", txtProjectLocation.Text + "\\app\\src\\main\\res\\values", false);
                CopyAllFilesToPath(excutePath + "\\Resources\\res\\drawable", txtProjectLocation.Text + "\\app\\src\\main\\res\\drawable", false);
                CopyAllFilesToPath(excutePath + "\\Resources\\res\\layout", txtProjectLocation.Text + "\\app\\src\\main\\res\\layout", true);
            }
        }

        private void CopyAllFilesToPath(string sourcePath, string targetPath, bool isConfigPackage)
        {
            if (Directory.Exists(sourcePath))
            {
                string[] files = Directory.GetFiles(sourcePath);

                // Copy the files and overwrite destination files if they already exist.
                foreach (string s in files)
                {
                    // Use static Path methods to extract only the file name from the path.
                    string fileName = Path.GetFileName(s);
                    string destFile = Path.Combine(targetPath, fileName);

                    if (isConfigPackage)
                    {
                        string ext = Path.GetExtension(s);
                        if (ext.Equals(".java"))
                        {
                            ConfigPackageJAVA(sourcePath + "\\" + fileName, txtPackage.Text);
                        }
                        else if (ext.Equals(".xml"))
                        {
                            ConfigPackageXML(sourcePath + "\\" + fileName, txtPackage.Text);
                        }
                    }

                    File.Copy(s, destFile, true);
                }
            }
        }

        // Change default package for this project
        private void ConfigPackageJAVA(string filePath, string package)
        {
            string content = "";
            var lines = File.ReadLines(filePath);
            string fileName = Path.GetFileName(filePath);

            foreach (var line in lines)
            {
                if (line.Contains("package"))
                {
                    if (fileName.Contains("Activity.java"))
                    {
                        content += "package " + package + ".auth;";
                    }
                    else
                    {
                        content += "package " + package + ".custom;";
                    }
                }
                else if (line.Contains("import ") && line.Contains(".R;"))
                {
                    content += "import " + package + ".R;";
                }
                else
                {
                    content += line;
                }
                content += Environment.NewLine;
            }
            File.WriteAllText(filePath, content);
        }

        private void ConfigPackageXML(string filePath, string package)
        {
            string content = "";
            var lines = File.ReadLines(filePath);
            foreach (var line in lines)
            {
                if (line.Contains(".custom.EditTextCSE"))
                {
                    content += "<" + package + ".custom.EditTextCSE";
                }
                else if (line.Contains(".custom.ButtonCSE"))
                {
                    content += "<" + package + ".custom.ButtonCSE";
                }
                else if (line.Contains(".custom.TextViewCSE"))
                {
                    content += "<" + package + ".custom.TextViewCSE";
                }
                else
                {
                    content += line;
                }
                content += Environment.NewLine;
            }
            File.WriteAllText(filePath, content);
        }

        private void ConfigManifest(string sourcePath, string projectPath, string package)
        {
            // E:\Github\MasterChef\app\src\main
            string content = "";
            var lines = File.ReadLines(sourcePath + "\\AndroidManifest.xml");
            foreach (var line in lines)
            {
                if (line.Contains("package=\""))
                {
                    content += "package=\""+ package +"\">";
                }
                else
                {
                    content += line;
                }
                content += Environment.NewLine;
            }
            File.WriteAllText(projectPath + "\\app\\src\\main\\AndroidManifest.xml", content);
        }

        private string ConvertPackageToPath(string pathProject, string package)
        {
            return pathProject + "\\app\\src\\main\\java\\" + package.Replace(".", "\\");
        }

        private void txtPackage_KeyDown(object sender, KeyEventArgs e)
        {
            btnGenerate_Click(sender, e);
        }
    }
}
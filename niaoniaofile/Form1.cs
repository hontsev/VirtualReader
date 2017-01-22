using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;

namespace niaoniaofile
{
    public partial class Form1 : Form
    {
        public delegate void sendStringDelegate(string str);
        private NiaoNiaoFileController nnc;

        public Form1()
        {
            InitializeComponent();
            nnc = new NiaoNiaoFileController();
        }

        public void print(string str)
        {
            if (textBox2.InvokeRequired)
            {
                sendStringDelegate mevent = new sendStringDelegate(print);
                Invoke(mevent, (object)str);
            }
            else
            {
                textBox2.AppendText(str + "\r\n");
            }
        }

        //private void changeFile(string fname)
        //{
        //    FileStream file=File.Open(fname,FileMode.Open);
        //    StreamReader sr = new StreamReader(file);
        //    string res = sr.ReadToEnd();
        //    sr.Close();
        //    file.Close();

        //    string[] list = res.Split('\n');
        //    int newlast = 24;
        //    int empty = 4;
        //    int originlast=8;
        //    for (int i = 2; i < list.Length-1; i++)
        //    {
        //        string[] oneline = list[i].Split(' ');
        //        int oribegin = Int32.Parse(oneline[3].ToString());
        //        int orilast = Int32.Parse(oneline[4].ToString());
        //        int beginnum = oribegin / originlast;
        //        int newbegin = beginnum * (newlast + empty);
        //        oneline[3] = newbegin.ToString();
        //        oneline[4] = newlast.ToString();
        //        string tres="";
        //        for(int j=0;j<oneline.Length;j++)
        //        {
        //            tres+=" "+oneline[j];
        //        }
        //        list[i]=tres;
        //    }
        //    string newname = "change.nn";
        //    FileStream output=File.Create(newname);
        //    StreamWriter sw = new StreamWriter(output);
        //    for (int i = 0; i < list.Length; i++)
        //    {
        //        sw.WriteLine(list[i]);
        //    }
        //        sw.Close();
        //    output.Close();
        //}


        private void getNNFile(object str)
        {

            print("开始生成袅袅文件。。");
            string path = @"output\";
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            string filename = @"output.nn";
            using (FileStream fs = new FileStream(path + filename, FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(nnc.getNiaoNiaoFile(str as string));
                }
            }
            print("生成完毕。");
        }



        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            string filename = openFileDialog1.FileName;
            //changeFile(filename);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            nnc.soundheight = int.Parse(numericUpDown1.Value.ToString());
            new Thread(getNNFile).Start(textBox1.Text);
        }

        private void workMakeNiaoNiaoFile()
        {
            print("begin");

        }

        //private void button1_Click(object sender, EventArgs e)
        //{
        //    string path = @"D:\downloads\fangsi.dctx";
        //    string outputpath = @"Dictionary\Dictionary.txt";
        //    Dictionary<string, string> output = new Dictionary<string, string>();
        //    using (FileStream fs = new FileStream(path, FileMode.Open))
        //    {
        //        using (StreamReader sr = new StreamReader(fs))
        //        {
        //            while (!sr.EndOfStream)
        //            {
        //                string tmp=sr.ReadLine().Replace("\r","");
        //                if (!string.IsNullOrWhiteSpace(tmp))
        //                {
        //                    string name = sr.ReadLine().Replace("\r", "");
        //                    output[name] = tmp;
        //                }
        //            }
        //        }
        //    }
        //    using (FileStream fs = new FileStream(outputpath, FileMode.Create))
        //    {
        //        using (StreamWriter sw = new StreamWriter(fs))
        //        {
        //            foreach (var item in output)
        //            {
        //                sw.WriteLine(string.Format("{0}|{1}",item.Key,item.Value));
        //            }
        //        }
        //    }
        //}

        private void button1_Click_1(object sender, EventArgs e)
        {
            string str = textBox3.Text;
            if (!string.IsNullOrWhiteSpace(str))
            {
                char ch = str[0];
                try
                {
                    var res = PinYinConverter.GetPinYinWithTone(ch);
                    string resstr = ch.ToString() + "\r\n";
                    foreach (var r in res)
                    {
                        resstr += string.Format("{0},", r);
                    }
                    print(resstr);
                }
                catch
                {

                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string str = textBox1.Text.Replace("\r", "").Replace("\n", "");
            string filename = @"Dictionary\SimpleWordAdd.txt";

            List<string> allres = new List<string>();
            bool getit = false;
            char han=' ';
            string pinyin="";
            int sound = 5;
            for (int i = 0; i < str.Length; i++)
            {
                char[] sound1={'ā','ō','ē','ī','ū','ǖ'};
                char[] sound2={'á','ó','é','í','ú','ǘ'};
                char[] sound3={'ǎ','ǒ','ě','ǐ','ǔ','ǚ'};
                char[] sound4={'à','ò','è','ì','ù','ǜ'};
                if (Regex.IsMatch(str[i].ToString(), @"[\u4e00-\u9fbb]"))
                {
                    if (han != ' ')
                    {
                        //存储一条
                        string res = string.Format("{0}|{1}{2}", han, pinyin, sound);
                        allres.Add(res);
                    }
                    getit = true;
                    pinyin = "";
                    han = str[i];
                }
                else
                {
                    if (!getit) continue;
                    if (str[i] == ' ' ) continue;
                    if (str[i] == '/')
                    {
                        getit = false; continue;
                    }
                    char ch = str[i];
                    for (int j = 0; j < sound1.Length; j++)
                    {
                        if (sound1[j] == ch)
                        {
                            sound = 1;
                            switch (j)
                            {
                                case 0: ch = 'a'; break;
                                case 1: ch = 'o'; break;
                                case 2: ch = 'e'; break;
                                case 3: ch = 'i'; break;
                                case 4: ch = 'u'; break;
                                case 5: ch = 'v'; break;
                                default: break;
                            }
                            break;
                        }
                    }
                    for (int j = 0; j < sound2.Length; j++)
                    {
                        if (sound2[j] == ch)
                        {
                            sound = 2;
                            switch (j)
                            {
                                case 0: ch = 'a'; break;
                                case 1: ch = 'o'; break;
                                case 2: ch = 'e'; break;
                                case 3: ch = 'i'; break;
                                case 4: ch = 'u'; break;
                                case 5: ch = 'v'; break;
                                default: break;
                            }
                            break;
                        }
                    }
                    for (int j = 0; j < sound3.Length; j++)
                    {
                        if (sound3[j] == ch)
                        {
                            sound = 3;
                            switch (j)
                            {
                                case 0: ch = 'a'; break;
                                case 1: ch = 'o'; break;
                                case 2: ch = 'e'; break;
                                case 3: ch = 'i'; break;
                                case 4: ch = 'u'; break;
                                case 5: ch = 'v'; break;
                                default: break;
                            }
                            break;
                        }
                    }
                    for (int j = 0; j < sound4.Length; j++)
                    {
                        if (sound4[j] == ch)
                        {
                            sound = 4;
                            switch (j)
                            {
                                case 0: ch = 'a'; break;
                                case 1: ch = 'o'; break;
                                case 2: ch = 'e'; break;
                                case 3: ch = 'i'; break;
                                case 4: ch = 'u'; break;
                                case 5: ch = 'v'; break;
                                default: break;
                            }
                            break;
                        }
                    }

                    pinyin += ch.ToString();

                }
            }
            if (han != ' ')
            {
                //存储一条
                string res = string.Format("{0}|{1}{2}", han, pinyin, sound);
                allres.Add(res);
            }
            //print(allres[0]);
            using (FileStream fs = new FileStream(filename, FileMode.Append))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    foreach (var line in allres)
                    {
                        sw.WriteLine(line);
                    }

                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
        }
    }
}

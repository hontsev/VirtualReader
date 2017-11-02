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
using System.Speech.Synthesis.TtsEngine;

namespace SpeechSynthesizer
{
    public partial class Form1 : Form
    {
        private NiaoNiaoFileController nnc;
        private UTAUFileController utauc;
        private MYSSController myssc;

        public Form1()
        {
            InitializeComponent();
            nnc = new NiaoNiaoFileController();
            utauc = new UTAUFileController();
        }

        public void print(string str)
        {
            if (textBox2.InvokeRequired)
            {
                MyDelegates.sendStringDelegate mevent = new MyDelegates.sendStringDelegate(print);
                Invoke(mevent, (object)str);
            }
            else
            {
                textBox2.AppendText(str + "\r\n");
            }
        }

        public void printDebug(string str)
        {
            if (textBox4.InvokeRequired)
            {
                MyDelegates.sendStringDelegate mevent = new MyDelegates.sendStringDelegate(printDebug);
                Invoke(mevent, (object)str);
            }
            else
            {
                textBox4.Text = str;
            }
        }

        public void setGUIStatus(bool enabled)
        {
            if (button2.InvokeRequired)
            {
                MyDelegates.sendBoolDelegate mevent = new MyDelegates.sendBoolDelegate(setGUIStatus);
                Invoke(mevent, (object)enabled);
            }
            else
            {
                if (enabled)
                {
                    button2.Enabled = true;
                    button3.Enabled = true;
                    button5.Enabled = true;
                    button8.Enabled = true;
                    button9.Enabled = true;
                }
                else
                {
                    button2.Enabled = false;
                    button3.Enabled = false;
                    button5.Enabled = false;
                    button8.Enabled = false;
                    //button9.Enabled = false;
                }
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
            setGUIStatus(false);
            print("开始生成袅袅文件，请稍候");
            string path = Application.StartupPath+@"\output\";
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            string filename = @"output.nn";
            string file=path+filename;
            using (FileStream fs = new FileStream(file, FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(nnc.getNiaoNiaoFile(str as string,this.print));
                }
            }
            print(string.Format("生成完毕。输出路径为\r\n{0}",file));
            setGUIStatus(true);
        }

        private static string removeInvalidChars(string str)
        {
            string strFileName = str;
            StringBuilder rBuilder = new StringBuilder(strFileName);
            foreach (char rInvalidChar in Path.GetInvalidFileNameChars())
                rBuilder.Replace(rInvalidChar.ToString(), string.Empty);
            return str;
        }

        private void getNNFileBySpeakers(object str)
        {
            setGUIStatus(false);
            print("开始生成袅袅文件，请稍候");
            string path = Application.StartupPath + @"\output\files\";
            if (Directory.Exists(path)) Directory.Delete(path,true);
            Directory.CreateDirectory(path);
            //string filename = @"output.nn";
            string[] sentences = ((string)str).Split('\n');
            for(int i=0;i<sentences.Length;i++)
            {
                string s = sentences[i].Replace("\r", "");
                int begin=s.IndexOf("：");
                if(begin<0)continue;
                string speaker=s.Substring(0,begin);
                string content=s.Substring(begin);
                string filename = string.Format("{0}{1}{2}.nn", i.ToString().PadLeft(4,'0'), speaker, removeInvalidChars(content).Substring(0, Math.Min(10, removeInvalidChars(content).Length)));
                string file = path + filename;
                using (FileStream fs = new FileStream(file, FileMode.Create))
                {
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        sw.Write(nnc.getNiaoNiaoFile(content, this.print));
                    }
                }
            }

            print(string.Format("生成完毕。输出路径为\r\n{0}", path));
            setGUIStatus(true);
        }

        private void getUTAUFile(object str)
        {
            setGUIStatus(false);
            print("开始生成UST文件，请稍候");
            string path = Application.StartupPath + @"\output\";
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            string filename = @"output.ust";
            string file = path + filename;
            using (FileStream fs = new FileStream(file, FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(utauc.getUSTFile(str as string, this.print));
                }
            }
            print(string.Format("生成完毕。输出路径为\r\n{0}", file));
            setGUIStatus(true);
        }

        private void createMYSSwavs(object sentences)
        {
            string bufferpath=@"tmp_talking\";
            string[] sentence=sentences as string[];
            for (int i=0;i< sentence.Length;i++)
            {
                if (!isReading) break;
                string filename = string.Format(@"{0}tmp_{1}.wav", bufferpath, i);
                int[] tmp = myssc.showSound(sentence[i], this.print);
                myssc.writeWAV(tmp,filename);
            }
        }

        private void getMYSSandRead(object str)
        {
            setGUIStatus(false);
            print("开始合成");
            PinYinConverter pyconv = new PinYinConverter();
            string[] sentences = pyconv.cutSentencesAll(str as string);
            print(string.Format("拆分句子完毕：共{0}个短句", sentences.Length));
            
            string bufferpath=@"tmp_talking\";
            if (Directory.Exists(bufferpath)) Directory.Delete(bufferpath, true);
            Directory.CreateDirectory(bufferpath);
            new Thread(createMYSSwavs).Start(sentences);
            for (int i = 0; i < sentences.Length; i++)
            {
                if (!isReading) break;
                string filename = string.Format(@"{0}tmp_{1}.wav", bufferpath, i);
                //int[] tmp = myssc.showSound(sentences[i], this.print);
                while (!File.Exists(filename)) if (!isReading) break;
                myssc.playSound(filename);
            }
            setGUIStatus(true);
            isReading = true;
            setIsReadStatus();
        }

        private void getMYSS(object str)
        {
            setGUIStatus(false);

            print("开始生成");
            int[] res=myssc.showSound(str as string, this.print);
            print("生成完毕。合成WAV文件。");
            myssc.writeWAV(res);
            print("WAV生成完毕。");
            //string path = Application.StartupPath + @"\output\";
            //if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            //string filename = @"output.ust";
            //string file = path + filename;
            //using (FileStream fs = new FileStream(file, FileMode.Create))
            //{
            //    using (StreamWriter sw = new StreamWriter(fs))
            //    {
            //        sw.Write(utauc.getUSTFile(str as string, this.print));
            //    }
            //}
            //print(string.Format("生成完毕。输出路径为\r\n{0}", file));
            setGUIStatus(true);
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            string filename = openFileDialog1.FileName;
            //changeFile(filename);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBox2.Text = "";
            nnc.soundSpeed = int.Parse(numericUpDown2.Value.ToString());
            nnc.soundheight = int.Parse(numericUpDown1.Value.ToString());
            if (checkBox1.Checked)
            {
                new Thread(getNNFileBySpeakers).Start(textBox1.Text);
            }
            else
            {
                new Thread(getNNFile).Start(textBox1.Text);
            }
            
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
            //Dictionary<string, string> dictionary = new Dictionary<string, string>();
            //using (FileStream fs = new FileStream(@"Dictionary\Dictionary.txt", FileMode.OpenOrCreate))
            //{
            //    using (StreamReader sr = new StreamReader(fs))
            //    {
            //        while (!sr.EndOfStream)
            //        {
            //            string[] tmp = sr.ReadLine().Split('|');
            //            if (tmp.Length == 2)
            //            {
            //                try
            //                {
            //                    int index = tmp[0].IndexOf('行');
            //                    if (index >= 0)
            //                    {
            //                        string[] list = tmp[1].Split(' ');
            //                        if (list[index] == "xing4") { list[index] = "xing2"; }
            //                        string tmp1 = "";
            //                        foreach (var i in list) tmp1 += i + " ";
            //                        if (tmp1.EndsWith(" ")) tmp1 = tmp1.Substring(0, tmp1.Length - 1);
            //                        dictionary[tmp[0]] = tmp1;
            //                    }
            //                    else
            //                    {
            //                        dictionary[tmp[0]] = tmp[1];
            //                    }

            //                }
            //                catch
            //                {
            //                }
            //            }
            //        }
            //    }
            //}

            //using (FileStream fs = new FileStream(@"Dictionary\Dictionary.txt", FileMode.Create))
            //{
            //    using (StreamWriter sr = new StreamWriter(fs))
            //    {
            //        foreach (var item in dictionary)
            //        {
            //            sr.WriteLine("{0}|{1}", item.Key, item.Value);
            //        }
            //    }
            //}

            string str = textBox3.Text;
            if (!string.IsNullOrWhiteSpace(str))
            {
                char ch = str[0];
                try
                {
                    printDebug(((int)ch).ToString());

                    var res = PinYinConverter.GetPinYinWithTone(ch);
                    string resstr = ch.ToString() + "\r\n";
                    foreach (var r in res)
                    {
                        resstr += string.Format("{0},", r);
                    }
                    printDebug(resstr);
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

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                Environment.Exit(0);
            }
            catch
            {

            }
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            textBox1.Focus();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            textBox2.Text = "";
            utauc.soundSpeed = int.Parse(numericUpDown3.Value.ToString());
            utauc.soundheight = int.Parse(numericUpDown4.Value.ToString());
            new Thread(getUTAUFile).Start(textBox1.Text);
        }

        private void getPinYinString(object str)
        {
            print("开始将文字转化为拼音。请稍等。");
            setGUIStatus(false);
            string outputstr = "";
            string tstr = (string)str;
            PinYinConverter pyc = new PinYinConverter();
            string[] sentences = pyc.cutSentencesAll(tstr);
            foreach (var s in sentences)
            {
                var res = pyc.getPinYinList(s);
                
                foreach (var sen in res)
                {
                    foreach (var word in sen)
                    {
                        outputstr += word + " ";
                    }
                    //outputstr += "|";
                }
                outputstr += "，";
            }

            printDebug(outputstr);
            setGUIStatus(true);
            print("拼音转化完毕。");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            
            
            new Thread(getPinYinString).Start((object)textBox3.Text.ToString());
        }

        private void speak(object str)
        {
            MicrosoftTTS read = new MicrosoftTTS();
            read.SpeakChina((string)str);

            //print("开始调微软的工具阅读此文本");
            //System.Speech.Synthesis.SpeechSynthesizer sp = new System.Speech.Synthesis.SpeechSynthesizer();
            //string readsource = str as string;
            //// Configure the audio output. 
            //sp.SetOutputToDefaultAudioDevice();
            //sp.SelectVoice("Microsoft Server Speech Text to Speech Voice (zh-CN, HuiHui)")
            //// Speak a string.
            //sp.Speak(readsource);
        }



        private void button6_Click(object sender, EventArgs e)
        {
            new Thread(speak).Start((object)textBox3.Text);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", Application.StartupPath + "\\output\\");
        }

        private bool initMYSS()
        {
            print("开始初始化合成引擎");
            setGUIStatus(false);
            try
            {
                int speed = int.Parse(numericUpDown5.Value.ToString());
                int height = int.Parse(numericUpDown6.Value.ToString());
                int pitch = int.Parse(numericUpDown7.Value.ToString());
                string filepath = textBox5.Text + @"\";
                if (myssc == null || myssc.filepath != filepath)
                    this.myssc = new MYSSController(filepath);
                myssc.soundheight = height;
                myssc.soundSpeed = speed;
                myssc.defaultpitch = pitch;
            }
            catch
            {
                print("合成引擎初始化失败。");
                setGUIStatus(true);
                return false;
            }
            return true;

        }

        private void button8_Click(object sender, EventArgs e)
        {
            if(initMYSS())
                new Thread(getMYSS).Start((object)textBox1.Text.ToString());
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.A)
            {
                textBox1.SelectAll();
            }
        }


        public bool isReading = false;
        private void setIsReadStatus()
        {
            if (button9.InvokeRequired)
            {
                MyDelegates.sendVoidDelegate mEvent = new MyDelegates.sendVoidDelegate(setIsReadStatus);
                Invoke(mEvent);
            }
            else
            {
                if (isReading)
                {
                    //结束朗读线程
                    isReading = false;
                    button9.Text = "开始朗读√";
                }
                else
                {
                    //开始
                    isReading = true;
                    button9.Text = "终止朗读√";
                    button9.Enabled = false;
                    if (initMYSS())
                        new Thread(getMYSSandRead).Start((object)textBox1.Text);
                    button9.Enabled = true;
                }
            }

        }
        private void button9_Click(object sender, EventArgs e)
        {

            setIsReadStatus();
        }
    }
}

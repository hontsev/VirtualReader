using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mySpeechSynthesizer;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.IO;

namespace SpeechSynthesizer
{
    class MYSSController
    {
        
        private PinYinConverter pyconv;

        public int soundheight;
        public int soundSpeed;

        string[] sounds;
        List<int[]> soundOrigins;
        public string filepath = "";
        string output =  @"output\tmp.wav";
        string outputOri = @"output\tmp_origin.wav";
        string outputTone = @"output\tmp_tone.wav";

        public MYSSController(string sourcePath)
        {
            pyconv = new PinYinConverter();
            soundheight = 120;
            soundSpeed = 600;

            //init
            filepath = sourcePath;

            initParams();
            initParamOrigins();


        }

        public string getHeader(int soundnum,int maxlen)
        {
            int blocknum = (maxlen / 32) + 2;
            string header = string.Format("{0}.0 4 4 {1} 19 0 0 0 0 0\n{2}\n", soundSpeed, blocknum, soundnum);
            return header;
        }


        public string DecodeBase64(string code, string code_type = "utf-8")
        {
            string decode = "";
            byte[] bytes = Convert.FromBase64String(code);
            try
            {
                decode = Encoding.GetEncoding(code_type).GetString(bytes);
            }
            catch
            {
                decode = code;
            }
            return decode;
        }

        public byte[] readVoiceD(int begin = 0, int len = -1)
        {
            string file = filepath + "voice.d";
            byte[] res;

            using (FileStream fs = new FileStream(file, FileMode.Open))
            {
                using (BinaryReader sr = new BinaryReader(fs))
                {
                    if (len < 0) len = (int)fs.Length;
                    res = new byte[len];
                    fs.Seek(begin, SeekOrigin.Begin);
                    res = sr.ReadBytes(len);
                }
            }
            return res;
        }
        public void initParams()
        {
            string file = filepath + "inf.d";
            List<string> res = new List<string>();
            using (FileStream fs = new FileStream(file, FileMode.Open))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    while (!sr.EndOfStream)
                    {
                        string nowstr = DecodeBase64(sr.ReadLine()).Replace("\r", "").Replace("\n", "");
                        res.Add(nowstr);
                    }
                }
            }
            res.RemoveAt(0);
            res.RemoveAt(0);
            sounds=res.ToArray();
        }

        public void initParamOrigins()
        {
            this.soundOrigins = new List<int[]>();
            foreach (string item in sounds)
            {
                string[] tmp = item.Split(' ');
                int begin = int.Parse(tmp[1]);
                int end = int.Parse(tmp[2]);
                soundOrigins.Add(WAVAnalyzer.getSample(readVoiceD(begin, end)));
            }
        }

        public int[] getZipDatas(int toneNum,int beforeToneNum=-1,int nextToneNum=-1)
        {
            int[] datas;
            switch (toneNum)
            {
                case 1:
                    if (nextToneNum == 5)
                    {
                        datas = new int[5] { 110, 110, 110, 110, 90 };
                    }
                    else
                    {
                        datas = new int[5] { 110, 110, 110, 110, 110 };
                    }
                    
                    break;
                case 2:
                    datas = new int[5] { 80, 80, 90, 105, 110 };
                    break;
                case 3:
                    datas = new int[5] { 70, 60, 60, 75, 75 };
                    break;
                case 4:
                    datas = new int[5] { 110, 98, 85, 72, 60 };
                    break;
                case 5:
                default:
                    datas = new int[5] { 85, 80, 75, 72, 70 };
                    break;
            }

            //datas = new int[2] { 70, 130 };

            return datas;
        }



        public int[] getSoundData(int n,int pit, double len=1.0)
        {
            //int pit = int.Parse(numericUpDown1.Value.ToString());
            int[] pitdata = getZipDatas(pit);
            //double len = double.Parse(numericUpDown2.Value.ToString()) / 100;
            //string[] tmp = sounds[n].Split(' ');
            //int begin = int.Parse(tmp[1]);
            //int end = int.Parse(tmp[2]);
            //int head = int.Parse(tmp[3]);
            //int foot = int.Parse(tmp[4]);
            return WAVAnalyzer.getWAVdata(soundOrigins[n], len, pitdata);
            
            //System.Media.SoundPlayer player = new System.Media.SoundPlayer(outputTone);
            //player.PlaySync();
        }

        private int getTargetNumber(string ch)
        {
            for (int i=0;i<sounds.Length;i++)
            {
                if (sounds[i].Split(' ')[0] == ch)
                {
                    return i;
                }
            }
            return 0;
        }

        public void writeWAV(int[] wavdata,string filename=null)
        {
            if (filename == null) filename = outputTone;
            WAVAnalyzer.writeWAV(wavdata, filename);
        }

        public void playSound(string filename)
        {
            System.Media.SoundPlayer player = new System.Media.SoundPlayer(filename);
            player.PlaySync();
        }

        public int[] showSound(string str, MyDelegates.sendStringDelegate printEvent)
        {
            printEvent(string.Format("开始生成字符串（开头为：{0}...）", (str.Length >= 7 ? str.Substring(0, 7) : str)));
            Stopwatch sw = new Stopwatch();
            sw.Start();
            string[] sentences = pyconv.cutSentencesAll(str);
            sw.Stop();
            printEvent(string.Format("拆分句子完毕：{0}ms，共{1}个短句", sw.ElapsedMilliseconds, sentences.Length));
            int num = 0;
            int index = 0;
            int nowindex = 0;

            int scale = 2;

            List<int> allres = new List<int>();

            foreach (var sentence in sentences)
            {
                for (int s = 0; s < 10000 * ((double)100 / soundSpeed); s++)
                    {
                        allres.Add(0);
                    }
                List<List<string>> pinyin = pyconv.getPinYinList(sentence);
                foreach (var p in pinyin)
                {
                    int d;
                    double length = 1;
                    for (int i = 0; i < p.Count; i++)
                    {
                        if (i < p.Count - 1)
                            //是一个词的中间字，因此间隔小
                            d = 0;
                        else
                            d = 1;

                        if (p[i].EndsWith("5"))
                        {
                            //轻声
                            length = 0.85;
                            //if (p.Count == 1 && index > 0) index -= 1;
                        }
                        else if (p[i].EndsWith("3"))
                        {
                            //上声，念得长
                            length = 1.15;
                        }
                        else if (p[i].EndsWith("4") && i == p.Count - 1)
                        {
                            //去声且在结尾，念得短
                            length = 1;
                        }
                        else if (p[i].EndsWith("2") && i < p.Count - 1)
                        {
                            //阳平且在句中，念得短
                            length = 0.95;
                        }
                        else
                        {
                            length = 1;
                        }
                        string beforep = "";
                        int beforetonenum = -1;
                        if (i >= 1)
                        {
                            beforep = p[i - 1];
                            beforetonenum = int.Parse(beforep.Substring(beforep.Length - 1));
                        }
                        string nextp = "";
                        int nexttonenum = -1;
                        if (i < p.Count - 1)
                        {
                            nextp = p[i + 1];
                            nexttonenum = int.Parse(nextp.Substring(nextp.Length - 1));
                        }
                        int tonenum=int.Parse(p[i].Substring(p[i].Length-1));
                        string ch=p[i].Substring(0,p[i].Length-1);

                        double sneeze = 0.22;
                        double part = 1.0 * (sneeze / 2);
                        int[] thisSoundFrame = WAVAnalyzer.getWAVdata(
                            soundOrigins[getTargetNumber(ch)],
                            (double)(length + part) * ((double)100 / soundSpeed), 
                            getZipDatas(tonenum, beforetonenum,nexttonenum));
                        for (int k = 0; k < thisSoundFrame.Length;k++ )
                        {
                            thisSoundFrame[k] = (int)((double)thisSoundFrame[k] * soundheight / 100);
                        }
                        
                        //for (int k = 0; k < partlen; k++)
                        //{
                        //    thisSoundFrame[k] = (int)(thisSoundFrame[k] * ((double)k / partlen));
                        //}
                        //for (int k = thisSoundFrame.Length - partlen; k < partlen; k++)
                        //{
                        //    thisSoundFrame[k] = (int)(thisSoundFrame[k] * (1 - (double)k / partlen));
                        //}
                        int partlen = (int)(thisSoundFrame.Length * part);
                        //if (thisSoundFrame.Length >= partlen)
                        //{
                            for (int k = 0; k < partlen; k++)
                            {
                                //allres[allres.Count - partlen + k] = (int)(allres[allres.Count - partlen + k] * ((double)k / partlen) + thisSoundFrame[k]*(1 - (double)k / partlen));
                                allres[allres.Count - partlen + k] = (int)(allres[allres.Count - partlen + k] * 1 + thisSoundFrame[k] * 1);

                            }
                            for (int k = partlen; k < thisSoundFrame.Length; k++)
                            {
                                allres.Add(thisSoundFrame[k]);
                            }
                        //}
                        //else
                        //{
                        //    for (int k = 0; k < thisSoundFrame.Length; k++)
                        //    {
                        //        allres.Add(thisSoundFrame[k]);
                        //    }
                        //}
                        
                        //foreach (var s in thisSoundFrame) allres.Add((int)((double)s*soundheight/100));

                        
                        if (d > 0)
                        {
                            for (int s = 0; s < 4200* ((double)100 / soundSpeed); s++)
                            {
                                allres.Add(0);
                            }
                        }
                        num++;
                    }
                }
                printEvent(string.Format("生成短句{0}/{1}", ++nowindex, sentences.Length));
            }

            
            return allres.ToArray();
        }
    }
}

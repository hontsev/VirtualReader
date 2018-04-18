using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using JiebaNet.Segmenter;
using System.Diagnostics;

namespace VirtualReader
{
    class UTAUFileController
    {
        private PinYinConverter pyconv;
        public int soundheight;
        public int soundSpeed;

        public UTAUFileController()
        {
            this.pyconv = new PinYinConverter();
            soundheight = 63;
            soundSpeed = 140;
        }

        private string getUSTHeader()
        {
            string res = "";
            //res += "[#VERSION]\r\n";
            //res += string.Format("{0}\r\n", "UST Version1.2");
            res += "[#SETTING]\r\n";
            res += string.Format("Tempo={0}.00\r\n", soundSpeed);
            res += string.Format("Tracks={0}\r\n", 1);
            res += string.Format("ProjectName={0}\r\n", "New Project");
            //res += string.Format("VoiceDir={0}\r\n", "%VOICE%枸杞子");
            //res += string.Format("OutFile={0}\r\n", "newfile.wav");
            //res += string.Format("CacheDir={0}\r\n", "xiaololi.cache");
            //res += string.Format("Tool1={0}\r\n", "wavtool.exe");
            //res += string.Format("Tool2={0}\r\n", "resampler.exe");
            res += string.Format("Mode2={0}\r\n", "True");
            return res;
        }

        private string getUSTFooter()
        {
            string res = "";
            res += "[#TRACKEND]\r\n";
            return res;
        }


        private string scalePBValue(string origin, bool isDouble, int len1, int len2 = 480)
        {
            if (isDouble) 
                return origin;
            string res = "";

            string[] datas = origin.Split(',');
            foreach (var d in datas)
            {
                if (isDouble)
                {
                    double tmpd = 0;
                    double.TryParse(d, out tmpd);
                    double resd = tmpd * len1 * 140/ len2 / soundSpeed;
                    res += string.Format("{0:0.0},", resd);
                }
                else
                {
                    int tmpi = 0;
                    int.TryParse(d, out tmpi);
                    int resi = (int)((double)tmpi * len1 * 140 / len2 / soundSpeed);
                    res += string.Format("{0},", resi);
                }

            }
            if (res.EndsWith(",")) res = res.Substring(0, res.Length - 1);

            return res;
        }

        public string getUSTNOTE(int index,string pinyin, int length, string beforepinyin,string nextpinyin)
        {
            if (index > 9999) return "";

            int toneIndex = 1;
            int nextIndex = 0;
            int beforeIndex = 0;
            int.TryParse(pinyin[pinyin.Length - 1].ToString(), out toneIndex);
            if(nextpinyin.Length>0)int.TryParse(nextpinyin[nextpinyin.Length - 1].ToString(), out nextIndex);
            if (beforepinyin.Length > 0) int.TryParse(beforepinyin[beforepinyin.Length - 1].ToString(), out beforeIndex);
            string realpinyin = Regex.Replace(pinyin, @"\d", "");
            string pbw = "";
            string pby = "";

            switch (toneIndex)
            {
                case 1:
                    pbw = scalePBValue("112,350,18", false, length);
                    pby = scalePBValue("14.0,17.0,0.0",true,length);
                    break;
                case 2:
                    pbw = scalePBValue("138,340,12",false,length);
                    pby = scalePBValue("-7,28.2,0.0", true, length);
                    if (beforeIndex == 2)
                    {
                        //2->[2]
                        pby = scalePBValue("5.0,28.2,0.0", true, length);
                    }
                    break;
                case 3:
                    pbw = scalePBValue("171,297,12",false,length);
                    pby = scalePBValue("-30.2,-18.5,0.0", true, length);
                    break;
                case 4:
                    pbw = scalePBValue("100,363,27",false,length);
                    pby = scalePBValue("15.1,-27.7,0.0", true, length);
                    if (nextIndex == 4)
                    {
                        //[4]->4
                        pby = scalePBValue("15.1,-10.7,0.0", true, length);
                    }
                    
                    break;
                case 5:
                    pbw = scalePBValue("103,357,20",false,length);
                    pby = scalePBValue("-16.2,-18.9,0.0", true, length);
                    break;
                default:
                    break;
            }

            string note = "";

            note += string.Format("[#{0}]\r\n", index.ToString().PadLeft(4, '0'));
            note += string.Format("Length={0}\r\n", length);
            note += string.Format("Lyric={0}\r\n", realpinyin);
            note += string.Format("NoteNum={0}\r\n", soundheight);
            note += string.Format("Intensity={0}\r\n",100);
            note += string.Format("Modulation={0}\r\n", 0);
            note += string.Format("PBS={0}\r\n", "-40");
            note += string.Format("PBW={0}\r\n", pbw);
            note += string.Format("PBY={0}\r\n", pby);
            //note += string.Format("PreUtterance={0}\r\n", 0);
            //note += string.Format("Overlap={0}\r\n", 0);
            return note;
        }

        public string getUSTR(int index, int length = 120)
        {
            if (index > 9999)  return "";

            string note = "";
            note += string.Format("[#{0}]\r\n", index.ToString().PadLeft(4, '0'));
            note += string.Format("Length={0}\r\n", length);
            note += string.Format("Lyric={0}\r\n", "R");
            note += string.Format("NoteNum={0}\r\n", soundheight);
            note += string.Format("Intensity={0}\r\n", 100);

            return note;
        }

        public string getUSTFile(string str,MyDelegates.sendStringDelegate printEvent)
        {
            string res = "";

            res += getUSTHeader();

            printEvent(string.Format("开始生成字符串（开头为：{0}...）", (str.Length >= 7 ? str.Substring(0, 7) : str)));
            string filecontent = "";
            Stopwatch sw = new Stopwatch();
            sw.Start();
            string[] sentences = pyconv.cutSentencesAll(str);
            sw.Stop();
            printEvent(string.Format("拆分句子完毕：{0}ms，共{1}个短句",sw.ElapsedMilliseconds,sentences.Length));
            int num = 0;
            int index = 0;
            sw.Reset();
            sw.Start();
            int nowindex = 0;

            int scale = 2;

            foreach (var sentence in sentences)
            {
                filecontent += getUSTR(index++, 580 / scale);
                List<List<string>> pinyin = pyconv.getPinYinList(sentence);
                foreach (var p in pinyin)
                {
                    int d;
                    int length = 480;
                    for (int i = 0; i < p.Count; i++)
                    {
                        if (i < p.Count - 1)
                            //是一个词的中间字，因此间隔小
                            d = 0;
                        else
                            d = 140;

                        if (p[i].EndsWith("5"))
                        {
                            //轻声
                            length = 420;
                           //if (p.Count == 1 && index > 0) index -= 1;
                        }
                        else if (p[i].EndsWith("3"))
                        {
                            //上声，念得长
                            length = 620;
                        }
                        else if (p[i].EndsWith("4") && i == p.Count - 1)
                        {
                            //去声且在结尾，念得短
                            length = 400;
                        }
                        else if (p[i].EndsWith("2") && i < p.Count - 1)
                        {
                            //阳平且在句中，念得短
                            length = 440;
                        }
                        else
                        {
                            length = 520;
                        }
                        string beforep = "";
                        if (i >= 1)
                        {
                            beforep = p[i - 1];
                        }
                        string nextp = "";
                        if (i < p.Count - 1)
                        {
                            nextp = p[i + 1];
                        }
                        //filecontent += getUSTR(index++, 50 / scale);
                        filecontent += getUSTNOTE(index++, p[i], length / scale, beforep, nextp);

                        if (d > 0)
                        {
                            filecontent += getUSTR(index++, d / scale);
                        }
                        num++;
                    }
                }
                printEvent(string.Format("生成短句{0}/{1}", ++nowindex, sentences.Length));
            }
            
            sw.Stop();
            printEvent(string.Format("文件生成完毕：{0}ms，总长度{1}", sw.ElapsedMilliseconds, res.Length));

            res += filecontent;
            res += getUSTFooter();

            return res;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace VirtualReader
{
    public enum BankType
    {
        JP_VC,
        ZH_VC,
        ZH_VCCV
    }
    public class OTOinfo
    {
        public string file;
        public string alias;
        public double left_blank;
        public double consonant;//辅音
        public double right_blank;
        public double pre_utterance;//提前发音
        public double overlap;//叠加

        public OTOinfo(string line)
        {
            try
            {
                var tmp1 = line.Trim().Split('=');
                this.file = tmp1[0];
                var tmp2 = tmp1[1].Split(',');
                this.alias = tmp2[0];
                this.left_blank = double.Parse(tmp2[1]);
                this.consonant = double.Parse(tmp2[2]);
                this.right_blank = double.Parse(tmp2[3]);
                this.pre_utterance = double.Parse(tmp2[4]);
                this.overlap = double.Parse(tmp2[5]);
                if (string.IsNullOrWhiteSpace(this.alias))
                {
                    //alias=filename
                    this.alias = this.file.Substring(0, this.file.Length - 4);
                }
            }
            catch
            {
                throw new Exception("oto read error");
            }

        }
    }

    class UTAUNote
    {
        public string name;
        public int note_num;
        public int length=100;
        public int volume=100;//0~100
        public int modulation = 0;//0~100
        public List<double> pitch_bend = new List<double> { 0.00 }; 

    }

    class UTAUGenerater
    {
        public string bank_path;
        public BankType type;
        public string resampler;
        public string wavtool;

        public Dictionary<string, OTOinfo> oto;


        public UTAUGenerater()
        {
            this.type = BankType.ZH_VC;
            this.resampler = "resampler.exe";
            this.wavtool = "wavtool.exe";
        }

        public void init(string path,BankType type=BankType.ZH_VC)
        {
            this.bank_path = path;
            this.type = type;

            // read oto
            string[] oto_content = File.ReadAllLines(this.bank_path + "/oto.ini");
            foreach(var line in oto_content)
            {
                try
                {
                    var oto_item = new OTOinfo(line);
                    this.oto[oto_item.alias] = oto_item;
                }
                catch
                {

                }
            }

        }

        public void resample()
        {

        }
        
    }
}

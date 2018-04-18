using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Speech.Synthesis;
using System.Speech;

namespace VirtualReader
{
    class MicrosoftTTS
    {
        public System.Speech.Synthesis.SpeechSynthesizer synth; //语音合成对象  
        public MicrosoftTTS()
        {
            synth = new System.Speech.Synthesis.SpeechSynthesizer();
        }
        public MicrosoftTTS(int m, int n)
        {
            //使用 synth 设置朗读音量 [范围 0 ~ 100]  
            synth.Volume = m;
            //使用 synth 设置朗读频率 [范围 -10 ~ 10]  
            synth.Rate = n;
        }
        public void SpeakChina(string ggg)
        {
            //SpVoice Voice = new SpVoice();  
            //synth.SelectVoice("Microsoft Lili");
            //Voice.Speak(ggg, SpFlags);  
            synth.SpeakAsync(ggg);
            //String speechPeople = synth.Voice;  
            //使用 synth 设置朗读音量 [范围 0 ~ 100]  
            // synth.Volume = 80;  
            //使用 synth 设置朗读频率 [范围 -10 ~ 10]  
            //      synth.Rate = 0;  
            //使用synth 合成 wav 音频文件:  
            //synth.SetOutputToWaveFile(string path);  
        }
        public void SpeakEnglish(string ggg)
        {
            //SpVoice Voice = new SpVoice();  
            synth.SelectVoice("VW Julie");
            synth.Speak(ggg); //ggg为要合成的内容  
        }
        public int m
        {
            get
            {
                return synth.Volume;
            }
            set
            {
                synth.Volume = value;
            }
        }
        public int n
        {
            get
            {
                return synth.Rate;
            }
            set
            {
                synth.Rate = value;
            }
        }
    }
}

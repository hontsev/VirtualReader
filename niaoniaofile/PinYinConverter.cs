using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.International.Converters.PinYinConverter;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using JiebaNet.Segmenter;

namespace niaoniaofile
{
    /// <summary>
    /// 汉字拼音转换类
    /// </summary>
    public class PinYinConverter
    {
        private PinyinDictionary dict;
        private List<string> wordList;

        public PinYinConverter()
        {
            dict = new PinyinDictionary();
            wordList = dict.Dictionary.Keys.ToList<string>();
        }

        /// <summary>
        /// 返回单个简体中文字的拼音列表
        /// </summary>
        /// <param name="inputChar">简体中文单字</param>      
        public static ReadOnlyCollection<string> GetPinYinWithTone(Char inputChar)
        {
            ChineseChar chineseChar = new ChineseChar(inputChar);
            return chineseChar.Pinyins;
        }

        /// <summary>
        /// 返回单个简体中文字的拼音个数
        /// </summary>
        /// <param name="inputChar">简体中文单字</param>      
        public static short GetPinYinCount(Char inputChar)
        {
            ChineseChar chineseChar = new ChineseChar(inputChar);
            return chineseChar.PinyinCount;
        }

        /// <summary>
        /// 返回单个简体中文字拼音列表中的第一个拼音
        /// </summary>
        /// <param name="inputChar">简体中文单字</param>      
        public static string GetFirstPinYinCount(Char inputChar)
        {
            //得到第一个拼音
            return GetPinYinWithTone(inputChar)[0];
        }


        /// <summary>
        /// 去除字符串里的空白内容
        /// </summary>
        /// <param name="ori"></param>
        /// <returns></returns>
        private static string removeBlanks(string ori)
        {
            string[] blanks = { "\t", " ", "　", "\r" };
            string res = ori;
            foreach (var b in blanks) res = res.Replace(b, string.Empty);
            return res;
        }


        /// <summary>
        /// 把字符串切成句子序列
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string[] cutSentences(string str)
        {
            List<string> result = new List<string>();
            string[] paragraphs = str.Split('\n');
            foreach (var parag in paragraphs)
            {
                List<string> thisparag = new List<string>();
                Regex regex = new Regex("[^。？！；…]+[。？！；…”’』」]*(?=[^。？！；…”’』」]*)");
                var res = regex.Matches(removeBlanks(parag));
                foreach (var ress in res)
                {
                    if (ress.ToString().Length >= 2)
                        thisparag.Add(ress.ToString());
                }
                if (thisparag.Count == 0) continue;
                foreach (var a in thisparag)
                {
                    result.Add(a);
                }
            }
            return result.ToArray();
        }

        /// <summary>
        /// 生成数字串对应的汉语读法
        /// </summary>
        /// <param name="numberString"></param>
        /// <param name="isCount">是否用带进制的记数法来读</param>
        /// <returns></returns>
        private string number2Han(string numberString,bool isCount=true )
        {
            if (string.IsNullOrWhiteSpace(numberString) || !Regex.IsMatch(numberString, @"\d*")) return "";

            Dictionary<int, char> hannum = new Dictionary<int, char>();
            hannum[0] = '零';
            hannum[1] = '一';
            hannum[2] = '二';
            hannum[3] = '三';
            hannum[4] = '四';
            hannum[5] = '五';
            hannum[6] = '六';
            hannum[7] = '七';
            hannum[8] = '八';
            hannum[9] = '九';

            string res = "";
            if (!isCount)
            {
                //用数字串读法来读
                foreach (var n in numberString)
                {
                    res += hannum[int.Parse(n.ToString())];
                }
            }
            else
            {
                //用个十百千的进制来读
                for (int i = 0; i < numberString.Length; i++)
                {
                    int nownum = int.Parse(numberString[numberString.Length - i - 1].ToString());

                    if (nownum != 0)
                    {
                        if (i == 1) res = "十" + res;
                        else if (i == 2) res = "百" + res;
                        else if (i == 3) res = "千" + res;
                        else if (i == 4) res = "万" + res;
                        else if (i == 5) res = "十" + res;
                        else if (i == 6) res = "百" + res;
                        else if (i == 7) res = "千" + res;
                    }
                    

                    if(!(nownum == 0 && res.StartsWith("零")))
                    {
                        res = hannum[nownum] + res;
                    }
                    if (nownum == 0 && i == 4) res = "万" + res;
                }

                if (res.Length >= 2)
                {
                    res=res.Replace("二百", "两百").Replace("二千", "两千");
                    if (res.StartsWith("二万")) res = "两" + res.Substring(1);
                    res=res.Replace("零万", "万");

                    if (res[0] == '一' && (res[1] == '十'))
                    {
                        res = res.Substring(1);
                    }
                    if (res.EndsWith("零")) res = res.Substring(0, res.Length - 1);
                }
            }

            
            return res;
        }

        private string translateNumbers(string str)
        {
            string res="";
            string numstr = "";

            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] >= '0' && str[i] <= '9')
                {
                    numstr += str[i];
                }
                else
                {
                    if (str[i] == '年' || numstr.Length >= 8) res += number2Han(numstr, false);
                    else res += number2Han(numstr);
                    res += str[i];
                    numstr="";
                }
            }
            if (!string.IsNullOrWhiteSpace(numstr))
            {
                if (numstr.Length >= 8) res += number2Han(numstr, false);
                else res += number2Han(numstr);
            }
            return res;
        }

        /// <summary>
        /// 细细地切分，每一个标点都切开
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public string[] cutSentencesAll(string str)
        {
            List<string> result = new List<string>();

            string[] tmp = str
                .Replace('：', '，')
                .Replace('。', '，')
                .Replace('\n', '，')
                .Replace('；', '，')
                .Replace('、', '，')
                .Replace('？', '，')
                .Replace('！', '，')
                .Replace('!', '，')
                .Replace('?', '，')
                .Replace('…', '，')
                .Split('，');


            for (int i = 0; i < tmp.Length; i++)
            {
                tmp[i] = translateNumbers(tmp[i]);
                if (!string.IsNullOrWhiteSpace(Regex.Replace(tmp[i], @"[^\u4e00-\u9fa5]*", "")))
                    result.Add(tmp[i]);
            }

            return result.ToArray();
        }




        public List<string> getPinYinListByWord(string word)
        {
            List<string> res = new List<string>();

            if (word.Length <= 0)
            {
                return null;
            }
            else if (word.Length == 1)
            {
                //词典中没有这个词。只能按逐字取拼音
                foreach (var w in word)
                {
                    //先检查dictionary里的修正列表
                    if (dict.dictionary2.ContainsKey(w.ToString()))
                    {
                        res.Add(dict.dictionary2[w.ToString()]);
                    }
                    else
                    {
                        //从标准库里取第一个音作为其缺省读音
                        string py = PinYinConverter.GetFirstPinYinCount(w).ToLower();
                        res.Add(py);
                    }
                }
            }
            else if (dict.Dictionary.ContainsKey(word))
            {
                //直接取词典中的拼音
                string[] tmp = dict.Dictionary[word].ToLower().Split(' ');
                foreach (var a in tmp)
                {
                    if (!string.IsNullOrWhiteSpace(a)) res.Add(a);
                }
            }
            else
            {
                bool havesubstr = false;
                foreach (var item in dict.Dictionary)
                {
                    if (item.Key.Length <= 1) continue;
                    int index = word.IndexOf(item.Key);
                    if (index >= 0)
                    {
                        //词典中有这个词，递归然后归并拼音序列
                        string substr1 = word.Substring(0, index);
                        string substr2 = word.Substring(index + item.Key.Length);
                        List<string> subres1 = getPinYinListByWord(substr1);
                        List<string> subres2 = getPinYinListByWord(substr2);
                        if (subres1 != null)
                        {
                            foreach (var r in subres1)
                            {
                                res.Add(r);
                            }
                        }
                        foreach (var r in item.Value.Split(' '))
                        {
                            if (!string.IsNullOrWhiteSpace(r)) res.Add(r);
                        }
                        if (subres2 != null)
                        {
                            foreach (var r in subres2)
                            {
                                res.Add(r);
                            }
                        }
                        havesubstr = true;
                        break;
                    }
                }
                if (!havesubstr)
                {
                    //词典中没有这个词。只能按逐字取拼音
                    foreach (var w in word)
                    {
                        //先检查dictionary里的修正列表
                        if (dict.dictionary2.ContainsKey(w.ToString()))
                        {
                            res.Add(dict.dictionary2[w.ToString()]);
                        }
                        else
                        {
                            //从标准库里取第一个音作为其缺省读音
                            string py = PinYinConverter.GetFirstPinYinCount(w).ToLower();
                            res.Add(py);
                        }
                    }
                }
            }

            return res;
        }

        /// <summary>
        /// 根据读音字符串获取其所属音调（其实就是最后一位的字符对应的数字）
        /// </summary>
        /// <param name="pinyin"></param>
        /// <returns></returns>
        private int getToneIndex(string pinyin)
        {
            if (pinyin.EndsWith("1")) return 1;
            else if (pinyin.EndsWith("2")) return 2;
            else if (pinyin.EndsWith("3")) return 3;
            else if (pinyin.EndsWith("4")) return 4;
            else return 5;
        }



        /// <summary>
        /// 根据语流音变的特点来修正词语读音
        /// </summary>
        /// <param name="pinyin"></param>
        /// <returns></returns>
        private void reformWordPinYin(ref List<string> pinyin)
        {

            //自右向左，找出这个词里相邻两个上声，根据语流音变特点，将前一个变成阳平。 
            for (int i = pinyin.Count - 1; i > 0; i--)
            {
                if (getToneIndex(pinyin[i - 1]) == 3 && getToneIndex(pinyin[i]) == 3)
                {
                    pinyin[i - 1] = pinyin[i - 1].Substring(0, pinyin[i - 1].Length - 1) + "2";
                    break;
                }
            }

        }

        private bool isNumberWord(char ch)
        {
            char[] numberWord = { '零', '一', '二', '三', '四', '五', '六', '七', '八', '九', '十', '百', '千', '万', '壹', '贰', '叁', '肆', '伍', '陆', '柒', '捌', '玖', '拾', '佰', '仟', '亿', '兆', '年', '月', '日', '时', '分', '秒' };
            foreach (var c in numberWord)
            {
                if (ch == c) return true;
            }
            return false;
        }

        /// <summary>
        /// 根据语流音变的特点来修正全句读音
        /// </summary>
        /// <param name="pinyin"></param>
        /// <param name="sentence"></param>
        private void reformSentencePinYin(ref List<List<string>> pinyin,string sentence)
        {
            List<string> tmp = new List<string>();
            foreach (var w in pinyin)
            {
                foreach (var py in w)
                {
                    tmp.Add(py);
                }
            }
            //自左向右，找出这个句子里相邻两个上声，根据语流音变特点，将前一个变成阳平。 
            bool find = false;
            do
            {
                find = false;
                for (int i = 0; i < tmp.Count - 1; i++)
                {
                    if (getToneIndex(tmp[i + 1]) == 3 && getToneIndex(tmp[i]) == 3)
                    {
                        tmp[i] = tmp[i].Substring(0, tmp[i].Length - 1) + "2";
                        find = true;
                        break;
                    }
                }
            } while (find);

            for (int i = 0; i < tmp.Count - 1; i++)
            {
                if (tmp[i]=="yi1" && sentence[i]=='一' && !isNumberWord(sentence[i + 1]))
                {
                    //关于“一”字的语流音变处理
                    //后跟阴平、阳平、上声时读去声
                    if(i<tmp.Count-1 && getToneIndex(tmp[i+1])<=3)
                        tmp[i] = tmp[i].Substring(0, tmp[i].Length - 1) + "4";
                    //后跟去声时读阳平
                    else if(i<tmp.Count-1 && getToneIndex(tmp[i+1])==4)
                        tmp[i] = tmp[i].Substring(0, tmp[i].Length - 1) + "2";
                    //其他情形，即作为句尾或后接轻声时，保持阴平不变
                }
                else if (tmp[i].StartsWith("bu") && sentence[i] == '不')
                {
                    //关于“不”字的语流音变处理
                    //后跟去声时读阳平
                    if (i < tmp.Count - 1 && getToneIndex(tmp[i + 1]) == 4)
                        tmp[i] = tmp[i].Substring(0, tmp[i].Length - 1) + "2";
                    //其他情形读去声
                    else
                        tmp[i] = tmp[i].Substring(0, tmp[i].Length - 1) + "4";
                }
            }

            int k = 0;
            foreach (var word in pinyin)
            {
                for (int i = 0; i < word.Count; i++)
                {
                    word[i] = tmp[k + i];
                }
                k += word.Count;
            }

        }

        /// <summary>
        /// 获取汉语句子的拼音序列
        /// </summary>
        /// <param name="sentence"></param>
        /// <returns></returns>
        public List<List<string>> getPinYinList(string sentence)
        {
            JiebaNet.Segmenter.JiebaSegmenter seg = new JiebaSegmenter();
            var words1 = seg.Cut(sentence);
            List<string> words = new List<string>();
            bool isjoin = false;
            foreach (var w in words1)
            {
                if (words.Count == 0)
                {
                    //第一个
                    if (w.Length == 1)
                    {
                        words.Add(w);
                        isjoin = true;
                    }
                    else
                    {
                        words.Add(w);
                        isjoin = false;
                    }
                }
                else if (w.Length >= 2)
                {
                    words.Add(w);
                    isjoin = false;
                }
                else 
                {
                    if (w.Length == 1)
                    {
                        if (isjoin) 
                            words[words.Count - 1] += w;
                        else if (words[words.Count - 1].Length <= 1)
                        {
                            isjoin = true;
                            words[words.Count - 1] += w;
                        }
                        else
                        {
                            words.Add(w);
                            isjoin = true;
                        }
                    }
                }
            }

            //转为拼音
            List<List<string>> pinyin = new List<List<string>>();
            foreach (string word in words)
            {
                try
                {
                    List<string> tmpw = getPinYinListByWord(word);
                    reformWordPinYin(ref tmpw);
                    if (tmpw.Count > 0) pinyin.Add(tmpw);
                }
                catch
                {

                }

            }
            reformSentencePinYin(ref pinyin, sentence);
            return pinyin;
        }

    }
}
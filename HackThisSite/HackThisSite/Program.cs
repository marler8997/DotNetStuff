using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace HackThisSite
{
    class Program
    {
        static void PrintUsage()
        {
            Console.WriteLine("Usage: Unscramble.exe <word-list-file>");
        }

        static void Main(string[] args)
        {

            if (args.Length == 0)
            {
                PrintUsage();
                return;
            }

            //
            // 1. Get Word List Dictionary
            //
            Dictionary<UInt32, List<String>> dictionary;
            using (StreamReader reader = new StreamReader(new FileStream(args[0], FileMode.Open)))
            {
                dictionary = MakeWordDictionary(reader);
            }

            foreach (KeyValuePair<UInt32, List<String>> pair in dictionary)
            {
                Console.WriteLine("[{0}] Count={1} First = {2}", pair.Key, pair.Value.Count, pair.Value[0]); 
            }

            //

            String[] words = GetWords();


        }

        static String[] GetWords()
        {
            WebRequest request = HttpWebRequest.Create("http://www.hackthissite.org/missions/prog/1");

            StreamReader reader = new StreamReader(request.GetResponse().GetResponseStream());
            
            
            String content = reader.ReadToEnd();
            Console.WriteLine(content);
            Int32 i = content.IndexOf("List of scrambled words:");
            content = content.Substring(i, 1000);

            Console.WriteLine(content);





            return null;
        }


        String Unscramble(String words)
        {
            return null;
        }




        static Dictionary<UInt32, List<String>> MakeWordDictionary(StreamReader reader)
        {
            Dictionary<UInt32, List<String>> dictionary = new Dictionary<UInt32, List<String>>();
            String line;
            while ((line = reader.ReadLine()) != null)
            {
                UInt32 sum = line.CharacterSum();

                List<String> list;
                if (dictionary.TryGetValue(sum, out list))
                {
                    list.Add(line);
                }
                else
                {
                    list = new List<String>();
                    list.Add(line);
                    dictionary[sum] = list;
                }
            }
            return dictionary;
        }
    }

    static class StringExt
    {
        public static UInt32 CharacterSum(this String str)
        {
            UInt32 sum = 0;
            for (int i = 0; i < str.Length; i++)
            {
                sum += (UInt32)str[i];
            }
            return sum;
        }
    }
}

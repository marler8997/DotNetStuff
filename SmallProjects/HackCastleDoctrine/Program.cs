using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

using More;

namespace HackCastleDoctrine
{
    class Program
    {

        const String serverHostname = "server.thecastledoctrine.net";

        const String emptyHouse  = "vwZstju07hqFu2vc4b7gFagvEZg12v4w2hL1JJM/3kKSanU1nezMLa8UKASof/YFngcaRbqEos3w5dubeW74VbRn3ZD58lcpSv+vjfpXpC+hH7srdQaTlPU/SWadsMK7VFp+GN1h6Pk3fHaj03j9D5vj/yKonWW35FPbVysGUU6PjLZKsmUiNSn9CsKCB7WHHbB3x5ps81AsL3cTnyc+RB/xPFiS2l/nua9uBVbANvM/nWgwlovP2+eezxx0XHp6N3VQWoBc+94L6vHXwjvZoD5fwvVRagfOI3AnXBp8EnFSvyyB1i0QH/Kbyfn1hleH4jA24i3RBNknjmu865a8Cn/MbScKVtsqVD88uILI1lXCACLPDKFvc0b9J5lNAL/UrsB9FjK7sPtU7d38xbdOqpvhD1cDJl2XMn8VxOUDF4+DjQaEwRsQ4N0inrjbx3ULv2DuX804L1IyfU4bzgeK5PlCuM34jKJVEzcC/9hiN/9b0hGwvgPFgzXRyd30ZqofZSScWa48lKUA6WLnWG+rbiSb3qoMQGWk8IqjpOlwfj4LOyQ80Fg7GGjcy46zqrXM/SNMmvxUl2+vnFo2b0bigvk0BxTM/ADrBUplDjH0Mlq1BlGykMdCe6e3QE7ka+kB9ECych8ErFI1uD3zJ2foIWZjoKPfEHJ3OsFy1u5FKG2AyxoF3oauDt7ChrzafD7HMoqg+gwzJhczZ0QwbnUgBhAGzaaRFuZ+3+u5zqxazq8l9c7QqmGYHe6yIRo5dazmdcv+Xf4ynxbq/MSsjv+XCSImG/0gx/qQwjk8ZFgWlwsq5WBpOLR1PV9wrB8+l/t+nme+BgH0LyVcjC0X+PH6QJqPdFtgPvYMXVJkRjrSzmajyKhgRjri+s5ndgNJFdheyTOIOglUfXAp5DSwNnN/axjJNM4l4xhyRzyYn4McndgUzJyoP9XmlOWDUlSJR8v8HE33Fqp5UobmF1kYT9GncwDJlH4TG8CC6Bvb8A6mmY0f9VxSodKQ3t7/4pMIu9gZgbCVTchXPp/A0IaPox8IUWfBnTOEx/vQTukiixJrdNAW5uGB+JAcpzIMxUYxCjg1XfH9kNwvmKu6smVx7rBSFqH+7JylQsuwVN9jK+YpacQ3P3LAYFrFTlU2vrQL5g9HhP0ELw/LbUW9WWdXPl1SanidM//AjQZjoUPN+xNZB6CenhSxR7aBb7yjI9uttNG3gychS5C2+grMy0MIyH31olqNo52LItAhacpPeOoOre/jz7WToR28nALqwL/4M5DBSxrwsxCUAoQqo3wBRYwjMVT/8cV6FwKNAsoRzheGhMBf2pPTWyNVrvGjfdxpvxbhdjn/SpBtR/CI04D+TIJph7CZo/O9OeUJkX35vqIYWRCjCsjNWEqAeTJS0uLMIRNvpGYVkcRDOJ3AKZHnImm8chqUl7EoS0bNGtIpOAbyREYul8Xym2AE+fyjm4zivRE6DdlVuo/lYRBWB8S99tR5Tk0tYjErnbRXT1hkIqtZdNA5Mgcrq34pmtBceMJ2XVPe72p1Ez05ShCykfOg1VtBqO617L+A4gsmSb6ja9X/mkgQSo335RyymzBo0eVlnrnSDYQjAsSa7sb40VYcTM3yJIKZvKyoYZTOdybXkGaNnceXUMJ0ENleRQw4FK7zYZrrhZHwW69UMKExQqrqP119cy0Ess1PdIN33dUrCw/FYjWfoDrWQSBfgIsOMKA0lwDhG2fzdRMyJS5wmQe/Pr+zf1UyaFHVK4vMWz2zpsnzkl/2Skpxwt+c0qDH8hDXv9Ia9Idn5GGBUL09cf8xnbvir7KokVpphBp9s1s39LS3EZjn8cOuXnCgUrovSteLhXgnrVo3lc1o2uemwTagwmN1hk7LAi0onbEI6/k8s0JivphBHGinsAU4xdhrwc3DbrTXhnB3yU0ceaQLXkQDSWwaknc4bmMDf7hamnZah+aNkTUrWBi1Q4FfpP5olY6BFsh4LEKsIq3bPgWDeHbyAyVkAN3dde8Byc9LbxGYCsQKdjXWhoJ8yw9Wk1DQ9BOYdVFesB/J8gMAUGsD0AkdVChFwHomu+0ncE+p2zoHSpzvB1xQKQRT/cJhsJPdUsiPqhne15f1BvU1GBCwS0PXiLw9Vkd3QtKPEtzKsvXir1QS+RHob8TWh7LlK3oSzIEBQjKEY42Bo4nGFXx5V2S6gujHd4zBcGFy7D7M/cNEq3QHs4V9L8NEVGcejlbags3GzidZSwHQ/leOe1wlyUnXYjxvPA1afBOnm0IMVajbHhLx2BdJMzZS5kqgwiRbn65DKzHVNvCIYoFpqkLXWrgSkQfjLPut5Mzi78NVO+UZ5o4BLRYM5LPkOgby4cDrq674DPzkdilPaWeD/c7cIn2DE63nz+Yu6sfNQiEQyVhveOahX27gWr1l+PT7+tCQFxPj37LCkL1X0QJcDuL1AOubeReLgV8n5t1n5nkTokC36cvs+jyuS9R3ePTbOwlf8IlBPTGhfeIurX+EMUS0jtvV/cXLw0NsUxi1UkgiwwLzy28XnFiWAwFE0pBG7CLm9fq0WIOcrOlrPAt1zBasQ09ywE5W/LJLlcRuldTmT623nREe7UX6aD6/tbLAwn2a/KoNpvH934PqiDBltOyeMINlOEBKhKzhtnAndg6zNUX9u1KIQmAsgbYnfary1ZasDz/fUXyjqsit/Vzekm5zVNx6DJQ4rufOVuDieXk6koImrn1o7D819pa+d9RsHCpE1pY5rXN1lBu9pXwp4xldu1gSR8v6xxssAGZZYgZq1edzqd8orh6AOpea9/h9IvwTo+XTnA1wqjr+4rSbOUbSF3hS38oEK4nwT+lhr2vKrchn2rI1gxKnrjsDOxsEG/W9OrrG37Oye3DlXNOLSIx6IlOOA7yE9xaZ7hA6z0bP5ddbGKXA3zvI+hWjNIZkYMHVKbL6fhhJdyeFT2lN7zWYlFwGSGjqVl6ZmtpOJWySZwiV88GgerbX+eip5EPDEs7yCJhFpaBedhGMIvLkhDSci+0X8cKIduAJTQ83dwA/7KmW7ykd1KBM8rIegVXikes4eZ2HYCZ4";
        const String emptyHouse2 = "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";

        const String exampleHouse = "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";
        const String exampleHouse2 = "NybIsVwbsMWSxwr4tOfaNjClthiT+kUPkwnBfs0BcCkjLFEMuZvPc8W4G6hXlCraNq0fhLUJaJ5QQghGGOFNtAbCBSBFnx2XC6eYNdDujPjy4zrcKVoXDD8lYeZRFFbeiY9iHFy0kZq6JSGcy1Ip4Hd9eCPQRRXOBpxJwJkKyDUZS3dcDpR5+Zp1KhATQci7sgYkV20EbFrKA6dweqmEYJdRxMxDzKxZAjB65HHJVmlzwN6B70JDL6ndqguicj+zt/XSrxs55H/iKHt4XEKaWVOQSvlfD+a9oMgohFJmyWu489qQ6MPnMdf6hvkIYMVHt7dLFwX4QxOq4ucIpE6dKk89RQwAzHU3vkZ526ZYWT1hnFk1xLLYcFJGNKirVG5hJWQJ/VSiYdwuRZa82wuIoj4g2R5gquwWhIdsyUHbE4J9idIt8LbC59/tpy1Ml/PuX7ib/X98ZV8w+EpDtdKF23cAYiiM2Dh2GFG66kRvrEWCXZ0SR6j3V95Yq6+ZmmHcs8Emvwl2JYEN+XkBmmcqKkUZc0/LtH7YttkCj5IE70vN/ujM+qO30YXM95VaXh+ICeCjEm/zJJQF3LtJD3gGknwL7wPttmCwOa+OSk4ky/SbhqHoear8ICyDPym7usN1YQu+HJ4oZpUYW2buKMGJXdi4lCsCr7xBTtwmKVriR80kV0t5+T6+pYHV5Q3yo4hxu74DFSFWrjchEdHys8uwaQyq1TiqbPhu8l0Fcn7NK77niVsd2pWFPuv3JlfVwgKpslj0VYVxfh4TAGN1VWY8IDe44Dzr9ZVtjiBaPVHKr07LskR3w5vJiSrcLftK5Vod5C+lV3118JZJQsEReCdduKi+RxrFI64VsUkfAcGt0ZJf2c4JM9c8dXHeq/uHr5HbNUlmqo5iFgJbBJhU3elRZ2y1rpiZ450qsVMkvDTgUwhHjbeoyAItd9hOjQNPLd1pQyXE9yE1LIdCTK7nuLLnmKHiV42V4EfVXjvkmmWRVN+EK7sd2lq88Gin13P23YfyIVGvjO1OCLMKtANAEiwKGF8Oc10zDym/UzAjNDOzHCD/bgJ81icKUKfjtBN8mVPyuQ8xH0Q61IofWz+kql9YiZT6I7M4SLvjk5pDlCbAY9vR7lJUjwAC25nqHoyL0BROOzDuaiqVYluyijcVi0wUdaV4B1DHWXepY173gW0fa7X2NM4VHmfWq48I1KBXlOVPRrsg3bEMXwGXup2MFBbhTQ8paB9tdRoUdl0Qmw9hvJoKFleRzKpIIIsxlXAvPE05JMLGcJ1C0ELQHCxYOBwUoJQOgNVaMsJf45d5Kc0TxY1htfX77kE1AJOvhqxXZpXiLVoa4r5+nvwTh7qRaJKzeJdQ4MS9vBMqE8ZltDXBEWGgqfEJjp0PfbYiDVNbP0guPjLxOxKgK4Lm/MsSe/9neES601bOE9E8XNHYCJmuJsqM7K1pRANpKSSTUv0SNTIDSMx8FWlMd+vkRWOV0hal8SnaWD88ZugWM1XZ4/HDjYNpRWbMO0JA8eJz8xc4XurLZyFOKcEA7GVJ/pA4EJNwlqeLHIWtbpkQ/dq1GbZEBZFVba56JLbfsVRMkfcnazoDJaP3FBbLD5cLu06wnh+0MenHanuX8Gtm3wWvOUdeoZZWemAA+fi8jA2lphufu/RGVEwX72D0Tr/gzhCIDRxwnf3ReNm4KwGcDl500lr+l7CeK//g4MpASOmxbbDVCYHRZTMhJv2rYsOrdpa5UgtlNLN2BIR9p5lMG0RGDYmjn25BwYgIFLK6tWwuNMNXcVX2VxuT6dvBR6MlCT98k7bHavyrtfvm8/ZjpZxtjrtJ4lcPMk4Gs43xC2V2XQ7hXz6vwA1exJuOg3QfvTDrbmzHDAAsz+BcNtR7oeCtd5fkZ01rjeJ8Z3EAaaUO3HuOJGLDM2RV+eAsYuXjlvSIdqgoJIU+KxdfX22y8EXRtLoPOGgmhan9N/7bJQA0/zb9PYZfUGysuPhFMpEOH2X1+CXsp4Mz8ItO02FvqDT9xFHKWxLcKwelplmEylM7KFmN5ebhfCk9+jxGEctIEpihReH7I8tqnYK499zYjGwHmRxcHlK2xAz8LICp+ggpOUOOtoNumNp51bBHjbmt5ZB01DwmPC1GuRXK8aCMR8X+p09xT9/QYKS7MKNiyQ5wocUoNQiMZ+cbiw0ib376CaMTc/WLMJOWf/BDYH6tmuxoEU9Fje//OndULA545r3lzXg1UP1GvajF2Gm7zcaeyfnzKxZs2G06Ih+kk4LVhIeKUdJ8sOfjNJYlNZktNG9Zr3+kBUx/rAVj1ouvrpYBdGqAdY+BfiHSPng1Zbz/3JbBIRpxvLoqUeD/hJneulJpZ9FvenSRjWQLo95iIoPJgHAMXstxOLZ3baoGfM/22iQiaV9FDY08F2ioe45Vpm1pTjVzOiHAKAoKoInfhDHw3gkKNgI1RElhh+LEZmf8kw0E9fmpQSfteq0gzfqXQT3A5qi18hnEpMmGnooxiLBOIyao8rfyG6I+r1MV0Mi8jCGdYl2j9+hRpaFEZAGw4s1H6QmJwl4pNnG3DJT+prqIDHURFz/AMH1B3iDMBnQLtbxZP7P5rcO/EaEjCB10uELiTEyBL4CkgXs0XMybYUCiXJ7Uu5uo2P/aBY79dCIj3eQSn1RQHhlZ9Kl7eUajxh+w6rPPpaWYPnHdRvefbgBQAXnU2iboL4S0Kn6yaujBXAFOiR4+fJuS7BKvK24KiXSXaZyurdkIZNXzQ8x+r/UpKCnCrGjCeTKaX9SCzuGnH0qoEya1yHAiRo68xh7U9B3suoTSfKxZ/OA+1ID9GtasH8/2u189W4vQucMsTfakxPxrU/uTVshvSyJo1ikRJkgB1SvmEnU598O7EWT9UrrEdvDUgu/W1RTnLqtdbEqK/okNuaUUD8LVKZcjh7G5kLNkhhtzbpJ/w5s3CzDCl9tzoap4qDedjvjRvB7k78yihgq8T4XHMIGNEaLm1drReDEazoipV4JEaDbTgMoaeZz6ON7L88Zryd61jldRrFwmvz6C2T0XrOEe1Yy6EQu/VEysszoV/VjMlltKNbP044Im1PYRtZ45o5F8+u58e6sFIajUvQuA8ceqxMKlG3of6MEbnwR2F+qTMRRrsYnrqp9qZbE+7FkiB1VyY42pbDsE7fa+tZ6ZS0pKbMAJ2phEb3PIZVHgCzMYKb8=";
        const String exampleHouse3 = "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";


        const String firstList = "998#998#998#998#998#998#998#998#998#998#998#998#998#998#998#998#998#998#998#998#998#998#998#998#998#998#998#998#998#998#998#998#998#0#0#0#0#1#1#1#1#1#1#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#998#998#0#0#0#0#1#1#1#1#1#1#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#998#998#0#0#0#0#1#1#1#1#1#1#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#998#998#0#0#0#0#1#1#1#1#1#1#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#998#998#0#0#0#0#1#1#1#1#1#1#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#998#998#0#0#0#0#1#51#51#51#51#103#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#998#998#0#0#0#0#51#51#51#51#51#51#1#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#998#998#0#0#0#0#110#110#110#110#110#110#21#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#998#998#70#1#1#0#1#1#1#1#1#1#1#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#998#998#0#1#1#0#1040#1023#1010#1#1#1#1#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#998#998#0#1#1#1#1#1#1#1#1#1#1#1#1#1#1#1#1#1#1#1#1#0#0#0#0#0#0#0#0#0#998#998#0#1#1#1#1#1#1#1#1#1#1#1#1#1#1#1#1#1#1#1#1#0#0#0#0#0#0#0#0#0#998#998#0#1#1#1#1#1#1#1#1#1#1#1#1#1#1#1#1#1#1#1#1#0#0#0#0#0#0#0#0#0#998#998#0#1#1#1#1#1#1#1#1#1#1#1#1#1#1#1#1#1#1#1#1#0#0#0#0#0#0#0#0#0#998#998#0#0#0#110#110#110#0#110#110#110#0#110#110#110#21#999#1#1#1#1#1#0#0#0#0#0#0#0#0#0#998#0#0#1#1#51#51#51#1#51#51#51#1#51#51#51#1#1#1#1#1#1#1#0#0#0#0#0#0#0#0#0#998#998#20#1#1#51#51#51#1#51#51#51#1#51#51#51#1#1#1#1#1#1#1#0#0#0#0#0#0#0#0#0#998#998#0#1#1#51#51#51#1#51#51#51#1#51#51#51#1#1#1#1#1#1#1#0#0#0#0#0#0#0#0#0#998#998#0#51#1#51#51#1#0#102#102#0#0#108#1#1#1#1#1#1#1#1#1#0#0#0#0#0#0#0#0#0#998#998#20#51#1#51#1#1#30#51#1#1#1#51#1#1#1#1#1#1#1#1#0#0#0#0#0#0#0#0#0#0#998#998#71#110#0#108#0#0#0#108#0#102#1#51#1#1#1#1#0#0#0#0#0#0#0#0#0#0#0#0#0#0#998#998#110#0#0#102#102#102#103#102#102#102#51#51#1#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#998#998#1#1#1#1#1#1#1#1#1#1#1#1#1#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#998#998#1#1#1#1#1#1#1#1#1#1#1#1#1#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#998#998#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#998#998#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#998#998#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#998#998#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#998#998#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#998#998#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#0#998#998#998#998#998#998#998#998#998#998#998#998#998#998#998#998#998#998#998#998#998#998#998#998#998#998#998#998#998#998#998#998#998";
//#
//#
//#
//152:
        
        const String secondList = "1@2#2@40#3@92#0@0#21@8#20@6#111@240#103@80#102@2#120@2#121@2#51@6#101@20#108@20#100@20#107@20#109@4#113@4#106@4#104@8#105@8#110@20#30@240#112@720#70@320#71@40#72@8#130@320#131@20#500@400#509@2400#501@800#502@1200#503@150#512@200#505@200#506@400#507@1200#508@100#510@100#511@1800";
        const String hash = "b716a7a1045dddf219ddd2113ad407e9cf813743";



        public class HouseObjectComparer : IComparer<HouseObjectDefinition>
        {
            public int Compare(HouseObjectDefinition x, HouseObjectDefinition y)
            {
                return Int32IncreasingComparer.Instance.Compare(x.id, y.id);
            }
        }


        static Int32 Main(string[] args)
        {

            CDLoader.Load(@"C:\Users\Jonathan Marler\Desktop\CastleDoctrine_v31");
            List<HouseObjectDefinition> houseObjectDefinitions = CDLoader.HouseObjectDefinitions;
            Dictionary<UInt16, HouseObjectDefinition> map = CDLoader.HouseObjectDefinitionMap;



            Byte[] house = Convert.FromBase64String(emptyHouse);
            //Byte[] house = Convert.FromBase64String(exampleHouse);
            //Byte[] house = Convert.FromBase64String(exampleHouse3);
            Byte[] house2 = Convert.FromBase64String(exampleHouse2);

            Console.WriteLine("house1.Length = {0}", house.Length);
            Console.WriteLine("house2.Length = {0}", house2.Length);

            for (int i = 0; i < house.Length; i++)
            {
                Byte b = house[i];
                Console.Write("{0,4} {1,4} 0x{1:X}", i, b, (char)b);

                if (i % 20 == 0)
                {
                    int length = house.Length - i;
                    if(length > 20) length = 20;
                    Console.Write(" \"{0}\"", Encoding.ASCII.GetString(house, i, length));
                }

                HouseObjectDefinition o;
                if(map.TryGetValue(b, out o))
                {
                    Console.Write(" {0}", o.pathName);
                }


                Console.WriteLine();
            }

            /*
            String[] firstListStrings = firstList.Split('#');
            String[] secondListStrings = secondList.Split('#');

            Console.WriteLine("firstList.Length = {0}", firstListStrings.Length);
            Console.WriteLine("secondList.Length = {0}", secondListStrings.Length);


            // Print in reverse order (map is from bottom to top)
            for (int i = 31; true; i--)
            {
                Console.WriteLine();
                int rowOffset = i * 32;
                for (int j = 0; j < 32; j++)
                {
                    Console.Write(" {0,4}", firstListStrings[rowOffset + j]);
                }
                if (i == 0) break;
            }
            */


            HouseObjectDefinition[] defs = houseObjectDefinitions.ToArray();
            Array.Sort(defs, new HouseObjectComparer());


            Console.WriteLine("House Object Definitions:");

            for (int i = 0; i < defs.Length; i++)
            {
                HouseObjectDefinition o = defs[i];
                Console.WriteLine("{0} {1}", o.id, o.pathName);

            }






            return 0;



            /*
            String command = args[0];

            if (command.Equals("gethouse", StringComparison.CurrentCultureIgnoreCase))
            {
                String source = args[1];


                if(source.StartsWith("server:"))
                {
                    WebRequest request = HttpWebRequest.Create("http://{0}/gameServer/server.php?action=start_rob_house

                }
                else
                {

                }




                GET /gameServer/server.php?action=start_rob_house&user_id=13986&to_rob_home_id=1&to_rob_character_name=Conrad_Eric_Vandyke&map_encryption_key=CDDAA591D8A09BA6079FC61C5E2C026051CDFB76&sequence_number=1226&ticket_hmac=BCBCDB4029C06679046DCD27DC14CE1B0496B00E HTTP/1.0
Host: thecastledoctrine.net









            }
            else
            {
                Console.WriteLine("Unknown Command '{0}'", command);
                return 1;
            }

            return 0;
            */
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WordChain;

namespace WordChainTests
{
    [TestClass]
    public class WordChainerTest
    {
        private string FindWordsChain(String from, String to, IEnumerable<String> dic)
        {
            WordChainer wc = new WordChainer(dic);
            var path = wc.FindChain(from, to);
            return String.Join(",", path);
        }

        private string FindWordsChain(String from, String to, String dic)
        {
            return FindWordsChain(from, to, dic.Split(','));
        }

        private String GetResourcePath(String fname)
        {
            return Path.Combine("../../Resources", fname);
        }

        private IEnumerable<String> Load3CharsWordDic()
        {
            String fname = GetResourcePath("word3.txt");
            return File.ReadLines(fname);
        }

        private IEnumerable<String> Load4CharsWordDic()
        {
            String fname = GetResourcePath("word4.txt");
            return File.ReadLines(fname);
        }

        [TestMethod]
        public void TestFromDescription()
        {
            var res = FindWordsChain("КОТ", "ТОН", "КОТ,ТОН,НОТА,КОТЫ,РОТ,РОТА,ТОТ");
            Assert.AreEqual(res, "КОТ,ТОТ,ТОН");
        }

        [TestMethod]
        public void Test2()
        {
            var res = FindWordsChain("КОТ", "УХО", "КОТ,ТОН,РОТ,ТОТ,НОС,ТОМ,ТОР,УХО,ШОК,ШОВ,ШИП,ШОП,ТОП,ШИК");
            Assert.AreEqual("", res);
        }

        [TestMethod]
        public void TestKot2Shik()
        {
            var res = FindWordsChain("КОТ", "ШИК", "КОТ,ТОН,РОТ,ТОТ,НОС,ТОМ,ТОР,УХО,ШОК,ШОВ,ШИП,ШОП,ТОП,ШИК");
            Assert.AreEqual("КОТ,ТОТ,ТОП,ШОП,ШОК,ШИК", res);
        }

        [TestMethod]
        public void Test3()
        {
            var res = FindWordsChain("КОТ", "УХО", "КОТ,УХО,КОЛ,,ОКО");
            Assert.AreEqual("", res);
        }


        [TestMethod]
        public void TestKotNotFound()
        {
            var dic = Load3CharsWordDic();
            var res = FindWordsChain("КОТ", "УХО", dic);

            Assert.AreEqual("", res);
        }

        [TestMethod]
        public void TestKot2Fish()
        {
            var dic = Load3CharsWordDic();
            var res = FindWordsChain("КОТ", "ФИШ", dic);

            Assert.AreEqual("КОТ,КАТ,КАН,ПАН,ПАК,МАК,МАГ,МИГ,ФИГ,ФИШ", res);
        }

        [TestMethod]
        public void TestMuxa2Slon()
        {
            var dic = Load4CharsWordDic();
            var res = FindWordsChain("МУХА", "СЛОН", dic);

            Assert.AreEqual("МУХА,МУРА,ТУРА,ТАРА,КАРА,КАРЕ,КАФЕ,КАФР,КАЮР,КАЮК,КАИК,КАИН,КЛИН,КЛОН,СЛОН", res);
        }

        [TestMethod]
        public void TestChainerRunner()
        {
            var result = Program.FindChain(GetResourcePath("exampleInputs.txt"), GetResourcePath("exampleDict.txt"));
        }
    }
}

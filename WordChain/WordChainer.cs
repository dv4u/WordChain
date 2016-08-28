using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace WordChain
{
    public class WordChainer
    {
        private readonly IList<string> _dict;

        private readonly int[] _indexies;

        /// <summary>
        /// currently processed item from the queue or -1 if we process fromWord
        /// </summary>
        private int _currentProcessingIndex;

        /// <summary>
        /// items with index in [_yetNotInChainIndex; _outOfPlayIndex) are not in chain yet
        /// </summary>
        private int _yetNotInChainIndex;

        /// <summary>
        /// items with index >= _outOfPlayIndex was removed from processing (different length, duplicate words etc)
        /// </summary>
        private int _outOfPlayIndex;


        public WordChainer(IEnumerable<string> dict)
        {
            _dict = new List<string>(dict);
            _indexies = new int[_dict.Count()];
        }

        /// <summary>
        /// show current dictionary with all indexies taken part in processing
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder res = new StringBuilder();
            if (_currentProcessingIndex < 0)
                res.Append("^,");
            for (int i = 0; i < _dict.Count; i++)
            {
                if(i>0)
                    res.Append(',');
                if(i==_currentProcessingIndex)
                    res.Append('^');
                if (i == _yetNotInChainIndex)
                    res.Append('&');
                if (i == _outOfPlayIndex)
                    res.Append('#');
                res.Append(_dict[i]);
            }
            if(_outOfPlayIndex >= _dict.Count)
                res.Append(",#");
            return res.ToString();
        }

        /// <summary>
        /// check if word "from" can be converted to word "to" by changing only one char. Assume that words can not be equal, otherwise Assert fails
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        private bool IsConvertable(string from, string to)
        {
            if (from.Length != to.Length)
            {
                return false;
            }

            int diffCount=0;
            for (int i = 0; i < from.Length; i++)
            {
                if (from[i] != to[i])
                {
                    diffCount++;
                    if(diffCount > 1)
                        return false;
                }
            }
            
            // equal words should be removed from processing before
            Debug.Assert(diffCount == 1);
            return true;
        }

        /// <summary>
        /// swap two items in dictionary
        /// </summary>
        /// <param name="i1"></param>
        /// <param name="i2"></param>
        private void swap(int i1, int i2)
        {
            if(i1==i2) return;

            String tmp = _dict[i1];
            _dict[i1] = _dict[i2];
            _dict[i2] = tmp;
        }

        /// <summary>
        /// remove word at index from processing, since it can not be part of chain e.g. has different length
        /// </summary>
        /// <param name="index"></param>
        private void RemoveFromProcessing(int index)
        {
            Debug.Assert(index < _outOfPlayIndex - 1);
            Debug.Assert(index >= _yetNotInChainIndex);
            String itemToSwap = _dict[index];

            _outOfPlayIndex--;
            swap(index, _outOfPlayIndex);
            ShowState(() => itemToSwap + " was removed");
        }

        private void ShowState(Func<string> func)
        {
#if DEBUG
            Debug.WriteLine(func());
            Debug.WriteLine(this.ToString());
#endif
        }

        /// <summary>
        /// add item at index to queue for later checking
        /// </summary>
        /// <param name="index"></param>
        private void AddToQueue(int index)
        {
            Debug.Assert(index < _outOfPlayIndex );
            Debug.Assert(index >= _yetNotInChainIndex);
            String itemToSwap = _dict[index];

            swap(index, _yetNotInChainIndex);
            _indexies[_yetNotInChainIndex] = _currentProcessingIndex;
            _yetNotInChainIndex++;
            ShowState(() => itemToSwap + " was added to queue");
        }

        private void InitProcessing()
        {
            _yetNotInChainIndex = 0;
            _outOfPlayIndex = _dict.Count;
            _currentProcessingIndex = -1;
        }

        /// <summary>
        /// find chain of words between fromWord and toWord
        /// </summary>
        /// <param name="fromWord"></param>
        /// <param name="toWord"></param>
        /// <returns>enumerable of found words, starting with fromWord and ending with toWord,
        /// if no such path was found, returns empty enumerable
        /// </returns>
        public IEnumerable<String> FindChain(string fromWord, string toWord)
        {
            fromWord = fromWord.ToUpper();
            toWord = toWord.ToUpper();

            InitProcessing();

            if (fromWord == toWord || IsConvertable(fromWord, toWord))
                return new List<string>(){fromWord, toWord};
            
            string currentWord = fromWord;

            bool firstRun = true;

            while (_currentProcessingIndex < _yetNotInChainIndex)
            {
                ShowState(() => "Search continuation for " + currentWord);
                for (int i = _yetNotInChainIndex; i < _outOfPlayIndex; )
                {
                    string anotherWord = _dict[i];

                    if (firstRun)
                    {
                        _dict[i] = _dict[i].ToUpper();
                        anotherWord = _dict[i];

                        if (anotherWord.Length != currentWord.Length ||
                            anotherWord == fromWord || anotherWord == toWord)
                        {
                            RemoveFromProcessing(i);
                            // don't update current word index, since now it contains unprocessed word taken from the back 
                            // in RemoveFromProcessing
                            continue;
                        }
                    }


                    if (IsConvertable(currentWord, anotherWord))
                    {
                        if (IsConvertable(anotherWord, toWord)) 
                            return RestorePath(new List<string>(){fromWord, anotherWord, toWord});
                        
                        AddToQueue(i);
                    }

                    i++;
                }
                
                _currentProcessingIndex++;

                Debug.Assert(_currentProcessingIndex <= _yetNotInChainIndex);
                if (_currentProcessingIndex == _yetNotInChainIndex)
                {
                    ShowState(() => "Queue is empty, nothing more to process");
                    return Enumerable.Empty<String>();
                }

                currentWord = _dict[_currentProcessingIndex];
                firstRun = false;

            }
            return Enumerable.Empty<String>();
        }


        /// <summary>
        /// after path was found, restore it using _indexies array
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        private IEnumerable<string> RestorePath(List<String> result)
        {
            for (int i = _currentProcessingIndex; i >= 0; i = _indexies[i])
                result.Insert(1, _dict[i]);
            return result;
        }
    }
}
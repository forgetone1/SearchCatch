using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SearchCatch
{
    public class CatchMatch
    {
        private int currentIndex { get; set; }
        private string inpuText { get; set; }

        private string key { get { return "catch"; } }
        private char leftBrackets { get { return '{'; } }
        private char rightBrackets { get { return '}'; } }

        public CatchMatch(string input)
        {
            this.currentIndex = 0;
            this.inpuText = input;
        }

        public List<string> getCatchesText()
        {
            List<string> result = new List<string>();
            var retList = getCatches();
            foreach (var item in retList)
            {
                var text = this.inpuText.Substring(item.startIndex, item.endIndex - item.startIndex);
                result.Add(text);
            }
            return result;

        }

        private List<CatchPosition> getCatches()
        {
            List<CatchPosition> list = new List<CatchPosition>();
            var cp = getPosition();

            while (cp.hasCatch)
            {
                list.Add(cp);
                cp = getPosition();
            }
            return list;
        }

        private CatchPosition getPosition()
        {
            var cp = new CatchPosition();
            int bracketsNonFinished = 0;

            var index = this.inpuText.IndexOf(key, this.currentIndex);
            if (index - 1 <= 0 || this.inpuText[index - 1] == '\\')
            {
                return cp;
            }

            cp.startIndex = index;
            int lastIndex = 0;

            if (index >= 0)
            {
                cp.hasCatch = true;

                for (int i = index; i < this.inpuText.Length; i++)
                {
                    lastIndex = index;
                    if (this.inpuText[i] == leftBrackets)
                    {
                        bracketsNonFinished++;
                    }
                    else if (this.inpuText[i] == rightBrackets)
                    {
                        bracketsNonFinished--;
                        if (bracketsNonFinished == 0)
                        {
                            cp.endIndex = i;
                            this.currentIndex = i;
                            break;
                        }

                    }
                }
            }
            if (cp.endIndex == 0)
            {
                cp.hasCatch = false;
            }
            return cp;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab3
{
    internal class ProgramInternalForm
    {
        private List<Tuple<string, PositionInHashTable>> elem;

        public ProgramInternalForm()
        {
            this.elem = new List<Tuple<string, PositionInHashTable>>();
        }

        public void addElem(string token, PositionInHashTable position)
        {
            this.elem.Add(new Tuple<string, PositionInHashTable>(token, position));
        }

        public override string ToString()
        {
            string str = "Program Internal Form:\n";

            for(int i = 0; i < this.elem.Count; i++)
            {
                str += "(" + this.elem[i].Item1 + ", " + this.elem[i].Item2.ToString() +")\n";
            }

            return str;
        }

    }
}

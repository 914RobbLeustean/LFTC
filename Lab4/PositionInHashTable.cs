using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab3
{
    public class PositionInHashTable
    {
        public PositionInHashTable(int hash, int position)
        {
            Hash = hash;
            Position = position;
        }

        public int Hash { get; set; }
        public int Position { get; set; }
        public string ToString()
        {
            return "[" + this.Hash + ", " + this.Position + "]";
        }
    }
}

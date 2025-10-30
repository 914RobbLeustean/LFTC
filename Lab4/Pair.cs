using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab3
{
    public class Pair
    {
        public Pair(string token, int position)
        {
            Token = token;
            Position = position;
        }

        public string Token { get; set; }
        public int Position { get; set; }

        public string ToString() 
        {
            return "[" + this.Token + ", " + this.Position +  "]";
        }
    }
}

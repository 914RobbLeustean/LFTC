using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab3
{
    internal class HashTable
    {
        private int _size;
        private List<List<Pair>> _symbolTable;

        public int Size { get { return _size; } }

        public HashTable(int size)
        {
            this._size = size;
            this._symbolTable = new List<List<Pair>>();
            for (int i = 0; i < this._size; i++)
            {
                this._symbolTable.Add(new List<Pair>());
            }
        }

        private int hash(string key)
        {
            int sum_chars = 0;
            char[] key_characters = key.ToCharArray();
            foreach (char c in key_characters)
            {
                sum_chars += c;
            }
            return sum_chars % this._size;
        }

        public bool existKey(string key)
        {
            int hashedKey = this.hash(key);

            if (this._symbolTable[hashedKey].Count > 0)
            {
                for (int i = 0; i < this._symbolTable[hashedKey].Count; i++)
                {
                    if (this._symbolTable[hashedKey][i].Token.Equals(key))
                        return true;
                }
            }

            return false;
        }

        public int getPosition(string key)
        {
            int hashedKey = this.hash(key);

            if (this._symbolTable[hashedKey].Count == 1)
                return this._symbolTable[hashedKey][0].Position;

            if (this._symbolTable[hashedKey].Count > 1)
            {
                for (int i = 0; i < this._symbolTable[hashedKey].Count; i++)
                {
                    if (this._symbolTable[hashedKey][i].Token.Equals(key))
                        return this._symbolTable[hashedKey][i].Position;
                }
            }

            return 0;
        }

        public PositionInHashTable getFullPosition(string key)
        {
            int hashedKey = this.hash(key);

            if (this._symbolTable[hashedKey].Count == 1)
                return new PositionInHashTable(hashedKey,this._symbolTable[hashedKey][0].Position);

            if (this._symbolTable[hashedKey].Count > 1)
            {
                for (int i = 0; i < this._symbolTable[hashedKey].Count; i++)
                {
                    if (this._symbolTable[hashedKey][i].Token.Equals(key))
                        return new PositionInHashTable(hashedKey, this._symbolTable[hashedKey][i].Position);
                }
            }

            return null;
        }

        public void addKey(string key)
        {
            int hashedKey = this.hash(key);
            this._symbolTable[hashedKey].Add(new Pair(key, this._symbolTable[hashedKey].Count + 1));
        }

        public override string ToString()
        {
            string rez = "";
            for (int i = 0; i < _size; i++)
            {
                for (int j = 0; j < this._symbolTable[i].Count; j++)
                {
                    rez += "Token " + this._symbolTable[i][j].Token + " on position " + this._symbolTable[i][j].Position + "\n";
                }
            }
            rez += "\n";
            return rez;
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab3
{
    internal class SymbolTable_hashTable
    {
        private int _size;
        private HashTable _hashTable;

        public SymbolTable_hashTable(int size)
        {
            this._size = size;
            this._hashTable = new HashTable(this._size);
        }

        public PositionInHashTable getFullPosition(string key)
        {
            return this._hashTable.getFullPosition(key);
        }

        public int getPosition(string id)
        {
            return _hashTable.getPosition(id);
        }

        public int getPosition(int id)
        {
            return _hashTable.getPosition(id.ToString());
        }

        public void addId(string id)
        {
            if(!this._hashTable.existKey(id))
                this._hashTable.addKey(id);
        }

        public void addId(int id)
        {
            if (!this._hashTable.existKey(id.ToString()))
                this._hashTable.addKey(id.ToString());
        }

        public bool existKey(string id)
        {
            return _hashTable.existKey(id);
        }

        public bool existKey(int id)
        {
            return _hashTable.existKey(id.ToString());
        }


        public override string ToString()
        {
            return _hashTable.ToString();
        }

        public int getSize() { return _size; }

    }
}

# Documentation: Lexical Analyzer for MiniFilter

This document details the implementation of the lexical analyzer for the MiniFilter Domain-Specific Language.

## 1. Use of Tools (Python `re` Module)

For this implementation, we used Python's built-in `re` (regular expression) module. This approach was chosen over tools like `flex` because it is self-contained, requires no separate compilation step, and is highly effective for smaller languages.

The core of the analyzer is built on two key features of the `re` module:

1.  **Named Groups (`?P<NAME>...`)**: Each token is defined as a regex pattern with a unique name (e.g., `(?P<ID>[a-zA-Z_]...))`). This allows us to know *which* pattern matched.
2.  **`re.compile()` and `re.finditer()`**: All individual token patterns are joined together with the `|` (OR) operator into a single, highly efficient "master regex." We compile this regex once. Then, `re.finditer()` walks through the entire source code, finding the *next* matching token at each step.

The analyzer iterates over these matches. For each match, it checks the `match.lastgroup` attribute to get the token's name (e.g., "ID", "SELECT", "NUMBER").

## 2. Lexical Token Detection

The order of token definitions is critical. We must check for specific **keywords** (like `SELECT`) *before* checking for general **identifiers** (`ID`). Otherwise, `SELECT` would be incorrectly matched as an `ID`.

### Identifiers
* **Regex**: `(?P<ID>[a-zA-Z_][a-zA-Z0-9_]*)`
* **Detection**: This pattern matches any word that starts with a letter or underscore, followed by any number of letters, numbers, or underscores.
* **Action**: When an `ID` is detected (e.g., `order_id`), it is passed to the **Symbol Table (ST)**. The ST adds the symbol (if new) and returns its unique integer ID. This ID is then stored in the **Program Internal Form (PIF)**.

    * `PIF` Entry: `('ID', 5)` (where 5 is the ST's ID for `order_id`)

### Constants
Constants are literal values present in the code. In our implementation, they are also stored in the Symbol Table to keep the PIF uniform (it only stores token codes and ST IDs).

* **Number**:
    * **Regex**: `(?P<NUMBER>\d+(\.\d+)?)`
    * **Detection**: This matches one or more digits, optionally followed by a decimal point and more digits. It correctly identifies integers (`1500`) and floats (`1500.00`).
* **String**:
    * **Regex**: `(?P<STRING>"[^"]*")`
    * **Detection**: This matches any sequence of characters (except a double-quote) that is enclosed in double-quotes (e.g., `"USA"`).

* **Action**: When a constant is found, it's added to the ST (e.g., `"USA"` or `1500.00`). The ST returns its unique ID, which is placed in the PIF.
    * `PIF` Entry: `('CONST', 8)` (where 8 is the ST's ID for `"USA"`)

### Keywords, Operators, and Delimiters
* **Regex**: e.g., `(?P<SELECT>SELECT)`, `(?P<OP_EQ>==)`, `(?P<SEMICOLON>;)`
* **Detection**: These are simple, fixed-string matches.
* **Action**: These tokens do not represent user-defined symbols, so they do not need to be in the Symbol Table. The PIF stores the token type directly. A placeholder ID (like -1) is used to indicate "Not Applicable."
    * `PIF` Entry: `('SELECT', -1)`

## 3. ST Management in C

As required, here are the C data structures and operations for managing a Symbol Table. A **Hash Table** is the standard, most efficient choice for a compiler's ST. A **Binary Search Tree (BST)** is a simpler, but typically slower, alternative.

### Option 1: Hash Table (Preferred)

A hash table provides $O(1)$ average-case time complexity for lookups, insertions, and deletions. It uses a hash function to map a symbol (string) to an index in an array. Collisions (when two symbols hash to the same index) are handled using a linked list at that index.

```c
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#define ST_SIZE 100 // Size of the hash table array

// Node for the linked list at each table index
typedef struct ST_Node {
    char* symbol;
    int id;
    struct ST_Node* next;
} ST_Node;

// The Hash Table structure
typedef struct HashTable {
    ST_Node* table[ST_SIZE];
    int current_id;
} HashTable;

// Simple hash function (djb2 algorithm)
unsigned long hash(char* str) {
    unsigned long hash = 5381;
    int c;
    while ((c = *str++))
        hash = ((hash << 5) + hash) + c; // hash * 33 + c
    return hash % ST_SIZE;
}

HashTable* st_create() {
    // Allocate memory for the table and initialize all pointers to NULL
    HashTable* st = (HashTable*)malloc(sizeof(HashTable));
    if (st == NULL) return NULL;
    
    st->current_id = 0;
    for (int i = 0; i < ST_SIZE; i++) {
        st->table[i] = NULL;
    }
    return st;
}

// Add a symbol to the ST. Returns the symbol's unique ID.
int st_add(HashTable* st, char* symbol) {
    unsigned long index = hash(symbol);
    
    // 1. Search for the symbol in the linked list at this index
    ST_Node* current = st->table[index];
    while (current != NULL) {
        if (strcmp(current->symbol, symbol) == 0) {
            // Found it! Return the existing ID.
            return current->id;
        }
        current = current->next;
    }
    
    // 2. Not found. Create a new node.
    ST_Node* newNode = (ST_Node*)malloc(sizeof(ST_Node));
    newNode->symbol = strdup(symbol); // Duplicate the string
    newNode->id = st->current_id++;
    
    // 3. Add the new node to the front of the list
    newNode->next = st->table[index];
    st->table[index] = newNode;
    
    return newNode->id;
}

// Example usage:
// int main() {
//     HashTable* st = st_create();
//     int id1 = st_add(st, "order_id");   // id1 = 0
//     int id2 = st_add(st, "total_amount"); // id2 = 1
//     int id3 = st_add(st, "order_id");   // id3 = 0 (returns existing)
//     printf("ID for order_id: %d\n", id3);
//     return 0;
// }
import re
import sys

class SymbolTable:
    """
    A simple Symbol Table implementation.
    For this Python example, it's built on a dictionary (a hash table).
    It stores symbols (identifiers and constants) and ensures each
    unique symbol has a unique integer ID.
    """
    def __init__(self):
        # The 'table' maps the symbol (string) to its unique ID (int)
        self.table = {}
        # The 'current_id' ensures each new symbol gets a fresh ID
        self.current_id = 0

    def add(self, symbol):
        """
        Adds a symbol to the table if it's not already present.
        Returns the unique ID for the symbol.
        """
        if symbol not in self.table:
            self.table[symbol] = self.current_id
            self.current_id += 1
        return self.table[symbol]

    def get_id(self, symbol):
        """Retrieves the ID for a symbol. Returns -1 if not found."""
        return self.table.get(symbol, -1)

    def __str__(self):
        """Provides a clean string representation for printing."""
        if not self.table:
            return "<Empty Symbol Table>"
        
        header = "--- Symbol Table ---\n"
        header += "ID  | Symbol\n"
        header += "----|-----------------\n"
        
        # Sort by ID for consistent output
        sorted_items = sorted(self.table.items(), key=lambda item: item[1])
        
        lines = [f"{uid:<3} | {symbol}" for symbol, uid in sorted_items]
        return header + "\n".join(lines)


class LexicalAnalyzer:
    """
    Performs lexical analysis on a MiniFilter source program.
    """
    def __init__(self):
        # 1. Define all tokens as regex patterns
        # The 'named groups' (?P<TOKEN_NAME>...) are crucial.
        # Order matters: Keywords must come before the general IDENTIFIER.
        self.token_specs = [
            # Keywords
            ('SELECT',   r'(?P<SELECT>SELECT)'),
            ('FROM',     r'(?P<FROM>FROM)'),
            ('WHERE',    r'(?P<WHERE>WHERE)'),
            ('AND',      r'(?P<AND>AND)'),
            ('OR',       r'(?P<OR>OR)'),
            ('TRUE',     r'(?P<TRUE>true)'),
            ('FALSE',    r'(?P<FALSE>false)'),
            
            # Literals / Constants
            ('NUMBER',   r'(?P<NUMBER>\d+(\.\d+)?)'),
            ('STRING',   r'(?P<STRING>"[^"]*")'),
            
            # Identifiers
            ('ID',       r'(?P<ID>[a-zA-Z_][a-zA-Z0-9_]*)'),
            
            # Operators and Delimiters
            ('OP_EQ',    r'(?P<OP_EQ>==)'),
            ('OP_NEQ',   r'(?P<OP_NEQ>!=)'),
            ('OP_GTE',   r'(?P<OP_GTE>>=)'),
            ('OP_LTE',   r'(?P<OP_LTE><=)'),  
            ('OP_GT',    r'(?P<OP_GT>>)'),  
            ('OP_MUL',   r'(?P<OP_MUL>\*)'),
            ('COMMA',    r'(?P<COMMA>,)'),
            ('LPAREN',   r'(?P<LPAREN>\()'),
            ('RPAREN',   r'(?P<RPAREN>\))'),
            ('SEMICOLON',r'(?P<SEMICOLON>;)'),
            
            # Misc
            ('NEWLINE',  r'(?P<NEWLINE>\n)'),
            ('WHITESPACE',r'(?P<WHITESPACE>[ \t]+)'),
            ('COMMENT',  r'(?P<COMMENT>#[^\n]*)'),
            
            # Error
            ('MISMATCH', r'(?P<MISMATCH>.)'),
        ]
        
        # 2. Combine all patterns into one master regex
        self.master_regex = re.compile(
            '|'.join([spec[1] for spec in self.token_specs])
        )
        
        # 3. Initialize ST, PIF, and errors
        self.symbol_table = SymbolTable()
        self.pif = []
        self.errors = []

    def analyze(self, source_code):
        """
        Analyzes the source code and populates the PIF, ST, and error list.
        """
        line_num = 1
        col_num = 1
        
        # 4. Iterate over all matches in the source code
        for mo in self.master_regex.finditer(source_code):
            token_type = mo.lastgroup  # Get the name of the matched group
            token_value = mo.group()
            
            # 5. Handle special tokens (ignore or count)
            if token_type == 'NEWLINE':
                line_num += 1
                col_num = 1
                continue
            elif token_type in ('WHITESPACE', 'COMMENT'):
                col_num += len(token_value)
                continue
            
            # 6. Handle lexical errors
            elif token_type == 'MISMATCH':
                error_msg = f"Lexical Error: Unexpected character '{token_value}' at line {line_num}, col {col_num}"
                self.errors.append(error_msg)
                col_num += len(token_value)
                continue

            # 7. Handle valid tokens
            
            # For Identifiers and Constants, add to ST and put ID in PIF
            if token_type == 'ID':
                st_id = self.symbol_table.add(token_value)
                self.pif.append(('ID', st_id))
                
            elif token_type in ('STRING', 'NUMBER'):
                # We also store constants in the ST for a uniform PIF
                st_id = self.symbol_table.add(token_value)
                self.pif.append(('CONST', st_id))
            
            # For Keywords and Operators, just put the token type in PIF
            # We use -1 to signify "N/A" for the ST ID
            else:
                self.pif.append((token_type, -1))
            
            col_num += len(token_value)

        return self.pif, self.symbol_table, self.errors

    def print_results(self):
        """Helper function to pretty-print all outputs."""
        print("="*30)
        print("--- Program Internal Form (PIF) ---")
        print("Token Type    | ST ID")
        print("--------------|-------")
        for token, uid in self.pif:
            print(f"{token:<13} | {uid}")
        
        print("\n" + "="*30)
        print(self.symbol_table)
        
        print("\n" + "="*30)
        print("--- Lexical Errors ---")
        if not self.errors:
            print("<No errors found>")
        else:
            for err in self.errors:
                print(err)
        print("="*30)

# --- Main Execution ---

# Input Program (from Lab 2, Program 003)
# I've added an invalid character '$' to demonstrate error detection.
program_to_analyze = """
# Finds high-value, non-test orders from North
# America in Q4 2024 that are either flagged
# for review or have been returned.

SELECT
    order_id,
    customer_email,
    total_amount,
    order_status
FROM
    all_orders
WHERE
    (shipping_country == "USA" OR shipping_country == "Canada")
    AND
    (is_flagged_for_review == true OR order_status == "RETURNED")
    AND
    total_amount >= 1500.00
    AND
    is_test_order != false
    AND
    order_timestamp $ >= 1727740800;
"""

if __name__ == "__main__":
    lexer = LexicalAnalyzer()
    lexer.analyze(program_to_analyze)
    lexer.print_results()
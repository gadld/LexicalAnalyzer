/*
  Buttercup compiler - This class performs the lexical analysis, 
  (a.k.a. scanning).
  Copyright (C) 2013 Ariel Ortiz, ITESM CEM
  
  This program is free software: you can redistribute it and/or modify
  it under the terms of the GNU General Public License as published by
  the Free Software Foundation, either version 3 of the License, or
  (at your option) any later version.
  
  This program is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU General Public License for more details.
  
  You should have received a copy of the GNU General Public License
  along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Buttercup {

    class Scanner {

        readonly string input;


        static readonly Regex regex = new Regex(
            @"                             
                (?<Assign>     [=]       )
              | (?<Comment>    [/][*][^*]*[*]+([^/*][^*]*[*]+)*[/]       )
              | (?<Comment>    [/][/].*       )

              | (?<Compare>    [==]       )
              | (?<Compare>    [!=]       )
              | (?<Great>      [>]       )
              | (?<GreatEq>    [>=]       )
              | (?<Less>       [<]       )
              | (?<LessEq>     [<=]       )
              | (?<BitOr2>     [|]       )
              | (?<BitOr>     [\^]       )
              | (?<BitAnd>     [&]       )
              | (?<Less>     [<<]       )
              | (?<Greater>     [>>]       )
              | (?<Greater2>     [>>>]       )
              | (?<Power>      [**]       ) 
              | (?<Mul>        [*]       )
              | (?<Neg>        [-]       )
              | (?<Plus>       [+]       ) 
              | (?<Division>   [/]       ) 
              | (?<Modulus>    [%]       ) 
              | (?<Modulus>    [!]       ) 
              | (?<Modulus>    [~]       ) 
              | (?<Base2>      0[b|B](0|1)+       )
              | (?<Base8>      0[o|O][0-7]+       )
              | (?<Base16>     0[x|X]([0-9]|[a-fA-F])+       )
              | (?<False>      [#]f      )
              | (?<IntLiteral> \d+       )
              | (?<Less>       [<]       )
              | (?<Newline>    \n        )
              | (?<ParLeft>    [(]       )
              | (?<ParRight>   [)]       )
              | (?<BracketLeft>   [{]       )
              | (?<ParRight>   [}]       )      
              | (?<Colon>       [:]       )      
              | (?<SemiColon>       [;]       )      
              | (?<Comma>      [,]       ) 
              | (?<String>    [""](\\.|[^""])*[""]      )
              | (?<String>    ['](\\.|[^'])*[']      )
              | (?<Identifier> [a-zA-Z]+ )
              | (?<WhiteSpace> \s        )     # Must go anywhere after Newline.
              | (?<Other>      .         )     # Must be last: match any other character.
            ", 
            RegexOptions.IgnorePatternWhitespace 
                | RegexOptions.Compiled
                | RegexOptions.Multiline
            );

        static readonly IDictionary<string, TokenCategory> keywords =
            new Dictionary<string, TokenCategory>() {
                {"break", TokenCategory.BOOL},
                {"else", TokenCategory.END},
                {"return", TokenCategory.IF},
                {"case", TokenCategory.INT},
                {"false", TokenCategory.PRINT},
                {"switch", TokenCategory.THEN},
                {"continue", TokenCategory.THEN},
                {"for", TokenCategory.THEN},
                {"true", TokenCategory.THEN},
                {"default", TokenCategory.THEN},
                {"if", TokenCategory.THEN},
                {"do", TokenCategory.THEN},
                {"in", TokenCategory.THEN},
                {"var", TokenCategory.THEN},

            };

        static readonly IDictionary<string, TokenCategory> nonKeywords =
            new Dictionary<string, TokenCategory>() {
                {"And", TokenCategory.AND},
                {"Assign", TokenCategory.ASSIGN},
                {"False", TokenCategory.FALSE},
                {"IntLiteral", TokenCategory.INT_LITERAL},
                {"Less", TokenCategory.LESS},
                {"Mul", TokenCategory.MUL},
                {"Neg", TokenCategory.NEG},
                {"ParLeft", TokenCategory.PARENTHESIS_OPEN},
                {"ParRight", TokenCategory.PARENTHESIS_CLOSE},
                {"BracketLeft", TokenCategory.BRACKET_OPEN},
                {"BracketRight", TokenCategory.BRACKET_CLOSE},
                {"Colon", TokenCategory.COLON},
                {"SemiColon", TokenCategory.SEMI_COLON},
                {"Plus", TokenCategory.PLUS},
                {"True", TokenCategory.TRUE}                
            };

        public Scanner(string input) {
            this.input = input;
        }

        public IEnumerable<Token> Start() {

            var row = 1;
            var columnStart = 0;

            Func<Match, TokenCategory, Token> newTok = (m, tc) =>
                new Token(m.Value, tc, row, m.Index - columnStart + 1);

            foreach (Match m in regex.Matches(input)) {

                if (m.Groups["Newline"].Success) {

                    // Found a new line.
                    row++;
                    columnStart = m.Index + m.Length;

                } else if (m.Groups["WhiteSpace"].Success 
                    || m.Groups["Comment"].Success) {
                    // Skip white space and comments.

                } else if (m.Groups["Identifier"].Success) {

                    if (keywords.ContainsKey(m.Value)) {

                        // Matched string is a Buttercup keyword.
                        yield return newTok(m, keywords[m.Value]);                                               

                    } else { 

                        // Otherwise it's just a plain identifier.
                        yield return newTok(m, TokenCategory.IDENTIFIER);
                    }
                } else if (m.Groups["String"].Success) {

                   yield return newTok(m, TokenCategory.String);

                } else if (m.Groups["Other"].Success) {

                    // Found an illegal character.
                    yield return newTok(m, TokenCategory.ILLEGAL_CHAR);

                } else {

                    // Match must be one of the non keywords.
                    foreach (var name in nonKeywords.Keys) {
                        if (m.Groups[name].Success) {
                            yield return newTok(m, nonKeywords[name]);
                            break;
                        }
                    }
                }
            }

            yield return new Token(null, 
                                   TokenCategory.EOF, 
                                   row, 
                                   input.Length - columnStart + 1);
        }
    }
}

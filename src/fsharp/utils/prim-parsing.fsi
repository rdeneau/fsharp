// Copyright (c) Microsoft Corporation.  All Rights Reserved.  See License.txt in the project root for license information.

// NOTE: the code in this file is a drop-in replacement runtime for Parsing.fsi from the FsLexYacc repository

namespace Internal.Utilities.Text.Parsing

open Internal.Utilities.Text.Lexing

[<Sealed>]
type internal IParseState = 

    /// Get the start and end position for the terminal or non-terminal at a given index matched by the production.
    member InputRange: index:int -> Position * Position

    /// Get the end position for the terminal or non-terminal at a given index matched by the production.
    member InputEndPosition: index:int -> Position 

    /// Get the start position for the terminal or non-terminal at a given index matched by the production.
    member InputStartPosition: index:int -> Position 

    /// Get the start of the range of positions matched by the production.
    member ResultStartPosition: Position

    /// Get the end of the range of positions matched by the production.
    member ResultEndPosition: Position 

    /// Get the full range of positions matched by the production.
    member ResultRange: Position * Position

    /// Get the value produced by the terminal or non-terminal at the given position.
    member GetInput   : index:int -> obj 

    /// Raise an error in this parse context.
    member RaiseError<'b> : unit -> 'b 

    /// Return the LexBuffer for this parser instance.
    member LexBuffer : LexBuffer<char>


/// The context provided when a parse error occurs.
[<Sealed>]
type internal ParseErrorContext<'tok> =
      /// The stack of state indexes active at the parse error .
      member StateStack  : int list

      /// The state active at the parse error.
      member ParseState : IParseState

      /// The tokens that would cause a reduction at the parse error.
      member ReduceTokens: int list

      /// The stack of productions that would be reduced at the parse error.
      member ReducibleProductions : int list list

      /// The token that caused the parse error.
      member CurrentToken : 'tok option

      /// The token that would cause a shift at the parse error.
      member ShiftTokens : int list

      /// The message associated with the parse error.
      member Message : string

/// Tables generated by fsyacc
/// The type of the tables contained in a file produced by the <c>fsyacc.exe</c> parser generator.
type internal Tables<'tok> = 
    { 
      /// The reduction table.
      reductions: (IParseState -> obj)[] 

      /// The token number indicating the end of input.
      endOfInputTag: int

      /// A function to compute the tag of a token.
      tagOfToken: 'tok -> int

      /// A function to compute the data carried by a token.
      dataOfToken: 'tok -> obj 

      /// The sparse action table elements.
      actionTableElements: uint16[]

      /// The sparse action table row offsets.
      actionTableRowOffsets: uint16[]

      /// The number of symbols for each reduction.
      reductionSymbolCounts: uint16[]

      /// The immediate action table.
      immediateActions: uint16[]      

      /// The sparse goto table.
      gotos: uint16[]

      /// The sparse goto table row offsets.
      sparseGotoTableRowOffsets: uint16[]

      /// The sparse table for the productions active for each state.
      stateToProdIdxsTableElements: uint16[]  

      /// The sparse table offsets for the productions active for each state.
      stateToProdIdxsTableRowOffsets: uint16[]  

      /// This table is logically part of the Goto table.
      productionToNonTerminalTable: uint16[]

      /// This function is used to hold the user specified "parse_error" or "parse_error_rich" functions.
      parseError:  ParseErrorContext<'tok> -> unit

      /// The total number of terminals.
      numTerminals: int

      /// The tag of the error terminal.
      tagOfErrorTerminal: int }

    /// Interpret the parser table taking input from the given lexer, using the given lex buffer, and the given start state.
    /// Returns an object indicating the final synthesized value for the parse.
    member Interpret :  lexer:(LexBuffer<char> -> 'tok) * lexbuf:LexBuffer<char> * initialState:int -> obj 

/// Indicates an accept action has occurred.
exception internal Accept of obj

/// Indicates a parse error has occurred and parse recovery is in progress.
exception internal RecoverableParseError

#if DEBUG
module internal Flags =
  val mutable debug : bool
#endif

/// Helpers used by generated parsers.
module internal ParseHelpers = 

   /// The default implementation of the parse_error_rich function.
   val parse_error_rich: (ParseErrorContext<'tok> -> unit) option

   /// The default implementation of the parse_error function.
   val parse_error: string -> unit


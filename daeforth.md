Syntax
======

Comments
--------

	(( This is a comment ))

Values
------

Common:

	true false (( bool ))
	() (( unit ))

In .NET mode:

	5 (( int value 5 ))
	5.0 (( float value 5 ))
	null (( null object ))

In GLSL mode:

	5 (( float value 5 ))
	#5 (( int value 5 ))

Word
----

	: name ... ; (( Word accepting no defined parameters ))
	: name (| arg arg2 |) ... ; (( Word accepting two arguments, arg and arg2 ))
	: name (| arg:float arg2:int |) ... ; (( Word accepting a float and int argument ))
	: name (| -> float |) ... ; (( Word accepting no arguments, returning a float ))
	: name (| arg:float arg2:int -> float |) ... ; (( Word accepting a float and int argument, returning a float ))
	: name (| -> float float |) ... ; (( Word accepting no arguments, returning two floats ))
	: name (| :float :float -> float |) ... ; (( Word accepting two floats (unnamed) and returning a float ))

Macro
-----

	:m name ... ; (( Macro accepting no defined parameters ))
	:m name 5 _ + ; (( Macro accepting one parameter '_' ))
	:m name (| arg |) 5 arg + ; (( Macro accepting one parameter, arg ))
	:m name (| $arg |) 5 arg + ; (( Macro accepting a parameter, arg, stored in a variable before use ))

Locals and macro locals
-----------------------

	5 =name (( Store 5 in variable 'name' ))
	5 =>name (( Store 5 in macro local 'name' ))
	5 =$name (( Store 5 in macro local 'name', while ensuring it's backed by a variable ))

Arrays
------

	[ 0 1 2 3 ]
	[ [ 0 1 2 ] [ 3 4 5 ] [ 6 7 8 ] ]
	0...3 (( [ 0 1 2 ] ))
	[ 0..3 ]  (( [ 0 1 2 3 ] ))
	[ 0...3 10...13 ] (( [ 0 1 2 10 11 12 ] ))
	[ 0...100,10 ] (( [ 0 10 20 30 40 50 60 70 80 90 ] ))
	[ 0..100,10 ]  (( [ 0 10 20 30 40 50 60 70 80 90 100 ] ))

Array assignments
-----------------

	[ 0 1 2 ] =[ foo bar baz ]  (( Assign foo=0, bar=1, baz=2 ))
	[ 0 1 2 ] =>[ foo bar baz ] (( Macro local assignment ))
	[ 0 1 2 ] =$[ foo bar baz ] (( Variable backed macro local assignment ))
	[ [ 0 1 ] [ 2 3 4 ] [ 5 6 ] ] =[ [ foo bar ] [ baz hax omg ] blah ] (( foo=0, bar=1, baz=2, hax=3, omg=4, blah=[ 5 6 ] ))

Map, reduce
-----------

	[ 0 1 2 ] \+      (( 0 1 + 2 + ))
	[ 0 1 2 ] /sin    (( [ 0 sin 1 sin 2 sin ] ))
	[ 0 1 2 ] /sin \+ (( 0 sin 1 sin + 2 sin + ))

Swizzle
-------

These operators only work in GLSL mode.  (XXX: And for built-in vector classes maybe ... ?)

	[ 0 1 2 ] .x (( 0 ))
	[ 0 1 2 ] .xy (( [ 0 1 ] ))
	[ 0 1 2 ] .xy.z (( [ 0 1 ] 2 ))
	[ 0 1 2 ] .xyyy (( [ 0 1 1 1 ] ))

Indexing
--------

	[ 5 6 7 ] 2 [] (( 7 ))
	[ 5 6 7 ] =temp 18 temp 2 =[] (( [ 5 6 18 ] =temp ))

Blocks
------

	foo { 5 + } call (( foo 5 + ))
	{ 5 + } =>temp foo *temp (( foo 5 + ))
	[ 0 1 ] /{ 5 + } \+ (( 0 5 + 1 5 + + ))
	[ 0 1 2 ] /{ (| arg |) 50 arg * } (( [ 50 0 *  50 1 *  50 2 * ] ))
	{ (| $arg |) 50 arg * } (( Same thing, ensuring variable storage first ))

Any word/macro can be turned into a block with `&name`.

Conditional Compilation
-----------------------

	5 true ~{ 5 + } (( 5 5 + ))
	5 false ~{ 5 + } (( 5 ))

Variable literal
----------------

	:m gl_FragCoord $gl_FragCoord offset + ;
	gl_FragCoord 10 * (( gl_FragCoord offset + 10 * ))

String
------

	"this is a\nstring"
	"this is a
	string"

Duplication
-----------

	5 !+ (( 5 5 +  or  5 dup + ))
	: square !* ;

Typed variable declaration
--------------------------

	@float =some-float-var
	@float global =some-global-float
	@float varying =some-varying-float
	@float uniform =some-uniform-float
	@float attribute =some-attribute-float

Using
-----

	import Some.Namespace

Namespace
---------

	namespace My.Namespace

Applies to all subsequent class definitions

Classes/structs
---------------

	:class my-class (| base-class i-some-interface |)
		@float =a-float-field ( Becomes AFloatField )

		{ 5 } =an-int-property ( Read-only property, AnIntProperty )

		@string private =backing
		:property a-string-property (| string |)
			:get this .backing ;
			:set =backing ;
		;

		: my-word (| int -> int |) 5 + ; ( Maps to MyWord in .NET-land )
	;

	:struct [ custom-nullable T ( struct ) ]
		@T private =backing
		false private =has-value?

		: custom-nullable
			=backing (( Implicit argument for the constructor! ))
			true =has-value?
		;

		{
			&backing
			{ () @invalid-operation-exception new throw }
			has-value? if
		} =value
	;

All members and types are public by default

Instantiation
-------------

	() @[ list int ] new =list
	( 5 ) list .add (( list contains [ 5 ] ))
	[ 1 2 3 ] /{ ( _ ) list .add } (( list contains [ 5 1 2 3 ] ))

	[ "tex.png" "tex2.jpg" ] /{ ( $"assets/textures/{_}" false ) @texture new }

Static method calls
-------------------

	( ", " 1..5 /.to-string ) @string .join (( "1, 2, 3, 4, 5" ))

Attributes
----------

Attributes apply to the very next class/struct/field/property/method declaration in the compilation unit.

	:attribute an-attribute-class "first parameter" 5 "third parameter" ;

	:attribute attribute-usage @attribute-targets .all false :allow-multiple ;
	:class custom-attribute (| attribute |)
		@string readonly =name
		: custom-attribute =name ;
	;

The latter is equivalent to:

	[AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
	public class CustomAttribute : Attribute {
		public readonly string Name;
		public CustomAttribute(string name) {
			Name = name;
		}
	}

Block to callable
-----------------

	{ 5 } @[ func int ] cast
	{ 5 + } @[ func int int ] cast 10 swap call (( 15 ))
	{ @console .write-line } @[ action string ] cast =test
	[ "foo" "bar" ] /*test

Words
=====

- `flatten` -- Extract elements from array to the stack.  `[ 0 1 2 ] flatten + + (( 0 1 2 + + ))`
- `flatten-deep` -- Same as flatten, but recursively flattens any subarrays.
- `dup` -- Duplicate element at the top of the stack. `5 dup + (( 5 5 + ))`
- `swap` -- Swap top two elements of the stack. `5 10 swap / (( 10 5 / ))`
- `drop` -- Pops the top element of the stack. `5 10 drop (( 5 ))`
- `store` -- Ensure the top element of the stack is backed by a variable.
- `take (| n |)` -- Move `n`th item on the stack to the top.  `0 1 2 3 4  3 take (( 1 ))`
- `call (| block |)` -- Invokes `block`.  `10 { 5 + } call (( 10 5 + ))`
- `compile (| array |)` -- Compiles an array of tokens to a block.
- `int (| value |)`, `$value float`, `$value bool` -- Cast to respective type.
- `times (| block times |)` -- Calls `block` `times` times.  Turns into a `for` loop.  Block is passed an argument indicating the loop index.
- `mtimes (| block times |)` -- Identical to `times`, but unrolled at compiletime.
- `when (| block cond |)`, `unless (| block cond |)` -- Calls `block` if `cond` evaluates to true for `when` or false for `unless`.
	- If the condition can be evaluated at compiletime, only the relevant branch will be taken.
- `if (| then else cond |)` -- Calls `then` if `cond` evaluates to true.  Otherwise calls `else`.
	- If the condition can be evaluated at compiletime, only the relevant branch will be taken.
	- For runtime conditions, if one or more values are returned, they must have equal type and length on both sides of the branch.
- `select (| a b cond |)` -- If `cond` evaluates to true, push `a` otherwise `b`.
- `match (| patterns |)` -- Described below.
- `size (| arr |)` -- Returns the length of `arr`.
- `upto (| top |)` -- Returns an array containing 0-`top`, exclusive.
- `enumerate (| arr |)` -- Returns an enumerated array of `arr`.  `[ 4 5 6 ] enumerate (( [ [ 0 4 ] [ 1 5 ] [ 2 6 ] ] ))`
- `return (| value |)` -- Returns from word with given value.  If `value` is unit ( `()` ), no value is returned.
- `global (| type |)` -- Decorates a type assigned to a variable to indicate globalness.  `@vec3 global =some-position`
- `defmacro (| block name |)` -- Declares a new macro based on a block.  ``{ 5 + } `add-five defmacro``
- `defword (| block name |)` -- Declares a new word based on a block.  ``{ 5 + } `add-five-word defword``
- `swizzle (| arr indices |)` -- Swizzles an array using an array of indices.  `[ 4 5 6 7 ] [ 1 2 2 2 2 0 ] swizzle (( [ 5 6 6 6 6 4 ] ))`
- `car (| arr |)` and `cdr (| arr |)` -- Returns the first and rest of an array, respectively.
- `join (| arr-1 arr-2 |)` -- Concatenate two arrays and return the result.
- `+`, `-`, `*`, `/`, `%`, `**` -- Binary math operations
- `&`, `|`, `^` -- Bitwise binary operations.
- `or`, `and` -- Boolean and/or
- `neg` -- Unary negation
- `not` -- Unary boolean or bitwise negation (depending on input type)

`match`
-------

Match allows extensive pattern matching.  Basic form:

	:m do-match
		25 =>temp
		[
			[ 5 10 ] { 25 } (( match either 5 or 10 ))
			[ 1..3 ] { 10 } (( match 1 2 3 ))
			[ [ &a 5 ] ] { a 2 * } (( match any array [ x 5 ] ))
			[ [ () temp ] ] { 1000 } (( match any array [ x 25 ] ))
			[ [ &a &b &c ] ] { a b + c + } (( match any array [ x y z ] ))
			[ [ () () () () ...rest ] ] { rest } (( match any array of 4 or more elements, binding remaining elements in rest ))
			[ 1234 ] { 0 } (( match 1234 ))
			1234 { 0 } (( match 1234 ))
			[ &a ] { a } (( match any value ))
			&a { a } (( same match ))
			() { 0 } (( match any value, ignoring it ))
		] _ match ;

	5 do-match (( 25 ))
	10 do-match (( 25 ))

	1 do-match (( 10 ))

	[ 1 2 3 ] do-match (( 1 2 + 3 + ))
	[ 1 2 ] do-match (( 1000 ))
	[ 10 5 ] do-match (( 10 2 * ))

	1234 do-match (( 0 ))
	123 do-match (( 123 ))

Notes:

- Variable bindings are stored *only* in macro locals, scoped to the match block.
- To pattern-match an array, you *must* wrap it in another array for disambiguation purposes.

Matches that happen entirely at compile-time may take any types and return anything (or nothing).  Matches that happen at runtime can match anything and return anything, but types that don't apply in a given instance will not end up in final code.  All matches that apply at runtime must have the same return(s).

### Match constraints

If the final element of the match list is a block, it will be used as a constraint.  It gets the current value as a parameter (which you are not required to consume) and will get any variable bindings from the match.

	[
		[ () { 5 % 0 == } ] { 5 } (( Matches any element that is evenly divisible by 5 ))
		[ [ &a &b ] { a b > } ] { 1 } (( Matches any 2-array where the first element is greater than the second ))
	] _ do-match

### List operations

	:m car
		[
			[ [ () ...rest ] ] { rest }
		] _ match ;

	:m cdr
		[
			[ [ &first ... ] ] { first }
			() { () }
		] _ match ;

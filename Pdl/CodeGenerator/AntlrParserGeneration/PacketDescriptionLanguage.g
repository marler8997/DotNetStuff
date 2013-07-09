grammar PacketDescriptionLanguage;

options
{
	language=CSharp3;
	output=AST;
}
tokens
{
	ENUM_DEFINITION;
	ENUM_DECLARATION;
	DATA_BLOCK_DEFINITION;
	SIMPLE_TYPE_FIELD;
	ARRAY_TYPE_FIELD;
}

@parser::namespace {Marler.Pdl}
@lexer::namespace {Marler.Pdl}



OPTIONAL_KEYWORD
	:'optional';
primitiveTypeModifier
	:OPTIONAL_KEYWORD;



//
// Primitive Types
//
BIT_TYPE 
	: 'bit' ;

BYTE_TYPE  
	:'byte';
USHORT_TYPE
	:'ushort';
UINT_TYPE
	:'uint';
ULONG_TYPE
	:'ulong';
unsignedIntegerType 
	: BYTE_TYPE | USHORT_TYPE | UINT_TYPE | ULONG_TYPE;
	
SBYTE_TYPE
	:'sbyte';
SHORT_TYPE
	:'short';
INT_TYPE
	:'int';
LONG_TYPE
	:'long';
	
signedIntegerType
	: SBYTE_TYPE | SHORT_TYPE | INT_TYPE | LONG_TYPE;
	
FLOAT_TYPE
	:'float';
DOUBLE_TYPE
	:'double';
floatingPointType
	: FLOAT_TYPE | DOUBLE_TYPE;
	
	
STRING_TYPE
	:'ascii';

	

ID  :	('a'..'z'|'A'..'Z'|'_') ('a'..'z'|'A'..'Z'|'0'..'9'|'_')*
    ;
    
TYPE_ID	:('a'..'z'|'A'..'Z'|'_')('a'..'z'|'A'..'Z'|'0'..'9'|'_'|'.')*
	;
    
type
: BIT_TYPE | unsignedIntegerType | signedIntegerType | STRING_TYPE | floatingPointType | TYPE_ID;

    
/*
    
INT :	'0'..'9'+
    ;

FLOAT
    :   ('0'..'9')+ '.' ('0'..'9')* EXPONENT?
    |   '.' ('0'..'9')+ EXPONENT?
    |   ('0'..'9')+ EXPONENT
    ;
*/


INTEGER
	:'-'? '0'..'9'+;


COMMENT
    :   '//' ~('\n'|'\r')* '\r'? '\n' {$channel=HIDDEN;}
    |   '/*' ( options {greedy=false;} : . )* '*/' {$channel=HIDDEN;}
    ;

fragment
EXPONENT : ('e'|'E') ('+'|'-')? ('0'..'9')+ ;




enumDefinitionOptionalDeclaration
	: 'enum' unsignedIntegerType? ID '{' enumValues? ','? '}' enumDeclaration? -> ^(ENUM_DEFINITION ID unsignedIntegerType? enumDeclaration? enumValues?)
	;
enumDeclaration
	: ID ';' -> ^(ENUM_DECLARATION ID)
	;

enumDefinition
	: 'enum' unsignedIntegerType? ID '{' enumValues? ','? '}' -> ^(ENUM_DEFINITION ID unsignedIntegerType? enumValues?)
	;
enumValues
	: enumValue anotherEnumValue*;
anotherEnumValue
	: ',' enumValue -> enumValue;
enumValue
	: ID -> ^(ID)
	| ID '=' INTEGER -> ^(ID INTEGER)
	;
	

arrayLength 
	: INTEGER 
	| unsignedIntegerType
	;

public packetDescriptionLanguage 
	: globalDefinition* EOF
	;

globalDefinition
	: enumDefinition 
	| dataBlockDefinition;
	
dataBlockDefinition
	: ID '{' dataBlockField* '}' -> ^(DATA_BLOCK_DEFINITION ID dataBlockField*)
	;


dataBlockField
	: enumDefinitionOptionalDeclaration
	| primitiveTypeModifier* type ID ';' -> ^(SIMPLE_TYPE_FIELD type ID primitiveTypeModifier*)
	| type '[' arrayLength ']' ID ';' -> ^(ARRAY_TYPE_FIELD type ID arrayLength)
	| '[' arrayLength ']' dataBlockDefinition -> ^(DATA_BLOCK_DEFINITION arrayLength dataBlockDefinition)
	;

// fsharplint:disable MemberNames InterfaceNames
namespace rec Fable.AST.Lua


type Const =
    | ConstNumber of float
    | ConstString of string
    | ConstBool of bool
    | ConstNull

type LuaIdentity =
    { Namespace: string option
      Name: string
    }

type UnaryOp =
    | Not
type BinaryOp =
    | Equals
    | Multiply
    | Divide
    | Plus
    | Minus
    | BinaryTodo of string

type GetKind =
    | FieldGet of fieldName: string

type Expr =
    | Ident of LuaIdentity
    | Const of Const
    | Unary of UnaryOp * Expr
    | Binary of BinaryOp * Expr * Expr
    | Get of Expr * kind: GetKind
    | FunctionCall of f: Expr * args: Expr list
    | AnonymousFunc of args: string list * body: Statement list
    | Unknown of string
    | Let of name: string * Expr
    | Macro of string * args: Expr list
    | IfThenElse of guardExpr: Expr * thenExpr: Expr * elseExpr: Expr
    | NoOp

type Statement =
    | Assignment of name: string * Expr
    | FunctionDeclaration of name: string * args: string list * body: Statement list
    | Return of Expr
    | Do of Expr

type File =
    { Filename: string
      Statements: (Statement) list
      ASTDebug: string }
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftOne.Soe.Common.Util
{
    #region ITokeniser

    public interface ITokeniser
    {
        List<RpnToken> Tokenise(string expression);
        RpnOperator CreateOperator(RpnToken token);
    }

    #endregion

    #region MathParser

    public class MathParser : ITokeniser
    {
        // There must be a one to one correspondence between allOps and precedence
        private string[] allOps = {
            ",", "(", ")", "!", ":", "?", "||", "&&", "|", "^", "&",
            "=", "==", "<", "<=", ">", ">=", "<<", ">>", "<<=", ">>=", "<>",
            "+", "-", "*", "/", "//", "%", "**",
            ":=", "+=", "-=", "*=", "/=", "//=", "%="
        };

        private int[] precedence = {
	      //,  (  )  !   :  ?  || && |  ^  & 
			0, 0, 0, 16, 2, 3, 4, 4, 5, 5, 5,
		  //=  == <  <= >  >= << >> <<= >>= <>
			6, 6, 6, 6, 6, 6, 6, 6,  6, 6,  6,
		  //+  -  *  /  \  %  **
			7, 7, 8, 8, 8, 8, 9,
		  //:= += -= *= /= //= %=
			1, 1, 1, 1, 1, 1,  1
        };

        const int assgnPrecedence = 1;
        private string[] arithmeticOps = { "+", "-", "*", "/", "//", "%", "**" };
        private string[] comparisonOps = { "=", "==", "!=", "<", "<=", ">", ">=", "<<", ">>", "<<=", ">>=", "<>" };
        private string[] logicalOps = { "&&", "||" };
        private string[] bitwiseOps = { "&", "|", "^" };
        private string[] condOps = { "?", ":" };
        private string[] assignOps = { ":=", "+=", "-=", "*=", "/=", "//=", "%=" };

        public List<RpnToken> Tokenise(string expression)
        {
            List<RpnToken> tokens = new List<RpnToken>();
            // Split the expression string into indivual tokens
            List<string> tokenList = Split(expression);

            RpnToken prevToken = null;
            bool addToken = true;
            string affixSign = "";
            bool negate = false;

            for (int stx = 0; stx < tokenList.Count; ++stx)
            {
                string strToken = tokenList[stx].Trim();
                if (strToken.Length > 0)
                {
                    // check for string literal
                    if (strToken[0] == '\'' || strToken[0] == '"')
                    {
                        if (strToken.Length > 3)
                            // replace double string delimiter with single
                            if (strToken[0] == '"')
                                strToken = strToken.Replace("\"\"", "\"");
                            else
                                strToken = strToken.Replace("''", "'");
                    }
                    if (strToken.Length == 1)
                    {
                        // check for negation operator
                        if ("!^\\".IndexOf(strToken[0]) >= 0)
                        {
                            if (stx + 1 < tokenList.Count)
                            {
                                if (tokenList[stx + 1] != "(")
                                {
                                    negate = true;
                                    continue;
                                }
                                else
                                    strToken = "!";    // convert to internal negation op code
                            }
                        }
                    }
                    else
                        // replace external not equal with internal format 
                        if (strToken == "<>")
                    {
                        negate = true;
                        strToken = "=";
                    }

                    RpnToken token = new RpnToken(affixSign + strToken);
                    affixSign = "";
                    EvalToken(token, negate);
                    negate = false;
                    if (token.IsOperator)
                    {
                        if ((strToken == "+" || strToken == "-"))
                            if (prevToken == null || (prevToken.IsOperator || prevToken.StrValue == "(" ||
                                prevToken.ElementType == ElementType.CondFalse ||
                                prevToken.ElementType == ElementType.CondTrue))
                            {
                                token.Precedence = 16;
                            }
                            else
                                if (prevToken == null || (prevToken.ElementType == ElementType.Function ||
                                  prevToken.ElementType == ElementType.Argument))
                            {
                                addToken = false;
                                affixSign = strToken;
                            }
                    }
                    if (token.StrValue == "(")
                    {
                        token.ElementType = ElementType.GroupStart;
                        if (prevToken != null)
                        {
                            if (prevToken.ElementType == ElementType.Identifier &&
                               (Char.IsLetter(prevToken.StrValue, 0) || prevToken.StrValue == "#"))
                            {
                                prevToken.ElementType = ElementType.Function;
                                addToken = false;
                            }
                        }
                    }
                    if (token.StrValue == ")")
                        token.ElementType = ElementType.GroupEnd;
                    if (addToken)
                    {
                        tokens.Add(token);
                        prevToken = token;
                    }
                    addToken = true;
                }
            }
            return tokens;
        }

        private List<String> Split(String expression)
        {
            List<String> tokenList = new List<String>();
            int expLen = expression.Length;
            int tokenStart = 0;
            bool haveOpr = false;
            bool inString = false;
            char stringDelm = ' ';
            int exp = 0;
            for (; exp < expLen; ++exp)
            {
                char ch = expression[exp];
                char nextch = ch;
                if (!inString)
                {
                    if (haveOpr)
                    {
                        tokenList.Add(expression.Substring(tokenStart, exp - tokenStart));
                        tokenStart = exp;
                    }
                }
                haveOpr = false;
                switch (ch)
                {
                    // [+\-*/%=<>]{1,2}
                    case '+':
                    case '-':
                    case '*':
                    case '/':
                    case '%':
                    case '<':
                    case '>':
                    case '=':
                        if (!inString)
                        {
                            if (exp > tokenStart)
                                tokenList.Add(expression.Substring(tokenStart, exp - tokenStart));
                            tokenStart = exp;
                            haveOpr = true;
                            if (exp + 1 < expLen)
                            {
                                nextch = expression[exp + 1];
                                if ((ch == '<' && nextch == '<') || (ch == '>' && nextch == '>'))
                                    ++exp;
                                else if (ch == '/' && nextch == ch)
                                    ++exp;
                                else if (ch == '*' && ch == nextch)
                                    ++exp;
                                if (exp + 1 < expLen)
                                    nextch = expression[exp + 1];
                                if (nextch == '=')
                                    ++exp;
                            }
                        }
                        break;

                    // [(),!^:\?\\]{1}
                    case '(':
                    case ')':
                    case ',':
                    case '!':
                    case '^':
                    case ':':
                    case '?':
                    case '\\':
                        if (!inString)
                        {
                            if (exp > tokenStart)
                                tokenList.Add(expression.Substring(tokenStart, exp - tokenStart));
                            tokenStart = exp;
                            if (ch == ':' && exp + 1 < expLen)
                            {
                                nextch = expression[exp + 1];
                                if (nextch == '=')
                                    ++exp;
                            }
                            haveOpr = true;
                        }
                        break;

                    // [&\|]{1,2}
                    case '&':
                    case '|':
                        if (!inString)
                        {
                            if (exp > tokenStart)
                                tokenList.Add(expression.Substring(tokenStart, exp - tokenStart));
                            tokenStart = exp;
                            haveOpr = true;
                            if (exp + 1 < expLen)
                            {
                                nextch = expression[exp + 1];
                                if (ch == nextch)
                                    ++exp;
                            }
                        }
                        break;

                    // string [" ']
                    case '"':
                    case '\'':
                        if (inString)
                        {
                            if (ch == stringDelm)
                            {
                                if (exp + 1 < expLen)
                                {
                                    nextch = expression[exp + 1];
                                    if (ch == nextch)
                                        ++exp;
                                    else
                                        inString = false;
                                }
                                else
                                    inString = false;
                            }
                        }
                        else
                        {
                            if (exp > tokenStart)
                                tokenList.Add(expression.Substring(tokenStart, exp - tokenStart));
                            tokenStart = exp;
                            inString = true;
                            stringDelm = ch;
                        }
                        break;
                }
            }

            if (tokenStart < expLen)
                tokenList.Add(expression.Substring(tokenStart));

            return tokenList;
        }

        private void EvalToken(RpnToken token, bool negate)
        {
            string strValue = token.StrValue;
            char ch = strValue[0];
            if (ch == '"' || ch == '\'')
            {
                // check for a hex or unicode literal string
                ch = strValue[strValue.Length - 1];
                if (ch == 'x' || ch == 'X')
                    HexString(token, strValue.Substring(1, strValue.Length - 3), ElementType.HexLiteral);
                else if (ch == 'u' || ch == 'U')
                    HexString(token, strValue.Substring(1, strValue.Length - 3), ElementType.UCLiteral);
                else
                {
                    token.ElementType = ElementType.Literal;
                    token.StrValue = strValue.Substring(1, strValue.Length - 2);
                }
            }
            else
            {
                // is this a function argument list delimitor
                if (ch == ',' && strValue.Length == 1)
                    token.ElementType = ElementType.Argument;
                else if (Char.IsDigit(ch) || ch == '.' || ch == '-' || ch == '+')
                {
                    // check for numeric or hex constant
                    if (strValue.Length > 2 && ch == '0' && (strValue[1] == 'x' || strValue[1] == 'X'))
                    {
                        HexString(token, strValue.Substring(2), ElementType.Constant);
                    }
                    else
                    {
                        //Dont replace for english
                        string locStrValue = !System.Globalization.CultureInfo.CurrentCulture.IsEnglish() ? strValue.Replace('.', ',') : strValue;
                        if (Double.TryParse(locStrValue, out double dbl))
                        {
                            token.ConstValue = dbl;
                            token.ElementType = ElementType.Constant;
                        }
                    }
                }
                else if (strValue.Length == 1 && (ch == '?' || ch == ':'))
                {
                    // check for conditional statement
                    if (ch == '?')
                    {
                        token.ElementType = ElementType.CondTrue;
                        token.Association = Association.Right;
                    }
                    else
                        token.ElementType = ElementType.CondFalse;
                }

                if (token.ElementType == ElementType.Identifier)
                {
                    // if token has not been classified then
                    // check for an operator token
                    if (Array.IndexOf(allOps, token.StrValue) >= 0)
                    {
                        token.ElementType = ElementType.Operator;
                        token.Association = Association.Left;
                    }
                    else
                    {
                        // if not an operator then check for a qualified indentifier
                        int pos;
                        if ((pos = strValue.LastIndexOf('.')) >= 0)
                        {
                            token.IsQualified = true;
                            token.Qualifier = "";
                            if (pos > 1)
                            {
                                token.Qualifier = strValue.Substring(0, pos);
                                token.StrValue = strValue.Substring(pos + 1);
                            }
                            else
                                token.StrValue = strValue.Substring(pos + 1);
                        }
                    }
                }
                token.Negate = negate;
                if (token.IsOperator)
                {
                    int opIndex = Array.IndexOf<string>(allOps, token.StrValue);
                    token.Precedence = precedence[opIndex];
                    if (token.StrValue == "**")
                        token.Association = Association.Right;
                    if (token.Precedence == assgnPrecedence)
                    {
                        token.ElementType = ElementType.Assignment;
                        token.Association = Association.Right;
                    }
                }
            }
        }

        private void HexString(RpnToken token, string hexChar, ElementType type)
        {
            string hex;
            int rc, count, cx, hexCh, cw;
            System.Text.StringBuilder sb = new StringBuilder(100);
            cw = 2;
            if (type == ElementType.UCLiteral)
                cw = 4;
            count = hexChar.Length / cw;
            cx = 0;
            if ((count * cw) != hexChar.Length)
                cx = (hexChar.Length % cw) - cw;
            hex = hexChar.Substring(0, cw + cx);
            rc = 0;
            while (rc == 0 && count > 0)
            {
                if (Int32.TryParse(hex, System.Globalization.NumberStyles.AllowHexSpecifier, null, out hexCh))
                    sb.Append((char)hexCh);
                else
                    rc = -1;
                cx += cw;
                if (--count > 0)
                    hex = hexChar.Substring(cx, cw);
            }

            if (rc == 0)
            {
                token.StrValue = sb.ToString();
                token.ElementType = type;
                if (type == ElementType.Constant)
                {
                    token.ConstValue = Double.NaN;
                    long temp;
                    if (Int64.TryParse(hexChar, System.Globalization.NumberStyles.AllowHexSpecifier, null, out temp))
                    {
                        token.StrValue = temp.ToString();
                    }
                    else
                        token.ElementType = ElementType.HexLiteral;
                }
            }
        }

        public RpnOperator CreateOperator(RpnToken token)
        {
            if (token.Negate && token.StrValue == "=")
            {
                token.StrValue = "!=";
                token.Negate = false;
            }

            RpnOperator newOp = new RpnOperator(token);
            if (IsArithmeticOperator(token))
            {
                newOp.OpGroup = OperatorGroup.Arithmetic;
                if (token.StrValue == "**")
                    newOp.OpType = OperatorType.Exponent;
                else if (token.StrValue == "//")
                    newOp.OpType = OperatorType.Remainder;
                else
                    newOp.OpType = (OperatorType)((int)token.StrValue[0]);
            }
            else if (IsComparisonOperator(token))
            {
                newOp.OpGroup = OperatorGroup.Comparison;
                newOp.OpType = (OperatorType)((int)token.StrValue[0]);
                string ts = token.StrValue;
                if (ts == "<=" || ts == "<<=")
                    newOp.OpType = OperatorType.CompLessThanEqual;
                else if (ts == ">=" || ts == ">>=")
                    newOp.OpType = OperatorType.CompGreaterEqual;
                if (token.Negate)
                {
                    newOp.Negate();
                    newOp.StrValue = "!" + newOp.StrValue;
                }
            }
            else if (IsLogicalOperator(token))
            {
                newOp.OpGroup = OperatorGroup.Logical;
                newOp.OpType = (OperatorType)(128 + (int)token.StrValue[0]);
            }
            else if (IsBitwiseOperator(token))
            {
                newOp.OpGroup = OperatorGroup.Bitwise;
                newOp.OpType = (OperatorType)((int)token.StrValue[0]);
            }
            else if (IsCondOperator(token))
            {
                newOp.OpGroup = OperatorGroup.Conditional;
                newOp.OpType = (OperatorType)((int)token.StrValue[0]);
            }
            else if (IsAssignOperator(token))
            {
                newOp.OpGroup = OperatorGroup.Assignment;
                newOp.OpType = (OperatorType)(128 + (int)token.StrValue[0]);
                if (token.StrValue[0] == '/' && token.StrValue[1] == '/')
                    newOp.OpType = OperatorType.AssignRemainder;
            }
            else if (token.StrValue == "!")
            {
                newOp.OpGroup = OperatorGroup.Logical;
                newOp.OpType = OperatorType.Negation;
            }
            else
                throw new Exception("Okänd operator: " + token.StrValue);

            return newOp;
        }

        private bool IsArithmeticOperator(RpnToken token)
        {
            int nPos = Array.IndexOf(arithmeticOps, token.StrValue);
            if (nPos != -1)
                return true;
            else
                return false;
        }

        private bool IsComparisonOperator(RpnToken token)
        {
            int nPos = Array.IndexOf(comparisonOps, token.StrValue);
            if (nPos != -1)
                return true;
            else
                return false;
        }

        private bool IsLogicalOperator(RpnToken token)
        {
            int nPos = Array.IndexOf(logicalOps, token.StrValue);
            if (nPos != -1)
                return true;
            else
                return false;
        }

        private bool IsBitwiseOperator(RpnToken token)
        {
            int nPos = Array.IndexOf(bitwiseOps, token.StrValue);
            if (nPos != -1)
                return true;
            else
                return false;
        }

        private bool IsCondOperator(RpnToken token)
        {
            int nPos = Array.IndexOf(condOps, token.StrValue);
            if (nPos != -1)
                return true;
            else
                return false;
        }

        private bool IsAssignOperator(RpnToken token)
        {
            int nPos = Array.IndexOf(assignOps, token.StrValue);
            if (nPos != -1)
                return true;
            else
                return false;
        }
    }

    #endregion

    #region Parser

    public class Parser
    {
        private struct CurrFunc
        {
            internal int elemCnt, condCnt;
            internal RpnFunction func;
        }

        private class RpnCtrl
        {
            private Stack<RpnToken> opcStack;
            private List<RpnElement> rpnExpr;
            private ITokeniser tokeniser;
            private int condEnd;

            internal RpnCtrl(Stack<RpnToken> stkOp, ITokeniser tokeniser, List<RpnElement> rpnExpr)
            {
                this.opcStack = stkOp;
                this.tokeniser = tokeniser;
                this.rpnExpr = rpnExpr;
                this.condEnd = -1;
            }

            internal void SetCondEnd()
            {
                if (this.condEnd < 0)
                    this.condEnd = this.opcStack.Count - 1;
            }

            internal void AddStack(RpnToken token)
            {
                while (this.opcStack.Count != 0)
                {
                    RpnToken stkTop = this.opcStack.Peek();
                    ElementType et = stkTop.ElementType;
                    if (token.Precedence > stkTop.Precedence || et == ElementType.GroupStart ||
                        et == ElementType.Function || et == ElementType.Assignment ||
                        (et == ElementType.CondTrue && token.ElementType == ElementType.CondTrue))
                        break;
                    if (token.Association == Association.Right && token.Precedence == stkTop.Precedence)
                        break;
                    stkTop = this.opcStack.Pop();
                    RpnOperator oprtr = this.tokeniser.CreateOperator(stkTop);
                    this.rpnExpr.Add(oprtr);
                }

                this.opcStack.Push(token);
            }

            internal void PopStack(bool groupEnd)
            {
                while (this.opcStack.Count > 0)
                {
                    ElementType et = this.opcStack.Peek().ElementType;
                    if (!groupEnd)
                    {
                        if (this.opcStack.Count == this.condEnd
                            || et == ElementType.CondTrue || et == ElementType.CondFalse)
                            break;
                    }

                    if (et == ElementType.GroupStart || et == ElementType.Function)
                    {
                        if (this.opcStack.Count == this.condEnd)
                            this.condEnd = -1;
                        break;
                    }

                    RpnOperator oprtr = this.tokeniser.CreateOperator(this.opcStack.Pop());
                    this.rpnExpr.Add(oprtr);
                }
            }
        }

        /// <summary>
        ///	* Read a token.
        ///	* If the token is a number, then add it to the output queue.
        ///	* If the token is a function token, then push it onto the stack.
        ///	* If the token is a function argument separator (e.g., a comma):
        ///		* Until the topmost element of the stack is a left parenthesis, pop the element onto the output queue. If no left parentheses are encountered, either the separator was misplaced or parentheses were mismatched.
        ///
        ///	* If the token is an operator, o1, then:
        ///
        ///		* while there is an operator, o2, at the top of the stack, and either
        ///	
        ///			o1 is associative or left-associative and its precedence is less than (lower precedence) or equal to that of o2, or
        ///			o1 is right-associative and its precedence is less than (lower precedence) that of o2,
        ///					pop o2 off the stack, onto the output queue;
        ///
        ///		* push o1 onto the stack.
        ///
        ///	* If the token is a left parenthesis, then push it onto the stack.
        ///	* If the token is a right parenthesis:
        ///
        ///		* Until the token at the top of the stack is a left parenthesis, pop operators off the stack onto the output queue.
        ///		* Pop the left parenthesis from the stack, but not onto the output queue.
        ///		* If the token at the top of the stack is a function token, pop it and onto the output queue.
        ///		* If the stack runs out without finding a left parenthesis, then there are mismatched parentheses.
        ///
        ///	* When there are no more tokens to read:
        ///
        ///		* While there are still operator tokens in the stack:
        ///
        /// 		* If the operator token on the top of the stack is a parenthesis, then there are mismatched parenthesis.
        ///			* Pop the operator onto the output queue.
        ///
        ///	* Exit.
        /// </summary>
        /// <param name="tokeniser"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        static public List<RpnElement> ParseExpression(ITokeniser tokeniser, string expression)
        {
            Stack<RpnToken> opcStack = new Stack<RpnToken>();
            Stack<RpnToken> condStk = new Stack<RpnToken>();
            Stack<CurrFunc> funcStk = new Stack<CurrFunc>();
            CurrFunc currFunc = new CurrFunc();
            List<RpnElement> rpnExpr = new List<RpnElement>();
            ElementType et;
            RpnCtrl ctrl = new RpnCtrl(opcStack, tokeniser, rpnExpr);

            // the elemCnt field is use to determine if the function has at least one
            // argument when the closing perenthese is encountered
            currFunc.elemCnt = currFunc.condCnt = 0;
            currFunc.func = null;

            List<RpnToken> tokens = tokeniser.Tokenise(expression);
            for (int tx = 0; tx < tokens.Count; ++tx)
            {
                RpnToken token = tokens[tx];
                switch (token.ElementType)
                {
                    case ElementType.Constant:
                    case ElementType.Literal:
                    case ElementType.HexLiteral:    // hex literal
                    case ElementType.UCLiteral:     // unicode literal
                        rpnExpr.Add(token);         // add element to expression list
                        if (currFunc.func != null)
                            currFunc.elemCnt += 1;	// increment count of elements in argument
                        break;
                    case ElementType.Identifier:
                        rpnExpr.Add(token);         // add element to expression list
                        if (currFunc.func != null)
                            currFunc.elemCnt += 1;	// increment count of elements in argument
                        break;
                    case ElementType.Function:
                        if (currFunc.func != null)
                            currFunc.elemCnt += 1;	// increment count of elements in argument
                        currFunc.condCnt = condStk.Count;
                        funcStk.Push(currFunc);     // save current function definition
                        currFunc.elemCnt = 0;       // reset argument element count 
                        currFunc.func = new RpnFunction(token);  // get new function defintion
                        opcStack.Push(token);       // push func token onto opcode stack 
                        break;
                    case ElementType.Argument:
                        if (currFunc.func == null)
                            throw new Exception("Ingen matchande funktion funnen");
                        currFunc.func.ArgCount += 1;  // increment count of function arguments
                        if (currFunc.elemCnt == 0)
                            rpnExpr.Add(new RpnToken());
                        currFunc.elemCnt = 0;         // reset argument element count
                        ctrl.PopStack(true);          // pop elements from opcode stack until func start is found 
                        if (opcStack.Peek().ElementType != ElementType.Function)
                            throw new Exception("Ingen matchande funktion med angivet argument funnen");

                        // terminate any conditional expressions in the current argument
                        while (condStk.Count > 0)
                        {
                            token = condStk.Pop();
                            int condIndex = token.OpIndex;
                            RpnOperator condOp = (RpnOperator)(rpnExpr[condIndex]);
                            condOp.CondGoto = rpnExpr.Count;
                        }
                        break;
                    case ElementType.Assignment:
                        // an assignment operator must be precceded by an Identifier
                        //if (rpnExpr[rpnExpr.Count - 1].ElementType == ElementType.Identifier)
                        opcStack.Push(token);
                        //else
                        //throw new ApplicationException("Invalid assignment: " + rpnExpr[0].StrValue);
                        break;
                    case ElementType.CondTrue:
                        ctrl.AddStack(token);     // add COndTrue element to opcode stack
                        ctrl.SetCondEnd();        // set end of condition expression
                        RpnOperator oprtr = tokeniser.CreateOperator(token);  // vreate CondTrue operator
                        token.OpIndex = rpnExpr.Count;   // save index to operator on expression list
                        rpnExpr.Add(oprtr);       // add operator to expression list 
                        break;
                    case ElementType.CondFalse:
                        ctrl.PopStack(false);     // pop elements from the opcode stack until a CondTrue is found 
                        oprtr = tokeniser.CreateOperator(token);  // create CondFalse operator
                        token.OpIndex = rpnExpr.Count;  // save index to operator on expression list
                        rpnExpr.Add(oprtr);       // add operator to expression list
                        condStk.Push(token);      // save CondFalse token
                        et = ElementType.Null;
                        if (opcStack.Count > 0)
                        {
                            token = opcStack.Pop();
                            et = token.ElementType;
                        }

                        if (et != ElementType.CondTrue)
                            throw new Exception("Villkor 'sant' saknas");
                        // set index to first element of false expression
                        int cx = token.OpIndex;
                        RpnOperator co = (RpnOperator)(rpnExpr[cx]);
                        co.CondGoto = rpnExpr.Count;
                        break;
                    case ElementType.GroupStart:
                        opcStack.Push(token);
                        currFunc.condCnt = condStk.Count;
                        funcStk.Push(currFunc);     // save current function definition
                        break;
                    case ElementType.GroupEnd:
                        ctrl.PopStack(true);    // pop elements from the opcode stack until a left perentheses is found 
                        et = ElementType.Null;
                        if (opcStack.Count > 0)
                        {
                            token = opcStack.Pop();
                            et = token.ElementType;
                        }

                        if (et != ElementType.GroupStart)
                        {
                            if (et == ElementType.Function)
                            {
                                if (currFunc.elemCnt > 0)
                                    currFunc.func.ArgCount += 1;
                                rpnExpr.Add(currFunc.func);   // add function to expression list
                            }
                            else
                            {
                                if (et == ElementType.CondTrue)
                                    throw new Exception("Villkor 'falskt' saknas");
                                else
                                    throw new Exception("Ej matchande parenteser: " + expression);
                            }
                        }

                        // terminate any conditional expressions
                        currFunc = funcStk.Pop();

                        while (condStk.Count > currFunc.condCnt)
                        {
                            token = condStk.Pop();
                            cx = token.OpIndex;
                            co = (RpnOperator)(rpnExpr[cx]);
                            if (et == ElementType.Function)
                                co.CondGoto = rpnExpr.Count - 1;
                            else
                                co.CondGoto = rpnExpr.Count;
                        }
                        break;
                    case ElementType.Operator:
                        ctrl.AddStack(token);   // add operator to opcode stack
                        break;
                    default:
                        throw new Exception("Okänd symbol: " + token.ToString());
                }
            }

            // pop elements from the opcode stack until end of stack or
            // end of conditional expression
            ctrl.PopStack(false);
            // terminate any conditional expressions
            while (condStk.Count > 0)
            {
                RpnToken token = condStk.Pop();
                int condIndex = token.OpIndex;
                RpnOperator condOp = (RpnOperator)(rpnExpr[condIndex]);
                condOp.CondGoto = rpnExpr.Count;
            }

            // pop any remaining elements from the opcode stack
            while (opcStack.Count != 0)
            {
                RpnToken stkTop = opcStack.Pop();
                if (stkTop.StrValue == "(")
                    throw new Exception("Ej matchande parenteser: " + expression);
                if (stkTop.ElementType == ElementType.CondTrue)
                    throw new Exception("Villkor 'falskt' saknas");
                else
                {
                    RpnOperator oprtr = tokeniser.CreateOperator(stkTop);
                    rpnExpr.Add(oprtr);
                }
            }

            return rpnExpr;
        }
    }

    #endregion

    #region RpnElement

    public class RpnElement
    {
        private string strValue;
        private ElementType type;
        private bool negate, isQualified;

        public RpnElement(string strValue)
        {
            this.strValue = strValue;
            this.negate = false;
            this.isQualified = false;
        }

        public RpnElement(RpnToken token)
        {
            this.strValue = token.StrValue;
            this.type = token.ElementType;
            this.negate = token.Negate;
            this.isQualified = token.IsQualified;
        }

        public string StrValue
        {
            get { return this.strValue; }
            set { this.strValue = value; }
        }

        public ElementType ElementType
        {
            get { return this.type; }
            set { this.type = value; }
        }

        public bool Negate
        {
            get { return this.negate; }
        }

        public override string ToString()
        {
            return this.strValue;
        }

        public bool IsNull
        {
            get { return this.ElementType == ElementType.Null; }
        }

        public bool IsQualified
        {
            get { return this.isQualified; }
            set { this.isQualified = value; }
        }
    }

    #endregion

    #region RpnFunction

    public class RpnFunction : RpnElement
    {
        private string qualifier;
        private int argCount;

        public RpnFunction(RpnToken token)
            : base(token)
        {
            this.qualifier = token.Qualifier;
            this.argCount = 0;
        }

        public string Name
        {
            get { return base.StrValue; }
        }

        public override string ToString()
        {
            return String.Format("{0} {1} {2}", base.ElementType, base.StrValue, this.argCount);
        }

        public int ArgCount
        {
            get { return this.argCount; }
            set { this.argCount = value; }
        }

        public string Qualifier
        {
            get { return this.qualifier; }
        }
    }

    #endregion

    #region RpnOperand

    /// <summary>
    /// Base class for all Operands.  Provides datastorage
    /// </summary>
    public class RpnOperand
    {
        private bool isString, isNumeric, isNull;
        private string strValue, fieldName;
        private double numValue;

        public RpnOperand()
        {
            this.isNull = false;
            this.strValue = "";
            this.isString = true;
            this.isNumeric = true;
            this.numValue = 0;
        }

        public RpnOperand(string value)
        {
            SetValue(value);
        }

        public RpnOperand(double value)
        {
            SetValue(value);
        }

        public void SetValue(string value)
        {
            this.isNull = false;
            this.strValue = value;
            this.isString = true;
            if (Double.TryParse(this.strValue, out this.numValue))
                this.isNumeric = true;
            else
                this.numValue = Double.NaN;
        }

        public void SetValue(double value)
        {
            this.isNull = false;
            this.numValue = value;
            this.isString = false;
            this.isNumeric = true;
        }

        public RpnOperand(RpnToken token)
        {
            this.isNull = false;
            if (token.ElementType == ElementType.Constant)
            {
                this.isString = false;
                this.numValue = token.ConstValue;
                this.isNumeric = true;
            }
            else
            {
                if (token.ElementType == ElementType.HexLiteral)
                {
                    this.isString = true;
                    this.strValue = token.StrValue;
                    this.numValue = token.ConstValue;
                    if (token.ConstValue != Double.NaN)
                        this.isNumeric = true;
                }
                else
                    if (token.IsNull)
                {
                    this.isNull = true;
                    this.isNumeric = false;
                    this.isString = false;
                    this.numValue = Double.NaN;
                }
                else
                {
                    this.isString = true;
                    this.strValue = token.StrValue;
                    if (Double.TryParse(this.strValue, out this.numValue))
                        this.isNumeric = true;
                    else
                        this.numValue = Double.NaN;
                }
            }
        }

        public bool IsString
        {
            get { return this.isString; }
        }

        public bool IsNumeric
        {
            get { return this.isNumeric; }
        }

        public string FieldName
        {
            get { return this.fieldName; }
            set { this.fieldName = value; }
        }

        public string StrValue
        {
            get
            {
                string data = this.strValue;
                if (!this.isString)
                    data = this.numValue.ToString();
                return data;
            }
        }

        public double NumValue
        {
            get { return this.numValue; }
        }

        public bool IsNull
        {
            get { return this.isNull; }
        }

        public char CharValue
        {
            get
            {
                char ch = '\0';
                if (this.isString)
                    ch = this.strValue[0];
                else
                    if (this.isNumeric && this.numValue < Char.MaxValue)
                {
                    int num = (int)this.numValue;
                    ch = (char)num;
                }
                return ch;
            }
        }
    }

    #endregion

    #region RpnOperator

    public enum OperatorType
    {
        Unknown,
        BitwiseAnd = (int)'&',
        BitwiseOr = (int)'|',
        BitwiseXor = (int)'^',
        LogicalAnd = 128 + (int)'&',
        LogicalOr = 128 + (int)'|',
        LogicalNot = 128 + (int)'^',
        Negation = 128 + (int)'!',
        CompEqual = (int)'=',
        CompNotEqual = (int)'!',
        CompGreater = (int)'>',
        CompGreaterEqual = 128 + (int)'>',
        CompLessThan = (int)'<',
        CompLessThanEqual = 128 + (int)'|',
        Plus = (int)'+',
        Minus = (int)'-',
        Multiply = (int)'*',
        Divide = (int)'/',
        Remainder = (int)'\\',
        IntegerDivide = (int)'%',
        Exponent = 255,
        CondTrue = (int)'?',
        CondFalse = (int)':',
        AssignEqual = 128 + (int)':',
        AssignPlus = 128 + (int)'+',
        AssignMinus = 128 + (int)'-',
        AssignMultiply = 128 + (int)'*',
        AssignDivide = 128 + (int)'/',
        AssignIntegerDivide = 128 + (int)'%',
        AssignRemainder = 128 + (int)'\\',
    }

    public enum OperatorGroup
    {
        Unknown,
        Arithmetic,
        Comparison,
        Logical,
        Bitwise,
        Conditional,
        Assignment,
        StructField
    }

    public enum Association
    {
        NA,
        Left,
        Right
    }

    /// <summary>
    /// Base class of all operators.  Provides datastorage
    /// </summary>
    public class RpnOperator : RpnElement
    {
        private OperatorType opType;
        private OperatorGroup opGroup;
        private Association assoc;
        private int precedence;
        private int condGoto;

        public RpnOperator(RpnToken token)
            : base(token)
        {
            this.precedence = token.Precedence;
            this.assoc = token.Association;
        }

        public bool IsMonadic
        {
            get { return (this.precedence == 16); }
        }

        public OperatorType OpType
        {
            get { return this.opType; }
            set { this.opType = value; }
        }

        public OperatorGroup OpGroup
        {
            get { return this.opGroup; }
            set { this.opGroup = value; }
        }

        public int CondGoto
        {
            get { return this.condGoto; }
            set { this.condGoto = value; }
        }

        public Association Association
        {
            get { return this.assoc; }
        }

        public override string ToString()
        {
            string data = String.Format("{0} {1}", base.ElementType, base.StrValue);
            if (this.opGroup == OperatorGroup.Conditional)
                data += ' ' + this.condGoto.ToString();
            return data;
        }

        public new void Negate()
        {
            switch (opType)
            {
                case OperatorType.CompEqual:
                    this.opType = OperatorType.CompNotEqual;
                    break;
                case OperatorType.CompGreater:
                    opType = OperatorType.CompLessThanEqual;
                    break;
                case OperatorType.CompGreaterEqual:
                    this.opType = OperatorType.CompLessThan;
                    break;
                case OperatorType.CompLessThan:
                    this.opType = OperatorType.CompGreaterEqual;
                    break;
                case OperatorType.CompLessThanEqual:
                    this.opType = OperatorType.CompGreater;
                    break;
            }
        }
    }

    #endregion

    #region RpnToken

    public enum ElementType
    {
        Null = 0,
        Literal = 1,
        HexLiteral,
        UCLiteral,
        Identifier,
        Constant,
        Function,
        Argument,
        GroupStart,
        GroupEnd,
        Operator,
        CondTrue,
        CondFalse,
        Assignment
    }

    /// <summary>
    /// Represents each token in the expression
    /// </summary>
    public class RpnToken : RpnElement
    {
        private double constant;
        private string qualifier;
        private int precedence;
        private Association assoc;
        private int opIndex;
        private bool negate;

        public RpnToken()
            : base("")
        {
            base.ElementType = ElementType.Null;
            this.qualifier = "";
            this.precedence = 0;
            this.opIndex = -1;
            this.assoc = Association.NA;
            this.negate = false;
        }

        public RpnToken(string strValue)
            : base(strValue)
        {
            base.ElementType = ElementType.Identifier;
            this.qualifier = "";
            this.precedence = 0;
            this.opIndex = -1;
            this.assoc = Association.NA;
        }

        public bool IsOperator
        {
            get { return (base.ElementType >= ElementType.Operator); }
        }

        public new bool Negate
        {
            get { return this.negate; }
            set { this.negate = value; }
        }

        public Association Association
        {
            get { return this.assoc; }
            set { this.assoc = value; }
        }

        public int Precedence
        {
            get { return this.precedence; }
            set { this.precedence = value; }
        }

        public double ConstValue
        {
            get { return this.constant; }
            set { this.constant = value; }
        }

        public int OpIndex
        {
            get { return this.opIndex; }
            set { this.opIndex = value; }
        }

        public string Qualifier
        {
            get
            {
                string data = "";
                if (base.IsQualified)
                    data = this.qualifier;
                return data;
            }
            set
            {
                this.qualifier = value;
                base.IsQualified = true;
            }
        }

        public override string ToString()
        {
            string data = String.Format("{0} {1}", base.ElementType, base.StrValue);
            if (this.qualifier.Length > 0)
                data += ' ' + this.qualifier;
            return data;
        }
    }

    #endregion

    #region RpnParser

    public class RpnParser
    {
        private List<string> Identifiers;

        public ActionResult Parse(string formula, List<string> identifiers)
        {
            this.Identifiers = identifiers;

            ActionResult result = new ActionResult();

            if (formula.Length > 0)
            {
                try
                {
                    ITokeniser parser = new MathParser();
                    List<RpnElement> tokens = Parser.ParseExpression(parser, formula);

                    // Evaluate formula
                    RpnOperand op = RpnEval(tokens);
                    result.StringValue = op.StrValue;
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.ErrorMessage = ex.Message;
                }
            }

            return result;
        }

        private RpnOperand RpnEval(List<RpnElement> tokens)
        {
            Stack<RpnOperand> oprndStack = new Stack<RpnOperand>();
            RpnOperand oprnd = null;

            for (int nextElem = 0; nextElem < tokens.Count; ++nextElem)
            {
                RpnElement token = tokens[nextElem];
                switch (token.ElementType)
                {
                    // Create an operand from the token and push it onto the operand stack
                    case ElementType.Literal:
                    case ElementType.Constant:
                        oprnd = new RpnOperand((RpnToken)token);
                        break;
                    // Get the current value of the identifier and push it onto the operand stack
                    case ElementType.Identifier:
                        oprnd = EvalIdentifier((RpnToken)token);
                        break;
                    // Pop the left and right sides of the operator from the operand stack
                    // Perform the operation and push the results onto the operand stack
                    case ElementType.Operator:
                        RpnOperator oper = (RpnOperator)token;
                        RpnOperand lhs, rhs;
                        lhs = rhs = null;
                        if (oper.IsMonadic && oprndStack.Count >= 1)
                        {
                            lhs = new RpnOperand(0);
                            rhs = oprndStack.Pop();
                        }
                        else
                        {
                            if (oprndStack.Count >= 2)
                            {
                                rhs = oprndStack.Pop();
                                lhs = oprndStack.Pop();
                            }
                        }
                        if (lhs == null)
                            StackError("operator", token.StrValue);
                        else
                            oprnd = EvalOperator(oper, lhs, rhs);
                        break;
                    // Determine if True or False condition has be met
                    // Set element index to the next element to be processed
                    case ElementType.CondTrue:
                    case ElementType.CondFalse:
                        oper = (RpnOperator)token;
                        if (oprndStack.Count < 1)
                            StackError("operator", token.StrValue);
                        if (oper.OpType == OperatorType.CondTrue)
                        {
                            lhs = oprndStack.Pop();
                            if (lhs.NumValue == 0)
                                nextElem = oper.CondGoto - 1;  // for loop will add one
                        }
                        else
                            nextElem = oper.CondGoto - 1; // for loop will add one
                        continue;
                    // Pop the function arguments from the operand stack and place into an array
                    // Evalualate the function and push the results onto the operand stack
                    case ElementType.Function:
                        RpnFunction func = (RpnFunction)token;
                        if (oprndStack.Count >= func.ArgCount)
                        {
                            RpnOperand[] args = new RpnOperand[func.ArgCount];
                            for (int i = func.ArgCount - 1; i >= 0; --i)
                                args[i] = oprndStack.Pop();

                            oprnd = EvalFunction(func, args);
                        }
                        else
                            StackError("funktion", token.StrValue);
                        break;
                    // Pop Identifier and value to be assigned fron the stack
                    // Push the resulting operand onto the operand stack
                    case ElementType.Assignment:
                        if (oprndStack.Count > 1)
                        {
                            rhs = oprndStack.Pop();   // value to be assigned
                            lhs = oprndStack.Pop();   // identifier to be assigned to
                            oprnd = AssignOperator((RpnOperator)token, lhs, rhs);
                        }
                        else
                            StackError("värde", token.StrValue);
                        break;
                }
                oprndStack.Push(oprnd);
            }

            if (oprndStack.Count != 1)
                StackError("resultat", "");

            return oprndStack.Pop();
        }

        private void StackError(string token, string value)
        {
            string text = String.Format("Fel antal operander för {0}: {1}", token, value);
            throw new Exception(text);
        }

        private RpnOperand EvalIdentifier(RpnToken token)
        {
            int pos;
            string id = token.StrValue;
            if (token.IsQualified)
                id = token.Qualifier + '.' + id;
            RpnOperand oprnd = null;
            if (id.ToLower() == "pi")
                oprnd = new RpnOperand(Math.PI);
            else
            {
                foreach (string identifier in this.Identifiers)
                {
                    if ((pos = identifier.IndexOf('=')) > 0)
                    {
                        string code = identifier.Substring(0, pos).Trim();
                        string value = identifier.Substring(pos + 1).Trim();
                        if (id == code)
                        {
                            oprnd = new RpnOperand(value);
                            break;
                        }
                    }
                }

                if (oprnd == null)
                {
                    double numValue = 0;
                    if (id.Length == 1 && Char.IsLetter(id[0]))
                    {
                        char ndxChar = id.ToLower()[0];
                        int index = ndxChar - 'a';
                        numValue = assignTo[index];
                    }
                    oprnd = new RpnOperand(numValue);
                }
                oprnd.FieldName = id;
            }

            return oprnd;
        }

        private double[] assignTo = new double[26];

        private RpnOperand AssignOperator(RpnOperator oper, RpnOperand lhs, RpnOperand rhs)
        {
            char ndxChar = lhs.FieldName.ToLower()[0];
            int index = ndxChar - 'a';
            double result = rhs.NumValue;
            double prevValue = assignTo[index];

            switch (oper.OpType)
            {
                case OperatorType.AssignMinus:
                    result = prevValue - result;
                    break;
                case OperatorType.AssignPlus:
                    result = prevValue + result;
                    break;
                case OperatorType.AssignMultiply:
                    result = prevValue * result;
                    break;
                case OperatorType.AssignDivide:
                    result = prevValue / result;
                    break;
                case OperatorType.AssignRemainder:
                    result = prevValue % result;
                    break;
                case OperatorType.AssignIntegerDivide:
                    result = (prevValue / (int)result);
                    break;
            }

            assignTo[index] = result;

            return new RpnOperand(result);
        }

        private RpnOperand EvalOperator(RpnOperator op, RpnOperand lhs, RpnOperand rhs)
        {
            RpnOperand oprnd = null;
            switch (op.OpGroup)
            {
                case OperatorGroup.Arithmetic:
                    oprnd = MathOperator(op, lhs, rhs);
                    break;
                case OperatorGroup.Bitwise:
                    oprnd = BitWiseOperator(op, lhs, rhs);
                    break;
                case OperatorGroup.Comparison:
                    oprnd = CompOperator(op, lhs, rhs);
                    break;
                case OperatorGroup.Logical:
                    oprnd = LogicalOperator(op, lhs, rhs);
                    break;
                default:
                    throw new Exception("Felaktig operator: " + op.ToString());
            }

            return oprnd;
        }

        private RpnOperand MathOperator(RpnOperator op, RpnOperand lhs, RpnOperand rhs)
        {
            bool isString = false;
            string retStr = "";
            double result = 0;
            double left = lhs.NumValue;
            double right = rhs.NumValue;
            if (Double.IsNaN(left) || Double.IsNaN(right))
            {
                if (op.OpType == OperatorType.Plus)
                    isString = true;
                else
                    throw new Exception("Felaktig strängoperation: " + op.StrValue);
            }

            switch (op.OpType)
            {
                case OperatorType.Plus:
                    if (isString)
                        retStr = lhs.StrValue + rhs.StrValue;
                    else
                        result = lhs.NumValue + rhs.NumValue;
                    break;
                case OperatorType.Minus:
                    result = lhs.NumValue - rhs.NumValue;
                    break;
                case OperatorType.Multiply:
                    result = lhs.NumValue * rhs.NumValue;
                    break;
                case OperatorType.Remainder:
                    result = lhs.NumValue % rhs.NumValue;
                    break;
                case OperatorType.Divide:
                case OperatorType.IntegerDivide:
                    result = lhs.NumValue / rhs.NumValue;
                    if (op.OpType == OperatorType.IntegerDivide)
                        result = (int)result;
                    break;
            }

            RpnOperand oprnd;
            if (isString)
                oprnd = new RpnOperand(retStr);
            else
                oprnd = new RpnOperand(result);

            return oprnd;
        }

        private RpnOperand CompOperator(RpnOperator op, RpnOperand lhs, RpnOperand rhs)
        {
            int comp = 0;
            if (lhs.IsString && rhs.IsString)
                comp = lhs.StrValue.CompareTo(rhs.StrValue);
            else
                comp = lhs.NumValue.CompareTo(rhs.NumValue);

            double result = 0;
            switch (op.OpType)
            {
                case OperatorType.CompEqual:
                    if (comp == 0)
                        result = 1;
                    break;
                case OperatorType.CompNotEqual:
                    if (comp != 0)
                        result = 1;
                    break;
                case OperatorType.CompGreater:
                    if (comp > 0)
                        result = 1;
                    break;
                case OperatorType.CompGreaterEqual:
                    if (comp >= 0)
                        result = 1;
                    break;
                case OperatorType.CompLessThan:
                    if (comp < 0)
                        result = 1;
                    break;
                case OperatorType.CompLessThanEqual:
                    if (comp <= 0)
                        result = 1;
                    break;
            }

            return new RpnOperand(result);
        }

        private RpnOperand BitWiseOperator(RpnOperator op, RpnOperand lhs, RpnOperand rhs)
        {
            RpnOperand oprnd;
            double lNum = lhs.NumValue;
            double rNum = rhs.NumValue;
            if (Double.IsNaN(lNum) || Double.IsNaN(rNum))
            {
                throw new Exception("Bitvis operation ej definierad för strängar");
            }
            else
            {
                double temp = Math.Round(lNum);
                if (temp > lNum)
                    lNum = temp - 1;
                else
                    lNum = temp;
                temp = Math.Round(rNum);
                if (temp > rNum)
                    rNum = temp - 1;
                else
                    rNum = temp;
                long left = Convert.ToInt64(lNum);
                long right = Convert.ToInt64(rNum);
                long bwLong = 0;
                switch (op.OpType)
                {
                    case OperatorType.BitwiseAnd:
                        bwLong = left & right;
                        break;
                    case OperatorType.BitwiseOr:
                        bwLong = left | right;
                        break;
                    case OperatorType.BitwiseXor:
                        bwLong = left ^ right;
                        break;
                }
                oprnd = new RpnOperand(Convert.ToDouble(bwLong));
            }

            return oprnd;
        }

        private RpnOperand LogicalOperator(RpnOperator op, RpnOperand lhs, RpnOperand rhs)
        {
            bool rhBool, lhBool;
            double result = 0;
            lhBool = rhBool = false;
            if (rhs.NumValue == 1)
                rhBool = true;
            if (lhs.NumValue == 1)
                lhBool = true;

            switch (op.OpType)
            {
                case OperatorType.LogicalAnd:
                    if (lhBool && rhBool)
                        result = 1;
                    break;
                case OperatorType.LogicalOr:
                    if (lhBool || rhBool)
                        result = 1;
                    break;
            }

            return new RpnOperand(result);
        }

        private RpnOperand EvalFunction(RpnFunction func, RpnOperand[] args)
        {
            double result = 0;
            bool argError = false;
            switch (func.Name)
            {
                case "Min":
                    if (args.Length != 2)
                        argError = true;
                    else
                        result = Math.Min(args[0].NumValue, args[1].NumValue);
                    break;
                case "Max":
                    if (args.Length != 2)
                        argError = true;
                    else
                        result = Math.Max(args[0].NumValue, args[1].NumValue);
                    break;
                case "Sin":
                    if (args.Length != 1)
                        argError = true;
                    else
                    {
                        result = Math.Sin(args[0].NumValue);
                        result = Math.Round(result, 10);
                    }
                    break;
                default:
                    throw new Exception("Ogiltig funktion: " + func.Name);
            }

            if (argError)
                throw new Exception("Fel antal argument för funktionen: " + func.Name + " (" + args.Length.ToString() + ")");

            return new RpnOperand(result);
        }
    }

    #endregion
}

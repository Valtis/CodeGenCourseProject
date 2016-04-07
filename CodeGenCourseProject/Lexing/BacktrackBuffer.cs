using CodeGenCourseProject;
using CodeGenCourseProject.Tokens;
using System.Collections.Generic;

namespace CodeGenCourseWork.Lexing
{
    /*
    Token backtrack buffer. 
    
    Maintains a queue of tokens, which allows backtracking up to queue size
    */
    internal class BacktrackBuffer
    {
        private readonly int bufferSize;
        private IList<Token> backtrackBuffer;
        private int backtrackPosition;

        internal BacktrackBuffer(int size)
        {
            backtrackBuffer = new List<Token>();
            bufferSize = size;
            // used to index the backtrack buffer. -1 means empty
            backtrackPosition = -1;
        }

        internal bool Empty()
        {
            return backtrackPosition == -1;
        }

        internal void Backtrack()
        {
            ++backtrackPosition;
            if (backtrackPosition >= bufferSize ||
                backtrackPosition >= backtrackBuffer.Count)
            {
                throw new InternalCompilerError("Attempted to backtrack too many times");
            }
        }

        internal Token GetToken()
        {
            return backtrackBuffer[backtrackPosition--];
        }

        internal Token PeekToken()
        {
            return backtrackBuffer[backtrackPosition];
        }

        internal void AddToken(Token token)
        {
            if (backtrackPosition != -1)
            {
                --backtrackPosition;
                return;
            }

            backtrackBuffer.Insert(0, token);
            while (backtrackBuffer.Count > bufferSize)
            {
                backtrackBuffer.RemoveAt(backtrackBuffer.Count - 1);
            } 
        }

        
    }

}

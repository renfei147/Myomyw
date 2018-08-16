﻿using System;
using System.Collections.Generic;
using System.Threading;

namespace Assets.GameEngine
{
    public enum ChessTypeName
    {
        Common = 0,
        Key = 1,
        Flip = 2,
        AddCol = 3,
        DelCol = 4
    }

    public abstract class ChessType
    {
        protected ChessType(string name, ChessTypeName type)
        {
            Name = name;
            Type = type;
        }

        public delegate void OnOperationEvent();

        public ChessTypeName Type { get; }
        public string Name { get; }

        public virtual void Process() => OnOperation?.Invoke();

        public event OnOperationEvent OnOperation;

        private delegate void Operation();
    }

    public class ChessCommon : ChessType
    {
        public ChessCommon() : base("Common", ChessTypeName.Common)
        {
        }
    }
    
    public class ChessKey : ChessType
    {
        public ChessKey() : base("Key", ChessTypeName.Key)
        {
        }

        public override void Process()
        {
            base.Process();
        }
    }
    
    public class ChessFlip : ChessType
    {
        public ChessFlip() : base("Flip", ChessTypeName.Flip)
        {
        }
            
        public override void Process()
        {
            base.Process();
        }
    }
    
    public class ChessAddCol : ChessType
    {
        public ChessAddCol() : base("AddCol", ChessTypeName.AddCol)
        {
        } 
        
        public override void Process()
        {
            base.Process();
        }
    }
    
    public class ChessDelCol : ChessType
    {
        public ChessDelCol() : base("DelCol", ChessTypeName.DelCol)
        {
        }
        
        public override void Process()
        {
            base.Process();
        }
    }

    public static class ChessTypeManager
    {
        private static readonly Dictionary<ChessTypeName, ChessType> Types = new Dictionary<ChessTypeName, ChessType>();

        static ChessTypeManager()
        {
            Register(new ChessCommon());
            Register(new ChessKey());
            Register(new ChessFlip());
            Register(new ChessAddCol());
            Register(new ChessDelCol());
        }

        public static ChessType Get(ChessTypeName name) => Types[name];
        
        private static void Register(ChessType type) => Types.Add(type.Type, type);
    }

    public class ChessBoard
    {
        private ChessTypeName[] _chessBoard;

        public ChessBoard()
        {
            ResizeBoard(3, 3);
        }

        public static ChessBoard Current { get; private set; }

        public int SizeLeft { get; private set; }

        public int SizeRight { get; private set; }

        public void MakeCurrent() => Current = this;

        private void ProcessExtraChess(ChessTypeName typeName) => ChessTypeManager.Get(typeName).Process();

        public ChessTypeName GetChess(int left, int right)
        {
            return _chessBoard[left * SizeLeft + right];
        }

        public void SetChess(ChessTypeName chess, int left, int right)
        {
            _chessBoard[left * SizeLeft + right] = chess;
        }

        private void ResizeBoard(int newSizeLeft, int newSizeRight)
        {
            var board = new ChessTypeName[newSizeLeft * newSizeRight];
            for (var i = 0; i < Math.Min(SizeLeft, newSizeLeft); ++i)
            for (var j = 0; j < Math.Min(SizeRight, newSizeRight); ++j)
                board[i * newSizeLeft + j] = GetChess(i, j);
            _chessBoard = board;
            SizeLeft = newSizeLeft;
            SizeRight = newSizeRight;
        }
    }
}
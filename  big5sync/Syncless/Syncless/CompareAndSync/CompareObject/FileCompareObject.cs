﻿using System.Collections.Generic;

namespace Syncless.CompareAndSync.CompareObject
{
    public class FileCompareObject : BaseCompareObject
    {
        //Actual file
        private string[] _hash;
        private long[] _length, _lastWriteTime;

        //Meta file
        private string[] _metaHash;
        private long[] _metaLength, _metaLastWriteTime;

        //Optimization
        private int _sourcePosition;
        private List<int> _conflictPos;

        public FileCompareObject(string name, int numOfPaths, FolderCompareObject parent)
            : base(name, numOfPaths, parent)
        {
            _hash = new string[numOfPaths];
            _length = new long[numOfPaths];
            _lastWriteTime = new long[numOfPaths];

            _metaHash = new string[numOfPaths];
            _metaLength = new long[numOfPaths];
            _metaLastWriteTime = new long[numOfPaths];
        }

        public string[] Hash
        {
            get { return _hash; }
            set { _hash = value; }
        }

        public long[] Length
        {
            get { return _length; }
            set { _length = value; }
        }

        public long[] LastWriteTime
        {
            get { return _lastWriteTime; }
            set { _lastWriteTime = value; }
        }

        public string[] MetaHash
        {
            get { return _metaHash; }
            set { _metaHash = value; }
        }

        public long[] MetaLength
        {
            get { return _metaLength; }
            set { _metaLength = value; }
        }

        public long[] MetaLastWriteTime
        {
            get { return _metaLastWriteTime; }
            set { _metaLastWriteTime = value; }
        }

        public List<int> ConflictPositions
        {
            get
            {
                if (_conflictPos == null)
                    _conflictPos = new List<int>();
                return _conflictPos;
            }
            set { _conflictPos = value; }
        }

        public int SourcePosition
        {
            get { return _sourcePosition; }
            set { _sourcePosition = value; }
        }

    }
}

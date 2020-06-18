// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0

using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace OrmGift.Utils
{
    public struct HashCodeCombiner
    {
        private long _combinedHash64;

        public int CombinedHash
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _combinedHash64.GetHashCode(); }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private HashCodeCombiner(long seed)
        {
            _combinedHash64 = seed;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator int(HashCodeCombiner self)
        {
            return self.CombinedHash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(int i)
        {
            _combinedHash64 = ((_combinedHash64 << 5) + _combinedHash64) ^ i;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HashCodeCombiner Start()
        {
            return new HashCodeCombiner(0x1505L);
        }
    }
}

// State layout adapted from PCG Random Number Generation for C.
// Copyright 2014 Melissa O'Neill <oneill@pcg-random.org>
// Licensed under the Apache License, Version 2.0; see Pcg32.LICENSE.txt.
// Source: https://github.com/imneme/pcg-c-basic/blob/bc39cd76ac3d541e618606bcc6e1e5ba5e5e6aa3/pcg_basic.h
// Changes (2026-09-20): immutable C# value holder with internal construction.
namespace FightMatch.Core
{
    public sealed class Pcg32CoreState
    {
        public ulong State { get; }
        public ulong Increment { get; }

        internal Pcg32CoreState(ulong state, ulong increment)
        {
            State = state;
            Increment = increment;
        }
    }
}

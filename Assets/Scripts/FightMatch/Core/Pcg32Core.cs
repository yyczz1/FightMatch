/*
 * PCG Random Number Generation for C.
 * Copyright 2014 Melissa O'Neill <oneill@pcg-random.org>
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License. A copy is in Pcg32.LICENSE.txt.
 *
 * Source: https://github.com/imneme/pcg-c-basic/blob/bc39cd76ac3d541e618606bcc6e1e5ba5e5e6aa3/pcg_basic.c
 * Changes (2026-09-20): C# port with immutable state, strict 63-bit sequence
 * input, and 16-byte little-endian core encoding. No global generator.
 * For additional information and licensing options: http://www.pcg-random.org
 */
using System;

namespace FightMatch.Core
{
    public static class Pcg32Core
    {
        public static Pcg32CoreState Initialize(ulong initState, ulong initSequence)
        {
            if (initSequence >= (1UL << 63))
                throw new ArgumentOutOfRangeException(nameof(initSequence));

            var increment = (initSequence << 1) | 1UL;
            Next32(new Pcg32CoreState(0, increment), out var first);
            var seeded = new Pcg32CoreState(unchecked(first.State + initState), increment);
            Next32(seeded, out var initialized);
            return initialized;
        }

        public static Pcg32CoreState Restore(ulong state, ulong increment)
        {
            if ((increment & 1UL) == 0)
                throw new ArgumentOutOfRangeException(nameof(increment));

            return new Pcg32CoreState(state, increment);
        }

        public static uint Next32(Pcg32CoreState state, out Pcg32CoreState next)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            unchecked
            {
                var old = state.State;
                var xorshifted = (uint)(((old >> 18) ^ old) >> 27);
                var rotation = (int)(old >> 59);
                var word = rotation == 0
                    ? xorshifted
                    : (xorshifted >> rotation) | (xorshifted << (32 - rotation));
                next = new Pcg32CoreState(old * 6364136223846793005UL + state.Increment,
                    state.Increment);
                return word;
            }
        }

        public static byte[] EncodeCore(Pcg32CoreState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            var bytes = new byte[16];
            for (var i = 0; i < 8; i++)
            {
                bytes[i] = unchecked((byte)(state.State >> (8 * i)));
                bytes[i + 8] = unchecked((byte)(state.Increment >> (8 * i)));
            }
            return bytes;
        }

        public static Pcg32CoreState DecodeCore(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));
            if (bytes.Length != 16)
                throw new ArgumentException("Core state must contain exactly 16 bytes.", nameof(bytes));

            ulong state = 0;
            ulong increment = 0;
            for (var i = 0; i < 8; i++)
            {
                state |= (ulong)bytes[i] << (8 * i);
                increment |= (ulong)bytes[i + 8] << (8 * i);
            }
            if ((increment & 1UL) == 0)
                throw new ArgumentException("Core increment must be odd.", nameof(bytes));

            return new Pcg32CoreState(state, increment);
        }
    }
}
